using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// A processing code applied to any tax processed through an employee's payroll.
    /// This code indicates various ways to process the tax. 
    /// This corresponds to FATERDX codes in Colleague
    /// </summary>
    public enum PayrollTaxProcessingCode
    {
        /// <summary>
        /// Specify that a fixed amount of tax should be processed
        /// </summary>
        FixedAmount,

        /// <summary>
        /// Specify that an additional amount should be added to the calculated tax amount
        /// </summary>
        AdditionalTaxAmount,

        /// <summary>
        /// Specify that an additional amount should be added to the taxable amount before the tax amount is calculated
        /// </summary>
        AdditionalTaxableAmount,

        /// <summary>
        /// Specify that this tax is exempt, but that the taxable amounts are updated with earnings
        /// </summary>
        TaxExempt,

        /// <summary>
        /// Specify that this is a regular tax calculation
        /// </summary>
        Regular,

        /// <summary>
        /// Specify that this tax is no longer active and should not be processed through payroll.
        /// </summary>
        Inactive,

        /// <summary>
        /// Specify that this tax is exempt and that taxable amounts are NOT updated with earnings.
        /// </summary>
        TaxableExempt

    }
}
