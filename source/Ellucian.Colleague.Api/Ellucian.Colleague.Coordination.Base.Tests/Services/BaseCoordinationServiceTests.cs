//Copyright 2020-2022 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.EthosExtend;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{


    [TestClass]
    public class BaseCoordinationServiceTests: CurrentUserSetup
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role( 105, "Student" );
            protected Ellucian.Colleague.Domain.Entities.Role viewPersonRole = new Ellucian.Colleague.Domain.Entities.Role( 1, "VIEW.PERSON" );
            protected Ellucian.Colleague.Domain.Entities.Role createPersonRole = new Ellucian.Colleague.Domain.Entities.Role( 2, "CREATE.PERSON" );


            public class PersonUserFactory: ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser( new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
                            SessionFixationId = "abc123",
                        } );
                    }
                }
            }
        }

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IStaffRepository> _staffRepoMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;

        private IAddressService _addresService;
        private ICurrentUserFactory _currentUserFactory;
        private List<EthosSecurity> _ethosSecurity;
        private List<EthosSecurityDefinitions> _ethosSecDefs;
        Domain.Base.Entities.EthosExtensibleData _ethosExtensibleData;

        [TestInitialize]
        public void Initialize()
        {
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _loggerMock = new Mock<ILogger>();
            _staffRepoMock = new Mock<IStaffRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();

            // Mock permissions
            var permissionViewAnyPerson = new Ellucian.Colleague.Domain.Entities.Permission( BasePermissionCodes.ViewAnyPerson );
            personRole.AddPermission( permissionViewAnyPerson );
            _roleRepositoryMock.Setup( rpm => rpm.Roles ).Returns( new List<Domain.Entities.Role>() { personRole } );

            _ethosSecDefs = new List<EthosSecurityDefinitions>()
            {
                new EthosSecurityDefinitions("id", "user", "Student", "VIEW.PERSON", false)
            };
            _ethosSecurity = new List<EthosSecurity>()
            {
                new EthosSecurity("addresses", _ethosSecDefs)
                {
                    PropertyDefinitions = new List<EthosSecurityDefinitions>()
                    {
                        new EthosSecurityDefinitions("id", "Student", "1", "VIEW.PERSON", true),
                        new EthosSecurityDefinitions("id", "user", "2", "VIEW.PERSON", true)
                    }
                }
            };
            // Set up current user
            _currentUserFactory = new CurrentUserSetup.PersonUserFactory();
            List<Domain.Base.Entities.EthosExtensibleDataRow> dataRowList = new List<Domain.Base.Entities.EthosExtensibleDataRow>()
            {
                new Domain.Base.Entities.EthosExtensibleDataRow("addr1", "addr1", "addrLine1", "address", "string", "123 Any Str", 50),
                new Domain.Base.Entities.EthosExtensibleDataRow("houseNumber", "number", "addrLine1", "address", "number", "123", 50),                new Domain.Base.Entities.EthosExtensibleDataRow("addr1", "addr1", "addrLine1", "address", "string", "123 Any Str", 50),
                new Domain.Base.Entities.EthosExtensibleDataRow("whenMoved", "date", "addrLine1", "address", "date", "123 Any Str", 50),
                new Domain.Base.Entities.EthosExtensibleDataRow("whatTime", "time", "addrLine1", "address", "time", "123 Any Str", 50),
                new Domain.Base.Entities.EthosExtensibleDataRow("datetime", "datetime", "addrLine1", "address", "datetime", "123 Any Str", 50),
                new Domain.Base.Entities.EthosExtensibleDataRow("default", "default", "addrLine1", "address", "default", "123 Any Str", 50)

            };
            _ethosExtensibleData = new Domain.Base.Entities.EthosExtensibleData( "addresses", "1.0.0", "schemaType1", "1", "", dataRowList );
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _loggerMock = null;
            _staffRepoMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void Ctor_Adapter_Null_ArgumentNullException()
        {
            try
            {
                _addresService = new AddressService( null, null, _configurationRepoMock.Object, null, _currentUserFactoryMock.Object, _roleRepositoryMock.Object, _loggerMock.Object );
            }
            catch( ArgumentNullException e )
            {
                Assert.AreEqual( "Value cannot be null.\r\nParameter name: adapterRegistry", e.Message );
                throw;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void Ctor_CurrentUser_Null_ArgumentNullException()
        {
            try
            {
                _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, null, _roleRepositoryMock.Object, _loggerMock.Object );
            }
            catch( ArgumentNullException e )
            {
                Assert.AreEqual( "Value cannot be null.\r\nParameter name: currentUserFactory", e.Message );
                throw;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void Ctor_RoleRepo_Null_ArgumentNullException()
        {
            try
            {
                _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactoryMock.Object, null, _loggerMock.Object );
            }
            catch( ArgumentNullException e )
            {
                Assert.AreEqual( "Value cannot be null.\r\nParameter name: roleRepository", e.Message );
                throw;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public void Ctor_Logger_Null_ArgumentNullException()
        {
            try
            {
                _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactoryMock.Object, _roleRepositoryMock.Object, null );
            }
            catch( ArgumentNullException e )
            {
                Assert.AreEqual( "Value cannot be null.\r\nParameter name: logger", e.Message );
                throw;
            }
        }

        [TestMethod]
        public async Task GetDataPrivacyListByApi_GetDataPrivacyListByApi_With_APIName()
        {
            _roleRepositoryMock.Setup( repo => repo.GetRolesAsync() )
                .ReturnsAsync(
                                new List<Domain.Entities.Role>()
                                {
                                    new Domain.Entities.Role( 1, "Admin" ),
                                    new Domain.Entities.Role( 2, "Student" )
                                }
                             );
            _configurationRepoMock.Setup( repo => repo.GetEthosDataPrivacyConfiguration( It.IsAny<bool>() ) ).ReturnsAsync( _ethosSecurity );
            _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            var results = await _addresService.GetDataPrivacyListByApi( "addresses", It.IsAny<bool>() );
            Assert.IsNotNull( results );
        }

        [TestMethod]
        public async Task GetDataPrivacyListByApi_GetDataPrivacyListByApi_With_EthosResourceRouteInfo()
        {
            var roles = new List<Domain.Entities.Role>()
                                {
                                    new Domain.Entities.Role( 1, "Admin" ),
                                    new Domain.Entities.Role( 2, "Student" )
                                };
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "VIEW.PERSON" ) { } );
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "CREATE.PERSON" ) { } );
            _roleRepositoryMock.Setup( repo => repo.GetRolesAsync() )
                .ReturnsAsync( roles );
            _configurationRepoMock.Setup( repo => repo.GetEthosDataPrivacyConfiguration( It.IsAny<bool>() ) ).ReturnsAsync( _ethosSecurity );
            _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };

            var results = await _addresService.GetDataPrivacyListByApi( info, It.IsAny<bool>() );
            Assert.IsNotNull( results );
        }

        [TestMethod]
        public async Task GetDataPrivacyListByApi_GetDataPrivacyListByApi_With_NUll_EthosDataPrivacyList()
        {
            var roles = new List<Domain.Entities.Role>()
                                {
                                    new Domain.Entities.Role( 1, "Admin" ),
                                    new Domain.Entities.Role( 2, "Student" )
                                };
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "VIEW.PERSON" ) { } );
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "CREATE.PERSON" ) { } );
            _roleRepositoryMock.Setup( repo => repo.GetRolesAsync() )
                .ReturnsAsync( roles );
            _configurationRepoMock.Setup( repo => repo.GetEthosDataPrivacyConfiguration( It.IsAny<bool>() ) ).ReturnsAsync( _ethosSecurity );
            _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "BAD_API_NAME",
                ResourceVersionNumber = "1.0.0"
            };

            var results = await _addresService.GetDataPrivacyListByApi( info, It.IsAny<bool>() );
            Assert.IsNotNull( results );
            Assert.AreEqual( 0, results.Count() );
        }

        [TestMethod]
        public async Task GetDataPrivacyListByApi_GetDataPrivacyListByApi_With_NotAllowedToViewData()
        {
            var roles = new List<Domain.Entities.Role>()
                                {
                                    new Domain.Entities.Role( 1, "Admin" ),
                                    new Domain.Entities.Role( 2, "Student" )
                                };
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "VIEW.PERSON" ) { } );
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "CREATE.PERSON" ) { } );
            _roleRepositoryMock.Setup( repo => repo.GetRolesAsync() )
                .ReturnsAsync( roles );
            _ethosSecurity[ 0 ].PropertyDefinitions.FirstOrDefault().NotAllowedToViewData = true;
            _configurationRepoMock.Setup( repo => repo.GetEthosDataPrivacyConfiguration( It.IsAny<bool>() ) ).ReturnsAsync( _ethosSecurity );
            _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };

            var results = await _addresService.GetDataPrivacyListByApi( info, It.IsAny<bool>() );
            Assert.IsNotNull( results );
        }

        [TestMethod]
        public async Task GetDataPrivacyListByApi_GetDataPrivacyListByApi_With_AllowedToViewData()
        {
            var roles = new List<Domain.Entities.Role>()
                                {
                                    new Domain.Entities.Role( 1, "Admin" ),
                                    new Domain.Entities.Role( 2, "Student" )
                                };
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "VIEW.PERSON" ) { } );
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "CREATE.PERSON" ) { } );
            _roleRepositoryMock.Setup( repo => repo.GetRolesAsync() )
                .ReturnsAsync( roles );
            _ethosSecurity[ 0 ].PropertyDefinitions.FirstOrDefault().NotAllowedToViewData = false;
            _configurationRepoMock.Setup( repo => repo.GetEthosDataPrivacyConfiguration( It.IsAny<bool>() ) ).ReturnsAsync( _ethosSecurity );
            _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };

            var results = await _addresService.GetDataPrivacyListByApi( info, It.IsAny<bool>() );
            Assert.IsNotNull( results );
        }

        [TestMethod]
        public async Task GetDataPrivacyListByApi_GetDataPrivacyListByApi_With_NotAllowedToViewData_True()
        {
            var roles = new List<Domain.Entities.Role>()
                                {
                                    new Domain.Entities.Role( 1, "Admin" ),
                                    new Domain.Entities.Role( 2, "Student" )
                                };
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "VIEW.PERSON" ) { } );
            roles[ 1 ].AddPermission( new Domain.Entities.Permission( "CREATE.PERSON" ) { } );
            _roleRepositoryMock.Setup( repo => repo.GetRolesAsync() )
                .ReturnsAsync( roles );
            _ethosSecurity[ 0 ].PropertyDefinitions.FirstOrDefault().NotAllowedToViewData = true;
            _ethosSecurity[ 0 ].PropertyDefinitions.ElementAt(1).NotAllowedToViewData = true;
            _configurationRepoMock.Setup( repo => repo.GetEthosDataPrivacyConfiguration( It.IsAny<bool>() ) ).ReturnsAsync( _ethosSecurity );
            _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };

            var results = await _addresService.GetDataPrivacyListByApi( info, It.IsAny<bool>() );
            Assert.IsNotNull( results );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetDataPrivacyListByApi_GetDataPrivacyListByApi_With_NotAllowedToViewData_Null_Configuration()
        {
            _configurationRepoMock = null;
            _addresService = new AddressService( _adapterRegistryMock.Object, null, null, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };

            var results = await _addresService.GetDataPrivacyListByApi( info, It.IsAny<bool>() );
        }

        [TestMethod]
        public async Task GetExtendedEthosDataByResource()
        {
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };
            _configurationRepoMock.Setup( repo => repo.GetExtendedEthosConfigurationByResource( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()) )
                                  .ReturnsAsync( _ethosExtensibleData );
            _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            var results = await _addresService.GetExtendedEthosDataByResource( info, new List<string>() { }, false );
            Assert.IsNotNull( results );
        }

        [TestMethod]
        [ExpectedException(typeof( ArgumentNullException ) )]
        public async Task GetExtendedEthosDataByResource_ArgumentNullException()
        {
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };
            _addresService = new AddressService( _adapterRegistryMock.Object, null, null, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            var results = await _addresService.GetExtendedEthosDataByResource( info, new List<string>() { }, false );
        }

        [TestMethod]
        public async Task GetExtendedEthosConfigurationByResource()
        {
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };
            _configurationRepoMock.Setup( repo => repo.GetExtendedEthosConfigurationByResource( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()) )
                                  .ReturnsAsync( _ethosExtensibleData );
            _addresService = new AddressService( _adapterRegistryMock.Object, null, _configurationRepoMock.Object, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            var results = await _addresService.GetExtendedEthosConfigurationByResource( info, false );
            Assert.IsNotNull( results );
        }

        [TestMethod]
        [ExpectedException( typeof( ArgumentNullException ) )]
        public async Task GetExtendedEthosConfigurationByResource_ArgumentNullException()
        {
            EthosResourceRouteInfo info = new EthosResourceRouteInfo()
            {
                BypassCache = false,
                EthosResourceIdentifier = "1",
                ExtendedSchemaResourceId = "id",
                ResourceName = "addresses",
                ResourceVersionNumber = "1.0.0"
            };
            _addresService = new AddressService( _adapterRegistryMock.Object, null, null, null, _currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object );
            var results = await _addresService.GetExtendedEthosConfigurationByResource( info, false );
        }
    }
}