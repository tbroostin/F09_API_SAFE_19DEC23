using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Information about an Employment Proficiency Licensing Authority
    /// </summary>
    public class ProficiencyLicensingAuthority
    {
        /// <summary>
        /// Unique system Id for this Proficiency Licensing Authority
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of the Proficiency Licensing Authority
        /// </summary>
        public string Description { get; set; }

    }
}
