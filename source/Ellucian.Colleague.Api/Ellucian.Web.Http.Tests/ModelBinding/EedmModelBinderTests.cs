// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Http.Filters;
using Ellucian.Web.Http.ModelBinding;
using Ellucian.Web.Infrastructure.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Routing;

namespace Ellucian.Web.Http.Tests.ModelBinding
{
    [TestClass]
    public class EedmModelBinderTests
    {        
        private Mock<ILogger> _loggerMock;
        private ILogger _logger;
        Mock<System.Web.Http.Metadata.ModelMetadataProvider> _modelMetadataProviderMock;
        Mock<IHttpRouteData> _httpRouteDataMock;

        ModelBinderTestData _testData;
        EedmModelBinder eedmModelBinder;
        MockPersonsController controller;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = MockLogger.Instance;
            _logger = _loggerMock.Object;
            _modelMetadataProviderMock = new Mock<System.Web.Http.Metadata.ModelMetadataProvider>();
            _httpRouteDataMock = new Mock<IHttpRouteData>();
            controller = new MockPersonsController(_logger);
        }

        [TestCleanup]
        public void Clean()
        {
            _loggerMock = null;
            _logger = null;
            _modelMetadataProviderMock = null;
            _httpRouteDataMock = null;
            _testData = null;
            eedmModelBinder = null;
            controller = null;
        }

        private HttpRequestMessage CreateRequest(string url, HttpMethod method)
        {
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = method;            
            return request;
        }

        private HttpActionContext CreateHttpActionContext(string jsonRequestString, HttpMethod method, object obj)
        {
            HttpRequestMessage request = CreateRequest(jsonRequestString, method);
            HttpActionContext actionContext = new HttpActionContext() { ControllerContext = new HttpControllerContext() { Request = request } };
            return actionContext;
        }

        [TestMethod]
        public void BindModel_Test_Empty_BodyString_False_Result()
        {
            //Arrange
            string urlGuid = "1234";
            string bodyGuid = urlGuid;
            string bodyString = string.Empty;
            string requestURI = "http://localhost/Ellucian.Colleague.Api/MockPersons/{0}";
            string jsonQueryString = String.Format(requestURI, urlGuid);

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("MockPersons/{guid}", "PutHousingAssignmentAsync");
            HttpRoute route = new HttpRoute("MockPersons/{guid}", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.Add("MockPersons/{guid}", route);

            _testData = ModelBinderTestData.CreateClass(bodyGuid);
            HttpActionContext actionContext = CreateHttpActionContext(jsonQueryString, HttpMethod.Put, _testData);
            actionContext.ActionArguments.Add("guid", urlGuid);
            actionContext.Request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            actionContext.Request.SetConfiguration(config);
            actionContext.Request.SetRouteData(data);

            var metadataProvider = _modelMetadataProviderMock.Object;
            var metaData = new System.Web.Http.Metadata.ModelMetadata(metadataProvider, controller.GetType(), null, _testData.GetType(), "id");

            var bindingContext = new System.Web.Http.ModelBinding.ModelBindingContext()
            {
                ModelMetadata = metaData,
                ModelState = controller.ModelState
            };
            bindingContext.ModelName = "ModelBinderTestData";

            eedmModelBinder = new EedmModelBinder();
            var result = eedmModelBinder.BindModel(actionContext, bindingContext);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BindModel_Test_BindingContext_Null_False_Result()
        {
            //Arrange
            string urlGuid = "1234";
            string bodyGuid = urlGuid;
            string bodyString = string.Concat(@"{", "\"id\":\"", string.Format("{0}\"", bodyGuid), "}");
            string requestURI = "http://localhost/Ellucian.Colleague.Api/MockPersons/{0}";
            string jsonQueryString = String.Format(requestURI, urlGuid);

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("MockPersons/{guid}", "PutHousingAssignmentAsync");

            HttpRoute route = new HttpRoute("MockPersons/{guid}", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.Add("MockPersons/{guid}", route);

            _testData = ModelBinderTestData.CreateClass(bodyGuid);
            HttpActionContext actionContext = CreateHttpActionContext(jsonQueryString, HttpMethod.Put, _testData);
            actionContext.ActionArguments.Add("guid", urlGuid);
            actionContext.Request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            actionContext.Request.SetConfiguration(config);
            actionContext.Request.SetRouteData(data);

            eedmModelBinder = new EedmModelBinder();
            var result = eedmModelBinder.BindModel(actionContext, null);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void BindModel_Test_True_Result()
        {
            //Arrange
            string urlGuid = string.Empty;
            string bodyGuid = Guid.NewGuid().ToString();
            string bodyString = string.Concat(@"{", "\"id\":\"", string.Format("{0}\"", bodyGuid), "}");
            string requestURI = "http://localhost/Ellucian.Colleague.Api/MockPersons/{0}";
            string jsonQueryString = String.Format(requestURI, urlGuid);

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("MockPersons/{guid}", "PutHousingAssignmentAsync");
            HttpRoute route = new HttpRoute("MockPersons/{guid}", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.Add("MockPersons/{guid}", route);

            _testData = ModelBinderTestData.CreateClass(bodyGuid);
            HttpActionContext actionContext = CreateHttpActionContext(jsonQueryString, HttpMethod.Put, _testData);
            actionContext.ActionArguments.Add("guid", urlGuid);
            actionContext.Request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            actionContext.Request.SetConfiguration(config);
            actionContext.Request.SetRouteData(data);

            var metadataProvider = _modelMetadataProviderMock.Object;
            var metaData = new System.Web.Http.Metadata.ModelMetadata(metadataProvider, controller.GetType(), null, _testData.GetType(), "id");

            var bindingContext = new System.Web.Http.ModelBinding.ModelBindingContext()
            {
                ModelMetadata = metaData,
                ModelState = controller.ModelState
            };
            bindingContext.ModelName = "ModelBinderTestData";

            eedmModelBinder = new EedmModelBinder();
            var result = eedmModelBinder.BindModel(actionContext, bindingContext);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BindModel_Test_POST_CannotDefineGuidException()
        {
            //Arrange
            string urlGuid = string.Empty;
            string bodyGuid = Guid.NewGuid().ToString();
            string bodyString = string.Concat(@"{","\"id\":\"", string.Format("{0}\"", bodyGuid), "}");
            string requestURI = "http://localhost/Ellucian.Colleague.Api/MockPersons/{0}";
            string jsonQueryString = String.Format(requestURI, urlGuid);

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("MockPersons/", "PostHousingAssignmentAsync");
            HttpRoute route = new HttpRoute("MockPersons/", routeValueDict); 
            
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.Add("MockPersons/", route);
            HttpRouteData data = new HttpRouteData(route);

            _testData = ModelBinderTestData.CreateClass(bodyGuid);
            HttpActionContext actionContext = CreateHttpActionContext(jsonQueryString, HttpMethod.Post, _testData);
            actionContext.Request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            actionContext.Request.SetConfiguration(config);
            actionContext.Request.SetRouteData(data);

            var metadataProvider = _modelMetadataProviderMock.Object;
            var metaData = new System.Web.Http.Metadata.ModelMetadata(metadataProvider, controller.GetType(), null, _testData.GetType(), "id");

            var bindingContext = new System.Web.Http.ModelBinding.ModelBindingContext()
            {
                ModelMetadata = metaData,
                ModelState = controller.ModelState                
            };
            bindingContext.ModelName = "ModelBinderTestData";

            eedmModelBinder = new EedmModelBinder();
            var result = eedmModelBinder.BindModel(actionContext, bindingContext);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BindModel_Test_PUT_BadGuid()
        {
            //Arrange
            string urlGuid = "119442ca-cd1c-44ef-a527-0710cf16024";
            string bodyGuid = "119442ca-cd1c-44ef-a527-0710cf16024";
            string requestURI = "http://localhost/Ellucian.Colleague.Api/MockPersons/{0}";
            string jsonQueryString = String.Format(requestURI, urlGuid);
            string bodyString = string.Concat(@"{", "\"id\":\"", string.Format("{0}\"", bodyGuid), "}");


            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("MockPersons/", "PostHousingAssignmentAsync");
            HttpRoute route = new HttpRoute("MockPersons/", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.Add("MockPersons/", route);

            _testData = ModelBinderTestData.CreateClass(bodyGuid);
            HttpActionContext actionContext = CreateHttpActionContext(jsonQueryString, HttpMethod.Put, _testData);
            actionContext.Request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            actionContext.Request.SetConfiguration(config);
            actionContext.Request.SetRouteData(data);

            var metadataProvider = _modelMetadataProviderMock.Object;
            var metaData = new System.Web.Http.Metadata.ModelMetadata(metadataProvider, controller.GetType(), null, _testData.GetType(), "id");

            var bindingContext = new System.Web.Http.ModelBinding.ModelBindingContext()
            {
                ModelMetadata = metaData,
                ModelState = controller.ModelState
            };
            bindingContext.ModelName = "ModelBinderTestData";

            eedmModelBinder = new EedmModelBinder();
            var result = eedmModelBinder.BindModel(actionContext, bindingContext);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BindModel_Test_PUT_NillGuidInUrl()
        {
            //Arrange
            string urlGuid = Guid.NewGuid().ToString();
            string bodyGuid = Guid.NewGuid().ToString();
            string requestURI = "http://localhost/Ellucian.Colleague.Api/MockPersons/{0}";
            string jsonQueryString = String.Format(requestURI, Guid.Empty.ToString());
            string bodyString = string.Concat(@"{", "\"id\":\"", string.Format("{0}\"", bodyGuid), "}");

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("MockPersons/{guid}", "PutHousingAssignmentAsync");
            HttpRoute route = new HttpRoute("MockPersons/{guid}", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.Add("MockPersons/{guid}", route);

            _testData = ModelBinderTestData.CreateClass(bodyGuid);
            HttpActionContext actionContext = CreateHttpActionContext(jsonQueryString, HttpMethod.Put, _testData);
            actionContext.ActionArguments.Add("guid", Guid.Empty.ToString());
            actionContext.Request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            actionContext.Request.SetConfiguration(config);
            actionContext.Request.SetRouteData(data);

            var metadataProvider = _modelMetadataProviderMock.Object;
            var metaData = new System.Web.Http.Metadata.ModelMetadata(metadataProvider, controller.GetType(), null, _testData.GetType(), "id");

            var bindingContext = new System.Web.Http.ModelBinding.ModelBindingContext()
            {
                ModelMetadata = metaData,
                ModelState = controller.ModelState
            };
            bindingContext.ModelName = "ModelBinderTestData";

            eedmModelBinder = new EedmModelBinder();
            var result = eedmModelBinder.BindModel(actionContext, bindingContext);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BindModel_Test_PUT_UrlAndBodyGuidDifferent()
        {
            //Arrange
            string urlGuid = Guid.NewGuid().ToString();
            string bodyGuid = Guid.NewGuid().ToString();
            string requestURI = "http://localhost/Ellucian.Colleague.Api/MockPersons/{0}";
            string jsonQueryString = String.Format(requestURI, urlGuid);
            string bodyString = string.Concat(@"{", "\"id\":\"", string.Format("{0}\"", bodyGuid), "}");


            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("MockPersons/{guid}", "PutHousingAssignmentAsync");

            HttpRoute route = new HttpRoute("MockPersons/{guid}", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.Add("MockPersons/{guid}", route);

            _testData = ModelBinderTestData.CreateClass(bodyGuid);
            HttpActionContext actionContext = CreateHttpActionContext(jsonQueryString, HttpMethod.Put, _testData);
            actionContext.ActionArguments.Add("guid", urlGuid);
            actionContext.Request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            actionContext.Request.SetConfiguration(config);
            actionContext.Request.SetRouteData(data);

            var metadataProvider = _modelMetadataProviderMock.Object;
            var metaData = new System.Web.Http.Metadata.ModelMetadata(metadataProvider, controller.GetType(), null, _testData.GetType(), "id");

            var bindingContext = new System.Web.Http.ModelBinding.ModelBindingContext()
            {
                ModelMetadata = metaData,
                ModelState = controller.ModelState
            };
            bindingContext.ModelName = "ModelBinderTestData";

            eedmModelBinder = new EedmModelBinder();
            var result = eedmModelBinder.BindModel(actionContext, bindingContext);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BindModel_Test_PUT_BadUrlGuid()
        {
            //Arrange
            string urlGuid = "1234";
            string bodyGuid = urlGuid;
            string requestURI = "http://localhost/Ellucian.Colleague.Api/MockPersons/{0}";
            string jsonQueryString = String.Format(requestURI, urlGuid);
            string bodyString = string.Concat(@"{", "\"id\":\"", string.Format("{0}\"", bodyGuid), "}");

            HttpRouteValueDictionary routeValueDict = new HttpRouteValueDictionary();
            routeValueDict.Add("MockPersons/{guid}", "PutHousingAssignmentAsync");

            HttpRoute route = new HttpRoute("MockPersons/{guid}", routeValueDict);
            HttpRouteData data = new HttpRouteData(route);

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.Add("MockPersons/{guid}", route);

            _testData = ModelBinderTestData.CreateClass(bodyGuid);
            HttpActionContext actionContext = CreateHttpActionContext(jsonQueryString, HttpMethod.Put, _testData);
            actionContext.ActionArguments.Add("guid", urlGuid);
            actionContext.Request.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
            actionContext.Request.SetConfiguration(config);
            actionContext.Request.SetRouteData(data);

            var metadataProvider = _modelMetadataProviderMock.Object;
            var metaData = new System.Web.Http.Metadata.ModelMetadata(metadataProvider, controller.GetType(), null, _testData.GetType(), "id");

            var bindingContext = new System.Web.Http.ModelBinding.ModelBindingContext()
            {
                ModelMetadata = metaData,
                ModelState = controller.ModelState
            };
            bindingContext.ModelName = "ModelBinderTestData";

            eedmModelBinder = new EedmModelBinder();
            var result = eedmModelBinder.BindModel(actionContext, bindingContext);
        }
    }

    public class MockPersonsController : ApiController
    {
        private ILogger _logger;

        //private 
        public MockPersonsController(ILogger logger)
        {
            _logger = slf4net.LoggerFactory.GetLogger("ColleagueAPIApplication");
        }

        [HttpPost, EedmResponseFilter]
        [Route("MockPersons/")]
        public ModelBinderTestData PostHousingAssignmentAsync([ModelBinder(typeof(EedmModelBinder))] ModelBinderTestData source)
        {
            return ModelBinderTestData.CreateClass(Guid.NewGuid().ToString());
        }

        [HttpPut, EedmResponseFilter]
        [Route("MockPersons/guid")]
        public ModelBinderTestData PutHousingAssignmentAsync([FromUri] string guid, [ModelBinder(typeof(EedmModelBinder))] ModelBinderTestData source)
        {
            return ModelBinderTestData.CreateClass(Guid.NewGuid().ToString());
        }
    }
    /// <summary>
    /// Class to test the valid guids. 
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ModelBinderTestData
    {
        /// <summary>
        /// A Globally Unique ID (GUID)
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }
        public ModelBinderTestData()
        {

        }

        public static ModelBinderTestData CreateClass(string idValue)
        {
            return new ModelBinderTestData() { Id = idValue };
        }
    }
}
