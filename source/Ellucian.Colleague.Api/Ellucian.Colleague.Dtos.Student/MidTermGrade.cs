using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student {

    /// <summary>
    /// Intermediate grades recorded for an academic credit.
    /// </summary>
    public class MidTermGrade {
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
        public DateTime? GradeTimestamp { get; set; }
    }
}
