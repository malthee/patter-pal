using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic;
using patter_pal.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace patter_pal.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserService _userService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, UserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [Authorize(Policy = "LoggedInPolicy")]
        public async Task<IActionResult> App()
        {
            // TODO app with conversationid, get user id from claims etc.
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            bool loggedIn = await _userService.IsLoggedIn();
            if (loggedIn)
            {
                return RedirectToAction(nameof(App));
            }

            if (TempData["Error"] != null)
            {
                ViewData["Error"] = TempData["Error"];
            }

            ViewData["IsLoggedIn"] = loggedIn;
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Privacy()
        {
            ViewData["IsLoggedIn"] = await _userService.IsLoggedIn();
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}