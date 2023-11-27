using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic;
using patter_pal.Models;
using System.Diagnostics;

namespace patter_pal.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuthService _authService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, AuthService authService)
        {
            _logger = logger;
            _authService = authService;
        }

        [Authorize(Policy = "LoggedInPolicy")]
        public IActionResult App()
        {
            return View();
        }

        [Authorize(Policy = "LoggedInPolicy")]
        public async Task<IActionResult> Stats()
        {
            bool loggedIn = await _authService.IsLoggedIn();
            if (!loggedIn)
            {
                return RedirectToAction(nameof(Index));
            }

             ViewData["Error"] = TempData["Error"];
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            bool loggedIn = await _authService.IsLoggedIn();
            if (loggedIn)
            {
                return RedirectToAction(nameof(App));
            }

            // Error and success set by other actions
            ViewData["Error"] = TempData["Error"];
            ViewData["Success"] = TempData["Success"];
            ViewData["IsLoggedIn"] = loggedIn;
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Privacy()
        {
            ViewData["IsLoggedIn"] = await _authService.IsLoggedIn();
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var ex = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (ex == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}