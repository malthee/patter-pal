using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic;
using patter_pal.Models;
using System.Diagnostics;

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

        public IActionResult Index()
        {
            var model = new Model
            {
                UserEmail = _userService.UserData?.Email
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}