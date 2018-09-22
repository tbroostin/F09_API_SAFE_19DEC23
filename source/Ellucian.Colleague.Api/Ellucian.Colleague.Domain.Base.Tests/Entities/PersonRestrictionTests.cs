using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class PersonRestrictionTests
    {
        [TestClass]
        public class StudentRestrictionConstructor
        {
            private string id;
            private string studentId;
            private string restrictionId;
            private DateTime? startDate;
            private DateTime? endDate;
            private int? severity;
            private string visibleToUsers;
            private PersonRestriction studentRestriction;

            [TestInitialize]
            public void Initialize()
            {
                id = "001";
                studentId = "S0001";
                restrictionId = "R0001";
                startDate = (DateTime?)new DateTime();
                endDate = (DateTime?)new DateTime();
                severity = (int?)3;
                visibleToUsers = "Y";
                studentRestriction = new PersonRestriction(id, studentId, restrictionId, startDate, endDate, severity, visibleToUsers);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentRestriction_NullId()
            {
                studentRestriction = new PersonRestriction(null, studentId, restrictionId, startDate, endDate, severity, visibleToUsers);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentRestriction_EmptyId()
            {
                studentRestriction = new PersonRestriction(string.Empty, studentId, restrictionId, startDate, endDate, severity, visibleToUsers);
            }

            [TestMethod]
            public void StudentRestriction_Id()
            {
                Assert.AreEqual(id, studentRestriction.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentRestriction_NullStudentId()
            {
                studentRestriction = new PersonRestriction(id, null, restrictionId, startDate, endDate, severity, visibleToUsers);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentRestriction_EmptyStudentId()
            {
                studentRestriction = new PersonRestriction(id, string.Empty, restrictionId, startDate, endDate, severity, visibleToUsers);
            }

            [TestMethod]
            public void StudentRestriction_StudentId()
            {
                Assert.AreEqual(studentId, studentRestriction.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentRestriction_NullRestrictionId()
            {
                studentRestriction = new PersonRestriction(id, studentId, null, startDate, endDate, severity, visibleToUsers);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentRestriction_EmptyRestrictionId()
            {
                studentRestriction = new PersonRestriction(id, studentId, string.Empty, startDate, endDate, severity, visibleToUsers);
            }

            [TestMethod]
            public void StudentRestriction_RestrictionId()
            {
                Assert.AreEqual(restrictionId, studentRestriction.RestrictionId);
            }

            [TestMethod]
            public void StudentRestriction_StartDate()
            {
                Assert.AreEqual(startDate, studentRestriction.StartDate);
            }

            [TestMethod]
            public void StudentRestriction_EndDate()
            {
                Assert.AreEqual(endDate, studentRestriction.EndDate);
            }

            [TestMethod]
            public void StudentRestriction_Severity()
            {
                Assert.AreEqual(severity, studentRestriction.Severity);
            }

            [TestMethod]
            public void StudentRestriction_OfficeUseOnlyTrue_VisibleToUsersNo()
            {
                studentRestriction = new PersonRestriction(id, studentId, restrictionId, startDate, endDate, severity, "N");
                Assert.IsTrue(studentRestriction.OfficeUseOnly);
            }

            [TestMethod]
            public void StudentRestriction_OfficeUseOnlyTrue_VisibleToUsersYes()
            {
                studentRestriction = new PersonRestriction(id, studentId, restrictionId, startDate, endDate, severity, "Y");
                Assert.IsFalse(studentRestriction.OfficeUseOnly);
            }

            [TestMethod]
            public void StudentRestriction_OfficeUseOnlyTrue_NullVisibleToUsers()
            {
                studentRestriction = new PersonRestriction(id, studentId, restrictionId, startDate, endDate, severity, null);
                Assert.IsTrue(studentRestriction.OfficeUseOnly);
            }

            [TestMethod]
            public void StudentRestriction_OfficeUseOnlyTrue_EmptyVisibleToUsers()
            {
                studentRestriction = new PersonRestriction(id, studentId, restrictionId, startDate, endDate, severity, string.Empty);
                Assert.IsTrue(studentRestriction.OfficeUseOnly);
            }
        }

        [TestClass]
        public class StudentRestrictionNullableConstructor
        {
            private string id;
            private string studentId;
            private string restrictionId;
            private DateTime? startDate;
            private DateTime? endDate;
            private int? severity;
            private string visibleToUsers;
            private PersonRestriction studentRestriction;

            [TestInitialize]
            public void Initialize()
            {
                id = "001";
                studentId = "S0001";
                restrictionId = "R0001";
                startDate = null;
                endDate = null;
                severity = null;
                visibleToUsers = "N";
                studentRestriction = new PersonRestriction(id, studentId, restrictionId, startDate, endDate, severity, visibleToUsers);
            }

            [TestMethod]
            public void StudentRestriction_NullStartDate()
            {
                Assert.AreEqual(startDate, studentRestriction.StartDate);
            }

            [TestMethod]
            public void StudentRestriction_NullEndDate()
            {
                Assert.AreEqual(endDate, studentRestriction.EndDate);
            }

            [TestMethod]
            public void StudentRestriction_NullSeverity()
            {
                Assert.AreEqual(severity, studentRestriction.Severity);
            }

            [TestMethod]
            public void StudentRestriction_OfficeUseOnlyTrue1()
            {
                // handle "Y", "N", "" as false, true, true
                Assert.AreEqual((visibleToUsers == "Y" ? false : (visibleToUsers == "N" ? true : true)), studentRestriction.OfficeUseOnly);
            }
        }

        [TestClass]
        public class StudentRestrictionNullable2Constructor
        {
            private string id;
            private string studentId;
            private string restrictionId;
            private DateTime? startDate;
            private DateTime? endDate;
            private int? severity;
            private string visibleToUsers;
            private PersonRestriction studentRestriction;

            [TestInitialize]
            public void Initialize()
            {
                id = "001";
                studentId = "S0001";
                restrictionId = "R0001";
                startDate = null;
                endDate = null;
                severity = null;
                visibleToUsers = "";
                studentRestriction = new PersonRestriction(id, studentId, restrictionId, startDate, endDate, severity, visibleToUsers);
            }


            [TestMethod]
            public void StudentRestriction_OfficeUseOnlyTrue2()
            {
                // handle "Y", "N", "" as false, true, true
                Assert.AreEqual((visibleToUsers == "Y" ? false : (visibleToUsers == "N" ? true : true)), studentRestriction.OfficeUseOnly);
            }
        }

    }
}
