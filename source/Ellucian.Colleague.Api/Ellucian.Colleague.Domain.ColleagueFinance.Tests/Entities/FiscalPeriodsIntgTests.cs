//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class FiscalPeriodsIntgTests
    {
        [TestClass]
        public class FiscalPeriodsIntgConstructor
        {
            private string guid;
            private string recordkey;
            private string title;
            private string status;
            private FiscalPeriodsIntg FiscalPeriodsIntgs;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                recordkey = "2017";
                title = "Fiscal Period: 2017";
                status = "O";
                FiscalPeriodsIntgs = new FiscalPeriodsIntg(guid, recordkey)
                { Status = status, Title = title };
            }

            [TestMethod]
            public void FiscalPeriodsIntg_RecordKey()
            {
                Assert.AreEqual(recordkey, FiscalPeriodsIntgs.Id);
            }

            [TestMethod]
            public void FiscalPeriodsIntg_Title()
            {
                Assert.AreEqual(title, FiscalPeriodsIntgs.Title);
            }

            [TestMethod]
            public void FiscalPeriodsIntg_Status()
            {
                Assert.AreEqual(status, FiscalPeriodsIntgs.Status);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void FiscalPeriodsIntg_GuidNullException()
            {
                new FiscalPeriodsIntg(null, recordkey);
            }                         
        }

        [TestClass]
        public class FiscalPeriodsIntg_Equals
        {
            private string guid;
            private string recordkey;         
            private FiscalPeriodsIntg FiscalPeriodsIntgs1;
            private FiscalPeriodsIntg FiscalPeriodsIntgs2;
            private FiscalPeriodsIntg FiscalPeriodsIntgs3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                recordkey = "2017";              
                FiscalPeriodsIntgs1 = new FiscalPeriodsIntg(guid, recordkey);
                FiscalPeriodsIntgs2 = new FiscalPeriodsIntg(guid, recordkey);
                FiscalPeriodsIntgs3 = new FiscalPeriodsIntg(Guid.NewGuid().ToString(), recordkey);
            }

            [TestMethod]
            public void FiscalPeriodsIntgDifferentCodeNotEqual()
            {
                Assert.IsFalse(FiscalPeriodsIntgs1.Equals(FiscalPeriodsIntgs3));
            }
        }

        [TestClass]
        public class FiscalPeriodsIntg_GetHashCode
        {
            private string guid;
            private string recordkey;           
            private FiscalPeriodsIntg FiscalPeriodsIntgs1;
            private FiscalPeriodsIntg FiscalPeriodsIntgs2;
            private FiscalPeriodsIntg FiscalPeriodsIntgs3;

            [TestInitialize]
            public void Initialize()
            {
                guid = Guid.NewGuid().ToString();
                recordkey = "2017";              
                FiscalPeriodsIntgs1 = new FiscalPeriodsIntg(guid, recordkey);
                FiscalPeriodsIntgs2 = new FiscalPeriodsIntg(guid, recordkey);
                FiscalPeriodsIntgs3 = new FiscalPeriodsIntg(Guid.NewGuid().ToString(), recordkey);
            }

            [TestMethod]
            public void FiscalPeriodsIntgDifferentCodeHashNotEqual()
            {
                Assert.AreNotEqual(FiscalPeriodsIntgs1.GetHashCode(), FiscalPeriodsIntgs3.GetHashCode());
            }
        }
    }
}