// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.TransferWork
{
    /// <summary>
    /// External Non Course Work
    /// </summary>
    [Serializable]
    public class ExternalNonCourseWork
    {
        private string _name;
        private string _title;
        private string _gradeId;
        private decimal _score;
        private DateTime? _endDate;
        private string _status;

        /// <summary>
        /// Course Name
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Course Title
        /// </summary>
        public string Title { get { return _title; } }

        /// <summary>
        /// Score
        /// </summary>
        public decimal Score { get { return _score; } }

        /// <summary>
        /// Grades Id 
        /// </summary>
        public string GradeId { get { return _gradeId; } }

        /// <summary>
        /// Completion Date
        /// </summary>
        public DateTime? EndDate { get { return _endDate; } }

        /// <summary>
        /// Non Course Status
        /// </summary>
        public string Status { get { return _status; } }

        public ExternalNonCourseWork(string name, string title, string gradeId, decimal score, DateTime? endDate, string status)
        {
            _name = name;
            _title = title;
            _gradeId = gradeId;
            _score = score;
            _endDate = endDate;
            _status = status;
        }
    }

}
