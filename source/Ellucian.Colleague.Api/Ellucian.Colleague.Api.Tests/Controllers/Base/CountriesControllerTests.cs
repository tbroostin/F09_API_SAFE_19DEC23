// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class CountriesControllerTests
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

        private IReferenceDataRepository referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private CountriesController countriesController;
        private IEnumerable<Domain.Base.Entities.Country> countries;
        private IAdapterRegistry adapterRegistry;
        private Mock<IAdapterRegistry> adapterRegistryMock;
        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            referenceDataRepository = referenceDataRepositoryMock.Object;

            adapterRegistryMock = new Mock<IAdapterRegistry>();
            adapterRegistry = adapterRegistryMock.Object;
            var adapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Country>(adapterRegistry, logger);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.Country, Country>()).Returns(adapter);

            logger = new Mock<ILogger>().Object;

            countries = BuildCountries();
            countriesController = new CountriesController(adapterRegistry, referenceDataRepository, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            countriesController = null;
            referenceDataRepository = null;
        }

        [TestMethod]
        public async Task CountriesController_ReturnsCountriesReasonDtos()
        {
            referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(false)).Returns(Task.FromResult(countries));
            var countryDtos = await countriesController.GetAsync();
            Assert.IsTrue(countryDtos is IEnumerable<Country>);
            Assert.AreEqual(countries.Count(), countryDtos.Count());
        }

        [TestMethod]
        public async Task CountriesController_NullRepositoryResponse_ReturnsEmptyCountriesReasonDtos()
        {
            IEnumerable<Domain.Base.Entities.Country> nullCountryEntities = null;
            referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(false)).Returns(Task.FromResult(nullCountryEntities));
            var countryDtos = await countriesController.GetAsync();
            Assert.IsTrue(countryDtos is IEnumerable<Country>);
            Assert.AreEqual(0, countryDtos.Count());
        }

        [TestMethod]
        public async Task CountriesController_EmptyRepositoryResponse_ReturnsEmptyCountriesReasonDtos()
        {
            IEnumerable<Domain.Base.Entities.Country> emptyCountryEntities = new List<Domain.Base.Entities.Country>();
            referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(false)).Returns(Task.FromResult(emptyCountryEntities));
            var countryDtos = await countriesController.GetAsync();
            Assert.IsTrue(countryDtos is IEnumerable<Country>);
            Assert.AreEqual(0, countryDtos.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task CountriesController_Exception_ReturnsHttpResponseException_BadRequest()
        {
            try
            {
                referenceDataRepositoryMock.Setup(x => x.GetCountryCodesAsync(false)).Throws(new ApplicationException());
                var countryDtos = await countriesController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                throw ex;
            }
        }

        private IEnumerable<Domain.Base.Entities.Country> BuildCountries()
        {
            var countries = new List<Domain.Base.Entities.Country>()
                {
                    new Domain.Base.Entities.Country("LIFE", "Life Experience", "iso1"),
                    new Domain.Base.Entities.Country("OTHER", "Other reason", "iso2")
                };

            return countries;
        }
    }
}
