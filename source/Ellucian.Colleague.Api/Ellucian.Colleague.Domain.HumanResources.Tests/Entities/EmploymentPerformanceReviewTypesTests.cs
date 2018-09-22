//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
     [TestClass]
    public class EmploymentPerformanceReviewTypesTests
    {
        [TestClass]
         public class EmploymentPerformanceReviewTypesConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentPerformanceReviewType employmentPerformanceReviewTypes;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentPerformanceReviewTypes = new EmploymentPerformanceReviewType(guid, code, desc);
            }

            [TestMethod]
            public void EmploymentPerformanceReviewType_Code()
            {
                Assert.AreEqual(code, employmentPerformanceReviewTypes.Code);
            }

            [TestMethod]
            public void EmploymentPerformanceReviewType_Description()
            {
                Assert.AreEqual(desc, employmentPerformanceReviewTypes.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentPerformanceReviewType_GuidNullException()
            {
                new EmploymentPerformanceReviewType(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentPerformanceReviewType_CodeNullException()
            {
                new EmploymentPerformanceReviewType(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentPerformanceReviewType_DescNullException()
            {
                new EmploymentPerformanceReviewType(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentPerformanceReviewTypeGuidEmptyException()
            {
                new EmploymentPerformanceReviewType(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentPerformanceReviewTypeCodeEmptyException()
            {
                new EmploymentPerformanceReviewType(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void EmploymentPerformanceReviewTypeDescEmptyException()
            {
                new EmploymentPerformanceReviewType(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class EmploymentPerformanceReviewType_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentPerformanceReviewType employmentPerformanceReviewTypes1;
            private EmploymentPerformanceReviewType employmentPerformanceReviewTypes2;
            private EmploymentPerformanceReviewType employmentPerformanceReviewTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentPerformanceReviewTypes1 = new EmploymentPerformanceReviewType(guid, code, desc);
                employmentPerformanceReviewTypes2 = new EmploymentPerformanceReviewType(guid, code, "Second Year");
                employmentPerformanceReviewTypes3 = new EmploymentPerformanceReviewType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void EmploymentPerformanceReviewTypeSameCodesEqual()
            {
                Assert.IsTrue(employmentPerformanceReviewTypes1.Equals(employmentPerformanceReviewTypes2));
            }

            [TestMethod]
            public void EmploymentPerformanceReviewTypeDifferentCodeNotEqual()
            {
                Assert.IsFalse(employmentPerformanceReviewTypes1.Equals(employmentPerformanceReviewTypes3));
            }
        }

        [TestClass]
        public class EmploymentPerformanceReviewType_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private EmploymentPerformanceReviewType employmentPerformanceReviewTypes1;
            private EmploymentPerformanceReviewType employmentPerformanceReviewTypes2;
            private EmploymentPerformanceReviewType employmentPerformanceReviewTypes3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                employmentPerformanceReviewTypes1 = new EmploymentPerformanceReviewType(guid, code, desc);
                employmentPerformanceReviewTypes2 = new EmploymentPerformanceReviewType(guid, code, "Second Year");
                employmentPerformanceReviewTypes3 = new EmploymentPerformanceReviewType(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void EmploymentPerformanceReviewTypeSameCodeHashEqual()
            {
                Assert.AreEqual(employmentPerformanceReviewTypes1.GetHashCode(), employmentPerformanceReviewTypes2.GetHashCode());
            }

            [TestMethod]
            public void EmploymentPerformanceReviewTypeDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(employmentPerformanceReviewTypes1.GetHashCode(), employmentPerformanceReviewTypes3.GetHashCode());
            }
        }
    }
}
