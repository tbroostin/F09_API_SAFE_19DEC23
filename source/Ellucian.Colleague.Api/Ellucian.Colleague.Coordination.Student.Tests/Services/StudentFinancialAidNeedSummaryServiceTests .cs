// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
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
    public class StudentFinancialAidNeedSummaryServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
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

        /// <summary>
        /// This class tests the StudentFinancialAidNeedSummaryService class.
        /// </summary>
        [TestClass]
        public class StudentFinancialAidNeedSummaryServiceUnitTests : CurrentUserSetup
        {

            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IStudentReferenceDataRepository> refRepoMock;
            private IStudentReferenceDataRepository refRepo;
            private Mock<IStudentFinancialAidNeedSummaryRepository> aidNeedRepoMock;
            private IStudentFinancialAidNeedSummaryRepository aidNeedRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Student.Entities.StudentNeedSummary> allStudentFinancialAidNeedSummaries;
            private Tuple<IEnumerable<Domain.Student.Entities.StudentNeedSummary>, int> _studentFinancialAidNeedSummaryTuple;
            private ICollection<Domain.Student.Entities.FinancialAidYear> _financialAidYears = new List<Domain.Student.Entities.FinancialAidYear>();
            private StudentFinancialAidNeedSummaryService studentFinancialAidNeedSummaryService;
            private string studentFinancialAidNeedSummaryGuid = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4";
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");
            private Domain.Entities.Permission permissionViewAnyApplication;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 200;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                refRepoMock = new Mock<IStudentReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                aidNeedRepoMock = new Mock<IStudentFinancialAidNeedSummaryRepository>();
                aidNeedRepo = aidNeedRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                allStudentFinancialAidNeedSummaries = new TestStudentFinancialAidNeedSummaryRepository().GetStudentFinancialAidNeedSummaries();

                _studentFinancialAidNeedSummaryTuple = new Tuple<IEnumerable<Domain.Student.Entities.StudentNeedSummary>, int>(allStudentFinancialAidNeedSummaries, allStudentFinancialAidNeedSummaries.Count());

                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2013", "CODE1", "STATUS1") { HostCountry = "USA" });
                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("73244057-D1EC-4094-A0B7-DE602533E3A6", "2014", "CODE2", "STATUS2") { HostCountry = "CAN", status = "D" });
                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d8", "2015", "CODE3", "STATUS3") { HostCountry = "USA" });

                refRepoMock.Setup(repo => repo.GetFinancialAidYearsAsync(It.IsAny<bool>())).ReturnsAsync(_financialAidYears);

                aidNeedRepoMock.Setup(repo => repo.GetIsirCalcResultsGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("5X7G804T-NMK6-903A-74C3-4C04762K9P15");

                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentFinancialAidNeedSummaries);
                personRole.AddPermission(permissionViewAnyApplication);

                // Mock permissions
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                studentFinancialAidNeedSummaryService = new StudentFinancialAidNeedSummaryService(aidNeedRepo, personRepo, refRepo, baseConfigurationRepository, adapterRegistry, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                aidNeedRepo = null;
                personRepo = null;
                allStudentFinancialAidNeedSummaries = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                studentFinancialAidNeedSummaryService = null;
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummaryService_GetStudentFinancialAidNeedSummaryByIdAsync()
            {
                Domain.Student.Entities.StudentNeedSummary thisStudentFinancialAidNeedSummary = allStudentFinancialAidNeedSummaries.Where(m => m.Guid == studentFinancialAidNeedSummaryGuid).FirstOrDefault();
                aidNeedRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentFinancialAidNeedSummaryTuple.Item1.Where(m => m.Guid == studentFinancialAidNeedSummaryGuid).FirstOrDefault());
                Dtos.StudentFinancialAidNeedSummary studentFinancialAidNeedSummary = await studentFinancialAidNeedSummaryService.GetByIdAsync(studentFinancialAidNeedSummaryGuid);
                Assert.AreEqual(thisStudentFinancialAidNeedSummary.Guid, studentFinancialAidNeedSummary.Id);
                //Assert.AreEqual(thisFinancialAidApplication.Code, financialAidApplication.Code);
                //Assert.AreEqual(null, financialAidApplication.Description);
                //Assert.AreEqual(Dtos.SocialMediaTypeCategory.facebook, financialAidApplication.SocialMediaTypeCategory);
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummaryService_GetStudentFinancialAidNeedSummariesAsync_Count_Cache()
            {
                aidNeedRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<List<string>>())).ReturnsAsync(_studentFinancialAidNeedSummaryTuple);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary>, int> studentFinancialAidNeedSummary = await studentFinancialAidNeedSummaryService.GetAsync(0, 100, false);
                Assert.AreEqual(allStudentFinancialAidNeedSummaries.Count(), studentFinancialAidNeedSummary.Item2);
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummaryService_GetStudentFinancialAidNeedSummariesAsync_Cache()
            {
                aidNeedRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<List<string>>())).ReturnsAsync(_studentFinancialAidNeedSummaryTuple);

                Tuple<IEnumerable<Dtos.StudentFinancialAidNeedSummary>, int> studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummaryService.GetAsync(0, 100, false);
                Assert.AreEqual(allStudentFinancialAidNeedSummaries.ElementAt(0).Guid, studentFinancialAidNeedSummaries.Item1.ElementAt(0).Id);
                //Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Code, financialAidApplications.ElementAt(0).Code);
                //Assert.AreEqual(null, financialAidApplications.ElementAt(0).Description);
                //Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Description, financialAidApplications.ElementAt(0).Title);
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummaryService_GetStudentFinancialAidNeedSummariesAsync_Count_NonCache()
            {
                aidNeedRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<List<string>>())).ReturnsAsync(_studentFinancialAidNeedSummaryTuple);
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary>, int> studentFinancialAidNeedSummary = await studentFinancialAidNeedSummaryService.GetAsync(0, 100, true);
                Assert.AreEqual(allStudentFinancialAidNeedSummaries.Count(), studentFinancialAidNeedSummary.Item2);
            }

            [TestMethod]
            public async Task StudentFinancialAidNeedSummaryService_GetStudentFinancialAidNeedSummariesAsync_NonCache()
            {
                aidNeedRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<List<string>>())).ReturnsAsync(_studentFinancialAidNeedSummaryTuple);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.StudentFinancialAidNeedSummary>, int> studentFinancialAidNeedSummaries = await studentFinancialAidNeedSummaryService.GetAsync(0, 100, true);
                Assert.AreEqual(allStudentFinancialAidNeedSummaries.ElementAt(0).Guid, studentFinancialAidNeedSummaries.Item1.ElementAt(0).Id);
                //Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Code, financialAidApplications.ElementAt(0).Code);
                //Assert.AreEqual(null, financialAidApplications.ElementAt(0).Description);
                //Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Description, financialAidApplications.ElementAt(0).Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentFinancialAidNeedSummaryService_GetStudentFinancialAidNeedSummaryByIdAsync_ThrowsArgumentNullExc()
            {
                aidNeedRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<string>>())).Throws<ArgumentNullException>();
                await studentFinancialAidNeedSummaryService.GetByIdAsync("dshjfkj");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentFinancialAidNeedSummaryService_GetStudentFinancialAidNeedSummaryByIdAsync_ThrowsKeyNotFoundExc()
            {
                aidNeedRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await studentFinancialAidNeedSummaryService.GetByIdAsync("dshjfkj");
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task StudentFinancialAidNeedSummaryService_GetStudentFinancialAidNeedSummaryByIdAsync_ThrowsInvOpExc()
            {
                aidNeedRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).Throws<InvalidOperationException>();
                await studentFinancialAidNeedSummaryService.GetByIdAsync("dshjfkj");
            }
        }
    }
}