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
    public class CourseTopicsServiceTests
    {
        private const string courseTopicsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string courseTopicsCode = "AT";
        private ICollection<CourseTopic> _courseTopicsCollection;
        private CourseTopicsService _courseTopicsService;
        
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
           

            _courseTopicsCollection = new List<CourseTopic>()
                {
                    new CourseTopic("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new CourseTopic("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new CourseTopic("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetCourseTopicsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_courseTopicsCollection);

            _courseTopicsService = new CourseTopicsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _courseTopicsService = null;
            _courseTopicsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task CourseTopicsService_GetCourseTopicsAsync()
        {
            var results = await _courseTopicsService.GetCourseTopicsAsync(true);
            Assert.IsTrue(results is IEnumerable<CourseTopics>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task CourseTopicsService_GetCourseTopicsAsync_Count()
        {
            var results = await _courseTopicsService.GetCourseTopicsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task CourseTopicsService_GetCourseTopicsAsync_Properties()
        {
            var result =
                (await _courseTopicsService.GetCourseTopicsAsync(true)).FirstOrDefault(x => x.Code == courseTopicsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task CourseTopicsService_GetCourseTopicsAsync_Expected()
        {
            var expectedResults = _courseTopicsCollection.FirstOrDefault(c => c.Guid == courseTopicsGuid);
            var actualResult =
                (await _courseTopicsService.GetCourseTopicsAsync(true)).FirstOrDefault(x => x.Id == courseTopicsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseTopicsService_GetCourseTopicsByGuidAsync_Empty()
        {
            await _courseTopicsService.GetCourseTopicsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseTopicsService_GetCourseTopicsByGuidAsync_Null()
        {
            await _courseTopicsService.GetCourseTopicsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseTopicsService_GetCourseTopicsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetCourseTopicsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _courseTopicsService.GetCourseTopicsByGuidAsync("99");
        }

        [TestMethod]
        public async Task CourseTopicsService_GetCourseTopicsByGuidAsync_Expected()
        {
            var expectedResults =
                _courseTopicsCollection.First(c => c.Guid == courseTopicsGuid);
            var actualResult =
                await _courseTopicsService.GetCourseTopicsByGuidAsync(courseTopicsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task CourseTopicsService_GetCourseTopicsByGuidAsync_Properties()
        {
            var result =
                await _courseTopicsService.GetCourseTopicsByGuidAsync(courseTopicsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}