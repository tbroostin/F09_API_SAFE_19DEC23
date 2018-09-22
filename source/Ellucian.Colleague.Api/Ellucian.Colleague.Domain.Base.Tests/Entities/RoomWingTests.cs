// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class RoomWingTests
    {
        private string _guid;
        private string _code;
        private string _description;

        private RoomWing _roomWing;

        [TestInitialize]
        public void Initialize()
        {
            _guid = Guid.NewGuid().ToString();
            _code = "N";
            _description = "North";
        }

        [TestClass]
        public class RoomWingConstructor : RoomWingTests
        {
            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void RoomWingConstructorNullGuid()
            {
                _roomWing = new RoomWing(null, _code, _description);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void RoomWingConstructorEmptyGuid()
            {
                _roomWing = new RoomWing(string.Empty, _code, _description);
            }

            [TestMethod]
            public void RoomWingConstructorValidGuid()
            {
                _roomWing = new RoomWing(_guid, _code, _description);
                Assert.AreEqual(_guid, _roomWing.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void RoomWingConstructorNullCode()
            {
                _roomWing = new RoomWing(_guid, null, _description);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void RoomWingConstructorEmptyCode()
            {
                _roomWing = new RoomWing(_guid, string.Empty, _description);
            }

            [TestMethod]
            public void RoomWingConstructorValidCode()
            {
                _roomWing = new RoomWing(_guid, _code, _description);
                Assert.AreEqual(_code, _roomWing.Code);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void RoomWingConstructorNullDescription()
            {
                _roomWing = new RoomWing(_guid, _code, null);
            }

            [TestMethod]
            [ExpectedException(typeof (ArgumentNullException))]
            public void RoomWingConstructorEmptyDescription()
            {
                _roomWing = new RoomWing(_guid, _code, string.Empty);
            }

            [TestMethod]
            public void RoomWingConstructorValidDescription()
            {
                _roomWing = new RoomWing(_guid, _code, _description);
                Assert.AreEqual(_description, _roomWing.Description);
            }
        }
    }
}