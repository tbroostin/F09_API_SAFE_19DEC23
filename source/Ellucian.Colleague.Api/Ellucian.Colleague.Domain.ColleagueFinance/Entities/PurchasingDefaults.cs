// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Entities
{

    /// <summary>
    /// Class for Purchasing Defaults
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Is Approval Allow Returns is enabled for Procurement documents (true if Is Approval Returns Flag in APPD is (Y)es)
        /// </summary>
        public bool IsApprovalReturnsEnabled { get; set; }

    }
}
