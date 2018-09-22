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

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class BudgetCodesServiceTests  : GeneralLedgerCurrentUser
    {
        protected Ellucian.Colleague.Domain.Entities.Role budgetRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.BUDGET.PHASES");

        private const string budgetCodesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string budgetCodesCode = "2017";
        Tuple<IEnumerable<Budget>, int> _budgetCodesCollection;
        private BudgetCodesService _budgetCodesService;

        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IBudgetRepository> _budgetRepositoryMock;
        ICurrentUserFactory curntUserFactory;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear> _fiscalYearCollection;


        [TestInitialize]
        public async void Initialize()
        {
            _budgetRepositoryMock = new Mock<IBudgetRepository>();
            _referenceRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _configurationRepoMock = new Mock<IConfigurationRepository>();

            curntUserFactory = new GeneralLedgerCurrentUser.BudgetUser();

            _budgetCodesCollection = new Tuple<IEnumerable<Budget>, int>
               (new List<Domain.ColleagueFinance.Entities.Budget>()
            {
                    new Domain.ColleagueFinance.Entities.Budget()
                    { RecordKey = "FY2009OB",
                        BudgetCodeGuid =  "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                     Title = "2009 Operating Budget", Status = "O", FiscalYear = "2015"},
                    new Domain.ColleagueFinance.Entities.Budget() {
                        RecordKey = "FY2007", BudgetCodeGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                     Title = "FY 2007 Operating Budget Full", Status = "O" },
                    new Domain.ColleagueFinance.Entities.Budget()
                    {  RecordKey = "FY2010", BudgetCodeGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                     Title = "2010 Operating Budget", Status = "X" }
            }, 3);

            _fiscalYearCollection = new List<Domain.ColleagueFinance.Entities.FiscalYear>()
                {
                    new Domain.ColleagueFinance.Entities.FiscalYear("8a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "2015")
                    { Title = "Fiscal Year: 2015", Status = "O", InstitutionName = "Ellucian University"},
                    new Domain.ColleagueFinance.Entities.FiscalYear("949e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "2016")
                    { Title = "Fiscal Year: 2016", Status = "O", InstitutionName = "Ellucian University"},
                    new Domain.ColleagueFinance.Entities.FiscalYear("e2253ac7-9931-4560-b42f-1fccd43c952e", "2017")
                    { Title = "Fiscal Year: 2017", Status = "O", InstitutionName = "Ellucian University"}
                };

            

            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_budgetCodesCollection);

            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesIdFromGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(budgetCodesCode);

            _referenceRepositoryMock.Setup(repo => repo.GetCorpNameAsync())
                .ReturnsAsync("Ellucian");

            _referenceRepositoryMock.Setup(repo => repo.GetFiscalYearsAsync(It.IsAny<bool>()))
                .ReturnsAsync(_fiscalYearCollection);

            budgetRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(Ellucian.Colleague.Domain.ColleagueFinance.ColleagueFinancePermissionCodes.ViewBudgetCode));
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { budgetRole });


            _budgetCodesService = new BudgetCodesService(
                _budgetRepositoryMock.Object,
                _referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, curntUserFactory,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _budgetCodesService = null;
            _budgetCodesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
            _fiscalYearCollection = null;
        }

        [TestMethod]
        public async Task BudgetCodesService_GetBudgetCodesAsync()
        {
            var results = await _budgetCodesService.GetBudgetCodesAsync(true);
            Assert.IsTrue(results is IEnumerable<BudgetCodes>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task BudgetCodesService_GetBudgetCodesAsync_Count()
        {
            var results = await _budgetCodesService.GetBudgetCodesAsync(true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task BudgetCodesService_GetBudgetCodesAsync_Properties()
        {
            var result =
                (await _budgetCodesService.GetBudgetCodesAsync(true)).FirstOrDefault(x => x.Id == budgetCodesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Code);

        }

        [TestMethod]
        public async Task BudgetCodesService_GetBudgetCodesAsync_Expected()
        {
            var expectedResults = _budgetCodesCollection.Item1.FirstOrDefault(c => c.BudgetCodeGuid == budgetCodesGuid);
            var actualResult =
                (await _budgetCodesService.GetBudgetCodesAsync(true)).FirstOrDefault(x => x.Id == budgetCodesGuid);
            Assert.AreEqual(expectedResults.BudgetCodeGuid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);

        }

        [TestMethod]
        public async Task BudgetCodesService_GetBudgetCodesAsync_StatusClosed_Expected()
        {
            _budgetCodesCollection = new Tuple<IEnumerable<Budget>, int>
               (new List<Domain.ColleagueFinance.Entities.Budget>()
            {
                    new Domain.ColleagueFinance.Entities.Budget()
                    { BudgetCodeGuid =  "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                     Title = "Fiscal Year: 2015", Status = "O" }
            }, 1);


            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesAsync(It.IsAny<bool>()))
                .ReturnsAsync(_budgetCodesCollection);

            var expectedResults = _budgetCodesCollection.Item1.FirstOrDefault(c => c.BudgetCodeGuid == budgetCodesGuid);
            var actualResult =
                (await _budgetCodesService.GetBudgetCodesAsync(true)).FirstOrDefault(x => x.Id == budgetCodesGuid);
            Assert.AreEqual(expectedResults.BudgetCodeGuid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BudgetCodesService_GetBudgetCodesByGuidAsync_Empty()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesAsync(It.IsAny<string>()))
                //.ReturnsAsync(_budgetCodesCollection.Item1.FirstOrDefault());
                .Throws<KeyNotFoundException>();
            await _budgetCodesService.GetBudgetCodesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BudgetCodesService_GetBudgetCodesByGuidAsync_Null()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesAsync(It.IsAny<string>()))
                  //.ReturnsAsync(_budgetCodesCollection.Item1.FirstOrDefault());
                  .Throws<KeyNotFoundException>();
            await _budgetCodesService.GetBudgetCodesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BudgetCodesService_GetBudgetCodesByGuidAsync_InvalidId()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _budgetCodesService.GetBudgetCodesByGuidAsync("99");
        }

        [TestMethod]
        public async Task BudgetCodesService_GetBudgetCodesByGuidAsync_Expected()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesAsync(It.IsAny<string>()))
               .ReturnsAsync(_budgetCodesCollection.Item1.FirstOrDefault());
            var expectedResults =
                _budgetCodesCollection.Item1.FirstOrDefault(c => c.BudgetCodeGuid == budgetCodesGuid);
            var actualResult =
                await _budgetCodesService.GetBudgetCodesByGuidAsync(budgetCodesGuid);
            Assert.AreEqual(expectedResults.BudgetCodeGuid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);

        }

        [TestMethod]
        public async Task BudgetCodesService_GetBudgetCodesByGuidAsync_Properties()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesAsync(It.IsAny<string>()))
               .ReturnsAsync(_budgetCodesCollection.Item1.FirstOrDefault());
            var result =
                await _budgetCodesService.GetBudgetCodesByGuidAsync(budgetCodesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
        }
    }
}   