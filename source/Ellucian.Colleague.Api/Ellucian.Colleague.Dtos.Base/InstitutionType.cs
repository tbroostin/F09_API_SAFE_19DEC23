using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Institution type code and description
    /// </summary>
    public class InstitutionType
    {
        /// <summary>
        /// Unique system code for this institution type
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Institution type description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Category of either "H" or "C" (High School or College)
        /// </summary>
        public string Category { get; set; }
    }
    }
