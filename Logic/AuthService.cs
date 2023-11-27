using Microsoft.AspNetCore.Authentication;
using patter_pal.domain.Config;
using System.Security.Claims;

namespace patter_pal.Logic
{
    /// <summary>
    /// Managing data of currently logged in user.
    /// </summary>
    public class AuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppConfig _appConfig;

        public AuthService(IHttpContextAccessor httpContextAccessor, AppConfig appConfig)
        {
            _httpContextAccessor = httpContextAccessor;
            _appConfig = appConfig;
        }

        public async Task<bool> IsLoggedIn() => !string.IsNullOrEmpty(await GetUserId());

        public async Task<string?> GetUserId()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return null;
            }

            AuthenticateResult info = await httpContext.AuthenticateAsync("Cookies");
            return info.Principal?.FindFirstValue(ClaimTypes.Email);
        }

        public async Task Logout()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return;
            }

            await httpContext.SignOutAsync("Cookies");
        }

        public async Task<bool> TrySignInWithSpecialCode(string code)
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || !IsValidSpecialCode(code))
            {
                return false;
            }


            // Code is valid, manually sign in the user
            List<Claim> claims = new()
            {
                // Using email as other users also use email to keep it uniform even though this is a code
                new(ClaimTypes.Email, code) 
            };

            ClaimsIdentity identity = new(claims, "SpecialAccess");
            ClaimsPrincipal principal = new(identity);

            await httpContext.SignInAsync("Cookies", principal);
            return true;
        }

        private bool IsValidSpecialCode(string code)
        {
            return _appConfig.ValidSpecialCodes.Split(";").Any(c => c == code);
        }
    }
}
