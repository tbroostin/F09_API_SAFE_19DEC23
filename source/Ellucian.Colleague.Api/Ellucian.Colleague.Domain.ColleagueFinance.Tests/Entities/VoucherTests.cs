// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// This class tests the Voucher class as well as the abstract classes it extends. When we implement
    /// BPOs, RCVs and Requisitions there will be no need to write tests for the extended classes.
    /// </summary>
    [TestClass]
    public class VoucherTests
    {
        #region Initialize and Cleanup
        private TestVoucherRepository voucherRepository;
        private string personId = "1";
        private int versionNumber;

        [TestInitialize]
        public void Initialize()
        {
            voucherRepository = new TestVoucherRepository();
            versionNumber = 2;
        }

        [TestCleanup]
        public void Cleanup()
        {
            voucherRepository = null;
        }
        #endregion

        #region Constructor tests
        [TestMethod]
        public async Task Voucher_VerifyInheritance()
        {
            var repoVoucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var testVoucher = new Voucher(repoVoucher.Id, repoVoucher.Date, repoVoucher.Status, repoVoucher.VendorName);
            testVoucher.InvoiceNumber = repoVoucher.InvoiceNumber;
            testVoucher.InvoiceDate = repoVoucher.InvoiceDate;

            // BaseFinanceDocument
            Assert.AreEqual(repoVoucher.Id, testVoucher.Id, "Document ID should be initialized.");
            Assert.AreEqual(repoVoucher.Date, testVoucher.Date, "Document date should be initialized.");
            Assert.IsTrue(testVoucher.Approvers is ReadOnlyCollection<Approver>, "The document approvers should be the correct type.");
            Assert.IsTrue(testVoucher.Approvers.Count() == 0, "The document approvers list should have zero items.");

            // AccountsPayablePurchasingDocument
            Assert.AreEqual(repoVoucher.VendorName, testVoucher.VendorName, "Vendor name should be initialized.");
            Assert.IsTrue(testVoucher.LineItems is ReadOnlyCollection<LineItem>, "Document line items should be the correct type.");
            Assert.IsTrue(testVoucher.LineItems.Count() == 0, "Document line items should be initialized with 0 items.");
        }

        [TestMethod]
        public async Task Voucher_ConstructorInitialization()
        {
            var repoVoucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var testVoucher = new Voucher(repoVoucher.Id, repoVoucher.Date, repoVoucher.Status, repoVoucher.VendorName);
            testVoucher.InvoiceNumber = repoVoucher.InvoiceNumber;
            testVoucher.InvoiceDate = repoVoucher.InvoiceDate;

            // Voucher constructor
            Assert.AreEqual(repoVoucher.Status, testVoucher.Status, "Status should be initialized.");
            Assert.AreEqual(repoVoucher.InvoiceNumber, testVoucher.InvoiceNumber, "Invoice number should be initialized.");
            Assert.AreEqual(repoVoucher.InvoiceDate, testVoucher.InvoiceDate, "Invoice date should be initialized.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Voucher_NullVoucherId()
        {
            var voucher = new Voucher(null, new DateTime(2015, 1, 1), VoucherStatus.Outstanding, "Susty Corporation");
            voucher.InvoiceNumber = "IN12345";
            voucher.InvoiceDate = new DateTime(2015, 1, 5);
        }
        #endregion

        #region AddApprover tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddApprover_NullApprover()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            voucher.AddApprover(null);
        }

        [TestMethod]
        public async Task AddApprover_DuplicateId_DifferentName()
        {
            var voucher = await voucherRepository.GetVoucherAsync("4", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var approver1 = new Approver("0000123");
            approver1.SetApprovalName("Jeeves");
            approver1.ApprovalDate = new DateTime(2015, 1, 10);

            var approver2 = new Approver("0000123");
            approver2.SetApprovalName("Jenkins");
            approver2.ApprovalDate = new DateTime(2015, 1, 10);

            Assert.AreEqual(0, voucher.Approvers.Count(), "The voucher should start with 0 approvers.");
            voucher.AddApprover(approver1);
            Assert.AreEqual(1, voucher.Approvers.Count(), "The voucher should now have 1 approvers.");

            // Change the name and re-add the approver
            voucher.AddApprover(approver2);
            Assert.AreEqual(1, voucher.Approvers.Count(), "The voucher should still have 1 approvers.");
        }

        [TestMethod]
        public async Task AddApprover_DifferentId_DuplicateName()
        {
            var voucher = await voucherRepository.GetVoucherAsync("4", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var approver1 = new Approver("0000123");
            approver1.SetApprovalName("Jeeves");
            approver1.ApprovalDate = new DateTime(2015, 1, 10);

            var approver2 = new Approver("0000345");
            approver2.SetApprovalName("Jeeves");
            approver2.ApprovalDate = new DateTime(2015, 1, 10);

            Assert.AreEqual(0, voucher.Approvers.Count(), "The voucher should start with 0 approvers.");
            voucher.AddApprover(approver1);
            Assert.AreEqual(1, voucher.Approvers.Count(), "The voucher should now have 1 approvers.");

            // Change the name and re-add the approver
            voucher.AddApprover(approver2);
            Assert.AreEqual(2, voucher.Approvers.Count(), "The voucher should now have 2 approvers.");
        }

        [TestMethod]
        public async Task AddApprover_Success()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var approverCount = voucher.Approvers.Count();
            string approverId = "0001102";
            string approverName = "Aimee Rodgers";

            var newApprover = new Approver(approverId);
            newApprover.SetApprovalName(approverName);
            voucher.AddApprover(newApprover);
            var approver = voucher.Approvers.Where(x => x.ApprovalName == approverName).FirstOrDefault();

            Assert.IsTrue(voucher.Approvers.Count() == approverCount + 1);
            Assert.IsNull(approver.ApprovalDate);
        }
        #endregion

        #region AddLineItem tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddLineItem_NullLineItem()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            voucher.AddLineItem(null);
        }

        [TestMethod]
        public async Task AddLineItem_DuplicateLineItem()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var lineItem = voucher.LineItems.FirstOrDefault();
            var lineItemCount = voucher.LineItems.Count();
            voucher.AddLineItem(lineItem);

            Assert.IsTrue(voucher.LineItems.Count() == lineItemCount, "The size of the line items list should be the same as it's original size.");
        }

        [TestMethod]
        public async Task AddLineItem_Success()
        {
            var voucher = await voucherRepository.GetVoucherAsync("1", personId, GlAccessLevel.Full_Access, null, versionNumber);
            var lineItemCount = voucher.LineItems.Count();
            string lineItemId = "7483",
                description = "Line item";
            decimal quantity = 15m,
                price = 550.02m,
                extendedPrice = 600.00m;

            voucher.AddLineItem(new LineItem(lineItemId, description, quantity, price, extendedPrice));
            var lineItem = voucher.LineItems.Where(x => x.Id == lineItemId).FirstOrDefault();

            Assert.IsTrue(voucher.LineItems.Count() == lineItemCount + 1, "The line items count should reflect the new line item added.");
            Assert.AreEqual(description, lineItem.Description, "The description of the line item should match description of the line item added.");
            Assert.AreEqual(quantity, lineItem.Quantity, "The quantity of the line item should match quantity of the line item added.");
            Assert.AreEqual(price, lineItem.Price, "The price of the line item should match price of the line item added.");
            Assert.AreEqual(extendedPrice, lineItem.ExtendedPrice, "The extended price of the line item should match extended price of the line item added.");
        }
        #endregion

        #region PurchaseOrderId tests
        [TestMethod]
        public void PurchaseOrderId_Success()
        {
            string purchaseOrderId = "P000001";
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.Outstanding, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.PurchaseOrderId = purchaseOrderId;
            Assert.AreEqual(purchaseOrderId, voucher.PurchaseOrderId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void PurchaseOrderId_BpoAlreadySet()
        {
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.InProgress, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.BlanketPurchaseOrderId = "B000001";
            voucher.PurchaseOrderId = "P000001";
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void PurchaseOrderId_RcvAlreadySet()
        {
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.InProgress, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.RecurringVoucherId = "RV000001";
            voucher.PurchaseOrderId = "P000001";
        }
        #endregion

        #region BlanketPurchaseOrderId tests
        [TestMethod]
        public void BlanketPurchaseOrderId_Success()
        {
            string blanketPurchaseOrderId = "B000001";
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.InProgress, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.BlanketPurchaseOrderId = blanketPurchaseOrderId;
            Assert.AreEqual(blanketPurchaseOrderId, voucher.BlanketPurchaseOrderId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void BlanketPurchaseOrderId_PoAlreadySet()
        {
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.InProgress, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.PurchaseOrderId = "P000001";
            voucher.BlanketPurchaseOrderId = "B000001";
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void BlanketPurchaseOrderId_RcvAlreadySet()
        {
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.InProgress, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.RecurringVoucherId = "RV000001";
            voucher.BlanketPurchaseOrderId = "B000001";
        }
        #endregion

        #region RecurringVoucherId tests
        [TestMethod]
        public void RecurringVoucherId_Success()
        {
            string recurringVoucherId = "RV000001";
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.Cancelled, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.RecurringVoucherId = recurringVoucherId;
            Assert.AreEqual(recurringVoucherId, voucher.RecurringVoucherId);
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void RecurringVoucherId_PoAlreadySet()
        {
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.Cancelled, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.PurchaseOrderId = "P000001";
            voucher.RecurringVoucherId = "RV000001";
        }

        [TestMethod]
        [ExpectedException(typeof(ApplicationException))]
        public void RecurringVoucherId_BpoAlreadySet()
        {
            var voucher = new Voucher("1", new DateTime(2015, 1, 1), VoucherStatus.Cancelled, "Susty Corp");
            voucher.InvoiceNumber = "IN1234";
            voucher.InvoiceDate = new DateTime(2015, 1, 1);
            voucher.BlanketPurchaseOrderId = "B000001";
            voucher.RecurringVoucherId = "RV000001";
        }
        #endregion
    }
}
