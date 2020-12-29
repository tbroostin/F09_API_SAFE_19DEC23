// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// Voucher Web Configuration
    /// </summary>
    [Serializable]
    public class VoucherWebConfiguration
    {
        /// <summary>
        /// Default value of APType 
        /// </summary>
        public string DefaultAPTypeCode { get; set; }

        /// <summary>
        /// Invoice entry required Flag
        /// </summary>
        public bool IsInvoiceEntryRequired { get; set; }

        /// <summary>
        /// Allow Miscellaneous Vendor
        /// </summary>
        public bool AllowMiscVendor { get; set; }

        /// <summary>
        /// Gl required flag for Voucher
        /// </summary>
        public bool GlRequiredForVoucher { get; set; }

        /// <summary>
        /// Is Voucher Approval Needed (true if Voucher Approval Needed Flag in RPYP is (Y)es or (A)uto-Populate)
        /// </summary>
        public bool IsVoucherApprovalNeeded { get; set; }
    }
}
