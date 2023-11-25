using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using patter_pal.Controllers;
using patter_pal.Logic;
using patter_pal.Util;

/// <summary>
/// Authentification endpoints (currently only through 3rd parties).
/// </summary>
[AllowAnonymous]
public class AuthController : Controller
{
    private readonly UserService _userService;
    private readonly AppConfig _appConfig;

    public AuthController(UserService userService, AppConfig appConfig)
    {
        _userService = userService;
        _appConfig = appConfig;
    }

    [HttpGet]
    public IActionResult ExternalLogin(string provider)
    {
        // Set the callback URL to the ExternalLoginCallback action
        string? redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth");
        AuthenticationProperties properties = new() { RedirectUri = redirectUrl };

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
            return RedirectToAction("Index", "Home");
        }

        // Sign out of the external provider
        await HttpContext.SignOutAsync("Cookies");

        // Redirect to the appropriate page after successful login
        return RedirectToAction("App", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        // Sign out the user
        HttpContext.SignOutAsync();

        // Redirect to the home page or any other page after logout
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult DeleteEverything()
    {
        // TODO delete
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult SpecialAccess(string code)
    {
        if (IsValidSpecialCode(code))
        {
            // Code is valid, manually sign in the user
            List<Claim> claims = new()
            {
                new(ClaimTypes.Email, code) // You can customize the claims as needed
            };

            ClaimsIdentity identity = new(claims, "SpecialAccess");
            ClaimsPrincipal principal = new(identity);

            HttpContext.SignInAsync("Cookies", principal);

            // Redirect to the appropriate page after successful login
            return RedirectToAction("App", "Home");
        }
        else
        {
            // Code is invalid, handle the error (you may want to redirect to an error page)
            return RedirectToAction("Index", "Home");
        }
    }

    public bool IsValidSpecialCode(string code)
    {
        return _appConfig.ValidSpecialCodes.Split(";").Any(c => c == code);
    }
}
