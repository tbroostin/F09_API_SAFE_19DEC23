// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;

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
        /// Flag to determine if GL acct is required field to create purchase order
        /// </summary>  
        public bool CfWebPoGlRequired { get; set; }

        /// <summary>
        /// Flag to determine if miscellaneous vendors are allowed while creating purchase orders
        /// </summary>  
        public bool CfWebPoAllowMiscVendor { get; set; }

        /// <summary>
        /// Default value of APType 
        /// </summary>
        public string DefaultAPTypeCode { get; set; }

        /// <summary>
        /// Requisitions and Purchase order documents are restricted to use this list of AP Types
        /// </summary>
        public IEnumerable<string> RestrictToListedApTypeCodes { get; set; }

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

        /// <summary>
        /// Flag indicating whether the client requires attachments to move vouchers beyond the "In Progress" status.
        /// </summary>
        public bool AreVoucherAttachmentsRequired { get; set; }

        /// <summary>
        /// ID of the Attachment Collection that corresponds to Purchase Order.
        /// </summary>
        public string PurchaseOrderAttachmentCollectionId { get; set; }

        /// <summary>
        /// Flag indicating whether the client requires attachments to move purchase orders beyond the "In Progress" status.
        /// </summary>
        public bool ArePurchaseOrderAttachmentsRequired { get; set; }

        /// <summary>
        /// ID of the Attachment Collection that corresponds to Requisition.
        /// </summary>
        public string RequisitionAttachmentCollectionId { get; set; }

        /// <summary>
        /// Flag indicating whether the client requires attachments to move requisition beyond the "In Progress" status.
        /// </summary>
        public bool AreRequisitionAttachmentsRequired { get; set; }

        /// <summary>
        /// Flag to determine if GL remaining balance is displayed 
        /// </summary>  
        public bool DisplayGlBalance { get; set; }

        /// <summary>
        /// Flag to determine allow non-expense gl accounts
        /// </summary>  
        public bool AllowNonExpenseGlAccounts { get; set; }

        /// <summary>
        /// Requisition field information potentially collected when adding / modifying requisition
        /// </summary>
        public IEnumerable<ProcurementDocumentField> RequisitionFieldRequirements { get; set; }

        /// <summary>
        /// Purchase order field information potentially collected when adding / modifying purchase order
        /// </summary>
        public IEnumerable<ProcurementDocumentField> PurchaseOrderFieldRequirements { get; set; }

        /// <summary>
        /// Voucher field information potentially collected when adding / modifying voucher
        /// </summary>
        public IEnumerable<ProcurementDocumentField> VoucherFieldRequirements { get; set; }
    }
}
