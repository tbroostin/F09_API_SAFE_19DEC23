// Copyright 2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Api.Controllers.Base;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Models;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Tests.Controllers
{
    public class MessageControllerTestsBase
    {
        public TestContext TestContext { get; set; }

        protected Mock<IMessageService> messageServiceMock;
        protected Mock<IMessageRepository> messageRepositoryMock;
        protected Mock<ILogger> loggerMock;
        protected Mock<IAdapterRegistry> adapterRegistryMock;
        protected Mock<IReferenceDataRepository> refRepoMock;
        protected IReferenceDataRepository refRepo;

        protected MessageController messageController;
        protected List<Domain.Base.Entities.WorkTask> workTaskEntities;
        protected AutoMapperAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask> entityToDtoAdapter;
        protected List<Dtos.Base.WorkTask> workTaskDtos;

        protected string personId;
        protected string messageId;
        protected Dtos.Base.ExecutionState newState;
        protected string workflowId;
        protected string processCode;
        protected string subjectLine;

        [TestInitialize]
        public void InitializeBase()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            personId = "0000001";
            messageId = "00001";
            newState = Dtos.Base.ExecutionState.OpenNotActive;
            workflowId = "0001";
            processCode = "0000001";
            subjectLine = "00001";

            messageServiceMock = new Mock<IMessageService>();
            loggerMock = new Mock<ILogger>();
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            messageRepositoryMock = new Mock<IMessageRepository>();
            var loggerObject = new Mock<ILogger>().Object;
            var adapterRegistry = new Mock<IAdapterRegistry>();

            entityToDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask>()).Returns(entityToDtoAdapter);

            messageController = new MessageController(messageServiceMock.Object, loggerMock.Object)
            {
               Request = new HttpRequestMessage()
             };
            workTaskEntities = new List<Domain.Base.Entities.WorkTask>() {
                new Domain.Base.Entities.WorkTask("1","Category 1","First Work Task", "SSHRTA"),
                new Domain.Base.Entities.WorkTask("2","Category 2","Second Work Task", "SSHRLVA"),
                new Domain.Base.Entities.WorkTask("3","Category 1","Third Work Task", "SSHRTA")
            };

            workTaskDtos = new List<Dtos.Base.WorkTask>();
            foreach (var item in workTaskEntities)
            {
                workTaskDtos.Add(entityToDtoAdapter.MapToType(item));
            };

           
        }

        [TestCleanup]
        public void CleanupBase()
        {
            messageController = null;
            loggerMock = null;
            adapterRegistryMock = null;
            messageRepositoryMock = null;
            messageServiceMock = null;
            workTaskEntities = null;
            workTaskDtos = null;
        }


    }

    [TestClass]
    public class MessageControllerTests : MessageControllerTestsBase
    {
        [TestInitialize]
        public void Initialize()
        {
            InitializeBase();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CleanupBase();
        }

        [TestMethod]
        [Ignore]
        public async Task MessageUpdateWorklistAsync()
        {

            messageServiceMock.Setup<Task<Dtos.Base.WorkTask>>(i => i.UpdateMessageWorklistAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Dtos.Base.ExecutionState>())).ReturnsAsync(workTaskDtos[0]);
            messageController = new MessageController(this.messageServiceMock.Object, loggerMock.Object);
            var results = await this.messageController.UpdateMessageWorklistAsync(personId, newState, workTaskDtos[0]);
            
            Assert.IsNotNull(results);
            Assert.AreEqual("1", results.Id);

            
        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(NullReferenceException))]
        public async Task MessageUpdateWorklist_Exception()
        {

            messageServiceMock.Setup<Task<Dtos.Base.WorkTask>>(i => i.UpdateMessageWorklistAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Dtos.Base.ExecutionState>())).ReturnsAsync(workTaskDtos[0]);
            messageController = new MessageController(this.messageServiceMock.Object, loggerMock.Object);
            await messageController.UpdateMessageWorklistAsync(personId, newState, null);

        }

        [TestMethod]
        [Ignore]
        public async Task MessageCreateWorklistAsync()
        {

            messageServiceMock.Setup<Task<Dtos.Base.WorkTask>>(i => i.CreateMessageWorklistAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(workTaskDtos[0]);
            messageController = new MessageController(this.messageServiceMock.Object, loggerMock.Object);

            var results = await this.messageController.CreateMessageWorklistAsync("00001", "00001", "00001", "00001", "00001");

            Assert.IsNotNull(results);
            Assert.AreEqual("1", results.Id);
        }


        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(Exception))]
        public async Task MessageCreateWorklist_NullWorkflow()
        {

            messageServiceMock.Setup<Task<Dtos.Base.WorkTask>>(i => i.CreateMessageWorklistAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(workTaskDtos[0]);
            messageController = new MessageController(this.messageServiceMock.Object, loggerMock.Object);
            await this.messageController.CreateMessageWorklistAsync(null, processCode, subjectLine, "0001", personId);

        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(Exception))]
        public async Task MessageCreateWorklist_NullProcessCode()
        {

            messageServiceMock.Setup<Task<Dtos.Base.WorkTask>>(i => i.CreateMessageWorklistAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(workTaskDtos[0]);
            messageController = new MessageController(this.messageServiceMock.Object, loggerMock.Object);
            await this.messageController.CreateMessageWorklistAsync(personId, null, subjectLine, "0001", personId);

        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(Exception))]
        public async Task MessageCreateWorklist_NullSubjectLine()
        {

            messageServiceMock.Setup<Task<Dtos.Base.WorkTask>>(i => i.CreateMessageWorklistAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(workTaskDtos[0]);
            messageController = new MessageController(this.messageServiceMock.Object, loggerMock.Object);
            await this.messageController.CreateMessageWorklistAsync(personId, processCode, null, "0001", personId);

        }


        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(Exception))]
        public async Task MessageCreateWorklist_NullPersonId()
        {

            messageServiceMock.Setup<Task<Dtos.Base.WorkTask>>(i => i.CreateMessageWorklistAsync(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<String>())).ReturnsAsync(workTaskDtos[0]);
            messageController = new MessageController(this.messageServiceMock.Object, loggerMock.Object);
            await this.messageController.CreateMessageWorklistAsync(personId, processCode, subjectLine, "001", null);

        }


    }
   }
