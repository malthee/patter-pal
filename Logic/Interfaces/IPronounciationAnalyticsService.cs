using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using patter_pal.Models;

namespace patter_pal.Logic.Interfaces
{
    public interface IPronounciationAnalyticsService
    {
        /// <summary>
        /// Creates a new SpeechPronounciationResultData from a PronunciationAssessmentResult 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="pronounciationResult"></param>
        /// <returns></returns>
        Task<bool> AddSpeechPronounciationResultDataAsync(string userId, string language, PronunciationAssessmentResult pronounciationResult);

        Task<PronounciationAnalytics?> GetPronounciationAnalyticsAsync(
            string userId,
            string? language = null,
            int? maxDaysAgo = null);
    }
}
