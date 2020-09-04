//Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CorrespondenceRequestsServiceTests
    {
        public class StudentUserFactory : ICurrentUserFactory
        {
            public string PersonId { get; set; }
            public ProxySubjectClaims ProxySubjectClaims { get; set; }
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = PersonId ?? "0003914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "FINANCIAL AID COUNSELOR" },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = ProxySubjectClaims
                    });
                }
            }
        }
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IRoleRepository> roleRepositoryMock;
        public Mock<ILogger> loggerMock;
        public ICurrentUserFactory currentUserFactory;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        private string personId;

        private TestCorrespondenceRequestsRepository testCorrespondenceRequestsRepository;
        //private TestFinancialAidReferenceDataRepository testReferenceDataRepository;

        private IEnumerable<Domain.Base.Entities.CorrespondenceRequest> inputCorrespondenceRequestEntities;
        private Domain.Base.Entities.CorrespondenceRequest responseCorrespondenceRequestEntity;
        private AutoMapperAdapter<Domain.Base.Entities.CorrespondenceRequest, Dtos.Base.CorrespondenceRequest> CorrespondenceRequestDtoAdapter;

        private List<Dtos.Base.CorrespondenceRequest> expectedCorrespondenceRequests;
        private IEnumerable<Dtos.Base.CorrespondenceRequest> actualCorrespondenceRequests;

        private Mock<ICorrespondenceRequestsRepository> CorrespondenceRequestsRepositoryMock;
        // private Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;

        private CorrespondenceRequestsService CorrespondenceRequestsService;

        [TestInitialize]
        public async void BaseInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            roleRepositoryMock = new Mock<IRoleRepository>();
            currentUserFactory = new StudentUserFactory();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            personId = currentUserFactory.CurrentUser.PersonId;

            testCorrespondenceRequestsRepository = new TestCorrespondenceRequestsRepository();
            inputCorrespondenceRequestEntities = await testCorrespondenceRequestsRepository.GetCorrespondenceRequestsAsync(personId);
            responseCorrespondenceRequestEntity = inputCorrespondenceRequestEntities.First();

            CorrespondenceRequestsRepositoryMock = new Mock<ICorrespondenceRequestsRepository>();
            CorrespondenceRequestsRepositoryMock.Setup(l => l.GetCorrespondenceRequestsAsync(personId)).ReturnsAsync(inputCorrespondenceRequestEntities);
            CorrespondenceRequestsRepositoryMock.Setup(cc => cc.AttachmentNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(responseCorrespondenceRequestEntity);

            CorrespondenceRequestDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.CorrespondenceRequest, Dtos.Base.CorrespondenceRequest>(adapterRegistryMock.Object, loggerMock.Object);
            expectedCorrespondenceRequests = new List<Dtos.Base.CorrespondenceRequest>();
            foreach (var letterEntity in inputCorrespondenceRequestEntities)
            {
                expectedCorrespondenceRequests.Add(CorrespondenceRequestDtoAdapter.MapToType(letterEntity));
            }

            adapterRegistryMock.Setup<ITypeAdapter<Domain.Base.Entities.CorrespondenceRequest, Dtos.Base.CorrespondenceRequest>>(
                a => a.GetAdapter<Domain.Base.Entities.CorrespondenceRequest, Dtos.Base.CorrespondenceRequest>()
                ).Returns(CorrespondenceRequestDtoAdapter);

            CorrespondenceRequestsService = new CorrespondenceRequestsService(adapterRegistryMock.Object,
                CorrespondenceRequestsRepositoryMock.Object,
                baseConfigurationRepository,
                null,
                currentUserFactory,
                roleRepositoryMock.Object,
                loggerMock.Object);

            actualCorrespondenceRequests = await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(personId);
        }

        [TestCleanup]
        public void BaseCleanup()
        {
            adapterRegistryMock = null;
            loggerMock = null;
            roleRepositoryMock = null;
            currentUserFactory = null;
            baseConfigurationRepositoryMock = null;
            baseConfigurationRepository = null;

            personId = null;
            testCorrespondenceRequestsRepository = null;
            inputCorrespondenceRequestEntities = null;
            CorrespondenceRequestDtoAdapter = null;
            expectedCorrespondenceRequests = null;
            actualCorrespondenceRequests = null;
            CorrespondenceRequestsRepositoryMock = null;
            CorrespondenceRequestsService = null;
        }

        private void BuildService(ICurrentUserFactory userFactory = null)
        {
            if (userFactory == null)
            {
                userFactory = currentUserFactory;
            }
            CorrespondenceRequestsService = new CorrespondenceRequestsService(adapterRegistryMock.Object,
                                CorrespondenceRequestsRepositoryMock.Object,
                                baseConfigurationRepository,
                                null,
                                userFactory,
                                roleRepositoryMock.Object,
                                loggerMock.Object);
        }

        #region GetCorrespondenceRequestsTests
        [TestClass]
        public class GetCorrespondenceRequestsTests : CorrespondenceRequestsServiceTests
        {

            [TestInitialize]
            public void Initialize()
            {
                base.BaseInitialize();
            }

            [TestCleanup]
            public void Cleanup()
            {
                base.BaseCleanup();
            }

            [TestMethod]
            public void ObjectsHaveValueTest()
            {
                Assert.IsNotNull(expectedCorrespondenceRequests);
                Assert.IsNotNull(actualCorrespondenceRequests);
            }

            [TestMethod]
            public void NumCorrespondenceRequestsAreEqualTest()
            {
                Assert.IsTrue(expectedCorrespondenceRequests.Count() > 0);
                Assert.IsTrue(actualCorrespondenceRequests.Count() > 0);
                Assert.AreEqual(expectedCorrespondenceRequests.Count(), actualCorrespondenceRequests.Count());
            }

            [TestMethod]
            public void CorrespondenceRequestsProperties_EqualsTest()
            {
                var CorrespondenceRequestProperties = typeof(Dtos.Base.CorrespondenceRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Assert.IsTrue(CorrespondenceRequestProperties.Length > 0);
                foreach (var expectedLetter in expectedCorrespondenceRequests)
                {
                    var actualLetter = expectedCorrespondenceRequests.First(a => a.Code == expectedLetter.Code);
                    foreach (var property in CorrespondenceRequestProperties)
                    {
                        var expectedValue = property.GetValue(expectedLetter, null);
                        var actualValue = property.GetValue(actualLetter, null);
                        Assert.AreEqual(expectedValue, actualValue);
                    }
                }
            }

            [TestMethod]
            public async Task EmptyCorrespondenceRequestsListTest()
            {
                CorrespondenceRequestsRepositoryMock.Setup(
                    l => l.GetCorrespondenceRequestsAsync(personId)).ReturnsAsync(new List<Domain.Base.Entities.CorrespondenceRequest>());

                actualCorrespondenceRequests = await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(personId);

                Assert.IsNotNull(actualCorrespondenceRequests);
                Assert.IsTrue(actualCorrespondenceRequests.Count() == 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonIdRequiredTest()
            {
                await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(null);
            }

            /// <summary>
            /// User is not self 
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsNotSelf_CannotAccessDataTest()
            {
                var userFactory = new StudentUserFactory()
                {
                    PersonId = "0000001",
                };

                BuildService(userFactory);

                await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(personId);

            }


            /// <summary>
            /// User is proxy but does not have proper proxy permission
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CurrentUserIsProxyWithoutCordPermission_CannotAccessDataTest()
            {
                var userFactory = new StudentUserFactory()
                {
                    PersonId = "0000001",
                    ProxySubjectClaims = new ProxySubjectClaims
                    {
                        PersonId = personId
                    }
                };

                BuildService(userFactory);


                await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(personId);

            }

            /// <summary>
            /// User is proxy with proper proxy permission and retrieves the data
            /// </summary>
            /// <returns></returns>
            [TestMethod]
            public async Task CurrentUserIsProxyWithCordPermission_RetrievesData()
            {
                var userFactory = new StudentUserFactory()
                {
                    PersonId = "0000001",
                    ProxySubjectClaims = new ProxySubjectClaims
                    {
                        PersonId = personId,
                        Permissions = new List<string> { Domain.Base.Entities.ProxyWorkflowConstants.CoreRequiredDocuments.Value }
                    }
                };

                BuildService(userFactory);

                var actualCorrespondenceRequests = await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(personId);
                Assert.IsNotNull(actualCorrespondenceRequests);
                Assert.AreEqual(8, actualCorrespondenceRequests.Count());

            }
        }
        #endregion

        [TestClass]
        public class AttachmentNotificationAsyncTests : CorrespondenceRequestsServiceTests
        {

            [TestInitialize]
            public void Initialize()
            {
                base.BaseInitialize();
            }

            [TestCleanup]
            public void Cleanup()
            {
                base.BaseCleanup();
            }

            [TestMethod]
            public async Task AttachmentNotificationAsync_Success()
            {
                var inputNotification = new CorrespondenceAttachmentNotification() { PersonId = personId, CommunicationCode = "Code", AssignDate = DateTime.Today.AddDays(-2) };
                var actual = await CorrespondenceRequestsService.AttachmentNotificationAsync(inputNotification);
                Assert.AreEqual(responseCorrespondenceRequestEntity.PersonId, actual.PersonId);
                Assert.AreEqual(responseCorrespondenceRequestEntity.Code, actual.Code);
                Assert.AreEqual(responseCorrespondenceRequestEntity.AssignDate, actual.AssignDate);
                Assert.AreEqual(responseCorrespondenceRequestEntity.Instance, actual.Instance);
                Assert.AreEqual(responseCorrespondenceRequestEntity.Status.ToString(), actual.Status.ToString());
                Assert.AreEqual(responseCorrespondenceRequestEntity.StatusDescription, actual.StatusDescription);
                Assert.AreEqual(responseCorrespondenceRequestEntity.StatusDate, actual.StatusDate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AttachmentNotificationAsync_NullInput()
            {
                await CorrespondenceRequestsService.AttachmentNotificationAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentNotificationAsync_NullPersonId()
            {
                var inputNotification = new CorrespondenceAttachmentNotification() { CommunicationCode = "CommCode", AssignDate = DateTime.Today.AddDays(-2) };
                await CorrespondenceRequestsService.AttachmentNotificationAsync(inputNotification);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentNotificationAsync_EmptyPersonId()
            {
                var inputNotification = new CorrespondenceAttachmentNotification() { PersonId = string.Empty, CommunicationCode = "CommCode", AssignDate = DateTime.Today.AddDays(-2) };
                await CorrespondenceRequestsService.AttachmentNotificationAsync(inputNotification);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentNotificationAsync_NullCommunicationCode()
            {
                var inputNotification = new CorrespondenceAttachmentNotification() { PersonId = "PersonId", AssignDate = DateTime.Today.AddDays(-2) };
                await CorrespondenceRequestsService.AttachmentNotificationAsync(inputNotification);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AttachmentNotificationAsync_EmptyCommunicationCode()
            {
                var inputNotification = new CorrespondenceAttachmentNotification() { PersonId = "PersonId", CommunicationCode = string.Empty, AssignDate = DateTime.Today.AddDays(-2) };
                await CorrespondenceRequestsService.AttachmentNotificationAsync(inputNotification);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AttachmentService_PutAttachmentAsyncNoPermissions()
            {
                var inputNotification = new CorrespondenceAttachmentNotification() { PersonId = "PersonId", CommunicationCode = "Code", AssignDate = DateTime.Today.AddDays(-2) };
                await CorrespondenceRequestsService.AttachmentNotificationAsync(inputNotification);
            }
        }
    }
}
