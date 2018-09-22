// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class AccountsPayableSourcesControllerTests
    {
        [TestClass]
        public class AccountsPayableSourcesControllerGet
        {
            #region Test Context

            private TestContext testContextInstance;

            /// <summary>
            ///Gets or sets the test context which provides
            ///information about and functionality for the current test run.
            ///</summary>
            public TestContext TestContext
            {
                get
                {
                    return testContextInstance;
                }
                set
                {
                    testContextInstance = value;
                }
            }

            #endregion

            private AccountsPayableSourcesController AccountsPayableSourcesController;
            ILogger logger = new Mock<ILogger>().Object;
            private Mock<IAccountsPayableSourcesService> AccountsPayableSourcesService;
            List<AccountsPayableSources> accountsPayableSourceDtoList;
            private string accountsPayableSourceGuid = "03ef76f3-61be-4990-8a99-9a80282fc420";

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                AccountsPayableSourcesService = new Mock<IAccountsPayableSourcesService>();

                BuildData();

                AccountsPayableSourcesController = new AccountsPayableSourcesController(AccountsPayableSourcesService.Object,logger);                AccountsPayableSourcesController.Request = new HttpRequestMessage();
                AccountsPayableSourcesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                AccountsPayableSourcesController = null;
                AccountsPayableSourcesService = null;
                accountsPayableSourceDtoList = null;
            }

            #region GET ALL Tests
            [TestMethod]
            public async Task AccountsPayableSources_GetAll_Async()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ReturnsAsync(accountsPayableSourceDtoList);

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = accountsPayableSourceDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                    Assert.AreEqual(expected.DirectDeposit, actual.DirectDeposit);
                }
            }

            [TestMethod]
            public async Task AccountsPayableSources_GetAll_TrueCache_Async()
            {
                AccountsPayableSourcesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                AccountsPayableSourcesController.Request.Headers.CacheControl.NoCache = true;

                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesAsync(true)).ReturnsAsync(accountsPayableSourceDtoList);

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesAsync();
                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = accountsPayableSourceDtoList.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Id, actual.Id);
                    Assert.IsNull(actual.Description);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Title, actual.Title);
                    Assert.AreEqual(expected.DirectDeposit, actual.DirectDeposit);
                }
            }

            #region Get ALL Exception Tests

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAll_Exception()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAll__PermissionsException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ThrowsAsync(new PermissionsException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAll_ArgumentException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ThrowsAsync(new ArgumentException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAll_RepositoryException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ThrowsAsync(new RepositoryException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAll_IntegrationApiException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesAsync();
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAll_KeyNotFoundException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesAsync(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesAsync();
            }
            
            #endregion
            #endregion

            #region GET ID TESTS
            [TestMethod]
            public async Task AccountsPayableSources_GetById_Async()
            {
                AccountsPayableSourcesController.Request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue();
                AccountsPayableSourcesController.Request.Headers.CacheControl.NoCache = true;

                var expected = accountsPayableSourceDtoList.FirstOrDefault(i => i.Id.Equals(accountsPayableSourceGuid));

                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesByGuidAsync(accountsPayableSourceGuid)).ReturnsAsync(expected);

                var actual = await AccountsPayableSourcesController.GetAccountsPayableSourcesByIdAsync(accountsPayableSourceGuid);

                Assert.IsNotNull(actual);
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.IsNull(actual.Description);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Title, actual.Title);
                Assert.AreEqual(expected.DirectDeposit, actual.DirectDeposit);
            }



            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAById_Exception()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAById_KeyNotFoundException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAById__PermissionsException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAById_ArgumentException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesByIdAsync(It.IsAny<string>());
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAById_RepositoryException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesByIdAsync(It.IsAny<string>());
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSources_GetAById_IntegrationApiException()
            {
                AccountsPayableSourcesService.Setup(x => x.GetAccountsPayableSourcesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());

                var actuals = await AccountsPayableSourcesController.GetAccountsPayableSourcesByIdAsync(It.IsAny<string>());
            }
            #endregion
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSourcesController_PostThrowsIntAppiExc()
            {
                await AccountsPayableSourcesController.PostAccountsPayableSourcesAsync(accountsPayableSourceDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSourcesController_PutThrowsIntAppiExc()
            {
                var result = await AccountsPayableSourcesController.PutAccountsPayableSourcesAsync(accountsPayableSourceGuid, accountsPayableSourceDtoList[0]);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountsPayableSourcesController_DeleteThrowsIntAppiExc()
            {
                await AccountsPayableSourcesController.DeleteAccountsPayableSourcesAsync(accountsPayableSourceGuid);
            }

            private void BuildData()
            {
                accountsPayableSourceDtoList = new List<AccountsPayableSources>() 
                {
                    new AccountsPayableSources(){Id = "03ef76f3-61be-4990-8a99-9a80282fc420", Code = "AP", Description = null, Title = "Regular Vendor Payments", DirectDeposit = Dtos.EnumProperties.DirectDeposit.Enabled },
                    new AccountsPayableSources(){Id = "d2f4f0af-6714-48c7-88d5-1c40cb407b6c", Code = "AP2", Description = null, Title = "Accounts Payable 2",DirectDeposit = Dtos.EnumProperties.DirectDeposit.Enabled},
                    new AccountsPayableSources(){Id = "c517d7a5-f06a-42c8-85ab-b6320e1c0c2a", Code = "CAD", Description = null, Title = "Canadian Account Payable",DirectDeposit = Dtos.EnumProperties.DirectDeposit.Enabled},
                    new AccountsPayableSources(){Id = "6c591aaa-5d33-4b19-b5e9-f6cf8956ef0a", Code = "EUR", Description = null, Title = "Euro Account Payable",DirectDeposit = Dtos.EnumProperties.DirectDeposit.Disabled},
                    new AccountsPayableSources(){Id = "81cd5b52-9705-4b1b-8eed-669c63db05e2", Code = "PA", Description = null, Title = "Payroll",DirectDeposit = Dtos.EnumProperties.DirectDeposit.Disabled},
                    new AccountsPayableSources(){Id = "164dc1ad-4d72-4dae-9875-52f761bb0132", Code = "S2", Description = null, Title = "Student refunds test",DirectDeposit = Dtos.EnumProperties.DirectDeposit.Enabled}
                };



            }
        }
    }
}