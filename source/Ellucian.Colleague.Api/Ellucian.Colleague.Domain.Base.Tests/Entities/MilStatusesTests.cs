//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class MilStatusesTests
    {
        [TestClass]
        public class MilStatusesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private MilStatuses veteranStatuses;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "ARMY";
                desc = "Army";
                veteranStatuses = new MilStatuses(guid, code, desc);
            }

            [TestMethod]
            public void MilStatuses_Code()
            {
                Assert.AreEqual(code, veteranStatuses.Code);
            }

            [TestMethod]
            public void MilStatuses_Description()
            {
                Assert.AreEqual(desc, veteranStatuses.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MilStatuses_GuidNullException()
            {
                new MilStatuses(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MilStatuses_CodeNullException()
            {
                new MilStatuses(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MilStatuses_DescNullException()
            {
                new MilStatuses(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MilStatusesGuidEmptyException()
            {
                new MilStatuses(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MilStatusesCodeEmptyException()
            {
                new MilStatuses(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void MilStatusesDescEmptyException()
            {
                new MilStatuses(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class MilStatuses_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private MilStatuses veteranStatuses1;
            private MilStatuses veteranStatuses2;
            private MilStatuses veteranStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                veteranStatuses1 = new MilStatuses(guid, code, desc);
                veteranStatuses2 = new MilStatuses(guid, code, "Second Year");
                veteranStatuses3 = new MilStatuses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void MilStatusesSameCodesEqual()
            {
                Assert.IsTrue(veteranStatuses1.Equals(veteranStatuses2));
            }

            [TestMethod]
            public void MilStatusesDifferentCodeNotEqual()
            {
                Assert.IsFalse(veteranStatuses1.Equals(veteranStatuses3));
            }
        }

        [TestClass]
        public class MilStatuses_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private MilStatuses veteranStatuses1;
            private MilStatuses veteranStatuses2;
            private MilStatuses veteranStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                veteranStatuses1 = new MilStatuses(guid, code, desc);
                veteranStatuses2 = new MilStatuses(guid, code, "Second Year");
                veteranStatuses3 = new MilStatuses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void MilStatusesSameCodeHashEqual()
            {
                Assert.AreEqual(veteranStatuses1.GetHashCode(), veteranStatuses2.GetHashCode());
            }

            [TestMethod]
            public void MilStatusesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(veteranStatuses1.GetHashCode(), veteranStatuses3.GetHashCode());
            }
        }
    }
}
