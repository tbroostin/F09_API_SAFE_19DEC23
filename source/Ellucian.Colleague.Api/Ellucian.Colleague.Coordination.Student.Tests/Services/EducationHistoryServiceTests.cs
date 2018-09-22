// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class EducationHistoryServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role facultyRole = new Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }



        [TestClass]
        public class QueryEducationHistory: CurrentUserSetup
        {
            private EducationHistoryService educationHistoryService;
            private Mock<IEducationHistoryRepository> educationHistoryRepoMock;
            private IEducationHistoryRepository educationHistoryRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;

            private List<Domain.Student.Entities.EducationHistory> eduactionHistory;


            [TestInitialize]
            public async void Initialize()
            {
                educationHistoryRepoMock = new Mock<IEducationHistoryRepository>();
                educationHistoryRepo = educationHistoryRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                eduactionHistory = new List<Domain.Student.Entities.EducationHistory>();
                Domain.Student.Entities.EducationHistory hist1 = new Domain.Student.Entities.EducationHistory("1");
                hist1.Colleges = new List<Domain.Student.Entities.College>()
                {
                    new Domain.Student.Entities.College("c1") { CollegeName="good college" },
                     new Domain.Student.Entities.College("c2") { CollegeName="unknown college" }
                };
                hist1.HighSchools = new List<Domain.Student.Entities.HighSchool>()
                {
                    new Domain.Student.Entities.HighSchool("h1") { HighSchoolName="good highschool" },
                    new Domain.Student.Entities.HighSchool("h1") { HighSchoolName="unknown highschool" },

                };

                Domain.Student.Entities.EducationHistory hist2 = new Domain.Student.Entities.EducationHistory("2");
                hist2.Colleges = new List<Domain.Student.Entities.College>()
                {
                    new Domain.Student.Entities.College("c3") { CollegeName="another good college" },
                     new Domain.Student.Entities.College("c4") { CollegeName="another unknown college" }
                };
                hist2.HighSchools = new List<Domain.Student.Entities.HighSchool>()
                {
                    new Domain.Student.Entities.HighSchool("h3") { HighSchoolName="another good highschool" },
                    new Domain.Student.Entities.HighSchool("h4") { HighSchoolName="another unknown highschool" },

                };

                eduactionHistory.Add(hist1);
                eduactionHistory.Add(hist2);

                educationHistoryRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>())).ReturnsAsync(eduactionHistory);

                // Mock Adapters
                var educationHistoryDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.EducationHistory, Ellucian.Colleague.Dtos.Student.EducationHistory>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.EducationHistory, Ellucian.Colleague.Dtos.Student.EducationHistory>()).Returns(educationHistoryDtoAdapter);
                var highSchoolAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.HighSchool, Ellucian.Colleague.Dtos.Student.HighSchool>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.HighSchool, Ellucian.Colleague.Dtos.Student.HighSchool>()).Returns(highSchoolAdapter);
                var collegeAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.College, Ellucian.Colleague.Dtos.Student.College>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.College, Ellucian.Colleague.Dtos.Student.College>()).Returns(collegeAdapter);

                // Instantiate EducationHistoryService  with mocked items
                educationHistoryService = new EducationHistoryService(adapterRegistry, currentUserFactory, roleRepo, educationHistoryRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                educationHistoryRepo = null;
                adapterRegistry = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetEducationHisotry_WithNoPermissions()
            {
                var result = await educationHistoryService.QueryEducationHistoryByIdsAsync(new List<string>() {"S1" });
            }

            [TestMethod]
            public async Task GetEducationHistory_WithValidPermissions()
            {
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("VIEW.STUDENT.INFORMATION"));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { facultyRole });

                var result = await educationHistoryService.QueryEducationHistoryByIdsAsync(new List<string>() { "S1" });
                Assert.AreEqual(2, result.Count());
            }
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetEducationHistory_WithWrongPermissions()
            {
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { facultyRole });

                var result = await educationHistoryService.QueryEducationHistoryByIdsAsync(new List<string>() { "S1" });
                Assert.AreEqual(2, result.Count());
            }

        }

    }
}