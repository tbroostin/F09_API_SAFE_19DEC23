using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Define a Persons high school attendance
    /// </summary>
    public class PersonHighSchool
    {
        /// <summary>
        /// Person who attended this High School
        /// </summary>
        public string PersonId { get; set; }
        /// <summary>
        /// High School Corp ID
        /// </summary>
        public string HighSchoolId { get; set; }
        /// <summary>
        /// Grade Point Average achieved at this High School
        /// </summary>
        public decimal GradePointAverage { get; set; }
    }
}
