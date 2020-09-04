// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// Represents a response of create / update voucher
    /// </summary>
    public class VoucherCreateUpdateResponse
    {
        /// <summary>
        /// Voucher ID
        /// </summary>
        public string VoucherId { get; set; }


        /// <summary>
        /// The Voucher date
        /// </summary>
        public DateTime VoucherDate { get; set; }

        /// <summary>
        /// Flag to determine if error occured while creating purchase Order
        /// </summary>
        public bool ErrorOccured { get; set; }
        /// <summary>
        /// Errors
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// Flag to determine if warning raised while creating purchase Order
        /// </summary>
        public bool WarningOccured { get; set; }

        /// <summary>
        /// Warnings
        /// </summary>
        public List<string> WarningMessages { get; set; }
    }
}
