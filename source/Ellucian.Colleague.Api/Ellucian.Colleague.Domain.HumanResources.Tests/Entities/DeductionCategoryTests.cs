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
    public class DeductionCategoryTests
    {
        [TestClass]
        public class DeductionCategoryConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private DeductionCategory deductionCategories;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                deductionCategories = new DeductionCategory(guid, code, desc);
            }

            [TestMethod]
            public void DeductionCategory_Code()
            {
                Assert.AreEqual(code, deductionCategories.Code);
            }

            [TestMethod]
            public void DeductionCategory_Description()
            {
                Assert.AreEqual(desc, deductionCategories.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DeductionCategory_GuidNullException()
            {
                new DeductionCategory(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DeductionCategory_CodeNullException()
            {
                new DeductionCategory(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DeductionCategory_DescNullException()
            {
                new DeductionCategory(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DeductionCategoryGuidEmptyException()
            {
                new DeductionCategory(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DeductionCategoryCodeEmptyException()
            {
                new DeductionCategory(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void DeductionCategoryDescEmptyException()
            {
                new DeductionCategory(guid, code, string.Empty);
            }

        }

        [TestClass]
        public class DeductionCategory_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private DeductionCategory deductionCategories1;
            private DeductionCategory deductionCategories2;
            private DeductionCategory deductionCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                deductionCategories1 = new DeductionCategory(guid, code, desc);
                deductionCategories2 = new DeductionCategory(guid, code, "Second Year");
                deductionCategories3 = new DeductionCategory(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void DeductionCategorySameCodesEqual()
            {
                Assert.IsTrue(deductionCategories1.Equals(deductionCategories2));
            }

            [TestMethod]
            public void DeductionCategoryDifferentCodeNotEqual()
            {
                Assert.IsFalse(deductionCategories1.Equals(deductionCategories3));
            }
        }

        [TestClass]
        public class DeductionCategory_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private DeductionCategory deductionCategories1;
            private DeductionCategory deductionCategories2;
            private DeductionCategory deductionCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                deductionCategories1 = new DeductionCategory(guid, code, desc);
                deductionCategories2 = new DeductionCategory(guid, code, "Second Year");
                deductionCategories3 = new DeductionCategory(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void DeductionCategorySameCodeHashEqual()
            {
                Assert.AreEqual(deductionCategories1.GetHashCode(), deductionCategories2.GetHashCode());
            }

            [TestMethod]
            public void DeductionCategoryDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(deductionCategories1.GetHashCode(), deductionCategories3.GetHashCode());
            }
        }
    }
}
