//Copyright 2017 Ellucian Company L.P. and its affiliates.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;

using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class InstructorTenureTypesServiceTests
    {
        private const string instructorTenureTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string instructorTenureTypesCode = "AT";
        private ICollection<TenureTypes> _instructorTenureTypesCollection;
        private InstructorTenureTypesService _instructorTenureTypesService;
        private Mock<ILogger> _loggerMock;
        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private ICurrentUserFactory currentUserFactory;
        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            
            _instructorTenureTypesCollection = new List<TenureTypes>()
                {
                    new TenureTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new TenureTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new TenureTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetTenureTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_instructorTenureTypesCollection);

            _instructorTenureTypesService = new InstructorTenureTypesService(_referenceRepositoryMock.Object, _adapterRegistryMock.Object, currentUserFactory, _roleRepositoryMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _instructorTenureTypesService = null;
            _instructorTenureTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesAsync()
        {
            var results = await _instructorTenureTypesService.GetInstructorTenureTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<InstructorTenureTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesAsync_Count()
        {
            var results = await _instructorTenureTypesService.GetInstructorTenureTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesAsync_Properties()
        {
            var result =
                (await _instructorTenureTypesService.GetInstructorTenureTypesAsync(true)).FirstOrDefault(x => x.Code == instructorTenureTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesAsync_Expected()
        {
            var expectedResults = _instructorTenureTypesCollection.FirstOrDefault(c => c.Guid == instructorTenureTypesGuid);
            var actualResult =
                (await _instructorTenureTypesService.GetInstructorTenureTypesAsync(true)).FirstOrDefault(x => x.Id == instructorTenureTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesByGuidAsync_Empty()
        {
            await _instructorTenureTypesService.GetInstructorTenureTypesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesByGuidAsync_Null()
        {
            await _instructorTenureTypesService.GetInstructorTenureTypesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetTenureTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _instructorTenureTypesService.GetInstructorTenureTypesByGuidAsync("99");
        }

        [TestMethod]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesByGuidAsync_Expected()
        {
            var expectedResults =
                _instructorTenureTypesCollection.First(c => c.Guid == instructorTenureTypesGuid);
            var actualResult =
                await _instructorTenureTypesService.GetInstructorTenureTypesByGuidAsync(instructorTenureTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task InstructorTenureTypesService_GetInstructorTenureTypesByGuidAsync_Properties()
        {
            var result =
                await _instructorTenureTypesService.GetInstructorTenureTypesByGuidAsync(instructorTenureTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}