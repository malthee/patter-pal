﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using patter_pal.domain.Data;
using patter_pal.Logic;
using patter_pal.Logic.Interfaces;
using patter_pal.Models;

namespace patter_pal.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Consumes("application/json")]
    [Authorize(Policy = "LoggedInPolicy")]
    public class ConversationController : ControllerBase
    {
        private readonly ILogger<ConversationController> _logger;
        private readonly IConversationService _conversationService;
        private readonly AuthService _authService;

        public ConversationController(ILogger<ConversationController> logger, IConversationService conversationService, AuthService authService)
        {
            _logger = logger;
            _conversationService = conversationService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ConversationModel>>> GetConversations()
        {
            string userId = (await _authService.GetUserId())!;
            _logger.LogDebug($"Getting conversations for user {userId}");
            var result = await _conversationService.GetConversationsAsync(userId);
            if (result == null) return NotFound();

            return result.Select(c => new ConversationModel(c.Id, c.Title)).ToList();
        }


        [HttpGet("Chat")]
        public async Task<ActionResult<List<ChatMessageModel>>> GetChatsByConversationId(string conversationId)
        {
            string userId = (await _authService.GetUserId())!;
            _logger.LogDebug($"Getting chats for user {userId} and conversation {conversationId}");
            var result = await _conversationService.GetConversationAndChatsAsync(userId, conversationId);
            if (result == null) return NotFound();

            return result.Data.Select(c => new ChatMessageModel(c.Text, c.Language, c.Id, conversationId, c.IsUser)).ToList();
        }

        [HttpPut]
        public async Task<ActionResult> UpdateConversation(ConversationModel conversation)
        {
            string userId = (await _authService.GetUserId())!;
            _logger.LogDebug($"Updating conversation {conversation.Id} for user {userId}");
            var model = new ConversationData { Id = conversation.Id, Title = conversation.Title };

            return await _conversationService.UpdateConversationAsync(userId, model) ? Ok() : NotFound();
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteConversation(string conversationId)
        {
            string userId = (await _authService.GetUserId())!;
            _logger.LogDebug($"Deleting conversation {conversationId} for user {userId}");

            return await _conversationService.DeleteConversationAsync(userId, conversationId) ? Ok() : NotFound();
        }
    }
}
