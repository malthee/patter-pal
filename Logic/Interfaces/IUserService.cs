namespace patter_pal.Logic.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Deletes all data associated with <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> DeleteAllUserData(string userId);
    }
}
