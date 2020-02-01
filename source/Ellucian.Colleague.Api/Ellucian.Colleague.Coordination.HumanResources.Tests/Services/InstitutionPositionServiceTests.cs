using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class InstitutionPositionServiceTests
    {
        [TestClass]
        public class InstitutionPositionServiceTests_GETV7: CurrentUserSetup
        {
            Mock<IPositionRepository> positionRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            InstitutionPositionService institutionPositionService;
            IEnumerable<Domain.HumanResources.Entities.Position> positionEntities;
            Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int> positionEntityTuple;

            IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> employmentClassificationEntities;
            IEnumerable<Domain.Base.Entities.Department> departmentEntities;
            IEnumerable<Domain.Base.Entities.Location> locationEntities;
            IEnumerable<Domain.HumanResources.Entities.BargainingUnit> bargainingUnitEntities;
            IEnumerable<Domain.HumanResources.Entities.PositionPay> positionPayEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize() 
            {
                positionRepositoryMock = new Mock<IPositionRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewInstitutionPosition);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                institutionPositionService = new InstitutionPositionService(positionRepositoryMock.Object, hrReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                                baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup() 
            {
                positionEntityTuple = null;
                positionEntities = null;
                employmentClassificationEntities = null;
                departmentEntities = null;
                locationEntities = null;
                bargainingUnitEntities = null;
                positionPayEntities = null;

                positionRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAllAsync()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);                    
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAllAsync_EmptyTuple()
            {
                positionEntities = new List<Domain.HumanResources.Entities.Position>()
                {

                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, 0);
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAllAsync_NoItmsInTuple()
            {
                positionEntityTuple = null;
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll_Filter_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositionsAsync(offset, limit, "e43e7195-6eca-451d-b6c3-1e52fe540083",
                            It.IsAny<string>(), "d5f5eafb-3192-4479-8dca-6fe79bbde6e4", "50aadc94-3b09-4bec-bca6-a9c588ee8c11", It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll_StartOn_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), "2012-07-01", It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll_EndOn_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), "2012-07-01", It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GET_ById()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = positionEntities.ToList()[0];
                positionPayEntities.ToList()[0].SalaryMinimum = "WrongType";
                positionPayEntities.ToList()[0].SalaryMaximum = "       ";
                positionRepositoryMock.Setup(i => i.GetPositionByGuidAsync(id)).ReturnsAsync(expected);
                var actual = await institutionPositionService.GetInstitutionPositionByGuidAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
                Assert.AreEqual(expected.Title, actual.Title);    
            }

            
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAllAsync_NoPermissions_Exception()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                var actualsTuple = await institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAllAsync_CovertMethodTryCatch_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                referenceDataRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actualsTuple = await institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll_DateError_Async()
            {
                referenceDataRepositoryMock.Setup(i => i.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), "2012-07-01", It.IsAny<bool>());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAllAsync_SupervisorPositionId_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentOutOfRangeException());
                var actualsTuple = await institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAllAsync_AlternateSupervisorPositionId_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("1")).ReturnsAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11");
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("2")).ThrowsAsync(new ArgumentOutOfRangeException());
                var actualsTuple = await institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAllAsync_ArgumentException()
            {
                var nullEntities = positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    null,
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67",
                        PositionDept = "Animal Science"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAllAsync_NullGuid_ArgumentException()
            {
                positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "",
                        PositionDept = "Animal Science"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstitutionPositions_GET_ById_NullId_ArgumentNullException()
            {
                var actual = await institutionPositionService.GetInstitutionPositionByGuidAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstitutionPositions_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                positionRepositoryMock.Setup(i => i.GetPositionByGuidAsync(id)).ReturnsAsync(null);
                var actual = await institutionPositionService.GetInstitutionPositionByGuidAsync(id);
            }

            private void BuildData()
            {
                locationEntities = new List<Domain.Base.Entities.Location>() 
                {
                    new Domain.Base.Entities.Location("e43e7195-6eca-451d-b6c3-1e52fe540083", "BMA", "BMA Test"),
                    new Domain.Base.Entities.Location("95df0303-9b7f-4686-908f-1640b4881e23", "CD", "Central District Office"),
                    new Domain.Base.Entities.Location("ec49b053-7acc-411a-a766-8a7fc2f24ee3", "COE", "Colonial Ohio-East (coe) Campus"),
                    new Domain.Base.Entities.Location("eded9894-ea62-44f4-be8e-b141dfc00dba", "COEEA", "Coe-east"),
                    new Domain.Base.Entities.Location("17ff700b-8d20-43d7-be31-c34933baca75", "CVIL", "Loc Description"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locationEntities);

                departmentEntities = new List<Domain.Base.Entities.Department>() 
                {
                    new Domain.Base.Entities.Department("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions", true),
                    new Domain.Base.Entities.Department("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business", true),
                    new Domain.Base.Entities.Department("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics", true),
                    new Domain.Base.Entities.Department("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science", true),
                    new Domain.Base.Entities.Department("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology", true),
                };
                referenceDataRepositoryMock.Setup(i => i.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(departmentEntities);

                positionPayEntities = new List<Domain.HumanResources.Entities.PositionPay>() 
                {
                    new Domain.HumanResources.Entities.PositionPay("1")
                    { 
                        BargainingUnit = "ALU",
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-00-02-62-40110-52001", 1)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-00-02-62-40110-52001" },
                        StartDate = new DateTime(2016, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        SalaryMaximum = "75000",
                        SalaryMinimum = "45000",
                        HostCountry = "USA"
                    },
                    new Domain.HumanResources.Entities.PositionPay("2")
                    { 
                        CycleWorkTimeUnits = "HRS",                       
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10408-51001", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10408-51001" },
                        StartDate = new DateTime(2016, 02, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        SalaryMaximum = "75000",
                        SalaryMinimum = "45000",
                        HostCountry = "CAN"
                    },
                    new Domain.HumanResources.Entities.PositionPay("3")
                    { 
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10406-51000", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10406-51000" },
                        StartDate = new DateTime(2016, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        HostCountry = "CANADA"

                    },
                    new Domain.HumanResources.Entities.PositionPay("4")
                    { 
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10503-51000", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10503-51000" },
                        StartDate = new DateTime(2017, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        HostCountry = "USA"
                    },
                };
                positionRepositoryMock.Setup(i => i.GetPositionPayByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(positionPayEntities);

                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("1")).ReturnsAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11");
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("2")).ReturnsAsync("c0ac0559-49d7-42b5-81f4-58b4cc50b1c3");

                positionRepositoryMock.Setup(i => i.GetPositionIdFromGuidAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11")).ReturnsAsync("1");

                employmentClassificationEntities = new List<Domain.HumanResources.Entities.EmploymentClassification>() 
                {
                    new Domain.HumanResources.Entities.EmploymentClassification("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(employmentClassificationEntities);

                bargainingUnitEntities = new List<Domain.HumanResources.Entities.BargainingUnit>() 
                {
                    new Domain.HumanResources.Entities.BargainingUnit("d5f5eafb-3192-4479-8dca-6fe79bbde6e4", "ALU", "American Labor Union"),
                    new Domain.HumanResources.Entities.BargainingUnit("43170c03-e3e7-4ffa-a7fd-7a32fdaf0e37", "NEA", "National Education Association")
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetBargainingUnitsAsync(It.IsAny<bool>())).ReturnsAsync(bargainingUnitEntities);

                var positionPayList = new List<string>(){ "11-00-02-62-40110-52001", "11-01-01-00-10408-51001", "11-01-01-00-10406-51000", "11-01-01-00-10503-51000" };
                positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    new Domain.HumanResources.Entities.Position("1", "Associate Registrar", "AR", "REG", new DateTime(2012, 07, 01), true)
                    { 
                        Guid = "ce4d68f6-257d-4052-92c8-17eed0f088fa",
                        PositionClass = "ADJ",
                        PositionDept = "Admissions",
                        PositionPayScheduleIds = positionPayList,
                        PositionLocation = "CD",
                        SupervisorPositionId = "1",
                        AlternateSupervisorPositionId = "2"
                    },
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67",
                        PositionDept = "Animal Science"
                    },
                    new Domain.HumanResources.Entities.Position("3", "Solfeggio Coach", "SC", "AGBU", new DateTime(2016, 01, 01), true)
                    { 
                        Guid = "7ea5142f-12f1-4ac9-b9f3-73e4205dfc11",
                        PositionDept = "Agriculture Business",
                        EndDate = new DateTime(2016, 05, 01)
                    },
                    new Domain.HumanResources.Entities.Position("4", "Assistant Professor of Anthropology", "APA", "ANSC", new DateTime(), true)
                    { 
                        Guid = "db8f690b-071f-4d98-8da8-d4312511a4c1",
                        PositionDept = "Anthropology"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);

                referenceDataRepositoryMock.Setup(i => i.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("2012/07/01");

            }
        }

        [TestClass]
        public class InstitutionPositionServiceTests_GETV11 : CurrentUserSetup
        {
            Mock<IPositionRepository> positionRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            InstitutionPositionService institutionPositionService;
            IEnumerable<Domain.HumanResources.Entities.Position> positionEntities;
            Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int> positionEntityTuple;

            IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> employmentClassificationEntities;
            IEnumerable<Domain.Base.Entities.Department> departmentEntities;
            IEnumerable<Domain.Base.Entities.Location> locationEntities;
            IEnumerable<Domain.HumanResources.Entities.BargainingUnit> bargainingUnitEntities;
            IEnumerable<Domain.HumanResources.Entities.PositionPay> positionPayEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                positionRepositoryMock = new Mock<IPositionRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewInstitutionPosition);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                institutionPositionService = new InstitutionPositionService(positionRepositoryMock.Object, hrReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                                baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                positionEntityTuple = null;
                positionEntities = null;
                employmentClassificationEntities = null;
                departmentEntities = null;
                locationEntities = null;
                bargainingUnitEntities = null;
                positionPayEntities = null;

                positionRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll2Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll2Async_EmptyTuple()
            {
                positionEntities = new List<Domain.HumanResources.Entities.Position>()
                {

                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, 0);
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll2Async_NoItmsInTuple()
            {
                positionEntityTuple = null;
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll2_Filter_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions2Async(offset, limit, "e43e7195-6eca-451d-b6c3-1e52fe540083",
                            It.IsAny<string>(), "d5f5eafb-3192-4479-8dca-6fe79bbde6e4", new List<string>() { "50aadc94-3b09-4bec-bca6-a9c588ee8c11" }, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll2_StartOn_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), "2012-07-01", It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll2_EndOn_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), "2012-07-01", It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GET_ById2()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = positionEntities.ToList()[0];
                positionPayEntities.ToList()[0].SalaryMinimum = "WrongType";
                positionPayEntities.ToList()[0].SalaryMaximum = "       ";
                positionRepositoryMock.Setup(i => i.GetPositionByGuidAsync(id)).ReturnsAsync(expected);
                var actual = await institutionPositionService.GetInstitutionPositionByGuid2Async(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
                Assert.AreEqual(expected.Title, actual.Title);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll2Async_NoPermissions_Exception()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                var actualsTuple = await institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll2Async_CovertMethodTryCatch_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                referenceDataRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actualsTuple = await institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll2_DateError_Async()
            {
                referenceDataRepositoryMock.Setup(i => i.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), "2012-07-01", It.IsAny<bool>());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll2Async_SupervisorPositionId_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentOutOfRangeException());
                var actualsTuple = await institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll2Async_AlternateSupervisorPositionId_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("1")).ReturnsAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11");
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("2")).ThrowsAsync(new ArgumentOutOfRangeException());
                var actualsTuple = await institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll2Async_ArgumentException()
            {
                var nullEntities = positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    null,
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67",
                        PositionDept = "Animal Science"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll2Async_NullGuid_ArgumentException()
            {
                positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "",
                        PositionDept = "Animal Science"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositions2Async(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstitutionPositions_GET_ById2_NullId_ArgumentNullException()
            {
                var actual = await institutionPositionService.GetInstitutionPositionByGuid2Async(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstitutionPositions_GET_ById2_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                positionRepositoryMock.Setup(i => i.GetPositionByGuidAsync(id)).ReturnsAsync(null);
                var actual = await institutionPositionService.GetInstitutionPositionByGuid2Async(id);
            }

            private void BuildData()
            {
                locationEntities = new List<Domain.Base.Entities.Location>() 
                {
                    new Domain.Base.Entities.Location("e43e7195-6eca-451d-b6c3-1e52fe540083", "BMA", "BMA Test"),
                    new Domain.Base.Entities.Location("95df0303-9b7f-4686-908f-1640b4881e23", "CD", "Central District Office"),
                    new Domain.Base.Entities.Location("ec49b053-7acc-411a-a766-8a7fc2f24ee3", "COE", "Colonial Ohio-East (coe) Campus"),
                    new Domain.Base.Entities.Location("eded9894-ea62-44f4-be8e-b141dfc00dba", "COEEA", "Coe-east"),
                    new Domain.Base.Entities.Location("17ff700b-8d20-43d7-be31-c34933baca75", "CVIL", "Loc Description"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locationEntities);

                departmentEntities = new List<Domain.Base.Entities.Department>() 
                {
                    new Domain.Base.Entities.Department("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions", true),
                    new Domain.Base.Entities.Department("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business", true),
                    new Domain.Base.Entities.Department("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics", true),
                    new Domain.Base.Entities.Department("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science", true),
                    new Domain.Base.Entities.Department("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology", true),
                };
                referenceDataRepositoryMock.Setup(i => i.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(departmentEntities);

                positionPayEntities = new List<Domain.HumanResources.Entities.PositionPay>() 
                {
                    new Domain.HumanResources.Entities.PositionPay("1")
                    { 
                        BargainingUnit = "ALU",
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-00-02-62-40110-52001", 1)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-00-02-62-40110-52001" },
                        PospayFndgGlNo = new List<string>(){ "11-00-02-62-40110-52001" },
                        PospayFndgPct = new List<decimal?>(){ 20 },
                        StartDate = new DateTime(2016, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        SalaryMaximum = "75000",
                        SalaryMinimum = "45000",
                        HostCountry = "USA"
                    },
                    new Domain.HumanResources.Entities.PositionPay("2")
                    { 
                        CycleWorkTimeUnits = "HRS",                       
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10408-51001", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10408-51001" },
                        PospayFndgGlNo = new List<string>(){ "11-01-01-00-10408-51001" },
                        PospayFndgPct = new List<decimal?>(){ 20 },
                        StartDate = new DateTime(2016, 02, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        SalaryMaximum = "75000",
                        SalaryMinimum = "45000",
                        HostCountry = "CAN"
                    },
                    new Domain.HumanResources.Entities.PositionPay("3")
                    { 
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10406-51000", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10406-51000" },
                        PospayFndgGlNo = new List<string>(){ "11-01-01-00-10406-51000" },
                        PospayFndgPct = new List<decimal?>(){ 20 },
                        StartDate = new DateTime(2016, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        HostCountry = "CANADA"

                    },
                    new Domain.HumanResources.Entities.PositionPay("4")
                    { 
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10503-51000", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10503-51000" },
                        PospayFndgGlNo = new List<string>(){ "11-01-01-00-10503-51000" },
                        PospayFndgPct = new List<decimal?>(){ 20 },
                        StartDate = new DateTime(2017, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        HostCountry = "USA"
                    },
                };
                positionRepositoryMock.Setup(i => i.GetPositionPayByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(positionPayEntities);

                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("1")).ReturnsAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11");
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("2")).ReturnsAsync("c0ac0559-49d7-42b5-81f4-58b4cc50b1c3");

                positionRepositoryMock.Setup(i => i.GetPositionIdFromGuidAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11")).ReturnsAsync("1");

                employmentClassificationEntities = new List<Domain.HumanResources.Entities.EmploymentClassification>() 
                {
                    new Domain.HumanResources.Entities.EmploymentClassification("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(employmentClassificationEntities);

                bargainingUnitEntities = new List<Domain.HumanResources.Entities.BargainingUnit>() 
                {
                    new Domain.HumanResources.Entities.BargainingUnit("d5f5eafb-3192-4479-8dca-6fe79bbde6e4", "ALU", "American Labor Union"),
                    new Domain.HumanResources.Entities.BargainingUnit("43170c03-e3e7-4ffa-a7fd-7a32fdaf0e37", "NEA", "National Education Association")
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetBargainingUnitsAsync(It.IsAny<bool>())).ReturnsAsync(bargainingUnitEntities);

                var positionPayList = new List<string>() { "11-00-02-62-40110-52001", "11-01-01-00-10408-51001", "11-01-01-00-10406-51000", "11-01-01-00-10503-51000" };
                positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    new Domain.HumanResources.Entities.Position("1", "Associate Registrar", "AR", "REG", new DateTime(2012, 07, 01), true)
                    { 
                        Guid = "ce4d68f6-257d-4052-92c8-17eed0f088fa",
                        PositionClass = "ADJ",
                        PositionDept = "Admissions",
                        PositionPayScheduleIds = positionPayList,
                        PositionLocation = "CD",
                        SupervisorPositionId = "1",
                        AlternateSupervisorPositionId = "2"
                    },
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67",
                        PositionDept = "Animal Science"
                    },
                    new Domain.HumanResources.Entities.Position("3", "Solfeggio Coach", "SC", "AGBU", new DateTime(2016, 01, 01), true)
                    { 
                        Guid = "7ea5142f-12f1-4ac9-b9f3-73e4205dfc11",
                        PositionDept = "Agriculture Business",
                        EndDate = new DateTime(2016, 05, 01)
                    },
                    new Domain.HumanResources.Entities.Position("4", "Assistant Professor of Anthropology", "APA", "ANSC", new DateTime(), true)
                    { 
                        Guid = "db8f690b-071f-4d98-8da8-d4312511a4c1",
                        PositionDept = "Anthropology"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);

                referenceDataRepositoryMock.Setup(i => i.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("2012/07/01");

            }
        }

        [TestClass]
        public class InstitutionPositionServiceTests_GETV12 : CurrentUserSetup
        {
            Mock<IPositionRepository> positionRepositoryMock;
            Mock<IHumanResourcesReferenceDataRepository> hrReferenceDataRepositoryMock;
            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            InstitutionPositionService institutionPositionService;
            IEnumerable<Domain.HumanResources.Entities.Position> positionEntities;
            Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int> positionEntityTuple;

            IEnumerable<Domain.HumanResources.Entities.EmploymentClassification> employmentClassificationEntities;
            IEnumerable<Domain.Base.Entities.Department> departmentEntities;
            IEnumerable<Domain.Base.Entities.Location> locationEntities;
            IEnumerable<Domain.HumanResources.Entities.BargainingUnit> bargainingUnitEntities;
            IEnumerable<Domain.HumanResources.Entities.PositionPay> positionPayEntities;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAnyPerson;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                positionRepositoryMock = new Mock<IPositionRepository>();
                hrReferenceDataRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(HumanResourcesPermissionCodes.ViewInstitutionPosition);
                personRole.AddPermission(permissionViewAnyPerson);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                institutionPositionService = new InstitutionPositionService(positionRepositoryMock.Object, hrReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object,
                                                baseConfigurationRepository, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                positionEntityTuple = null;
                positionEntities = null;
                employmentClassificationEntities = null;
                departmentEntities = null;
                locationEntities = null;
                bargainingUnitEntities = null;
                positionPayEntities = null;

                positionRepositoryMock = null;
                hrReferenceDataRepositoryMock = null;
                referenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll3Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll3Async_EmptyTuple()
            {
                positionEntities = new List<Domain.HumanResources.Entities.Position>()
                {

                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, 0);
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll3Async_NoItmsInTuple()
            {
                positionEntityTuple = null;
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll3_Filter_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions3Async(offset, limit, "", "e43e7195-6eca-451d-b6c3-1e52fe540083",
                            It.IsAny<string>(), "d5f5eafb-3192-4479-8dca-6fe79bbde6e4", new List<string>() { "50aadc94-3b09-4bec-bca6-a9c588ee8c11" }, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll3_StartOn_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), "2012-07-01", It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll3_EndOn_Async()
            {
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), "2012-07-01", It.IsAny<bool>());

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = positionEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.EndDate, actual.EndOn);
                    Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                    Assert.AreEqual(expected.StartDate, actual.StartOn);
                    Assert.AreEqual(expected.Title, actual.Title);
                }

            }

            [TestMethod]
            public async Task InstitutionPositions_GET_ById3()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                var expected = positionEntities.ToList()[0];
                positionPayEntities.ToList()[0].SalaryMinimum = "WrongType";
                positionPayEntities.ToList()[0].SalaryMaximum = "       ";
                positionRepositoryMock.Setup(i => i.GetPositionByGuidAsync(id)).ReturnsAsync(expected);
                var actual = await institutionPositionService.GetInstitutionPositionByGuid3Async(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.EndDate, actual.EndOn);
                Assert.AreEqual(expected.PositionAuthorizedDate, actual.AuthorizedOn);
                Assert.AreEqual(expected.StartDate, actual.StartOn);
                Assert.AreEqual(expected.Title, actual.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll3Async_NoPermissions_Exception()
            {
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { });
                var actualsTuple = await institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll3Async_CovertMethodTryCatch_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                referenceDataRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actualsTuple = await institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll3_DateError_Async()
            {
                referenceDataRepositoryMock.Setup(i => i.GetUnidataFormattedDate(It.IsAny<string>())).ThrowsAsync(new Exception());
                var actualsTuple =
                    await
                        institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), "2012-07-01", It.IsAny<bool>());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll3Async_SupervisorPositionId_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentOutOfRangeException());
                var actualsTuple = await institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            public async Task InstitutionPositions_GETAll3Async_AlternateSupervisorPositionId_Exception()
            {
                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("1")).ReturnsAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11");
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("2")).ThrowsAsync(new ArgumentOutOfRangeException());
                var actualsTuple = await institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll3Async_ArgumentException()
            {
                var nullEntities = positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    null,
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67",
                        PositionDept = "Animal Science"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task InstitutionPositions_GETAll3Async_NullGuid_ArgumentException()
            {
                positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "",
                        PositionDept = "Animal Science"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);
                var actualsTuple = await institutionPositionService.GetInstitutionPositions3Async(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstitutionPositions_GET_ById3_NullId_ArgumentNullException()
            {
                var actual = await institutionPositionService.GetInstitutionPositionByGuid3Async(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstitutionPositions_GET_ById3_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "ce4d68f6-257d-4052-92c8-17eed0f088fa";
                positionRepositoryMock.Setup(i => i.GetPositionByGuidAsync(id)).ReturnsAsync(null);
                var actual = await institutionPositionService.GetInstitutionPositionByGuid3Async(id);
            }

            private void BuildData()
            {
                locationEntities = new List<Domain.Base.Entities.Location>() 
                {
                    new Domain.Base.Entities.Location("e43e7195-6eca-451d-b6c3-1e52fe540083", "BMA", "BMA Test"),
                    new Domain.Base.Entities.Location("95df0303-9b7f-4686-908f-1640b4881e23", "CD", "Central District Office"),
                    new Domain.Base.Entities.Location("ec49b053-7acc-411a-a766-8a7fc2f24ee3", "COE", "Colonial Ohio-East (coe) Campus"),
                    new Domain.Base.Entities.Location("eded9894-ea62-44f4-be8e-b141dfc00dba", "COEEA", "Coe-east"),
                    new Domain.Base.Entities.Location("17ff700b-8d20-43d7-be31-c34933baca75", "CVIL", "Loc Description"),
                };
                referenceDataRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locationEntities);

                departmentEntities = new List<Domain.Base.Entities.Department>() 
                {
                    new Domain.Base.Entities.Department("d4ff9cf9-3300-4dca-b52e-59c905021893", "Admissions", "Admissions", true),
                    new Domain.Base.Entities.Department("161b17b2-5b8b-482b-8ff3-2454323aa8e6", "Agriculture Business", "Agriculture Business", true),
                    new Domain.Base.Entities.Department("5f8aeedd-8102-4d8f-8dbc-ecd32c374e87", "Agriculture Mechanics", "Agriculture Mechanics", true),
                    new Domain.Base.Entities.Department("ba66205d-79a8-4244-95f9-d2770a129a97", "Animal Science", "Animal Science", true),
                    new Domain.Base.Entities.Department("ccce9689-aab1-47ab-ae76-fa128fe8b97e", "Anthropology", "Anthropology", true),
                };
                referenceDataRepositoryMock.Setup(i => i.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(departmentEntities);

                positionPayEntities = new List<Domain.HumanResources.Entities.PositionPay>() 
                {
                    new Domain.HumanResources.Entities.PositionPay("1")
                    { 
                        BargainingUnit = "ALU",
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-00-02-62-40110-52001", 1)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-00-02-62-40110-52001" },
                        PospayFndgGlNo = new List<string>(){ "11-00-02-62-40110-52001" },
                        PospayFndgPct = new List<decimal?>(){ 20 },
                        StartDate = new DateTime(2016, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        SalaryMaximum = "75000",
                        SalaryMinimum = "45000",
                        HostCountry = "USA"
                    },
                    new Domain.HumanResources.Entities.PositionPay("2")
                    { 
                        CycleWorkTimeUnits = "HRS",                       
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10408-51001", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10408-51001" },
                        PospayFndgGlNo = new List<string>(){ "11-01-01-00-10408-51001" },
                        PospayFndgPct = new List<decimal?>(){ 20 },
                        StartDate = new DateTime(2016, 02, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        SalaryMaximum = "75000",
                        SalaryMinimum = "45000",
                        HostCountry = "CAN"
                    },
                    new Domain.HumanResources.Entities.PositionPay("3")
                    { 
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10406-51000", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10406-51000" },
                        PospayFndgGlNo = new List<string>(){ "11-01-01-00-10406-51000" },
                        PospayFndgPct = new List<decimal?>(){ 20 },
                        StartDate = new DateTime(2016, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        HostCountry = "CANADA"

                    },
                    new Domain.HumanResources.Entities.PositionPay("4")
                    { 
                        CycleWorkTimeUnits = "HRS", 
                        CycleWorkTimeAmount = 40,
                        FundingSource = new List<Domain.HumanResources.Entities.PositionFundingSource>()
                        {
                            new Domain.HumanResources.Entities.PositionFundingSource("11-01-01-00-10503-51000", 2)
                        },
                        PospayPrjFndgGlNo = new List<string>(){ "11-01-01-00-10503-51000" },
                        PospayFndgGlNo = new List<string>(){ "11-01-01-00-10503-51000" },
                        PospayFndgPct = new List<decimal?>(){ 20 },
                        StartDate = new DateTime(2017, 01, 01),
                        YearWorkTimeUnits = "HRS",
                        YearWorkTimeAmount = 2080,
                        HostCountry = "USA"
                    },
                };
                positionRepositoryMock.Setup(i => i.GetPositionPayByIdsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(positionPayEntities);

                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("1")).ReturnsAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11");
                positionRepositoryMock.Setup(i => i.GetPositionGuidFromIdAsync("2")).ReturnsAsync("c0ac0559-49d7-42b5-81f4-58b4cc50b1c3");

                positionRepositoryMock.Setup(i => i.GetPositionIdFromGuidAsync("50aadc94-3b09-4bec-bca6-a9c588ee8c11")).ReturnsAsync("1");

                employmentClassificationEntities = new List<Domain.HumanResources.Entities.EmploymentClassification>() 
                {
                    new Domain.HumanResources.Entities.EmploymentClassification("c1b91008-ba77-4b5b-8b77-84f5a7ae1632", "ADJ", "Adjunct Faculty", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("874dee09-8662-47e6-af0d-504c257493a3", "SUP", "Support", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("29391a8c-75e7-41e8-a5ff-5d7f7598b87c", "AS", "Anuj Test", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                    new Domain.HumanResources.Entities.EmploymentClassification("5b05410c-c94c-464a-98ee-684198bde60b", "ITS", "IT Support", Domain.HumanResources.Entities.EmploymentClassificationType.Employee),
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetEmploymentClassificationsAsync(It.IsAny<bool>())).ReturnsAsync(employmentClassificationEntities);

                bargainingUnitEntities = new List<Domain.HumanResources.Entities.BargainingUnit>() 
                {
                    new Domain.HumanResources.Entities.BargainingUnit("d5f5eafb-3192-4479-8dca-6fe79bbde6e4", "ALU", "American Labor Union"),
                    new Domain.HumanResources.Entities.BargainingUnit("43170c03-e3e7-4ffa-a7fd-7a32fdaf0e37", "NEA", "National Education Association")
                };
                hrReferenceDataRepositoryMock.Setup(i => i.GetBargainingUnitsAsync(It.IsAny<bool>())).ReturnsAsync(bargainingUnitEntities);

                var positionPayList = new List<string>() { "11-00-02-62-40110-52001", "11-01-01-00-10408-51001", "11-01-01-00-10406-51000", "11-01-01-00-10503-51000" };
                positionEntities = new List<Domain.HumanResources.Entities.Position>() 
                {
                    new Domain.HumanResources.Entities.Position("1", "Associate Registrar", "AR", "REG", new DateTime(2012, 07, 01), true)
                    { 
                        Guid = "ce4d68f6-257d-4052-92c8-17eed0f088fa",
                        PositionClass = "ADJ",
                        PositionDept = "Admissions",
                        PositionPayScheduleIds = positionPayList,
                        PositionLocation = "CD",
                        SupervisorPositionId = "1",
                        AlternateSupervisorPositionId = "2"
                    },
                    new Domain.HumanResources.Entities.Position("2", "Music Teacher", "MT", "MUSC", DateTime.Now.AddDays(1), true)
                    { 
                        Guid = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67",
                        PositionDept = "Animal Science"
                    },
                    new Domain.HumanResources.Entities.Position("3", "Solfeggio Coach", "SC", "AGBU", new DateTime(2016, 01, 01), true)
                    { 
                        Guid = "7ea5142f-12f1-4ac9-b9f3-73e4205dfc11",
                        PositionDept = "Agriculture Business",
                        EndDate = new DateTime(2016, 05, 01)
                    },
                    new Domain.HumanResources.Entities.Position("4", "Assistant Professor of Anthropology", "APA", "ANSC", new DateTime(), true)
                    { 
                        Guid = "db8f690b-071f-4d98-8da8-d4312511a4c1",
                        PositionDept = "Anthropology"
                    }
                };
                positionEntityTuple = new Tuple<IEnumerable<Domain.HumanResources.Entities.Position>, int>(positionEntities, positionEntities.Count());
                positionRepositoryMock.Setup(i => i.GetPositionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionEntityTuple);

                referenceDataRepositoryMock.Setup(i => i.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("2012/07/01");

            }
        }
    }
}
