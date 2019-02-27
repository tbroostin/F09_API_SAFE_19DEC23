//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Repositories;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class CorrespondenceRequestsServiceTests
    {
        public class StudentUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Matt",
                        PersonId = "0003914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { "FINANCIAL AID COUNSELOR" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }

        public class StudentUserFactoryWithProxy : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "Gregory",
                        PersonId = "0013914",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Student",
                        Roles = new List<string>() { },
                        SessionFixationId = "abc123",
                        ProxySubjectClaims = new ProxySubjectClaims()
                        {
                            PersonId = "0003914"
                        }
                    });
                }
            }
        }

        #region GetCorrespondenceRequestsTests
        [TestClass]
        public class GetCorrespondenceRequestsTests
        {

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
            private AutoMapperAdapter<Domain.Base.Entities.CorrespondenceRequest, Dtos.Base.CorrespondenceRequest> CorrespondenceRequestDtoAdapter;

            private List<Dtos.Base.CorrespondenceRequest> expectedCorrespondenceRequests;
            private IEnumerable<Dtos.Base.CorrespondenceRequest> actualCorrespondenceRequests;

            private Mock<ICorrespondenceRequestsRepository> CorrespondenceRequestsRepositoryMock;
            // private Mock<IFinancialAidReferenceDataRepository> referenceDataRepositoryMock;

            private CorrespondenceRequestsService CorrespondenceRequestsService;

            [TestInitialize]
            public async void Initialize()
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

                CorrespondenceRequestsRepositoryMock = new Mock<ICorrespondenceRequestsRepository>();
                CorrespondenceRequestsRepositoryMock.Setup(l => l.GetCorrespondenceRequestsAsync(personId)).ReturnsAsync(inputCorrespondenceRequestEntities);

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
            public void Cleanup()
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

            private void BuildService()
            {
                CorrespondenceRequestsService = new CorrespondenceRequestsService(adapterRegistryMock.Object,
                                    CorrespondenceRequestsRepositoryMock.Object,
                                    baseConfigurationRepository,
                                    null,
                                    currentUserFactory,
                                    roleRepositoryMock.Object,
                                    loggerMock.Object);
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
                currentUserFactory = new StudentUserFactoryWithProxy();

                BuildService();

                await CorrespondenceRequestsService.GetCorrespondenceRequestsAsync(personId);

            }
        }
        #endregion        
    }
}
