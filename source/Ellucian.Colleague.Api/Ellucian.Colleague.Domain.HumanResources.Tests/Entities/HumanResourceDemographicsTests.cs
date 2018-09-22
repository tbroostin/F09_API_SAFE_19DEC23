/* Copyright 2017 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class HumanResourceDemographicsTests
    {
        public string id;
        public string firstName;
        public string lastName;
        public string preferredName;
        public HumanResourceDemographics hrDemo;

        [TestInitialize]
        public void Initialize()
        {
            id = "001";
            firstName = "Foo";
            lastName = "Bar";
            preferredName = "Nickfoo";
        }

        [TestMethod]
        public void PropertiesAreSet()
        {
            hrDemo = new HumanResourceDemographics(id, firstName, lastName, preferredName);
            Assert.AreEqual(id, hrDemo.Id);
            Assert.AreEqual(firstName, hrDemo.FirstName);
            Assert.AreEqual(lastName, hrDemo.LastName);
            Assert.AreEqual(preferredName, hrDemo.PreferredName);
        }
    }
}
