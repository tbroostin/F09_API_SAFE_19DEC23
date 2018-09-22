using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about a student listed in a roster
    /// </summary>
    public class RosterStudent
    {
        /// <summary>
        /// Unique system Id of this student
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// First name of this student
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Middle name or initial of this student
        /// </summary>
        public string MiddleName { get; set; }
        /// <summary>
        /// Last name of this student
        /// </summary>
        public string LastName { get; set; }
    }
}
