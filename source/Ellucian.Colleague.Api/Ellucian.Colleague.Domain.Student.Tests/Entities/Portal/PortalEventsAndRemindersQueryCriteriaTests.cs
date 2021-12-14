// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Portal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Portal
{
    [TestClass]
    public class PortalEventsAndRemindersQueryCriteriaTests
    {
        PortalEventsAndRemindersQueryCriteria entity;

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod]
        public void PortalEventsAndRemindersQueryCriteria_Constructor_StartDate_Greater_than_EndDate()
        {
            entity = new PortalEventsAndRemindersQueryCriteria(DateTime.Today, DateTime.Today.AddDays(-1));
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod]
        public void PortalEventsAndRemindersQueryCriteria_Constructor_StartDate_greater_than_today_no_end_date()
        {
            entity = new PortalEventsAndRemindersQueryCriteria(DateTime.Today.AddDays(1));
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod]
        public void PortalEventsAndRemindersQueryCriteria_Constructor_EndDate_earlier_than_today_no_start_date()
        {
            entity = new PortalEventsAndRemindersQueryCriteria(null, DateTime.Today.AddDays(-11));
        }

        [TestMethod]
        public void PortalEventsAndRemindersQueryCriteria_Constructor_no_optional_parameters()
        {
            entity = new PortalEventsAndRemindersQueryCriteria();
            Assert.AreEqual(DateTime.Today, entity.StartDate);
            Assert.AreEqual(DateTime.Today, entity.EndDate);
            CollectionAssert.AreEqual(new List<string>() { "CS" }, entity.EventTypeCodes.ToList());
        }

        [TestMethod]
        public void PortalEventsAndRemindersQueryCriteria_Constructor_with_optional_parameters()
        {
            entity = new PortalEventsAndRemindersQueryCriteria(DateTime.Today.AddDays(1), DateTime.Today.AddDays(7), new List<string>() { "HO" });
            Assert.AreEqual(DateTime.Today.AddDays(1), entity.StartDate);
            Assert.AreEqual(DateTime.Today.AddDays(7), entity.EndDate);
            CollectionAssert.AreEqual(new List<string>() { "HO" }, entity.EventTypeCodes.ToList());
        }
    }
}
