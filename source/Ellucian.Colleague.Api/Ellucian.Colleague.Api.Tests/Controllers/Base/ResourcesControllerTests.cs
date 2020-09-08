// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.Attributes;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Routes;
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

            private ResourcesController resourcesController;
            private HttpConfiguration configuration;
            private RouteCollection httpRouteCollection = new RouteCollection();
            List<ApiResources> resourcesDtoList = new List<ApiResources>();
            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IBulkLoadRequestService> bulkLoadServiceMock;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                BuildData();

                resourcesController = new ResourcesController(bulkLoadServiceMock.Object, cacheProviderMock.Object, loggerMock.Object);
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
            public void Resources_GET()
            {
                resourcesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                var result = resourcesController.GetResources();
                var noName = result.Any(i => string.IsNullOrEmpty(i.Name));
                Assert.IsFalse(noName);
            }

            [TestMethod]
            public void Resources_From_Cache_GET()
            {
                resourcesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                var result = resourcesController.GetResources();
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
