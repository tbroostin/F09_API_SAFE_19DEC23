using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Define a list within Student for High School and GPA
    /// </summary>
    public class HighSchoolGpa
    {
        /// <summary>
        /// High School Person ID
        /// </summary>
        public string HighSchoolId { get; set; }
        /// <summary>
        /// High School GPA
        /// </summary>
        public decimal? Gpa { get; set; }
        /// <summary>
        /// Date the person last attended this High School
        /// </summary>
        public string LastAttendedYear { get; set; }
    }
}
