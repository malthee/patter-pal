using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using patter_pal.domain.Config;
using patter_pal.domain.Data;

namespace patter_pal.dataservice.Azure
{
    public class CosmosService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly CosmosServiceContainer<ConversationData> _cosmosServiceContainerConversations;
        private readonly CosmosServiceContainer<SpeechPronounciationResultData> _cosmosServiceContainerSpeech;
        private readonly string _convCN;
        private readonly string _pronouncCN;

        public CosmosService(
            ILogger<CosmosServiceContainer<ConversationData>> loggerConversation, ILogger<CosmosServiceContainer<SpeechPronounciationResultData>> loggerSpeech,
            AppConfig appConfig)
        {
            // If this app grows this should be split up into more DAOs
            // These vars are safe as they come from env variables
            _cosmosClient = new CosmosClient(appConfig.DbConnectionString);
            _convCN = appConfig.CosmosDbConversationContainer;
            _pronouncCN = appConfig.CosmosDbPronounciationResultContainer;
            _cosmosServiceContainerConversations = new CosmosServiceContainer<ConversationData>(loggerConversation, _cosmosClient, appConfig.CosmosDbName, _convCN, appConfig.CosmosDbConversationPk);
            _cosmosServiceContainerSpeech = new CosmosServiceContainer<SpeechPronounciationResultData>(loggerSpeech, _cosmosClient, appConfig.CosmosDbName, _pronouncCN, appConfig.CosmodDbPronounciationResultPk);
        }

        public async Task InitializeService()
        {
            await _cosmosServiceContainerConversations.InitializeDatabaseAndContainerAsync();
            await _cosmosServiceContainerSpeech.InitializeDatabaseAndContainerAsync();
        }

        public async Task<ConversationData?> GetUserConversationAsync(string userId, string conversationId)
        {
            string query = $"SELECT * FROM {_convCN} WHERE {_convCN}.id = @p0 and {_convCN}.UserId = @p1";
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
            string query = $"SELECT {_convCN}.id, {_convCN}.UserId, {_convCN}.Title FROM {_convCN} WHERE {_convCN}.UserId = @p0";
            return await _cosmosServiceContainerConversations.QueryAsync(query, userId);
        }

        public async Task<List<SpeechPronounciationResultData>?> GetSpeechPronounciationResultDataAsync(string userId, string? language = null, DateTime? minTimestamp = null)
        {
            string query = $"SELECT * FROM {_pronouncCN} WHERE {_pronouncCN}.UserId = @p0";
            var args = new List<object>() { userId };

            if (language != null)
            {
                query += $" AND {_pronouncCN}.Language = @p1";
                args.Add(language);
            }

            if (minTimestamp != null)
            {
                query += $" AND {_pronouncCN}.Timestamp >= ";
                query += "@p" + args.Count;
                args.Add(minTimestamp);
            }

            return await _cosmosServiceContainerSpeech.QueryAsync(query, args.ToArray());
        }

        public async Task<bool> AddSpeechPronounciationResultDataAsync(string userId, SpeechPronounciationResultData data)
        {
            data.Id = Guid.NewGuid().ToString();
            data.UserId = userId;
            return await _cosmosServiceContainerSpeech.AddOrUpdateAsync(data);
        }

        public async Task<bool> DeleteAllUserData(string userId)
        {
            return
                await _cosmosServiceContainerConversations.DeletePartitionAsync(userId) &&
                await _cosmosServiceContainerSpeech.DeletePartitionAsync(userId);
        }
    }
}
