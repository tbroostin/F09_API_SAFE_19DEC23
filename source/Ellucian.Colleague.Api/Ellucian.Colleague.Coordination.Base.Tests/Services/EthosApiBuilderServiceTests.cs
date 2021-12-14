// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Web.Security;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class EthosApiBuilderServiceTests
    {
        // Sets up a Current user that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetEthosApiBuilder : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private IAdapterRegistry _adapterRegistry;
            private Mock<IRoleRepository> _roleRepoMock;
            private IRoleRepository _roleRepo;
            private ICurrentUserFactory _currentUserFactory;
            private IEthosApiBuilderRepository _ethosApiBuilderRepository;
            private Mock<IEthosApiBuilderRepository> _ethosApiBuilderRepositoryMock;
            private Mock<IColleagueTransactionInvoker> _transManagerMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            private ILogger _logger;
            private EthosApiBuilderService _ethosApiBuilderService;

            private EthosApiConfiguration ethosApiConfiguration;
            private EthosExtensibleData ethosExtensibleData;

            private List<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilderCollection;
            private const string ethosApiBuilderGuid = "a830e686-7692-4012-8da5-b1b5d44389b4";
            private GuidLookupResult guidLookupResult;

            private Domain.Entities.Permission permissionViewAnyPerson;
            private Domain.Entities.Permission permissionCreatePerson;
            private Domain.Entities.Permission permissionDeletePerson;
            Dictionary<string, string> ethosApiBuilderGuidDictionary = new Dictionary<string, string>();


            [TestInitialize]
            public void Initialize()
            {
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _adapterRegistry = _adapterRegistryMock.Object;
                _roleRepoMock = new Mock<IRoleRepository>();
                _roleRepo = _roleRepoMock.Object;
                _logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                _ethosApiBuilderRepositoryMock = new Mock<IEthosApiBuilderRepository>();
                _ethosApiBuilderRepository = _ethosApiBuilderRepositoryMock.Object;

                _transManagerMock = new Mock<IColleagueTransactionInvoker>();

                // Set up current user
                _currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                // Mock permissions
                permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.ViewAnyPerson);
                permissionCreatePerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.UpdatePerson);
                permissionDeletePerson = new Ellucian.Colleague.Domain.Entities.Permission(BasePermissionCodes.DeletePersonContact);
                facultyRole.AddPermission(permissionViewAnyPerson);
                facultyRole.AddPermission(permissionCreatePerson);
                facultyRole.AddPermission(permissionDeletePerson);
                _roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });

                ethosApiBuilderCollection = new List<Domain.Base.Entities.EthosApiBuilder>();
                var testConfigurationRepository = new TestConfigurationRepository();

                ethosApiConfiguration = testConfigurationRepository.GetEthosApiConfigurationByResource("person-health", false).GetAwaiter().GetResult();

                ethosExtensibleData = testConfigurationRepository.GetExtendedEthosDataByResource("person-health", "1.0.0", "141", new List<string>() { ethosApiBuilderGuid }, true, false).GetAwaiter().GetResult().FirstOrDefault();
                var ethosExtensibleDataDto = new Web.Http.EthosExtend.EthosExtensibleData()
                {
                    ApiResourceName = ethosExtensibleData.ApiResourceName,
                    ApiVersionNumber = ethosExtensibleData.ApiVersionNumber,
                    ColleagueTimeZone = ethosExtensibleData.ColleagueTimeZone,
                    ResourceId = ethosExtensibleData.ResourceId,
                    ExtendedSchemaType = ethosExtensibleData.ExtendedSchemaType
                };
                guidLookupResult = new GuidLookupResult()
                {
                    Entity = "PERSON",
                    PrimaryKey = "1"
                };

                var ethosApiBuilder = new Domain.Base.Entities.EthosApiBuilder(ethosApiBuilderGuid, "1", "ethosApiBuilder");

                var output = string.Empty;
                if (!ethosApiBuilderGuidDictionary.TryGetValue("1", out output))
                    ethosApiBuilderGuidDictionary.Add("1", ethosExtensibleData.ResourceId);

                ethosApiBuilderCollection.Add(ethosApiBuilder);

                var Limit = ethosApiBuilderCollection.Count();
                var filterDictionary = new Dictionary<string, EthosExtensibleDataFilter>();

                var expectedCollection = new Tuple<IEnumerable<Domain.Base.Entities.EthosApiBuilder>, int>(ethosApiBuilderCollection, Limit);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<EthosApiConfiguration>(), filterDictionary, It.IsAny<bool>())).ReturnsAsync(expectedCollection);

                baseConfigurationRepositoryMock.Setup(x => x.GetEthosApiConfigurationByResource(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ethosApiConfiguration);

                _ethosApiBuilderService = new EthosApiBuilderService(_adapterRegistry, _ethosApiBuilderRepository,
                    baseConfigurationRepository, _currentUserFactory, _roleRepo, _logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                ethosExtensibleData = null;
                _ethosApiBuilderService = null;
                _adapterRegistry = null;
                _adapterRegistryMock = null;
                _logger = null;
                _ethosApiBuilderRepository = null;
                _ethosApiBuilderRepositoryMock = null;
                _roleRepo = null;
                _roleRepoMock = null;
                _transManagerMock = null;

            }

            #region GetEthosApiBuilderAsync

            [TestMethod]
            public async Task EthosApiBuilderService_GetAllEthosApiBuilder()
            {
                var filterDictionary = new Dictionary<string, EthosExtensibleDataFilter>();
                var filterDictionaryDto = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();
                Tuple<IEnumerable<Domain.Base.Entities.EthosApiBuilder>, int> ethosApiBuilder = new Tuple<IEnumerable<Domain.Base.Entities.EthosApiBuilder>, int>(ethosApiBuilderCollection.Where(x => x.Guid == ethosApiBuilderGuid), 1);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderAsync(It.IsAny<int>(), It.IsAny<int>(), ethosApiConfiguration, filterDictionary, It.IsAny<bool>())).ReturnsAsync(ethosApiBuilder);

                var actualEthosApiBuilder = (await _ethosApiBuilderService.GetEthosApiBuilderAsync(0, 100, "person=health", filterDictionaryDto, false));

                var expectedEthosApiBuilder = (ethosApiBuilderCollection.Where(x => x.Guid == ethosApiBuilderGuid)).ToList();

                Assert.IsTrue(actualEthosApiBuilder is Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>);

                var actual = actualEthosApiBuilder.Item1.ElementAtOrDefault(0);
                var expected = expectedEthosApiBuilder.ElementAtOrDefault(0);

                Assert.AreEqual(expected.Guid, actual.Id, "Id");
            }


            #endregion GetEthosApiBuilderAsync

            #region GetEthosApiBuilderById

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EthosApiBuilderService_GetEthosApiBuilderById_ArgumentNullException()
            {
                await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync("", "person-health");
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EthosApiBuilderService_GetEthosApiBuilderById_InvalidID()
            {
                IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosApiBuilderCollection.Where(x => x.Guid == ethosApiBuilderGuid);
                var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Guid, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilder);

                try
                {
                    var actual = await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync("invalid", "peson-health");
                }
                catch (IntegrationApiException ex)
                {
                    Assert.AreEqual("GUID.Not.Found", ex.Errors.FirstOrDefault().Code);
                    Assert.AreEqual("Invalid GUID for person-health: 'invalid'", ex.Errors.FirstOrDefault().Message);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task EthosApiBuilderService_GetEthosApiBuilderById()
            {
                IEnumerable<Domain.Base.Entities.EthosApiBuilder> ethosApiBuilders = ethosApiBuilderCollection.Where(x => x.Guid == ethosApiBuilderGuid);
                var ethosApiBuilder = ethosApiBuilders.ElementAtOrDefault(0);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Guid, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilder);

                var actual = await _ethosApiBuilderService.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Guid, "peson-health");

                var expected = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilder.Guid);

                Assert.IsTrue(actual is Dtos.EthosApiBuilder);

                Assert.AreEqual(expected.Guid, actual.Id, "Id");
            }
            #endregion GetEthosApiBuilderById

            #region PostEthosApiBuilder

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EthosApiBuilderService_PostEthosApiBuilder_ArgumentNullException()
            {
                await _ethosApiBuilderService.PostEthosApiBuilderAsync(null, ethosApiConfiguration.ResourceName);
            }


            [TestMethod]
            public async Task EthosApiBuilderService_PostEthosApiBuilder()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder()
                {
                    Id = ethosApiBuilderEntity.Guid,
                };

                _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateEthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>(), ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Id, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);

                var actual = await _ethosApiBuilderService.PostEthosApiBuilderAsync(ethosApiBuilder, ethosApiConfiguration.ResourceName);

                Assert.AreEqual(ethosApiBuilder.Id, actual.Id, "Id");
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EthosApiBuilderService_PostEthosApiBuilder_NullId()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder();

                await _ethosApiBuilderService.PostEthosApiBuilderAsync(ethosApiBuilder, ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EthosApiBuilderService_PostEthosApiBuilder_EthosApiBuilderNull()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder()
                {
                    Id = ethosApiBuilderEntity.Guid,
                };

                _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateEthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>(), ethosApiConfiguration)).Throws(new IntegrationApiException());
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Id, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);

                await _ethosApiBuilderService.PostEthosApiBuilderAsync(ethosApiBuilder, ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EthosApiBuilderService_PostEthosApiBuilder_EthosApiBuilderEmpty()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder()
                {
                    Id = ethosApiBuilderEntity.Guid,
                };

                _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateEthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>(), ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Id, ethosApiConfiguration)).Throws(new IntegrationApiException());

                await _ethosApiBuilderService.PostEthosApiBuilderAsync(ethosApiBuilder, ethosApiConfiguration.ResourceName);
            }

            #endregion PostEthosApiBuilder

            #region PutEthosApiBuilder

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EthosApiBuilderService_PutEthosApiBuilder_ArgumentNullException()
            {
                await _ethosApiBuilderService.PutEthosApiBuilderAsync(ethosApiBuilderGuid, null, ethosApiConfiguration.ResourceName);
            }


            [TestMethod]
            public async Task EthosApiBuilderService_PutEthosApiBuilder()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder()
                {
                    Id = ethosApiBuilderEntity.Guid,
                };

                _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateEthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>(), ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Id, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetRecordInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookupResult);

                var actual = await _ethosApiBuilderService.PutEthosApiBuilderAsync(ethosApiBuilderEntity.Guid, ethosApiBuilder, ethosApiConfiguration.ResourceName);

                Assert.AreEqual(ethosApiBuilder.Id, actual.Id, "Id");
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EthosApiBuilderService_PutEthosApiBuilder_NullId()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder();

                await _ethosApiBuilderService.PutEthosApiBuilderAsync(ethosApiBuilderEntity.Guid, ethosApiBuilder, ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EthosApiBuilderService_PutEthosApiBuilder_EthosApiBuilderNull()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder()
                {
                    Id = ethosApiBuilderEntity.Guid,
                };

                _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateEthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>(), ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Id, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetRecordInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);

                await _ethosApiBuilderService.PutEthosApiBuilderAsync(ethosApiBuilderEntity.Guid, ethosApiBuilder, ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task EthosApiBuilderService_PutEthosApiBuilder_EthosApiBuilderGuidWrongType()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder()
                {
                    Id = ethosApiBuilderEntity.Guid,
                };

                _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateEthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>(), ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Id, ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetRecordInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { Entity = "STUDENTS", PrimaryKey = "1" });

                await _ethosApiBuilderService.PutEthosApiBuilderAsync(ethosApiBuilderEntity.Guid, ethosApiBuilder, ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task EthosApiBuilderService_PutEthosApiBuilder_EthosApiBuilderEmpty()
            {
                var ethosApiBuilderEntity = ethosApiBuilderCollection.FirstOrDefault(x => x.Guid == ethosApiBuilderGuid);
                Dtos.EthosApiBuilder ethosApiBuilder = new Dtos.EthosApiBuilder()
                {
                    Id = ethosApiBuilderEntity.Guid,
                };

                _ethosApiBuilderRepositoryMock.Setup(x => x.UpdateEthosApiBuilderAsync(It.IsAny<Domain.Base.Entities.EthosApiBuilder>(), ethosApiConfiguration)).ReturnsAsync(ethosApiBuilderEntity);
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetEthosApiBuilderByIdAsync(ethosApiBuilder.Id, ethosApiConfiguration)).Throws(new IntegrationApiException());
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetRecordInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookupResult);

                await _ethosApiBuilderService.PutEthosApiBuilderAsync(ethosApiBuilderEntity.Guid, ethosApiBuilder, ethosApiConfiguration.ResourceName);
            }

            #endregion PutEthosApiBuilder

            #region DeleteEthosApiBuilder


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EthosApiBuilderService_DeleteEthosApiBuilder_NullId()
            {
                await _ethosApiBuilderService.DeleteEthosApiBuilderAsync(null, ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task EthosApiBuilderService_DeleteEthosApiBuilder_EmptyId()
            {
                await _ethosApiBuilderService.DeleteEthosApiBuilderAsync("", ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task EthosApiBuilderService_DeleteEthosApiBuilder_InvalidId()
            {
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetRecordInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                await _ethosApiBuilderService.DeleteEthosApiBuilderAsync(ethosApiBuilderGuid, ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task EthosApiBuilderService_DeleteEthosApiBuilder_WrongType()
            {
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetRecordInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(new GuidLookupResult() { Entity = "STUDENTS", PrimaryKey = "1" });
                await _ethosApiBuilderService.DeleteEthosApiBuilderAsync(ethosApiBuilderGuid, ethosApiConfiguration.ResourceName);
            }

            [TestMethod]
            public async Task EthosApiBuilderService_DeleteEthosApiBuilder()
            {
                _ethosApiBuilderRepositoryMock.Setup(x => x.GetRecordInfoFromGuidAsync(It.IsAny<string>())).ReturnsAsync(guidLookupResult);
                await _ethosApiBuilderService.DeleteEthosApiBuilderAsync(ethosApiBuilderGuid, ethosApiConfiguration.ResourceName);
            }
            #endregion

        }
    }
}