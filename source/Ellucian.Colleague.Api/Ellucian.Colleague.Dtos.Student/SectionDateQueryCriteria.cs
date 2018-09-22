using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Dtos.Student
{
    /// <summary>
    /// Used to query a set of section ids 
    /// </summary>
    public class SectionDateQueryCriteria
    {
        /// <summary>
        /// List of section Id being queried
        /// </summary>
        public IEnumerable<string> SectionIds { get; set; }
    }
}
