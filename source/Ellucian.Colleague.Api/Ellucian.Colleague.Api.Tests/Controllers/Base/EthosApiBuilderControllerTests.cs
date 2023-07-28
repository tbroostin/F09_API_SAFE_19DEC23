// Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class EthosApiBuilderControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEthosApiBuilderService> ethosApiBuilderServiceMock;
        private Mock<ILogger> loggerMock;

        private EthosApiBuilderController ethosApiBuilderController;

        private List<Dtos.EthosApiBuilder> ethosApiBuilderCollection;
        private Web.Http.EthosExtend.EthosApiConfiguration ethosApiConfiguration;
        private Web.Http.EthosExtend.EthosExtensibleData ethosExtensibleData;
        private Web.Http.EthosExtend.EthosResourceRouteInfo routeInfo;

        private const string ethosApiBuilderSubjectAreaGuid = "a830e686-7692-4012-8da5-b1b5d44389b4"; //BU

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            ethosApiBuilderServiceMock = new Mock<IEthosApiBuilderService>();
            loggerMock = new Mock<ILogger>();

            ethosApiBuilderCollection = new List<Dtos.EthosApiBuilder>();
            var testConfigurationRepository = new TestConfigurationRepository();

            var ethosApiConfigurationEntity = testConfigurationRepository.GetEthosApiConfigurationByResource("x-person-health", false).GetAwaiter().GetResult();

            ethosApiConfiguration = new Web.Http.EthosExtend.EthosApiConfiguration()
            {
                ResourceName = ethosApiConfigurationEntity.ResourceName,
                PrimaryEntity = ethosApiConfigurationEntity.PrimaryEntity,
                PrimaryApplication = ethosApiConfigurationEntity.PrimaryApplication,
                PrimaryTableName = ethosApiConfigurationEntity.PrimaryTableName,
                PrimaryGuidSource = ethosApiConfigurationEntity.PrimaryGuidSource,
                PrimaryGuidDbType = ethosApiConfigurationEntity.PrimaryGuidDbType,
                PrimaryGuidFileName = ethosApiConfigurationEntity.PrimaryGuidFileName,
                PageLimit = ethosApiConfigurationEntity.PageLimit,
                SelectFileName = ethosApiConfigurationEntity.SelectFileName,
                SelectSubroutineName = ethosApiConfigurationEntity.SelectSubroutineName,
                SavingField = ethosApiConfigurationEntity.SavingField,
                SavingOption = ethosApiConfigurationEntity.SavingOption,
                SelectColumnName = ethosApiConfigurationEntity.SelectColumnName,
                SelectRules = ethosApiConfigurationEntity.SelectRules,
                SelectParagraph = ethosApiConfigurationEntity.SelectParagraph,
                HttpMethods = new List<Web.Http.EthosExtend.EthosApiSupportedMethods>(),
                SelectionCriteria = new List<Web.Http.EthosExtend.EthosApiSelectCriteria>(),
                SortColumns = new List<Web.Http.EthosExtend.EthosApiSortCriteria>()
            };
            foreach (var method in ethosApiConfigurationEntity.HttpMethods)
            {
                ethosApiConfiguration.HttpMethods.Add(new Web.Http.EthosExtend.EthosApiSupportedMethods(method.Method, method.Permission, method.Description, method.Summary));
            }
            foreach (var select in ethosApiConfigurationEntity.SelectionCriteria)
            {
                ethosApiConfiguration.SelectionCriteria.Add(
                    new Web.Http.EthosExtend.EthosApiSelectCriteria(select.SelectConnector,
                    select.SelectColumn,
                    select.SelectOper,
                    select.SelectValue)
                );
            }
            foreach (var sort in ethosApiConfigurationEntity.SortColumns)
            {
                ethosApiConfiguration.SortColumns.Add(new Web.Http.EthosExtend.EthosApiSortCriteria(sort.SortColumn, sort.SortSequence));
            }

            Dictionary<string, Dictionary<string, string>> allColumnData = null;
            var ethosExtensibleDataEntity = testConfigurationRepository.GetExtendedEthosDataByResource("x-person-health", "1.0.0", "141", new List<string>() { "1" }, allColumnData, true, false).GetAwaiter().GetResult().FirstOrDefault();
            ethosExtensibleData = new Web.Http.EthosExtend.EthosExtensibleData()
            {
                ApiResourceName = ethosExtensibleDataEntity.ApiResourceName,
                ApiVersionNumber = ethosExtensibleDataEntity.ApiVersionNumber,
                ColleagueTimeZone = ethosExtensibleDataEntity.ColleagueTimeZone,
                ResourceId = ethosExtensibleDataEntity.ResourceId,
                ExtendedSchemaType = ethosExtensibleDataEntity.ExtendedSchemaType,
                ExtendedDataList = new List<Web.Http.EthosExtend.EthosExtensibleDataRow>()
                {
                    new Web.Http.EthosExtend.EthosExtensibleDataRow()
                    {
                        ColleagueColumnName = "PHL.IMMUNIZATIONS",
                        JsonPath = "/",
                        JsonPropertyType = Web.Http.EthosExtend.JsonPropertyTypeExtensions.String,
                        FullJsonPath = "/immunizations",
                        ColleagueFileName = "PERSON.HEALTH",
                        ExtendedDataValue = "POL"
                    }
                }
            };

            routeInfo = new Web.Http.EthosExtend.EthosResourceRouteInfo()
            {
                ResourceVersionNumber = "1.0.0",
                BypassCache = true,
                RequestMethod = new HttpMethod("GET"),
                ResourceName = "x-person-health"
            };

            var ethosApiBuilder = new Dtos.EthosApiBuilder
            {
                _Id = ethosApiBuilderSubjectAreaGuid
            };
            ethosApiBuilderCollection.Add(ethosApiBuilder);

            var expected = ethosApiBuilderCollection.FirstOrDefault();
            var expectedObject = new JObject();
            expectedObject.Add("Id", expected._Id);
            expectedObject.Add("immunizations", "POL");

            ethosApiBuilderController = new EthosApiBuilderController(ethosApiBuilderServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            ethosApiBuilderController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            ethosApiBuilderController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost/x-person-health") };
            ethosApiBuilderController.Request.SetRequestContext(new System.Web.Http.Controllers.HttpRequestContext() { RouteData = new HttpRouteData(new HttpRoute("x-person-health")) });
            ethosApiBuilderController.Request.Properties.Add("PartialInputJsonObject", expectedObject);
            ethosApiBuilderController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false, Public = true };
            ethosApiBuilderController.Request.Headers.Add("Accept", "application/vnd.hedtech.integration.v1+json");
            ethosApiBuilderController.Request.Properties.Add("EthosExtendedDataObject", expectedObject);
            ethosApiBuilderController.Request.Method = new HttpMethod("GET");

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiConfigurationByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<bool>())).ReturnsAsync(ethosApiConfiguration);
            ethosApiBuilderServiceMock.Setup(x => x.GetExtendedEthosConfigurationByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<bool>())).ReturnsAsync(ethosExtensibleData);
            ethosApiBuilderServiceMock.Setup(x => x.GetExtendedEthosDataByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(new List<Web.Http.EthosExtend.EthosExtensibleData>() { ethosExtensibleData });
        }

        [TestCleanup]
        public void Cleanup()
        {
            ethosApiBuilderController = null;
            ethosApiBuilderCollection = null;
            loggerMock = null;
            ethosApiBuilderServiceMock = null;
            ethosApiConfiguration = null;
            ethosExtensibleData = null;
            routeInfo = null;
        }

        #region EthosApiBuilder

        [TestMethod]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_GetAll()
        {
            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            var expectedCollection = new Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>(ethosApiBuilderCollection, Limit);
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).ReturnsAsync(expectedCollection);

            Paging paging = new Paging(Limit, offset);
            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();

            var dto = new Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>(ethosApiBuilderCollection, ethosApiBuilderCollection.Count());
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).ReturnsAsync(dto);

            var cancelToken = new System.Threading.CancellationToken(false);
            var response = await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "", paging);
            var pagedResponse = response as PagedHttpActionResult<IEnumerable<Dtos.EthosApiBuilder>>;
            
            HttpResponseMessage httpResponseMessage = await pagedResponse.ExecuteAsync(cancelToken);
            List<Dtos.EthosApiBuilder> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.EthosApiBuilder>>)httpResponseMessage.Content)
                                                                        .Value as List<Dtos.EthosApiBuilder>;

            Assert.IsNotNull(results);
            Assert.AreEqual(Limit, results.Count());

            foreach (var actual in results)
            {
                var expected = ethosApiBuilderCollection.FirstOrDefault(i => i._Id.Equals(actual._Id));
                Assert.AreEqual(expected._Id, actual._Id);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_InvalidVersion()
        {
            ethosApiBuilderController.Request.Headers.Remove("Accept");

            ethosApiBuilderController.Request.Headers.Add("Accept", "application/vnd.hedtech.integration.vX+json");

            ethosApiBuilderController.RequestContext.RouteData = new HttpRouteData(new HttpRoute("{resource}"));
           
            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            var expectedCollection = new Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>(ethosApiBuilderCollection, Limit);
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).ReturnsAsync(expectedCollection);
            
            Paging paging = new Paging(Limit, offset);
            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "", paging);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_String_ArgumentNullException()
        {
            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "", It.IsAny<Paging>());
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_PermissionsException()
        {
            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<PermissionsException>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();

            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "", It.IsAny<Paging>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_ArgumentException()
        {
            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<ArgumentException>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();

            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "", It.IsAny<Paging>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_RepositoryException()
        {

            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<RepositoryException>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();

            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "", It.IsAny<Paging>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_IntegrationApiException()
        {

            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<IntegrationApiException>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();

            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "", It.IsAny<Paging>()));
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilder_Exception()
        {

            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).Throws<Exception>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();

            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "", It.IsAny<Paging>()));
        }

        #endregion GetEthosApiBuilder

        #region GetEthosApiBuilderByGuid

        [TestMethod]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).ReturnsAsync(expected);

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();

            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "1", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder result = (Dtos.EthosApiBuilder)response;

            Assert.AreEqual(expected._Id, result._Id, "Id");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_NullArgument()
        {
            ethosApiBuilderController.Request.SetRequestContext(new System.Web.Http.Controllers.HttpRequestContext() { RouteData = new HttpRouteData(new HttpRoute("")) });
            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, null, "1", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder result = (Dtos.EthosApiBuilder)response;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_EmptyArgument()
        {
            ethosApiBuilderController.Request.SetRequestContext(new System.Web.Http.Controllers.HttpRequestContext() { RouteData = new HttpRouteData(new HttpRoute("")) });
            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "", "1", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder result = (Dtos.EthosApiBuilder)response;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_PermissionsException()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<PermissionsException>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "1", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder result = (Dtos.EthosApiBuilder)response;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_ArgumentException()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<ArgumentException>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "1", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder result = (Dtos.EthosApiBuilder)response;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_RepositoryException()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<RepositoryException>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "1", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder result = (Dtos.EthosApiBuilder)response;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_IntegrationApiException()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<IntegrationApiException>();
            
            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "1", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder result = (Dtos.EthosApiBuilder)response;
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetEthosApiBuilderByGuid_Exception()
        {
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).Throws<Exception>();

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(ethosApiBuilderDto, "x-person-health", "1", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder result = (Dtos.EthosApiBuilder)response;
        }

        #endregion GetEthosApiBuilderByGuid

        #region GetAlternativeRouteOrNotAcceptable

        [TestMethod]
        public async Task EthosApiBuilderController_GetAlternativeRouteOrNotAcceptable_GetAll()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("controller", "EthosApiBuilder");
            routeValueDict.Add("action", "GetAlternativeRouteOrNotAcceptable");
            HttpRoute route = new HttpRoute("x-person-health/{guid}", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);
            
            ethosApiBuilderController.Request.SetRouteData(data);

            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            var filterDictionary = new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>();

            var expectedCollection = new Tuple<IEnumerable<Dtos.EthosApiBuilder>, int>(ethosApiBuilderCollection, Limit);
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderAsync(offset, Limit, "x-person-health", filterDictionary, It.IsAny<bool>())).ReturnsAsync(expectedCollection);

            Paging paging = new Paging(Limit, offset);

            var dto = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(It.IsAny<string>(), "x-person-health")).ReturnsAsync(dto);

            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(dto, "x-person-health", dto._Id, paging));

            Dtos.EthosApiBuilder actual = (Dtos.EthosApiBuilder)response;

            Assert.IsNotNull(response);
            Assert.AreEqual(actual._Id, dto._Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_GetAlternativeRouteOrNotAcceptable_Exception()
        {
            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("controller", "EthosApiBuilder");
            routeValueDict.Add("action", "GetAlternativeRouteOrNotAcceptable");
            HttpRoute route = new HttpRoute("x-person-health/{guid}", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);

            ethosApiBuilderController.Request.SetRouteData(data);

            int offset = 0;
            int Limit = ethosApiBuilderCollection.Count();

            Paging paging = new Paging(Limit, offset);

            var dto = ethosApiBuilderCollection.FirstOrDefault();

            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiConfigurationByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<bool>())).ReturnsAsync(new Web.Http.EthosExtend.EthosApiConfiguration());

            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(dto, "x-person-health", dto._Id, paging);
        }


        #endregion

        #region Put

        [TestMethod]
        public async Task EthosApiBuilderController_PutEthosApiBuilder()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("PUT");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.PutEthosApiBuilderAsync(expected._Id, It.IsAny<Dtos.EthosApiBuilder>(), It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).ReturnsAsync(expected);
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(expected._Id, ethosApiConfiguration.ResourceName)).ReturnsAsync(expected);

            var ethosApiBuilderDto = new Dtos.EthosApiBuilder();
            var cancelToken = new System.Threading.CancellationToken(false);
            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", expected._Id, It.IsAny<Paging>()));
            Dtos.EthosApiBuilder actual = (Dtos.EthosApiBuilder)response;

            Assert.AreEqual(expected._Id, actual._Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PutEthosApiBuilder_NullId()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("PUT");

            var expected = ethosApiBuilderCollection.FirstOrDefault();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", It.IsAny<string>(), It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PutEthosApiBuilder_NullObject()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("PUT");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(It.IsAny<Dtos.EthosApiBuilder>(), "x-person-health", expected._Id, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PutEthosApiBuilder_EmptyIdProperty()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("PUT");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(new Dtos.EthosApiBuilder() { _Id = "" }, "x-person-health", expected._Id, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PutEthosApiBuilder_PermissionsException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("PUT");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.PutEthosApiBuilderAsync(expected._Id, expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<PermissionsException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", expected._Id, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PutEthosApiBuilder_ArgumentException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("PUT");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.PutEthosApiBuilderAsync(expected._Id, expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<ArgumentException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", expected._Id, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PutEthosApiBuilder_RepositoryException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("PUT");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.PutEthosApiBuilderAsync(expected._Id, expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<RepositoryException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", expected._Id, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PutEthosApiBuilder_IntegrationApiException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("PUT");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.PutEthosApiBuilderAsync(expected._Id, expected, It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<IntegrationApiException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", expected._Id, It.IsAny<Paging>());
        }
        #endregion

        #region Post

        [TestMethod]
        public async Task EthosApiBuilderController_PostEthosApiBuilder()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("POST");

            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = Guid.Empty.ToString();
            ethosApiBuilderServiceMock.Setup(x => x.PostEthosApiBuilderAsync(expected, It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).ReturnsAsync(expected);
            ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiBuilderByIdAsync(expected._Id, ethosApiConfiguration.ResourceName)).ReturnsAsync(expected);

            var response = (await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", "", It.IsAny<Paging>()));
            Dtos.EthosApiBuilder actual = (Dtos.EthosApiBuilder)response;
            Assert.AreEqual(expected._Id, actual._Id, "Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PostEthosApiBuilder_NullArgument()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("POST");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = Guid.Empty.ToString();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(null, "x-person-health", "", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PostEthosApiBuilder_PermissionsException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("POST");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = Guid.Empty.ToString();
            ethosApiBuilderServiceMock.Setup(x => x.PostEthosApiBuilderAsync(expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<PermissionsException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", "", It.IsAny<Paging>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PostEthosApiBuilder_ArgumentException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("POST");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = Guid.Empty.ToString();
            ethosApiBuilderServiceMock.Setup(x => x.PostEthosApiBuilderAsync(expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<ArgumentException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", "", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PostEthosApiBuilder_RepositoryException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("POST");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = Guid.Empty.ToString();
            ethosApiBuilderServiceMock.Setup(x => x.PostEthosApiBuilderAsync(expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<RepositoryException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", "", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PostEthosApiBuilder_IntegrationApiException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("POST");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = Guid.Empty.ToString();
            ethosApiBuilderServiceMock.Setup(x => x.PostEthosApiBuilderAsync(expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<IntegrationApiException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", "", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PostEthosApiBuilder_ConfigurationException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("POST");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = Guid.Empty.ToString();
            ethosApiBuilderServiceMock.Setup(x => x.PostEthosApiBuilderAsync(expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<ConfigurationException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", "", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_PostEthosApiBuilder_Exception()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("POST");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = Guid.Empty.ToString();
            ethosApiBuilderServiceMock.Setup(x => x.PostEthosApiBuilderAsync(expected, routeInfo, new Dictionary<string, Web.Http.EthosExtend.EthosExtensibleDataFilter>())).Throws<Exception>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(expected, "x-person-health", "", It.IsAny<Paging>());
        }
        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_DeleteEthosApiBuilder_EmptyArgument()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("DELETE");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = "";
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(null, "x-person-health", "", It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_DeleteEthosApiBuilder_NullArgument()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("DELETE");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            expected._Id = null;
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(null, "x-person-health", null, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_DeleteEthosApiBuilder_PermissionsException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("DELETE");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.DeleteEthosApiBuilderAsync(expected._Id, ethosApiConfiguration.ResourceName)).Throws<PermissionsException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(null, "x-person-health", expected._Id, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_DeleteEthosApiBuilder_ArgumentException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("DELETE");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.DeleteEthosApiBuilderAsync(expected._Id, ethosApiConfiguration.ResourceName)).Throws<ArgumentException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(null, "x-person-health", expected._Id, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_DeleteEthosApiBuilder_RepositoryException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("DELETE");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.DeleteEthosApiBuilderAsync(expected._Id, ethosApiConfiguration.ResourceName)).Throws<RepositoryException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(null, "x-person-health", expected._Id, It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EthosApiBuilderController_DeleteEthosApiBuilder_IntegrationApiException()
        {
            ethosApiBuilderController.Request.Method = new HttpMethod("DELETE");
            var expected = ethosApiBuilderCollection.FirstOrDefault();
            ethosApiBuilderServiceMock.Setup(x => x.DeleteEthosApiBuilderAsync(expected._Id, ethosApiConfiguration.ResourceName)).Throws<IntegrationApiException>();
            await ethosApiBuilderController.GetAlternativeRouteOrNotAcceptable(null, "x-person-health", expected._Id, It.IsAny<Paging>());
        }
        #endregion
    }
}