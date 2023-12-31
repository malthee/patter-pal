﻿using patter_pal.Controllers;
using patter_pal.dataservice.Azure;
using patter_pal.domain.Data;
using patter_pal.Logic.Interfaces;

namespace patter_pal.Logic.Cosmos
{
    public class ConversationService : IConversationService
    {
        private readonly ILogger<ConversationService> _logger;
        private readonly CosmosService _cosmosService;

        public ConversationService(ILogger<ConversationService> logger, CosmosService cosmosService)
        {
            _logger = logger;
            _cosmosService = cosmosService;
        }

        public async Task<bool> AddConversationAsync(string userId, ConversationData conversationData)
        {
            conversationData.Id = Guid.NewGuid().ToString();
            conversationData.UserId = userId;
            return await _cosmosService.AddOrUpdateChatConversationDataAsync(conversationData);
        }

        public async Task<bool> AddChatAsync(string userId, string conversationId, ChatData chat)
        {
            ConversationData? conversation = await _cosmosService.GetUserConversationAsync(userId, conversationId);
            if (conversation is null)
            {
                _logger.LogInformation($"Could not find conversation '{conversationId}' of user '{userId}'");
                return false;
            }
            conversation.AddChatMessage(chat);
            return await _cosmosService.AddOrUpdateChatConversationDataAsync(conversation);
        }

        // shallow => does not include ChatData
        public async Task<List<ConversationData>?> GetConversationsAsync(string userId)
        {
            return await _cosmosService.GetUserConversationsShallowAsync(userId);
        }

        public async Task<ConversationData?> GetConversationAndChatsAsync(string userId, string conversationId)
        {
            return await _cosmosService.GetUserConversationAsync(userId, conversationId);
        }

        public async Task<bool> UpdateConversationAsync(string userId, ConversationData conversation)
        {
            ConversationData? existingConversation = await _cosmosService.GetUserConversationAsync(userId, conversation.Id);
            if (existingConversation is null)
            {
                _logger.LogInformation($"Could not find conversation '{conversation.Id}' of user '{userId}'");
                return false;
            }

            existingConversation.Title = conversation.Title;
            return await _cosmosService.UpdateConversationTitleAsync(existingConversation);
        }

        public async Task<bool> DeleteConversationAsync(string userId, string conversationId)
        {
            ConversationData? conversation = await _cosmosService.GetUserConversationAsync(userId, conversationId);
            if (conversation is null)
            {
                _logger.LogInformation($"Could not find conversation '{conversationId}' of user '{userId}'");
                return false;
            }
            return await _cosmosService.DeleteConversationAsync(conversation);
        }

    }
}
