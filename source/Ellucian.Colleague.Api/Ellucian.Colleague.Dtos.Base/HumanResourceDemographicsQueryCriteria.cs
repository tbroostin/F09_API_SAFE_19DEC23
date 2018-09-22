using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Used to create a query for HumanResourceDemographics  
    /// </summary>
    public class HumanResourceDemographicsQueryCriteria
    {

        /// <summary>
        /// Person Ids to retrieve - may not be null or empty
        /// </summary>
        public IEnumerable<string> Ids { get; set; }

    }
}
