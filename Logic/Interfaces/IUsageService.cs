namespace patter_pal.Logic.Interfaces
{
    public interface IUsageService
    {
        /// <summary>
        /// Increments the chat-request counter for <paramref name="userId"/> by 1.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> IncrementUserRequestCounterAsync(string userId);

        /// <summary>
        /// Checks if <paramref name="userId"/> has any requests remaining.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> HasUserRequestsRemainingAsync(string userId);
    }
}
