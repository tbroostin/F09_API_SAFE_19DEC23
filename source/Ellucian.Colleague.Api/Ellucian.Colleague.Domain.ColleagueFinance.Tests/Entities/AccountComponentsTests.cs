//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class ShippingMethodsTests
    {
        private string guid;
        private string code;
        private string description;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "T";
            description = "Tests";
        }

        [TestMethod]
        public void ShippingMethodsConstructorTest()
        {
            var entity = new ShippingMethod(guid, code, description);
            Assert.AreEqual(code, entity.Code);
            Assert.AreEqual(description, entity.Description);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShippingMethodsConstructorNullCodeTest()
        {
            new ShippingMethod(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ShippingMethodsConstructorNullDescriptionTest()
        {
            new ShippingMethod(guid, code, null);
        }
    }
}