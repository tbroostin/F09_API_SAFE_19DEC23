// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;

namespace Ellucian.Colleague.Domain.Finance.Repositories
{
    /// <summary>
    /// Interface to the Finance Configuration repository
    /// </summary>
    public interface IFinanceConfigurationRepository
    {
        /// <summary>
        /// Get the base configuration for Student Finance
        /// </summary>
        /// <returns>Student Finance configuration</returns>
        FinanceConfiguration GetFinanceConfiguration();

        /// <summary>
        /// Get financial periods
        /// </summary>
        /// <returns>List of financial periods</returns>
        IEnumerable<FinancialPeriod> GetFinancialPeriods();

        /// <summary>
        /// Get due date overrides
        /// </summary>
        /// <returns>Due date override detail</returns>
        DueDateOverrides GetDueDateOverrides();

        /// <summary>
        /// Get configuration for Immediate Payment Control
        /// </summary>
        /// <returns>IPC configuration</returns>
        ImmediatePaymentControl GetImmediatePaymentControl();
    }
}
