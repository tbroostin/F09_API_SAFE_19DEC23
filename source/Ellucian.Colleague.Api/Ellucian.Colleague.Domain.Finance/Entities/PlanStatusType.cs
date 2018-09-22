// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Status of a payment plan
    /// </summary>
    [Serializable]
    public enum PlanStatusType
    {
        /// <summary>
        /// Open status - the payment plan has not been paid in full
        /// </summary>
        Open, 
        
        /// <summary>
        /// Paid status - the payment plan has been paid in full
        /// </summary>
        Paid, 
        
        /// <summary>
        /// Cancelled status - the payment plan has been cancelled
        /// </summary>
        Cancelled
    }
}
