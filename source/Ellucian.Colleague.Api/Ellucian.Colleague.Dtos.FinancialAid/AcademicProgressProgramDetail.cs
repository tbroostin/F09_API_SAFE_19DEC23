/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.FinancialAid
{
    /// <summary>
    /// AcademicProgressProgramDetail class contains program information
    /// for the academic progress evaluation
    /// </summary>
    public class AcademicProgressProgramDetail
    {
        /// <summary>
        /// Program code
        /// </summary>
        public string ProgramCode { get; set; }
        /// <summary>
        /// Program maximum credits
        /// </summary>
        public decimal? ProgramMaxCredits { get; set; }
        /// <summary>
        /// Program minimum credits
        /// </summary>
        public decimal? ProgramMinCredits { get; set; }
    }
}
