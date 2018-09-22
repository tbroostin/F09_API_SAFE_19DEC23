// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class WorkTaskTests
    {
        private string workTaskId;
        private string category;
        private string description;
        private string processCode;

        [TestInitialize]
        public void Initialize()
        {
            workTaskId = "101";
            category = "Time Approval";
            description = "Approve Time Entry for Susan Brown";
            processCode = "SSHRTA";
        }

        [TestMethod]
        public void WorkTask_Id()
        {
            var workTask = new WorkTask(workTaskId, category, description, processCode);
            Assert.AreEqual(workTaskId, workTask.Id);            
        }

        [TestMethod]
        public void WorkTask_Category()
        {
            var workTask = new WorkTask(workTaskId, category, description, processCode);
            Assert.AreEqual(category, workTask.Category);
        }

        [TestMethod]
        public void WorkTask_Description()
        {
            var workTask = new WorkTask(workTaskId, category, description, processCode);
            Assert.AreEqual(description, workTask.Description);
        }

        [TestMethod]
        public void WorkTask_ProcessCode()
        {
            var workTask = new WorkTask(workTaskId, category, description, processCode);
            Assert.AreEqual(processCode, workTask.ProcessCode);
        }

    }
}
