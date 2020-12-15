// Copyright 2020 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.TransferWork
{
    /// <summary>
    /// Equivalent General Credit
    /// </summary>
    public class EquivalentGeneralCredit
    {
        /// <summary>
        /// Course Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Course Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Subject
        /// </summary>
        public string CourseLevel { get; set; }

        /// <summary>
        /// Equivalent Credits
        /// </summary>
        public decimal Credits { get; set; }

        /// <summary>
        /// Acad Levels Id
        /// </summary>
        public string AcademicLevelId { get; set; }

        /// <summary>
        /// Credit Type
        /// </summary>
        public string CreditType { get; set; }

        /// <summary>
        /// CreditStatus
        /// </summary>
        public string CreditStatus { get; set; }

        /// <summary>
        /// Departments
        /// </summary>
        public List<string> DepartmentCodes { get; set; }
    }
}
