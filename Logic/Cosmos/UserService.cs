using patter_pal.Controllers;
using patter_pal.dataservice.Azure;
using patter_pal.Logic.Interfaces;

namespace patter_pal.Logic.Cosmos
{
    public class UserService : IUserService
    {
        private readonly CosmosService _cosmosService;
        private readonly ILogger<HomeController> _logger;

        public UserService(CosmosService cosmosService, ILogger<HomeController> logger)
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
