// Copyright 2014 Ellucian Company L.P. and it's affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Majors from Programs and Student Programs.  Each has a code, a spelled-out name, and active flag.
    /// </summary>
    public class StudentMinors
    {
        /// <summary>
        /// Ex: MATH
        /// </summary>
        public string Code { get; set; }  
        /// <summary>
        /// Ex: Mathematics
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Start date of Minor.  Same as Student Program except for Additional Minors.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// End date of Minor.  Same as Student Program except for Additional Minors.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}