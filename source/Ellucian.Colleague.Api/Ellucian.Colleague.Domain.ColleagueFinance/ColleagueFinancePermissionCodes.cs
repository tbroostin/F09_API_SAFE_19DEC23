// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base;
using System;

namespace Ellucian.Colleague.Domain.ColleagueFinance
{
    [Serializable]
    public static class ColleagueFinancePermissionCodes
    {
        // Update any Account Payable Invoice
        public const string UpdateApInvoices = "UPDATE.AP.INVOICES";

        // Update any Vendor
        public const string UpdateVendors = "UPDATE.VENDORS";

        //View any Account Funds Available information
        public const string ViewAccountFundsAvailable = "VIEW.ACCOUNT.FUNDS.AVAILABLE";

        //Permissions code that allows an external system to do a READ operation.
        public const string ViewLedgerActivities = "VIEW.LEDGER.ACTIVITIES";

        // View any Account Payable Invoice
        public const string ViewApInvoices = "VIEW.AP.INVOICES";

        // View any vendor information
        public const string ViewVendors = "VIEW.VENDORS";

        // View any vendor contacts information
        public const string ViewVendorContacts = "VIEW.VENDOR.CONTACT";
        
        // Create / Update any vendor contacts information
        public const string ProcessVendorContact = "PROCESS.VENDOR.CONTACT";

        // Update any Purchase Order
        public const string UpdatePurchaseOrders = "UPDATE.PURCHASE.ORDERS";

        // View any PurchaseOrder
        public const string ViewPurchaseOrders = "VIEW.PURCHASE.ORDERS";

        // View any BlanketPurchaseOrder
        public const string ViewBlanketPurchaseOrders = "VIEW.BLANKET.PURCHASE.ORDERS";

        // Update any BlanketPurchaseOrder
        public const string UpdateBlanketPurchaseOrders = "UPDATE.BLANKET.PURCHASE.ORDERS";

        // View any Requisitions
        public const string ViewRequisitions = "VIEW.REQUISITIONS";

        //View Grants
        public const string ViewGrants = "VIEW.GRANTS";

        // Update any Requisitions
        public const string UpdateRequisitions = "UPDATE.REQUISITIONS";

        // Delete any Requisitions
        public const string DeleteRequisitions = "DELETE.REQUISITIONS";
   
        // View any Payment Transaction
        public const string ViewPaymentTransactionsIntg = "VIEW.PAYMENT.TRANSACTIONS";

        // View any BudgetCode
        public const string ViewBudgetCode = "VIEW.BUDGET.CODES";

        // View any BudgetPhase
        public const string ViewBudgetPhase = "VIEW.BUDGET.PHASES";

        // Create GL Postings
        public const string CreateGLPostings = "CREATE.GL.POSTINGS";

        //Create Journal Entries
        public const string CreateJournalEntries = "CREATE.JOURNAL.ENTRIES";

        //Create Budget Entries
        public const string CreateBudgetEntries = "CREATE.BUDGET.ENTRIES";

        //Create Encumbrance Entries
        public const string CreateEncumbranceEntries = "CREATE.ENCUMBRANCE.ENTRIES";

        //View any BudgetPhaseLineItems
        public const string ViewBudgetPhaseLineItems = "VIEW.BUDGET.PHASE.LINE.ITEMS";

        //View any Procurement Receipts
        public const string ViewProcurementReceipts = "VIEW.PROCUREMENT.RECEIPTS";

        //Create any Procurement Receipts
        public const string CreateProcurementReceipts = "CREATE.PROCUREMENT.RECEIPTS";

        //View any FixedAssets
        public const string ViewFixedAssets = "VIEW.FIXED.ASSETS";

        //View any Accounting Strings 
        public const string ViewAccountingStrings = "VIEW.ACCOUNTING.STRINGS";

        //Causes the API to assign tax forms, box codes, and locations to line items based solely on the VENDORS record in Colleague, regardless of any tax form that may or may not be present in the payload. 
        public const string ByPassTaxForms = "BYPASS.PARTNER.TAX.FORMS";

        //Allows this partner system to create purchase orders directly in an "outstanding" status even if Colleague approvals are turned on.
        public const string ByPassVoucherApproval = "BYPASS.COLL.VOU.APPROVALS";

        //Allows this partner system to create purchase orders directly in an "outstanding" status even if Colleague approvals are turned on.
        public const string ByPassPurchaseOrderApproval = "BYPASS.COLL.PO.APPROVALS";

        //Allows this partner system to create purchase orders directly in an "outstanding" status even if Colleague approvals are turned on.
        public const string ByPassRequisitionApproval = "BYPASS.COLL.REQ.APPROVALS";

        //Allows this partner system to create blanket purchase orders directly in an "outstanding" status even if Colleague approvals are turned on.
        public const string ByPassBlanketPurchaseOrderApproval = "BYPASS.COLL.BPO.APPROVALS";


        // The following section contains permissions created by the Colleague Financials
        // team, which are kept separated from the permissions created by the Ethos team.

        #region Permissions created by the CF team

        // Enable user to view their own T4A information
        public const string ViewT4A = BasePermissionCodes.ViewT4A;

        // Enable user to view another user's T4A information (ie: Tax Information Admin)
        public const string ViewRecipientT4A = BasePermissionCodes.ViewRecipientT4A;

        // Create and update Draft Budget Adjustments and Budget Adjustments
        public const string CreateUpdateBudgetAdjustments = "CREATE.UPDATE.BUDGET.ADJUSTMENT";

        // View any Draft Budget Adjustments and Budget Adjustments
        public const string ViewBudgetAdjustments = "VIEW.BUDGET.ADJUSTMENT";

        // Delete any Draft Budget Adjustments and Budget Adjustments
        public const string DeleteBudgetAdjustments = "DELETE.BUDGET.ADJUSTMENT";

        // View any Budget Adjustments pending approval.
        public const string ViewBudgetAdjustmentsPendingApproval = "VIEW.BUD.ADJ.PENDING.APPR";

        // View any blanket purchase order
        public const string ViewBlanketPurchaseOrder = "VIEW.BLANKET.PURCHASE.ORDER";

        // View any journal entry
        public const string ViewJournalEntry = "VIEW.JOURNAL.ENTRY";

        // View any purchase order
        public const string ViewPurchaseOrder = "VIEW.PURCHASE.ORDER";

        // View any recurring voucher
        public const string ViewRecurringVoucher = "VIEW.RECURRING.VOUCHER";

        // View any requisition
        public const string ViewRequisition = "VIEW.REQUISITION";

        // Create or Update any requisition
        public const string CreateUpdateRequisition = "CREATE.UPDATE.REQUISITION";

        // Delete any requisition
        public const string DeleteRequisition = "DELETE.REQUISITION";

        // Create or Update any purchase order
        public const string CreateUpdatePurchaseOrder = "CREATE.UPDATE.PURCHASE.ORDER";

        // View any voucher
        public const string ViewVoucher = "VIEW.VOUCHER";

        // Enable user to view their own 1099MI information
        public const string View1099MISC = BasePermissionCodes.View1099MISC;

        // View any vendor information/s
        public const string ViewVendor = "VIEW.VENDOR";

        // View or Update procurement items
        public const string ViewUpdateProcurementReceiving = "UPDATE.RECEIVING";

        // View your document approval
        public const string ViewDocumentApproval = "VIEW.DOCUMENT.APPROVAL";
        // Create or Update Voucher
        public const string CreateUpdateVoucher = "CREATE.UPDATE.VOUCHER";

        #endregion
    }
}