using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Interest code and description
    /// </summary>
    public class Interest
    {
        /// <summary>
        /// Unique system code for this interest
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Interest description
        /// </summary>
        public string Description { get; set; }
    }
}