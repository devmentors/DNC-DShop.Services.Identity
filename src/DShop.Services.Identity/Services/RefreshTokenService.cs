using System;
using System.Threading.Tasks;
using DShop.Common.Authentication;
using DShop.Common.Types;
using DShop.Services.Identity.Domain;
using DShop.Services.Identity.Repositories;
using Microsoft.AspNetCore.Identity;

namespace DShop.Services.Identity.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtHandler _jwtHandler;
        private readonly IPasswordHasher<User> _passwordHasher;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository,
            IUserRepository userRepository,
            IJwtHandler jwtHandler,
            IPasswordHasher<User> passwordHasher)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _jwtHandler = jwtHandler;
            _passwordHasher = passwordHasher;
        }

        public async Task CreateAsync(Guid userId)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new DShopException(Codes.UserNotFound, 
                    $"User: '{userId}' was not found.");
            }
            await _refreshTokenRepository.AddAsync(new RefreshToken(user, _passwordHasher));
        }

        public async Task<JsonWebToken> CreateAccessTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetAsync(token);
            if (refreshToken == null)
            {
                throw new DShopException(Codes.RefreshTokenNotFound, 
                    "Refresh token was not found.");
            }
            if (refreshToken.Revoked)
            {
                throw new DShopException(Codes.RefreshTokenAlreadyRevoked, 
                    $"Refresh token: '{refreshToken.Id}' was revoked.");
            }
            var user = await _userRepository.GetAsync(refreshToken.UserId);
            if (user == null)
            {
                throw new DShopException(Codes.UserNotFound, 
                    $"User: '{refreshToken.UserId}' was not found.");
            }
            var jwt = _jwtHandler.CreateToken(user.Id, user.Role);
            jwt.RefreshToken = refreshToken.Token;
            
            return jwt;
        }

        public async Task RevokeAsync(string token, Guid userId)
        {
            var refreshToken = await _refreshTokenRepository.GetAsync(token);
            if (refreshToken == null || refreshToken.UserId != userId)
            {
                throw new DShopException(Codes.RefreshTokenNotFound, 
                    "Refresh token was not found.");
            }
            refreshToken.Revoke();
            await _refreshTokenRepository.UpdateAsync(refreshToken);
        }
    }
}