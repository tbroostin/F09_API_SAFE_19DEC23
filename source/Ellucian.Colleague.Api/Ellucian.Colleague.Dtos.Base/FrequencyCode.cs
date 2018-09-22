using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base

{
    /// <summary>
    /// Information about a Frequency Codes
    /// </summary>
    public class FrequencyCode
    {
        /// <summary>
        /// Unique system Id for this Frequency Code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this Frequency Code
        /// </summary>
        public string Description { get; set; }
    }
}