using System;
using System.Threading.Tasks;
using DShop.Common.Authentication;

namespace DShop.Services.Identity.Services
{
    public interface IRefreshTokenService
    {
        Task CreateAsync(Guid userId);
        Task<JsonWebToken> CreateAccessTokenAsync(string refreshToken);
        Task RevokeAsync(string refreshToken, Guid userId);
    }
}