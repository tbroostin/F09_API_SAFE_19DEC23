using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class StaffAssignmentTests
    {
        private string id;
        private DateTime? startDate;
        private DateTime? endDate;
        private StaffAssignment staffAssignment;

        [TestInitialize]
        public void Initialize()
        {
            id = "staff1";
            startDate = DateTime.Today.AddDays(5);
            endDate = DateTime.Today.AddDays(6);
            staffAssignment = new StaffAssignment(id);
        }

        [TestCleanup]
        public void Cleanup()
        {
            staffAssignment = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StaffAssignment_NullStaffId_Exception()
        {
            var newStaffAssignmnet = new StaffAssignment(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StaffAssignment_EmptyStaffId_Exception()
        {
            var newStaffAssignmnet = new StaffAssignment(string.Empty);
        }

        [TestMethod]
        public void TermRegistrationDate_Constructor_WithDataSuccess()
        {
            Assert.AreEqual(id, staffAssignment.StaffId);
            Assert.IsNull(staffAssignment.StartDate);
            Assert.IsNull(staffAssignment.EndDate);
            staffAssignment.StartDate = startDate;
            staffAssignment.EndDate = endDate;
            Assert.AreEqual(startDate, staffAssignment.StartDate);
            Assert.AreEqual(endDate, staffAssignment.EndDate);
        }

    }
}
