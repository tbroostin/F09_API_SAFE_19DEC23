using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// DTO Class having stipend information associated with employee compensation
    /// </summary>
    public class EmployeeStipend
    {
        /// <summary>
        /// Description of the stipend received by employee
        /// </summary>
        public string StipendDescription { get; set; }
        /// <summary>
        /// Stipend Amount received by employee
        /// </summary>
        public decimal? StipendAmount { get; set; }

    }
}
