// Copyright 2021 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Repositories
{
    /// <summary>
    /// Interface to the Approval Configuration repository.
    /// </summary>
    public interface IApprovalConfigurationRepository
    {
        /// <summary>
        /// Get the Approval configuration for Colleague Financials.
        /// </summary>
        /// <returns>Approval configuration settings.</returns>
        Task<ApprovalConfiguration> GetApprovalConfigurationAsync();
    }
}
