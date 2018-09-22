using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Information about Special Needs for individuals
    /// </summary>
    public class SpecialNeed
    {
        /// <summary>
        /// Unique system Id for this Special Need
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this Special Need
        /// </summary>
        public string Description { get; set; }
    }
}
