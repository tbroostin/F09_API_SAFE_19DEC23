//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class FiscalPeriodsServiceTests : GeneralLedgerCurrentUser
    {
        private const string fiscalPeriodsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string fiscalPeriodsCode = "2017";
        private ICollection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.FiscalPeriodsIntg> _fiscalPeriodsCollection;
        private ICollection<Ellucian.Colleague.Domain.ColleagueFinance.Entities.FiscalYear> _fiscalYearsCollection;
        private FiscalPeriodsService _fiscalPeriodsService;

        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private AccountFundsAvailableUser currentUserFactory;
        protected Domain.Entities.Role viewFiscalPeriodsIntgRole = new Domain.Entities.Role(1, "VIEW.FISCAL.PERIODS.INTG");
       
        [TestInitialize]
        public async void Initialize()
        {
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            currentUserFactory = new GeneralLedgerCurrentUser.AccountFundsAvailableUser();


            _fiscalPeriodsCollection = new List<FiscalPeriodsIntg>()
                {
                    new FiscalPeriodsIntg("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015"),
                    new FiscalPeriodsIntg("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2016"),
                    new FiscalPeriodsIntg("d2253ac7-9931-4560-b42f-1fccd43c952e", "2017")
                };
            _fiscalYearsCollection = new List<Ellucian.Colleague.Domain.ColleagueFinance.Entities.FiscalYear>()
                {
                    new FiscalYear("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015"),
                    new FiscalYear("949e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2016"),
                    new FiscalYear("e2253ac7-9931-4560-b42f-1fccd43c952e", "2017")
                };

            _referenceRepositoryMock.Setup(repo => repo.GetFiscalPeriodsIntgAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fiscalPeriodsCollection);
            _referenceRepositoryMock.Setup(repo => repo.GetFiscalYearsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fiscalYearsCollection);
             _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewFiscalPeriodsIntgRole });

          
            _fiscalPeriodsService = new FiscalPeriodsService(_referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, currentUserFactory,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _fiscalPeriodsService = null;
            _fiscalPeriodsCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
            currentUserFactory = null;
        }

        [TestMethod]
        public async Task FiscalPeriodsService_GetFiscalPeriodsAsync()
        {
            var results = await _fiscalPeriodsService.GetFiscalPeriodsAsync(true);
            Assert.IsTrue(results is IEnumerable<FiscalPeriods>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task FiscalPeriodsService_GetFiscalPeriodsAsync_Count()
        {
            var results = await _fiscalPeriodsService.GetFiscalPeriodsAsync(true);
            Assert.AreEqual(3, results.Count());
        }

      
        [TestMethod]
        public async Task FiscalPeriodsService_GetFiscalPeriodsAsync_Expected()
        {
            var expectedResults = _fiscalPeriodsCollection.FirstOrDefault(c => c.Guid == fiscalPeriodsGuid);
            var actualResult =
                (await _fiscalPeriodsService.GetFiscalPeriodsAsync(true)).FirstOrDefault(x => x.Id == fiscalPeriodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
          
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FiscalPeriodsService_GetFiscalPeriodsByGuidAsync_Empty()
        {
            await _fiscalPeriodsService.GetFiscalPeriodsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FiscalPeriodsService_GetFiscalPeriodsByGuidAsync_Null()
        {
            await _fiscalPeriodsService.GetFiscalPeriodsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task FiscalPeriodsService_GetFiscalPeriodsByGuidAsync_InvalidId()
        {
            _referenceRepositoryMock.Setup(repo => repo.GetFiscalPeriodsIntgAsync(It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await _fiscalPeriodsService.GetFiscalPeriodsByGuidAsync("99");
        }

        [TestMethod]
        public async Task FiscalPeriodsService_GetFiscalPeriodsByGuidAsync_Expected()
        {
            var expectedResults =
                _fiscalPeriodsCollection.First(c => c.Guid == fiscalPeriodsGuid);
            var actualResult =
                await _fiscalPeriodsService.GetFiscalPeriodsByGuidAsync(fiscalPeriodsGuid);
            Assert.AreEqual(expectedResults.Guid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
       

        }

        [TestMethod]
        public async Task FiscalPeriodsService_GetFiscalPeriodsByGuidAsync_Properties()
        {
            var result =
                await _fiscalPeriodsService.GetFiscalPeriodsByGuidAsync(fiscalPeriodsGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNull(result.Title);
            

        }
    }
}