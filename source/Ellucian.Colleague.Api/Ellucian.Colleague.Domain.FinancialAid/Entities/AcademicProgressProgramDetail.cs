/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Entities
{
    /// <summary>
    /// AcademicProgressProgramDetail class contains program information
    /// necessary for academic progress evaluation
    /// </summary>
    [Serializable]
    public class AcademicProgressProgramDetail
    {
        /// <summary>
        /// Program code
        /// </summary>
        public string ProgramCode { get { return programCode; } }
        private readonly string programCode;

        /// <summary>
        /// Program maximum credits
        /// </summary>
        public decimal? ProgramMaxCredits { get { return programMaxCredits; } }
        private readonly decimal? programMaxCredits;

        /// <summary>
        /// Program minimum credits
        /// </summary>
        public decimal? ProgramMinCredits { get { return programMinCredits; } }
        private readonly decimal? programMinCredits;

        /// <summary>
        /// Constructor that accepts program code, max and min credits
        /// </summary>
        /// <param name="code">program code</param>
        /// <param name="maxCredits">program maximum credits</param>
        /// <param name="minCredits">program minimum credits</param>
        public AcademicProgressProgramDetail(string code, decimal? maxCredits, decimal? minCredits)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }
            if (maxCredits.HasValue && maxCredits < 0)
            {
                throw new ArgumentException("maxCredits cannot be less than zero.");
            }
            if (minCredits.HasValue && minCredits < 0)
            {
                throw new ArgumentException("minCredits cannot be less than zero.");
            }
            programCode = code;
            programMaxCredits = maxCredits;
            programMinCredits = minCredits;
        }
    }
}
