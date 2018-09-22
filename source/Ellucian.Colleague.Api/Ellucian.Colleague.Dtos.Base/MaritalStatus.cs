using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Marital status code and description
    /// </summary>
    public class MaritalStatus
    {
        /// <summary>
        /// Unique system code for this marital status
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Marital status description
        /// </summary>
        public string Description { get; set; }
    }
}