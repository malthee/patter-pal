using patter_pal.dataservice.DataObjects;
using patter_pal.Models;

namespace patter_pal.Logic.Interfaces
{
    public interface IPronounciationAnalyticsService
    {
        Task<bool> AddSpeechPronounciationResultDataAsync(string userId, SpeechPronounciationResultData data);

        Task<PronounciationAnalytics?> GetPronounciationAnalyticsAsync(
            string userId,
            string? language = null,
            int? maxDaysAgo = null);
    }
}
