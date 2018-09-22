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
    public class WorkTaskDtoAdapterTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;

        public WorkTaskDtoAdapter workTaskDtoAdapter;

        [TestInitialize]
        public void Initialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            
            workTaskDtoAdapter = new WorkTaskDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
        }

        [TestMethod]
        public void WorkTaskDtoAdapter_MapsDtoFieldsCorrectly()
        {
            var workTaskDto = new Dtos.Base.WorkTask()
            {
                Category = "Time Approval",
                Description = "Time approval description and details",
                Id = "70001",
                TaskProcess = WorkTaskProcess.TimeApproval
            };
            var workTaskEntity = workTaskDtoAdapter.MapToType(workTaskDto);
            Assert.AreEqual(workTaskEntity.Category, workTaskDto.Category);
            Assert.AreEqual(workTaskEntity.Description, workTaskDto.Description);
            Assert.AreEqual(workTaskEntity.Id, workTaskDto.Id);
        }

        [TestMethod]
        public void WorkTaskDtoAdapter_MapsNoProcessCode()
        {
            var workTaskDto = new Dtos.Base.WorkTask()
            {
                Category = "No Category",
                Description = "No description",
                Id = "70002",
                TaskProcess = WorkTaskProcess.None
            };
            var workTaskEntity = workTaskDtoAdapter.MapToType(workTaskDto);
            Assert.AreEqual(workTaskEntity.ProcessCode, string.Empty);
        }
        [TestMethod]
        public void WorkTaskDtoAdapter_MapsLeaveRequestApprovalTaskProcessCode()
        {
            var workTaskDto = new Dtos.Base.WorkTask()
            {
                Category = "Leave Request Approval",
                Description = "Leave request approval description and details",
                Id = "70003",
                TaskProcess = WorkTaskProcess.LeaveRequestApproval
            };
            var workTaskEntity = workTaskDtoAdapter.MapToType(workTaskDto);
            Assert.AreEqual(workTaskEntity.ProcessCode, "SSHRLVA");
        }
        [TestMethod]
        public void WorkTaskDtoAdapter_MapsTimeEntryApprovalTaskProcessCode()
        {
            var workTaskDto = new Dtos.Base.WorkTask()
            {
                Category = "Time Approval",
                Description = "Time approval description and details",
                Id = "70004",
                TaskProcess = WorkTaskProcess.TimeApproval
            };
            var workTaskEntity = workTaskDtoAdapter.MapToType(workTaskDto);
            Assert.AreEqual(workTaskEntity.ProcessCode, "SSHRTA");
        }
    }
}
