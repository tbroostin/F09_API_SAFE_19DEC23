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
    public class RoommateCharacteristicsServiceTests
    {
        private const string roommateCharacteristicsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string roommateCharacteristicsCode = "AT";
        private ICollection<Domain.Student.Entities.RoommateCharacteristics> _roommateCharacteristicsCollection;
        private RoommateCharacteristicsService _roommateCharacteristicsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _roommateCharacteristicsCollection = new List<Domain.Student.Entities.RoommateCharacteristics>()
                {
                    new Domain.Student.Entities.RoommateCharacteristics("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.RoommateCharacteristics("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.RoommateCharacteristics("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetRoommateCharacteristicsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_roommateCharacteristicsCollection);

            _roommateCharacteristicsService = new RoommateCharacteristicsService(_referenceRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _roommateCharacteristicsService = null;
            _roommateCharacteristicsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsAsync()
        {
            var results = await _roommateCharacteristicsService.GetRoommateCharacteristicsAsync(true);
            Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.RoommateCharacteristics>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsAsync_Count()
        {
            var results = await _roommateCharacteristicsService.GetRoommateCharacteristicsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsAsync_Properties()
        {
            var result =
                (await _roommateCharacteristicsService.GetRoommateCharacteristicsAsync(true)).FirstOrDefault(x => x.Code == roommateCharacteristicsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsAsync_Expected()
        {
            var expectedResults = _roommateCharacteristicsCollection.FirstOrDefault(c => c.Guid == roommateCharacteristicsGuid);
            var actualResult =
                (await _roommateCharacteristicsService.GetRoommateCharacteristicsAsync(true)).FirstOrDefault(x => x.Id == roommateCharacteristicsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsByGuidAsync_Empty()
        {
            await _roommateCharacteristicsService.GetRoommateCharacteristicsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsByGuidAsync_Null()
        {
            await _roommateCharacteristicsService.GetRoommateCharacteristicsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetRoommateCharacteristicsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _roommateCharacteristicsService.GetRoommateCharacteristicsByGuidAsync("99");
        }

        [TestMethod]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsByGuidAsync_Expected()
        {
            var expectedResults =
                _roommateCharacteristicsCollection.First(c => c.Guid == roommateCharacteristicsGuid);
            var actualResult =
                await _roommateCharacteristicsService.GetRoommateCharacteristicsByGuidAsync(roommateCharacteristicsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task RoommateCharacteristicsService_GetRoommateCharacteristicsByGuidAsync_Properties()
        {
            var result =
                await _roommateCharacteristicsService.GetRoommateCharacteristicsByGuidAsync(roommateCharacteristicsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}