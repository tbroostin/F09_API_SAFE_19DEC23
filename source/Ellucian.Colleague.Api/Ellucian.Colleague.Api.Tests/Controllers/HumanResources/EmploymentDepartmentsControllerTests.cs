//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmploymentDepartmentsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IEmploymentDepartmentsService> employmentDepartmentsServiceMock;
        private Mock<ILogger> loggerMock;
        private EmploymentDepartmentsController employmentDepartmentsController;      
        private IEnumerable<Domain.HumanResources.Entities.EmploymentDepartment> allDepts;
        private List<Dtos.EmploymentDepartments> employmentDepartmentsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            employmentDepartmentsServiceMock = new Mock<IEmploymentDepartmentsService>();
            loggerMock = new Mock<ILogger>();
            employmentDepartmentsCollection = new List<Dtos.EmploymentDepartments>();

            allDepts = new List<Domain.HumanResources.Entities.EmploymentDepartment>()
                {
                    new Domain.HumanResources.Entities.EmploymentDepartment("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.HumanResources.Entities.EmploymentDepartment("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.HumanResources.Entities.EmploymentDepartment("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allDepts)
            {
                var employmentDepartments = new Ellucian.Colleague.Dtos.EmploymentDepartments
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                employmentDepartmentsCollection.Add(employmentDepartments);
            }

            employmentDepartmentsController = new EmploymentDepartmentsController(employmentDepartmentsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            employmentDepartmentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            employmentDepartmentsController = null;
            allDepts = null;
            employmentDepartmentsCollection = null;
            loggerMock = null;
            employmentDepartmentsServiceMock = null;
        }

        [TestMethod]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartments_ValidateFields_Nocache()
        {
            employmentDepartmentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsAsync(false)).ReturnsAsync(employmentDepartmentsCollection);
       
            var sourceContexts = (await employmentDepartmentsController.GetEmploymentDepartmentsAsync()).ToList();
            Assert.AreEqual(employmentDepartmentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentDepartmentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartments_ValidateFields_Cache()
        {
            employmentDepartmentsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsAsync(true)).ReturnsAsync(employmentDepartmentsCollection);

            var sourceContexts = (await employmentDepartmentsController.GetEmploymentDepartmentsAsync()).ToList();
            Assert.AreEqual(employmentDepartmentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = employmentDepartmentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartments_KeyNotFoundException()
        {
            //
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsAsync(false))
                .Throws<KeyNotFoundException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartments_PermissionsException()
        {
            
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsAsync(false))
                .Throws<PermissionsException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartments_ArgumentException()
        {
            
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsAsync(false))
                .Throws<ArgumentException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartments_RepositoryException()
        {
            
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsAsync(false))
                .Throws<RepositoryException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartments_IntegrationApiException()
        {
            
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsAsync(false))
                .Throws<IntegrationApiException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsAsync();
        }

        [TestMethod]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartmentsByGuidAsync_ValidateFields()
        {
            var expected = employmentDepartmentsCollection.FirstOrDefault();
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await employmentDepartmentsController.GetEmploymentDepartmentsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartments_Exception()
        {
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsAsync(false)).Throws<Exception>();
            await employmentDepartmentsController.GetEmploymentDepartmentsAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartmentsByGuidAsync_Exception()
        {
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await employmentDepartmentsController.GetEmploymentDepartmentsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartmentsByGuid_KeyNotFoundException()
        {
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsByGuidAsync(It.IsAny<string>()))
                .Throws<KeyNotFoundException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartmentsByGuid_PermissionsException()
        {
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsByGuidAsync(It.IsAny<string>()))
                .Throws<PermissionsException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartmentsByGuid_ArgumentException()
        {
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsByGuidAsync(It.IsAny<string>()))
                .Throws<ArgumentException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartmentsByGuid_RepositoryException()
        {
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsByGuidAsync(It.IsAny<string>()))
                .Throws<RepositoryException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartmentsByGuid_IntegrationApiException()
        {
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsByGuidAsync(It.IsAny<string>()))
                .Throws<IntegrationApiException>();
            await employmentDepartmentsController.GetEmploymentDepartmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_GetEmploymentDepartmentsByGuid_Exception()
        {
            employmentDepartmentsServiceMock.Setup(x => x.GetEmploymentDepartmentsByGuidAsync(It.IsAny<string>()))
                .Throws<Exception>();
            await employmentDepartmentsController.GetEmploymentDepartmentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_PostEmploymentDepartmentsAsync_Exception()
        {
            await employmentDepartmentsController.PostEmploymentDepartmentsAsync(employmentDepartmentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_PutEmploymentDepartmentsAsync_Exception()
        {
            var sourceContext = employmentDepartmentsCollection.FirstOrDefault();
            await employmentDepartmentsController.PutEmploymentDepartmentsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EmploymentDepartmentsController_DeleteEmploymentDepartmentsAsync_Exception()
        {
            await employmentDepartmentsController.DeleteEmploymentDepartmentsAsync(employmentDepartmentsCollection.FirstOrDefault().Id);
        }
    }
}