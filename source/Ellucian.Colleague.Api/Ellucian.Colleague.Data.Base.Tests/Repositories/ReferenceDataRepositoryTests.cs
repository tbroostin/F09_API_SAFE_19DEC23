// Copyright 2014-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Base.Tests.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.TestUtil;
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

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class ReferenceDataRepositoryTests
    {
        /// <summary>
        /// Test class for Address Types
        /// </summary>
        [TestClass]
        public class AddressTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AddressType2> allAddressTypes;
            ApplValcodes addressTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build address type responses used for mocking
                allAddressTypes = new TestAddressTypeRepository().Get();
                addressTypeValcodeResponse = BuildValcodeResponse(allAddressTypes);

                // Build addressType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_ADREL.TYPES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                //localCacheMock = null;
                addressTypeValcodeResponse = null;
                allAddressTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsAddressTypesCache()
            {
                for (int i = 0; i < allAddressTypes.Count(); i++)
                {
                    Assert.AreEqual(allAddressTypes.ElementAt(i).Code, (await referenceDataRepo.GetAddressTypes2Async(false)).ElementAt(i).Code);
                    Assert.AreEqual(allAddressTypes.ElementAt(i).Description, (await referenceDataRepo.GetAddressTypes2Async(false)).ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetsAddressTypesNonCache()
            {

                for (int i = 0; i < allAddressTypes.Count(); i++)
                {
                    Assert.AreEqual(allAddressTypes.ElementAt(i).Code, (await referenceDataRepo.GetAddressTypes2Async(true)).ElementAt(i).Code);
                    Assert.AreEqual(allAddressTypes.ElementAt(i).Description, (await referenceDataRepo.GetAddressTypes2Async(true)).ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetAddressTypes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES", It.IsAny<bool>())).ReturnsAsync(addressTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                // But after data accessor read, set up mocking so we can verify the list of addressTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<AddressType2>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_ADREL.TYPES"), null)).Returns(true);
                var addressTypes = await referenceDataRepo.GetAddressTypes2Async(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_ADREL.TYPES"), null)).Returns(addressTypes);

                // Verify that addressTypes were returned, which means they came from the "repository".
                Assert.IsTrue(addressTypes.Count() == 16);

                // Verify that the addressType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<AddressType2>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task GetAddressTypes_GetsCachedAddressTypes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "ADREL.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allAddressTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES", true)).Returns(new ApplValcodes());

                // Assert the addressTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetAddressTypes2Async(false)).Count() == 16);
                // Verify that the addressTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Setup localCacheMock as the object for the CacheProvider
                //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to address types valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES", It.IsAny<bool>())).ReturnsAsync(addressTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var addressType = allAddressTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "ADREL.TYPES", addressType.Code }),
                            new RecordKeyLookupResult() { Guid = addressType.Guid });
                    }
                    return Task.FromResult(result);
                });


                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<AddressType2> addressTypes)
            {
                ApplValcodes addressTypesResponse = new ApplValcodes();
                addressTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in addressTypes)
                {
                    string addLocType = "";
                    switch (item.AddressTypeCategory)
                    {
                        case AddressTypeCategory.Billing:
                            addLocType = "billing";
                            break;
                        case AddressTypeCategory.Business:
                            addLocType = "business";
                            break;
                        case AddressTypeCategory.Home:
                            addLocType = "home";
                            break;
                        case AddressTypeCategory.Mailing:
                            addLocType = "mailing";
                            break;
                        case AddressTypeCategory.Vacation:
                            addLocType = "vacation";
                            break;
                        case AddressTypeCategory.School:
                            addLocType = "school";
                            break;
                        case AddressTypeCategory.Shipping:
                            addLocType = "shipping";
                            break;
                        case AddressTypeCategory.Branch:
                            addLocType = "branch";
                            break;
                        case AddressTypeCategory.Family:
                            addLocType = "family";
                            break;
                        case AddressTypeCategory.Parent:
                            addLocType = "parent";
                            break;
                        case AddressTypeCategory.Main:
                            addLocType = "main";
                            break;
                        case AddressTypeCategory.Support:
                            addLocType = "support";
                            break;
                        case AddressTypeCategory.Pobox:
                            addLocType = "pobox";
                            break;
                        case AddressTypeCategory.Region:
                            addLocType = "regional";
                            break;
                        case AddressTypeCategory.MatchingGifts:
                            addLocType = "matchingGifts";
                            break;
                        default:
                            addLocType = "";
                            break;
                    }

                    addressTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", addLocType, ""));
                }
                return addressTypesResponse;
            }
        }

        /// <summary>
        /// Test class for Buildings
        /// </summary>
        [TestClass]
        public class Buildings
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Building> allBuildings;
            string codeItemName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allBuildings = new TestBuildingRepository().Get();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllBuildings");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allBuildings = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task GetsBuildingsCacheAsync()
            {
                IEnumerable<Building> buildings = await referenceDataRepo.GetBuildingsAsync(false);

                for (int i = 0; i < allBuildings.Count(); i++)
                {
                    Assert.AreEqual(allBuildings.ElementAt(i).Guid, buildings.ElementAt(i).Guid);
                    Assert.AreEqual(allBuildings.ElementAt(i).Code, buildings.ElementAt(i).Code);
                    Assert.AreEqual(allBuildings.ElementAt(i).Description, buildings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsBuildingsNonCacheAsync()
            {
                IEnumerable<Building> buildings = await referenceDataRepo.GetBuildingsAsync(true);

                for (int i = 0; i < allBuildings.Count(); i++)
                {
                    Assert.AreEqual(allBuildings.ElementAt(i).Guid, buildings.ElementAt(i).Guid);
                    Assert.AreEqual(allBuildings.ElementAt(i).Code, buildings.ElementAt(i).Code);
                    Assert.AreEqual(allBuildings.ElementAt(i).Description, buildings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BuildingsAsync()
            {
                IEnumerable<Building> buildings = await referenceDataRepo.BuildingsAsync();

                for (int i = 0; i < allBuildings.Count(); i++)
                {
                    Assert.AreEqual(allBuildings.ElementAt(i).Guid, buildings.ElementAt(i).Guid);
                    Assert.AreEqual(allBuildings.ElementAt(i).Code, buildings.ElementAt(i).Code);
                    Assert.AreEqual(allBuildings.ElementAt(i).Description, buildings.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Buildings read
                var buildingsCollection = new Collection<DataContracts.Buildings>(allBuildings.Select(record =>
                    new Data.Base.DataContracts.Buildings()
                    {
                        Recordkey = record.Code,
                        BldgDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());


                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Buildings>("BUILDINGS", "", true))
                    .ReturnsAsync(buildingsCollection);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allBuildings.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "BUILDINGS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Races
        /// </summary>
        [TestClass]
        public class Races
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Race> allRaces;
            ApplValcodes raceValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build races responses used for mocking
                allRaces = new TestRaceRepository().Get();
                raceValcodeResponse = BuildValcodeResponse(allRaces);

                // Build race repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_PERSON.RACES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                raceValcodeResponse = null;
                allRaces = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsRacesCacheAsync()
            {
                var races = await referenceDataRepo.GetRacesAsync(false);
                for (int i = 0; i < races.Count(); i++)
                {
                    Assert.AreEqual(allRaces.ElementAt(i).Code, races.ElementAt(i).Code);
                    Assert.AreEqual(allRaces.ElementAt(i).Description, races.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsRacesNonCacheAsync()
            {
                var races = await referenceDataRepo.GetRacesAsync(true);
                for (int i = 0; i < races.Count(); i++)
                {
                    Assert.AreEqual(allRaces.ElementAt(i).Code, races.ElementAt(i).Code);
                    Assert.AreEqual(allRaces.ElementAt(i).Description, races.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task RacesAsync()
            {
                var races = await referenceDataRepo.RacesAsync();
                for (int i = 0; i < races.Count(); i++)
                {
                    Assert.AreEqual(allRaces.ElementAt(i).Code, races.ElementAt(i).Code);
                    Assert.AreEqual(allRaces.ElementAt(i).Description, races.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetRaces_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(raceValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of races was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<Race>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_PERSON.RACES"), null)).Returns(true);
                var races = await referenceDataRepo.GetRacesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_PERSON.RACES"), null)).Returns(races);
                // Verify that races were returned, which means they came from the "repository".
                Assert.IsTrue(races.Count() == 5);

                // Verify that the race item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<Race>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
                
            }

            [TestMethod]
            public async Task GetRaces_GetsCachedRacesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PERSON.EMAIL.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allRaces).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.RACES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the races are returned
                Assert.IsTrue((await referenceDataRepo.GetRacesAsync(false)).Count() == 5);
                // Verify that the races were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to race valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.RACES", It.IsAny<bool>())).ReturnsAsync(raceValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var race = allRaces.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "PERSON.RACES", race.Code }),
                            new RecordKeyLookupResult() { Guid = race.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Race> races)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in races)
                {
                    string newType = "";
                    switch (item.Type)
                    {
                        case RaceType.AmericanIndian:
                            newType = "1";
                            break;
                        case RaceType.Asian:
                            newType = "2";
                            break;
                        case RaceType.Black:
                            newType = "3";
                            break;
                        case RaceType.PacificIslander:
                            newType = "4";
                            break;
                        default:
                            newType = "5";
                            break;
                    }
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, newType, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Chapters codes
        /// </summary>
        [TestClass]
        public class ChaptersTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Chapter> allChapters;
            string codeItemName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allChapters = new TestGeographicAreaRepository().GetChapters();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllChapters");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allChapters = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsChaptersCacheAsync()
            {
                var chapters = await referenceDataRepo.GetChaptersAsync(false);

                for (int i = 0; i < allChapters.Count(); i++)
                {
                    Assert.AreEqual(allChapters.ElementAt(i).Guid, chapters.ElementAt(i).Guid);
                    Assert.AreEqual(allChapters.ElementAt(i).Code, chapters.ElementAt(i).Code);
                    Assert.AreEqual(allChapters.ElementAt(i).Description, chapters.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsChaptersNonCacheAsync()
            {
                var chapters = await referenceDataRepo.GetChaptersAsync(true);

                for (int i = 0; i < allChapters.Count(); i++)
                {
                    Assert.AreEqual(allChapters.ElementAt(i).Guid, chapters.ElementAt(i).Guid);
                    Assert.AreEqual(allChapters.ElementAt(i).Code, chapters.ElementAt(i).Code);
                    Assert.AreEqual(allChapters.ElementAt(i).Description, chapters.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Chapters read
                var chaptersCollection = new Collection<Chapters>(allChapters.Select(record =>
                    new Data.Base.DataContracts.Chapters()
                    {
                        Recordkey = record.Code,
                        ChaptersDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Chapters>("CHAPTERS", "", true))
                    .ReturnsAsync(chaptersCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var chapter = allChapters.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CHAPTERS", chapter.Code }),
                            new RecordKeyLookupResult() { Guid = chapter.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Citizenship Statuses
        /// </summary>
        [TestClass]
        public class CitizenshipStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<CitizenshipStatus> allCitizenshipStatuses;
            ApplValcodes citizenshipStatusValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build citizenship statuses responses used for mocking
                allCitizenshipStatuses = new TestCitizenshipStatusRepository().Get();
                citizenshipStatusValcodeResponse = BuildValcodeResponse(allCitizenshipStatuses);

                // Build race repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_ALIEN.STATUSES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                citizenshipStatusValcodeResponse = null;
                allCitizenshipStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsCitizenshipStatusesCacheAsync()
            {
                var citizenshipStatuses = await referenceDataRepo.GetCitizenshipStatusesAsync(false);
                for (int i = 0; i < citizenshipStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCitizenshipStatuses.ElementAt(i).Code, citizenshipStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allCitizenshipStatuses.ElementAt(i).Description, citizenshipStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsCitizenshipStatusesNonCacheAsync()
            {
                var citizenshipStatuses = await referenceDataRepo.GetCitizenshipStatusesAsync(true);
                for (int i = 0; i < citizenshipStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCitizenshipStatuses.ElementAt(i).Code, citizenshipStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allCitizenshipStatuses.ElementAt(i).Description, citizenshipStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetCitizenshipStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(citizenshipStatusValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of citizenship statuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<CitizenshipStatus>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_ALIEN.STATUSES"), null)).Returns(true);
                var citizenshipStatuses = await referenceDataRepo.GetCitizenshipStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_ALIEN.STATUSES"), null)).Returns(citizenshipStatuses);
                // Verify that citizenship statuses were returned, which means they came from the "repository".
                Assert.IsTrue(citizenshipStatuses.Count() == 3);

                // Verify that the citizenship status item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<CitizenshipStatus>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetCitizenshipStatuses_GetsCachedCitizenshipStatusesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "ALIEN.STATUSES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allCitizenshipStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ALIEN.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the citizenship statuses are returned
                Assert.IsTrue((await referenceDataRepo.GetCitizenshipStatusesAsync(false)).Count() == 3);
                // Verify that the citizenship statuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to citizenship status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ALIEN.STATUSES", It.IsAny<bool>())).ReturnsAsync(citizenshipStatusValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var citizenshipStatus = allCitizenshipStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "ALIEN.STATUSES", citizenshipStatus.Code }),
                            new RecordKeyLookupResult() { Guid = citizenshipStatus.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<CitizenshipStatus> citizenshipStatuses)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in citizenshipStatuses)
                {
                    string newType = "";
                    switch (item.CitizenshipStatusType)
                    {
                        case CitizenshipStatusType.Citizen:
                            newType = "NA";
                            break;
                        case CitizenshipStatusType.NonCitizen:
                            newType = "NRA";
                            break;
                        default:
                            newType = "NRA";
                            break;
                    }
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, newType, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Commerce Tax Codes
        /// </summary>
        [TestClass]
        public class CommerceTaxCodesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<CommerceTaxCode> allCommerceTaxCodes;

            string codeItemName;
            private ApiSettings apiSettings;
            string[] guids = new string[]
            {
                "625c69ff-280b-4ed3-9474-662a43616a8a",
                "bfea651b-8e27-4fcd-abe3-04573443c04c",
                "9ae3a175-1dfd-4937-b97b-3c9ad596e023",
                "e9e6837f-2c51-431b-9069-4ac4c0da3041"
            };

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                // Build responses used for mocking
                allCommerceTaxCodes = new TestCommerceTaxCodesRepository().GetCommerceTaxCodes();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllCommerceTaxCodes");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allCommerceTaxCodes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsCommerceTaxCodesCacheAsync()
            {
                var commerceTaxCodes = await referenceDataRepo.GetCommerceTaxCodesAsync(false);

                for (int i = 0; i < allCommerceTaxCodes.Count(); i++)
                {
                    var expected = allCommerceTaxCodes.FirstOrDefault(r => r.Guid.Equals(commerceTaxCodes.ElementAt(i).Guid, StringComparison.OrdinalIgnoreCase));

                    Assert.AreEqual(expected.Guid, commerceTaxCodes.ElementAt(i).Guid);
                    Assert.AreEqual(expected.Code, commerceTaxCodes.ElementAt(i).Code);
                    Assert.AreEqual(expected.Description, commerceTaxCodes.ElementAt(i).Description);
                    Assert.AreEqual(expected.ApTaxEffectiveDates.Count(), commerceTaxCodes.ElementAt(i).ApTaxEffectiveDates.Count());
                }
            }

            [TestMethod]
            public async Task GetsCommerceTaxCodesNonCacheAsync()
            {
                var commerceTaxCodes = await referenceDataRepo.GetCommerceTaxCodesAsync(It.IsAny<bool>());

                for (int i = 0; i < allCommerceTaxCodes.Count(); i++)
                {
                    var expected = allCommerceTaxCodes.FirstOrDefault(r => r.Guid.Equals(commerceTaxCodes.ElementAt(i).Guid, StringComparison.OrdinalIgnoreCase));
                    Assert.AreEqual(expected.Guid, commerceTaxCodes.ElementAt(i).Guid);
                    Assert.AreEqual(expected.Code, commerceTaxCodes.ElementAt(i).Code);
                    Assert.AreEqual(expected.Description, commerceTaxCodes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsCommerceTaxCodeGuidAsync()
            {
                var commerceTaxCodes = await referenceDataRepo.GetCommerceTaxCodesAsync(It.IsAny<bool>());
                for (int i = 0; i < allCommerceTaxCodes.Count(); i++)
                {
                    var commerceTaxCodeGuid = await referenceDataRepo.GetCommerceTaxCodeGuidAsync(commerceTaxCodes.ElementAt(i).Code);
                    Assert.AreEqual(allCommerceTaxCodes.ElementAt(i).Guid, commerceTaxCodeGuid);                    
                }
            }



            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to CommerceTaxCodes read
                var taxesCollection = new Collection<ApTaxes>(allCommerceTaxCodes.Select(record =>
                    new Data.Base.DataContracts.ApTaxes()
                    {
                        Recordkey = record.Code,
                        ApTaxDesc = record.Description,
                        RecordGuid = record.Guid,
                        ApTaxEffectiveDate = record.ApTaxEffectiveDates
                    }).ToList());

                //Guids
                dataAccessorMock.Setup(repo => repo.SelectAsync("LDM.GUID", It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>(),
                    It.IsAny<bool>(), It.IsAny<int>())).ReturnsAsync(guids);
                //Ids
                dataAccessorMock.Setup(repo => repo.SelectAsync("LDM.GUID", It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(guids);

                var ldmRecords = new Collection<LdmGuid>()
                {
                    new LdmGuid()
                    {
                        Recordkey = "625c69ff-280b-4ed3-9474-662a43616a8a", LdmGuidEntity = "AP.TAXES", LdmGuidPrimaryKey = "BA"                                                                                             
                    },
                    new LdmGuid()
                    {
                        Recordkey = "bfea651b-8e27-4fcd-abe3-04573443c04c", LdmGuidEntity = "AP.TAXES", LdmGuidPrimaryKey = "CA"
                    },
                    new LdmGuid()
                    {
                        Recordkey = "9ae3a175-1dfd-4937-b97b-3c9ad596e023", LdmGuidEntity = "AP.TAXES", LdmGuidPrimaryKey = "CC"
                    },
                    new LdmGuid()
                    {
                        Recordkey = "e9e6837f-2c51-431b-9069-4ac4c0da3041", LdmGuidEntity = "AP.TAXES", LdmGuidPrimaryKey = "EP"
                    }
                };
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<LdmGuid>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(ldmRecords);

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ApTaxes>("AP.TAXES", "", true))
                    .ReturnsAsync(taxesCollection);
                
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var denomination = allCommerceTaxCodes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "AP.TAXES", denomination.Code }),
                            new RecordKeyLookupResult() { Guid = denomination.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }


        /// <summary>
        /// Test class for Counties codes
        /// </summary>
        [TestClass]
        public class CountiesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<County> allCounties;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allCounties = new TestGeographicAreaRepository().GetCounties();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllCounties");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allCounties = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsCountiesCacheAsync()
            {
                var counties = await referenceDataRepo.GetCountiesAsync(false);

                for (int i = 0; i < allCounties.Count(); i++)
                {
                    Assert.AreEqual(allCounties.ElementAt(i).Guid, counties.ElementAt(i).Guid);
                    Assert.AreEqual(allCounties.ElementAt(i).Code, counties.ElementAt(i).Code);
                    Assert.AreEqual(allCounties.ElementAt(i).Description, counties.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsCountiesNonCacheAsync()
            {
                var counties = await referenceDataRepo.GetCountiesAsync(true);

                for (int i = 0; i < allCounties.Count(); i++)
                {
                    Assert.AreEqual(allCounties.ElementAt(i).Guid, counties.ElementAt(i).Guid);
                    Assert.AreEqual(allCounties.ElementAt(i).Code, counties.ElementAt(i).Code);
                    Assert.AreEqual(allCounties.ElementAt(i).Description, counties.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Counties read
                var countiesCollection = new Collection<Counties>(allCounties.Select(record =>
                    new Data.Base.DataContracts.Counties()
                    {
                        Recordkey = record.Code,
                        CntyDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Counties>("COUNTIES", "", true))
                    .ReturnsAsync(countiesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var county = allCounties.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "COUNTIES", county.Code }),
                            new RecordKeyLookupResult() { Guid = county.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Denominations codes
        /// </summary>
        [TestClass]
        public class DenominationsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Denomination> allDenominations;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allDenominations = new TestReligionRepository().GetDenominations();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllDenominations");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allDenominations = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsDenominationsCacheAsync()
            {
                var denominations = await referenceDataRepo.DenominationsAsync();

                for (int i = 0; i < allDenominations.Count(); i++)
                {
                    Assert.AreEqual(allDenominations.ElementAt(i).Guid, denominations.ElementAt(i).Guid);
                    Assert.AreEqual(allDenominations.ElementAt(i).Code, denominations.ElementAt(i).Code);
                    Assert.AreEqual(allDenominations.ElementAt(i).Description, denominations.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsDenominationsNonCacheAsync()
            {
                var denominations = await referenceDataRepo.GetDenominationsAsync(true);

                for (int i = 0; i < allDenominations.Count(); i++)
                {
                    Assert.AreEqual(allDenominations.ElementAt(i).Guid, denominations.ElementAt(i).Guid);
                    Assert.AreEqual(allDenominations.ElementAt(i).Code, denominations.ElementAt(i).Code);
                    Assert.AreEqual(allDenominations.ElementAt(i).Description, denominations.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Denominations read
                var denominationsCollection = new Collection<Denominations>(allDenominations.Select(record =>
                    new Data.Base.DataContracts.Denominations()
                    {
                        Recordkey = record.Code,
                        DenomDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Denominations>("DENOMINATIONS", "", true))
                    .ReturnsAsync(denominationsCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var denomination = allDenominations.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "DENOMINATIONS", denomination.Code }),
                            new RecordKeyLookupResult() { Guid = denomination.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }
             
        /// <summary>
        /// Test class for Departments
        /// </summary>
        [TestClass]
        public class DepartmentsTest
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            string _cacheKey;            
            IEnumerable<Department> _allDepartments;         
            private ReferenceDataRepository _referenceDataRepo;
            private ApiSettings apiSettings;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                _allDepartments = new TestDepartmentRepository().Get();

                _referenceDataRepo = BuildValidReferenceDataRepository();
                _cacheKey = _referenceDataRepo.BuildFullCacheKey("AllBaseDepartments");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                _allDepartments = null;
                _referenceDataRepo = null;
                _cacheKey = null;

            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetDepartmentsAsync_False()
            {
                var results = await _referenceDataRepo.GetDepartmentsAsync(false);
                Assert.AreEqual(_allDepartments.Count(), results.Count());

                foreach (var academicDepartment in _allDepartments)
                {
                    var result = results.FirstOrDefault(i => i.Guid == academicDepartment.Guid);

                    Assert.AreEqual(academicDepartment.Code, result.Code);
                    Assert.AreEqual(academicDepartment.Description, result.Description);
                    Assert.AreEqual(academicDepartment.Guid, result.Guid);
                }

            }

           
            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();

                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();

                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();
               
                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                var ldmGuidDepartment = new List<string>();
                var records = new Collection<Depts>();

                foreach (var item in _allDepartments)
                {
                    ldmGuidDepartment.Add(item.Guid);
                    var record = new Depts()
                    {
                        RecordGuid = item.Guid,
                        DeptsDesc = item.Description,
                        Recordkey = item.Code,
                        DeptsDivision = item.Division,
                        DeptsSchool = item.School,
                        DeptsInstitutionsId = item.InstitutionId,
                        DeptsType = item.DepartmentType
                    };
                    records.Add(record);

                }
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Depts>("DEPTS", It.IsAny<GuidLookup[]>(), It.IsAny<bool>())).ReturnsAsync(records);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Depts>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(records);
                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Depts>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(records);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(ldmGuidDepartment.ToArray());

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = _allDepartments.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(record.Guid,
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, GuidLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = _allDepartments.Where(e => e.Guid == recordKeyLookup.Guid).FirstOrDefault();
                        result.Add(record.Guid,
                          new GuidLookupResult() { PrimaryKey = record.Code });
                    }
                    return Task.FromResult(result);
                });

                return new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object,
                     loggerMock.Object, apiSettings);

            }
        }
       
        /// <summary>
        /// Test class for Email Types codes
        /// </summary>
        [TestClass]
        public class EmailTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<EmailType> allEmailTypes;
            ApplValcodes emailTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build email types responses used for mocking
                allEmailTypes = new TestEmailTypeRepository().Get();
                emailTypeValcodeResponse = BuildValcodeResponse(allEmailTypes);


                // Build emailType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_PERSON.EMAIL.TYPES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                //localCacheMock = null;
                emailTypeValcodeResponse = null;
                allEmailTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEmailTypesCacheAsync()
            {
                for (int i = 0; i < allEmailTypes.Count(); i++)
                {
                    Assert.AreEqual(allEmailTypes.ElementAt(i).Code, (await referenceDataRepo.GetEmailTypesAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(allEmailTypes.ElementAt(i).Description, (await referenceDataRepo.GetEmailTypesAsync(false)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEmailTypesNonCacheAsync()
            {
                for (int i = 0; i < allEmailTypes.Count(); i++)
                {
                    Assert.AreEqual(allEmailTypes.ElementAt(i).Code, (await referenceDataRepo.GetEmailTypesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(allEmailTypes.ElementAt(i).Description, (await referenceDataRepo.GetEmailTypesAsync(true)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetEmailTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.EMAIL.TYPES", It.IsAny<bool>())).ReturnsAsync(emailTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of emailTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<EmailType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_PERSON.EMAIL.TYPES"), null)).Returns(true);
                var emailTypes = await referenceDataRepo.GetEmailTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_PERSON.EMAIL.TYPES"), null)).Returns(emailTypes);
                // Verify that emailTypes were returned, which means they came from the "repository".
                Assert.IsTrue(emailTypes.Count() == 14);

                // Verify that the emailType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<EmailType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);


            }

            [TestMethod]
            public async Task GetEmailTypes_GetsCachedEmailTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PERSON.EMAIL.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allEmailTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.EMAIL.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the emailTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetEmailTypesAsync(false)).Count() == 14);
                // Verify that the emailTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Setup localCacheMock as the object for the CacheProvider
                //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to emailType valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.EMAIL.TYPES", It.IsAny<bool>())).ReturnsAsync(emailTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var emailType = allEmailTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "PERSON.EMAIL.TYPES", emailType.Code }),
                            new RecordKeyLookupResult() { Guid = emailType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<EmailType> emailTypes)
            {
                ApplValcodes emailTypesResponse = new ApplValcodes();
                emailTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in emailTypes)
                {
                    string emailType = "";
                    switch (item.EmailTypeCategory)
                    {
                        case EmailTypeCategory.Billing:
                            emailType = "billing";
                            break;
                        case EmailTypeCategory.Business:
                            emailType = "business";
                            break;
                        case EmailTypeCategory.Sales:
                            emailType = "sales";
                            break;
                        case EmailTypeCategory.Media:
                            emailType = "media";
                            break;
                        case EmailTypeCategory.Support:
                            emailType = "support";
                            break;
                        case EmailTypeCategory.School:
                            emailType = "school";
                            break;
                        case EmailTypeCategory.Personal:
                            emailType = "personal";
                            break;
                        case EmailTypeCategory.Legal:
                            emailType = "legal";
                            break;
                        case EmailTypeCategory.HR:
                            emailType = "hr";
                            break;
                        case EmailTypeCategory.Parent:
                            emailType = "parent";
                            break;
                        case EmailTypeCategory.General:
                            emailType = "general";
                            break;
                        case EmailTypeCategory.Family:
                            emailType = "family";
                            break;
                        case EmailTypeCategory.MatchingGifts:
                            emailType = "matchingGifts";
                            break;
                        default:
                            emailType = "";
                            break;
                    }

                    emailTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", emailType, ""));
                }
                return emailTypesResponse;
            }
        }
               

        [TestClass]
        public class IntgPersonEmerPhoneTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<IntgPersonEmerPhoneTypes> allIntgPersonEmerPhoneTypes;
            ApplValcodes emergencyContactPhoneAvailabilitiesValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            IReferenceDataRepository referenceDataRepository;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allIntgPersonEmerPhoneTypes = new TestPersonEmerPhoneTypesRepository().GetIntgPersonEmerPhoneTypesAsync();
                emergencyContactPhoneAvailabilitiesValcodeResponse = BuildValcodeResponse(allIntgPersonEmerPhoneTypes);
                var emergencyContactPhoneAvailabilitiesValResponse = new List<string>() { "2" };
                emergencyContactPhoneAvailabilitiesValcodeResponse.ValActionCode1 = emergencyContactPhoneAvailabilitiesValResponse;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.EMER.PHONE.TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                emergencyContactPhoneAvailabilitiesValcodeResponse = null;
                allIntgPersonEmerPhoneTypes = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task BaseReferenceDataRepo_GetsIntgPersonEmerPhoneTypesCacheAsync()
            {
                var emergencyContactPhoneAvailabilities = await referenceDataRepo.GetIntgPersonEmerPhoneTypesAsync(false);

                for (int i = 0; i < allIntgPersonEmerPhoneTypes.Count(); i++)
                {
                    Assert.AreEqual(allIntgPersonEmerPhoneTypes.ElementAt(i).Code, emergencyContactPhoneAvailabilities.ElementAt(i).Code);
                    Assert.AreEqual(allIntgPersonEmerPhoneTypes.ElementAt(i).Description, emergencyContactPhoneAvailabilities.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetsIntgPersonEmerPhoneTypesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetIntgPersonEmerPhoneTypesAsync(true);

                for (int i = 0; i < allIntgPersonEmerPhoneTypes.Count(); i++)
                {
                    Assert.AreEqual(allIntgPersonEmerPhoneTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allIntgPersonEmerPhoneTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetIntgPersonEmerPhoneTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.EMER.PHONE.TYPES", It.IsAny<bool>())).ReturnsAsync(emergencyContactPhoneAvailabilitiesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of emergencyContactPhoneAvailabilities was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgPersonEmerPhoneTypes>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.EMER.PHONE.TYPES"), null)).Returns(true);
                var emergencyContactPhoneAvailabilities = await referenceDataRepo.GetIntgPersonEmerPhoneTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.EMER.PHONE.TYPES"), null)).Returns(emergencyContactPhoneAvailabilities);
                // Verify that emergencyContactPhoneAvailabilities were returned, which means they came from the "repository".
                Assert.IsTrue(emergencyContactPhoneAvailabilities.Count() == 3);

                // Verify that the emergencyContactPhoneAvailabilities item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgPersonEmerPhoneTypes>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetIntgPersonEmerPhoneTypes_GetsCachedIntgPersonEmerPhoneTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.PERSON.EMER.PHONE.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allIntgPersonEmerPhoneTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.EMER.PHONE.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the emergencyContactPhoneAvailabilities are returned
                Assert.IsTrue((await referenceDataRepo.GetIntgPersonEmerPhoneTypesAsync(false)).Count() == 3);
                // Verify that the semergencyContactPhoneAvailabilities were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to emergencyContactPhoneAvailabilities domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.EMER.PHONE.TYPES", It.IsAny<bool>())).ReturnsAsync(emergencyContactPhoneAvailabilitiesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var emergencyContactPhoneAvailabilities = allIntgPersonEmerPhoneTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "INTG.PERSON.EMER.PHONE.TYPES", emergencyContactPhoneAvailabilities.Code }),
                            new RecordKeyLookupResult() { Guid = emergencyContactPhoneAvailabilities.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<IntgPersonEmerPhoneTypes> emergencyContactPhoneAvailabilities)
            {
                ApplValcodes emergencyContactPhoneAvailabilitiesResponse = new ApplValcodes();
                emergencyContactPhoneAvailabilitiesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in emergencyContactPhoneAvailabilities)
                {
                    emergencyContactPhoneAvailabilitiesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return emergencyContactPhoneAvailabilitiesResponse;
            }
        }

        [TestClass]
        public class IntgPersonEmerTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<IntgPersonEmerTypes> allIntgPersonEmerTypes;
            ApplValcodes emergencyContactTypesValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            IReferenceDataRepository referenceDataRepository;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allIntgPersonEmerTypes = new TestPersonEmerTypesRepository().GetIntgPersonEmerTypesAsync();
                emergencyContactTypesValcodeResponse = BuildValcodeResponse(allIntgPersonEmerTypes);
                var emergencyContactTypesValResponse = new List<string>() { "2" };
                emergencyContactTypesValcodeResponse.ValActionCode1 = emergencyContactTypesValResponse;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.EMER.TYPES_GUID");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                emergencyContactTypesValcodeResponse = null;
                allIntgPersonEmerTypes = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task BaseReferenceDataRepo_GetsIntgPersonEmerTypesCacheAsync()
            {
                var emergencyContactTypes = await referenceDataRepo.GetIntgPersonEmerTypesAsync(false);

                for (int i = 0; i < allIntgPersonEmerTypes.Count(); i++)
                {
                    Assert.AreEqual(allIntgPersonEmerTypes.ElementAt(i).Code, emergencyContactTypes.ElementAt(i).Code);
                    Assert.AreEqual(allIntgPersonEmerTypes.ElementAt(i).Description, emergencyContactTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetsIntgPersonEmerTypesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetIntgPersonEmerTypesAsync(true);

                for (int i = 0; i < allIntgPersonEmerTypes.Count(); i++)
                {
                    Assert.AreEqual(allIntgPersonEmerTypes.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allIntgPersonEmerTypes.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetIntgPersonEmerTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.EMER.TYPES", It.IsAny<bool>())).ReturnsAsync(emergencyContactTypesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of emergencyContactTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgPersonEmerTypes>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.EMER.TYPES"), null)).Returns(true);
                var emergencyContactTypes = await referenceDataRepo.GetIntgPersonEmerTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.EMER.TYPES"), null)).Returns(emergencyContactTypes);
                // Verify that emergencyContactTypes were returned, which means they came from the "repository".
                Assert.IsTrue(emergencyContactTypes.Count() == 3);

                // Verify that the emergencyContactTypes item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgPersonEmerTypes>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetIntgPersonEmerTypes_GetsCachedIntgPersonEmerTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.PERSON.EMER.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allIntgPersonEmerTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.EMER.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the emergencyContactTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetIntgPersonEmerTypesAsync(false)).Count() == 3);
                // Verify that the semergencyContactTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to emergencyContactTypes domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.EMER.TYPES", It.IsAny<bool>())).ReturnsAsync(emergencyContactTypesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var emergencyContactTypes = allIntgPersonEmerTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "INTG.PERSON.EMER.TYPES", emergencyContactTypes.Code }),
                            new RecordKeyLookupResult() { Guid = emergencyContactTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<IntgPersonEmerTypes> emergencyContactTypes)
            {
                ApplValcodes emergencyContactTypesResponse = new ApplValcodes();
                emergencyContactTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in emergencyContactTypes)
                {
                    emergencyContactTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return emergencyContactTypesResponse;
            }
        }

    


    /// <summary>
    /// Test class for Ethnicity codes
    /// </summary>
    [TestClass]
        public class Ethnicities
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ethnicity> allEthnicity;
            ApplValcodes ethnicityValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build ethnicities responses used for mocking
                allEthnicity = new TestEthnicityRepository().Get();
                ethnicityValcodeResponse = BuildValcodeResponse(allEthnicity);


                // Build ethnicities repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_PERSON.ETHNICS_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                ethnicityValcodeResponse = null;
                allEthnicity = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsEthnicitiesCacheAsync()
            {
                for (int i = 0; i < allEthnicity.Count(); i++)
                {
                    Assert.AreEqual(allEthnicity.ElementAt(i).Code, (await referenceDataRepo.GetEthnicitiesAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(allEthnicity.ElementAt(i).Description, (await referenceDataRepo.GetEthnicitiesAsync(false)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsEthnicitiesNonCacheAsync()
            {
                for (int i = 0; i < allEthnicity.Count(); i++)
                {
                    Assert.AreEqual(allEthnicity.ElementAt(i).Code, (await referenceDataRepo.GetEthnicitiesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(allEthnicity.ElementAt(i).Description, (await referenceDataRepo.GetEthnicitiesAsync(true)).ElementAt(i).Description);
            }
            }

            [TestMethod]
            public async Task GetEthnicities_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ETHNICS", It.IsAny<bool>())).ReturnsAsync(ethnicityValcodeResponse);
                
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of ethnicities was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<Ethnicity>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_PERSON.ETHNICS"), null)).Returns(true);
                var ethnicities = await referenceDataRepo.GetEthnicitiesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_PERSON.ETHNICS"), null)).Returns(ethnicities);
                // Verify that ethnicities were returned, which means they came from the "repository".
                Assert.IsTrue(ethnicities.Count() == 2);

                // Verify that the ethnicity item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<Ethnicity>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);


            }

            [TestMethod]
            public async Task GetEthnicities_GetsCachedEthnicitiesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PERSON.ETHNICS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allEthnicity).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ETHNICS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the ethnics are returned
                Assert.IsTrue((await referenceDataRepo.GetEthnicitiesAsync(false)).Count() == 2);
                // Verify that the ethnicities were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to ethnicity valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ETHNICS", It.IsAny<bool>())).ReturnsAsync(ethnicityValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var ethnicity = allEthnicity.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "PERSON.ETHNICS", ethnicity.Code }),
                            new RecordKeyLookupResult() { Guid = ethnicity.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ethnicity> ethnicities)
            {
                ApplValcodes ethnicitiesResponse = new ApplValcodes();
                ethnicitiesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in ethnicities)
                {
                    ethnicitiesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return ethnicitiesResponse;
            }
        }

        /// <summary>
        /// Test class for Geographic Area Types
        /// </summary>
        [TestClass]
        public class GeographicAreaTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<GeographicAreaType> allGeographicAreaTypes;
            ApplValcodes geographicAreaTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build geographic area types responses used for mocking
                allGeographicAreaTypes = new TestGeographicAreaTypeRepository().Get();
                geographicAreaTypeValcodeResponse = BuildValcodeResponse(allGeographicAreaTypes);

                // Build geographic area types repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_INTG.GEO.AREA.TYPES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                geographicAreaTypeValcodeResponse = null;
                allGeographicAreaTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsGeographicAreaTypesCacheAsync()
            {
                var geographicAreaTypes = await referenceDataRepo.GetGeographicAreaTypesAsync(false);
                for (int i = 0; i < geographicAreaTypes.Count(); i++)
                {
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Code, geographicAreaTypes.ElementAt(i).Code);
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Description, geographicAreaTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsGeographicAreaTypesNonCacheAsync()
            {
                var geographicAreaTypes = await referenceDataRepo.GetGeographicAreaTypesAsync(true);
                for (int i = 0; i < geographicAreaTypes.Count(); i++)
                {
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Code, geographicAreaTypes.ElementAt(i).Code);
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Description, geographicAreaTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetGeographicAreaTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(geographicAreaTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of geographic area types was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<GeographicAreaType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_INTG.GEO.AREA.TYPES"), null)).Returns(true);
                var geographicAreaTypes = await referenceDataRepo.GetGeographicAreaTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_INTG.GEO.AREA.TYPES"), null)).Returns(geographicAreaTypes);
                // Verify that geographic area types were returned, which means they came from the "repository".
                Assert.IsTrue(geographicAreaTypes.Count() == 4);

                // Verify that the geographic area type item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<GeographicAreaType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetGeographicAreaTypes_GetsCachedGeographicAreaTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.GEO.AREA.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allGeographicAreaTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.GEO.AREA.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the geographic area types are returned
                Assert.IsTrue((await referenceDataRepo.GetGeographicAreaTypesAsync(false)).Count() == 4);
                // Verify that the geographic area types were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to citizenship status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.GEO.AREA.TYPES", It.IsAny<bool>())).ReturnsAsync(geographicAreaTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var geographicAreaType = allGeographicAreaTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "INTG.GEO.AREA.TYPES", geographicAreaType.Code }),
                            new RecordKeyLookupResult() { Guid = geographicAreaType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<GeographicAreaType> geographicAreaTypes)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in geographicAreaTypes)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "" /*newType*/, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Identity Document Types
        /// </summary>
        [TestClass]
        public class IdentityDocumentTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<IdentityDocumentType> allIdentityDocumentTypes;
            ApplValcodes identityDocumentTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build identity document types responses used for mocking
                allIdentityDocumentTypes = new TestIdentityDocumentTypeRepository().Get();
                identityDocumentTypeValcodeResponse = BuildValcodeResponse(allIdentityDocumentTypes);

                // Build identity document type repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_INTG.IDENTITY.DOC.TYPES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                identityDocumentTypeValcodeResponse = null;
                allIdentityDocumentTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsIdentityDocumentTypesCacheAsync()
            {
                var identityDocumentTypes = await referenceDataRepo.GetIdentityDocumentTypesAsync(false);
                for (int i = 0; i < identityDocumentTypes.Count(); i++)
                {
                    Assert.AreEqual(allIdentityDocumentTypes.ElementAt(i).Code, identityDocumentTypes.ElementAt(i).Code);
                    Assert.AreEqual(allIdentityDocumentTypes.ElementAt(i).Description, identityDocumentTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsIdentityDocumentTypesNonCacheAsync()
            {
                var identityDocumentTypes = await referenceDataRepo.GetIdentityDocumentTypesAsync(true);
                for (int i = 0; i < identityDocumentTypes.Count(); i++)
                {
                    Assert.AreEqual(allIdentityDocumentTypes.ElementAt(i).Code, identityDocumentTypes.ElementAt(i).Code);
                    Assert.AreEqual(allIdentityDocumentTypes.ElementAt(i).Description, identityDocumentTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetIdentityDocumentTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(identityDocumentTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of identity document types was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<IdentityDocumentType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_INTG.IDENTITY.DOC.TYPES"), null)).Returns(true);
                var identityDocumentTypes = await referenceDataRepo.GetIdentityDocumentTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_INTG.IDENTITY.DOC.TYPES"), null)).Returns(identityDocumentTypes);
                // Verify that identity document types were returned, which means they came from the "repository".
                Assert.IsTrue(identityDocumentTypes.Count() == allIdentityDocumentTypes.Count());

                // Verify that the identity document type item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<IdentityDocumentType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetIdentityDocumentTypes_GetsCachedIdentityDocumentTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.IDENTITY.DOC.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allIdentityDocumentTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.IDENTITY.DOC.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the identity document types are returned
                Assert.IsTrue((await referenceDataRepo.GetIdentityDocumentTypesAsync(false)).Count() == allIdentityDocumentTypes.Count());
                // Verify that the identity document types were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to identity document types valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.IDENTITY.DOC.TYPES", It.IsAny<bool>())).ReturnsAsync(identityDocumentTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var identityDocumentType = allIdentityDocumentTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "INTG.IDENTITY.DOC.TYPES", identityDocumentType.Code }),
                            new RecordKeyLookupResult() { Guid = identityDocumentType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<IdentityDocumentType> identityDocumentTypes)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in identityDocumentTypes)
                {
                    string newType = "";
                    switch (item.IdentityDocumentTypeCategory)
                    {
                       
                        case IdentityDocumentTypeCategory.Passport:
                            newType = "PASSPORT";
                            break;
                        case IdentityDocumentTypeCategory.PhotoId:
                            newType = "LICENSE";
                            break;
                        case IdentityDocumentTypeCategory.Other:
                            newType = "OTHER";
                            break;
                        default:
                            newType = "OTHER";
                            break;
                    }
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, newType, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Instructional Platform codes
        /// </summary>
        [TestClass]
        public class InstructionalPlatforms
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            List<InstructionalPlatform> _allInstructionalPlatform;
            ApplValcodes _instructionalPlatformValcodeResponse;
            string _valcodeName;
            private ApiSettings apiSettings;
            ReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build instructional platforms responses used for mocking
                _allInstructionalPlatform = new List<InstructionalPlatform>
                {
                    new InstructionalPlatform("840e72f0-57b9-42a2-ae88-df3c2262fbbc", "CE", "Continuing Education"),
                    new InstructionalPlatform("e986b8a5-25f3-4aa0-bd0e-90982865e749", "D", "Institutional"),
                    new InstructionalPlatform("b5cc288b-8692-474e-91be-bdc55778e2f5", "TR", "Transfer")
                };

                _instructionalPlatformValcodeResponse = BuildValcodeResponse(_allInstructionalPlatform);


                // Build instructional platforms repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("UT_PORTAL.LEARN.TARGETS_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _instructionalPlatformValcodeResponse = null;
                _allInstructionalPlatform = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsInstructionalPlatformsCacheAsync()
            {
                for (var i = 0; i < _allInstructionalPlatform.Count(); i++)
                {
                    Assert.AreEqual(_allInstructionalPlatform.ElementAt(i).Code, (await _referenceDataRepo.GetInstructionalPlatformsAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(_allInstructionalPlatform.ElementAt(i).Description, (await _referenceDataRepo.GetInstructionalPlatformsAsync(false)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsInstructionalPlatformsNonCacheAsync()
            {
                for (var i = 0; i < _allInstructionalPlatform.Count(); i++)
                {
                    Assert.AreEqual(_allInstructionalPlatform.ElementAt(i).Code, (await _referenceDataRepo.GetInstructionalPlatformsAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(_allInstructionalPlatform.ElementAt(i).Description, (await _referenceDataRepo.GetInstructionalPlatformsAsync(true)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetInstructionalPlatforms_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "PORTAL.LEARN.TARGETS", It.IsAny<bool>())).ReturnsAsync(_instructionalPlatformValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of instructional platforms was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<InstructionalPlatform>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("UT_PORTAL.LEARN.TARGETS"), null)).Returns(true);
                var instructionalPlatforms = await _referenceDataRepo.GetInstructionalPlatformsAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("UT_PORTAL.LEARN.TARGETS"), null)).Returns(instructionalPlatforms);
                // Verify that instructional platforms were returned, which means they came from the "repository".
                Assert.IsTrue(instructionalPlatforms.Count() == 3);

                // Verify that the instructional platform item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<InstructionalPlatform>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task GetInstructionalPlatforms_GetsCachedInstructionalPlatformsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PORTAL.LEARN.TARGETS" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allInstructionalPlatform).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "PORTAL.LEARN.TARGETS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the instructional platforms are returned
                Assert.IsTrue((await _referenceDataRepo.GetInstructionalPlatformsAsync(false)).Count() == 3);
                // Verify that the instructional platforms were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to ethnicity valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("UT.VALCODES", "PORTAL.LEARN.TARGETS", It.IsAny<bool>())).ReturnsAsync(_instructionalPlatformValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var instructionalPlatform = _allInstructionalPlatform.FirstOrDefault(e => e.Code == recordKeyLookup.SecondaryKey);
                        result.Add(string.Join("+", new string[] { "UT.VALCODES", "PORTAL.LEARN.TARGETS", instructionalPlatform.Code }),
                            new RecordKeyLookupResult() { Guid = instructionalPlatform.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new ReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object, apiSettings);

                return _referenceDataRepo;
            }

            private static ApplValcodes BuildValcodeResponse(IEnumerable<InstructionalPlatform> instructionalPlatforms)
            {
                var ethnicitiesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                };
                foreach (var item in instructionalPlatforms)
                {
                    ethnicitiesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return ethnicitiesResponse;
            }
        }
        
        /// <summary>
        /// Test class for Interests
        /// </summary>
        [TestClass]
        public class Interests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Interest> allInterests;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allInterests = new TestInterestsRepository().GetInterests();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllHedmInterests");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allInterests = null;
                referenceDataRepo = null;
                codeItemName = null;
            }


            [TestMethod]
            public async Task ReferenceDataRepository_GetInterests_NonCache()
            {
                var interests = await referenceDataRepo.GetInterestsAsync(true);

                for (int i = 0; i < allInterests.Count(); i++)
                {
                    Assert.AreEqual(allInterests.ElementAt(i).Guid, interests.ElementAt(i).Guid);
                    Assert.AreEqual(allInterests.ElementAt(i).Code, interests.ElementAt(i).Code);
                    Assert.AreEqual(allInterests.ElementAt(i).Description, interests.ElementAt(i).Description);
                    Assert.AreEqual(allInterests.ElementAt(i).Type, interests.ElementAt(i).Type);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetsInterests_Cache()
            {
                var interests = await referenceDataRepo.GetInterestsAsync(false);

                for (int i = 0; i < allInterests.Count(); i++)
                {
                    Assert.AreEqual(allInterests.ElementAt(i).Guid, interests.ElementAt(i).Guid);
                    Assert.AreEqual(allInterests.ElementAt(i).Code, interests.ElementAt(i).Code);
                    Assert.AreEqual(allInterests.ElementAt(i).Description, interests.ElementAt(i).Description);
                    Assert.AreEqual(allInterests.ElementAt(i).Type, interests.ElementAt(i).Type);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Interests read
                var interestsCollection = new Collection<Data.Base.DataContracts.Interests>(allInterests.Select(record =>
                    new Data.Base.DataContracts.Interests()
                    {
                        Recordkey = record.Code,
                        IntDesc = record.Description,
                        IntType = record.Type,
                        RecordGuid = record.Guid
                    }).ToList());


                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Interests>("INTERESTS", "", true))
                    .ReturnsAsync(interestsCollection);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allInterests.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "INTERESTS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Interest Types
        /// </summary>
        [TestClass]
        public class InterestTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<InterestType> allInterestTypes;
            ApplValcodes interestTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build interest type responses used for mocking
                allInterestTypes = new TestInterestTypesRepository().GetInterestTypes();
                interestTypeValcodeResponse = BuildInterestTypesValcodeResponse(allInterestTypes);


                // Build interestType repository
                referenceDataRepo = BuildInterestTypesValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_INTEREST.TYPES_GUID");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                interestTypeValcodeResponse = null;
                allInterestTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task ReferenceDataRepository_InterestTypes_GetInterestTypes_Cache()
            {
                var interestTypes = await referenceDataRepo.GetInterestTypesAsync(false);
                for (int i = 0; i < allInterestTypes.Count(); i++)
                {
                    Assert.AreEqual(allInterestTypes.ElementAt(i).Code, interestTypes.ElementAt(i).Code);
                    Assert.AreEqual(allInterestTypes.ElementAt(i).Description, interestTypes.ElementAt(i).Description);
                   Assert.AreEqual(allInterestTypes.ElementAt(i).Guid, interestTypes.ElementAt(i).Guid);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_InterestTypes_GetInterestTypes_NonCache()
            {
                var interestTypes = await referenceDataRepo.GetInterestTypesAsync(true);
                
                for (int i = 0; i < allInterestTypes.Count(); i++)
                {
                    Assert.AreEqual(allInterestTypes.ElementAt(i).Code, interestTypes.ElementAt(i).Code);
                    Assert.AreEqual(allInterestTypes.ElementAt(i).Description, interestTypes.ElementAt(i).Description);
                    Assert.AreEqual(allInterestTypes.ElementAt(i).Guid, interestTypes.ElementAt(i).Guid);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_InterestTypes_GetInterestTypes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTEREST.TYPES", It.IsAny<bool>())).ReturnsAsync(interestTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of phoneTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<PhoneType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_INTEREST.TYPES"), null)).Returns(true);
                var interestTypes = await referenceDataRepo.GetInterestTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_INTEREST.TYPES"), null)).Returns(interestTypes);

                // Verify that types were returned, which means they came from the "repository".
                Assert.IsTrue(interestTypes.Count() == allInterestTypes.Count());

                // Verify that the type item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(valcodeName, It.IsAny<Task<List<PersonNameType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task ReferenceDataRepository_InterestTypes_GetInterestTypes_GetsCachedInterestTypes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item 
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allInterestTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "INTEREST.TYPES", true)).Returns(new ApplValcodes());

                // Assert the types are returned
                Assert.IsTrue((await referenceDataRepo.GetInterestTypesAsync(false)).Count() == allInterestTypes.Count());
                // Verify that the types were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildInterestTypesValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to phoneType valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTEREST.TYPES", It.IsAny<bool>())).ReturnsAsync(interestTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var personNameType = allInterestTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "INTEREST.TYPES", personNameType.Code }),
                            new RecordKeyLookupResult() { Guid = personNameType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildInterestTypesValcodeResponse(IEnumerable<InterestType> interestTypes)
            {
                ApplValcodes interestTypesResponse = new ApplValcodes();
                interestTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in interestTypes)
                {
                    interestTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return interestTypesResponse;
            }
        }
       
        /// <summary>
        /// Test class for Language codes
        /// </summary>
        [TestClass]
        public class LocationsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Location> allLocations;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allLocations = new TestLocationRepository().Get();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllLocations");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allLocations = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public void GetsLocationsNoArg()
            {
                for (int i = 0; i < allLocations.Count(); i++)
                {
                    Assert.AreEqual(allLocations.ElementAt(i).Guid, referenceDataRepo.Locations.ElementAt(i).Guid);
                    Assert.AreEqual(allLocations.ElementAt(i).Code, referenceDataRepo.Locations.ElementAt(i).Code);
                    Assert.AreEqual(allLocations.ElementAt(i).Description, referenceDataRepo.Locations.ElementAt(i).Description);
                    CollectionAssert.AreEqual(allLocations.ElementAt(i).AddressLines, referenceDataRepo.Locations.ElementAt(i).AddressLines);
                    CollectionAssert.AreEqual(allLocations.ElementAt(i).BuildingCodes, referenceDataRepo.Locations.ElementAt(i).BuildingCodes);
                    Assert.AreEqual(allLocations.ElementAt(i).CampusLocation, referenceDataRepo.Locations.ElementAt(i).CampusLocation);
                    Assert.AreEqual(allLocations.ElementAt(i).City, referenceDataRepo.Locations.ElementAt(i).City);
                    Assert.AreEqual(allLocations.ElementAt(i).Country, referenceDataRepo.Locations.ElementAt(i).Country);
                    Assert.AreEqual(allLocations.ElementAt(i).PostalCode, referenceDataRepo.Locations.ElementAt(i).PostalCode);
                    Assert.AreEqual(allLocations.ElementAt(i).State, referenceDataRepo.Locations.ElementAt(i).State);
                    Assert.AreEqual(allLocations.ElementAt(i).HideInSelfServiceCourseSearch, referenceDataRepo.Locations.ElementAt(i).HideInSelfServiceCourseSearch);
                    Assert.AreEqual(allLocations.ElementAt(i).SortOrder, referenceDataRepo.Locations.ElementAt(i).SortOrder);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Locations read
                var locationsCollection = new Collection<Locations>(allLocations.Select(record =>
                    new Data.Base.DataContracts.Locations()
                    {
                        Recordkey = record.Code,
                        LocDesc = record.Description,
                        RecordGuid = record.Guid,
                        LocAddress = record.AddressLines,
                        LocCity = record.City,
                        LocState = record.State,
                        LocZip = record.PostalCode,
                        LocCountry = record.Country,
                        LocCampusLocation = record.CampusLocation,
                        LocBuildings = record.BuildingCodes.ToList(),
                        LocHideInSsCourseSearch = record.HideInSelfServiceCourseSearch ? "Y" : string.Empty,
                        LocSortOrder = record.SortOrder.HasValue ? record.SortOrder.Value : (int?)null
                    }).ToList());

                dataAccessorMock.Setup<Collection<Locations>>(acc => acc.BulkReadRecord<Locations>("LOCATIONS", It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(locationsCollection);

                dataAccessorMock.Setup<Dictionary<string, RecordKeyLookupResult>>(acc => acc.Select(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var location = allLocations.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "LOCATIONS", location.Code }),
                            new RecordKeyLookupResult() { Guid = location.Guid });
                    }
                    return result;
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

       /// <summary>
        /// Test class for Location Types
       /// </summary>
        [TestClass]
        public class LocationTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<LocationTypeItem> allLocationTypes;
            ApplValcodes locationTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build location type responses used for mocking
                allLocationTypes = new TestLocationTypeRepository().Get();
                locationTypeValcodeResponse = BuildValcodeResponse(allLocationTypes);

                // Build locationType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_ADREL.TYPES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                //localCacheMock = null;
                locationTypeValcodeResponse = null;
                allLocationTypes = null;
                referenceDataRepo = null;
            }
        
            [TestMethod]
            public async Task GetsLocationTypesCache()
            {
                for (int i = 0; i < allLocationTypes.Count(); i++)
                {
                    Assert.AreEqual(allLocationTypes.ElementAt(i).Code, (await referenceDataRepo.GetLocationTypesAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(allLocationTypes.ElementAt(i).Description, (await referenceDataRepo.GetLocationTypesAsync(false)).ElementAt(i).Description);
                    
                }
            }
     
            [TestMethod]
            public async Task GetsLocationTypesNonCache()
            {

                for (int i = 0; i < allLocationTypes.Count(); i++)
                {
                    Assert.AreEqual(allLocationTypes.ElementAt(i).Code, (await referenceDataRepo.GetLocationTypesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(allLocationTypes.ElementAt(i).Description, (await referenceDataRepo.GetLocationTypesAsync(true)).ElementAt(i).Description);
                    
                }
            }
         
            [TestMethod]
            public async Task GetLocationTypes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES", It.IsAny<bool>())).ReturnsAsync(locationTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                // But after data accessor read, set up mocking so we can verify the list of locationTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<LocationTypeItem>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_ADREL.TYPES"), null)).Returns(true);
                var locationTypes = await referenceDataRepo.GetLocationTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_ADREL.TYPES"), null)).Returns(locationTypes);

                // Verify that locationTypes were returned, which means they came from the "repository".
                Assert.IsTrue(locationTypes.Count() == 15);

                // Verify that the locationType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<LocationTypeItem>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }
       
            [TestMethod]
            public async Task GetLocationTypes_GetsCachedLocationTypes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "ADREL.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allLocationTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES", true)).Returns(new ApplValcodes());

                // Assert the locationTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetLocationTypesAsync(false)).Count() == 15);
                // Verify that the locationTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Setup localCacheMock as the object for the CacheProvider
                //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to location types valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ADREL.TYPES", It.IsAny<bool>())).ReturnsAsync(locationTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var locationType = allLocationTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "ADREL.TYPES", locationType.Code }),
                            new RecordKeyLookupResult() { Guid = locationType.Guid });
                    }
                    return Task.FromResult(result);
                });


                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<LocationTypeItem> locationTypes)
            {
                ApplValcodes locationTypesResponse = new ApplValcodes();
                locationTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in locationTypes)
                {
                    string entType = "";
                    switch (item.Type.EntityType)
                    {
                        case EntityType.Organization:
                            entType = "organization";
                            break;
                        default:
                            entType = "person";
                            break;
                    }

                    string perLocType = "";
                    switch (item.Type.PersonLocationType)
                    {
                        case PersonLocationType.Billing:
                            perLocType = "billing";
                            break;
                        case PersonLocationType.Business:
                            perLocType = "business";
                            break;
                        case PersonLocationType.Home:
                            perLocType = "home";
                            break;
                        case PersonLocationType.Mailing:
                            perLocType = "mailing";
                            break;
                        case PersonLocationType.Vacation:
                            perLocType = "vacation";
                            break;
                        case PersonLocationType.School:
                            perLocType = "school";
                            break;
                        case PersonLocationType.Shipping:
                            perLocType = "shipping";
                            break;
                        default:
                            perLocType = "";
                            break;
                    }

                    string orgLocType = "";
                    switch (item.Type.OrganizationLocationType)
                    {
                        case OrganizationLocationType.Branch:
                            orgLocType = "branch";
                            break;
                        case OrganizationLocationType.Business:
                            orgLocType = "business";
                            break;
                        case OrganizationLocationType.Main:
                            orgLocType = "main";
                            break;
                        case OrganizationLocationType.Support:
                            orgLocType = "support";
                            break;
                        case OrganizationLocationType.Pobox:
                            orgLocType = "pobox";
                            break;
                        case OrganizationLocationType.Region:
                            orgLocType = "regional";
                            break;
                        default:
                            orgLocType = "";
                            break;
                    }
                    locationTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", perLocType, orgLocType));
                }
                return locationTypesResponse;
            }
        }

        /// <summary>
        /// Test class for Marital Statuses
        /// </summary>
        [TestClass]
        public class MaritalStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<MaritalStatus> allMaritalStatuses;
            ApplValcodes maritalStatusValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build marital statuses responses used for mocking
                allMaritalStatuses = new TestMaritalStatusRepository().Get();
                maritalStatusValcodeResponse = BuildValcodeResponse(allMaritalStatuses);

                // Build marital statuses repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_MARITAL.STATUSES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                maritalStatusValcodeResponse = null;
                allMaritalStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsMaritalStatusesCacheAsync()
            {
                var maritalStatuses = await referenceDataRepo.GetMaritalStatusesAsync(false);
                for (int i = 0; i < maritalStatuses.Count(); i++)
                {
                    Assert.AreEqual(allMaritalStatuses.ElementAt(i).Code, maritalStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allMaritalStatuses.ElementAt(i).Description, maritalStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsMaritalStatusesNonCacheAsync()
            {
                var maritalStatuses = await referenceDataRepo.GetMaritalStatusesAsync(true);
                for (int i = 0; i < maritalStatuses.Count(); i++)
                {
                    Assert.AreEqual(allMaritalStatuses.ElementAt(i).Code, maritalStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allMaritalStatuses.ElementAt(i).Description, maritalStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task MaritalStatusesAsync()
            {
                var maritalStatuses = await referenceDataRepo.MaritalStatusesAsync();
                for (int i = 0; i < maritalStatuses.Count(); i++)
                {
                    Assert.AreEqual(allMaritalStatuses.ElementAt(i).Code, maritalStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allMaritalStatuses.ElementAt(i).Description, maritalStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetMaritalStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(maritalStatusValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of marital statuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<MaritalStatus>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_MARITAL.STATUSES"), null)).Returns(true);
                var maritalStatuses = await referenceDataRepo.GetMaritalStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_MARITAL.STATUSES"), null)).Returns(maritalStatuses);
                // Verify that marital statuses were returned, which means they came from the "repository".
                Assert.IsTrue(maritalStatuses.Count() == 5);

                // Verify that the marital status item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<MaritalStatus>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetMaritalStatuses_GetsCachedMaritalStatusesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "MARITAL.STATUSES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allMaritalStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MARITAL.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the marital statuses are returned
                Assert.IsTrue((await referenceDataRepo.GetMaritalStatusesAsync(false)).Count() == 5);
                // Verify that the marital statuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to marital status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MARITAL.STATUSES", It.IsAny<bool>())).ReturnsAsync(maritalStatusValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var maritalStatus = allMaritalStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "MARITAL.STATUSES", maritalStatus.Code }),
                            new RecordKeyLookupResult() { Guid = maritalStatus.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<MaritalStatus> maritalStatuses)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in maritalStatuses)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Communication codes
        /// </summary>
        [TestClass]
        public class CommunicationCodes : BaseRepositorySetup
        {

            public List<CommunicationCode> expectedCommunicationCodes
            {
                get { return expectedCommunicationCodesRepository.GetCommunicationCodeEntities().ToList(); }
            }
            public List<CommunicationCode> actualCommunicationCodes
            {
                get { return actualReferenceDataRepository.CommunicationCodes.ToList(); }
            }

            public ReferenceDataRepository actualReferenceDataRepository;
            public TestCommunicationCodesRepository expectedCommunicationCodesRepository;
            private ApiSettings apiSettings;

            [TestInitialize]
            public void TestInitialize()
            {
                MockInitialize();
                
                expectedCommunicationCodesRepository = new TestCommunicationCodesRepository();
  
                actualReferenceDataRepository = BuildReferenceDataRepository();

                apiSettings = new ApiSettings("TEST");
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedCommunicationCodes, actualCommunicationCodes);
            }

            [TestMethod]
            public void NullCorewebDefaultsLogsErrorTest()
            {
                expectedCommunicationCodesRepository.corewebDefaultsData = null;
                CollectionAssert.AreEqual(expectedCommunicationCodes, actualCommunicationCodes);
                loggerMock.Verify(l => l.Info("Null CorewebDefaults record returned from database"));                
            }

            [TestMethod]
            public void ErrorGettingCorewebDefaultsLogsErrorTest()
            {
                dataReaderMock.Setup(r => r.ReadRecord<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                    .Throws(new Exception("ex"));

                CollectionAssert.AreEqual(expectedCommunicationCodes, actualCommunicationCodes);
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), "Error retrieving COREWEB.DEFAULTS record"));
            }

            [TestMethod]
            public void NullCcCodesCollectionTest()
            {
                expectedCommunicationCodesRepository.communicationCodeData = null;
                Assert.AreNotEqual(0, actualCommunicationCodes);
                loggerMock.Verify(l => l.Error("Null CcCodes records returned by DataReader"));
            }

            [TestMethod]
            public void EmptyCcCodesCollectionTest()
            {
                expectedCommunicationCodesRepository.communicationCodeData = new List<TestCommunicationCodesRepository.CommunicationCodeRecord>();
                Assert.AreNotEqual(0, actualCommunicationCodes);
                loggerMock.Verify(l => l.Error("Null CcCodes records returned by DataReader"));
            }

            [TestMethod]
            public void NullCorewebDefaultsSetsIsStudentViewableToFalseForAllTest()
            {
                expectedCommunicationCodesRepository.corewebDefaultsData = null;
                Assert.IsTrue(actualCommunicationCodes.All(cc => !cc.IsStudentViewable));
            }

            [TestMethod]
            public void NullCodesFromCorewebDefaultsSetsIsStudentViewableToFalseForAllTest()
            {
                expectedCommunicationCodesRepository.corewebDefaultsData = new TestCommunicationCodesRepository.CorewebDefaultsRecord();
                Assert.IsTrue(actualCommunicationCodes.All(cc => !cc.IsStudentViewable));
            }

            [TestMethod]
            public void CodeContainedInViewableList_IsStudentViewableIsTrueTest()
            {
                expectedCommunicationCodesRepository.corewebDefaultsData.communicationCodesAsRequiredDocuments.Add("foobar");
                expectedCommunicationCodesRepository.communicationCodeData.Add(new TestCommunicationCodesRepository.CommunicationCodeRecord()
                    {
                        Guid = "5bce3016-b7de-4e7e-ac7f-b53714a15f1c",
                        RecordKey = "FOOBAR",
                        CcDescription = "description"
                    });

                CollectionAssert.AreEqual(expectedCommunicationCodes, actualCommunicationCodes);

                var actual = actualCommunicationCodes.First(c => c.Code == "FOOBAR");
                Assert.IsTrue(actual.IsStudentViewable);
            }

            [TestMethod]
            public void CodeNotContainedInViewableList_IsStudentViewableIsFalseTest()
            {
                expectedCommunicationCodesRepository.communicationCodeData.Add(new TestCommunicationCodesRepository.CommunicationCodeRecord()
                {
                    Guid = "5bce3016-b7de-4e7e-ac7f-b53714a15f1c",
                    RecordKey = "FOOBAR",
                    CcDescription = "description"
                });

                CollectionAssert.AreEqual(expectedCommunicationCodes, actualCommunicationCodes);

                var actual = actualCommunicationCodes.First(c => c.Code == "FOOBAR");
                Assert.IsFalse(actual.IsStudentViewable);
            }

            [TestMethod]
            public void AllowAttachments_IsTrueWhenY_Test()
            {
                expectedCommunicationCodesRepository.communicationCodeData.Add(new TestCommunicationCodesRepository.CommunicationCodeRecord()
                {
                    Guid = "5bce3016-b7de-4e7e-ac7f-b53714a15f1c",
                    RecordKey = "FOOBAR",
                    CcDescription = "description",
                    CcAllowsAttachments = "y"
                });

                CollectionAssert.AreEqual(expectedCommunicationCodes, actualCommunicationCodes);

                var actual = actualCommunicationCodes.First(c => c.Code == "FOOBAR");
                Assert.IsTrue(actual.AllowsAttachments);
            }

            [TestMethod]
            public void AllowAttachments_IsFalseWhenN_Test()
            {
                expectedCommunicationCodesRepository.communicationCodeData.Add(new TestCommunicationCodesRepository.CommunicationCodeRecord()
                {
                    Guid = "5bce3016-b7de-4e7e-ac7f-b53714a15f1c",
                    RecordKey = "FOOBAR",
                    CcDescription = "description",
                    CcAllowsAttachments = "n"
                });

                CollectionAssert.AreEqual(expectedCommunicationCodes, actualCommunicationCodes);

                var actual = actualCommunicationCodes.First(c => c.Code == "FOOBAR");
                Assert.IsFalse(actual.AllowsAttachments);
            }
            [TestMethod]
            public void AllowAttachments_IsFalseWhenNull_Test()
            {
                CollectionAssert.AreEqual(expectedCommunicationCodes, actualCommunicationCodes);

                var actual = actualCommunicationCodes.First(c => c.Code == "CcCode1");
                Assert.IsFalse(actual.AllowsAttachments);
            }

            [TestMethod]
            public void NullUrlsInRecord_EmptyHyperlinksList()
            {
                expectedCommunicationCodesRepository.communicationCodeData.ForEach(cc => cc.CcUrls = null);
                Assert.IsTrue(actualCommunicationCodes.All(cc => cc.Hyperlinks.Count() == 0));
            }

            [TestMethod]
            public void ErrorCreatingCommunicationCode_LogsDataErrorTest()
            {
                expectedCommunicationCodesRepository.communicationCodeData.Add(new TestCommunicationCodesRepository.CommunicationCodeRecord()
                    {
                        RecordKey = "FOOBAR",
                        CcDescription = ""
                    });

                Assert.IsNull(actualCommunicationCodes.FirstOrDefault(c => c.Code == "FOOBAR"));
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            }


            /// <summary>
            /// Helper method to set up the "actual" ReferenceDataRepository
            /// </summary>
            /// <returns>ReferenceDataRepository</returns>
            private ReferenceDataRepository BuildReferenceDataRepository()
            {
                dataReaderMock.Setup(r => r.ReadRecord<CorewebDefaults>("CORE.PARMS", "COREWEB.DEFAULTS", true))
                    .Returns<string, string, bool>((e, r, b) =>
                        expectedCommunicationCodesRepository.corewebDefaultsData == null ? null :
                        new CorewebDefaults()
                        {
                            CorewebCcCodes = expectedCommunicationCodesRepository.corewebDefaultsData.communicationCodesAsRequiredDocuments
                        });

                dataReaderMock.Setup(r => r.BulkReadRecord<CcCodes>("", true))
                    .Returns<string, bool>((s, b) =>
                        expectedCommunicationCodesRepository.communicationCodeData == null ? null :
                        new Collection<CcCodes>(expectedCommunicationCodesRepository.communicationCodeData.Select(record =>
                            new CcCodes()
                            {
                                RecordGuid = record.Guid,
                                Recordkey = record.RecordKey,
                                CcDescription = record.CcDescription,
                                CcExplanation = record.CcExplanation,
                                CcFaYear = record.CcFaYear,
                                CcOffice = record.CcOffice,
                                CcAllowAttachments = record.CcAllowsAttachments,
                                CcUrlsEntityAssociation = record.CcUrls == null ? null : record.CcUrls.Select(url =>
                                    new CcCodesCcUrls()
                                    {
                                        CcUrlAssocMember = url.url,
                                        CcTitleAssocMember = url.title
                                    }).ToList()
                            }).ToList()));

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
                loggerMock.Setup(l => l.IsWarnEnabled).Returns(true);
                loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

                return new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);           
            }
        }


        [TestClass]
        public class AdmissionApplicationSupportingItemTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<CommunicationCode> allAdmissionApplicationSupportingItemTypes;
            string codeItemName;
            ReferenceDataRepository referenceDataRepo;
            private ApiSettings apiSettings;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allAdmissionApplicationSupportingItemTypes = new TestAdmissionApplicationSupportingItemTypeRepository().Get();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllCommunicationCodesHedm");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                referenceDataRepo = null;
                allAdmissionApplicationSupportingItemTypes = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task GetsAdmissionApplicationSupportingItemTypesCache()
            {
                var results = await referenceDataRepo.GetAdmissionApplicationSupportingItemTypesAsync(false);
                // only first 2 records in TestAdmissionApplicationSupportingItemTypesRepository setup for valid office codes
                for (int i = 0; i < 2; i++)
                {
                    Assert.AreEqual(allAdmissionApplicationSupportingItemTypes.ElementAt(i).Guid, results.ElementAt(i).Guid);
                    Assert.AreEqual(allAdmissionApplicationSupportingItemTypes.ElementAt(i).Code, results.ElementAt(i).Code);
                    Assert.AreEqual(allAdmissionApplicationSupportingItemTypes.ElementAt(i).Description, results.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsAdmissionApplicationSupportingItemTypesNonCache()
            {
                var results = await referenceDataRepo.GetAdmissionApplicationSupportingItemTypesAsync(false);
                // only first 2 records in TestAdmissionApplicationSupportingItemTypesRepository setup for valid office codes
                for (int i = 0; i < 2; i++)
                {
                    Assert.AreEqual(allAdmissionApplicationSupportingItemTypes.ElementAt(i).Guid, results.ElementAt(i).Guid);
                    Assert.AreEqual(allAdmissionApplicationSupportingItemTypes.ElementAt(i).Code, results.ElementAt(i).Code);
                    Assert.AreEqual(allAdmissionApplicationSupportingItemTypes.ElementAt(i).Description, results.ElementAt(i).Description);
                }
            }
            
            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                
                var AdmissionApplicationSupportingItemTypesCollection = new Collection<CcCodes>(allAdmissionApplicationSupportingItemTypes.Select(record =>
                    new Data.Base.DataContracts.CcCodes()
                    {
                        Recordkey = record.Code,
                        CcDescription = record.Description,
                        RecordGuid = record.Guid,
                        CcOffice = record.OfficeCodeId

                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<CcCodes>("CC.CODES", "", true))
                    .ReturnsAsync(AdmissionApplicationSupportingItemTypesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup<Collection<CcCodes>>(acc => acc.BulkReadRecord<CcCodes>("AdmissionApplicationSupportingItemTypes", "", true))
                    .Returns(AdmissionApplicationSupportingItemTypesCollection);

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var AdmissionApplicationSupportingItemType = allAdmissionApplicationSupportingItemTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CC.CODES", AdmissionApplicationSupportingItemType.Code }),
                            new RecordKeyLookupResult() { Guid = AdmissionApplicationSupportingItemType.Guid });
                    }
                    return Task.FromResult(result);
                });

                string fileName = "CORE.PARMS";
                string field = "LDM.DEFAULTS";
                var officeCodes = new List<string>();
                officeCodes.Add("ADM");
                officeCodes.Add("REG");
                LdmDefaults ldmDefaults = new LdmDefaults() {LdmdDfltAdmOfficeCodes = officeCodes};
                dataAccessorMock.Setup(i => i.ReadRecord<LdmDefaults>(fileName, field, It.IsAny<bool>())).Returns(ldmDefaults);
                
                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class GetLocationsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Location> allLocations;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allLocations = new TestLocationRepository().Get();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllLocations");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allLocations = null;
                referenceDataRepo = null;
                allLocations = null;
                loggerMock = null;
            }

            [TestMethod]
            public void GetsLocationsCache()
            {
                for (int i = 0; i < allLocations.Count(); i++)
                {
                    var expected = allLocations.ElementAt(i);
                    var actual = referenceDataRepo.GetLocations(false).ElementAt(i);
                    Assert.AreEqual(expected.Guid, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    CollectionAssert.AreEqual(expected.AddressLines, actual.AddressLines);
                    CollectionAssert.AreEqual(expected.BuildingCodes, actual.BuildingCodes);
                    Assert.AreEqual(expected.CampusLocation, actual.CampusLocation);
                    Assert.AreEqual(expected.City, actual.City);
                    Assert.AreEqual(expected.Country, actual.Country);
                    Assert.AreEqual(expected.PostalCode, actual.PostalCode);
                    Assert.AreEqual(expected.State, actual.State);
                    Assert.AreEqual(expected.HideInSelfServiceCourseSearch, actual.HideInSelfServiceCourseSearch);
                    Assert.AreEqual(expected.SortOrder, actual.SortOrder);
                }
            }

            [TestMethod]
            public void GetsLocationsNonCache()
            {
                for (int i = 0; i < allLocations.Count(); i++)
                {
                    var expected = allLocations.ElementAt(i);
                    var actual = referenceDataRepo.GetLocations(true).ElementAt(i);
                    Assert.AreEqual(expected.Guid, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    CollectionAssert.AreEqual(expected.AddressLines, actual.AddressLines);
                    CollectionAssert.AreEqual(expected.BuildingCodes, actual.BuildingCodes);
                    Assert.AreEqual(expected.CampusLocation, actual.CampusLocation);
                    Assert.AreEqual(expected.City, actual.City);
                    Assert.AreEqual(expected.Country, actual.Country);
                    Assert.AreEqual(expected.PostalCode, actual.PostalCode);
                    Assert.AreEqual(expected.State, actual.State);
                    Assert.AreEqual(expected.HideInSelfServiceCourseSearch, actual.HideInSelfServiceCourseSearch);
                    Assert.AreEqual(expected.SortOrder, actual.SortOrder);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Other Ccds read
                var locationsCollection = new Collection<Locations>(allLocations.Select(record =>
                    new Data.Base.DataContracts.Locations()
                    {
                        Recordkey = record.Code,
                        LocDesc = record.Description,
                        RecordGuid = record.Guid,
                        LocAddress = record.AddressLines,
                        LocCity = record.City,
                        LocState = record.State,
                        LocZip = record.PostalCode,
                        LocCountry = record.Country,
                        LocCampusLocation = record.CampusLocation,
                        LocBuildings = record.BuildingCodes.ToList(),
                        LocHideInSsCourseSearch = record.HideInSelfServiceCourseSearch ? "Y" : string.Empty,
                        LocSortOrder = record.SortOrder.HasValue ? record.SortOrder.Value : (int?)null
                    }).ToList());

                dataAccessorMock.Setup<Collection<Locations>>(acc => acc.BulkReadRecord<Locations>("LOCATIONS", It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(locationsCollection);

                dataAccessorMock.Setup<Dictionary<string, RecordKeyLookupResult>>(acc => acc.Select(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var location = allLocations.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "LOCATIONS", location.Code }),
                            new RecordKeyLookupResult() { Guid = location.Guid });
                    }
                    return result;
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class GetLocationsAsyncTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Location> allLocations;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allLocations = new TestLocationRepository().Get();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllLocations");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allLocations = null;
                referenceDataRepo = null;
                allLocations = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task GetsLocationsAsyncCache()
            {
                for (int i = 0; i < allLocations.Count(); i++)
                {
                    var expected = allLocations.ElementAt(i);
                    var asyncLocations = (await referenceDataRepo.GetLocationsAsync(false));
                    var actual = (await referenceDataRepo.GetLocationsAsync(false)).ElementAt(i);
                    Assert.AreEqual(expected.Guid, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    CollectionAssert.AreEqual(expected.AddressLines, actual.AddressLines);
                    CollectionAssert.AreEqual(expected.BuildingCodes, actual.BuildingCodes);
                    Assert.AreEqual(expected.CampusLocation, actual.CampusLocation);
                    Assert.AreEqual(expected.City, actual.City);
                    Assert.AreEqual(expected.Country, actual.Country);
                    Assert.AreEqual(expected.PostalCode, actual.PostalCode);
                    Assert.AreEqual(expected.State, actual.State);
                    Assert.AreEqual(expected.HideInSelfServiceCourseSearch, actual.HideInSelfServiceCourseSearch);
                    Assert.AreEqual(expected.SortOrder, actual.SortOrder);
                }
            }

            [TestMethod]
            public async Task GetsLocationsAsyncNonCache()
            {
                for (int i = 0; i < allLocations.Count(); i++)
                {
                    var expected = allLocations.ElementAt(i);
                    var actual = (await referenceDataRepo.GetLocationsAsync(true)).ElementAt(i);
                    Assert.AreEqual(expected.Guid, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    CollectionAssert.AreEqual(expected.AddressLines, actual.AddressLines);
                    CollectionAssert.AreEqual(expected.BuildingCodes, actual.BuildingCodes);
                    Assert.AreEqual(expected.CampusLocation, actual.CampusLocation);
                    Assert.AreEqual(expected.City, actual.City);
                    Assert.AreEqual(expected.Country, actual.Country);
                    Assert.AreEqual(expected.PostalCode, actual.PostalCode);
                    Assert.AreEqual(expected.State, actual.State);
                    Assert.AreEqual(expected.HideInSelfServiceCourseSearch, actual.HideInSelfServiceCourseSearch);
                    Assert.AreEqual(expected.SortOrder, actual.SortOrder);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Other Ccds read
                var locationsCollection = new Collection<Locations>(allLocations.Select(record =>
                    new Data.Base.DataContracts.Locations()
                    {
                        Recordkey = record.Code,
                        LocDesc = record.Description,
                        RecordGuid = record.Guid,
                        LocAddress = record.AddressLines,
                        LocCity = record.City,
                        LocState = record.State,
                        LocZip = record.PostalCode,
                        LocCountry = record.Country,
                        LocCampusLocation = record.CampusLocation,
                        LocBuildings = record.BuildingCodes.ToList(),
                        LocHideInSsCourseSearch = record.HideInSelfServiceCourseSearch ? "Y" : string.Empty,
                        LocSortOrder = record.SortOrder.HasValue ? record.SortOrder.Value : (int?)null
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Locations>("LOCATIONS", It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(Task.FromResult(locationsCollection));

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var location = allLocations.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "LOCATIONS", location.Code }),
                            new RecordKeyLookupResult() { Guid = location.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }


        /// <summary>
        /// Test class for MilStatuses
        /// </summary>
        [TestClass]
        public class MilStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.MilStatuses> allMilStatuses;
            ApplValcodes milStatusValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build milStatuses responses used for mocking
                allMilStatuses = new TestMilStatusesRepository().Get();
                milStatusValcodeResponse = BuildValcodeResponse(allMilStatuses);

                // Build race repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_MIL.STATUSES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                milStatusValcodeResponse = null;
                allMilStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsMilStatusesCacheAsync()
            {
                var milStatuses = await referenceDataRepo.GetMilStatusesAsync(false);
                for (int i = 0; i < milStatuses.Count(); i++)
                {
                    Assert.AreEqual(allMilStatuses.ElementAt(i).Code, milStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allMilStatuses.ElementAt(i).Description, milStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsMilStatusesNonCacheAsync()
            {
                var milStatuses = await referenceDataRepo.GetMilStatusesAsync(true);
                for (int i = 0; i < milStatuses.Count(); i++)
                {
                    Assert.AreEqual(allMilStatuses.ElementAt(i).Code, milStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allMilStatuses.ElementAt(i).Description, milStatuses.ElementAt(i).Description);
                }
            }

       
            [TestMethod]
            public async Task GetMilStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(milStatusValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of milStatuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<Race>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_MIL.STATUSES"), null)).Returns(true);
                var milStatuses = await referenceDataRepo.GetMilStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_MIL.STATUSES"), null)).Returns(milStatuses);
                // Verify that milStatuses were returned, which means they came from the "repository".
                Assert.IsTrue(milStatuses.Count() == 3);

                // Verify that the race item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<Race>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetMilStatuses_GetsCachedMilStatusesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PERSON.EMAIL.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allMilStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MIL.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the milStatuses are returned
                Assert.IsTrue((await referenceDataRepo.GetMilStatusesAsync(false)).Count() == 3);
                // Verify that the milStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to race valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "MIL.STATUSES", It.IsAny<bool>())).ReturnsAsync(milStatusValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var race = allMilStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "MIL.STATUSES", race.Code }),
                            new RecordKeyLookupResult() { Guid = race.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<Ellucian.Colleague.Domain.Base.Entities.MilStatuses> milStatuses)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in milStatuses)
                {
                    string newType = "";
                    /*switch (item.Type)
                    {
                        case RaceType.AmericanIndian:
                            newType = "1";
                            break;
                        case RaceType.Asian:
                            newType = "2";
                            break;
                        case RaceType.Black:
                            newType = "3";
                            break;
                        case RaceType.PacificIslander:
                            newType = "4";
                            break;
                        default:
                            newType = "5";
                            break;
                    }*/
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, newType, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        [TestClass]
        public class OtherCcdsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<OtherCcd> allOtherCcds;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allOtherCcds = new TestAcademicCredentialsRepository().GetOtherCcds();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("BaseAllOtherCcds");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allOtherCcds = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public void GetsOtherCcdsCache()
            {
                for (int i = 0; i < allOtherCcds.Count(); i++)
                {
                    Assert.AreEqual(allOtherCcds.ElementAt(i).Guid, referenceDataRepo.GetOtherCcds(false).ElementAt(i).Guid);
                    Assert.AreEqual(allOtherCcds.ElementAt(i).Code, referenceDataRepo.GetOtherCcds(false).ElementAt(i).Code);
                    Assert.AreEqual(allOtherCcds.ElementAt(i).Description, referenceDataRepo.GetOtherCcds(false).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public void GetsOtherCcdsNonCache()
            {
                for (int i = 0; i < allOtherCcds.Count(); i++)
                {
                    Assert.AreEqual(allOtherCcds.ElementAt(i).Guid, referenceDataRepo.GetOtherCcds(true).ElementAt(i).Guid);
                    Assert.AreEqual(allOtherCcds.ElementAt(i).Code, referenceDataRepo.GetOtherCcds(true).ElementAt(i).Code);
                    Assert.AreEqual(allOtherCcds.ElementAt(i).Description, referenceDataRepo.GetOtherCcds(true).ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Other Ccds read
                var otherCcdsCollection = new Collection<OtherCcds>(allOtherCcds.Select(record =>
                    new Data.Base.DataContracts.OtherCcds()
                    {
                        Recordkey = record.Code,
                        OccdDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup<Collection<OtherCcds>>(acc => acc.BulkReadRecord<OtherCcds>("OTHER.CCDS", "", true))
                    .Returns(otherCcdsCollection);

                dataAccessorMock.Setup<Dictionary<string, RecordKeyLookupResult>>(acc => acc.Select(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var otherCcd = allOtherCcds.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "OTHER.CCDS", otherCcd.Code }),
                            new RecordKeyLookupResult() { Guid = otherCcd.Guid });
                    }
                    return result;
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }
        
        [TestClass]
        public class OtherDegreesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<OtherDegree> allOtherDegrees;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allOtherDegrees = new TestAcademicCredentialsRepository().GetOtherDegrees();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("BaseAllOtherDegrees");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allOtherDegrees = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public void GetsOtherDegreesCache()
            {
                for (int i = 0; i < allOtherDegrees.Count(); i++)
                {
                    Assert.AreEqual(allOtherDegrees.ElementAt(i).Guid, referenceDataRepo.GetOtherDegrees(false).ElementAt(i).Guid);
                    Assert.AreEqual(allOtherDegrees.ElementAt(i).Code, referenceDataRepo.GetOtherDegrees(false).ElementAt(i).Code);
                    Assert.AreEqual(allOtherDegrees.ElementAt(i).Description, referenceDataRepo.GetOtherDegrees(false).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public void GetsOtherDegreesNonCache()
            {
                for (int i = 0; i < allOtherDegrees.Count(); i++)
                {
                    Assert.AreEqual(allOtherDegrees.ElementAt(i).Guid, referenceDataRepo.GetOtherDegrees(true).ElementAt(i).Guid);
                    Assert.AreEqual(allOtherDegrees.ElementAt(i).Code, referenceDataRepo.GetOtherDegrees(true).ElementAt(i).Code);
                    Assert.AreEqual(allOtherDegrees.ElementAt(i).Description, referenceDataRepo.GetOtherDegrees(true).ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Other Degrees read
                var otherDegreesCollection = new Collection<OtherDegrees>(allOtherDegrees.Select(record =>
                    new Data.Base.DataContracts.OtherDegrees()
                    {
                        Recordkey = record.Code,
                        OdegDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup<Collection<OtherDegrees>>(acc => acc.BulkReadRecord<OtherDegrees>("OTHER.DEGREES", "", true))
                    .Returns(otherDegreesCollection);

                dataAccessorMock.Setup<Dictionary<string, RecordKeyLookupResult>>(acc => acc.Select(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var otherDegree = allOtherDegrees.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "OTHER.DEGREES", otherDegree.Code }),
                            new RecordKeyLookupResult() { Guid = otherDegree.Guid });
                    }
                    return result;
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }
        
        /// <summary>
        /// Test class for Other Honors codes
        /// </summary>
        [TestClass]
        public class OtherHonorsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<OtherHonor> allOtherHonors;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allOtherHonors = new TestAcademicHonorsRepository().GetOtherHonors();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("BaseAllOtherHonors");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allOtherHonors = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsOtherHonorsCacheAsync()
            {
                var otherHonors = await referenceDataRepo.GetOtherHonorsAsync(false);

                for (int i = 0; i < allOtherHonors.Count(); i++)
                {
                    Assert.AreEqual(allOtherHonors.ElementAt(i).Guid, otherHonors.ElementAt(i).Guid);
                    Assert.AreEqual(allOtherHonors.ElementAt(i).Code, otherHonors.ElementAt(i).Code);
                    Assert.AreEqual(allOtherHonors.ElementAt(i).Description, otherHonors.ElementAt(i).Description);
                }
            }
         
            [TestMethod]
            public async Task GetsOtherHonorsNonCacheAsync()
            {
                var otherHonors = await referenceDataRepo.GetOtherHonorsAsync(true);
                
                for (int i = 0; i < allOtherHonors.Count(); i++)
                {
                    Assert.AreEqual(allOtherHonors.ElementAt(i).Guid, otherHonors.ElementAt(i).Guid);
                    Assert.AreEqual(allOtherHonors.ElementAt(i).Code, otherHonors.ElementAt(i).Code);
                    Assert.AreEqual(allOtherHonors.ElementAt(i).Description, otherHonors.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Other Honors read
                var otherHonorsCollection = new Collection<OtherHonors>(allOtherHonors.Select(record =>
                    new Data.Base.DataContracts.OtherHonors()
                    {
                        Recordkey = record.Code,
                        OhonDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<OtherHonors>("OTHER.HONORS", "", true))
                    .ReturnsAsync(otherHonorsCollection);
                
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var otherHonor = allOtherHonors.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "OTHER.HONORS", otherHonor.Code }),
                            new RecordKeyLookupResult() { Guid = otherHonor.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class OtherMajorsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<OtherMajor> allOtherMajors;
            string codeItemName;
            private ApiSettings apiSettings;
            
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allOtherMajors = new TestAcademicDisciplineRepository().GetOtherMajors();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllOtherMajors");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allOtherMajors = null;
                referenceDataRepo = null;
            }

           
            [TestMethod]
            public async Task GetsOtherMajorsCacheAsync()
            {
                IEnumerable<OtherMajor> otherMajors = await referenceDataRepo.GetOtherMajorsAsync(false);

                for (int i = 0; i < allOtherMajors.Count(); i++)
                {
                    Assert.AreEqual(allOtherMajors.ElementAt(i).Guid, otherMajors.ElementAt(i).Guid);
                    Assert.AreEqual(allOtherMajors.ElementAt(i).Code, otherMajors.ElementAt(i).Code);
                    Assert.AreEqual(allOtherMajors.ElementAt(i).Description, otherMajors.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsOtherMajorsNonCacheAsync()
            {
                IEnumerable<OtherMajor> otherMajors = await referenceDataRepo.GetOtherMajorsAsync(true);

                for (int i = 0; i < allOtherMajors.Count(); i++)
                {
                    Assert.AreEqual(allOtherMajors.ElementAt(i).Guid, otherMajors.ElementAt(i).Guid);
                    Assert.AreEqual(allOtherMajors.ElementAt(i).Code, otherMajors.ElementAt(i).Code);
                    Assert.AreEqual(allOtherMajors.ElementAt(i).Description, otherMajors.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Other Majors read
                var otherMajorsCollection = new Collection<OtherMajors>(allOtherMajors.Select(record =>
                    new Data.Base.DataContracts.OtherMajors()
                    {
                        Recordkey = record.Code,
                        OmajDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());


                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.OtherMajors>("OTHER.MAJORS", "", true))
                    .ReturnsAsync(otherMajorsCollection);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                
                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var record = allOtherMajors.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "OTHER.MAJORS", record.Code }),
                            new RecordKeyLookupResult() { Guid = record.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class OtherMinorsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<OtherMinor> allOtherMinors;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allOtherMinors = new TestAcademicDisciplineRepository().GetOtherMinors();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllOtherMinors");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allOtherMinors = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsOtherMinorsCacheAsync()
            {
                var otherMinors = await referenceDataRepo.GetOtherMinorsAsync(false);

                for (int i = 0; i < allOtherMinors.Count(); i++)
                {
                    Assert.AreEqual(allOtherMinors.ElementAt(i).Guid, otherMinors.ElementAt(i).Guid);
                    Assert.AreEqual(allOtherMinors.ElementAt(i).Code, otherMinors.ElementAt(i).Code);
                    Assert.AreEqual(allOtherMinors.ElementAt(i).Description, otherMinors.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsOtherMinorsNonCacheAsync()
            {
                var otherMinors = await referenceDataRepo.GetOtherMinorsAsync(true);

                for (int i = 0; i < allOtherMinors.Count(); i++)
                {
                    Assert.AreEqual(allOtherMinors.ElementAt(i).Guid, otherMinors.ElementAt(i).Guid);
                    Assert.AreEqual(allOtherMinors.ElementAt(i).Code, otherMinors.ElementAt(i).Code);
                    Assert.AreEqual(allOtherMinors.ElementAt(i).Description, otherMinors.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Other Minors read
                var otherMinorsCollection = new Collection<OtherMinors>(allOtherMinors.Select(record =>
                    new Data.Base.DataContracts.OtherMinors()
                    {
                        Recordkey = record.Code,
                        OminDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<OtherMinors>("OTHER.MINORS", "", true))
                    .ReturnsAsync(otherMinorsCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var otherMinor = allOtherMinors.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "OTHER.MINORS", otherMinor.Code }),
                            new RecordKeyLookupResult() { Guid = otherMinor.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class OtherSpecialsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<OtherSpecial> allOtherSpecials;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allOtherSpecials = new TestAcademicDisciplineRepository().GetOtherSpecials();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllOtherSpecials");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allOtherSpecials = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsOtherSpecialsCacheAsync()
            {
                var otherSpecials = await referenceDataRepo.GetOtherSpecialsAsync(false);

                for (int i = 0; i < allOtherSpecials.Count(); i++)
                {
                    Assert.AreEqual(allOtherSpecials.ElementAt(i).Guid, otherSpecials.ElementAt(i).Guid);
                    Assert.AreEqual(allOtherSpecials.ElementAt(i).Code, otherSpecials.ElementAt(i).Code);
                    Assert.AreEqual(allOtherSpecials.ElementAt(i).Description, otherSpecials.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsOtherSpecialsNonCacheAsync()
            {
                var otherSpecials = await referenceDataRepo.GetOtherSpecialsAsync(true);

                for (int i = 0; i < allOtherSpecials.Count(); i++)
                {
                    Assert.AreEqual(allOtherSpecials.ElementAt(i).Guid, otherSpecials.ElementAt(i).Guid);
                    Assert.AreEqual(allOtherSpecials.ElementAt(i).Code, otherSpecials.ElementAt(i).Code);
                    Assert.AreEqual(allOtherSpecials.ElementAt(i).Description, otherSpecials.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Other Specials read
                var otherSpecialsCollection = new Collection<OtherSpecials>(allOtherSpecials.Select(record =>
                    new Data.Base.DataContracts.OtherSpecials()
                    {
                        Recordkey = record.Code,
                        OspecDesc = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<OtherSpecials>("OTHER.SPECIALS", "", true))
                    .ReturnsAsync(otherSpecialsCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
               .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var otherSpecial = allOtherSpecials.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "OTHER.SPECIALS", otherSpecial.Code }),
                            new RecordKeyLookupResult() { Guid = otherSpecial.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class SuffixesTests : BaseRepositorySetup
        {
            public ReferenceDataRepository actualReferenceDataRepository;

            [TestInitialize]
            public void TestInitialize()
            {
                MockInitialize();
                actualReferenceDataRepository = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                actualReferenceDataRepository = null;
            }

            [TestMethod]
            public void Suffix_AllValuesNotEmptyTest()
            {
                List<string> suffixCodes = new List<string> { "Jr.", "Sr.", "III" };
                List<string> suffixDescriptions = new List<string> { "Junior", "Senior", "III" };
                List<string> suffixInternalCodes = new List<string> { "JR", "SR", "III" };
                SetupSuffixDataReader(suffixCodes, suffixDescriptions, suffixInternalCodes);

                IEnumerable<Suffix> testSuffixes = actualReferenceDataRepository.Suffixes;
                Assert.IsTrue(testSuffixes.Count() == suffixCodes.Count);
            }

            [TestMethod]
            public void Suffix_EmptyValuesTest()
            {
                // The PPS form in Colleague (which sources the prefix/suffix data) can be populated in such a way that allows
                // empty values in every combination possible; this test case tests that scenario
                //
                // Start with 3 valid, completely filled-in entries
                List<string> suffixCodes = new List<string> { "Jr.", "Sr.", "III" };
                List<string> suffixDescriptions = new List<string> { "Junior", "Senior", "III" };
                List<string> suffixInternalCodes = new List<string> { "JR", "SR", "III" };
                int completeSuffixes = suffixCodes.Count;

                // No values in any of the three
                suffixCodes.Add(String.Empty); suffixDescriptions.Add(String.Empty); suffixInternalCodes.Add(String.Empty);
                // Value only in code
                suffixCodes.Add("OnlyCode"); suffixDescriptions.Add(String.Empty); suffixInternalCodes.Add(String.Empty);
                // Value only in description
                suffixCodes.Add(String.Empty); suffixDescriptions.Add("OnlyDescription"); suffixInternalCodes.Add(String.Empty);
                // Value only in internal code
                suffixCodes.Add(String.Empty); suffixDescriptions.Add(String.Empty); suffixInternalCodes.Add("OnlyInternalCode");
                // Value in both code and description
                suffixCodes.Add("CODEAndDescription"); suffixDescriptions.Add("CodeAndDESCRIPTION"); suffixInternalCodes.Add(String.Empty);
                // Value in both code and internal code (NOTE: Only scenario of the bunch that will be included in the resulting Suffixes property)
                suffixCodes.Add("CODEAndInternalCode"); suffixDescriptions.Add(String.Empty); suffixInternalCodes.Add("CodeAndINTERNALCODE");
                // Value in both description and internal code
                suffixCodes.Add(String.Empty); suffixDescriptions.Add("DESCRIPTIONAndInternalCode"); suffixInternalCodes.Add("DescriptionAndINTERNALCODE");

                SetupSuffixDataReader(suffixCodes, suffixDescriptions, suffixInternalCodes);

                IEnumerable<Suffix> testSuffixes = actualReferenceDataRepository.Suffixes;

                // Assert that the total suffixes in the resulting property is the total number of complete suffixes plus the one that is accepted from
                // the list of "special" scenarios allowed from the PPS form in Colleague
                Assert.IsTrue(testSuffixes.Count() == (completeSuffixes + 1));
            }

            private void SetupSuffixDataReader(List<string> suffixCodes, List<string> suffixDescriptions, List<string> suffixInternalCodes)
            {
                //Set up the dataReaderMock so that every time a read occurs, we return mocked up data contacts
                Suffixes suffixDataObject;
                suffixDataObject = new Suffixes();
                suffixDataObject.SuffixesCodes = suffixCodes;
                suffixDataObject.SuffixesDescs = suffixDescriptions;
                suffixDataObject.SuffixesInternalCodes = suffixInternalCodes;
                dataReaderMock.Setup<Suffixes>(dr => dr.ReadRecord<Suffixes>("CORE.PARMS", "SUFFIXES", true)).Returns(suffixDataObject);
            }
        }

        [TestClass]
        public class PersonalPronounTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<PersonalPronounType> allPersonalPronounTypes;
            ApplValcodes personalPronounTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build personal pronoun type responses used for mocking
                allPersonalPronounTypes = new List<PersonalPronounType>
                {
                    new PersonalPronounType("1", "He", "He/Him/His"),
                    new PersonalPronounType("2", "She", "She/Her/Hers"),
                    new PersonalPronounType("3", "Ze", "Ze/Zir/Zirs"),
                };
                personalPronounTypeValcodeResponse = BuildValcodeResponse(allPersonalPronounTypes);

                // Build personalPronounType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_PERSONAL.PRONOUNS_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                //localCacheMock = null;
                personalPronounTypeValcodeResponse = null;
                allPersonalPronounTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsPersonalPronounTypesCache()
            {
                var actualPronouns = await referenceDataRepo.GetPersonalPronounTypesAsync(false);
                for (int i = 0; i < allPersonalPronounTypes.Count(); i++)
                {
                    Assert.AreEqual(allPersonalPronounTypes.ElementAt(i).Code, actualPronouns.ElementAt(i).Code);
                    Assert.AreEqual(allPersonalPronounTypes.ElementAt(i).Description, actualPronouns.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetsPersonalPronounTypesNonCache()
            {

                for (int i = 0; i < allPersonalPronounTypes.Count(); i++)
                {
                    Assert.AreEqual(allPersonalPronounTypes.ElementAt(i).Code, (await referenceDataRepo.GetPersonalPronounTypesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(allPersonalPronounTypes.ElementAt(i).Description, (await referenceDataRepo.GetPersonalPronounTypesAsync(true)).ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetPersonalPronounTypes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSONAL.PRONOUNS", It.IsAny<bool>())).ReturnsAsync(personalPronounTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                // But after data accessor read, set up mocking so we can verify the list of personalPronounTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<PersonalPronounType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_PERSONAL.PRONOUNS"), null)).Returns(true);
                var personalPronounTypes = await referenceDataRepo.GetPersonalPronounTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_PERSONAL.PRONOUNS"), null)).Returns(personalPronounTypes);

                // Verify that personalPronounTypes were returned, which means they came from the "repository".
                Assert.IsTrue(personalPronounTypes.Count() == 3);

                // Verify that the personalPronounType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<PersonalPronounType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task GetPersonalPronounTypes_GetsCachedPersonalPronounTypes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PERSONAL.PRONOUNS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allPersonalPronounTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "PERSONAL.PRONOUNS", true)).Returns(new ApplValcodes());

                // Assert the personalPronounTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetPersonalPronounTypesAsync(false)).Count() == 3);
                // Verify that the personalPronounTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to personal pronoun types valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSONAL.PRONOUNS", It.IsAny<bool>())).ReturnsAsync(personalPronounTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var addressType = allPersonalPronounTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "PERSONAL.PRONOUNS", addressType.Code }),
                            new RecordKeyLookupResult() { Guid = addressType.Guid });
                    }
                    return Task.FromResult(result);
                });


                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<PersonalPronounType> personalPronoupersonalPronounTypes)
            {
                ApplValcodes personalPronounTypesResponse = new ApplValcodes();
                personalPronounTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in personalPronoupersonalPronounTypes)
                {
                    personalPronounTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return personalPronounTypesResponse;
            }

        }

        [TestClass]
        public class GenderIdentityTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<GenderIdentityType> allGenderIdentityTypes;
            ApplValcodes genderIdentityTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build personal pronoun type responses used for mocking
                allGenderIdentityTypes = new List<GenderIdentityType>
                {
                    new GenderIdentityType("1", "FEM", "Female"),
                    new GenderIdentityType("2", "MAL", "Male"),
                    new GenderIdentityType("3", "TFM", "Transexual (F/M)"),
                };
                genderIdentityTypeValcodeResponse = BuildValcodeResponse(allGenderIdentityTypes);

                // Build genderIdentityType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_GENDER.IDENTITIES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                //localCacheMock = null;
                genderIdentityTypeValcodeResponse = null;
                allGenderIdentityTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsGenderIdentityTypesCache()
            {
                var actualGenderIdentities = await referenceDataRepo.GetGenderIdentityTypesAsync(false);
                for (int i = 0; i < allGenderIdentityTypes.Count(); i++)
                {
                    Assert.AreEqual(allGenderIdentityTypes.ElementAt(i).Code, actualGenderIdentities.ElementAt(i).Code);
                    Assert.AreEqual(allGenderIdentityTypes.ElementAt(i).Description, actualGenderIdentities.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetsPersonalPronounTypesNonCache()
            {

                for (int i = 0; i < allGenderIdentityTypes.Count(); i++)
                {
                    Assert.AreEqual(allGenderIdentityTypes.ElementAt(i).Code, (await referenceDataRepo.GetGenderIdentityTypesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(allGenderIdentityTypes.ElementAt(i).Description, (await referenceDataRepo.GetGenderIdentityTypesAsync(true)).ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetGenderIdentityTypes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "GENDER.IDENTITIES", It.IsAny<bool>())).ReturnsAsync(genderIdentityTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                // But after data accessor read, set up mocking so we can verify the list of genderIdentityTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<GenderIdentityType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_GENDER.IDENTITIES"), null)).Returns(true);
                var genderIdentityTypes = await referenceDataRepo.GetGenderIdentityTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_GENDER.IDENTITIES"), null)).Returns(genderIdentityTypes);

                // Verify that genderIdentityTypes were returned, which means they came from the "repository".
                Assert.IsTrue(genderIdentityTypes.Count() == 3);

                // Verify that the genderIdentityType item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<GenderIdentityType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task GetGenderIdentityTypes_GetsCachedGenderIdentityTypes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PERSONAL.PRONOUNS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allGenderIdentityTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "GENDER.IDENTITIES", true)).Returns(new ApplValcodes());

                // Assert the genderIdentityTypes are returned
                Assert.IsTrue((await referenceDataRepo.GetGenderIdentityTypesAsync(false)).Count() == 3);
                // Verify that the genderIdentityTypes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to personal pronoun types valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "GENDER.IDENTITIES", It.IsAny<bool>())).ReturnsAsync(genderIdentityTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var addressType = allGenderIdentityTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "GENDER.IDENTITIES", addressType.Code }),
                            new RecordKeyLookupResult() { Guid = addressType.Guid });
                    }
                    return Task.FromResult(result);
                });


                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<GenderIdentityType> genderIdentityTypes)
            {
                ApplValcodes genderIdentityTypesResponse = new ApplValcodes();
                genderIdentityTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in genderIdentityTypes)
                {
                    genderIdentityTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return genderIdentityTypesResponse;
            }

        }
        

        /// <summary>
        /// Test class for Person Filters codes
        /// </summary>
        [TestClass]
        public class PersonFiltersTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<PersonFilter> allPersonFilters;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allPersonFilters = new TestPersonFilterRepository().GetPersonFilters();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllPersonFilters");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allPersonFilters = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsPersonFiltersCacheAsync()
            {
                var personFilters = await referenceDataRepo.GetPersonFiltersAsync(false);

                for (int i = 0; i < allPersonFilters.Count(); i++)
                {
                    Assert.AreEqual(allPersonFilters.ElementAt(i).Guid, personFilters.ElementAt(i).Guid);
                    Assert.AreEqual(allPersonFilters.ElementAt(i).Code, personFilters.ElementAt(i).Code);
                    Assert.AreEqual(allPersonFilters.ElementAt(i).Description, personFilters.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsPersonFiltersNonCacheAsync()
            {
                var personFilters = await referenceDataRepo.GetPersonFiltersAsync(true);

                for (int i = 0; i < allPersonFilters.Count(); i++)
                {
                    Assert.AreEqual(allPersonFilters.ElementAt(i).Guid, personFilters.ElementAt(i).Guid);
                    Assert.AreEqual(allPersonFilters.ElementAt(i).Code, personFilters.ElementAt(i).Code);
                    Assert.AreEqual(allPersonFilters.ElementAt(i).Description, personFilters.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Person Filters read
                var personFiltersCollection = new Collection<SaveListParms>(allPersonFilters.Select(record =>
                    new Data.Base.DataContracts.SaveListParms()
                    {
                        Recordkey = record.Code,
                        SlpDescription = record.Description,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<SaveListParms>("SAVE.LIST.PARMS", "", true))
                    .ReturnsAsync(personFiltersCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var personFilter = allPersonFilters.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "SAVE.LIST.PARMS", personFilter.Code }),
                            new RecordKeyLookupResult() { Guid = personFilter.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Person Name Types
        /// </summary>
        [TestClass]
        public class PersonNameTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<PersonNameTypeItem> allPersonNameTypes;
            ApplValcodes personNameTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build person name type responses used for mocking
                allPersonNameTypes = new TestPersonNameTypeRepository().Get();
                personNameTypeValcodeResponse = BuildPersonNameTypesValcodeResponse(allPersonNameTypes);


                // Build personNameType repository
                referenceDataRepo = BuildPersonNameTypesValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.NAME.TYPES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                personNameTypeValcodeResponse = null;
                allPersonNameTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task PersonNameTypes_GetsPersonNameTypesCache()
            {
                for (int i = 0; i < allPersonNameTypes.Count(); i++)
                {
                    Assert.AreEqual(allPersonNameTypes.ElementAt(i).Code, (await referenceDataRepo.GetPersonNameTypesAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(allPersonNameTypes.ElementAt(i).Description, (await referenceDataRepo.GetPersonNameTypesAsync(false)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task PersonNameTypes_GetsPersonNameTypesNonCache()
            {
                for (int i = 0; i < allPersonNameTypes.Count(); i++)
                {
                    Assert.AreEqual(allPersonNameTypes.ElementAt(i).Code, (await referenceDataRepo.GetPersonNameTypesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(allPersonNameTypes.ElementAt(i).Description, (await referenceDataRepo.GetPersonNameTypesAsync(true)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task PersonNameTypes_GetPersonNameTypes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.NAME.TYPES", It.IsAny<bool>())).ReturnsAsync(personNameTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of phoneTypes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<PhoneType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.NAME.TYPES"), null)).Returns(true);
                var personNameTypes = await referenceDataRepo.GetPersonNameTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_INTG.PERSON.NAME.TYPES"), null)).Returns(personNameTypes);

                // Verify that types were returned, which means they came from the "repository".
                Assert.IsTrue(personNameTypes.Count() == allPersonNameTypes.Count());

                // Verify that the type item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(valcodeName, It.IsAny<Task<List<PersonNameType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task PersonNameTypes_GetPersonNameTypes_GetsCachedPersonNameTypes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item 
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allPersonNameTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.NAME.TYPES", true)).Returns(new ApplValcodes());

                // Assert the types are returned
                Assert.IsTrue((await referenceDataRepo.GetPersonNameTypesAsync(false)).Count() == allPersonNameTypes.Count());
                // Verify that the types were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildPersonNameTypesValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to phoneType valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.PERSON.NAME.TYPES", It.IsAny<bool>())).ReturnsAsync(personNameTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var personNameType = allPersonNameTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "INTG.PERSON.NAME.TYPES", personNameType.Code }),
                            new RecordKeyLookupResult() { Guid = personNameType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildPersonNameTypesValcodeResponse(IEnumerable<PersonNameTypeItem> personNameTypes)
            {
                ApplValcodes personNameTypesResponse = new ApplValcodes();
                personNameTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in personNameTypes)
                {
                    personNameTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", item.Type.ToString(), ""));
                }
                return personNameTypesResponse;
            }
        }

        [TestClass]
        public class PersonOriginCodesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<PersonOriginCodes> allPersonOriginCodes;
            ApplValcodes PersonOriginCodesValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build personal pronoun type responses used for mocking
                allPersonOriginCodes = new List<PersonOriginCodes>
                {
                    new PersonOriginCodes("1", "R", "Recruitment Activities"),
                    new PersonOriginCodes("2", "CC", "Corporate Contact"),
                    new PersonOriginCodes("3", "ML", "Mailing List"),
                };
                PersonOriginCodesValcodeResponse = BuildValcodeResponse(allPersonOriginCodes);

                // Build PersonOriginCodes repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_PERSON.ORIGIN.CODES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                //localCacheMock = null;
                PersonOriginCodesValcodeResponse = null;
                allPersonOriginCodes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsPersonOriginCodesCache()
            {
                var actualPersonOriginCodes = await referenceDataRepo.GetPersonOriginCodesAsync(false);
                for (int i = 0; i < allPersonOriginCodes.Count(); i++)
                {
                    Assert.AreEqual(allPersonOriginCodes.ElementAt(i).Code, actualPersonOriginCodes.ElementAt(i).Code);
                    Assert.AreEqual(allPersonOriginCodes.ElementAt(i).Description, actualPersonOriginCodes.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetsPersonOriginCodesNonCache()
            {

                for (int i = 0; i < allPersonOriginCodes.Count(); i++)
                {
                    Assert.AreEqual(allPersonOriginCodes.ElementAt(i).Code, (await referenceDataRepo.GetPersonOriginCodesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(allPersonOriginCodes.ElementAt(i).Description, (await referenceDataRepo.GetPersonOriginCodesAsync(true)).ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetPersonOriginCodes_WritesToCache()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ORIGIN.CODES", It.IsAny<bool>())).ReturnsAsync(PersonOriginCodesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                // But after data accessor read, set up mocking so we can verify the list of PersonOriginCodes was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<PersonOriginCodes>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_PERSON.ORIGIN.CODES"), null)).Returns(true);
                var PersonOriginCodes = await referenceDataRepo.GetPersonOriginCodesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_PERSON.ORIGIN.CODES"), null)).Returns(PersonOriginCodes);

                // Verify that PersonOriginCodes were returned, which means they came from the "repository".
                Assert.IsTrue(PersonOriginCodes.Count() == 3);

                // Verify that the PersonOriginCodes item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<PersonOriginCodes>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task GetPersonOriginCodes_GetsCachedPersonOriginCodes()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PERSONAL.PRONOUNS" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allPersonOriginCodes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "PERSON.ORIGIN.CODES", true)).Returns(new ApplValcodes());

                // Assert the PersonOriginCodes are returned
                Assert.IsTrue((await referenceDataRepo.GetPersonOriginCodesAsync(false)).Count() == 3);
                // Verify that the PersonOriginCodes were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to personal pronoun types valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PERSON.ORIGIN.CODES", It.IsAny<bool>())).ReturnsAsync(PersonOriginCodesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var addressType = allPersonOriginCodes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "PERSON.ORIGIN.CODES", addressType.Code }),
                            new RecordKeyLookupResult() { Guid = addressType.Guid });
                    }
                    return Task.FromResult(result);
                });


                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<PersonOriginCodes> PersonOriginCodes)
            {
                ApplValcodes PersonOriginCodesResponse = new ApplValcodes();
                PersonOriginCodesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in PersonOriginCodes)
                {
                    PersonOriginCodesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return PersonOriginCodesResponse;
            }

        }
        
        [TestClass]
        public class PrefixesTests : BaseRepositorySetup
        {
            public ReferenceDataRepository actualReferenceDataRepository;

            [TestInitialize]
            public void TestInitialize()
            {
                MockInitialize();
                actualReferenceDataRepository = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                actualReferenceDataRepository = null;
            }

            [TestMethod]
            public void Prefix_AllValuesNotEmptyTest()
            {
                List<string> prefixCodes = new List<string> { "Mr.", "Mrs.", "Dr." };
                List<string> prefixDescriptions = new List<string> { "Mister", "Missus", "Doctor" };
                List<string> prefixInternalCodes = new List<string> { "MR", "MRS", "DR" };
                SetupPrefixDataReader(prefixCodes, prefixDescriptions, prefixInternalCodes);

                IEnumerable<Prefix> testPrefixes = actualReferenceDataRepository.Prefixes;
                Assert.IsTrue(testPrefixes.Count() == prefixCodes.Count);
            }

            [TestMethod]
            public void Prefix_EmptyValuesTest()
            {
                // The PPS form in Colleague (which sources the prefix/suffix data) can be populated in such a way that allows
                // empty values in every combination possible; this test case tests that scenario
                //
                // Start with 3 valid, completely filled-in entries
                List<string> prefixCodes = new List<string> { "Mr.", "Mrs.", "Dr." };
                List<string> prefixDescriptions = new List<string> { "Mister", "Missus", "Doctor" };
                List<string> prefixInternalCodes = new List<string> { "MR", "MRS", "DR" };
                int completePrefixes = prefixCodes.Count;

                // No values in any of the three
                prefixCodes.Add(String.Empty); prefixDescriptions.Add(String.Empty); prefixInternalCodes.Add(String.Empty);
                // Value only in code
                prefixCodes.Add("OnlyCode"); prefixDescriptions.Add(String.Empty); prefixInternalCodes.Add(String.Empty);
                // Value only in description
                prefixCodes.Add(String.Empty); prefixDescriptions.Add("OnlyDescription"); prefixInternalCodes.Add(String.Empty);
                // Value only in internal code
                prefixCodes.Add(String.Empty); prefixDescriptions.Add(String.Empty); prefixInternalCodes.Add("OnlyInternalCode");
                // Value in both code and description
                prefixCodes.Add("CODEAndDescription"); prefixDescriptions.Add("CodeAndDESCRIPTION"); prefixInternalCodes.Add(String.Empty);
                // Value in both code and internal code (NOTE: Only scenario of the bunch that will be included in the resulting Prefixes property)
                prefixCodes.Add("CODEAndInternalCode"); prefixDescriptions.Add(String.Empty); prefixInternalCodes.Add("CodeAndINTERNALCODE");
                // Value in both description and internal code
                prefixCodes.Add(String.Empty); prefixDescriptions.Add("DESCRIPTIONAndInternalCode"); prefixInternalCodes.Add("DescriptionAndINTERNALCODE");

                SetupPrefixDataReader(prefixCodes, prefixDescriptions, prefixInternalCodes);

                IEnumerable<Prefix> testPrefixes = actualReferenceDataRepository.Prefixes;

                // Assert that the total prefixes in the resulting property is the total number of complete prefixes plus the one that is accepted from
                // the list of "special" scenarios allowed from the PPS form in Colleague
                Assert.IsTrue(testPrefixes.Count() == (completePrefixes + 1));
            }

            private void SetupPrefixDataReader(List<string> prefixCodes, List<string> prefixDescriptions, List<string> prefixInternalCodes)
            {
                //Set up the dataReaderMock so that every time a read occurs, we return mocked up data contacts
                Prefixes prefixDataObject;
                prefixDataObject = new Prefixes();
                prefixDataObject.PrefixesCodes = prefixCodes;
                prefixDataObject.PrefixesDescs = prefixDescriptions;
                prefixDataObject.PrefixesInternalCodes = prefixInternalCodes;
                dataReaderMock.Setup<Prefixes>(dr => dr.ReadRecord<Prefixes>("CORE.PARMS", "PREFIXES", true)).Returns(prefixDataObject);
            }
        }

        /// <summary>
        /// Test class for Phone Types
        /// </summary>
        //[TestClass]
        //public class PhoneTypes
        //{
        //    Mock<IColleagueTransactionFactory> transFactoryMock;
        //    Mock<ICacheProvider> cacheProviderMock;
        //    Mock<IColleagueDataReader> dataAccessorMock;
        //    Mock<ILogger> loggerMock;
        //    IEnumerable<PhoneType> allPhoneTypes;
        //    ApplValcodes phoneTypeValcodeResponse;
        //    string valcodeName;

        //    ReferenceDataRepository referenceDataRepo;

        //    [TestInitialize]
        //    public void Initialize()
        //    {
        //        loggerMock = new Mock<ILogger>();

        //        // Build address type responses used for mocking
        //        allPhoneTypes = new TestPhoneTypeRepository().Get();
        //        phoneTypeValcodeResponse = BuildValcodeResponse(allPhoneTypes);

        //        // Build addressType repository
        //        referenceDataRepo = BuildValidReferenceDataRepository();
        //        valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_PHONE.TYPES_GUID");
        //    }

        //    [TestCleanup]
        //    public void Cleanup()
        //    {
        //        transFactoryMock = null;
        //        dataAccessorMock = null;
        //        cacheProviderMock = null;
        //        //localCacheMock = null;
        //        phoneTypeValcodeResponse = null;
        //        allPhoneTypes = null;
        //        referenceDataRepo = null;
        //    }

        //    [TestMethod]
        //    public async Task GetsPhoneTypesCache()
        //    {
        //        for (int i = 0; i < allPhoneTypes.Count(); i++)
        //        {
        //            Assert.AreEqual(allPhoneTypes.ElementAt(i).Code, (await referenceDataRepo.GetPhoneTypesAsync(false)).ElementAt(i).Code);
        //            Assert.AreEqual(allPhoneTypes.ElementAt(i).Description, (await referenceDataRepo.GetPhoneTypesAsync(false)).ElementAt(i).Description);

        //        }
        //    }

        //    [TestMethod]
        //    public async Task GetsPhoneTypesNonCache()
        //    {

        //        for (int i = 0; i < allPhoneTypes.Count(); i++)
        //        {
        //            Assert.AreEqual(allPhoneTypes.ElementAt(i).Code, (await referenceDataRepo.GetPhoneTypesAsync(true)).ElementAt(i).Code);
        //            Assert.AreEqual(allPhoneTypes.ElementAt(i).Description, (await referenceDataRepo.GetPhoneTypesAsync(true)).ElementAt(i).Description);

        //        }
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypes_WritesToCache()
        //    {
        //        // Set up local cache mock to respond to cache request:
        //        //  -to "Contains" request, return "false" to indicate item is not in cache
        //        //  -to cache "Get" request, return null so we know it's reading from the "repository"
        //        cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
        //        cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

        //        // return a valid response to the data accessor request
        //        dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES", It.IsAny<bool>())).ReturnsAsync(phoneTypeValcodeResponse);

        //        cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
        //         x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
        //         .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


        //        // But after data accessor read, set up mocking so we can verify the list of addressTypes was written to the cache
        //        cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<PhoneType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

        //        cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_PHONE.TYPES"), null)).Returns(true);
        //        var phoneTypes = await referenceDataRepo.GetPhoneTypesAsync(false);
        //        cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_PHONE.TYPES"), null)).Returns(phoneTypes);

        //        // Verify that phoneTypes were returned, which means they came from the "repository".
        //        Assert.IsTrue(phoneTypes.Count() == 17);

        //        // Verify that the phoneType item was added to the cache after it was read from the repository
        //        cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<PhoneType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
        //    }

        //    [TestMethod]
        //    public async Task GetPhoneTypes_GetsCachedPhoneTypes()
        //    {
        //        // Set up local cache mock to respond to cache request:
        //        //  -to "Contains" request, return "true" to indicate item is in cache
        //        //  -to "Get" request, return the cache item (in this case the "PHONE.TYPES" cache item)
        //        cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
        //        cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allPhoneTypes).Verifiable();

        //        // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
        //        dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES", true)).Returns(new ApplValcodes());

        //        // Assert the phoneTypes are returned
        //        Assert.IsTrue((await referenceDataRepo.GetPhoneTypesAsync(false)).Count() == 17);
        //        // Verify that the phoneTypes were retrieved from cache
        //        cacheProviderMock.Verify(m => m.Get(valcodeName, null));
        //    }

        //    private ReferenceDataRepository BuildValidReferenceDataRepository()
        //    {
        //        // transaction factory mock
        //        transFactoryMock = new Mock<IColleagueTransactionFactory>();
        //        // Cache Mock
        //        //localCacheMock = new Mock<ObjectCache>();
        //        // Cache Provider Mock
        //        cacheProviderMock = new Mock<ICacheProvider>();
        //        // Set up data accessor for mocking 
        //        dataAccessorMock = new Mock<IColleagueDataReader>();

        //        // Setup localCacheMock as the object for the CacheProvider
        //        //cacheProviderMock.Setup(provider => provider.GetCache(It.IsAny<string>())).Returns(localCacheMock.Object);

        //        // Set up dataAccessorMock as the object for the DataAccessor
        //        transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

        //        // Setup response to phone types valcode read
        //        dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES", It.IsAny<bool>())).ReturnsAsync(phoneTypeValcodeResponse);

        //        cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
        //         x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
        //         .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


        //        dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
        //        {
        //            var result = new Dictionary<string, RecordKeyLookupResult>();
        //            foreach (var recordKeyLookup in recordKeyLookups)
        //            {
        //                var phoneType = allPhoneTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
        //                result.Add(string.Join("+", new string[] { "CORE.VALCODES", "PHONE.TYPES", phoneType.Code }),
        //                    new RecordKeyLookupResult() { Guid = phoneType.Guid });
        //            }
        //            return Task.FromResult(result);
        //        });


        //        // Construct repository
        //        referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

        //        return referenceDataRepo;
        //    }

        //    private ApplValcodes BuildValcodeResponse(IEnumerable<PhoneType> phoneTypes)
        //    {
        //        ApplValcodes phoneTypesResponse = new ApplValcodes();
        //        phoneTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
        //        foreach (var item in phoneTypes)
        //        {
        //            string phoneType = "";
        //            switch (item.PhoneTypeCategory)
        //            {
        //                case PhoneTypeCategory.Billing:
        //                    phoneType = "billing";
        //                    break;
        //                case PhoneTypeCategory.Business:
        //                    phoneType = "business";
        //                    break;
        //                case PhoneTypeCategory.Home:
        //                    phoneType = "home";
        //                    break;
        //                case PhoneTypeCategory.Mobile:
        //                    phoneType = "mobile";
        //                    break;
        //                case PhoneTypeCategory.Vacation:
        //                    phoneType = "vacation";
        //                    break;
        //                case PhoneTypeCategory.School:
        //                    phoneType = "school";
        //                    break;
        //                case PhoneTypeCategory.Fax:
        //                    phoneType = "fax";
        //                    break;
        //                case PhoneTypeCategory.TDD:
        //                    phoneType = "tdd";
        //                    break;
        //                case PhoneTypeCategory.Branch:
        //                    phoneType = "branch";
        //                    break;
        //                case PhoneTypeCategory.Family:
        //                    phoneType = "family";
        //                    break;
        //                case PhoneTypeCategory.Parent:
        //                    phoneType = "parent";
        //                    break;
        //                case PhoneTypeCategory.Main:
        //                    phoneType = "main";
        //                    break;
        //                case PhoneTypeCategory.Support:
        //                    phoneType = "support";
        //                    break;
        //                case PhoneTypeCategory.Pager:
        //                    phoneType = "pager";
        //                    break;
        //                case PhoneTypeCategory.Region:
        //                    phoneType = "regional";
        //                    break;
        //                case PhoneTypeCategory.Matching:
        //                    phoneType = "matching";
        //                    break;
        //                default:
        //                    phoneType = "";
        //                    break;
        //            }

        //            phoneTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", phoneType, ""));
        //        }
        //        return phoneTypesResponse;
        //    }
        //    //Mock<IColleagueTransactionFactory> transFactoryMock;
        //    //Mock<ICacheProvider> cacheProviderMock;
        //    //Mock<IColleagueDataReader> dataAccessorMock;
        //    //Mock<ILogger> loggerMock;
        //    //IEnumerable<PhoneType> allPhoneTypes;
        //    //ApplValcodes phoneTypeValcodeResponse;
        //    //string valcodeName;

        //    //ReferenceDataRepository referenceDataRepo;

        //    //[TestInitialize]
        //    //public void Initialize()
        //    //{
        //    //    loggerMock = new Mock<ILogger>();

        //    //    // Build phone type responses used for mocking
        //    //    allPhoneTypes = new TestPhoneTypeRepository().Get();
        //    //    phoneTypeValcodeResponse = BuildPhoneTypesValcodeResponse(allPhoneTypes);


        //    //    // Build phoneType repository
        //    //    referenceDataRepo = BuildPhoneTypesValidReferenceDataRepository();
        //    //    valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_PHONE.TYPES_GUID");
        //    //}

        //    //[TestCleanup]
        //    //public void Cleanup()
        //    //{
        //    //    transFactoryMock = null;
        //    //    dataAccessorMock = null;
        //    //    cacheProviderMock = null;
        //    //    //localCacheMock = null;
        //    //    phoneTypeValcodeResponse = null;
        //    //    allPhoneTypes = null;
        //    //    referenceDataRepo = null;
        //    //}

        //    //[TestMethod]
        //    //public async Task PhoneTypes_GetsPhoneTypesCache()
        //    //{
        //    //    for (int i = 0; i < allPhoneTypes.Count(); i++)
        //    //    {
        //    //        Assert.AreEqual(allPhoneTypes.ElementAt(i).Code, (await referenceDataRepo.GetPhoneTypesAsync(false)).ElementAt(i).Code);
        //    //        Assert.AreEqual(allPhoneTypes.ElementAt(i).Description, (await referenceDataRepo.GetPhoneTypesAsync(false)).ElementAt(i).Description);
        //    //    }
        //    //}

        //    //[TestMethod]
        //    //public async Task PhoneTypes_GetsPhoneTypesNonCache()
        //    //{
        //    //    for (int i = 0; i < allPhoneTypes.Count(); i++)
        //    //    {
        //    //        Assert.AreEqual(allPhoneTypes.ElementAt(i).Code, (await referenceDataRepo.GetPhoneTypesAsync(true)).ElementAt(i).Code);
        //    //        Assert.AreEqual(allPhoneTypes.ElementAt(i).Description, (await referenceDataRepo.GetPhoneTypesAsync(true)).ElementAt(i).Description);
        //    //    }
        //    //}

        //    //[TestMethod]
        //    //public async Task PhoneTypes_GetPhoneTypes_WritesToCache()
        //    //{
        //    //    // Set up local cache mock to respond to cache request:
        //    //    //  -to "Contains" request, return "false" to indicate item is not in cache
        //    //    //  -to cache "Get" request, return null so we know it's reading from the "repository"
        //    //    cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
        //    //    cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

        //    //    // return a valid response to the data accessor request
        //    //    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES", It.IsAny<bool>())).ReturnsAsync(phoneTypeValcodeResponse);

        //    //    cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
        //    //     .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

        //    //    // But after data accessor read, set up mocking so we can verify the list of phoneTypes was written to the cache
        //    //    cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<PhoneType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

        //    //    cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_PHONE.TYPES"), null)).Returns(true);
        //    //    var phoneTypes = await referenceDataRepo.GetPhoneTypesAsync(false);
        //    //    cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_PHONE.TYPES"), null)).Returns(phoneTypes);

        //    //    // Verify that phoneTypes were returned, which means they came from the "repository".
        //    //    Assert.IsTrue(phoneTypes.Count() == allPhoneTypes.Count());

        //    //    // Verify that the phoneType item was added to the cache after it was read from the repository
        //    //    cacheProviderMock.Verify(m => m.Add(valcodeName, It.IsAny<Task<List<PhoneType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
        //    //}

        //    //[TestMethod]
        //    //public async Task PhoneTypes_GetPhoneTypes_GetsCachedPhoneTypes()
        //    //{
        //    //    // Set up local cache mock to respond to cache request:
        //    //    //  -to "Contains" request, return "true" to indicate item is in cache
        //    //    //  -to "Get" request, return the cache item (in this case the "PHONE.TYPES" cache item)
        //    //    cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
        //    //    cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allPhoneTypes).Verifiable();

        //    //    // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
        //    //    dataAccessorMock.Setup<ApplValcodes>(acc => acc.ReadRecord<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES", true)).Returns(new ApplValcodes());

        //    //    // Assert the phoneTypes are returned
        //    //    Assert.IsTrue((await referenceDataRepo.GetPhoneTypesAsync(false)).Count() == allPhoneTypes.Count());
        //    //    // Verify that the phoneTypes were retrieved from cache
        //    //    cacheProviderMock.Verify(m => m.Get(valcodeName, null));
        //    //}

        //    //private ReferenceDataRepository BuildPhoneTypesValidReferenceDataRepository()
        //    //{
        //    //    // transaction factory mock
        //    //    transFactoryMock = new Mock<IColleagueTransactionFactory>();
        //    //    // Cache Mock
        //    //    //localCacheMock = new Mock<ObjectCache>();
        //    //    // Cache Provider Mock
        //    //    cacheProviderMock = new Mock<ICacheProvider>();
        //    //    // Set up data accessor for mocking 
        //    //    dataAccessorMock = new Mock<IColleagueDataReader>();

        //    //    // Set up dataAccessorMock as the object for the DataAccessor
        //    //    transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

        //    //    // Setup response to phoneType valcode read
        //    //    dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PHONE.TYPES", It.IsAny<bool>())).ReturnsAsync(phoneTypeValcodeResponse);

        //    //    cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
        //    //     x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
        //    //     .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


        //    //    dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
        //    //    {
        //    //        var result = new Dictionary<string, RecordKeyLookupResult>();
        //    //        foreach (var recordKeyLookup in recordKeyLookups)
        //    //        {
        //    //            var phoneType = allPhoneTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
        //    //            result.Add(string.Join("+", new string[] { "CORE.VALCODES", "PHONE.TYPES", phoneType.Code }),
        //    //                new RecordKeyLookupResult() { Guid = phoneType.Guid });
        //    //        }
        //    //        return Task.FromResult(result);
        //    //    });

        //    //    // Construct repository
        //    //    referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

        //    //    return referenceDataRepo;
        //    //}

        //    //private ApplValcodes BuildPhoneTypesValcodeResponse(IEnumerable<PhoneType> phoneTypes)
        //    //{
        //    //    ApplValcodes phoneTypesResponse = new ApplValcodes();
        //    //    phoneTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
        //    //    foreach (var item in phoneTypes)
        //    //    {
        //    //        string phoneType = "";
        //    //        switch (item.PhoneTypeCategory)
        //    //        {
        //    //            case PhoneTypeCategory.Billing:
        //    //                phoneType = "billing";
        //    //                break;
        //    //            case PhoneTypeCategory.Business:
        //    //                phoneType = "business";
        //    //                break;
        //    //            case PhoneTypeCategory.Home:
        //    //                phoneType = "home";
        //    //                break;
        //    //            case PhoneTypeCategory.TDD:
        //    //                phoneType = "tdd";
        //    //                break;
        //    //            case PhoneTypeCategory.Fax:
        //    //                phoneType = "fax";
        //    //                break;
        //    //            case PhoneTypeCategory.Vacation:
        //    //                phoneType = "vacation";
        //    //                break;
        //    //            case PhoneTypeCategory.School:
        //    //                phoneType = "school";
        //    //                break;
        //    //            case PhoneTypeCategory.Mobile:
        //    //                phoneType = "mobile";
        //    //                break;
        //    //            case PhoneTypeCategory.Branch:
        //    //                phoneType = "branch";
        //    //                break;
        //    //            case PhoneTypeCategory.Family:
        //    //                phoneType = "family";
        //    //                break;
        //    //            case PhoneTypeCategory.Parent:
        //    //                phoneType = "parent";
        //    //                break;
        //    //            case PhoneTypeCategory.Main:
        //    //                phoneType = "main";
        //    //                break;
        //    //            case PhoneTypeCategory.Support:
        //    //                phoneType = "support";
        //    //                break;
        //    //            case PhoneTypeCategory.Pager:
        //    //                phoneType = "pager";
        //    //                break;
        //    //            case PhoneTypeCategory.Region:
        //    //                phoneType = "regional";
        //    //                break;
        //    //            case PhoneTypeCategory.Matching:
        //    //                phoneType = "matching";
        //    //                break;
        //    //            default:
        //    //                phoneType = "";
        //    //                break;
        //    //        }
        //    //        phoneTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", phoneType, ""));
        //    //    }
        //    //    return phoneTypesResponse;
        //    //}
        //}

        /// <summary>
        /// Test class for Privacy Statuses
        /// </summary>
        [TestClass]
        public class PrivacyStatuses
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<PrivacyStatus> allPrivacyStatuses;
            ApplValcodes privacyStatusValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build privacy statuses responses used for mocking
                allPrivacyStatuses = new TestPrivacyStatusRepository().Get();
                privacyStatusValcodeResponse = BuildValcodeResponse(allPrivacyStatuses);

                // Build privacy statuses repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_PRIVACY.CODES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                privacyStatusValcodeResponse = null;
                allPrivacyStatuses = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsPrivacyStatusesCacheAsync()
            {
                var privacyStatuses = await referenceDataRepo.GetPrivacyStatusesAsync(false);
                for (int i = 0; i < privacyStatuses.Count(); i++)
                {
                    Assert.AreEqual(allPrivacyStatuses.ElementAt(i).Code, privacyStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allPrivacyStatuses.ElementAt(i).Description, privacyStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsPrivacyStatusesNonCacheAsync()
            {
                var privacyStatuses = await referenceDataRepo.GetPrivacyStatusesAsync(true);
                for (int i = 0; i < privacyStatuses.Count(); i++)
                {
                    Assert.AreEqual(allPrivacyStatuses.ElementAt(i).Code, privacyStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allPrivacyStatuses.ElementAt(i).Description, privacyStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetPrivacyStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(privacyStatusValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of privacy statuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<PrivacyStatus>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_PRIVACY.CODES"), null)).Returns(true);
                var privacyStatuses = await referenceDataRepo.GetPrivacyStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_PRIVACY.CODES"), null)).Returns(privacyStatuses);
                // Verify that privacy statuses were returned, which means they came from the "repository".
                Assert.IsTrue(privacyStatuses.Count() == 4);

                // Verify that the privacy status item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<PrivacyStatus>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetPrivacyStatuses_GetsCachedPrivacyStatusesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "PRIVACY.CODES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allPrivacyStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PRIVACY.CODES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the privacy statuses are returned
                Assert.IsTrue((await referenceDataRepo.GetPrivacyStatusesAsync(false)).Count() == 4);
                // Verify that the privacy statuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "PRIVACY.CODES", It.IsAny<bool>())).ReturnsAsync(privacyStatusValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var privacyStatus = allPrivacyStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "PRIVACY.CODES", privacyStatus.Code }),
                            new RecordKeyLookupResult() { Guid = privacyStatus.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<PrivacyStatus> privacyStatuses)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in privacyStatuses)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Privacy Messages
        /// </summary>
        [TestClass]
        public class PrivacyMessages
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            Dictionary<string, string> allPrivacyMessages;
            Dflts dfltsResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build privacy statuses responses used for mocking
                allPrivacyMessages = new Dictionary<string, string>();
                allPrivacyMessages.Add("test1", "Test 1 Message");
                allPrivacyMessages.Add("test2", "Test 2 Message");
                allPrivacyMessages.Add("test3", "Test 3 Message");

                dfltsResponse = BuildDfltsResponse(allPrivacyMessages);

                // Build privacy statuses repository
                referenceDataRepo = BuildValidReferenceDataRepository();

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                dfltsResponse = null;
                allPrivacyMessages = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsPrivacyMessagesAsync()
            {
                var privacyMessages = await referenceDataRepo.GetPrivacyMessagesAsync();
                
                Assert.AreEqual(allPrivacyMessages.Count, privacyMessages.Count);
                
                for (int i = 0; i < privacyMessages.Count(); i++)
                {
                    Assert.AreEqual(allPrivacyMessages.ElementAt(i).Key, privacyMessages.ElementAt(i).Key);
                    Assert.AreEqual(allPrivacyMessages.ElementAt(i).Value, privacyMessages.ElementAt(i).Value);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to privacy status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<Dflts>("CORE.PARMS", "DEFAULTS", It.IsAny<bool>())).ReturnsAsync(dfltsResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private Dflts BuildDfltsResponse(Dictionary<string, string> privacyMessages)
            {
                Dflts response = new Dflts();
                response.DfltsPrivacyEntityAssociation = new List<DfltsDfltsPrivacy>();
                foreach (var item in privacyMessages)
                {
                    response.DfltsPrivacyEntityAssociation.Add(new DfltsDfltsPrivacy(item.Key, item.Value));
                }
                return response;
            }
        }

        /// <summary>
        /// Test class for Relationship statuses codes
        /// </summary>
        [TestClass]
        public class RelationshipStatuses
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            List<Ellucian.Colleague.Domain.Base.Entities.RelationshipStatus> _allRelationshipStatuses;
            ApplValcodes _relationshipStatusValcodeResponse;
            string _valcodeName;
            private ApiSettings apiSettings;
            ReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build RELATION.STATUSES responses used for mocking
                _allRelationshipStatuses = new TestRelationshipStatusesRepository().GetRelationshipStatuses().ToList();

                _relationshipStatusValcodeResponse = BuildValcodeResponse(_allRelationshipStatuses);

                // Build RELATION.STATUSES repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("CORE_RELATION.STATUSES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _relationshipStatusValcodeResponse = null;
                _allRelationshipStatuses = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRelationshipStatus_Cache()
            {
                for (var i = 0; i < _allRelationshipStatuses.Count(); i++)
                {
                    Assert.AreEqual(_allRelationshipStatuses.ElementAt(i).Code, (await _referenceDataRepo.GetRelationshipStatusesAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(_allRelationshipStatuses.ElementAt(i).Description, (await _referenceDataRepo.GetRelationshipStatusesAsync(false)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRelationshipStatus_NonCache()
            {
                for (var i = 0; i < _allRelationshipStatuses.Count(); i++)
                {
                    Assert.AreEqual(_allRelationshipStatuses.ElementAt(i).Code, (await _referenceDataRepo.GetRelationshipStatusesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(_allRelationshipStatuses.ElementAt(i).Description, (await _referenceDataRepo.GetRelationshipStatusesAsync(true)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRelationshipStatus_WritesToCache()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "RELATION.STATUSES", It.IsAny<bool>())).ReturnsAsync(_relationshipStatusValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("CORE_RELATION.STATUSES"), null)).Returns(true);
                var relationshipStatus = await _referenceDataRepo.GetRelationshipStatusesAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("CORE_RELATION.STATUSES"), null)).Returns(relationshipStatus);
                // Verify that data was returned, which means they came from the "repository".
                Assert.IsTrue(relationshipStatus.Count() == 4);

            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRelationshipStatus_GetsCachedRelationshipStatus()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "REMARK.CODES" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allRelationshipStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "RELATION.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the relationship statuses are returned
                Assert.IsTrue((await _referenceDataRepo.GetRelationshipStatusesAsync(false)).Count() == 4);
                // Verify that the relationship statuses were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to the valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "RELATION.STATUSES", It.IsAny<bool>())).ReturnsAsync(_relationshipStatusValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var relationshipStatuses = _allRelationshipStatuses.FirstOrDefault(e => e.Code == recordKeyLookup.SecondaryKey);
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "RELATION.STATUSES", relationshipStatuses.Code }),
                            new RecordKeyLookupResult() { Guid = relationshipStatuses.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new ReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object, apiSettings);

                return _referenceDataRepo;
            }

            private static ApplValcodes BuildValcodeResponse(IEnumerable<RelationshipStatus> relationshipStatuses)
            {
                var relationshipStatusesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                };
                foreach (var item in relationshipStatuses)
                {
                    relationshipStatusesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return relationshipStatusesResponse;
            }
        }

        [TestClass]
        public class RelationTypeTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<RelationType> allRelationTypes;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allRelationTypes = new TestRelationTypesRepository().GetRelationTypes();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllRelationTypes");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allRelationTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsRelationTypesCache()
            {
                for (int i = 0; i < allRelationTypes.Count(); i++)
                {
                    Assert.AreEqual(allRelationTypes.ElementAt(i).Guid, (await referenceDataRepo.GetRelationTypesAsync(false)).ElementAt(i).Guid);
                    Assert.AreEqual(allRelationTypes.ElementAt(i).Code, (await referenceDataRepo.GetRelationTypesAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(allRelationTypes.ElementAt(i).Description, (await referenceDataRepo.GetRelationTypesAsync(false)).ElementAt(i).Description);
                    Assert.AreEqual(allRelationTypes.ElementAt(i).OrgIndicator, (await referenceDataRepo.GetRelationTypesAsync(false)).ElementAt(i).OrgIndicator);
                }
            }

            [TestMethod]
            public async Task GetsRelationTypesNonCache()
            {
                for (int i = 0; i < allRelationTypes.Count(); i++)
                {
                    Assert.AreEqual(allRelationTypes.ElementAt(i).Guid, (await referenceDataRepo.GetRelationTypesAsync(true)).ElementAt(i).Guid);
                    Assert.AreEqual(allRelationTypes.ElementAt(i).Code, (await referenceDataRepo.GetRelationTypesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(allRelationTypes.ElementAt(i).Description, (await referenceDataRepo.GetRelationTypesAsync(true)).ElementAt(i).Description);
                    Assert.AreEqual(allRelationTypes.ElementAt(i).OrgIndicator, (await referenceDataRepo.GetRelationTypesAsync(true)).ElementAt(i).OrgIndicator); 
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to read
                var relationTypesCollection = new Collection<Data.Base.DataContracts.RelationTypes>(allRelationTypes.Select(record =>
                    new Data.Base.DataContracts.RelationTypes()
                    {
                        Recordkey = record.Code,
                        ReltyDesc = record.Description,
                        RecordGuid = record.Guid,
                        ReltyOrgIndicator = record.OrgIndicator
                    }).ToList());

                dataAccessorMock.Setup<Task<Collection<Data.Base.DataContracts.RelationTypes>>>(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.RelationTypes>("RELATION.TYPES", "", true))
                    .ReturnsAsync(relationTypesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var subjects = allRelationTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "RELATION.TYPES", subjects.Code }),
                            new RecordKeyLookupResult() { Guid = subjects.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Build  repository
                return new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }
        }

        /// <summary>
        /// Test class for Relation Types
        /// </summary>
        [TestClass]
        public class RelationTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataReaderMock;
            Mock<ILogger> loggerMock;
            ReferenceDataRepository referenceDataRepo;
            private ApiSettings apiSettings;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                cacheProviderMock = new Mock<ICacheProvider>();
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                dataReaderMock = new Mock<IColleagueDataReader>();

                // set up cache for async
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                    x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null)).Returns(Task.FromResult(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1))));

                // set up the data reader mock to return the above data structure.  Requires the use of 'Task.FromResult' to avoid hanging.
                dataReaderMock.Setup<Task<Collection<Data.Base.DataContracts.RelationTypes>>>(dr => dr.BulkReadRecordAsync<Data.Base.DataContracts.RelationTypes>(It.IsAny<string>(), "", true)).Returns(Task.FromResult(dataFromDataReader()));
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                apiSettings = new ApiSettings("TEST");

                // Build  repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataReaderMock = null;
                cacheProviderMock = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRelationTypesAsync_Count()
            {
                var repoData = await referenceDataRepo.GetRelationshipTypesAsync();
                Assert.AreEqual(dataFromDataReader().Count, repoData.Count());
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRelationTypesAsync_Content()
            {
                var repoData = await referenceDataRepo.GetRelationshipTypesAsync();
                var sourceData = dataFromDataReader();
                for (int i = 0; i < sourceData.Count(); i++)
                {
                    Assert.AreEqual(sourceData.ElementAt(i).Recordkey, repoData.ElementAt(i).Code);
                    Assert.AreEqual(sourceData.ElementAt(i).ReltyDesc, repoData.ElementAt(i).Description);
                    var expectedInverseCode = string.IsNullOrEmpty(sourceData.ElementAt(i).ReltyInverseRelationType) ?
                        sourceData.ElementAt(i).Recordkey :
                        sourceData.ElementAt(i).ReltyInverseRelationType;
                    Assert.AreEqual(expectedInverseCode, repoData.ElementAt(i).InverseCode);
                }
            }

            private Collection<Data.Base.DataContracts.RelationTypes> dataFromDataReader()
            {
                return new Collection<DataContracts.RelationTypes>()
                {
                    new Data.Base.DataContracts.RelationTypes(){Recordkey = "P", ReltyDesc="Parent", ReltyInverseRelationType = "C"},
                    new Data.Base.DataContracts.RelationTypes(){Recordkey = "C", ReltyDesc="Child", ReltyInverseRelationType = "P"},
                    new Data.Base.DataContracts.RelationTypes(){Recordkey = "CZ", ReltyDesc="Contact", ReltyInverseRelationType = "CZ"},
                    new Data.Base.DataContracts.RelationTypes(){Recordkey = "O", ReltyDesc="Other", ReltyInverseRelationType = ""},
                };
            }
        }

        /// <summary>
        /// Test class for EEDM RelationTypes codes
        /// </summary>
        [TestClass]
        public class RelationTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Data.Base.DataContracts.RelationTypes> _relationshipTypesCollection;
            string codeItemName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _relationshipTypesCollection = new List<Data.Base.DataContracts.RelationTypes>()
                {
                    new Data.Base.DataContracts.RelationTypes(){RecordGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", Recordkey = "P", ReltyDesc="Parent", ReltyInverseRelationType = "C"},
                    new Data.Base.DataContracts.RelationTypes(){RecordGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", Recordkey = "C", ReltyDesc="Child", ReltyInverseRelationType = "P"},
                    new Data.Base.DataContracts.RelationTypes(){RecordGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e", Recordkey = "CZ", ReltyDesc="Contact", ReltyInverseRelationType = "CZ"},
                    new Data.Base.DataContracts.RelationTypes(){RecordGuid = "d2253ac7-9931-4560-b42f-1fccd43c9abc", Recordkey = "O", ReltyDesc="Other", ReltyInverseRelationType = ""},
                };

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllRelationTypes2");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _relationshipTypesCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsRelationTypesCacheAsync()
            {
                var result = await referenceDataRepo.GetRelationTypes2Async(false);

                for (int i = 0; i < _relationshipTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_relationshipTypesCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_relationshipTypesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_relationshipTypesCollection.ElementAt(i).ReltyDesc, result.ElementAt(i).Description);
                    Assert.AreEqual(_relationshipTypesCollection.ElementAt(i).ReltyInverseRelationType, result.ElementAt(i).InverseRelType);
                }
            }

            [TestMethod]
            public async Task GetsRelationTypesNonCacheAsync()
            {
                var result = await referenceDataRepo.GetRelationTypes2Async(true);

                for (int i = 0; i < _relationshipTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_relationshipTypesCollection.ElementAt(i).RecordGuid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_relationshipTypesCollection.ElementAt(i).Recordkey, result.ElementAt(i).Code);
                    Assert.AreEqual(_relationshipTypesCollection.ElementAt(i).ReltyDesc, result.ElementAt(i).Description);
                    Assert.AreEqual(_relationshipTypesCollection.ElementAt(i).ReltyInverseRelationType, result.ElementAt(i).InverseRelType);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to RelationTypes read
                var entityCollection = new Collection<Data.Base.DataContracts.RelationTypes>(_relationshipTypesCollection.Select(record =>
                    new Data.Base.DataContracts.RelationTypes()
                    {
                        Recordkey = record.Recordkey,
                        ReltyDesc = record.ReltyDesc,
                        RecordGuid = record.RecordGuid,
                        ReltyInverseRelationType = record.ReltyInverseRelationType
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.RelationTypes>("RELATION.TYPES", "", true))
                    .ReturnsAsync(entityCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var entity = _relationshipTypesCollection.Where(e => e.Recordkey == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "RELATIONTYPES", entity.Recordkey }),
                            new RecordKeyLookupResult() { Guid = entity.RecordGuid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Remark Codes codes
        /// </summary>
        [TestClass]
        public class RemarkCodes
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            List<Ellucian.Colleague.Domain.Base.Entities.RemarkCode> _allRemarkCodes;
            ApplValcodes _remarkCodeValcodeResponse;
            string _valcodeName;
            private ApiSettings apiSettings;
            ReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build REMARK.CODES responses used for mocking
                _allRemarkCodes = new TestRemarkCodeRepository().GetRemarkCode().ToList();

                _remarkCodeValcodeResponse = BuildValcodeResponse(_allRemarkCodes);

                // Build REMARK.CODES repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("CORE_REMARK.CODES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _remarkCodeValcodeResponse = null;
                _allRemarkCodes = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRemarkCode_Cache()
            {
                for (var i = 0; i < _allRemarkCodes.Count(); i++)
                {
                    Assert.AreEqual(_allRemarkCodes.ElementAt(i).Code, (await _referenceDataRepo.GetRemarkCodesAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(_allRemarkCodes.ElementAt(i).Description, (await _referenceDataRepo.GetRemarkCodesAsync(false)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRemarkCode_NonCache()
            {
                for (var i = 0; i < _allRemarkCodes.Count(); i++)
                {
                    Assert.AreEqual(_allRemarkCodes.ElementAt(i).Code, (await _referenceDataRepo.GetRemarkCodesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(_allRemarkCodes.ElementAt(i).Description, (await _referenceDataRepo.GetRemarkCodesAsync(true)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetsRemarkCodesGuidAsync()
            {
                var remarkCodes = await _referenceDataRepo.GetRemarkCodesAsync(It.IsAny<bool>());
                for (int i = 0; i < _allRemarkCodes.Count(); i++)
                {
                    var remarkCodeGuid = await _referenceDataRepo.GetRemarkCodesGuidAsync(remarkCodes.ElementAt(i).Code);
                    Assert.AreEqual(_allRemarkCodes.ElementAt(i).Guid, remarkCodeGuid);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRemarkCode_WritesToCache()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "REMARK.CODES", It.IsAny<bool>())).ReturnsAsync(_remarkCodeValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
          
                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("CORE_REMARK.CODES"), null)).Returns(true);
                var remarkCodes = await _referenceDataRepo.GetRemarkCodesAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("CORE_REMARK.CODES"), null)).Returns(remarkCodes);
                // Verify that data was returned, which means they came from the "repository".
                Assert.IsTrue(remarkCodes.Count() == 4);

             }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRemarkCode_GetsCachedRemarkCode()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "REMARK.CODES" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allRemarkCodes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "REMARK.CODES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the remark codes are returned
                Assert.IsTrue((await _referenceDataRepo.GetRemarkCodesAsync(false)).Count() == 4);
                // Verify that the remark codes were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to the valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "REMARK.CODES", It.IsAny<bool>())).ReturnsAsync(_remarkCodeValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var remarkCodes = _allRemarkCodes.FirstOrDefault(e => e.Code == recordKeyLookup.SecondaryKey);
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "REMARK.CODES", remarkCodes.Code }),
                            new RecordKeyLookupResult() { Guid = remarkCodes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new ReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object, apiSettings);

                return _referenceDataRepo;
            }

            private static ApplValcodes BuildValcodeResponse(IEnumerable<RemarkCode> remarkCodes)
            {
                var remarkCodesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                };
                foreach (var item in remarkCodes)
                {
                    remarkCodesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return remarkCodesResponse;
            }
        }

        /// <summary>
        /// Test class for Remark Types codes
        /// </summary>
        [TestClass]
        public class RemarkTypes
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            List<Ellucian.Colleague.Domain.Base.Entities.RemarkType> _allRemarkTypes;
            ApplValcodes _remarkTypeValcodeResponse;
            string _valcodeName;
            private ApiSettings apiSettings;
            ReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build REMARK.TYPES responses used for mocking
                _allRemarkTypes = new TestRemarkTypeRepository().GetRemarkType().ToList();

                _remarkTypeValcodeResponse = BuildValcodeResponse(_allRemarkTypes);

                // Build REMARK.TYPES repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("CORE_REMARK.TYPES_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _remarkTypeValcodeResponse = null;
                _allRemarkTypes = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRemarkType_Cache()
            {
                for (var i = 0; i < _allRemarkTypes.Count(); i++)
                {
                    Assert.AreEqual(_allRemarkTypes.ElementAt(i).Code, (await _referenceDataRepo.GetRemarkTypesAsync(false)).ElementAt(i).Code);
                    Assert.AreEqual(_allRemarkTypes.ElementAt(i).Description, (await _referenceDataRepo.GetRemarkTypesAsync(false)).ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRemarkType_NonCache()
            {
                for (var i = 0; i < _allRemarkTypes.Count(); i++)
                {
                    Assert.AreEqual(_allRemarkTypes.ElementAt(i).Code, (await _referenceDataRepo.GetRemarkTypesAsync(true)).ElementAt(i).Code);
                    Assert.AreEqual(_allRemarkTypes.ElementAt(i).Description, (await _referenceDataRepo.GetRemarkTypesAsync(true)).ElementAt(i).Description);
                }
            }


            [TestMethod]
            public async Task ReferenceDataRepository_GetsRemarkTypesGuidAsync()
            {
                var remarkTypes = await _referenceDataRepo.GetRemarkTypesAsync(It.IsAny<bool>());
                for (int i = 0; i < _allRemarkTypes.Count(); i++)
                {
                    var remarkTypeGuid = await _referenceDataRepo.GetRemarkTypesGuidAsync(remarkTypes.ElementAt(i).Code);
                    Assert.AreEqual(_allRemarkTypes.ElementAt(i).Guid, remarkTypeGuid);
                }
            }


            [TestMethod]
            public async Task ReferenceDataRepository_GetRemarkType_WritesToCache()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "REMARK.TYPES", It.IsAny<bool>())).ReturnsAsync(_remarkTypeValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of remark types was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<RemarkType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("CORE_REMARK.TYPES"), null)).Returns(true);
                var remarkTypes = await _referenceDataRepo.GetRemarkTypesAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("CORE_REMARK.TYPES"), null)).Returns(remarkTypes);
                // Verify that data was returned, which means they came from the "repository".
                Assert.IsTrue(remarkTypes.Count() == 4);

                // Verify that the remark code item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<RemarkType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetRemarkType_GetsCachedRemarkType()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "REMARK.TYPES" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allRemarkTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "REMARK.TYPES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the remark codes are returned
                Assert.IsTrue((await _referenceDataRepo.GetRemarkTypesAsync(false)).Count() == 4);
                // Verify that the remark codes were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to the valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "REMARK.TYPES", It.IsAny<bool>())).ReturnsAsync(_remarkTypeValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var remarkTypes = _allRemarkTypes.FirstOrDefault(e => e.Code == recordKeyLookup.SecondaryKey);
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "REMARK.TYPES", remarkTypes.Code }),
                            new RecordKeyLookupResult() { Guid = remarkTypes.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new ReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object, apiSettings);

                return _referenceDataRepo;
            }

            private static ApplValcodes BuildValcodeResponse(IEnumerable<RemarkType> remarkTypes)
            {
                var remarkTypesResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                };
                foreach (var item in remarkTypes)
                {
                    remarkTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return remarkTypesResponse;
            }
        }

        [TestClass]
        public class RestrictionTypeTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Restriction> allRestrictionTypes;
            string codeItemName;

            ReferenceDataRepository referenceDataRepo;

            Mock<IColleagueTransactionInvoker> transManagerMock;
            IColleagueTransactionInvoker transManager;
            private ApiSettings apiSettings;

            [TestInitialize]
            public void Initialize()
            {
                transManagerMock = new Mock<IColleagueTransactionInvoker>();
                transManager = transManagerMock.Object;
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allRestrictionTypes = new TestRestrictionRepository().Get();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllRestrictions");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allRestrictionTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetRestrictionTypesCache()
            {
                var restrictionTypes = await referenceDataRepo.GetRestrictionsAsync(false);
                for (int i = 0; i < allRestrictionTypes.Count(); i++)
                {
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Guid, restrictionTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Code, restrictionTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Description, restrictionTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsRestrictionTypesNonCache()
            {
                var restrictionTypes = await referenceDataRepo.GetRestrictionsAsync(true);
                for (int i = 0; i < allRestrictionTypes.Count(); i++)
                {
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Guid, restrictionTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Code, restrictionTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Description, restrictionTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetRestrictionTypes_NoArgument()
            {
                var restrictionTypes = await referenceDataRepo.RestrictionsAsync();
                for (int i = 0; i < allRestrictionTypes.Count(); i++)
                {
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Guid, restrictionTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Code, restrictionTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Description, restrictionTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetRestrictionTypesWithCategoryCache()
            {
                var restrictionTypes = await referenceDataRepo.GetRestrictionsWithCategoryAsync(false);
                for (int i = 0; i < allRestrictionTypes.Count(); i++)
                {
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Guid, restrictionTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Code, restrictionTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).Description, restrictionTypes.ElementAt(i).Description);
                    Assert.AreEqual(allRestrictionTypes.ElementAt(i).RestIntgCategory, restrictionTypes.ElementAt(i).RestIntgCategory);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);
                // Set up transManagerMock as the object for the transaction manager
                transFactoryMock.Setup(transFac => transFac.GetTransactionInvoker()).Returns(transManager);
                transManagerMock.Setup(mgr => mgr.Execute<GetRestrictionHyperlinksRequest, GetRestrictionHyperlinksResponse>(It.IsAny<GetRestrictionHyperlinksRequest>())).Returns(new GetRestrictionHyperlinksResponse());
                
                // Setup response to read
                var restrictionTypesCollection = new Collection<Data.Base.DataContracts.Restrictions>(allRestrictionTypes.Select(record =>
                    new Data.Base.DataContracts.Restrictions()
                    {
                        Recordkey = record.Code,
                        RestDesc = record.Description,
                        RecordGuid = record.Guid,
                        RestSeverity = record.Severity,
                        RestPrtlFollowUpApp = record.FollowUpApplication,
                        RestPrtlFollowUpLabel = record.FollowUpLabel,
                        RestPrtlFollowUpLinkDef = record.FollowUpLinkDefinition,
                        RestPrtlFollowUpWaForm = record.FollowUpWebAdvisorForm,
                        RestIntgCategory = ((int)record.RestIntgCategory + 1).ToString()
                    }).ToList());

                dataAccessorMock.Setup<Task<Collection<Data.Base.DataContracts.Restrictions>>>(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.Restrictions>("RESTRICTIONS", "", true))
                    .ReturnsAsync(restrictionTypesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var subjects = allRestrictionTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "RESTRICTIONS", subjects.Code }),
                            new RecordKeyLookupResult() { Guid = subjects.Guid });
                    }
                    return Task.FromResult(result);
                });


                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class RoomCharacteristicTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RoomCharacteristic> roomCharacteristicEntities;
            ApplValcodes _roomCharacteristicValcodeResponse;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                BuildData();
                _roomCharacteristicValcodeResponse = BuildValcodeResponse(roomCharacteristicEntities);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                //codeItemName = referenceDataRepo.BuildFullCacheKey("AllRelationTypes");

                apiSettings = new ApiSettings("TEST");
            }

            private void BuildData()
            {
                roomCharacteristicEntities = new List<Domain.Base.Entities.RoomCharacteristic>() 
                {
                    new Domain.Base.Entities.RoomCharacteristic("84e13c85-3faf-42fe-8dd9-ee87621c53fd", "LT", "Natural Lighting"),
                    new Domain.Base.Entities.RoomCharacteristic("e7bccff3-1487-4aa5-b1a8-ac0d900951a0", "SM", "Smoking"),
                    new Domain.Base.Entities.RoomCharacteristic("623603f8-b7ef-40e5-9cd0-34b2a0de7605", "MA", "Male Room"),
                    new Domain.Base.Entities.RoomCharacteristic("ee4cb17c-c625-49ff-bdcc-9a2bda200e5c", "FE", "Female Room")
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                roomCharacteristicEntities = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetRoomCharacteristicsCache_True()
            {
                var roomCharacteristics = await referenceDataRepo.GetRoomCharacteristicsAsync(true);
                for (int i = 0; i < roomCharacteristicEntities.Count(); i++)
                {
                    Assert.AreEqual(roomCharacteristicEntities.ElementAt(i).Guid, roomCharacteristics.ElementAt(i).Guid);
                    Assert.AreEqual(roomCharacteristicEntities.ElementAt(i).Code, roomCharacteristics.ElementAt(i).Code);
                    Assert.AreEqual(roomCharacteristicEntities.ElementAt(i).Description, roomCharacteristics.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetRoomCharacteristicsCache_False()
            {
                var roomCharacteristics = await referenceDataRepo.GetRoomCharacteristicsAsync(false);
                for (int i = 0; i < roomCharacteristicEntities.Count(); i++)
                {
                    Assert.AreEqual(roomCharacteristicEntities.ElementAt(i).Guid, roomCharacteristics.ElementAt(i).Guid);
                    Assert.AreEqual(roomCharacteristicEntities.ElementAt(i).Code, roomCharacteristics.ElementAt(i).Code);
                    Assert.AreEqual(roomCharacteristicEntities.ElementAt(i).Description, roomCharacteristics.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to citizenship status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ROOM.CHARACTERISTICS", It.IsAny<bool>())).ReturnsAsync(_roomCharacteristicValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var roomCharacteristic = roomCharacteristicEntities.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "ROOM.CHARACTERISTICS", roomCharacteristic.Code }),
                            new RecordKeyLookupResult() { Guid = roomCharacteristic.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private static ApplValcodes BuildValcodeResponse(IEnumerable<RoomCharacteristic> sourceContext)
            {
                var sourceContextResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                };
                foreach (var item in sourceContext)
                {
                    sourceContextResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return sourceContextResponse;
            }
        }
        
        [TestClass]
        public class RoomTypeTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.RoomTypes> allRoomTypes;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allRoomTypes = new TestRoomTypesRepository().Get();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllRelationTypes");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allRoomTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetRoomTypesCache()
            {
                var relationTypes = await referenceDataRepo.GetRoomTypesAsync(false);
                for (int i = 0; i < allRoomTypes.Count(); i++)
                {
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Guid, relationTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Code, relationTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Description, relationTypes.ElementAt(i).Description);
                      }
            }

            [TestMethod]
            public async Task GetsRoomTypesNonCache()
            {
                var relationTypes = await referenceDataRepo.GetRoomTypesAsync(true);
                for (int i = 0; i < allRoomTypes.Count(); i++)
                {
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Guid, relationTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Code, relationTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Description, relationTypes.ElementAt(i).Description);
                      }
            }

            [TestMethod]
            public async Task GetRoomTypes_NoArgument()
            {
                var relationTypes = await referenceDataRepo.RoomTypesAsync();
                for (int i = 0; i < allRoomTypes.Count(); i++)
                {
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Guid, relationTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Code, relationTypes.ElementAt(i).Code);
                    Assert.AreEqual(allRoomTypes.ElementAt(i).Description, relationTypes.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to read
                var relationTypesCollection = new Collection<Data.Base.DataContracts.RoomTypes>(allRoomTypes.Select(record =>
                    new Data.Base.DataContracts.RoomTypes()
                    {
                        Recordkey = record.Code,
                        RmtpDescription = record.Description,
                        RecordGuid = record.Guid
                        //,ReltyOrgIndicator = record.OrgIndicator
                    }).ToList());

                dataAccessorMock.Setup<Task<Collection<Data.Base.DataContracts.RoomTypes>>>(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.RoomTypes>("ROOM.TYPES", "", true))
                    .ReturnsAsync(relationTypesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var subjects = allRoomTypes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ROOM.TYPES", subjects.Code }),
                            new RecordKeyLookupResult() { Guid = subjects.Guid });
                    }
                    return Task.FromResult(result);
                });


                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        /// <summary>
        /// Test class for Room Wings
        /// </summary>
        [TestClass]
        public class RoomWings
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            IEnumerable<RoomWing> _allRoomWings;
            ApplValcodes _roomWingsValcodeResponse;
            string _valcodeName;
            private ApiSettings apiSettings;
            ReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                _allRoomWings = new TestRoomWingsRepository().Get().ToList();
                _roomWingsValcodeResponse = BuildValcodeResponse(_allRoomWings);

                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("CORE_ROOM.WINGS_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _roomWingsValcodeResponse = null;
                _allRoomWings = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetRoomWingsCacheAsync()
            {
                var roomWings = (await _referenceDataRepo.GetRoomWingsAsync(false)).ToList();
                for (var i = 0; i < roomWings.Count(); i++)
                {
                    Assert.AreEqual(_allRoomWings.ElementAt(i).Code, roomWings.ElementAt(i).Code);
                    Assert.AreEqual(_allRoomWings.ElementAt(i).Description, roomWings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetRoomWingsNonCacheAsync()
            {
                var roomWings = (await _referenceDataRepo.GetRoomWingsAsync(true)).ToList();
                for (var i = 0; i < roomWings.Count(); i++)
                {
                    Assert.AreEqual(_allRoomWings.ElementAt(i).Code, roomWings.ElementAt(i).Code);
                    Assert.AreEqual(_allRoomWings.ElementAt(i).Description, roomWings.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetRoomWings_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(_roomWingsValcodeResponse);

                _cacheProviderMock.Setup(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                     .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of types was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<RoomWing>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("CORE_ROOM.WINGS"), null)).Returns(true);
                var roomWings = await _referenceDataRepo.GetRoomWingsAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("CORE_ROOM.WINGS"), null)).Returns(roomWings);
                // Verify that the social media types were returned, which means they came from the "repository".
                Assert.IsTrue(roomWings.Count() == _allRoomWings.Count());

                // Verify that the social media types item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<RoomWing>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetRoomWings_GetsCachedRoomWingsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
              
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allRoomWings).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ROOM.WINGS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the types are returned
                Assert.IsTrue((await _referenceDataRepo.GetRoomWingsAsync(false)).Count() == _allRoomWings.Count());
                // Verify that the types were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to citizenship status valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ROOM.WINGS", It.IsAny<bool>())).ReturnsAsync(_roomWingsValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var roomWing = _allRoomWings.FirstOrDefault(e => e.Code == recordKeyLookup.SecondaryKey);
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "ROOM.WINGS", roomWing.Code }),
                            new RecordKeyLookupResult() { Guid = roomWing.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new ReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object, apiSettings);

                return _referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<RoomWing> roomWing)
            {
                var valcodeResponse = new ApplValcodes {ValsEntityAssociation = new List<ApplValcodesVals>()};
                foreach (var item in roomWing)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for ZipCodeXlats codes
        /// </summary>
        [TestClass]
        public class ZipCodeXlatsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<ZipcodeXlat> allZipCodeXlats;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                allZipCodeXlats = new TestGeographicAreaRepository().GetZipCodeXlats();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllZipxlat");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allZipCodeXlats = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsZipCodeXlatsCacheAsync()
            {
                var zipCodeXlats = await referenceDataRepo.GetZipCodeXlatAsync(false);

                for (int i = 0; i < allZipCodeXlats.Count(); i++)
                {
                    Assert.AreEqual(allZipCodeXlats.ElementAt(i).Guid, zipCodeXlats.ElementAt(i).Guid);
                    Assert.AreEqual(allZipCodeXlats.ElementAt(i).Code, zipCodeXlats.ElementAt(i).Code);
                    Assert.AreEqual(allZipCodeXlats.ElementAt(i).Description, zipCodeXlats.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsZipCodeXlatsNonCacheAsync()
            {
                var zipCodeXlats = await referenceDataRepo.GetZipCodeXlatAsync(true);

                for (int i = 0; i < allZipCodeXlats.Count(); i++)
                {
                    Assert.AreEqual(allZipCodeXlats.ElementAt(i).Guid, zipCodeXlats.ElementAt(i).Guid);
                    Assert.AreEqual(allZipCodeXlats.ElementAt(i).Code, zipCodeXlats.ElementAt(i).Code);
                    Assert.AreEqual(allZipCodeXlats.ElementAt(i).Description, zipCodeXlats.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Chapters read
                var zipCodeXlatsCollection = new Collection<ZipCodeXlat>(allZipCodeXlats.Select(record =>
                    new Data.Base.DataContracts.ZipCodeXlat()
                    {
                        Recordkey = record.Code,
                        RecordGuid = record.Guid
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<ZipCodeXlat>("ZIP.CODE.XLAT", "", true))
                    .ReturnsAsync(zipCodeXlatsCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var zipCodeXlat = allZipCodeXlats.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ZIP.CODE.XLAT", zipCodeXlat.Code }),
                            new RecordKeyLookupResult() { Guid = zipCodeXlat.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }      

        /// <summary>
        /// Test class for Social Media Types
        /// </summary>
        [TestClass]
        public class SocialMediaTypes
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<SocialMediaType> allSocialMediaTypes;
            ApplValcodes socialMediaTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build social media types responses used for mocking
                allSocialMediaTypes = new TestSocialMediaTypesRepository().GetSocialMediaTypes();
                socialMediaTypeValcodeResponse = BuildValcodeResponse(allSocialMediaTypes);

                // Build social media types repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("CORE_SOCIAL.MEDIA.NETWORKS_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                socialMediaTypeValcodeResponse = null;
                allSocialMediaTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsSocialMediaTypesCacheAsync()
            {
                var socialMediaTypes = await referenceDataRepo.GetSocialMediaTypesAsync(false);
                for (int i = 0; i < socialMediaTypes.Count(); i++)
                {
                    Assert.AreEqual(allSocialMediaTypes.ElementAt(i).Code, socialMediaTypes.ElementAt(i).Code);
                    Assert.AreEqual(allSocialMediaTypes.ElementAt(i).Description, socialMediaTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsSocialMediaTypesNonCacheAsync()
            {
                var socialMediaTypes = await referenceDataRepo.GetSocialMediaTypesAsync(true);
                for (int i = 0; i < socialMediaTypes.Count(); i++)
                {
                    Assert.AreEqual(allSocialMediaTypes.ElementAt(i).Code, socialMediaTypes.ElementAt(i).Code);
                    Assert.AreEqual(allSocialMediaTypes.ElementAt(i).Description, socialMediaTypes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetSocialMediaTypes_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(socialMediaTypeValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of types was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<SocialMediaType>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_SOCIAL.MEDIA.NETWORKS"), null)).Returns(true);
                var socialMediaTypes = await referenceDataRepo.GetSocialMediaTypesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_SOCIAL.MEDIA.NETWORKS"), null)).Returns(socialMediaTypes);
                // Verify that the social media types were returned, which means they came from the "repository".
                Assert.IsTrue(socialMediaTypes.Count() == 18);

                // Verify that the social media types item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<SocialMediaType>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task GetSocialMediaTypes_GetsCachedSocialMediaTypesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.GEO.AREA.TYPES" cache item)
                cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allSocialMediaTypes).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "SOCIAL.MEDIA.NETWORKS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the types are returned
                var temp = (await referenceDataRepo.GetSocialMediaTypesAsync(false)).Count();
                Assert.IsTrue((await referenceDataRepo.GetSocialMediaTypesAsync(false)).Count() == 18);
                // Verify that the types were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to citizenship status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "SOCIAL.MEDIA.NETWORKS", It.IsAny<bool>())).ReturnsAsync(socialMediaTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var socialMediaType = allSocialMediaTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "SOCIAL.MEDIA.NETWORKS", socialMediaType.Code }),
                            new RecordKeyLookupResult() { Guid = socialMediaType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<SocialMediaType> socialMediaTypes)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in socialMediaTypes)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "" /*newType*/, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
        }

        /// <summary>
        /// Test class for Source Context codes
        /// </summary>
        [TestClass]
        public class SourceContextTests
        {
            Mock<IColleagueTransactionFactory> _transFactoryMock;
            Mock<ICacheProvider> _cacheProviderMock;
            Mock<IColleagueDataReader> _dataAccessorMock;
            Mock<ILogger> _loggerMock;
            List<Ellucian.Colleague.Domain.Base.Entities.SourceContext> _allSourceContexts;
            ApplValcodes _sourceContextValcodeResponse;
            string _valcodeName;
            private ApiSettings apiSettings;
            ReferenceDataRepository _referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = new Mock<ILogger>();

                // Build REMARK.TYPES responses used for mocking
                _allSourceContexts = new TestSourceContextRepository().GetSourceContexts().ToList();

                _sourceContextValcodeResponse = BuildValcodeResponse(_allSourceContexts);

                // Build INTG.SOURCE.CONTEXTS repository
                _referenceDataRepo = BuildValidReferenceDataRepository();
                _valcodeName = _referenceDataRepo.BuildFullCacheKey("CORE_INTG.SOURCE.CONTEXTS_GUID");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                _transFactoryMock = null;
                _dataAccessorMock = null;
                _cacheProviderMock = null;
                _sourceContextValcodeResponse = null;
                _allSourceContexts = null;
                _referenceDataRepo = null;
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetSourceContexts_Cache()
            {
                var refSourceContexts = await _referenceDataRepo.GetSourceContextsAsync(false);
                for (var i = 0; i < _allSourceContexts.Count(); i++)
                {
                    Assert.AreEqual(_allSourceContexts.ElementAt(i).Code, refSourceContexts.ElementAt(i).Code);
                    Assert.AreEqual(_allSourceContexts.ElementAt(i).Description, refSourceContexts.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetSourceContexts_NonCache()
            {
                var refSourceContexts = await _referenceDataRepo.GetSourceContextsAsync(true);
                for (var i = 0; i < _allSourceContexts.Count(); i++)
                {
                    Assert.AreEqual(_allSourceContexts.ElementAt(i).Code, refSourceContexts.ElementAt(i).Code);
                    Assert.AreEqual(_allSourceContexts.ElementAt(i).Description, refSourceContexts.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetSourceContexts_WritesToCache()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(false);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(null);

                // return a valid response to the data accessor request
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.SOURCE.CONTEXTS", It.IsAny<bool>())).ReturnsAsync(_sourceContextValcodeResponse);

                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of remark codes was written to the cache
                _cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<SourceContext>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                _cacheProviderMock.Setup(x => x.Contains(_referenceDataRepo.BuildFullCacheKey("CORE_INTG.SOURCE.CONTEXTS"), null)).Returns(true);
                var sourceContexts = await _referenceDataRepo.GetSourceContextsAsync(false);
                _cacheProviderMock.Setup(x => x.Get(_referenceDataRepo.BuildFullCacheKey("CORE_INTG.SOURCE.CONTEXTS"), null)).Returns(sourceContexts);
                // Verify that data was returned, which means they came from the "repository".
                Assert.IsTrue(sourceContexts.Count() == 4);

                // Verify that the remark code item was added to the cache after it was read from the repository
                _cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<InstructionalPlatform>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);
            }

            [TestMethod]
            public async Task ReferenceDataRepository_GetSourceContexts_GetsCachedSourceContexts()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "INTG.SOURCE.CONTEXTS" cache item)
                _cacheProviderMock.Setup(x => x.Contains(_valcodeName, null)).Returns(true);
                _cacheProviderMock.Setup(x => x.Get(_valcodeName, null)).Returns(_allSourceContexts).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.SOURCE.CONTEXTS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the source contexts are returned
                Assert.IsTrue((await _referenceDataRepo.GetSourceContextsAsync(false)).Count() == 4);
                // Verify that the remark codes were retrieved from cache
                _cacheProviderMock.Verify(m => m.Get(_valcodeName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                _transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                _cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                _dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                _transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(_dataAccessorMock.Object);

                // Setup response to the valcode read
                _dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.SOURCE.CONTEXTS", It.IsAny<bool>())).ReturnsAsync(_sourceContextValcodeResponse);
                _cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                _dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var sourceContexts = _allSourceContexts.FirstOrDefault(e => e.Code == recordKeyLookup.SecondaryKey);
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "INTG.SOURCE.CONTEXTS", sourceContexts.Code }),
                            new RecordKeyLookupResult() { Guid = sourceContexts.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                _referenceDataRepo = new ReferenceDataRepository(_cacheProviderMock.Object, _transFactoryMock.Object, _loggerMock.Object, apiSettings);

                return _referenceDataRepo;
            }

            private static ApplValcodes BuildValcodeResponse(IEnumerable<SourceContext> sourceContext)
            {
                var sourceContextResponse = new ApplValcodes
                {
                    ValsEntityAssociation = new List<ApplValcodesVals>()
                };
                foreach (var item in sourceContext)
                {
                    sourceContextResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return sourceContextResponse;
            }
        }

        [TestClass]
        public class GradeChangeReasonTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.GradeChangeReason> allGradeChangeReasonsTypes;
            ApplValcodes gradeChangeReasonTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build email types responses used for mocking
                allGradeChangeReasonsTypes = new TestGradeChangeReasonRepository().Get();
                gradeChangeReasonTypeValcodeResponse = BuildValcodeResponse(allGradeChangeReasonsTypes);


                // Build emailType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("INTG.GRADE.CHANGE.REASONS");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allGradeChangeReasonsTypes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetGradeChangeReasonsCache()
            {
                var gradeChangeReasons = await referenceDataRepo.GetGradeChangeReasonAsync(false);
                for (int i = 0; i < allGradeChangeReasonsTypes.Count(); i++)
                {
                    Assert.AreEqual(allGradeChangeReasonsTypes.ElementAt(i).Guid, gradeChangeReasons.ElementAt(i).Guid);
                    Assert.AreEqual(allGradeChangeReasonsTypes.ElementAt(i).Code, gradeChangeReasons.ElementAt(i).Code);
                    Assert.AreEqual(allGradeChangeReasonsTypes.ElementAt(i).Description, gradeChangeReasons.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetGradeChangeReasonsNonCache()
            {
                var gradeChangeReasons = await referenceDataRepo.GetGradeChangeReasonAsync(true);
                for (int i = 0; i < allGradeChangeReasonsTypes.Count(); i++)
                {
                    Assert.AreEqual(allGradeChangeReasonsTypes.ElementAt(i).Guid, gradeChangeReasons.ElementAt(i).Guid);
                    Assert.AreEqual(allGradeChangeReasonsTypes.ElementAt(i).Code, gradeChangeReasons.ElementAt(i).Code);
                    Assert.AreEqual(allGradeChangeReasonsTypes.ElementAt(i).Description, gradeChangeReasons.ElementAt(i).Description);

                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response togradeChangeReasonType valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("ST.VALCODES", "INTG.GRADE.CHANGE.REASONS", It.IsAny<bool>())).ReturnsAsync(gradeChangeReasonTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach(var recordKeyLookup in recordKeyLookups)
                    {
                        var gcrType = allGradeChangeReasonsTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "ST.VALCODES", "INTG.GRADE.CHANGE.REASONS", gcrType.Code }),
                            new RecordKeyLookupResult() { Guid = gcrType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<GradeChangeReason> gradeChangeReasonTypes)
            {
                ApplValcodes gradeChangeReasonTypesResponse = new ApplValcodes();
                gradeChangeReasonTypesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach(var item in gradeChangeReasonTypes)
                {
                    gradeChangeReasonTypesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return gradeChangeReasonTypesResponse;
            }
        }

        /// <summary>
        /// Test class for Ethnicity codes
        /// </summary>
        [TestClass]
        public class GeographicAreaTypeTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<GeographicAreaType> allGeographicAreaTypes;
            ApplValcodes geographicAreaTypeValcodeResponse;
            string valcodeName;
            private ApiSettings apiSettings;

            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build email types responses used for mocking
                allGeographicAreaTypes = new TestGeographicAreaRepository().Get();
                geographicAreaTypeValcodeResponse = BuildValcodeResponse(allGeographicAreaTypes);


                // Build emailType repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                valcodeName = referenceDataRepo.BuildFullCacheKey("INTG.GEO.AREA.TYPES");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allGeographicAreaTypes = null;
                referenceDataRepo = null;
                loggerMock = null;
                geographicAreaTypeValcodeResponse = null;
            }

            [TestMethod]
            public async Task GetGeographicAreaTypesCache()
            {
                var geographicAreaTypes = await referenceDataRepo.GetGeographicAreaTypesAsync(false);
                for (int i = 0; i < allGeographicAreaTypes.Count(); i++)
                {
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Guid, geographicAreaTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Code, geographicAreaTypes.ElementAt(i).Code);
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Description, geographicAreaTypes.ElementAt(i).Description);

                }
            }

            [TestMethod]
            public async Task GetGeographicAreaTypesNonCache()
            {
                var geographicAreaTypes = await referenceDataRepo.GetGeographicAreaTypesAsync(true);
                for (int i = 0; i < allGeographicAreaTypes.Count(); i++)
                {
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Guid, geographicAreaTypes.ElementAt(i).Guid);
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Code, geographicAreaTypes.ElementAt(i).Code);
                    Assert.AreEqual(allGeographicAreaTypes.ElementAt(i).Description, geographicAreaTypes.ElementAt(i).Description);

                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Mock
                //localCacheMock = new Mock<ObjectCache>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response togradeChangeReasonType valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "INTG.GEO.AREA.TYPES", It.IsAny<bool>())).ReturnsAsync(geographicAreaTypeValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var geoType = allGeographicAreaTypes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "INTG.GEO.AREA.TYPES", geoType.Code }),
                            new RecordKeyLookupResult() { Guid = geoType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<GeographicAreaType> geographicAreaTypes)
            {
                ApplValcodes geographicAreaTypeValcodeResponse = new ApplValcodes();
                geographicAreaTypeValcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in geographicAreaTypes)
                {
                    geographicAreaTypeValcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "", item.Code, "", "", ""));
                }
                return geographicAreaTypeValcodeResponse;
            }
        }

        /// <summary>
        /// Test class for ZipCodeXlats codes
        /// </summary>
        [TestClass]
        public class VocationsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Vocation> allVocations;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                BuildData();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllVocations");

                apiSettings = new ApiSettings("TEST");
            }

            private void BuildData()
            {
                allVocations = new List<Vocation>() 
                {
                    new Vocation("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Vocation("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Vocation("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Test")
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allVocations = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsVocationsCacheAsync()
            {
                var vocations = await referenceDataRepo.GetVocationsAsync(false);

                for (int i = 0; i < allVocations.Count(); i++)
                {
                    Assert.AreEqual(allVocations.ElementAt(i).Guid, vocations.ElementAt(i).Guid);
                    Assert.AreEqual(allVocations.ElementAt(i).Code, vocations.ElementAt(i).Code);
                    Assert.AreEqual(allVocations.ElementAt(i).Description, vocations.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsVocationsNonCacheAsync()
            {
                var vocations = await referenceDataRepo.GetVocationsAsync(true);

                for (int i = 0; i < allVocations.Count(); i++)
                {
                    Assert.AreEqual(allVocations.ElementAt(i).Guid, vocations.ElementAt(i).Guid);
                    Assert.AreEqual(allVocations.ElementAt(i).Code, vocations.ElementAt(i).Code);
                    Assert.AreEqual(allVocations.ElementAt(i).Description, vocations.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Chapters read
                var VocationsCollection = new Collection<Vocations>(allVocations.Select(record =>
                    new Data.Base.DataContracts.Vocations()
                    {
                        Recordkey = record.Code,
                        RecordGuid = record.Guid,
                        VocationsDesc = record.Description
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Vocations>("VOCATIONS", "", true))
                    .ReturnsAsync(VocationsCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var vocation = allVocations.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "VOCATIONS", vocation.Code }),
                            new RecordKeyLookupResult() { Guid = vocation.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }


        [TestClass]
        public class CorrStatusesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<CorrStatus> allCorrStatuses;
            ApplValcodes admissionApplicationSupportingItemStatusesValcodeResponse;
            string domainEntityNameName;
            ReferenceDataRepository referenceDataRepo;
            private ApiSettings apiSettings;
            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                allCorrStatuses = new List<CorrStatus>()
                {
                    new CorrStatus("3d75a5dd-1251-4c76-84c6-4f5a7e5a4111", "CO1", "Status Description 1"),
                    new CorrStatus("3d75a5dd-1251-4c76-84c6-4f5a7e5a4222", "CO2", "Status Description 2"),
                    new CorrStatus("3d75a5dd-1251-4c76-84c6-4f5a7e5a4333", "CO3", "Status Description 3"),
                };

                admissionApplicationSupportingItemStatusesValcodeResponse = BuildValcodeResponse(allCorrStatuses);
                //var admissionApplicationSupportingItemStatusesValResponse = new List<string>() { "2" };
                //admissionApplicationSupportingItemStatusesValcodeResponse.ValActionCode1 = admissionApplicationSupportingItemStatusesValResponse;

                apiSettings = new ApiSettings("TEST");

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("CORE_CORR.STATUSES_GUID"); 

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                admissionApplicationSupportingItemStatusesValcodeResponse = null;
                allCorrStatuses = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task BaseReferenceDataRepo_GetsCorrStatusesCacheAsync()
            {
                var admissionApplicationSupportingItemStatuses = await referenceDataRepo.GetCorrStatusesAsync(false);

                for (int i = 0; i < allCorrStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCorrStatuses.ElementAt(i).Code, admissionApplicationSupportingItemStatuses.ElementAt(i).Code);
                    Assert.AreEqual(allCorrStatuses.ElementAt(i).Description, admissionApplicationSupportingItemStatuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetsCorrStatusesNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetCorrStatusesAsync(true);

                for (int i = 0; i < allCorrStatuses.Count(); i++)
                {
                    Assert.AreEqual(allCorrStatuses.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allCorrStatuses.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetCorrStatuses_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CORR.STATUSES", It.IsAny<bool>())).ReturnsAsync(admissionApplicationSupportingItemStatusesValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of admissionApplicationSupportingItemStatuses was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<CorrStatus>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORR.STATUSES"), null)).Returns(true);
                var admissionApplicationSupportingItemStatuses = await referenceDataRepo.GetCorrStatusesAsync(false);
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORR.STATUSES"), null)).Returns(admissionApplicationSupportingItemStatuses);
                // Verify that admissionApplicationSupportingItemStatuses were returned, which means they came from the "repository".
                Assert.IsTrue(admissionApplicationSupportingItemStatuses.Count() == 3);

                // Verify that the admissionApplicationSupportingItemStatuses item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<CorrStatus>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetCorrStatuses_GetsCachedCorrStatusesAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "CORR.STATUSES" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allCorrStatuses).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CORR.STATUSES", true)).ReturnsAsync(new ApplValcodes());

                // Assert the admissionApplicationSupportingItemStatuses are returned
                Assert.IsTrue((await referenceDataRepo.GetCorrStatusesAsync(false)).Count() == 3);
                // Verify that the sadmissionApplicationSupportingItemStatuses were retrieved from cache
                cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to admissionApplicationSupportingItemStatuses domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "CORR.STATUSES", It.IsAny<bool>())).ReturnsAsync(admissionApplicationSupportingItemStatusesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var admissionApplicationSupportingItemStatuses = allCorrStatuses.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "CORR.STATUSES", admissionApplicationSupportingItemStatuses.Code }),
                            new RecordKeyLookupResult() { Guid = admissionApplicationSupportingItemStatuses.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<CorrStatus> admissionApplicationSupportingItemStatuses)
            {
                ApplValcodes admissionApplicationSupportingItemStatusesResponse = new ApplValcodes();
                admissionApplicationSupportingItemStatusesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in admissionApplicationSupportingItemStatuses)
                {
                    admissionApplicationSupportingItemStatusesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return admissionApplicationSupportingItemStatusesResponse;
            }
        }

        /// <summary>
        /// Test class for AltIdTypes codes
        /// </summary>
        [TestClass]
        public class AltIdTypesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<AltIdTypes> _alternativeCredentialTypesCollection;
            string codeItemName;
            ApplValcodes altIdTypesValcodeResponse;

            ReferenceDataRepository referenceDataRepo;
            private ApiSettings apiSettings;
            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                _alternativeCredentialTypesCollection = new List<AltIdTypes>()
                {
                    new AltIdTypes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new AltIdTypes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new AltIdTypes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
                altIdTypesValcodeResponse = BuildValcodeResponse(_alternativeCredentialTypesCollection);

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllAltIdTypes");

                apiSettings = new ApiSettings("TEST");
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                _alternativeCredentialTypesCollection = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsAltIdTypesCacheAsync()
            {
                var result = await referenceDataRepo.GetAlternateIdTypesAsync(false);

                for (int i = 0; i < _alternativeCredentialTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_alternativeCredentialTypesCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_alternativeCredentialTypesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_alternativeCredentialTypesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsAltIdTypesNonCacheAsync()
            {
                var result = await referenceDataRepo.GetAlternateIdTypesAsync(true);

                for (int i = 0; i < _alternativeCredentialTypesCollection.Count(); i++)
                {
                    Assert.AreEqual(_alternativeCredentialTypesCollection.ElementAt(i).Guid, result.ElementAt(i).Guid);
                    Assert.AreEqual(_alternativeCredentialTypesCollection.ElementAt(i).Code, result.ElementAt(i).Code);
                    Assert.AreEqual(_alternativeCredentialTypesCollection.ElementAt(i).Description, result.ElementAt(i).Description);
                }
            }
            private ApplValcodes BuildValcodeResponse(IEnumerable<AltIdTypes> altIdTypes)
            {
                ApplValcodes valcodeResponse = new ApplValcodes();
                valcodeResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in altIdTypes)
                {
                    valcodeResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "" /*newType*/, item.Code, "", "", ""));
                }
                return valcodeResponse;
            }
            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to citizenship status valcode read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "ALT.ID.TYPES", It.IsAny<bool>())).ReturnsAsync(altIdTypesValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var altCredIdType = _alternativeCredentialTypesCollection.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "ALT.ID.TYPES", altCredIdType.Code }),
                            new RecordKeyLookupResult() { Guid = altCredIdType.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }

        [TestClass]
        public class TaxFormsTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<TaxForms2> allTaxForms;
            ApplValcodes taxFormsValcodeResponse;
            string domainEntityNameName;
            ApiSettings apiSettings;

            Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            IReferenceDataRepository referenceDataRepository;
           ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();
                apiSettings = new ApiSettings("TEST");

                allTaxForms = new List<TaxForms2>()
                {
                    new TaxForms2("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "A1"),
                    new TaxForms2("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "A2"),
                    new TaxForms2("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural", "A3")
                };
                taxFormsValcodeResponse = BuildValcodeResponse(allTaxForms);
                var taxFormsValResponse = new List<string>() { "2" };
                taxFormsValcodeResponse.ValActionCode1 = taxFormsValResponse;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                referenceDataRepo = BuildValidReferenceDataRepository();
                domainEntityNameName = referenceDataRepo.BuildFullCacheKey("CORE_TAX.FORMS");

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                   x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                   .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                taxFormsValcodeResponse = null;
                allTaxForms = null;
                referenceDataRepo = null;
            }


            [TestMethod]
            public async Task BaseReferenceDataRepo_GetsTaxFormsCacheAsync()
            {
                var taxForms = await referenceDataRepo.GetTaxFormsBaseAsync(It.IsAny<bool>());

                for (int i = 0; i < allTaxForms.Count(); i++)
                {
                    Assert.AreEqual(allTaxForms.ElementAt(i).Code, taxForms.ElementAt(i).Code);
                    Assert.AreEqual(allTaxForms.ElementAt(i).Description, taxForms.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetsTaxFormsNonCacheAsync()
            {
                var statuses = await referenceDataRepo.GetTaxFormsBaseAsync(true);

                for (int i = 0; i < allTaxForms.Count(); i++)
                {
                    Assert.AreEqual(allTaxForms.ElementAt(i).Code, statuses.ElementAt(i).Code);
                    Assert.AreEqual(allTaxForms.ElementAt(i).Description, statuses.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetTaxForms_WritesToCacheAsync()
            {

                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "false" to indicate item is not in cache
                //  -to cache "Get" request, return null so we know it's reading from the "repository"
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(null);

                // return a valid response to the data accessor request
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE", "TAX.FORMS", It.IsAny<bool>())).ReturnsAsync(taxFormsValcodeResponse);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
                 x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                 .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                // But after data accessor read, set up mocking so we can verify the list of taxForms was written to the cache
                cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<TaxForms>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                cacheProviderMock.Setup(x => x.Contains(referenceDataRepo.BuildFullCacheKey("CORE_TAX.FORMS"), null)).Returns(true);
                var taxForms = await referenceDataRepo.GetTaxFormsBaseAsync(It.IsAny<bool>());
                cacheProviderMock.Setup(x => x.Get(referenceDataRepo.BuildFullCacheKey("CORE_TAX.FORMS"), null)).Returns(taxForms);
                // Verify that taxForms were returned, which means they came from the "repository".
                Assert.IsTrue(taxForms.Count() == 3);

                // Verify that the taxForms item was added to the cache after it was read from the repository
                cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<TaxForms>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

            }

            [TestMethod]
            public async Task BaseReferenceDataRepo_GetTaxForms_GetsCachedTaxFormsAsync()
            {
                // Set up local cache mock to respond to cache request:
                //  -to "Contains" request, return "true" to indicate item is in cache
                //  -to "Get" request, return the cache item (in this case the "TAX.FORMS" cache item)
                cacheProviderMock.Setup(x => x.Contains(domainEntityNameName, null)).Returns(true);
                cacheProviderMock.Setup(x => x.Get(domainEntityNameName, null)).Returns(allTaxForms).Verifiable();

                // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE", "TAX.FORMS", true)).ReturnsAsync(new ApplValcodes());

                // Assert the taxForms are returned
                var results = await referenceDataRepo.GetTaxFormsBaseAsync(It.IsAny<bool>());
                Assert.IsTrue(results.Count() == 3);
                // Verify that the staxForms were retrieved from cache
                //cacheProviderMock.Verify(m => m.Get(domainEntityNameName, null));
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to taxForms domainEntityName read
                dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CORE.VALCODES", "TAX.FORMS", It.IsAny<bool>())).ReturnsAsync(taxFormsValcodeResponse);
                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var taxForms = allTaxForms.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "CORE.VALCODES", "TAX.FORMS", taxForms.Code }),
                            new RecordKeyLookupResult() { Guid = taxForms.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }

            private ApplValcodes BuildValcodeResponse(IEnumerable<TaxForms2> taxForms)
            {
                ApplValcodes taxFormsResponse = new ApplValcodes();
                taxFormsResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
                foreach (var item in taxForms)
                {
                    taxFormsResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "2", item.Code, "3", "", ""));
                }
                return taxFormsResponse;
            }
        }

        /// <summary>
        /// Test class for ZipCodeXlats codes
        /// </summary>
        [TestClass]
        public class BoxCodesTests
        {
            Mock<IColleagueTransactionFactory> transFactoryMock;
            Mock<ICacheProvider> cacheProviderMock;
            Mock<IColleagueDataReader> dataAccessorMock;
            Mock<ILogger> loggerMock;
            IEnumerable<Domain.Base.Entities.BoxCodes> allBoxCodes;
            string codeItemName;
            private ApiSettings apiSettings;
            ReferenceDataRepository referenceDataRepo;

            [TestInitialize]
            public void Initialize()
            {
                loggerMock = new Mock<ILogger>();

                // Build responses used for mocking
                BuildData();

                // Build repository
                referenceDataRepo = BuildValidReferenceDataRepository();
                codeItemName = referenceDataRepo.BuildFullCacheKey("AllBoxCodes");

                apiSettings = new ApiSettings("TEST");
            }

            private void BuildData()
            {
                allBoxCodes = new List<Domain.Base.Entities.BoxCodes>()
                {
                    new Domain.Base.Entities.BoxCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic", "W2"),
                    new Domain.Base.Entities.BoxCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic", "W2"),
                    new Domain.Base.Entities.BoxCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Test", "W2")
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                transFactoryMock = null;
                dataAccessorMock = null;
                cacheProviderMock = null;
                allBoxCodes = null;
                referenceDataRepo = null;
            }

            [TestMethod]
            public async Task GetsBoxCodesCacheAsync()
            {
                var boxCodes = await referenceDataRepo.GetAllBoxCodesAsync(false);

                for (int i = 0; i < allBoxCodes.Count(); i++)
                {
                    Assert.AreEqual(allBoxCodes.ElementAt(i).Guid, boxCodes.ElementAt(i).Guid);
                    Assert.AreEqual(allBoxCodes.ElementAt(i).Code, boxCodes.ElementAt(i).Code);
                    Assert.AreEqual(allBoxCodes.ElementAt(i).Description, boxCodes.ElementAt(i).Description);
                }
            }

            [TestMethod]
            public async Task GetsBoxCodessNonCacheAsync()
            {
                var boxCodes = await referenceDataRepo.GetAllBoxCodesAsync(true);

                for (int i = 0; i < allBoxCodes.Count(); i++)
                {
                    Assert.AreEqual(allBoxCodes.ElementAt(i).Guid, boxCodes.ElementAt(i).Guid);
                    Assert.AreEqual(allBoxCodes.ElementAt(i).Code, boxCodes.ElementAt(i).Code);
                    Assert.AreEqual(allBoxCodes.ElementAt(i).Description, boxCodes.ElementAt(i).Description);
                }
            }

            private ReferenceDataRepository BuildValidReferenceDataRepository()
            {
                // transaction factory mock
                transFactoryMock = new Mock<IColleagueTransactionFactory>();
                // Cache Provider Mock
                cacheProviderMock = new Mock<ICacheProvider>();
                // Set up data accessor for mocking 
                dataAccessorMock = new Mock<IColleagueDataReader>();

                // Set up dataAccessorMock as the object for the DataAccessor
                transFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataAccessorMock.Object);

                // Setup response to Chapters read
                var boxCodesCollection = new Collection<Data.Base.DataContracts.BoxCodes>(allBoxCodes.Select(record =>
                    new Data.Base.DataContracts.BoxCodes()
                    {
                        BxcBoxNumber = record.Code,
                        Recordkey = record.Code,
                        RecordGuid = record.Guid,
                        BxcDesc = record.Description
                    }).ToList());

                dataAccessorMock.Setup(acc => acc.BulkReadRecordAsync<Data.Base.DataContracts.BoxCodes>("BOX.CODES", "", true))
                    .ReturnsAsync(boxCodesCollection);

                cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
                    .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


                dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
                {
                    var result = new Dictionary<string, RecordKeyLookupResult>();
                    foreach (var recordKeyLookup in recordKeyLookups)
                    {
                        var vocation = allBoxCodes.Where(e => e.Code == recordKeyLookup.PrimaryKey).FirstOrDefault();
                        result.Add(string.Join("+", new string[] { "BOX.CODES", vocation.Code }),
                            new RecordKeyLookupResult() { Guid = vocation.Guid });
                    }
                    return Task.FromResult(result);
                });

                // Construct repository
                referenceDataRepo = new ReferenceDataRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                return referenceDataRepo;
            }
        }
    }
}