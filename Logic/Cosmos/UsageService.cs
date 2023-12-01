using patter_pal.dataservice.Azure;
using patter_pal.Logic.Interfaces;

namespace patter_pal.Logic.Cosmos
{
    public class UsageService : IUsageService
    {
        private readonly CosmosService _cosmosService;

        public UsageService(CosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        public async Task<bool> HasUserRequestsRemainingAsync(string userId)
        {
            return await _cosmosService.HasUserRequestsRemainingAsync(userId);
        }

        public async Task<bool> IncrementUserRequestCounterAsync(string userId)
        {
            return await _cosmosService.IncrementUserRequestCounterAsync(userId);
        }
    }
}
