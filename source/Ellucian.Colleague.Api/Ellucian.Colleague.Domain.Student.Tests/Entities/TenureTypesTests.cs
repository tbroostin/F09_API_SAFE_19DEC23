//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class TenureTypesTests
    {
        [TestClass]
        public class TenureTypesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private TenureTypes instructorTenureTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                instructorTenureTypes = new TenureTypes(guid, code, desc);
            }

            [TestMethod]
            public void TenureTypes_Code()
            {
                Assert.AreEqual(code, instructorTenureTypes.Code);
            }

            [TestMethod]
            public void TenureTypes_Description()
            {
                Assert.AreEqual(desc, instructorTenureTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TenureTypes_GuidNullException()
            {
                new TenureTypes(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TenureTypes_CodeNullException()
            {
                new TenureTypes(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TenureTypes_DescNullException()
            {
                new TenureTypes(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TenureTypesGuidEmptyException()
            {
                new TenureTypes(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TenureTypesCodeEmptyException()
            {
                new TenureTypes(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TenureTypesDescEmptyException()
            {
                new TenureTypes(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class TenureTypes_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private TenureTypes instructorTenureTypes1;
            private TenureTypes instructorTenureTypes2;
            private TenureTypes instructorTenureTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                instructorTenureTypes1 = new TenureTypes(guid, code, desc);
                instructorTenureTypes2 = new TenureTypes(guid, code, "Second Year");
                instructorTenureTypes3 = new TenureTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void TenureTypesSameCodesEqual()
            {
                Assert.IsTrue(instructorTenureTypes1.Equals(instructorTenureTypes2));
            }

            [TestMethod]
            public void TenureTypesDifferentCodeNotEqual()
            {
                Assert.IsFalse(instructorTenureTypes1.Equals(instructorTenureTypes3));
            }
        }

        [TestClass]
        public class TenureTypes_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private TenureTypes instructorTenureTypes1;
            private TenureTypes instructorTenureTypes2;
            private TenureTypes instructorTenureTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                instructorTenureTypes1 = new TenureTypes(guid, code, desc);
                instructorTenureTypes2 = new TenureTypes(guid, code, "Second Year");
                instructorTenureTypes3 = new TenureTypes(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void TenureTypesSameCodeHashEqual()
            {
                Assert.AreEqual(instructorTenureTypes1.GetHashCode(), instructorTenureTypes2.GetHashCode());
            }

            [TestMethod]
            public void TenureTypesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(instructorTenureTypes1.GetHashCode(), instructorTenureTypes3.GetHashCode());
            }
        }
    }
}
