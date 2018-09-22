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
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class CourseCategoriesServiceTests
    {
        private const string courseCategoriesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string courseCategoriesCode = "AT";
        private ICollection<CourseType> _courseCategoriesCollection;
        private CourseCategoriesService _courseCategoriesService;

        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _courseCategoriesCollection = new List<CourseType>()
                {
                    new CourseType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new CourseType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new CourseType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetCourseTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_courseCategoriesCollection);

            _courseCategoriesService = new CourseCategoriesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _courseCategoriesService = null;
            _courseCategoriesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task CourseCategoriesService_GetCourseCategoriesAsync()
        {
            var results = await _courseCategoriesService.GetCourseCategoriesAsync(true);
            Assert.IsTrue(results is IEnumerable<CourseCategories>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task CourseCategoriesService_GetCourseCategoriesAsync_Count()
        {
            var results = await _courseCategoriesService.GetCourseCategoriesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task CourseCategoriesService_GetCourseCategoriesAsync_Properties()
        {
            var result =
                (await _courseCategoriesService.GetCourseCategoriesAsync(true)).FirstOrDefault(x => x.Code == courseCategoriesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task CourseCategoriesService_GetCourseCategoriesAsync_Expected()
        {
            var expectedResults = _courseCategoriesCollection.FirstOrDefault(c => c.Guid == courseCategoriesGuid);
            var actualResult =
                (await _courseCategoriesService.GetCourseCategoriesAsync(true)).FirstOrDefault(x => x.Id == courseCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CourseCategoriesService_GetCourseCategoriesByGuidAsync_Empty()
        {
            await _courseCategoriesService.GetCourseCategoriesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CourseCategoriesService_GetCourseCategoriesByGuidAsync_Null()
        {
            await _courseCategoriesService.GetCourseCategoriesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task CourseCategoriesService_GetCourseCategoriesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetCourseTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _courseCategoriesService.GetCourseCategoriesByGuidAsync("99");
        }

        [TestMethod]
        public async Task CourseCategoriesService_GetCourseCategoriesByGuidAsync_Expected()
        {
            var expectedResults =
                _courseCategoriesCollection.First(c => c.Guid == courseCategoriesGuid);
            var actualResult =
                await _courseCategoriesService.GetCourseCategoriesByGuidAsync(courseCategoriesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task CourseCategoriesService_GetCourseCategoriesByGuidAsync_Properties()
        {
            var result =
                await _courseCategoriesService.GetCourseCategoriesByGuidAsync(courseCategoriesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}