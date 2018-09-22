// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;

namespace Ellucian.Colleague.Domain.ColleagueFinance.Tests.Entities
{
    [TestClass]
    public class CommodityCodeTests
    {
        private string guid;
        private string code;
        private string description;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "CODE";
            description = "Description";
        }

        [TestMethod]
        public void CommodityCodeConstructorTest()
        {
            var county = new CommodityCode(guid, code, description);
            Assert.AreEqual(code, county.Code);
            Assert.AreEqual(description, county.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommodityCodeConstructorNullCodeTest()
        {
            new CommodityCode(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommodityCodeConstructorNullDescriptionTest()
        {
            new CommodityCode(guid, code, null);
        }
    }
}
