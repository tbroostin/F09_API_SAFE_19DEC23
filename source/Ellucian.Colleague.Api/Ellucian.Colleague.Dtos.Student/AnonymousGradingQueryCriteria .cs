// Copyright 2021 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Query request for a student's Anonymous Grading Ids
    /// </summary>
    public class AnonymousGradingQueryCriteria
    {
        /// <summary>
        /// Id of the student.
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// when provided, only retrieve anonymous grading IDs for specified academic terms.
        /// </summary>
        public IEnumerable<string> TermIds { get; set; }

        /// <summary>
        /// when provided, only retrieve anonymous grading IDs for specified course sections.
        /// </summary>
        public IEnumerable<string> SectionIds { get; set; }
    }
}
