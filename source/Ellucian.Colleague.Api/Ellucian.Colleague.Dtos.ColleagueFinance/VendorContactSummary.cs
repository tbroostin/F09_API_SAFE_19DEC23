using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Vendor's Contact information
    /// </summary>
    public class VendorContactSummary
    {
        /// <summary>
        /// Vendor Contact's Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Vendor Contact's Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Vendor Contact's Phone Number
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Vendor Contact's Email Address
        /// </summary>
        public string Email { get; set; }
    }
}
