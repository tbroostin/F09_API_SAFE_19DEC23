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

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class CurrencyRepositoryTests : BaseRepositorySetup
    {
        string cacheKey;
        CurrencyRepository currencyRepository;
        Domain.Base.Entities.CurrencyConv currencyEntity;
        Collection<DataContracts.CurrencyConv> _allCurrencyConvDataContracts;
        UpdateCurrencyRequest updateRequest;
        UpdateCurrencyResponse updateResponse;
  
        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            BuildObjects();

            var currency = _allCurrencyConvDataContracts.FirstOrDefault(x => x.Recordkey.Equals("USA"));

            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.CurrencyConv>("USA", It.IsAny<bool>())).ReturnsAsync(currency);

            MockRecords<DataContracts.CurrencyConv>("CURRENCY.CONV", _allCurrencyConvDataContracts);
            MockRecord<DataContracts.CurrencyConv>("CURRENCY.CONV", currency, currency.RecordGuid);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
              .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            
            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.CurrencyConv>("CURRENCY.CONV", "", It.IsAny<bool>()))
                .ReturnsAsync(_allCurrencyConvDataContracts);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var record = _allCurrencyConvDataContracts.Where(e => e.CurrencyConvIsoCode == recordKeyLookup.PrimaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "CURRENCY.CONV", record.CurrencyConvIsoCode }),
                        new RecordKeyLookupResult() { Guid = record.RecordGuid });
                }
                return Task.FromResult(result);
            });


            dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>())).Returns<GuidLookup[]>(recordKeyLookups =>
            {
                var results = new Dictionary<string, GuidLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var record = _allCurrencyConvDataContracts.Where(e => e.RecordGuid == recordKeyLookup.Guid).FirstOrDefault();
                    results.Add(record.RecordGuid,
                      new GuidLookupResult() { PrimaryKey = record.CurrencyConvIsoCode, Entity = "CURRENCY.CONV" });
                }
                return Task.FromResult(results);
            });
            currencyRepository = new CurrencyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            cacheKey = currencyRepository.BuildFullCacheKey("AllCurrencyCodes2");
        }

        private void BuildObjects()
        {
            _allCurrencyConvDataContracts = BuildCurrencies();

            var record = _allCurrencyConvDataContracts.FirstOrDefault(x => x.Recordkey.Equals("USA"));

            currencyEntity = new Domain.Base.Entities.CurrencyConv(record.RecordGuid, record.Recordkey, record.CurrencyConvDesc, record.CurrencyConvIsoCode);
            updateRequest = new UpdateCurrencyRequest()
            {
                Currencies = currencyEntity.Code,
                IsoCode = "USD"
            };

            updateResponse = new UpdateCurrencyResponse()
            {
                CurrencyConvErrors = null
            };        
        }

        [TestCleanup]
        public void Cleanup()
        {

            MockCleanup();

            currencyRepository = null;
            currencyEntity = null;

            updateRequest = null;
            updateResponse = null;
        }

        [TestMethod]
        public async Task CurrencyRepo_GetCurrencyCodesAsync_Cache()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.CurrencyConv>("", It.IsAny<bool>()))
                .ReturnsAsync(_allCurrencyConvDataContracts);

            var results = await currencyRepository.GetCurrencyConversionAsync(false);

            foreach (var result in results)
            {
                var expected = _allCurrencyConvDataContracts.FirstOrDefault(i => i.RecordGuid.Equals(result.Guid));

                Assert.AreEqual(expected.RecordGuid, result.Guid);
            }
        }

        [TestMethod]
        public async Task CurrencyRepo_GetCurrencyCodesAsync_NoCache()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.CurrencyConv>("", It.IsAny<bool>()))
                .ReturnsAsync(_allCurrencyConvDataContracts);

            var results = await currencyRepository.GetCurrencyConversionAsync(true);

            foreach (var result in results)
            {
                var expected = _allCurrencyConvDataContracts.FirstOrDefault(i => i.RecordGuid.Equals(result.Guid));

                Assert.AreEqual(expected.RecordGuid, result.Guid);
            }
        }

        [TestMethod]
        public async Task CurrencyRepo_GetCurrencyConversionByGuidAsync()
        {
            var expectedDataContract = _allCurrencyConvDataContracts.FirstOrDefault();

            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.CurrencyConv>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedDataContract);

            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.CurrencyConv>("", It.IsAny<bool>()))
                .ReturnsAsync(_allCurrencyConvDataContracts);

            var result = await currencyRepository.GetCurrencyConversionByGuidAsync(expectedDataContract.RecordGuid);

            var expected = _allCurrencyConvDataContracts.FirstOrDefault(i => i.RecordGuid.Equals(result.Guid));

            Assert.AreEqual(expected.RecordGuid, result.Guid);
            Assert.AreEqual(expected.CurrencyConvIsoCode, result.Code);

        }

        [TestMethod]
        public async Task CurrencyRepo_UpdateCurrencyAsync()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.CurrencyConv>("", It.IsAny<bool>()))
                .ReturnsAsync(_allCurrencyConvDataContracts);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.CurrencyConv>(It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(_allCurrencyConvDataContracts);

            dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.CurrencyConv>("CURRENCY.CONV", "", true))
                 .ReturnsAsync(_allCurrencyConvDataContracts);


            transManagerMock.Setup(i => i.ExecuteAsync<UpdateCurrencyRequest, UpdateCurrencyResponse>(It.IsAny<UpdateCurrencyRequest>())).ReturnsAsync(updateResponse);
            var result = await currencyRepository.UpdateCurrencyConversionAsync(currencyEntity);

            Assert.AreEqual(updateRequest.Currencies, result.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task CurrencyRepo_UpdateCurrencyAsync_Empty()
        {
            transManagerMock.Setup(i => i.ExecuteAsync<UpdateCurrencyRequest, UpdateCurrencyResponse>(It.IsAny<UpdateCurrencyRequest>())).ReturnsAsync(updateResponse);
            var result = await currencyRepository.UpdateCurrencyConversionAsync(null);

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task CurrencyRepo_UpdateCurrencyAsync_Error()
        {
            updateResponse = new UpdateCurrencyResponse()
            {
                CurrencyConvErrors = new List<CurrencyConvErrors>()
                    { new CurrencyConvErrors() { ErrorCode = "error", ErrorMsg = "An error has occurred." } }
            };

            transManagerMock.Setup(i => i.ExecuteAsync<UpdateCurrencyRequest, UpdateCurrencyResponse>(It.IsAny<UpdateCurrencyRequest>())).ReturnsAsync(updateResponse);
            var result = await currencyRepository.UpdateCurrencyConversionAsync(currencyEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CurrencyRepo_UpdateCurrencyAsync_InvalidOperationException()
        {
            updateRequest.Currencies = Guid.Empty.ToString();
            transManagerMock.Setup(i => i.ExecuteAsync<UpdateCurrencyRequest, UpdateCurrencyResponse>(It.IsAny<UpdateCurrencyRequest>())).Throws<InvalidOperationException>();
            var result = await currencyRepository.UpdateCurrencyConversionAsync(currencyEntity);
        }
     
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CurrencyRepo_GetCurrencyByGuidAsync_Empty()
        {
            var result = await currencyRepository.GetCurrencyConversionByGuidAsync("");
        }

        private Collection<DataContracts.CurrencyConv> BuildCurrencies()
        {
            var collection = new Collection<DataContracts.CurrencyConv>();

            collection.Add(new DataContracts.CurrencyConv()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                Recordkey = "EUR",
                CurrencyConvDesc = "Euro",
                CurrencyConvIsoCode = "EUR"
            });
            collection.Add(new DataContracts.CurrencyConv()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                Recordkey = "USA",
                CurrencyConvDesc = "US Dollars",
                CurrencyConvIsoCode = "USD"
            });
            collection.Add(new DataContracts.CurrencyConv()
            {
                RecordGuid = Guid.NewGuid().ToString(),
                Recordkey = "CAD",
                CurrencyConvDesc = "Canadian",
                CurrencyConvIsoCode = "CAD"
            });
            return collection;
        }
    }

    /// <summary>
    /// Test class for IntgIsoCurrencyCodes codes
    /// </summary>
    [TestClass]
    public class IntgIsoCurrencyCodes_Tests
    {
        Mock<IColleagueTransactionFactory> transFactoryMock;
        Mock<ICacheProvider> cacheProviderMock;
        Mock<IColleagueDataReader> dataAccessorMock;
        Mock<ILogger> loggerMock;
        IEnumerable<IntgIsoCurrencyCodes> allIntgIsoCurrencyCodes;
        ApplValcodes intgIsoCurrencyCodesValcodeResponse;
        string valcodeName;
        private ApiSettings apiSettings;

        CurrencyRepository currencyRepository;

        [TestInitialize]
        public void Initialize()
        {
            loggerMock = new Mock<ILogger>();

            allIntgIsoCurrencyCodes = new List<IntgIsoCurrencyCodes>()
                {
                    new IntgIsoCurrencyCodes("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "USA", "United States", "A"),
                    new IntgIsoCurrencyCodes("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "CAN", "Canada", "A"),
                    new IntgIsoCurrencyCodes("d2253ac7-9931-4560-b42f-1fccd43c952e", "RUB", "Russia", "A")
                };
            intgIsoCurrencyCodesValcodeResponse = BuildValcodeResponse(allIntgIsoCurrencyCodes);


            // Build intgIsoCurrencyCodes repository
            currencyRepository = BuildValidReferenceDataRepository();
            valcodeName = currencyRepository.BuildFullCacheKey("CF_INTG.ISO.CURRENCY.CODES_GUID");

            apiSettings = new ApiSettings("TEST");
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            dataAccessorMock = null;
            cacheProviderMock = null;
            intgIsoCurrencyCodesValcodeResponse = null;
            allIntgIsoCurrencyCodes = null;
            currencyRepository = null;
        }

        [TestMethod]
        public async Task GetsIntgIsoCurrencyCodesCacheAsync()
        {
            for (int i = 0; i < allIntgIsoCurrencyCodes.Count(); i++)
            {
                Assert.AreEqual(allIntgIsoCurrencyCodes.ElementAt(i).Code, (await currencyRepository.GetIntgIsoCurrencyCodesAsync(false)).ElementAt(i).Code);
                Assert.AreEqual(allIntgIsoCurrencyCodes.ElementAt(i).Description, (await currencyRepository.GetIntgIsoCurrencyCodesAsync(false)).ElementAt(i).Description);
            }
        }

        [TestMethod]
        public async Task GetsIntgIsoCurrencyCodesNonCacheAsync()
        {
            for (int i = 0; i < allIntgIsoCurrencyCodes.Count(); i++)
            {
                Assert.AreEqual(allIntgIsoCurrencyCodes.ElementAt(i).Code, (await currencyRepository.GetIntgIsoCurrencyCodesAsync(true)).ElementAt(i).Code);
                Assert.AreEqual(allIntgIsoCurrencyCodes.ElementAt(i).Description, (await currencyRepository.GetIntgIsoCurrencyCodesAsync(true)).ElementAt(i).Description);
            }
        }

        [TestMethod]
        public async Task GetIntgIsoCurrencyCodes_WritesToCacheAsync()
        {

            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "false" to indicate item is not in cache
            //  -to cache "Get" request, return null so we know it's reading from the "repository"
            cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(false);
            cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(null);

            // return a valid response to the data accessor request
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.ISO.CURRENCY.CODES", It.IsAny<bool>())).ReturnsAsync(intgIsoCurrencyCodesValcodeResponse);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x =>
             x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
             .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            // But after data accessor read, set up mocking so we can verify the list of intgIsoCurrencyCodes was written to the cache
            cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgIsoCurrencyCodes>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

            cacheProviderMock.Setup(x => x.Contains(currencyRepository.BuildFullCacheKey("CF_INTG.ISO.CURRENCY.CODES"), null)).Returns(true);
            var intgIsoCurrencyCodes = await currencyRepository.GetIntgIsoCurrencyCodesAsync(false);
            cacheProviderMock.Setup(x => x.Get(currencyRepository.BuildFullCacheKey("CF_INTG.ISO.CURRENCY.CODES"), null)).Returns(intgIsoCurrencyCodes);
            // Verify that intgIsoCurrencyCodes were returned, which means they came from the "repository".
            Assert.IsTrue(intgIsoCurrencyCodes.Count() == 3);

            // Verify that the intgIsoCurrencyCodes item was added to the cache after it was read from the repository
            cacheProviderMock.Verify(m => m.Add(It.IsAny<string>(), It.IsAny<Task<List<IntgIsoCurrencyCodes>>>(), It.IsAny<CacheItemPolicy>(), null), Times.Never);

        }

        [TestMethod]
        public async Task GetIntgIsoCurrencyCodes_GetsCachedIntgIsoCurrencyCodesAsync()
        {
            // Set up local cache mock to respond to cache request:
            //  -to "Contains" request, return "true" to indicate item is in cache
            //  -to "Get" request, return the cache item (in this case the "PERSON.EMAIL.TYPES" cache item)
            cacheProviderMock.Setup(x => x.Contains(valcodeName, null)).Returns(true);
            cacheProviderMock.Setup(x => x.Get(valcodeName, null)).Returns(allIntgIsoCurrencyCodes).Verifiable();

            // return null for request, so that if we have a result, it wasn't the data accessor that returned it.
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.ISO.CURRENCY.CODES", true)).ReturnsAsync(new ApplValcodes());

            // Assert the intgIsoCurrencyCodes are returned
            Assert.IsTrue((await currencyRepository.GetIntgIsoCurrencyCodesAsync(false)).Count() == 3);
            // Verify that the intgIsoCurrencyCodes were retrieved from cache
            cacheProviderMock.Verify(m => m.Get(valcodeName, null));
        }

        private CurrencyRepository BuildValidReferenceDataRepository()
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

            // Setup response to intgIsoCurrencyCodes valcode read
            dataAccessorMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>("CF.VALCODES", "INTG.ISO.CURRENCY.CODES", It.IsAny<bool>())).ReturnsAsync(intgIsoCurrencyCodesValcodeResponse);
            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
            .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));

            dataAccessorMock.Setup(acc => acc.SelectAsync(It.IsAny<RecordKeyLookup[]>())).Returns<RecordKeyLookup[]>(recordKeyLookups =>
            {
                var result = new Dictionary<string, RecordKeyLookupResult>();
                foreach (var recordKeyLookup in recordKeyLookups)
                {
                    var intgIsoCurrencyCodes = allIntgIsoCurrencyCodes.Where(e => e.Code == recordKeyLookup.SecondaryKey).FirstOrDefault();
                    result.Add(string.Join("+", new string[] { "CF.VALCODES", "INTG.ISO.CURRENCY.CODES", intgIsoCurrencyCodes.Code }),
                        new RecordKeyLookupResult() { Guid = intgIsoCurrencyCodes.Guid });
                }
                return Task.FromResult(result);
            });

            // Construct repository
            currencyRepository = new CurrencyRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);

            return currencyRepository;
        }

        private ApplValcodes BuildValcodeResponse(IEnumerable<IntgIsoCurrencyCodes> intgIsoCurrencyCodes)
        {
            ApplValcodes intgIsoCurrencyCodesResponse = new ApplValcodes();
            intgIsoCurrencyCodesResponse.ValsEntityAssociation = new List<ApplValcodesVals>();
            foreach (var item in intgIsoCurrencyCodes)
            {
              
                intgIsoCurrencyCodesResponse.ValsEntityAssociation.Add(new ApplValcodesVals("", item.Description, "A", item.Code, "", "", ""));
            }
            return intgIsoCurrencyCodesResponse;
        }
    }
}