// Copyright 2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class InstitutionEmployerTests
    {
        private string guid;
        private string employerId;
        private string preferredName;

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            employerId = "0000043";            
            preferredName = "Ellucian University";
        }

        [TestMethod]
        public void InstitutionEmployerConstructorTest()
        {
            var institutionEmployer = new InstitutionEmployers(guid, employerId);
            Assert.AreEqual(guid, institutionEmployer.Guid);
            Assert.AreEqual(employerId, institutionEmployer.EmployerId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstitutionEmployerNullGuidTest()
        {
            new InstitutionEmployers(null, employerId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InstitutionEmployerNullEmployerIdTest()
        {
            new InstitutionEmployers(guid, null);
        }
    }
}
