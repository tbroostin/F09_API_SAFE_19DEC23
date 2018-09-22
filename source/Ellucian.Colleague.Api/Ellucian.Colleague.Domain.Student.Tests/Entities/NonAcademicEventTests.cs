// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class NonAcademicEventTests
    {
        NonAcademicEvent eventEntity;
        DateTime currentTime = DateTime.Now;

        [TestInitialize]
        public void Initialize()
        {
            eventEntity = new NonAcademicEvent("1", "Spring", "Event Title 1");
            eventEntity.BuildingCode = "Building Code";
            eventEntity.Date = DateTime.Today;
            eventEntity.EndTime = currentTime;
            eventEntity.EventTypeCode = "Event Type";
            eventEntity.LocationCode = "Location Code";
            eventEntity.RoomCode = "Room Code";
            eventEntity.StartTime = currentTime.AddHours(-2);
            eventEntity.Venue = "Venue";
            eventEntity.Description = "Description";
        }

        [TestMethod]
        public void NonAcademicEvent_checkAllEventInfo()
        {
            Assert.AreEqual("1", eventEntity.Id);
            Assert.AreEqual("Spring", eventEntity.TermCode);
            Assert.AreEqual("Event Title 1", eventEntity.Title);
            Assert.AreEqual("Building Code", eventEntity.BuildingCode);
            Assert.AreEqual(DateTime.Today, eventEntity.Date);
            Assert.AreEqual(currentTime, eventEntity.EndTime);
            Assert.AreEqual("Event Type", eventEntity.EventTypeCode);
            Assert.AreEqual("Location Code", eventEntity.LocationCode);
            Assert.AreEqual("Room Code", eventEntity.RoomCode);
            Assert.AreEqual(currentTime.AddHours(-2), eventEntity.StartTime);
            Assert.AreEqual("Venue", eventEntity.Venue);
            Assert.AreEqual("Description", eventEntity.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NonAcademicEvent_setEndTimeEarlierThanStartTime()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent("1", "Spring", "Event Title 1");
            eventFail.StartTime = DateTime.Now.AddHours(2);
            eventFail.EndTime = DateTime.Now;
        }

        [TestMethod]
        public void NonAcademicEvent_setEndTimeNull()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent("1", "Spring", "Event Title 1");
            eventFail.StartTime = DateTime.Now.AddHours(2);
            eventFail.EndTime = null;
            Assert.IsNull(eventFail.EndTime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void NonAcademicEvent_setStartTimeLaterThanEndTime()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent("1", "Spring", "Event Title 1");
            eventFail.EndTime = DateTime.Now;
            eventFail.StartTime = DateTime.Now.AddHours(2);

        }

        [TestMethod]
        public void NonAcademicEvent_setStartTimeNull()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent("1", "Spring", "Event Title 1");
            eventFail.EndTime = DateTime.Now.AddHours(2);
            eventFail.StartTime = null;
            Assert.IsNull(eventFail.StartTime);
        }

        [TestMethod]
        public void NonAcademicEvent_StartTimeOnly()
        {
            var startTime = DateTime.Now.AddHours(-2);
            NonAcademicEvent eventTest = new NonAcademicEvent("1", "Spring", "Event Title 1");
            eventTest.StartTime = startTime;
            Assert.AreEqual("1", eventTest.Id);
            Assert.AreEqual("Spring", eventTest.TermCode);
            Assert.AreEqual("Event Title 1", eventTest.Title);
            Assert.IsNull(eventTest.BuildingCode);
            Assert.IsNull(eventTest.Date);
            Assert.IsNull(eventTest.EndTime);
            Assert.IsNull(eventTest.EventTypeCode);
            Assert.IsNull(eventTest.LocationCode);
            Assert.IsNull(eventTest.RoomCode);
            Assert.IsNull(eventTest.Venue);
            Assert.AreEqual(startTime, eventTest.StartTime);

        }

        [TestMethod]
        public void NonAcademicEvent_startTimeNull()
        {
            NonAcademicEvent eventTest = new NonAcademicEvent("1", "Spring", "Event Title 1");
            eventTest.EndTime = currentTime;
            Assert.AreEqual(null, eventTest.StartTime);
            Assert.AreEqual(currentTime, eventTest.EndTime);
        }

        [TestMethod]
        public void NonAcademicEvent_endTimeNull()
        {
            NonAcademicEvent eventTest = new NonAcademicEvent("1", "Spring", "Event Title 1");
            eventTest.StartTime = currentTime;
            Assert.AreEqual(null, eventTest.EndTime);
            Assert.AreEqual(currentTime, eventTest.StartTime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonAcademicEvent_NullRequiredId()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent(null, "Spring", "Event Title 1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonAcademicEvent_EmptyRequiredId()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent(string.Empty, "Spring", "Event Title 1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonAcademicEvent_NullRequiredTerm()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent("1", null, "Event Title 1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonAcademicEvent_EmptyRequiredTerm()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent("1", string.Empty, "Event Title 1");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonAcademicEvent_NullRequiredTitle()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent("1", "Spring", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NonAcademicEvent_EmptyRequiredTitle()
        {
            NonAcademicEvent eventFail = new NonAcademicEvent("1", "Spring", string.Empty);
            
        }
    }
}