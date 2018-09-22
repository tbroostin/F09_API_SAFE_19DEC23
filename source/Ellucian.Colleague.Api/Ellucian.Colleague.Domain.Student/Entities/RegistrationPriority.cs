// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// Contains a priority registration time for a student
    /// </summary>
    [Serializable]
    public class RegistrationPriority
    {
        private string _id;
        private string _studentId;
        private string _termCode;
        private DateTimeOffset? _start;
        private DateTimeOffset? _end;

        public string Id
        {
            get { return _id; }
            set
            {
                if (_id == "")
                {
                    _id = value;
                }
                else
                {
                    throw new InvalidOperationException("id cannot be changed");
                }
            }
        }
        /// <summary>
        /// Id of the student for which the priority applies
        /// </summary>
        public string StudentId { get { return _studentId; } }
        /// <summary>
        /// Term id for which the priority applies. (Optional) If blank it is a non-term type of priority
        /// </summary>
        public string TermCode { get { return _termCode; } }
        /// <summary>
        /// Starting date and time for the priority.
        /// </summary>
        public DateTimeOffset? Start { get { return _start; } }
        /// <summary>
        /// Ending Date and time for the priority. Can be blank.
        /// </summary>
        public DateTimeOffset? End { get { return _end; } }

        /// <summary>
        /// Constructor for the priority registration
        /// </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <param name="termCode"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public RegistrationPriority(string id, string studentId, string termCode, DateTimeOffset? start, DateTimeOffset? end)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "student id is required");
            }
            _id = id;
            _studentId = studentId;
            if (string.IsNullOrEmpty(termCode))
            {
                _termCode = null;
            }
            else
            {
                _termCode = termCode;
            }
            _start = start;
            _end = end;
        }
    }
}
