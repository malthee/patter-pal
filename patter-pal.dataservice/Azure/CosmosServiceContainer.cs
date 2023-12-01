using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using patter_pal.domain.Data;
using Container = Microsoft.Azure.Cosmos.Container;

namespace patter_pal.dataservice.Azure
{
    /// <summary>
    /// A wrapper around the CosmosClient to provide a typed interface to the Cosmos DB.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CosmosServiceContainer<T>
        where T : ContainerItem
    {
        private readonly ILogger<CosmosServiceContainer<T>> _logger;
        private readonly CosmosClient _cosmosClient;

        public readonly string DatabaseName;
        public readonly string ContainerName;
        public readonly string PartitionKey;

        public CosmosServiceContainer(ILogger<CosmosServiceContainer<T>> logger, CosmosClient cosmosClient, string databaseName, string containerName, string partitionKey)
        {
            _logger = logger;
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

        public async Task<bool> AddOrUpdateAsync<T>(T data, Action<T, T>? modificationFunc = null)
            where T : ContainerItem
        {
            Container container = GetContainer();

            try
            {
                // Attempt to read the item from Cosmos DB
                ItemResponse<T> existingItem = await container.ReadItemAsync<T>(
                    data.Id,
                    new PartitionKey(data.UserId));

                // If the item exists, update it
                modificationFunc?.Invoke(existingItem.Resource, data);

                await container.ReplaceItemAsync(
                    existingItem.Resource,
                    existingItem.Resource.Id,
                    new PartitionKey(data.UserId));

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // If the item does not exist, create a new one
                try
                {
                    await container.CreateItemAsync(data, new PartitionKey(data.UserId));
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to Add in AddOrUpdateAsync ${data.Id}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to Update in AddOrUpdateAsync ${data.Id}");
            }

            return false;
        }

        public async Task<bool> DeleteAsync(T data)
        {
            Container container = GetContainer();

            try
            {
                await container.DeleteItemAsync<T>(
                    data.Id,
                    new PartitionKey(data.UserId));

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, $"Could not find ${data.Id} to delete");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to DeleteAsync ${data.Id}");
            }

            return false;
        }

        public async Task<bool> DeletePartitionAsync(string partitionKey)
        {
            Container container = GetContainer();

            try
            {
                string query = $"SELECT * FROM {ContainerName} WHERE {ContainerName}.{PartitionKey[1..]} = @pk";
                var queryDefinition = new QueryDefinition(query).WithParameter("@pk", partitionKey);
                using FeedIterator<T> feedIterator = container.GetItemQueryIterator<T>(queryDefinition);

                List<T> res = new();
                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<T> response = await feedIterator.ReadNextAsync();
                    response.ToList().ForEach(async (i) => await container.DeleteItemAsync<T>(i.Id, new PartitionKey(partitionKey)));
                }

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, $"Could not partition ${partitionKey} to delete");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to DeletePartitionAsync ${partitionKey}");
            }

            return false;
        }

        public async Task<List<T>?> QueryAsync<T>(string query, params object[] ps)
            where T : ContainerItem
        {
            Container container = GetContainer();

            try
            {
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
            catch (Exception e)
            {
                _logger.LogError(e, $"Query threw exception ${query}");
            }

            return null;
        }
    }
}