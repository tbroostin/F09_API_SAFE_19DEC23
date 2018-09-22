using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student.Requirements
{
    /// <summary>
    /// Requirement clause limiting the number of credits that can be taken at given CourseLevels
    /// "MAXIMUM 6 100,200 LEVEL CREDITS"
    /// </summary>
    public class MaxCreditAtLevels
    {
        /// <summary>
        /// Maximum number of credits
        /// </summary>
        public int MaxCredits { get; set; }
        /// <summary>
        /// CourseLevels to which this limit applies
        /// </summary>
        public IEnumerable<string> Levels { get; set; }
    }
}