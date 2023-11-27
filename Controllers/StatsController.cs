using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using patter_pal.Logic;
using patter_pal.Logic.Interfaces;
using patter_pal.Models;
using patter_pal.Util;

namespace patter_pal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Policy = "LoggedInPolicy")]
    public class StatsController : ControllerBase
    {
        private readonly IPronounciationAnalyticsService _pronounciationService;
        private readonly AuthService _authService;

        public StatsController(IPronounciationAnalyticsService pronounciationService, AuthService authService)
        {
            _pronounciationService = pronounciationService;
            _authService = authService;
        }


        [HttpGet]
        public async Task<ActionResult<PronounciationAnalyticsModel>> GetAnalytics(string? language = null, string? timePeriod = null, string? timeResolution = null)
        {
            string userId = (await _authService.GetUserId())!;
            if (language is not null && language == LanguageConstants.LanguageAll)
            {
                language = null;
            }

            var result = await _pronounciationService.GetPronounciationAnalyticsAsync(userId, language, timePeriod, timeResolution);
            if(result is null) return NotFound();

            return result;
        }
    }
}
