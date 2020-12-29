// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.BudgetManagement.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.BudgetManagement.Repositories
{
    /// <summary>
    /// Interface to the Budget Development Configuration repository.
    /// </summary>
    public interface IBudgetDevelopmentConfigurationRepository
    {
        /// <summary>
        /// Get the Budget Development configuration parameters.
        /// </summary>
        /// <returns>Budget Development configuration.</returns>
        Task<BudgetConfiguration> GetBudgetDevelopmentConfigurationAsync();
    }
}
