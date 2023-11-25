using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using patter_pal.dataservice.DataObjects;
using System.Diagnostics;

namespace patter_pal.dataservice.Azure
{
    public class CosmosService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosServiceContainer<ConversationData> _cosmosServiceContainerConversations;
        private readonly CosmosServiceContainer<SpeechPronounciationResultData> _cosmosServiceContainerSpeech;
        public CosmosService(string connectionString, string cosmosDbDb1, string cosmosDbDb1C1, string cosmosDbDb1C1Pk, string cosmosDbDb1C2, string cosmosDbDb1C2Pk)
        {
            _cosmosClient = new CosmosClient(connectionString);
            _cosmosServiceContainerConversations = new CosmosServiceContainer<ConversationData>(_cosmosClient, cosmosDbDb1, cosmosDbDb1C1, cosmosDbDb1C1Pk);
            _cosmosServiceContainerSpeech = new CosmosServiceContainer<SpeechPronounciationResultData>(_cosmosClient, cosmosDbDb1, cosmosDbDb1C2, cosmosDbDb1C2Pk);
        }

        public async Task InitializeService()
        {
            await _cosmosServiceContainerConversations.InitializeDatabaseAndContainerAsync();
            await _cosmosServiceContainerSpeech.InitializeDatabaseAndContainerAsync();
        }
        
        public async Task<ConversationData?> GetUserConversationAsync(string userId, string conversationId)
        {
            string query = "SELECT * FROM c1 WHERE c1.id = @p0 and c1.UserId = @p1";
            List<ConversationData>? res = await _cosmosServiceContainerConversations.QueryAsync(query, conversationId, userId);
            return res?.FirstOrDefault();
        }

        public async Task<bool> AddOrUpdateChatConversationDataAsync(ConversationData chatConversation)
        {
            return await _cosmosServiceContainerConversations.AddOrUpdateAsync(chatConversation, (db, arg) =>
            {
                db.Title = arg.Title;
                db.Data = arg.Data;
            });
        }

        public async Task<bool> UpdateConversationTitleAsync(ConversationData chatConversation)
        {
            return await _cosmosServiceContainerConversations.AddOrUpdateAsync(chatConversation, (db, arg) =>
            {
                db.Title = arg.Title;
            });
        }

        public async Task<bool> DeleteConversationAsync(ConversationData data)
        {
            return await _cosmosServiceContainerConversations.DeleteAsync(data);
        }

        public async Task<List<ConversationData>?> GetUserConversationsShallowAsync(string userId)
        {
            string query = "SELECT c1.id, c1.UserId, c1.Title FROM c1 WHERE c1.UserId = @p0";
            return await _cosmosServiceContainerConversations.QueryAsync(query, userId);
        }

        //TODO:
        // Add SpeechPronounciationResultData(SpeechPronounciationResultData data)
        // Delete AllUserData(string userId)

        public async Task<List<SpeechPronounciationResultData>?> GetSpeechPronounciationResultDataAsync(string userId, string? language = null)
        {
            if (language is null)
            {
                string query = "SELECT * FROM c2 WHERE c2.UserId = @p0";
                return await _cosmosServiceContainerSpeech.QueryAsync(query, userId);
            }
            else
            {
                string query = "SELECT * FROM c2 WHERE c2.UserId = @p0 AND c2.Language = @p1";
                return await _cosmosServiceContainerSpeech.QueryAsync(query, userId, language);
            }
        }

        public async Task<bool> AddSpeechPronounciationResultDataAsync(string userId, SpeechPronounciationResultData data)
        {
            data.Id = Guid.NewGuid().ToString();
            data.UserId = userId;
            return await _cosmosServiceContainerSpeech.AddOrUpdateAsync(data, (db, arg) =>
            {
                
            });
        }

        public async Task<bool> DeleteAllUserData(string userId)
        {
            return 
                await _cosmosServiceContainerConversations.DeletePartitionAsync(userId) && 
                await _cosmosServiceContainerSpeech.DeletePartitionAsync(userId);
        }

    }
}
