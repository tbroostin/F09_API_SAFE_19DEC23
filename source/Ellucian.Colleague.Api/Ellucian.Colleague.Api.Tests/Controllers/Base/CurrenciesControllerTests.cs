// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CurrenciesControllerTests
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private ICurrenciesService currenciesService;
        private Mock<ICurrenciesService> currenciesServiceMock;

        private CurrenciesController currenciesController;
        private IEnumerable<Domain.Base.Entities.CurrencyConv> allCurrencies;
        
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        private List<Dtos.Currencies> currencyCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
           

            currenciesServiceMock = new Mock<ICurrenciesService>();
            currenciesService = currenciesServiceMock.Object;


            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
          
            logger = new Mock<ILogger>().Object;

            allCurrencies = BuildCurrencies();
            
            currencyCollection = new List<Dtos.Currencies>();
            foreach (var source in allCurrencies)
            {
                var currencyDto = new Ellucian.Colleague.Dtos.Currencies
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    ISOCode = source.IsoCode
                };
                currencyCollection.Add(currencyDto);
            }

            currenciesController = new CurrenciesController(currenciesService, adapterRegistry, logger)
            {
                Request = new HttpRequestMessage()
            };
            currenciesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

        }

        [TestCleanup]
        public void Cleanup()
        {
            currenciesController = null;
            currenciesServiceMock = null;
            currenciesService = null;
            adapterRegistryMock = null;
        }

        [TestMethod]
        public async Task CurrenciesController_GetCurrencies_ValidateFields_Nocache()
        {
            currenciesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            currenciesServiceMock.Setup(x => x.GetCurrenciesAsync(false)).ReturnsAsync(currencyCollection);

            var sourceContexts = (await currenciesController.GetCurrenciesAsync()).ToList();
            Assert.AreEqual(currencyCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = currencyCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.ISOCode, actual.ISOCode, "IsoCode, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task CurrenciesController_GetCurrencies_ValidateFields_Cache()
        {
            currenciesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            currenciesServiceMock.Setup(x => x.GetCurrenciesAsync(true)).ReturnsAsync(currencyCollection);

            var sourceContexts = (await currenciesController.GetCurrenciesAsync()).ToList();
            Assert.AreEqual(currencyCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = currencyCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.ISOCode, actual.ISOCode, "IsoCode, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrencies_KeyNotFoundException()
        {
            //
            currenciesServiceMock.Setup(x => x.GetCurrenciesAsync(false))
                .Throws<KeyNotFoundException>();
            await currenciesController.GetCurrenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrencies_PermissionsException()
        {

            currenciesServiceMock.Setup(x => x.GetCurrenciesAsync(false))
                .Throws<PermissionsException>();
            await currenciesController.GetCurrenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrencies_ArgumentException()
        {

         
            currenciesServiceMock.Setup(x => x.GetCurrenciesAsync(false))
                .Throws<ArgumentException>();
            await currenciesController.GetCurrenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrencies_RepositoryException()
        {

            currenciesServiceMock.Setup(x => x.GetCurrenciesAsync(false))
                .Throws<RepositoryException>();
            await currenciesController.GetCurrenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrencies_IntegrationApiException()
        {

            currenciesServiceMock.Setup(x => x.GetCurrenciesAsync(false))
                .Throws<IntegrationApiException>();
            await currenciesController.GetCurrenciesAsync();
        }

        [TestMethod]
        public async Task CurrenciesController_GetCurrenciesByGuidAsync_ValidateFields()
        {
            var expected = currencyCollection.FirstOrDefault();
            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await currenciesController.GetCurrenciesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            Assert.AreEqual(expected.ISOCode, actual.ISOCode, "IsoCode");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrencies_Exception()
        {
            currenciesServiceMock.Setup(x => x.GetCurrenciesAsync(false)).Throws<Exception>();
            await currenciesController.GetCurrenciesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrenciesByGuidAsync_Exception()
        {
            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await currenciesController.GetCurrenciesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrenciesByGuid_KeyNotFoundException()
        {
            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await currenciesController.GetCurrenciesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrenciesByGuid_PermissionsException()
        {
            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await currenciesController.GetCurrenciesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrenciesByGuid_ArgumentException()
        {
            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await currenciesController.GetCurrenciesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrenciesByGuid_RepositoryException()
        {
            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await currenciesController.GetCurrenciesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrenciesByGuid_IntegrationApiException()
        {
            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await currenciesController.GetCurrenciesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_GetCurrenciesByGuid_Exception()
        {
            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await currenciesController.GetCurrenciesByGuidAsync(Guid.NewGuid().ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_PostCurrenciesAsync_Exception()
        {
            await currenciesController.PostCurrenciesAsync(currencyCollection.FirstOrDefault());
        }

        [TestMethod]     
        public async Task CurrenciesController_PutCurrenciesAsync()
        {
            var expected = currencyCollection.FirstOrDefault();

            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            currenciesServiceMock.Setup(x => x.PutCurrenciesAsync(expected.Id, It.IsAny<Currencies>())).ReturnsAsync(expected);

            var actual   = await currenciesController.PutCurrenciesAsync(expected.Id, expected);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            Assert.AreEqual(expected.ISOCode, actual.ISOCode, "IsoCode");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_PutCurrenciesAsync_InvalidKey()
        {
            var expected = currencyCollection.FirstOrDefault();

            currenciesServiceMock.Setup(x => x.PutCurrenciesAsync(expected.Id, It.IsAny<Currencies>()))
                    .Throws<KeyNotFoundException>();

            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();

            await currenciesController.PutCurrenciesAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_PutCurrenciesAsync_RepositoryExcecption()
        {
            var expected = currencyCollection.FirstOrDefault();

            currenciesServiceMock.Setup(x => x.GetCurrenciesByGuidAsync(expected.Id, It.IsAny<bool>()))
                .Throws<RepositoryException>();

            await currenciesController.PutCurrenciesAsync(expected.Id, expected);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CurrenciesController_DeleteCurrenciesAsync_Exception()
        {
            await currenciesController.DeleteCurrenciesAsync(currencyCollection.FirstOrDefault().Id);
        }


        private IEnumerable<Domain.Base.Entities.CurrencyConv> BuildCurrencies()
        {
            return new List<Domain.Base.Entities.CurrencyConv>()
                {
                    new Domain.Base.Entities.CurrencyConv(Guid.NewGuid().ToString(), "EUR", "Euro",  "EUR"),
                    new Domain.Base.Entities.CurrencyConv(Guid.NewGuid().ToString(), "USA", "US Dollars", "USD"),
                    new Domain.Base.Entities.CurrencyConv(Guid.NewGuid().ToString(), "CAD", "Canadian", "CAD")
                };
        }
    }
}
