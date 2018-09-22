
//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class AttendanceTypesTests
    {
        [TestClass]
        public class AttendanceTypesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private AttendanceTypes attendanceCategories;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                attendanceCategories = new AttendanceTypes(guid, code, desc);
            }

            [TestMethod]
            public void AttendanceTypes_Code()
            {
                Assert.AreEqual(code, attendanceCategories.Code);
            }

            [TestMethod]
            public void AttendanceTypes_Description()
            {
                Assert.AreEqual(desc, attendanceCategories.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttendanceTypes_GuidNullException()
            {
                new AttendanceTypes(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttendanceTypes_CodeNullException()
            {
                new AttendanceTypes(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttendanceTypes_DescNullException()
            {
                new AttendanceTypes(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttendanceTypesGuidEmptyException()
            {
                new AttendanceTypes(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttendanceTypesCodeEmptyException()
            {
                new AttendanceTypes(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AttendanceTypesDescEmptyException()
            {
                new AttendanceTypes(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class AttendanceTypes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private AttendanceTypes attendanceCategories1;
            private AttendanceTypes attendanceCategories2;
            private AttendanceTypes attendanceCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                attendanceCategories1 = new AttendanceTypes(guid, code, desc);
                attendanceCategories2 = new AttendanceTypes(guid, code, "Second Year");
                attendanceCategories3 = new AttendanceTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AttendanceTypesSameCodesEqual()
            {
                Assert.IsTrue(attendanceCategories1.Equals(attendanceCategories2));
            }

            [TestMethod]
            public void AttendanceTypesDifferentCodeNotEqual()
            {
                Assert.IsFalse(attendanceCategories1.Equals(attendanceCategories3));
            }
        }

        [TestClass]
        public class AttendanceTypes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private AttendanceTypes attendanceCategories1;
            private AttendanceTypes attendanceCategories2;
            private AttendanceTypes attendanceCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                attendanceCategories1 = new AttendanceTypes(guid, code, desc);
                attendanceCategories2 = new AttendanceTypes(guid, code, "Second Year");
                attendanceCategories3 = new AttendanceTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void AttendanceTypesSameCodeHashEqual()
            {
                Assert.AreEqual(attendanceCategories1.GetHashCode(), attendanceCategories2.GetHashCode());
            }

            [TestMethod]
            public void AttendanceTypesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(attendanceCategories1.GetHashCode(), attendanceCategories3.GetHashCode());
            }
        }
    }
}
