using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DShop.Services.Identity.Services;
using DShop.Messages.Commands.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DShop.Services.Identity.Controllers
{
    [Route("")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokenService _refreshTokenService;

        public IdentityController(IIdentityService identityService,
            IRefreshTokenService refreshTokenService)
        {
            _identityService = identityService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get()
            => Content($"User id: '{User.Identity.Name}'.");

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUp command)
        {
            await _identityService.SignUpAsync(command.Email, command.Password);

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