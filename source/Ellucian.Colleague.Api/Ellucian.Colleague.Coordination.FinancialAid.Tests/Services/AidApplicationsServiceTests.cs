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
    /// Summary description for AidApplicationsServiceTests
    /// </summary>
    [TestClass]
    public class AidApplicationsServiceTests
    {
        [TestClass]
        public class AidApplicationsServiceTests_GET : CurrentUserSetup
        {
            Mock<IAidApplicationsRepository> aidApplicationsRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IAidApplicationDemographicsRepository> aidApplicationDemographicsRepositoryMock;
            Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            AidApplicationsService aidApplicationsService;
            IEnumerable<Domain.FinancialAid.Entities.AidApplications> aidApplicationsEntities;
            Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplications>, int> aidApplicationsEntityTuple;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAidApplications;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                aidApplicationsRepositoryMock = new Mock<IAidApplicationsRepository>();
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
                permissionViewAidApplications = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAidApplications);
                personRole.AddPermission(permissionViewAidApplications);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                aidApplicationsService = new AidApplicationsService(aidApplicationsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                                studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);
                var entityToDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.AidApplications, AidApplications>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.FinancialAid.Entities.AidApplications, AidApplications>()).Returns(entityToDtoAdapter);
            }

            [TestCleanup]
            public void Cleanup()
            {
                aidApplicationsEntityTuple = null;
                aidApplicationsEntities = null;
                aidApplicationsRepositoryMock = null;
                aidApplicationDemographicsRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                financialAidReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task AidApplications_GETAllAsync()
            {
                AidApplications filter = new AidApplications();
                var actualsTuple =
                    await
                        aidApplicationsService.GetAidApplicationsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplications_GETAllAsync_EmptyTuple()
            {
                AidApplications filter = new AidApplications();
                aidApplicationsEntities = new List<Domain.FinancialAid.Entities.AidApplications>()
                {

                };
                aidApplicationsEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplications>, int>(aidApplicationsEntities, 0);
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null)).ReturnsAsync(aidApplicationsEntityTuple);
                var actualsTuple = await aidApplicationsService.GetAidApplicationsAsync(offset, limit, filter);

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task AidApplications_GETAllAsync_AppDemoIdFilter()
            {
                string AidAppDemoId = "70";
                AidApplications filter = new AidApplications()
                {
                    AppDemoID = AidAppDemoId
                };
                aidApplicationsRepositoryMock = null;
                aidApplicationsRepositoryMock = new Mock<IAidApplicationsRepository>();
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null)).ReturnsAsync(aidApplicationsEntityTuple);

                aidApplicationsService = null;
                aidApplicationsService = new AidApplicationsService(aidApplicationsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                                studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);

                var actualsTuple =
                    await
                        aidApplicationsService.GetAidApplicationsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplications_GETAllAsync_PersonIdFilter()
            {
                //For some reason need to reset repo's and service to truly run the tests
                string PersonId = "0000100";
                AidApplications filter = new AidApplications()
                {
                    PersonId = PersonId
                };
                aidApplicationsRepositoryMock = null;
                aidApplicationsRepositoryMock = new Mock<IAidApplicationsRepository>();
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<string>(), null, null, null)).ReturnsAsync(aidApplicationsEntityTuple);

                aidApplicationsService = null;
                aidApplicationsService = new AidApplicationsService(aidApplicationsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                                studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);
                var actualsTuple =
                    await
                        aidApplicationsService.GetAidApplicationsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplications_GETAllAsync_AidApplicationType()
            {
                AidApplications filter = new AidApplications() {ApplicationType = "CALISIR" };

                aidApplicationsRepositoryMock = null;
                aidApplicationsRepositoryMock = new Mock<IAidApplicationsRepository>();
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, It.IsAny<string>(), null, null)).ReturnsAsync(aidApplicationsEntityTuple);

                aidApplicationsService = null;
                aidApplicationsService = new AidApplicationsService(aidApplicationsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                                studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);

                var actualsTuple =
                    await
                        aidApplicationsService.GetAidApplicationsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplications_GETAllAsync_AidYear()
            {
                AidApplications filter = new AidApplications() { AidYear = "2023" };

                aidApplicationsRepositoryMock = null;
                aidApplicationsRepositoryMock = new Mock<IAidApplicationsRepository>();
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, It.IsAny<string>(), null)).ReturnsAsync(aidApplicationsEntityTuple);

                aidApplicationsService = null;
                aidApplicationsService = new AidApplicationsService(aidApplicationsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                                studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);

                var actualsTuple =
                    await
                        aidApplicationsService.GetAidApplicationsAsync(offset, limit, filter);

                Assert.AreEqual(aidApplicationsEntityTuple.Item1.Count(), actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task AidApplications_GETAllAsync_ApplicantAssignedId()
            {
                AidApplications filter = new AidApplications() { ApplicantAssignedId = "987654321" };
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, It.IsAny<string>())).ReturnsAsync(aidApplicationsEntityTuple);

                aidApplicationsService = null;
                aidApplicationsService = new AidApplicationsService(aidApplicationsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                                studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);

                var actualsTuple =
                    await
                        aidApplicationsService.GetAidApplicationsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AidApplications_GET_ReturnsNullEntity_KeyNotFoundException()
            {
                aidApplicationsRepositoryMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null))
                    .Throws<KeyNotFoundException>();
                await aidApplicationsService.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AidApplications_GET_ReturnsNullEntity_InvalidOperationException()
            {
                aidApplicationsRepositoryMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null))
                    .Throws<InvalidOperationException>();
                await aidApplicationsService.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AidApplications_GET_ReturnsNullEntity_RepositoryException()
            {
                aidApplicationsRepositoryMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null))
                    .Throws<RepositoryException>();
                await aidApplicationsService.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AidApplications_GET_ReturnsNullEntity_Exception()
            {
                aidApplicationsRepositoryMock.Setup(x => x.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null))
                    .Throws<Exception>();
                await aidApplicationsService.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            public async Task AidApplications_GET_ById()
            {
                var id = "1";
                var expected = aidApplicationsEntities.ToList()[0];
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsByIdAsync(id)).ReturnsAsync(expected);
                var actual = await aidApplicationsService.GetAidApplicationsByIdAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AidApplications_GET_ById_NullId_ArgumentNullException()
            {
                var actual = await aidApplicationsService.GetAidApplicationsByIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AidApplications_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "1";
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsByIdAsync(id)).Throws<KeyNotFoundException>();
                var actual = await aidApplicationsService.GetAidApplicationsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AidApplications_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "1";
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsByIdAsync(id)).Throws<InvalidOperationException>();
                var actual = await aidApplicationsService.GetAidApplicationsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AidApplications_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "1";
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsByIdAsync(id)).Throws<RepositoryException>();
                var actual = await aidApplicationsService.GetAidApplicationsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AidApplications_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "1";
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsByIdAsync(id)).Throws<Exception>();
                var actual = await aidApplicationsService.GetAidApplicationsByIdAsync(id);
            }

            private void BuildData()
            {

                Domain.FinancialAid.Entities.AidApplications aidApplicationsEntity = new Domain.FinancialAid.Entities.AidApplications("1", "1");

                aidApplicationsEntity.PersonId = "0000100";
                aidApplicationsEntity.AidApplicationType = "CALISIR";
                aidApplicationsEntity.AidYear = "2023";
                aidApplicationsEntity.AssignedID = "987654321";

                aidApplicationsEntity.StudentMaritalDate = "202210";
                aidApplicationsEntity.StudentMaritalStatus = "2";

                aidApplicationsEntity.StudentLegalResDate = "202010";
                aidApplicationsEntity.StudentLegalResSt = "PA";
                aidApplicationsEntity.StudentLegalResB4 = true;

                var firstParent = new ParentDetails();
                aidApplicationsEntity.P1GradeLvl = "3";
                aidApplicationsEntity.P1Ssn = 987654123;
                aidApplicationsEntity.P1LastName = "TestParentLN";
                aidApplicationsEntity.P1FirstInit = "IN";
                aidApplicationsEntity.P1Dob = new DateTime(1975, 6, 30);

                aidApplicationsEntity.P2GradeLvl = "3";
                aidApplicationsEntity.P2Ssn = 987654321;
                aidApplicationsEntity.P2LastName = "SecondParentLN";
                aidApplicationsEntity.P2FirstInit = "SN";
                aidApplicationsEntity.P2Dob = new DateTime(1970, 1, 30);

                aidApplicationsEntity.PMaritalDate = "199510";
                aidApplicationsEntity.PMaritalStatus = "1";

                aidApplicationsEntity.ParentEmail = "parent.test@ellucian.com";

                aidApplicationsEntity.PLegalResDate = "198010";
                aidApplicationsEntity.PLegalResSt = "PA";
                aidApplicationsEntity.PLegalResB4 = true;

                aidApplicationsEntity.PNbrFamily = 12;
                aidApplicationsEntity.PNbrCollege = 1;


                aidApplicationsEntity.PSsiBen = true;
                aidApplicationsEntity.PFoodStamps = true;
                aidApplicationsEntity.PLunchBen = true;
                aidApplicationsEntity.PTanf = true;
                aidApplicationsEntity.PWic = true;
                aidApplicationsEntity.PTaxReturnFiled = "2";
                aidApplicationsEntity.PTaxFormType = "1";
                aidApplicationsEntity.PTaxFilingStatus = "3";
                aidApplicationsEntity.PSched1 = "1";
                aidApplicationsEntity.PDisWorker = "1";
                aidApplicationsEntity.PAgi = -100;
                aidApplicationsEntity.PUsTaxPaid = 500;
                aidApplicationsEntity.P1Income = 1000;
                aidApplicationsEntity.P2Income = 2000;
                aidApplicationsEntity.PCash = 400;
                aidApplicationsEntity.PInvNetWorth = 300;
                aidApplicationsEntity.PBusNetWorth = 400;
                aidApplicationsEntity.PEduCredit = 100;
                aidApplicationsEntity.PChildSupportPd = 100;
                aidApplicationsEntity.PNeedBasedEmp = 100;
                aidApplicationsEntity.PGrantScholAid = 100;
                aidApplicationsEntity.PCombatPay = 100;
                aidApplicationsEntity.PCoOpEarnings = 150;
                aidApplicationsEntity.PPensionPymts = 200;
                aidApplicationsEntity.PIraPymts = 300;
                aidApplicationsEntity.PChildSupRcvd = 100;
                aidApplicationsEntity.PUntxIntInc = 100;
                aidApplicationsEntity.PUntxIraPen = 300;
                aidApplicationsEntity.PMilClerAllow = 500;
                aidApplicationsEntity.PVetNonEdBen = 100;
                aidApplicationsEntity.POtherUntxInc = 50;

                aidApplicationsEntity.HsGradType = "1";
                aidApplicationsEntity.HsName = "El Paso Elementary School";
                aidApplicationsEntity.HsCity = "El Paso";
                aidApplicationsEntity.HsState = "TX";
                aidApplicationsEntity.HsCode = "123";

                aidApplicationsEntity.DegreeBy = true;
                aidApplicationsEntity.GradeLevelInCollege = "1";
                aidApplicationsEntity.DegreeOrCertificate = "2";
                aidApplicationsEntity.BornBefore = true;
                aidApplicationsEntity.Married = true;
                aidApplicationsEntity.GradOrProfProgram = false;
                aidApplicationsEntity.ActiveDuty = true;
                aidApplicationsEntity.UsVeteran = true;
                aidApplicationsEntity.DependentChildren = false;
                aidApplicationsEntity.OtherDependents = true;
                aidApplicationsEntity.OrphanWardFoster = true;
                aidApplicationsEntity.EmancipatedMinor = true;
                aidApplicationsEntity.LegalGuardianship = true;
                aidApplicationsEntity.HomelessAtRisk = true;
                aidApplicationsEntity.HomelessByHud = true;
                aidApplicationsEntity.HomelessBySchool = false;
                aidApplicationsEntity.StudentNumberInCollege = 2;
                aidApplicationsEntity.StudentNumberInFamily = 5;


                aidApplicationsEntity.SSsiBen = true;
                aidApplicationsEntity.SFoodStamps = true;
                aidApplicationsEntity.SLunchBen = true;
                aidApplicationsEntity.STanf = true;
                aidApplicationsEntity.SWic = true;
                aidApplicationsEntity.StudentTaxReturnFiled = "1";
                aidApplicationsEntity.StudentTaxFilingStatus = "1";
                aidApplicationsEntity.StudentTaxFormType = "1";
                aidApplicationsEntity.StudentSched1 = "1";
                aidApplicationsEntity.SDislWorker = "1";
                aidApplicationsEntity.StudentAgi = 6000;
                aidApplicationsEntity.StudentUsTaxPd = 500;
                aidApplicationsEntity.SStudentInc = 7890;
                aidApplicationsEntity.SpouseInc = 4000;
                aidApplicationsEntity.StudentCash = 400;
                aidApplicationsEntity.StudentInvNetWorth = 400;
                aidApplicationsEntity.StudentBusNetWorth = 800;
                aidApplicationsEntity.StudentEduCredit = 765;
                aidApplicationsEntity.StudentChildSupPaid = 400;
                aidApplicationsEntity.StudentNeedBasedEmp = 650;
                aidApplicationsEntity.StudentGrantScholAid = 450;
                aidApplicationsEntity.StudentCombatPay = 50;
                aidApplicationsEntity.StudentCoOpEarnings = 10;
                aidApplicationsEntity.StudentPensionPayments = 500;
                aidApplicationsEntity.StudentIraPayments = 600;
                aidApplicationsEntity.StudentChildSupRecv = 700;
                aidApplicationsEntity.StudentInterestIncome = 500;
                aidApplicationsEntity.StudentUntxIraPen = 600;
                aidApplicationsEntity.StudentMilitaryClergyAllow = 500;
                aidApplicationsEntity.StudentVetNonEdBen = 600;
                aidApplicationsEntity.StudentOtherUntaxedInc = 450;
                aidApplicationsEntity.StudentOtherNonRepMoney = 700;


                aidApplicationsEntity.SchoolCode1 = "B00001";
                aidApplicationsEntity.HousingPlan1 = "1";

                aidApplicationsEntity.SchoolCode2 = "B00002";
                aidApplicationsEntity.HousingPlan2 = "1";

                aidApplicationsEntity.SchoolCode3 = "B00003";
                aidApplicationsEntity.HousingPlan3 = "2";

                aidApplicationsEntity.SchoolCode4 = "B00004";
                aidApplicationsEntity.HousingPlan4 = "1";

                aidApplicationsEntity.SchoolCode5 = "B00005";
                aidApplicationsEntity.HousingPlan5 = "1";

                aidApplicationsEntity.SchoolCode6 = "B00006";
                aidApplicationsEntity.HousingPlan6 = "1";

                aidApplicationsEntity.SchoolCode7 = "B00007";
                aidApplicationsEntity.HousingPlan7 = "1";

                aidApplicationsEntity.SchoolCode8 = "B00008";
                aidApplicationsEntity.HousingPlan8 = "1";

                aidApplicationsEntity.SchoolCode9 = "B00009";
                aidApplicationsEntity.HousingPlan9 = "1";

                aidApplicationsEntity.SchoolCode10 = "B000010";
                aidApplicationsEntity.HousingPlan10 = "1";

                aidApplicationsEntity.ApplicationCompleteDate = new DateTime(2023, 01, 01);
                aidApplicationsEntity.SignedFlag = "P";
                aidApplicationsEntity.PreparerEin = 987678912;
                aidApplicationsEntity.PreparerSigned = "1";
                aidApplicationsEntity.PreparerSsn = 456670987;

                aidApplicationsEntities = new List<Domain.FinancialAid.Entities.AidApplications>()
                {
                    aidApplicationsEntity,
                    new Domain.FinancialAid.Entities.AidApplications("2", "2"),
                    new Domain.FinancialAid.Entities.AidApplications("3", "3"),
                    new Domain.FinancialAid.Entities.AidApplications("4", "4")
                };

                Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographicsEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics("1", "0000100", "2023", "CALISIR");

                aidApplicationsEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplications>, int>(aidApplicationsEntities, aidApplicationsEntities.Count());
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null)).ReturnsAsync(aidApplicationsEntityTuple);
                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationsEntities.ToList()[0]);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationDemographicsEntity);

            }
        }

        [TestClass]
        public class AidApplicationsServiceTests_PUT_POST : CurrentUserSetup
        {
            Mock<IAidApplicationsRepository> aidApplicationsRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IAidApplicationDemographicsRepository> aidApplicationDemographicsRepositoryMock;
            Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            AidApplicationsService aidApplicationsService;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private AidApplications aidApplicationsDto;
            private Domain.Entities.Permission permissionViewAidApplications;



            private const string aidAppDemoId = "1";
            private const string personId = "0000100";
            private const string aidYear = "2023";
            private const string aidApplicationType = "CALISIR";
            private const string applicantAssignedId = "987654321";

            [TestInitialize]
            public void Initialize()
            {
                aidApplicationsRepositoryMock = new Mock<IAidApplicationsRepository>();
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
                permissionViewAidApplications = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAidApplications);
                personRole.AddPermission(permissionViewAidApplications);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                aidApplicationsService = new AidApplicationsService(aidApplicationsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                                studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, baseConfigurationRepository, loggerMock.Object);
                var dtoToEntityAdapter = new AutoMapperAdapter<AidApplications, Domain.FinancialAid.Entities.AidApplications>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(x => x.GetAdapter<AidApplications, Domain.FinancialAid.Entities.AidApplications>()).Returns(dtoToEntityAdapter);

                var entityToDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.AidApplications, AidApplications>(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.FinancialAid.Entities.AidApplications, AidApplications>()).Returns(entityToDtoAdapter);

            }

            [TestCleanup]
            public void Cleanup()
            {
                aidApplicationsDto = null;
                aidApplicationsRepositoryMock = null;
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
            public async Task AidApplications_PUT()
            {
                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidAppDemoId, result.AppDemoID);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_DtoNull_Exception()
            {
                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_DtoAppDemoIdNull_Exception()
            {
                aidApplicationsDto.AppDemoID = "";
                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_FinAidYearEmptyList_Exception()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_FinAidYearNull_Exception()
            {
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_FinAidYearThrowsException()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ThrowsAsync(new Exception());

                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_AidApplicationTypesEmptyList_Exception()
            {
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationsDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_AidApplicationTypesNull_Exception()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_AidApplicationTypesThrowsException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_UpdateRepoThrowsRepositoryException()
            {
                aidApplicationsRepositoryMock.Setup(i => i.UpdateAidApplicationsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplications>())).ThrowsAsync(new RepositoryException());
                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_UpdateRepoThrowsException()
            {
                aidApplicationsRepositoryMock.Setup(i => i.UpdateAidApplicationsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplications>())).ThrowsAsync(new Exception());
                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_PUT_UpdateRepoReturnsNullException()
            {
                aidApplicationsRepositoryMock.Setup(i => i.UpdateAidApplicationsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplications>())).ReturnsAsync(() => null);
                var result = await aidApplicationsService.PutAidApplicationsAsync(aidAppDemoId, aidApplicationsDto);
            }

            #endregion

            #region POST
            [TestMethod]
            public async Task AidApplications_POST()
            {
                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidAppDemoId, result.AppDemoID);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_DtoNull_Exception()
            {
                var result = await aidApplicationsService.PostAidApplicationsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_AppDemoIdNull_Exception()
            {
                aidApplicationsDto.AppDemoID = "";
                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_FinAidYearEmptyList_Exception()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_FinAidYearNull_Exception()
            {
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_FinAidYearThrowsException()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ThrowsAsync(new Exception());

                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_AidApplicationTypesEmptyList_Exception()
            {
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationsDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_AidApplicationTypesNull_Exception()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_AidApplicationTypesThrowsException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_CreateRepoThrowsRepositoryException()
            {
                aidApplicationsRepositoryMock.Setup(i => i.CreateAidApplicationsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplications>())).ThrowsAsync(new RepositoryException());
                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_CreateRepoThrowsException()
            {
                aidApplicationsRepositoryMock.Setup(i => i.CreateAidApplicationsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplications>())).ThrowsAsync(new Exception());
                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplications_POST_CreateRepoReturnsNullException()
            {
                aidApplicationsRepositoryMock.Setup(i => i.CreateAidApplicationsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplications>())).ReturnsAsync(() => null);
                var result = await aidApplicationsService.PostAidApplicationsAsync(aidApplicationsDto);
            }

            #endregion

            private void BuildData()
            {
                aidApplicationsDto = new AidApplications();

                aidApplicationsDto.Id = aidAppDemoId;
                aidApplicationsDto.AppDemoID = aidAppDemoId;
                aidApplicationsDto.PersonId = personId;
                aidApplicationsDto.ApplicationType = aidApplicationType;
                aidApplicationsDto.AidYear = aidYear;
                aidApplicationsDto.ApplicantAssignedId = applicantAssignedId;

                aidApplicationsDto.StudentMarital = new StudentMaritalInfo();
                aidApplicationsDto.StudentMarital.Date = "202210";
                aidApplicationsDto.StudentMarital.Status = AidApplicationsStudentMarital.MarriedOrRemarried;

                aidApplicationsDto.StudentLegalResidence = new LegalResidence();
                aidApplicationsDto.StudentLegalResidence.Date = "202010";
                aidApplicationsDto.StudentLegalResidence.State = "PA";
                aidApplicationsDto.StudentLegalResidence.ResidentBefore = true;

                var parents = new ParentsInfo();
                var firstParent = new ParentDetails();
                firstParent.EducationalLevel = AidApplicationsParentEdLevel.CollegeOrBeyond;
                firstParent.SsnOrItin = 987654123;
                firstParent.LastName = "TestParentLN";
                firstParent.FirstInitial = "IN";
                firstParent.BirthDate = new DateTime(1975, 6, 30);
                parents.FirstParent = firstParent;

                var secondParent = new ParentDetails();
                firstParent.EducationalLevel = AidApplicationsParentEdLevel.CollegeOrBeyond;
                firstParent.SsnOrItin = 987654321;
                firstParent.LastName = "SecondParentLN";
                firstParent.FirstInitial = "SN";
                firstParent.BirthDate = new DateTime(1970, 1, 30);
                parents.SecondParent = firstParent;

                parents.ParentMarital = new ParentMaritalInfo();
                parents.ParentMarital.Date = "199510";
                parents.ParentMarital.Status = AidApplicationsParentMarital.MarriedOrRemarried;

                parents.EmailAddress = "parent.test@ellucian.com";

                parents.ParentLegalResidence = new LegalResidence();
                parents.ParentLegalResidence.Date = "198010";
                parents.ParentLegalResidence.State = "PA";
                parents.ParentLegalResidence.ResidentBefore = true;

                parents.NumberInFamily = 12;
                parents.NumberInCollege = 1;

                parents.Income = new ParentsIncome()
                {
                    SsiBenefits = true,
                    FoodStamps = true,
                    LunchBenefits = true,
                    TanfBenefits = true,
                    WicBenefits = true,
                    TaxReturnFiled = AidApplicationsTaxReturnFiledDto.WillFile,
                    TaxFormType = AidApplicationsTaxFormTypeDto.IRS1040,
                    TaxFilingStatus = AidApplicationsTaxFilingStatusDto.MarriedFiledSeparateReturn,
                    Schedule1Filed = AidApplicationsYesOrNoDto.Yes,
                    DislocatedWorker = AidApplicationsYesOrNoDto.Yes,
                    AdjustedGrossIncome = -100,
                    UsTaxPaid = 500,
                    FirstParentWorkEarnings = 1000,
                    SecondParentworkEarnings = 2000,
                    CashSavingsChecking = 400,
                    InvestmentNetWorth = 300,
                    BusinessOrFarmNetWorth = 400,
                    EducationalCredits = 100,
                    ChildSupportPaid = 100,
                    NeedBasedEmployment = 100,
                    GrantOrScholarshipAid = 100,
                    CombatPay = 100,
                    CoopEarnings = 150,
                    PensionPayments = 200,
                    IraPayments = 300,
                    ChildSupportReceived = 100,
                    TaxExemptInterstIncome = 100,
                    UntaxedIraAndPensions = 300,
                    MilitaryOrClergyAllowances = 500,
                    VeteranNonEdBenefits = 100,
                    OtherUntaxedIncome = 50
                };
                aidApplicationsDto.Parents = parents;

                aidApplicationsDto.HighSchool = new HighSchoolDetails()
                {
                    GradType = AidApplicationsHSGradtype.GEDOrStateEquivalentTest,
                    Name = "El Paso Elementary School",
                    City = "El Paso",
                    State = "TX",
                    Code = "123"
                };

                aidApplicationsDto.DegreeBy = true;
                aidApplicationsDto.GradeLevelInCollege = AidApplicationsGradLvlInCollege.FirstYearAttendedCollegeBefore;
                aidApplicationsDto.DegreeOrCertificate = AidApplicationsDegreeOrCert.SecondBachelorsDegree;
                aidApplicationsDto.BornBefore = true;
                aidApplicationsDto.Married = true;
                aidApplicationsDto.GradOrProfProgram = false;
                aidApplicationsDto.ActiveDuty = true;
                aidApplicationsDto.USVeteran = true;
                aidApplicationsDto.DependentChildren = false;
                aidApplicationsDto.OtherDependents = true;
                aidApplicationsDto.OrphanWardFoster = true;
                aidApplicationsDto.EmancipatedMinor = true;
                aidApplicationsDto.LegalGuardianship = true;
                aidApplicationsDto.HomelessAtRisk = true;
                aidApplicationsDto.HomelessByHud = true;
                aidApplicationsDto.HomelessBySchool = false;
                aidApplicationsDto.StudentNumberInCollege = 2;
                aidApplicationsDto.StudentNumberInFamily = 5;

                aidApplicationsDto.StudentsIncome = new StudentIncome()
                {
                    MedicaidOrSSIBenefits = true,
                    FoodStamps = true,
                    LunchBenefits = true,
                    TanfBenefits = true,
                    WicBenefits = true,
                    TaxReturnFiled = AidApplicationsTaxReturnFiledDto.WillFile,
                    TaxFilingStatus = AidApplicationsTaxFilingStatusDto.MarriedFiledJointReturn,
                    TaxFormType = AidApplicationsTaxFormTypeDto.IRS1040,
                    Schedule1Filed = AidApplicationsYesOrNoDto.Yes,
                    DislocatedWorker = AidApplicationsYesOrNoDto.Yes,
                    AdjustedGrossIncome = 6000,
                    UsTaxPaid = 500,
                    WorkEarnings = 7890,
                    SpouseWorkEarnings = 4000,
                    CashSavingsChecking = 400,
                    InvestmentNetWorth = 400,
                    BusinessNetWorth = 800,
                    EducationalCredit = 765,
                    ChildSupportPaid = 400,
                    NeedBasedEmployment = 650,
                    GrantAndScholarshipAid = 450,
                    CombatPay = 50,
                    CoopEarnings = 10,
                    PensionPayments = 500,
                    IraPayments = 600,
                    ChildSupportReceived = 700,
                    InterestIncome = 500,
                    UntaxedIraPension = 600,
                    MilitaryClergyAllowance = 500,
                    VeteranNonEdBenefits = 600,
                    OtherNonReportedMoney = 450,
                    OtherUntaxedIncome = 700
                };

                aidApplicationsDto.SchoolCode1 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00001",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                };
                aidApplicationsDto.SchoolCode2 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00002",
                    HousingPlan = AidApplicationHousingPlanDto.OffCampus
                };
                aidApplicationsDto.SchoolCode3 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00003",
                    HousingPlan = AidApplicationHousingPlanDto.WithParent
                };
                aidApplicationsDto.SchoolCode4 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00004",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                };
                aidApplicationsDto.SchoolCode5 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00005",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                };
                aidApplicationsDto.SchoolCode6 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00006",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                };
                aidApplicationsDto.SchoolCode7 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00007",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                };
                aidApplicationsDto.SchoolCode8 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00008",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                };
                aidApplicationsDto.SchoolCode9 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B00009",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                };
                aidApplicationsDto.SchoolCode10 = new Dtos.FinancialAid.SchoolCode()
                {
                    Code = "B000010",
                    HousingPlan = AidApplicationHousingPlanDto.OnCampus
                };
                aidApplicationsDto.ApplicationCompleteDate = new DateTime(2023, 01, 01);
                aidApplicationsDto.SignedFlag = "P";
                aidApplicationsDto.PreparerEin = 987678912;
                aidApplicationsDto.PreparerSigned = "Yes";
                aidApplicationsDto.PreparerSsn = 456670987;

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

                Domain.FinancialAid.Entities.AidApplications aidApplicationsEntity = new Domain.FinancialAid.Entities.AidApplications(aidAppDemoId, aidAppDemoId);

                aidApplicationsEntity.PersonId = personId;
                aidApplicationsEntity.AidApplicationType = aidApplicationType;
                aidApplicationsEntity.AidYear = aidYear;
                aidApplicationsEntity.AssignedID = applicantAssignedId;

                Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographicsEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics(aidAppDemoId, personId, aidYear, aidApplicationType);

                aidApplicationsRepositoryMock.Setup(i => i.GetAidApplicationsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationsEntity);
                aidApplicationsRepositoryMock.Setup(i => i.CreateAidApplicationsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplications>())).ReturnsAsync(aidApplicationsEntity);
                aidApplicationsRepositoryMock.Setup(i => i.UpdateAidApplicationsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplications>())).ReturnsAsync(aidApplicationsEntity);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationDemographicsEntity);
            }
        }
    }
}

