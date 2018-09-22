using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RoomTests
    {
        static string guid;
        static string id;
        static string code;
        static string buildingCode;
        static string description;
        static Room room;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString().ToLowerInvariant();
            id = "ABC4*A123";
            code = "A123";
            buildingCode = "ABC4";
            description = "ABC4 Room";

            room = new Room(guid, id, description);
        }

        [TestClass]
        public class RoomConstructor : RoomTests
        {
            [TestMethod]
            public void RoomGuid()
            {
                Assert.AreEqual(guid, room.Guid);
            }

            [TestMethod]
            public void RoomId()
            {
                Assert.AreEqual(id, room.Id);
            }

            [TestMethod]
            public void Description()
            {
                Assert.AreEqual(description, room.Description);
            }

            [TestMethod]
            public void RoomCode()
            {
                Assert.AreEqual(code, room.Code);
            }

            [TestMethod]
            public void RoomNumber()
            {
                Assert.AreEqual(code, room.Number);
            }

            [TestMethod]
            public void BuildingCode()
            {
                Assert.AreEqual(buildingCode, room.BuildingCode);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GuidNullException()
            {
                room = new Room(null, id, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GuidEmptyException()
            {
                room = new Room(string.Empty, id, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomCodeNullException()
            {
                room = new Room(guid, null, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomCodeEmptyException()
            {
                room = new Room(guid, string.Empty, description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_AddRoom_InvalidRoom_NoAsterisk()
            {
                room = new Room(guid, "304", description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_AddRoom_InvalidRoom_NoBuilding()
            {
                room = new Room(guid, "*304", description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_AddRoom_InvalidRoom_NoRoom()
            {
                room = new Room(guid, "BLDG*", description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void Event_AddRoom_InvalidRoom_TooManyParts()
            {
                room = new Room(guid, "BLDG*304*", description);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomDescriptionNullException()
            {
                room = new Room(guid, id, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomDescriptionEmptyException()
            {
                room = new Room(guid, id, string.Empty);
            }
        }

        [TestClass]
        public class RoomEquals : RoomTests
        {
            [TestMethod]
            public void RoomEquals_Null_False()
            {
                Assert.IsFalse(room.Equals(null));
            }

            [TestMethod]
            public void RoomEquals_NonRoomObject_False()
            {
                Assert.IsFalse(room.Equals("abcd"));
            }

            [TestMethod]
            public void RoomEquals_SameCodeAndBuilding_True()
            {
                var room2 = new Room(Guid.NewGuid().ToString(), id, description); 
                Assert.IsTrue(room.Equals(room2));
            }

            [TestMethod]
            public void RoomEquals_DifferentCodeAndBuilding_False()
            {
                var id2 = "ABC5*A124";
                var room2 = new Room(Guid.NewGuid().ToString(), id2, description);
                Assert.IsFalse(room.Equals(room2));
            }

            [TestMethod]
            public void RoomEquals_SameCodeAndDifferentBuilding_False()
            {
                var id2 = "ABC4*A124";
                var room2 = new Room(Guid.NewGuid().ToString(), id2, description);
                Assert.IsFalse(room.Equals(room2));
            }

            [TestMethod]
            public void RoomEquals_DifferentCodeAndSameBuilding_False()
            {
                var code2 = "ABC4*A125";
                var room2 = new Room(Guid.NewGuid().ToString(), code2, description);
                Assert.IsFalse(room.Equals(room2));
            }
        }

        [TestClass]
        public class RoomGetHashCode : RoomTests
        {
            [TestMethod]
            public void RoomGetHashCode_Equals()
            {
                var room2 = new Room(Guid.NewGuid().ToString(), id, description);
                Assert.AreEqual(room.GetHashCode(), room2.GetHashCode());
            }

            [TestMethod]
            public void RoomGetHashCode_NotEquals()
            {
                var id2 = "ABC4*A125";
                var room2 = new Room(Guid.NewGuid().ToString(), id2, description);
                Assert.AreNotEqual(room.GetHashCode(), room2.GetHashCode());
            }
        }
    }
}
