// Copyright 2020 Ellucian Company L.P. and its affiliates.using System;
using System;

namespace Ellucian.Colleague.Dtos.Student.TransferWork
{
    /// <summary>
    /// External Non Course Work
    /// </summary>
    public class ExternalNonCourseWork
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
        /// Score
        /// </summary>
        public decimal Score { get; set; }

        /// <summary>
        /// Grades Id 
        /// </summary>
        public string GradeId { get; set; }

        /// <summary>
        /// Completion Date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Non Course Status
        /// </summary>
        public string Status { get; set; }
    }
}
