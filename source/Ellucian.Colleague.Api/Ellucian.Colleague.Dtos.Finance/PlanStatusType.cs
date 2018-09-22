// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.Finance
{
    /// <summary>
    /// Status of a payment plan
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlanStatusType
    {
        /// <summary>
        /// Open - some component of the payment plan has not been paid in full
        /// </summary>
        Open,

        /// <summary>
        /// Paid - all scheduled payments and plan fees have been paid in full
        /// </summary>
        Paid,

        /// <summary>
        /// Cancelled - the payment plan has been cancelled
        /// </summary>
        Cancelled
    }
}