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
    public class EmploymentProficiencyTests
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
        public void EmploymentProficiencyConstructorTest()
        {
            var rehireType = new EmploymentProficiency(guid, code, description);
            Assert.AreEqual(code, rehireType.Code);
            Assert.AreEqual(description, rehireType.Description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmploymentProficiencyConstructorNullCodeTest()
        {
            new EmploymentProficiency(guid, null, description);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmploymentProficiencyConstructorNullDescriptionTest()
        {
            new EmploymentProficiency(guid, code, null);
        }
    }
}
