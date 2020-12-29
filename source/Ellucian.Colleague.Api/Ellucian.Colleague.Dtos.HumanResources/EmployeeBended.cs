using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO class having Benefit-Deduction information associated with employee compensation
    /// </summary>
    public class EmployeeBended
    {
        /// <summary>
        /// Code(ID) of the benefit/deduction in which the employee is enrolled
        /// </summary>
        public string BenededCode { get; set; }

        /// <summary>
        ///  Description of the benefit or deduction code
        /// </summary>
        public string BenededDescription { get; set; }

        /// <summary>
        /// Amount paid by employer
        /// </summary>
        public decimal? BenededEmployerAmount { get; set; }

        /// <summary>
        /// Amount paid by employee
        /// </summary>
        public decimal? BenededEmployeeAmount { get; set; }
    }
}
