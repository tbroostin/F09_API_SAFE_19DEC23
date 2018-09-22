// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PersonFiltersControllerTests
    {
        [TestClass]
        public class PersonFiltersControllerGet
        {
            #region Test Context

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

            #endregion

            private PersonFiltersController PersonFiltersController;
            private Mock<IReferenceDataRepository> ReferenceRepositoryMock;
            private IReferenceDataRepository ReferenceRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonFilter> allPersonFilterEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IDemographicService> demographicServiceMock;
            private IDemographicService demographicService;
            List<PersonFilter> PersonFilterList;
            private string personFiltersGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                ReferenceRepositoryMock = new Mock<IReferenceDataRepository>();
                ReferenceRepository = ReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.PersonFilter, PersonFilter>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                demographicServiceMock = new Mock<IDemographicService>();
                demographicService = demographicServiceMock.Object;

                allPersonFilterEntities = new TestPersonFilterRepository().GetPersonFilters();
                PersonFilterList = new List<PersonFilter>();

                PersonFiltersController = new PersonFiltersController(demographicService, logger);
                PersonFiltersController.Request = new HttpRequestMessage();
                PersonFiltersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var personFilter in allPersonFilterEntities)
                {
                    PersonFilter target = ConvertPersonFiltersEntityToDto(personFilter);
                    PersonFilterList.Add(target);
                }
                ReferenceRepositoryMock.Setup(x => x.GetPersonFiltersAsync(It.IsAny<bool>())).ReturnsAsync(allPersonFilterEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                PersonFiltersController = null;
                ReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetPersonFiltersByGuidAsync_Validate()
            {
                var thisPersonFilter = PersonFilterList.Where(m => m.Id == personFiltersGuid).FirstOrDefault();

                demographicServiceMock.Setup(x => x.GetPersonFilterByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisPersonFilter);

                var personFilter = await PersonFiltersController.GetPersonFilterByIdAsync(personFiltersGuid);
                Assert.AreEqual(thisPersonFilter.Id, personFilter.Id);
                Assert.AreEqual(thisPersonFilter.Code, personFilter.Code);
                Assert.AreEqual(thisPersonFilter.Description, personFilter.Description);
            }

            [TestMethod]
            public async Task PersonFiltersController_GetHedmAsync()
            {
                demographicServiceMock.Setup(gc => gc.GetPersonFiltersAsync(It.IsAny<bool>())).ReturnsAsync(PersonFilterList);

                var result = await PersonFiltersController.GetPersonFiltersAsync();
                Assert.AreEqual(result.Count(), allPersonFilterEntities.Count());

                int count = allPersonFilterEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PersonFilterList[i];
                    var actual = allPersonFilterEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PersonFiltersController_GetHedmAsync_CacheControlNotNull()
            {
                PersonFiltersController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                demographicServiceMock.Setup(gc => gc.GetPersonFiltersAsync(It.IsAny<bool>())).ReturnsAsync(PersonFilterList);

                var result = await PersonFiltersController.GetPersonFiltersAsync();
                Assert.AreEqual(result.Count(), allPersonFilterEntities.Count());

                int count = allPersonFilterEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PersonFilterList[i];
                    var actual = allPersonFilterEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PersonFiltersController_GetHedmAsync_NoCache()
            {
                PersonFiltersController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                PersonFiltersController.Request.Headers.CacheControl.NoCache = true;

                demographicServiceMock.Setup(gc => gc.GetPersonFiltersAsync(It.IsAny<bool>())).ReturnsAsync(PersonFilterList);

                var result = await PersonFiltersController.GetPersonFiltersAsync();
                Assert.AreEqual(result.Count(), allPersonFilterEntities.Count());

                int count = allPersonFilterEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PersonFilterList[i];
                    var actual = allPersonFilterEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PersonFiltersController_GetHedmAsync_Cache()
            {
                PersonFiltersController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                PersonFiltersController.Request.Headers.CacheControl.NoCache = false;

                demographicServiceMock.Setup(gc => gc.GetPersonFiltersAsync(It.IsAny<bool>())).ReturnsAsync(PersonFilterList);

                var result = await PersonFiltersController.GetPersonFiltersAsync();
                Assert.AreEqual(result.Count(), allPersonFilterEntities.Count());

                int count = allPersonFilterEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = PersonFilterList[i];
                    var actual = allPersonFilterEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task PersonFiltersController_GetByIdHedmAsync()
            {
                var thisPersonFilter = PersonFilterList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                demographicServiceMock.Setup(x => x.GetPersonFilterByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisPersonFilter);

                var personFilter = await PersonFiltersController.GetPersonFilterByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisPersonFilter.Id, personFilter.Id);
                Assert.AreEqual(thisPersonFilter.Code, personFilter.Code);
                Assert.AreEqual(thisPersonFilter.Description, personFilter.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonFiltersController_GetThrowsIntAppiExc()
            {
                demographicServiceMock.Setup(gc => gc.GetPersonFiltersAsync(It.IsAny<bool>())).Throws<Exception>();

                await PersonFiltersController.GetPersonFiltersAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonFiltersController_GetByIdThrowsIntAppiExc()
            {
                demographicServiceMock.Setup(gc => gc.GetPersonFilterByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await PersonFiltersController.GetPersonFilterByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonFiltersController_PostThrowsIntAppiExc()
            {
                await PersonFiltersController.PostPersonFilterAsync(PersonFilterList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonFiltersController_PutThrowsIntAppiExc()
            {
                var result = await PersonFiltersController.PutPersonFilterAsync(PersonFilterList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonFiltersController_DeleteThrowsIntAppiExc()
            {
                await PersonFiltersController.DeletePersonFilterAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
            /// <summary>
            /// Converts a PersonFilter domain entity to its corresponding PersonFilter DTO
            /// </summary>
            /// <param name="source">PersonFilter domain entity</param>
            /// <returns>PersonFilter DTO</returns>
            private Ellucian.Colleague.Dtos.PersonFilter ConvertPersonFiltersEntityToDto(Ellucian.Colleague.Domain.Base.Entities.PersonFilter source)
            {
                var personFilter = new Ellucian.Colleague.Dtos.PersonFilter();
                personFilter.Id = source.Guid;
                personFilter.Code = source.Code;
                personFilter.Title = source.Description;
                personFilter.Description = null;

                return personFilter;
            }
        }
    }
}