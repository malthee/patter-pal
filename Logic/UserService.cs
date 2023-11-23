using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using patter_pal.dataservice.Azure;
using patter_pal.dataservice.DataObjects;
using patter_pal.dataservice.Interfaces;
using System.Security.Claims;

namespace patter_pal.Logic
{
    /// <summary>
    /// Managing data of currently logged in user.
    /// </summary>
    public class UserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
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

    }
}
