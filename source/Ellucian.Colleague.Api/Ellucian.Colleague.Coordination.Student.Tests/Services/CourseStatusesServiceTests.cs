//Copyright 2018 Ellucian Company L.P. and its affiliates.


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
    public class CourseStatusesServiceTests
    {
        private const string courseStatusesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string courseStatusesCode = "AT";
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.CourseStatuses> _courseStatusesCollection;
        private CourseStatusesService _courseStatusesService;
        
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
       

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
           _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
           

            _courseStatusesCollection = new List<Ellucian.Colleague.Domain.Student.Entities.CourseStatuses>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.CourseStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic") { Status = CourseStatus.Active },
                    new Ellucian.Colleague.Domain.Student.Entities.CourseStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic") { Status = CourseStatus.Active },
                    new Ellucian.Colleague.Domain.Student.Entities.CourseStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural") { Status = CourseStatus.Active }
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetCourseStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_courseStatusesCollection);

            _courseStatusesService = new CourseStatusesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _courseStatusesService = null;
            _courseStatusesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task CourseStatusesService_GetCourseStatusesAsync()
        {
            var results = await _courseStatusesService.GetCourseStatusesAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.CourseStatuses>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task CourseStatusesService_GetCourseStatusesAsync_Count()
        {
            var results = await _courseStatusesService.GetCourseStatusesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task CourseStatusesService_GetCourseStatusesAsync_Properties()
        {
            var result =
                (await _courseStatusesService.GetCourseStatusesAsync(true)).FirstOrDefault(x => x.Code == courseStatusesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
        }

        [TestMethod]
        public async Task CourseStatusesService_GetCourseStatusesAsync_Expected()
        {
            var expectedResults = _courseStatusesCollection.FirstOrDefault(c => c.Guid == courseStatusesGuid);
            var actualResult =
                (await _courseStatusesService.GetCourseStatusesAsync(true)).FirstOrDefault(x => x.Id == courseStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseStatusesService_GetCourseStatusesByGuidAsync_Empty()
        {
            await _courseStatusesService.GetCourseStatusesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseStatusesService_GetCourseStatusesByGuidAsync_Null()
        {
            await _courseStatusesService.GetCourseStatusesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task CourseStatusesService_GetCourseStatusesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetCourseStatusesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _courseStatusesService.GetCourseStatusesByGuidAsync("99");
        }

        [TestMethod]
        public async Task CourseStatusesService_GetCourseStatusesByGuidAsync_Expected()
        {
            var expectedResults =
                _courseStatusesCollection.First(c => c.Guid == courseStatusesGuid);
            var actualResult =
                await _courseStatusesService.GetCourseStatusesByGuidAsync(courseStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
        }

        [TestMethod]
        public async Task CourseStatusesService_GetCourseStatusesByGuidAsync_Properties()
        {
            var result =
                await _courseStatusesService.GetCourseStatusesByGuidAsync(courseStatusesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
        }
    }
}