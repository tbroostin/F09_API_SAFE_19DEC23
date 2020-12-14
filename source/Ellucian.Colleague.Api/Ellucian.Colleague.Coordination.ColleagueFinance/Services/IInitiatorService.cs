// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for Initiator
    /// </summary>
    public interface IInitiatorService : IBaseService
    {
        /// <summary>
        /// Fetches the initiators from keyword 
        /// </summary>
        /// <param name="queryKeyword"></param>
        /// <returns>Initiator DTO as response</returns>
        Task<IEnumerable<Initiator>> QueryInitiatorByKeywordAsync(string queryKeyword);
    }
}
