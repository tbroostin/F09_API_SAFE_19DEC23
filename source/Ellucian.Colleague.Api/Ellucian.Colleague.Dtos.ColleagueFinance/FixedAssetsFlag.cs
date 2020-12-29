using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Fixed Asset transfer flags used for procurement line items
    /// </summary>
    public class FixedAssetsFlag
    {
        /// <summary>
        /// Unique code of this Fixed Asset transfer flag
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this Fixed Asset transfer flag
        /// </summary>
        public string Description { get; set; }
    }
}
