// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;
namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class LeaveRequestTests
    {
        public string id;
        public string perLeaveId;
        public string employeeId;
        public DateTime? startDate;
        public DateTime? endDate;
        public string approverId;
        public string approverName;
        public string employeeName;
        public LeaveStatusAction status;
        public List<LeaveRequestDetail> leaveRequestDetails;
        public List<LeaveRequestComment> leaveRequestComments;
        public bool enableDeleteForSupervisor;
        public bool isWithdrawn;
        public string withdrawOption;
        public bool isWithdrawPendingApproval;
        public LeaveRequest leaveRequest;
        [TestClass]
        public class LeaveRequestConstructorTests : LeaveRequestTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "13";
                perLeaveId = "697";
                employeeId = "0011560";
                startDate = DateTime.Today;
                endDate = DateTime.Today.AddDays(1);
                approverId = "0010351";
                approverName = "Hadrian O. Racz";
                employeeName = "Hadria A. Racz";
                status = LeaveStatusAction.Draft;
                leaveRequestDetails = new List<LeaveRequestDetail>()
                {
                    new LeaveRequestDetail("38","13",DateTime.Today,8.00m, false,"0011560")
                };
                isWithdrawn = true;
                withdrawOption = "A";
                enableDeleteForSupervisor = false;
                isWithdrawPendingApproval = false;
            }
            public LeaveRequest CreateLeaveRequest()
            {
                return new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval, isWithdrawn, withdrawOption, enableDeleteForSupervisor);
            }
            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                leaveRequest = CreateLeaveRequest();
                Assert.AreEqual(id, leaveRequest.Id);
                Assert.AreEqual(perLeaveId, leaveRequest.PerLeaveId);
                Assert.AreEqual(employeeId, leaveRequest.EmployeeId);
                Assert.AreEqual(startDate, leaveRequest.StartDate);
                Assert.AreEqual(endDate, leaveRequest.EndDate);
                Assert.AreEqual(approverId, leaveRequest.ApproverId);
                Assert.AreEqual(approverName, leaveRequest.ApproverName);
                Assert.AreEqual(status, leaveRequest.Status);
                CollectionAssert.AreEqual(leaveRequestDetails, leaveRequest.LeaveRequestDetails);
                Assert.AreEqual(isWithdrawn, leaveRequest.IsWithdrawn);
                Assert.AreEqual(withdrawOption, leaveRequest.WithdrawOption);
                Assert.AreEqual(enableDeleteForSupervisor, leaveRequest.EnableDeleteForSupervisor);
                Assert.AreEqual(isWithdrawPendingApproval, leaveRequest.IsWithdrawPendingApproval);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PerLeaveIdRequiredTest()
            {
                perLeaveId = "";
                CreateLeaveRequest();
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmployeeIdRequiredTest()
            {
                employeeId = "";
                CreateLeaveRequest();
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StartDateRequiredTest()
            {
                startDate = null;
                CreateLeaveRequest();
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EndDateRequiredTest()
            {
                endDate = null;
                CreateLeaveRequest();
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ApproverIdRequiredTest()
            {
                approverId = "";
                CreateLeaveRequest();
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeaveRequestDetailsNullTest()
            {
                leaveRequestDetails = null;
                CreateLeaveRequest();
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void LeaveRequestDetailsAnyTest()
            {
                leaveRequestDetails = new List<LeaveRequestDetail>();
                CreateLeaveRequest();
            }
        }
        [TestClass]
        public class LeaveRequestAttributeTests : LeaveRequestTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "13";
                perLeaveId = "697";
                employeeId = "0011560";
                startDate = DateTime.Today;
                endDate = DateTime.Today.AddDays(1);
                approverId = "0010351";
                approverName = "Hadrian O. Racz";
                status = LeaveStatusAction.Draft;
                leaveRequestDetails = new List<LeaveRequestDetail>()
                {
                    new LeaveRequestDetail("38","13",DateTime.Today,8.00m, false,"0011560")
                };
                leaveRequestComments = new List<LeaveRequestComment>()
                {
                    new LeaveRequestComment("1","13","0011560","Test Comments","Jen Brown")
                };
                isWithdrawn = false;
                withdrawOption = null;
                enableDeleteForSupervisor = false;
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval, isWithdrawn, withdrawOption, enableDeleteForSupervisor);
            }
            [TestMethod]
            public void IdTest()
            {
                var id = "13";
                Assert.AreEqual(id, leaveRequest.Id);
            }
            [TestMethod]
            public void IdCanBeNullTest()
            {
                id = null;
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval);
                Assert.AreEqual(id, leaveRequest.Id);
            }
            [TestMethod]
            public void IdCanBeEmptyTest()
            {
                id = "";
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval);
                Assert.AreEqual(id, leaveRequest.Id);
            }
            [TestMethod]
            public void StatusTest()
            {
                var status = LeaveStatusAction.Draft;
                Assert.AreEqual(status, leaveRequest.Status);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void StartDateCannotBeAfterEndDateTest()
            {
                endDate = DateTime.Today;
                startDate = endDate.Value.AddDays(1);
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval);
            }
            [TestMethod]
            public void StartDateCanEqualEndDateTest()
            {
                startDate = DateTime.Today;
                endDate = startDate;
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval);
                Assert.AreEqual(leaveRequest.StartDate, leaveRequest.EndDate);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EndDateCannotBeBeforeStartDateTest()
            {
                endDate = startDate.Value.AddDays(-1);
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval);
            }
            [TestMethod]
            public void IsWithdrawnTest()
            {
                isWithdrawn = true;
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval, isWithdrawn, withdrawOption, enableDeleteForSupervisor);
                Assert.AreEqual(isWithdrawn, leaveRequest.IsWithdrawn);
            }
            [TestMethod]
            public void WithdrawOptionTest()
            {
                withdrawOption = "W";
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval, isWithdrawn, withdrawOption, enableDeleteForSupervisor);
                Assert.AreEqual(withdrawOption, leaveRequest.WithdrawOption);
            }
            [TestMethod]
            public void WithdrawOptionCanBeNullTest()
            {
                withdrawOption = null;
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval, isWithdrawn, withdrawOption, enableDeleteForSupervisor);
                Assert.AreEqual(withdrawOption, leaveRequest.WithdrawOption);
            }
            [TestMethod]
            public void EnableDeleteForSupervisorTest()
            {
                enableDeleteForSupervisor = true;
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval, false, null, enableDeleteForSupervisor);
                Assert.AreEqual(enableDeleteForSupervisor, leaveRequest.EnableDeleteForSupervisor);
            }
            [TestMethod]
            public void IsWithdrawPendingApprovalTest()
            {
                isWithdrawPendingApproval = true;
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, employeeName, status, leaveRequestDetails, leaveRequestComments, isWithdrawPendingApproval, false, null, enableDeleteForSupervisor);
                Assert.AreEqual(isWithdrawPendingApproval, leaveRequest.IsWithdrawPendingApproval);
            }
        }
    }
}