/* Copyright 2022 Ellucian Company L.P. and its affiliates. */
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Records the Unsubmit/Withdraw types for Leave Request
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LeaveRequestActionType
    {
        /// <summary>
        /// Reject
        /// </summary>
        R,

        /// <summary>
        /// Unsubmit before approval
        /// </summary>
        U,

        /// <summary>
        /// Withdraw approved request requires no approval
        /// </summary>
        W,

        /// <summary>
        /// Withdraw approved request requires approval
        /// </summary>
        A
    }
}
