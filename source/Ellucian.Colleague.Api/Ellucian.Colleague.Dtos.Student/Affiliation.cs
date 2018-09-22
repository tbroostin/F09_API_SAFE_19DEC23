using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Affiliation to Campus Organizations
    /// </summary>>
    public class Affiliation
    {
        /// <summary>
        /// Unique code for this affiliation
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Affiliation description
        /// </summary>
        public string Description { get; set; }
    }
}