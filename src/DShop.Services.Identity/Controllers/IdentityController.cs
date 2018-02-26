using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DShop.Services.Identity.Services;
using DShop.Messages.Commands.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DShop.Common.RabbitMq;
using DShop.Messages.Events.Identity;
using System;

namespace DShop.Services.Identity.Controllers
{
    [Route("")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IBusPublisher _busPublisher;

        public IdentityController(IIdentityService identityService,
            IRefreshTokenService refreshTokenService,
            IBusPublisher busPublisher)
        {
            _identityService = identityService;
            _refreshTokenService = refreshTokenService;
            _busPublisher = busPublisher;
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get()
            => Content($"User id: '{User.Identity.Name}'.");

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUp command)
        {
            var id = Guid.NewGuid();
            await _identityService.SignUpAsync(id, command.Email, command.Password);
            await _busPublisher.PublishEventAsync(new SignedUp(Guid.NewGuid(), id,
                command.Email,command.FirstName, command.LastName, command.Address));

            return Created("me", null);
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignIn command)
            => Ok(await _identityService.SignInAsync(command.Email, command.Password));

        [HttpPost("refresh-tokens/{refreshToken}/refresh")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
            => Ok(await _refreshTokenService.CreateAccessTokenAsync(refreshToken));
    }
}