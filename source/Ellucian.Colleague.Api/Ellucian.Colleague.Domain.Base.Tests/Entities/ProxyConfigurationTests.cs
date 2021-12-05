// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProxyConfigurationTests
    {
        private bool isEnabled;
        private string disclosureDocumentId;
        private string emailDocumentId;
        private bool canAddOtherUsers;
        private ProxyConfiguration config;
        private List<ProxyAndUserPermissionsMap> proxyAndUserPermissionsMap; 

        [TestInitialize]
        public void ProxyConfigurationTests_Initialize()
        {
            isEnabled = true;
            disclosureDocumentId = "PRX.PARA";
            emailDocumentId = "MAIL.PARA";
            canAddOtherUsers = true;
            proxyAndUserPermissionsMap = new List<ProxyAndUserPermissionsMap>()
            {
                new ProxyAndUserPermissionsMap("10", "APPROVE.REJECT.LEAVE.REQUEST", "TMLA"),
                new ProxyAndUserPermissionsMap("11", "APPROVE.REJECT.TIME.ENTRY", "TMTA")
            };
        }

        [TestCleanup]
        public void ProxyConfigurationTests_Cleanup()
        {
            isEnabled = false;
            disclosureDocumentId = null;
            emailDocumentId = null;
            config = null;
        }

        [TestClass]
        public class ProxyConfigurationTests_Constructor : ProxyConfigurationTests
        {
            [TestMethod]
            public void ProxyConfigurationTests_Constructor_IsEnabled()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                Assert.IsTrue(config.ProxyIsEnabled);
            }

            [TestMethod]
            public void ProxyConfigurationTests_Constructor_DisclosureDocumentId()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                Assert.AreEqual(disclosureDocumentId, config.DisclosureReleaseDocumentId);
            }

            [TestMethod]
            public void ProxyConfigurationTests_Constructor_DisclosureReleaseText()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                Assert.IsNotNull(config.DisclosureReleaseText);
                Assert.AreEqual(string.Empty, config.DisclosureReleaseText);
            }

            [TestMethod]
            public void ProxyConfigurationTests_Constructor_EmailDocumentId()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                Assert.AreEqual(emailDocumentId, config.ProxyEmailDocumentId);
            }

            [TestMethod]
            public void ProxyConfigurationTests_Constructor_WorkflowGroups()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                Assert.AreEqual(0, config.WorkflowGroups.Count);
            }

            [TestMethod]
            public void ProxyConfigurationTests_Constructor_CanAddOtherUsers()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                Assert.AreEqual(canAddOtherUsers, config.CanAddOtherUsers);
            }

            [TestMethod]
            public void ProxyConfigurationTests_Constructor_ProxyAndUserPermissionsMap()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                CollectionAssert.AreEqual(proxyAndUserPermissionsMap.ToList(), config.ProxyAndUserPermissionsMap.ToList());
            }
        }

        [TestClass]
        public class ProxyConfigurationTests_AddWorkflowGroup : ProxyConfigurationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProxyConfigurationTests_AddWorkflowGroup_NullWorkflowGroup()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddWorkflowGroup(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ProxyConfigurationTests_AddWorkflowGroup_EmptyWorkflowGroup()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddWorkflowGroup(new ProxyWorkflowGroup("SF", "Student Finance"));
            }

            [TestMethod]
            public void ProxyConfigurationTests_AddWorkflowGroup_Valid()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                var group = new ProxyWorkflowGroup("SF", "Student Finance");
                group.AddWorkflow(new ProxyWorkflow("SFAA", "Student Finance Account Activity", "SF", true));
                config.AddWorkflowGroup(group);
                Assert.AreEqual(1, config.WorkflowGroups.Count);
            }

            [TestMethod]
            public void ProxyConfigurationTests_AddWorkflowGroup_DuplicateWorkflow()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                var group = new ProxyWorkflowGroup("SF", "Student Finance");
                group.AddWorkflow(new ProxyWorkflow("SFAA", "Student Finance Account Activity", "SF", true));
                config.AddWorkflowGroup(group);
                config.AddWorkflowGroup(group);
                Assert.AreEqual(1, config.WorkflowGroups.Count);
            }
        }

        [TestClass]
        public class ProxyConfigurationTests_AddRelationshipTypeCode : ProxyConfigurationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProxyConfigurationTests_AddRelationshipTypeCode_NullCode()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddRelationshipTypeCode(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProxyConfigurationTests_AddRelationshipTypeCode_EmptyCode()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddRelationshipTypeCode(string.Empty);
            }

            [TestMethod]
            public void ProxyConfigurationTests_AddRelationshipTypeCode_Valid()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddRelationshipTypeCode("PAR");
                Assert.AreEqual(1, config.RelationshipTypeCodes.Count);
            }

            [TestMethod]
            public void ProxyConfigurationTests_AddRelationshipTypeCode_DuplicateWorkflow()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddRelationshipTypeCode("PAR");
                config.AddRelationshipTypeCode("PAR");
                Assert.AreEqual(1, config.RelationshipTypeCodes.Count);
            }
        }

        [TestClass]
        public class ProxyConfigurationTests_AddDemographicField : ProxyConfigurationTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ProxyConfigurationTests_AddDemographicField_NullField()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddDemographicField(null);
            }

            [TestMethod]
            public void ProxyConfigurationTests_AddDemographicField_Valid()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddDemographicField(new DemographicField("code", "desc", DemographicFieldRequirement.Required));
                Assert.AreEqual(1, config.DemographicFields.Count);
            }

            [TestMethod]
            public void ProxyConfigurationTests_AddRelationshipTypeCode_DuplicateWorkflow()
            {
                config = new ProxyConfiguration(isEnabled, disclosureDocumentId, emailDocumentId, canAddOtherUsers, true, proxyAndUserPermissionsMap);
                config.AddDemographicField(new DemographicField("code", "desc", DemographicFieldRequirement.Required));
                config.AddDemographicField(new DemographicField("code", "desc2", DemographicFieldRequirement.Hidden));
                Assert.AreEqual(1, config.DemographicFields.Count);
            }
        }
    }
}
