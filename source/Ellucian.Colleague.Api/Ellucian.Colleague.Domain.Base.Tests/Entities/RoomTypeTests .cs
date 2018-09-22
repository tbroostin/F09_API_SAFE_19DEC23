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
    public class RoomTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private RoomType type;
        private RoomTypes roomType;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "110";
            description = "Lecture Hall";
            type = RoomType.Classroom;
        }

        [TestClass]
        public class RoomTypeConstructor : RoomTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomTypeConstructorNullGuid()
            {
                roomType = new RoomTypes(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomTypeConstructorEmptyGuid()
            {
                roomType = new RoomTypes(string.Empty, code, description, type);
            }

            [TestMethod]
            public void RoomTypeConstructorValidGuid()
            {
                roomType = new RoomTypes(guid, code, description, type);
                Assert.AreEqual(guid, roomType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomTypeConstructorNullCode()
            {
                roomType = new RoomTypes(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomTypeConstructorEmptyCode()
            {
                roomType = new RoomTypes(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void RoomTypeConstructorValidCode()
            {
                roomType = new RoomTypes(guid, code, description, type);
                Assert.AreEqual(code, roomType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomTypeConstructorNullDescription()
            {
                roomType = new RoomTypes(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RoomTypeConstructorEmptyDescription()
            {
                roomType = new RoomTypes(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void RoomTypeConstructorValidDescription()
            {
                roomType = new RoomTypes(guid, code, description, type);
                Assert.AreEqual(description, roomType.Description);
            }

            [TestMethod]
            public void RoomTypeConstructorValidType()
            {
                roomType = new RoomTypes(guid, code, description, type);
                Assert.AreEqual(type, roomType.Type);
            }
        }
    }
}
