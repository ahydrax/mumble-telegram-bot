using System;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Services.Mumble;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KNFA.Bots.MTB.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly IMumbleInfo _mumbleInfo;

        public ApiController(IMumbleInfo mumbleInfo)
        {
            _mumbleInfo = mumbleInfo ?? throw new ArgumentNullException(nameof(mumbleInfo));
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _mumbleInfo.GetUsersAsync();
            return Ok(users);
        }
    }
}
