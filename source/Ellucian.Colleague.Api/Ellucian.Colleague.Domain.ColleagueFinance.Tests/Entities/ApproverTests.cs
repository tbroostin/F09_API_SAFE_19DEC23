// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

using System;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    /// <summary>
    /// This class tests the valid and invalid conditions of an Approver domain entity. The
    /// Approver domain entity requires an ID.
    /// </summary>
    [TestClass]
    public class ApproverTests
    {
        #region Initialize and Cleanup
        [TestInitialize]
        public void Initialize()
        {

        }

        [TestCleanup]
        public void Cleanup()
        {

        }
        #endregion

        #region Approver

        [TestMethod]
        public void Approver_Base()
        {
            string approverId = "GTT";
            var approver = new Approver(approverId);

            Assert.AreEqual(approverId, approver.ApproverId);
            Assert.IsNull(approver.ApprovalDate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Approver_NullApproverId()
        {
            var approver = new Approver(null);
        }

        #region SetApproverName method tests

        [TestMethod]
        public void SetApproverName_NullName()
        {
            string approverId = "TGL";
            var approver = new Approver(approverId);
            approver.SetApprovalName(null);
            Assert.AreEqual(approverId, approver.ApprovalName);
        }

        [TestMethod]
        public void SetApproverName_NotNullName()
        {
            string approverId = "AJK",
                approvalName = "Andy Kleehammer";
            var approver = new Approver(approverId);
            approver.SetApprovalName(approvalName);
            Assert.AreEqual(approvalName, approver.ApprovalName);
        }
        #endregion

        #endregion

        #region Approver Validation

        [TestMethod]
        public void ApproverValidationResponse_Base()
        {
            string approverId = "TGL";
            var approverValidationResponse = new ApproverValidationResponse(approverId);
            Assert.AreEqual(approverId, approverValidationResponse.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApproverValidationResponse_NullId()
        {
            var approverValidationResponse = new ApproverValidationResponse(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApproverValidationResponse_EmptyId()
        {
            var approverValidationResponse = new ApproverValidationResponse(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApproverValidationResponse_BlankId()
        {
            var approverValidationResponse = new ApproverValidationResponse("");
        }
        #endregion
    }
}
