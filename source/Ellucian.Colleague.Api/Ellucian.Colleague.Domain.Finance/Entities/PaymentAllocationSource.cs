// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Finance.Entities
{
    /// <summary>
    /// Source of a payment's allocation
    /// </summary>
    [Serializable]
    public enum PaymentAllocationSource
    {
        /// <summary>
        /// User-allocated payment
        /// </summary>
        User,

        /// <summary>
        /// Payment made by a sponsor
        /// </summary>
        SponsoredBilling,

        /// <summary>
        /// Payment made by financial aid
        /// </summary>
        FinancialAid,

        /// <summary>
        /// System-allocated payment
        /// </summary>
        System
    }
}
