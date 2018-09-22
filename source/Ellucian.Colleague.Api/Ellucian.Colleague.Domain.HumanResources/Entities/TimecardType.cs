using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public enum TimecardType
    {
        /// <summary>
        /// No timecard type will be applied to the associated position
        /// </summary>
        None,
        /// <summary>
        /// Summary hours
        /// </summary>
        Summary,
        /// <summary>
        /// Detailed time
        /// </summary>
        Detail
    }
}
