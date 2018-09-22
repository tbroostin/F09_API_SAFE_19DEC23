using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Degree type code and description
    /// </summary>
    public class DegreeType
    {
        /// <summary>
        /// Unique system code for this degree type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Degree type description
        /// </summary>
        public string Description { get; set; }
    }
}