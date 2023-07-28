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
    /// Summary description for AidApplicationAdditionalInfoServiceTests
    /// </summary>
    [TestClass]
    public class AidApplicationAdditionalInfoServiceTests
    {
        [TestClass]
        public class AidApplicationAdditionalInfoServiceTests_GET : CurrentUserSetup
        {
            Mock<IAidApplicationAdditionalInfoRepository> aidApplicationAdditionalInfoRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IAidApplicationDemographicsRepository> aidApplicationDemographicsRepositoryMock;
            Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            AidApplicationAdditionalInfoService aidApplicationAdditionalInfoService;
            IEnumerable<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo> aidApplicationAdditionalInfoEntities;
            Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>, int> aidApplicationAdditionalInfoEntityTuple;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAidApplicationAdditionalInfo;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                aidApplicationAdditionalInfoRepositoryMock = new Mock<IAidApplicationAdditionalInfoRepository>();
                aidApplicationDemographicsRepositoryMock = new Mock<IAidApplicationDemographicsRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new StudentUserFactory();

                // Mock permissions
                permissionViewAidApplicationAdditionalInfo = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAidApplicationAdditionalInfo);
                personRole.AddPermission(permissionViewAidApplicationAdditionalInfo);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                aidApplicationAdditionalInfoService = new AidApplicationAdditionalInfoService(aidApplicationAdditionalInfoRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);
                var entityToDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo, AidApplicationAdditionalInfo>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo, AidApplicationAdditionalInfo>()).Returns(entityToDtoAdapter);
            }

            [TestCleanup]
            public void Cleanup()
            {
                aidApplicationAdditionalInfoEntityTuple = null;
                aidApplicationAdditionalInfoEntities = null;
                aidApplicationAdditionalInfoRepositoryMock = null;
                aidApplicationDemographicsRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                financialAidReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task AidApplicationAdditionalInfo_GETAllAsync()
            {
                AidApplicationAdditionalInfo filter = new AidApplicationAdditionalInfo();
                var actualsTuple =
                    await
                        aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationAdditionalInfoEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationAdditionalInfo_GETAllAsync_EmptyTuple()
            {
                AidApplicationAdditionalInfo filter = new AidApplicationAdditionalInfo();
                aidApplicationAdditionalInfoEntities = new List<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>()
                {

                };
                aidApplicationAdditionalInfoEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>, int>(aidApplicationAdditionalInfoEntities, 0);
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null)).ReturnsAsync(aidApplicationAdditionalInfoEntityTuple);
                var actualsTuple = await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(offset, limit, filter);

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task AidApplicationAdditionalInfo_GETAllAsync_AppDemoIdFilter()
            {
                string AppDemoId = "70";
                AidApplicationAdditionalInfo filter = new AidApplicationAdditionalInfo()
                {
                    AppDemoId = AppDemoId
                };
                aidApplicationAdditionalInfoRepositoryMock = null;
                aidApplicationAdditionalInfoRepositoryMock = new Mock<IAidApplicationAdditionalInfoRepository>();
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null)).ReturnsAsync(aidApplicationAdditionalInfoEntityTuple);

                aidApplicationAdditionalInfoService = null;
                aidApplicationAdditionalInfoService = new AidApplicationAdditionalInfoService(aidApplicationAdditionalInfoRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationAdditionalInfoEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationAdditionalInfo_GETAllAsync_PersonIdFilter()
            {
                //For some reason need to reset repo's and service to truly run the tests
                string PersonId = "0000100";
                AidApplicationAdditionalInfo filter = new AidApplicationAdditionalInfo()
                {
                    PersonId = PersonId
                };
                aidApplicationAdditionalInfoRepositoryMock = null;
                aidApplicationAdditionalInfoRepositoryMock = new Mock<IAidApplicationAdditionalInfoRepository>();
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<string>(), null, null, null)).ReturnsAsync(aidApplicationAdditionalInfoEntityTuple);

                aidApplicationAdditionalInfoService = null;
                aidApplicationAdditionalInfoService = new AidApplicationAdditionalInfoService(aidApplicationAdditionalInfoRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationAdditionalInfoEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationAdditionalInfo_GETAllAsync_AidApplicationType()
            {
                AidApplicationAdditionalInfo filter = new AidApplicationAdditionalInfo() { ApplicationType = "CALISIR" };

                aidApplicationAdditionalInfoRepositoryMock = null;
                aidApplicationAdditionalInfoRepositoryMock = new Mock<IAidApplicationAdditionalInfoRepository>();
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, It.IsAny<string>(), null, null)).ReturnsAsync(aidApplicationAdditionalInfoEntityTuple);

                aidApplicationAdditionalInfoService = null;
                aidApplicationAdditionalInfoService = new AidApplicationAdditionalInfoService(aidApplicationAdditionalInfoRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationAdditionalInfoEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationAdditionalInfo_GETAllAsync_AidYear()
            {
                AidApplicationAdditionalInfo filter = new AidApplicationAdditionalInfo() { AidYear = "2023" };

                aidApplicationAdditionalInfoRepositoryMock = null;
                aidApplicationAdditionalInfoRepositoryMock = new Mock<IAidApplicationAdditionalInfoRepository>();
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, It.IsAny<string>(), null)).ReturnsAsync(aidApplicationAdditionalInfoEntityTuple);

                aidApplicationAdditionalInfoService = null;
                aidApplicationAdditionalInfoService = new AidApplicationAdditionalInfoService(aidApplicationAdditionalInfoRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(offset, limit, filter);

                Assert.AreEqual(aidApplicationAdditionalInfoEntityTuple.Item1.Count(), actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task AidApplicationAdditionalInfo_GETAllAsync_ApplicantAssignedId()
            {
                AidApplicationAdditionalInfo filter = new AidApplicationAdditionalInfo() { ApplicantAssignedId = "987654321" };
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, It.IsAny<string>())).ReturnsAsync(aidApplicationAdditionalInfoEntityTuple);

                aidApplicationAdditionalInfoService = null;
                aidApplicationAdditionalInfoService = new AidApplicationAdditionalInfoService(aidApplicationAdditionalInfoRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationAdditionalInfoEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AidApplicationAdditionalInfo_GET_ReturnsNullEntity_KeyNotFoundException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null))
                    .Throws<KeyNotFoundException>();
                await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AidApplicationAdditionalInfo_GET_ReturnsNullEntity_InvalidOperationException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null))
                    .Throws<InvalidOperationException>();
                await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AidApplicationAdditionalInfo_GET_ReturnsNullEntity_RepositoryException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null))
                    .Throws<RepositoryException>();
                await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AidApplicationAdditionalInfo_GET_ReturnsNullEntity_Exception()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(x => x.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null))
                    .Throws<Exception>();
                await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            public async Task AidApplicationAdditionalInfo_GET_ById()
            {
                var id = "1";
                var expected = aidApplicationAdditionalInfoEntities.ToList()[0];
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(id)).ReturnsAsync(expected);
                var actual = await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoByIdAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AidApplicationAdditionalInfo_GET_ById_NullId_ArgumentNullException()
            {
                var actual = await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoByIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AidApplicationAdditionalInfo_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "1";
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(id)).Throws<KeyNotFoundException>();
                var actual = await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AidApplicationAdditionalInfo_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "1";
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(id)).Throws<InvalidOperationException>();
                var actual = await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AidApplicationAdditionalInfo_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "1";
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(id)).Throws<RepositoryException>();
                var actual = await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AidApplicationAdditionalInfo_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "1";
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(id)).Throws<Exception>();
                var actual = await aidApplicationAdditionalInfoService.GetAidApplicationAdditionalInfoByIdAsync(id);
            }

            private void BuildData()
            {

                Domain.FinancialAid.Entities.AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity = new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo("1", "1");

                aidApplicationAdditionalInfoEntity.PersonId = "0000100";
                aidApplicationAdditionalInfoEntity.ApplicationType = "CALISIR";
                aidApplicationAdditionalInfoEntity.AidYear = "2023";
                aidApplicationAdditionalInfoEntity.ApplicantAssignedId = "987654321";
                aidApplicationAdditionalInfoEntity.StudentStateId = "182739475";
                aidApplicationAdditionalInfoEntity.FosterCare = true;
                aidApplicationAdditionalInfoEntity.ApplicationCounty = "Orange";
                aidApplicationAdditionalInfoEntity.WardshipState = "CA";
                aidApplicationAdditionalInfoEntity.ChafeeConsideration = true;
                aidApplicationAdditionalInfoEntity.CreateCcpgRecord = true;
                aidApplicationAdditionalInfoEntity.User1 = "testUser1";
                aidApplicationAdditionalInfoEntity.User2 = "testUser2";
                aidApplicationAdditionalInfoEntity.User3 = "testUser3";
                aidApplicationAdditionalInfoEntity.User4 = "testUser4";
                aidApplicationAdditionalInfoEntity.User5 = "testUser5";
                aidApplicationAdditionalInfoEntity.User6 = "testUser6";
                aidApplicationAdditionalInfoEntity.User7 = "testUser7";
                aidApplicationAdditionalInfoEntity.User8 = "testUser8";
                aidApplicationAdditionalInfoEntity.User9 = "testUser9";
                aidApplicationAdditionalInfoEntity.User10 = "testUser10";
                aidApplicationAdditionalInfoEntity.User11 = "testUser11";
                aidApplicationAdditionalInfoEntity.User12 = "testUser12";
                aidApplicationAdditionalInfoEntity.User13 = "testUser13";
                aidApplicationAdditionalInfoEntity.User14 = "testUser14";
                aidApplicationAdditionalInfoEntity.User15 = new DateTime(2000, 1, 30);
                aidApplicationAdditionalInfoEntity.User16 = new DateTime(2000, 2, 20);
                aidApplicationAdditionalInfoEntity.User17 = new DateTime(2000, 3, 30);
                aidApplicationAdditionalInfoEntity.User18 = new DateTime(2000, 4, 30);
                aidApplicationAdditionalInfoEntity.User19 = new DateTime(2000, 5, 30);
                aidApplicationAdditionalInfoEntity.User21 = new DateTime(2000, 6, 30);


                aidApplicationAdditionalInfoEntities = new List<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>()
                {
                    aidApplicationAdditionalInfoEntity,
                    new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo("2", "2"),
                    new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo("3", "3"),
                    new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo("4", "4")
                };

                Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographicsEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics("1", "0000100", "2023", "CALISIR");

                aidApplicationAdditionalInfoEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>, int>(aidApplicationAdditionalInfoEntities, aidApplicationAdditionalInfoEntities.Count());
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null)).ReturnsAsync(aidApplicationAdditionalInfoEntityTuple);
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationAdditionalInfoEntities.ToList()[0]);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationDemographicsEntity);

            }
        }

        [TestClass]
        public class AidApplicationAdditionalInfoServiceTests_PUT_POST : CurrentUserSetup
        {
            Mock<IAidApplicationAdditionalInfoRepository> aidApplicationAdditionalInfoRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IAidApplicationDemographicsRepository> aidApplicationDemographicsRepositoryMock;
            Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            AidApplicationAdditionalInfoService aidApplicationAdditionalInfoService;
            IEnumerable<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo> aidApplicationAdditionalInfoEntities;
            Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>, int> aidApplicationAdditionalInfoEntityTuple;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private AidApplicationAdditionalInfo aidApplicationAdditionalInfoDto;
            private Domain.Entities.Permission permissionViewAidApplicationAdditionalInfo;



            private const string aidAppDemoId = "1";
            private const string personId = "0000100";
            private const string aidYear = "2023";
            private const string aidApplicationType = "CALISIR";
            private const string applicantAssignedId = "987654321";

            [TestInitialize]
            public void Initialize()
            {
                aidApplicationAdditionalInfoRepositoryMock = new Mock<IAidApplicationAdditionalInfoRepository>();
                aidApplicationDemographicsRepositoryMock = new Mock<IAidApplicationDemographicsRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                // Set up current user
                currentUserFactory = new StudentUserFactory();

                // Mock permissions
                permissionViewAidApplicationAdditionalInfo = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAidApplicationAdditionalInfo);
                personRole.AddPermission(permissionViewAidApplicationAdditionalInfo);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                aidApplicationAdditionalInfoService = new AidApplicationAdditionalInfoService(aidApplicationAdditionalInfoRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);
                var dtoToEntityAdapter = new AutoMapperAdapter<AidApplicationAdditionalInfo, Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(x => x.GetAdapter<AidApplicationAdditionalInfo, Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>()).Returns(dtoToEntityAdapter);

                var entityToDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo, AidApplicationAdditionalInfo>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo, AidApplicationAdditionalInfo>()).Returns(entityToDtoAdapter);

            }

            [TestCleanup]
            public void Cleanup()
            {
                aidApplicationAdditionalInfoDto = null;
                aidApplicationAdditionalInfoRepositoryMock = null;
                aidApplicationDemographicsRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                financialAidReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            #region PUT
            [TestMethod]
            public async Task AidApplicationAdditionalInfo_PUT()
            {
                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidAppDemoId, result.AppDemoId);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_DtoNull_Exception()
            {
                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_DtoAppDemoIdNull_Exception()
            {
                aidApplicationAdditionalInfoDto.AppDemoId = "";
                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_FinAidYearEmptyList_Exception()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_FinAidYearNull_Exception()
            {
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_FinAidYearThrowsException()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ThrowsAsync(new Exception());

                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_AidApplicationTypesEmptyList_Exception()
            {
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationAdditionalInfoDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_AidApplicationTypesNull_Exception()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_AidApplicationTypesThrowsException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_UpdateRepoThrowsRepositoryException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.UpdateAidApplicationAdditionalInfoAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>())).ThrowsAsync(new RepositoryException());
                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_UpdateRepoThrowsException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.UpdateAidApplicationAdditionalInfoAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>())).ThrowsAsync(new Exception());
                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_PUT_UpdateRepoReturnsNullException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.UpdateAidApplicationAdditionalInfoAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>())).ReturnsAsync(() => null);
                var result = await aidApplicationAdditionalInfoService.PutAidApplicationAdditionalInfoAsync(aidAppDemoId, aidApplicationAdditionalInfoDto);
            }

            #endregion

            #region POST
            [TestMethod]
            public async Task AidApplicationAdditionalInfo_POST()
            {
                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidAppDemoId, result.AppDemoId);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_DtoNull_Exception()
            {
                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_AppDemoIdNull_Exception()
            {
                aidApplicationAdditionalInfoDto.AppDemoId = "";
                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_FinAidYearEmptyList_Exception()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_FinAidYearNull_Exception()
            {
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_FinAidYearThrowsException()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ThrowsAsync(new Exception());

                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_AidApplicationTypesEmptyList_Exception()
            {
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationAdditionalInfoDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_AidApplicationTypesNull_Exception()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_AidApplicationTypesThrowsException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_CreateRepoThrowsRepositoryException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.CreateAidApplicationAdditionalInfoAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>())).ThrowsAsync(new RepositoryException());
                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_CreateRepoThrowsException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.CreateAidApplicationAdditionalInfoAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>())).ThrowsAsync(new Exception());
                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationAdditionalInfo_POST_CreateRepoReturnsNullException()
            {
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.CreateAidApplicationAdditionalInfoAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>())).ReturnsAsync(() => null);
                var result = await aidApplicationAdditionalInfoService.PostAidApplicationAdditionalInfoAsync(aidApplicationAdditionalInfoDto);
            }

            #endregion

            private void BuildData()
            {
                aidApplicationAdditionalInfoDto = new AidApplicationAdditionalInfo();

                aidApplicationAdditionalInfoDto.Id = aidAppDemoId;
                aidApplicationAdditionalInfoDto.AppDemoId = aidAppDemoId;
                aidApplicationAdditionalInfoDto.PersonId = personId;
                aidApplicationAdditionalInfoDto.ApplicationType = aidApplicationType;
                aidApplicationAdditionalInfoDto.AidYear = aidYear;
                aidApplicationAdditionalInfoDto.ApplicantAssignedId = applicantAssignedId;
                aidApplicationAdditionalInfoDto.StudentStateId = "182739475";
                aidApplicationAdditionalInfoDto.FosterCare = true;
                aidApplicationAdditionalInfoDto.ApplicationCounty = "Orange";
                aidApplicationAdditionalInfoDto.WardshipState = "CA";
                aidApplicationAdditionalInfoDto.ChafeeConsideration = true;
                aidApplicationAdditionalInfoDto.CreateCcpgRecord = true;
                aidApplicationAdditionalInfoDto.User1 = "testUser1";
                aidApplicationAdditionalInfoDto.User2 = "testUser2";
                aidApplicationAdditionalInfoDto.User3 = "testUser3";
                aidApplicationAdditionalInfoDto.User4 = "testUser4";
                aidApplicationAdditionalInfoDto.User5 = "testUser5";
                aidApplicationAdditionalInfoDto.User6 = "testUser6";
                aidApplicationAdditionalInfoDto.User7 = "testUser7";
                aidApplicationAdditionalInfoDto.User8 = "testUser8";
                aidApplicationAdditionalInfoDto.User9 = "testUser9";
                aidApplicationAdditionalInfoDto.User10 = "testUser10";
                aidApplicationAdditionalInfoDto.User11 = "testUser11";
                aidApplicationAdditionalInfoDto.User12 = "testUser12";
                aidApplicationAdditionalInfoDto.User13 = "testUser13";
                aidApplicationAdditionalInfoDto.User14 = "testUser14";
                aidApplicationAdditionalInfoDto.User15 = new DateTime(2000, 1, 30);
                aidApplicationAdditionalInfoDto.User16 = new DateTime(2000, 2, 20);
                aidApplicationAdditionalInfoDto.User17 = new DateTime(2000, 3, 30);
                aidApplicationAdditionalInfoDto.User18 = new DateTime(2000, 4, 30);
                aidApplicationAdditionalInfoDto.User19 = new DateTime(2000, 5, 30);
                aidApplicationAdditionalInfoDto.User21 = new DateTime(2000, 6, 30);

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

                Domain.FinancialAid.Entities.AidApplicationAdditionalInfo aidApplicationAdditionalInfoEntity = new Domain.FinancialAid.Entities.AidApplicationAdditionalInfo(aidAppDemoId, aidAppDemoId);

                aidApplicationAdditionalInfoEntity.PersonId = personId;
                aidApplicationAdditionalInfoEntity.ApplicationType = aidApplicationType;
                aidApplicationAdditionalInfoEntity.AidYear = aidYear;
                aidApplicationAdditionalInfoEntity.ApplicantAssignedId = applicantAssignedId;
                aidApplicationAdditionalInfoEntity.StudentStateId = "182739475";
                aidApplicationAdditionalInfoEntity.FosterCare = true;
                aidApplicationAdditionalInfoEntity.ApplicationCounty = "Orange";
                aidApplicationAdditionalInfoEntity.WardshipState = "CA";
                aidApplicationAdditionalInfoEntity.ChafeeConsideration = true;
                aidApplicationAdditionalInfoEntity.CreateCcpgRecord = true;
                aidApplicationAdditionalInfoEntity.User1 = "testUser1";
                aidApplicationAdditionalInfoEntity.User2 = "testUser2";
                aidApplicationAdditionalInfoEntity.User3 = "testUser3";
                aidApplicationAdditionalInfoEntity.User4 = "testUser4";
                aidApplicationAdditionalInfoEntity.User5 = "testUser5";
                aidApplicationAdditionalInfoEntity.User6 = "testUser6";
                aidApplicationAdditionalInfoEntity.User7 = "testUser7";
                aidApplicationAdditionalInfoEntity.User8 = "testUser8";
                aidApplicationAdditionalInfoEntity.User9 = "testUser9";
                aidApplicationAdditionalInfoEntity.User10 = "testUser10";
                aidApplicationAdditionalInfoEntity.User11 = "testUser11";
                aidApplicationAdditionalInfoEntity.User12 = "testUser12";
                aidApplicationAdditionalInfoEntity.User13 = "testUser13";
                aidApplicationAdditionalInfoEntity.User14 = "testUser14";
                aidApplicationAdditionalInfoEntity.User15 = new DateTime(2000, 1, 30);
                aidApplicationAdditionalInfoEntity.User16 = new DateTime(2000, 7, 30);
                aidApplicationAdditionalInfoEntity.User17 = new DateTime(2000, 3, 30);
                aidApplicationAdditionalInfoEntity.User18 = new DateTime(2000, 4, 30);
                aidApplicationAdditionalInfoEntity.User19 = new DateTime(2000, 5, 30);
                aidApplicationAdditionalInfoEntity.User21 = new DateTime(2000, 6, 30);

                Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographicsEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics(aidAppDemoId, personId, aidYear, aidApplicationType);

                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.GetAidApplicationAdditionalInfoByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationAdditionalInfoEntity);
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.CreateAidApplicationAdditionalInfoAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>())).ReturnsAsync(aidApplicationAdditionalInfoEntity);
                aidApplicationAdditionalInfoRepositoryMock.Setup(i => i.UpdateAidApplicationAdditionalInfoAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationAdditionalInfo>())).ReturnsAsync(aidApplicationAdditionalInfoEntity);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationDemographicsEntity);
            }
        }
    }
}
