// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class CommerceTaxCodeTests
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
        public void CommerceTaxCodeConstructorTest()
        {
            var commerceTaxCode = new CommerceTaxCode(guid, code, description);
            Assert.AreEqual(code, commerceTaxCode.Code);
            Assert.AreEqual(description, commerceTaxCode.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommerceTaxCodeConstructorNullCodeTest()
        {
            new CommerceTaxCode(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommerceTaxCodeConstructorNullDescriptionTest()
        {
            new CommerceTaxCode(guid, code, null);
        }
    }
}
