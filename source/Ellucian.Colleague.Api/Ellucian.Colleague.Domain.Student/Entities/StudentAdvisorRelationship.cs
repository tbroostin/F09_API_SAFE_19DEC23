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
        public StudentAdvisorRelationship()
        {

        }

        public StudentAdvisorRelationship(string id, string guid, string advisor, string student, DateTime? startOn)
        {
            if(string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("studentAdvisementId", "Student advisor id is required.");
            }

            if (string.IsNullOrWhiteSpace(guid))
            {
                throw new ArgumentNullException("GUID", "GUID is required.");
            }

            if (string.IsNullOrWhiteSpace(advisor))
            {
                throw new ArgumentNullException("advisor", "Advisor id is required.");
            }

            if (string.IsNullOrWhiteSpace(student))
            {
                throw new ArgumentNullException("student", "Student id is required.");
            }

            if (!startOn.HasValue)
            {
                throw new ArgumentNullException("startOn", "Start on is required.");
            }

            this.Id = id;
            this.Guid = guid;
            this.Advisor = advisor;
            this.Student = student;
            this.StartOn = startOn.Value;
        }
        /// <summary>
        /// ID of STUDENT.ADVISEMENT table
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Guid value for STUDENT.ADVISMENT id
        /// </summary>
        public string Guid { get; set; }

        /// <summary>
        /// advisor ID
        /// </summary>
        public string Advisor { get; set; }

        /// <summary>
        /// Student ID
        /// </summary>
        public string Student { get; set; }

        /// <summary>
        /// advisor type
        /// </summary>
        public string AdvisorType { get; set; }

        /// <summary>
        /// Academic Program 
        /// </summary>
        public string Program { get; set; }

        /// <summary>
        /// Start date of advisement
        /// </summary>
        public DateTime? StartOn { get; set; }
        
        /// <summary>
        /// end date of advisement
        /// </summary>
        public DateTime? EndOn { get; set; }
        
        /// <summary>
        /// Prioritization
        /// </summary>
        public string AssignedPriority { get; set; }

        /// <summary>
        /// Term ID
        /// </summary>
        public string StartAcademicPeriod { get; set; }
    }
}
