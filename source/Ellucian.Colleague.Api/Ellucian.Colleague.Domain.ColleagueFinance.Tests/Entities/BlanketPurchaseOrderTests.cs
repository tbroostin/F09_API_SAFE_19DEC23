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
    /// Test valid and invalid conditions for a blanket purchase order repository.
    /// </summary>
    [TestClass]
    public class BlanketPurchaseOrderTests
    {
        #region Initialize and Cleanup

        private TestBlanketPurchaseOrderRepository blanketPurchaseOrderRepository;
        private string personId = "1";

        [TestInitialize]
        public void Initialize()
        {
            blanketPurchaseOrderRepository = new TestBlanketPurchaseOrderRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            blanketPurchaseOrderRepository = null;
        }
        #endregion

        #region Constructor Tests
        [TestMethod]
        public async Task BlanketPurchaseOrder_ConstructorInitialization()
        {
            var repoBlanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            var testBlanketPurchaseOrder = new BlanketPurchaseOrder(repoBlanketPurchaseOrder.Id, repoBlanketPurchaseOrder.Guid, repoBlanketPurchaseOrder.Number, repoBlanketPurchaseOrder.VendorName,
                repoBlanketPurchaseOrder.Status, repoBlanketPurchaseOrder.StatusDate, repoBlanketPurchaseOrder.Date);

            Assert.AreEqual(repoBlanketPurchaseOrder.Number, testBlanketPurchaseOrder.Number, "Number should be initialized.");
            Assert.AreEqual(repoBlanketPurchaseOrder.Status, testBlanketPurchaseOrder.Status, "Status should be initialized.");
            Assert.AreEqual(repoBlanketPurchaseOrder.StatusDate, testBlanketPurchaseOrder.StatusDate, "Status date should be initialize.");
            Assert.IsTrue(testBlanketPurchaseOrder.Requisitions is ReadOnlyCollection<string>, "Requisitions should be the correct type.");
            Assert.IsTrue(testBlanketPurchaseOrder.Requisitions.Count() == 0, "Requisitions list should have 0 items.");
            Assert.IsTrue(testBlanketPurchaseOrder.Vouchers is ReadOnlyCollection<string>, "Vouchers should be the correct type.");
            Assert.IsTrue(testBlanketPurchaseOrder.Vouchers.Count() == 0, "Vouchers should have 0 items.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BlanketPurchaseOrder_NullNumber()
        {
            var blanketPurchaseOrder = new BlanketPurchaseOrder("1", null, "Consulting Unlimited, Inc.", BlanketPurchaseOrderStatus.Outstanding, new DateTime(2015, 2, 10), new DateTime(2015, 2, 10));
        }
        #endregion

        #region AddRequisitions Tests
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddRequisition_NullRequisitionId()
        {
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            blanketPurchaseOrder.AddRequisition(null);
        }

        [TestMethod]
        public async Task AddRequisition_DuplicateRequisition()
        {
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            var requisitionCount = blanketPurchaseOrder.Requisitions.Count();
            var requisitionId = blanketPurchaseOrder.Requisitions.FirstOrDefault();

            // Requisition ID 11 (number 0000111) is already associated to this blanket purchase order
            // (see TestBlanketPurchaseOrderRepository)
            blanketPurchaseOrder.AddRequisition(requisitionId);

            Assert.IsTrue(blanketPurchaseOrder.Requisitions.Count() == requisitionCount, "The size of the requisitions list should be the same as its original size.");
        }

        [TestMethod]
        public async Task AddRequisition_Success()
        {
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("1", personId, GlAccessLevel.Full_Access, null);
            var requisitionCount = blanketPurchaseOrder.Requisitions.Count();
            var requisitionId = "14";
            string requisitionNumber = "0000114";
            string vendorName = "Ellucian Consulting Associates";
            RequisitionStatus status = new RequisitionStatus();
            status = RequisitionStatus.Outstanding;
            var statusDate = Convert.ToDateTime("03/10/2015");
            DateTime requisitionDate = Convert.ToDateTime("03/10/2015");

            var newRequisition = new Requisition(requisitionId, requisitionNumber, vendorName, status, statusDate, requisitionDate);
            blanketPurchaseOrder.AddRequisition(requisitionId);

            Assert.IsTrue(blanketPurchaseOrder.Requisitions.Count() == requisitionCount + 1, "The requisition count should reflect the new requisition added.");
        }
        #endregion

        #region AddVouchers Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddVoucher_NullVoucher()
        {
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("4", personId, GlAccessLevel.Full_Access, null);
            blanketPurchaseOrder.AddVoucher(null);
        }

        [TestMethod]
        public async Task AddVoucher_DuplicateVoucher()
        {
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("4", personId, GlAccessLevel.Full_Access, null);
            var voucherCount = blanketPurchaseOrder.Vouchers.Count();
            var voucherNumber = blanketPurchaseOrder.Vouchers.FirstOrDefault();

            // Voucher V0009991 is already associated to this blanket purchase order
            // (see TestBlanketPurchaseOrderRepository)
            blanketPurchaseOrder.AddVoucher(voucherNumber);

            Assert.IsTrue(blanketPurchaseOrder.Vouchers.Count() == voucherCount, "The size of the vouchers list should be the same as it's original size.");
        }

        [TestMethod]
        public async Task AddVoucher_Success()
        {
            var blanketPurchaseOrder = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync("4", personId, GlAccessLevel.Full_Access, null);
            var voucherCount = blanketPurchaseOrder.Vouchers.Count();
            string voucherNumber = "V0009997";
            VoucherStatus status = new VoucherStatus();
            status = VoucherStatus.Outstanding;
            DateTime voucherDate = Convert.ToDateTime("05/25/2015");
            DateTime voucherInvoiceDate = Convert.ToDateTime("05/25/2015");

            var newVoucher = new Voucher(voucherNumber, voucherDate, status, "Vendor Name");
            newVoucher.InvoiceNumber = "I0001";
            newVoucher.InvoiceDate = voucherInvoiceDate;
            blanketPurchaseOrder.AddVoucher(voucherNumber);

            Assert.IsTrue(blanketPurchaseOrder.Vouchers.Count() == voucherCount + 1, "The voucher count should reflect the new voucher added.");
        }
        #endregion
    }
}
