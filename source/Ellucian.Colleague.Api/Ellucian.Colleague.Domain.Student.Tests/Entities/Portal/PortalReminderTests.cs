// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Portal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Portal
{
    [TestClass]
    public class PortalReminderTests
    {
        PortalReminder entity;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalReminder_Constructor_null_Id()
        {
            entity = new PortalReminder(null, DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today, DateTime.Today.AddHours(10), "City", "Region", "HO Holiday", "Short Text", "Participant 1, Participant 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PortalReminder_Constructor_invalid_end_date()
        {
            entity = new PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today.AddDays(-1), DateTime.Today.AddHours(10), "City", "Region", "HO Holiday", "Short Text", "Participant 1, Participant 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalReminder_Constructor_null_reminder_type()
        {
            entity = new PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today, DateTime.Today.AddHours(10), "City", "Region", null, "Short Text", "Participant 1, Participant 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalReminder_Constructor_null_short_text()
        {
            entity = new PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today, DateTime.Today.AddHours(10), "City", "Region", "HO Holiday", null, "Participant 1, Participant 2");
        }

        [TestMethod]
        public void PortalReminder_Constructor_valid()
        {
            entity = new PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today, DateTime.Today.AddHours(10), "City", "Region", "HO Holiday", "Short Text", "Participant 1, Participant 2");
            Assert.AreEqual("1", entity.Id);
            Assert.AreEqual(DateTime.Today, entity.StartDate);
            Assert.AreEqual(DateTime.Today.AddHours(9), entity.StartTime);
            Assert.AreEqual(DateTime.Today, entity.EndDate);
            Assert.AreEqual(DateTime.Today.AddHours(10), entity.EndTime);
            Assert.AreEqual("City", entity.City);
            Assert.AreEqual("Region", entity.Region);
            Assert.AreEqual("HO Holiday", entity.ReminderType);
            Assert.AreEqual("Short Text", entity.ShortText);
            Assert.AreEqual("Participant 1, Participant 2", entity.Participants);
        }

        [TestMethod]
        public void PortalReminder_Constructor_valid_no_end_date_or_time()
        {
            entity = new PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(9), null, null, "City", "Region", "HO Holiday", "Short Text", "Participant 1, Participant 2");
            Assert.AreEqual("1", entity.Id);
            Assert.AreEqual(DateTime.Today, entity.StartDate);
            Assert.AreEqual(DateTime.Today.AddHours(9), entity.StartTime);
            Assert.AreEqual(null, entity.EndDate);
            Assert.AreEqual(null, entity.EndTime);
            Assert.AreEqual("City", entity.City);
            Assert.AreEqual("Region", entity.Region);
            Assert.AreEqual("HO Holiday", entity.ReminderType);
            Assert.AreEqual("Short Text", entity.ShortText);
            Assert.AreEqual("Participant 1, Participant 2", entity.Participants);
        }
    }
}
