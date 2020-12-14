//Copyright 2020 Ellucian Company L.P.and its affiliates.
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    public interface IInitiatorRepository
    {

        /// <summary>
        /// Gets the initiator for search criteria.
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns></returns>
        Task<IEnumerable<Initiator>> QueryInitiatorByKeywordAsync(string searchCriteria);
    }
}
