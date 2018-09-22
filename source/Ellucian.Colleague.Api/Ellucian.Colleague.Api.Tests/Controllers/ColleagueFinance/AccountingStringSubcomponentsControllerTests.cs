//Copyright 2018 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class AccountingStringSubcomponentsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAccountingStringSubcomponentsService> accountingStringSubcomponentsServiceMock;
        private Mock<ILogger> loggerMock;
        private AccountingStringSubcomponentsController accountingStringSubcomponentsController;
        private IEnumerable<Domain.ColleagueFinance.Entities.AcctStructureIntg> allAcctStructureIntg;
        private List<Dtos.AccountingStringSubcomponents> accountingStringSubcomponentsCollection;
        private string expectedGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            accountingStringSubcomponentsServiceMock = new Mock<IAccountingStringSubcomponentsService>();
            loggerMock = new Mock<ILogger>();
            accountingStringSubcomponentsCollection = new List<Dtos.AccountingStringSubcomponents>();

            allAcctStructureIntg = new List<Domain.ColleagueFinance.Entities.AcctStructureIntg>()
                {
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "FUND", "fund"),
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "GL.CLASS", "GL Class"),
                    new Domain.ColleagueFinance.Entities.AcctStructureIntg("d2253ac7-9931-4560-b42f-1fccd43c952e", "PROGRAM", "Program")
                };

            foreach (var source in allAcctStructureIntg)
            {
                var accountingStringSubcomponents = new Ellucian.Colleague.Dtos.AccountingStringSubcomponents
                {
                    Id = source.Guid,
                    Title = source.Title,

                };
                accountingStringSubcomponentsCollection.Add(accountingStringSubcomponents);
            }

            accountingStringSubcomponentsController = new AccountingStringSubcomponentsController(accountingStringSubcomponentsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            accountingStringSubcomponentsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            accountingStringSubcomponentsController = null;
            allAcctStructureIntg = null;
            accountingStringSubcomponentsCollection = null;
            loggerMock = null;
            accountingStringSubcomponentsServiceMock = null;
        }

        [TestMethod]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponents_ValidateFields_Nocache()
        {
            accountingStringSubcomponentsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsAsync(false)).ReturnsAsync(accountingStringSubcomponentsCollection);

            var sourceContexts = (await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsAsync()).ToList();
            Assert.AreEqual(accountingStringSubcomponentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = accountingStringSubcomponentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponents_ValidateFields_Cache()
        {
            accountingStringSubcomponentsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsAsync(true)).ReturnsAsync(accountingStringSubcomponentsCollection);

            var sourceContexts = (await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsAsync()).ToList();
            Assert.AreEqual(accountingStringSubcomponentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = accountingStringSubcomponentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponents_KeyNotFoundException()
        {
            //
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsAsync(false))
                .Throws<KeyNotFoundException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponents_PermissionsException()
        {

            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsAsync(false))
                .Throws<PermissionsException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponents_ArgumentException()
        {

            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsAsync(false))
                .Throws<ArgumentException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponents_RepositoryException()
        {

            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsAsync(false))
                .Throws<RepositoryException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponents_IntegrationApiException()
        {

            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsAsync(false))
                .Throws<IntegrationApiException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsAsync();
        }

        [TestMethod]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponentsByGuidAsync_ValidateFields()
        {
            var expected = accountingStringSubcomponentsCollection.FirstOrDefault();
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponents_Exception()
        {
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsAsync(false)).Throws<Exception>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponentsByGuidAsync_Exception()
        {
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponentsByGuid_KeyNotFoundException()
        {
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<KeyNotFoundException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponentsByGuid_PermissionsException()
        {
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<PermissionsException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponentsByGuid_ArgumentException()
        {
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<ArgumentException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponentsByGuid_RepositoryException()
        {
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<RepositoryException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponentsByGuid_IntegrationApiException()
        {
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<IntegrationApiException>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_GetAccountingStringSubcomponentsByGuid_Exception()
        {
            accountingStringSubcomponentsServiceMock.Setup(x => x.GetAccountingStringSubcomponentsByGuidAsync(It.IsAny<string>(), It.IsAny<bool>()))
                .Throws<Exception>();
            await accountingStringSubcomponentsController.GetAccountingStringSubcomponentsByGuidAsync(expectedGuid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_PostAccountingStringSubcomponentsAsync_Exception()
        {
            await accountingStringSubcomponentsController.PostAccountingStringSubcomponentsAsync(accountingStringSubcomponentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_PutAccountingStringSubcomponentsAsync_Exception()
        {
            var sourceContext = accountingStringSubcomponentsCollection.FirstOrDefault();
            await accountingStringSubcomponentsController.PutAccountingStringSubcomponentsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringSubcomponentsController_DeleteAccountingStringSubcomponentsAsync_Exception()
        {
            await accountingStringSubcomponentsController.DeleteAccountingStringSubcomponentsAsync(accountingStringSubcomponentsCollection.FirstOrDefault().Id);
        }
    }
}