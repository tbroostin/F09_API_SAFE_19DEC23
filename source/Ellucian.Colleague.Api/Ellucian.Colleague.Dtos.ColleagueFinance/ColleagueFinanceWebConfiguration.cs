// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This holds all the Procurement default values
    /// </summary>
    public class ColleagueFinanceWebConfiguration
    {
        /// <summary>
        /// Default Email Type 
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
        /// Requisition Desired Number Of Days 
        /// </summary>
        public int? CfWebReqDesiredDays { get; set; }

        /// <summary>
        /// Default value of APType 
        /// </summary>
        public string DefaultAPTypeCode { get; set; }

        /// <summary>
        /// Default taxcodes
        /// </summary>
        public IEnumerable<string> DefaultTaxCodes { get; set; }

        /// <summary>
        /// PurchasingDefaults
        /// </summary>
        public PurchasingDefaults PurchasingDefaults { get; set; }
    }
}
