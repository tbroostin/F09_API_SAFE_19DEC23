using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class SpecialDayTests
    {
        public string id;
        public string description;
        public string calendarId;
        public string typeCode;
        public bool isHoliday;
        public bool isFullDay;
        public DateTimeOffset startDateTime;
        public DateTimeOffset endDateTime;

        public SpecialDay specialDay
        {
            get
            {
                return new SpecialDay(id, description, calendarId, typeCode, isHoliday, isFullDay, startDateTime, endDateTime);
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            id = "1";
            description = "New Years Day";
            calendarId = "HOLIDAYS";
            typeCode = "HOL";
            isHoliday = true;
            isFullDay = true;
            startDateTime = new DateTimeOffset(new DateTime(2018, 1, 1));
            endDateTime = new DateTimeOffset(new DateTime(2018, 1, 1));
        }
    }

    [TestClass]
    public class SpecialDayConstructorTests : SpecialDayTests
    {

        [TestMethod]
        public void AttributesTest()
        {
            var actual = specialDay;
            Assert.AreEqual(id, actual.Id);
            Assert.AreEqual(description, actual.Description);
            Assert.AreEqual(calendarId, actual.CampusCalendarId);
            Assert.AreEqual(typeCode, actual.SpecialDayTypeCode);
            Assert.AreEqual(isHoliday, actual.IsHoliday);
            Assert.AreEqual(isFullDay, actual.IsFullDay);
            Assert.AreEqual(startDateTime, actual.StartDateTime);
            Assert.AreEqual(endDateTime, actual.EndDateTime);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IdRequiredTest()
        {
            id = null;
            var actual = specialDay;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DescriptionRequiredTest()
        {
            description = null;
            var actual = specialDay;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CampusCalendarIdRequiredTest()
        {
            calendarId = null;
            var actual = specialDay;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SpecialDayTypeCodeRequiredTest()
        {
            typeCode = null;
            var actual = specialDay;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartDateTimeIsDefaultTest()
        {
            startDateTime = default(DateTimeOffset);
            var actual = specialDay;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EndDateTimeIsDefaultTest()
        {
            endDateTime = default(DateTimeOffset);
            var actual = specialDay;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void StartDateTimeIsAfterEndDateTimeTest()
        {
            startDateTime = endDateTime.AddSeconds(1);
            var actual = specialDay;
        }
    }


    [TestClass]
    public class SpecialDayEqualsTests : SpecialDayTests
    {
        [TestMethod]
        public void IdsAreEqualTest()
        {
            var specialDay1 = specialDay;
            var specialDay2 = specialDay;

            Assert.IsTrue(specialDay1.Equals(specialDay2));
            Assert.IsTrue(specialDay2.Equals(specialDay1));
        }

        [TestMethod]
        public void IdsNotEqualTest()
        {
            var specialDay1 = specialDay;
            id = "foo";
            var specialDay2 = specialDay;

            Assert.IsFalse(specialDay1.Equals(specialDay2));
            Assert.IsFalse(specialDay2.Equals(specialDay1));
        }

        [TestMethod]
        public void NullNotEqualTest()
        {
            Assert.IsFalse(specialDay.Equals(null));
        }

        [TestMethod]
        public void DifferentTypeNotEqualTest()
        {
            Assert.IsFalse(specialDay.Equals(1));
        }
    }

    [TestClass]
    public class SpecialDayGetHashCodeTests : SpecialDayTests
    {
        [TestMethod]
        public void IdsSame_HashCodeSameTest()
        {
            var specialDay1 = specialDay;
            var specialDay2 = specialDay;

            Assert.AreEqual(specialDay1.GetHashCode(), specialDay2.GetHashCode());
        }

        [TestMethod]
        public void IdsNotSame_HashCodeNotSameTest()
        {
            var specialDay1 = specialDay;
            id = "foo";
            var specialDay2 = specialDay;

            Assert.AreNotEqual(specialDay1.GetHashCode(), specialDay2.GetHashCode());
        }
    }

    [TestClass]
    public class SpecialDayCompareTests : SpecialDayTests
    {
        [TestMethod]
        public void CompareStartDateTimeTest()
        {
            var specialDay1 = specialDay;
            startDateTime = startDateTime.AddMinutes(-1);
            var specialDay2 = specialDay;

            //specialday2 is earlier than specialDay1

            Assert.AreEqual(1, specialDay1.CompareTo(specialDay2));
            Assert.AreEqual(-1, specialDay2.CompareTo(specialDay1));
        }

        [TestMethod]
        public void CompareDescriptionTest()
        {
            var specialDay1 = specialDay;
            description = description + "extra";
            var specialDay2 = specialDay;

            //specialDay1 and specialDay2 have the same startDateTime, so
            //compare by description
            //specialDay1 is "less than" specialDay2

            Assert.AreEqual(-1, specialDay1.CompareTo(specialDay2));
            Assert.AreEqual(1, specialDay2.CompareTo(specialDay1));
        }

        [TestMethod]
        public void CompareIdTest()
        {
            var specialDay1 = specialDay;
            id = id + "extra";
            var specialDay2 = specialDay;

            //specialDay1 and specialDay2 have the same startDateTime and description, so
            //compare by id
            //specialDay1 is "less than" specialDay2

            Assert.AreEqual(-1, specialDay1.CompareTo(specialDay2));
            Assert.AreEqual(1, specialDay2.CompareTo(specialDay1));
        }
    }
}
