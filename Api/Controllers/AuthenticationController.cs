using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Mapping.Dtos.Authentication;
using Application.Persistance;
using Domain.Entities;
using Api.Exceptions;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public AuthenticationController(UserManager<IdentityUser> userManager, IUnitOfWork unitOfWork, TokenValidationParameters tokenValidationParameters)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _tokenValidationParameters = tokenValidationParameters;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(UserRegistrationRequest requestUser)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Validate existence of the unique user key
            var exists = await _userManager.FindByNameAsync(requestUser.Username);
            if (exists is not null)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { "Email already exists" }
                });
            }

            // Create user on database
            var user = new IdentityUser()
            {
                Email = requestUser.Email,
                UserName = requestUser.Username,
                PhoneNumber = requestUser.PhoneNumber,
            };

            var created = await _userManager.CreateAsync(user, requestUser.Password);
            if (!created.Succeeded) {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    RefreshToken = Guid.Empty,
                    Errors = created.Errors.Select(e => e.ToString() ?? string.Empty).ToList()
                });
            }

            // Response with the authorization token
            var authResponse = await GenerateJwtToken(user);
            return Ok(authResponse);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UserLoginRequest requestUser)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Validate existence of the unique user key
            var dbUser = await _userManager.FindByNameAsync(requestUser.Username);
            if (dbUser is null)
            {
                return NotFound(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Unexisting user"
                    }
                });
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(dbUser, requestUser.Password);
            if (!isPasswordValid)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Unexisting user"
                    }
                });
            }

            var authResponse = await GenerateJwtToken(dbUser);
            return Ok(authResponse);
        }

        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid parameters" }
                });
            }

            var authResponse  = await VerifyAndGenerateToken(tokenRequest);
            if (authResponse is null)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>() { "Invalid token" }
                });
            }

            return Ok(authResponse);
        }

        private async Task<AuthResponse> GenerateJwtToken(IdentityUser user)
        {
            var signKey = Encoding.ASCII.GetBytes(ApplicationConfiguration.JwtSecret);

            // Token specifications
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token identifier
                    new Claim("id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
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
                UserId = Guid.Parse(user.Id)
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
                if (validatedToken is JwtSecurityToken jwtSecurityToken){

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

                // Is a using token?
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

                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
                return await GenerateJwtToken(dbUser);                
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