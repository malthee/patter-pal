using patter_pal.dataservice.DataObjects;
using patter_pal.dataservice.Interfaces;

namespace patter_pal.Logic
{
    /// <summary>
    /// Handling login/logout.
    /// Managing data of currently logged in user.
    /// </summary>
    public class UserService
    {
        private readonly IUserJourneyDataService _userJourneyDataService;
        public UserJourneyData? UserData { get; set; }

        public UserService(IUserJourneyDataService userJourneyDataService)
        {
            _userJourneyDataService = userJourneyDataService;
        }

        public bool HasUserData() => UserData != null;

        /// <summary>
        /// Logs in user with provided <paramref name="email"/>.
        /// Retrieves <see cref="UserJourneyData"/> if exists or inits it with empty data.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task LoginUser(string email)
        {
            UserData = await _userJourneyDataService.Persist(email);
        }

        /// <summary>
        /// Unlinks data of currently logged in user´.
        /// </summary>
        public void Logout()
        {
            UserData = null;
        }
    }
}
