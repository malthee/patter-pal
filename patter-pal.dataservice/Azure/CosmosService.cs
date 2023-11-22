using Microsoft.Azure.Cosmos;
using patter_pal.dataservice.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.dataservice.Azure
{
    public class CosmosService
    {
        private CosmosClient _cosmosClient;
        private readonly string _databaseName = "db1";
        private readonly string _containerName = "c1";
        public CosmosService(string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
            Console.WriteLine("aaa");
        }

        public async Task InitializeDatabaseAndContainerAsync()
        {
            DatabaseResponse database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);
            await database.Database.CreateContainerIfNotExistsAsync(_containerName, "/Email");
        }

        public async Task<UserJourneyData> UpsertUserJourneyDataAsync(UserJourneyData userJourneyData)
        {
            var container = _cosmosClient.GetContainer(_databaseName, _containerName);

            try
            {
                // Attempt to read the item from Cosmos DB
                ItemResponse<UserJourneyData> existingItem = await container.ReadItemAsync<UserJourneyData>(
                    userJourneyData.Id,
                    new PartitionKey(userJourneyData.Email));

                // If the item exists, update it
                existingItem.Resource.JourneyDetails = userJourneyData.JourneyDetails;

                ItemResponse<UserJourneyData> response = await container.ReplaceItemAsync(
                    existingItem.Resource,
                    existingItem.Resource.Id,
                    new PartitionKey(userJourneyData.Email));

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If the item does not exist, create a new one
                ItemResponse<UserJourneyData> response = await container.CreateItemAsync(
                    userJourneyData,
                    new PartitionKey(userJourneyData.Email));

                return response.Resource;
            }
        }

        public async Task<UserJourneyData?> TryGetUserJourneyDataAsync(string email)
        {
            var container = _cosmosClient.GetContainer(_databaseName, _containerName);

            try
            {
                var response = await container.ReadItemAsync<UserJourneyData>(
                    email,
                    new PartitionKey(email));

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If the item is not found, return null
                return null;
            }
        }

    }
}
