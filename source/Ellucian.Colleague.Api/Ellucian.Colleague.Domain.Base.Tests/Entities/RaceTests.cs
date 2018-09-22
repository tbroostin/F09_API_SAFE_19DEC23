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
    public class RaceTests
    {
        private string guid;
        private string code;
        private string description;
        private RaceType type;
        private Race race;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "AS";
            description = "Asian";
            type = RaceType.Asian;
        }

        [TestClass]
        public class RaceConstructor : RaceTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RaceConstructorNullGuid()
            {
                race = new Race(null, code, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RaceConstructorEmptyGuid()
            {
                race = new Race(string.Empty, code, description, type);
            }

            [TestMethod]
            public void RaceConstructorValidGuid()
            {
                race = new Race(guid, code, description, type);
                Assert.AreEqual(guid, race.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RaceConstructorNullCode()
            {
                race = new Race(guid, null, description, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RaceConstructorEmptyCode()
            {
                race = new Race(guid, string.Empty, description, type);
            }

            [TestMethod]
            public void RaceConstructorValidCode()
            {
                race = new Race(guid, code, description, type);
                Assert.AreEqual(code, race.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RaceConstructorNullDescription()
            {
                race = new Race(guid, code, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RaceConstructorEmptyDescription()
            {
                race = new Race(guid, code, string.Empty, type);
            }

            [TestMethod]
            public void RaceConstructorValidDescription()
            {
                race = new Race(guid, code, description, type);
                Assert.AreEqual(description, race.Description);
            }

            [TestMethod]
            public void RaceConstructorValidType()
            {
                race = new Race(guid, code, description, type);
                Assert.AreEqual(type, race.Type);
            }
        }
    }
}
