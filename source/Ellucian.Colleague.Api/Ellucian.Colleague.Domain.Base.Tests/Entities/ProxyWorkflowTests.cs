// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ProxyWorkflowTests
    {
        private string code;
        private string desc;
        private string groupId;
        private bool isEnabled;

        [TestInitialize]
        public void Initialize()
        {
            code = "SFAA";
            desc = "Student Finance Account Activity";
            groupId = "SF";
            isEnabled = true;
        }

        [TestCleanup]
        public void Cleanup()
        {
            code = null;
            desc = null;
            groupId = null;
            isEnabled = false;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyWorkflow_Constructor_NullGroupId()
        {
            var entity = new ProxyWorkflow(code, desc, null, isEnabled);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProxyWorkflow_Constructor_EmptyGroupId()
        {
            var entity = new ProxyWorkflow(code, desc, string.Empty, isEnabled);
        }

        [TestMethod]
        public void ProxyWorkflow_WorkflowGroupId()
        {
            var entity = new ProxyWorkflow(code, desc, groupId, isEnabled);
            Assert.AreEqual(groupId, entity.WorkflowGroupId);
        }

        [TestMethod]
        public void ProxyWorkflow_IsEnabled()
        {
            var entity = new ProxyWorkflow(code, desc, groupId, isEnabled);
            Assert.AreEqual(isEnabled, entity.IsEnabled);
        }
    }
}
