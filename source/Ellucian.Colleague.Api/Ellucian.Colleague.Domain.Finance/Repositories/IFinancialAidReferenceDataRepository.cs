//Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    public interface IFinancialAidReferenceDataRepository
    {
        /// <summary>
        /// Public accessor for Financial Aid awards
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FinancialAidAward>> GetFinancialAidAwardsAsync();

        /// <summary>
        /// Public accessor for Financial Aid Award categories
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FinancialAidAwardCategory>> GetFinancialAidAwardCategoriesAsync();
    }
}
