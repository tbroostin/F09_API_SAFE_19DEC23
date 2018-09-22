using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Denomination code and description
    /// </summary>
    public class Denomination
    {
        /// <summary>
        /// Unique system code for this denomination
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Denomination description
        /// </summary>
        public string Description { get; set; }
    }
}