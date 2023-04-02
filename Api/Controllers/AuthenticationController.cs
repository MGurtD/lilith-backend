using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(UserManager<IdentityUser> userManager, IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;

            _logger.LogDebug("NLog injected into HomeController");
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(UserRegistrationRequest requestUser)
        {
            _logger.LogInformation("Register request from");

            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Validate existence of the unique user key
            var exists = await _userManager.FindByNameAsync(requestUser.Username);
            if (exists is not null)
            {
                return BadRequest(new AuthResponse()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Email already exists"
                    }
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
                    Errors = new List<string>()
                    {
                        "Could not insert the user on the database"
                    }
                });
            }

            // Response with the authorization token
            var token = GenerateJwtToken(user);
            return Ok(new AuthResponse()
            {
                Token = token,
                Result = true
            });
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

            var jwtToken = GenerateJwtToken(dbUser);
            return Ok(new AuthResponse()
            {
                Result = true,
                Token = jwtToken
            });
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var signKey = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtConfig:Secret").Value);

            // Token specifications
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                }),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(signKey), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            return jwtToken;
        }

    }
}