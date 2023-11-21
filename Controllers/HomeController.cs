using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic;
using patter_pal.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace patter_pal.Controllers
{
    public class Model
    {
        public string? UserEmail { get; set; }
    }
    
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
            var info = await HttpContext.AuthenticateAsync("Cookies");
            if (info == null)
            {
                //TODO: handle error
                //return RedirectToAction(nameof(Login));
                return RedirectToAction("Index", "Home");
            }

            // Get the user's email (you can customize this based on your needs)
            string email = info.Principal?.FindFirstValue(ClaimTypes.Email) ?? "";

            await _userService.LoginUser(email);
            var model = new Model
            {
                UserEmail = _userService.UserData?.Id
            };
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var model = new Model
            {
                UserEmail = _userService.UserData?.Id
            };
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
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