﻿// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{
    /// <summary>
    /// ColleagueFinanceWebConfiguration
    /// </summary>
    [Serializable]
    public class ColleagueFinanceWebConfiguration
    {
        /// <summary>
        /// This Email Type is used for notifying a Procurement user
        /// </summary>  
        public string DefaultEmailType { get; set; }

        /// <summary>
        /// Flag to determine if GL acct is required field to create requisition
        /// </summary>  
        public bool CfWebReqGlRequired { get; set; }

        /// <summary>
        /// Flag to determine if miscellaneous vendors are allowed while creating requisition
        /// </summary>  
        public bool CfWebReqAllowMiscVendor { get; set; }

        /// <summary>
        /// Default value of APType 
        /// </summary>
        public string DefaultAPTypeCode { get; set; }

        /// <summary>
        /// Requisition Desired Number Of Days 
        /// </summary>
        public int? CfWebReqDesiredDays { get; set; }

        /// <summary>
        /// Flag to determine if GL acct is required field to create purchase order
        /// </summary>  
        public bool CfWebPoGlRequired { get; set; }

        /// <summary>
        /// Flag to determine if miscellaneous vendors are allowed while creating purchase orders
        /// </summary>  
        public bool CfWebPoAllowMiscVendor { get; set; }

        /// <summary>
        /// Default taxcodes
        /// </summary>
        public IEnumerable<string> DefaultTaxCodes { get; set; }

        /// <summary>
        /// PurchasingDefaults
        /// </summary>
        public PurchasingDefaults PurchasingDefaults { get; set; }

        /// <summary>
        /// VoucherWebConfiguration
        /// </summary>
        public VoucherWebConfiguration RequestPaymentDefaults { get; set; }

        /// <summary>
        /// ID of the Attachment Collection that corresponds to Vouchers (aka: Request for Payments).
        /// </summary>
        public string VoucherAttachmentCollectionId { get; set; }
    }
    
}
