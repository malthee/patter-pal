using Azure;
using Microsoft.Azure.Cosmos;
using patter_pal.dataservice.DataObjects;

namespace patter_pal.dataservice.Azure
{
    public class CosmosService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosServiceContainer<ChatConversationData> _cosmosServiceContainer;
        public CosmosService(string connectionString)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _cosmosServiceContainer = new CosmosServiceContainer<ChatConversationData>(_cosmosClient, "db1", "c1", "/UserId");
        }

        public async Task InitializeService()
        {
            await _cosmosServiceContainer.InitializeDatabaseAndContainerAsync();
        }

        public async Task<ChatConversationData?> GetUserConversationAsync(string id)
        {
            string query = "SELECT * FROM c1 where c1.id = @p0";
            List<ChatConversationData> res = await _cosmosServiceContainer.QueryAsync(query, id.ToString());
            return res.FirstOrDefault();
        }

        public ChatConversationData CreateNewConversation(string userId, string title)
        {
            return ChatConversationData.NewConversation(userId, title);
        }

        public async Task<ChatConversationData> AddOrUpdateChatConversationDataAsync(ChatConversationData chatConversation)
        {
            return await _cosmosServiceContainer.AddOrUpdateAsync(chatConversation, (db, arg) =>
            {
                db.Data = arg.Data;
            });
        }

        public async Task DeleteConversationAsync(ChatConversationData data)
        {
            await _cosmosServiceContainer.DeleteAsync(data);
            return;
        }

        public async Task<List<ChatConversationData>> GetUserConversationsShallowAsync(string userId)
        {
            string query = "SELECT c1.id, c1.UserId, c1.Title FROM c1 WHERE c1.UserId = @p0";
            var res = await _cosmosServiceContainer.QueryAsync(query, userId);
            return res;
        }

    }
}
