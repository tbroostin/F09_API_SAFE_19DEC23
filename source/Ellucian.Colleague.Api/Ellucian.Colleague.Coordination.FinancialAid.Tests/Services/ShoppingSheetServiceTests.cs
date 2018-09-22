/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class ShoppingSheetServiceTests : FinancialAidServiceTestsSetup
    {

        public Mock<IFinancialAidOfficeRepository> financialAidOfficeRepositoryMock;
        public Mock<IStudentAwardYearRepository> studentAwardYearRepositoryMock;
        public Mock<IFinancialAidReferenceDataRepository> financialAidReferenceDataRepositoryMock;
        public Mock<IStudentAwardRepository> studentAwardRepositoryMock;
        public Mock<IStudentBudgetComponentRepository> studentBudgetComponentRepositoryMock;
        public Mock<IFafsaRepository> fafsaRepositoryMock;
        public Mock<IProfileApplicationRepository> profileApplicationRepositoryMock;
        public Mock<IRuleTableRepository> ruleTableRepositoryMock;
        public Mock<IRuleRepository> ruleRepositoryMock;

        public TestFinancialAidOfficeRepository financialAidOfficeRepository;
        public TestStudentAwardYearRepository studentAwardYearRepository;
        public TestFinancialAidReferenceDataRepository financialAidReferenceDataRepository;
        public TestStudentAwardRepository studentAwardRepository;
        public TestStudentBudgetComponentRepository studentBudgetComponentRepository;
        public TestFafsaRepository fafsaRepository;
        public TestProfileApplicationRepository profileApplicationRepository;
      

        public ITypeAdapter<Domain.FinancialAid.Entities.ShoppingSheet, ShoppingSheet> shoppingSheetEntityToDtoAdapter;

        public FunctionEqualityComparer<ShoppingSheet> shoppingSheetDtoComparer;

        public void ShoppingSheetServiceTestsInitialize()
        {
            BaseInitialize();

            financialAidOfficeRepositoryMock = new Mock<IFinancialAidOfficeRepository>();
            studentAwardYearRepositoryMock = new Mock<IStudentAwardYearRepository>();
            financialAidReferenceDataRepositoryMock = new Mock<IFinancialAidReferenceDataRepository>();
            studentAwardRepositoryMock = new Mock<IStudentAwardRepository>();
            studentBudgetComponentRepositoryMock = new Mock<IStudentBudgetComponentRepository>();
            fafsaRepositoryMock = new Mock<IFafsaRepository>();
            profileApplicationRepositoryMock = new Mock<IProfileApplicationRepository>();
            ruleTableRepositoryMock = new Mock<IRuleTableRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();


            financialAidOfficeRepository = new TestFinancialAidOfficeRepository();
            studentAwardYearRepository = new TestStudentAwardYearRepository();
            financialAidReferenceDataRepository = new TestFinancialAidReferenceDataRepository();
            studentAwardRepository = new TestStudentAwardRepository();
            studentBudgetComponentRepository = new TestStudentBudgetComponentRepository();
            fafsaRepository = new TestFafsaRepository();
            profileApplicationRepository = new TestProfileApplicationRepository();

            shoppingSheetEntityToDtoAdapter = new ShoppingSheetEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);

            shoppingSheetDtoComparer = new FunctionEqualityComparer<ShoppingSheet>(
                (s1, s2) => s1.AwardYear == s2.AwardYear && s1.StudentId == s2.StudentId,
                (ss) => ss.AwardYear.GetHashCode() ^ ss.StudentId.GetHashCode());
        }

        [TestClass]
        public class GetShoppingSheetsTests : ShoppingSheetServiceTests
        {
            public string studentId;

            ////Domain entities can be modified for tests by changing the record representations in the test repositories
            public IEnumerable<Domain.FinancialAid.Entities.FinancialAidOffice> financialAidOfficeEntities
            { get { return financialAidOfficeRepository.GetFinancialAidOffices(); } }

            public IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear> studentAwardYearEntities
            { get { return studentAwardYearRepository.GetStudentAwardYears(studentId, new CurrentOfficeService(financialAidOfficeEntities)); } }

            public IEnumerable<Domain.FinancialAid.Entities.BudgetComponent> budgetComponentEntities
            { get { return financialAidReferenceDataRepository.BudgetComponents; } }

            public IEnumerable<Domain.FinancialAid.Entities.StudentAward> studentAwardEntities
            { get { return studentAwardRepository.GetAllStudentAwardsAsync(studentId, studentAwardYearEntities, financialAidReferenceDataRepository.Awards, financialAidReferenceDataRepository.AwardStatuses).Result; } }

            public IEnumerable<Domain.FinancialAid.Entities.StudentBudgetComponent> studentBudgetComponentEntities
            { get { return studentBudgetComponentRepository.GetStudentBudgetComponentsAsync(studentId, studentAwardYearEntities).Result; } }

            public IEnumerable<Domain.FinancialAid.Entities.Fafsa> fafsaEntities
            { get { return fafsaRepository.GetFafsasAsync(new List<string>() { studentId }, studentAwardYearEntities.Select(y => y.Code)).Result; } }

            public IEnumerable<Domain.FinancialAid.Entities.ProfileApplication> profileApplicationEntities
            { get { return profileApplicationRepository.GetProfileApplicationsAsync(studentId, studentAwardYearEntities).Result; } }

            public IEnumerable<Domain.FinancialAid.Entities.FinancialAidApplication2> financialAidApplicationEntities
            {
                get
                {
                    var financialAidApplications = new List<Domain.FinancialAid.Entities.FinancialAidApplication2>();
                    financialAidApplications.AddRange(fafsaEntities);
                    financialAidApplications.AddRange(profileApplicationEntities);
                    return financialAidApplications;
                }
            }

            public async Task<IEnumerable<Domain.FinancialAid.Entities.ShoppingSheet>> shoppingSheetEntities()
            {
                var entities = new List<Domain.FinancialAid.Entities.ShoppingSheet>();
                foreach (var studentAwardYear in studentAwardYearEntities)
                {
                    try
                    {
                        entities.Add(await ShoppingSheetDomainService.BuildShoppingSheetAsync(studentAwardYear, studentAwardEntities, budgetComponentEntities,
                              studentBudgetComponentEntities, financialAidApplicationEntities, null));
                    }
                    catch (Exception) { }
                }
                return entities;
            }

            //Dtos
            public async Task<List<ShoppingSheet>> expectedShoppingSheets()
            {
                 return (await shoppingSheetEntities()).Select(shoppingSheet => shoppingSheetEntityToDtoAdapter.MapToType(shoppingSheet)).ToList(); 
            }

            public async Task<IEnumerable<ShoppingSheet>> actualShoppingSheetsAsync()
            {
                return await shoppingSheetService.GetShoppingSheetsAsync(studentId); 
            }

            public ShoppingSheetService shoppingSheetService;

            [TestInitialize]
            public void Initialize()
            {
                ShoppingSheetServiceTestsInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                financialAidOfficeRepositoryMock.Setup(r => r.GetFinancialAidOfficesAsync())
                    .Returns(() => financialAidOfficeRepository.GetFinancialAidOfficesAsync());

                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult(studentAwardYearRepository.GetStudentAwardYears(id, currentOfficeService)));

                financialAidReferenceDataRepositoryMock.Setup(r => r.Awards)
                    .Returns(() => financialAidReferenceDataRepository.Awards);

                financialAidReferenceDataRepositoryMock.Setup(r => r.AwardStatuses)
                    .Returns(() => financialAidReferenceDataRepository.AwardStatuses);

                financialAidReferenceDataRepositoryMock.Setup(r => r.BudgetComponents)
                    .Returns(() => financialAidReferenceDataRepository.BudgetComponents);

                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>, IEnumerable<Domain.FinancialAid.Entities.Award>, IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>
                        ((id, studentAwardYears, awards, awardStatuses) => studentAwardRepository.GetAllStudentAwardsAsync(id, studentAwardYears, awards, awardStatuses));

                studentBudgetComponentRepositoryMock.Setup(r => r.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>((id, studentAwardYears) => studentBudgetComponentRepository.GetStudentBudgetComponentsAsync(id, studentAwardYears));

                fafsaRepositoryMock.Setup(r => r.GetFafsasAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>, IEnumerable<string>>((ids, awardYears) => fafsaRepository.GetFafsasAsync(ids, awardYears));

                profileApplicationRepositoryMock.Setup(r => r.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>
                    ((id, studentAwardYears) => profileApplicationRepository.GetProfileApplicationsAsync(id, studentAwardYears));

                adapterRegistryMock.Setup(r => r.GetAdapter<Domain.FinancialAid.Entities.ShoppingSheet, ShoppingSheet>())
                    .Returns(shoppingSheetEntityToDtoAdapter);

                BuildService();
            }

            private void BuildService()
            {
                shoppingSheetService = new ShoppingSheetService(
                                    adapterRegistryMock.Object,
                                    financialAidOfficeRepositoryMock.Object,
                                    studentAwardYearRepositoryMock.Object,
                                    financialAidReferenceDataRepositoryMock.Object,
                                    studentAwardRepositoryMock.Object,
                                    studentBudgetComponentRepositoryMock.Object,
                                    fafsaRepositoryMock.Object,
                                    profileApplicationRepositoryMock.Object,
                                    ruleTableRepositoryMock.Object,
                                    ruleRepositoryMock.Object,
                                    baseConfigurationRepository,
                                    currentUserFactory,
                                    roleRepositoryMock.Object,
                                    loggerMock.Object);
            }

            /// <summary>
            /// User is self
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ExpectedEqualsActual()
            {
                CollectionAssert.AreEqual((await expectedShoppingSheets()).ToList(), (await actualShoppingSheetsAsync()).ToList(), shoppingSheetDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdIsRequiredTest()
            {
                await shoppingSheetService.GetShoppingSheetsAsync(null);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CounselorCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildService();

                CollectionAssert.AreEqual((await expectedShoppingSheets()).ToList(), (await actualShoppingSheetsAsync()).ToList(), shoppingSheetDtoComparer);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorWithNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                
                BuildService();

                await actualShoppingSheetsAsync();
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ProxyCanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildService();

                CollectionAssert.AreEqual((await expectedShoppingSheets()).ToList(), (await actualShoppingSheetsAsync()).ToList(), shoppingSheetDtoComparer);
            }

            /// <summary>
            /// User is proxy for different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyForDifferentPersons_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildService();

                await actualShoppingSheetsAsync();
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserNotRequestingSelfAndUserNotCounselorOrProxyTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));

                try
                {
                    await shoppingSheetService.GetShoppingSheetsAsync("foobar");
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(string.Format("{0} does not have permission to access shopping sheet resources for {1}", currentUserFactory.CurrentUser.PersonId, "foobar")));
                    throw;
                }
            }

            [TestMethod]
            public async Task NullStudentAwardYearsReturnsEmptyListTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult((new List<Domain.FinancialAid.Entities.StudentAwardYear>()).AsEnumerable()));

                Assert.AreEqual(0, (await actualShoppingSheetsAsync()).Count());
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no award years for which to get shopping sheets", studentId)));
            }

            [TestMethod]
            public async Task EmptyStudentAwardYearsReturnsEmptyListTest()
            {
                studentAwardYearRepositoryMock.Setup(r => r.GetStudentAwardYearsAsync(It.IsAny<string>(), It.IsAny<CurrentOfficeService>(), It.IsAny<bool>()))
                    .Returns<string, CurrentOfficeService, bool>((id, currentOfficeService, b) => Task.FromResult((new List<Domain.FinancialAid.Entities.StudentAwardYear>()).AsEnumerable()));

                Assert.AreEqual(0, (await actualShoppingSheetsAsync()).Count());
                loggerMock.Verify(l => l.Info(string.Format("Student {0} has no award years for which to get shopping sheets", studentId)));
            }

            /// <summary>
            /// ShoppingSheetDomainService will throw an exception if BudgetComponents are null, but StudentBudgetComponents
            /// are not null. That's tested in ShoppingSheetDomainServiceTests, so just testing that it has no effect
            /// on the ShoppingSheetService method.
            /// </summary>
            [TestMethod]
            public async Task NullBudgetAndStudentBudgetComponentsHasNoEffectTest()
            {
                financialAidReferenceDataRepository.BudgetComponentData = new List<TestFinancialAidReferenceDataRepository.BudgetComponentRecord>();
                studentBudgetComponentRepository.csStudentRecords = new List<TestStudentBudgetComponentRepository.CsStudentRecord>();

                Assert.AreEqual((await expectedShoppingSheets()).Count(), (await actualShoppingSheetsAsync()).Count());
            }

            [TestMethod]
            public async Task NullStudentAwardsHasNoEffectTest()
            {
                studentAwardRepository.awardPeriodData = new List<TestStudentAwardRepository.TestAwardPeriodRecord>();

                Assert.AreEqual((await expectedShoppingSheets()).Count(), (await actualShoppingSheetsAsync()).Count());
            }

            [TestMethod]
            public async Task NullFafsasAndProfilesDoNotGetSentToShoppingSheetDomainServiceTest()
            {
                fafsaRepositoryMock.Setup(r => r.GetFafsasAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                    .Returns<IEnumerable<string>, IEnumerable<string>>((ids, awardYears) => Task.FromResult((IEnumerable<Domain.FinancialAid.Entities.Fafsa>)null));

                profileApplicationRepositoryMock.Setup(r => r.GetProfileApplicationsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>
                    ((id, studentAwardYears) => Task.FromResult((IEnumerable<Domain.FinancialAid.Entities.ProfileApplication>)null));

                Assert.IsTrue((await actualShoppingSheetsAsync()).All(s => !s.FamilyContribution.HasValue));
            }

            [TestMethod]
            public async Task CatchExceptionThrownFromBuildingShoppingSheet_LogMessageTest()
            {
                studentBudgetComponentRepositoryMock.Setup(r => r.GetStudentBudgetComponentsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>((id, studentAwardYears) => Task.FromResult((IEnumerable<Domain.FinancialAid.Entities.StudentBudgetComponent> )null));
                studentAwardRepositoryMock.Setup(r => r.GetAllStudentAwardsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.Award>>(), It.IsAny<IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>()))
                    .Returns<string, IEnumerable<Domain.FinancialAid.Entities.StudentAwardYear>, IEnumerable<Domain.FinancialAid.Entities.Award>, IEnumerable<Domain.FinancialAid.Entities.AwardStatus>>
                        ((id, studentAwardYears, awards, awardStatuses) => Task.FromResult((IEnumerable<Domain.FinancialAid.Entities.StudentAward>) null));

                Assert.AreEqual(0, (await actualShoppingSheetsAsync()).Count());

                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), "Unable to create shopping sheet for studentId {0} and awardYear {1}", It.IsAny<object[]>()));
            }

        }
    }
}
