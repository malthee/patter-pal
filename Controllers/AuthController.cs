using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using patter_pal.Controllers;
using patter_pal.domain.Config;
using patter_pal.Logic;
using patter_pal.Logic.Interfaces;
using static patter_pal.Util.ControllerHelper;

/// <summary>
/// Authentication endpoints (currently only through 3rd parties and special access codes).
/// </summary>
[AllowAnonymous]
public class AuthController : Controller
{
    private readonly AuthService _authService;
    private readonly IUserService _userService;
    private readonly AppConfig _appConfig;

    public AuthController(AuthService authService, IUserService userService, AppConfig appConfig)
    {
        _authService = authService;
        _userService = userService;
        _appConfig = appConfig;
    }

    [HttpGet]
    public IActionResult ExternalLogin(string provider)
    {
        // Set the callback URL to the ExternalLoginCallback action
        string? redirectUrl = Url.Action(nameof(ExternalLoginCallback));
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
            TempData["Error"] = "Unsuccessful login.";
            return RedirectToAction(nameof(HomeController.Index), GetControllerName<HomeController>());
        }

        // Sign out of the external provider
        await _authService.Logout();

        return RedirectToAction(nameof(HomeController.App), GetControllerName<HomeController>());
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Sign out the user
        await _authService.Logout();

        return RedirectToAction(nameof(HomeController.Index), GetControllerName<HomeController>());
    }

    [HttpPost]
    [Authorize(Policy = "LoggedInPolicy")]
    public async Task<IActionResult> DeleteEverything()
    {
        string? userId = await _authService.GetUserId();
        if (userId == null)
        {
            TempData["Error"] = "You are not logged in.";
        }
        else if (!await _userService.DeleteAllUserData(userId)) {
            TempData["Error"] = "Could not delete data, try again later.";
        }

        TempData["Success"] = "Deleted your data.";
        await _authService.Logout();
        return RedirectToAction(nameof(HomeController.Index), GetControllerName<HomeController>());
    }

    [HttpPost]
    public async Task<IActionResult> SpecialAccess(string code)
    {
        if (await _authService.TrySignInWithSpecialCode(code))
        {
            return RedirectToAction(nameof(HomeController.App), GetControllerName<HomeController>());
        }

        // Code is invalid, handle the error (you may want to redirect to an error page)
        TempData["Error"] = "Invalid access code";
        return RedirectToAction(nameof(HomeController.Index), GetControllerName<HomeController>());
    }
}
