// Copyright 2013-2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Source of a payment's allocation
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
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
