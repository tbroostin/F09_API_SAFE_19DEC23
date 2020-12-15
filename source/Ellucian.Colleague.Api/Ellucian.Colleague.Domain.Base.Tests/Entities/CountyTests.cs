// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            Assert.AreEqual(guid, county.Guid);
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
