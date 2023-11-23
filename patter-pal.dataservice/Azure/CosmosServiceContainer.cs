using Microsoft.Azure.Cosmos;
using patter_pal.dataservice.DataObjects;
using Container = Microsoft.Azure.Cosmos.Container;

namespace patter_pal.dataservice.Azure
{
    public class CosmosServiceContainer<T>
        where T : ContainerItem
    {
        private readonly CosmosClient _cosmosClient;
        
        public readonly string DatabaseName;
        public readonly string ContainerName;
        public readonly string PartitionKey;

        public CosmosServiceContainer(CosmosClient cosmosClient, string databaseName, string containerName, string partitionKey)
        {
            _cosmosClient = cosmosClient;
            DatabaseName = databaseName;
            ContainerName = containerName;
            PartitionKey = partitionKey;
        }

        public async Task InitializeDatabaseAndContainerAsync()
        {
            DatabaseResponse database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseName);
            await database.Database.CreateContainerIfNotExistsAsync(ContainerName, PartitionKey);
        }

        public Container GetContainer()
        {
            return _cosmosClient.GetContainer(DatabaseName, ContainerName);
        }

        
        public async Task<T> AddOrUpdateAsync(T data, Action<T, T> modificationFunc)
        {
            Container container = GetContainer();

            try
            {
                // Attempt to read the item from Cosmos DB
                ItemResponse<T> existingItem = await container.ReadItemAsync<T>(
                    data.Id,
                    new PartitionKey(data.Email));

                // If the item exists, update it
                modificationFunc(existingItem.Resource, data);

                ItemResponse<T> response = await container.ReplaceItemAsync(
                    existingItem.Resource,
                    existingItem.Resource.Id,
                    new PartitionKey(data.Email));

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If the item does not exist, create a new one
                ItemResponse<T> response = await container.CreateItemAsync(
                    data,
                    new PartitionKey(data.Email));

                return response.Resource;
            }
        }

        public async Task DeleteAsync(T data)
        {
            Container container = GetContainer();

            try
            {
                ItemResponse<T> deletedItem = await container.DeleteItemAsync<T>(
                    data.Id,
                    new PartitionKey(data.Email));

                return;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // TODO: log
                return;
            }
        }

        public async Task<List<T>> QueryAsync(string query, params string[] ps)
        {
            Container container = GetContainer();
            QueryDefinition queryDefinition = new(query);

            for (int i = 0; i < ps.Length; i++)
            {
                queryDefinition = queryDefinition.WithParameter($"@p{i}", ps[i]);
            }
            using FeedIterator<T> feedIterator = container.GetItemQueryIterator<T>(queryDefinition);

            List<T> res = new();
            while (feedIterator.HasMoreResults)
            {
                FeedResponse<T> response = await feedIterator.ReadNextAsync();
                response.ToList().ForEach(res.Add);
            }

            return res;
        }
    }
}