using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Information about an Employment Proficiency Level
    /// </summary>
    public class EmploymentProficiencyLevel
    {
        /// <summary>
        /// Unique system Id for this Employment Proficiency Level
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of the Employment Proficiency Level
        /// </summary>
        public string Description { get; set; }

    }
}
