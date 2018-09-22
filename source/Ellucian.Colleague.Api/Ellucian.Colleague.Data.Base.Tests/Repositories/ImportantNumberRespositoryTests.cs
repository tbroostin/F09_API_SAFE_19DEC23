// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using Ellucian.Data.Colleague;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Domain.Base.Tests;
using slf4net;
using Ellucian.Web.Cache;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ImportantNumberRepositoryTests
    {
        [TestClass]
        public class ImportantNumberNonAnonymousTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ObjectCache> localCacheMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            TestImportantNumberRepository testImpRepo;
            IEnumerable<ImportantNumber> allImps;
            Collection<ImportantNumbers> impResponseData;
            IEnumerable<ImportantNumberCategory> allCats;
            ApplValcodes mdcResponse;

            ImportantNumberRepository impRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                testImpRepo = new TestImportantNumberRepository();
                allImps = testImpRepo.Get();
                allCats = testImpRepo.ImportantNumberCategories;
                impResponseData = BuildImportantNumbersResponse(allImps);
                mdcResponse = BuildApplValcodesResponse(allCats);
                impRepo = BuildImportantNumberRepository();
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                localCacheMock = null;
                testImpRepo = null;
                allImps = null;
                allCats = null;
                impResponseData = null;
                mdcResponse = null;
                impRepo = null;
            }

            [TestMethod]
            public void Get_ReturnsAllImportantNumbers()
            {
                var imps = impRepo.Get();
                Assert.AreEqual(imps.Count(), allImps.Count());
            }

            [TestMethod]
            public void Get_ValidSingleImp()
            {
                string impId = "1";
                // change to a get all and choose first()
                ImportantNumber imp = impRepo.Get().First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Name, check.Name);
            }

            [TestMethod]
            public void Get_InvalidSingleImp()
            {
                string impId = "9999999";
                IEnumerable<ImportantNumber> impList = impRepo.Get().Where(i => i.Id == impId);
                Assert.AreEqual(0, impList.Count());
            }

            [TestMethod]
            public void Get_Id()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Id, check.Id);
            }

            [TestMethod]
            public void Get_Name()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Name, check.Name);
            }

            [TestMethod]
            public void Get_City()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.City, check.City);
            }

            [TestMethod]
            public void Get_State()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.State, check.State);
            }

            [TestMethod]
            public void Get_Postal()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.PostalCode, check.PostalCode);
            }

            [TestMethod]
            public void Get_Country()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Country, check.Country);
            }

            [TestMethod]
            public void Get_Phone()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Phone, check.Phone);
            }

            [TestMethod]
            public void Get_Extension()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Extension, check.Extension);
            }

            [TestMethod]
            public void Get_Category()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Category, check.Category);
            }

            [TestMethod]
            public void Get_Email()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Email, check.Email);
            }

            [TestMethod]
            public void Get_AddressLines()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                Assert.IsNull(imp.AddressLines);
            }

            [TestMethod]
            public void Get_BuildingCode()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.BuildingCode, check.BuildingCode);
            }

            [TestMethod]
            public void Get_OfficeUseOnly()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.OfficeUseOnly, check.OfficeUseOnly);
            }

            [TestMethod]
            public void Get_Latitude()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Latitude, check.Latitude);
            }

            [TestMethod]
            public void Get_Longitude()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.Longitude, check.Longitude);
            }

            [TestMethod]
            public void Get_LocationCode()
            {
                string impId = "1";
                ImportantNumber imp = impRepo.Get().Where(ii => ii.Id == impId).First();
                ImportantNumber check = allImps.Where(ii => ii.Id == impId).First();
                Assert.AreEqual(imp.LocationCode, check.LocationCode);
            }

            [TestMethod]
            public void Get_ImportantNumberCategoriesCount()
            {
                IEnumerable<ImportantNumberCategory> cats = impRepo.ImportantNumberCategories;
                Assert.AreEqual(cats.Count(), allCats.Count());
            }

            [TestMethod]
            public void Get_ImportantNumberCategories()
            {
                string code = "FOOD";
                ImportantNumberCategory cat = impRepo.ImportantNumberCategories.Where(c => c.Code == code).First();
                ImportantNumberCategory check = allCats.Where(c => c.Code == code).First();
                Assert.AreEqual(cat.Description, check.Description);
            }

            [TestMethod]
            public void Get_CachedImportantNumbers()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "ImportantNumbers" cache item)
                string cacheKey = impRepo.BuildFullCacheKey("ImportantNumbers");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(allImps).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup<ICollection<ImportantNumbers>>(acc => acc.BulkReadRecord<ImportantNumbers>(string.Empty, true)).Returns(new Collection<ImportantNumbers>());

                // Assert that proper course was returned
                var imps = impRepo.Get();
                Assert.AreEqual(imps.Count(), allImps.Count());
                // Verify that Get was called to get the courses from cache
                cacheProviderMock.Verify(m => m.Get(cacheKey, null));
            }

            private Collection<ImportantNumbers> BuildImportantNumbersResponse(IEnumerable<ImportantNumber> imps)
            {
                Collection<ImportantNumbers> repoImps = new Collection<ImportantNumbers>();
                foreach (var imp in imps)
                {
                    var repoImp = new ImportantNumbers();
                    repoImp.Recordkey = imp.Id;
                    repoImp.ImpnumName = imp.Name;
                    repoImp.ImpnumCity = imp.City;
                    repoImp.ImpnumState = imp.State;
                    repoImp.ImpnumZip = imp.PostalCode;
                    repoImp.ImpnumCountry = imp.Country;
                    repoImp.ImpnumPhone = imp.Phone;
                    repoImp.ImpnumPhoneExt = imp.Extension;
                    repoImp.ImpnumCategory = imp.Category;
                    repoImp.ImpnumEmailAddress = imp.Email;
                    repoImp.ImpnumAddressLines = imp.AddressLines;
                    repoImp.ImpnumBuilding = imp.BuildingCode;
                    repoImp.ImpnumExportToMobile = (imp.OfficeUseOnly ? "N" : "Y");
                    repoImp.ImpnumLatitude = imp.Latitude;
                    repoImp.ImpnumLongitude = imp.Longitude;
                    repoImp.ImpnumLocation = imp.LocationCode;
                    repoImps.Add(repoImp);
                }
                return repoImps;
            }

            private ApplValcodes BuildApplValcodesResponse(IEnumerable<ImportantNumberCategory> cats)
            {
                ApplValcodes response = new ApplValcodes();
                response.Recordkey = "MOBILE.DIRECTORY.CATEGORIES";
                response.ValsEntityAssociation = new List<ApplValcodesVals>();

                foreach (var imp in cats)
                {
                    string code = imp.Code;
                    string desc = imp.Description;
                    ApplValcodesVals applValcodesVals = new ApplValcodesVals(code, desc, null, code, null, null, null);
                    response.ValsEntityAssociation.Add(applValcodesVals);
                }
                return response;
            }

            private ImportantNumberRepository BuildImportantNumberRepository()
            {
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                localCacheMock = new Mock<ObjectCache>();
                cacheProviderMock = new Mock<ICacheProvider>();
                dataAccessorMock = new Mock<IColleagueDataReader>();
                // By default dataAccessorMock is created with IsAnonymous set to false.
                dataAccessorMock.Setup<Collection<ImportantNumbers>>(acc => acc.BulkReadRecord<ImportantNumbers>("", true)).Returns(impResponseData);
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "MOBILE.DIRECTORY.CATEGORIES", true)).Returns(mdcResponse);
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                //transFactoryMock.Setup(transFac => transFac.GetDataReader(true)).Returns(dataAccessorMock.Object);
                impRepo = new ImportantNumberRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                return impRepo;
            }
        }

    }

    [TestClass]
    public class ImportantNumberAnonymousTests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<ObjectCache> localCacheMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> anonymousDataAccessorMock;
        Mock<ILogger> loggerMock;
        TestImportantNumberRepository testImpRepo;
        IEnumerable<ImportantNumber> allImps;
        IEnumerable<ImportantNumberCategory> allCats;
        Collection<ImportantNumbers> impResponseData;
        ApplValcodes mdcResponse;

        ImportantNumberRepository impRepo;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();
            testImpRepo = new TestImportantNumberRepository();
            allImps = testImpRepo.Get();
            allCats = testImpRepo.ImportantNumberCategories;
            impResponseData = BuildImportantNumbersResponse(allImps);
            mdcResponse = BuildApplValcodesResponse(allCats);
            impRepo = BuildImportantNumberRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            anonymousDataAccessorMock = null;
            cacheProviderMock = null;
            localCacheMock = null;
            testImpRepo = null;
            allImps = null;
            allCats = null;
            impResponseData = null;
            mdcResponse = null;
            impRepo = null;
        }

        [TestMethod]
        public void Get_ReturnsAllImportantNumbers()
        {
            var imps = impRepo.Get();
            Assert.AreEqual(imps.Count(), allImps.Count());
        }

        [TestMethod]
        public void Get_ImportantNumberCategoriesCount()
        {
            IEnumerable<ImportantNumberCategory> cats = impRepo.ImportantNumberCategories;
            Assert.AreEqual(cats.Count(), allCats.Count());
        }

        [TestMethod]
        public void Get_CachedImportantNumbers()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "true" to indicate item is in cache
            //  -to "Get" request, return the cache item (in this case the "ImportantNumbers" cache item)
            string cacheKey = impRepo.BuildFullCacheKey("ImportantNumbers_Anonymous");
            cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
            cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(allImps).Verifiable();

            // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
            anonymousDataAccessorMock.Setup<ICollection<ImportantNumbers>>(acc => acc.BulkReadRecord<ImportantNumbers>(string.Empty, true)).Returns(new Collection<ImportantNumbers>());
            // Assert that proper course was returned
            var imps = impRepo.Get();
            Assert.AreEqual(allImps.Count(), imps.Count() );
            // Verify that Get was called to get the courses from cache
            cacheProviderMock.Verify(m => m.Get(cacheKey, null));
        }

        private Collection<ImportantNumbers> BuildImportantNumbersResponse(IEnumerable<ImportantNumber> imps)
        {
            Collection<ImportantNumbers> repoImps = new Collection<ImportantNumbers>();
            foreach (var imp in imps)
            {
                var repoImp = new ImportantNumbers();
                repoImp.Recordkey = imp.Id;
                repoImp.ImpnumName = imp.Name;
                repoImp.ImpnumCity = imp.City;
                repoImp.ImpnumState = imp.State;
                repoImp.ImpnumZip = imp.PostalCode;
                repoImp.ImpnumCountry = imp.Country;
                repoImp.ImpnumPhone = imp.Phone;
                repoImp.ImpnumPhoneExt = imp.Extension;
                repoImp.ImpnumCategory = imp.Category;
                repoImp.ImpnumEmailAddress = imp.Email;
                repoImp.ImpnumAddressLines = imp.AddressLines;
                repoImp.ImpnumBuilding = imp.BuildingCode;
                repoImp.ImpnumExportToMobile = (imp.OfficeUseOnly ? "N" : "Y");
                repoImp.ImpnumLatitude = imp.Latitude;
                repoImp.ImpnumLongitude = imp.Longitude;
                repoImp.ImpnumLocation = imp.LocationCode;
                repoImps.Add(repoImp);
            }
            return repoImps;
        }

        private ApplValcodes BuildApplValcodesResponse(IEnumerable<ImportantNumberCategory> cats)
        {
            ApplValcodes response = new ApplValcodes();
            response.Recordkey = "MOBILE.DIRECTORY.CATEGORIES";
            response.ValsEntityAssociation = new List<ApplValcodesVals>();
            Hashtable present = new Hashtable();
            foreach (var imp in cats)
            {
                string code = imp.Code;
                string desc = imp.Description;
                ApplValcodesVals applValcodesVals = new ApplValcodesVals(code, desc, null, code, null, null, null);
                response.ValsEntityAssociation.Add(applValcodesVals);
            }
            return response;
        }

        private ImportantNumberRepository BuildImportantNumberRepository()
        {
            transFactoryMock = new Mock<IColleagueTransactionFactory>();
            localCacheMock = new Mock<ObjectCache>();
            cacheProviderMock = new Mock<ICacheProvider>();
            anonymousDataAccessorMock = new Mock<IColleagueDataReader>();
            // To mock the property on the dataReader to force it to be true so that it will be an anonymous read. 
            anonymousDataAccessorMock.SetupGet<bool>(acc => acc.IsAnonymous).Returns(true);
            anonymousDataAccessorMock.Setup<Collection<ImportantNumbers>>(acc => acc.BulkReadRecord<ImportantNumbers>(String.Empty, true)).Returns(impResponseData);
            anonymousDataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "MOBILE.DIRECTORY.CATEGORIES", true)).Returns(mdcResponse);
            transFactoryMock.Setup(transFac => transFac.GetDataReader(true)).Returns(anonymousDataAccessorMock.Object);
            impRepo = new ImportantNumberRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            return impRepo;
        }

    }
}
