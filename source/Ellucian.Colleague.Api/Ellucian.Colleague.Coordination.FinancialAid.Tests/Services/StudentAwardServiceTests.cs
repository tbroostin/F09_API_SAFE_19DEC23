//Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{

    [TestClass]
    public class StudentAwardServiceTests : FinancialAidServiceTestsSetup
    {
        public string studentId;
        public string awardYear;
        public string awardId;

        public Mock<IStudentAwardRepository> studentAwardRepositoryMock;
        public Mock<IStudentLoanLimitationRepository> studentLoanLimitationRepositoryMock;
        public Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
        public Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;
        public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
        public Mock<ITermRepository> termRepositoryMock;
        public Mock<IAwardPackageChangeRequestRepository> awardPackageChangeRequestRepositoryMock;
        public Mock<ICommunicationRepository> communicationRepositoryMock;

        public TestFinancialAidOfficeRepository financialAidOfficeRepository;
        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestStudentAwardRepository studentAwardRepository;
        public TestFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        public TestStudentLoanLimitationRepository studentLoanLimitationRepository;
        public TestCommunicationRepository communicationRepository;
        public TestTermRepository termRepo;
        public CurrentOfficeService currentOfficeService;
        public List<Domain.FinancialAid.Entities.Award> allAwards;
        public List<Domain.FinancialAid.Entities.AwardStatus> allAwardStatuses;

        public StudentAwardEntityToDtoAdapter studentAwardDtoAdapter;
        public StudentAwardDtoToEntityAdapter studentAwardEntityAdapter;

        public FunctionEqualityComparer<Dtos.FinancialAid.StudentAward> studentAwardDtoComparer;

        public StudentAwardService studentAwardService;
        
        private void BuildStudentAwardService()
        {
            studentAwardService =  new StudentAwardService(
                adapterRegistryMock.Object,
                studentAwardRepositoryMock.Object,
                studentLoanLimitationRepositoryMock.Object,
                financialAidReferenceDataRepositoryMock.Object,
                financialAidOfficeRepositoryMock.Object,
                studentAwardYearRepositoryMock.Object,
                awardPackageChangeRequestRepositoryMock.Object,
                communicationRepositoryMock.Object,
                termRepositoryMock.Object,
                baseConfigurationRepository,
                currentUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object);
        }

        ////Domain entities can be modified for tests by changing the record representations in the test repositories
        public IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> financialAidOfficeEntities
        { get { return financialAidOfficeRepository.GetFinancialAidOffices(); } }

        public IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities
        { get { return studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeEntities)); } }

        public IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwardEntities
        { get { return (studentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYearEntities, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses)).Result; } }

        //Dtos
        public virtual List<Dtos.FinancialAid.StudentAward> expectedStudentAwards
        {
            get { return studentAwardEntities.Select(studentAwardEntity => studentAwardDtoAdapter.MapToType(studentAwardEntity)).ToList(); }
        }

        public IEnumerable<Dtos.FinancialAid.StudentAward> actualStudentAwards;

        public void StudentAwardServiceTestsInitialize()
        {
            BaseInitialize();

            studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
            studentLoanLimitationRepositoryMock = new Mock<IStudentLoanLimitationRepository>();
            financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
            financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
            termRepositoryMock = new Mock<ITermRepository>();
            awardPackageChangeRequestRepositoryMock = new Mock<IAwardPackageChangeRequestRepository>();
            communicationRepositoryMock = new Mock<ICommunicationRepository>();

            financialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            studentAwardRepository = new TestStudentAwardRepository();
            financialAidReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
            studentLoanLimitationRepository = new TestStudentLoanLimitationRepository();
            termRepo = new TestTermRepository();
            communicationRepository = new TestCommunicationRepository();
            allAwards = financialAidReferenceDataRepository.Awards.ToList();
            allAwardStatuses = financialAidReferenceDataRepository.AwardStatuses.ToList();

            financialAidReferenceDataRepositoryMock.Setup(rdr => rdr.Awards).Returns(allAwards);
            financialAidReferenceDataRepositoryMock.Setup(rdr => rdr.AwardStatuses).Returns(allAwardStatuses);
            
            studentAwardDtoAdapter = new StudentAwardEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
            studentAwardEntityAdapter = new StudentAwardDtoToEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            studentAwardDtoComparer = new FunctionEqualityComparer<Dtos.FinancialAid.StudentAward>(
                (s1, s2) => s1.AwardId == s2.AwardId && s1.StudentId == s2.StudentId && s1.AwardYearId == s2.AwardYearId,
                (s) => s.AwardYearId.GetHashCode() ^ s.StudentId.GetHashCode() ^ s.AwardId.GetHashCode());

            financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
            .Returns(financialAidOfficeRepository.GetFinancialAidOfficesAsync());

            studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult(studentAwardYearRepository.GetStudentAwardYears(id, currentOfficeService)));

            financialAidReferenceDataRepositoryMock.Setup(r => r.Awards)
                    .Returns(() => financialAidReferenceDataRepository.Awards);

            financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses)
                .Returns(() => financialAidReferenceDataRepository.AwardStatuses);

            studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>, IEnumerable<Domain.FinancialAid.Entities.Award>, IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>
                    (async (id, studentAwardYears, awards, awardStatuses) => await studentAwardRepository.GetAllStudentAwardsAsync(id, studentAwardYears, awards, awardStatuses));

            studentAwardRepositoryMock.Setup(r => r.GetStudentAwardsForYearAsync(It.IsAny<string>(), It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                .Returns<string, Domain.FinancialAid.Entities.StudentAwardYear, IEnumerable<Domain.FinancialAid.Entities.Award>, IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>
                    ((id, studentAwardYear, awards, awardStatuses) => studentAwardRepository.GetStudentAwardsForYearAsync(id, studentAwardYear, awards, awardStatuses));

            studentAwardRepositoryMock.Setup(r => r.UpdateStudentAwardsAsync(It.IsAny<Domain.FinancialAid.Entities.StudentAwardYear>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAward>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                .Returns<Domain.FinancialAid.Entities.StudentAwardYear, IEnumerable<Domain.FinancialAid.Entities.StudentAward>, IEnumerable<Domain.FinancialAid.Entities.Award>, IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>(
                (studentAwardYear, studentAwards, awards, awardStatuses) => studentAwardRepository.UpdateStudentAwardsAsync(studentAwardYear, studentAwards, awards, awardStatuses));

            studentLoanLimitationRepositoryMock.Setup(r => r.GetStudentLoanLimitationsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>(
                (id, studentAwardYears) => studentLoanLimitationRepository.GetStudentLoanLimitationsAsync(id, studentAwardYears));

            communicationRepositoryMock.Setup(r => r.SubmitCommunication(It.IsAny<Domain.Base.Entities.Communication>()))
                .Returns<Domain.Base.Entities.Communication>((com) => communicationRepository.SubmitCommunication(com));

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.FinancialAid.Entities.StudentAward, Dtos.FinancialAid.StudentAward>())
                    .Returns(studentAwardDtoAdapter);

            BuildStudentAwardService();
        }

        public void StudentAwardServiceTestsCleanup()
        {
            BaseCleanup();
            studentAwardRepositoryMock = null;
            studentLoanLimitationRepositoryMock = null;
            financialAidReferenceDataRepositoryMock = null;
            financialAidOfficeRepositoryMock = null;
            studentAwardYearRepositoryMock = null;

            financialAidOfficeRepository = null;
            studentAwardYearRepository = null;
            financialAidReferenceDataRepository = null;
            currentOfficeService = null;
            allAwards = null;
            allAwardStatuses = null;
        }

        [TestClass]
        public class StudentAwardService_GetAllStudentAwards : StudentAwardServiceTests
        {

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardServiceTestsInitialize();
                studentId = currentUserFactory.CurrentUser.PersonId;
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardServiceTestsCleanup();
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActual()
            {
                actualStudentAwards = await studentAwardService.GetStudentAwardsAsync(studentId);
                CollectionAssert.AreEqual(expectedStudentAwards, actualStudentAwards.ToList(), studentAwardDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequired()
            {
                await studentAwardService.GetStudentAwardsAsync("");
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CounselorCanAccessStudentData()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildStudentAwardService();

                await studentAwardService.GetStudentAwardsAsync(studentId);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorWithNoPermissions_CannotAccessStudentData()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                
                BuildStudentAwardService();

                await studentAwardService.GetStudentAwardsAsync(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ProxyCanAccessStudentData()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildStudentAwardService();

                await studentAwardService.GetStudentAwardsAsync(studentId);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserIsProxyDifferentPerson_CannotAccessStudentData()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildStudentAwardService();

                await studentAwardService.GetStudentAwardsAsync(studentId);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserMustBeStudentOrCounselorOrProxyTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                try
                {
                    await studentAwardService.GetStudentAwardsAsync(studentId);
                }
                catch (Exception)
                {
                    var message = string.Format("{0} does not have permission to get student awards for {1}", currentUserId, studentId);
                    loggerMock.Verify(l => l.Error(message));
                    throw;
                }
            }

            [TestMethod]
            public async Task NoStudentAwardYears_LogsMessageReturnsEmptyListTest()
            {
                studentAwardYearRepository.ClearAwardYears();
                var emptyDtoList = await studentAwardService.GetStudentAwardsAsync(studentId);
                Assert.AreEqual(0, emptyDtoList.Count());
                loggerMock.Verify(l => l.Error(string.Format("No award years exist for student {0}", studentId)));
            }

            [TestMethod]
            public async Task EmptyStudentAwardsReturnsEmptyList()
            {
                studentAwardRepository.awardData = new List<TestStudentAwardRepository.TestAwardRecord>();
                studentAwardRepository.awardPeriodData = new List<TestStudentAwardRepository.TestAwardPeriodRecord>();

                var emptyDtoList = await studentAwardService.GetStudentAwardsAsync(studentId);
                Assert.AreEqual(0, emptyDtoList.Count());
                loggerMock.Verify(l => l.Info(string.Format("No awards exist in any of the award years for student {0}", studentId)));
            }

            [TestMethod]
            public async Task EmptyFilteredStudentAwards_LogsMessageReturnsEmptyListTest()
            {
                financialAidOfficeRepository.officeParameterRecordData.ForEach(p => p.IsAwardingActive = "N");
                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
            .Returns(financialAidOfficeRepository.GetFinancialAidOfficesAsync());
                var emptyDtoList = await studentAwardService.GetStudentAwardsAsync(studentId);
                Assert.AreEqual(0, emptyDtoList.Count());
                loggerMock.Verify(l => l.Info(string.Format("Office Configurations have filtered out all student awards for student {0}", studentId)));
            }

        }

        [TestClass]
        public class StudentAwardService_GetYearStudentAwards : StudentAwardServiceTests
        {

            public Domain.FinancialAid.Entities.StudentAwardYear studentAwardYearEntity
            {
                get { return studentAwardYearEntities.First(); }
            }
            public IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwardEntitiesForYear
            {
                get { return studentAwardEntities.Where(sa => sa.StudentAwardYear.Equals(studentAwardYearEntity)); }
            }

            public override List<Dtos.FinancialAid.StudentAward> expectedStudentAwards
            {
                get { return studentAwardEntitiesForYear.Select(studentAwardEntity => studentAwardDtoAdapter.MapToType(studentAwardEntity)).ToList(); }
            }

            public IEnumerable<Dtos.FinancialAid.StudentAward> actualStudentAwards;
            
            [TestInitialize]
            public void Initialize()
            {
                StudentAwardServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                awardYear = studentAwardYearEntity.Code;
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardServiceTestsCleanup();
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActual()
            {
                actualStudentAwards = await studentAwardService.GetStudentAwardsAsync(studentId, awardYear);
                CollectionAssert.AreEqual(expectedStudentAwards, actualStudentAwards.ToList(), studentAwardDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequired()
            {
                await studentAwardService.GetStudentAwardsAsync("", studentAwardYearEntity.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardYearRequired()
            {
                await studentAwardService.GetStudentAwardsAsync(studentId, "");
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CounselorCanAccessStudentData()
            {
                bool exceptionThrown = false;
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildStudentAwardService();

                try
                {
                    await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorWithNoPermissions_CannotAccessStudentData()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
               
                BuildStudentAwardService();

                await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ProxyCanAccessStudentData()
            {
                bool exceptionThrown = false;
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildStudentAwardService();

                try
                {
                    await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code);
                }
                catch { exceptionThrown = true; }
                Assert.IsFalse(exceptionThrown);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyForADifferentPerson_CannotAccessStudentData()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildStudentAwardService();

                await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserMustBeStudentOrCounselorOrProxyTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                try
                {
                    await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to get student awards for {1}", currentUserId, studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AwardYearDoesNotExistInStudentAwardYears_LogMessageReturnEmptyListTest()
            {
                var fakeAwardYear = "foobar";
                try
                {
                    await studentAwardService.GetStudentAwardsAsync(studentId, fakeAwardYear);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("awardYear {0} does not exist for student {1}", fakeAwardYear, studentId)));
                    throw;
                }
            }

            [TestMethod]
            public async Task EmptyStudentAwardsReturnsEmptyList()
            {
                studentAwardRepository.awardData = new List<TestStudentAwardRepository.TestAwardRecord>();
                studentAwardRepository.awardPeriodData = new List<TestStudentAwardRepository.TestAwardPeriodRecord>();

                var emptyDtoList = await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code);
                Assert.IsTrue(emptyDtoList.Count() == 0);
                loggerMock.Verify(l => l.Info(string.Format("awardYear {0} has no awards for student {1}", studentAwardYearEntity.Code, studentId)));
            }


            [TestMethod]
            public async Task EmptyFilteredAwards_LogsMessageReturnsEmptyListTest()
            {
                financialAidOfficeRepository.officeParameterRecordData.ForEach(o => o.IsAwardingActive = "");
                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
            .Returns(financialAidOfficeRepository.GetFinancialAidOfficesAsync());
                var emptyDtoList = await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code);
                Assert.AreEqual(0, emptyDtoList.Count());
                loggerMock.Verify(l => l.Info(string.Format("FA Office {0} filtered out all StudentAwards for student {1} awardYear {2}", studentAwardYearEntity.CurrentOffice.Id, studentId, studentAwardYearEntity.Code)));
            }       

        }

        [TestClass]
        public class StudentAwardService_GetSingleStudentAward : StudentAwardServiceTests
        {
            public Domain.FinancialAid.Entities.StudentAwardYear studentAwardYearEntity
            {
                get { return studentAwardYearEntities.First(); }
            }
            public IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwardEntitiesForYear
            {
                get { return studentAwardEntities.Where(sa => sa.StudentAwardYear.Equals(studentAwardYearEntity)); }
            }

            public Dtos.FinancialAid.StudentAward expectedStudentAward
            {
                get
                {
                    return studentAwardDtoAdapter.MapToType(
                        studentAwardEntitiesForYear.First(sa => sa.Award.Code == awardId));
                }
            }

            public Dtos.FinancialAid.StudentAward actualStudentAward;
            
            [TestInitialize]
            public void Initialize()
            {
                StudentAwardServiceTestsInitialize();


                studentId = currentUserFactory.CurrentUser.PersonId;
                awardYear = studentAwardYearEntity.Code;
                awardId = studentAwardEntitiesForYear.First().Award.Code;
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardServiceTestsCleanup();
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualStudentAward = await studentAwardService.GetStudentAwardsAsync(studentId, awardYear, awardId);
                Assert.AreEqual(expectedStudentAward.AwardYearId, actualStudentAward.AwardYearId);
                Assert.AreEqual(expectedStudentAward.StudentId, actualStudentAward.StudentId);
                Assert.AreEqual(expectedStudentAward.AwardId, actualStudentAward.AwardId);
                Assert.AreEqual(expectedStudentAward.StudentAwardPeriods.Count(), actualStudentAward.StudentAwardPeriods.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardIdRequired()
            {
                await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code, "");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentAwardCannotBeFoundTest()
            {
                financialAidOfficeRepository.officeParameterRecordData.ForEach(o => o.IsAwardingActive = null);
                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
            .Returns(financialAidOfficeRepository.GetFinancialAidOfficesAsync());
                try
                {
                    await studentAwardService.GetStudentAwardsAsync(studentId, studentAwardYearEntity.Code, awardId);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Unable to retrieve studentAward resource for student {0}, awardYear {1} and awardId {2}", studentId, awardYear, awardId)));
                    throw;
                }
            }
        }

        [TestClass]
        public class StudentAwardService_UpdateStudentAwards : StudentAwardServiceTests
        {
            public Domain.FinancialAid.Entities.StudentAwardYear studentAwardYearEntity
            {
                get { return studentAwardYearEntities.First(); }
            }
            public IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwardEntitiesForYear
            {
                get { return studentAwardEntities.Where(sa => sa.StudentAwardYear.Equals(studentAwardYearEntity)); }
            }

            public override List<Dtos.FinancialAid.StudentAward> expectedStudentAwards
            {
                get { return studentAwardEntitiesForYear.Select(studentAwardEntity => studentAwardDtoAdapter.MapToType(studentAwardEntity)).ToList(); }
            }

            public List<Dtos.FinancialAid.StudentAward> inputStudentAwards;

            public List<Domain.Base.Entities.Communication> submittedCommunications;

            [TestInitialize]
            public void Initialize()
            {
                StudentAwardServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;
                awardYear = studentAwardYearEntity.Code;

                inputStudentAwards = expectedStudentAwards;

                submittedCommunications = new List<Domain.Base.Entities.Communication>();
                communicationRepositoryMock.Setup(r => r.SubmitCommunication(It.IsAny<Domain.Base.Entities.Communication>()))
                    .Callback<Domain.Base.Entities.Communication>((comm) => submittedCommunications.Add(comm));
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardServiceTestsCleanup();
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualStudentAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, expectedStudentAwards);
                CollectionAssert.AreEqual(expectedStudentAwards, actualStudentAwards.ToList(), studentAwardDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await studentAwardService.UpdateStudentAwardsAsync(null, awardYear, expectedStudentAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AwardYearRequiredTest()
            {
                await studentAwardService.UpdateStudentAwardsAsync(studentId, null, expectedStudentAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InputList_RequiredTest()
            {
                await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, null);
            }

            [TestMethod]
            public async Task EmptyInputList_ReturnsEmptyList()
            {
                var emptyList = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, new List<Dtos.FinancialAid.StudentAward>());
                Assert.IsTrue(emptyList.Count() == 0);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsNotSelf_ExceptionTest()
            {
                var currentStudentId = studentId;
                studentId = "foobar";
                try
                {
                    var updatedAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, expectedStudentAwards);
                }
                catch (PermissionsException)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to update StudentAwards for {1}", currentStudentId, studentId)));
                    throw;
                }
            }
            
            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NotAllAwardsForSameStudent_ExceptionTest()
            {
                var inputAwards = expectedStudentAwards;
                inputAwards.First().StudentId = "foobar";
                await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NotAllAwardsForSameYear_ExceptionTest()
            {
                await studentAwardService.UpdateStudentAwardsAsync(studentId, "foobar", expectedStudentAwards);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoStudentAwardYears_LogsMessageThrowsInvalidOperationExceptionTest()
            {
                studentAwardYearRepository.ClearAwardYears();
                try
                {
                    var emptyDtoList = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputStudentAwards);
                }
                catch (Exception e)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Student {0} has no award years", studentId)));
                    throw e;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task AwardYearDoesNotExist_LogMessageThrowExceptionTest()
            {
                var inputAwards = expectedStudentAwards;
                awardYear = "foobar";
                inputAwards.ForEach(a => a.AwardYearId = awardYear);

                try
                {
                    await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputAwards);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Award Year {0} does not exist for studentId {1}", awardYear, studentId)));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoAwards_LogMessageThrowExceptionTest()
            {
                var inputStudentAwards = expectedStudentAwards;
                financialAidReferenceDataRepository.awardRecordData = new List<TestFinancialAidReferenceDataRepository.AwardRecord>();
                try
                {
                    var updatedAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputStudentAwards);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No awards exist")));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task NoAwardStatuses_LogMessageThrowExceptionTest()
            {
                financialAidReferenceDataRepository.AwardActionData = new string[,] { };
                try
                {
                    var updatedAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputStudentAwards);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No award statuses exist")));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task EmptyStudentAwards_LogMessageThrowException()
            {
                var inputStudentAwards = expectedStudentAwards;
                studentAwardRepository.awardData = new List<TestStudentAwardRepository.TestAwardRecord>();
                studentAwardRepository.awardPeriodData = new List<TestStudentAwardRepository.TestAwardPeriodRecord>();

                try
                {
                    await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputStudentAwards);
                }
                catch (Exception e)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No student awards exist for student {0} awardYear {1}", studentId, awardYear)));
                    throw e;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InputAwardDoesNotExist_LogMessageThrowExceptionTest()
            {
                var inputAwards = expectedStudentAwards;
                inputAwards.Add(new Dtos.FinancialAid.StudentAward()
                    {
                        AwardYearId = awardYear,
                        StudentId = studentId,
                        AwardId = "foobar"
                    });

                try
                {
                    await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputAwards);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("Award {0} does not exist", "foobar")));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task NoStudentLoanLimitationsExist_LogMessageThrowExceptionTest()
            {
                studentLoanLimitationRepository.testLoanLimitsData = new List<TestStudentLoanLimitationRepository.TestLoanLimitation>();
                try
                {
                    var updatedAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, expectedStudentAwards);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("No student loan limitations exist for student {0} awardYear {1}", studentId, awardYear)));
                    throw;
                }
            }

            [TestMethod]
            public void NoCommunicationsSubmittedTest()
            {
                financialAidOfficeRepository.officeParameterRecordData.ForEach(o =>
                    {
                        o.AcceptedAwardCommunicationCode = string.Empty;
                        o.RejectedAwardCommunicationCode = string.Empty;
                        o.LoanChangeCommunicationCode = string.Empty;
                    });

                var updatedAwards = actualStudentAwards;

                Assert.AreEqual(0, submittedCommunications.Count());
            }

            [TestMethod]
            public async Task MultipleCommunicationsSubmittedTest()
            {
                var loanChangeAward = studentAwardRepository.DeepCopy(studentAwardEntitiesForYear.First(sa => sa.Award.IsFederalDirectLoan));
                loanChangeAward.StudentAwardPeriods.ForEach(p => p.AwardAmount = p.AwardAmount + 1);
                inputStudentAwards.Add(studentAwardDtoAdapter.MapToType(loanChangeAward));

                var acceptedAwardStatus = financialAidReferenceDataRepository.AwardStatuses.First(s => s.Category == AwardStatusCategory.Accepted);
                var acceptedAward = studentAwardRepository.DeepCopy(
                    studentAwardEntitiesForYear.First(sa =>
                        sa.StudentAwardPeriods.All(p => p.AwardStatus != null && p.AwardStatus.Category != Domain.FinancialAid.Entities.AwardStatusCategory.Accepted && !p.IsTransmitted)));
                
                acceptedAward.StudentAwardPeriods.ForEach(p => p.AwardStatus = acceptedAwardStatus );
                             
                inputStudentAwards.Add(studentAwardDtoAdapter.MapToType(acceptedAward));

                var loanChangeCommCode = "FOO";
                var acceptedChangeCommCode = "BAR";
                financialAidOfficeRepository.officeParameterRecordData.ForEach(o =>
                {
                    o.AcceptedAwardCommunicationCode = acceptedChangeCommCode;
                    o.RejectedAwardCommunicationCode = string.Empty;
                    o.LoanChangeCommunicationCode = loanChangeCommCode;
                });
                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
            .Returns(financialAidOfficeRepository.GetFinancialAidOfficesAsync());
                var updatedAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputStudentAwards);

                Assert.AreEqual(2, submittedCommunications.Count());
                Assert.IsTrue(submittedCommunications.Select(c => c.Code).Contains(loanChangeCommCode));
                Assert.IsTrue(submittedCommunications.Select(c => c.Code).Contains(acceptedChangeCommCode));
            }

            [TestMethod]
            public async Task OneCommunicationsSubmittedTest()
            {
                var loanChangeAward = studentAwardRepository.DeepCopy(studentAwardEntitiesForYear.First(sa => sa.Award.IsFederalDirectLoan));
                loanChangeAward.StudentAwardPeriods.ForEach(p => p.AwardAmount = p.AwardAmount + 1);
                inputStudentAwards.Add(studentAwardDtoAdapter.MapToType(loanChangeAward));

                var commCode = "FOOBAR";
                financialAidOfficeRepository.officeParameterRecordData.ForEach(o =>
                {
                    o.AcceptedAwardCommunicationCode = string.Empty;
                    o.RejectedAwardCommunicationCode = string.Empty;
                    o.LoanChangeCommunicationCode = commCode;
                });
                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
            .Returns(financialAidOfficeRepository.GetFinancialAidOfficesAsync());
                var updatedAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputStudentAwards);

                Assert.AreEqual(1, submittedCommunications.Count());
                Assert.AreEqual(commCode, submittedCommunications[0].Code);
            }

            [TestMethod]
            public async Task ErrorSubmittingCommunicationTest()
            {
                var exception = new ApplicationException("ae");
                communicationRepositoryMock.Setup(r => r.SubmitCommunication(It.IsAny<Domain.Base.Entities.Communication>()))
                    .Throws(exception);

                var loanChangeAward = studentAwardRepository.DeepCopy(studentAwardEntitiesForYear.First(sa => sa.Award.IsFederalDirectLoan));
                loanChangeAward.StudentAwardPeriods.ForEach(p => p.AwardAmount = p.AwardAmount + 1);
                inputStudentAwards.Add(studentAwardDtoAdapter.MapToType(loanChangeAward));

                var commCode = "FOOBAR";
                financialAidOfficeRepository.officeParameterRecordData.ForEach(o =>
                {
                    o.AcceptedAwardCommunicationCode = string.Empty;
                    o.RejectedAwardCommunicationCode = string.Empty;
                    o.LoanChangeCommunicationCode = commCode;
                });
            
                var updatedAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, inputStudentAwards);

                loggerMock.Verify(l => l.Warn(It.Is<Exception>(e => e.GetType() == exception.GetType()), It.IsAny<string>()));

            }

            [TestMethod]
            public async Task ExcludePeriodFromView_PeriodNotReturnedTest()
            {
                financialAidOfficeRepository.officeParameterRecordData.ForEach(o => o.AwardPeriodsToExcludeFromView = new List<string>() { "13/SP" });
                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
            .Returns(financialAidOfficeRepository.GetFinancialAidOfficesAsync());
                var studentAwardWithExcludedPeriod = new Dtos.FinancialAid.StudentAward() {
                    AwardId = "Goofy",
                    AwardYearId = "2012",
                    StudentAwardPeriods = new List<Dtos.FinancialAid.StudentAwardPeriod>()
                    {
                        new Dtos.FinancialAid.StudentAwardPeriod() {AwardPeriodId = "13/SP", AwardAmount=2354, AwardId = "Goofy", AwardYearId = "2012", AwardStatusId = "P"},
                        new Dtos.FinancialAid.StudentAwardPeriod() {AwardPeriodId = "12/FA", AwardAmount=2354, AwardId = "Goofy", AwardYearId = "2012", AwardStatusId = "P"}
                    },
                    StudentId = "0003914"
                };
                var updatedAwards = await studentAwardService.UpdateStudentAwardsAsync(studentId, studentAwardYearEntity.Code, new List<Dtos.FinancialAid.StudentAward> { studentAwardWithExcludedPeriod });
                Assert.IsTrue(updatedAwards.Any());
                Assert.IsTrue(updatedAwards.First().StudentAwardPeriods.Count == 1);
                Assert.IsTrue(updatedAwards.First().StudentAwardPeriods.First().AwardPeriodId != "13/SP");
            }

        }

        [TestClass]
        public class StudentAwardService_UpdateStudentAward : StudentAwardServiceTests
        { 
            public Domain.FinancialAid.Entities.StudentAwardYear studentAwardYearEntity
            {
                get { return studentAwardYearEntities.First(); }
            }
            public IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwardEntitiesForYear
            {
                get { return studentAwardEntities.Where(sa => sa.StudentAwardYear.Equals(studentAwardYearEntity)); }
            }

            public Dtos.FinancialAid.StudentAward expectedStudentAward
            {
                get
                {
                    return studentAwardDtoAdapter.MapToType(
                        studentAwardEntitiesForYear.First(sa => sa.Award.Code == awardId));
                }
            }

            public Dtos.FinancialAid.StudentAward actualStudentAward;
            
            [TestInitialize]
            public void Initialize()
            {
                StudentAwardServiceTestsInitialize();
                studentId = currentUserFactory.CurrentUser.PersonId;
                awardYear = studentAwardYearEntity.Code;
                awardId = studentAwardEntitiesForYear.First().Award.Code;
            }

            [TestCleanup]
            public void Cleanup()
            {
                StudentAwardServiceTestsCleanup();
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualStudentAward = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, awardId, expectedStudentAward);
                Assert.AreEqual(expectedStudentAward.AwardYearId, actualStudentAward.AwardYearId);
                Assert.AreEqual(expectedStudentAward.StudentId, actualStudentAward.StudentId);
                Assert.AreEqual(expectedStudentAward.AwardId, actualStudentAward.AwardId);
                Assert.AreEqual(expectedStudentAward.StudentAwardPeriods.Count(), actualStudentAward.StudentAwardPeriods.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardIdThrowsExceptionTest()
            {
                await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, null, expectedStudentAward);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullAwardThrowsExceptionTest()
            {
                await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, awardId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task MismatchedInputDtoAwardIdThrowsExceptionTest()
            {
                awardId = "foobar";

                try
                {
                    var update = await studentAwardService.UpdateStudentAwardsAsync(studentId, awardYear, awardId, expectedStudentAward);
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("awardId {0} must match awardId {1} of studentAward resource", awardId, expectedStudentAward.AwardId)));
                    throw;
                }
            }
        }
    }
}
