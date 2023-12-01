using patter_pal.Controllers;
using patter_pal.dataservice.Azure;
using patter_pal.Logic.Interfaces;

namespace patter_pal.Logic.Cosmos
{
    public class UserService : IUserService
    {
        private readonly CosmosService _cosmosService;
        private readonly ILogger<UserService> _logger;

        public UserService(CosmosService cosmosService, ILogger<UserService> logger)
        {
            _cosmosService = cosmosService;
            _logger = logger;
        }

        public Task<bool> DeleteAllUserData(string userId)
        {
            _logger.LogInformation($"Deleting all user data for user {userId}");
            return _cosmosService.DeleteAllUserData(userId);
        }
    }
}
