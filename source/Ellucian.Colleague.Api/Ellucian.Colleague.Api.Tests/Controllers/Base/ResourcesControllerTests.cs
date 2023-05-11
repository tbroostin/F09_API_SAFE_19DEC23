// Copyright 2017-2021 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Routing;
using Ellucian.Colleague.Coordination.Base.Services;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Tests;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class ResourcesControllerTests
    {
        [TestClass]
        public class ResourcesControllerTests_GET
        {
            public TestContext TestContext { get; set; }
            private const string HedtechIntegrationMediaTypeFormat = "application/vnd.hedtech.integration.v{0}+json";
            private const string EEDM_WEBAPI_RESOURCES_CACHE_KEY = "EEDM_WEBAPI_RESOURCES_CACHE_KEY";
            private readonly List<string> versionedSupportedMethods = new List<string>() { "put", "post", "get" };
            private readonly List<string> versionlessSupportedMethods = new List<string>() {  "get", "delete" };
            private const string BulkRequestMediaType = "application/vnd.hedtech.integration.bulk-requests.v1.0.0+json";

            private ResourcesController resourcesController;
            private HttpConfiguration configuration;
            private RouteCollection httpRouteCollection = new RouteCollection();
            List<ApiResources> resourcesDtoList = new List<ApiResources>();

            private Mock<IEthosApiBuilderService> ethosApiBuilderServiceMock;

            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IBulkLoadRequestService> bulkLoadServiceMock;
            private Mock<ILogger> loggerMock;
            private Web.Http.EthosExtend.EthosExtensibleData ethosExtensibleData;
            private TestConfigurationRepository testConfigurationRepository;
            private List<Domain.Base.Entities.EthosExtensibleData> allEthosExtensibleData;
       


        [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                BuildData();

                resourcesController = new ResourcesController(bulkLoadServiceMock.Object, ethosApiBuilderServiceMock.Object,
                    cacheProviderMock.Object, loggerMock.Object);
                resourcesController.Request = new HttpRequestMessage();
                resourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, configuration);
            }
            [TestCleanup]
            public void Cleanup()
            {
                resourcesController = null;
                TestContext = null;
                configuration = null;
                httpRouteCollection = null;
                resourcesDtoList = null;
                cacheProviderMock = null;
            }

            [TestMethod]        
            public async Task Resources_GET()
            {
                resourcesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var result = await resourcesController.GetResources();
                var noName = result.Any(i => string.IsNullOrEmpty(i.Name));
                Assert.IsFalse(noName);
            }

            [TestMethod]         
            public async Task  Resources_From_Cache_GET()
            {
                resourcesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var result = await resourcesController.GetResources();
                var noName = result.Any(i => string.IsNullOrEmpty(i.Name));
                Assert.IsFalse(noName);
            }

            [TestMethod]
            
            public void Resources_GET_filter1()
            {
                var T = typeof(TestDto); 
                var retval = resourcesController.IterateProperties("criteria", T);              
                Assert.IsNotNull(retval);

                var x = retval.ToList();
                Assert.AreEqual("title", x[0]);
                Assert.AreEqual("startOn", x[1]);

            }

          
            [TestMethod]
            public async Task Resources_From_Cache_GET_ethosExtensibleData()
            {
                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_RESOURCES_CACHE_KEY, null)).Returns(false);

                resourcesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = false
                };
                var result = await resourcesController.GetResources();

                var ethosExtensibleData = allEthosExtensibleData
                        .FirstOrDefault(x => x.ApiResourceName.Equals("x-person-health"));

                 var xPersonHealth = result.FirstOrDefault(x => x.Name.Equals("x-person-health"));
                Assert.IsNotNull(xPersonHealth);
                Assert.AreEqual(xPersonHealth.Representations.Count, 3);
                var versionless = xPersonHealth.Representations.FirstOrDefault(y => y.XMediaType.Equals("application/json"));
                Assert.IsNotNull(versionless);
                CollectionAssert.AreEquivalent(versionless.Methods, versionlessSupportedMethods);
                Assert.AreEqual(versionless.VersionNumber, ethosExtensibleData.ApiVersionNumber);
                var versioned = xPersonHealth.Representations.FirstOrDefault(z => z.XMediaType.Equals("application/vnd.hedtech.integration.v1.0.0+json"));
                Assert.IsNotNull(versioned);
                CollectionAssert.AreEquivalent(versioned.Methods, versionedSupportedMethods);
                Assert.AreEqual(versioned.VersionNumber, ethosExtensibleData.ApiVersionNumber);

                Assert.IsNotNull(versioned.DeprecationNotice);
                Assert.AreEqual(versioned.DeprecationNotice.DeprecatedOn, ethosExtensibleData.DeprecationDate);
                Assert.AreEqual(versioned.DeprecationNotice.SunsetOn, ethosExtensibleData.SunsetDate);
                Assert.AreEqual(versioned.DeprecationNotice.Description, ethosExtensibleData.DeprecationNotice);
                var isCustom = xPersonHealth.Representations.FirstOrDefault().Customizations.IsCustomResource;
                Assert.IsTrue(isCustom);

            }

            [TestMethod]
            public async Task Resources_From_Cache_GET_Versions()
            {
                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_RESOURCES_CACHE_KEY, null)).Returns(false);

                resourcesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = false
                };
                var result = await resourcesController.GetResources();

                var ethosExtensibleData = allEthosExtensibleData
                        .FirstOrDefault(x => x.ApiResourceName.Equals("x-person-health"));

                var xPersonHealth = result.FirstOrDefault(x => x.Name.Equals("x-person-health"));
                Assert.IsNotNull(xPersonHealth);
                Assert.AreEqual(xPersonHealth.Representations.Count, 3);
                var versionless = xPersonHealth.Representations.FirstOrDefault(y => y.XMediaType.Equals("application/json"));
                Assert.IsNotNull(versionless);

                var versioned = xPersonHealth.Representations.FirstOrDefault(z => z.XMediaType.Equals("application/vnd.hedtech.integration.v1.0.0+json"));
                Assert.IsNotNull(versioned);
                Assert.AreEqual(versioned.VersionNumber, ethosExtensibleData.ApiVersionNumber);

                var wholeNumberVersion = xPersonHealth.Representations.FirstOrDefault(z => z.XMediaType.Equals("application/vnd.hedtech.integration.v1+json"));
                Assert.IsNotNull(wholeNumberVersion);

                var isCustom = xPersonHealth.Representations.FirstOrDefault().Customizations.IsCustomResource;
                Assert.IsTrue(isCustom);
            }

            /*
            [TestMethod]
            public async Task Resources_From_Cache_GET_BulkSupportedRoutes()
            {
                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_RESOURCES_CACHE_KEY, null)).Returns(false);

                bulkLoadServiceMock.Setup(x => x.IsBulkLoadSupported()).Returns(true);

                resourcesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = false
                };
                var result = await resourcesController.GetResources();

                Assert.IsNotNull(result);
                var bulkRequestMediaType = result.FirstOrDefault
                    (x => x.Representations.Any
                        (y => y.XMediaType.Equals(BulkRequestMediaType)));
                Assert.IsNotNull(bulkRequestMediaType);
            }
            */

            private void BuildData()
            {
                RouteConfig.RegisterRoutes(httpRouteCollection);
                configuration = new HttpConfiguration();
                int count = httpRouteCollection.Count;

                for (int i = 0; i < count; i++)
                {
                    var p = httpRouteCollection[i].GetType().GetProperties();
                    var x = p[0].GetValue(httpRouteCollection[i], null);
                    if (x is IHttpRoute)
                    {
                        if (!(x as IHttpRoute).RouteTemplate.Equals("1.0/{controller}/{id}"))
                        {
                            configuration.Routes.Add(i.ToString(), x as IHttpRoute);
                        }
                    }
                }

                resourcesDtoList = new List<ApiResources>()
                {
                    new ApiResources()
                    {
                        Name = "academic-catalogs",
                        Representations = new List<Representation>()
                        {
                            new Representation()
                            {
                                XMediaType = string.Format(HedtechIntegrationMediaTypeFormat, 6),
                                Methods = new List<string>() { "get", "put", "post" }
                            }
                        }
                    }
                };
                cacheProviderMock = new Mock<Ellucian.Web.Cache.ICacheProvider>();
                cacheProviderMock.Setup(x => x.Contains(EEDM_WEBAPI_RESOURCES_CACHE_KEY, null)).Returns(true);
                cacheProviderMock.Setup(x => x[EEDM_WEBAPI_RESOURCES_CACHE_KEY]).Returns(resourcesDtoList);

                loggerMock = new Mock<ILogger>();
                
                bulkLoadServiceMock = new Mock<IBulkLoadRequestService>();
                ethosApiBuilderServiceMock = new Mock<IEthosApiBuilderService>();
                 testConfigurationRepository = new TestConfigurationRepository();

                Dictionary<string, Dictionary<string, string>> allColumnData = null;
                var ethosExtensibleDataEntity = testConfigurationRepository.GetExtendedEthosDataByResource("x-person-health", "1.0.0", "application/vnd.hedtech.integration.v1.0.0+json", new List<string>() { "1" }, allColumnData, true, false).GetAwaiter().GetResult().FirstOrDefault();
                ethosExtensibleData = new Web.Http.EthosExtend.EthosExtensibleData()
                {
                    ApiResourceName = ethosExtensibleDataEntity.ApiResourceName,
                    ApiVersionNumber = ethosExtensibleDataEntity.ApiVersionNumber,
                    ColleagueTimeZone = ethosExtensibleDataEntity.ColleagueTimeZone,
                    ResourceId = ethosExtensibleDataEntity.ResourceId,
                    ExtendedSchemaType = ethosExtensibleDataEntity.ExtendedSchemaType
                };
                var ethosExtensibleDataDomain = new Domain.Base.Entities.EthosExtensibleData(
                        ethosExtensibleDataEntity.ApiResourceName,
                        ethosExtensibleDataEntity.ApiVersionNumber,
                        ethosExtensibleDataEntity.ExtendedSchemaType,
                        ethosExtensibleDataEntity.ResourceId,
                        ethosExtensibleDataEntity.ColleagueTimeZone.ToString(), null)
                {
                    HttpMethodsSupported = new List<string> { "get", "put", "post", "delete" },
                    DeprecationDate = DateTime.Today.AddDays(30),
                    SunsetDate = DateTime.Today.AddDays(60),
                    DeprecationNotice = "hello world",
                    IsCustomResource = true
                };
                
                allEthosExtensibleData
                    = new List<Domain.Base.Entities.EthosExtensibleData>() { ethosExtensibleDataDomain };
                ethosApiBuilderServiceMock.Setup(x => x.GetExtendedEthosConfigurationByResource(It.IsAny<Web.Http.EthosExtend.EthosResourceRouteInfo>(), It.IsAny<bool>())).ReturnsAsync(ethosExtensibleData);
                ethosApiBuilderServiceMock.Setup(x => x.GetAllExtendedEthosConfigurations(It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(allEthosExtensibleData);
            }
        }
    }

    [System.Runtime.Serialization.DataContract]
    public class TestDto
    {
        [FilterProperty("criteria")]
        [DataMember(Name = "title")]
        public string Title { get; set; }

        [FilterProperty("criteria")]
        [DataMember(Name = "startOn")]
        public DateTimeOffset? StartOn { get; set; }

        public DateTime? EndOn { get; set; }

        [FilterProperty("criteria")]
        public TestGuidObject Course { get; set; }

        [FilterProperty("criteria")]
        public TestDtoProperty TestDtoProperty { get; set; }
    
        public TestDto() : base()
        {       
        }
    }

    /// <summary>
    /// A GUID container
    /// </summary>
    [System.Runtime.Serialization.DataContract]
    public class TestGuidObject
    {
        public string Id { get; set; }

        public TestGuidObject()
        {
        }

        public TestGuidObject(string id)
        {
            Id = id;
        }
    }

    [System.Runtime.Serialization.DataContract]
    public class TestDtoProperty
    {
        public TestEnum? TestEnum { get; set; }

        [FilterProperty("criteria")]
        public string Name1 { get; set; }

        public string Name2 { get; set; }

        public TestDtoProperty() { }
    }

  
    public enum TestEnum
    {
        NotSet,
        Closed,
        Open,
        Pending,
        Cancelled
    }
}
