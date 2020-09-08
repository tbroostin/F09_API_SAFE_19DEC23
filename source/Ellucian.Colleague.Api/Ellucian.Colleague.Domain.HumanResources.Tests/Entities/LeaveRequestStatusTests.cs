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
    public class LeaveRequestStatusTests
    {
        public string id;
        public string leaveRequestId;
        public string actionerId;
        public LeaveStatusAction actionType;

        public LeaveRequestStatus leaveRequestStatus;

        [TestClass]
        public class LeaveRequestStatusConstructorTests : LeaveRequestStatusTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "2";
                leaveRequestId = "13";
                actionerId = "0011560";
                actionType = LeaveStatusAction.Submitted;
            }

            public LeaveRequestStatus CreateLeaveRequestStatus()
            {
                return new LeaveRequestStatus(id, leaveRequestId, actionType, actionerId);
            }

            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                leaveRequestStatus = CreateLeaveRequestStatus();
                Assert.AreEqual(id, leaveRequestStatus.Id);
                Assert.AreEqual(leaveRequestId, leaveRequestStatus.LeaveRequestId);
                Assert.AreEqual(actionerId, leaveRequestStatus.ActionerId);
                Assert.AreEqual(actionType, leaveRequestStatus.ActionType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ActionerIdRequiredTest()
            {
                actionerId = "";
                CreateLeaveRequestStatus();
            }
        }

        [TestClass]
        public class LeaveRequestStatusAttributesTests : LeaveRequestStatusTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "2";
                leaveRequestId = "13";
                actionerId = "0011560";
                actionType = LeaveStatusAction.Submitted;

                leaveRequestStatus = new LeaveRequestStatus(id, leaveRequestId, actionType, actionerId);
            }

            [TestMethod]
            public void IdTest()
            {
                var id = "2";
                Assert.AreEqual(id, leaveRequestStatus.Id);
            }

            [TestMethod]
            public void IdCanBeNullTest()
            {
                id = null;
                leaveRequestStatus = new LeaveRequestStatus(id, leaveRequestId, actionType, actionerId);
                Assert.AreEqual(id, leaveRequestStatus.Id);
            }

            [TestMethod]
            public void IdCanBeEmptyTest()
            {
                id = "";
                leaveRequestStatus = new LeaveRequestStatus(id, leaveRequestId, actionType, actionerId);
                Assert.AreEqual(id, leaveRequestStatus.Id);
            }

            [TestMethod]
            public void LeaveRequestIdTest()
            {
                var leaveRequestId = "13";
                Assert.AreEqual(leaveRequestId, leaveRequestStatus.LeaveRequestId);
            }

            [TestMethod]
            public void LeaveRequestIdCanBeNullTest()
            {
                leaveRequestId = null;
                leaveRequestStatus = new LeaveRequestStatus(id, leaveRequestId, actionType, actionerId);
                Assert.AreEqual(leaveRequestId, leaveRequestStatus.LeaveRequestId);
            }

            [TestMethod]
            public void LeaveRequestIdCanBeEmptyTest()
            {
                leaveRequestId = "";
                leaveRequestStatus = new LeaveRequestStatus(id, leaveRequestId, actionType, actionerId);
                Assert.AreEqual(leaveRequestId, leaveRequestStatus.LeaveRequestId);
            }

            [TestMethod]
            public void StatusTest()
            {
                var status = LeaveStatusAction.Submitted;
                Assert.AreEqual(status, leaveRequestStatus.ActionType);
            }
        }
    }
}
