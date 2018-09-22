// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Base.Tests;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SourcesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<ISourceService> sourceServiceMock;
        private Mock<ILogger> loggerMock;
        private SourcesController sourcesController;
        private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.TestSource> allTestSources;
        private IEnumerable<Domain.Base.Entities.SourceContext> allSources; 
        private List<Dtos.Source> sourceCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            sourceServiceMock = new Mock<ISourceService>();
            loggerMock = new Mock<ILogger>();
            sourceCollection = new List<Dtos.Source>();

            allSources = new TestSourceContextRepository().GetSourceContexts();
            allTestSources = new TestStudentReferenceDataRepository().GetTestSourcesAsync(false).Result;

            var remarksId = allSources.FirstOrDefault(x => x.Code == "TESTS").Guid;
            foreach (var testSource in allTestSources)
            {
                var source = new Ellucian.Colleague.Dtos.Source
                {
                    Id = testSource.Guid,
                    Code = testSource.Code,
                    Title = testSource.Description,
                    Status = LifeCycleStatus.Active,
                    Contexts = new List<GuidObject2>() { new GuidObject2(remarksId) },
                    Description = null
                };
                sourceCollection.Add(source);
            }

            sourcesController = new SourcesController(sourceServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            sourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            sourcesController = null;
            allTestSources = null;
            sourceCollection = null;
            loggerMock = null;
            sourceServiceMock = null;
        }

        [TestMethod]
        public async Task SourcesController_GetSources_ValidateFields_Nocache()
        {
            sourcesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            sourceServiceMock.Setup(x => x.GetSourcesAsync(false)).ReturnsAsync(sourceCollection);

            var sources = (await sourcesController.GetSourcesAsync()).ToList();
            Assert.AreEqual(sourceCollection.Count, sources.Count);
            for (var i = 0; i < sources.Count; i++)
            {
                var expected = sourceCollection[i];
                var actual = sources[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.Status, actual.Status, "Status, Index=" + i.ToString());
                Assert.AreEqual(expected.Contexts[0].Id, actual.Contexts[0].Id, "Contexts.Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SourcesController_GetSources_ValidateFields_Cache()
        {
            sourcesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            sourceServiceMock.Setup(x => x.GetSourcesAsync(true)).ReturnsAsync(sourceCollection);

            var sourceContexts = (await sourcesController.GetSourcesAsync()).ToList();
            Assert.AreEqual(sourceCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = sourceCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
                Assert.AreEqual(expected.Status, actual.Status, "Status, Index=" + i.ToString());
                Assert.AreEqual(expected.Contexts[0].Id, actual.Contexts[0].Id, "Contexts.Id, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task SourcesController_GetSourcesByIdAsync_ValidateFields()
        {
            var expected = sourceCollection.FirstOrDefault();
            sourceServiceMock.Setup(x => x.GetSourceByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await sourcesController.GetSourceByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            Assert.AreEqual(expected.Status, actual.Status, "Status");
            Assert.AreEqual(expected.Contexts[0].Id, actual.Contexts[0].Id, "Contexts.Id");
        }


        [TestMethod]
        public async Task SourcesController_GetSourcesByIdAsync_ApplicationSource()
        {
            var expected = sourceCollection.FirstOrDefault();
            Assert.IsNotNull(expected);
            var applicationSourceGuid = allSources.FirstOrDefault(x => x.Code == "APPLICATION.SOURCES").Guid;
            //var contextSourceCollection = new List<GuidObject2>();
            Assert.IsNotNull(applicationSourceGuid);
            expected.Contexts = new List<GuidObject2>() {new GuidObject2(applicationSourceGuid)};

            sourceServiceMock.Setup(x => x.GetSourceByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await sourcesController.GetSourceByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
            Assert.AreEqual(expected.Status, actual.Status, "Status");
            Assert.AreEqual(applicationSourceGuid, actual.Contexts[0].Id, "Contexts.Id");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourcesController_GetSources_Exception()
        {
            sourceServiceMock.Setup(x => x.GetSourcesAsync(false)).Throws<Exception>();
            await sourcesController.GetSourcesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourcesController_GetSourcesByIdAsync_Exception()
        {
            sourceServiceMock.Setup(x => x.GetSourceByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await sourcesController.GetSourceByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourcesController_PostSourcesAsync_Exception()
        {
            await sourcesController.PostSourcesAsync(sourceCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourcesController_PutSourcesAsync_Exception()
        {
            var sourceContext = sourceCollection.FirstOrDefault();
            await sourcesController.PutSourcesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task SourcesController_DeleteSourcesAsync_Exception()
        {
            await sourcesController.DeleteSourcesAsync(sourceCollection.FirstOrDefault().Id);
        }
    }
}