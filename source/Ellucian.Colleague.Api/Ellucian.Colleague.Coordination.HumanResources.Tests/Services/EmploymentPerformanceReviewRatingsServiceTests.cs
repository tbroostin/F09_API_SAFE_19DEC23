//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class EmploymentPerformanceReviewRatingsServiceTests : HumanResourcesServiceTestsSetup
    {
        private const string employmentPerformanceReviewRatingsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string employmentPerformanceReviewRatingsCode = "AT";
        private ICollection<EmploymentPerformanceReviewRating> _employmentPerformanceReviewRatingsCollection;
        private EmploymentPerformanceReviewRatingsService _employmentPerformanceReviewRatingsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            MockInitialize();
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;

            _employmentPerformanceReviewRatingsCollection = new List<EmploymentPerformanceReviewRating>()
                {
                    new EmploymentPerformanceReviewRating("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new EmploymentPerformanceReviewRating("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new EmploymentPerformanceReviewRating("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentPerformanceReviewRatingsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_employmentPerformanceReviewRatingsCollection);

            _employmentPerformanceReviewRatingsService = new EmploymentPerformanceReviewRatingsService(_referenceRepositoryMock.Object, adapterRegistryMock.Object, employeeCurrentUserFactory, _configurationRepository, roleRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _configurationRepository = null;
            _configurationRepositoryMock = null;
            _employmentPerformanceReviewRatingsService = null;
            _employmentPerformanceReviewRatingsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsAsync()
        {
            var results = await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsAsync(true);
            Assert.IsTrue(results is IEnumerable<EmploymentPerformanceReviewRatings>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsAsync_Count()
        {
            var results = await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsAsync_Properties()
        {
            var result =
                (await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsAsync(true)).FirstOrDefault(x => x.Code == employmentPerformanceReviewRatingsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsAsync_Expected()
        {
            var expectedResults = _employmentPerformanceReviewRatingsCollection.FirstOrDefault(c => c.Guid == employmentPerformanceReviewRatingsGuid);
            var actualResult =
                (await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsAsync(true)).FirstOrDefault(x => x.Id == employmentPerformanceReviewRatingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsByGuidAsync_Empty()
        {
            await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsByGuidAsync_Null()
        {
            await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentPerformanceReviewRatingsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsByGuidAsync("99");
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsByGuidAsync_Expected()
        {
            var expectedResults =
                _employmentPerformanceReviewRatingsCollection.First(c => c.Guid == employmentPerformanceReviewRatingsGuid);
            var actualResult =
                await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsByGuidAsync(employmentPerformanceReviewRatingsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewRatingsService_GetEmploymentPerformanceReviewRatingsByGuidAsync_Properties()
        {
            var result =
                await _employmentPerformanceReviewRatingsService.GetEmploymentPerformanceReviewRatingsByGuidAsync(employmentPerformanceReviewRatingsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}