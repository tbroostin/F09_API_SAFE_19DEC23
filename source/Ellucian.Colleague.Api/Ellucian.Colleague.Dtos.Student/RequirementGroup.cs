using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to request a search by requirement/Subrequirement/group.  All 3 components are required. 
    /// </summary>
    public class RequirementGroup
    {
        /// <summary>
        /// Required. Code for the Requirement.
        /// </summary>
        public string RequirementCode { get; set; }

        /// <summary>
        /// Required. SubRequirementID.
        /// </summary>
        public string SubRequirementId { get; set; }

        /// <summary>
        /// Required. Group Id.
        /// </summary>
        public string GroupId { get; set; }
    }
}
