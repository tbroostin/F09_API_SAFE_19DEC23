// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.Base.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Domain.Entities;
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
    public class WorkTaskServiceTestsBase : GenericUserFactory
    {
        protected WorkTaskService service = null;
        protected WorkTaskService serviceForProxy = null;
        protected Mock<IWorkTaskRepository> mockWorkTaskRepository;
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
        protected string personId;
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
            proxyUserId = "0000008";
            roleId = "1";

            proxyCurrentUserFactory = new UserFactoryWithProxy();
            
            workTaskEntities = new List<Domain.Base.Entities.WorkTask>() {
                new Domain.Base.Entities.WorkTask("1","Category 1","First Work Task", "SSHRTA"),
                new Domain.Base.Entities.WorkTask("2","Category 2","Second Work Task", "SSHRLVA"),
                new Domain.Base.Entities.WorkTask("3","Category 1","Third Work Task", "SSHRTA")
            };

            proxyWorkTaskEntities = new List<Domain.Base.Entities.WorkTask>()
            {
                new Domain.Base.Entities.WorkTask("8","Time Approval","Self-Service Time Approval Task", "SSHRTA"),
                new Domain.Base.Entities.WorkTask("9","Other Employee","Other Employee Task", "OTHEMP"),
                new Domain.Base.Entities.WorkTask("10","Other Employee 2","Other Employee Task 2", "OTNEMP")
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
            proxyConfig = new Domain.Base.Entities.ProxyConfiguration(true, "", "", true, true, new List<Domain.Base.Entities.ProxyAndUserPermissionsMap>());
            //Add workflow groups to proxy config
            for (var j = 0; j < workflowGroupEntities.Count(); j++)
            {
                proxyConfig.AddWorkflowGroup(workflowGroupEntities.ElementAt(j));
            } 

            // Build all service objects to use each of the user factories built above
            BuildWorkTaskService();

            workTaskDtos = new List<WorkTask>();
            foreach (var item in workTaskEntities)
            {
                workTaskDtos.Add(entityToDtoAdapter.MapToType(item));
            };

            proxyWorkTaskDtos = new List<WorkTask>();
            foreach (var item in proxyWorkTaskEntities)
            {
                proxyWorkTaskDtos.Add(entityToDtoAdapter.MapToType(item));
            };

            //***DO WE NEED DTOS FOR THESE?
            //workflowGroupDtos = new List<ProxyWorkflowGroup>();
            //foreach(var item in workflowGroupEntities)
            //{
            //    workflowGroupDtos.Add(wfgEntityToDtoAdapter.MapToType(item));
            //};

            //workflowDtos = new List<ProxyWorkflow>();
            //foreach(var item in workflowEntities)
            //{
            //    workflowDtos.Add(wfEntityToDtoAdapter.MapToType(item));
            //};
            //***

            repoRoles = new List<Domain.Entities.Role>() {
                new Domain.Entities.Role(2, "Parent"),
                new Domain.Entities.Role(1, "Student")
            };

            proxyRepoRoles = new List<Domain.Entities.Role>() {
                new Domain.Entities.Role(3, "Proxy User")
            };
        }

        public void CleanupBase()
        {
            service = null;
            serviceForProxy = null;
            mockWorkTaskRepository = null;
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
            mockWorkTaskRepository = new Mock<IWorkTaskRepository>();
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

            // Set up the current user with a subset of projects and set up the service.
            service = new WorkTaskService(adapterRegistry.Object, mockWorkTaskRepository.Object, currentUserFactory, mockRoleRepository.Object, mockProxyRepository.Object, loggerObject);

            // Set up current user with proxy access
            serviceForProxy = new WorkTaskService(adapterRegistry.Object, mockWorkTaskRepository.Object, proxyCurrentUserFactory, mockRoleRepository.Object, mockProxyRepository.Object, loggerObject);
        }
    }

    [TestClass]
    public class WorkTaskServiceTests : WorkTaskServiceTestsBase
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
        public async Task GetAsync_Success()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);
            // Set up WorkTask repository response--which, by expecting values in the roles field, also verifies that role ids were built based on the role response + the current user roles.
            mockWorkTaskRepository.Setup<Task<List<Domain.Base.Entities.WorkTask>>>(repo => repo.GetAsync(personId, It.Is<List<string>>(y => y.Contains(roleId))))
                .ReturnsAsync(proxyWorkTaskEntities);

            // Set up the Proxy Repository
            mockProxyRepository.Setup(r => r.GetProxyConfigurationAsync()).ReturnsAsync(proxyConfig);

            var actualResult = await this.service.GetAsync(personId);
            Assert.AreEqual(3, actualResult.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetAsync_NoPersonId_ThrowsException()
        {
            var actualResult = await this.service.GetAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetAsync_PersonIdDoesNotMatchCurrentUser_ThrowsException()
        {
            var actualResult = await this.service.GetAsync("0000002");
        }

        [TestMethod]
        public async Task GetAsync_ProxyUser()
        {
            // Set up role repository response
            mockRoleRepository.Setup(repo => repo.GetRolesAsync()).ReturnsAsync(repoRoles);
            // Set up WorkTask repository response--which, by expecting values in the roles field, also verifies that role ids were built based on the role response + the current user roles.
            mockWorkTaskRepository.Setup<Task<List<Domain.Base.Entities.WorkTask>>>(repo => repo.GetAsync(personId, It.Is<List<string>>(y => y.Contains(roleId))))
                .ReturnsAsync(proxyWorkTaskEntities);

            // Set up the Proxy Repository
            mockProxyRepository.Setup(r => r.GetProxyConfigurationAsync()).ReturnsAsync(proxyConfig);

            var actualResult = await this.serviceForProxy.GetAsync(personId);
            Assert.AreEqual(2, actualResult.Count());
        }
    }
}
