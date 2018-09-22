// Copyright 2014 Ellucian Company L.P. and it's affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Majors from Programs and Student Programs.  Each has a code, a spelled-out name, and active flag.
    /// </summary>
    [Serializable]
    public class StudentMinors
    {
        private readonly string _code;
        private readonly string _name;
        private readonly DateTime? _startDate;

        /// <summary>
        /// Ex: MATH
        /// </summary>
        public string Code { get { return _code; } }
        /// <summary>
        /// Ex: Mathematics
        /// </summary>
        public string Name { get { return _name; } }
        /// <summary>
        /// Start Date for the Minor.  Same as Student Program except for Additional Minors.
        /// </summary>
        public DateTime? StartDate { get { return _startDate; } }
        /// <summary>
        /// End date of Minor.  Same as Student Program except for Additional Minors.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Initialize the Student Minors Method
        /// </summary>
        /// <param name="minorCode">Minor Code</param>
        /// <param name="minorName">Description of the Minor</param>
        /// <param name="startDate">Student Minor association starts on this date</param>
        /// <param name="endDate">Student Minor association ends on this date</param>
        public StudentMinors(string minorCode, string minorName, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(minorCode))
            {
                throw new ArgumentNullException("minorCode");
            }
            if (string.IsNullOrEmpty(minorName))
            {
                throw new ArgumentNullException("minorName");
            }
            if (startDate == null || startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate");
            }
            _code = minorCode;
            _name = minorName;
            _startDate = startDate;
            EndDate = endDate;
        }
    }
}