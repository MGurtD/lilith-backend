using Api.Mapping.Dtos.Authentication;
using Application.Dtos;

namespace Application.Services
{
    public interface IAuthenticationService
    {
        Task<AuthResponse> Register(UserDto request);
        Task<AuthResponse> Login(UserLoginRequest request);
        Task<AuthResponse> RefreshToken(TokenRequest request);
        Task<bool> Enable(Guid id);
        Task<AuthResponse> Logout(Guid id);
    }
}
