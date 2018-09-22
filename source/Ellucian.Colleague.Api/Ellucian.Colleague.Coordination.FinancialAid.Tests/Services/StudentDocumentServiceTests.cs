//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.FinancialAid.Tests.Services
{
    [TestClass]
    public class StudentDocumentServiceTests
    {
        #region GetStudentDocumentsTests
        [TestClass]
        public class GetStudentDocumentTests : FinancialAidServiceTestsSetup
        {
            private string studentId;

            private TestStudentDocumentRepository testStudentDocumentRepository;
            //private TestFinancialAidReferenceDataRepository testReferenceDataRepository;

            private IEnumerable<Domain.FinancialAid.Entities.StudentDocument> inputStudentDocumentEntities;
            private AutoMapperAdapter<Domain.FinancialAid.Entities.StudentDocument, Dtos.FinancialAid.StudentDocument> StudentDocumentDtoAdapter;

            private List<Dtos.FinancialAid.StudentDocument> expectedStudentDocuments;
            private IEnumerable<Dtos.FinancialAid.StudentDocument> actualStudentDocuments;

            private Mock<IStudentDocumentRepository> StudentDocumentRepositoryMock;
            // private Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;

            private StudentDocumentService StudentDocumentService;

            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {
                BaseInitialize();

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentId = currentUserFactory.CurrentUser.PersonId;

                testStudentDocumentRepository = new TestStudentDocumentRepository();
                inputStudentDocumentEntities = await testStudentDocumentRepository.GetDocumentsAsync(studentId);

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                StudentDocumentRepositoryMock = new Mock<IStudentDocumentRepository>();
                StudentDocumentRepositoryMock.Setup(l => l.GetDocumentsAsync(studentId)).ReturnsAsync(inputStudentDocumentEntities);

                StudentDocumentDtoAdapter = new AutoMapperAdapter<Domain.FinancialAid.Entities.StudentDocument, Dtos.FinancialAid.StudentDocument>(adapterRegistryMock.Object, loggerMock.Object);
                expectedStudentDocuments = new List<Dtos.FinancialAid.StudentDocument>();
                foreach (var letterEntity in inputStudentDocumentEntities)
                {
                    expectedStudentDocuments.Add(StudentDocumentDtoAdapter.MapToType(letterEntity));
                }

                adapterRegistryMock.Setup<ITypeAdapter<Domain.FinancialAid.Entities.StudentDocument, Dtos.FinancialAid.StudentDocument>>(
                    a => a.GetAdapter<Domain.FinancialAid.Entities.StudentDocument, Dtos.FinancialAid.StudentDocument>()
                    ).Returns(StudentDocumentDtoAdapter);

                StudentDocumentService = new StudentDocumentService(adapterRegistryMock.Object,
                    StudentDocumentRepositoryMock.Object,
                    baseConfigurationRepository,
                    currentUserFactory,
                    roleRepositoryMock.Object,
                    loggerMock.Object);

                actualStudentDocuments = await StudentDocumentService.GetStudentDocumentsAsync(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                BaseCleanup();

                studentId = null;
                testStudentDocumentRepository = null;
                inputStudentDocumentEntities = null;
                StudentDocumentDtoAdapter = null;
                expectedStudentDocuments = null;
                actualStudentDocuments = null;
                StudentDocumentRepositoryMock = null;
                StudentDocumentService = null;
            }

            private void BuildService()
            {
                StudentDocumentService = new StudentDocumentService(adapterRegistryMock.Object,
                                    StudentDocumentRepositoryMock.Object,
                                    baseConfigurationRepository,
                                    currentUserFactory,
                                    roleRepositoryMock.Object,
                                    loggerMock.Object);
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedStudentDocuments);
                Assert.IsNotNull(actualStudentDocuments);
            }

            [TestMethod]
            public void NumStudentDocumentsAreEqualTest()
            {
                Assert.IsTrue(expectedStudentDocuments.Count() > 0);
                Assert.IsTrue(actualStudentDocuments.Count() > 0);
                Assert.AreEqual(expectedStudentDocuments.Count(), actualStudentDocuments.Count());
            }

            [TestMethod]
            public void StudentDocumentProperties_EqualsTest()
            {
                var StudentDocumentProperties = typeof(Dtos.FinancialAid.StudentDocument).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.IsTrue(StudentDocumentProperties.Length > 0);
                foreach (var expectedLetter in expectedStudentDocuments)
                {
                    var actualLetter = expectedStudentDocuments.First(a => a.Code == expectedLetter.Code);
                    foreach (var property in StudentDocumentProperties)
                    {
                        var expectedValue = property.GetValue(expectedLetter, null);
                        var actualValue = property.GetValue(actualLetter, null);
                        Assert.AreEqual(expectedValue, actualValue);
                    }
                }
            }

            [TestMethod]
            public async Task EmptyStudentDocumentListTest()
            {
                StudentDocumentRepositoryMock.Setup(
                    l => l.GetDocumentsAsync(studentId)).ReturnsAsync(new List<Domain.FinancialAid.Entities.StudentDocument>());

                actualStudentDocuments = await StudentDocumentService.GetStudentDocumentsAsync(studentId);

                Assert.IsNotNull(actualStudentDocuments);
                Assert.IsTrue(actualStudentDocuments.Count() == 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentIdRequiredTest()
            {
                await StudentDocumentService.GetStudentDocumentsAsync(null);
            }

            /// <summary>
            /// User is neither self, nor proxy, nor admin
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserNotSelfNoPermissions_CannotAccessDataTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));
                Assert.IsFalse(currentUserFactory.CurrentUser.ProxySubjects.Any());

                studentId = "foobar";
                await StudentDocumentService.GetStudentDocumentsAsync(studentId);
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

                BuildService();

                Assert.IsNotNull(await StudentDocumentService.GetStudentDocumentsAsync(studentId));

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

                await StudentDocumentService.GetStudentDocumentsAsync(studentId);

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

                Assert.IsNotNull(await StudentDocumentService.GetStudentDocumentsAsync(studentId));

            }

            /// <summary>
            /// User is proxy for different person
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyForDifferentPerson_CannotAccessDataTest()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactoryWithDifferentProxy();

                BuildService();

                await StudentDocumentService.GetStudentDocumentsAsync(studentId);

            }

            [TestMethod]
            public async Task PermissionExceptionLogsErrorTest()
            {
                Assert.IsFalse(currentUserFactory.CurrentUser.IsInRole("FINANCIAL AID COUNSELOR"));

                var currentUserId = studentId;
                studentId = "foobar";

                bool permissionExceptionCaught = false;
                var message = string.Format("{0} does not have permission to access document information for {1}", currentUserId, studentId);
                try
                {
                    await StudentDocumentService.GetStudentDocumentsAsync(studentId);
                }
                catch (PermissionsException)
                {
                    permissionExceptionCaught = true;
                }

                Assert.IsTrue(permissionExceptionCaught);

                loggerMock.Verify(l => l.Error(message));
            }
        }
        #endregion        
    }
}

