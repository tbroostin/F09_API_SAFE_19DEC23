//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Vocation = Ellucian.Colleague.Domain.Base.Entities.Vocation;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class EmployeeVocationsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEmploymentVocationService> employmentVocationsServiceMock;
        private Mock<ILogger> loggerMock;
        private EmploymentVocationsController employmentVocationsController;
        private IEnumerable<Ellucian.Colleague.Domain.Base.Entities.Vocation> allEmployeeVocations;
        private List<Dtos.EmploymentVocation> employmentVocationCollection;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            employmentVocationsServiceMock = new Mock<IEmploymentVocationService>();
            loggerMock = new Mock<ILogger>();
            employmentVocationCollection = new List<Dtos.EmploymentVocation>();

            allEmployeeVocations = new List<Vocation>()
                {
                    new Vocation("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Vocation("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Vocation("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Test")
                };

            foreach (var source in allEmployeeVocations)
            {
                var vocations = new Ellucian.Colleague.Dtos.EmploymentVocation
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = string.IsNullOrEmpty(source.Description)? source.Code : source.Description,
                    Description = null
                };
                employmentVocationCollection.Add(vocations);
            }

            employmentVocationsController = new EmploymentVocationsController(employmentVocationsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            employmentVocationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            employmentVocationsController = null;
            allEmployeeVocations = null;
            employmentVocationCollection = null;
            loggerMock = null;
            employmentVocationsServiceMock = null;
        }

        [TestMethod]
        public async Task EmployeeVocationsController_GetEmployeeVocations_ValidateFields_cache()
        {
            employmentVocationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationsAsync(false)).ReturnsAsync(employmentVocationCollection);

            var sourceContexts = (await employmentVocationsController.GetEmploymentVocationsAsync()).ToList();
            Assert.AreEqual(employmentVocationCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentVocationCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EmployeeVocationsController_GetEmployeeVocations_ValidateFields_Nocache()
        {
            employmentVocationsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationsAsync(true)).ReturnsAsync(employmentVocationCollection);

            var sourceContexts = (await employmentVocationsController.GetEmploymentVocationsAsync()).ToList();
            Assert.AreEqual(employmentVocationCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentVocationCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EmployeeVocationsController_GetEmployeeVocationsByGuidAsync()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationByGuidAsync(It.IsAny<string>())).ReturnsAsync(employmentVocationCollection.First());

            var sourceContext = (await employmentVocationsController.GetEmploymentVocationByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc"));
            var expected = employmentVocationCollection.First();
            var actual = sourceContext;
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Title, actual.Title);
            Assert.AreEqual(expected.Code, actual.Code);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocations_KeyNotFoundException()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationsAsync(false)).Throws<KeyNotFoundException>();
            await employmentVocationsController.GetEmploymentVocationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocations_PermissionsException()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationsAsync(false)).Throws<PermissionsException>();
            await employmentVocationsController.GetEmploymentVocationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocations_InvalidOperationException()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationsAsync(false)).Throws<InvalidOperationException>();
            await employmentVocationsController.GetEmploymentVocationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocations_Exception()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationsAsync(false)).Throws<Exception>();
            await employmentVocationsController.GetEmploymentVocationsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocationsByGuidAsync_NullId_Exception()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await employmentVocationsController.GetEmploymentVocationByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocationsByGuidAsync_KeyNotFoundException()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await employmentVocationsController.GetEmploymentVocationByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocationsByGuidAsync_PermissionsException()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await employmentVocationsController.GetEmploymentVocationByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocationsByGuidAsync_InvalidOperationException()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationByGuidAsync(It.IsAny<string>())).Throws<InvalidOperationException>();
            await employmentVocationsController.GetEmploymentVocationByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_GetEmployeeVocationsByGuidAsync_Exception()
        {
            employmentVocationsServiceMock.Setup(x => x.GetEmploymentVocationByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await employmentVocationsController.GetEmploymentVocationByGuidAsync("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_PostEmployeeVocationsAsync_Exception()
        {
            await employmentVocationsController.PostEmploymentVocationAsync(employmentVocationCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_PutEmployeeVocationsAsync_Exception()
        {
            var sourceContext = employmentVocationCollection.FirstOrDefault();
            await employmentVocationsController.PutEmploymentVocationAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmployeeVocationsController_DeleteEmployeeVocationsAsync_Exception()
        {
            await employmentVocationsController.DeleteEmploymentVocationAsync(employmentVocationCollection.FirstOrDefault().Id);
        }
    }
}