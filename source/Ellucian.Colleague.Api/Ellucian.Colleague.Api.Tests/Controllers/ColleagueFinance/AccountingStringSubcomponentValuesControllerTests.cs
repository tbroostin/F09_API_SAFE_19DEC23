//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class AccountingStringSubcomponentValuesControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAccountingStringSubcomponentValuesService> accountingStringSubcomponentValuesServiceMock;
        private Mock<ILogger> loggerMock;
        private AccountingStringSubcomponentValuesController accountingStringSubcomponentValuesController;
        private IEnumerable<Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues> allAcctStringSubValues;
        private List<Dtos.AccountingStringSubcomponentValues> accountingStringSubcomponentValuesCollection;
        private Tuple<IEnumerable<Dtos.AccountingStringSubcomponentValues>, int> accountingStringSubcomponentValuesTuple;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            accountingStringSubcomponentValuesServiceMock = new Mock<IAccountingStringSubcomponentValuesService>();
            loggerMock = new Mock<ILogger>();
            accountingStringSubcomponentValuesCollection = new List<Dtos.AccountingStringSubcomponentValues>();

            allAcctStringSubValues = new List<Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues>()
                {
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "84", "Internal Service Fund", "FD"),
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "6", "JPA Fund", "FC"),
                    new Domain.ColleagueFinance.Entities.AccountingStringSubcomponentValues("d2253ac7-9931-4560-b42f-1fccd43c952e", "8", "Anuj Fund", "OB")
                };

            foreach (var source in allAcctStringSubValues)
            {
                var accountingStringSubcomponentValues = new Ellucian.Colleague.Dtos.AccountingStringSubcomponentValues
                {
                    Id = source.Guid,
                    Code = source.Code,
                    Title = source.Description,
                    Description = null
                };
                accountingStringSubcomponentValuesCollection.Add(accountingStringSubcomponentValues);
            }
            accountingStringSubcomponentValuesTuple = new Tuple<IEnumerable<Dtos.AccountingStringSubcomponentValues>, int>(accountingStringSubcomponentValuesCollection, accountingStringSubcomponentValuesCollection.Count());
            accountingStringSubcomponentValuesController = new AccountingStringSubcomponentValuesController(accountingStringSubcomponentValuesServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            accountingStringSubcomponentValuesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountingStringSubcomponentValuesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            accountingStringSubcomponentValuesController = null;
            allAcctStringSubValues = null;
            accountingStringSubcomponentValuesCollection = null;
            loggerMock = null;
            accountingStringSubcomponentValuesServiceMock = null;
        }

        [TestMethod]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValues_ValidateFields_Nocache()
        {
            accountingStringSubcomponentValuesController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), false)).ReturnsAsync(accountingStringSubcomponentValuesTuple);

            var results = await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesAsync(It.IsAny<Paging>());
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.AccountingStringSubcomponentValues> ActualsAPI =
                ((ObjectContent<IEnumerable<Dtos.AccountingStringSubcomponentValues>>)httpResponseMessage.Content).Value as
                    List<Dtos.AccountingStringSubcomponentValues>;

            Assert.AreEqual(accountingStringSubcomponentValuesCollection.Count, ActualsAPI.Count);
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = accountingStringSubcomponentValuesCollection[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValues_ValidateFields_Cache()
        {
            accountingStringSubcomponentValuesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), true)).ReturnsAsync(accountingStringSubcomponentValuesTuple);

            var results = await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesAsync(It.IsAny<Paging>());
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await results.ExecuteAsync(cancelToken);
            List<Dtos.AccountingStringSubcomponentValues> ActualsAPI =
                ((ObjectContent<IEnumerable<Dtos.AccountingStringSubcomponentValues>>)httpResponseMessage.Content).Value as
                    List<Dtos.AccountingStringSubcomponentValues>;

            Assert.AreEqual(accountingStringSubcomponentValuesCollection.Count, ActualsAPI.Count);
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = accountingStringSubcomponentValuesCollection[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValues_KeyNotFoundException()
        {
            //
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValues_PermissionsException()
        {

            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValues_ArgumentException()
        {

            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValues_RepositoryException()
        {

            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValues_IntegrationApiException()
        {

            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuidAsync_ValidateFields()
        {
            var expected = accountingStringSubcomponentValuesCollection.FirstOrDefault();
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuidAsync_ValidateFields_CacheTrue()
        {
            accountingStringSubcomponentValuesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            var expected = accountingStringSubcomponentValuesCollection.FirstOrDefault();
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValues_Exception()
        {
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Throws<Exception>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesAsync(It.IsAny<Paging>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuidAsync_Exception()
        {
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuid_KeyNotFoundException()
        {
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuid_PermissionsException()
        {
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuid_ArgumentException()
        {
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuid_RepositoryException()
        {
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuid_IntegrationApiException()
        {
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_GetAccountingStringSubcomponentValuesByGuid_Exception()
        {
            accountingStringSubcomponentValuesServiceMock.Setup(x => x.GetAccountingStringSubcomponentValuesByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await accountingStringSubcomponentValuesController.GetAccountingStringSubcomponentValuesByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_PostAccountingStringSubcomponentValuesAsync_Exception()
        {
            await accountingStringSubcomponentValuesController.PostAccountingStringSubcomponentValuesAsync(accountingStringSubcomponentValuesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_PutAccountingStringSubcomponentValuesAsync_Exception()
        {
            var sourceContext = accountingStringSubcomponentValuesCollection.FirstOrDefault();
            await accountingStringSubcomponentValuesController.PutAccountingStringSubcomponentValuesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentValuesController_DeleteAccountingStringSubcomponentValuesAsync_Exception()
        {
            await accountingStringSubcomponentValuesController.DeleteAccountingStringSubcomponentValuesAsync(accountingStringSubcomponentValuesCollection.FirstOrDefault().Id);
        }
    }
}