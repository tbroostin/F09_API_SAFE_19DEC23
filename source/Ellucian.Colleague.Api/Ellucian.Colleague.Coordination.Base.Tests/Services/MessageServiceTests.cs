// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class MessageServiceTestsBase
    {

        protected MessageService service = null;
        protected MessageService serviceForProxy = null;
        protected Mock<IMessageRepository> mockMessageRepository;
        protected Mock<IRoleRepository> mockRoleRepository;
        protected Mock<IProxyRepository> mockProxyRepository;
        protected ICurrentUserFactory currentUserFactory;
        public ICurrentUserFactory proxyCurrentUserFactory;
        protected ICurrentUserFactory currentUserWithoutProxyFactory;
        protected Domain.Base.Entities.ProxyConfiguration proxyConfig;
        protected List<Domain.Base.Entities.WorkTask> workTaskEntities;
        protected List<Domain.Base.Entities.WorkTask> proxyWorkTaskEntities;
        protected List<Domain.Base.Entities.ProxyWorkflowGroup> workflowGroupEntities;
        protected List<Domain.Base.Entities.ProxyWorkflow> workflowEntities;
        protected List<WorkTask> workTaskDtos;
        protected List<WorkTask> proxyWorkTaskDtos;
        protected List<ProxyWorkflowGroup> workflowGroupDtos;
        protected List<ProxyWorkflow> workflowDtos;
        protected IEnumerable<Domain.Entities.Role> repoRoles;
        protected IEnumerable<Domain.Entities.Role> proxyRepoRoles;
        protected AutoMapperAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask> entityToDtoAdapter;
        protected AutoMapperAdapter<Domain.Base.Entities.ProxyWorkflowGroup, Dtos.Base.ProxyWorkflowGroup> wfgEntityToDtoAdapter;
        protected AutoMapperAdapter<Domain.Base.Entities.ProxyWorkflow, Dtos.Base.ProxyWorkflow> wfEntityToDtoAdapter;
        protected AutoMapperAdapter<Dtos.Base.WorkTask, Domain.Base.Entities.WorkTask> dtoToEntityAdapter;
        protected AutoMapperAdapter<Domain.Base.Entities.ExecutionState, Dtos.Base.ExecutionState> execStateEntityToDtoAdapter;
        protected AutoMapperAdapter<Dtos.Base.ExecutionState, Domain.Base.Entities.ExecutionState> execStateDtoToEntityAdapter;
        protected string personId;
        protected string messageId;
        protected ExecutionState newState;
        protected string workflowId;
        protected string processCode;
        protected string subjectLine;
        protected string proxyUserId;
        protected string roleId;

        public class UserFactoryWithProxy : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Alex",
                        PersonId = "0000008",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "Student" },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0000001",
                            Permissions = { "TMTA", "OTEM" }
                        }
                    });
                }
            }
        }



        public void InitializeBase()
        {
            personId = "0000001";
            messageId = "00001";
            newState = ExecutionState.OpenNotActive;
            workflowId = "0001";
            processCode = "0000001";
            subjectLine = "00001";
            proxyUserId = "0000008";
            roleId = "1";



            workTaskEntities = new List<Domain.Base.Entities.WorkTask>() {
                new Domain.Base.Entities.WorkTask("1","Category 1","First Work Task", "SSHRTA"),
                new Domain.Base.Entities.WorkTask("2","Category 2","Second Work Task", "SSHRLVA"),
                new Domain.Base.Entities.WorkTask("3","Category 1","Third Work Task", "SSHRTA")
            };


            workflowEntities = new List<Domain.Base.Entities.ProxyWorkflow>()
            {
                new Domain.Base.Entities.ProxyWorkflow("TMTA", "Time Approval", "EM", true) {
                    WorklistCategorySpecialProcessCode = "SSHRTA"
                },
                new Domain.Base.Entities.ProxyWorkflow("OTEM", "Other Employee Workflow", "OE", true) {
                    WorklistCategorySpecialProcessCode = "OTHEMP"
                },
                new Domain.Base.Entities.ProxyWorkflow("ONEM", "Other Employee Workflow 2", "NE", true)
                {
                    WorklistCategorySpecialProcessCode = "OTNEMP"
                }
            };

            workflowGroupEntities = new List<Domain.Base.Entities.ProxyWorkflowGroup>()
            {
                new Domain.Base.Entities.ProxyWorkflowGroup("EM","Employee",true),
                new Domain.Base.Entities.ProxyWorkflowGroup("OE","Employee Group 2",true),
                new Domain.Base.Entities.ProxyWorkflowGroup("NE","Employee Group 3",true)
            };

            // Add workflow entities to correct workflow group entity
            for (var k = 0; k < workflowGroupEntities.Count(); k++)
            {
                workflowGroupEntities.ElementAt(k).AddWorkflow(workflowEntities.ElementAt(k));
            }

            // Build all service objects to use each of the user factories built above
            BuildWorkTaskService();

            workTaskDtos = new List<WorkTask>();
            foreach (var item in workTaskEntities)
            {
                workTaskDtos.Add(entityToDtoAdapter.MapToType(item));
            };



            repoRoles = new List<Domain.Entities.Role>() {
                new Domain.Entities.Role(2, "Parent"),
                new Domain.Entities.Role(1, "Student")
            };

        }

        public void CleanupBase()
        {
            service = null;
            serviceForProxy = null;
            mockMessageRepository = null;
            mockRoleRepository = null;
            mockProxyRepository = null;
            workTaskEntities = null;
            proxyWorkTaskEntities = null;
            workflowGroupEntities = null;
            workflowEntities = null;
            workTaskDtos = null;
            proxyWorkTaskDtos = null;
            workflowGroupDtos = null;
            workflowDtos = null;
        }

        private void BuildWorkTaskService()
        {
            // We need the unit tests to be independent of "real" implementations of these classes,
            // so we use Moq to create mock implementations that are based on the same interfaces
            // Initialize the mock repositories
            mockMessageRepository = new Mock<IMessageRepository>();
            mockRoleRepository = new Mock<IRoleRepository>();
            mockProxyRepository = new Mock<IProxyRepository>();
            var loggerObject = new Mock<ILogger>().Object;

            // Set up current user
            currentUserFactory = new GenericUserFactory.UserFactory();

            // Set up and mock the adapter, and setup the GetAdapter method.
            var adapterRegistry = new Mock<IAdapterRegistry>();

            // Set up the entity to DTO adapter
            entityToDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Domain.Base.Entities.WorkTask, Dtos.Base.WorkTask>()).Returns(entityToDtoAdapter);

            // Set up the DTO to entity adapter
            dtoToEntityAdapter = new AutoMapperAdapter<Dtos.Base.WorkTask, Domain.Base.Entities.WorkTask>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Dtos.Base.WorkTask, Domain.Base.Entities.WorkTask>()).Returns(dtoToEntityAdapter);

            // Set up the DTO to entity adapter for execution states
            execStateDtoToEntityAdapter = new AutoMapperAdapter<Dtos.Base.ExecutionState, Domain.Base.Entities.ExecutionState>(adapterRegistry.Object, loggerObject);
            adapterRegistry.Setup(x => x.GetAdapter<Dtos.Base.ExecutionState, Domain.Base.Entities.ExecutionState>()).Returns(execStateDtoToEntityAdapter);

            // Set up the current user with a subset of projects and set up the service.
            service = new MessageService(adapterRegistry.Object, mockMessageRepository.Object, currentUserFactory, mockRoleRepository.Object, loggerObject);
            
        }
    }

    [TestClass]
    public class MessageServiceTests : MessageServiceTestsBase
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
        public async Task MessageUpdateAsync()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);

            mockMessageRepository.Setup<Task<Domain.Base.Entities.WorkTask>>(repo => repo.UpdateMessageWorklistAsync(messageId, personId, execStateDtoToEntityAdapter.MapToType(newState)))
                .ReturnsAsync(workTaskEntities[0]);

            var actualResult = await this.service.UpdateMessageWorklistAsync(messageId, personId, newState);
            Assert.AreEqual("1", actualResult.Id);

        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(PermissionsException))]
        public async Task MessageUpdateAsync_NullPersonId()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);

            mockMessageRepository.Setup<Task<Domain.Base.Entities.WorkTask>>(repo => repo.UpdateMessageWorklistAsync(messageId, personId, execStateDtoToEntityAdapter.MapToType(newState)))
                .ReturnsAsync(workTaskEntities[0]);
            await this.service.UpdateMessageWorklistAsync(messageId, null, newState);

        }


        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MessageUpdateAsync_NullMessageId()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);

            mockMessageRepository.Setup<Task<Domain.Base.Entities.WorkTask>>(repo => repo.UpdateMessageWorklistAsync(messageId, personId, execStateDtoToEntityAdapter.MapToType(newState)))
                .ReturnsAsync(workTaskEntities[0]);
            await this.service.UpdateMessageWorklistAsync(null, personId, newState);

        }


        [TestMethod]
        [Ignore]
        public async Task MessageCreateAsync()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);

            mockMessageRepository.Setup<Task<Domain.Base.Entities.WorkTask>>(repo => repo.CreateMessageWorklistAsync(personId, workflowId, processCode, subjectLine))
                .ReturnsAsync(workTaskEntities[0]);

            var actualResult = await this.service.CreateMessageWorklistAsync(personId, workflowId, processCode, subjectLine);
            Assert.AreEqual("1", actualResult.Id);

        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MessageCreateAsync_NullMessageId()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);

            mockMessageRepository.Setup<Task<Domain.Base.Entities.WorkTask>>(repo => repo.CreateMessageWorklistAsync(personId, workflowId, processCode, subjectLine))
                .ReturnsAsync(workTaskEntities[0]);
            await this.service.CreateMessageWorklistAsync(null, workflowId, processCode, subjectLine);

        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MessageCreateAsync_NullWorkflowId()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);

            mockMessageRepository.Setup<Task<Domain.Base.Entities.WorkTask>>(repo => repo.CreateMessageWorklistAsync(personId, workflowId, processCode, subjectLine))
                .ReturnsAsync(workTaskEntities[0]);
            await this.service.CreateMessageWorklistAsync(personId, null, processCode, subjectLine);

        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MessageCreateAsync_NullProcessId()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);

            mockMessageRepository.Setup<Task<Domain.Base.Entities.WorkTask>>(repo => repo.CreateMessageWorklistAsync(personId, workflowId, processCode, subjectLine))
                .ReturnsAsync(workTaskEntities[0]);
            await this.service.CreateMessageWorklistAsync(personId, workflowId, null, subjectLine);

        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task MessageCreateAsync_SubjectLine()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);

            mockMessageRepository.Setup<Task<Domain.Base.Entities.WorkTask>>(repo => repo.CreateMessageWorklistAsync(personId, workflowId, processCode, subjectLine))
                .ReturnsAsync(workTaskEntities[0]);
            await this.service.CreateMessageWorklistAsync(personId, workflowId, processCode, null);

        }

    }
}
