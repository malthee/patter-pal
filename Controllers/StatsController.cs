using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using patter_pal.dataservice.Azure;
using patter_pal.dataservice.DataObjects;
using patter_pal.Logic;
using patter_pal.Models;

namespace patter_pal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Policy = "LoggedInPolicy")]
    public class StatsController : ControllerBase
    {
        private readonly CosmosService _cosmosService;
        private readonly UserService _userService;

        public StatsController(CosmosService cosmosService, UserService userService)
        {
            _cosmosService = cosmosService;
            _userService = userService;
        }


        [HttpGet("Data")]
        public async Task<ActionResult<SpeechPronounciationResultModel>> Data(string? language = null)
        {
            string? userId = await _userService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            if (language is not null && language == "All")
            {
                language = null;
            }

            var data = await _cosmosService.GetSpeechPronounciationResultDataAsync(userId, language);
            var stats = CreateStats(data, language);
            return Ok(stats);
        }

        private static SpeechPronounciationResultModel CreateStats(List<SpeechPronounciationResultData> data, string? language)
        {
            List<WordStatistic>? stats = language == null
                ? null
                : data
                    .Where(d => d.Language == language)
                    .SelectMany(d => d.Words)
                    .GroupBy(w => w.Text, w => w)
                    .Select(g => new WordStatistic { Text = g.Key, AverageAccuracy = g.Average(w => w.AccuracyScore) })
                    .OrderBy(ws => ws.AverageAccuracy)
                    .Take(10)
                    .ToList();

            return new SpeechPronounciationResultModel
            {
                SpeechAssessments = data.Select(d => new SpeechAssessmentData
                {
                    Timestamp = d.Timestamp,
                    AccuracyScore = d.AccuracyScore,
                    CompletenessScore = d.CompletenessScore,
                    FluencyScore = d.FluencyScore,
                    Language = d.Language,
                    PronounciationScore = d.PronounciationScore
                }).ToList(),
                BottomTenWords = stats
            };
        }
    }
}
