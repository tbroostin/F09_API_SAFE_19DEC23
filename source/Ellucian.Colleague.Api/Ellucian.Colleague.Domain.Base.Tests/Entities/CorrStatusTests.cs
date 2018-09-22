//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class CorrStatusTests
    {
        [TestClass]
        public class CorrStatusConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private CorrStatus admissionApplicationSupportingItemStatuses;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                admissionApplicationSupportingItemStatuses = new CorrStatus(guid, code, desc);
            }

            [TestMethod]
            public void CorrStatus_Code()
            {
                Assert.AreEqual(code, admissionApplicationSupportingItemStatuses.Code);
            }

            [TestMethod]
            public void CorrStatus_Description()
            {
                Assert.AreEqual(desc, admissionApplicationSupportingItemStatuses.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CorrStatus_GuidNullException()
            {
                new CorrStatus(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CorrStatus_CodeNullException()
            {
                new CorrStatus(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CorrStatus_DescNullException()
            {
                new CorrStatus(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CorrStatusGuidEmptyException()
            {
                new CorrStatus(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CorrStatusCodeEmptyException()
            {
                new CorrStatus(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CorrStatusDescEmptyException()
            {
                new CorrStatus(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class CorrStatus_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private CorrStatus admissionApplicationSupportingItemStatuses1;
            private CorrStatus admissionApplicationSupportingItemStatuses2;
            private CorrStatus admissionApplicationSupportingItemStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                admissionApplicationSupportingItemStatuses1 = new CorrStatus(guid, code, desc);
                admissionApplicationSupportingItemStatuses2 = new CorrStatus(guid, code, "Second Year");
                admissionApplicationSupportingItemStatuses3 = new CorrStatus(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CorrStatusSameCodesEqual()
            {
                Assert.IsTrue(admissionApplicationSupportingItemStatuses1.Equals(admissionApplicationSupportingItemStatuses2));
            }

            [TestMethod]
            public void CorrStatusDifferentCodeNotEqual()
            {
                Assert.IsFalse(admissionApplicationSupportingItemStatuses1.Equals(admissionApplicationSupportingItemStatuses3));
            }
        }

        [TestClass]
        public class CorrStatus_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private CorrStatus admissionApplicationSupportingItemStatuses1;
            private CorrStatus admissionApplicationSupportingItemStatuses2;
            private CorrStatus admissionApplicationSupportingItemStatuses3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                admissionApplicationSupportingItemStatuses1 = new CorrStatus(guid, code, desc);
                admissionApplicationSupportingItemStatuses2 = new CorrStatus(guid, code, "Second Year");
                admissionApplicationSupportingItemStatuses3 = new CorrStatus(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void CorrStatusSameCodeHashEqual()
            {
                Assert.AreEqual(admissionApplicationSupportingItemStatuses1.GetHashCode(), admissionApplicationSupportingItemStatuses2.GetHashCode());
            }

            [TestMethod]
            public void CorrStatusDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(admissionApplicationSupportingItemStatuses1.GetHashCode(), admissionApplicationSupportingItemStatuses3.GetHashCode());
            }
        }
    }
}
