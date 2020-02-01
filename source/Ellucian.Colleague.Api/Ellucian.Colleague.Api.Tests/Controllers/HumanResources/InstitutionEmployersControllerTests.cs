// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Newtonsoft.Json;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class InstitutionEmployersControllerTests
    {
        [TestClass]
        public class InstitutionEmployersControllerGet
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

            private InstitutionEmployersController InstitutionEmployersController;
            private Mock<IInstitutionEmployersRepository> institutionEmployerRepositoryMock;
            private IInstitutionEmployersRepository institutionEmployerRepository;
            private IAdapterRegistry AdapterRegistry;
            private IEnumerable<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionEmployers> allInstitutionEmployersEntities;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IInstitutionEmployersService> institutionEmployersServiceMock;
            private IInstitutionEmployersService institutionEmployersService;
            List<Ellucian.Colleague.Dtos.InstitutionEmployers> InstitutionEmployersList;
            private string institutionEmployersGuid = "81fda6ce-77aa-4283-a878-75bbea227937";
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                institutionEmployerRepositoryMock = new Mock<IInstitutionEmployersRepository>();
                institutionEmployerRepository = institutionEmployerRepositoryMock.Object;

                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                AdapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionEmployers, Dtos.InstitutionEmployers>(AdapterRegistry, logger);
                AdapterRegistry.AddAdapter(testAdapter);

                institutionEmployersServiceMock = new Mock<IInstitutionEmployersService>();
                institutionEmployersService = institutionEmployersServiceMock.Object;

                allInstitutionEmployersEntities = new TestInstitutionEmployersRepository().GetInstitutionEmployersAsync(); 
                InstitutionEmployersList = new List<Dtos.InstitutionEmployers>();

                InstitutionEmployersController = new InstitutionEmployersController(institutionEmployersService, logger);
                InstitutionEmployersController.Request = new HttpRequestMessage();
                InstitutionEmployersController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

                foreach (var institutionEmployers in allInstitutionEmployersEntities)
                {
                    Dtos.InstitutionEmployers target = ConvertInstitutionEmployersEntityToDto(institutionEmployers);
                    InstitutionEmployersList.Add(target);
                }
                institutionEmployerRepositoryMock.Setup(x => x.GetInstitutionEmployersAsync(false)).ReturnsAsync(allInstitutionEmployersEntities);
            }

            [TestCleanup]
            public void Cleanup()
            {
                InstitutionEmployersController = null;
                institutionEmployerRepository = null;
            }

            [TestMethod]
            public async Task GetInstitutionEmployersByGuidAsync_Validate()
            {
                var thisInstitutionEmployers = InstitutionEmployersList.Where(m => m.Id == institutionEmployersGuid).FirstOrDefault();

                institutionEmployersServiceMock.Setup(x => x.GetInstitutionEmployersByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisInstitutionEmployers);

                var institutionEmployers = await InstitutionEmployersController.GetInstitutionEmployersByGuidAsync(institutionEmployersGuid);
                Assert.AreEqual(thisInstitutionEmployers.Id, institutionEmployers.Id);
                Assert.AreEqual(thisInstitutionEmployers.Code, institutionEmployers.Code);
                Assert.AreEqual(thisInstitutionEmployers.Title, institutionEmployers.Title);
            }

            [TestMethod]
            public async Task InstitutionEmployersController_GetInstitutionEmployersAsync()
            {
                InstitutionEmployersController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(true)).ReturnsAsync(InstitutionEmployersList);

                var result = await InstitutionEmployersController.GetInstitutionEmployersAsync(null);
                Assert.AreEqual(result.Count(), allInstitutionEmployersEntities.Count());

                int count = allInstitutionEmployersEntities.Count();
                for (int i = 0; i < count; i++)
                {
                    var expected = InstitutionEmployersList[i];
                    var actual = allInstitutionEmployersEntities.ToList()[i];

                    Assert.AreEqual(expected.Id, actual.Guid);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.PreferredName);
                }
            }

            [TestMethod]
            public async Task InstitutionEmployersController_GetInstitutionEmployersAsync_ValidateFields_Filter()
            {
                InstitutionEmployersController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(true)).ReturnsAsync(InstitutionEmployersList);
                var filterGroupName = "criteria";
                var filterRecord = InstitutionEmployersList.FirstOrDefault(i => i.Code.Equals("x"));
                InstitutionEmployersController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName), filterRecord);
                var sourceContexts = (await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter)).ToList();
                Assert.AreEqual(sourceContexts.Count, 1);
                var expected = filterRecord;
                var actual = sourceContexts[0];
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.Code, actual.Code);

            }

            [TestMethod]
            public async Task InstitutionEmployersController_GetInstitutionEmployersAsync_ValidateFields_Null_Filter()
            {
                InstitutionEmployersController.Request.Headers.CacheControl =
                    new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(true)).ReturnsAsync(InstitutionEmployersList);
                var filterGroupName = "criteria";
                InstitutionEmployersController.Request.Properties.Add(string.Format("FilterObject{0}", filterGroupName),
                      new Dtos.InstitutionEmployers() { Id = "" });
                //EmptyFilterProperties
                InstitutionEmployersController.Request.Properties.Add("EmptyFilterProperties", true);
                var sourceContexts = (await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter)).ToList();
                Assert.AreEqual(sourceContexts.Count, 0);
                

            }

            [TestMethod]
            public async Task InstitutionEmployersController_GetByIdHedmAsync()
            {
                var thisInstitutionEmployers = InstitutionEmployersList.Where(m => m.Id == "81fda6ce-77aa-4283-a878-75bbea227937").FirstOrDefault();

                institutionEmployersServiceMock.Setup(x => x.GetInstitutionEmployersByGuidAsync(It.IsAny<string>())).ReturnsAsync(thisInstitutionEmployers);

                var institutionEmployers = await InstitutionEmployersController.GetInstitutionEmployersByGuidAsync("81fda6ce-77aa-4283-a878-75bbea227937");
                Assert.AreEqual(thisInstitutionEmployers.Id, institutionEmployers.Id);
                Assert.AreEqual(thisInstitutionEmployers.Code, institutionEmployers.Code);
                Assert.AreEqual(thisInstitutionEmployers.Title, institutionEmployers.Title);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetThrowsIntAppiExc()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(false)).Throws<Exception>();

                await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetThrowsJsonSerializationException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(false)).Throws<JsonSerializationException>();

                await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetThrowsKeyNotFoundException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(false)).Throws<KeyNotFoundException>();

                await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetThrowsPermissionsException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(false)).Throws<PermissionsException>();

                await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetThrowsArgumentException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(false)).Throws<ArgumentException>();

                await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetThrowsRepositoryException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(false)).Throws<RepositoryException>();

                await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetThrowsIntegrationApiException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersAsync(false)).Throws<IntegrationApiException>();

                await InstitutionEmployersController.GetInstitutionEmployersAsync(criteriaFilter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetByIdThrowsIntAppiExc()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersByGuidAsync(It.IsAny<string>())).Throws<Exception>();

                await InstitutionEmployersController.GetInstitutionEmployersByGuidAsync("sdjfh");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetByIdThrowsKeyNotFoundException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();

                await InstitutionEmployersController.GetInstitutionEmployersByGuidAsync("sdjfh");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetByIdThrowsPermissionsException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();

                await InstitutionEmployersController.GetInstitutionEmployersByGuidAsync("sdjfh");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetByIdThrowsArgumentException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();

                await InstitutionEmployersController.GetInstitutionEmployersByGuidAsync("sdjfh");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetByIdThrowsRepositoryException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();

                await InstitutionEmployersController.GetInstitutionEmployersByGuidAsync("sdjfh");
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_GetByIdThrowsIntegrationApiException()
            {
                institutionEmployersServiceMock.Setup(gc => gc.GetInstitutionEmployersByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();

                await InstitutionEmployersController.GetInstitutionEmployersByGuidAsync("sdjfh");
            }
            
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_PostThrowsIntAppiExc()
            {
                await InstitutionEmployersController.PostInstitutionEmployersAsync(InstitutionEmployersList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_PutThrowsIntAppiExc()
            {
                var institutionalEmployersDto = new Dtos.InstitutionEmployers(); 
                var result = await InstitutionEmployersController.PutInstitutionEmployersAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023", InstitutionEmployersList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InstitutionEmployersController_DeleteThrowsIntAppiExc()
            {
                await InstitutionEmployersController.DeleteInstitutionEmployersAsync("9ae3a175-1dfd-4937-b97b-3c9ad596e023");
            }

            /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
            /// <summary>
            /// Converts a InstitutionEmployers domain entity to its corresponding InstitutionEmployers DTO
            /// </summary>
            /// <param name="source">InstitutionEmployers domain entity</param>
            /// <returns>InstitutionEmployers DTO</returns>
            private Ellucian.Colleague.Dtos.InstitutionEmployers ConvertInstitutionEmployersEntityToDto(Ellucian.Colleague.Domain.HumanResources.Entities.InstitutionEmployers source)
            {
                var institutionEmployers = new Ellucian.Colleague.Dtos.InstitutionEmployers();
                institutionEmployers.Id = source.Guid;
                institutionEmployers.Code = source.Code;
                institutionEmployers.Title = source.PreferredName;
                var address = new Dtos.DtoProperties.InstitutionEmployersAddress();
                address.AddressLines = source.AddressLines;
                address.City = source.City;
                address.State = source.State;
                address.Country = source.Country;
                address.PostalCode = source.PostalCode;
                if (address != null)
                {
                    institutionEmployers.Address = address;
                }
                institutionEmployers.PhoneNumber = source.PhoneNumber;

                return institutionEmployers;
            }
        }
    }
}
