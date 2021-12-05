//Copyright 2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class RegionIsoCodesServiceTests
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
        public class RegionIsoCodesServiceTests_RegionIsoCodes
        {
            private const string regionsIsoGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string regionCode = "US-AK";

            private List<RegionIsoCodes> _regionIsoDtoCollection;
            private List<Domain.Base.Entities.Place> _placeEntityCollection;
            private RegionIsoCodesService _regionIsoCodesService;

            private Mock<ILogger> _loggerMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private Mock<IConfigurationRepository> _configurationRepoMock;
            private Mock<IReferenceDataRepository> _referenceRepositoryMock;

            [TestInitialize]
            public async void Initialize()
            {
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _loggerMock = new Mock<ILogger>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _configurationRepoMock = new Mock<IConfigurationRepository>();
                _referenceRepositoryMock = new Mock<IReferenceDataRepository>();

                _placeEntityCollection = new List<Domain.Base.Entities.Place>()
                {
                    new Domain.Base.Entities.Place(Guid.NewGuid().ToString()) { PlacesCountry =  "Mex", PlacesDesc = "Baja California", PlacesRegion = "MX-BSN" } ,
                    new Domain.Base.Entities.Place(regionsIsoGuid) { PlacesCountry = "USA", PlacesDesc = "Alaska", PlacesRegion = "US-AK" },
                    new Domain.Base.Entities.Place(Guid.NewGuid().ToString()) { PlacesCountry = "CAN",  PlacesDesc = "Alberta", PlacesRegion = "CA-AB" }
                };
                
                _regionIsoDtoCollection = new List<Dtos.RegionIsoCodes>();
                foreach (var source in _placeEntityCollection)
                {
                    var regionIsoCodes = new Ellucian.Colleague.Dtos.RegionIsoCodes
                    {
                        Id = source.Guid,
                        Title = source.PlacesDesc,
                        ISOCode = source.PlacesRegion,
                        Status = (!string.IsNullOrEmpty(source.PlacesInactive) && source.PlacesInactive.Equals("Y", StringComparison.OrdinalIgnoreCase))
                        ? Status.Inactive : Status.Active
                    };
                    _regionIsoDtoCollection.Add(regionIsoCodes);
                }

                _referenceRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>()))
                    .ReturnsAsync(_placeEntityCollection);

                _referenceRepositoryMock.Setup(repo => repo.GetRegionPlaceByGuidAsync(regionsIsoGuid))
                   .ReturnsAsync(_placeEntityCollection.FirstOrDefault(x => x.Guid == regionsIsoGuid));

                _regionIsoCodesService = new RegionIsoCodesService(_referenceRepositoryMock.Object,
                     _adapterRegistryMock.Object,
                     _currentUserFactoryMock.Object,
                    _roleRepositoryMock.Object,
                    _configurationRepoMock.Object, _loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _regionIsoCodesService = null;
                _loggerMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _configurationRepoMock = null;
            }

            [TestMethod]
            public async Task RegionIsoCodesService_GetRegionIsoCodesAsync()
            {
                var results = await _regionIsoCodesService.GetRegionIsoCodesAsync(null, true);
                Assert.IsTrue(results is IEnumerable<RegionIsoCodes>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task RegionIsoCodesService_GetRegionIsoCodesAsync_Count()
            {
                var results = await _regionIsoCodesService.GetRegionIsoCodesAsync(null, true);
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task RegionIsoCodesService_GetRegionIsoCodesAsync_Properties()
            {
                var result =
                    (await _regionIsoCodesService.GetRegionIsoCodesAsync(null, true)).FirstOrDefault(x => x.ISOCode == regionCode);
                Assert.IsNotNull(result.Id);

                Assert.IsNotNull(result.ISOCode);

            }

            [TestMethod]
            public async Task RegionIsoCodesService_GetRegionIsoCodesAsync_Expected()
            {
                var expectedResults = _regionIsoDtoCollection.FirstOrDefault(c => c.Id == regionsIsoGuid);
                var actualResult =
                    (await _regionIsoCodesService.GetRegionIsoCodesAsync(null, true)).FirstOrDefault(x => x.Id == regionsIsoGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);

            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task RegionIsoCodesService_GetRegionIsoCodesAsync_RepositoryException()
            {
                _referenceRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>()))
                  .ThrowsAsync(new RepositoryException());

                var expectedResults = _regionIsoDtoCollection.FirstOrDefault(c => c.Id == regionsIsoGuid);
                var actualResult =
                    (await _regionIsoCodesService.GetRegionIsoCodesAsync(null, true)).FirstOrDefault(x => x.Id == regionsIsoGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegionIsoCodesService_GetRegionIsoCodesAsync_KeyNotFoundException()
            {
                _referenceRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>()))
                  .ThrowsAsync(new KeyNotFoundException());

                var expectedResults = _regionIsoDtoCollection.FirstOrDefault(c => c.Id == regionsIsoGuid);
                var actualResult =
                    (await _regionIsoCodesService.GetRegionIsoCodesAsync(null, true)).FirstOrDefault(x => x.Id == regionsIsoGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task RegionIsoCodesService_GetRegionIsoCodesAsync_InvalidOperationException()
            {
                _referenceRepositoryMock.Setup(repo => repo.GetPlacesAsync(It.IsAny<bool>()))
                  .ThrowsAsync(new InvalidOperationException());

                var expectedResults = _regionIsoDtoCollection.FirstOrDefault(c => c.Id == regionsIsoGuid);
                var actualResult =
                    (await _regionIsoCodesService.GetRegionIsoCodesAsync(null, true)).FirstOrDefault(x => x.Id == regionsIsoGuid);
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegionIsoCodesService_GetRegionIsoCodesByGuidAsync_Empty()
            {
                await _regionIsoCodesService.GetRegionIsoCodesByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegionIsoCodesService_GetRegionIsoCodesByGuidAsync_Null()
            {
                await _regionIsoCodesService.GetRegionIsoCodesByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegionIsoCodesService_GetRegionIsoCodesByGuidAsync_Invalid()
            {
                await _regionIsoCodesService.GetRegionIsoCodesByGuidAsync("invalid");
            }

            [TestMethod]
            public async Task RegionIsoCodesService_GetRegionIsoCodesByGuidAsync_Expected()
            {
                var expectedResults =
                    _regionIsoDtoCollection.First(c => c.Id == regionsIsoGuid);
                var actualResult =
                    await _regionIsoCodesService.GetRegionIsoCodesByGuidAsync(regionsIsoGuid);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Title, actualResult.Title);
            }

            [TestMethod]
            public async Task RegionIsoCodesService_GetRegionIsoCodesByGuidAsync_Properties()
            {
                var result =
                    await _regionIsoCodesService.GetRegionIsoCodesByGuidAsync(regionsIsoGuid);
                Assert.IsNotNull(result.Id);
                Assert.IsNotNull(result.Status);
                Assert.IsNotNull(result.ISOCode);
                Assert.IsNotNull(result.Title);
            }

        }
    }
}