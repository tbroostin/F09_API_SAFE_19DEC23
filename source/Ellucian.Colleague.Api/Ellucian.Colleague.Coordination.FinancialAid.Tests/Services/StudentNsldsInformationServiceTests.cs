//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Repositories;
using Moq;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentNsldsInformationServiceTests
    {
        [TestClass]
        public class GetStudentNsldsInformationTests : FinancialAidServiceTestsSetup
        {
            private StudentNsldsInformation expectedNsldsInformation;
            private StudentNsldsInformation actualNsldsInformation;

            private Domain.FinancialAid.Entities.StudentNsldsInformation nsldsInformationEntity;
            string studentId;

            private AutoMapperAdapter<Domain.FinancialAid.Entities.StudentNsldsInformation, StudentNsldsInformation> entityToDtoAdapter
            {
                get { return new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentNsldsInformation, StudentNsldsInformation>(adapterRegistryMock.Object, loggerMock.Object); }
            }

            private StudentNsldsInformationService studentNsldsInformationService;
            private Mock<IStudentNsldsInformationRepository> nsldsInformationRepositoryMock;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            

            [TestInitialize]
            public void Initialize()
            {
                BaseInitialize();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                studentId = "0003914";
                nsldsInformationEntity = new Domain.FinancialAid.Entities.StudentNsldsInformation(studentId, 43526);
                expectedNsldsInformation = entityToDtoAdapter.MapToType(nsldsInformationEntity);

                nsldsInformationRepositoryMock = new Mock<IStudentNsldsInformationRepository>();

                adapterRegistryMock.Setup(a => a.GetAdapter<Domain.FinancialAid.Entities.StudentNsldsInformation, StudentNsldsInformation>())
                    .Returns(new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentNsldsInformation, StudentNsldsInformation>(adapterRegistryMock.Object, loggerMock.Object));

                nsldsInformationRepositoryMock.Setup(r => r.GetStudentNsldsInformationAsync(It.IsAny<string>()))
                    .ReturnsAsync(nsldsInformationEntity);

                BuildStudentNsldsInformationService();
            }

            private void BuildStudentNsldsInformationService()
            {
                studentNsldsInformationService = new StudentNsldsInformationService(adapterRegistryMock.Object,
                    nsldsInformationRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                nsldsInformationRepositoryMock = null;
                studentNsldsInformationService = null;

                nsldsInformationEntity = null;
                expectedNsldsInformation = null;
                actualNsldsInformation = null;
            }

            [TestMethod]
            public async Task StudentNsldsInformationReturned_IsNotNullTest()
            {
                Assert.IsNotNull(await studentNsldsInformationService.GetStudentNsldsInformationAsync(studentId));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ArgumentNullExceptionThrownTest()
            {
                await studentNsldsInformationService.GetStudentNsldsInformationAsync(null);
            }

            /// <summary>
            /// User is neither self, no proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UserHasNoAccessToStudentNsldsData_PermissionsExceptionThrownTest()
            {
                await studentNsldsInformationService.GetStudentNsldsInformationAsync("foo");
            }

            /// <summary>
            /// User is counselor
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CounselorCanViewStudentNsldsInformationTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
                counselorRole.AddPermission(new Permission(StudentPermissionCodes.ViewFinancialAidInformation));
                roleRepositoryMock.Setup(r => r.Roles).Returns(new List<Role>() { counselorRole });
                BuildStudentNsldsInformationService();

                bool exceptionThrown = false;
                try
                {
                    actualNsldsInformation = await studentNsldsInformationService.GetStudentNsldsInformationAsync(studentId);
                }
                catch
                {
                    exceptionThrown = true;
                }
                Assert.IsFalse(exceptionThrown);
                Assert.IsNotNull(actualNsldsInformation);
            }

            /// <summary>
            /// User is counselor with no permissions
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CounselorCannotViewStudentNsldsInformationTest()
            {
                currentUserFactory = new CurrentUserSetup.CounselorUserFactory();
               
                BuildStudentNsldsInformationService();

                await studentNsldsInformationService.GetStudentNsldsInformationAsync(studentId);
            }

            /// <summary>
            /// User is proxy
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task ProxyCanViewStudentNsldsInformationTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithProxy();
                BuildStudentNsldsInformationService();

                bool exceptionThrown = false;
                try
                {
                    actualNsldsInformation = await studentNsldsInformationService.GetStudentNsldsInformationAsync(currentUserFactory.CurrentUser.ProxySubjects.First().PersonId);
                }
                catch
                {
                    exceptionThrown = true;
                }
                Assert.IsFalse(exceptionThrown);
                Assert.IsNotNull(actualNsldsInformation);
            }

            /// <summary>
            /// User is proxy for a different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ProxyForDifferentPersonCannotViewStudentNsldsInformationTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildStudentNsldsInformationService();

                await studentNsldsInformationService.GetStudentNsldsInformationAsync(studentId);
            }

            [TestMethod]
            public async Task ActualNsldsInformation_EqualsExpectedTest()
            {
                actualNsldsInformation = await studentNsldsInformationService.GetStudentNsldsInformationAsync(studentId);
                Assert.AreEqual(expectedNsldsInformation.StudentId, actualNsldsInformation.StudentId);
                Assert.AreEqual(expectedNsldsInformation.PellLifetimeEligibilityUsedPercentage, actualNsldsInformation.PellLifetimeEligibilityUsedPercentage);
            }

            [TestMethod]
            public async Task NullStudentNsldsInformationReceived_ExceptionHandledTest()
            {
                nsldsInformationRepositoryMock.Setup(r => r.GetStudentNsldsInformationAsync(It.IsAny<string>()))
                    .ReturnsAsync(() => null);

                BuildStudentNsldsInformationService();
                Assert.IsNull(await studentNsldsInformationService.GetStudentNsldsInformationAsync(studentId));
            }
        }
    }
}
