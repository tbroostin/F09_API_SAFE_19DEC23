//Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class VendorTermTests
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
        public void VendorTermConstructorTest()
        {
            var entity = new VendorTerm(guid, code, description);
            Assert.AreEqual(code, entity.Code);
            Assert.AreEqual(description, entity.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VendorTermConstructorNullCodeTest()
        {
            new VendorTerm(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VendorTermConstructorNullDescriptionTest()
        {
            new VendorTerm(guid, code, null);
        }
    }
}
