using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayrollRegisterLeaveEntryTests
    {
        public string expectedLeaveCode = "LIFE";
        public string expectedLeaveType = "D";
        public decimal? expectedLeaveTaken = 5m;
        public decimal? expectedLeaveRemaining = 10m;

        [TestMethod]
        public void ConstructorPropertiesAreSetTest()
        {
            var actual = new PayrollRegisterLeaveEntry(expectedLeaveCode, expectedLeaveType, expectedLeaveTaken, expectedLeaveRemaining);
            Assert.AreEqual(expectedLeaveCode, actual.LeaveCode);
            Assert.AreEqual(expectedLeaveType, actual.LeaveType);
            Assert.AreEqual(expectedLeaveTaken, actual.LeaveTaken);
            Assert.AreEqual(expectedLeaveRemaining, actual.LeaveRemaining);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullLeaveCodeThrowsException()
        {
            new PayrollRegisterLeaveEntry(null, expectedLeaveType, expectedLeaveTaken, expectedLeaveRemaining);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void NullLeaveTypeThrowsException()
        {
            new PayrollRegisterLeaveEntry(expectedLeaveCode, null, expectedLeaveTaken, expectedLeaveRemaining);
        }

        [TestMethod]
        public void LeaveTakenReturnsZeroWhenNullTest()
        {
            var actual = new PayrollRegisterLeaveEntry(expectedLeaveCode, expectedLeaveType, null, expectedLeaveRemaining);
            Assert.AreEqual(0, actual.LeaveTaken);
            Assert.AreEqual(expectedLeaveRemaining, actual.LeaveRemaining);
        }

        [TestMethod]
        public void LeaveRemainingReturnsZeroWhenNullTest()
        {
            var actual = new PayrollRegisterLeaveEntry(expectedLeaveCode, expectedLeaveType, expectedLeaveTaken, null);
            Assert.AreEqual(0, actual.LeaveRemaining);
        }
    }
}
