using System;
using System.Linq;
using KNFA.Bots.MTB.Services.Mumble;
using Microsoft.AspNetCore.Mvc;

namespace KNFA.Bots.MTB.Controllers
{
    public class HomeController : Controller
    {
        private readonly EventProtocol _eventProtocol;

        public HomeController(EventProtocol eventProtocol)
        {
            _eventProtocol = eventProtocol ?? throw new ArgumentNullException(nameof(eventProtocol));
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            var users = _eventProtocol.GetUsers().Select(x => x.Name).ToArray();
            return View(users);
        }
    }
}
