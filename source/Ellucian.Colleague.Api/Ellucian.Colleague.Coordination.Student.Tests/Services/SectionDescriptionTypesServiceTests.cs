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
    public class SectionDescriptionTypesServiceTests
    {
        private const string sectionDescriptionTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string sectionDescriptionTypesCode = "AT";
        private ICollection<Domain.Student.Entities.SectionDescriptionType> _sectionDescriptionTypesCollection;
        private SectionDescriptionTypesService _sectionDescriptionTypesService;

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


            _sectionDescriptionTypesCollection = new List<Domain.Student.Entities.SectionDescriptionType>()
                {
                    new Domain.Student.Entities.SectionDescriptionType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SectionDescriptionType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SectionDescriptionType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetSectionDescriptionTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_sectionDescriptionTypesCollection);

            _sectionDescriptionTypesService = new SectionDescriptionTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _sectionDescriptionTypesService = null;
            _sectionDescriptionTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypesAsync()
        {
            var results = await _sectionDescriptionTypesService.GetSectionDescriptionTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.SectionDescriptionTypes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypesAsync_Count()
        {
            var results = await _sectionDescriptionTypesService.GetSectionDescriptionTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }


         [TestMethod]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypesAsync_Properties()
        {
            var result =
                (await _sectionDescriptionTypesService.GetSectionDescriptionTypesAsync(true)).FirstOrDefault(x => x.Code == sectionDescriptionTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }
    

        [TestMethod]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypesAsync_Expected()
        {
            var expectedResults = _sectionDescriptionTypesCollection.FirstOrDefault(c => c.Guid == sectionDescriptionTypesGuid);
            var actualResult =
                (await _sectionDescriptionTypesService.GetSectionDescriptionTypesAsync(true)).FirstOrDefault(x => x.Id == sectionDescriptionTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypeByGuidAsync_Empty()
        {
            await _sectionDescriptionTypesService.GetSectionDescriptionTypeByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypeByGuidAsync_Null()
        {
            await _sectionDescriptionTypesService.GetSectionDescriptionTypeByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypeByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetSectionDescriptionTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _sectionDescriptionTypesService.GetSectionDescriptionTypeByGuidAsync("99");
        }

        [TestMethod]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypeByGuidAsync_Expected()
        {
            var expectedResults =
                _sectionDescriptionTypesCollection.First(c => c.Guid == sectionDescriptionTypesGuid);
            var actualResult =
                await _sectionDescriptionTypesService.GetSectionDescriptionTypeByGuidAsync(sectionDescriptionTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task SectionDescriptionTypesService_GetSectionDescriptionTypeByGuidAsync_Properties()
        {
            var result =
                await _sectionDescriptionTypesService.GetSectionDescriptionTypeByGuidAsync(sectionDescriptionTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}