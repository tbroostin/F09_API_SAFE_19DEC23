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
    public class LeaveRequestDetailTests
    {
        public string id;
        public string leaveRequestId;
        public DateTime leaveDate;
        public decimal? leaveHours;
        public bool processedFlag;

        public LeaveRequestDetail leaveRequestDetail;

        [TestClass]
        public class LeaveRequestDetailConstructorTests : LeaveRequestDetailTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "38";
                leaveRequestId = "13";
                leaveDate = DateTime.Today;
                leaveHours = 8.00m;
                processedFlag = false;
            }

            public LeaveRequestDetail CreateLeaveRequestDetail()
            {
                return new LeaveRequestDetail(id, leaveRequestId, leaveDate, leaveHours, processedFlag);
            }

            [TestMethod]
            public void ConstructorSetsPropertiesTest()
            {
                leaveRequestDetail = CreateLeaveRequestDetail();
                Assert.AreEqual(id, leaveRequestDetail.Id);
                Assert.AreEqual(leaveRequestId, leaveRequestDetail.LeaveRequestId);
                Assert.AreEqual(leaveDate, leaveRequestDetail.LeaveDate);
                Assert.AreEqual(leaveHours, leaveRequestDetail.LeaveHours);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void LeaveDateRequiredTest()
            {
                leaveDate = DateTime.MinValue;
                CreateLeaveRequestDetail();
            }
        }

        [TestClass]
        public class LeaveRequestDetailAttributeTests : LeaveRequestDetailTests
        {
            [TestInitialize]
            public void Initialize()
            {
                id = "38";
                leaveRequestId = "13";
                leaveDate = DateTime.Today;
                leaveHours = 8.00m;

                leaveRequestDetail = new LeaveRequestDetail(id, leaveRequestId, leaveDate, leaveHours, processedFlag);
            }

            [TestMethod]
            public void IdTest()
            {
                var id = "38";
                Assert.AreEqual(id, leaveRequestDetail.Id);
            }

            [TestMethod]
            public void IdCanBeNullTest()
            {
                id = null;
                leaveRequestDetail = new LeaveRequestDetail(id, leaveRequestId, leaveDate, leaveHours, processedFlag);
                Assert.AreEqual(id, leaveRequestDetail.Id);
            }

            [TestMethod]
            public void IdCanBeEmptyTest()
            {
                id = "";
                leaveRequestDetail = new LeaveRequestDetail(id, leaveRequestId, leaveDate, leaveHours, processedFlag);
                Assert.AreEqual(id, leaveRequestDetail.Id);
            }

            [TestMethod]
            public void LeaveRequestIdTest()
            {
                var leaveRequestId = "13";
                Assert.AreEqual(leaveRequestId, leaveRequestDetail.LeaveRequestId);
            }

            [TestMethod]
            public void LeaveRequestIdCanBeNullTest()
            {
                leaveRequestId = null;
                leaveRequestDetail = new LeaveRequestDetail(id, leaveRequestId, leaveDate, leaveHours, processedFlag);
                Assert.AreEqual(leaveRequestId, leaveRequestDetail.LeaveRequestId);
            }

            [TestMethod]
            public void LeaveRequestIdCanBeEmptyTest()
            {
                leaveRequestId = "";
                leaveRequestDetail = new LeaveRequestDetail(id, leaveRequestId, leaveDate, leaveHours, processedFlag);
                Assert.AreEqual(leaveRequestId, leaveRequestDetail.LeaveRequestId);
            }

            [TestMethod]
            public void LeaveHoursTest()
            {
                var leaveHours = 8.00m;
                leaveRequestDetail = new LeaveRequestDetail(id, leaveRequestId, leaveDate, leaveHours, processedFlag);
                Assert.AreEqual(leaveHours, leaveRequestDetail.LeaveHours);
            }

            [TestMethod]
            public void LeaveHoursCanBeNullTest()
            {
                decimal? leaveHours = null;
                leaveRequestDetail = new LeaveRequestDetail(id, leaveRequestId, leaveDate, leaveHours, processedFlag);
                Assert.AreEqual(leaveHours, leaveRequestDetail.LeaveHours);
            }
        }
    }
}
