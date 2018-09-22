using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Data page filter
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// Filter value
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Count of items containing this value
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// Indicates whether this filter value was selected
        /// </summary>
        public bool Selected { get; set; }
    }
}