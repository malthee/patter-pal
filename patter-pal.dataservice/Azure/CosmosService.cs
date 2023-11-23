using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using patter_pal.dataservice.DataObjects;

namespace patter_pal.dataservice.Azure
{
    public class CosmosService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosServiceContainer<ConversationData> _cosmosServiceContainer;
        public CosmosService(string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _cosmosServiceContainer = new CosmosServiceContainer<ConversationData>(_cosmosClient, "db1", "c1", "/UserId");
        }

        public async Task InitializeService()
        {
            await _cosmosServiceContainer.InitializeDatabaseAndContainerAsync();
        }

        public async Task<ConversationData?> GetUserConversationAsync(string userId, string conversationId)
        {
            string query = "SELECT * FROM c1 WHERE c1.id = @p0 and c1.UserId = @p1";
            List<ConversationData>? res = await _cosmosServiceContainer.QueryAsync(query, conversationId, userId);
            return res?.FirstOrDefault();
        }

        public async Task<bool> AddOrUpdateChatConversationDataAsync(ConversationData chatConversation)
        {
            return await _cosmosServiceContainer.AddOrUpdateAsync(chatConversation, (db, arg) =>
            {
                db.Title = arg.Title;
                db.Data = arg.Data;
            });
        }

        public async Task<bool> UpdateConversationTitleAsync(ConversationData chatConversation)
        {
            return await _cosmosServiceContainer.AddOrUpdateAsync(chatConversation, (db, arg) =>
            {
                db.Title = arg.Title;
            });
        }

        public async Task<bool> DeleteConversationAsync(ConversationData data)
        {
            return await _cosmosServiceContainer.DeleteAsync(data);
        }

        public async Task<List<ConversationData>?> GetUserConversationsShallowAsync(string userId)
        {
            string query = "SELECT c1.id, c1.UserId, c1.Title FROM c1 WHERE c1.UserId = @p0";
            return await _cosmosServiceContainer.QueryAsync(query, userId);
        }

    }
}
