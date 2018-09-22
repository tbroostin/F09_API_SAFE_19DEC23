// Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class NonAcademicAttendanceTests
    {
        private string id;
        private string personId;
        private string eventId;
        private decimal? unitsEarned;

        private NonAcademicAttendance entity;

        [TestInitialize]
        public void Initialize()
        {
            id = "123";
            personId = "0001234";
            eventId = "234";
            unitsEarned = 27m;
        }

        [TestClass]
        public class NonAcademicAttendance_Constructor_Tests : NonAcademicAttendanceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonAcademicAttendance_Constructor_Null_Id()
            {
                entity = new NonAcademicAttendance(null, personId, eventId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonAcademicAttendance_Constructor_Null_PersonId()
            {
                entity = new NonAcademicAttendance(id, null, eventId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NonAcademicAttendance_Constructor_Null_EventId()
            {
                entity = new NonAcademicAttendance(id, personId, null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void NonAcademicAttendance_Constructor_Invalid_UnitsEarned()
            {
                entity = new NonAcademicAttendance(id, personId, eventId, -30m);
            }

            [TestMethod]
            public void NonAcademicAttendance_Constructor_Default_Values()
            {
                entity = new NonAcademicAttendance(id, personId, eventId);
                Assert.AreEqual(id, entity.Id);
                Assert.AreEqual(personId, entity.PersonId);
                Assert.AreEqual(eventId, entity.EventId);
                Assert.IsNull(entity.UnitsEarned);
            }

            [TestMethod]
            public void NonAcademicAttendance_Constructor_with_NonAcademicAttendanceIds()
            {
                entity = new NonAcademicAttendance(id, personId, eventId, unitsEarned);
                Assert.AreEqual(unitsEarned, entity.UnitsEarned);
            }
        }
    }
}