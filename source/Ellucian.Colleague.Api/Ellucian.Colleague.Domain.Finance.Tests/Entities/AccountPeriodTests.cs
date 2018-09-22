// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities.AccountActivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class AccountPeriodTests
    {
        [TestMethod]
        public void AccountPeriod_AssociatedPeriods()
        {
            var ap = new AccountPeriod();
            Assert.AreEqual(null, ap.AssociatedPeriods);
        }

        [TestMethod]
        public void AccountPeriod_Balance()
        {
            var ap = new AccountPeriod();
            Assert.AreEqual(null, ap.Balance);
        }

        [TestMethod]
        public void AccountPeriod_Description()
        {
            var ap = new AccountPeriod();
            Assert.AreEqual(string.Empty, ap.Description);
        }

        [TestMethod]
        public void AccountPeriod_Id()
        {
            var ap = new AccountPeriod();
            Assert.AreEqual(string.Empty, ap.Id);
        }

        [TestMethod]
        public void AccountPeriod_StartDate()
        {
            var ap = new AccountPeriod();
            Assert.AreEqual(null, ap.StartDate);
        }

        [TestMethod]
        public void AccountPeriod_EndtDate()
        {
            var ap = new AccountPeriod();
            Assert.AreEqual(null, ap.EndDate);
        }
    }
}
