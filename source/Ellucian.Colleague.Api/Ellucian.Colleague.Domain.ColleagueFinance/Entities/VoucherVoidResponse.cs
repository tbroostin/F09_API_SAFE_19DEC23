using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Represents a response entity of void voucher
    /// </summary>
    [Serializable]
    public class VoucherVoidResponse
    {
        /// <summary>
        /// Voucher ID
        /// </summary>
        public string VoucherId { get; set; }
        
        /// <summary>
        /// Flag to determine if error occured while voiding Voucher
        /// </summary>
        public bool ErrorOccured { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Flag to determine if warning raised while voiding Voucher
        /// </summary>
        public bool WarningOccured { get; set; }

        /// <summary>
        /// Warnings
        /// </summary>
        public List<string> WarningMessages { get; set; }
    }
}
