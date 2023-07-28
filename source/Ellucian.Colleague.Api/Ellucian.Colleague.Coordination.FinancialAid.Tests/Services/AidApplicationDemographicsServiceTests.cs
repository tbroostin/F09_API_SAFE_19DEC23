/*Copyright 2023 Ellucian Company L.P. and its affiliates.*/

using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    /// <summary>
    /// Summary description for AidApplicationDemographicsServiceTests
    /// </summary>
    [TestClass]
    public class AidApplicationDemographicsServiceTests
    {
        [TestClass]
        public class AidApplicationDemographicsServiceTests_GET : CurrentUserSetup
        {
            Mock<IAidApplicationDemographicsRepository> aidApplicationDemographicsRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
            Mock<ICountryRepository> countryRepositoryMock;

            Mock<IReferenceDataRepository> refDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            AidApplicationDemographicsService aidApplicationDemographicsService;
            IEnumerable<Domain.FinancialAid.Entities.AidApplicationDemographics> aidApplicationDemographicsEntities;
            Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationDemographics>, int> aidApplicationDemographicsEntityTuple;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAidApplicationDemographics;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                aidApplicationDemographicsRepositoryMock = new Mock<IAidApplicationDemographicsRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                countryRepositoryMock = new Mock<ICountryRepository>();


                refDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Mock permissions
                permissionViewAidApplicationDemographics = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAidApplicationDemographics);
                personRole.AddPermission(permissionViewAidApplicationDemographics);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                aidApplicationDemographicsService = new AidApplicationDemographicsService(aidApplicationDemographicsRepositoryMock.Object, refDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               financialAidReferenceDataRepositoryMock.Object, countryRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                aidApplicationDemographicsEntityTuple = null;
                aidApplicationDemographicsEntities = null;
                aidApplicationDemographicsRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                financialAidReferenceDataRepositoryMock = null;
                countryRepositoryMock = null;
                refDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task AidApplicationDemographics_GETAllAsync()
            {
                AidApplicationDemographics filter = new AidApplicationDemographics();
                var actualsTuple =
                    await
                        aidApplicationDemographicsService.GetAidApplicationDemographicsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationDemographicsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationDemographics_GETAllAsync_EmptyTuple()
            {
                AidApplicationDemographics filter = new AidApplicationDemographics();
                aidApplicationDemographicsEntities = new List<Domain.FinancialAid.Entities.AidApplicationDemographics>()
                {

                };
                aidApplicationDemographicsEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationDemographics>, int>(aidApplicationDemographicsEntities, 0);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null)).ReturnsAsync(aidApplicationDemographicsEntityTuple);
                var actualsTuple = await aidApplicationDemographicsService.GetAidApplicationDemographicsAsync(offset, limit, filter);

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task AidApplicationDemographics_GETAllAsync_PersonIdFilter()
            {
                //For some reason need to reset repo's and service to truly run the tests
                string PersonId = "0000100";
                AidApplicationDemographics filter = new AidApplicationDemographics()
                {
                    PersonId = PersonId
                };
                aidApplicationDemographicsRepositoryMock = null;
                aidApplicationDemographicsRepositoryMock = new Mock<IAidApplicationDemographicsRepository>();
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null)).ReturnsAsync(aidApplicationDemographicsEntityTuple);

                aidApplicationDemographicsService = null;
                aidApplicationDemographicsService = new AidApplicationDemographicsService(aidApplicationDemographicsRepositoryMock.Object, refDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               financialAidReferenceDataRepositoryMock.Object, countryRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);

                var actualsTuple =
                    await
                        aidApplicationDemographicsService.GetAidApplicationDemographicsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationDemographicsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationDemographics_GETAllAsync_AidApplicationType()
            {
                AidApplicationDemographics filter = new AidApplicationDemographics() { ApplicationType = "CALISIR" };

                aidApplicationDemographicsRepositoryMock = null;
                aidApplicationDemographicsRepositoryMock = new Mock<IAidApplicationDemographicsRepository>();
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<string>(), null, null)).ReturnsAsync(aidApplicationDemographicsEntityTuple);

                aidApplicationDemographicsService = null;
                aidApplicationDemographicsService = new AidApplicationDemographicsService(aidApplicationDemographicsRepositoryMock.Object, refDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               financialAidReferenceDataRepositoryMock.Object, countryRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);

                var actualsTuple =
                   await
                       aidApplicationDemographicsService.GetAidApplicationDemographicsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationDemographicsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationDemographics_GETAllAsync_AidYear()
            {
                AidApplicationDemographics filter = new AidApplicationDemographics() { AidYear = "2023" };

                aidApplicationDemographicsRepositoryMock = null;
                aidApplicationDemographicsRepositoryMock = new Mock<IAidApplicationDemographicsRepository>();
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, It.IsAny<string>(), null)).ReturnsAsync(aidApplicationDemographicsEntityTuple);

                aidApplicationDemographicsService = null;
                aidApplicationDemographicsService = new AidApplicationDemographicsService(aidApplicationDemographicsRepositoryMock.Object, refDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               financialAidReferenceDataRepositoryMock.Object, countryRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);
                var actualsTuple =
                   await
                       aidApplicationDemographicsService.GetAidApplicationDemographicsAsync(offset, limit, filter);

                Assert.AreEqual(aidApplicationDemographicsEntityTuple.Item1.Count(), actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task AidApplicationDemographics_GETAllAsync_ApplicantAssignedId()
            {
                AidApplicationDemographics filter = new AidApplicationDemographics() { ApplicantAssignedId = "987654321" };
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, It.IsAny<string>())).ReturnsAsync(aidApplicationDemographicsEntityTuple);

                var actualsTuple =
                    await
                        aidApplicationDemographicsService.GetAidApplicationDemographicsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationDemographicsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationDemographics_GET_ById()
            {
                var id = "1";
                var expected = aidApplicationDemographicsEntities.ToList()[0];
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(id)).ReturnsAsync(expected);
                var actual = await aidApplicationDemographicsService.GetAidApplicationDemographicsByIdAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AidApplicationDemographics_GET_ById_NullId_ArgumentNullException()
            {
                var actual = await aidApplicationDemographicsService.GetAidApplicationDemographicsByIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AidApplicationDemographics_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "1";
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(id)).Throws<KeyNotFoundException>();
                var actual = await aidApplicationDemographicsService.GetAidApplicationDemographicsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AidApplicationDemographics_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "1";
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(id)).Throws<InvalidOperationException>();
                var actual = await aidApplicationDemographicsService.GetAidApplicationDemographicsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AidApplicationDemographics_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "1";
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(id)).Throws<RepositoryException>();
                var actual = await aidApplicationDemographicsService.GetAidApplicationDemographicsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AidApplicationDemographics_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "1";
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(id)).Throws<Exception>();
                var actual = await aidApplicationDemographicsService.GetAidApplicationDemographicsByIdAsync(id);
            }

            private void BuildData()
            {
                string personId = "0000100";
                string aidYear = "2023";
                string aidApplicationType = "CALISIR";
                string applicantAssignedId = "987654321";
                Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographicsEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics("1", personId, aidYear, aidApplicationType);

                aidApplicationDemographicsEntity.ApplicantAssignedId = applicantAssignedId;
                aidApplicationDemographicsEntity.LastName = "TestLastName";
                aidApplicationDemographicsEntity.OrigName = "La";
                aidApplicationDemographicsEntity.FirstName = "TestFirstName";
                aidApplicationDemographicsEntity.MiddleInitial = "MI";

                aidApplicationDemographicsEntity.Address = new Domain.FinancialAid.Entities.Address();
                aidApplicationDemographicsEntity.Address.AddressLine = "street 123";
                aidApplicationDemographicsEntity.Address.City = "TestCity";
                aidApplicationDemographicsEntity.Address.State = "PA";
                aidApplicationDemographicsEntity.Address.Country = "US";
                aidApplicationDemographicsEntity.Address.ZipCode = "99999";


                aidApplicationDemographicsEntity.BirthDate = new DateTime(2000, 1, 30);
                aidApplicationDemographicsEntity.PhoneNumber = "999-0000-000";
                aidApplicationDemographicsEntity.EmailAddress = "test@testing.com";

                aidApplicationDemographicsEntity.CitizenshipStatusType = Domain.FinancialAid.Entities.AidApplicationCitizenshipStatus.Citizen;

                aidApplicationDemographicsEntity.AlternatePhoneNumber = "111-0000-1000";
                aidApplicationDemographicsEntity.StudentTaxIdNumber = "1234567";

                aidApplicationDemographicsEntities = new List<Domain.FinancialAid.Entities.AidApplicationDemographics>()
                {
                    aidApplicationDemographicsEntity,
                    new Domain.FinancialAid.Entities.AidApplicationDemographics("2", "0000200", aidYear, aidApplicationType),
                    new Domain.FinancialAid.Entities.AidApplicationDemographics("3", personId, "2022", aidApplicationType),
                    new Domain.FinancialAid.Entities.AidApplicationDemographics("4", personId, aidYear, "CCPG")
                };

                aidApplicationDemographicsEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationDemographics>, int>(aidApplicationDemographicsEntities, aidApplicationDemographicsEntities.Count());
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null)).ReturnsAsync(aidApplicationDemographicsEntityTuple);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationDemographicsEntities.ToList()[0]);

            }
        }

        [TestClass]
        public class AidApplicationDemographicsServiceTests_PUT_POST : CurrentUserSetup
        {
            Mock<IAidApplicationDemographicsRepository> aidApplicationDemographicsRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
            Mock<ICountryRepository> countryRepositoryMock;

            Mock<IReferenceDataRepository> refDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            AidApplicationDemographicsService aidApplicationDemographicsService;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private AidApplicationDemographics aidApplicationDemographicsDto;
            private Domain.Entities.Permission permissionViewAidApplicationDemographics;
            private const string aidAppDemoId = "1";
            private const string personId = "0000100";
            private const string aidYear = "2023";
            private const string aidApplicationType = "CALISIR";
            private const string applicantAssignedId = "987654321";

            [TestInitialize]
            public void Initialize()
            {
                aidApplicationDemographicsRepositoryMock = new Mock<IAidApplicationDemographicsRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                countryRepositoryMock = new Mock<ICountryRepository>();


                refDataRepositoryMock = new Mock<IReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Mock permissions
                permissionViewAidApplicationDemographics = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.UpdateAidApplicationDemographics);
                personRole.AddPermission(permissionViewAidApplicationDemographics);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                aidApplicationDemographicsService = new AidApplicationDemographicsService(aidApplicationDemographicsRepositoryMock.Object, refDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               financialAidReferenceDataRepositoryMock.Object, countryRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                aidApplicationDemographicsDto = null;
                aidApplicationDemographicsRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                financialAidReferenceDataRepositoryMock = null;
                countryRepositoryMock = null;
                refDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            #region PUT
            [TestMethod]
            public async Task AidApplicationDemographics_PUT()
            {
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidAppDemoId, result.Id);
                Assert.AreEqual(personId, result.PersonId);
                Assert.AreEqual(aidYear, result.AidYear);
                Assert.AreEqual(aidApplicationType, result.ApplicationType);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_DtoNull_Exception()
            {
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_DtoIdNull_Exception()
            {
                aidApplicationDemographicsDto.Id = "";
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_PersonIdNull_Exception()
            {
                aidApplicationDemographicsDto.PersonId = "";
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_AidYearNull_Exception()
            {
                aidApplicationDemographicsDto.AidYear = "";
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_AidApplTypeNull_Exception()
            {
                aidApplicationDemographicsDto.ApplicationType = "";
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_FinAidYearEmptyList_Exception()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_FinAidYearNull_Exception()
            {
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_FinAidYearThrowsException()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ThrowsAsync(new Exception());

                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_AidApplicationTypesEmptyList_Exception()
            {
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationDemographicsDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_AidApplicationTypesNull_Exception()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_AidApplicationTypesThrowsException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_GetCountryEmptyList_Exception()
            {
                var countriesList = new List<Domain.Base.Entities.Country>();
                countryRepositoryMock.Setup(i => i.GetCountryCodesAsync(false)).ReturnsAsync(countriesList);

                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationDemographicsDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_GetCountryNull_Exception()
            {
                countryRepositoryMock.Setup(i => i.GetCountryCodesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_GetCountryThrowsException()
            {
                countryRepositoryMock.Setup(i => i.GetCountryCodesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_GetStatesEmptyList_Exception()
            {
                var statesList = new List<Domain.Base.Entities.State>();
                refDataRepositoryMock.Setup(i => i.GetStateCodesAsync(false)).ReturnsAsync(statesList);

                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_GetStatesNull_Exception()
            {
                refDataRepositoryMock.Setup(i => i.GetStateCodesAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_GetStatesThrowsException()
            {
                refDataRepositoryMock.Setup(i => i.GetStateCodesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_UpdateRepoThrowsRepositoryException()
            {
                aidApplicationDemographicsRepositoryMock.Setup(i => i.UpdateAidApplicationDemographicsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationDemographics>())).ThrowsAsync(new RepositoryException());
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_UpdateRepoThrowsException()
            {
                aidApplicationDemographicsRepositoryMock.Setup(i => i.UpdateAidApplicationDemographicsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationDemographics>())).ThrowsAsync(new Exception());
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_PUT_UpdateRepoReturnsNullException()
            {
                aidApplicationDemographicsRepositoryMock.Setup(i => i.UpdateAidApplicationDemographicsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationDemographics>())).ReturnsAsync(() => null);
                var result = await aidApplicationDemographicsService.PutAidApplicationDemographicsAsync(aidAppDemoId, aidApplicationDemographicsDto);
            }

            #endregion

            #region POST
            [TestMethod]
            public async Task AidApplicationDemographics_POST()
            {
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidAppDemoId, result.Id);
                Assert.AreEqual(personId, result.PersonId);
                Assert.AreEqual(aidYear, result.AidYear);
                Assert.AreEqual(aidApplicationType, result.ApplicationType);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_DtoNull_Exception()
            {
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_PersonIdNull_Exception()
            {
                aidApplicationDemographicsDto.PersonId = "";
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_AidYearNull_Exception()
            {
                aidApplicationDemographicsDto.AidYear = "";
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_AidApplTypeNull_Exception()
            {
                aidApplicationDemographicsDto.ApplicationType = "";
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_FinAidYearEmptyList_Exception()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_FinAidYearNull_Exception()
            {
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_FinAidYearThrowsException()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ThrowsAsync(new Exception());

                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_AidApplicationTypesEmptyList_Exception()
            {
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationDemographicsDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_AidApplicationTypesNull_Exception()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_AidApplicationTypesThrowsException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_GetCountryEmptyList_Exception()
            {
                var countriesList = new List<Domain.Base.Entities.Country>();
                countryRepositoryMock.Setup(i => i.GetCountryCodesAsync(false)).ReturnsAsync(countriesList);

                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationDemographicsDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_GetCountryNull_Exception()
            {
                countryRepositoryMock.Setup(i => i.GetCountryCodesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_GetCountryThrowsException()
            {
                countryRepositoryMock.Setup(i => i.GetCountryCodesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_GetStatesEmptyList_Exception()
            {
                var statesList = new List<Domain.Base.Entities.State>();
                refDataRepositoryMock.Setup(i => i.GetStateCodesAsync(false)).ReturnsAsync(statesList);

                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_GetStatesNull_Exception()
            {
                refDataRepositoryMock.Setup(i => i.GetStateCodesAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_GetStatesThrowsException()
            {
                refDataRepositoryMock.Setup(i => i.GetStateCodesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_CreateRepoThrowsRepositoryException()
            {
                aidApplicationDemographicsRepositoryMock.Setup(i => i.CreateAidApplicationDemographicsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationDemographics>())).ThrowsAsync(new RepositoryException());
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_CreateRepoThrowsException()
            {
                aidApplicationDemographicsRepositoryMock.Setup(i => i.CreateAidApplicationDemographicsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationDemographics>())).ThrowsAsync(new Exception());
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationDemographics_POST_CreateRepoReturnsNullException()
            {
                aidApplicationDemographicsRepositoryMock.Setup(i => i.CreateAidApplicationDemographicsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationDemographics>())).ReturnsAsync(() => null);
                var result = await aidApplicationDemographicsService.PostAidApplicationDemographicsAsync(aidApplicationDemographicsDto);
            }

            #endregion

            private void BuildData()
            {
                aidApplicationDemographicsDto = new AidApplicationDemographics();
                aidApplicationDemographicsDto.Id = aidAppDemoId;
                aidApplicationDemographicsDto.PersonId = personId;
                aidApplicationDemographicsDto.AidYear = aidYear;
                aidApplicationDemographicsDto.ApplicationType = aidApplicationType;
                aidApplicationDemographicsDto.ApplicantAssignedId = applicantAssignedId;
                aidApplicationDemographicsDto.LastName = "TestLastName";
                aidApplicationDemographicsDto.OrigName = "La";
                aidApplicationDemographicsDto.FirstName = "TestFirstName";
                aidApplicationDemographicsDto.MiddleInitial = "MI";

                aidApplicationDemographicsDto.Address = new Address();
                aidApplicationDemographicsDto.Address.AddressLine = "street 123";
                aidApplicationDemographicsDto.Address.City = "TestCity";
                aidApplicationDemographicsDto.Address.State = "PA";
                aidApplicationDemographicsDto.Address.Country = "US";
                aidApplicationDemographicsDto.Address.ZipCode = "99999";


                aidApplicationDemographicsDto.BirthDate = new DateTime(2000, 1, 30);
                aidApplicationDemographicsDto.PhoneNumber = "999-0000-000";
                aidApplicationDemographicsDto.EmailAddress = "test@testing.com";

                aidApplicationDemographicsDto.CitizenshipStatusType = AidApplicationCitizenshipStatus.Citizen;

                aidApplicationDemographicsDto.AlternatePhoneNumber = "111-0000-1000";
                aidApplicationDemographicsDto.StudentTaxIdNumber = "1234567";

                //GetFinancialAidYearsAsync
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                Domain.FinancialAid.Entities.FinancialAidYear finAidYear = new Domain.FinancialAid.Entities.FinancialAidYear("bb66b971-3ee0-4477-9bb7-539721f93434", aidYear, "DESC", "STATUS");
                finAidYears.Add(finAidYear);
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                //GetAidApplicationTypesAsync
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                Domain.Student.Entities.AidApplicationType aidApplicationTypeEntity = new Domain.Student.Entities.AidApplicationType(aidApplicationType, "DESC");
                aidApplicationTypes.Add(aidApplicationTypeEntity);
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                //GetCountryCodesAsync
                var countriesList = new List<Domain.Base.Entities.Country>();
                Domain.Base.Entities.Country countryEntity = new Domain.Base.Entities.Country("US", "United States Of India", "US");
                countriesList.Add(countryEntity);
                countryRepositoryMock.Setup(i => i.GetCountryCodesAsync(false)).ReturnsAsync(countriesList);


                //GetStateCodesAsync
                var statesList = new List<Domain.Base.Entities.State>();
                Domain.Base.Entities.State stateEntity = new Domain.Base.Entities.State("PA", "PHIL", "PA");
                statesList.Add(stateEntity);
                refDataRepositoryMock.Setup(i => i.GetStateCodesAsync(false)).ReturnsAsync(statesList);

                Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographicsEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics(aidAppDemoId, personId, aidYear, aidApplicationType);

                aidApplicationDemographicsEntity.ApplicantAssignedId = applicantAssignedId;
                aidApplicationDemographicsEntity.LastName = "TestLastName";
                aidApplicationDemographicsEntity.OrigName = "La";
                aidApplicationDemographicsEntity.FirstName = "TestFirstName";
                aidApplicationDemographicsEntity.MiddleInitial = "MI";

                aidApplicationDemographicsEntity.Address = new Domain.FinancialAid.Entities.Address();
                aidApplicationDemographicsEntity.Address.AddressLine = "street 123";
                aidApplicationDemographicsEntity.Address.City = "TestCity";
                aidApplicationDemographicsEntity.Address.State = "PA";
                aidApplicationDemographicsEntity.Address.Country = "US";
                aidApplicationDemographicsEntity.Address.ZipCode = "99999";


                aidApplicationDemographicsEntity.BirthDate = new DateTime(2000, 1, 30);
                aidApplicationDemographicsEntity.PhoneNumber = "999-0000-000";
                aidApplicationDemographicsEntity.EmailAddress = "test@testing.com";

                aidApplicationDemographicsEntity.CitizenshipStatusType = Domain.FinancialAid.Entities.AidApplicationCitizenshipStatus.Citizen;

                aidApplicationDemographicsEntity.AlternatePhoneNumber = "111-0000-1000";
                aidApplicationDemographicsEntity.StudentTaxIdNumber = "1234567";

                //aidApplicationDemographicsEntities = new List<Domain.FinancialAid.Entities.AidApplicationDemographics>()
                //{
                //    aidApplicationDemographicsEntity,
                //    new Domain.FinancialAid.Entities.AidApplicationDemographics("2", "0000200", aidYear, aidApplicationType),
                //    new Domain.FinancialAid.Entities.AidApplicationDemographics("3", personId, "2022", aidApplicationType),
                //    new Domain.FinancialAid.Entities.AidApplicationDemographics("4", personId, aidYear, "CCPG")
                //};

                //aidApplicationDemographicsEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationDemographics>, int>(aidApplicationDemographicsEntities, aidApplicationDemographicsEntities.Count());
                //aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null)).ReturnsAsync(aidApplicationDemographicsEntityTuple);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationDemographicsEntity);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.CreateAidApplicationDemographicsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationDemographics>())).ReturnsAsync(aidApplicationDemographicsEntity);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.UpdateAidApplicationDemographicsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationDemographics>())).ReturnsAsync(aidApplicationDemographicsEntity);
            }
        }
    }
}
