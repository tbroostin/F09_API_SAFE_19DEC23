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
    public class StudentFinancialAidApplicationServiceTests
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
        /// This class tests the FinancialAidApplicationService2 class.
        /// </summary>
        [TestClass]
        public class StudentFinancialAidApplicationServiceUnitTests : CurrentUserSetup
        {

            private Domain.Entities.Permission permissionViewAnyApplication;
            private IAdapterRegistry adapterRegistry;
            private ICollection<Domain.Student.Entities.FinancialAidYear> _financialAidYears = new List<Domain.Student.Entities.FinancialAidYear>();
            private IConfigurationRepository baseConfigurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Domain.Student.Entities.Fafsa> allFinancialAidApplications;
            private ILogger logger;
            private IPersonRepository personRepo;
            private IRoleRepository roleRepo;
            private IStudentFinancialAidApplicationRepository faAppRepo;
            private IStudentReferenceDataRepository refRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IRoleRepository> roleRepoMock;
            private Mock<IStudentFinancialAidApplicationRepository> faAppRepoMock;
            private Mock<IStudentReferenceDataRepository> refRepoMock;
            private string financialAidApplicationGuid = "31d8aa32-dbe6-4a49-a1c4-2cad39e232e4";
            private StudentFinancialAidApplicationService financialAidApplicationService;
            private Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int> _financialAidApplicationTuple;

            [TestInitialize]
            public void Initialize()
            {
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                refRepoMock = new Mock<IStudentReferenceDataRepository>();
                refRepo = refRepoMock.Object;
                faAppRepoMock = new Mock<IStudentFinancialAidApplicationRepository>();
                faAppRepo = faAppRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                allFinancialAidApplications = new TestFinancialAidApplicationRepository().GetFinancialAidApplications();

                _financialAidApplicationTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(allFinancialAidApplications, allFinancialAidApplications.Count());

                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("9C3B805D-CFE6-483B-86C3-4C20562F8C15", "2013", "CODE1", "STATUS1") { HostCountry = "USA" });
                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("73244057-D1EC-4094-A0B7-DE602533E3A6", "2014", "CODE2", "STATUS2") { HostCountry = "CAN", status = "D" });
                _financialAidYears.Add(new Domain.Student.Entities.FinancialAidYear("1df164eb-8178-4321-a9f7-24f12d3991d8", "2015", "CODE3", "STATUS3") { HostCountry = "USA" });

                refRepoMock.Setup(repo => repo.GetFinancialAidYearsAsync(It.IsAny<bool>())).ReturnsAsync(_financialAidYears);

                // Set up current user
                //currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                //currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Set up current user
                //currentUserFactory = userFactoryMock.Object;
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyApplication = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewFinancialAidApplications);
                personRole.AddPermission(permissionViewAnyApplication);

                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                financialAidApplicationService = new StudentFinancialAidApplicationService(faAppRepo, personRepo, refRepo, baseConfigurationRepository, adapterRegistry, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                refRepo = null;
                faAppRepo = null;
                personRepo = null;
                allFinancialAidApplications = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
                financialAidApplicationService = null;
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationByIdAsync()
            {
                Domain.Student.Entities.Fafsa thisFinancialAidApplication = allFinancialAidApplications.Where(m => m.Guid == financialAidApplicationGuid).FirstOrDefault();
                faAppRepoMock.Setup(repo => repo.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_financialAidApplicationTuple.Item1.Where(m => m.Guid == financialAidApplicationGuid).FirstOrDefault());
                Dtos.FinancialAidApplication financialAidApplication = await financialAidApplicationService.GetByIdAsync(financialAidApplicationGuid);
                Assert.AreEqual(thisFinancialAidApplication.Guid, financialAidApplication.Id);
            }


            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Count_Cache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<List<string>>())).ReturnsAsync(_financialAidApplicationTuple);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplication = await financialAidApplicationService.GetAsync(0, 100, false);
                Assert.AreEqual(allFinancialAidApplications.Count(), financialAidApplication.Item2);
            }


            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_NoRecords()
            {
                var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<List<string>>())).ReturnsAsync(emptyTuple);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplication = await financialAidApplicationService.GetAsync(0, 100, false);
                Assert.AreEqual(emptyTuple.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Null_Tuple()
            {
                var emptyTuple = new Tuple<IEnumerable<Domain.Student.Entities.Fafsa>, int>(new List<Domain.Student.Entities.Fafsa>(), 0);
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<List<string>>())).ReturnsAsync(null);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplication = await financialAidApplicationService.GetAsync(0, 100, false);
                Assert.AreEqual(emptyTuple.Item1.Count(), 0);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Cache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<List<string>>())).ReturnsAsync(_financialAidApplicationTuple);

                Tuple<IEnumerable<Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, false);
                Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Guid, financialAidApplications.Item1.ElementAt(0).Id);
            }


            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_Count_NonCache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<List<string>>())).ReturnsAsync(_financialAidApplicationTuple);
                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplication = await financialAidApplicationService.GetAsync(0, 100, true);
                Assert.AreEqual(allFinancialAidApplications.Count(), financialAidApplication.Item2);
            }

            [TestMethod]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationsAsync_NonCache()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), true, It.IsAny<List<string>>())).ReturnsAsync(_financialAidApplicationTuple);

                Tuple<IEnumerable<Ellucian.Colleague.Dtos.FinancialAidApplication>, int> financialAidApplications = await financialAidApplicationService.GetAsync(0, 100, true);
                Assert.AreEqual(allFinancialAidApplications.ElementAt(0).Guid, financialAidApplications.Item1.ElementAt(0).Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentFinancialAidApplicationService_GetFinancialAidApplicationByIdAsync_ThrowsInvOpExc()
            {
                faAppRepoMock.Setup(repo => repo.GetAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<List<string>>())).Throws<KeyNotFoundException>();
                await financialAidApplicationService.GetByIdAsync("dshjfkj");
            }
        }
    }
}