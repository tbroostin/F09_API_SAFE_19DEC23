// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Represents a request to create/update new voucher
    /// </summary>
    public class VoucherCreateUpdateRequest
    {
        /// <summary>
        /// Person ID
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// List of email address for which confirmation mail will be sent.
        /// </summary>
        public List<string> ConfEmailAddresses { get; set; }
        
        /// <summary>
        /// The voucher object
        /// </summary>
        public Voucher2 Voucher { get; set; }

        /// <summary>
        /// Vendor Info with Address details
        /// </summary>
        public VendorsVoucherSearchResult VendorsVoucherInfo { get; set; }
    }
}
