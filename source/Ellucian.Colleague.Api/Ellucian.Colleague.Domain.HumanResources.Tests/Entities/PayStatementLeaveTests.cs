/* Copyright 2017 Ellucian Company L.P. and affiliates */
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementLeaveTests
    {
        public string leaveTypeId = "D";
        public string expectedLeaveTypeDescription = "Description";
        public decimal? expectedLeaveTaken = 5m;
        public decimal? expectedLeaveRemaining = 10m;

        [TestMethod]
        public void ConstructorPropertiesAreSetTest()
        {
            var actual = new PayStatementLeave(leaveTypeId, expectedLeaveTypeDescription, expectedLeaveTaken, expectedLeaveRemaining);
            Assert.AreEqual(leaveTypeId, actual.LeaveTypeId);
            Assert.AreEqual(expectedLeaveTypeDescription, actual.Description);
            Assert.AreEqual(expectedLeaveTaken, actual.LeaveTaken);
            Assert.AreEqual(expectedLeaveRemaining, actual.LeaveRemaining);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullLeaveTypeIdThrowsException()
        {
            var actual = new PayStatementLeave(null, expectedLeaveTypeDescription, expectedLeaveTaken, expectedLeaveRemaining);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullLeaveDescriptionThrowsException()
        {
            var actual = new PayStatementLeave(leaveTypeId, null, expectedLeaveTaken, expectedLeaveRemaining);
        }
    }
}
