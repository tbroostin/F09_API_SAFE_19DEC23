using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// Enumeration of rehire-type eligibility status
    /// </summary>
    [Serializable]
    public enum RehireTypeCategory
    {
        /// <summary>
        /// Eligible
        /// </summary>
        Eligible,
        /// <summary>
        /// Ineligible
        /// </summary>
        Ineligible
    }
}
