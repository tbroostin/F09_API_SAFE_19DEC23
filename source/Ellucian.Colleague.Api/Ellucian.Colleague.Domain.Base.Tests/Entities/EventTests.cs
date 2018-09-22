// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class EventTests
    {
        // declare private vars
        private string id;
        private string description;
        private string type;
        private string location;
        private string pointer;
        private DateTime date;
        private DateTimeOffset startTime;
        private DateTimeOffset endTime;
        private DateTimeOffset start;
        private DateTimeOffset end;
        private List<string> buildings;
        private List<string> rooms;
        private Event cal1;
        private Event cal2;
        private TimeSpan offset;

        public void Main_Initialize()
        {
            id = "1";
            description = "HIST*100 Early Roman History";
            type = "CS";
            location = "MC";
            pointer = "3441";
            date = new DateTime(2012, 8, 21);
            offset = date.ToLocalTime() - date;
            startTime = new DateTime(date.Year, date.Month, date.Day, 9, 00, 00);
            endTime = new DateTime(date.Year, date.Month, date.Day, 9, 50, 00);
            start = new DateTimeOffset(date.Year, date.Month, date.Day, startTime.Hour, startTime.Minute, startTime.Second, offset);
            end = new DateTimeOffset(date.Year, date.Month, date.Day, endTime.Hour, endTime.Minute, endTime.Second, offset);
            buildings = new List<string> { "BRAN", "CORN" };
            rooms = new List<string> { "101", "549" };

            cal1 = new Event(id, description, type, location, pointer, startTime, endTime);
        }

        [TestClass]
        public class Event_Constructor : EventTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.Main_Initialize();
                cal2 = new Event(id, description, type, null, pointer, startTime, endTime);
            }

            [TestMethod]
            public void Event_Constructor_Id()
            {
                Assert.AreEqual(id, cal1.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void Event_Constructor_IdChange()
            {
                cal1 = new Event(id, description, type, location, pointer, startTime, endTime);
                cal1.Id = "abcdef";
            }

            [TestMethod]
            public void Event_Constructor_Description()
            {
                Assert.AreEqual(description, cal1.Description);
            }

            [TestMethod]
            public void Event_Constructor_Type()
            {
                Assert.AreEqual(type, cal1.Type);
            }

            [TestMethod]
            public void Event_Constructor_LocationCode()
            {
                Assert.AreEqual(location, cal1.LocationCode);
            }

            [TestMethod]
            public void Event_Constructor_NullLocationCode()
            {
                Assert.AreEqual(null, cal2.LocationCode);
            }

            [TestMethod]
            public void Event_Constructor_Location_NoRooms()
            {
                Assert.AreEqual("Location: " + location, cal1.Location);
            }

            [TestMethod]
            public void Event_Constructor_Pointer()
            {
                Assert.AreEqual(pointer, cal1.Pointer);
            }

            [TestMethod]
            public void Event_Constructor_Start()
            {
                Assert.AreEqual(start, cal1.Start);
            }

            [TestMethod]
            public void Event_Constructor_End()
            {
                Assert.AreEqual(end, cal1.End);
            }

            [TestMethod]
            public void Event_Constructor_StartTime()
            {
                Assert.AreEqual(start, cal1.StartTime);
            }

            [TestMethod]
            public void Event_Constructor_EndTime()
            {
                Assert.AreEqual(end, cal1.EndTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_Constructor_ThrowsExceptionIfTypeNull()
            {
                new Event(id, description, null, location, pointer, startTime, endTime);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_Constructor_ThrowsExceptionIfTypeEmpty()
            {
                new Event(id, description, "", location, pointer, startTime, endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_Constructor_ThrowsExceptionIfPointerNull()
            {
                new Event(id, description, type, location, null, startTime, endTime);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_Constructor_ThrowsExceptionIfPointerEmpty()
            {
                new Event(id, description, type, location, "", startTime, endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_Constructor_ThrowsExceptionIfEndBeforeStart()
            {
                new Event(id, description, type, location, pointer, endTime, startTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_Constructor_DefaultStartTime()
            {
                cal1 = new Event(id, description, type, location, pointer, default(DateTimeOffset), endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_Constructor_DefaultEndTime()
            {
                cal1 = new Event(id, description, type, location, pointer, startTime, default(DateTimeOffset));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_Constructor_ThrowsExceptionIfStartDateIsMinValue()
            {
                var minDateWithAnyTime = new DateTimeOffset(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, offset);
                new Event(id, description, type, location, pointer, minDateWithAnyTime, endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_Constructor_ThrowsExceptionIfEndDateIsMinValue()
            {
                var minDateWithAnyTime = new DateTimeOffset(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, offset);
                new Event(id, description, type, location, pointer, startTime, minDateWithAnyTime);
            }

        }

        [TestClass]
        public class Event_Id : EventTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.Main_Initialize();
                cal2 = new Event(null, description, type, null, pointer, startTime, endTime);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void Event_Id_ChangeAfterSetInConstructor()
            {
                cal1.Id = "1";
            }

            [TestMethod]
            public void Event_Id_Assignment()
            {
                cal2.Id = "1";
                Assert.AreEqual("1", cal2.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void Event_Id_ChangeAfterAssignment()
            {
                cal2.Id = "1";
                cal2.Id = "2";
            }
        }

        [TestClass]
        public class Event_AddRoom : EventTests
        {
            private List<string> roomIds = new List<string>();

            [TestInitialize]
            public void Initialize()
            {
                base.Main_Initialize();
                cal2 = new Event(id, description, type, null, pointer, startTime, endTime);
                for (int i = 0; i < buildings.Count; i++)
                {
                    roomIds.Add(buildings[i] + "*" + rooms[i]);
                    cal1.AddRoom(roomIds[i]);
                    cal2.AddRoom(roomIds[i]);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_AddRoom_NullPersonId()
            {
                cal1.AddRoom(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_AddRoom_EmptyPersonId()
            {
                cal1.AddRoom(string.Empty);
            }

            [TestMethod]
            public void Event_AddRoom_RoomIds()
            {
                Assert.AreEqual(roomIds.Count, cal1.RoomIds.Count);
                CollectionAssert.AreEqual(roomIds, cal1.RoomIds.ToList());
            }

            [TestMethod]
            public void Event_AddRoom_Buildings()
            {
                Assert.AreEqual(buildings.Count, cal1.Buildings.Count);
                CollectionAssert.AreEqual(buildings, cal1.Buildings.ToList());
            }

            [TestMethod]
            public void Event_AddRoom_Rooms()
            {
                Assert.AreEqual(rooms.Count, cal1.Rooms.Count);
                CollectionAssert.AreEqual(rooms, cal1.Rooms.ToList());
            }

            [TestMethod]
            public void Event_AddRoom_DuplicateRoomId()
            {
                cal1.AddRoom(roomIds[0]);
                Assert.AreEqual(roomIds.Count, cal1.RoomIds.Count);
                CollectionAssert.AreEqual(roomIds, cal1.RoomIds.ToList());
            }

            [TestMethod]
            public void Event_AddRoom_Location_SingleRoom()
            {
                var cal3 = new Event(id, description, type, location, pointer, startTime, endTime);
                cal3.AddRoom(roomIds[0]);
                Assert.AreEqual("Location: " + location + ", Building:" + buildings[0] + ", Room:" + rooms[0], cal3.Location);
            }

            [TestMethod]
            public void Event_AddRoom_Location_SingleRoom_NoLocation()
            {
                var cal3 = new Event(id, description, type, null, pointer, startTime, endTime);
                cal3.AddRoom(roomIds[0]);
                Assert.AreEqual("Building:" + buildings[0] + ", Room:" + rooms[0], cal3.Location);
            }

            [TestMethod]
            public void Event_AddRoom_Location_DoubleRoom()
            {
                Assert.AreEqual("Location: " + location + ", Building:" + buildings[0] + ", Room:" + rooms[0] + ", Building:" + buildings[1] + ", Room:" + rooms[1], cal1.Location);
            }

            [TestMethod]
            public void Event_AddRoom_Location_DoubleRoom_NoLocation()
            {
                var cal3 = new Event(id, description, type, null, pointer, startTime, endTime);
                cal3.AddRoom(roomIds[0]);
                cal3.AddRoom(roomIds[1]);
                Assert.AreEqual("Building:" + buildings[0] + ", Room:" + rooms[0] + ", Building:" + buildings[1] + ", Room:" + rooms[1], cal3.Location);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_AddRoom_InvalidRoom_NoAsterisk()
            {
                cal1.AddRoom("304");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_AddRoom_InvalidRoom_NoBuilding()
            {
                cal1.AddRoom("*304");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_AddRoom_InvalidRoom_NoRoom()
            {
                cal1.AddRoom("BLDG*");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_AddRoom_InvalidRoom_TooManyParts()
            {
                cal1.AddRoom("BLDG*304*");
            }
        }

        [TestClass]
        public class Event_AddPerson : EventTests
        {
            private List<string> personIds = new List<string>() { "1234567", "8901234", "5678901" };

            [TestInitialize]
            public void Initialize()
            {
                base.Main_Initialize();
                for (int i = 0; i < personIds.Count; i++)
                {
                    cal1.AddPerson(personIds[i]);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_AddPerson_NullPersonId()
            {
                cal1.AddPerson(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void Event_AddPerson_EmptyPersonId()
            {
                cal1.AddPerson(string.Empty);
            }

            [TestMethod]
            public void Event_AddPerson_PersonIds()
            {
                Assert.AreEqual(personIds.Count, cal1.PersonIds.Count);
                CollectionAssert.AreEqual(personIds, cal1.PersonIds.ToList());
            }

            [TestMethod]
            public void Event_AddPerson_DuplicatePersonId()
            {
                cal1.AddPerson(personIds[0]);
                Assert.AreEqual(personIds.Count, cal1.PersonIds.Count);
                CollectionAssert.AreEqual(personIds, cal1.PersonIds.ToList());
            }
        }
    }
}
