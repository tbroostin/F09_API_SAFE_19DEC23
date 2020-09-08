// Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
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
    public class AdmissionApplicationSupportingItemStatusesServiceTests
    {
        // The service to be tested
        private AdmissionApplicationSupportingItemStatusesService _admissionApplicationSupportingItemStatusesService;

        private Mock<IReferenceDataRepository> _refRepoMock;
        private Mock<IEventRepository> _eventRepoMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IRoleRepository> _roleRepoMock;
        private Mock<ILogger> _loggerMock;

        private ICurrentUserFactory _currentUserFactory;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        // Emergency information data for one person for tests
        private const string personId = "S001";

        private IEnumerable<Domain.Base.Entities.CorrStatus> _corrStatuses;

        private string _guid = "b0eba383-5acf-4050-949d-8bb7a17c5012";

        [TestInitialize]
        public void Initialize()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _refRepoMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _roleRepoMock = new Mock<IRoleRepository>();
            _loggerMock = new Mock<ILogger>();

            // Set up current user
            _currentUserFactory = new PersonServiceTests.CurrentUserSetup.PersonUserFactory();
            _admissionApplicationSupportingItemStatusesService = new AdmissionApplicationSupportingItemStatusesService( _refRepoMock.Object, _adapterRegistryMock.Object, _currentUserFactory,
                _roleRepoMock.Object, _configurationRepositoryMock.Object, _loggerMock.Object );

            _corrStatuses = new TestAdmissionApplicationSupportingItemStatusesRepository().Get();
            _refRepoMock.Setup( repo => repo.GetCorrStatusesAsync( It.IsAny<bool>() ) ).ReturnsAsync( _corrStatuses );
        }

        [TestCleanup]
        public void Cleanup()
        {
            _admissionApplicationSupportingItemStatusesService = null;
            _refRepoMock = null;
            _configurationRepositoryMock = null;
            _eventRepoMock = null;
            _adapterRegistryMock = null;
            _roleRepoMock = null;
            _currentUserFactory = null;
        }

        [TestMethod]
        public async Task GetAdmissionApplicationSupportingItemStatusByGuidAsync()
        {
            Domain.Base.Entities.CorrStatus corStatus = _corrStatuses.Where( m => m.Guid == _guid ).FirstOrDefault();
            _refRepoMock.Setup( repo => repo.GetCorrStatusesAsync( It.IsAny<bool>() ) ).ReturnsAsync( _corrStatuses.Where( m => m.Guid == _guid ) );
            var admApplSupItemStat = await _admissionApplicationSupportingItemStatusesService.GetAdmissionApplicationSupportingItemStatusByGuidAsync( _guid );
            Assert.AreEqual( corStatus.Guid, admApplSupItemStat.Id );
            Assert.AreEqual( corStatus.Code, admApplSupItemStat.Code );
            Assert.AreEqual( corStatus.Description, admApplSupItemStat.Title );
            if( corStatus.Action == "0" )
            {
                Assert.AreEqual( admApplSupItemStat.Type, AdmissionApplicationSupportingItemStatusType.Waived );
            }
            else if( corStatus.Action == "1" )
            {
                Assert.AreEqual( admApplSupItemStat.Type, AdmissionApplicationSupportingItemStatusType.Received );
            }
            else
            {
                Assert.AreEqual( admApplSupItemStat.Type, AdmissionApplicationSupportingItemStatusType.Incomplete );
            }
        }

        [TestMethod]
        public async Task GetAdmissionApplicationSupportingItemStatusesAsync()
        {
            _refRepoMock.Setup( repo => repo.GetCorrStatusesAsync( true ) ).ReturnsAsync( _corrStatuses.Where( m => m.Guid == _guid ) );
            var admApplSupItemStats = await _admissionApplicationSupportingItemStatusesService.GetAdmissionApplicationSupportingItemStatusesAsync( It.IsAny<bool>() );

            foreach( var admApplSupItemStat in admApplSupItemStats )
            {
                Domain.Base.Entities.CorrStatus corStatus = _corrStatuses.FirstOrDefault( m => m.Guid.Equals( admApplSupItemStat.Id, System.StringComparison.InvariantCultureIgnoreCase ) );
                Assert.AreEqual( corStatus.Guid, admApplSupItemStat.Id );
                Assert.AreEqual( corStatus.Code, admApplSupItemStat.Code );
                Assert.AreEqual( corStatus.Description, admApplSupItemStat.Title );
                if( corStatus.Action == "0" )
                {
                    Assert.AreEqual( admApplSupItemStat.Type, AdmissionApplicationSupportingItemStatusType.Waived );
                }
                else if( corStatus.Action == "1" )
                {
                    Assert.AreEqual( admApplSupItemStat.Type, AdmissionApplicationSupportingItemStatusType.Received );
                }
                else
                {
                    Assert.AreEqual( admApplSupItemStat.Type, AdmissionApplicationSupportingItemStatusType.Incomplete );
                }
            }
        }

        [TestMethod]
        [ExpectedException( typeof( KeyNotFoundException ) )]
        public async Task GetAdmissionApplicationSupportingItemStatusByGuidAsync_InvalidOperationException()
        {
            //_refRepoMock.Setup( repo => repo.GetCorrStatusesAsync( It.IsAny<bool>() ) ).ReturnsAsync( _corrStatuses.Where( m => m.Guid == _guid ) );
            var guid = Guid.NewGuid().ToString();
            try
            {
                var admApplSupItemStat = await _admissionApplicationSupportingItemStatusesService.GetAdmissionApplicationSupportingItemStatusByGuidAsync( guid );
            }
            catch( KeyNotFoundException e )
            {
                Assert.AreEqual( string.Format( "admission-application-supporting-item-statuses not found for GUID {0}", guid ), e.Message );
                throw;
            }
        }

        [TestMethod]
        [ExpectedException( typeof( KeyNotFoundException ) )]
        public async Task GetAdmissionApplicationSupportingItemStatusByGuidAsync_KeyNotFoundException()
        {
            _refRepoMock.Setup( repo => repo.GetCorrStatusesAsync( It.IsAny<bool>() ) ).ThrowsAsync( new KeyNotFoundException() );
            var guid = Guid.NewGuid().ToString();
            try
            {
                var admApplSupItemStat = await _admissionApplicationSupportingItemStatusesService.GetAdmissionApplicationSupportingItemStatusByGuidAsync( guid );
            }
            catch( KeyNotFoundException e )
            {
                Assert.AreEqual( string.Format( "admission-application-supporting-item-statuses not found for GUID {0}", guid ), e.Message );
                throw;
            }
        }
    }
}