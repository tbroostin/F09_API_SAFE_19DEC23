// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.BudgetManagement;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Services
{
    /// <summary>
    /// Methods implemented for a Budget Development Configuration service.
    /// </summary>
    public interface IBudgetDevelopmentConfigurationService
    {
        /// <summary>
        /// Get Budget Development Configuration parameters.
        /// </summary>
        /// <returns></returns>
        Task<BudgetConfiguration> GetBudgetDevelopmentConfigurationAsync();
    }
}

