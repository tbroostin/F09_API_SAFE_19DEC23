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
    public class ChargeAssessmentMethodsServiceTests
    {
        private const string chargeAssessmentMethodsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string chargeAssessmentMethodsCode = "AT";
        private ICollection<ChargeAssessmentMethod> _chargeAssessmentMethodsCollection;
        private ChargeAssessmentMethodsService _chargeAssessmentMethodsService;
        
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
           

            _chargeAssessmentMethodsCollection = new List<ChargeAssessmentMethod>()
                {
                    new ChargeAssessmentMethod("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new ChargeAssessmentMethod("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new ChargeAssessmentMethod("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };

           
            _referenceRepositoryMock.Setup(repo => repo.GetChargeAssessmentMethodsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_chargeAssessmentMethodsCollection);

            _chargeAssessmentMethodsService = new ChargeAssessmentMethodsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _chargeAssessmentMethodsService = null;
            _chargeAssessmentMethodsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock= null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsAsync()
        {
            var results = await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsAsync(true);
            Assert.IsTrue(results is IEnumerable<ChargeAssessmentMethods>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsAsync_Count()
        {
            var results = await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsAsync_Properties()
        {
            var result =
                (await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsAsync(true)).FirstOrDefault(x => x.Code == chargeAssessmentMethodsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
           
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsAsync_Expected()
        {
            var expectedResults = _chargeAssessmentMethodsCollection.FirstOrDefault(c => c.Guid == chargeAssessmentMethodsGuid);
            var actualResult =
                (await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsAsync(true)).FirstOrDefault(x => x.Id == chargeAssessmentMethodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsByGuidAsync_Empty()
        {
            await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsByGuidAsync_Null()
        {
            await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof (KeyNotFoundException))]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetChargeAssessmentMethodsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsByGuidAsync("99");
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsByGuidAsync_Expected()
        {
            var expectedResults =
                _chargeAssessmentMethodsCollection.First(c => c.Guid == chargeAssessmentMethodsGuid);
            var actualResult =
                await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsByGuidAsync(chargeAssessmentMethodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);
            
        }

        [TestMethod]
        public async Task ChargeAssessmentMethodsService_GetChargeAssessmentMethodsByGuidAsync_Properties()
        {
            var result =
                await _chargeAssessmentMethodsService.GetChargeAssessmentMethodsByGuidAsync(chargeAssessmentMethodsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);
            
        }
    }
}