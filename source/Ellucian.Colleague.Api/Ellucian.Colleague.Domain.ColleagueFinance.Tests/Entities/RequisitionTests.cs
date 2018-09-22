// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// Test valid and invalid conditions for a requisition repository
    /// </summary>
    [TestClass]
    public class RequisitionTests
    {
        #region Initialize and Cleanup

        private TestRequisitionRepository requisitionRepository;
        private string personId = "1";

        [TestInitialize]
        public void Initialize()
        {
            requisitionRepository = new TestRequisitionRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            requisitionRepository = null;
        }
        #endregion

        #region Constructor Tests

        [TestMethod]
        public async Task Requisition_ConstructorInitialization()
        {
            var repoRequisition = await requisitionRepository.GetRequisitionAsync("1", personId, GlAccessLevel.Full_Access, null);
            var testRequisition = new Requisition(repoRequisition.Id, repoRequisition.Number, repoRequisition.VendorName, repoRequisition.Status, repoRequisition.StatusDate, repoRequisition.Date);

            Assert.AreEqual(repoRequisition.Number, testRequisition.Number, "Number should be initialized.");
            Assert.AreEqual(repoRequisition.Status, testRequisition.Status, "Status should be initialized.");
            Assert.AreEqual(repoRequisition.StatusDate, testRequisition.StatusDate, "StatusDate should be initialized.");
            Assert.IsTrue(testRequisition.PurchaseOrders is ReadOnlyCollection<string>, "PurchaseOrders should be the correct type.");
            Assert.IsTrue(testRequisition.PurchaseOrders.Count() == 0, "PurchaseOrders list should have 0 items.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Requisition_NullNumber()
        {
            var requisition = new Requisition("1", null, "Consulting Unlimited, Inc.", RequisitionStatus.Outstanding, new DateTime(2015, 2, 10), new DateTime(2015, 2, 10));
        }

        #endregion

        #region AddPurchaseOrders Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AddPurchaseOrder_NullPurchaseOrderId()
        {
            var requisition = await requisitionRepository.GetRequisitionAsync("1", personId, GlAccessLevel.Full_Access, null);
            requisition.AddPurchaseOrder(null);
        }

        [TestMethod]
        public async Task AddPurchaseOrder_DuplicatePurchaseOrder()
        {
            var requisition = await requisitionRepository.GetRequisitionAsync("3", personId, GlAccessLevel.Full_Access, null);
            var purchaseOrderCount = requisition.PurchaseOrders.Count();
            var purchaseOrderId = requisition.PurchaseOrders.FirstOrDefault();

            // PO ID 31 (P0000331) is already associated to this requisition
            // (see TestRequisitionRepository)
            requisition.AddPurchaseOrder(purchaseOrderId);

            Assert.IsTrue(requisition.PurchaseOrders.Count() == purchaseOrderCount, "The size of the purchase order list should be the same as its original size.");
        }

        [TestMethod]
        public async Task AddPurchaseOrder_Success()
        {
            var requisition = await requisitionRepository.GetRequisitionAsync("1", personId, GlAccessLevel.Full_Access, null);
            var purchaseOrderCount = requisition.PurchaseOrders.Count();
            var purchaseOrderId = "99";
            string purchaseOrderNumber = "0009901";
            string vendorName = "Ellucian Requisition Vendor Associates";
            PurchaseOrderStatus status = new PurchaseOrderStatus();
            status = PurchaseOrderStatus.Outstanding;
            var statusDate = Convert.ToDateTime("03/10/2015");
            DateTime purchaseOrderDate = Convert.ToDateTime("03/10/2015");

            var newPurchaseOrder = new PurchaseOrder(purchaseOrderId, purchaseOrderNumber, vendorName, status, statusDate, purchaseOrderDate);
            requisition.AddPurchaseOrder(purchaseOrderId);

            Assert.IsTrue(requisition.PurchaseOrders.Count() == purchaseOrderCount + 1, "The purchase order count should reflect the new purchase order added.");
        }
        #endregion

    }
}
