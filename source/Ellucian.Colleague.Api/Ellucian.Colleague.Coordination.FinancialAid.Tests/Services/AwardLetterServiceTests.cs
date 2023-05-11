//Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class AwardLetterServiceTests : FinancialAidServiceTestsSetup
    {
        public TestAwardLetterRepository testAwardLetterRepository;
        public TestAwardLetterHistoryRepository testAwardLetterHistoryRepository;
        public TestFinancialAidReferenceDataRepository testFinancialAidReferenceDataRepository;
        public TestStudentAwardRepository testStudentAwardRepository;
        public TestStudentRepository testStudentRepository;
        public TestApplicantRepository testApplicantRepository;
        public TestStudentAwardYearRepository testStudentAwardYearRepository;
        public TestFinancialAidOfficeRepository testOfficeRepository;
        public Domain.FinancialAid.Tests.TestFafsaRepository testFafsaRepository;

        public Mock<IAwardLetterRepository> awardLetterRepositoryMock;
        public Mock<IAwardLetterHistoryRepository> awardLetterHistoryRepositoryMock;
        public Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
        public Mock<IStudentAwardRepository> studentAwardRepositoryMock;
        public Mock<IStudentRepository> studentRepositoryMock;
        public Mock<IApplicantRepository> applicantRepositoryMock;
        public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
        public Mock<IFinancialAidOfficeRepository> officeRepositoryMock;
        public Mock<IFafsaRepository> fafsaRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        public void AwardLetterServiceTestsInitialize()
        {
            BaseInitialize();

            testAwardLetterRepository = new TestAwardLetterRepository();
            testAwardLetterHistoryRepository = new TestAwardLetterHistoryRepository();
            testFinancialAidReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
            testStudentAwardRepository = new TestStudentAwardRepository();
            testStudentRepository = new TestStudentRepository();
            testApplicantRepository = new TestApplicantRepository();
            testStudentAwardYearRepository = new TestStudentAwardYearRepository();
            testOfficeRepository = new TestFinancialAidOfficeRepository();
            testFafsaRepository = new Domain.FinancialAid.Tests.TestFafsaRepository();
        }

        #region Obsolete methods tests

        [TestClass]
        public class GetSingleAwardLetterTests : AwardLetterServiceTests
        {
            private string studentId;
            private string awardYear;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;
            private List<Fafsa> fafsaRecords;

            private IEnumerable<Domain.FinancialAid.Entities.AwardLetter> inputAwardLetterEntities;

            private Dtos.FinancialAid.AwardLetter expectedAwardLetter;
            private Dtos.FinancialAid.AwardLetter actualAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                studentId = currentUserFactory.CurrentUser.PersonId;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                awardYear = studentAwardYears.First().Code;

                fafsaRecords = new List<Fafsa>();
                foreach (var year in studentAwardYears)
                {
                    fafsaRecords.AddRange(testFafsaRepository.GetFafsaByStudentIdsAsync(new List<string> { studentId }, year.Code).Result);
                }

                inputAwardLetterEntities = testAwardLetterRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                awardLetterRepositoryMock.Setup(l => l.GetAwardLetters(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Fafsa>>())).Returns(inputAwardLetterEntities);

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                fafsaRepositoryMock.Setup(fr => fr.GetFafsaByStudentIdsAsync(new List<string> { studentId }, awardYear))
                    .Returns(testFafsaRepository.GetFafsaByStudentIdsAsync(new List<string> { studentId }, awardYear));

                var awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntities.First(a => a.AwardYear.Code == awardYear), studentAwards.Result, applicant);

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = awardLetterService.GetAwardLetters(studentId, awardYear);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testAwardLetterRepository = null;
                testFafsaRepository = null;

                expectedAwardLetter = null;
                actualAwardLetter = null;
                awardLetterRepositoryMock = null;
                awardLetterService = null;
                fafsaRepositoryMock = null;
            }


            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedAwardLetter);
                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                awardLetterService.GetAwardLetters(null, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                awardLetterService.GetAwardLetters(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void NoAwardLetterExistsforAwardYearTest()
            {
                var badAwardYear = "foobar";
                try
                {
                    awardLetterService.GetAwardLetters(studentId, badAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No award letter exists or is not active in configuration for {0} for student {1}", badAwardYear, studentId)));
                    throw;
                }
            }

            /// <summary>
            /// Current user is counselor
            /// </summary>
            [TestMethod]
            public void CurrentUserIsCounselor_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    actualAwardLetter = awardLetterService.GetAwardLetters(studentId, awardYear);
                }
                catch { exceptionThrown = true; }
                finally
                {
                    Assert.IsFalse(exceptionThrown);
                    Assert.IsNotNull(actualAwardLetter);
                }
            }

            /// <summary>
            /// Current user is counselor with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsCounselorWithNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();                

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = awardLetterService.GetAwardLetters(studentId, awardYear);                
            }

            /// <summary>
            /// Current user is proxy
            /// </summary>
            [TestMethod]
            public void CurrentUserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    actualAwardLetter = awardLetterService.GetAwardLetters(studentId, awardYear);
                }
                catch { exceptionThrown = true; }
                finally
                {
                    Assert.IsFalse(exceptionThrown);
                    Assert.IsNotNull(actualAwardLetter);
                }
            }

            /// <summary>
            /// Current user is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

               actualAwardLetter = awardLetterService.GetAwardLetters(studentId, awardYear);               
            }

            /// <summary>
            /// User is not self nor proxy nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void UserIsNotSelfNorProxyNorAdmin_ExceptionThrownTest()
            {
                awardLetterService.GetAwardLetters("bar", awardYear);
            }

            [TestMethod]
            public void UserIsStudentTest()
            {

                var lastName = "LastName";
                var student = new Domain.Student.Entities.Student(studentId, lastName, null, new List<string>(), new List<string>());
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                actualAwardLetter = awardLetterService.GetAwardLetters(studentId, awardYear);

                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            public void UserIsApplicantTest()
            {
                var lastName = "LastName";
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                var applicant = new Domain.Student.Entities.Applicant(studentId, lastName);
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                actualAwardLetter = awardLetterService.GetAwardLetters(studentId, awardYear);
                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void UserIsNeitherStudentNorApplicantTest()
            {
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                try
                {
                    actualAwardLetter = awardLetterService.GetAwardLetters(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Cannot retrieve award letter information for non-student/non-applicant person {0}.", studentId)));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetAwardLetterReportTests : AwardLetterServiceTests
        {
            private string pathToReport;
            private string pathToLogo;
            private AwardLetterService awardLetterService;

            [TestInitialize]
            public void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                
                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                
                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                
                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                
                studentRepositoryMock = new Mock<IStudentRepository>();
                
                applicantRepositoryMock = new Mock<IApplicantRepository>();
                
                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                
                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();

                fafsaRepositoryMock = new Mock<IFafsaRepository>();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                pathToReport = "pathToReport";
                pathToLogo = "pathToLogo";
                
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                testAwardLetterRepository = null;
                testFafsaRepository = null;

                awardLetterRepositoryMock = null;
                awardLetterService = null;
                fafsaRepositoryMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardLetterDtoIsNull_ExceptionThrownTest()
            {
                awardLetterService.GetAwardLetters(null, pathToReport, pathToLogo);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PathToReportIsNull_ExceptionThrownTest()
            {
                awardLetterService.GetAwardLetters(new Dtos.FinancialAid.AwardLetter(), null, pathToLogo);
            }
        }

        [TestClass]
        public class GetSingleAwardLetter2Tests : AwardLetterServiceTests
        {
            private string studentId;
            private string awardYear;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;
            private List<Fafsa> fafsaRecords;

            private IEnumerable<Domain.FinancialAid.Entities.AwardLetter> inputAwardLetterEntities;

            private Dtos.FinancialAid.AwardLetter expectedAwardLetter;
            private Dtos.FinancialAid.AwardLetter actualAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                awardYear = studentAwardYears.First().Code;

                fafsaRecords = new List<Fafsa>();
                foreach (var year in studentAwardYears)
                {
                    fafsaRecords.AddRange(testFafsaRepository.GetFafsaByStudentIdsAsync(new List<string> { studentId }, year.Code).Result);
                }

                inputAwardLetterEntities = testAwardLetterRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                awardLetterRepositoryMock.Setup(l => l.GetAwardLetters(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Fafsa>>())).Returns(inputAwardLetterEntities);

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).Returns(testOfficeRepository.GetFinancialAidOfficesAsync());

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                fafsaRepositoryMock.Setup(fr => fr.GetFafsaByStudentIdsAsync(new List<string> { studentId }, awardYear))
                    .Returns(testFafsaRepository.GetFafsaByStudentIdsAsync(new List<string> { studentId }, awardYear));

                var awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntities.First(a => a.AwardYear.Code == awardYear), studentAwards.Result, applicant);

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = awardLetterService.GetAwardLetters2(studentId, awardYear);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testAwardLetterRepository = null;
                testFafsaRepository = null;

                expectedAwardLetter = null;
                actualAwardLetter = null;
                awardLetterRepositoryMock = null;
                awardLetterService = null;
                fafsaRepositoryMock = null;
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedAwardLetter);
                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                awardLetterService.GetAwardLetters2(null, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardYearRequiredTest()
            {
                awardLetterService.GetAwardLetters2(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void NoAwardLetterExistsforAwardYearTest()
            {
                var badAwardYear = "foobar";
                try
                {
                    awardLetterService.GetAwardLetters2(studentId, badAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No award letter exists or is not active in configuration for {0} for student {1}", badAwardYear, studentId)));
                    throw;
                }
            }

            /// <summary>
            /// Current user is counselor
            /// </summary>
            [TestMethod]
            public void CurrentUserIsCounselor_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    actualAwardLetter = awardLetterService.GetAwardLetters2(studentId, awardYear);
                }
                catch { exceptionThrown = true; }
                finally
                {
                    Assert.IsFalse(exceptionThrown);
                    Assert.IsNotNull(actualAwardLetter);
                }
            }

            /// <summary>
            /// Current user is counselor with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsCounselorNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = awardLetterService.GetAwardLetters2(studentId, awardYear);                
            }

            /// <summary>
            /// Current user is proxy
            /// </summary>
            [TestMethod]
            public void CurrentUserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    actualAwardLetter = awardLetterService.GetAwardLetters2(studentId, awardYear);
                }
                catch { exceptionThrown = true; }
                finally
                {
                    Assert.IsFalse(exceptionThrown);
                    Assert.IsNotNull(actualAwardLetter);
                }
            }

            /// <summary>
            /// Current user is proxy for different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsProxyDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = awardLetterService.GetAwardLetters2(studentId, awardYear);                
            }

            /// <summary>
            /// User is not self nor proxy nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void UserIsNotSelfNorProxyNorAdmin_ExceptionThrownTest()
            {
                awardLetterService.GetAwardLetters2("bar", awardYear);
            }

            [TestMethod]
            public void UserIsStudentTest()
            {

                var lastName = "LastName";
                var student = new Domain.Student.Entities.Student(studentId, lastName, null, new List<string>(), new List<string>());
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                actualAwardLetter = awardLetterService.GetAwardLetters2(studentId, awardYear);

                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            public void UserIsApplicantTest()
            {
                var lastName = "LastName";
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                var applicant = new Domain.Student.Entities.Applicant(studentId, lastName);
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                actualAwardLetter = awardLetterService.GetAwardLetters2(studentId, awardYear);
                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void UserIsNeitherStudentNorApplicantTest()
            {
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                try
                {
                    actualAwardLetter = awardLetterService.GetAwardLetters2(studentId, awardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Cannot retrieve award letter information for non-student/non-applicant person {0}.", studentId)));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetAwardLetterReport2Tests : AwardLetterServiceTests
        {
            private string pathToReport;
            private string pathToLogo;
            private AwardLetterService awardLetterService;

            [TestInitialize]
            public void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();

                studentRepositoryMock = new Mock<IStudentRepository>();

                applicantRepositoryMock = new Mock<IApplicantRepository>();

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();

                fafsaRepositoryMock = new Mock<IFafsaRepository>();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                pathToReport = "pathToReport";
                pathToLogo = "pathToLogo";

            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                testAwardLetterRepository = null;
                testFafsaRepository = null;

                awardLetterRepositoryMock = null;
                awardLetterService = null;
                fafsaRepositoryMock = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardLetterDtoIsNull_ExceptionThrownTest()
            {
                awardLetterService.GetAwardLetterReport2(null, new Dtos.FinancialAid.AwardLetterConfiguration(), pathToReport, pathToLogo);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AwardLetterConfigurationDtoIsNull_ExceptionThrownTest()
            {
                awardLetterService.GetAwardLetterReport2(new Dtos.FinancialAid.AwardLetter2(), null, pathToReport, pathToLogo);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PathToReportIsNull_ExceptionThrownTest()
            {
                awardLetterService.GetAwardLetterReport2(new Dtos.FinancialAid.AwardLetter2(), new Dtos.FinancialAid.AwardLetterConfiguration(), null, pathToLogo);
            }
        }

        [TestClass]
        public class GetSingleAwardLetter3Tests : AwardLetterServiceTests
        {
            private string studentId;
            private StudentAwardYear awardYear;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;            

            private Domain.FinancialAid.Entities.AwardLetter2 inputAwardLetterEntity;

            private Dtos.FinancialAid.AwardLetter2 expectedAwardLetter;
            private Dtos.FinancialAid.AwardLetter2 actualAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public async void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                awardYear = studentAwardYears.First();

                var allAwards = testFinancialAidReferenceDataRepository.Awards;
                
                
                inputAwardLetterEntity = await testAwardLetterHistoryRepository.GetAwardLetterAsync(studentId, awardYear, allAwards, false);

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();
                awardLetterHistoryRepositoryMock.Setup(l => l.GetAwardLetterAsync(It.IsAny<string>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<bool>())).ReturnsAsync(inputAwardLetterEntity);

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());
                

                var awardLetterEntityAdapter = new AwardLetter2EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                //expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity, applicant);
                expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity);

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.GetAwardLetter3Async(studentId, awardYear.Code);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
                studentId = null;
                awardYear = null;
                studentAwardYears = null;
                inputAwardLetterEntity = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                awardLetterService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await awardLetterService.GetAwardLetter3Async(null, awardYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardYearIsRequiredTest()
            {
                await awardLetterService.GetAwardLetter3Async(studentId, null);
            }

            /// <summary>
            /// User is not self, proxy, or admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserIsSelf_ExceptionThrownTest()
            {
                await awardLetterService.GetAwardLetter3Async("0004791", awardYear.Code);
            }

            /// <summary>
            /// Current user is counselor
            /// </summary>
            [TestMethod]
            public async Task CurrentUserIsCounselor_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    actualAwardLetter = await awardLetterService.GetAwardLetter3Async(studentId, awardYear.Code);
                }
                catch { exceptionThrown = true; }
                finally
                {
                    Assert.IsFalse(exceptionThrown);
                    Assert.IsNotNull(actualAwardLetter);
                }
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserHasPermissions_ExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                await awardLetterService.GetAwardLetter3Async(currentUserFactory.CurrentUser.PersonId, awardYear.Code);
            }

            /// <summary>
            /// Current user is proxy
            /// </summary>
            [TestMethod]
            public async Task CurrentUserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    actualAwardLetter = await awardLetterService.GetAwardLetter3Async(studentId, awardYear.Code);
                }
                catch { exceptionThrown = true; }
                finally
                {
                    Assert.IsFalse(exceptionThrown);
                    Assert.IsNotNull(actualAwardLetter);
                }
            }

            /// <summary>
            /// Current user is proxy for different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.GetAwardLetter3Async(studentId, awardYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardYearsReturned_ExceptionThrownTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync((IEnumerable<StudentAwardYear>)null);
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetter3Async(studentId, awardYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoMatchingStudentAwardYear_ExceptionThrownTest()
            {
                await awardLetterService.GetAwardLetter3Async(studentId, "foo");
            }

            [TestMethod]
            public async Task NoAwardLetterEntityReturned_InitializedDtoIsReturnedTest()
            {
                awardLetterHistoryRepositoryMock.Setup(r => r.GetAwardLetterAsync(It.IsAny<string>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.GetAwardLetter3Async(studentId, awardYear.Code);
                Assert.IsNotNull(actualAwardLetter);
                Assert.IsNull(actualAwardLetter.Id);
            }


            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ExpectedAwardLetterReturnedTest()
            {
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
                Assert.AreEqual(expectedAwardLetter.StudentId, actualAwardLetter.StudentId);
                Assert.AreEqual(expectedAwardLetter.StudentName, actualAwardLetter.StudentName);
                Assert.AreEqual(expectedAwardLetter.AwardLetterParameterId, actualAwardLetter.AwardLetterParameterId);
            }
        }

        [TestClass]
        public class GetAllAwardLettersTests : AwardLetterServiceTests
        {
            private string studentId;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwards;
            private List<Fafsa> fafsaRecords;

            private IEnumerable<Domain.FinancialAid.Entities.AwardLetter> inputAwardLetterEntities;

            private List<Dtos.FinancialAid.AwardLetter> expectedAwardLetters;
            private IEnumerable<Dtos.FinancialAid.AwardLetter> actualAwardLetters;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                testAwardLetterRepository = new TestAwardLetterRepository();
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();
                testOfficeRepository = new TestFinancialAidOfficeRepository();

                studentId = currentUserFactory.CurrentUser.PersonId;
                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                fafsaRecords = new List<Fafsa>();

                foreach (var year in studentAwardYears)
                {
                    fafsaRecords.AddRange(testFafsaRepository.GetFafsaByStudentIdsAsync(new List<string> { studentId }, year.Code).Result);
                }

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                fafsaRepositoryMock.Setup(fr => fr.GetFafsaByStudentIdsAsync(new List<string> { studentId }, It.IsAny<string>())).ReturnsAsync(fafsaRecords);

                inputAwardLetterEntities = testAwardLetterRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                awardLetterRepositoryMock.Setup(l => l.GetAwardLetters(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Fafsa>>())).Returns(inputAwardLetterEntities);

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses).Result;

                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(studentAwardYears);

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());

                var awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedAwardLetters = new List<Dtos.FinancialAid.AwardLetter>();
                foreach (var letterEntity in inputAwardLetterEntities)
                {
                    expectedAwardLetters.Add(awardLetterEntityAdapter.MapToType(letterEntity, studentAwards, applicant));
                }

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testAwardLetterRepository = null;
                inputAwardLetterEntities = null;
                expectedAwardLetters = null;
                actualAwardLetters = null;
                awardLetterRepositoryMock = null;
                awardLetterService = null;
            }

            /// <summary>
            /// User is self 
            /// </summary>
            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedAwardLetters);
                Assert.IsNotNull(actualAwardLetters);
            }

            [TestMethod]
            public void NumAwardLettersAreEqualTest()
            {
                Assert.IsTrue(expectedAwardLetters.Count() > 0);
                Assert.IsTrue(actualAwardLetters.Count() > 0);
                Assert.AreEqual(expectedAwardLetters.Count(), actualAwardLetters.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                awardLetterService.GetAwardLetters(null);
            }

            /// <summary>
            /// User is not self nor proxy nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserNotSelfNorProxyAndNoPermissionTest()
            {
                studentId = "foobar";
                awardLetterService.GetAwardLetters(studentId);
            }

            /// <summary>
            /// Current user is counselor test
            /// </summary>
            [TestMethod]
            public void CurrentUserWithPermissionCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    awardLetterService.GetAwardLetters(studentId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// Current user is counselor with no perissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserWithNoPermissionCannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

               awardLetterService.GetAwardLetters(studentId);
            }

            /// <summary>
            /// Current user is proxy
            /// </summary>
            [TestMethod]
            public void CurrentUserIsProxyCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    awardLetterService.GetAwardLetters(studentId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// Current user is proxy for a different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsProxyDifferentPersonCannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                awardLetterService.GetAwardLetters(studentId);               
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PermissionExceptionLogsErrorTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                var message = string.Format("{0} does not have permission to access award letters for {1}", currentUserId, studentId);
                try
                {
                    awardLetterService.GetAwardLetters(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(message));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void NullStudentAwardYearsTest()
            {
                studentAwardYears = null;
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(studentAwardYears);

                try
                {
                    actualAwardLetters = awardLetterService.GetAwardLetters(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void EmptyStudentAwardYearsTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(new List<StudentAwardYear>());

                try
                {
                    actualAwardLetters = awardLetterService.GetAwardLetters(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId)));
                    throw;
                }
            }

            [TestMethod]
            public void EmptyAwardLetterListTest()
            {
                awardLetterRepositoryMock.Setup<IEnumerable<Domain.FinancialAid.Entities.AwardLetter>>(
                    l => l.GetAwardLetters(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Fafsa>>())).Returns(new List<Domain.FinancialAid.Entities.AwardLetter>());

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.AreEqual(0, actualAwardLetters.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no award letters", studentId)));
            }

            [TestMethod]
            public void NullAwardLetterListTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.AwardLetter> nullList = null;
                awardLetterRepositoryMock.Setup<IEnumerable<Domain.FinancialAid.Entities.AwardLetter>>(
                    l => l.GetAwardLetters(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Fafsa>>())).Returns(nullList);

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.AreEqual(0, actualAwardLetters.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no award letters", studentId)));
            }


            [TestMethod]
            public void NullFilteredAwardLettersTest()
            {
                //Cannot be tested until StudentAwardYear is integrated in AwardLetter domain and ApplyConfigurationService
                //is updated
            }

            [TestMethod]
            public void EmptyFilteredAwardLettersTest()
            {
                //Cannot be tested until StudentAwardYear is integrated in AwardLetter domain and ApplyConfigurationService
                //is updated
            }

            [TestMethod]
            public void NullOrEmptyFilteredStudentAwardsTest()
            {
                foreach (var studentAwardYear in studentAwardYears)
                {
                    studentAwardYear.CurrentConfiguration.IsSelfServiceActive = true;
                    studentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet.AddRange(new List<AwardStatusCategory>()
                    {
                        AwardStatusCategory.Accepted,
                        AwardStatusCategory.Denied,
                        AwardStatusCategory.Estimated,
                        AwardStatusCategory.Pending,
                        AwardStatusCategory.Rejected
                    });
                }

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.AreEqual(0, actualAwardLetters.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Configuration filtered out all StudentAwards for student {0}", studentId)));
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards or configuration filtered out all StudentAwards.", studentId)));
            }

            [TestMethod]
            public void EmptyStudentAwardsTest()
            {
                var studentAwards = new List<StudentAward>();
                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync(studentAwards);

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.AreEqual(0, actualAwardLetters.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards", studentId)));
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards or configuration filtered out all StudentAwards.", studentId)));

            }

            [TestMethod]
            public void NullStudentAwardsTest()
            {
                IEnumerable<StudentAward> studentAwards = null;
                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync(studentAwards);

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.AreEqual(0, actualAwardLetters.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards", studentId)));
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards or configuration filtered out all StudentAwards.", studentId)));
            }

            [TestMethod]
            public void UserIsStudentTest()
            {

                var lastName = "LastName";
                var student = new Domain.Student.Entities.Student(studentId, lastName, null, new List<string>(), new List<string>());
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Count() > 0);
            }

            [TestMethod]
            public void UserIsApplicantTest()
            {
                var lastName = "LastName";
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                var applicant = new Domain.Student.Entities.Applicant(studentId, lastName);
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);
                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Count() > 0);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void UserIsNeitherStudentNorApplicantTest()
            {
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                try
                {
                    actualAwardLetters = awardLetterService.GetAwardLetters(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Cannot retrieve award letter information for non-student/non-applicant person {0}.", studentId)));
                    throw;
                }
            }

            [TestMethod]
            public void NoAwardLetterWhenNoStudentAwardsForYearTest()
            {
                var removedYear = studentAwardYears.First();
                var testStudentAwards = studentAwards.Where(a => !a.StudentAwardYear.Equals(removedYear));

                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync(testStudentAwards);

                actualAwardLetters = awardLetterService.GetAwardLetters(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Count() > 0);
                Assert.AreEqual(null, actualAwardLetters.FirstOrDefault(l => l.AwardYearCode == removedYear.Code));
            }
        }

        [TestClass]
        public class GetAllAwardLetters2Tests : AwardLetterServiceTests
        {
            private string studentId;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwards;
            private List<Fafsa> fafsaRecords;

            private IEnumerable<Domain.FinancialAid.Entities.AwardLetter> inputAwardLetterEntities;

            private List<Dtos.FinancialAid.AwardLetter> expectedAwardLetters;
            private IEnumerable<Dtos.FinancialAid.AwardLetter> actualAwardLetters;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                testAwardLetterRepository = new TestAwardLetterRepository();
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();
                testOfficeRepository = new TestFinancialAidOfficeRepository();

                studentId = currentUserFactory.CurrentUser.PersonId;
                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);
                fafsaRecords = new List<Fafsa>();

                foreach (var year in studentAwardYears)
                {
                    fafsaRecords.AddRange(testFafsaRepository.GetFafsaByStudentIdsAsync(new List<string> { studentId }, year.Code).Result);
                }

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                fafsaRepositoryMock.Setup(fr => fr.GetFafsaByStudentIdsAsync(new List<string> { studentId }, It.IsAny<string>())).ReturnsAsync(fafsaRecords);

                inputAwardLetterEntities = testAwardLetterRepository.GetAwardLetters(studentId, studentAwardYears, fafsaRecords);

                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                awardLetterRepositoryMock.Setup(l => l.GetAwardLetters(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Fafsa>>())).Returns(inputAwardLetterEntities);

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses).Result;

                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(studentAwardYears);

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());

                var awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedAwardLetters = new List<Dtos.FinancialAid.AwardLetter>();
                foreach (var letterEntity in inputAwardLetterEntities)
                {
                    expectedAwardLetters.Add(awardLetterEntityAdapter.MapToType(letterEntity, studentAwards, applicant));
                }

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testAwardLetterRepository = null;
                inputAwardLetterEntities = null;
                expectedAwardLetters = null;
                actualAwardLetters = null;
                awardLetterRepositoryMock = null;
                awardLetterService = null;
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedAwardLetters);
                Assert.IsNotNull(actualAwardLetters);
            }

            [TestMethod]
            public void NumAwardLettersAreEqualTest()
            {
                Assert.IsTrue(expectedAwardLetters.Count() > 0);
                Assert.IsTrue(actualAwardLetters.Count() > 0);
                Assert.AreEqual(expectedAwardLetters.Count(), actualAwardLetters.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void StudentIdRequiredTest()
            {
                awardLetterService.GetAwardLetters2(null);
            }

            /// <summary>
            /// Current user is not self nor proxy nor admin
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserNotSelfNorProxyAndNoPermissionTest()
            {
                studentId = "foobar";
                awardLetterService.GetAwardLetters2(studentId);
            }

            /// <summary>
            /// Current user is counselor
            /// </summary>
            [TestMethod]
            public void CurrentUserWithPermissionCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    awardLetterService.GetAwardLetters2(studentId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// Current user is counselor with no permissions
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserWithNoPermissionCannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                awardLetterService.GetAwardLetters2(studentId);
               
            }

            /// <summary>
            /// Current user is proxy
            /// </summary>
            [TestMethod]
            public void CurrentUserIsProxyCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    awardLetterService.GetAwardLetters2(studentId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// Current user is proxy for different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsProxyDifferentPersonCannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                awardLetterService.GetAwardLetters2(studentId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void PermissionExceptionLogsErrorTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                var message = string.Format("{0} does not have permission to access award letters for {1}", currentUserId, studentId);
                try
                {
                    awardLetterService.GetAwardLetters2(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(message));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void NullStudentAwardYearsTest()
            {
                studentAwardYears = null;
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(studentAwardYears);

                try
                {
                    actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void EmptyStudentAwardYearsTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), true)).ReturnsAsync(new List<StudentAwardYear>());

                try
                {
                    actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no financial aid data or no award years are active in the configuration.", studentId)));
                    throw;
                }
            }

            [TestMethod]
            public void EmptyAwardLetterListTest()
            {
                awardLetterRepositoryMock.Setup<IEnumerable<Domain.FinancialAid.Entities.AwardLetter>>(
                    l => l.GetAwardLetters(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Fafsa>>())).Returns(new List<Domain.FinancialAid.Entities.AwardLetter>());

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.AreEqual(0, actualAwardLetters.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no award letters", studentId)));
            }

            [TestMethod]
            public void NullAwardLetterListTest()
            {
                IEnumerable<Domain.FinancialAid.Entities.AwardLetter> nullList = null;
                awardLetterRepositoryMock.Setup<IEnumerable<Domain.FinancialAid.Entities.AwardLetter>>(
                    l => l.GetAwardLetters(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Fafsa>>())).Returns(nullList);

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.AreEqual(0, actualAwardLetters.Count());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no award letters", studentId)));
            }


            [TestMethod]
            public void NullStudentAwards_AwardLettersReturnedTest()
            {
                IEnumerable<StudentAward> studentAwards = null;
                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync(studentAwards);

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Any());
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards", studentId)));
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards", studentId)));
            }

            [TestMethod]
            public void EmptyStudentAwards_AwardLetterReturnedTest()
            {
                var studentAwards = new List<StudentAward>();
                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync(studentAwards);

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Any());
                foreach (var letter in actualAwardLetters)
                {
                    //Total row always gets added even if there are no awards
                    Assert.IsTrue(letter.AwardTableRows.Count == 1);
                }
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards", studentId)));
                loggerMock.Verify(l => l.Debug(string.Format("Student {0} has no awards", studentId)));
            }

            [TestMethod]
            public void NullOrEmptyFilteredStudentAwards_AwardLettersReturnedTest()
            {
                foreach (var studentAwardYear in studentAwardYears)
                {
                    studentAwardYear.CurrentConfiguration.IsSelfServiceActive = true;
                    studentAwardYear.CurrentConfiguration.ExcludeAwardStatusCategoriesFromAwardLetterAndShoppingSheet.AddRange(new List<AwardStatusCategory>()
                    {
                        AwardStatusCategory.Accepted,
                        AwardStatusCategory.Denied,
                        AwardStatusCategory.Estimated,
                        AwardStatusCategory.Pending,
                        AwardStatusCategory.Rejected
                    });
                }

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Any());
                loggerMock.Verify(l => l.Debug(string.Format("Configuration filtered out all StudentAwards for student {0}", studentId)));
            }

            [TestMethod]
            public void UserIsStudentTest()
            {

                var lastName = "LastName";
                var student = new Domain.Student.Entities.Student(studentId, lastName, null, new List<string>(), new List<string>());
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Count() > 0);
            }

            [TestMethod]
            public void UserIsApplicantTest()
            {
                var lastName = "LastName";
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                var applicant = new Domain.Student.Entities.Applicant(studentId, lastName);
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);
                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Count() > 0);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void UserIsNeitherStudentNorApplicantTest()
            {
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                try
                {
                    actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Cannot retrieve award letter information for non-student/non-applicant person {0}.", studentId)));
                    throw;
                }
            }

            [TestMethod]
            public void AwardLetterExists_WhenNoStudentAwardsForYearTest()
            {
                var removedYear = studentAwardYears.First();
                var testStudentAwards = studentAwards.Where(a => !a.StudentAwardYear.Equals(removedYear));

                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync(testStudentAwards);

                actualAwardLetters = awardLetterService.GetAwardLetters2(studentId);

                Assert.IsNotNull(actualAwardLetters);
                Assert.IsTrue(actualAwardLetters.Count() > 0);
                Assert.IsNotNull(actualAwardLetters.FirstOrDefault(l => l.AwardYearCode == removedYear.Code));
            }
        }        

        [TestClass]
        public class GetAllAwardLetters3Tests : AwardLetterServiceTests
        {
            private string studentId;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;
            private Task<IEnumerable<Domain.FinancialAid.Entities.StudentAward>> studentAwards;            

            private IEnumerable<Domain.FinancialAid.Entities.AwardLetter2> inputAwardLetterEntities;

            private List<Dtos.FinancialAid.AwardLetter2> expectedAwardLetters;
            private List<Dtos.FinancialAid.AwardLetter2> actualAwardLetters;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public async void Initialize()
            {
                AwardLetterServiceTestsInitialize();                

                studentId = currentUserFactory.CurrentUser.PersonId;
                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);                

                var awardYear = studentAwardYears.First();

                var allAwards = testFinancialAidReferenceDataRepository.Awards;

                inputAwardLetterEntities = await testAwardLetterHistoryRepository.GetAwardLettersAsync(studentId, studentAwardYears, allAwards);

                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                fafsaRepositoryMock = new Mock<IFafsaRepository>();

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();
                awardLetterHistoryRepositoryMock.Setup(l => l.GetAwardLettersAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>())).ReturnsAsync(inputAwardLetterEntities);

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(studentAwardYears);

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());

                var awardLetterEntityAdapter = new AwardLetter2EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedAwardLetters = new List<Dtos.FinancialAid.AwardLetter2>();
                foreach (var letterEntity in inputAwardLetterEntities)
                {
                    //expectedAwardLetters.Add(awardLetterEntityAdapter.MapToType(letterEntity, applicant));
                    expectedAwardLetters.Add(awardLetterEntityAdapter.MapToType(letterEntity));
                }

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetters = (await awardLetterService.GetAwardLetters3Async(studentId)).ToList();
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentId = null;
                studentAwardYears = null;
                studentAwards = null;
                inputAwardLetterEntities = null;
                expectedAwardLetters = null;
                actualAwardLetters = null;
                awardLetterService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await awardLetterService.GetAwardLetters3Async(null);
            }
            
            /// <summary>
            /// User is not self nor proxy nor counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserIsSelf_ExceptionThrownTest()
            {
                await awardLetterService.GetAwardLetters3Async("0004791");
            }

            /// <summary>
            /// User is a counselor but does not have the correct permission
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserHasPermissions_ExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                await awardLetterService.GetAwardLetters3Async(currentUserFactory.CurrentUser.PersonId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsCounselor_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    await awardLetterService.GetAwardLetters3Async(studentId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    await awardLetterService.GetAwardLetters3Async(studentId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// User is proxy for another person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsProxyForAnotherPerson_CannnotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetters3Async(studentId);                
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardYearsReturned_ExceptionThrownTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync((IEnumerable<StudentAwardYear>)null);
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetters3Async(studentId);
            }

            [TestMethod]
            public async Task NoAwardLetterEntitiesReceived_EmptyDtoListReturnedTest()
            {
                awardLetterHistoryRepositoryMock.Setup(r => r.GetAwardLettersAsync(It.IsAny<string>(), It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(new List<AwardLetter2>());
                Assert.IsTrue((await awardLetterService.GetAwardLetters3Async(studentId)).Count() == 0);
            }

            [TestMethod]
            public void ExpectedNumberOfAwardLetterDtosIsReturnedTest()
            {
                Assert.AreEqual(expectedAwardLetters.Count, actualAwardLetters.Count);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AllExpectedAwardLettersReturnedTest()
            {
                foreach (var expectedLetter in expectedAwardLetters)
                {
                    Assert.IsTrue(actualAwardLetters.Any(al => al.Id == expectedLetter.Id));
                }
            }

        }

        [TestClass]        
        public class GetAwardLetterByIdAsyncTests : AwardLetterServiceTests
        {
            private string studentId;
            private string recordId;

            private Domain.FinancialAid.Entities.AwardLetter2 inputAwardLetterEntity;

            private Dtos.FinancialAid.AwardLetter2 expectedAwardLetter;
            private Dtos.FinancialAid.AwardLetter2 actualAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public async void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                var studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);                

                var allAwards = testFinancialAidReferenceDataRepository.Awards;

                inputAwardLetterEntity = await testAwardLetterHistoryRepository.GetAwardLetterAsync(studentId, studentAwardYears.First(ay => ay.Code == "2015"), allAwards, false);
                recordId = inputAwardLetterEntity.Id;

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();
                awardLetterHistoryRepositoryMock.Setup(l => l.GetAwardLetterByIdAsync(It.IsAny<string>(), It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(inputAwardLetterEntity);

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());


                var awardLetterEntityAdapter = new AwardLetter2EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                //expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity, applicant);
                expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity);

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.GetAwardLetterByIdAsync(studentId, recordId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
                studentId = null;
                recordId = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                awardLetterService = null;
                inputAwardLetterEntity = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await awardLetterService.GetAwardLetterByIdAsync(null, recordId);
            }

            /// <summary>
            /// User is not self, proxy, or admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserIsSelf_ExceptionThrownTest()
            {
                await awardLetterService.GetAwardLetterByIdAsync("0004791", recordId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            public async Task UserIsCounselor_CanAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    await awardLetterService.GetAwardLetterByIdAsync(studentId, recordId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }                
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserHasPermissions_ExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                await awardLetterService.GetAwardLetterByIdAsync(currentUserFactory.CurrentUser.PersonId, recordId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            public async Task UserIsProxy_CanAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    await awardLetterService.GetAwardLetterByIdAsync(studentId, recordId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// User is proxy for different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsProxyDifferentPerson_CannotAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetterByIdAsync(studentId, recordId);                
            }

            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RecordIdIsRequiredTest()
            {
                await awardLetterService.GetAwardLetterByIdAsync(studentId, null);
            }

            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardYearsReturned_ExceptionThrownTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync((IEnumerable<StudentAwardYear>)null);
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetterByIdAsync(studentId, recordId);
            }

            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            public async Task EmptyAwardLetterEntityReceived_InitializedDtoIsReturnedTest()
            {
                awardLetterHistoryRepositoryMock.Setup(r => r.GetAwardLetterByIdAsync(It.IsAny<string>(), It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(new AwardLetter2());

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                Assert.IsNotNull(await awardLetterService.GetAwardLetterByIdAsync(studentId, recordId));
            }


            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            public void ExpectedAwardLetterDtoIsReturnedTest()
            {
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
                Assert.AreEqual(expectedAwardLetter.AwardLetterParameterId, actualAwardLetter.AwardLetterParameterId);
                Assert.AreEqual(expectedAwardLetter.StudentId, actualAwardLetter.StudentId);
            }
        }

        [TestClass]
        public class UpdateAwardLetterTests : AwardLetterServiceTests
        {
            private string studentId;

            private Domain.FinancialAid.Entities.StudentAwardYear studentAwardYear;

            private AwardLetterDtoToEntityAdapter awardLetterDtoAdapter;

            private Dtos.FinancialAid.AwardLetter inputAwardLetterDto;
            private Domain.FinancialAid.Entities.AwardLetter inputAwardLetterEntity;
            private Domain.FinancialAid.Entities.AwardLetter updatedAwardLetterEntity;
            private Task<IEnumerable<Fafsa>> fafsaRecords;

            private Dtos.FinancialAid.AwardLetter expectedUpdatedAwardLetter;
            private Dtos.FinancialAid.AwardLetter actualUpdatedAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                testStudentAwardYearRepository = new TestStudentAwardYearRepository();
                testOfficeRepository = new TestFinancialAidOfficeRepository();

                inputAwardLetterDto = new Dtos.FinancialAid.AwardLetter()
                {
                    StudentId = studentId,
                    AwardYearCode = "2014",
                    AcceptedDate = DateTime.Today
                };

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYear = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService).FirstOrDefault(sae => sae.Code == inputAwardLetterDto.AwardYearCode);

                awardLetterDtoAdapter = new AwardLetterDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                inputAwardLetterEntity = awardLetterDtoAdapter.MapToType(inputAwardLetterDto, studentAwardYear);

                fafsaRecords = Task.FromResult((new List<Fafsa>()).AsEnumerable());
                fafsaRepositoryMock = new Mock<IFafsaRepository>();

                fafsaRecords = testFafsaRepository.GetFafsaByStudentIdsAsync(new List<string> { studentId }, studentAwardYear.Code);
                fafsaRepositoryMock.Setup(fr => fr.GetFafsaByStudentIdsAsync(new List<string> { studentId }, studentAwardYear.Code)).Returns(fafsaRecords);


                updatedAwardLetterEntity = inputAwardLetterEntity;
                updatedAwardLetterEntity.AddAwardCategoryGroup("Category1", 0, GroupType.AwardCategories);
                updatedAwardLetterEntity.AddAwardPeriodColumnGroup("Period1", 0, GroupType.AwardPeriodColumn);
                updatedAwardLetterEntity.NonAssignedAwardsGroup = new AwardLetterGroup("Group 3 title", 1, GroupType.AwardCategories);

                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                awardLetterRepositoryMock.Setup(l =>
                    l.UpdateAwardLetter(It.IsAny<Domain.FinancialAid.Entities.AwardLetter>(),
                                        It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(),
                                        It.IsAny<Domain.FinancialAid.Entities.Fafsa>())
                    ).Returns(updatedAwardLetterEntity);

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                var applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(s => s.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());

                var awardLetterEntityAdapter = new AwardLetterEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedUpdatedAwardLetter = awardLetterEntityAdapter.MapToType(updatedAwardLetterEntity, studentAwards.Result, applicant);

                adapterRegistryMock.Setup(a => a.GetAdapter<Dtos.FinancialAid.AwardLetter, Domain.FinancialAid.Entities.AwardLetter>())
                    .Returns(new AutoMapperAdapter<Dtos.FinancialAid.AwardLetter, Domain.FinancialAid.Entities.AwardLetter>(adapterRegistryMock.Object, loggerMock.Object));

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                                    awardLetterRepositoryMock.Object,
                                    awardLetterHistoryRepositoryMock.Object,
                                    financialAidReferenceDataRepositoryMock.Object,
                                    studentAwardRepositoryMock.Object,
                                    studentRepositoryMock.Object,
                                    applicantRepositoryMock.Object,
                                    studentAwardYearRepositoryMock.Object,
                                    officeRepositoryMock.Object,
                                    fafsaRepositoryMock.Object,
                                    baseConfigurationRepository,
                                    currentUserFactory,
                                    roleRepositoryMock.Object,
                                    loggerMock.Object);


            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                inputAwardLetterEntity = null;
                expectedUpdatedAwardLetter = null;
                actualUpdatedAwardLetter = null;
                awardLetterRepositoryMock = null;
                awardLetterService = null;
            }

            /// <summary>
            /// User Is Self
            /// </summary>
            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                actualUpdatedAwardLetter = awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                Assert.AreEqual(expectedUpdatedAwardLetter.StudentId, actualUpdatedAwardLetter.StudentId);
                Assert.AreEqual(expectedUpdatedAwardLetter.AwardYearCode, actualUpdatedAwardLetter.AwardYearCode);
                Assert.AreEqual(expectedUpdatedAwardLetter.AcceptedDate, actualUpdatedAwardLetter.AcceptedDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void NullArgumentThrowsExceptionTest()
            {

                awardLetterService.UpdateAwardLetter(null);
            }

            [TestMethod]
            public void AwardLetterStudentIdIsRequiredTest()
            {
                inputAwardLetterDto.StudentId = string.Empty;
                var exceptionCaught = false;
                try
                {
                    awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                }
                catch (ArgumentException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void AwardLetterAwardYearIsRequiredTest()
            {
                inputAwardLetterDto.AwardYearCode = "";
                try
                {
                    awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            public void CurrentUserNotSelf_CannotUpdateTest()
            {
                inputAwardLetterDto.StudentId = "foobar";
                var exceptionCaught = false;
                try
                {
                    awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                }
                catch (PermissionsException)
                {
                    exceptionCaught = true;
                }
                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserWithPermissionButNotSelf_CannotUpdateTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public void CurrentUserIsProxy_CannotUpdateTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void NullActiveStudentAwardYearThrowsExceptionTest()
            {
                inputAwardLetterDto.AwardYearCode = "foobar";
                try
                {
                    awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student has no financial aid data for {0}", inputAwardLetterDto.AwardYearCode)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void NullStudentAwardsThrowsExceptionTest()
            {
                IEnumerable<StudentAward> nullAwardList = null;
                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>()))
                    .ReturnsAsync(nullAwardList);

                try
                {
                    awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student has no awards or Configuration filtered out all StudentAwards for student {0} and award year {1}", inputAwardLetterDto.StudentId, inputAwardLetterDto.AwardYearCode)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void EmptyStudentAwardsThrowsExceptionTest()
            {
                IEnumerable<StudentAward> emptyList = new List<StudentAward>();
                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>()))
                    .ReturnsAsync(emptyList);

                try
                {
                    awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student has no awards or Configuration filtered out all StudentAwards for student {0} and award year {1}", inputAwardLetterDto.StudentId, inputAwardLetterDto.AwardYearCode)));
                    throw;
                }
            }

            [TestMethod]
            public void AwardLetterForStudentTest()
            {
                Domain.Student.Entities.Student student = new Domain.Student.Entities.Student(inputAwardLetterDto.StudentId, "lastName", null, new List<string>(), new List<string>());
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                actualUpdatedAwardLetter = awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                Assert.AreEqual(inputAwardLetterDto.StudentId, actualUpdatedAwardLetter.StudentId);
            }

            [TestMethod]
            public void AwardLetterForApplicantTest()
            {
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(inputAwardLetterDto.StudentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                actualUpdatedAwardLetter = awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                Assert.AreEqual(inputAwardLetterDto.StudentId, actualUpdatedAwardLetter.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PersonNotStudentOrApplicantThrowsExceptionTest()
            {
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.GetAsync(studentId)).ReturnsAsync(student);

                Domain.Student.Entities.Applicant applicant = null;
                applicantRepositoryMock.Setup(r => r.GetApplicantAsync(studentId)).ReturnsAsync(applicant);

                try
                {
                    awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Cannot create award letter for non-student/non-applicant person {0}.", inputAwardLetterDto.StudentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public void NullAwardLetterReturnedFromUpdateThrowsExceptionTest()
            {
                updatedAwardLetterEntity = null;
                awardLetterRepositoryMock.Setup(r => r.UpdateAwardLetter(It.IsAny<AwardLetter>(), It.IsAny<StudentAwardYear>(), It.IsAny<Fafsa>())).Returns(updatedAwardLetterEntity);

                try
                {
                    awardLetterService.UpdateAwardLetter(inputAwardLetterDto);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Null award letter object returned by repository update method for student {0} award year {1}", inputAwardLetterDto.StudentId, inputAwardLetterDto.AwardYearCode)));
                    throw;
                }
            }
        }       

        [TestClass]
        public class UpdateAwardLetter2Tests : AwardLetterServiceTests
        {
            private string studentId;
            private StudentAwardYear studentAwardYear;

            private AwardLetter2 inputAwardLetterEntity;            

            private Dtos.FinancialAid.AwardLetter2 expectedAwardLetter;
            private Dtos.FinancialAid.AwardLetter2 actualAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public async void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                var studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);                

                var allAwards = testFinancialAidReferenceDataRepository.Awards;

                var inputLetter = await testAwardLetterHistoryRepository.GetAwardLetterAsync(studentId, studentAwardYears.First(ay => ay.Code == "2015"), allAwards, false);
                inputAwardLetterEntity = await testAwardLetterHistoryRepository.UpdateAwardLetterAsync(studentId, inputLetter, studentAwardYears.First(ay => ay.Code == "2015"), allAwards);                

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();
                awardLetterHistoryRepositoryMock.Setup(l => l.UpdateAwardLetterAsync(It.IsAny<string>(), It.IsAny<AwardLetter2>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(inputAwardLetterEntity);

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());


                var awardLetterEntityAdapter = new AwardLetter2EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                //expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity, applicant);
                expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity);

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
                studentId = null;
                studentAwardYear = null;                
                inputAwardLetterEntity = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                awardLetterService = null;

                fafsaRepositoryMock = null;
                awardLetterRepositoryMock = null;
                awardLetterHistoryRepositoryMock = null;
                financialAidReferenceDataRepositoryMock = null;
                studentAwardRepositoryMock = null;
                studentRepositoryMock = null;
                applicantRepositoryMock = null;
                studentAwardYearRepositoryMock = null;
                officeRepositoryMock = null;
               
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ActualAwardLetter_EqualsExpectedTest()
            {
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
                Assert.AreEqual(expectedAwardLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
                Assert.AreEqual(DateTime.Today, actualAwardLetter.AcceptedDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardLetter_ExceptionThrownTest()
            {
                await awardLetterService.UpdateAwardLetter2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullStudentId_ExceptionThrownTest()
            {
                expectedAwardLetter.StudentId = null;
                await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NullStudentId_ExpectedMessageIsLoggedTest()
            {
                expectedAwardLetter.StudentId = null;
                try
                {
                    await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
                }
                catch (ArgumentException)
                {
                    loggerMock.Verify(l => l.Error("Input argument awardLetter is invalid. StudentId cannot be null or empty"));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task EmptyStringAwardYear_ExceptionThrownTest()
            {
                expectedAwardLetter.AwardLetterYear = string.Empty;
                await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NullAwardLetterYear_ExpectedMessageIsLoggedTest()
            {
                expectedAwardLetter.AwardLetterYear = null;
                try
                {
                    await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
                }
                catch (ArgumentException)
                {
                    loggerMock.Verify(l => l.Error("Input argument awardLetter is invalid. AwardYear cannot be null or empty"));
                }
            }

            /// <summary>
            /// User is not self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserIsSelf_ExceptionThrownTest()
            {
                expectedAwardLetter.StudentId = "foo";
                await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NotUserIsSelf_ExpectedMessageIsLoggedTest()
            {
                expectedAwardLetter.StudentId = "foo";
                try
                {
                    await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
                }
                catch (PermissionsException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to update award letter for {1}", currentUserFactory.CurrentUser.PersonId, expectedAwardLetter.StudentId)));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoMatchingAwardYearRetreived_ExceptionThrownTest()
            {
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(new List<StudentAwardYear>());

                await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NoMatchingAwardYearRetrieved_ExpectedMessageIsLoggedTest()
            {
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(new List<StudentAwardYear>());

                try
                {
                    await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
                }
                catch (InvalidOperationException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student has no financial aid data for {0}", expectedAwardLetter.AwardLetterYear)));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardsRetrieved_ExceptionThrownTest()
            {
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync((IEnumerable<StudentAward>)null);

                await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NoStudentAwardsRetrieved_ExcpectedMessageIsLoggedTest()
            {
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync((IEnumerable<StudentAward>)null);

                try
                {
                    await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
                }
                catch (InvalidOperationException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student has no awards or Configuration filtered out all StudentAwards for student {0} and award year {1}", expectedAwardLetter.StudentId, expectedAwardLetter.AwardLetterYear)));
                }
            }            

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task NoUpdatedAwardLetterEntityReceived_ExceptionThrownTest()
            {
                awardLetterHistoryRepositoryMock.Setup(l => l.UpdateAwardLetterAsync(It.IsAny<string>(), It.IsAny<AwardLetter2>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(() => null);

                await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NoUpdatedAwardLetterEntityReceived_ExpectedMessageIsLoggedTest()
            {
                awardLetterHistoryRepositoryMock.Setup(l => l.UpdateAwardLetterAsync(It.IsAny<string>(), It.IsAny<AwardLetter2>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(() => null);

                try
                {
                    await awardLetterService.UpdateAwardLetter2Async(expectedAwardLetter);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Null award letter object returned by repository update method for student {0} award year {1}", expectedAwardLetter.StudentId, expectedAwardLetter.AwardLetterYear)));
                }
            }
        }

        #endregion

        [TestClass]
        public class UpdateAwardLetter3AsyncTests : AwardLetterServiceTests
        {
            private string studentId;
            private StudentAwardYear studentAwardYear;

            private AwardLetter3 inputAwardLetterEntity;

            private Dtos.FinancialAid.AwardLetter3 expectedAwardLetter;
            private Dtos.FinancialAid.AwardLetter3 actualAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public async void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                var studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                var allAwards = testFinancialAidReferenceDataRepository.Awards;

                var inputLetter = await testAwardLetterHistoryRepository.GetAwardLetter2Async(studentId, studentAwardYears.First(ay => ay.Code == "2015"), allAwards, false);
                inputAwardLetterEntity = await testAwardLetterHistoryRepository.UpdateAwardLetter2Async(studentId, inputLetter, studentAwardYears.First(ay => ay.Code == "2015"), allAwards);

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();
                awardLetterHistoryRepositoryMock.Setup(l => l.UpdateAwardLetter2Async(It.IsAny<string>(), It.IsAny<AwardLetter3>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(inputAwardLetterEntity);

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());


                var awardLetterEntityAdapter = new AwardLetter3EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                //expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity, applicant);
                expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity);

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
                studentId = null;
                studentAwardYear = null;
                inputAwardLetterEntity = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                awardLetterService = null;

                fafsaRepositoryMock = null;
                awardLetterRepositoryMock = null;
                awardLetterHistoryRepositoryMock = null;
                financialAidReferenceDataRepositoryMock = null;
                studentAwardRepositoryMock = null;
                studentRepositoryMock = null;
                applicantRepositoryMock = null;
                studentAwardYearRepositoryMock = null;
                officeRepositoryMock = null;

            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ActualAwardLetter_EqualsExpectedTest()
            {
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
                Assert.AreEqual(expectedAwardLetter.AcceptedDate, actualAwardLetter.AcceptedDate);
                Assert.AreEqual(DateTime.Today, actualAwardLetter.AcceptedDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardLetter_ExceptionThrownTest()
            {
                await awardLetterService.UpdateAwardLetter3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task NullStudentId_ExceptionThrownTest()
            {
                expectedAwardLetter.StudentId = null;
                await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NullStudentId_ExpectedMessageIsLoggedTest()
            {
                expectedAwardLetter.StudentId = null;
                try
                {
                    await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
                }
                catch (ArgumentException)
                {
                    loggerMock.Verify(l => l.Error("Input argument awardLetter is invalid. StudentId cannot be null or empty"));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task EmptyStringAwardYear_ExceptionThrownTest()
            {
                expectedAwardLetter.AwardLetterYear = string.Empty;
                await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NullAwardLetterYear_ExpectedMessageIsLoggedTest()
            {
                expectedAwardLetter.AwardLetterYear = null;
                try
                {
                    await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
                }
                catch (ArgumentException)
                {
                    loggerMock.Verify(l => l.Error("Input argument awardLetter is invalid. AwardYear cannot be null or empty"));
                }
            }

            /// <summary>
            /// User is not self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserIsSelf_ExceptionThrownTest()
            {
                expectedAwardLetter.StudentId = "foo";
                await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NotUserIsSelf_ExpectedMessageIsLoggedTest()
            {
                expectedAwardLetter.StudentId = "foo";
                try
                {
                    await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
                }
                catch (PermissionsException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to update award letter for {1}", currentUserFactory.CurrentUser.PersonId, expectedAwardLetter.StudentId)));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoMatchingAwardYearRetreived_ExceptionThrownTest()
            {
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(new List<StudentAwardYear>());

                await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NoMatchingAwardYearRetrieved_ExpectedMessageIsLoggedTest()
            {
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(new List<StudentAwardYear>());

                try
                {
                    await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
                }
                catch (InvalidOperationException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student has no financial aid data for {0}", expectedAwardLetter.AwardLetterYear)));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardsRetrieved_ExceptionThrownTest()
            {
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync((IEnumerable<StudentAward>)null);

                await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NoStudentAwardsRetrieved_ExcpectedMessageIsLoggedTest()
            {
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).ReturnsAsync((IEnumerable<StudentAward>)null);

                try
                {
                    await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
                }
                catch (InvalidOperationException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student has no awards or Configuration filtered out all StudentAwards for student {0} and award year {1}", expectedAwardLetter.StudentId, expectedAwardLetter.AwardLetterYear)));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task NoUpdatedAwardLetterEntityReceived_ExceptionThrownTest()
            {
                awardLetterHistoryRepositoryMock.Setup(l => l.UpdateAwardLetter2Async(It.IsAny<string>(), It.IsAny<AwardLetter3>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(() => null);

                await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
            }

            [TestMethod]
            public async Task NoUpdatedAwardLetterEntityReceived_ExpectedMessageIsLoggedTest()
            {
                awardLetterHistoryRepositoryMock.Setup(l => l.UpdateAwardLetter2Async(It.IsAny<string>(), It.IsAny<AwardLetter3>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(() => null);

                try
                {
                    await awardLetterService.UpdateAwardLetter3Async(expectedAwardLetter);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Null award letter object returned by repository update method for student {0} award year {1}", expectedAwardLetter.StudentId, expectedAwardLetter.AwardLetterYear)));
                }
            }
        }

        [TestClass]
        public class GetAwardLetters4AsyncTests : AwardLetterServiceTests
        {
            private string studentId;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;
            private Task<IEnumerable<Domain.FinancialAid.Entities.StudentAward>> studentAwards;

            private IEnumerable<Domain.FinancialAid.Entities.AwardLetter3> inputAwardLetterEntities;

            private List<Dtos.FinancialAid.AwardLetter3> expectedAwardLetters;
            private List<Dtos.FinancialAid.AwardLetter3> actualAwardLetters;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public async void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                var awardYear = studentAwardYears.First();

                var allAwards = testFinancialAidReferenceDataRepository.Awards;

                inputAwardLetterEntities = await testAwardLetterHistoryRepository.GetAwardLetters2Async(studentId, studentAwardYears, allAwards);

                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                fafsaRepositoryMock = new Mock<IFafsaRepository>();

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();
                awardLetterHistoryRepositoryMock.Setup(l => l.GetAwardLetters2Async(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>())).ReturnsAsync(inputAwardLetterEntities);

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync(studentAwardYears);

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());

                var awardLetterEntityAdapter = new AwardLetter3EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedAwardLetters = new List<Dtos.FinancialAid.AwardLetter3>();
                foreach (var letterEntity in inputAwardLetterEntities)
                {
                    //expectedAwardLetters.Add(awardLetterEntityAdapter.MapToType(letterEntity, applicant));
                    expectedAwardLetters.Add(awardLetterEntityAdapter.MapToType(letterEntity));
                }

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetters = (await awardLetterService.GetAwardLetters4Async(studentId)).ToList();
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentId = null;
                studentAwardYears = null;
                studentAwards = null;
                inputAwardLetterEntities = null;
                expectedAwardLetters = null;
                actualAwardLetters = null;
                awardLetterService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await awardLetterService.GetAwardLetters4Async(null);
            }

            /// <summary>
            /// User is not self nor proxy nor counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserIsSelf_ExceptionThrownTest()
            {
                await awardLetterService.GetAwardLetters4Async("0004791");
            }

            /// <summary>
            /// User is a counselor but does not have the correct permission
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserHasPermissions_ExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                await awardLetterService.GetAwardLetters4Async(currentUserFactory.CurrentUser.PersonId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsCounselor_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    await awardLetterService.GetAwardLetters4Async(studentId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task UserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    await awardLetterService.GetAwardLetters4Async(studentId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// User is proxy for another person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsProxyForAnotherPerson_CannnotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetters4Async(studentId);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardYearsReturned_ExceptionThrownTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync((IEnumerable<StudentAwardYear>)null);
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetters4Async(studentId);
            }

            [TestMethod]
            public async Task NoAwardLetterEntitiesReceived_EmptyDtoListReturnedTest()
            {
                awardLetterHistoryRepositoryMock.Setup(r => r.GetAwardLetters2Async(It.IsAny<string>(), It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(new List<AwardLetter3>());
                Assert.IsTrue((await awardLetterService.GetAwardLetters4Async(studentId)).Count() == 0);
            }

            [TestMethod]
            public void ExpectedNumberOfAwardLetterDtosIsReturnedTest()
            {
                Assert.AreEqual(expectedAwardLetters.Count, actualAwardLetters.Count);
            }

            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void AllExpectedAwardLettersReturnedTest()
            {
                foreach (var expectedLetter in expectedAwardLetters)
                {
                    Assert.IsTrue(actualAwardLetters.Any(al => al.Id == expectedLetter.Id));
                }
            }

        }

        [TestClass]
        public class GetAwardLetterById2AsyncTests : AwardLetterServiceTests
        {
            private string studentId;
            private string recordId;

            private Domain.FinancialAid.Entities.AwardLetter3 inputAwardLetterEntity;

            private Dtos.FinancialAid.AwardLetter3 expectedAwardLetter;
            private Dtos.FinancialAid.AwardLetter3 actualAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public async void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                var studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                var allAwards = testFinancialAidReferenceDataRepository.Awards;

                inputAwardLetterEntity = await testAwardLetterHistoryRepository.GetAwardLetter2Async(studentId, studentAwardYears.First(ay => ay.Code == "2015"), allAwards, false);
                recordId = inputAwardLetterEntity.Id;

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();

                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();
                awardLetterHistoryRepositoryMock.Setup(l => l.GetAwardLetterById2Async(It.IsAny<string>(), It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(inputAwardLetterEntity);

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());


                var awardLetterEntityAdapter = new AwardLetter3EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                //expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity, applicant);
                expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity);

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.GetAwardLetterById2Async(studentId, recordId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
                studentId = null;
                recordId = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                awardLetterService = null;
                inputAwardLetterEntity = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await awardLetterService.GetAwardLetterById2Async(null, recordId);
            }

            /// <summary>
            /// User is not self, proxy, or admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserIsSelf_ExceptionThrownTest()
            {
                await awardLetterService.GetAwardLetterById2Async("0004791", recordId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            public async Task UserIsCounselor_CanAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    await awardLetterService.GetAwardLetterById2Async(studentId, recordId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserHasPermissions_ExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                await awardLetterService.GetAwardLetterById2Async(currentUserFactory.CurrentUser.PersonId, recordId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            public async Task UserIsProxy_CanAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    await awardLetterService.GetAwardLetterById2Async(studentId, recordId);
                }
                catch { exceptionThrown = true; }
                finally { Assert.IsFalse(exceptionThrown); }
            }

            /// <summary>
            /// User is proxy for different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsProxyDifferentPerson_CannotAccesDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetterById2Async(studentId, recordId);
            }

            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RecordIdIsRequiredTest()
            {
                await awardLetterService.GetAwardLetterById2Async(studentId, null);
            }

            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardYearsReturned_ExceptionThrownTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync((IEnumerable<StudentAwardYear>)null);
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetterById2Async(studentId, recordId);
            }

            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            public async Task EmptyAwardLetterEntityReceived_InitializedDtoIsReturnedTest()
            {
                awardLetterHistoryRepositoryMock.Setup(r => r.GetAwardLetterById2Async(It.IsAny<string>(), It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>()))
                    .ReturnsAsync(new AwardLetter3());

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                Assert.IsNotNull(await awardLetterService.GetAwardLetterById2Async(studentId, recordId));
            }


            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            [TestCategory("GetAwardLetterByIdAsync")]
            public void ExpectedAwardLetterDtoIsReturnedTest()
            {
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
                Assert.AreEqual(expectedAwardLetter.AwardLetterParameterId, actualAwardLetter.AwardLetterParameterId);
                Assert.AreEqual(expectedAwardLetter.StudentId, actualAwardLetter.StudentId);
            }
        }

        [TestClass]
        public class GetAwardLetter4AsyncTests : AwardLetterServiceTests
        {
            private string studentId;
            private StudentAwardYear awardYear;
            private IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYears;

            private Domain.FinancialAid.Entities.AwardLetter3 inputAwardLetterEntity;

            private Dtos.FinancialAid.AwardLetter3 expectedAwardLetter;
            private Dtos.FinancialAid.AwardLetter3 actualAwardLetter;

            private AwardLetterService awardLetterService;

            [TestInitialize]
            public async void Initialize()
            {
                AwardLetterServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                var currentOfficeService = new CurrentOfficeService(testOfficeRepository.GetFinancialAidOffices());
                studentAwardYears = testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService);

                awardYear = studentAwardYears.First();

                var allAwards = testFinancialAidReferenceDataRepository.Awards;


                inputAwardLetterEntity = await testAwardLetterHistoryRepository.GetAwardLetter2Async(studentId, awardYear, allAwards, false);

                fafsaRepositoryMock = new Mock<IFafsaRepository>();
                awardLetterRepositoryMock = new Mock<IAwardLetterRepository>();
                awardLetterHistoryRepositoryMock = new Mock<IAwardLetterHistoryRepository>();
                awardLetterHistoryRepositoryMock.Setup(l => l.GetAwardLetter2Async(It.IsAny<string>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<bool>())).ReturnsAsync(inputAwardLetterEntity);

                financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards).Returns(testFinancialAidReferenceDataRepository.Awards);
                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses).Returns(testFinancialAidReferenceDataRepository.AwardStatuses);

                var studentAwards = testStudentAwardRepository.GetAllStudentAwardsAsync(
                    studentId,
                    testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService),
                    testFinancialAidReferenceDataRepository.Awards,
                    testFinancialAidReferenceDataRepository.AwardStatuses);

                studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
                studentAwardRepositoryMock.Setup(r =>
                    r.GetAllStudentAwardsAsync(studentId, It.IsAny<IEnumerable<StudentAwardYear>>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<IEnumerable<AwardStatus>>())
                    ).Returns(studentAwards);

                studentRepositoryMock = new Mock<IStudentRepository>();
                Domain.Student.Entities.Student student = null;
                studentRepositoryMock.Setup(r => r.Get(studentId)).Returns(student);

                applicantRepositoryMock = new Mock<IApplicantRepository>();
                Domain.Student.Entities.Applicant applicant = new Domain.Student.Entities.Applicant(studentId, "LastName");
                applicantRepositoryMock.Setup(r => r.GetApplicant(studentId)).Returns(applicant);

                studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
                studentAwardYearRepositoryMock.Setup(
                    y => y.GetStudentAwardYearsAsync(studentId, It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())
                    ).ReturnsAsync(testStudentAwardYearRepository.GetStudentAwardYears(studentId, currentOfficeService));

                officeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
                officeRepositoryMock.Setup(f => f.GetFinancialAidOfficesAsync()).ReturnsAsync(testOfficeRepository.GetFinancialAidOffices());


                var awardLetterEntityAdapter = new AwardLetter3EntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                //expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity, applicant);
                expectedAwardLetter = awardLetterEntityAdapter.MapToType(inputAwardLetterEntity);

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.GetAwardLetter4Async(studentId, awardYear.Code);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();
                studentId = null;
                awardYear = null;
                studentAwardYears = null;
                inputAwardLetterEntity = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                awardLetterService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await awardLetterService.GetAwardLetter4Async(null, awardYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardYearIsRequiredTest()
            {
                await awardLetterService.GetAwardLetter4Async(studentId, null);
            }

            /// <summary>
            /// User is not self, proxy, or admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserIsSelf_ExceptionThrownTest()
            {
                await awardLetterService.GetAwardLetter4Async("0004791", awardYear.Code);
            }

            /// <summary>
            /// Current user is counselor
            /// </summary>
            [TestMethod]
            public async Task CurrentUserIsCounselor_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    actualAwardLetter = await awardLetterService.GetAwardLetter4Async(studentId, awardYear.Code);
                }
                catch { exceptionThrown = true; }
                finally
                {
                    Assert.IsFalse(exceptionThrown);
                    Assert.IsNotNull(actualAwardLetter);
                }
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NotUserHasPermissions_ExceptionThrownTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                await awardLetterService.GetAwardLetter4Async(currentUserFactory.CurrentUser.PersonId, awardYear.Code);
            }

            /// <summary>
            /// Current user is proxy
            /// </summary>
            [TestMethod]
            public async Task CurrentUserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                bool exceptionThrown = false;
                try
                {
                    actualAwardLetter = await awardLetterService.GetAwardLetter4Async(studentId, awardYear.Code);
                }
                catch { exceptionThrown = true; }
                finally
                {
                    Assert.IsFalse(exceptionThrown);
                    Assert.IsNotNull(actualAwardLetter);
                }
            }

            /// <summary>
            /// Current user is proxy for different person
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.GetAwardLetter4Async(studentId, awardYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardYearsReturned_ExceptionThrownTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>())).ReturnsAsync((IEnumerable<StudentAwardYear>)null);
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                await awardLetterService.GetAwardLetter4Async(studentId, awardYear.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoMatchingStudentAwardYear_ExceptionThrownTest()
            {
                await awardLetterService.GetAwardLetter4Async(studentId, "foo");
            }

            [TestMethod]
            public async Task NoAwardLetterEntityReturned_InitializedDtoIsReturnedTest()
            {
                awardLetterHistoryRepositoryMock.Setup(r => r.GetAwardLetter2Async(It.IsAny<string>(), It.IsAny<StudentAwardYear>(), It.IsAny<IEnumerable<Award>>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                awardLetterService = new AwardLetterService(adapterRegistryMock.Object,
                    awardLetterRepositoryMock.Object,
                    awardLetterHistoryRepositoryMock.Object,
                    financialAidReferenceDataRepositoryMock.Object,
                    studentAwardRepositoryMock.Object,
                    studentRepositoryMock.Object,
                    applicantRepositoryMock.Object,
                    studentAwardYearRepositoryMock.Object,
                    officeRepositoryMock.Object,
                    fafsaRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualAwardLetter = await awardLetterService.GetAwardLetter4Async(studentId, awardYear.Code);
                Assert.IsNotNull(actualAwardLetter);
                Assert.IsNull(actualAwardLetter.Id);
            }


            /// <summary>
            /// User is self
            /// </summary>
            [TestMethod]
            public void ExpectedAwardLetterReturnedTest()
            {
                Assert.AreEqual(expectedAwardLetter.Id, actualAwardLetter.Id);
                Assert.AreEqual(expectedAwardLetter.StudentId, actualAwardLetter.StudentId);
                Assert.AreEqual(expectedAwardLetter.StudentName, actualAwardLetter.StudentName);
                Assert.AreEqual(expectedAwardLetter.AwardLetterParameterId, actualAwardLetter.AwardLetterParameterId);
            }
        }

    }
}
