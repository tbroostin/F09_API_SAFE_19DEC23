// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmploymentProficienciesControllerTests
    {
        [TestClass]
        public class EmploymentProficienciesControllerGet
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

            private EmploymentProficienciesController EmploymentProficienciesController;
            private Mock<IHumanResourcesReferenceDataRepository> hrReferenceRepositoryMock;
            private IHumanResourcesReferenceDataRepository hrReferenceRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentProficiency> allEmploymentProficiencyEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IEmploymentProficiencyService> employmentProficiencyServiceMock;
            private IEmploymentProficiencyService employmentProficiencyService;
            List<Ellucian.Colleague.Dtos.EmploymentProficiency> EmploymentProficiencyList;
            private string employmentProficienciesGuid = "625c69ff-280b-4ed3-9474-662a43616a8a";
 
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                hrReferenceRepositoryMock = new Mock<IHumanResourcesReferenceDataRepository>();
                hrReferenceRepository = hrReferenceRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentProficiency, Dtos.EmploymentProficiency>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                employmentProficiencyServiceMock = new Mock<IEmploymentProficiencyService>();
                employmentProficiencyService = employmentProficiencyServiceMock.Object;

                allEmploymentProficiencyEntities = new TestEmploymentProficiencyRepository().GetEmploymentProficiencies();
                EmploymentProficiencyList = new List<Dtos.EmploymentProficiency>();

                EmploymentProficienciesController = new EmploymentProficienciesController(employmentProficiencyService, logger);
                EmploymentProficienciesController.Request = new HttpRequestMessage();
                EmploymentProficienciesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var employmentProficiency in allEmploymentProficiencyEntities)
                {
                    Dtos.EmploymentProficiency target = ConvertEmploymentProficiencyEntityToDto(employmentProficiency);
                    EmploymentProficiencyList.Add(target);
                }
                hrReferenceRepositoryMock.Setup(x => x.GetEmploymentProficienciesAsync(It.IsAny<bool>())).ReturnsAsync(allEmploymentProficiencyEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                EmploymentProficienciesController = null;
                hrReferenceRepository = null;
            }

            [TestMethod]
            public async Task GetEmploymentProficienciesByGuidAsync_Validate()
            {
                var thisEmploymentProficiency = EmploymentProficiencyList.Where(m => m.Id == employmentProficienciesGuid).FirstOrDefault();

                employmentProficiencyServiceMock.Setup(x => x.GetEmploymentProficiencyByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisEmploymentProficiency);

                var employmentProficiency = await EmploymentProficienciesController.GetEmploymentProficiencyByIdAsync(employmentProficienciesGuid);
                Assert.AreEqual(thisEmploymentProficiency.Id, employmentProficiency.Id);
                Assert.AreEqual(thisEmploymentProficiency.Code, employmentProficiency.Code);
                Assert.AreEqual(thisEmploymentProficiency.Description, employmentProficiency.Description);
            }

            [TestMethod]
            public async Task EmploymentProficienciesController_GetHedmAsync()
            {
                employmentProficiencyServiceMock.Setup(gc => gc.GetEmploymentProficienciesAsync(It.IsAny<bool>())).ReturnsAsync(EmploymentProficiencyList);

                var result = await EmploymentProficienciesController.GetEmploymentProficienciesAsync();
                Assert.AreEqual(result.Count(), allEmploymentProficiencyEntities.Count());

                int count = allEmploymentProficiencyEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = EmploymentProficiencyList[i];
                    var actual = allEmploymentProficiencyEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task EmploymentProficienciesController_GetHedmAsync_CacheControlNotNull()
            {
                EmploymentProficienciesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                employmentProficiencyServiceMock.Setup(gc => gc.GetEmploymentProficienciesAsync(It.IsAny<bool>())).ReturnsAsync(EmploymentProficiencyList);

                var result = await EmploymentProficienciesController.GetEmploymentProficienciesAsync();
                Assert.AreEqual(result.Count(), allEmploymentProficiencyEntities.Count());

                int count = allEmploymentProficiencyEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = EmploymentProficiencyList[i];
                    var actual = allEmploymentProficiencyEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task EmploymentProficienciesController_GetHedmAsync_NoCache()
            {
                EmploymentProficienciesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmploymentProficienciesController.Request.Headers.CacheControl.NoCache = true;

                employmentProficiencyServiceMock.Setup(gc => gc.GetEmploymentProficienciesAsync(It.IsAny<bool>())).ReturnsAsync(EmploymentProficiencyList);

                var result = await EmploymentProficienciesController.GetEmploymentProficienciesAsync();
                Assert.AreEqual(result.Count(), allEmploymentProficiencyEntities.Count());

                int count = allEmploymentProficiencyEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = EmploymentProficiencyList[i];
                    var actual = allEmploymentProficiencyEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task EmploymentProficienciesController_GetHedmAsync_Cache()
            {
                EmploymentProficienciesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                EmploymentProficienciesController.Request.Headers.CacheControl.NoCache = false;

                employmentProficiencyServiceMock.Setup(gc => gc.GetEmploymentProficienciesAsync(It.IsAny<bool>())).ReturnsAsync(EmploymentProficiencyList);

                var result = await EmploymentProficienciesController.GetEmploymentProficienciesAsync();
                Assert.AreEqual(result.Count(), allEmploymentProficiencyEntities.Count());

                int count = allEmploymentProficiencyEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = EmploymentProficiencyList[i];
                    var actual = allEmploymentProficiencyEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Description);
                }
            }

            [TestMethod]
            public async Task EmploymentProficienciesController_GetByIdHedmAsync()
            {
                var thisEmploymentProficiency = EmploymentProficiencyList.Where(m => m.Id == "625c69ff-280b-4ed3-9474-662a43616a8a").FirstOrDefault();

                employmentProficiencyServiceMock.Setup(x => x.GetEmploymentProficiencyByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisEmploymentProficiency);

                var employmentProficiency = await EmploymentProficienciesController.GetEmploymentProficiencyByIdAsync("625c69ff-280b-4ed3-9474-662a43616a8a");
                Assert.AreEqual(thisEmploymentProficiency.Id, employmentProficiency.Id);
                Assert.AreEqual(thisEmploymentProficiency.Code, employmentProficiency.Code);
                Assert.AreEqual(thisEmploymentProficiency.Description, employmentProficiency.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentProficienciesController_GetThrowsIntAppiExc()
            {
                employmentProficiencyServiceMock.Setup(gc => gc.GetEmploymentProficienciesAsync(It.IsAny<bool>())).Throws<Exception>();

                await EmploymentProficienciesController.GetEmploymentProficienciesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentProficienciesController_GetByIdThrowsIntAppiExc()
            {
                employmentProficiencyServiceMock.Setup(gc => gc.GetEmploymentProficiencyByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await EmploymentProficienciesController.GetEmploymentProficiencyByIdAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentProficienciesController_PostThrowsIntAppiExc()
            {
                await EmploymentProficienciesController.PostEmploymentProficiencyAsync(EmploymentProficiencyList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentProficienciesController_PutThrowsIntAppiExc()
            {
                var result = await EmploymentProficienciesController.PutEmploymentProficiencyAsync(EmploymentProficiencyList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task EmploymentProficienciesController_DeleteThrowsIntAppiExc()
            {
                await EmploymentProficienciesController.DeleteEmploymentProficiencyAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a EmploymentProficiency domain entity to its corresponding EmploymentProficiency DTO
            /// </summary>
            /// <param name="source">EmploymentProficiency domain entity</param>
            /// <returns>EmploymentProficiency DTO</returns>
            private Ellucian.Colleague.Dtos.EmploymentProficiency ConvertEmploymentProficiencyEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.EmploymentProficiency source)
            {
                var employmentProficiency = new Ellucian.Colleague.Dtos.EmploymentProficiency();
                employmentProficiency.Id = source.Guid;
                employmentProficiency.Code = source.Code;
                employmentProficiency.Title = source.Description;
                employmentProficiency.Description = null;

                return employmentProficiency;
            }
        }
    }
}