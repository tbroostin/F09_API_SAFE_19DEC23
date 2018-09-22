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
    public class StudentMajors
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
        /// Start Date for the Major.  Same as Student Program except for Additional Majors
        /// </summary>
        public DateTime? StartDate { get { return _startDate; } }
        /// <summary>
        /// End Date for the Major.  Same as Student Program except for Additional Majors
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Initialize the Student Majors method
        /// </summary>
        /// <param name="majorCode">Major code</param>
        /// <param name="majorName">Description of the Major</param>
        /// <param name="startDate">Student Major association starts on this date</param>
        /// <param name="endDate">Student Major association ends on this date</param>
        public StudentMajors(string majorCode, string majorName, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(majorCode))
            {
                // todo: srm Match case for arguments with exception "majorCode"
                throw new ArgumentNullException("majorCode");
            }
            if (string.IsNullOrEmpty(majorName))
            {
                throw new ArgumentNullException("majorName");
            }
            if (startDate == null || startDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("startDate");
            }
            //todo: srm Make sure End Date is after Start Date
            _code = majorCode;
            _name = majorName;
            _startDate = startDate;
            EndDate = endDate;
        }
    }
}