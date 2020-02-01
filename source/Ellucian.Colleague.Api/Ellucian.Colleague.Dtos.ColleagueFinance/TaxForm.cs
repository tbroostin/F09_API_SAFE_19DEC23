using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Tax Form
    /// </summary>
    public class TaxForm
    {
        /// <summary>
        /// Unique code of this Tax Form
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Description of this Tax Form
        /// </summary>
        public string Description { get; set; }
    }
}
