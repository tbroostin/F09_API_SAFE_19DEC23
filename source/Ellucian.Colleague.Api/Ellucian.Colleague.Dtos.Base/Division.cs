using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// An institutionally-defined division
    /// </summary>
    public class Division
    {
        /// <summary>
        /// Unique ID for this Division
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of Division
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// School for this Division
        /// </summary>
        public string SchoolCode { get; set; }
    }
}