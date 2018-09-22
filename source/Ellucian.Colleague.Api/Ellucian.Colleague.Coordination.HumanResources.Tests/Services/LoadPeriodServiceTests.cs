using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Tests.Services
{
    [TestClass]
    public class LoadPeriodServiceTests : HumanResourcesServiceTestsSetup
    {
        private LoadPeriodService loadPeriodService; 
        private Mock<ILoadPeriodRepository> loadPeriodRepoMock;
        private ILoadPeriodRepository loadPeriodRepo;
        private List<LoadPeriod> loadPeriodEntities;

        [TestInitialize]
        public void Initalize()
        {
            MockInitialize();

            var loadPeriodAdapter = new AutoMapperAdapter<LoadPeriod, Dtos.Base.LoadPeriod>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(ar => ar.GetAdapter<LoadPeriod, Dtos.Base.LoadPeriod>()).Returns(loadPeriodAdapter);

            loadPeriodRepoMock = new Mock<ILoadPeriodRepository>();
            loadPeriodRepo = loadPeriodRepoMock.Object;
            loadPeriodService = new LoadPeriodService(loadPeriodRepoMock.Object, adapterRegistryMock.Object, employeeCurrentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

            loadPeriodEntities = new List<LoadPeriod>()
            {
                new LoadPeriod("FA14", "Fall 2014", new DateTime(2014, 9, 1), new DateTime(2014, 12, 5)),
                new LoadPeriod("SP15", "Spring 2015", new DateTime(2015, 1, 1), new DateTime(2015, 5, 7)),
                new LoadPeriod("FA16", "Fall 2016", new DateTime(2016, 9, 2), new DateTime(2016, 12, 6))
            };

            loadPeriodRepoMock.Setup(lpRepo => lpRepo.GetLoadPeriodsByIdsAsync(It.IsAny<List<string>>())).ReturnsAsync(loadPeriodEntities);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetLoadPeriodsByIdsAsync_NullIds_Throw()
        {
            await loadPeriodService.GetLoadPeriodsByIdsAsync(null);
        }

        [TestMethod]
        public async Task GetLoadPeriodsByIdsAsync_EmptyList_Throw()
        {
            var dtos = await loadPeriodService.GetLoadPeriodsByIdsAsync(new List<string>());
            Assert.AreEqual(0, dtos.Count());
        }

        [TestMethod]
        public async Task GetLoadPeriodsByIdsAsync_WithExistingIds_ReturnDTOs()
        {
            var ids = new List<string>() { "FA14", "SP15", "FA16" };
            var dtos = await loadPeriodService.GetLoadPeriodsByIdsAsync(ids);
            Assert.AreEqual(3, dtos.Count());
        }
    }
}
