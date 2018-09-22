//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class HrStatusesTests
    {
        [TestClass]
        public class HrStatusesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private HrStatuses contractTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                contractTypes = new HrStatuses(guid, code, desc);
            }

            [TestMethod]
            public void HrStatuses_Code()
            {
                Assert.AreEqual(code, contractTypes.Code);
            }

            [TestMethod]
            public void HrStatuses_Description()
            {
                Assert.AreEqual(desc, contractTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HrStatuses_GuidNullException()
            {
                new HrStatuses(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HrStatuses_CodeNullException()
            {
                new HrStatuses(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HrStatuses_DescNullException()
            {
                new HrStatuses(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HrStatusesGuidEmptyException()
            {
                new HrStatuses(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HrStatusesCodeEmptyException()
            {
                new HrStatuses(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void HrStatusesDescEmptyException()
            {
                new HrStatuses(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class HrStatuses_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private HrStatuses contractTypes1;
            private HrStatuses contractTypes2;
            private HrStatuses contractTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                contractTypes1 = new HrStatuses(guid, code, desc);
                contractTypes2 = new HrStatuses(guid, code, "Second Year");
                contractTypes3 = new HrStatuses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void HrStatusesSameCodesEqual()
            {
                Assert.IsTrue(contractTypes1.Equals(contractTypes2));
            }

            [TestMethod]
            public void HrStatusesDifferentCodeNotEqual()
            {
                Assert.IsFalse(contractTypes1.Equals(contractTypes3));
            }
        }

        [TestClass]
        public class HrStatuses_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private HrStatuses contractTypes1;
            private HrStatuses contractTypes2;
            private HrStatuses contractTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                contractTypes1 = new HrStatuses(guid, code, desc);
                contractTypes2 = new HrStatuses(guid, code, "Second Year");
                contractTypes3 = new HrStatuses(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void HrStatusesSameCodeHashEqual()
            {
                Assert.AreEqual(contractTypes1.GetHashCode(), contractTypes2.GetHashCode());
            }

            [TestMethod]
            public void HrStatusesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(contractTypes1.GetHashCode(), contractTypes3.GetHashCode());
            }
        }
    }
}