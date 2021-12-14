// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities.Portal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities.Portal
{
    [TestClass]
    public class PortalEventTests
    {
        PortalEvent entity;

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalEvent_Constructor_null_Id()
        {
            entity = new PortalEvent(null, "123", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today.AddHours(10), "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalEvent_Constructor_null_CourseSectionId()
        {
            entity = new PortalEvent("123", null, DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today.AddHours(10), "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalEvent_Constructor_null_Date()
        {
            entity = new PortalEvent("1", "123", null, DateTime.Today.AddHours(9), DateTime.Today.AddHours(10), "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PortalEvent_Constructor_invalid_times()
        {
            entity = new PortalEvent("1", "123", DateTime.Today, DateTime.Today.AddHours(10), DateTime.Today.AddHours(9), "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PortalEvent_Constructor_end_time_no_start_time()
        {
            entity = new PortalEvent("1", "123", DateTime.Today, null, DateTime.Today.AddHours(9), "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2");
        }

        [TestMethod]
        public void PortalEvent_Constructor_valid()
        {
            entity = new PortalEvent("1", "123", DateTime.Today, DateTime.Today.AddHours(9), DateTime.Today.AddHours(10), "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2");
            Assert.AreEqual("1", entity.Id);
            Assert.AreEqual("123", entity.CourseSectionId);
            Assert.AreEqual(DateTime.Today, entity.Date);
            Assert.AreEqual(DateTime.Today.AddHours(9), entity.StartTime);
            Assert.AreEqual(DateTime.Today.AddHours(10), entity.EndTime);
            Assert.AreEqual("World History 1", entity.Description);
            Assert.AreEqual("Lester B. Pearson Hall", entity.Building);
            Assert.AreEqual("102", entity.Room);
            Assert.AreEqual("CS Course Section Meeting", entity.EventType);
            Assert.AreEqual("HIST", entity.Subject);
            Assert.AreEqual("HIST-100", entity.CourseNumber);
            Assert.AreEqual("01", entity.SectionNumber);
            Assert.AreEqual("Participant 1, Participant 2", entity.Participants);
        }

        [TestMethod]
        public void PortalEvent_Constructor_valid_no_times()
        {
            entity = new PortalEvent("1", "123", DateTime.Today, null, null, "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2");
            Assert.AreEqual("1", entity.Id);
            Assert.AreEqual("123", entity.CourseSectionId);
            Assert.AreEqual(DateTime.Today, entity.Date);
            Assert.AreEqual(null, entity.StartTime);
            Assert.AreEqual(null, entity.EndTime);
            Assert.AreEqual("World History 1", entity.Description);
            Assert.AreEqual("Lester B. Pearson Hall", entity.Building);
            Assert.AreEqual("102", entity.Room);
            Assert.AreEqual("CS Course Section Meeting", entity.EventType);
            Assert.AreEqual("HIST", entity.Subject);
            Assert.AreEqual("HIST-100", entity.CourseNumber);
            Assert.AreEqual("01", entity.SectionNumber);
            Assert.AreEqual("Participant 1, Participant 2", entity.Participants);
        }

        [TestMethod]
        public void PortalEvent_Constructor_valid_no_end_time()
        {
            entity = new PortalEvent("1", "123", DateTime.Today, DateTime.Today.AddHours(9), null, "World History 1", "Lester B. Pearson Hall", "102", "CS Course Section Meeting", "HIST", "HIST-100", "01", "Participant 1, Participant 2");
            Assert.AreEqual("1", entity.Id);
            Assert.AreEqual("123", entity.CourseSectionId);
            Assert.AreEqual(DateTime.Today, entity.Date);
            Assert.AreEqual(DateTime.Today.AddHours(9), entity.StartTime);
            Assert.AreEqual(null, entity.EndTime);
            Assert.AreEqual("World History 1", entity.Description);
            Assert.AreEqual("Lester B. Pearson Hall", entity.Building);
            Assert.AreEqual("102", entity.Room);
            Assert.AreEqual("CS Course Section Meeting", entity.EventType);
            Assert.AreEqual("HIST", entity.Subject);
            Assert.AreEqual("HIST-100", entity.CourseNumber);
            Assert.AreEqual("01", entity.SectionNumber);
            Assert.AreEqual("Participant 1, Participant 2", entity.Participants);
        }
    }
}
