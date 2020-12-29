/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Domain.Student.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Tests.Entities
{
    [TestClass]
    public class CampusOrganization2Tests
    {
        public string campusOrganizationId;
        public string campusOrganizationDescription;

        public CampusOrganization2 campusOrg2;

        public void CampusOrganization2TestInitialize()
        {
            campusOrganizationId = "CYC";
            campusOrganizationDescription = "Cycling Club";
        }

        [TestClass]
        public class CampusOrganization2ConstructorTests : CampusOrganization2Tests
        {
            public new CampusOrganization2 campusOrg2
            {
                get
                {
                    return new CampusOrganization2(campusOrganizationId, campusOrganizationDescription);
                }
            }

            [TestInitialize]
            public void Initialize()
            {
                CampusOrganization2TestInitialize();
            }

            [TestMethod]
            public void CampusOrganizationIdTest()
            {
                Assert.AreEqual(campusOrganizationId, campusOrg2.CampusOrganizationId);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusOrganizationIdRequiredTest()
            {
                campusOrganizationId = "";
                var error = campusOrg2;
            }

            [TestMethod]
            public void CampusOrganizationDescriptionTest()
            {
                Assert.AreEqual(campusOrganizationDescription, campusOrg2.CampusOrganizationDescription);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CampusOrganizationDescriptionRequiredTest()
            {
                campusOrganizationDescription = "";
                var error = campusOrg2;
            }
        }

        [TestClass]
        public class AttributeTests : CampusOrganization2Tests
        {
            [TestInitialize]
            public void Initialize()
            {
                CampusOrganization2TestInitialize();
                campusOrg2 = new CampusOrganization2(campusOrganizationId, campusOrganizationDescription);
            }

            [TestMethod]
            public void CampusOrganizationIdTest()
            {
                var campusOrganizationId = "CYC";
                Assert.AreEqual(campusOrganizationId, campusOrg2.CampusOrganizationId);
            }

            [TestMethod]
            public void CampusOrganizationDescriptionTest()
            {
                var campusOrganizationDescription = "Cycling Club";
                Assert.AreEqual(campusOrganizationDescription, campusOrg2.CampusOrganizationDescription);
            }
        }
    }
}
