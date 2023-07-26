/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// Enumeration of possible types of Professional Judgment Indicator
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum JudgementIndicator
    {
        /// <summary>
        /// Processes
        /// </summary>
        [EnumMember(Value = "adjustmentProcessed")]
        AdjustmentProcessed = 1,

        /// <summary>
        /// Failed
        /// </summary>
        [EnumMember(Value = "adjustmentFailed")]
        AdjustmentFailed = 2,

    }
}