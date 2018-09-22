using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Information about an Advisor Type
    /// </summary>
    public class AdvisorType
    {
        /// <summary>
        /// Unique system Id for this advisor type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of the Advisor Type
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Rank.  Types can have a numeric value/rank
        /// where the lower the number, the more important
        /// the type.
        /// </summary>
        public string Rank { get; set; }

    }
}
