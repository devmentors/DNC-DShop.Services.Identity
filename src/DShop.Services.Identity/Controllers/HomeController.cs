using Microsoft.AspNetCore.Mvc;

namespace DShop.Services.Identity.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("")]
        public IActionResult Get() => Ok("DShop Identity Service");
    }
}