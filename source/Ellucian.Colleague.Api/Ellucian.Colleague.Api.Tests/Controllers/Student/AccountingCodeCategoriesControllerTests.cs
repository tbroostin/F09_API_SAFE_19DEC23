//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AccountingCodeCategoriesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAccountingCodeCategoriesService> accountingCodeCategoriesServiceMock;
        private Mock<ILogger> loggerMock;
        private AccountingCodeCategoriesController accountingCodeCategoriesController;      
        private IEnumerable<Domain.Student.Entities.ArCategory> allArCategories;
        private List<Dtos.AccountingCodeCategory> accountingCodeCategoriesCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize() 
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            accountingCodeCategoriesServiceMock = new Mock<IAccountingCodeCategoriesService>();
            loggerMock = new Mock<ILogger>();
            accountingCodeCategoriesCollection = new List<Dtos.AccountingCodeCategory>();

            allArCategories  = new List<Domain.Student.Entities.ArCategory>()
                {
                    new Domain.Student.Entities.ArCategory("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT", "Athletic"),
                    new Domain.Student.Entities.ArCategory("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC", "Academic"),
                    new Domain.Student.Entities.ArCategory("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU", "Cultural")
                };
            
            foreach (var source in allArCategories)
            {
                var accountingCodeCategories = new Ellucian.Colleague.Dtos.AccountingCodeCategory
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                accountingCodeCategoriesCollection.Add(accountingCodeCategories);
            }

            accountingCodeCategoriesController = new AccountingCodeCategoriesController(accountingCodeCategoriesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            accountingCodeCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            accountingCodeCategoriesController = null;
            allArCategories = null;
            accountingCodeCategoriesCollection = null;
            loggerMock = null;
            accountingCodeCategoriesServiceMock = null;
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategories_ValidateFields_Nocache()
        {
            accountingCodeCategoriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoriesAsync(false)).ReturnsAsync(accountingCodeCategoriesCollection);
       
            var sourceContexts = (await accountingCodeCategoriesController.GetAccountingCodeCategoriesAsync()).ToList();
            Assert.AreEqual(accountingCodeCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = accountingCodeCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategories_ValidateFields_Cache()
        {
            accountingCodeCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoriesAsync(true)).ReturnsAsync(accountingCodeCategoriesCollection);

            var sourceContexts = (await accountingCodeCategoriesController.GetAccountingCodeCategoriesAsync()).ToList();
            Assert.AreEqual(accountingCodeCategoriesCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = accountingCodeCategoriesCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

         [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategories_KeyNotFoundException()
        {
            //
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoriesAsync(false))
                .Throws<KeyNotFoundException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategories_PermissionsException()
        {
            
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoriesAsync(false))
                .Throws<PermissionsException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategories_ArgumentException()
        {
            
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoriesAsync(false))
                .Throws<ArgumentException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategories_RepositoryException()
        {
            
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoriesAsync(false))
                .Throws<RepositoryException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategories_IntegrationApiException()
        {
            
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoriesAsync(false))
                .Throws<IntegrationApiException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoriesAsync();
        }

        [TestMethod]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategoriesByGuidAsync_ValidateFields()
        {
            accountingCodeCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var expected = accountingCodeCategoriesCollection.FirstOrDefault();
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoryByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await accountingCodeCategoriesController.GetAccountingCodeCategoryByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategories_Exception()
        {
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoriesAsync(false)).Throws<Exception>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoriesAsync();       
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategoriesByGuidAsync_Exception()
        {
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoryByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoryByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategoriesByGuid_KeyNotFoundException()
        {
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoryByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoryByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategoryByGuid_PermissionsException()
        {
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoryByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoryByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategoriesByGuid_ArgumentException()
        {
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoryByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoryByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategoriesByGuid_RepositoryException()
        {
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoryByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoryByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategoriesByGuid_IntegrationApiException()
        {
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoryByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoryByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_GetAccountingCodeCategoriesByGuid_Exception()
        {
            accountingCodeCategoriesServiceMock.Setup(x => x.GetAccountingCodeCategoryByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await accountingCodeCategoriesController.GetAccountingCodeCategoryByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_PostAccountingCodeCategoriesAsync_Exception()
        {
            await accountingCodeCategoriesController.PostAccountingCodeCategoryAsync(accountingCodeCategoriesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_PutAccountingCodeCategoriesAsync_Exception()
        {
            var sourceContext = accountingCodeCategoriesCollection.FirstOrDefault();
            await accountingCodeCategoriesController.PutAccountingCodeCategoryAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingCodeCategoriesController_DeleteAccountingCodeCategoriesAsync_Exception()
        {
            await accountingCodeCategoriesController.DeleteAccountingCodeCategoryAsync(accountingCodeCategoriesCollection.FirstOrDefault().Id);
        }
    }
}