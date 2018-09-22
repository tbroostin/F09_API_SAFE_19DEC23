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

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class FloorCharacteristicsServiceTests
    {
        private const string floorCharacteristicsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string floorCharacteristicsCode = "AT";
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics> _floorCharacteristicsCollection;
        private FloorCharacteristicsService _floorCharacteristicsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _floorCharacteristicsCollection = new List<Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.FloorCharacteristics("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetFloorCharacteristicsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_floorCharacteristicsCollection);

            _floorCharacteristicsService = new FloorCharacteristicsService(_referenceRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _floorCharacteristicsService = null;
            _floorCharacteristicsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsAsync()
        {
            var results = await _floorCharacteristicsService.GetFloorCharacteristicsAsync(true);
            Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.FloorCharacteristics>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsAsync_Count()
        {
            var results = await _floorCharacteristicsService.GetFloorCharacteristicsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsAsync_Properties()
        {
            var result =
                (await _floorCharacteristicsService.GetFloorCharacteristicsAsync(true)).FirstOrDefault(x => x.Code == floorCharacteristicsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsAsync_Expected()
        {
            var expectedResults = _floorCharacteristicsCollection.FirstOrDefault(c => c.Guid == floorCharacteristicsGuid);
            var actualResult =
                (await _floorCharacteristicsService.GetFloorCharacteristicsAsync(true)).FirstOrDefault(x => x.Id == floorCharacteristicsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsByGuidAsync_Empty()
        {
            await _floorCharacteristicsService.GetFloorCharacteristicsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsByGuidAsync_Null()
        {
            await _floorCharacteristicsService.GetFloorCharacteristicsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetFloorCharacteristicsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _floorCharacteristicsService.GetFloorCharacteristicsByGuidAsync("99");
        }

        [TestMethod]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsByGuidAsync_Expected()
        {
            var expectedResults =
                _floorCharacteristicsCollection.First(c => c.Guid == floorCharacteristicsGuid);
            var actualResult =
                await _floorCharacteristicsService.GetFloorCharacteristicsByGuidAsync(floorCharacteristicsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task FloorCharacteristicsService_GetFloorCharacteristicsByGuidAsync_Properties()
        {
            var result =
                await _floorCharacteristicsService.GetFloorCharacteristicsByGuidAsync(floorCharacteristicsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}