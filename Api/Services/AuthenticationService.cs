using Api.Exceptions;
using Api.Mapping.Dtos.Authentication;
using Application.Dtos;
using Application.Persistance;
using Application.Services;
using AutoMapper;
using Domain.Entities;
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
        private readonly IMapper _mapper;

        public AuthenticationService(IUnitOfWork unitOfWork, IMapper mapper, TokenValidationParameters tokenValidationParameters)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenValidationParameters = tokenValidationParameters;
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
                    Errors = new List<string>() { $"El nom d'usuari {request.Username} no está disponible." }
                };
            }

            request.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(request.Password);

            var user = _mapper.Map<User>(request);
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
                    Errors = new List<string>() { $"L'usuari {request.Username} no existeix." }
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
                        "La contrasenya no es vàlida."
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
                        $"L'usuari ${request.Username} está deshabilitat."
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
            var signKey = Encoding.ASCII.GetBytes(ApplicationConfiguration.JwtSecret);

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
                }),
                Expires = DateTime.UtcNow.Add(ApplicationConfiguration.JwtExpirationTime),
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
                            Errors = new List<string>() { "Invalid algorithm" }
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
                        Errors = new List<string>() { $"Token is valid until {expiryDate}" }
                    };
                }

                // Exist on the persistance layer?
                var storedToken = _unitOfWork.UserRefreshTokens.Find(urt => urt.Token == tokenRequest.RefreshToken).FirstOrDefault();
                if (storedToken is null)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { $"Refresh token '{tokenRequest.RefreshToken}' does not exist" }
                    };
                }

                if (storedToken.Used)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { $"Refresh token used" }
                    };
                }

                // Is a revoked token?
                if (storedToken.Revoked)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { $"Refresh token revoked" }
                    };
                }

                // Has the same identifier?
                var jti = tokenInVerification.Claims.First(t => t.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != Guid.Parse(jti))
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { $"The unique key of the refresh token does not match. Stored: {storedToken.JwtId} != Requested: {jti}" }
                    };
                }

                // Refresh token has been expired?
                if (storedToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResponse()
                    {
                        Result = false,
                        Errors = new List<string>() { $"Expired token" }
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
                        Errors = new List<string>() { $"User not found" }
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
