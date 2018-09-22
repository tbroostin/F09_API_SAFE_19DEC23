using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// An institutionally-defined disability
    /// </summary>
    public class DisabilityType
    {
        /// <summary>
        /// Unique code for this disability
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of disability
        /// </summary>
        public string Description { get; set; }
    }
}

