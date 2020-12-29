// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IPersonMatchingRequestsRepository : IEthosExtended
    {
        Task<Tuple<IEnumerable<PersonMatchRequest>, int>> GetPersonMatchRequestsAsync(int offset, int limit, PersonMatchRequest criteriaObj = null,
            string[] filterPersonIds = null, bool bypassCache = false);
        Task<PersonMatchRequest> GetPersonMatchRequestsByIdAsync(string id, bool bypassCache = false);

        Task<PersonMatchRequest> CreatePersonMatchingRequestsInitiationsProspectsAsync(PersonMatchRequestInitiation request);
    }
}