using Azure;
using Microsoft.Azure.Cosmos;
using patter_pal.dataservice.DataObjects;

namespace patter_pal.dataservice.Azure
{
    public class CosmosService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosServiceContainer<UserJourneyData> _cosmosServiceContainer;
        public CosmosService(string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _cosmosServiceContainer = new CosmosServiceContainer<UserJourneyData>(_cosmosClient, "db1", "c1", "/Email");
        }

        public async Task InitializeService()
        {
            await _cosmosServiceContainer.InitializeDatabaseAndContainerAsync();
        }

        public async Task<List<UserJourneyData>> GetUserData(string email)
        {
            string query = "SELECT * FROM c1 where c1.Email = @p0";
            return await _cosmosServiceContainer.QueryAsync(query, email);
        }

        public async Task<UserJourneyData> UpsertUserJourneyDataAsync(UserJourneyData userJourneyData)
        {
            return await _cosmosServiceContainer.AddOrUpdateAsync(userJourneyData, (db, arg) =>
            {
                db.JourneyDetails = arg.JourneyDetails;
            });
        }

        public async Task DeleteUserDataAsync(UserJourneyData data)
        {
            await _cosmosServiceContainer.DeleteAsync(data);
            return;
        }

        public async Task<UserJourneyData?> TryGetUserJourneyDataAsync(string email)
        {
            string query = "SELECT * FROM c1 WHERE c1.Email = @p0";
            var res = await _cosmosServiceContainer.QueryAsync(query, email);
            return res.FirstOrDefault();
        }

    }
}
