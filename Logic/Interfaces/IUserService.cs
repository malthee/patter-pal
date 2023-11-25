namespace patter_pal.Logic.Interfaces
{
    public interface IUserService
    {
        Task<bool> DeleteAllUserData(string userId);
    }
}
