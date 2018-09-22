//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class FiscalYearTests
    {
        [TestClass]
        public class FiscalYearConstructor
        {
            private string guid;
            private string recordkey;
            private string title;
            private string status;
            private FiscalYear fiscalYears;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                recordkey = "2017";
                title = "Fiscal Year: 2017";
                status = "O";
                fiscalYears = new FiscalYear(guid, recordkey)
                { Status = status, Title = title };
            }

            [TestMethod]
            public void FiscalYear_RecordKey()
            {
                Assert.AreEqual(recordkey, fiscalYears.Id);
            }

            [TestMethod]
            public void FiscalYear_Title()
            {
                Assert.AreEqual(title, fiscalYears.Title);
            }

            [TestMethod]
            public void FiscalYear_Status()
            {
                Assert.AreEqual(status, fiscalYears.Status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void FiscalYear_GuidNullException()
            {
                new FiscalYear(null, recordkey);
            }
                          
        }

        [TestClass]
        public class FiscalYear_Equals
        {
            private string guid;
            private string recordkey;
            private FiscalYear fiscalYears1;
            private FiscalYear fiscalYears2;
            private FiscalYear fiscalYears3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                recordkey = "2017";
                fiscalYears1 = new FiscalYear(guid, recordkey);
                fiscalYears2 = new FiscalYear(guid, recordkey);
                fiscalYears3 = new FiscalYear(Guid.NewGuid().ToString(), recordkey);
            }

            [TestMethod]
            public void FiscalYearDifferentCodeNotEqual()
            {
                Assert.IsFalse(fiscalYears1.Equals(fiscalYears3));
            }
        }

        [TestClass]
        public class FiscalYear_GetHashCode
        {
            private string guid;
            private string recordkey;
            private FiscalYear fiscalYears1;
            private FiscalYear fiscalYears2;
            private FiscalYear fiscalYears3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                recordkey = "2017";
                fiscalYears1 = new FiscalYear(guid, recordkey);
                fiscalYears2 = new FiscalYear(guid, recordkey);
                fiscalYears3 = new FiscalYear(Guid.NewGuid().ToString(), recordkey);
            }

            [TestMethod]
            public void FiscalYearDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(fiscalYears1.GetHashCode(), fiscalYears3.GetHashCode());
            }
        }
    }
}
