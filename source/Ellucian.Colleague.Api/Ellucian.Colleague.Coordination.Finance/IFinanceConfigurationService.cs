// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.Configuration;

namespace Ellucian.Colleague.Coordination.Finance
{
    public interface IFinanceConfigurationService
    {
        /// <summary>
        /// Get the base configuration for Student Finance
        /// </summary>
        /// <returns>Student Finance configuration</returns>
        FinanceConfiguration GetFinanceConfiguration();

        /// <summary>
        /// Get the configuration for Immediate Payment Control
        /// </summary>
        /// <returns>Immediate Payment Control configuration</returns>
        ImmediatePaymentControl GetImmediatePaymentControl();
    }
}
