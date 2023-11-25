using patter_pal.dataservice.DataObjects;

namespace patter_pal.Logic.Interfaces
{
    public interface IConversationService
    {
        /// <summary>
        /// Assigns an Id to <paramref name="conversationData"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationData"></param>
        /// <returns></returns>
        Task<bool> AddConversationAsync(string userId, ConversationData conversationData);

        /// <summary>
        /// Assigns an Id to <paramref name="chat"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <param name="chat"></param>
        /// <returns></returns>
        Task<bool> AddChatAsync(string userId, string conversationId, ChatData chat);
        Task<List<ConversationData>?> GetConversationsAsync(string userId);
        Task<ConversationData?> GetConversationAndChatsAsync(string userId, string conversationId);
        Task<bool> UpdateConversationAsync(string userId, ConversationData conversation);
        Task<bool> DeleteConversationAsync(string userId, string conversationId);
    }
}