using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using patter_pal.Controllers;
using patter_pal.Logic;

/// <summary>
/// Authentification endpoints (currently only through 3rd parties).
/// </summary>
public class AuthController : Controller
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult ExternalLogin(string provider)
    {
        // Set the callback URL to the ExternalLoginCallback action
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { provider });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        // Challenge the external provider
        return Challenge(properties, provider);
    }

    [HttpGet]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        // Retrieve the external login information
        var info = await HttpContext.AuthenticateAsync("Cookies");
        if (info == null)
        {
            //TODO: handle error
            //return RedirectToAction(nameof(Login));
            return RedirectToAction(nameof(HomeController.Index));
        }

        // Get the user's email (you can customize this based on your needs)
        string email = info.Principal?.FindFirstValue(ClaimTypes.Email) ?? "";

        await _userService.LoginUser(email);

        // Sign out of the external provider
        await HttpContext.SignOutAsync("Cookies");

        // Redirect to the appropriate page after successful login
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        // Sign out the user
        HttpContext.SignOutAsync();
        _userService.Logout();

        // Redirect to the home page or any other page after logout
        return RedirectToAction("Index", "Home");
    }

}
