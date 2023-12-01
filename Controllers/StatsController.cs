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
        private readonly ILogger<StatsController> _logger;

        public StatsController(IPronounciationAnalyticsService pronounciationService, AuthService authService, ILogger<StatsController> logger)
        {
            _pronounciationService = pronounciationService;
            _authService = authService;
            _logger = logger;
        }


        [HttpGet]
        public async Task<ActionResult<PronounciationAnalyticsModel>> GetAnalytics(string? language = null, string? timePeriod = null, string? timeResolution = null)
        {
            string userId = (await _authService.GetUserId())!;
            _logger.LogDebug($"Getting analytics for user ${userId}");

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
