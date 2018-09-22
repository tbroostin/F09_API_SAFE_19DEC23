//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class SapStatusesTests
    {
        [TestClass]
        public class SapStatusesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private SapStatuses financialAidAcademicProgressStatuses;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                financialAidAcademicProgressStatuses = new SapStatuses(guid, code, desc);
            }

            [TestMethod]
            public void SapStatuses_Code()
            {
                Assert.AreEqual(code, financialAidAcademicProgressStatuses.Code);
            }

            [TestMethod]
            public void SapStatuses_Description()
            {
                Assert.AreEqual(desc, financialAidAcademicProgressStatuses.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SapStatuses_GuidNullException()
            {
                new SapStatuses(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SapStatuses_CodeNullException()
            {
                new SapStatuses(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SapStatuses_DescNullException()
            {
                new SapStatuses(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SapStatusesGuidEmptyException()
            {
                new SapStatuses(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SapStatusesCodeEmptyException()
            {
                new SapStatuses(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SapStatusesDescEmptyException()
            {
                new SapStatuses(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class SapStatuses_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private SapStatuses financialAidAcademicProgressStatuses1;
            private SapStatuses financialAidAcademicProgressStatuses2;
            private SapStatuses financialAidAcademicProgressStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                financialAidAcademicProgressStatuses1 = new SapStatuses(guid, code, desc);
                financialAidAcademicProgressStatuses2 = new SapStatuses(guid, code, "Second Year");
                financialAidAcademicProgressStatuses3 = new SapStatuses(Guid.NewGuid().ToString(), "200", desc) { FssiCategory = "D"};
            }

            [TestMethod]
            public void SapStatusesSameCodesEqual()
            {
                Assert.IsTrue(financialAidAcademicProgressStatuses1.Equals(financialAidAcademicProgressStatuses2));
            }

            [TestMethod]
            public void SapStatusesDifferentCodeNotEqual()
            {
                Assert.IsFalse(financialAidAcademicProgressStatuses1.Equals(financialAidAcademicProgressStatuses3));
            }

            [TestMethod]
            public void SapStatusesDifferentCategoryEqualD()
            {
                Assert.AreEqual(financialAidAcademicProgressStatuses3.FssiCategory, "D");
            }
        }

        [TestClass]
        public class SapStatuses_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private SapStatuses financialAidAcademicProgressStatuses1;
            private SapStatuses financialAidAcademicProgressStatuses2;
            private SapStatuses financialAidAcademicProgressStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                financialAidAcademicProgressStatuses1 = new SapStatuses(guid, code, desc);
                financialAidAcademicProgressStatuses2 = new SapStatuses(guid, code, "Second Year");
                financialAidAcademicProgressStatuses3 = new SapStatuses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void SapStatusesSameCodeHashEqual()
            {
                Assert.AreEqual(financialAidAcademicProgressStatuses1.GetHashCode(), financialAidAcademicProgressStatuses2.GetHashCode());
            }

            [TestMethod]
            public void SapStatusesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(financialAidAcademicProgressStatuses1.GetHashCode(), financialAidAcademicProgressStatuses3.GetHashCode());
            }
        }
    }
}
