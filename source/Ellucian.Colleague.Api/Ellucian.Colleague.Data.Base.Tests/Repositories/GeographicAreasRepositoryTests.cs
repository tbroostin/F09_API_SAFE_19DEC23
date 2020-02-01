// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Transactions;
using Ellucian.Colleague.Data.Base.DataContracts;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class GeographicAreasRepositoryTests : BaseRepositorySetup
    {
        //string cacheKey;
        GeographicAreasRepository geographicAreasRepository;
        //Domain.Base.Entities.GeographicArea geographicAreaEntity;

        private List<string> chaptersList;
        private List<string> countiesList;
        private List<string> zipCodesList;
        private List<string> geographicAreasIdList;

        Collection<DataContracts.Chapters> _allChaptersDataContracts;
        Collection<DataContracts.Counties> _allCountiesDataContracts;
        Collection<DataContracts.ZipCodeXlat> _allZipCodeXlatDataContracts;
  
        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
            x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            geographicAreasRepository = new GeographicAreasRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            TestDataSetup();

            dataReaderMock.Setup(repo => repo.SelectAsync("CHAPTERS", It.IsAny<string>())).ReturnsAsync(chaptersList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("COUNTIES", It.IsAny<string>())).ReturnsAsync(countiesList.ToArray());
            dataReaderMock.Setup(repo => repo.SelectAsync("ZIP.CODE.XLAT", It.IsAny<string>())).ReturnsAsync(zipCodesList.ToArray());

            var ids = new List<string>();
            GetCacheApiKeysResponse resp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllGeographicAreas",
                Entity = "",
                Sublist = ids.ToList(),
                TotalCount = ids.ToList().Count,
                KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
            };

            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .ReturnsAsync(resp);

            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Chapters>("CHAPTERS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(_allChaptersDataContracts);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Counties>("COUNTIES", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(_allCountiesDataContracts);
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<ZipCodeXlat>("ZIP.CODE.XLAT", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(_allZipCodeXlatDataContracts);

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Chapters>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_allChaptersDataContracts[0]);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Counties>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_allCountiesDataContracts[0]);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<ZipCodeXlat>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_allZipCodeXlatDataContracts[0]);

            var keys = geographicAreasIdList;
            GetCacheApiKeysResponse apiResp = new GetCacheApiKeysResponse()
            {
                Offset = 0,
                Limit = 1,
                CacheName = "AllGeographicAreas",
                Entity = "",
                Sublist = keys.ToList(),
                TotalCount = keys.ToList().Count,
                KeyCacheInfo = new List<KeyCacheInfo>()
                {
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 5905,
                        KeyCacheMin = 1,
                        KeyCachePart = "000",
                        KeyCacheSize = 5905
                    },
                    new KeyCacheInfo()
                    {
                        KeyCacheMax = 7625,
                        KeyCacheMin = 5906,
                        KeyCachePart = "001",
                        KeyCacheSize = 1720
                    }
                }
            };

            transManagerMock.Setup(mgr => mgr.ExecuteAsync<GetCacheApiKeysRequest, GetCacheApiKeysResponse>(It.IsAny<GetCacheApiKeysRequest>()))
                .ReturnsAsync(apiResp);                     

        }
        
        public void TestDataSetup()
        {
            chaptersList = new List<string>()
            {
                "BALT", "EU", "MIDA"
            };

            countiesList = new List<string>()
            {
                "ANN", "FX", "OC"
            };

            zipCodesList = new List<string>()
            {
                "22101", "90210", "92646"
            };

            geographicAreasIdList = new List<string>()
            {
                "CHAPTERS*BALT", "CHAPTERS*EU", "CHAPTERS*MIDA", "COUNTIES*ANN", "COUNTIES*FX", "COUNTIES*OC",
                "ZIPCODEXLAT*22101", "ZIPCODEXLAT*90210", "ZIPCODEXLAT*92646"
            };


            _allChaptersDataContracts = new Collection<Chapters>();
            _allChaptersDataContracts.Add(new Chapters()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                ChaptersDesc = "Baltimore",
                Recordkey = "BALT"
            });
            _allChaptersDataContracts.Add(new Chapters()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                ChaptersDesc = "European Union",
                Recordkey = "EU"
            });
            _allChaptersDataContracts.Add(new Chapters()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                ChaptersDesc = "Mid-Atlantic Region",
                Recordkey = "MIDA"
            });

            _allCountiesDataContracts = new Collection<Counties>();
            _allCountiesDataContracts.Add(new Counties()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                CntyDesc = "Annandale",
                Recordkey = "ANN"
            });
            _allCountiesDataContracts.Add(new Counties()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                CntyDesc = "Fairfax",
                Recordkey = "FX"
            });
            _allCountiesDataContracts.Add(new Counties()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                CntyDesc = "Orange County",
                Recordkey = "OC"
            });

            _allZipCodeXlatDataContracts = new Collection<ZipCodeXlat>();
            _allZipCodeXlatDataContracts.Add(new ZipCodeXlat()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                Recordkey = "22101"
            });
            _allZipCodeXlatDataContracts.Add(new ZipCodeXlat()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                Recordkey = "90210"
            });
            _allZipCodeXlatDataContracts.Add(new ZipCodeXlat()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                Recordkey = "92646"
            });
        }



        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
            geographicAreasRepository = null;
        }


        [TestMethod]
        public async Task GetAllGeographicAreas_V6()
        {
            var testData = await geographicAreasRepository.GetGeographicAreasAsync(0, 100);            
            Assert.AreEqual(9, testData.Item2);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetAllGeographicAreas_NullChaptersException_V6()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Chapters>("CHAPTERS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(null);
            
            var testData = await geographicAreasRepository.GetGeographicAreasAsync(0, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetAllGeographicAreas_MissingChaptersException_V6()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Chapters>("CHAPTERS", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<Chapters>());

            var testData = await geographicAreasRepository.GetGeographicAreasAsync(0, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetAllGeographicAreas_NullCountiesException_V6()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Counties>("COUNTIES", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(null);

            var testData = await geographicAreasRepository.GetGeographicAreasAsync(0, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetAllGeographicAreas_MissingCountiesException_V6()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Counties>("COUNTIES", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<Counties>());

            var testData = await geographicAreasRepository.GetGeographicAreasAsync(0, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetAllGeographicAreas_NullZipCodesException_V6()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<ZipCodeXlat>("ZIP.CODE.XLAT", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(null);

            var testData = await geographicAreasRepository.GetGeographicAreasAsync(0, 100);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetAllGeographicAreas_MissingZipCodesException_V6()
        {
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<ZipCodeXlat>("ZIP.CODE.XLAT", It.IsAny<string[]>(), It.IsAny<bool>()))
                .ReturnsAsync(new Collection<ZipCodeXlat>());

            var testData = await geographicAreasRepository.GetGeographicAreasAsync(0, 100);
        }

        [TestMethod]
        public async Task GetGeographicAreasById_Chapter_V6()
        {
            string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
            GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
            var lookUpResults = new Dictionary<string, GuidLookupResult>();
            lookUpResults.Add("CHAPTERS", new GuidLookupResult() { Entity = "CHAPTERS", PrimaryKey = "BALT" });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

            var testData = await geographicAreasRepository.GetGeographicAreaByIdAsync(guid);

            if (testData != null)
            {
                Assert.AreEqual(_allChaptersDataContracts[0].Recordkey, testData.Code);
                Assert.AreEqual(_allChaptersDataContracts[0].ChaptersDesc, testData.Description);
            }
        }

        [TestMethod]
        public async Task GetGeographicAreasById_County_V6()
        {
            string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
            GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
            var lookUpResults = new Dictionary<string, GuidLookupResult>();
            lookUpResults.Add("COUNTIES", new GuidLookupResult() { Entity = "COUNTIES", PrimaryKey = "OC" });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

            var testData = await geographicAreasRepository.GetGeographicAreaByIdAsync(guid);

            if (testData != null)
            {
                Assert.AreEqual(_allCountiesDataContracts[0].Recordkey, testData.Code);
                Assert.AreEqual(_allCountiesDataContracts[0].CntyDesc, testData.Description);
            }
        }

        [TestMethod]
        public async Task GetGeographicAreasById_Zip_V6()
        {           
            string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
            GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
            var lookUpResults = new Dictionary<string, GuidLookupResult>();
            lookUpResults.Add("ZIP.CODE.XLAT", new GuidLookupResult() { Entity = "ZIP.CODE.XLAT", PrimaryKey = "92646" });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

            var testData = await geographicAreasRepository.GetGeographicAreaByIdAsync(guid);

            if (testData != null)
            {
                Assert.AreEqual(_allZipCodeXlatDataContracts[0].Recordkey, testData.Code);
            }
        }
        
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetGeographicAreasById_InvalidGuid_V6()
        {
            string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
            GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
            var lookUpResults = new Dictionary<string, GuidLookupResult>();
            // Force invalid GUID with a missing entity
            lookUpResults.Add("ZIP.CODE.XLAT", new GuidLookupResult() { Entity = "", PrimaryKey = "92646" });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

            var testData = await geographicAreasRepository.GetGeographicAreaByIdAsync(guid);

            if (testData != null)
            {
                Assert.AreEqual(_allZipCodeXlatDataContracts[0].Recordkey, testData.Code);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetGeographicAreasById_NullChapter_V6()
        {
            string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
            GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
            var lookUpResults = new Dictionary<string, GuidLookupResult>();
            lookUpResults.Add("CHAPTERS", new GuidLookupResult() { Entity = "CHAPTERS", PrimaryKey = "BALT" });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Chapters>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(null);

            var testData = await geographicAreasRepository.GetGeographicAreaByIdAsync(guid);

            if (testData != null)
            {
                Assert.AreEqual(_allZipCodeXlatDataContracts[0].Recordkey, testData.Code);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetGeographicAreasById_NullCounty_V6()
        {
            string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
            GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
            var lookUpResults = new Dictionary<string, GuidLookupResult>();
            lookUpResults.Add("COUNTIES", new GuidLookupResult() { Entity = "COUNTIES", PrimaryKey = "OC" });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Counties>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(null);

            var testData = await geographicAreasRepository.GetGeographicAreaByIdAsync(guid);

            if (testData != null)
            {
                Assert.AreEqual(_allZipCodeXlatDataContracts[0].Recordkey, testData.Code);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetGeographicAreasById_NullZipCode_V6()
        {
            string guid = "db2cb4bf-9531-4d38-8bcf-a96bc8628156";
            GuidLookup[] lookup = new GuidLookup[] { new GuidLookup(guid) };
            var lookUpResults = new Dictionary<string, GuidLookupResult>();
            lookUpResults.Add("ZIP.CODE.XLAT", new GuidLookupResult() { Entity = "ZIP.CODE.XLAT", PrimaryKey = "92646" });
            dataReaderMock.Setup(i => i.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResults);

            dataReaderMock.Setup(repo => repo.ReadRecordAsync<ZipCodeXlat>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(null);

            var testData = await geographicAreasRepository.GetGeographicAreaByIdAsync(guid);

            if (testData != null)
            {
                Assert.AreEqual(_allZipCodeXlatDataContracts[0].Recordkey, testData.Code);
            }
        }

    }    
}