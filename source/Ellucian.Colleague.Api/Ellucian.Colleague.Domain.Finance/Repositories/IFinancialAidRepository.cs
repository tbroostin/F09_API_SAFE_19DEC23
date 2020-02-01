// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    /// <summary>
    /// Interface to the Financial Aid repository
    /// </summary>
    public interface IFinancialAidRepository
    {
        /// <summary>
        /// Public accessor for Potential D7 transmittals
        /// </summary>
        /// <returns>
        /// Enumeration of <see cref="PotentialD7FinancialAid"/>.
        /// </returns>
        /// <param name="criteria">A <see cref="PotentialD7FinancialAidCriteria"/> object containing the query criteria</param>
        Task<IEnumerable<PotentialD7FinancialAid>> GetPotentialD7FinancialAidAsync(PotentialD7FinancialAidCriteria criteria);
    }
}
