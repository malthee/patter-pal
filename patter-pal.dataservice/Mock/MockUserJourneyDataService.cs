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


    }
}
