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
    public class AssessmentPercentileTypesServiceTests
    {
        private const string assessmentPercentileTypesGuid = "792b6834-2f9c-409c-8afa-e0081972adb4";
        private const string assessmentPercentileTypesCode = "1";
        private ICollection<IntgTestPercentileType> _intgTestPercentileTypesCollection;
        private AssessmentPercentileTypesService _assessmentPercentileTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _intgTestPercentileTypesCollection = new List<IntgTestPercentileType>()
            {
                new IntgTestPercentileType("792b6834-2f9c-409c-8afa-e0081972adb4", "1", "1st percentile"),
                new IntgTestPercentileType("ab8395f3-663d-4d09-b3f6-af28668dc362", "2", "2nd percentile")
            };


            _referenceRepositoryMock.Setup(repo => repo.GetIntgTestPercentileTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_intgTestPercentileTypesCollection);

            _assessmentPercentileTypesService = new AssessmentPercentileTypesService(_referenceRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _assessmentPercentileTypesService = null;
            _intgTestPercentileTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesAsync()
        {
            var results = await _assessmentPercentileTypesService.GetAssessmentPercentileTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<AssessmentPercentileTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesAsync_Count()
        {
            var results = await _assessmentPercentileTypesService.GetAssessmentPercentileTypesAsync(true);
            Assert.AreEqual(2, results.Count());
        }

        [TestMethod]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesAsync_Properties()
        {
            var result =
                (await _assessmentPercentileTypesService.GetAssessmentPercentileTypesAsync(true)).FirstOrDefault(x => x.Code == assessmentPercentileTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesAsync_Expected()
        {
            var expectedResults = _intgTestPercentileTypesCollection.FirstOrDefault(c => c.Guid == assessmentPercentileTypesGuid);
            var actualResult =
                (await _assessmentPercentileTypesService.GetAssessmentPercentileTypesAsync(true)).FirstOrDefault(x => x.Id == assessmentPercentileTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesByGuidAsync_Empty()
        {
            await _assessmentPercentileTypesService.GetAssessmentPercentileTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesByGuidAsync_Null()
        {
            await _assessmentPercentileTypesService.GetAssessmentPercentileTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetIntgTestPercentileTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _assessmentPercentileTypesService.GetAssessmentPercentileTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _intgTestPercentileTypesCollection.First(c => c.Guid == assessmentPercentileTypesGuid);
            var actualResult =
                await _assessmentPercentileTypesService.GetAssessmentPercentileTypesByGuidAsync(assessmentPercentileTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AssessmentPercentileTypesService_GetAssessmentPercentileTypesByGuidAsync_Properties()
        {
            var result =
                await _assessmentPercentileTypesService.GetAssessmentPercentileTypesByGuidAsync(assessmentPercentileTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
        }
    }
}