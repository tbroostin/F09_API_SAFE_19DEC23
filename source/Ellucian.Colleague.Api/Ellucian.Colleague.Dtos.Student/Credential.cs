using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Defines the Credentials Earned at High School or College
    /// </summary>
    public class Credential
    {
        /// <summary>
        /// Any Honors earned
        /// </summary>
        public List<string> Honors { get; set; }
        /// <summary>
        /// Any awards to the Student
        /// </summary>
        public List<string> Awards { get; set; }
        /// <summary>
        /// Certificates or CCDs earned
        /// </summary>
        public List<string> Certificates { get; set; }
        /// <summary>
        /// Majors associated to the Credentials
        /// </summary>
        public List<string> Majors { get; set; }
        /// <summary>
        /// Minors associated to the Credentials
        /// </summary>
        public List<string> Minors { get; set; }
        /// <summary>
        /// Specializations associated to the Credentials
        /// </summary>
        public List<string> Specializations { get; set; }
        /// <summary>
        /// Any degree earned for this Credential
        /// </summary>
        public string Degree { get; set; }
        /// <summary>
        /// Date that the degree was earned
        /// </summary>
        public DateTime? DegreeDate { get; set; }
        /// <summary>
        /// Start Date for the Credential
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End Date for the Credential
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Commencement Date when the credential was earned
        /// </summary>
        public DateTime? CommencementDate { get; set; }
        /// <summary>
        /// Number of Years it took to earn the credentials
        /// </summary>
        public int? NumberOfYears { get; set; }
        /// <summary>
        /// Any comments specific to these credentials.
        /// </summary>
        public string Comments { get; set; }
    }
}
