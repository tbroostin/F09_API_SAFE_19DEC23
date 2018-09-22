//Copyright 2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
     [TestClass]
    public class StudentAcadCredStatusesTests
    {
        [TestClass]
        public class StudentAcadCredStatusesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTransferStatus courseTransferStatuses;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                courseTransferStatuses = new CourseTransferStatus(guid, code, desc);
            }

            [TestMethod]
            public void StudentAcadCredStatuses_Code()
            {
                Assert.AreEqual(code, courseTransferStatuses.Code);
            }

            [TestMethod]
            public void StudentAcadCredStatuses_Description()
            {
                Assert.AreEqual(desc, courseTransferStatuses.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAcadCredStatuses_GuidNullException()
            {
                new CourseTransferStatus(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAcadCredStatuses_CodeNullException()
            {
                new CourseTransferStatus(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAcadCredStatuses_DescNullException()
            {
                new CourseTransferStatus(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAcadCredStatusesGuidEmptyException()
            {
                new CourseTransferStatus(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAcadCredStatusesCodeEmptyException()
            {
                new CourseTransferStatus(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentAcadCredStatusesDescEmptyException()
            {
                new CourseTransferStatus(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class StudentAcadCredStatuses_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTransferStatus courseTransferStatuses1;
            private CourseTransferStatus courseTransferStatuses2;
            private CourseTransferStatus courseTransferStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                courseTransferStatuses1 = new CourseTransferStatus(guid, code, desc);
                courseTransferStatuses2 = new CourseTransferStatus(guid, code, "Second Year");
                courseTransferStatuses3 = new CourseTransferStatus(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void StudentAcadCredStatusesSameCodesEqual()
            {
                Assert.IsTrue(courseTransferStatuses1.Equals(courseTransferStatuses2));
            }

            [TestMethod]
            public void StudentAcadCredStatusesDifferentCodeNotEqual()
            {
                Assert.IsFalse(courseTransferStatuses1.Equals(courseTransferStatuses3));
            }
        }

        [TestClass]
        public class StudentAcadCredStatuses_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CourseTransferStatus courseTransferStatuses1;
            private CourseTransferStatus courseTransferStatuses2;
            private CourseTransferStatus courseTransferStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                courseTransferStatuses1 = new CourseTransferStatus(guid, code, desc);
                courseTransferStatuses2 = new CourseTransferStatus(guid, code, "Second Year");
                courseTransferStatuses3 = new CourseTransferStatus(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void StudentAcadCredStatusesSameCodeHashEqual()
            {
                Assert.AreEqual(courseTransferStatuses1.GetHashCode(), courseTransferStatuses2.GetHashCode());
            }

            [TestMethod]
            public void StudentAcadCredStatusesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(courseTransferStatuses1.GetHashCode(), courseTransferStatuses3.GetHashCode());
            }
        }
    }
}
