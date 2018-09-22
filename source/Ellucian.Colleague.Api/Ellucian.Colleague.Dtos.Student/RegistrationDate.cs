using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Collection of Registration Dates with locations on Terms
    /// </summary>
    public class RegistrationDate
    {
        /// <summary>
        /// Registration starts on this date
        /// </summary>
        public DateTime? RegistrationStartDate { get; set; }
        /// <summary>
        /// Registration ends on this date
        /// </summary>
        public DateTime? RegistrationEndDate { get; set; }
        /// <summary>
        /// Pre-Registration starts on this date
        /// </summary>
        public DateTime? PreRegistrationStartDate { get; set; }
        /// <summary>
        /// Pre-Registration ends on this date
        /// </summary>
        public DateTime? PreRegistrationEndDate { get; set; }
        /// <summary>
        /// The Add period starts on this date
        /// </summary>
        public DateTime? AddStartDate { get; set; }
        /// <summary>
        /// The Add period ends on this date
        /// </summary>
        public DateTime? AddEndDate { get; set; }
        /// <summary>
        /// The Drop period starts on this date
        /// </summary>
        public DateTime? DropStartDate { get; set; }
        /// <summary>
        /// The Drop period ends on this date
        /// </summary>
        public DateTime? DropEndDate { get; set; }
        /// <summary>
        /// Grades are required if a student drops a course after this date
        /// </summary>
        public DateTime? DropGradeRequiredDate { get; set; }
        /// <summary>
        /// Registration Dates may be different by locations
        /// </summary>
        public string Location { get; set; }
    }
}
