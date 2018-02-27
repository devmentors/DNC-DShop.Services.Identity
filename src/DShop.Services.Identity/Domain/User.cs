using System;
using DShop.Common.Types;
using DShop.Messages.Entities;
using Microsoft.AspNetCore.Identity;

namespace DShop.Services.Identity.Domain
{
    public class User : IIdentifiable
    {
        public Guid Id { get; protected set; }
        public string Email { get; protected set; }
        public string Role { get; protected set; }
        public string PasswordHash { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime UpdatedAt { get; protected set; }

        protected User()
        {
        }

        public User(Guid id, string email, string role)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new DShopException(Codes.InvalidEmail, 
                    "Email can not be empty.");
            }
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new DShopException(Codes.InvalidRole, 
                    "Role can not be empty.");
            }        
            Id = id;
            Email = email.ToLowerInvariant();
            Role = role;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetPassword(string password, IPasswordHasher<User> passwordHasher)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new DShopException(Codes.InvalidPassword, 
                    "Password can not be empty.");
            }             
            PasswordHash = passwordHasher.HashPassword(this, password);
        }

        public bool ValidatePassword(string password, IPasswordHasher<User> passwordHasher)
            => passwordHasher.VerifyHashedPassword(this, PasswordHash, password) != PasswordVerificationResult.Failed;
    }
}