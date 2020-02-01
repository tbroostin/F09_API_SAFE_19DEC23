// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// All midterm grading complete indications for a section
    /// </summary>
    public class SectionMidtermGradingComplete
    {
        /// <summary>
        /// Unique section Id
        /// </summary>
        public string SectionId { get; set; }

        /// <summary>
        /// Indications that midterm grading 1 is complete
        /// </summary>
        public IEnumerable<GradingCompleteIndication> MidtermGrading1Complete { get; set; }

        /// <summary>
        /// Indications that midterm grading 2 is complete
        /// </summary>
        public IEnumerable<GradingCompleteIndication> MidtermGrading2Complete { get; set; }

        /// <summary>
        /// Indications that midterm grading 3 is complete
        /// </summary>
        public IEnumerable<GradingCompleteIndication> MidtermGrading3Complete { get; set; }

        /// <summary>
        /// Indications that midterm grading 4 is complete
        /// </summary>
        public IEnumerable<GradingCompleteIndication> MidtermGrading4Complete { get; set; }

        /// <summary>
        /// Indications that midterm grading 5 is complete
        /// </summary>
        public IEnumerable<GradingCompleteIndication> MidtermGrading5Complete { get; set; }

        /// <summary>
        /// Indications that midterm grading 6 is complete
        /// </summary>
        public IEnumerable<GradingCompleteIndication> MidtermGrading6Complete { get; set; }

    }
}
