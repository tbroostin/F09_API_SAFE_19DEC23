using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Provides a class that describes a planned credit.
    /// </summary>
    public class PlannedCredit
    {
        /// <summary>
        /// Id of the course that is being planned. Required.
        /// </summary>
        public string CourseId { get; set; }

        /// <summary>
        /// Term Id for which the course is planned.  If blank this is a "non-term" item.
        /// </summary>
        public string TermCode { get; set; }

        /// <summary>
        /// Status indicates whether credit is replaced or possibly replaced
        /// </summary>
        public ReplacedStatus ReplacedStatus { get; set; }

        /// <summary>
        /// Status indicates whether credit is a replacement or a possible replacement of another credit
        /// </summary>
        public ReplacementStatus ReplacementStatus { get; set; }

    }
}
