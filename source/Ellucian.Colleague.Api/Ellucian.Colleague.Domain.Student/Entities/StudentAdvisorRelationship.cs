using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student advisor relationships entity
    /// </summary>
    [Serializable]
    public class StudentAdvisorRelationship
    {
        /// <summary>
        /// ID of STUDENT.ADVISEMENT table
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Guid value for STUDENT.ADVISMENT id
        /// </summary>
        public string guid { get; set; }

        /// <summary>
        /// advisor ID
        /// </summary>
        public string advisor { get; set; }

        /// <summary>
        /// Student ID
        /// </summary>
        public string student { get; set; }

        /// <summary>
        /// advisor type
        /// </summary>
        public string advisorType { get; set; }

        /// <summary>
        /// Academic Program 
        /// </summary>
        public string program { get; set; }

        /// <summary>
        /// Start date of advisement
        /// </summary>
        public DateTime? startOn { get; set; }
        
        /// <summary>
        /// end date of advisement
        /// </summary>
        public DateTime? endOn { get; set; }
        
        /// <summary>
        /// Prioritization
        /// </summary>
        public string assignedPriority { get; set; }

        /// <summary>
        /// Term ID
        /// </summary>
        public string startAcademicPeriod { get; set; }
    }
}
