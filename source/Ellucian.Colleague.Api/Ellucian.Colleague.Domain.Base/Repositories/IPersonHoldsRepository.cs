// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IPersonHoldsRepository : IEthosExtended
    {
        Task<PersonRestriction> GetPersonHoldByIdAsync(string id);
        Task<Tuple<IEnumerable<PersonRestriction>, int>> GetPersonHoldsAsync(int offset, int limit);
        Task<string> GetStudentHoldGuidFromIdAsync(string id);
        Task<string> GetStudentHoldIdFromGuidAsync(string personHoldsId);
        Task<IEnumerable<PersonRestriction>> GetPersonHoldsByPersonIdAsync(string personId);
        Task<IEnumerable<PersonHoldResponse>> DeletePersonHoldsAsync(string personHoldsId);
        Task<PersonHoldResponse> UpdatePersonHoldAsync(PersonHoldRequest request);
    }
}
