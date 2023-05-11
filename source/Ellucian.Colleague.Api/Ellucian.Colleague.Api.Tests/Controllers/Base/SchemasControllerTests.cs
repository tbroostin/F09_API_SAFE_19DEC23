//// Copyright 2021 Ellucian Company L.P. and its affiliates.
// this controller is obselete as it is replaced by metadata endpoint. However
// we are leaving the code here as it has some important logic that might be needed in the future. 
// for generating schemas for self service APIs. 
//using Ellucian.Colleague.Api.Controllers;
//using Ellucian.Colleague.Configuration.Licensing;
//using Ellucian.Colleague.Dtos;
//using Ellucian.Web.Cache;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Web.Http;
//using System.Web.Http.Hosting;
//using System.Web.Http.Routing;
//using System.Web.Routing;
//using Ellucian.Colleague.Coordination.Base.Services;
//using slf4net;
//using System.Threading.Tasks;
//using Ellucian.Colleague.Domain.Base.Tests;
//using Ellucian.Colleague.Domain.Base.Entities;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json.Schema;

//namespace Ellucian.Colleague.Api.Tests.Controllers.Base
//{
//    [TestClass]
//    public class SchemasControllerTests
//    {
//        [TestClass]
//        public class SchemasControllerTests_GET
//        {
//            public TestContext TestContext { get; set; }
//            private const string HedtechIntegrationMediaTypeFormat = "application/vnd.hedtech.integration.v{0}+json";
//            private const string EEDM_WEBAPI_SCHEMAS_CACHE_KEY = "EEDM_WEBAPI_SCHEMAS_CACHE_KEY";

//            private SchemasController schemasController;
//            private HttpConfiguration configuration;
//            private RouteCollection httpRouteCollection = new RouteCollection();
//            List<ApiResources> resourcesDtoList = new List<ApiResources>();

//            private Mock<IEthosApiBuilderService> ethosApiBuilderServiceMock;

//            private Mock<ICacheProvider> cacheProviderMock;
//            private Mock<IBulkLoadRequestService> bulkLoadServiceMock;
//            private Mock<ILogger> loggerMock;
//            private Web.Http.EthosExtend.EthosExtensibleData ethosExtensibleData;
//            private TestConfigurationRepository testConfigurationRepository;
//            private Web.Http.EthosExtend.EthosApiConfiguration ethosApiConfiguration;
//            private List<EthosExtensibleDataRow> extendedDataList;
//            private Domain.Base.Entities.EthosExtensibleData ethosExtensibleDataDomain;


//            [TestInitialize]
//            public void Initialize()
//            {
//                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
//                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

//                BuildData();

//                schemasController = new SchemasController(bulkLoadServiceMock.Object, ethosApiBuilderServiceMock.Object,
//                    cacheProviderMock.Object, loggerMock.Object);
//                schemasController.Request = new HttpRequestMessage();
//                schemasController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, configuration);
//            }
//            [TestCleanup]
//            public void Cleanup()
//            {
//                schemasController = null;
//                TestContext = null;
//                configuration = null;
//                httpRouteCollection = null;
//                resourcesDtoList = null;
//                cacheProviderMock = null;
//                ethosApiConfiguration = null;
//                ethosExtensibleDataDomain = null;
//                extendedDataList = null;
//            }

//            [TestMethod]
//            [Obsolete]
//            public async Task Schemas_GetSchemas_InvalidResource()
//            {
//                ethosApiBuilderServiceMock.Setup(x => x.GetAllExtendedEthosConfigurations(It.IsAny<bool>(), It.IsAny<bool>()))
//                   .ReturnsAsync(new List<Domain.Base.Entities.EthosExtensibleData>() { ethosExtensibleDataDomain });

//                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_SCHEMAS_CACHE_KEY, null)).Returns(false);

//                schemasController.Request.Headers.CacheControl = new CacheControlHeaderValue
//                {
//                    NoCache = true,
//                    Public = false
//                };
//                var result = await schemasController.GetSchemas("x-invalid");

//                Assert.IsNotNull(result);
//                Assert.IsNull(result.FirstOrDefault());
//            }

//            [TestMethod]
//            [Obsolete]
//            public async Task Schemas_GetSchemas_MissingExtendedDataList()
//            {

//                ethosApiBuilderServiceMock.Setup(x => x.GetAllExtendedEthosConfigurations(It.IsAny<bool>(), It.IsAny<bool>()))
//                   .ReturnsAsync(new List<Domain.Base.Entities.EthosExtensibleData>() { ethosExtensibleDataDomain });

//                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_SCHEMAS_CACHE_KEY, null)).Returns(false);

//                schemasController.Request.Headers.CacheControl = new CacheControlHeaderValue
//                {
//                    NoCache = true,
//                    Public = false
//                };
//                var result = await schemasController.GetSchemas("x-person-health");

//                Assert.IsNotNull(result);
//                Assert.IsNull(result.FirstOrDefault());
//            }

//            [TestMethod]
//            [Obsolete]
//            public async Task Schemas_GetSchemas_Valid()
//            {
//                extendedDataList = new List<EthosExtensibleDataRow>()
//                {
//                    new EthosExtensibleDataRow("FIRST.NAME", "PERSON", "firstName", "", "string", "", 35),
//                    new EthosExtensibleDataRow("LAST.NAME", "PERSON", "lastName", "", "string", "", 35)
//               };
//                foreach (var item in extendedDataList)
//                    ethosExtensibleDataDomain.AddItemToExtendedData(item);

//                ethosApiBuilderServiceMock.Setup(x => x.GetAllExtendedEthosConfigurations(It.IsAny<bool>(), It.IsAny<bool>()))
//                   .ReturnsAsync(new List<Domain.Base.Entities.EthosExtensibleData>() { ethosExtensibleDataDomain });

//                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_SCHEMAS_CACHE_KEY, null)).Returns(false);

//                schemasController.Request.Headers.CacheControl = new CacheControlHeaderValue
//                {
//                    NoCache = true,
//                    Public = false
//                };
//                var result = await schemasController.GetSchemas("x-person-health");

//                var xPersonHealth = result.FirstOrDefault();
//                Assert.IsNotNull(xPersonHealth);

//                // Our implementation adds a 'required' block to the end of the generated schema
//                //  However...JsonSchema has a required property at the root level, which is defined as a boolean.   
//                // Attempting to Parse the schema will result in an error "can not map Array to Boolean"
//                // therefore, its necessary to remove the required block and validate 
//                // that seperately. 
//                var strPersonHealth = xPersonHealth.ToString();

//                strPersonHealth = strPersonHealth.Replace("\r\n", "");           
//                var indexOfLastCommma = strPersonHealth.LastIndexOf(",  \"required\":");
//                var indexOfLastBrace = strPersonHealth.LastIndexOf('}');

//                var sub = strPersonHealth.Substring(indexOfLastCommma, indexOfLastBrace - indexOfLastCommma);
//                var sub2 = string.Concat(strPersonHealth.Remove(indexOfLastCommma, indexOfLastBrace - indexOfLastCommma ), "}");

//                JsonSchema schema = JsonSchema.Parse(sub2);
//                JObject person = JObject.Parse(@"{
//                      'id': '91a98cb8-af02-4b3f-b7b7-014bf41a43ad',
//                      'firstName': 'Marcus',
//                      'lastName': 'Aurelius'
//                    }");

//                var validSchema = person.IsValid(schema);
//                Assert.IsTrue(validSchema);
//            }

//            [TestMethod]
//            [Obsolete]
//            public async Task Schemas_GetSchemas_Invalid()
//            {
//                extendedDataList = new List<EthosExtensibleDataRow>()
//                {
//                    new EthosExtensibleDataRow("FIRST.NAME", "PERSON", "firstName", "", "string", "", 35),
//                    new EthosExtensibleDataRow("LAST.NAME", "PERSON", "lastName", "", "string", "", 35)
//               };
//                foreach (var item in extendedDataList)
//                    ethosExtensibleDataDomain.AddItemToExtendedData(item);

//                ethosApiBuilderServiceMock.Setup(x => x.GetAllExtendedEthosConfigurations(It.IsAny<bool>(), It.IsAny<bool>()))
//                   .ReturnsAsync(new List<Domain.Base.Entities.EthosExtensibleData>() { ethosExtensibleDataDomain });

//                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_SCHEMAS_CACHE_KEY, null)).Returns(false);

//                schemasController.Request.Headers.CacheControl = new CacheControlHeaderValue
//                {
//                    NoCache = true,
//                    Public = false
//                };
//                var result = await schemasController.GetSchemas("x-person-health");

//                var xPersonHealth = result.FirstOrDefault();
//                Assert.IsNotNull(xPersonHealth);

//                var strPersonHealth = xPersonHealth.ToString();

//                strPersonHealth = strPersonHealth.Replace("\r\n", "");
//                var indexOfLastCommma = strPersonHealth.LastIndexOf(",  \"required\":");
//                var indexOfLastBrace = strPersonHealth.LastIndexOf('}');

//                var sub = strPersonHealth.Substring(indexOfLastCommma, indexOfLastBrace - indexOfLastCommma);
//                var sub2 = string.Concat(strPersonHealth.Remove(indexOfLastCommma, indexOfLastBrace - indexOfLastCommma), "}");

//                JsonSchema schema = JsonSchema.Parse(sub2);
//                JObject person = JObject.Parse(@"{
//                      'id': '91a98cb8-af02-4b3f-b7b7-014bf41a43ad',
//                      'firstName': 'Marcus',
//                      'lastName': 'Aurelius',
//                      'hobbies': ['.NET', 'Blogging', 'Reading', 'Xbox']
//                    }");

//                //We want this to fail since the schema does not allow additional properties
//                var validSchema = person.IsValid(schema);

//                Assert.IsFalse(validSchema);

//            }

//            [TestMethod]
//            [Obsolete]
//            public async Task Schemas_GetSchemas_InvalidPayload()
//            {
//                extendedDataList = new List<EthosExtensibleDataRow>()
//                {
//                    new EthosExtensibleDataRow("FIRST.NAME", "PERSON", "firstName", "", "string", "", 35),
//                     new EthosExtensibleDataRow("LAST.NAME", "PERSON", "lastName", "", "string", "", 35)

//                };
//                foreach (var item in extendedDataList)
//                    ethosExtensibleDataDomain.AddItemToExtendedData(item);

//                ethosApiBuilderServiceMock.Setup(x => x.GetAllExtendedEthosConfigurations(It.IsAny<bool>(), It.IsAny<bool>()))
//                   .ReturnsAsync(new List<Domain.Base.Entities.EthosExtensibleData>() { ethosExtensibleDataDomain });

//                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_SCHEMAS_CACHE_KEY, null)).Returns(false);

//                schemasController.Request.Headers.CacheControl = new CacheControlHeaderValue
//                {
//                    NoCache = true,
//                    Public = false
//                };
//                var result = await schemasController.GetSchemas("x-person-health");

//                var xPersonHealth = result.FirstOrDefault();
//                Assert.IsNotNull(xPersonHealth);
//                var strPersonHealth = xPersonHealth.ToString();

//                strPersonHealth = strPersonHealth.Replace("\r\n", "");
//                var indexOfLastCommma = strPersonHealth.LastIndexOf(",  \"required\":");
//                var indexOfLastBrace = strPersonHealth.LastIndexOf('}');

//                var sub = strPersonHealth.Substring(indexOfLastCommma, indexOfLastBrace - indexOfLastCommma);
//                var sub2 = string.Concat(strPersonHealth.Remove(indexOfLastCommma, indexOfLastBrace - indexOfLastCommma), "}");

//                JsonSchema schema = JsonSchema.Parse(sub2);

//                JObject person = JObject.Parse(@"{
//                      'firstName': null,
//                      'lastName': 0.123,
//                    }");
//                IList<string> messages = new List<string>();
//                Assert.IsFalse(person.IsValid(schema, out messages));

//                Assert.IsTrue(messages.Contains("Invalid type. Expected String but got Null. Line 2, position 39."));
//                Assert.IsTrue(messages.Contains("Invalid type. Expected String but got Float. Line 3, position 39."));

//            }


//            private void BuildData()
//            {
//                RouteConfig.RegisterRoutes(httpRouteCollection);
//                configuration = new HttpConfiguration();
//                int count = httpRouteCollection.Count;

//                for (int i = 0; i < count; i++)
//                {
//                    var p = httpRouteCollection[i].GetType().GetProperties();
//                    var x = p[0].GetValue(httpRouteCollection[i], null);
//                    if (x is IHttpRoute)
//                    {
//                        if (!(x as IHttpRoute).RouteTemplate.Equals("1.0/{controller}/{id}"))
//                        {
//                            configuration.Routes.Add(i.ToString(), x as IHttpRoute);
//                        }
//                    }
//                }

//                cacheProviderMock = new Mock<Ellucian.Web.Cache.ICacheProvider>();
//                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_SCHEMAS_CACHE_KEY, null)).Returns(true);
//                cacheProviderMock.Setup(x => x[EEDM_WEBAPI_SCHEMAS_CACHE_KEY]).Returns(resourcesDtoList);

//                loggerMock = new Mock<ILogger>();

//                bulkLoadServiceMock = new Mock<IBulkLoadRequestService>();
//                ethosApiBuilderServiceMock = new Mock<IEthosApiBuilderService>();
//                testConfigurationRepository = new TestConfigurationRepository();

//                var ethosExtensibleDataEntity = testConfigurationRepository.GetExtendedEthosDataByResource("x-person-health", "1.0.0", "141", new List<string>() { "1" }, true, false).GetAwaiter().GetResult().FirstOrDefault();
//                ethosExtensibleData = new Web.Http.EthosExtend.EthosExtensibleData()
//                {
//                    ApiResourceName = ethosExtensibleDataEntity.ApiResourceName,
//                    ApiVersionNumber = ethosExtensibleDataEntity.ApiVersionNumber,
//                    ColleagueTimeZone = ethosExtensibleDataEntity.ColleagueTimeZone,
//                    ResourceId = ethosExtensibleDataEntity.ResourceId,
//                    ExtendedSchemaType = ethosExtensibleDataEntity.ExtendedSchemaType
//                };

//                ethosExtensibleDataDomain = new Domain.Base.Entities.EthosExtensibleData(
//                        ethosExtensibleDataEntity.ApiResourceName,
//                        ethosExtensibleDataEntity.ApiVersionNumber,
//                        ethosExtensibleDataEntity.ExtendedSchemaType,
//                        ethosExtensibleDataEntity.ResourceId,
//                        ethosExtensibleDataEntity.ColleagueTimeZone.ToString(), extendedDataList)
//                {

//                    HttpMethodsSupported = new List<string> { "get" },
//                    DeprecationDate = DateTime.Today.AddDays(30),
//                    SunsetDate = DateTime.Today.AddDays(60),
//                    DeprecationNotice = "hello world"
//                };


//                ethosApiConfiguration = new Web.Http.EthosExtend.EthosApiConfiguration
//                {
//                    ResourceName = ethosExtensibleDataEntity.ApiResourceName,
//                    PrimaryGuidFileName = "PERSON",
//                    PrimaryGuidSource = "ID"
//                };


//                ethosApiBuilderServiceMock.Setup(x => x.GetEthosApiConfigurationByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<bool>()))
//                    .ReturnsAsync(ethosApiConfiguration);
//                ethosApiBuilderServiceMock.Setup(x => x.GetExtendedEthosConfigurationByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<bool>()))
//                    .ReturnsAsync(ethosExtensibleData);
//            }
//        }
//    }
//}