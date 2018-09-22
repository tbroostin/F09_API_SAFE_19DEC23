// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class CountyTests
    {
        private string guid;
        private string code;
        private string description;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "FFX";
            description = "Fairfax";
        }

        [TestMethod]
        public void CountyConstructorTest()
        {
            var county = new County(guid, code, description);
            Assert.AreEqual(code, county.Code);
            Assert.AreEqual(description, county.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CountyConstructorNullCodeTest()
        {
            new County(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CountyConstructorNullDescriptionTest()
        {
            new County(guid, code, null);
        }
    }
}
