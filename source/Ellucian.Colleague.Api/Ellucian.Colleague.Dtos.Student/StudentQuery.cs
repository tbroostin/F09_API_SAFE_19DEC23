using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// DTO for incoming JSON formatted student query
    /// </summary>
    public class StudentQuery
    {
        /// <summary>
        /// Student's last name
        /// </summary>
        public string lastName { get; set; }
        /// <summary>
        /// Student's first name
        /// </summary>
        public string firstName { get; set; }
        /// <summary>
        /// Student's date of birth
        /// </summary>
        public DateTime? dateOfBirth { get; set; }
        /// <summary>
        /// Student's former name
        /// </summary>
        public string formerName { get; set; }
        /// <summary>
        /// Student's ID
        /// </summary>
        public string studentId { get; set; }
        /// <summary>
        /// Student's SSN or SIN
        /// </summary>
        public string governmentId { get; set; }
        /// <summary>
        /// Student's registered terms
        /// </summary>
        public string termId { get; set; }
    }
}