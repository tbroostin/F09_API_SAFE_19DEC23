// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities
{
    /// <summary>
    /// waitlisted student with details on student id, rating and status date for a course section
    /// </summary>
    [Serializable]
    public class SectionWaitlistStudent
    {
        private string _sectionId;

        /// <summary>
        /// Course section ID
        /// </summary>
        public string SectionId { get { return _sectionId; } }

        private string _studentId;

        /// <summary>
        /// Student Id
        /// </summary>
        public string StudentId { get { return _studentId; } }

        private int? _rank;

        /// <summary>
        /// Rank
        /// </summary>
        public int? Rank { get { return _rank; } }

        private int? _rating;

        /// <summary>
        /// Rating
        /// </summary>
        public int? Rating { get { return _rating; } }

        private String _statusCode;

        /// <summary>
        /// Status code of the waitlisted student
        /// </summary>
        public String StatusCode { get { return _statusCode; } }


        private DateTime? _statusDate;

        /// <summary>
        /// Status Date
        /// </summary>
        public DateTime? StatusDate { get { return _statusDate; } }

        private DateTime? _waitTime;

        /// <summary>
        /// Wait Time
        /// </summary>
        public DateTime? WaitTime { get { return _waitTime; } }


        /// <summary>
        /// Creates a new instance of the <see cref="SectionWaitlist"/> class
        /// </summary>
        /// <param name="sectionId">Course section ID</param>
        public SectionWaitlistStudent(string sectionId, string studentId, int? rank, int? rating, string statusCode,DateTime? statusDate,DateTime? waitTime)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                throw new ArgumentNullException("studentId", "Cannot build a section waitlist without a student ID.");
            }
            if (string.IsNullOrEmpty(sectionId))
            {
                throw new ArgumentNullException("sectionId", "Cannot build a section waitlist without a section ID.");
            }
            _sectionId = sectionId;
            _studentId = studentId;
            _rank = rank;
            _rating = rating;
            _statusDate = statusDate;
            _waitTime = waitTime;
            _statusCode = statusCode;
        }
    }
}
