// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class StateTests
    {
        private string code;
        private string description;
        private string country;

        [TestInitialize]
        public void Initialize()
        {
            code = "VA";
            description = "Virginia";
            country = "USA";
        }

        [TestMethod]
        public void StateConstructorTest()
        {
            var state = new State(code, description);
            Assert.AreEqual(code, state.Code);
            Assert.AreEqual(description, state.Description);
            Assert.IsNull(state.CountryCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StateConstructorNullCodeTest()
        {
            new State(null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StateConstructorNullDescriptionTest()
        {
            new State(code, null);
        }

        public void StateConstructorTestWithCountry()
        {
            var state = new State(code, description, country);
            Assert.AreEqual(code, state.Code);
            Assert.AreEqual(description, state.Description);
            Assert.AreEqual(country, state.CountryCode);
        }
    }
}
