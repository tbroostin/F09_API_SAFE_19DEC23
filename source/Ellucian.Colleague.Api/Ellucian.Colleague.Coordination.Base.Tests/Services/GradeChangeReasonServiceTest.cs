// Copyright 2015 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
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
    public class GradeChangeReasonServiceTest
    {
        [TestClass]
        public class GradeChangeReasonService_Get: CurrentUserSetup
        {
            private IConfigurationRepository configurationRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IReferenceDataRepository> refRepoMock;
            private IReferenceDataRepository refRepo;
            public Mock<ILogger> loggerMock;
            private ILogger logger;
            private string gradeChangeReasonGuid = "bf775687-6dfe-42ef-b7c0-aee3d9e681cf";
            private Domain.Entities.Permission permissionViewAnyPerson;

            private IEnumerable<Domain.Base.Entities.GradeChangeReason> allGradeChangeReasons;
            private GradeChangeReasonService gradeChangeReasonService;

            [TestInitialize]
            public void Initialize()
            {
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                refRepoMock = new Mock<IReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                allGradeChangeReasons = new TestGradeChangeReasonRepository().Get();
                gradeChangeReasonService = new GradeChangeReasonService(configurationRepository, refRepo, adapterRegistry, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                configurationRepository = null;
                refRepo = null;
                allGradeChangeReasons = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                gradeChangeReasonService = null;
            }

            [TestMethod]
            public async Task GetGradeChangeReasonsService_CountGradeChangeReasonsAsync()
            {
                refRepoMock.Setup(repo => repo.GetGradeChangeReasonAsync(false)).ReturnsAsync(allGradeChangeReasons);
                var gradeChangeReasons = await gradeChangeReasonService.GetAsync(false);
                Assert.AreEqual(6, gradeChangeReasons.Count());
            }

            [TestMethod]
            public async Task GetGradeChangeReasonServiceByGuid_CountGradeChangeReasonsAsync()
            {
                Ellucian.Colleague.Domain.Base.Entities.GradeChangeReason thisGCR = allGradeChangeReasons.Where(i => i.Guid == gradeChangeReasonGuid).FirstOrDefault();
                refRepoMock.Setup(repo => repo.GetGradeChangeReasonAsync(true)).ReturnsAsync(allGradeChangeReasons.Where( i => i.Guid == gradeChangeReasonGuid));

                var gradeChangeReasonType = await gradeChangeReasonService.GetGradeChangeReasonByIdAsync(gradeChangeReasonGuid);
                Assert.AreEqual(gradeChangeReasonType.Id, thisGCR.Guid);
                Assert.AreEqual(gradeChangeReasonType.Code, thisGCR.Code);
                Assert.AreEqual(gradeChangeReasonType.Description, string.Empty);
                Assert.AreEqual(gradeChangeReasonType.Title, thisGCR.Description);
            }
        }

        // sets up a current user
        public abstract class CurrentUserSetup
        {
            public Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory :ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }
    }
}
