// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonHoldsService : IBaseService
    {
        //Task DeletePersonHoldAsync(string id, string personHoldsId);
        //Task<IEnumerable<Dtos.PersonHold>> GetPersonHoldsAsync();
        Task<Tuple<IEnumerable<Dtos.PersonHold>, int>> GetPersonHoldsAsync(int offset, int limit, bool bypassCache = false);

        Task<Dtos.PersonHold> GetPersonHoldAsync(string id, bool bypassCache = false);
        Task<IEnumerable<Dtos.PersonHold>> GetPersonHoldsAsync(string personId, bool bypassCache = false);
        Task DeletePersonHoldAsync(string personHoldsId);
        Task<Dtos.PersonHold> CreatePersonHoldAsync(Dtos.PersonHold personHold);
        Task<Dtos.PersonHold> UpdatePersonHoldAsync(string id, Dtos.PersonHold personHold);
    }
}