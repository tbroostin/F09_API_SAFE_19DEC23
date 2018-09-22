using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Visa type code and description
    /// </summary>
    public class VisaType
    {
        /// <summary>
        /// Unique system code for this visa type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Visa type description
        /// </summary>
        public string Description { get; set; }
    }
}