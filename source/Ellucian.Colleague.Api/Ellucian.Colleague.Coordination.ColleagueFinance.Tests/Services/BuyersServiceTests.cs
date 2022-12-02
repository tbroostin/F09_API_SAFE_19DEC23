//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Web.Http.Exceptions;


namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class BuyersServiceTests
    {
        private const string buyerGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string buyerCode = "AT";
        private IEnumerable<Buyer> _buyersCollection;
        private Tuple<IEnumerable<Buyer>, int> _buyerTuple;
        private BuyersService _buyerService;
        private Mock<IBuyerRepository> _buyerRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IRoleRepository> _roleRepoMock;
        private ICurrentUserFactory _currentUserFactory;
        private Mock<IConfigurationRepository> _configRepositoryMock;
        int offset = 0;
        int limit = 2;

        [TestInitialize]
        public void Initialize()
        {
            _buyerRepositoryMock = new Mock<IBuyerRepository>();
            _loggerMock = new Mock<ILogger>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _roleRepoMock = new Mock<IRoleRepository>();
            _configRepositoryMock = new Mock<IConfigurationRepository>();
            _currentUserFactory = new CurrentUserSetup.PersonUserFactory();

            BuildData();

            _buyerService = new BuyersService(_buyerRepositoryMock.Object, _adapterRegistryMock.Object, _currentUserFactory, _roleRepoMock.Object, _loggerMock.Object,_configRepositoryMock.Object);
        }
       
        [TestCleanup]
        public void Cleanup()
        {
            _buyerRepositoryMock = null;
            _loggerMock = null;
            _adapterRegistryMock = null;
            _roleRepoMock = null;
            _currentUserFactory = null;
            _buyerRepositoryMock = null;
            _buyersCollection = null;
        }

        [TestMethod]
        public async Task BuyersService_GetBuyersAsync()
        {
            var actuals = await _buyerService.GetBuyersAsync(offset, limit, It.IsAny<bool>());
            Assert.IsNotNull(actuals);
        }

        [TestMethod]
        public async Task BuyersService_GetBuyersByGuidAsync()
        {
            var id = "3af740fe-ef2b-49f1-9f66-e7d9491e2064";
            _buyerRepositoryMock.Setup(repo => repo.GetBuyerAsync(id)).ReturnsAsync(_buyersCollection.FirstOrDefault());
            var actual = await _buyerService.GetBuyersByGuidAsync(id);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BuyersService_GetBuyersByGuidAsync_KeyNotFoundException()
        {
            var id = "3af740fe-ef2b-49f1-9f66-e7d9491e2064";
            _buyerRepositoryMock.Setup(repo => repo.GetBuyerAsync(id)).ThrowsAsync(new KeyNotFoundException());
            var actual = await _buyerService.GetBuyersByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BuyersService_GetBuyersByGuidAsync_InvalidOperationException()
        {
            var id = "3af740fe-ef2b-49f1-9f66-e7d9491e2064";
            _buyerRepositoryMock.Setup(repo => repo.GetBuyerAsync(id)).ThrowsAsync(new InvalidOperationException());
            var actual = await _buyerService.GetBuyersByGuidAsync(id);
        }

        [TestMethod]
        [ExpectedException(typeof(ColleagueWebApiException))]
        public async Task BuyersService_GetBuyersByGuidAsync_Exception()
        {
            var id = "3af740fe-ef2b-49f1-9f66-e7d9491e2064";
            var expected = _buyersCollection.FirstOrDefault();
            expected.Name = string.Empty;
            _buyerRepositoryMock.Setup(repo => repo.GetBuyerAsync(id)).ReturnsAsync(expected);
            var actual = await _buyerService.GetBuyersByGuidAsync(id);
        }

        private void BuildData()
        {
            _buyersCollection = new List<Buyer>() 
            {
                new Buyer()
                {
                    EndOn = DateTime.Today.AddDays(30),
                    Guid = "3af740fe-ef2b-49f1-9f66-e7d9491e2064",
                    Name = "First1, Last1",
                    PersonGuid = "4efe4633-b817-4fac-aada-2ca7d28de833",
                    RecordKey = "1",
                    StartOn = DateTime.Today,
                    Status = "active"
                },
                new Buyer()
                {
                    EndOn = DateTime.Today.AddDays(40),
                    Guid = "8e1d05b3-534c-4a92-9b24-adcc11afdb0a",
                    Name = "First2, Last2",
                    PersonGuid = "58e39925-e912-45df-8a6f-f2af5f5f76ac",
                    RecordKey = "2",
                    StartOn = DateTime.Today.AddDays(2),
                    Status = "inactive"
                }
            };
            _buyerTuple = new Tuple<IEnumerable<Buyer>,int>(_buyersCollection, _buyersCollection.Count());
            _buyerRepositoryMock.Setup(repo => repo.GetBuyersAsync(offset, limit, It.IsAny<bool>())).ReturnsAsync(_buyerTuple);
        }

    }
}
