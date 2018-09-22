// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Entities.Requirements;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System.Threading;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class CatalogRepositoryTests : BaseRepositorySetup
    {
        ICollection<Catalog> allCatalogs;
        Collection<Catalogs> catalogsResponseData;
        CatalogRepository catalogRepository;

        [TestInitialize]
        public async void Initialize()
        {
            MockInitialize();
            allCatalogs = await new TestCatalogRepository().GetAsync();
            catalogsResponseData = BuildCatalogResponse(allCatalogs);
            catalogRepository = BuildValidCatalogRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataReaderMock = null;
            cacheProviderMock = null;
            localCacheMock = null;
            loggerMock = null;
            transManagerMock = null;
        }

        [TestMethod]
        public async Task Get_All_Catalogs()
        {
            var catalogResponse = await catalogRepository.GetAsync();
            Assert.IsTrue(catalogResponse.Count() == 6);
            Catalog baditem = catalogResponse.Where(c => c.Code == "1999").FirstOrDefault();
            Assert.IsNull(baditem);
        }

        [TestMethod]
        public async Task Get_All_Test_Properties()
        {
            var catalogResponse = await catalogRepository.GetAsync();
            foreach (var testCatalog in allCatalogs)
            {
                Catalog repoCatalog = catalogResponse.Where(c => c.Code == testCatalog.Code).FirstOrDefault();
                Assert.AreEqual(testCatalog.StartDate, repoCatalog.StartDate);
                Assert.AreEqual(testCatalog.EndDate, repoCatalog.EndDate);
            }
        }

        [TestMethod]
        public async Task Get_Cache_Test_Properties()
        {
            var catalogResponse = await catalogRepository.GetAsync(false);
            foreach (var testCatalog in allCatalogs)
            {
                Catalog repoCatalog = catalogResponse.Where(c => c.Code == testCatalog.Code).FirstOrDefault();
                Assert.AreEqual(testCatalog.StartDate, repoCatalog.StartDate);
                Assert.AreEqual(testCatalog.EndDate, repoCatalog.EndDate);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task ThrowsExceptionIfAccessReturnsException()
        {
            CatalogRepository catalogRepo = BuildInvalidCatalogRepository();
            var repoCatalogs =await catalogRepo.GetAsync();
        }

        [TestMethod]
        public async Task Get_GetsCachedCatalogs()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "true" to indicate item is in cache
            //  -to "Get" request, return the cache item (in this case the "AllCatalogs" cache item)

            string cacheKey = catalogRepository.BuildFullCacheKey("AllCatalogs");
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(
                null,
                new SemaphoreSlim(1, 1)
             )));
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(allCatalogs.ToList()).Verifiable();
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            // return null for the data reader request, so that if we have a result, we know it wasn't the data accessor that returned it.
            dataReaderMock.Setup<Task<Collection<Catalogs>>>(acc => acc.BulkReadRecordAsync<Catalogs>("", true)).Returns(Task.FromResult(new Collection<Catalogs>()));

            // Assert that all catalogs were returned
            var catalogs = await catalogRepository.GetAsync();
            Assert.IsTrue(catalogs.Count() >= 4);
            // Verify that Get was called to get the list of programs from cache
            cacheProviderMock.Verify(m => m.Get(cacheKey, null));
        }

        private Collection<Catalogs> BuildCatalogResponse(ICollection<Catalog> allCatalogs)
        {
            Collection<Catalogs> repoCatalogs = new Collection<Catalogs>();
            foreach (var catalog in allCatalogs)
            {
                var catalogContract = new Catalogs();
                catalogContract.Recordkey = catalog.Code;
                catalogContract.CatStartDate = catalog.StartDate;
                catalogContract.CatEndDate = catalog.EndDate;
                catalogContract.CatAcadPrograms = catalog.AcadPrograms;
                repoCatalogs.Add(catalogContract);
            }
            // Add in a catalog without a start date - it should not get created and added to repository and should not throw exception.
            var badContract = new Catalogs();
            badContract.Recordkey = "1999";
            repoCatalogs.Add(badContract);
            return repoCatalogs;
        }

        private CatalogRepository BuildValidCatalogRepository()
        {
            // Set up repo response for "all" programs requests
            dataReaderMock.Setup<Task<Collection<Catalogs>>>(acc => acc.BulkReadRecordAsync<Catalogs>("", true)).Returns(Task.FromResult(catalogsResponseData));

            CatalogRepository catalogRepo = new CatalogRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            return catalogRepo;
        }

        private CatalogRepository BuildInvalidCatalogRepository()
        {
            var transFactoryMock = new Mock<IColleagueTransactionFactory>();

            // Set up data accessor for mocking 
            var dataReaderMock = new Mock<IColleagueDataReader>();
            transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

            Exception expectedFailure = new Exception("fail");

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<Catalogs>("", true)).Throws(expectedFailure);

            CatalogRepository repository = new CatalogRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return repository;
        }
    }
}