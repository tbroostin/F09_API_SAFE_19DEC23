// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class PlanStatusTests
    {
        PlanStatusType status = PlanStatusType.Open;
        DateTime date = DateTime.Today;

        [TestMethod]
        public void PlanStatus_Constructor_ValidStatus()
        {
            var result = new PlanStatus(status, date);

            Assert.AreEqual(status, result.Status);
        }

        [TestMethod]
        public void PlanStatus_Constructor_ValidDate()
        {
            var result = new PlanStatus(status, date);

            Assert.AreEqual(date, result.Date);
        }
    }
}
