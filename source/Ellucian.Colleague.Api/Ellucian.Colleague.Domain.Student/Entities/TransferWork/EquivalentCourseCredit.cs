// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Domain.Student.Entities.TransferWork
{
    /// <summary>
    /// Equivalent Course Credit
    /// </summary>
    [Serializable]
    public class EquivalentCoursCredit
    {
        private string _courseId;
        private string _name;
        private string _title;
        private decimal _credits;
        private string _gradeId;
        private string _academicLevelId;
        private string _creditType;
        private string _creditStatus;

        /// <summary>
        /// Colleague Courses Id
        /// </summary>
        public string CourseId { get { return _courseId; } }

        /// <summary>
        /// Course Name
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Course Title
        /// </summary>
        public string Title { get { return _title; } }

        /// <summary>
        /// Equivalent Credits
        /// </summary>
        public decimal Credits { get { return _credits; } }

        /// <summary>
        /// Grades Id
        /// </summary>
        public string GradeId { get { return _gradeId; } }

        /// <summary>
        /// Academic Level Id
        /// </summary>
        public string AcademicLevelId { get { return _academicLevelId; } }

        /// <summary>
        /// Credit Type
        /// </summary>
        public string CreditType {  get { return _creditType; } }

        /// <summary>
        /// CreditStatus
        /// </summary>
        public string CreditStatus { get { return _creditStatus; } }

        public EquivalentCoursCredit(string courseId, string name, string title, decimal credits, string gradeId, string academicLevelId, string creditType, string creditStatus)
        {
            if (string.IsNullOrEmpty(courseId))
            {
                throw new ArgumentNullException("courseId is requiredl.");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name is requiredl.");
            }
            _courseId = courseId;
            _name = name;
            _title = title;
            _credits = credits;
            _gradeId = gradeId;
            _academicLevelId = academicLevelId;
            _creditType = creditType;
            _creditStatus = creditStatus;
        }
    }

}
