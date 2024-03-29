﻿using Application.Contracts.Auth;

namespace Application.Services
{
    public interface IAuthenticationService
    {
        Task<AuthResponse> Register(UserRegisterRequest request);
        Task<AuthResponse> Login(UserLoginRequest request);
        Task<bool> ChangePassword(UserLoginRequest request);
        Task<AuthResponse> RefreshToken(TokenRequest request);
        Task<bool> Enable(Guid id);
        Task<AuthResponse> Logout(Guid id);
    }
}
