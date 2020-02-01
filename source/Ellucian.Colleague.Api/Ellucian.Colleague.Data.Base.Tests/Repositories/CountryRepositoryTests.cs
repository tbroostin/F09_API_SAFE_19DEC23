// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class CountryRepositoryTests : BaseRepositorySetup
    {
        string cacheKey;
        CountryRepository countriesRepository;
        Country countryEntity;
        Collection<Countries> _allCountriesDataContracts;
        UpdateCountryRequest updateRequest;
        UpdateCountryResponse updateResponse;

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();

            BuildObjects();

            var country = _allCountriesDataContracts.FirstOrDefault(x => x.Recordkey.Equals("US"));

            dataReaderMock.Setup(i => i.ReadRecordAsync<Countries>("US", It.IsAny<bool>())).ReturnsAsync(country);

            //dataReaderMock.Setup(i => i.BulkReadRecordAsync<Countries>(It.IsAny<string>(), It.IsAny<bool>()))
            //    .ReturnsAsync(_allCountriesDataContracts);

            MockRecords<Countries>("COUNTRIES", _allCountriesDataContracts);
            MockRecord<Countries>("COUNTRIES", country, country.RecordGuid);

            cacheProviderMock.Setup<Task<Tuple<object, SemaphoreSlim>>>(x => x.GetAndLockSemaphoreAsync(It.IsAny<string>(), null))
              .ReturnsAsync(new Tuple<object, SemaphoreSlim>(null, new SemaphoreSlim(1, 1)));


            countriesRepository = new CountryRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
            cacheKey = countriesRepository.BuildFullCacheKey("AllCountries2");

           
        }

        private void BuildObjects()
        {
            var allCountries = TestCountriesRepository.GetAllCountriesRecords();
            _allCountriesDataContracts = new Collection<Countries>(allCountries.ToList());

            var record = _allCountriesDataContracts.FirstOrDefault(x => x.Recordkey.Equals("US"));

            countryEntity = new Country(record.RecordGuid, record.Recordkey, record.CtryDesc, record.CtryIsoCode, record.CtryIsoAlpha3Code);
            updateRequest = new UpdateCountryRequest()
            {
                Country = countryEntity.Code,
                IsoCode = "XXX"
            };

            updateResponse = new UpdateCountryResponse()
            {
                CountryErrors = null
            };

        }

        [TestCleanup]
        public void Cleanup()
        {

            MockCleanup();

            countriesRepository = null;
            countryEntity = null;

            updateRequest = null;
            updateResponse = null;
        }

        [TestMethod]
        public async Task CountryRepo_GetCountryCodesAsync_Cache()
        {
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<Countries>("", It.IsAny<bool>()))
                .ReturnsAsync(_allCountriesDataContracts);

            var results = await countriesRepository.GetCountryCodesAsync(false);

            foreach (var result in results)
            {
                var expected = _allCountriesDataContracts.FirstOrDefault(i => i.RecordGuid.Equals(result.Guid));

                Assert.AreEqual(expected.RecordGuid, result.Guid);
            }
        }

        //[TestMethod]
        //public async Task CountryRepo_GetCountryCodesAsync_NoCache()
        //{
        //    dataReaderMock.Setup(i => i.BulkReadRecordAsync<Countries>("", It.IsAny<bool>()))
        //        .ReturnsAsync(_allCountriesDataContracts);

        //    dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Countries>("COUNTRIES", "", true))
        //       .ReturnsAsync(_allCountriesDataContracts);
        //   // cacheProviderMock.Setup(x => x.Contains(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        //    cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()))
        //        .Returns(true);

        //    // But after data accessor read, set up mocking so we can verify the list of citizenship statuses was written to the cache
        //    cacheProviderMock.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<Task<IEnumerable<CitizenshipStatus>>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

        //    cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(true);
            
        //    cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(_allCountriesDataContracts);

        //    var results = await countriesRepository.GetCountryCodesAsync(true);

        //    foreach (var result in results)
        //    {
        //        var expected = _allCountriesDataContracts.FirstOrDefault(i => i.RecordGuid.Equals(result.Guid));

        //        Assert.AreEqual(expected.RecordGuid, result.Guid);
        //    }
        //}

        //[TestMethod]
        //public async Task CountryRepo_UpdateCountryAsync()
        //{
        //    dataReaderMock.Setup(i => i.BulkReadRecordAsync<Countries>("", It.IsAny<bool>()))
        //        .ReturnsAsync(_allCountriesDataContracts);

        //    dataReaderMock.Setup(i => i.BulkReadRecordAsync<Countries>(It.IsAny<string>(), It.IsAny<bool>()))
        //        .ReturnsAsync(_allCountriesDataContracts);

        //    dataReaderMock.Setup(acc => acc.BulkReadRecordAsync<DataContracts.Countries>("COUNTRIES", "", true))
        //        .ReturnsAsync(_allCountriesDataContracts);

        //    transManagerMock.Setup(i => i.ExecuteAsync<UpdateCountryRequest, UpdateCountryResponse>(It.IsAny<UpdateCountryRequest>())).ReturnsAsync(updateResponse);
        //    var result = await countriesRepository.UpdateCountryAsync(countryEntity);

        //    Assert.AreEqual(updateRequest.Country, result.Code);
        //}

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task CountryRepo_UpdateCountryAsync_Empty()
        {
            transManagerMock.Setup(i => i.ExecuteAsync<UpdateCountryRequest, UpdateCountryResponse>(It.IsAny<UpdateCountryRequest>())).ReturnsAsync(updateResponse);
            var result = await countriesRepository.UpdateCountryAsync(null);

        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task CountryRepo_UpdateCountryAsync_Error()
        {
            updateResponse = new UpdateCountryResponse()
            {
                CountryErrors = new List<CountryErrors>()
                    { new CountryErrors() { ErrorCode = "error", ErrorMsg = "An error has occurred." } }
            };

            transManagerMock.Setup(i => i.ExecuteAsync<UpdateCountryRequest, UpdateCountryResponse>(It.IsAny<UpdateCountryRequest>())).ReturnsAsync(updateResponse);
            var result = await countriesRepository.UpdateCountryAsync(countryEntity);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task CountryRepo_UpdateCountryAsync_InvalidOperationException()
        {
            updateRequest.Country = Guid.Empty.ToString();
            transManagerMock.Setup(i => i.ExecuteAsync<UpdateCountryRequest, UpdateCountryResponse>(It.IsAny<UpdateCountryRequest>())).Throws<InvalidOperationException>();
            var result = await countriesRepository.UpdateCountryAsync(countryEntity);
        }

     
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CountryRepo_GetCountryByGuidAsync_empty()
        {
            var result = await countriesRepository.GetCountryByGuidAsync("");
        }
    }
}