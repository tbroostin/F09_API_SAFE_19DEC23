// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Portal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Portal
{
    [TestClass]
    public class PortalEventsAndRemindersTests
    {
        PortalEventsAndReminders entity;

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void PortalEventsAndReminders_Constructor_null_HostShortDateFormat()
        {
            entity = new PortalEventsAndReminders(null);
        }

        [TestMethod]
        public void PortalEventsAndReminders_Constructor_valid()
        {
            entity = new PortalEventsAndReminders("MDY");
            Assert.AreEqual("MDY", entity.HostShortDateFormat);
            Assert.AreEqual(0, entity.Events.Count);
            Assert.AreEqual(0, entity.Reminders.Count);
        }

        [TestMethod]
        public void PortalEventsAndReminders_AddEventReminder_null()
        {
            entity = new PortalEventsAndReminders("MDY");
            entity.AddPortalEvent(null);
            Assert.AreEqual(0, entity.Events.Count);
        }

        [TestMethod]
        public void PortalEventsAndReminders_AddEventReminder_valid_no_duplicates()
        {
            entity = new PortalEventsAndReminders("MDY");
            entity.AddPortalEvent(new PortalEvent("1", "123", DateTime.Today, null, null, "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2"));
            entity.AddPortalEvent(new PortalEvent("1", "123", DateTime.Today, null, null, "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2"));
            Assert.AreEqual(1, entity.Events.Count);
        }

        [TestMethod]
        public void PortalEventsAndReminders_AddPortalReminder_null()
        {
            entity = new PortalEventsAndReminders("MDY");
            entity.AddPortalReminder(null);
            Assert.AreEqual(0, entity.Reminders.Count);
        }

        [TestMethod]
        public void PortalEventsAndReminders_AddPortalReminder_valid_no_duplicates()
        {
            entity = new PortalEventsAndReminders("MDY");
            entity.AddPortalReminder(new PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today, DateTime.Today.AddHours(10), "City", "Region", "HO Holiday", "Short Text", "Participant 1, Participant 2"));
            entity.AddPortalReminder(new PortalReminder("1", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today, DateTime.Today.AddHours(10), "City", "Region", "HO Holiday", "Short Text", "Participant 1, Participant 2"));
            Assert.AreEqual(1, entity.Reminders.Count);
        }
    }
}
