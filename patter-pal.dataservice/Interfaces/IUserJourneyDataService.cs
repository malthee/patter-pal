using patter_pal.dataservice.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.dataservice.Interfaces
{
    public interface IUserJourneyDataService
    {
        /// <summary>
        /// Adds <paramref name="email"/> to datastore if not exists already.
        /// Retrieves user journey data.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<UserJourneyData> Persist(string email);
        Task<UserJourneyData> Persist(UserJourneyData userJourneyData);

        // TODO get evaluations of user progress like accuracy, metrics over time in different languages
    }
}
