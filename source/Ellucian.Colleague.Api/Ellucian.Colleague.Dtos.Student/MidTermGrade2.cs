// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;

namespace Ellucian.Colleague.Dtos.Student {

    /// <summary>
    /// Intermediate grades recorded for an academic credit.
    /// </summary>
    public class MidTermGrade2 {
        /// <summary>
        /// Sequence of this grade relative to the other midterm grades
        /// </summary>
        public int Position { get; set; }
        /// <summary>
        /// Id of the grade earned
        /// </summary>
        public string GradeId { get; set; }
        /// <summary>
        /// Date/time midterm grade recorded
        /// </summary>
        public DateTimeOffset? GradeTimestamp { get; set; }
    }
}
