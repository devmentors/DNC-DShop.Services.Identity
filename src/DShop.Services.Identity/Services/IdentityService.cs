using System;
using System.Threading.Tasks;
using DShop.Common.Authentication;
using DShop.Common.RabbitMq;
using DShop.Common.Types;
using DShop.Messages.Events.Identity;
using DShop.Services.Identity.Domain;
using DShop.Services.Identity.Repositories;
using Microsoft.AspNetCore.Identity;

namespace DShop.Services.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtHandler _jwtHandler;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IBusPublisher _busPublisher;

        public IdentityService(IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IJwtHandler jwtHandler,
            IRefreshTokenRepository refreshTokenRepository,
            IBusPublisher busPublisher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtHandler = jwtHandler;
            _refreshTokenRepository = refreshTokenRepository;
            _busPublisher = busPublisher;
        }

        public async Task SignUpAsync(string email, string password, string role = Role.User)
        {
            var user = await _userRepository.GetAsync(email);
            if (user != null)
            {
                var reason = $"Email: '{email}' is already in use.";
                var code = Codes.EmailInUse;
                await _busPublisher.PublishEventAsync(new SignUpRejected(Guid.NewGuid(), user.Id,
                    reason, code));
                throw new DShopException(code, reason);
            }
            user = new User(email, role);
            user.SetPassword(password, _passwordHasher);
            await _userRepository.AddAsync(user);
            await _busPublisher.PublishEventAsync(new SignedUp(Guid.NewGuid(), user.Id, user.Email));
        }

        public async Task<JsonWebToken> SignInAsync(string email, string password)
        {
            var user = await _userRepository.GetAsync(email);
            if (user == null || !user.ValidatePassword(password, _passwordHasher))
            {
                var reason = "Invalid credentials.";
                var code = Codes.InvalidCredentials;
                await _busPublisher.PublishEventAsync(new SignInRejected(Guid.NewGuid(), user.Id,
                    reason, code));
                throw new DShopException(code, reason);
            }
            var refreshToken = new RefreshToken(user, _passwordHasher);
            var jwt = _jwtHandler.CreateToken(user.Id, user.Role);
            jwt.RefreshToken = refreshToken.Token;
            await _busPublisher.PublishEventAsync(new SignedIn(Guid.NewGuid(), user.Id));

            return jwt;
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
            {
                throw new DShopException(Codes.UserNotFound, 
                    $"User: '{userId}' was not found.");
            }
            if (!user.ValidatePassword(currentPassword, _passwordHasher))
            {
                throw new DShopException(Codes.InvalidCredentials, 
                    "Invalid current password.");
            }
            user.SetPassword(newPassword, _passwordHasher);
            await _userRepository.UpdateAsync(user);            
        }
    }
}