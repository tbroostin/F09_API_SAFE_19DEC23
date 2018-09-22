/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    
    /// <summary>
    /// Categorizing a BenefitDeductionType into a Benefit or a Deduction. This is derived based on the special processing code 2 on the BENDED.TYPE validation code table.
    /// </summary>
    [Serializable]
    public enum BenefitDeductionTypeCategory
    {
        /// <summary>
        /// Benefit category
        /// </summary>
        Benefit,

        /// <summary>
        /// Deduction category
        /// </summary>
        Deduction
    }
}
