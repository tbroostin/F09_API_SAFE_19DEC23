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
    public class SectionStatusesServiceTests
    {
        private const string sectionStatusesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string sectionStatusesCode = "AT";
        private ICollection<Domain.Student.Entities.SectionStatuses> _sectionStatusesCollection;
        private SectionStatusesService _sectionStatusesService;
        
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


            _sectionStatusesCollection = new List<Domain.Student.Entities.SectionStatuses>()
                {
                    new Domain.Student.Entities.SectionStatuses("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.SectionStatuses("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.SectionStatuses("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetSectionStatusesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_sectionStatusesCollection);

            _sectionStatusesService = new SectionStatusesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _sectionStatusesService = null;
            _sectionStatusesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task SectionStatusesService_GetSectionStatusesAsync()
        {
            var results = await _sectionStatusesService.GetSectionStatusesAsync(true);
            Assert.IsTrue(results is IEnumerable<Dtos.SectionStatuses>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task SectionStatusesService_GetSectionStatusesAsync_Count()
        {
            var results = await _sectionStatusesService.GetSectionStatusesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task SectionStatusesService_GetSectionStatusesAsync_Properties()
        {
            var result =
                (await _sectionStatusesService.GetSectionStatusesAsync(true)).FirstOrDefault(x => x.Code == sectionStatusesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task SectionStatusesService_GetSectionStatusesAsync_Expected()
        {
            var expectedResults = _sectionStatusesCollection.FirstOrDefault(c => c.Guid == sectionStatusesGuid);
            var actualResult =
                (await _sectionStatusesService.GetSectionStatusesAsync(true)).FirstOrDefault(x => x.Id == sectionStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task SectionStatusesService_GetSectionStatusesByGuidAsync_Empty()
        {
            await _sectionStatusesService.GetSectionStatusesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task SectionStatusesService_GetSectionStatusesByGuidAsync_Null()
        {
            await _sectionStatusesService.GetSectionStatusesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task SectionStatusesService_GetSectionStatusesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetSectionStatusesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _sectionStatusesService.GetSectionStatusesByGuidAsync("99");
        }

        [TestMethod]
        public async Task SectionStatusesService_GetSectionStatusesByGuidAsync_Expected()
        {
            var expectedResults =
                _sectionStatusesCollection.First(c => c.Guid == sectionStatusesGuid);
            var actualResult =
                await _sectionStatusesService.GetSectionStatusesByGuidAsync(sectionStatusesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task SectionStatusesService_GetSectionStatusesByGuidAsync_Properties()
        {
            var result =
                await _sectionStatusesService.GetSectionStatusesByGuidAsync(sectionStatusesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}