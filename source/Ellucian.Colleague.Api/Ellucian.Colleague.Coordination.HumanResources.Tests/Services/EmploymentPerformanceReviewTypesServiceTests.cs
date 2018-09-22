//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{    
    [TestClass]
    public class EmploymentPerformanceReviewTypesServiceTests : HumanResourcesServiceTestsSetup
    {
        private const string employmentPerformanceReviewTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string employmentPerformanceReviewTypesCode = "AT";
        private ICollection<EmploymentPerformanceReviewType> _employmentPerformanceReviewTypesCollection;
        private EmploymentPerformanceReviewTypesService _employmentPerformanceReviewTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;

        [TestInitialize]
        public async void Initialize()
        {
            MockInitialize();
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _employmentPerformanceReviewTypesCollection = new List<EmploymentPerformanceReviewType>()
                {
                    new EmploymentPerformanceReviewType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new EmploymentPerformanceReviewType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new EmploymentPerformanceReviewType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentPerformanceReviewTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_employmentPerformanceReviewTypesCollection);

            _employmentPerformanceReviewTypesService = new EmploymentPerformanceReviewTypesService(_referenceRepositoryMock.Object, adapterRegistryMock.Object,
                employeeCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _employmentPerformanceReviewTypesService = null;
            _employmentPerformanceReviewTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesAsync()
        {
            var results = await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<EmploymentPerformanceReviewTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesAsync_Count()
        {
            var results = await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesAsync_Properties()
        {
            var result =
                (await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesAsync(true)).FirstOrDefault(x => x.Code == employmentPerformanceReviewTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesAsync_Expected()
        {
            var expectedResults = _employmentPerformanceReviewTypesCollection.FirstOrDefault(c => c.Guid == employmentPerformanceReviewTypesGuid);
            var actualResult =
                (await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesAsync(true)).FirstOrDefault(x => x.Id == employmentPerformanceReviewTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesByGuidAsync_Empty()
        {
            await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesByGuidAsync_Null()
        {
            await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetEmploymentPerformanceReviewTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _employmentPerformanceReviewTypesCollection.First(c => c.Guid == employmentPerformanceReviewTypesGuid);
            var actualResult =
                await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesByGuidAsync(employmentPerformanceReviewTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task EmploymentPerformanceReviewTypesService_GetEmploymentPerformanceReviewTypesByGuidAsync_Properties()
        {
            var result =
                await _employmentPerformanceReviewTypesService.GetEmploymentPerformanceReviewTypesByGuidAsync(employmentPerformanceReviewTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}