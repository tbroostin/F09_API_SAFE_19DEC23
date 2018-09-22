using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Defines the High School along with the HighSchoolGpa DTO
    /// </summary>
    public class HighSchool : HighSchoolGpa
    {
        /// <summary>
        /// Name of High School
        /// </summary>
        public string HighSchoolName { get; set; }
        /// <summary>
        /// Graduation Type, such as Diploma or GED
        /// </summary>
        public string GraduationType { get; set; }
        /// <summary>
        /// Summary Credits received at High School
        /// </summary>
        public decimal? SummaryCredits { get; set; }
        /// <summary>
        /// Credentials Earned at High School
        /// </summary>
        public List<Credential> Credentials { get; set; }
        /// <summary>
        /// Credentials End on this date (usually they do not end)
        /// </summary>
        public DateTime? CredentialsEndDate { get; set; }
        /// <summary>
        /// Any additional comments associated to this attendance at this high school
        /// </summary>
        public string Comments { get; set; }
    }
}
