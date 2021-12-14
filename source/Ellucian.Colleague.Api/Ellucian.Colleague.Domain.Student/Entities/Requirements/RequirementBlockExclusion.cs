using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// Idenitifies exclsuion Type like MAJ or MIN along with boolean flag if exclusion is only valid for first requirment of exclusion type.
    /// </summary>
    [Serializable]
    public class RequirementBlockExclusion
    {
        /// <summary>
        /// 
        /// </summary>
        public string ExclusionType { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public bool FirstOnlyFlag { get; private set; }

        public RequirementBlockExclusion(string exclusionType, bool firstOnlyFlag)
        {
            ExclusionType = exclusionType;
            FirstOnlyFlag = firstOnlyFlag;
        }

        public RequirementBlockExclusion(string exclusionType)
        {
            ExclusionType = exclusionType;
            FirstOnlyFlag = false;
        }
    }
}
