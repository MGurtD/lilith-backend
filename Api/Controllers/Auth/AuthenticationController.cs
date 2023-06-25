using Microsoft.AspNetCore.Mvc;
using Application.Dto;
using Application.Services;
using Application.Contracts.Purchase;

namespace Api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Use the authentication service register
            var authResponse = await _authenticationService.Register(request);
            return authResponse.Result ? Ok(authResponse) : BadRequest(authResponse);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(UserLoginRequest requestUser)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Use the authentication service register
            var authResponse = await _authenticationService.Login(requestUser);
            return authResponse.Result ? Ok(authResponse) : BadRequest(authResponse);
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(UserLoginRequest requestUser)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest();

            // Use the authentication service register
            var changed = await _authenticationService.ChangePassword(requestUser);
            return changed ? Ok() : StatusCode(StatusCodes.Status500InternalServerError);
        }

        // TODO > Implementar refresh tokens quan hagi separat les responsabilitats de autenticaci� i autoritzaci�
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

            var authResponse = await _authenticationService.RefreshToken(tokenRequest);
            return authResponse.Result ? Ok(authResponse) : BadRequest(authResponse);
        }

    }
}