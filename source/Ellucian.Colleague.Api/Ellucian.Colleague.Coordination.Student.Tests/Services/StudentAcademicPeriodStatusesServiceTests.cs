//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
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
    public class StudentAcademicPeriodStatusesServiceTests
    {
        private const string studentAcademicPeriodStatusesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string studentAcademicPeriodStatusesCode = "P";
        private ICollection<Domain.Student.Entities.StudentStatus> _studentAcademicPeriodStatusesCollection;
        private StudentAcademicPeriodStatusesService _studentAcademicPeriodStatusesService;

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


            _studentAcademicPeriodStatusesCollection = new List<Domain.Student.Entities.StudentStatus>()
                {
                    new Domain.Student.Entities.StudentStatus("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "P", "Preregistered", "P"),
                    new Domain.Student.Entities.StudentStatus("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "R", "Registered", "R"),
                    new Domain.Student.Entities.StudentStatus("d2253ac7-9931-4560-b42f-1fccd43c952e", "T", "Transcripted", "T"),
                    new Domain.Student.Entities.StudentStatus("e2253ac7-9931-4560-b42f-1fccd43c952e", "W", "Withdrawn", "W"),
                    new Domain.Student.Entities.StudentStatus("f2253ac7-9931-4560-b42f-1fccd43c952e", "X", "Deleted", "X"),
                };


            _referenceRepositoryMock.Setup(repo => repo.GetStudentStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_studentAcademicPeriodStatusesCollection);

            _studentAcademicPeriodStatusesService = new StudentAcademicPeriodStatusesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentAcademicPeriodStatusesService = null;
            _studentAcademicPeriodStatusesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesAsync()
        {
            var results = await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesAsync(true);
            Assert.IsTrue(results is IEnumerable<StudentAcademicPeriodStatuses>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesAsync_Count()
        {
            var results = await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesAsync(true);
            Assert.AreEqual(5, results.Count());
        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesAsync_Properties()
        {
            var result =
                (await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesAsync(true)).FirstOrDefault(x => x.Code == studentAcademicPeriodStatusesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesAsync_Expected()
        {
            var expectedResults = _studentAcademicPeriodStatusesCollection.FirstOrDefault(c => c.Guid == studentAcademicPeriodStatusesGuid);
            var actualResult =
                (await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesAsync(true)).FirstOrDefault(x => x.Id == studentAcademicPeriodStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesByGuidAsync_Empty()
        {
            await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesByGuidAsync_Null()
        {
            await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetStudentStatusesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesByGuidAsync("99");
        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesByGuidAsync_Expected()
        {
            var expectedResults =
                _studentAcademicPeriodStatusesCollection.First(c => c.Guid == studentAcademicPeriodStatusesGuid);
            var actualResult =
                await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesByGuidAsync(studentAcademicPeriodStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task StudentAcademicPeriodStatusesService_GetStudentAcademicPeriodStatusesByGuidAsync_Properties()
        {
            var result =
                await _studentAcademicPeriodStatusesService.GetStudentAcademicPeriodStatusesByGuidAsync(studentAcademicPeriodStatusesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}