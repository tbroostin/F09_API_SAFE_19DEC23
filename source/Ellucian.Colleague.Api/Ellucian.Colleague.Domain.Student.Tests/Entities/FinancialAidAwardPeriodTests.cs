// Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class FinancialAidAwardPeriodTests
    {
        [TestClass]
        public class FinancialAidAwardPeriodConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidAwardPeriod financialAidAwardPeriod;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                status = "STATUS1";
                financialAidAwardPeriod = new FinancialAidAwardPeriod(guid, code, desc, status);
            }

            [TestMethod]
            public void FinancialAidAwardPeriodGuid()
            {
                Assert.AreEqual(guid, financialAidAwardPeriod.Guid);
            }

            [TestMethod]
            public void FinancialAidAwardPeriodCode()
            {
                Assert.AreEqual(code, financialAidAwardPeriod.Code);
            }

            [TestMethod]
            public void FinancialAidAwardPeriodDescription()
            {
                Assert.AreEqual(desc, financialAidAwardPeriod.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidAwardPeriodGuidNullException()
            {
                new FinancialAidAwardPeriod(null, code, desc, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidAwardPeriodCodeNullException()
            {
                new FinancialAidAwardPeriod(guid, null, desc, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidAwardPeriodDescNullException()
            {
                new FinancialAidAwardPeriod(guid, code, null, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidAwardPeriodStatusNullException()
            {
                new FinancialAidAwardPeriod(guid, code, desc, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidAwardPeriodGuidEmptyException()
            {
                new FinancialAidAwardPeriod(string.Empty, code, desc, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidAwardPeriodCodeEmptyException()
            {
                new FinancialAidAwardPeriod(guid, string.Empty, desc, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidAwardPeriodDescEmptyException()
            {
                new FinancialAidAwardPeriod(guid, code, string.Empty, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidAwardPeriodStatusEmptyException()
            {
                new FinancialAidAwardPeriod(guid, code, desc, string.Empty);
            }
        }

        [TestClass]
        public class FinancialAidAwardPeriodEquals
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidAwardPeriod financialAidAwardPeriod1;
            private FinancialAidAwardPeriod financialAidAwardPeriod2;
            private FinancialAidAwardPeriod financialAidAwardPeriod3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                status = "STATUS1";
                financialAidAwardPeriod1 = new FinancialAidAwardPeriod(guid, code, desc, status);
                financialAidAwardPeriod2 = new FinancialAidAwardPeriod(guid, code, "DESC2", status);
                financialAidAwardPeriod3 = new FinancialAidAwardPeriod(Guid.NewGuid().ToString(), "CODE3", desc, status);
            }

            [TestMethod]
            public void FinancialAidAwardPeriodSameCodesEqual()
            {
                Assert.IsTrue(financialAidAwardPeriod1.Equals(financialAidAwardPeriod2));
            }

            [TestMethod]
            public void FinancialAidAwardPeriodDifferentCodeNotEqual()
            {
                Assert.IsFalse(financialAidAwardPeriod1.Equals(financialAidAwardPeriod3));
            }
        }

        [TestClass]
        public class FinancialAidAwardPeriodGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidAwardPeriod financialAidAwardPeriod1;
            private FinancialAidAwardPeriod financialAidAwardPeriod2;
            private FinancialAidAwardPeriod financialAidAwardPeriod3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                status = "STATUS1";
                financialAidAwardPeriod1 = new FinancialAidAwardPeriod(guid, code, desc, status);
                financialAidAwardPeriod2 = new FinancialAidAwardPeriod(guid, code, "DESC2", status);
                financialAidAwardPeriod3 = new FinancialAidAwardPeriod(Guid.NewGuid().ToString(), "CODE3", desc, status);
            }

            [TestMethod]
            public void FinancialAidAwardPeriodSameCodeHashEqual()
            {
                Assert.AreEqual(financialAidAwardPeriod1.GetHashCode(), financialAidAwardPeriod2.GetHashCode());
            }

            [TestMethod]
            public void FinancialAidAwardPeriodDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(financialAidAwardPeriod1.GetHashCode(), financialAidAwardPeriod3.GetHashCode());
            }
        }
    }
}