using System;
using DShop.Common.Types;
using DShop.Messages.Entities;
using Microsoft.AspNetCore.Identity;

namespace DShop.Services.Identity.Domain
{
    public class RefreshToken : IIdentifiable
    {
        public Guid Id { get; protected set; }
        public Guid UserId { get; protected set; }
        public string Token { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? RevokedAt { get; protected set; }
        public bool Revoked => RevokedAt.HasValue;

        protected RefreshToken()
        {
        }

        public RefreshToken(User user, IPasswordHasher<User> passwordHasher)
        {
            Id = Guid.NewGuid();
            UserId = user.Id;
            CreatedAt = DateTime.UtcNow;
            Token = passwordHasher.HashPassword(user, Guid.NewGuid().ToString("N"))
                .Replace("=", string.Empty)
                .Replace("+", string.Empty)
                .Replace("/", string.Empty);
        }

        public void Revoke()
        {
            if (Revoked)
            {
                throw new DShopException(Codes.RefreshTokenAlreadyRevoked, 
                    $"Refresh token: '{Id}' was already revoked at '{RevokedAt}'.");
            }
            RevokedAt = DateTime.UtcNow;
        }
    }
}