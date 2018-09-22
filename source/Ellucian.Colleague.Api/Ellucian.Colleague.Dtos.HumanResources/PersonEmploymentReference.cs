using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.HumanResources
{
    /// <summary>
    /// Information about an Employment Reference
    /// </summary>
    public class PersonEmploymentReference
    {
        /// <summary>
        /// Unique system Id for this Person Employment Reference
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of the Person Employment Reference
        /// </summary>
        public string Description { get; set; }

    }
}
