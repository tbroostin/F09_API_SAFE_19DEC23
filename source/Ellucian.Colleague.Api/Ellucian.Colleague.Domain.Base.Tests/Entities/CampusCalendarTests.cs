// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class CampusCalendarTests
    {
        private string id;
        private string description;
        private TimeSpan defaultStartOfDay;
        private TimeSpan defaultEndOfDay;
        private CampusCalendar campusCalendar;

        [TestInitialize]
        public void Initialize()
        {
            id = "MAIN";
            description = "Main Calendar for PID2";
            defaultStartOfDay = new TimeSpan(8, 0, 0);
            defaultEndOfDay = new TimeSpan(17, 0, 0);
              
            campusCalendar = new CampusCalendar(id, description, defaultStartOfDay, defaultEndOfDay);
        }

        [TestClass]
        public class CampusCalendarConstructorTests : CampusCalendarTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusCalendarConstructorNullId()
            {
                campusCalendar = new CampusCalendar(null, description, defaultStartOfDay, defaultEndOfDay);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusCalendarConstructorEmptyId()
            {
                campusCalendar = new CampusCalendar(string.Empty, description, defaultStartOfDay, defaultEndOfDay);
            }

            [TestMethod]
            public void CampusCalendarConstructorValidId()
            {
                campusCalendar = new CampusCalendar(id, description, defaultStartOfDay, defaultEndOfDay);
                Assert.AreEqual(id, campusCalendar.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusCalendarConstructorNullDescription()
            {
                campusCalendar = new CampusCalendar(id, null, defaultStartOfDay, defaultEndOfDay);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusCalendarConstructorEmptyDescription()
            {
                campusCalendar = new CampusCalendar(id, string.Empty, defaultStartOfDay, defaultEndOfDay);
            }

            [TestMethod]
            public void CampusCalendarConstructorValidDescription()
            {
                campusCalendar = new CampusCalendar(id, description, defaultStartOfDay, defaultEndOfDay);
                Assert.AreEqual(description, campusCalendar.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void CampusCalendarConstructorInvalidEndOfDay()
            {
                campusCalendar = new CampusCalendar(id, description, defaultEndOfDay, defaultStartOfDay);
            }

            [TestMethod]
            public void CampusCalendarConstructorValidStartOfDay()
            {
                campusCalendar = new CampusCalendar(id, description, defaultStartOfDay, defaultEndOfDay);
                Assert.AreEqual(defaultStartOfDay, campusCalendar.DefaultStartOfDay);
            }

            [TestMethod]
            public void CampusCalendarConstructorValidEndOfDay()
            {
                campusCalendar = new CampusCalendar(id, description, defaultStartOfDay, defaultEndOfDay);
                Assert.AreEqual(defaultEndOfDay, campusCalendar.DefaultEndOfDay);
            }
        }

        [TestClass]
        public class CampusCalendarAddBookedEventDayTests : CampusCalendarTests
        {
            [TestMethod]
            public void CampusCalendarAddBookedEventDay()
            {
                var bookedEventDay = DateTime.Today.AddDays(5);
                campusCalendar.AddBookedEventDate(bookedEventDay);
                Assert.AreEqual(bookedEventDay, campusCalendar.BookedEventDates[0]);
            }

            [TestMethod]
            public void CampusCalendarAddDuplicateBookedEventDay()
            {
                var bookedEventDay = DateTime.Today.AddDays(5);
                campusCalendar.AddBookedEventDate(bookedEventDay);
                campusCalendar.AddBookedEventDate(bookedEventDay);
                Assert.AreEqual(1, campusCalendar.BookedEventDates.Count);
                Assert.AreEqual(bookedEventDay, campusCalendar.BookedEventDates[0]);
            }
        }

        [TestClass]
        public class CampusCalendarAddSpecialDayTests : CampusCalendarTests
        {
            [TestMethod]
            public void AddSpecialDayTest()
            {
                var specialDay = new SpecialDay("1", "New Years Day", campusCalendar.Id, "HOL", true, true, new DateTimeOffset(new DateTime(2018, 1, 1)), new DateTimeOffset(new DateTime(2018, 1, 1)));
                campusCalendar.AddSpecialDay(specialDay);
                Assert.AreEqual(specialDay, campusCalendar.SpecialDays[0]);
            }

            [TestMethod]
            public void DoNotAddDuplicateSpecialDayTest()
            {
                var specialDay1 = new SpecialDay("1", "New Years Day", campusCalendar.Id, "HOL", true, true, new DateTimeOffset(new DateTime(2018, 1, 1)), new DateTimeOffset(new DateTime(2018, 1, 1)));
                var specialDay2 = new SpecialDay("1", "New Years Day", campusCalendar.Id, "HOL", true, true, new DateTimeOffset(new DateTime(2018, 1, 1)), new DateTimeOffset(new DateTime(2018, 1, 1)));
                campusCalendar.AddSpecialDay(specialDay1);
                campusCalendar.AddSpecialDay(specialDay2);

                Assert.AreEqual(1, campusCalendar.SpecialDays.Count);
                Assert.AreSame(specialDay1, campusCalendar.SpecialDays[0]);
            }

            [TestMethod]
            public void DoNotAddDifferentCalendarSpecialDayTest()
            {
                var specialDay = new SpecialDay("1", "New Years Day", "foobar", "HOL", true, true, new DateTimeOffset(new DateTime(2018, 1, 1)), new DateTimeOffset(new DateTime(2018, 1, 1)));
                campusCalendar.AddSpecialDay(specialDay);

                Assert.IsFalse(campusCalendar.SpecialDays.Any());
            }
        }

       
    }
}
