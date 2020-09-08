// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.HumanResources.Entities;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Entities
{
    [TestClass]
    public class BenefitsEnrollmentConfigurationTests
    {
        public BenefitsEnrollmentConfiguration benefitsEnrollmentConfiguration;
        public List<string> relationshipTypes;

        [TestInitialize]
        public void Initialize()
        {
            relationshipTypes = new List<string>() { "A", "B" };
        }

        public BenefitsEnrollmentConfiguration CreateConfiguration()
        {
            return new BenefitsEnrollmentConfiguration() { RelationshipTypes = relationshipTypes };
        }

        [TestMethod]
        public void ConstructorTest()
        {
            benefitsEnrollmentConfiguration = CreateConfiguration();
            Assert.AreEqual(2, benefitsEnrollmentConfiguration.RelationshipTypes.Count);
        }

    }
}
