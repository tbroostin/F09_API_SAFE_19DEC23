// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Tests;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class SourceContextControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IDemographicService> demographicServiceMock;
        private Mock<ILogger> loggerMock;
        private SourceContextController sourceContextController;      
        private IEnumerable<Domain.Base.Entities.SourceContext> allSourceContext;
        private List<Dtos.SourceContext> sourceContextCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            
            demographicServiceMock = new Mock<IDemographicService>();
            loggerMock = new Mock<ILogger>();
            sourceContextCollection = new List<Dtos.SourceContext>();

            allSourceContext = new TestSourceContextRepository().GetSourceContexts();
            
            foreach (var source in allSourceContext)
            {
                var sourceContext = new Ellucian.Colleague.Dtos.SourceContext
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                sourceContextCollection.Add(sourceContext);
            }

            sourceContextController = new SourceContextController(demographicServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            sourceContextController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sourceContextController = null;
            allSourceContext = null;
            sourceContextCollection = null;
            loggerMock = null;
            demographicServiceMock = null;
        }

        [TestMethod]
        public async Task SourceContextController_GetSourceContext_ValidateFields_Nocache()
        {
            sourceContextController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            demographicServiceMock.Setup(x => x.GetSourceContextsAsync(false)).ReturnsAsync(sourceContextCollection);
       
            var sourceContexts = (await sourceContextController.GetSourceContextsAsync()).ToList();
            Assert.AreEqual(sourceContextCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sourceContextCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SourceContextController_GetSourceContext_ValidateFields_Cache()
        {
            sourceContextController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            demographicServiceMock.Setup(x => x.GetSourceContextsAsync(true)).ReturnsAsync(sourceContextCollection);

            var sourceContexts = (await sourceContextController.GetSourceContextsAsync()).ToList();
            Assert.AreEqual(sourceContextCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sourceContextCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SourceContextController_GetSourceContextsByIdAsync_ValidateFields()
        {
            var expected = sourceContextCollection.FirstOrDefault();
            demographicServiceMock.Setup(x => x.GetSourceContextsByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await sourceContextController.GetSourceContextsByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourceContextController_GetSourceContext_Exception()
        {
            demographicServiceMock.Setup(x => x.GetSourceContextsAsync(false)).Throws<Exception>();
            await sourceContextController.GetSourceContextsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourceContextController_GetSourceContextsByIdAsync_Exception()
        {
            demographicServiceMock.Setup(x => x.GetSourceContextsByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await sourceContextController.GetSourceContextsByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourceContextController_PostSourceContextsAsync_Exception()
        {
            await sourceContextController.PostSourceContextsAsync(sourceContextCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourceContextController_PutSourceContextsAsync_Exception()
        {
            var sourceContext = sourceContextCollection.FirstOrDefault();
            await sourceContextController.PutSourceContextsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourceContextController_DeleteSourceContextsAsync_Exception()
        {
            await sourceContextController.DeleteSourceContextsAsync(sourceContextCollection.FirstOrDefault().Id);
        }
    }
}