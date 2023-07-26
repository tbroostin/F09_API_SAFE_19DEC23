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
    /// Summary description for AidApplicationResultsServiceTests
    /// </summary>
    [TestClass]
    public class AidApplicationResultsServiceTests
    {
        [TestClass]
        public class AidApplicationResultsServiceTests_GET : CurrentUserSetup
        {
            Mock<IAidApplicationResultsRepository> aidApplicationResultsRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IAidApplicationDemographicsRepository> aidApplicationDemographicsRepositoryMock;
            Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            AidApplicationResultsService aidApplicationResultsService;
            IEnumerable<Domain.FinancialAid.Entities.AidApplicationResults> aidApplicationResultsEntities;
            Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationResults>, int> aidApplicationResultsEntityTuple;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Domain.Entities.Permission permissionViewAidApplicationResults;

            int offset = 0;
            int limit = 4;

            [TestInitialize]
            public void Initialize()
            {
                aidApplicationResultsRepositoryMock = new Mock<IAidApplicationResultsRepository>();
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
                permissionViewAidApplicationResults = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAidApplicationResults);
                personRole.AddPermission(permissionViewAidApplicationResults);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                aidApplicationResultsService = new AidApplicationResultsService(aidApplicationResultsRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                aidApplicationResultsEntityTuple = null;
                aidApplicationResultsEntities = null;
                aidApplicationResultsRepositoryMock = null;
                aidApplicationDemographicsRepositoryMock = null;
                studentReferenceDataRepositoryMock = null;
                financialAidReferenceDataRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;
                roleRepositoryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task AidApplicationResults_GETAllAsync()
            {
                AidApplicationResults filter = new AidApplicationResults();
                var actualsTuple =
                    await
                        aidApplicationResultsService.GetAidApplicationResultsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationResultsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationResults_GETAllAsync_EmptyTuple()
            {
                AidApplicationResults filter = new AidApplicationResults();
                aidApplicationResultsEntities = new List<Domain.FinancialAid.Entities.AidApplicationResults>()
                {

                };
                aidApplicationResultsEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationResults>, int>(aidApplicationResultsEntities, 0);
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null,null)).ReturnsAsync(aidApplicationResultsEntityTuple);
                var actualsTuple = await aidApplicationResultsService.GetAidApplicationResultsAsync(offset, limit, filter);

                Assert.AreEqual(0, actualsTuple.Item1.Count());
            }

            [TestMethod]
            public async Task AidApplicationResults_GETAllAsync_AppDemoIdFilter()
            {
                string AppDemoId = "70";
                AidApplicationResults filter = new AidApplicationResults()
                {
                    AppDemoId = AppDemoId
                };
                aidApplicationResultsRepositoryMock = null;
                aidApplicationResultsRepositoryMock = new Mock<IAidApplicationResultsRepository>();
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, null, null, null,null)).ReturnsAsync(aidApplicationResultsEntityTuple);

                aidApplicationResultsService = null;
                aidApplicationResultsService = new AidApplicationResultsService(aidApplicationResultsRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationResultsService.GetAidApplicationResultsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationResultsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationResults_GETAllAsync_PersonIdFilter()
            {
                //For some reason need to reset repo's and service to truly run the tests
                string PersonId = "0000100";
                AidApplicationResults filter = new AidApplicationResults()
                {
                    PersonId = PersonId
                };
                aidApplicationResultsRepositoryMock = null;
                aidApplicationResultsRepositoryMock = new Mock<IAidApplicationResultsRepository>();
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, It.IsAny<string>(), null, null,null, null)).ReturnsAsync(aidApplicationResultsEntityTuple);

                aidApplicationResultsService = null;
                aidApplicationResultsService = new AidApplicationResultsService(aidApplicationResultsRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationResultsService.GetAidApplicationResultsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationResultsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationResults_GETAllAsync_AidApplicationType()
            {
                AidApplicationResults filter = new AidApplicationResults() { ApplicationType = "CALISIR" };

                aidApplicationResultsRepositoryMock = null;
                aidApplicationResultsRepositoryMock = new Mock<IAidApplicationResultsRepository>();
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, It.IsAny<string>(), null, null,null)).ReturnsAsync(aidApplicationResultsEntityTuple);

                aidApplicationResultsService = null;
                aidApplicationResultsService = new AidApplicationResultsService(aidApplicationResultsRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationResultsService.GetAidApplicationResultsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationResultsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationResults_GETAllAsync_AidYear()
            {
                AidApplicationResults filter = new AidApplicationResults() { AidYear = "2023" };

                aidApplicationResultsRepositoryMock = null;
                aidApplicationResultsRepositoryMock = new Mock<IAidApplicationResultsRepository>();
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, It.IsAny<string>(), null,null)).ReturnsAsync(aidApplicationResultsEntityTuple);

                aidApplicationResultsService = null;
                aidApplicationResultsService = new AidApplicationResultsService(aidApplicationResultsRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationResultsService.GetAidApplicationResultsAsync(offset, limit, filter);

                //Assert.AreEqual(0, actualsTuple.Item1.Count());
                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationResultsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }
            [TestMethod]
            public async Task AidApplicationResults_GETAllAsync_TransactionNumber()
            {
                AidApplicationResults filter = new AidApplicationResults() { TransactionNumber = 61 };

                aidApplicationResultsRepositoryMock = null;
                aidApplicationResultsRepositoryMock = new Mock<IAidApplicationResultsRepository>();
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, It.IsAny<int?>(), null)).ReturnsAsync(aidApplicationResultsEntityTuple);

                aidApplicationResultsService = null;
                aidApplicationResultsService = new AidApplicationResultsService(aidApplicationResultsRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationResultsService.GetAidApplicationResultsAsync(offset, limit, filter);

                //Assert.AreEqual(0, actualsTuple.Item1.Count());
                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationResultsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            public async Task AidApplicationResults_GETAllAsync_ApplicantAssignedId()
            {
                AidApplicationResults filter = new AidApplicationResults() { ApplicantAssignedId = "987654321" };
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null,It.IsAny<string>())).ReturnsAsync(aidApplicationResultsEntityTuple);

                aidApplicationResultsService = null;
                aidApplicationResultsService = new AidApplicationResultsService(aidApplicationResultsRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);

                var actualsTuple =
                    await
                        aidApplicationResultsService.GetAidApplicationResultsAsync(offset, limit, filter);

                Assert.IsNotNull(actualsTuple);

                int count = actualsTuple.Item1.Count();

                for (int i = 0; i < count; i++)
                {
                    var expected = aidApplicationResultsEntities.ToList()[i];
                    var actual = actualsTuple.Item1.ToList()[i];

                    Assert.IsNotNull(actual);

                    Assert.AreEqual(expected.Id, actual.Id);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AidApplicationResults_GET_ReturnsNullEntity_KeyNotFoundException()
            {
                aidApplicationResultsRepositoryMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null,null))
                    .Throws<KeyNotFoundException>();
                await aidApplicationResultsService.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AidApplicationResults_GET_ReturnsNullEntity_InvalidOperationException()
            {
                aidApplicationResultsRepositoryMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null,null))
                    .Throws<InvalidOperationException>();
                await aidApplicationResultsService.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AidApplicationResults_GET_ReturnsNullEntity_RepositoryException()
            {
                aidApplicationResultsRepositoryMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null,null))
                    .Throws<RepositoryException>();
                await aidApplicationResultsService.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AidApplicationResults_GET_ReturnsNullEntity_Exception()
            {
                aidApplicationResultsRepositoryMock.Setup(x => x.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null,null))
                    .Throws<Exception>();
                await aidApplicationResultsService.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null);
            }

            [TestMethod]
            public async Task AidApplicationResults_GET_ById()
            {
                var id = "1";
                var expected = aidApplicationResultsEntities.ToList()[0];
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsByIdAsync(id)).ReturnsAsync(expected);
                var actual = await aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);

                Assert.IsNotNull(actual);

                Assert.AreEqual(expected.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AidApplicationResults_GET_ById_NullId_ArgumentNullException()
            {
                var actual = await aidApplicationResultsService.GetAidApplicationResultsByIdAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AidApplicationResults_GET_ById_ReturnsNullEntity_KeyNotFoundException()
            {
                var id = "1";
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsByIdAsync(id)).Throws<KeyNotFoundException>();
                var actual = await aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task AidApplicationResults_GET_ById_ReturnsNullEntity_InvalidOperationException()
            {
                var id = "1";
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsByIdAsync(id)).Throws<InvalidOperationException>();
                var actual = await aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task AidApplicationResults_GET_ById_ReturnsNullEntity_RepositoryException()
            {
                var id = "1";
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsByIdAsync(id)).Throws<RepositoryException>();
                var actual = await aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AidApplicationResults_GET_ById_ReturnsNullEntity_Exception()
            {
                var id = "1";
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsByIdAsync(id)).Throws<Exception>();
                var actual = await aidApplicationResultsService.GetAidApplicationResultsByIdAsync(id);
            }

            private void BuildData()
            {

                Domain.FinancialAid.Entities.AidApplicationResults aidApplicationResultsEntity = new Domain.FinancialAid.Entities.AidApplicationResults("1", "1");

                aidApplicationResultsEntity.PersonId = "0000100";
                aidApplicationResultsEntity.AidYear = "2023";
                aidApplicationResultsEntity.AidApplicationType = "CALISIR";
                aidApplicationResultsEntity.ApplicantAssignedId = "987654321";
                aidApplicationResultsEntity.TransactionNumber = 1;
                aidApplicationResultsEntity.DependencyOverride = "1";
                aidApplicationResultsEntity.DependencyOverSchoolCode = "E20234";
                aidApplicationResultsEntity.DependencyStatus = "I";
                aidApplicationResultsEntity.TransactionSource = "4B";
                aidApplicationResultsEntity.TransactionReceiptDate = new DateTime(2001, 1, 20);
                aidApplicationResultsEntity.SpecialCircumstances = "2";
                aidApplicationResultsEntity.ParentAssetExceeded = true;
                aidApplicationResultsEntity.StudentAssetExceeded = true;
                aidApplicationResultsEntity.DestinationNumber = "TG87902";
                aidApplicationResultsEntity.StudentCurrentPseudoId = "675023526";
                aidApplicationResultsEntity.CorrectionAppliedAgainst = "15";
                aidApplicationResultsEntity.ProfJudgementIndicator = "1";
                aidApplicationResultsEntity.ApplicationDataSource = "2B";
                aidApplicationResultsEntity.ApplicationReceiptDate = new DateTime(2001, 1, 20);
                aidApplicationResultsEntity.AddressOnlyChangeFlag = "4";
                aidApplicationResultsEntity.PushedApplicationFlag = true;
                aidApplicationResultsEntity.EfcChangeFlag = "1";
                aidApplicationResultsEntity.LastNameChange = "N";
                aidApplicationResultsEntity.RejectStatusChange = false;
                aidApplicationResultsEntity.SarcChange = false;
                aidApplicationResultsEntity.ComputeNumber = "325";
                aidApplicationResultsEntity.CorrectionSource = "S";
                aidApplicationResultsEntity.DuplicateIdIndicator = true;
                aidApplicationResultsEntity.GraduateFlag = false;
                aidApplicationResultsEntity.TransactionProcessedDate = new DateTime(2003, 11, 22);
                aidApplicationResultsEntity.ProcessedRecordType = "H";
                aidApplicationResultsEntity.RejectReasonCodes = new List<string>(){ "r1" };
                aidApplicationResultsEntity.AutomaticZeroIndicator = false;
                aidApplicationResultsEntity.SimplifiedNeedsTest = "N";
                aidApplicationResultsEntity.ParentCalculatedTaxStatus = "3";
                aidApplicationResultsEntity.StudentCalculatedTaxStatus = "3";
                aidApplicationResultsEntity.StudentAddlFinCalcTotal = 20602400;
                aidApplicationResultsEntity.studentOthUntaxIncomeCalcTotal = 20802300;
                aidApplicationResultsEntity.ParentAddlFinCalcTotal = 13000208;
                aidApplicationResultsEntity.ParentOtherUntaxIncomeCalcTotal = 40802400;
                aidApplicationResultsEntity.InvalidHighSchool = false;
                aidApplicationResultsEntity.AssumCitizenship = "1";
                aidApplicationResultsEntity.AssumSMarStat = "1";
                aidApplicationResultsEntity.AssumSAgi = 4182240;
                aidApplicationResultsEntity.AssumSTaxPd = 5130135;
                aidApplicationResultsEntity.AssumSIncWork = 1291250;
                aidApplicationResultsEntity.AssumSpIncWork = 7540278;
                aidApplicationResultsEntity.AssumSAddlFinAmt = 22703403;
                aidApplicationResultsEntity.AssumBirthDatePrior = "2";
                aidApplicationResultsEntity.AssumSMarried = "2";
                aidApplicationResultsEntity.AssumChildren = "2";
                aidApplicationResultsEntity.AssumLegalDep = "2";
                aidApplicationResultsEntity.AssumSNbrFamily = 99;
                aidApplicationResultsEntity.AssumSNbrCollege = 1;
                aidApplicationResultsEntity.AssumSAssetTholdExc = false;
                aidApplicationResultsEntity.AssumPMarStat = "1";
                aidApplicationResultsEntity.AssumPar1Ssn = true;
                aidApplicationResultsEntity.AssumPar2Ssn = false;
                aidApplicationResultsEntity.AssumPNbrFamily = 82;
                aidApplicationResultsEntity.AssumPNbrCollege = 7;
                aidApplicationResultsEntity.AssumPAgi = 3322502;
                aidApplicationResultsEntity.AssumPTaxPd = 4530234;
                aidApplicationResultsEntity.AssumPar1Income = 4260275;
                aidApplicationResultsEntity.AssumPar2Income = 2350357;
                aidApplicationResultsEntity.AssumPAddlFinAmt = 15213506;
                aidApplicationResultsEntity.AssumPAssetTholdExc = true;
                aidApplicationResultsEntity.PrimaryEfc = 351250;
                aidApplicationResultsEntity.SecondaryEfc = 572026;
                aidApplicationResultsEntity.SignatureRejectEfc = 662032;
                aidApplicationResultsEntity.PrimaryEfcType = "6";
                aidApplicationResultsEntity.PriAlt1mnthEfc = 345671;
                aidApplicationResultsEntity.PriAlt2mnthEfc = 345672;
                aidApplicationResultsEntity.PriAlt3mnthEfc = 345673;
                aidApplicationResultsEntity.PriAlt4mnthEfc = 345674;
                aidApplicationResultsEntity.PriAlt5mnthEfc = 345675;
                aidApplicationResultsEntity.PriAlt6mnthEfc = 345676;
                aidApplicationResultsEntity.PriAlt7mnthEfc = 345677;
                aidApplicationResultsEntity.PriAlt8mnthEfc = 345678;
                aidApplicationResultsEntity.PriAlt10mnthEfc = 345680;
                aidApplicationResultsEntity.PriAlt11mnthEfc = 345681;
                aidApplicationResultsEntity.PriAlt12mnthEfc = 345682;
                aidApplicationResultsEntity.TotalIncome = 72370346;
                aidApplicationResultsEntity.AllowancesAgainstTotalIncome = 3234512;
                aidApplicationResultsEntity.TaxAllowance = 8314355;
                aidApplicationResultsEntity.EmploymentAllowance = 2685413;
                aidApplicationResultsEntity.IncomeProtectionAllowance = 3459835;
                aidApplicationResultsEntity.AvailableIncome = 83383552;
                aidApplicationResultsEntity.AvailableIncomeContribution = 3349835;
                aidApplicationResultsEntity.DiscretionaryNetWorth = 432498350;
                aidApplicationResultsEntity.NetWorth = 598512160;
                aidApplicationResultsEntity.AssetProtectionAllowance = 426286233;
                aidApplicationResultsEntity.ParentContributionAssets = 962861;
                aidApplicationResultsEntity.AdjustedAvailableIncome = 49628617;
                aidApplicationResultsEntity.TotalPrimaryStudentContribution = 1985426;
                aidApplicationResultsEntity.TotalPrimaryParentContribution = 5459833;
                aidApplicationResultsEntity.ParentContribution = 3285134;
                aidApplicationResultsEntity.StudentTotalIncome = 79628618;
                aidApplicationResultsEntity.StudentAllowanceAgainstIncome = 5962823;
                aidApplicationResultsEntity.DependentStudentIncContrib = 4328261;
                aidApplicationResultsEntity.StudentDiscretionaryNetWorth = 373598352;
                aidApplicationResultsEntity.StudentAssetContribution = 6459839;
                aidApplicationResultsEntity.FisapTotalIncome = 49853222;
                aidApplicationResultsEntity.CorrectionFlags = "Test10";
                aidApplicationResultsEntity.HighlightFlags = "Test11";
                aidApplicationResultsEntity.CommentCodes = new List<string>(){"c1"};
                aidApplicationResultsEntity.ElectronicFedSchoolCodeInd = "1";
                aidApplicationResultsEntity.ElectronicTransactionIndicator = "Y";
                aidApplicationResultsEntity.VerificationSelected = "*";


                aidApplicationResultsEntities = new List<Domain.FinancialAid.Entities.AidApplicationResults>()
                {
                    aidApplicationResultsEntity,
                    new Domain.FinancialAid.Entities.AidApplicationResults("2", "2"),
                    new Domain.FinancialAid.Entities.AidApplicationResults("3", "3"),
                    new Domain.FinancialAid.Entities.AidApplicationResults("4", "4")
                };

                aidApplicationResultsEntityTuple = new Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationResults>, int>(aidApplicationResultsEntities, aidApplicationResultsEntities.Count());
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsAsync(It.IsAny<int>(), It.IsAny<int>(), null, null, null, null, null,null)).ReturnsAsync(aidApplicationResultsEntityTuple);
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationResultsEntities.ToList()[0]);

            }
        }

        [TestClass]
        public class AidApplicationResultsServiceTests_PUT_POST : CurrentUserSetup
        {
            Mock<IAidApplicationResultsRepository> aidApplicationResultsRepositoryMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            Mock<IAidApplicationDemographicsRepository> aidApplicationDemographicsRepositoryMock;
            Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            ICurrentUserFactory currentUserFactory;
            Mock<IRoleRepository> roleRepositoryMock;
            Mock<ILogger> loggerMock;

            AidApplicationResultsService aidApplicationResultsService;
            IEnumerable<Domain.FinancialAid.Entities.AidApplicationResults> aidApplicationResultsEntities;
            Tuple<IEnumerable<Domain.FinancialAid.Entities.AidApplicationResults>, int> aidApplicationResultsEntityTuple;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private AidApplicationResults aidApplicationResultsDto;
            private Domain.Entities.Permission permissionViewAidApplicationResults;



            private const string aidAppDemoId = "1";
            private const string personId = "0000100";
            private const string aidYear = "2023";
            private const string aidApplicationType = "CALISIR";
            private const string applicantAssignedId = "987654321";

            [TestInitialize]
            public void Initialize()
            {
                aidApplicationResultsRepositoryMock = new Mock<IAidApplicationResultsRepository>();
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
                permissionViewAidApplicationResults = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewAidApplicationResults);
                personRole.AddPermission(permissionViewAidApplicationResults);
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                aidApplicationResultsService = new AidApplicationResultsService(aidApplicationResultsRepositoryMock.Object, studentReferenceDataRepositoryMock.Object,
                                               aidApplicationDemographicsRepositoryMock.Object, financialAidReferenceDataRepositoryMock.Object,
                                               adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                aidApplicationResultsDto = null;
                aidApplicationResultsRepositoryMock = null;
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
            public async Task AidApplicationResults_PUT()
            {
                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidAppDemoId, result.AppDemoId);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_DtoNull_Exception()
            {
                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_DtoAppDemoIdNull_Exception()
            {
                aidApplicationResultsDto.AppDemoId = "";
                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_FinAidYearEmptyList_Exception()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_FinAidYearNull_Exception()
            {
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_FinAidYearThrowsException()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ThrowsAsync(new Exception());

                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_AidApplicationTypesEmptyList_Exception()
            {
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationResultsDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_AidApplicationTypesNull_Exception()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_AidApplicationTypesThrowsException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_UpdateRepoThrowsRepositoryException()
            {
                aidApplicationResultsRepositoryMock.Setup(i => i.UpdateAidApplicationResultsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationResults>())).ThrowsAsync(new RepositoryException());
                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_UpdateRepoThrowsException()
            {
                aidApplicationResultsRepositoryMock.Setup(i => i.UpdateAidApplicationResultsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationResults>())).ThrowsAsync(new Exception());
                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_PUT_UpdateRepoReturnsNullException()
            {
                aidApplicationResultsRepositoryMock.Setup(i => i.UpdateAidApplicationResultsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationResults>())).ReturnsAsync(() => null);
                var result = await aidApplicationResultsService.PutAidApplicationResultsAsync(aidAppDemoId, aidApplicationResultsDto);
            }

            #endregion

            #region POST
            [TestMethod]
            public async Task AidApplicationResults_POST()
            {
                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidAppDemoId, result.AppDemoId);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_DtoNull_Exception()
            {
                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_AppDemoIdNull_Exception()
            {
                aidApplicationResultsDto.AppDemoId = "";
                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_FinAidYearEmptyList_Exception()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(finAidYears);

                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_FinAidYearNull_Exception()
            {
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ReturnsAsync(() => null);
                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_FinAidYearThrowsException()
            {
                var finAidYears = new List<Domain.FinancialAid.Entities.FinancialAidYear>();
                financialAidReferenceDataRepositoryMock.Setup(i => i.GetFinancialAidYearsAsync(false)).ThrowsAsync(new Exception());

                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_AidApplicationTypesEmptyList_Exception()
            {
                var aidApplicationTypes = new List<Domain.Student.Entities.AidApplicationType>();
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(aidApplicationTypes);

                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
                Assert.IsNotNull(result);
                Assert.AreEqual(aidApplicationResultsDto.Id, result.Id);
            }


            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_AidApplicationTypesNull_Exception()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ReturnsAsync(() => null);

                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_AidApplicationTypesThrowsException()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAidApplicationTypesAsync(false)).ThrowsAsync(new Exception());
                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_CreateRepoThrowsRepositoryException()
            {
                aidApplicationResultsRepositoryMock.Setup(i => i.CreateAidApplicationResultsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationResults>())).ThrowsAsync(new RepositoryException());
                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_CreateRepoThrowsException()
            {
                aidApplicationResultsRepositoryMock.Setup(i => i.CreateAidApplicationResultsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationResults>())).ThrowsAsync(new Exception());
                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task AidApplicationResults_POST_CreateRepoReturnsNullException()
            {
                aidApplicationResultsRepositoryMock.Setup(i => i.CreateAidApplicationResultsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationResults>())).ReturnsAsync(() => null);
                var result = await aidApplicationResultsService.PostAidApplicationResultsAsync(aidApplicationResultsDto);
            }

            #endregion

            private void BuildData()
            {
                aidApplicationResultsDto = new AidApplicationResults
                {
                    Id = aidAppDemoId,
                    AppDemoId = aidAppDemoId,
                    PersonId = "0000100",
                    AidYear = "2023",
                    ApplicationType = "CALISIR",
                    ApplicantAssignedId = "987654321",
                    TransactionNumber = 1,
                    DependencyOverride = "1",
                    DependencyOverSchoolCode = "E20234",
                    DependencyStatus = "I",
                    TransactionSource = "4B",
                    TransactionReceiptDate = new DateTime(2001, 1, 20),
                    SpecialCircumstances = "2",
                    ParentAssetExceeded = true,
                    StudentAssetExceeded = true,
                    DestinationNumber = "TG87902",
                    StudentCurrentPseudoId = "675023526",
                    CorrectionAppliedAgainst = "15",
                    ProfJudgementIndicator = JudgementIndicator.AdjustmentFailed,
                    ApplicationDataSource = "2B",
                    ApplicationReceiptDate = new DateTime(2001, 1, 20),
                    AddressOnlyChangeFlag = "4",
                    PushedApplicationFlag = true,
                    EfcChangeFlag = EfcChangeFlag.EfcDecrease,
                    LastNameChange = LastNameChange.N,
                    RejectStatusChange = false,
                    SarcChange = false,
                    ComputeNumber = "325",
                    CorrectionSource = "S",
                    DuplicateIdIndicator = true,
                    GraduateFlag = false,
                    TransactionProcessedDate = new DateTime(2003, 11, 22),
                    ProcessedRecordType = "H",
                    RejectReasonCodes = new List<string>(){ "r1" },
                    AutomaticZeroIndicator = false,
                    SimplifiedNeedsTest = SimplifiedNeedsTest.N,
                    ParentCalculatedTaxStatus = "3",
                    StudentCalculatedTaxStatus = "3",
                    StudentAddlFinCalcTotal = 20602400,
                    studentOthUntaxIncomeCalcTotal = 20802300,
                    ParentAddlFinCalcTotal = 13000208,
                    ParentOtherUntaxIncomeCalcTotal = 40802400,
                    InvalidHighSchool = false,
                    Assumed = new Dtos.FinancialAid.AssumedStudentDetails()
                    {
                        Citizenship = AssumedCitizenshipStatus.AssumedCitizen,
                        StudentMaritalStatus = AssumedStudentMaritalStatus.AssumedMarriedRemarried,
                        StudentAgi = 4182240,
                        StudentTaxPaid = 5130135,
                        StudentWorkIncome = 1291250,
                        SpouseWorkIncome = 7540278,
                        StudentAddlFinInfoTotal = 22703403,
                        BirthDatePrior = AssumedYesNo.AssumedNo,
                        StudentMarried = AssumedYesNo.AssumedNo,
                        DependentChildren = AssumedYesNo.AssumedNo,
                        OtherDependents = "2",
                        studentFamilySize = 99,
                        StudentNumberInCollege = 1,
                        StudentAssetThreshold = false,
                        ParentMaritalStatus = AssumedParentMaritalStatus.AssumedMarriedRemarried,
                        FirstParentSsn = true,
                        SecondParentSsn = false,
                        ParentFamilySize = 82,
                        ParentNumCollege = 7,
                        ParentAgi = 3322502,
                        ParentTaxPaid = 4530234,
                        FirstParentWorkIncome = 4260275,
                        SecondParentWorkIncome = 2350357,
                        ParentAddlFinancial = 15213506,
                        ParentAssetThreshold = true
                    },
                    PrimaryEfc = 351250,
                    SecondaryEfc = 572026,
                    SignatureRejectEfc = 662032,
                    PrimaryEfcType = "6",
                    AlternatePrimaryEfc = new Dtos.FinancialAid.AlternatePrimaryEfc()
                    {
                        OneMonth = 345671,
                        TwoMonths = 345672,
                        ThreeMonths = 345673,
                        FourMonths = 345674,
                        FiveMonths = 345675,
                        SixMonths = 345676,
                        SevenMonths = 345677,
                        EightMonths = 345678,
                        TenMonths = 345680,
                        ElevenMonths = 345681,
                        TwelveMonths = 345682
                    },
                    TotalIncome = 72370346,
                    AllowancesAgainstTotalIncome = 3234512,
                    TaxAllowance = 8314355,
                    EmploymentAllowance = 2685413,
                    IncomeProtectionAllowance = 3459835,
                    AvailableIncome = 83383552,
                    AvailableIncomeContribution = 3349835,
                    DiscretionaryNetWorth = 432498350,
                    NetWorth = 598512160,
                    AssetProtectionAllowance = 426286233,
                    ParentContributionAssets = 962861,
                    AdjustedAvailableIncome = 49628617,
                    TotalPrimaryStudentContribution = 1985426,
                    TotalPrimaryParentContribution = 5459833,
                    ParentContribution = 3285134,
                    StudentTotalIncome = 79628618,
                    StudentAllowanceAgainstIncome = 5962823,
                    DependentStudentIncContrib = 4328261,
                    StudentDiscretionaryNetWorth = 373598352,
                    StudentAssetContribution = 6459839,
                    FisapTotalIncome = 49853222,
                    CorrectionFlags = "Test10",
                    HighlightFlags = "Test11",
                    CommentCodes = new List<string>() { "c1" },
                    ElectronicFedSchoolCodeInd = "1",
                    ElectronicTransactionIndicator = "Y",
                    VerificationSelected = "*"
                };
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

                Domain.FinancialAid.Entities.AidApplicationResults aidApplicationResultsEntity = new Domain.FinancialAid.Entities.AidApplicationResults(aidAppDemoId, aidAppDemoId);

                aidApplicationResultsEntity.PersonId = "0000100";
                aidApplicationResultsEntity.AidYear = "2023";
                aidApplicationResultsEntity.AidApplicationType = "CALISIR";
                aidApplicationResultsEntity.ApplicantAssignedId = "987654321";
                aidApplicationResultsEntity.TransactionNumber = 1;
                aidApplicationResultsEntity.DependencyOverride = "1";
                aidApplicationResultsEntity.DependencyOverSchoolCode = "E20234";
                aidApplicationResultsEntity.DependencyStatus = "I";
                aidApplicationResultsEntity.TransactionSource = "4B";
                aidApplicationResultsEntity.TransactionReceiptDate = new DateTime(2001, 1, 20);
                aidApplicationResultsEntity.SpecialCircumstances = "2";
                aidApplicationResultsEntity.ParentAssetExceeded = true;
                aidApplicationResultsEntity.StudentAssetExceeded = true;
                aidApplicationResultsEntity.DestinationNumber = "TG87902";
                aidApplicationResultsEntity.StudentCurrentPseudoId = "675023526";
                aidApplicationResultsEntity.CorrectionAppliedAgainst = "15";
                aidApplicationResultsEntity.ProfJudgementIndicator = "1";
                aidApplicationResultsEntity.ApplicationDataSource = "2B";
                aidApplicationResultsEntity.ApplicationReceiptDate = new DateTime(2001, 1, 20);
                aidApplicationResultsEntity.AddressOnlyChangeFlag = "4";
                aidApplicationResultsEntity.PushedApplicationFlag = true;
                aidApplicationResultsEntity.EfcChangeFlag = "1";
                aidApplicationResultsEntity.LastNameChange = "N";
                aidApplicationResultsEntity.RejectStatusChange = false;
                aidApplicationResultsEntity.SarcChange = false;
                aidApplicationResultsEntity.ComputeNumber = "325";
                aidApplicationResultsEntity.CorrectionSource = "S";
                aidApplicationResultsEntity.DuplicateIdIndicator = true;
                aidApplicationResultsEntity.GraduateFlag = false;
                aidApplicationResultsEntity.TransactionProcessedDate = new DateTime(2003, 11, 22);
                aidApplicationResultsEntity.ProcessedRecordType = "H";
                aidApplicationResultsEntity.RejectReasonCodes = new List<string>(){ "r1" };
                aidApplicationResultsEntity.AutomaticZeroIndicator = false;
                aidApplicationResultsEntity.SimplifiedNeedsTest = "N";
                aidApplicationResultsEntity.ParentCalculatedTaxStatus = "3";
                aidApplicationResultsEntity.StudentCalculatedTaxStatus = "3";
                aidApplicationResultsEntity.StudentAddlFinCalcTotal = 20602400;
                aidApplicationResultsEntity.studentOthUntaxIncomeCalcTotal = 20802300;
                aidApplicationResultsEntity.ParentAddlFinCalcTotal = 13000208;
                aidApplicationResultsEntity.ParentOtherUntaxIncomeCalcTotal = 40802400;
                aidApplicationResultsEntity.InvalidHighSchool = false;
                aidApplicationResultsEntity.AssumCitizenship = "1";
                aidApplicationResultsEntity.AssumSMarStat = "1";
                aidApplicationResultsEntity.AssumSAgi = 4182240;
                aidApplicationResultsEntity.AssumSTaxPd = 5130135;
                aidApplicationResultsEntity.AssumSIncWork = 1291250;
                aidApplicationResultsEntity.AssumSpIncWork = 7540278;
                aidApplicationResultsEntity.AssumSAddlFinAmt = 22703403;
                aidApplicationResultsEntity.AssumBirthDatePrior = "2";
                aidApplicationResultsEntity.AssumSMarried = "2";
                aidApplicationResultsEntity.AssumChildren = "2";
                aidApplicationResultsEntity.AssumLegalDep = "2";
                aidApplicationResultsEntity.AssumSNbrFamily = 99;
                aidApplicationResultsEntity.AssumSNbrCollege = 1;
                aidApplicationResultsEntity.AssumSAssetTholdExc = false;
                aidApplicationResultsEntity.AssumPMarStat = "1";
                aidApplicationResultsEntity.AssumPar1Ssn = true;
                aidApplicationResultsEntity.AssumPar2Ssn = false;
                aidApplicationResultsEntity.AssumPNbrFamily = 82;
                aidApplicationResultsEntity.AssumPNbrCollege = 7;
                aidApplicationResultsEntity.AssumPAgi = 3322502;
                aidApplicationResultsEntity.AssumPTaxPd = 4530234;
                aidApplicationResultsEntity.AssumPar1Income = 4260275;
                aidApplicationResultsEntity.AssumPar2Income = 2350357;
                aidApplicationResultsEntity.AssumPAddlFinAmt = 15213506;
                aidApplicationResultsEntity.AssumPAssetTholdExc = true;
                aidApplicationResultsEntity.PrimaryEfc = 351250;
                aidApplicationResultsEntity.SecondaryEfc = 572026;
                aidApplicationResultsEntity.SignatureRejectEfc = 662032;
                aidApplicationResultsEntity.PrimaryEfcType = "6";
                aidApplicationResultsEntity.PriAlt1mnthEfc = 345671;
                aidApplicationResultsEntity.PriAlt2mnthEfc = 345672;
                aidApplicationResultsEntity.PriAlt3mnthEfc = 345673;
                aidApplicationResultsEntity.PriAlt4mnthEfc = 345674;
                aidApplicationResultsEntity.PriAlt5mnthEfc = 345675;
                aidApplicationResultsEntity.PriAlt6mnthEfc = 345676;
                aidApplicationResultsEntity.PriAlt7mnthEfc = 345677;
                aidApplicationResultsEntity.PriAlt8mnthEfc = 345678;
                aidApplicationResultsEntity.PriAlt10mnthEfc = 345680;
                aidApplicationResultsEntity.PriAlt11mnthEfc = 345681;
                aidApplicationResultsEntity.PriAlt12mnthEfc = 345682;
                aidApplicationResultsEntity.TotalIncome = 72370346;
                aidApplicationResultsEntity.AllowancesAgainstTotalIncome = 3234512;
                aidApplicationResultsEntity.TaxAllowance = 8314355;
                aidApplicationResultsEntity.EmploymentAllowance = 2685413;
                aidApplicationResultsEntity.IncomeProtectionAllowance = 3459835;
                aidApplicationResultsEntity.AvailableIncome = 83383552;
                aidApplicationResultsEntity.AvailableIncomeContribution = 3349835;
                aidApplicationResultsEntity.DiscretionaryNetWorth = 432498350;
                aidApplicationResultsEntity.NetWorth = 598512160;
                aidApplicationResultsEntity.AssetProtectionAllowance = 426286233;
                aidApplicationResultsEntity.ParentContributionAssets = 962861;
                aidApplicationResultsEntity.AdjustedAvailableIncome = 49628617;
                aidApplicationResultsEntity.TotalPrimaryStudentContribution = 1985426;
                aidApplicationResultsEntity.TotalPrimaryParentContribution = 5459833;
                aidApplicationResultsEntity.ParentContribution = 3285134;
                aidApplicationResultsEntity.StudentTotalIncome = 79628618;
                aidApplicationResultsEntity.StudentAllowanceAgainstIncome = 5962823;
                aidApplicationResultsEntity.DependentStudentIncContrib = 4328261;
                aidApplicationResultsEntity.StudentDiscretionaryNetWorth = 373598352;
                aidApplicationResultsEntity.StudentAssetContribution = 6459839;
                aidApplicationResultsEntity.FisapTotalIncome = 49853222;
                aidApplicationResultsEntity.CorrectionFlags = "Test10";
                aidApplicationResultsEntity.HighlightFlags = "Test11";
                aidApplicationResultsEntity.CommentCodes = new List<string>(){ "c1" };
                aidApplicationResultsEntity.ElectronicFedSchoolCodeInd = "1";
                aidApplicationResultsEntity.ElectronicTransactionIndicator = "Y";
                aidApplicationResultsEntity.VerificationSelected = "*";

                Domain.FinancialAid.Entities.AidApplicationDemographics aidApplicationDemographicsEntity = new Domain.FinancialAid.Entities.AidApplicationDemographics(aidAppDemoId, personId, aidYear, aidApplicationType);
                aidApplicationDemographicsEntity.ApplicantAssignedId = "987654321";
                aidApplicationResultsRepositoryMock.Setup(i => i.GetAidApplicationResultsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationResultsEntity);
                aidApplicationResultsRepositoryMock.Setup(i => i.CreateAidApplicationResultsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationResults>())).ReturnsAsync(aidApplicationResultsEntity);
                aidApplicationResultsRepositoryMock.Setup(i => i.UpdateAidApplicationResultsAsync(It.IsAny<Domain.FinancialAid.Entities.AidApplicationResults>())).ReturnsAsync(aidApplicationResultsEntity);
                aidApplicationDemographicsRepositoryMock.Setup(i => i.GetAidApplicationDemographicsByIdAsync(It.IsAny<string>())).ReturnsAsync(aidApplicationDemographicsEntity);

            }
        }
    }
}
