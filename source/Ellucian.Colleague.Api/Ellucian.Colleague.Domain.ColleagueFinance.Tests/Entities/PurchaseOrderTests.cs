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
    /// Test valid and invalid conditions for a purchase order repository.
    /// </summary>
    [TestClass]
    public class PurchaseOrderTests
    {
        #region Initialize and Cleanup
        private TestPurchaseOrderRepository purchaseOrderRepository;
        private string personId = "1";

        [TestInitialize]
        public void Initialize()
        {
            purchaseOrderRepository = new TestPurchaseOrderRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            purchaseOrderRepository = null;
        }
        #endregion

        #region Constructor Tests
        [TestMethod]
        public async Task PurchaseOrder_ConstructorInitialization()
        {
            var repoPurchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            var testPurchaseOrder = new PurchaseOrder(repoPurchaseOrder.Id, repoPurchaseOrder.Number, repoPurchaseOrder.VendorName,
                repoPurchaseOrder.Status, repoPurchaseOrder.StatusDate, repoPurchaseOrder.Date);

            Assert.AreEqual(repoPurchaseOrder.Number, testPurchaseOrder.Number, "Number should be initialized.");
            Assert.AreEqual(repoPurchaseOrder.Status, testPurchaseOrder.Status, "Status should be initialized.");
            Assert.AreEqual(repoPurchaseOrder.StatusDate, testPurchaseOrder.StatusDate, "Status date should be initialize.");
            Assert.IsTrue(testPurchaseOrder.Requisitions is ReadOnlyCollection<string>, "Requisitions should be the correct type.");
            Assert.IsTrue(testPurchaseOrder.Requisitions.Count() == 0, "Requisitions list should have 0 items.");
            Assert.IsTrue(testPurchaseOrder.Vouchers is ReadOnlyCollection<string>, "Vouchers should be the correct type.");
            Assert.IsTrue(testPurchaseOrder.Vouchers.Count() == 0, "Vouchers should have 0 items.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PurchaseOrder_NullNumber()
        {
            var purchaseOrder = new PurchaseOrder("1", null, "Consulting Unlimited, Inc.", PurchaseOrderStatus.Outstanding, new DateTime(2015, 2, 10), new DateTime(2015, 2, 10));
        }
        #endregion

        #region AddRequisitions Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddRequisition_NullRequisitionId()
        {
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            purchaseOrder.AddRequisition(null);
        }

        [TestMethod]
        public async Task AddRequisition_DuplicateRequisition()
        {
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync("3", personId, GlAccessLevel.Full_Access, null);
            var requisitionCount = purchaseOrder.Requisitions.Count();
            var requisitionId = purchaseOrder.Requisitions.FirstOrDefault();

            // Requisition ID 31 (number 0000331) is already associated to this purchase order
            // (see TestPurchaseOrderRepository)
            purchaseOrder.AddRequisition(requisitionId);

            Assert.IsTrue(purchaseOrder.Requisitions.Count() == requisitionCount, "The size of the requisitions list should be the same as its original size.");
        }

        [TestMethod]
        public async Task AddRequisition_Success()
        {
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            var requisitionCount = purchaseOrder.Requisitions.Count();
            var requisitionId = "11";
            string requisitionNumber = "0001101";
            string vendorName = "Ellucian Consulting Associates";
            RequisitionStatus status = new RequisitionStatus();
            status = RequisitionStatus.Outstanding;
            var statusDate = Convert.ToDateTime("03/10/2015");
            DateTime requisitionDate = Convert.ToDateTime("03/10/2015");

            var newRequisition = new Requisition(requisitionId, requisitionNumber, vendorName, status, statusDate, requisitionDate);
            purchaseOrder.AddRequisition(requisitionId);

            Assert.IsTrue(purchaseOrder.Requisitions.Count() == requisitionCount + 1, "The requisition count should reflect the new requisition added.");
        }
        #endregion

        #region AddVouchers Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddVoucher_NullVoucher()
        {
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            purchaseOrder.AddVoucher(null);
        }

        [TestMethod]
        public async Task AddVoucher_DuplicateVoucher()
        {
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync("2", personId, GlAccessLevel.Full_Access, null);
            var voucherCount = purchaseOrder.Vouchers.Count();
            var voucherNumber = purchaseOrder.Vouchers.FirstOrDefault();

            // Voucher V0009992 is already associated to this purchase order
            // (see TestPurchaseOrderRepository)
            purchaseOrder.AddVoucher(voucherNumber);

            Assert.IsTrue(purchaseOrder.Vouchers.Count() == voucherCount, "The size of the vouchers list should be the same as it's original size.");
        }

        [TestMethod]
        public async Task AddVoucher_Success()
        {
            var purchaseOrder = await purchaseOrderRepository.GetPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            var voucherCount = purchaseOrder.Vouchers.Count();
            string voucherNumber = "V0009911";
            VoucherStatus status = new VoucherStatus();
            status = VoucherStatus.Outstanding;
            DateTime voucherDate = Convert.ToDateTime("03/10/2015");
            DateTime voucherInvoiceDate = Convert.ToDateTime("04/10/2015");

            var newVoucher = new Voucher(voucherNumber, voucherDate, status, "Vendor Name");
                newVoucher.InvoiceNumber = "I0001";
            newVoucher.InvoiceDate = voucherInvoiceDate;
            purchaseOrder.AddVoucher(voucherNumber);

            Assert.IsTrue(purchaseOrder.Vouchers.Count() == voucherCount + 1, "The voucher count should reflect the new voucher added.");
        }
        #endregion
    }
}
