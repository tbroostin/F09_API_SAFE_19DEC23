using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Citizen status code and description
    /// </summary>
    public class CitizenType
    {
        /// <summary>
        /// Unique code for this citizen type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this citizen type
        /// </summary>
        public string Description { get; set; }
    }
}