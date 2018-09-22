// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProxyWorkflowGroupTests
    {
        private string code;
        private string desc;

        [TestInitialize]
        public void Initialize()
        {
            code = "SF";
            desc = "Student Finance";
        }

        [TestCleanup]
        public void Cleanup()
        {
            code = null;
            desc = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyWorkflowGroup_Constructor_NullCode()
        {
            var entity = new ProxyWorkflowGroup(null, desc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyWorkflowGroup_Constructor_EmptyCode()
        {
            var entity = new ProxyWorkflowGroup(string.Empty, desc);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyWorkflowGroup_Constructor_NullDescription()
        {
            var entity = new ProxyWorkflowGroup(code, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyWorkflowGroup_Constructor_EmptyDescription()
        {
            var entity = new ProxyWorkflowGroup(code, string.Empty);
        }

        [TestMethod]
        public void ProxyWorkflowGroup_Code()
        {
            var entity = new ProxyWorkflowGroup(code, desc);
            Assert.AreEqual(code, entity.Code);
        }

        [TestMethod]
        public void ProxyWorkflowGroup_Description()
        {
            var entity = new ProxyWorkflowGroup(code, desc);
            Assert.AreEqual(desc, entity.Description);
        }

        [TestMethod]
        public void ProxyWorkflowGroup_Workflows()
        {
            var entity = new ProxyWorkflowGroup(code, desc);
            Assert.AreEqual(0, entity.Workflows.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyWorkflowGroup_AddWorkflow_NullWorkflow()
        {
            var entity = new ProxyWorkflowGroup(code, desc);
            entity.AddWorkflow(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProxyWorkflowGroup_AddWorkflow_WorkflowGroupIdMismatch()
        {
            var entity = new ProxyWorkflowGroup(code, desc);
            entity.AddWorkflow(new ProxyWorkflow("SFAA", "Student Finance Account Activity", code + "A", true));
        }

        [TestMethod]
        public void ProxyConfigurationTests_AddWorkflowGroup_Valid()
        {
            var entity = new ProxyWorkflowGroup(code, desc);
            entity.AddWorkflow(new ProxyWorkflow("SFAA", "Student Finance Account Activity", code, true));
            Assert.AreEqual(1, entity.Workflows.Count);
        }

        [TestMethod]
        public void ProxyConfigurationTests_AddWorkflowGroup_DuplicateWorkflow()
        {
            var entity = new ProxyWorkflowGroup(code, desc);
            entity.AddWorkflow(new ProxyWorkflow("SFAA", "Student Finance Account Activity", code, true));
            entity.AddWorkflow(new ProxyWorkflow("SFAA", "Student Finance Account Activity2", code, true));
            Assert.AreEqual(1, entity.Workflows.Count);
        }
    }
}
