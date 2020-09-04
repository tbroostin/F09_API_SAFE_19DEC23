using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Represents a request to void voucher
    /// </summary>
    public class VoucherVoidRequest
    {
        /// <summary>
        /// Person ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Voucher ID
        /// </summary>
        public string VoucherId { get; set; }

        /// <summary>
        ///  ConfirmationEmailAddresses
        /// </summary>
        public string ConfirmationEmailAddresses { get; set; }

        /// <summary>
        /// The voucher comments
        /// </summary>
        public string Comments { get; set; }
    }
}
