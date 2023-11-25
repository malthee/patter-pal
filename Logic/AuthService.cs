using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace patter_pal.Logic
{
    /// <summary>
    /// Managing data of currently logged in user.
    /// </summary>
    public class AuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
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
    }
}
