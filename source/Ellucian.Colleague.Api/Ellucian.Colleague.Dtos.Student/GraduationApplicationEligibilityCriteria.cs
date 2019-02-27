// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to define criteria to determine a student's eligibility to submit a graduation application 
    /// </summary>
    public class GraduationApplicationEligibilityCriteria
    {
        /// <summary>
        /// Program Codes for which graduation application eligibility are requested. 
        /// Must provide at least one program code to retrieve graduation application eligibility.
        /// </summary>
        public List<string> ProgramCodes { get; set; }
        /// <summary>
        /// Student for whom eligibility is requested
        /// </summary>
        public string StudentId { get; set; }

    }
}
