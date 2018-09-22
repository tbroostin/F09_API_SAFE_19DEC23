using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Criteria with which to post query of requirements
    /// </summary>
    public class RequirementQueryCriteria
    {
        /// <summary>
        /// List of requirement ids to query
        /// </summary>
        public List<string> RequirementIds { get; set; }
    }
}
