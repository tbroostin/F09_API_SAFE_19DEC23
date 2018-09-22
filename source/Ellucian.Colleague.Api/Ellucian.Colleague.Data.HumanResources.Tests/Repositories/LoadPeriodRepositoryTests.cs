using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using System.Collections.Generic;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using System.Threading.Tasks;
using System.Linq;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class LoadPeriodRepositoryTests : BaseRepositorySetup
    {
        private LoadPeriodRepository loadPeriodRepo;
        private List<LoadPeriods> loadPeriodRecords;

        [TestInitialize]
        public void Initalize()
        {
            MockInitialize();

            loadPeriodRecords = new List<LoadPeriods>()
            {
                new LoadPeriods()
                {
                    Recordkey = "FA14", LdpdDesc = "Fall 2014", LdpdStartDate = new DateTime(2014, 9, 1), LdpdEndDate = new DateTime(2014, 12, 5)
                },

                new LoadPeriods()
                {
                    Recordkey = "SP15", LdpdDesc = "Spring 2015", LdpdStartDate = new DateTime(2015, 1, 1), LdpdEndDate = new DateTime(2015, 5, 7)
                },

                new LoadPeriods()
                {
                    Recordkey = "FA16", LdpdDesc = "Fall 2016", LdpdStartDate = new DateTime(2016, 9, 2), LdpdEndDate = new DateTime(2016, 12, 6)
                }
            };

            MockRecordsAsync("LOAD.PERIODS", loadPeriodRecords);

            loadPeriodRepo = new LoadPeriodRepository(cacheProvider, transFactory, logger, apiSettings);
        }

        [TestCleanup]
        public void CleanUp()
        {
            MockCleanup();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetLoadPeriodsByIdsAsync_WithNullIds_Throws()
        {
            await loadPeriodRepo.GetLoadPeriodsByIdsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetLoadPeriodsByIdsAsync_WithEmptylIds_Throws()
        {
            await loadPeriodRepo.GetLoadPeriodsByIdsAsync(new List<string>());
        }

        [TestMethod]
        public async Task GetLoadPeriodsByIdsAsync_WithOneExistingId_ReturnMatching()
        {
            string id = "FA14";
            var returnedEntities = await loadPeriodRepo.GetLoadPeriodsByIdsAsync(new List<string>() { id });
            Assert.AreEqual(1, returnedEntities.Count());
            Assert.AreEqual(id, returnedEntities.First().Id);
        }

        [TestMethod]
        public async Task GetLoadPeriodsByIdsAsync_WithOneNotExistingId_ReturnEmptyList()
        {
            string id = "FakeID";
            var returnedEntities = await loadPeriodRepo.GetLoadPeriodsByIdsAsync(new List<string>() { id });
            Assert.AreEqual(0, returnedEntities.Count());
        }

        [TestMethod]
        public async Task GetLoadPeriodsByIdsAsync_WithMultipleNoneExistingIds_ReturnEmptyList()
        {
            var ids = new List<string>() { "FakeID", "AnotherFake" };
            var returnedEntities = await loadPeriodRepo.GetLoadPeriodsByIdsAsync(ids);
            Assert.AreEqual(0, returnedEntities.Count());
        }

        [TestMethod]
        public async Task GetLoadPeriodsByIdsAsync_WithMultipleExistingIds_ReturnMatching()
        {
            var ids = new List<string>() { "FA14", "SP15" };
            var returnedEntities = await loadPeriodRepo.GetLoadPeriodsByIdsAsync(ids);
            Assert.AreEqual(2, returnedEntities.Count());
            var entityIds = returnedEntities.Select(lp => lp.Id);
            CollectionAssert.AreEquivalent(ids, entityIds.ToList());
        }

        [TestMethod]
        public async Task GetLoadPeriodsByIdsAsync_WithSomeExistingIds_ReturnMatching()
        {
            var ids = new List<string>() { "FA14", "SP15", "FakeID" };
            var returnedEntities = await loadPeriodRepo.GetLoadPeriodsByIdsAsync(ids);
            Assert.AreEqual(2, returnedEntities.Count());
            var entityIds = returnedEntities.Select(lp => lp.Id);
            CollectionAssert.IsSubsetOf(entityIds.ToList(), ids);
        }

        [TestMethod]
        public async Task GetLoadPeriodsAsync_WithRecords_ReturnMatching()
        {
            var returnedEntities = await loadPeriodRepo.GetLoadPeriodsAsync();
            Assert.AreEqual(3, returnedEntities.Count());
        }
    }

    [TestClass]
    public class LoadPeriodRepositoryEmptyRecordsTests : BaseRepositorySetup
    {
        private LoadPeriodRepository loadPeriodRepo;
        private List<LoadPeriods> loadPeriodRecords;

        [TestInitialize]
        public void Initalize()
        {
            MockInitialize();

            loadPeriodRecords = new List<LoadPeriods>();

            MockRecordsAsync("LOAD.PERIODS", loadPeriodRecords);

            loadPeriodRepo = new LoadPeriodRepository(cacheProvider, transFactory, logger, apiSettings);
        }

        [TestCleanup]
        public void CleanUp()
        {
            MockCleanup();
        }

        [TestMethod]
        public async Task GetLoadPeriodsAsync_NoRecords_ReturnEmpty()
        {
            var returnedEntities = await loadPeriodRepo.GetLoadPeriodsAsync();
            Assert.AreEqual(0, returnedEntities.Count());
        }
    }
}

