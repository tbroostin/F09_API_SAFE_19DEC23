﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    /// This is Procurement item information response DTO
    /// </summary>
   public class ProcurementItemInformationResponse
    {
        /// <summary>
        /// Purchase order Id
        /// </summary>
        public string PurchaseOrderId { get; set; }

        /// <summary>
        /// Purchase Order Number
        /// </summary>
        public string PurchaseOrderNumber { get; set; }

        /// <summary>
        /// Item Id
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Item Description
        /// </summary>
        public string ItemDescription { get; set; }
    }
}
