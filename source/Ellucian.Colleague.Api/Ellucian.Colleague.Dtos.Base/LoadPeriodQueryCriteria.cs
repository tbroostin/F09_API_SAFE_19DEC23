using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Load Period Query Criteria
    /// </summary>
    public class LoadPeriodQueryCriteria
    {
        /// <summary>
        /// Load Period Ids
        /// </summary>
        public IEnumerable<string> Ids { get; set; }
    }
}
