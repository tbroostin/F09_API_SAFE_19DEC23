// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class RehireTypeTests
    {
        private string guid;
        private string code;
        private string description;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "code";
            description = "description";
        }

        [TestMethod]
        public void RehireTypeConstructorTest()
        {
            var rehireType = new RehireType(guid, code, description, "E");
            Assert.AreEqual(code, rehireType.Code);
            Assert.AreEqual(description, rehireType.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RehireTypeConstructorNullCodeTest()
        {
            new RehireType(guid, null, description, "E");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RehireTypeConstructorNullDescriptionTest()
        {
            new RehireType(guid, code, null, "E");
        }
    }
}
