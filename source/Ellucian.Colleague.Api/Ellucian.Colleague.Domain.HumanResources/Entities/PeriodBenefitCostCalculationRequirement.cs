/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Specify the cost calculation requirements for a period benefit
    /// </summary>
    [Serializable]
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
