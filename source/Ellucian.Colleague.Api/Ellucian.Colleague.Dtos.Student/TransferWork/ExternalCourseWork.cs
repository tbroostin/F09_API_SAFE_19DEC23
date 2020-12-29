// Copyright 2020 Ellucian Company L.P. and its affiliates.using System;
using System;

namespace Ellucian.Colleague.Dtos.Student.TransferWork
{
    /// <summary>
    /// External Course Work
    /// </summary>
    public class ExternalCourseWork
    {
        /// <summary>
        /// Course Name
        /// </summary>
        public string Course { get; set; }

        /// <summary>
        /// Course Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Transfer Credits
        /// </summary>
        public decimal Credits { get; set; }

        /// <summary>
        /// Grades Id 
        /// </summary>
        public string GradeId { get; set; }

        /// <summary>
        /// Score
        /// </summary>
        public decimal Score { get; set; }

        /// <summary>
        /// Completion Date
        /// </summary>
        public DateTime? EndDate { get; set; }

    }
}
