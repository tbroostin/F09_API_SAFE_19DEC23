//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Coordination.BudgetManagement.Services;
using Ellucian.Colleague.Domain.BudgetManagement.Repositories;
using Ellucian.Colleague.Domain.BudgetManagement;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Tests.Services
{
    [TestClass]
    public class BudgetPhasesServiceTests  : BudgetUser
    {
        protected Ellucian.Colleague.Domain.Entities.Role budgetRole = new Ellucian.Colleague.Domain.Entities.Role(1, "VIEW.BUDGET.PHASES");

        private const string budgetPhasesGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
        private const string budgetPhasesCode = "2017";
        Tuple<IEnumerable<Domain.BudgetManagement.Entities.Budget>, int> _budgetPhasesCollection;
        private BudgetPhasesService _budgetPhasesService;

        private Mock<IColleagueFinanceReferenceDataRepository> _referenceRepositoryMock;
        private Mock<IBudgetRepository> _budgetRepositoryMock;
        ICurrentUserFactory curntUserFactory;
        private Mock<ILogger> _loggerMock;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ICurrentUserFactory> _currentUserFactoryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;


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

            curntUserFactory = new BudgetUser();

            _budgetPhasesCollection = new Tuple<IEnumerable<Domain.BudgetManagement.Entities.Budget>, int>
               (new List<Domain.BudgetManagement.Entities.Budget>()
            {
                    new Domain.BudgetManagement.Entities.Budget()
                    { RecordKey = "1",
                        BudgetPhaseGuid =  "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                     Title = "2015 Operating Budget", Status = "O" },
                    new Domain.BudgetManagement.Entities.Budget() {
                        RecordKey = "2", BudgetPhaseGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                     Title = "2016 Operating Budget", Status = "O" },
                    new Domain.BudgetManagement.Entities.Budget()
                    {  RecordKey = "3", BudgetPhaseGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                     Title = "2017 Operating Budget", Status = "O" }
            }, 3);


            _budgetRepositoryMock.Setup(repo => repo.GetBudgetPhasesAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_budgetPhasesCollection);

            _budgetRepositoryMock.Setup(repo => repo.GetBudgetCodesIdFromGuidAsync(It.IsAny<string>()))
                .ReturnsAsync(budgetPhasesCode);

            budgetRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(BudgetManagementPermissionCodes.ViewBudgetPhase));
            _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { budgetRole });


            _budgetPhasesService = new BudgetPhasesService(
                _budgetRepositoryMock.Object,
                _referenceRepositoryMock.Object,
                _adapterRegistryMock.Object, curntUserFactory,
                _roleRepositoryMock.Object, _configurationRepoMock.Object, _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _budgetPhasesService = null;
            _budgetPhasesCollection = null;
            _referenceRepositoryMock = null;
            _loggerMock = null;
            _currentUserFactoryMock = null;
            _roleRepositoryMock = null;
            _configurationRepoMock = null;
        }

        [TestMethod]
        public async Task BudgetPhasesService_GetBudgetPhasesAsync()
        {
            var results = await _budgetPhasesService.GetBudgetPhasesAsync("", true);
            Assert.IsTrue(results is IEnumerable<BudgetPhases>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task BudgetPhasesService_GetBudgetPhasesAsync_Count()
        {
            var results = await _budgetPhasesService.GetBudgetPhasesAsync("", true);
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task BudgetPhasesService_GetBudgetPhasesAsync_Properties()
        {
            var result =
                (await _budgetPhasesService.GetBudgetPhasesAsync("", true)).FirstOrDefault(x => x.Id == budgetPhasesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
            Assert.IsNotNull(result.Status);

        }

        [TestMethod]
        public async Task BudgetPhasesService_GetBudgetPhasesAsync_Expected()
        {
            var expectedResults = _budgetPhasesCollection.Item1.FirstOrDefault(c => c.BudgetPhaseGuid == budgetPhasesGuid);
            var actualResult =
                (await _budgetPhasesService.GetBudgetPhasesAsync("", true)).FirstOrDefault(x => x.Id == budgetPhasesGuid);
            Assert.AreEqual(expectedResults.BudgetPhaseGuid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);

        }


        [TestMethod]
        public async Task BudgetPhasesService_GetBudgetPhasesAsync_Filter_Expected()
        {
            var expectedResults = _budgetPhasesCollection.Item1.FirstOrDefault(c => c.BudgetPhaseGuid == budgetPhasesGuid);
            var actualResult =
                (await _budgetPhasesService.GetBudgetPhasesAsync(expectedResults.BudgetPhaseGuid, It.IsAny<bool>())).FirstOrDefault(x => x.Id == budgetPhasesGuid);
            Assert.AreEqual(expectedResults.BudgetPhaseGuid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);

        }

        [TestMethod]
        public async Task BudgetPhasesService_GetBudgetPhasesAsync_StatusClosed_Expected()
        {
            _budgetPhasesCollection = new Tuple<IEnumerable<Domain.BudgetManagement.Entities.Budget>, int>
               (new List<Domain.BudgetManagement.Entities.Budget>()
            {
                    new Domain.BudgetManagement.Entities.Budget()
                    { BudgetPhaseGuid =  "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                     Title = "2015 Operating Budget", Status = "O" }
            }, 1);


            _budgetRepositoryMock.Setup(repo => repo.GetBudgetPhasesAsync("", It.IsAny<bool>()))
                .ReturnsAsync(_budgetPhasesCollection);

            var expectedResults = _budgetPhasesCollection.Item1.FirstOrDefault(c => c.BudgetPhaseGuid == budgetPhasesGuid);
            var actualResult =
                (await _budgetPhasesService.GetBudgetPhasesAsync(expectedResults.BudgetPhaseGuid, true)).FirstOrDefault(x => x.Id == budgetPhasesGuid);
            Assert.AreEqual(expectedResults.BudgetPhaseGuid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BudgetPhasesService_GetBudgetPhasesByGuidAsync_Empty()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetPhasesAsync(It.IsAny<string>()))
                //.ReturnsAsync(_budgetPhasesCollection.Item1.FirstOrDefault());
                .Throws<KeyNotFoundException>();
            await _budgetPhasesService.GetBudgetPhasesByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BudgetPhasesService_GetBudgetPhasesByGuidAsync_Null()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetPhasesAsync(It.IsAny<string>()))
                  //.ReturnsAsync(_budgetPhasesCollection.Item1.FirstOrDefault());
                  .Throws<KeyNotFoundException>();
            await _budgetPhasesService.GetBudgetPhasesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task BudgetPhasesService_GetBudgetPhasesByGuidAsync_InvalidId()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetPhasesAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();

            await _budgetPhasesService.GetBudgetPhasesByGuidAsync("99");
        }

        [TestMethod]
        public async Task BudgetPhasesService_GetBudgetPhasesByGuidAsync_Expected()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetPhasesAsync(It.IsAny<string>()))
               .ReturnsAsync(_budgetPhasesCollection.Item1.FirstOrDefault());
            var expectedResults =
                _budgetPhasesCollection.Item1.FirstOrDefault(c => c.BudgetPhaseGuid == budgetPhasesGuid);
            var actualResult =
                await _budgetPhasesService.GetBudgetPhasesByGuidAsync(budgetPhasesGuid);
            Assert.AreEqual(expectedResults.BudgetPhaseGuid, actualResult.Id);
            Assert.AreEqual(expectedResults.Title, actualResult.Title);

        }

        [TestMethod]
        public async Task BudgetPhasesService_GetBudgetPhasesByGuidAsync_Properties()
        {
            _budgetRepositoryMock.Setup(repo => repo.GetBudgetPhasesAsync(It.IsAny<string>()))
               .ReturnsAsync(_budgetPhasesCollection.Item1.FirstOrDefault());
            var result =
                await _budgetPhasesService.GetBudgetPhasesByGuidAsync(budgetPhasesGuid);
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result.Title);
        }
    }
}   