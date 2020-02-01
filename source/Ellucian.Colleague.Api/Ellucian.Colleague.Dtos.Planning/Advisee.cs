using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Dtos.Planning
{
    /// <summary>
    /// A student who is being advised
    /// </summary>
    public class Advisee : Person
    {
        /// <summary>
        /// ID of student's degree plan (aka, course plan)
        /// </summary>
        public int? DegreePlanId { get; set; }
        /// <summary>
        /// List of the student's enrolled programs
        /// </summary>
        public List<string> ProgramIds { get; set; }
        /// <summary>
        /// Indicates whether student is awaiting advisor approval
        /// </summary>
        public bool ApprovalRequested { get; set; }
        /// <summary>
        /// This advisee is assigned to the current advisor using the system
        /// </summary>
        public bool IsAdvisee { get; set; }
        /// <summary>
        /// Preferred email address of Advisee
        /// </summary>
        public string PreferredEmailAddress { get; set; }
        /// <summary>
        /// Student's stated educational goal, useful for advising. Such as: BA degree, Certification, New Career (institutionally defined)
        /// </summary>
        public string EducationalGoal { get; set; }

        /// <summary>
        /// The IDs of this advisee's advisors
        /// </summary>
        public List<string> AdvisorIds { get; set; }

        /// <summary>
        /// Name that should be used when displaying a advisee's name to advisors based on a client specified Name Address Hierarchy.
        /// The hierarchy that is used in calculating this name is defined in the Student Display Name Hierarchy on the SPWP form in Colleague.  
        /// If no hierarchy is provide on SPWP, this will be null.
        /// </summary>
        public PersonHierarchyName PersonDisplayName { get; set; }

        /// <summary>
        /// List of completed advisements for the student for a given date and time by a given advisor
        /// </summary>
        public IEnumerable<CompletedAdvisement> CompletedAdvisements { get; set; }
        /// <summary>
        /// Phonetypes hierarchy for student profile
        /// </summary>
        public List<string> PhoneTypesHierarchy { get; set; }
        /// <summary>
        /// All email address of advisee
        /// </summary>
        public List<EmailAddress> EmailAddresses { get; set; }
    }
}
