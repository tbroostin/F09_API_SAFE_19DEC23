/*Copyright 2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Used to categorize line items on the PayStatement in the Taxes, Benefits, Other Deductions section.
    /// </summary>
    public enum PayStatementDeductionType
    {
        /// <summary>
        /// A Tax deduction.
        /// </summary>
        Tax,

        /// <summary>
        /// A Benefit deduction.
        /// </summary>
        Benefit,

        /// <summary>
        /// Some other deduction
        /// </summary>
        Deduction
    }
}
