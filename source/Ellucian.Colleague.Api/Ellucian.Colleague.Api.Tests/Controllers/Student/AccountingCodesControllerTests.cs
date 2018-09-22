// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AccountingCodesControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IAccountingCodesService> accountingCodesServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            AccountingCodesController accountingCodesController;
            List<Ellucian.Colleague.Domain.Student.Entities.AccountingCode> accountingCodesEntities;
            List<Dtos.AccountingCode> accountingCodesDto = new List<Dtos.AccountingCode>();
            List<Dtos.AccountingCode2> accountingCodes2Dto = new List<Dtos.AccountingCode2>();
            int offset = 0;
            int limit = 2;
            string filterGroupName = "criteria";
            private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                accountingCodesServiceMock = new Mock<IAccountingCodesService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                accountingCodesEntities = new TestAccountingCodesRepository().Get();
                foreach (var accountingCodesEntity in accountingCodesEntities)
                {
                    Dtos.AccountingCode accountingCodeDto = new Dtos.AccountingCode();
                    accountingCodeDto.Id = accountingCodesEntity.Guid;
                    accountingCodeDto.Code = accountingCodesEntity.Code;
                    accountingCodeDto.Title = accountingCodesEntity.Description;
                    accountingCodeDto.Description = string.Empty;
                    accountingCodesDto.Add(accountingCodeDto);
                }

                foreach (var accountingCodesEntity in accountingCodesEntities)
                {
                    Dtos.AccountingCode2 accountingCodeDto = new Dtos.AccountingCode2();
                    accountingCodeDto.Id = accountingCodesEntity.Guid;
                    accountingCodeDto.Code = accountingCodesEntity.Code;
                    accountingCodeDto.Title = accountingCodesEntity.Description;
                    accountingCodeDto.Description = string.Empty;
                    accountingCodes2Dto.Add(accountingCodeDto);
                }

                accountingCodesController = new AccountingCodesController(adapterRegistryMock.Object, accountingCodesServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                accountingCodesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());
                accountingCodesController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            [TestCleanup]
            public void Cleanup()
            {
                accountingCodesController = null;
                accountingCodesEntities = null;
                accountingCodesDto = null;
                accountingCodes2Dto = null;
            }

            [TestMethod]
            public async Task AccountingCodesController_GetAll_NoCache_True()
            {
                accountingCodesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodesDto);

                var results = await accountingCodesController.GetAccountingCodesAsync();
                Assert.AreEqual(accountingCodesDto.Count, results.Count());

                foreach (var accountingCodeDto in accountingCodesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountingCodeDto.Id);
                    Assert.AreEqual(result.Id, accountingCodeDto.Id);
                    Assert.AreEqual(result.Code, accountingCodeDto.Code);
                    Assert.AreEqual(result.Title, accountingCodeDto.Title);
                    Assert.AreEqual(result.Description, accountingCodeDto.Description);
                }
            }

            [TestMethod]
            public async Task AccountingCodesController_GetAll_NoCache_False()
            {
                accountingCodesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCodesAsync(It.IsAny<bool>())).ReturnsAsync(accountingCodesDto);

                var results = await accountingCodesController.GetAccountingCodesAsync();
                Assert.AreEqual(accountingCodesDto.Count, results.Count());                
                
                foreach (var accountingCodeDto in accountingCodesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountingCodeDto.Id);
                    Assert.AreEqual(result.Id, accountingCodeDto.Id);
                    Assert.AreEqual(result.Code, accountingCodeDto.Code);
                    Assert.AreEqual(result.Title, accountingCodeDto.Title);
                    Assert.AreEqual(result.Description, accountingCodeDto.Description);
                }

            }

            [TestMethod]
            public async Task AccountingCodesController_GetById()
            {
                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var accountingCodeDto = accountingCodesDto.FirstOrDefault(i => i.Id == id);
                accountingCodesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = It.IsAny<bool>(),
                    Public = true
                };

                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCodeByIdAsync(id)).ReturnsAsync(accountingCodeDto);

                var result = await accountingCodesController.GetAccountingCodeByIdAsync(id);
                Assert.AreEqual(result.Id, accountingCodeDto.Id);
                Assert.AreEqual(result.Code, accountingCodeDto.Code);
                Assert.AreEqual(result.Title, accountingCodeDto.Title);
                Assert.AreEqual(result.Description, accountingCodeDto.Description);
            }

            [TestMethod]
            public async Task AccountingCodesController_GetAllV11_NoCache_True()
            {
                accountingCodesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCodes2Async(It.IsAny<AccountingCodeCategoryDtoProperty>(), It.IsAny<bool>())).ReturnsAsync(accountingCodes2Dto);

                var results = await accountingCodesController.GetAccountingCodes2Async(criteriaFilter);
                Assert.AreEqual(accountingCodesDto.Count, results.Count());

                foreach (var accountingCodeDto in accountingCodesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountingCodeDto.Id);
                    Assert.AreEqual(result.Id, accountingCodeDto.Id);
                    Assert.AreEqual(result.Code, accountingCodeDto.Code);
                    Assert.AreEqual(result.Title, accountingCodeDto.Title);
                    Assert.AreEqual(result.Description, accountingCodeDto.Description);
                }
            }

            [TestMethod]
            public async Task AccountingCodesController_GetAllV11_NoCache_False()
            {
                accountingCodesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCodes2Async(It.IsAny<AccountingCodeCategoryDtoProperty>(), It.IsAny<bool>())).ReturnsAsync(accountingCodes2Dto);

                var results = await accountingCodesController.GetAccountingCodes2Async(criteriaFilter);
                Assert.AreEqual(accountingCodesDto.Count, results.Count());

                foreach (var accountingCodeDto in accountingCodesDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == accountingCodeDto.Id);
                    Assert.AreEqual(result.Id, accountingCodeDto.Id);
                    Assert.AreEqual(result.Code, accountingCodeDto.Code);
                    Assert.AreEqual(result.Title, accountingCodeDto.Title);
                    Assert.AreEqual(result.Description, accountingCodeDto.Description);
                }

            }

            [TestMethod]
            public async Task AccountingCodesController_GetByIdV11()
            {
                string id = "a142d78a-b472-45de-8a4b-953258976a0b";
                var accountingCodeDto = accountingCodes2Dto.FirstOrDefault(i => i.Id == id);
                accountingCodesController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = It.IsAny<bool>(),
                    Public = true
                };

                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCode2ByIdAsync(id, It.IsAny<bool>())).ReturnsAsync(accountingCodeDto);

                var result = await accountingCodesController.GetAccountingCodeById2Async(id);
                Assert.AreEqual(result.Id, accountingCodeDto.Id);
                Assert.AreEqual(result.Code, accountingCodeDto.Code);
                Assert.AreEqual(result.Title, accountingCodeDto.Title);
                Assert.AreEqual(result.Description, accountingCodeDto.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountingCodesController_GetAll_Exception()
            {
                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCodesAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var result = await accountingCodesController.GetAccountingCodesAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountingCodesController_GetById_KeyNotFoundException()
            {
                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCodeByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var result = await accountingCodesController.GetAccountingCodeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountingCodesController_GetById_Exception()
            {
                accountingCodesServiceMock.Setup(ac => ac.GetAccountingCodeByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await accountingCodesController.GetAccountingCodeByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountingCodesController_PUT_Exception()
            {
                var result = await accountingCodesController.PutAccountingCode(It.IsAny<string>(), new Dtos.AccountingCode());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountingCodesController_POST_Exception()
            {
                var result = await accountingCodesController.PostAccountingCode(new Dtos.AccountingCode());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AccountingCodesController_DELETE_Exception()
            {
                await accountingCodesController.DeleteAccountingCode(It.IsAny<string>());
            }
        }
    }
}
