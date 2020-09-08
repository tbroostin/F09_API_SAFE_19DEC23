/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Specify the cost calculation requirements for a period benefit
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PeriodBenefitCostCalculationRequirement
    {
        /// <summary>
        /// No requirement
        /// </summary>
        None,
        /// <summary>
        /// Amount required 
        /// </summary>
        Amount,
        /// <summary>
        /// Percentage required
        /// </summary>
        Percentage,
        /// <summary>
        /// Insurance amount required
        /// </summary>
        Insurance,
        /// <summary>
        /// Flexible spending required
        /// </summary>
        Flex
    }
}
