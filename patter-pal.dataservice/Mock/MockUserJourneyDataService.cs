using patter_pal.dataservice.Azure;
using patter_pal.dataservice.DataObjects;
using patter_pal.dataservice.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patter_pal.dataservice.Mock
{
    /// <summary>
    /// Mock impl of <see cref="IUserJourneyDataService"/> for testing purposes.
    /// </summary>
    public class MockUserJourneyDataService : IUserJourneyDataService
    {
        private readonly CosmosService _cosmosService;

        public MockUserJourneyDataService(CosmosService cosmosService)
        {
            this._cosmosService = cosmosService;
        }

        public async Task<UserJourneyData> Persist(string email)
        {
            UserJourneyData? existing = await _cosmosService.TryGetUserJourneyDataAsync(email);
            return await _cosmosService.UpsertUserJourneyDataAsync(existing ?? new UserJourneyData { Id = email, Email = email });
            //return Task.FromResult(new UserJourneyData { Email = email });
        }

        public async Task<UserJourneyData> Persist(UserJourneyData userJourneyData)
        {
            return await _cosmosService.UpsertUserJourneyDataAsync(userJourneyData);
            //return Task.FromResult(new UserJourneyData { Email = email });
        }
    }
}
