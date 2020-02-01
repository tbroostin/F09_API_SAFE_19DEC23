/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Domain Entity class having Tax Benefits information associated with employee compensation
    /// </summary>
    [Serializable]
    public class EmployeeTax
    {
        /// <summary>
        /// Code(ID) of the tax in associated with an employee
        /// </summary>
        public string TaxCode { get; set; }

        /// <summary>
        /// Description of the tax code
        /// </summary>
        public string TaxDescription { get; set; }

        /// <summary>
        /// Amount paid by employer
        /// </summary>
        public decimal? TaxEmployerAmount { get; set; }

        /// <summary>
        /// Amount paid by employee
        /// </summary>
        public decimal? TaxEmployeeAmount { get; set; }

        public EmployeeTax(string taxCode, string taxCodeDescription, decimal? taxEmployerAmount, decimal? taxEmployeeAmount)
        {
            if (string.IsNullOrEmpty(taxCode))
                throw new ArgumentNullException("Tax Code cannot be empty");

            TaxCode = taxCode;
            TaxDescription = taxCodeDescription;
            TaxEmployerAmount = taxEmployerAmount;
            TaxEmployeeAmount = taxEmployeeAmount;
        }
    }
}
