using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.FinancialAid.Entities;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class FinancialAidFundClassificationTests
    {
        [TestClass]
        public class FinancialAidFundClassificationConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private FinancialAidFundClassification financialAidFundClassification;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                financialAidFundClassification = new FinancialAidFundClassification(guid, code, desc);
            }

            [TestMethod]
            public void FinancialAidFundClassificationGuid()
            {
                Assert.AreEqual(guid, financialAidFundClassification.Guid);
            }

            [TestMethod]
            public void FinancialAidFundClassificationCode()
            {
                Assert.AreEqual(code, financialAidFundClassification.Code);
            }

            [TestMethod]
            public void FinancialAidFundClassificationDescription()
            {
                Assert.AreEqual(desc, financialAidFundClassification.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundClassificationGuidNullException()
            {
                new FinancialAidFundClassification(null, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundClassificationCodeNullException()
            {
                new FinancialAidFundClassification(guid, null, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundClassificationDescNullException()
            {
                new FinancialAidFundClassification(guid, code, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundClassificationGuidEmptyException()
            {
                new FinancialAidFundClassification(string.Empty, code, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundClassificationCodeEmptyException()
            {
                new FinancialAidFundClassification(guid, string.Empty, desc);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidFundClassificationDescEmptyException()
            {
                new FinancialAidFundClassification(guid, code, string.Empty);
            }
        }

        [TestClass]
        public class FinancialAidFundClassificationEquals
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidFundClassification financialAidFundClassification1;
            private FinancialAidFundClassification financialAidFundClassification2;
            private FinancialAidFundClassification financialAidFundClassification3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                financialAidFundClassification1 = new FinancialAidFundClassification(guid, code, desc);
                financialAidFundClassification2 = new FinancialAidFundClassification(guid, code, "DESC2");
                financialAidFundClassification3 = new FinancialAidFundClassification(Guid.NewGuid().ToString(), "CODE3", desc);
            }

            [TestMethod]
            public void FinancialAidFundClassificationSameCodesEqual()
            {
                Assert.IsTrue(financialAidFundClassification1.Equals(financialAidFundClassification2));
            }

            [TestMethod]
            public void FinancialAidFundClassificationDifferentCodeNotEqual()
            {
                Assert.IsFalse(financialAidFundClassification1.Equals(financialAidFundClassification3));
            }
        }

        [TestClass]
        public class FinancialAidFundClassificationGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidFundClassification financialAidFundClassification1;
            private FinancialAidFundClassification financialAidFundClassification2;
            private FinancialAidFundClassification financialAidFundClassification3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                financialAidFundClassification1 = new FinancialAidFundClassification(guid, code, desc);
                financialAidFundClassification2 = new FinancialAidFundClassification(guid, code, "DESC2");
                financialAidFundClassification3 = new FinancialAidFundClassification(Guid.NewGuid().ToString(), "CODE3", desc);
            }

            [TestMethod]
            public void FinancialAidFundClassificationSameCodeHashEqual()
            {
                Assert.AreEqual(financialAidFundClassification1.GetHashCode(), financialAidFundClassification2.GetHashCode());
            }

            [TestMethod]
            public void FinancialAidFundClassificationDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(financialAidFundClassification1.GetHashCode(), financialAidFundClassification3.GetHashCode());
            }
        }
    }
}