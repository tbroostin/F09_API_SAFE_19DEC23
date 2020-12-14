// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Dtos.ColleagueFinance
{
    /// <summary>
    ///  This holds all the Purchasing default values  
    /// </summary>
    public class PurchasingDefaults
    {
        /// <summary>
        /// Default value of ShipToCode 
        /// </summary>
        public string DefaultShipToCode { get; set; }

        /// <summary>
        /// Is Requisition Approval Needed  (true if Requisition Approval Needed Flag in PUWP is (Y)es or (A)uto-Populate)
        /// </summary>
        public bool IsRequisitionApprovalNeeded { get; set; }

        /// <summary>
        /// Is PO Approval Needed (true if PO Approval Needed Flag in PUWP is (Y)es or (A)uto-Populate)
        /// </summary>
        public bool IsPOApprovalNeeded { get; set; }

    }
}
