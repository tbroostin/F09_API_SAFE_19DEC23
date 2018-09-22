//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Student.Entities;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
     [TestClass]
    public class AccountingCodeCategoryEntityTests
    {
        [TestClass]
        public class AccountingCodeCategoryEntityTestsConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private ArCategory accountingCodeCategory;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                accountingCodeCategory = new ArCategory(guid, code, desc);
            }

            [TestMethod]
            public void ArCategories_Code()
            {
                Assert.AreEqual(code, accountingCodeCategory.Code);
            }

            [TestMethod]
            public void ArCategories_Description()
            {
                Assert.AreEqual(desc, accountingCodeCategory.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ArCategories_GuidNullException()
            {
                new ArCategory(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ArCategories_CodeNullException()
            {
                new ArCategory(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ArCategories_DescNullException()
            {
                new ArCategory(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ArCategoriesGuidEmptyException()
            {
                new ArCategory(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ArCategoriesCodeEmptyException()
            {
                new ArCategory(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ArCategoriesDescEmptyException()
            {
                new ArCategory(guid, code, string.Empty);
            }
        }

        [TestClass]
        public class ArCategories_Equals
        {
            private string guid;
            private string code;
            private string desc;
            private ArCategory accountingCodeCategories1;
            private ArCategory accountingCodeCategories2;
            private ArCategory accountingCodeCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                accountingCodeCategories1 = new ArCategory(guid, code, desc);
                accountingCodeCategories2 = new ArCategory(guid, code, "Second Year");
                accountingCodeCategories3 = new ArCategory(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void ArCategoriesSameCodesEqual()
            {
                Assert.IsTrue(accountingCodeCategories1.Equals(accountingCodeCategories2));
            }

            [TestMethod]
            public void ArCategoriesDifferentCodeNotEqual()
            {
                Assert.IsFalse(accountingCodeCategories1.Equals(accountingCodeCategories3));
            }
        }

        [TestClass]
        public class ArCategories_GetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private ArCategory accountingCodeCategories1;
            private ArCategory accountingCodeCategories2;
            private ArCategory accountingCodeCategories3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "AD";
                desc = "Admissions";
                accountingCodeCategories1 = new ArCategory(guid, code, desc);
                accountingCodeCategories2 = new ArCategory(guid, code, "Second Year");
                accountingCodeCategories3 = new ArCategory(Guid.NewGuid().ToString(), "200", desc);
            }

            [TestMethod]
            public void ArCategoriesSameCodeHashEqual()
            {
                Assert.AreEqual(accountingCodeCategories1.GetHashCode(), accountingCodeCategories2.GetHashCode());
            }

            [TestMethod]
            public void ArCategoriesDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(accountingCodeCategories1.GetHashCode(), accountingCodeCategories3.GetHashCode());
            }
        }
    }
}
