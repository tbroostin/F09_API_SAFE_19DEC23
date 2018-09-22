using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Defines the College DTO
    /// </summary>
    public class College
    {
        /// <summary>
        /// College Id
        /// </summary>
        public string CollegeId { get; set; }
        /// <summary>
        /// Name of the College Attended
        /// </summary>
        public string CollegeName { get; set; }
        /// <summary>
        /// GPA earned at this college
        /// </summary>
        public decimal? Gpa { get; set; }
        /// <summary>
        /// Summary credits from the attendance at this college
        /// </summary>
        public decimal? SummaryCredits { get; set; }
        /// <summary>
        /// Last Year the student attended this college
        /// </summary>
        public string LastAttendedYear { get; set; }
        /// <summary>
        /// any credentials earned at this college
        /// </summary>
        public List<Credential> Credentials { get; set; }
        /// <summary>
        /// If credentials end, then this is the end date
        /// </summary>
        public DateTime? CredentialsEndDate { get; set; }
        /// <summary>
        /// Any additional comments for attendance at this college.
        /// </summary>
        public string Comments { get; set; }
    }
}
