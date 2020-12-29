// Copyright 2016-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class SourceServiceTests
    {
        private const string SourceGuid = "c5bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
        private const string SourceCode = "HS";
        private ICollection<TestSource> _testSourceCollection;
        private ICollection<RemarkCode> _remarkCodeCollection;
        private ICollection<AddressChangeSource> _addressChangeSourceCollection;
        private SourceService _sourceService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private IEnumerable<Domain.Base.Entities.SourceContext> _sourceContextsCollection;

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ICurrentUserFactory _currentStudentFactory;
        private IConfigurationRepository _configurationRepository;
        private Mock<IConfigurationRepository> _configurationRepositoryMock;
        private Mock<IRoleRepository> _roleRepoMock;
        private IRoleRepository _roleRepo;

        [TestInitialize]
        public async void Initialize()
        {
            _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();

            _testSourceCollection = (await new TestStudentReferenceDataRepository().GetTestSourcesAsync(false)).ToList();
            _addressChangeSourceCollection = new TestAddressChangeSourceRepository().GetAddressChangeSource().ToList();
            _remarkCodeCollection = new TestRemarkCodeRepository().GetRemarkCode().ToList();
            _sourceContextsCollection = new TestSourceContextRepository().GetSourceContexts();

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _configurationRepositoryMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepositoryMock.Object;
            _roleRepoMock = new Mock<IRoleRepository>();
            _roleRepo = _roleRepoMock.Object;


            _studentReferenceRepositoryMock.Setup(repo => repo.GetTestSourcesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_testSourceCollection);


            _referenceRepositoryMock.Setup(repo => repo.GetAddressChangeSourcesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_addressChangeSourceCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetRemarkCodesAsync(It.IsAny<bool>()))
               .ReturnsAsync(_remarkCodeCollection);

            _referenceRepositoryMock.Setup(repo => repo.GetSourceContextsAsync(It.IsAny<bool>()))
             .ReturnsAsync(_sourceContextsCollection);

            _currentStudentFactory = new StudentServiceTests.CurrentUserSetup.StudentUserFactory();            
            _sourceService = new SourceService(_adapterRegistry, _studentReferenceRepositoryMock.Object, _referenceRepositoryMock.Object, _currentStudentFactory, _configurationRepository, _roleRepo, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _sourceService = null;
            _testSourceCollection = null;
            _studentReferenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task SourceService_GetSourcesAsync()
        {
            var results = await _sourceService.GetSourcesAsync(false);
            Assert.IsTrue(results is IEnumerable<Dtos.Source>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task SourceService_GetSourcesAsync_Count()
        {
            GuidLookupResult sourceContext = new GuidLookupResult() { PrimaryKey = "APPL.TEST.SOURCES" };

            _referenceRepositoryMock.Setup(repo => repo.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(sourceContext);

            var results = await _sourceService.GetSourcesAsync(false);
            Assert.AreEqual(_testSourceCollection.Count + _remarkCodeCollection.Count + _addressChangeSourceCollection.Count, results.Count());
        }

        [TestMethod]
        public async Task SourceService_GetSourcesAsync_Properties()
        {
            var result =
                (await _sourceService.GetSourcesAsync(false)).FirstOrDefault(x => x.Code == SourceCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Status);
            Assert.IsNotNull(result.Contexts);
        }

        [TestMethod]
        public async Task SourceService_GetSourcesAsync_Expected()
        {
            var expectedResults = _testSourceCollection.FirstOrDefault(c => c.Guid == SourceGuid);
            var actualResult =
                (await _sourceService.GetSourcesAsync(false)).FirstOrDefault(x => x.Id == SourceGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task SourceService_GetSourceByIdAsync_Empty()
        {
            GuidLookupResult sourceContext = new GuidLookupResult() { PrimaryKey = "APPL.TEST.SOURCES" };

            _referenceRepositoryMock.Setup(repo => repo.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(sourceContext); 
            
            await _sourceService.GetSourceByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SourceService_GetSourceByGuidAsync_Null()
        {
            await _sourceService.GetSourceByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task SourceService_GetSourceByGuidAsync_InvalidId()
        {
            _studentReferenceRepositoryMock.Setup(repo => repo.GetTestSourcesAsync(false))
                .Throws<InvalidOperationException>();

            await _sourceService.GetSourceByIdAsync("99");
        }

        [TestMethod]
        public async Task SourceService_GetSourceByIdAsync_Tests_Expected()
        {
            var testSourceGuid = "c5bcb3a0-2e8d-4643-bd17-ba93f36e8f09";

            var expectedResults =
                _testSourceCollection.First(c => c.Guid == testSourceGuid);

            GuidLookupResult sourceContext = new GuidLookupResult() { PrimaryKey = "APPL.TEST.SOURCES" };

            _referenceRepositoryMock.Setup(repo => repo.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(sourceContext);

            var actualResult =
                await _sourceService.GetSourceByIdAsync(testSourceGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task SourceService_GetSourceByIdAsync_Address_Expected()
        {
            var addressGuid = "b831e686-7692-4012-8da5-b1b5d44389b4";

            var expectedResults =
               _addressChangeSourceCollection.FirstOrDefault(c => c.Guid == addressGuid);

            GuidLookupResult sourceContext = new GuidLookupResult() { PrimaryKey = "ADDRESS.CHANGE.SOURCES" };

            _referenceRepositoryMock.Setup(repo => repo.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(sourceContext);

            var actualResult =
                await _sourceService.GetSourceByIdAsync(addressGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
        }

        [TestMethod]
        public async Task SourceService_GetSourceByIdAsync_Remarks_Expected()
        {
            var remarkCodeGuid = "m330e686-7692-4012-8da5-b1b5d44389b4";

            var expectedResults =
                _remarkCodeCollection.FirstOrDefault(c => c.Guid == remarkCodeGuid);

            GuidLookupResult sourceContext = new GuidLookupResult() { PrimaryKey = "REMARK.CODES" };

            _referenceRepositoryMock.Setup(repo => repo.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(sourceContext);

            var actualResult =
                await _sourceService.GetSourceByIdAsync(remarkCodeGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
        }

        [TestMethod]
        public async Task SourceService_GetSourceByIdAsync_Properties()
        {
            GuidLookupResult sourceContext = new GuidLookupResult() { PrimaryKey = "APPL.TEST.SOURCES" };

            _referenceRepositoryMock.Setup(repo => repo.GetGuidLookupResultFromGuidAsync(It.IsAny<string>()))
                    .ReturnsAsync(sourceContext);

            var result =
                await _sourceService.GetSourceByIdAsync(SourceGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Status);
            Assert.IsNotNull(result.Contexts);
        }
    }
}