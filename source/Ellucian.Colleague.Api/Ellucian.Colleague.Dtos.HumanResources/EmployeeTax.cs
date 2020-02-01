using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{

    /// <summary>
    /// DTO class having Tax Benefits information associated with employee compensation
    /// </summary>
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

    }
}
