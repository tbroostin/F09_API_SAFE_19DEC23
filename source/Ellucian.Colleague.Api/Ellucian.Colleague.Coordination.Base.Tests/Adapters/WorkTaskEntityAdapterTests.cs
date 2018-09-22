// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Dtos.Base;

namespace Ellucian.Colleague.Coordination.Base.Tests.Adapters
{
    [TestClass]
    public class WorkTaskEntityAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        public WorkTaskEntityAdapter workTaskEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();

            workTaskEntityAdapter = new WorkTaskEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void WorkTaskEntityAdapter_MapsEntityFieldsCorrectly()
        {
            var workTaskEntity = new Domain.Base.Entities.WorkTask("80001", "Time Approval", "Time approval description and details", "SSHRTA");
            var workTaskDto = workTaskEntityAdapter.MapToType(workTaskEntity);
            Assert.AreEqual(workTaskDto.Category, workTaskEntity.Category);
            Assert.AreEqual(workTaskDto.Description, workTaskEntity.Description);
            Assert.AreEqual(workTaskDto.Id, workTaskEntity.Id);
        }

        [TestMethod]
        public void WorkTaskEntityAdapter_MapsNoProcessCode()
        {
            var workTaskEntity = new Domain.Base.Entities.WorkTask("80002", "No Category", "No description", "");
            var workTaskDto = workTaskEntityAdapter.MapToType(workTaskEntity);
            Assert.AreEqual(workTaskDto.TaskProcess, WorkTaskProcess.None);
        }

        [TestMethod]
        public void WorkTaskEntityAdapter_MapsLeaveRequestApprovalTaskProcessCode()
        {
            var workTaskEntity = new Domain.Base.Entities.WorkTask("80003", "Leave Request Approval", "Leave request approval description and details", "SSHRLVA");
            var workTaskDto = workTaskEntityAdapter.MapToType(workTaskEntity);
            Assert.AreEqual(workTaskDto.TaskProcess, WorkTaskProcess.LeaveRequestApproval);
        }

        [TestMethod]
        public void WorkTaskEntityAdapter_MapsTimeEntryApprovalTaskProcessCode()
        {
            var workTaskEntity = new Domain.Base.Entities.WorkTask("80004", "Time Approval", "Time approval description and details", "SSHRTA");
            var workTaskDto = workTaskEntityAdapter.MapToType(workTaskEntity);
            Assert.AreEqual(workTaskDto.TaskProcess, WorkTaskProcess.TimeApproval);
        }
    }
}
