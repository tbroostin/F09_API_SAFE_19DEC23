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
    public class PlanScheduleTests
    {
        DateTime dueDate = DateTime.Parse("08/01/2012");
        decimal amount = 500.00m;

        [TestMethod]
        public void PlanSchedule_Constructor_DueDate()
        {
            var result = new PlanSchedule(dueDate, amount);
            Assert.AreEqual(dueDate, result.DueDate);
        }

        [TestMethod]
        public void PlanSchedule_Constructor_Amount()
        {
            var result = new PlanSchedule(dueDate, amount);
            Assert.AreEqual(amount, result.Amount);
        }
    }
}