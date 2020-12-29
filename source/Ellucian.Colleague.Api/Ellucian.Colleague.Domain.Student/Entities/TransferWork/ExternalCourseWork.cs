// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.TransferWork
{

    /// <summary>
    /// External Course Work
    /// </summary>
    [Serializable]
    public class ExternalCourseWork
    {
        private string _course;
        private string _title;
        private decimal _credits;
        private string _gradeId;
        private DateTime? _endDate;
        private string _status;

        /// <summary>
        /// Course Name
        /// </summary>
        public string Course { get { return _course; } }

        /// <summary>
        /// Course Title
        /// </summary>
        public string Title { get { return _title; } }

        /// <summary>
        /// Transfer Credits
        /// </summary>
        public decimal Credits { get { return _credits; } }

        /// <summary>
        /// Grades Id 
        /// </summary>
        public string GradeId { get { return _gradeId; } }

        /// <summary>
        /// Completion Date
        /// </summary>
        public DateTime? EndDate { get { return _endDate; } }

        public ExternalCourseWork(string course, string title, decimal credits, string gradeId, DateTime? endDate)
        {
            _course = course;
            _title = title;
            _credits = credits;
            _gradeId = gradeId;
            _endDate = endDate;
        }
    }

}
