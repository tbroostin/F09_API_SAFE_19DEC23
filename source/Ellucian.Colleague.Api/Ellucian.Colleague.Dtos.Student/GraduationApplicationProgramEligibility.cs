// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Indicates eligibility of a student to apply to graduation from a particular program of study.
    /// </summary>
    public class GraduationApplicationProgramEligibility
    {
        /// <summary>
        /// Id of student for which eligibility applies
        /// </summary>
        public string StudentId { get; set; }
        
        /// <summary>
        /// Program code for which eligibility applies
        /// </summary>
        public string ProgramCode { get; set; }
        /// <summary>
        /// Boolean indicates if the student is eligible to apply for graduation in this student program
        /// </summary>
        public bool IsEligible { get; set; }
        /// <summary>
        /// List of reasons explaining why student is not eligible to apply to graduate in the associated student program.
        /// </summary>
        public IEnumerable<string> IneligibleMessages { get; set; }

    }
}
