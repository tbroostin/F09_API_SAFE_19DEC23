using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class FinancialAidOfficeItemTests
    {
        [TestClass]
        public class FinancialAidOfficeItemConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private string name;
            private FinancialAidOfficeItem financialAidOfficeItem;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                name = "NAME1";
                financialAidOfficeItem = new FinancialAidOfficeItem(guid, code, desc, name);
            }

            [TestMethod]
            public void FinancialAidOfficeItemGuid()
            {
                Assert.AreEqual(guid, financialAidOfficeItem.Guid);
            }

            [TestMethod]
            public void FinancialAidOfficeItemCode()
            {
                Assert.AreEqual(code, financialAidOfficeItem.Code);
            }

            [TestMethod]
            public void FinancialAidOfficeItemDescription()
            {
                Assert.AreEqual(desc, financialAidOfficeItem.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidOfficeItemGuidNullException()
            {
                new FinancialAidOfficeItem(null, code, desc, name);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidOfficeItemCodeNullException()
            {
                new FinancialAidOfficeItem(guid, null, desc, name);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidOfficeItemDescNullException()
            {
                new FinancialAidOfficeItem(guid, code, null, name);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidOfficeItemNameNullException()
            {
                new FinancialAidOfficeItem(guid, code, desc, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidOfficeItemGuidEmptyException()
            {
                new FinancialAidOfficeItem(string.Empty, code, desc, name);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidOfficeItemCodeEmptyException()
            {
                new FinancialAidOfficeItem(guid, string.Empty, desc, name);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidOfficeItemDescEmptyException()
            {
                new FinancialAidOfficeItem(guid, code, string.Empty, name);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidOfficeItemNameEmptyException()
            {
                new FinancialAidOfficeItem(guid, code, desc, string.Empty);
            }
        }

        [TestClass]
        public class FinancialAidOfficeItemEquals
        {
            private string guid;
            private string code;
            private string desc;
            private string name;
            private FinancialAidOfficeItem financialAidOfficeItem1;
            private FinancialAidOfficeItem financialAidOfficeItem2;
            private FinancialAidOfficeItem financialAidOfficeItem3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                name = "NAME1";
                financialAidOfficeItem1 = new FinancialAidOfficeItem(guid, code, desc, name);
                financialAidOfficeItem2 = new FinancialAidOfficeItem(guid, code, "DESC2", name);
                financialAidOfficeItem3 = new FinancialAidOfficeItem(Guid.NewGuid().ToString(), "CODE3", desc, name);
            }

            [TestMethod]
            public void FinancialAidOfficeItemSameCodesEqual()
            {
                Assert.IsTrue(financialAidOfficeItem1.Equals(financialAidOfficeItem2));
            }

            [TestMethod]
            public void FinancialAidOfficeItemDifferentCodeNotEqual()
            {
                Assert.IsFalse(financialAidOfficeItem1.Equals(financialAidOfficeItem3));
            }
        }

        [TestClass]
        public class FinancialAidOfficeItemGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private string name;
            private FinancialAidOfficeItem financialAidOfficeItem1;
            private FinancialAidOfficeItem financialAidOfficeItem2;
            private FinancialAidOfficeItem financialAidOfficeItem3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                name = "NAME1";
                financialAidOfficeItem1 = new FinancialAidOfficeItem(guid, code, desc, name);
                financialAidOfficeItem2 = new FinancialAidOfficeItem(guid, code, "DESC2", name);
                financialAidOfficeItem3 = new FinancialAidOfficeItem(Guid.NewGuid().ToString(), "CODE3", desc, name);
            }

            [TestMethod]
            public void FinancialAidOfficeItemSameCodeHashEqual()
            {
                Assert.AreEqual(financialAidOfficeItem1.GetHashCode(), financialAidOfficeItem2.GetHashCode());
            }

            [TestMethod]
            public void FinancialAidOfficeItemDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(financialAidOfficeItem1.GetHashCode(), financialAidOfficeItem3.GetHashCode());
            }
        }
    }
}