/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Used to categorize line items on the PayStatement in the Taxes, Benefits, Other Deductions section.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PayStatementDeductionType
    {
        /// <summary>
        ///  A Tax deduction.
        /// </summary>
        Tax,
        /// <summary>
        ///  A Benefit deduction.
        /// </summary>
        Benefit,
        /// <summary>
        /// Some other deduction
        /// </summary>
        Deduction
    }
}
