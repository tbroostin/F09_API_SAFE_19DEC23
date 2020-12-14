/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class PayCycleServiceTests : HumanResourcesServiceTestsSetup
    {
        public Mock<IPayCycleRepository> payCycleRepositoryMock;

        public Mock<IHumanResourcesReferenceDataRepository> referenceRepositoryMock;

        public Mock<IConfigurationRepository> configurationRepositoryMock;

        public TestPayCycleRepository testPayCycleRepository;

        public ITypeAdapter<Domain.HumanResources.Entities.PayCycle, Dtos.HumanResources.PayCycle> payCycleEntityToDtoAdapter;

        public PayCycleService actualService
        {
            get
            {
                return new PayCycleService(
                    referenceRepositoryMock.Object,
                    payCycleRepositoryMock.Object,
                    adapterRegistryMock.Object,
                    employeeCurrentUserFactory,
                    roleRepositoryMock.Object,
                    configurationRepositoryMock.Object,
                    loggerMock.Object);
            }
        }

        public FunctionEqualityComparer<Dtos.HumanResources.PayCycle> payCycleDtoComparer;

        public void PersonEmploymentStatusServiceTestsInitialize()
        {
            MockInitialize();

            payCycleRepositoryMock = new Mock<IPayCycleRepository>();
            testPayCycleRepository = new TestPayCycleRepository();

            payCycleEntityToDtoAdapter = new AutoMapperAdapter<Domain.HumanResources.Entities.PayCycle, Dtos.HumanResources.PayCycle>(adapterRegistryMock.Object, loggerMock.Object);

            payCycleRepositoryMock.Setup(r => r.GetPayCyclesAsync(It.IsAny<DateTime?>()))
                .Returns<IEnumerable<string>>((p) => testPayCycleRepository.GetPayCyclesAsync());

            adapterRegistryMock.Setup(r => r.GetAdapter<Domain.HumanResources.Entities.PayCycle, Dtos.HumanResources.PayCycle>())
                .Returns(payCycleEntityToDtoAdapter);

            payCycleDtoComparer = new FunctionEqualityComparer<Dtos.HumanResources.PayCycle>(
                (p1, p2) =>
                    p1.Id == p2.Id &&
                    p1.AnnualPayFrequency == p2.AnnualPayFrequency &&
                    p1.Description == p2.Description &&
                    p1.PayClassIds == p2.PayClassIds &&
                    p1.PayPeriods == p2.PayPeriods,
                (p) => p.Id.GetHashCode());
        }

        [TestClass]
        public class GetPersonEmploymentStatussTests : PersonEmploymentStatusServiceTests
        {
            [TestInitialize]
            public void Initialize()
            {
                PersonEmploymentStatusServiceTestsInitialize();
            }

            [TestMethod]
            public async Task RepositoryCalledWithCurrentUserIdTest()
            {
                await actualService.GetPersonEmploymentStatusesAsync();
                personEmploymentStatusRepositoryMock.Verify(r =>
                    r.GetPersonEmploymentStatusesAsync(It.Is<IEnumerable<string>>(list =>
                        list.Count() == 1 && list.ElementAt(0) == employeeCurrentUserFactory.CurrentUser.PersonId), null));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task RepositoryReturnsNullTest()
            {
                personEmploymentStatusRepositoryMock.Setup(r => r.GetPersonEmploymentStatusesAsync(It.IsAny<IEnumerable<string>>(), null))
                    .ReturnsAsync(null);

                try
                {
                    await actualService.GetPersonEmploymentStatusesAsync();
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await testPersonEmploymentStatusRepository.GetPersonEmploymentStatusesAsync(new List<string>() { employeeCurrentUserFactory.CurrentUser.PersonId }))
                    .Select(ppEntity => personEmploymentStatusEntityToDtoAdapter.MapToType(ppEntity));

                var actual = await actualService.GetPersonEmploymentStatusesAsync();

                CollectionAssert.AreEqual(expected.ToArray(), actual.ToArray(), personEmploymentStatusDtoComparer);
            }
        }
    }

    [TestClass]
    public class PayCycles2ServiceTests
    {
        private const string payCyclesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string payCyclesCode = "AT";
        private ICollection<PayCycle2> _payCyclesCollection;
        private PayCycleService _payCyclesService;

        private Mock<IHumanResourcesReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IPayCycleRepository> _payCycleRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
            _payCycleRepositoryMock = new Mock<IPayCycleRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();


            _payCyclesCollection = new List<PayCycle2>()
                {
                    new PayCycle2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new PayCycle2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new PayCycle2("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };


            _referenceRepositoryMock.Setup(repo => repo.GetPayCyclesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_payCyclesCollection);

            _payCyclesService = new PayCycleService(_referenceRepositoryMock.Object, _payCycleRepositoryMock.Object,
                _adapterRegistryMock.Object, _currentUserFactoryMock.Object,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _payCyclesService = null;
            _payCyclesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task PayCyclesService_GetPayCyclesAsync()
        {
            var results = await _payCyclesService.GetPayCyclesAsync(true);
            Assert.IsTrue(results is IEnumerable<PayCycles>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task PayCyclesService_GetPayCyclesAsync_Count()
        {
            var results = await _payCyclesService.GetPayCyclesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task PayCyclesService_GetPayCyclesAsync_Properties()
        {
            var result =
                (await _payCyclesService.GetPayCyclesAsync(true)).FirstOrDefault(x => x.Code == payCyclesCode);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);

        }

        [TestMethod]
        public async Task PayCyclesService_GetPayCyclesAsync_Expected()
        {
            var expectedResults = _payCyclesCollection.FirstOrDefault(c => c.Guid == payCyclesGuid);
            var actualResult =
                (await _payCyclesService.GetPayCyclesAsync(true)).FirstOrDefault(x => x.Id == payCyclesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayCyclesService_GetPayCyclesByGuidAsync_Empty()
        {
            await _payCyclesService.GetPayCyclesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayCyclesService_GetPayCyclesByGuidAsync_Null()
        {
            await _payCyclesService.GetPayCyclesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task PayCyclesService_GetPayCyclesByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetPayCyclesAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _payCyclesService.GetPayCyclesByGuidAsync("99");
        }

        [TestMethod]
        public async Task PayCyclesService_GetPayCyclesByGuidAsync_Expected()
        {
            var expectedResults =
                _payCyclesCollection.First(c => c.Guid == payCyclesGuid);
            var actualResult =
                await _payCyclesService.GetPayCyclesByGuidAsync(payCyclesGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Description, actualResult.Title);
            Assert.AreEqual(expectedResults.Code, actualResult.Code);

        }

        [TestMethod]
        public async Task PayCyclesService_GetPayCyclesByGuidAsync_Properties()
        {
            var result =
                await _payCyclesService.GetPayCyclesByGuidAsync(payCyclesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Code);
            Assert.IsNull(result.Description);
            Assert.IsNotNull(result.Title);

        }
    }
}
