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
    public class BillingOverrideReasonsServiceTests
    {
        private const string billingOverrideReasonsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string billingOverrideReasonsCode = "AT";
        private ICollection<Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons> _billingOverrideReasonsCollection;
        private BillingOverrideReasonsService _billingOverrideReasonsService;
        private Mock<ILogger> _loggerMock;
        private Mock<IStudentReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private IConfigurationRepository baseConfigurationRepository;
        private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
            baseConfigurationRepository = baseConfigurationRepositoryMock.Object;


            _billingOverrideReasonsCollection = new List<Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Ellucian.Colleague.Domain.Student.Entities.BillingOverrideReasons("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetBillingOverrideReasonsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_billingOverrideReasonsCollection);

            _billingOverrideReasonsService = new BillingOverrideReasonsService(_referenceRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _loggerMock.Object, baseConfigurationRepository);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _billingOverrideReasonsService = null;
            _billingOverrideReasonsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
        }

        [TestMethod]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsAsync()
        {
            var results = await _billingOverrideReasonsService.GetBillingOverrideReasonsAsync(true);
            Assert.IsTrue(results is IEnumerable<Ellucian.Colleague.Dtos.BillingOverrideReasons>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsAsync_Count()
        {
            var results = await _billingOverrideReasonsService.GetBillingOverrideReasonsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsAsync_Properties()
        {
            var result =
                (await _billingOverrideReasonsService.GetBillingOverrideReasonsAsync(true)).FirstOrDefault(x => x.Code == billingOverrideReasonsCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsAsync_Expected()
        {
            var expectedResults = _billingOverrideReasonsCollection.FirstOrDefault(c => c.Guid == billingOverrideReasonsGuid);
            var actualResult =
                (await _billingOverrideReasonsService.GetBillingOverrideReasonsAsync(true)).FirstOrDefault(x => x.Id == billingOverrideReasonsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsByGuidAsync_Empty()
        {
            await _billingOverrideReasonsService.GetBillingOverrideReasonsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsByGuidAsync_Null()
        {
            await _billingOverrideReasonsService.GetBillingOverrideReasonsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetBillingOverrideReasonsAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _billingOverrideReasonsService.GetBillingOverrideReasonsByGuidAsync("99");
        }

        [TestMethod]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsByGuidAsync_Expected()
        {
            var expectedResults =
                _billingOverrideReasonsCollection.First(c => c.Guid == billingOverrideReasonsGuid);
            var actualResult =
                await _billingOverrideReasonsService.GetBillingOverrideReasonsByGuidAsync(billingOverrideReasonsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task BillingOverrideReasonsService_GetBillingOverrideReasonsByGuidAsync_Properties()
        {
            var result =
                await _billingOverrideReasonsService.GetBillingOverrideReasonsByGuidAsync(billingOverrideReasonsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}