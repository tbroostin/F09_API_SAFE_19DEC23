using Ellucian.Colleague.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.HumanResources.Entities
{
    /// <summary>
    /// JobChangeReason
    /// </summary>
    [Serializable]
    public class JobChangeReason : GuidCodeItem
    {
        /// <summary>
         /// Initializes a new instance of the <see cref="JobChangeReason"/> class.
        /// </summary>
         public JobChangeReason(string guid, string code, string description)
            : base(guid, code, description)
        {
        }
    }
}
