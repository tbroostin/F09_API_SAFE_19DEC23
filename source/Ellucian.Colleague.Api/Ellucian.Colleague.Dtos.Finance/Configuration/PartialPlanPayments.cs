// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.

namespace Ellucian.Colleague.Dtos.Finance.Configuration
{
    /// <summary>
    /// Options for processing partial payments on payment plans
    /// </summary>
    public enum PartialPlanPayments
    {
        /// <summary>
        /// Partial payments on payment plans are allowed
        /// </summary>
        Allowed, 
        /// <summary>
        /// No partial payments on payment plans are allowed
        /// </summary>
        Denied, 
        /// <summary>
        /// Partial payments on payment plans are allowed unless overdue
        /// </summary>
        AllowedWhenNotOverdue
    }
}
