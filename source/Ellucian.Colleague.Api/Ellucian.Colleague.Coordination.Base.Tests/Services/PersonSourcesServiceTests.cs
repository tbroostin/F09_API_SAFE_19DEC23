﻿//Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class PersonSourcesServiceTests
    {
        private const string personSourcesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string personSourcesCode = "AT";
        private ICollection<PersonOriginCodes> _personSourcesCollection;
        private PersonSourcesService _personSourcesService;

        private Mock<IReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _personSourcesCollection = new List<PersonOriginCodes>()
                {
                    new PersonOriginCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new PersonOriginCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new PersonOriginCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetPersonOriginCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_personSourcesCollection);

            _personSourcesService = new PersonSourcesService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _personSourcesService = null;
            _personSourcesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PersonSourcesService_GetPersonSourcesAsync()
        {
            var results = await _personSourcesService.GetPersonSourcesAsync(true);
            Assert.IsTrue(results is IEnumerable<PersonSources>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PersonSourcesService_GetPersonSourcesAsync_Count()
        {
            var results = await _personSourcesService.GetPersonSourcesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task PersonSourcesService_GetPersonSourcesAsync_Properties()
        {
            var result =
                (await _personSourcesService.GetPersonSourcesAsync(true)).FirstOrDefault(x => x.Code == personSourcesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task PersonSourcesService_GetPersonSourcesAsync_Expected()
        {
            var expectedResults = _personSourcesCollection.FirstOrDefault(c => c.Guid == personSourcesGuid);
            var actualResult =
                (await _personSourcesService.GetPersonSourcesAsync(true)).FirstOrDefault(x => x.Id == personSourcesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonSourcesService_GetPersonSourcesByGuidAsync_Empty()
        {
            await _personSourcesService.GetPersonSourcesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonSourcesService_GetPersonSourcesByGuidAsync_Null()
        {
            await _personSourcesService.GetPersonSourcesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PersonSourcesService_GetPersonSourcesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetPersonOriginCodesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _personSourcesService.GetPersonSourcesByGuidAsync("99");
        }

        [TestMethod]
        public async Task PersonSourcesService_GetPersonSourcesByGuidAsync_Expected()
        {
            var expectedResults =
                _personSourcesCollection.First(c => c.Guid == personSourcesGuid);
            var actualResult =
                await _personSourcesService.GetPersonSourcesByGuidAsync(personSourcesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task PersonSourcesService_GetPersonSourcesByGuidAsync_Properties()
        {
            var result =
                await _personSourcesService.GetPersonSourcesByGuidAsync(personSourcesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}