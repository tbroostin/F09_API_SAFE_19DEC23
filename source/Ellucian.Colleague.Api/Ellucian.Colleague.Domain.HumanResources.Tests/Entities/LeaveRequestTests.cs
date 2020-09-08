// Copyright 2019 Ellucian Company L.P. and its affiliates.
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
        public LeaveStatusAction status;
        public List<LeaveRequestDetail> leaveRequestDetails;
        public List<LeaveRequestComment> leaveRequestComments;

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
                status = LeaveStatusAction.Draft;
                leaveRequestDetails = new List<LeaveRequestDetail>()
                {
                    new LeaveRequestDetail("38","13",DateTime.Today,8.00m)                    
                };             
            }

            public LeaveRequest CreateLeaveRequest()
            {
                return new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, status, leaveRequestDetails, leaveRequestComments);
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
                    new LeaveRequestDetail("38","13",DateTime.Today,8.00m)
                };

                leaveRequestComments = new List<LeaveRequestComment>()
                {
                    new LeaveRequestComment("1","13","0011560","Test Comments","Jen Brown")
                };   
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate,approverId, approverName, status, leaveRequestDetails, leaveRequestComments);
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
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, status, leaveRequestDetails, leaveRequestComments);
                Assert.AreEqual(id, leaveRequest.Id);
            }

            [TestMethod]
            public void IdCanBeEmptyTest()
            {
                id = "";
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, status, leaveRequestDetails, leaveRequestComments);
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
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, status, leaveRequestDetails, leaveRequestComments);
            }

            [TestMethod]
            public void StartDateCanEqualEndDateTest()
            {
                startDate = DateTime.Today;
                endDate = startDate;
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, status, leaveRequestDetails, leaveRequestComments);
                Assert.AreEqual(leaveRequest.StartDate, leaveRequest.EndDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void EndDateCannotBeBeforeStartDateTest()
            {
                endDate = startDate.Value.AddDays(-1);
                leaveRequest = new LeaveRequest(id, perLeaveId, employeeId, startDate, endDate, approverId, approverName, status, leaveRequestDetails, leaveRequestComments);
            }
        }
    }
        
}
