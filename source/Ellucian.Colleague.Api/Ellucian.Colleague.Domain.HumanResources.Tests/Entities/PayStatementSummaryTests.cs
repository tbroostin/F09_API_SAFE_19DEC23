/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class PayStatementSummaryTests
    {
        PayStatementSummary summary;
        string id;
        DateTime date;

        [TestInitialize]
        public void Initialize()
        {
            id = "001";
            date = DateTime.Now;
        }

        [TestMethod]
        public void PropertiesAreSetTest()
        {
            summary = new PayStatementSummary(id, date);
            Assert.AreEqual(id, summary.Id);
            Assert.AreEqual(date,summary.PayStatementDate);
        }

        [TestMethod,ExpectedException(typeof(ArgumentNullException))]
        public void NullIdTest()
        {
            new PayStatementSummary(null, date);
        }

        [TestMethod]
        public void EqualsOverride()
        {
            var summary1 = new PayStatementSummary(id, date);
            var summary3 = new PayStatementSummary(id, date);
            Assert.IsTrue(summary1.Equals(summary3));
            Assert.IsFalse(summary1.Equals(null));
            Assert.IsFalse(summary1.Equals(new Employee("12","123")));
        }

        [TestMethod]
        public void HashCodeOverride()
        {
            summary = new PayStatementSummary(id, date);
            Assert.AreEqual(id.GetHashCode(), summary.GetHashCode());
        }
    }
}
