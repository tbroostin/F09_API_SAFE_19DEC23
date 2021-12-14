using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities.Requirements
{
    /// <summary>
    /// A table that maps the requirments that are excluded by the Requirement id and all the requirements Ids that current Requirment Id excludes.
    /// </summary>
    [Serializable]
    
    public class RequirementExclusionTable
    {
        /// <summary>
        /// Requirement Id
        /// </summary>
        public string RequirmentId { get; set; }
        /// <summary>
        /// Requirement Code
        /// </summary>
        public string RequirementCode { get; set; }
        /// <summary>
        /// Reuqirment Type
        /// </summary>

        public RequirementType RequirementType { get; set; }
        /// <summary>
        /// List of all other requirments Ids that are excluding this particular requirement.
        /// Suppose Requirment Id is of type MAJ but GEN requireent has defined  MAJ in its exclusion. then this list will have GEN
        /// </summary>
        public List<string> ExcludedByRequirementIds { get; set; }
        /// <summary>
        /// List of all the requirements Ids that this particular requirement is excluding or excludes.
        /// Suppose requirment Id is of type MAJ and I have exclusion defined for MIN,  so this collection will have Ids of all requirments of MIN type
        /// </summary>
        public List<string> ExcludesRequirementIds { get; set; }
        

        public RequirementExclusionTable(string id, string code, RequirementType type)
        {
            this.RequirmentId = id;
            this.RequirementCode = code;
            this.RequirementType = type;
            ExcludedByRequirementIds = new List<string>();
            ExcludesRequirementIds = new List<string>();

            /* Example of this table
             * REQUIRMENT-ID  TYPE  EXCLUDED-BY   EXCLUDES
             * MAJ-REQ-ID     MAJ   GEN-REQ-ID      MIN-REQ-ID, MIN-REQ-ID-2
             * GEN-REQ-ID     GEN                   MAJ-REQ-ID
             * MIN-REQ-ID     MIN   MAJ-REQ-ID
             * MIN-REQ-ID-2   MIN   MAJ-REQ-ID
             */
        }

    }
}
