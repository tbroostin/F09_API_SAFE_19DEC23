// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class FinancialAidFundCategoriesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IFinancialAidFundCategoryService> financialAidFundCategoryServiceMock;
        private Mock<ILogger> loggerMock;
        private FinancialAidFundCategoriesController financialAidFundCategoriesController;
        private IEnumerable<FinancialAidFundCategory> allFinancialAidFundCategory;
        private List<Dtos.FinancialAidFundCategory> financialAidFundCategoryCollection;

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            financialAidFundCategoryServiceMock = new Mock<IFinancialAidFundCategoryService>();
            loggerMock = new Mock<ILogger>();
            financialAidFundCategoryCollection = new List<Dtos.FinancialAidFundCategory>();

            allFinancialAidFundCategory = (await new TestStudentReferenceDataRepository().GetFinancialAidFundCategoriesAsync(true)).ToList();

            foreach (var source in allFinancialAidFundCategory)
            {
                var financialAidFundCategory = new Ellucian.Colleague.Dtos.FinancialAidFundCategory
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null,
                };
                financialAidFundCategoryCollection.Add(financialAidFundCategory);
            }

            financialAidFundCategoriesController = new FinancialAidFundCategoriesController(financialAidFundCategoryServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            financialAidFundCategoriesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            financialAidFundCategoriesController = null;
            allFinancialAidFundCategory = null;
            financialAidFundCategoryCollection = null;
            loggerMock = null;
            financialAidFundCategoryServiceMock = null;
        }

        [TestMethod]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategory_ValidateFields_Nocache()
        {
            financialAidFundCategoriesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoriesAsync(false)).ReturnsAsync(financialAidFundCategoryCollection);

            var sourceContexts = (await financialAidFundCategoriesController.GetFinancialAidFundCategoriesAsync()).ToList();
            Assert.AreEqual(financialAidFundCategoryCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidFundCategoryCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategory_ValidateFields_Cache()
        {
            financialAidFundCategoriesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoriesAsync(true)).ReturnsAsync(financialAidFundCategoryCollection);

            var sourceContexts = (await financialAidFundCategoriesController.GetFinancialAidFundCategoriesAsync()).ToList();
            Assert.AreEqual(financialAidFundCategoryCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = financialAidFundCategoryCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategoriesByIdAsync_ValidateFields()
        {
            var expected = financialAidFundCategoryCollection.FirstOrDefault();
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoryByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await financialAidFundCategoriesController.GetFinancialAidFundCategoryByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategory_PermissionsException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoriesAsync(false)).Throws<PermissionsException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategory_KeyNotFoundException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoriesAsync(false)).Throws<KeyNotFoundException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategory_ArgumentNullException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoriesAsync(false)).Throws<ArgumentNullException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategory_RepositoryException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoriesAsync(false)).Throws<RepositoryException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategory_IntgApiException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoriesAsync(false)).Throws<IntegrationApiException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategory_Exception()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoriesAsync(false)).Throws<Exception>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoriesAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategoriesByIdAsync_PermissionsException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoryByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoryByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategoriesByIdAsync_KeyNotFoundException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoryByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoryByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategoriesByIdAsync_ArgumentNullException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoryByGuidAsync(It.IsAny<string>())).Throws<ArgumentNullException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoryByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategoriesByIdAsync_RepositoryException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoryByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoryByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategoriesByIdAsync_IntgApiException()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoryByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoryByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_GetFinancialAidFundCategoriesByIdAsync_Exception()
        {
            financialAidFundCategoryServiceMock.Setup(x => x.GetFinancialAidFundCategoryByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await financialAidFundCategoriesController.GetFinancialAidFundCategoryByIdAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_PostFinancialAidFundCategoriesAsync_Exception()
        {
            await financialAidFundCategoriesController.PostFinancialAidFundCategoryAsync(financialAidFundCategoryCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_PutFinancialAidFundCategoriesAsync_Exception()
        {
            var sourceContext = financialAidFundCategoryCollection.FirstOrDefault();
            await financialAidFundCategoriesController.PutFinancialAidFundCategoryAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task FinancialAidFundCategoryController_DeleteFinancialAidFundCategoriesAsync_Exception()
        {
            await financialAidFundCategoriesController.DeleteFinancialAidFundCategoryAsync(financialAidFundCategoryCollection.FirstOrDefault().Id);
        }
    }
}