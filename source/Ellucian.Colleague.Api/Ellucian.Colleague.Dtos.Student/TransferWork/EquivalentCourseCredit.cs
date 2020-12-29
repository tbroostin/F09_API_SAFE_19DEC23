// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student.TransferWork
{
    /// <summary>
    /// Equivalent Course Credit
    /// </summary>
    public class EquivalentCourseCredit
    {
        /// <summary>
        /// Colleague Courses Id
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Course Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Course Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Equivalent Credits
        /// </summary>
        public decimal Credits { get; set; }

        /// <summary>
        /// Grades Id
        /// </summary>
        public string GradeId { get; set; }

        /// <summary>
        /// Academic Levels Id
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

    }
}
