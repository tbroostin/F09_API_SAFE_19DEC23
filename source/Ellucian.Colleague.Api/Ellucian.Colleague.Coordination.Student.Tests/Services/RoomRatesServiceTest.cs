//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class RoomRatesServiceTests : SectionCoordinationServiceTests.CurrentUserSetup
    {
        private const string roomRatesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string roomRatesCode = "AT";
        private ICollection<RoomRate> _roomRatesCollection;
        private RoomRatesService _roomRatesService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IStudentRepository> _studentRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;
            _loggerMock = new Mock<ILogger>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            // Set up current user
            _currentUserFactory = new SectionCoordinationServiceTests.CurrentUserSetup.StudentUserFactory();

            _roomRatesCollection = new List<RoomRate>()
                {
                    new RoomRate("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") { DayRate = (decimal) 500, 
                        WeeklyRate = (decimal) 500, MonthlyRate = (decimal) 500, 
                        TermRate = (decimal) 500, AnnualRate = (decimal) 500 },
                    new RoomRate("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new RoomRate("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetRoomRatesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_roomRatesCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetHostCountryAsync())
                .ReturnsAsync("USA");

            _roomRatesService = new RoomRatesService(_referenceRepositoryMock.Object, _studentRepositoryMock.Object, _adapterRegistryMock.Object,
                _currentUserFactory, _roleRepo, _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _roomRatesService = null;
            _roomRatesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task RoomRatesService_GetRoomRatesAsync()
        {
            var results = await _roomRatesService.GetRoomRatesAsync(true);
            Assert.IsTrue(results is IEnumerable<RoomRates>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task RoomRatesService_GetRoomRatesAsync_Count()
        {
            var results = await _roomRatesService.GetRoomRatesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task RoomRatesService_GetRoomRatesAsync_Properties()
        {
            var result =
                (await _roomRatesService.GetRoomRatesAsync(true)).FirstOrDefault(x => x.Code == roomRatesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task RoomRatesService_GetRoomRatesAsync_Expected()
        {
            var expectedResults = _roomRatesCollection.FirstOrDefault(c => c.Guid == roomRatesGuid);
            var actualResult =
                (await _roomRatesService.GetRoomRatesAsync(true)).FirstOrDefault(x => x.Id == roomRatesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task RoomRatesService_GetRoomRatesByGuidAsync_Empty()
        {
            await _roomRatesService.GetRoomRatesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task RoomRatesService_GetRoomRatesByGuidAsync_Null()
        {
            await _roomRatesService.GetRoomRatesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task RoomRatesService_GetRoomRatesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRoomRatesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _roomRatesService.GetRoomRatesByGuidAsync("99");
        }

        [TestMethod]
        public async Task RoomRatesService_GetRoomRatesByGuidAsync_Expected()
        {
            var expectedResults =
                _roomRatesCollection.First(c => c.Guid == roomRatesGuid);
            var actualResult =
                await _roomRatesService.GetRoomRatesByGuidAsync(roomRatesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task RoomRatesService_GetRoomRatesByGuidAsync_Properties()
        {
            var result =
                await _roomRatesService.GetRoomRatesByGuidAsync(roomRatesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}