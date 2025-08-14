using Api.Exceptions;
using Application.Contracts.Auth;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly ILocalizationService _localizationService;

        public AuthenticationService(IUnitOfWork unitOfWork, TokenValidationParameters tokenValidationParameters, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _tokenValidationParameters = tokenValidationParameters;
            _localizationService = localizationService;
        }

        public async Task<AuthResponse> Register(UserRegisterRequest request)
        {
            // Validate existence of the unique user key
            var exists = _unitOfWork.Users.Find(u => u.Username == request.Username).FirstOrDefault();
            if (exists is not null)
            {
                return new AuthResponse() 
                {
                    Result = false,
                    Errors = new List<string>() { _localizationService.GetLocalizedString("UserNotAvailable", request.Username) }
                };
            }

            // Retrive the default role
            var defaultRole = _unitOfWork.Roles.Find(r => r.Name == "user").FirstOrDefault();
            if (defaultRole is null)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { _localizationService.GetLocalizedString("UserRoleNotFound") }
                };
            }

            // Generate instance of the user and add to database
            var user = new User
            {
                Username = request.Username,
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Disabled = true,
                RoleId = defaultRole.Id
            };
            await _unitOfWork.Users.Add(user);

            var authResponse = await GenerateJwtToken(user);
            return authResponse;
        }

        public async Task<bool> ChangePassword(UserLoginRequest request)
        {
            var user = _unitOfWork.Users.Find((u) => u.Username == request.Username).FirstOrDefault();
            if (user is null) 
            {
                return false;
            }

            var encrPassword = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);
            user.Password = encrPassword; // assign new password before update
            await _unitOfWork.Users.Update(user);
            return true;
        }

        public async Task<AuthResponse> Login(UserLoginRequest request)
        {
            var user = _unitOfWork.Users.Find(u => u.Username == request.Username).FirstOrDefault();
            if (user is null)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { _localizationService.GetLocalizedString("UserNotExist", request.Username) }
                };
            }

            var isPasswordValid = BCrypt.Net.BCrypt.EnhancedVerify(request.Password, user.Password);
            if (!isPasswordValid)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        _localizationService.GetLocalizedString("UserPasswordInvalid")
                    }
                };
            }

            if (user.Disabled)
            {
                return new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        _localizationService.GetLocalizedString("UserDisabled", request.Username)
                    }
                };
            }

            return await GenerateJwtToken(user);
        }
        public async Task<AuthResponse> RefreshToken(TokenRequest request)
        {
            return await VerifyAndGenerateToken(request);
        }
        public async Task<bool> Enable(Guid id)
        {
            var user = await _unitOfWork.Users.Get(id);
            if (user is null) return false;

            user.Disabled = false;
            await _unitOfWork.Users.Update(user);

            return true;
        }
        public Task<AuthResponse> Logout(Guid id)
        {
            throw new NotImplementedException();
        }


        private async Task<AuthResponse> GenerateJwtToken(User user)
        {
            var signKey = Encoding.ASCII.GetBytes(Settings.JwtSecret);

            // Token specifications
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token identifier
                    new Claim("id", user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("locale", string.IsNullOrWhiteSpace(user.PreferredLanguage) ? "ca" : user.PreferredLanguage)
                }),
                Expires = DateTime.UtcNow.Add(Settings.JwtExpirationTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signKey), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);
            var refreshToken = Guid.NewGuid();

            var userRefreshToken = new UserRefreshToken()
            {
                JwtId = Guid.Parse(token.Id),
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Used = false,
                Revoked = false,
                UserId = user.Id
            };

            await _unitOfWork.UserRefreshTokens.Add(userRefreshToken);

            return new AuthResponse()
            {
                Result = true,
                RefreshToken = refreshToken,
                Token = jwtToken
            };
        }

        private async Task<AuthResponse> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenInVerification = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {

                    // Is the same algorithm?
                    if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return new AuthResponse()
                        {
                            Result = false,
                            Errors = new List<string>() { _localizationService.GetLocalizedString("AuthInvalidAlgorithm") }
                        };
                    }
                }

                // Token is still valid?
                var utcExpiryDate = long.Parse(tokenInVerification.Claims.First(t => t.Type.Equals(JwtRegisteredClaimNames.Exp)).Value);
                var expiryDate = UnixTimeStampToDateTime(utcExpiryDate);

                if (expiryDate > DateTime.UtcNow)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { _localizationService.GetLocalizedString("AuthTokenValid", expiryDate) }
                    };
                }

                // Exist on the persistance layer?
                var storedToken = _unitOfWork.UserRefreshTokens.Find(urt => urt.Token == tokenRequest.RefreshToken).FirstOrDefault();
                if (storedToken is null)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { _localizationService.GetLocalizedString("AuthRefreshTokenNotExist", tokenRequest.RefreshToken) }
                    };
                }

                if (storedToken.Used)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { _localizationService.GetLocalizedString("AuthRefreshTokenUsed") }
                    };
                }

                // Is a revoked token?
                if (storedToken.Revoked)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { _localizationService.GetLocalizedString("AuthRefreshTokenRevoked") }
                    };
                }

                // Has the same identifier?
                var jti = tokenInVerification.Claims.First(t => t.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != Guid.Parse(jti))
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { _localizationService.GetLocalizedString("AuthRefreshTokenMismatch", storedToken.JwtId, jti) }
                    };
                }

                // Refresh token has been expired?
                if (storedToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { _localizationService.GetLocalizedString("AuthTokenExpired") }
                    };
                }

                storedToken.Used = true;
                await _unitOfWork.UserRefreshTokens.Update(storedToken);

                var user = await _unitOfWork.Users.Get(storedToken.UserId);
                if (user is null)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { _localizationService.GetLocalizedString("UserNotFound") }
                    };
                }

                return await GenerateJwtToken(user);
            }
            catch (Exception ex)
            {
                throw new ApiException(ex.Message);
            }
        }

        // Seconds from 1-1-1970 00:00:00
        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTimeValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return dateTimeValue.AddSeconds(unixTimeStamp).ToUniversalTime();
        }
    }
}
