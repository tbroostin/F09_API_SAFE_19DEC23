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
    public class SectionTitleTypesServiceTests
    {
        private const string sectionTitleTypesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string sectionTitleTypesCode = "AT";
        private ICollection<Domain.Student.Entities.SectionTitleType> _sectionTitleTypesCollection;
        private SectionTitleTypesService _sectionTitleTypesService;

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


            _sectionTitleTypesCollection = new List<Domain.Student.Entities.SectionTitleType>()
                {
                    new Domain.Student.Entities.SectionTitleType("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SectionTitleType("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SectionTitleType("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetSectionTitleTypesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_sectionTitleTypesCollection);

            _sectionTitleTypesService = new SectionTitleTypesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _sectionTitleTypesService = null;
            _sectionTitleTypesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task SectionTitleTypesService_GetSectionTitleTypesAsync()
        {
            var results = await _sectionTitleTypesService.GetSectionTitleTypesAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.SectionTitleType>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task SectionTitleTypesService_GetSectionTitleTypesAsync_Count()
        {
            var results = await _sectionTitleTypesService.GetSectionTitleTypesAsync(true);
            Assert.AreEqual(3, results.Count());
        }


         [TestMethod]
        public async Task SectionTitleTypesService_GetSectionTitleTypesAsync_Properties()
        {
            var result =
                (await _sectionTitleTypesService.GetSectionTitleTypesAsync(true)).FirstOrDefault(x => x.Code == sectionTitleTypesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }
    

        [TestMethod]
        public async Task SectionTitleTypesService_GetSectionTitleTypesAsync_Expected()
        {
            var expectedResults = _sectionTitleTypesCollection.FirstOrDefault(c => c.Guid == sectionTitleTypesGuid);
            var actualResult =
                (await _sectionTitleTypesService.GetSectionTitleTypesAsync(true)).FirstOrDefault(x => x.Id == sectionTitleTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SectionTitleTypesService_GetSectionTitleTypeByGuidAsync_Empty()
        {
            await _sectionTitleTypesService.GetSectionTitleTypeByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SectionTitleTypesService_GetSectionTitleTypeByGuidAsync_Null()
        {
            await _sectionTitleTypesService.GetSectionTitleTypeByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SectionTitleTypesService_GetSectionTitleTypeByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetSectionTitleTypesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _sectionTitleTypesService.GetSectionTitleTypeByGuidAsync("99");
        }

        [TestMethod]
        public async Task SectionTitleTypesService_GetSectionTitleTypeByGuidAsync_Expected()
        {
            var expectedResults =
                _sectionTitleTypesCollection.First(c => c.Guid == sectionTitleTypesGuid);
            var actualResult =
                await _sectionTitleTypesService.GetSectionTitleTypeByGuidAsync(sectionTitleTypesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task SectionTitleTypesService_GetSectionTitleTypeByGuidAsync_Properties()
        {
            var result =
                await _sectionTitleTypesService.GetSectionTitleTypeByGuidAsync(sectionTitleTypesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}