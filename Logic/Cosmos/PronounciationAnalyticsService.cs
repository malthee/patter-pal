using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using patter_pal.dataservice.Azure;
using patter_pal.domain.Config;
using patter_pal.domain.Data;
using patter_pal.Logic.Interfaces;
using patter_pal.Models;

namespace patter_pal.Logic.Cosmos
{
    public class PronounciationAnalyticsService : IPronounciationAnalyticsService
    {
        private readonly ILogger<PronounciationAnalyticsService> _logger;
        private readonly CosmosService _cosmosService;
        private readonly AppConfig _appConfig;

        public PronounciationAnalyticsService(ILogger<PronounciationAnalyticsService> logger, CosmosService cosmosService, AppConfig config)
        {
            _logger = logger;
            _cosmosService = cosmosService;
            _appConfig = config;
        }

        public Task<bool> AddSpeechPronounciationResultDataAsync(string userId, string language, PronunciationAssessmentResult pronounciationResult)
        {
            var speechResultData = new SpeechPronounciationResultData
            {
                Language = language,
                Timestamp = DateTime.UtcNow,
                UserId = userId,
                AccuracyScore = (decimal)pronounciationResult.AccuracyScore,
                FluencyScore = (decimal)pronounciationResult.FluencyScore,
                CompletenessScore = (decimal)pronounciationResult.CompletenessScore,
                PronounciationScore = (decimal)pronounciationResult.PronunciationScore,
                Words = new(pronounciationResult.Words.Select(w => new WordData() { AccuracyScore = (decimal)w.AccuracyScore, ErrorType = w.ErrorType, Text = w.Word }))
            };

            return _cosmosService.AddSpeechPronounciationResultDataAsync(userId, speechResultData);
        }

        public async Task<PronounciationAnalyticsModel?> GetPronounciationAnalyticsAsync(string userId, string? language = null, int? maxDaysAgo = null)
        {
            DateTime? daysAgoUtc = maxDaysAgo != null ? DateTime.UtcNow.AddDays(-maxDaysAgo.Value) : null;
            var pronounciationResults = await _cosmosService.GetSpeechPronounciationResultDataAsync(userId, language, daysAgoUtc);
            if(pronounciationResults == null)
            {
                _logger.LogWarning($"No pronounciation results found for user {userId}");
                return null;
            }

            // TODO fix for mispronounciations, more stats
            List<WordStatistic>? stats = language == null
                ? null
                : pronounciationResults
                    .Where(d => d.Language == language)
                    .SelectMany(d => d.Words)
                    .GroupBy(w => w.Text, w => w)
                    .Select(g => new WordStatistic { Text = g.Key, AverageAccuracy = g.Average(w => w.AccuracyScore) })
                    .OrderBy(ws => ws.AverageAccuracy)
                    .Take(_appConfig.PronounciationAnalyticsMaxWordCount)
                    .ToList();

            return new PronounciationAnalyticsModel
            {
                SpeechAssessments = pronounciationResults.Select(d => new SpeechAssessmentData
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
