using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace user_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet("check-me")]
        [Authorize]
        public IActionResult CheckMe()
        {
            var userId = User.FindFirst(OpenIddictConstants.Claims.Subject)?.Value;
            var name = User.Identity?.Name;
            return Ok(new { userId, name });
        }

    }
}
