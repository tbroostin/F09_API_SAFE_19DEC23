//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class FinancialAidYearTests
    {
        [TestClass]
        public class FinancialAidYearConstructor
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidYear financialAidYear;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                status = "STATUS1";
                financialAidYear = new FinancialAidYear(guid, code, desc, status);
            }

            [TestMethod]
            public void FinancialAidYearGuid()
            {
                Assert.AreEqual(guid, financialAidYear.Guid);
            }

            [TestMethod]
            public void FinancialAidYearCode()
            {
                Assert.AreEqual(code, financialAidYear.Code);
            }

            [TestMethod]
            public void FinancialAidYearDescription()
            {
                Assert.AreEqual(desc, financialAidYear.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidYearGuidNullException()
            {
                new FinancialAidYear(null, code, desc, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidYearCodeNullException()
            {
                new FinancialAidYear(guid, null, desc, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidYearDescNullException()
            {
                new FinancialAidYear(guid, code, null, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidYearStatusNullException()
            {
                new FinancialAidYear(guid, code, desc, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidYearGuidEmptyException()
            {
                new FinancialAidYear(string.Empty, code, desc, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidYearCodeEmptyException()
            {
                new FinancialAidYear(guid, string.Empty, desc, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidYearDescEmptyException()
            {
                new FinancialAidYear(guid, code, string.Empty, status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FinancialAidYearStatusEmptyException()
            {
                new FinancialAidYear(guid, code, desc, string.Empty);
            }
        }

        [TestClass]
        public class FinancialAidYearEquals
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidYear financialAidYear1;
            private FinancialAidYear financialAidYear2;
            private FinancialAidYear financialAidYear3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                status = "STATUS1";
                financialAidYear1 = new FinancialAidYear(guid, code, desc, status);
                financialAidYear2 = new FinancialAidYear(guid, code, "DESC2", status);
                financialAidYear3 = new FinancialAidYear(Guid.NewGuid().ToString(), "CODE3", desc, status);
            }

            [TestMethod]
            public void FinancialAidYearSameCodesEqual()
            {
                Assert.IsTrue(financialAidYear1.Equals(financialAidYear2));
            }

            [TestMethod]
            public void FinancialAidYearDifferentCodeNotEqual()
            {
                Assert.IsFalse(financialAidYear1.Equals(financialAidYear3));
            }
        }

        [TestClass]
        public class FinancialAidYearGetHashCode
        {
            private string guid;
            private string code;
            private string desc;
            private string status;
            private FinancialAidYear financialAidYear1;
            private FinancialAidYear financialAidYear2;
            private FinancialAidYear financialAidYear3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                code = "CODE1";
                desc = "DESC1";
                status = "STATUS1";
                financialAidYear1 = new FinancialAidYear(guid, code, desc, status);
                financialAidYear2 = new FinancialAidYear(guid, code, "DESC2", status);
                financialAidYear3 = new FinancialAidYear(Guid.NewGuid().ToString(), "CODE3", desc, status);
            }

            [TestMethod]
            public void FinancialAidYearSameCodeHashEqual()
            {
                Assert.AreEqual(financialAidYear1.GetHashCode(), financialAidYear2.GetHashCode());
            }

            [TestMethod]
            public void FinancialAidYearDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(financialAidYear1.GetHashCode(), financialAidYear3.GetHashCode());
            }
        }
    }
}