// Copyright 2015 Ellucian Company L.P. and it's affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student academic credit status, date, time, and reason.
    /// </summary>
    [Serializable]
    public class AcademicCreditStatus
    {
        private readonly string _status;
        private readonly DateTime? _date;
        private readonly DateTime? _time;
        private readonly string _reason;

        /// <summary>
        /// Student academic credit status
        /// </summary>
        public string Status { get { return _status; } }        
        /// <summary>
        /// Status date
        /// </summary>
        public DateTime? Date { get { return _date; } }
        /// <summary>
        /// Status time
        /// </summary>
        public DateTime? Time { get { return _time; } }
        /// <summary>
        /// Status reason
        /// </summary>
        public string Reason { get { return _reason;} }
                /// <summary>
        /// Initialize the Academic Credit Status method
        /// </summary>
        /// <param name="location">Student home location</param>
        /// <param name="startDate">Student home location start date</param>
        /// <param name="endDate">Student home location end date</param>
        public AcademicCreditStatus(string status, DateTime? date, DateTime? time, string reason)
        {
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException("status");
            }
            if (date == null || date == DateTime.MinValue)
            {
                throw new ArgumentNullException("date");
            }
            _status = status;
            _date = date;
            _time = time;
            _reason = reason;
        }
    }
}