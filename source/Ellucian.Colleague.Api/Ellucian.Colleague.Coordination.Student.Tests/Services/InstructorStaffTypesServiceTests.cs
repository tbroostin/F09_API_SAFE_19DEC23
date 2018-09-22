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
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class InstructorStaffTypesServiceTests
    {
        private const string instructorStaffTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string instructorStaffTypesCode = "AT";
        private ICollection<FacultyContractTypes> _facultyContractTypesCollection;
        private InstructorStaffTypesService _instructorStaffTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

            _facultyContractTypesCollection = new List<FacultyContractTypes>()
                {
                    new FacultyContractTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new FacultyContractTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new FacultyContractTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetFacultyContractTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_facultyContractTypesCollection);

            _instructorStaffTypesService = new InstructorStaffTypesService(_referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object, _roleRepositoryMock.Object,
                _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _instructorStaffTypesService = null;
            _facultyContractTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesAsync()
        {
            var results = await _instructorStaffTypesService.GetInstructorStaffTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<InstructorStaffTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesAsync_Count()
        {
            var results = await _instructorStaffTypesService.GetInstructorStaffTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesAsync_Properties()
        {
            var result =
                (await _instructorStaffTypesService.GetInstructorStaffTypesAsync(true)).FirstOrDefault(x => x.Code == instructorStaffTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesAsync_Expected()
        {
            var expectedResults = _facultyContractTypesCollection.FirstOrDefault(c => c.Guid == instructorStaffTypesGuid);
            var actualResult =
                (await _instructorStaffTypesService.GetInstructorStaffTypesAsync(true)).FirstOrDefault(x => x.Id == instructorStaffTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesByGuidAsync_Empty()
        {
            await _instructorStaffTypesService.GetInstructorStaffTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesByGuidAsync_Null()
        {
            await _instructorStaffTypesService.GetInstructorStaffTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetFacultyContractTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _instructorStaffTypesService.GetInstructorStaffTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _facultyContractTypesCollection.First(c => c.Guid == instructorStaffTypesGuid);
            var actualResult =
                await _instructorStaffTypesService.GetInstructorStaffTypesByGuidAsync(instructorStaffTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task InstructorStaffTypesService_GetInstructorStaffTypesByGuidAsync_Properties()
        {
            var result =
                await _instructorStaffTypesService.GetInstructorStaffTypesByGuidAsync(instructorStaffTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}