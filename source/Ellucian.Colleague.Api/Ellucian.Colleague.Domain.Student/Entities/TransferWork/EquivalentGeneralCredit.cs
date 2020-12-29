// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ellucian.Colleague.Domain.Student.Entities.TransferWork
{
    /// <summary>
    /// Equivalent General Credit
    /// </summary>
    [Serializable]
    public class EquivalentGeneralCredit
    {
        private string _subject;
        private string _courseLevel;
        private string _name;
        private string _title;
        private decimal _credits;
        private string _academicLevelId;
        private string _creditType;
        private string _creditStatus;
        private readonly List<string> _departmentCodes;

        /// <summary>
        /// Course Name
        /// </summary>
        public string Name { get { return _name; } }

        /// <summary>
        /// Course Title
        /// </summary>
        public string Title { get { return _title; } }

        /// <summary>
        /// Subject
        /// </summary>
        public string Subject { get { return _subject; } }

        /// <summary>
        /// Subject
        /// </summary>
        public string CourseLevel { get { return _courseLevel; } }

        /// <summary>
        /// Equivalent Credits
        /// </summary>
        public decimal Credits { get { return _credits; } }

        /// <summary>
        /// Acadademic Levels Id
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

        /// <summary>
        /// Departments
        /// </summary>
        public ReadOnlyCollection<string> DepartmentCodes { get; private set; }

        public EquivalentGeneralCredit(string name, string title, string subject, string courseLevel, decimal credits, string academicLevelId, string creditType, string creditStatus, List<string> departmentCodes )
        {
            _name = name;
            _title = title;
            _subject = subject;
            _courseLevel = courseLevel;
            _credits = credits;
            _academicLevelId = academicLevelId;
            _creditType = creditType;
            _creditStatus = creditStatus;
            _departmentCodes = departmentCodes;
            DepartmentCodes = _departmentCodes.AsReadOnly();
        }
    }

}
