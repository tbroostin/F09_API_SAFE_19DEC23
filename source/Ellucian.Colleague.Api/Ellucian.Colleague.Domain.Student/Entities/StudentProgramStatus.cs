// Copyright 2016 Ellucian Company L.P. and it's affiliates.

using System;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Student program status and status date.
    /// </summary>
    [Serializable]
    public class StudentProgramStatus
    {
        private readonly string _status;
        private readonly DateTime? _statusDate;

        /// <summary>
        /// Student status
        /// Ex: East
        /// </summary>
        public string Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Student Status Date
        /// </summary>
        public DateTime? StatusDate
        {
            get { return _statusDate; }
        }

        /// <summary>
        /// Initialize the Student Status
        /// </summary>
        /// <param name="status">Student status</param>
        /// <param name="statusDate">Student status date</param>

        public StudentProgramStatus(string status, DateTime? statusDate)
        {
            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentNullException("status");
            }
            if (statusDate == null || statusDate == DateTime.MinValue)
            {
                throw new ArgumentNullException("statusDate");
            }
            _status = status;
            _statusDate = statusDate;
        }
    }
}