//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class AptitudeAssessmentTypesServiceTests
    {
        private const string aptitudeAssessmentTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string aptitudeAssessmentTypesCode = "AD";
        private ICollection<NonCourseCategories> _nonCourseCategoriesCollection;
        private AptitudeAssessmentTypesService _aptitudeAssessmentTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;

        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();

            _nonCourseCategoriesCollection = new List<NonCourseCategories>()
            {
                new NonCourseCategories("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AD", "Admissions") { SpecialProcessingCode = "A"},
                new NonCourseCategories("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "PL", "Placement") { SpecialProcessingCode = "P"} ,
                new NonCourseCategories("d2253ac7-9931-4560-b42f-1fccd43c952e", "R", "Re-Test for Placement") { SpecialProcessingCode = "T"} 
            };


            _referenceRepositoryMock.Setup(repo => repo.GetNonCourseCategoriesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_nonCourseCategoriesCollection);

            _aptitudeAssessmentTypesService = new AptitudeAssessmentTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _aptitudeAssessmentTypesService = null;
            _nonCourseCategoriesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesAsync()
        {
            var results = await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<AptitudeAssessmentTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesAsync_Count()
        {
            var results = await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesAsync_Properties()
        {
            var result =
                (await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesAsync(true)).FirstOrDefault(x => x.Code == aptitudeAssessmentTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesAsync_Expected()
        {
            var expectedResults = _nonCourseCategoriesCollection.FirstOrDefault(c => c.Guid == aptitudeAssessmentTypesGuid);
            var actualResult =
                (await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesAsync(true)).FirstOrDefault(x => x.Id == aptitudeAssessmentTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesByGuidAsync_Empty()
        {
            await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesByGuidAsync_Null()
        {
            await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetNonCourseCategoriesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _nonCourseCategoriesCollection.First(c => c.Guid == aptitudeAssessmentTypesGuid);
            var actualResult =
                await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesByGuidAsync(aptitudeAssessmentTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task AptitudeAssessmentTypesService_GetAptitudeAssessmentTypesByGuidAsync_Properties()
        {
            var result =
                await _aptitudeAssessmentTypesService.GetAptitudeAssessmentTypesByGuidAsync(aptitudeAssessmentTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}