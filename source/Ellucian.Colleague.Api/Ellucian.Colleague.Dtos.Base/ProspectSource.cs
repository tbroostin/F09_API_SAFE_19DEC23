using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Prospect source code and description
    /// </summary>
    public class ProspectSource
    {
        /// <summary>
        /// Unique system code for this prospect source
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Prospect source description
        /// </summary>
        public string Description { get; set; }
    }
}