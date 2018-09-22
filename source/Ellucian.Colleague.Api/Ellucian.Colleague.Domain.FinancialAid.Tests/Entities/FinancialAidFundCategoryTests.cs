using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class FinancialAidFundCategoryTests
    {
        [TestClass]
        public class FinancialAidFundCategoryConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private FinancialAidFundCategory financialAidFundCategory;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                financialAidFundCategory = new FinancialAidFundCategory(guid, code, desc);
            }

            [TestMethod]
            public void FinancialAidFundCategoryGuid()
            {
                Assert.AreEqual(guid, financialAidFundCategory.Guid);
            }

            [TestMethod]
            public void FinancialAidFundCategoryCode()
            {
                Assert.AreEqual(code, financialAidFundCategory.Code);
            }

            [TestMethod]
            public void FinancialAidFundCategoryDescription()
            {
                Assert.AreEqual(desc, financialAidFundCategory.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundCategoryGuidNullException()
            {
                new FinancialAidFundCategory(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundCategoryCodeNullException()
            {
                new FinancialAidFundCategory(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundCategoryDescNullException()
            {
                new FinancialAidFundCategory(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundCategoryGuidEmptyException()
            {
                new FinancialAidFundCategory(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundCategoryCodeEmptyException()
            {
                new FinancialAidFundCategory(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundCategoryDescEmptyException()
            {
                new FinancialAidFundCategory(guid, code, string.Empty);
            }
        }

        [TestClass]
        public class FinancialAidFundCategoryEquals
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidFundCategory financialAidFundCategory1;
            private FinancialAidFundCategory financialAidFundCategory2;
            private FinancialAidFundCategory financialAidFundCategory3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                financialAidFundCategory1 = new FinancialAidFundCategory(guid, code, desc);
                financialAidFundCategory2 = new FinancialAidFundCategory(guid, code, "DESC2");
                financialAidFundCategory3 = new FinancialAidFundCategory(Guid.NewGuid().ToString(), "CODE3", desc);
            }

            [TestMethod]
            public void FinancialAidFundCategorySameCodesEqual()
            {
                Assert.IsTrue(financialAidFundCategory1.Equals(financialAidFundCategory2));
            }

            [TestMethod]
            public void FinancialAidFundCategoryDifferentCodeNotEqual()
            {
                Assert.IsFalse(financialAidFundCategory1.Equals(financialAidFundCategory3));
            }
        }

        [TestClass]
        public class FinancialAidFundCategoryGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidFundCategory financialAidFundCategory1;
            private FinancialAidFundCategory financialAidFundCategory2;
            private FinancialAidFundCategory financialAidFundCategory3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                financialAidFundCategory1 = new FinancialAidFundCategory(guid, code, desc);
                financialAidFundCategory2 = new FinancialAidFundCategory(guid, code, "DESC2");
                financialAidFundCategory3 = new FinancialAidFundCategory(Guid.NewGuid().ToString(), "CODE3", desc);
            }

            [TestMethod]
            public void FinancialAidFundCategorySameCodeHashEqual()
            {
                Assert.AreEqual(financialAidFundCategory1.GetHashCode(), financialAidFundCategory2.GetHashCode());
            }

            [TestMethod]
            public void FinancialAidFundCategoryDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(financialAidFundCategory1.GetHashCode(), financialAidFundCategory3.GetHashCode());
            }
        }
    }
}