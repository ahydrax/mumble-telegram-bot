using System;
using System.Linq;
using System.Threading.Tasks;
using KNFA.Bots.MTB.Services.Mumble;
using Microsoft.AspNetCore.Mvc;

namespace KNFA.Bots.MTB.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMumbleInfo _mumbleInfo;

        public HomeController(IMumbleInfo mumbleInfo)
        {
            _mumbleInfo = mumbleInfo ?? throw new ArgumentNullException(nameof(mumbleInfo));
        }

        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            var users = await _mumbleInfo.GetUsersAsync();
            return View(users.Select(x => x.Username).ToArray());
        }
    }
}
