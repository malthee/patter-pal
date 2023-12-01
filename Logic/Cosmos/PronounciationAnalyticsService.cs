using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using patter_pal.dataservice.Azure;
using patter_pal.domain.Config;
using patter_pal.domain.Data;
using patter_pal.Logic.Interfaces;
using patter_pal.Models;
using patter_pal.Util;

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

        public async Task<PronounciationAnalyticsModel?> GetPronounciationAnalyticsAsync(string userId, string? language = null, string? timePeriod = null, string? timeResolution = null)
        {
            timePeriod ??= TimePeriodConstants.DefaultTimePeriod;
            timeResolution ??= TimeResolutionConstants.DefaultTimeResolution;
            DateTime daysAgoUtc = TimePeriodToDaysAgoUtc(timePeriod);
            var pronounciationResults = await _cosmosService.GetSpeechPronounciationResultDataAsync(userId, language, daysAgoUtc);
            if (pronounciationResults == null)
            {
                _logger.LogWarning($"No pronounciation results found for user {userId}");
                return null;
            }

            List<WordStatistic>? stats = pronounciationResults
            // Get words of any language when language is not specified, check if this makes sense otherwise ui is empty 
                    .Where(d => language == null || d.Language == language)
                    .SelectMany(d => d.Words)
                    .GroupBy(w => w.Text, w => w)
                    .Select(g => new WordStatistic { Text = g.Key, AverageAccuracy = g.Average(w => w.AccuracyScore) })
                    .OrderBy(ws => ws.AverageAccuracy)
                    .Take(_appConfig.PronounciationAnalyticsMaxWordCount)
                    .ToList();

            // could be "hour", "day", "month"
            string chartDisplayFormatType = GetChartDisplayFormatType(timeResolution);
            var groupedPronunciationResults = pronounciationResults
                .GroupBy(d => new
                {
                    Timestamp = GetGroupedTimestamp(d.Timestamp, chartDisplayFormatType)
                })
                .Select(group => new SpeechAssessmentData
                {
                    Timestamp = group.Key.Timestamp,
                    AccuracyScore = group.Average(d => d.AccuracyScore),
                    CompletenessScore = group.Average(d => d.CompletenessScore),
                    FluencyScore = group.Average(d => d.FluencyScore),
                    PronounciationScore = group.Average(d => d.PronounciationScore)
                }).ToList();

            return new PronounciationAnalyticsModel
            {
                SpeechAssessments = groupedPronunciationResults,
                ChartDisplayFormatType = chartDisplayFormatType,
                ChartDisplayFormat = GetChartDisplayFormat(timeResolution),
                ChartUnit = GetChartUnit(timeResolution),
                BottomTenWords = stats
            };
        }

        private static DateTime GetGroupedTimestamp(DateTime timestamp, string displayFormatType)
        {
            if (displayFormatType == "hour")
            {
                return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0);
            }
            else if (displayFormatType == "day")
            {
                return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, 0, 0, 0);
            }
            else if (displayFormatType == "month")
            {
                return new DateTime(timestamp.Year, timestamp.Month, 1, 0, 0, 0);
            }
            else
            {
                throw new ArgumentException($"unrecognized displayFormatType '{displayFormatType}' used in {nameof(GetGroupedTimestamp)}");
            }
        }

        private static string GetChartDisplayFormatType(string timeResolution)
        {
            return timeResolution switch
            {
                "h" => "hour",
                "d" => "day",
                "m" => "month",
                _ => throw new ArgumentException($"unrecognized time resolution '{timeResolution}' used in {nameof(GetChartDisplayFormatType)}")
            };
        }

        private static string GetChartDisplayFormat(string timeResolution)
        {
            return timeResolution switch
            {
                "h" => "MMM D h A",
                "d" => "MMM D",
                "m" => "MMM",
                _ => throw new ArgumentException($"unrecognized time resolution '{timeResolution}' used in {nameof(GetChartDisplayFormat)}")
            };
        }

        private static string GetChartUnit(string timeResolution)
        {
            return timeResolution switch
            {
                "h" => "hour",
                "d" => "day",
                "m" => "month",
                _ => throw new ArgumentException($"unrecognized time resolution '{timeResolution}' used in {nameof(GetChartDisplayFormat)}")
            };
        }

        private static DateTime TimePeriodToDaysAgoUtc(string timePeriod)
        {
            string[] segments = timePeriod.Split('-');
            int dayMultiplier = segments[0] switch
            {
                "d" => 1,
                "w" => 7,
                "m" => 31, // I know sketchy
                "j" => 365,
                _ => throw new ArgumentException($"unrecognized time period '{segments[0]}' used in {nameof(TimePeriodToDaysAgoUtc)}")
            };
            int daysAgo = int.Parse(segments[1]) * dayMultiplier;
            return DateTime.UtcNow.AddDays(-daysAgo);
        }
    }
}
