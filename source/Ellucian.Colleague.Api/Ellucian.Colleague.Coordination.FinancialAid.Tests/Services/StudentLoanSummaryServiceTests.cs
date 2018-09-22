//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.FinancialAid.Adapters;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{

    [TestClass]
    public class StudentLoanSummaryServiceTests
    {

        [TestClass]
        public class GetStudentLoanSummaryTests : FinancialAidServiceTestsSetup
        {
            private string studentId;

            private TestStudentLoanSummaryRepository testStudentLoanSummaryRepository;
            private Domain.FinancialAid.Entities.StudentLoanSummary inputStudentLoanSummaryEntity;
            private StudentLoanSummaryEntityToDtoAdapter studentLoanSummaryDtoAdapter;

            private Dtos.FinancialAid.StudentLoanSummary expectedStudentLoanSummary;
            private Dtos.FinancialAid.StudentLoanSummary actualStudentLoanSummary;

            private Mock<IStudentLoanSummaryRepository> studentLoanSummaryRepositoryMock;
            //private Mock<IIpedsInstitutionRepository> ipedsInstitutionRepositoryMock;

            private StudentLoanSummaryService StudentLoanSummaryService;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                studentId = currentUserFactory.CurrentUser.PersonId;

                testStudentLoanSummaryRepository = new TestStudentLoanSummaryRepository();

                inputStudentLoanSummaryEntity = await testStudentLoanSummaryRepository.GetStudentLoanSummaryAsync(studentId);

                studentLoanSummaryRepositoryMock = new Mock<IStudentLoanSummaryRepository>();
                studentLoanSummaryRepositoryMock.Setup<Task<Domain.FinancialAid.Entities.StudentLoanSummary>>(l => l.GetStudentLoanSummaryAsync(studentId)).ReturnsAsync(inputStudentLoanSummaryEntity);

                studentLoanSummaryDtoAdapter = new StudentLoanSummaryEntityToDtoAdapter(adapterRegistryMock.Object, loggerMock.Object);
                expectedStudentLoanSummary = studentLoanSummaryDtoAdapter.MapToType(inputStudentLoanSummaryEntity);

                adapterRegistryMock.Setup<ITypeAdapter<Domain.FinancialAid.Entities.StudentLoanSummary, Dtos.FinancialAid.StudentLoanSummary>>(
                    a => a.GetAdapter<Domain.FinancialAid.Entities.StudentLoanSummary, Dtos.FinancialAid.StudentLoanSummary>()
                    ).Returns(studentLoanSummaryDtoAdapter);

                BuildService();

                actualStudentLoanSummary = await StudentLoanSummaryService.GetStudentLoanSummaryAsync(studentId);
            }

            private void BuildService()
            {
                StudentLoanSummaryService = new StudentLoanSummaryService(adapterRegistryMock.Object,
                                    studentLoanSummaryRepositoryMock.Object,
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
                testStudentLoanSummaryRepository = null;
                inputStudentLoanSummaryEntity = null;
                studentLoanSummaryDtoAdapter = null;
                expectedStudentLoanSummary = null;
                actualStudentLoanSummary = null;
                studentLoanSummaryRepositoryMock = null;
                StudentLoanSummaryService = null;
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedStudentLoanSummary);
                Assert.IsNotNull(actualStudentLoanSummary);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await StudentLoanSummaryService.GetStudentLoanSummaryAsync("");
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserNotSelfNoPermissions_CannotAccessDataTest()
            {
                studentId = "foobar";
                await StudentLoanSummaryService.GetStudentLoanSummaryAsync(studentId);
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserHasPermissions_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });

                StudentLoanSummaryService = new StudentLoanSummaryService(adapterRegistryMock.Object,
                    studentLoanSummaryRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                Assert.IsNotNull(await StudentLoanSummaryService.GetStudentLoanSummaryAsync(studentId));
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsCounselorHasNoPermissions_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                BuildService();

                await StudentLoanSummaryService.GetStudentLoanSummaryAsync(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxy_CanAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildService();

                Assert.IsNotNull(await StudentLoanSummaryService.GetStudentLoanSummaryAsync(studentId));
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();
                BuildService();

                await StudentLoanSummaryService.GetStudentLoanSummaryAsync(studentId);
            }

            [TestMethod]
            public async Task PermissionExceptionLogsErrorTest()
            {
                var currentUserId = studentId;
                studentId = "foobar";

                bool permissionExceptionCaught = false;
                var message = string.Format("{0} does not have permission to access loan summary information for {1}", currentUserId, studentId);
                try
                {
                    await StudentLoanSummaryService.GetStudentLoanSummaryAsync(studentId);
                }
                catch (PermissionsException)
                {
                    permissionExceptionCaught = true;
                }

                Assert.IsTrue(permissionExceptionCaught);

                loggerMock.Verify(l => l.Error(message));
            }

            [TestMethod]
            public void ActualStudentLoanSummary_EqualsExpectedTest()
            {
                Assert.AreEqual(expectedStudentLoanSummary.DirectLoanEntranceInterviewDate, actualStudentLoanSummary.DirectLoanEntranceInterviewDate);
                Assert.AreEqual(expectedStudentLoanSummary.DirectLoanMpnExpirationDate, actualStudentLoanSummary.DirectLoanMpnExpirationDate);
                Assert.AreEqual(expectedStudentLoanSummary.GraduatePlusLoanEntranceInterviewDate, actualStudentLoanSummary.GraduatePlusLoanEntranceInterviewDate);
                Assert.AreEqual(expectedStudentLoanSummary.PlusLoanMpnExpirationDate, actualStudentLoanSummary.PlusLoanMpnExpirationDate);
                Assert.AreEqual(expectedStudentLoanSummary.StudentLoanCombinedTotalAmount, actualStudentLoanSummary.StudentLoanCombinedTotalAmount);
                Assert.AreEqual(expectedStudentLoanSummary.StudentId, actualStudentLoanSummary.StudentId);
            }

        }
    }
}
