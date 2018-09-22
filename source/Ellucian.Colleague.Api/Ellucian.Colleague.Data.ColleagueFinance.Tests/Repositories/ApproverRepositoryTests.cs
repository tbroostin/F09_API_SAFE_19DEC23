﻿// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class ApproverRepositoryTests : BaseRepositorySetup
    {
        #region Initialize and Cleanup

        private ApproverRepository actualRepository;
        private TestApproverRepository testApproverRepository;
        private Approvals approvalsRecord;
        private ApproverValidationResponse approverValidationResponseEntity;
        private string nextApproverId;
        private string[] staffIds = new string[] { "BOB1" };
        private Opers opersRecord;

        [TestInitialize]
        public void Initialize()
        {
            this.MockInitialize();

            actualRepository = new ApproverRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            testApproverRepository = new TestApproverRepository();
            approvalsRecord = new Approvals();
            opersRecord = new Opers()
            {
                Recordkey = "BOB1",
                SysUserName = "First Bob"
            };
            InitializeMockStatements();
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualRepository = null;
            testApproverRepository = null;
            approvalsRecord = null;
            approverValidationResponseEntity = null;
            opersRecord = null;
        }
        #endregion


        #region Validate next approver

        [TestMethod]
        public async Task ValidateNextApproverAsync_Success_BeAmountAndclasses()
        {
            nextApproverId = "BOB1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, approvalsRecord.Recordkey);
            Assert.AreEqual(approverValidationResponseEntity.ApproverName, opersRecord.SysUserName);
            Assert.IsTrue(approverValidationResponseEntity.IsValid);
            Assert.IsNull(approverValidationResponseEntity.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_BeAmountOnly()
        {
            nextApproverId = "BOB1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approvalsRecord.ApprvClasses = null;
            approvalsRecord.ApprvClassesBeginDate = null;
            approvalsRecord.ApprvClassesBeginDate = null;
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, approvalsRecord.Recordkey);
            Assert.AreEqual(approverValidationResponseEntity.ApproverName, opersRecord.SysUserName);
            Assert.IsTrue(approverValidationResponseEntity.IsValid);
            Assert.IsNull(approverValidationResponseEntity.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_ClassesOnly()
        {
            nextApproverId = "BOB1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approvalsRecord.ApprvBeMaxAmt = null;
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, approvalsRecord.Recordkey);
            Assert.AreEqual(approverValidationResponseEntity.ApproverName, opersRecord.SysUserName);
            Assert.IsTrue(approverValidationResponseEntity.IsValid);
            Assert.IsNull(approverValidationResponseEntity.ErrorMessage);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateNextApproverAsync_NullId()
        {
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateNextApproverAsync_EmptyId()
        {
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ValidateNextApproverAsync_BlankId()
        {
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync("");
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_LowerCaseId()
        {
            nextApproverId = "bob1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId.ToUpperInvariant());
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, approvalsRecord.Recordkey.ToUpperInvariant());
            Assert.AreEqual(approverValidationResponseEntity.ApproverName, opersRecord.SysUserName);
            Assert.IsTrue(approverValidationResponseEntity.IsValid);
            Assert.IsNull(approverValidationResponseEntity.ErrorMessage);
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_NullApprovalsContract()
        {
            nextApproverId = "BOB1";
            approvalsRecord = null;
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, "BOB1");
            Assert.IsNull(approverValidationResponseEntity.ApproverName);
            Assert.IsFalse(approverValidationResponseEntity.IsValid);
            Assert.AreEqual(approverValidationResponseEntity.ErrorMessage, "Invalid ID - the approver ID does not have an approvals record.");
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_NoStaffIdSelected()
        {
            nextApproverId = "BOB2";
            staffIds = null;
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, "BOB2");
            Assert.IsNull(approverValidationResponseEntity.ApproverName);
            Assert.IsFalse(approverValidationResponseEntity.IsValid);
            Assert.AreEqual(approverValidationResponseEntity.ErrorMessage, "Invalid ID - No Staff record was found for the approver ID.");
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_NoClassesAndNoBeAmount()
        {
            nextApproverId = "BOB1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approvalsRecord.ApprvBeMaxAmt = null;
            approvalsRecord.ApprvClasses = null;
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, "BOB1");
            Assert.IsNull(approverValidationResponseEntity.ApproverName);
            Assert.IsFalse(approverValidationResponseEntity.IsValid);
            Assert.AreEqual(approverValidationResponseEntity.ErrorMessage, "Invalid ID - the approver ID is not setup to approve budget adjustments.");
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_BeginDateInFuture()
        {
            nextApproverId = "BOB1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approvalsRecord.ApprvBeginDate = DateTime.Today.AddDays(10);
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, "BOB1");
            Assert.IsNull(approverValidationResponseEntity.ApproverName);
            Assert.IsFalse(approverValidationResponseEntity.IsValid);
            Assert.AreEqual(approverValidationResponseEntity.ErrorMessage, "Invalid ID - the approver ID is not setup to approve budget adjustments.");
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_EndDateInPast()
        {
            nextApproverId = "BOB1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approvalsRecord.ApprvEndDate = DateTime.Today.AddDays(-10);
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, "BOB1");
            Assert.IsNull(approverValidationResponseEntity.ApproverName);
            Assert.IsFalse(approverValidationResponseEntity.IsValid);
            Assert.AreEqual(approverValidationResponseEntity.ErrorMessage, "Invalid ID - the approver ID is not setup to approve budget adjustments.");
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_OnePastClass()
        {
            nextApproverId = "BOB1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approvalsRecord.ApprvBeMaxAmt = null;
            approvalsRecord.ApprvClasses = new List<string> { "FUTURE", };
            approvalsRecord.ApprvClassesBeginDate = new List<DateTime?> { DateTime.Now.AddDays(-100) };
            approvalsRecord.ApprvClassesEndDate = new List<DateTime?> { DateTime.Now.AddDays(-10) };
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, approvalsRecord.Recordkey);
            Assert.IsNull(approverValidationResponseEntity.ApproverName);
            Assert.IsFalse(approverValidationResponseEntity.IsValid);
            Assert.AreEqual(approverValidationResponseEntity.ErrorMessage, "Invalid ID - the approver ID is restricted by the begin and end approval dates.");
        }

        [TestMethod]
        public async Task ValidateNextApproverAsync_OneFutureClass()
        {
            nextApproverId = "BOB1";
            approvalsRecord = testApproverRepository.approvalRecords.FirstOrDefault(x => x.Recordkey == nextApproverId);
            approvalsRecord.ApprvBeMaxAmt = null;
            approvalsRecord.ApprvClasses = new List<string> { "FUTURE", };
            approvalsRecord.ApprvClassesBeginDate = new List<DateTime?> { DateTime.Now.AddDays(10) };
            approvalsRecord.ApprvClassesEndDate = new List<DateTime?> { DateTime.Now.AddDays(100) };
            approverValidationResponseEntity = await actualRepository.ValidateApproverAsync(nextApproverId);

            Assert.AreEqual(approverValidationResponseEntity.Id, approvalsRecord.Recordkey);
            Assert.IsNull(approverValidationResponseEntity.ApproverName);
            Assert.IsFalse(approverValidationResponseEntity.IsValid);
            Assert.AreEqual(approverValidationResponseEntity.ErrorMessage, "Invalid ID - the approver ID is restricted by the begin and end approval dates.");
        }
        #endregion

        #region Private methods

        private void InitializeMockStatements()
        {
            // Mock the read records.
            dataReaderMock.Setup(dc => dc.ReadRecordAsync<Approvals>(It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(approvalsRecord);
            });

            dataReaderMock.Setup(dc => dc.ReadRecordAsync<Opers>("UT.OPERS", It.IsAny<string>(), true)).Returns(() =>
            {
                return Task.FromResult(opersRecord);
            });

            // Mock the selection of staff record.
            dataReaderMock.Setup(x => x.SelectAsync("STAFF", It.IsAny<string>())).Returns(() =>
            {
                return Task.FromResult(staffIds);
            });
        }

        #endregion
    }
}