// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Configuration.Licensing;
using System.Web.Http;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Dtos;
using System.Web;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class AccountingStringsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IAccountingStringService> _accountingStringServiceMock;
        private Mock<ILogger> _loggerMock;
        private AccountingStringsController _accountingStringsController;
        private List<Ellucian.Colleague.Dtos.AccountingStringComponent> _accountingStringComponentsCollection;
        private List<Ellucian.Colleague.Dtos.AccountingStringFormats> _accountingStringFormatsCollection;
        private AccountingStringFormats _accountingStringFormat;
        private AccountingString _accountingString;
        private HttpResponse _response;
        private readonly string _guid = "ABC08967-22E7-4D66-BA80-71BB995BCDC5";
        private List<AccountingStringComponentValues> _accountingStringComponentValuesCollection;
        private List<AccountingStringComponentValues2> _accountingStringComponentValuesCollection2;
        private List<AccountingStringComponentValues3> _accountingStringComponentValuesCollection3;

        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");
        private Ellucian.Web.Http.Models.QueryStringFilter effectiveOnFilter = new Web.Http.Models.QueryStringFilter("effectiveOn", "");

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _loggerMock = new Mock<ILogger>();
            _accountingStringServiceMock = new Mock<IAccountingStringService>();

            _accountingStringComponentsCollection = new List<AccountingStringComponent>();

            _accountingStringFormatsCollection = new List<AccountingStringFormats>();
            _accountingStringFormat = new AccountingStringFormats();

            _response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), _response);

            _accountingString = new AccountingString()
            {
                AccountingStringValue = "11-00-01-00-00000-10110",
                Description = "Account11"
            };

            _accountingStringComponentsCollection.Add( new AccountingStringComponent()
                { Code = "c1", Id =  "EBC08967-22E7-4D66-BA80-71BB995BCDC5", Title = "", Description = "Desc1"});
            _accountingStringComponentsCollection.Add(new AccountingStringComponent()
                { Code = "c2", Id = "DDC08967-22E7-4D66-BA80-71BB995BCDC5", Title = "", Description = "Desc2" });

            _accountingStringFormat.Id = "c0695351-f7c0-4e41-8b26-e75adc683cfd";
            _accountingStringFormat.Delimiter = "*";

            _accountingStringFormat.Components = new List<Components>()
            {
                new Components() {Component = new GuidObject2("EBC08967-22E7-4D66-BA80-71BB995BCDC5") , order = 1 },
                new Components() {Component = new GuidObject2(  "DDC08967-22E7-4D66-BA80-71BB995BCDC5" ) , order = 2 }
            };

            _accountingStringFormatsCollection.Add(_accountingStringFormat);



            _accountingStringComponentValuesCollection = new List<AccountingStringComponentValues>();

            var accountingStringComponentValue1 = new AccountingStringComponentValues()
            {
                Component = new GuidObject2("EBC08967-22E7-4D66-BA80-71BB995BCDC5"),
                Description = "Contribution Checking : General",
                DeterminingComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180db"),
                Id = "6f5e7bdb-7998-456c-9436-c77eaca180da",
                ParentComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180dc"),
                RelatedComponentDefaults = new List<GuidObject2>() { new GuidObject2("5414e60c-bc49-487a-88a5-ffc7f6245f3d") },
                TransactionStatus = AccountingTransactionStatus.available,
                Type = new AccountingStringComponentValuesType() { Account = AccountingTypeAccount.asset },
                Value = "11_00_01_00_00000_10110"
            };

            var accountingStringComponentValue2 = new AccountingStringComponentValues()
            {
                Component = new GuidObject2("EBC08967-22E7-4D66-BA80-71BB995BCDC5"),
                Description = "Contribution  Payroll Deduc : General",
                DeterminingComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180db"),
                Id = "a345152c-e909-443f-a93b-0ce089bfdd8a",
                ParentComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180dc"),
                RelatedComponentDefaults = new List<GuidObject2>() { new GuidObject2("5414e60c-bc49-487a-88a5-ffc7f6245f3d") },
                TransactionStatus = AccountingTransactionStatus.unavailable,
                Type = new AccountingStringComponentValuesType() { Account = AccountingTypeAccount.expense },
                Value = "11_00_01_00_00000_10113"
            };
            _accountingStringComponentValuesCollection.Add(accountingStringComponentValue1);
            _accountingStringComponentValuesCollection.Add(accountingStringComponentValue2);

            _accountingStringComponentValuesCollection2 = new List<AccountingStringComponentValues2>()
            {
                new AccountingStringComponentValues2()
                {
                    Component = new GuidObject2("EBC08967-22E7-4D66-BA80-71BB995BCDC5"),
                    Description = "Contribution Checking : General",
                    DeterminingComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180db"),
                    Id = "6f5e7bdb-7998-456c-9436-c77eaca180da",
                    ParentComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180dc"),
                    RelatedComponentDefaults = new List<GuidObject2>() { new GuidObject2("5414e60c-bc49-487a-88a5-ffc7f6245f3d") },
                    TransactionStatus = AccountingTransactionStatus.available,
                    Type = new AccountingStringComponentValuesType() { Account = AccountingTypeAccount.asset },
                    Value = "11_00_01_00_00000_10110",
                    BudgetPools = new List<Dtos.DtoProperties.BudgetPool>()
                    {
                        new Dtos.DtoProperties.BudgetPool()
                        {
                            FiscalYear = new GuidObject2("1f5e7bdb-7998-456c-9436-c77eaca180dz"),
                            AccountingComponent = new GuidObject2("2f5e7bdb-7998-456c-9436-c77eaca180dy")
                        }
                    }
                },
                new AccountingStringComponentValues2()
                {
                    Component = new GuidObject2("EBC08967-22E7-4D66-BA80-71BB995BCDC5"),
                    Description = "Contribution  Payroll Deduc : General",
                    DeterminingComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180db"),
                    Id = "a345152c-e909-443f-a93b-0ce089bfdd8a",
                    ParentComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180dc"),
                    RelatedComponentDefaults = new List<GuidObject2>() { new GuidObject2("5414e60c-bc49-487a-88a5-ffc7f6245f3d") },
                    TransactionStatus = AccountingTransactionStatus.unavailable,
                    Type = new AccountingStringComponentValuesType() { Account = AccountingTypeAccount.expense },
                    Value = "11_00_01_00_00000_10113"
                }
            };

            _accountingStringComponentValuesCollection3 = new List<AccountingStringComponentValues3>()
            {
                new AccountingStringComponentValues3()
                {
                    Component = new GuidObject2("EBC08967-22E7-4D66-BA80-71BB995BCDC5"),
                    Description = "Contribution Checking : General",
                    DeterminingComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180db"),
                    Id = "6f5e7bdb-7998-456c-9436-c77eaca180da",
                    ParentComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180dc"),
                    RelatedComponentDefaults = new List<GuidObject2>() { new GuidObject2("5414e60c-bc49-487a-88a5-ffc7f6245f3d") },
                    TransactionStatus = AccountingTransactionStatus.available,
                    Type = new AccountingStringComponentValuesType() { Account = AccountingTypeAccount.asset },
                    Value = "11_00_01_00_00000_10110",
                    BudgetPools = new List<Dtos.DtoProperties.BudgetPool>()
                    {
                        new Dtos.DtoProperties.BudgetPool()
                        {
                            FiscalYear = new GuidObject2("1f5e7bdb-7998-456c-9436-c77eaca180dz"),
                            AccountingComponent = new GuidObject2("2f5e7bdb-7998-456c-9436-c77eaca180dy")
                        }
                    }
                },
                new AccountingStringComponentValues3()
                {
                    Component = new GuidObject2("EBC08967-22E7-4D66-BA80-71BB995BCDC5"),
                    Description = "Contribution  Payroll Deduc : General",
                    DeterminingComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180db"),
                    Id = "a345152c-e909-443f-a93b-0ce089bfdd8a",
                    ParentComponent = new GuidObject2("6f5e7bdb-7998-456c-9436-c77eaca180dc"),
                    RelatedComponentDefaults = new List<GuidObject2>() { new GuidObject2("5414e60c-bc49-487a-88a5-ffc7f6245f3d") },
                    TransactionStatus = AccountingTransactionStatus.unavailable,
                    Type = new AccountingStringComponentValuesType() { Account = AccountingTypeAccount.expense },
                    Value = "11_00_01_00_00000_10113"
                }
            };

            _accountingStringServiceMock.Setup(x => x.GetAccoutingStringByFilterCriteriaAsync("test", null)).ReturnsAsync(_accountingString);
            _accountingStringsController = new AccountingStringsController(_accountingStringServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _accountingStringsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _accountingStringServiceMock = null;
            _accountingStringsController = null;
        }

        #region Accounting Strings

        [TestMethod]
        public async Task AccountingStringsController_GetAccoutingStringByFilterCriteriaAsync_Valid()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccoutingStringByFilterCriteriaAsync("test", null)).ReturnsAsync(_accountingString);
            var accountString = await _accountingStringsController.GetAccountingStringByFilterAsync("test", null);
            Assert.IsNotNull(accountString);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringsController_GetAccoutingStringByFilterCriteriaAsync_PermissionsException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccoutingStringByFilterCriteriaAsync("test", null)).ThrowsAsync(new PermissionsException());
            await _accountingStringsController.GetAccountingStringByFilterAsync("test", null);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringsController_GetAccoutingStringByFilterCriteriaAsync_RepositoryException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccoutingStringByFilterCriteriaAsync("test", null)).ThrowsAsync(new RepositoryException());
            await _accountingStringsController.GetAccountingStringByFilterAsync("test", null);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringsController_GetAccoutingStringByFilterCriteriaAsync_ArgumentException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccoutingStringByFilterCriteriaAsync("test", null)).ThrowsAsync(new ArgumentException());
            await _accountingStringsController.GetAccountingStringByFilterAsync("test", null);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringsController_GetAccoutingStringByFilterCriteriaAsync_IntegrationApiException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccoutingStringByFilterCriteriaAsync("test", null)).ThrowsAsync(new IntegrationApiException());
            await _accountingStringsController.GetAccountingStringByFilterAsync("test", null);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringsController_GetAccoutingStringByFilterCriteriaAsync_Exception()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccoutingStringByFilterCriteriaAsync("test", null)).ThrowsAsync(new Exception());
            await _accountingStringsController.GetAccountingStringByFilterAsync("test", null);
        }

        #endregion

        #region Accounting String Components

        [TestMethod]
        public async Task AccountingStringComponentsController_GetAccountingStringComponents_ValidateFields_Nocache()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = false};

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsAsync(false)).ReturnsAsync(_accountingStringComponentsCollection);

            var sourceContexts = (await _accountingStringsController.GetAccountingStringComponentsAsync()).ToList();
            Assert.AreEqual(_accountingStringComponentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = _accountingStringComponentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AccountingStringComponentsController_GetAccountingStringComponents_ValidateFields_Cache()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue {NoCache = true};

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsAsync(true)).ReturnsAsync(_accountingStringComponentsCollection);

            var sourceContexts = (await _accountingStringsController.GetAccountingStringComponentsAsync()).ToList();
            Assert.AreEqual(_accountingStringComponentsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = _accountingStringComponentsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Title, actual.Title, "Title, Index=" + i.ToString());
                Assert.AreEqual(expected.Code, actual.Code, "Code, Index=" + i.ToString());
            }
        }

        [TestMethod]
        public async Task AccountingStringComponentsController_GetAccountingStringComponentsByGuidAsync_ValidateFields()
        {
            var expected = _accountingStringComponentsCollection.FirstOrDefault();
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await _accountingStringsController.GetAccountingStringComponentsByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id, "Id");
            Assert.AreEqual(expected.Title, actual.Title, "Title");
            Assert.AreEqual(expected.Code, actual.Code, "Code");
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponents_Exception()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsAsync(false)).Throws<Exception>();
            await _accountingStringsController.GetAccountingStringComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponents_KeyNotFoundException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsAsync(false)).Throws<KeyNotFoundException>();
            await _accountingStringsController.GetAccountingStringComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponents_PermissionsException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsAsync(false)).Throws<PermissionsException>();
            await _accountingStringsController.GetAccountingStringComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponents_ArgumentException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsAsync(false)).Throws<ArgumentException>();
            await _accountingStringsController.GetAccountingStringComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponents_RepositoryException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsAsync(false)).Throws<RepositoryException>();
            await _accountingStringsController.GetAccountingStringComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponents_IntegrationApiException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsAsync(false)).Throws<IntegrationApiException>();
            await _accountingStringsController.GetAccountingStringComponentsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponentsByGuidAsync_Exception()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await _accountingStringsController.GetAccountingStringComponentsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponentsByGuidAsync_KeyNotFoundException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await _accountingStringsController.GetAccountingStringComponentsByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponentsByGuidAsync_PermissionsException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await _accountingStringsController.GetAccountingStringComponentsByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponentsByGuidAsync_ArgumentException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await _accountingStringsController.GetAccountingStringComponentsByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponentsByGuidAsync_RepositoryException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await _accountingStringsController.GetAccountingStringComponentsByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponentsByGuidAsync_IntegrationApiException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await _accountingStringsController.GetAccountingStringComponentsByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_GetAccountingStringComponentsByGuidAsync_WithGuidException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await _accountingStringsController.GetAccountingStringComponentsByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringComponentsController_PostAccountingStringComponentsAsync_Exception()
        {
            await _accountingStringsController.PostAccountingStringComponentsAsync(_accountingStringComponentsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringComponentsController_PutAccountingStringComponentsAsync_Exception()
        {
            var sourceContext = _accountingStringComponentsCollection.FirstOrDefault();
            Assert.IsNotNull(sourceContext);
            await _accountingStringsController.PutAccountingStringComponentsAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task AccountingStringComponentsController_DeleteAccountingStringComponentsAsync_Exception()
        {
            var sourceContext = _accountingStringComponentsCollection.FirstOrDefault();
            Assert.IsNotNull(sourceContext);
            await _accountingStringsController.DeleteAccountingStringComponentsAsync(sourceContext.Id);
        }
        #endregion

        #region Accounting String Formats

        [TestMethod]
        public async Task AccountingStringsController_GetAccountingStringFormats_ValidateFields_Nocache()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringFormatsAsync(false)).ReturnsAsync(_accountingStringFormatsCollection);

            var sourceContexts = (await _accountingStringsController.GetAccountingStringFormatsAsync()).ToList();
            Assert.AreEqual(_accountingStringFormatsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = _accountingStringFormatsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Delimiter, actual.Delimiter, "Delimiter, Index=" + i.ToString());
                Assert.AreEqual(expected.Components.Count, actual.Components.Count);
                for (int x = 0; x < actual.Components.Count; x++)
                { 
                    Assert.AreEqual(expected.Components[x].Component.Id, actual.Components[x].Component.Id);
                    Assert.AreEqual(expected.Components[x].order, actual.Components[x].order);
                }
            }
        }

        [TestMethod]
        public async Task AccountingStringsController_GetAccountingStringFormats_ValidateFields_Cache()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringFormatsAsync(true)).ReturnsAsync(_accountingStringFormatsCollection);

            var sourceContexts = (await _accountingStringsController.GetAccountingStringFormatsAsync()).ToList();
            Assert.AreEqual(_accountingStringFormatsCollection.Count, sourceContexts.Count);
            for (var i = 0; i < sourceContexts.Count; i++)
            {
                var expected = _accountingStringFormatsCollection[i];
                var actual = sourceContexts[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                Assert.AreEqual(expected.Delimiter, actual.Delimiter, "Delimiter, Index=" + i.ToString());
                Assert.AreEqual(expected.Components.Count, actual.Components.Count);
                for (int x = 0; x < actual.Components.Count; x++)
                {
                    Assert.AreEqual(expected.Components[x].Component.Id, actual.Components[x].Component.Id);
                    Assert.AreEqual(expected.Components[x].order, actual.Components[x].order);
                }
            }
        }

        [TestMethod]
        public async Task AccountingStringsController_GetAccountingStringFormatsByGuidAsync_ValidateFields()
        {
            var expected = _accountingStringFormat;
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringFormatsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await _accountingStringsController.GetAccountingStringFormatsByGuidAsync(expected.Id);
            
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Delimiter, actual.Delimiter);
            Assert.AreEqual(expected.Components.Count, actual.Components.Count);
            for (int x = 0; x < actual.Components.Count; x++)
            {
                Assert.AreEqual(expected.Components[x].Component.Id, actual.Components[x].Component.Id);
                Assert.AreEqual(expected.Components[x].order, actual.Components[x].order);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringsController_GetAccountingStringFormats_Exception()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringFormatsAsync(false)).Throws<Exception>();
            await _accountingStringsController.GetAccountingStringFormatsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringsController_GetAccountingStringFormatsByGuidAsync_Exception()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringFormatsByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await _accountingStringsController.GetAccountingStringFormatsByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringsController_PostAccountingStringFormatsAsync_Exception()
        {
            await _accountingStringsController.PostAccountingStringFormatsAsync(_accountingStringFormatsCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringsController_PutAccountingStringFormatsAsync_Exception()
        {
            var sourceContext = _accountingStringFormatsCollection.FirstOrDefault();
            await _accountingStringsController.PutAccountingStringFormatsAsync(_accountingStringFormat.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringsController_DeleteAccountingStringFormatsAsync_Exception()
        {
            await _accountingStringsController.DeleteAccountingStringFormatsAsync(_accountingStringFormat.Id);
        }

        #endregion

        #region Accounting String Component Values

        [TestMethod]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues_ValidateFields()
        {
            _accountingStringsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };        

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(_accountingStringComponentValuesCollection, 1);
            var criteria = ""; 
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);


            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValuesAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await accountingStringComponentValues.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.AccountingStringComponentValues>;

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = _accountingStringComponentValuesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                if (expected.Component != null)
                {
                    Assert.AreEqual(expected.Component.Id, actual.Component.Id);
                }
                Assert.AreEqual(expected.Description, actual.Description);
                if (expected.DeterminingComponent != null)
                {
                    Assert.AreEqual(expected.DeterminingComponent.Id, actual.DeterminingComponent.Id);
                }
                if (expected.ParentComponent != null)
                {
                    Assert.AreEqual(expected.ParentComponent.Id, actual.ParentComponent.Id);
                }
                if (expected.RelatedComponentDefaults != null && expected.RelatedComponentDefaults.Any())
                {
                    Assert.IsNotNull(expected.RelatedComponentDefaults.FirstOrDefault());
                    Assert.IsNotNull(actual.RelatedComponentDefaults.FirstOrDefault());
                    Assert.AreEqual(expected.RelatedComponentDefaults.FirstOrDefault().Id,
                        actual.RelatedComponentDefaults.FirstOrDefault().Id);
                }
                Assert.AreEqual(expected.TransactionStatus, actual.TransactionStatus);
                if (expected.Type != null)
                {
                    Assert.AreEqual(expected.Type.Account, actual.Type.Account);
                }
                Assert.AreEqual(expected.Value, actual.Value);
            }
        }        

        [TestMethod]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues_Filters()
        {
            var filterGroupName = "criteria";
            _accountingStringsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(_accountingStringComponentValuesCollection, 1);
            //var criteria = "{\"component\":\"EBC08967-22E7-4D66-BA80-71BB995BCDC5\", \"transactionstatus\":\"available\", \"typeAccount\":\"asset\", \"typeFund\":\"test\"}";
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);
            _accountingStringsController.Request.Properties.Add(
              string.Format("FilterObject{0}", filterGroupName),
              new Dtos.AccountingStringComponentValues() {
                  Component = new GuidObject2("EBC08967-22E7-4D66-BA80-71BB995BCDC5"),
                  TransactionStatus = AccountingTransactionStatus.available,
                  Type = new AccountingStringComponentValuesType()
                  {
                      Account = AccountingTypeAccount.asset,
                      Fund = "fund"
                  }
               });


            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValuesAsync(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await accountingStringComponentValues.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.AccountingStringComponentValues>;

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = _accountingStringComponentValuesCollection.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                if (expected.Component != null)
                {
                    Assert.AreEqual(expected.Component.Id, actual.Component.Id);
                }
                Assert.AreEqual(expected.Description, actual.Description);
                if (expected.DeterminingComponent != null)
                {
                    Assert.AreEqual(expected.DeterminingComponent.Id, actual.DeterminingComponent.Id);
                }
                if (expected.ParentComponent != null)
                {
                    Assert.AreEqual(expected.ParentComponent.Id, actual.ParentComponent.Id);
                }
                if (expected.RelatedComponentDefaults != null && expected.RelatedComponentDefaults.Any())
                {
                    Assert.IsNotNull(expected.RelatedComponentDefaults.FirstOrDefault());
                    Assert.IsNotNull(actual.RelatedComponentDefaults.FirstOrDefault());
                    Assert.AreEqual(expected.RelatedComponentDefaults.FirstOrDefault().Id,
                        actual.RelatedComponentDefaults.FirstOrDefault().Id);
                }
                Assert.AreEqual(expected.TransactionStatus, actual.TransactionStatus);
                if (expected.Type != null)
                {
                    Assert.AreEqual(expected.Type.Account, actual.Type.Account);
                }
                Assert.AreEqual(expected.Value, actual.Value);
            }
        }

        [TestMethod]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValuesByGuidAsync_ValidateFields()
        {
            var expected = _accountingStringComponentValuesCollection.FirstOrDefault();
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesByGuidAsync(expected.Id)).ReturnsAsync(expected);
            Assert.IsNotNull(expected);

            var actual = await _accountingStringsController.GetAccountingStringComponentValuesByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            if (expected.Component != null)
            {
                Assert.AreEqual(expected.Component.Id, actual.Component.Id);
            }
            Assert.AreEqual(expected.Description, actual.Description);
            if (expected.DeterminingComponent != null)
            {
                Assert.AreEqual(expected.DeterminingComponent.Id, actual.DeterminingComponent.Id);
            }
            if (expected.ParentComponent != null)
            {
                Assert.AreEqual(expected.ParentComponent.Id, actual.ParentComponent.Id);
            }
            if (expected.RelatedComponentDefaults != null && expected.RelatedComponentDefaults.Any())
            {
                Assert.IsNotNull(expected.RelatedComponentDefaults.FirstOrDefault());
                Assert.IsNotNull(actual.RelatedComponentDefaults.FirstOrDefault());
                Assert.AreEqual(expected.RelatedComponentDefaults.FirstOrDefault().Id,
                    actual.RelatedComponentDefaults.FirstOrDefault().Id);
            }
            Assert.AreEqual(expected.TransactionStatus, actual.TransactionStatus);
            if (expected.Type != null)
            {
                Assert.AreEqual(expected.Type.Account, actual.Type.Account);
            }
            Assert.AreEqual(expected.Value, actual.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues_Exception()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(_accountingStringComponentValuesCollection, 1);
           
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();

            await _accountingStringsController.GetAccountingStringComponentValuesAsync(new Paging(10, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues_KeyNotFoundException()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(_accountingStringComponentValuesCollection, 1);
           
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
            await _accountingStringsController.GetAccountingStringComponentValuesAsync(new Paging(10, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues_PermissionsException()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(_accountingStringComponentValuesCollection, 1);

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await _accountingStringsController.GetAccountingStringComponentValuesAsync(new Paging(10, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues_ArgumentException()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(_accountingStringComponentValuesCollection, 1);

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await _accountingStringsController.GetAccountingStringComponentValuesAsync(new Paging(10, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues_RepositoryException()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(_accountingStringComponentValuesCollection, 1);

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await _accountingStringsController.GetAccountingStringComponentValuesAsync(new Paging(10, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues_IntegrationApiException()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues>, int>(_accountingStringComponentValuesCollection, 1);

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesAsync(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await _accountingStringsController.GetAccountingStringComponentValuesAsync(new Paging(10, 0), criteriaFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValuesByGuidAsync_Exception()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await _accountingStringsController.GetAccountingStringComponentValuesByGuidAsync(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValuesByGuidAsync_KeyNotFoundException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesByGuidAsync(It.IsAny<string>())).Throws<KeyNotFoundException>();
            await _accountingStringsController.GetAccountingStringComponentValuesByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValuesByGuidAsync_PermissionsException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesByGuidAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await _accountingStringsController.GetAccountingStringComponentValuesByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValuesByGuidAsync_ArgumentException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesByGuidAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await _accountingStringsController.GetAccountingStringComponentValuesByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValuesByGuidAsync_RepositoryException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesByGuidAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await _accountingStringsController.GetAccountingStringComponentValuesByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValuesByGuidAsync_IntegrationApiException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesByGuidAsync(It.IsAny<string>())).Throws<IntegrationApiException>();
            await _accountingStringsController.GetAccountingStringComponentValuesByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValuesByGuidAsync_WithGuidException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValuesByGuidAsync(It.IsAny<string>())).Throws<Exception>();
            await _accountingStringsController.GetAccountingStringComponentValuesByGuidAsync(_guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_PostAccountingStringComponentValuesAsync_Exception()
        {
            await _accountingStringsController.PostAccountingStringComponentValuesAsync(_accountingStringComponentValuesCollection.FirstOrDefault());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_PutAccountingStringComponentValuesAsync_Exception()
        {
            var sourceContext = _accountingStringComponentValuesCollection.FirstOrDefault();
            Assert.IsNotNull(sourceContext);
            await _accountingStringsController.PutAccountingStringComponentValuesAsync(sourceContext.Id, sourceContext);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_DeleteAccountingStringComponentValuesAsync_Exception()
        {
            var sourceContext = _accountingStringComponentValuesCollection.FirstOrDefault();
            Assert.IsNotNull(sourceContext);
            await _accountingStringsController.DeleteAccountingStringComponentValuesAsync(sourceContext.Id);
        }

        [TestMethod]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2_ValidateFields()
        {
            _accountingStringsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues2>, int>(_accountingStringComponentValuesCollection2, 1);
            var criteria = @"{'component':{'id':'c6f10097-ad8e-4ece-9276-9e13de0983f8'}, 'transactionStatus':'available', 'type':{ 'account': 'revenue'}}";
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues2Async(It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(tuple);


            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues2Async(new Paging(10, 0), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await accountingStringComponentValues.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.AccountingStringComponentValues2>;

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = _accountingStringComponentValuesCollection2.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                if (expected.Component != null)
                {
                    Assert.AreEqual(expected.Component.Id, actual.Component.Id);
                }
                Assert.AreEqual(expected.Description, actual.Description);
                if (expected.DeterminingComponent != null)
                {
                    Assert.AreEqual(expected.DeterminingComponent.Id, actual.DeterminingComponent.Id);
                }
                if (expected.ParentComponent != null)
                {
                    Assert.AreEqual(expected.ParentComponent.Id, actual.ParentComponent.Id);
                }
                if (expected.RelatedComponentDefaults != null && expected.RelatedComponentDefaults.Any())
                {
                    Assert.IsNotNull(expected.RelatedComponentDefaults.FirstOrDefault());
                    Assert.IsNotNull(actual.RelatedComponentDefaults.FirstOrDefault());
                    Assert.AreEqual(expected.RelatedComponentDefaults.FirstOrDefault().Id,
                        actual.RelatedComponentDefaults.FirstOrDefault().Id);
                }
                Assert.AreEqual(expected.TransactionStatus, actual.TransactionStatus);
                if (expected.Type != null)
                {
                    Assert.AreEqual(expected.Type.Account, actual.Type.Account);
                }
                Assert.AreEqual(expected.Value, actual.Value);
                if (actual.BudgetPools != null && actual.BudgetPools.Any())
                {
                    Assert.AreEqual(actual.BudgetPools.Count(), 1);
                }
            }
        }

        [TestMethod]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues3_ValidateFields()
        {
            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var tuple = new Tuple<IEnumerable<Dtos.AccountingStringComponentValues3>, int>(_accountingStringComponentValuesCollection3, 2);
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny< Dtos.AccountingStringComponentValues3>(),
                default(DateTime?), It.IsAny<bool>())).ReturnsAsync(tuple);


            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues3Async(It.IsAny<Paging>(), effectiveOnFilter, criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);

            HttpResponseMessage httpResponseMessage = await accountingStringComponentValues.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.AccountingStringComponentValues3>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.AccountingStringComponentValues3>;

            Assert.IsNotNull(actuals);

            foreach (var actual in actuals)
            {
                var expected = _accountingStringComponentValuesCollection3.FirstOrDefault(i => i.Id.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));

                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Id, actual.Id);
                if (expected.Component != null)
                {
                    Assert.AreEqual(expected.Component.Id, actual.Component.Id);
                }
                Assert.AreEqual(expected.Description, actual.Description);
                if (expected.DeterminingComponent != null)
                {
                    Assert.AreEqual(expected.DeterminingComponent.Id, actual.DeterminingComponent.Id);
                }
                if (expected.ParentComponent != null)
                {
                    Assert.AreEqual(expected.ParentComponent.Id, actual.ParentComponent.Id);
                }
                if (expected.RelatedComponentDefaults != null && expected.RelatedComponentDefaults.Any())
                {
                    Assert.IsNotNull(expected.RelatedComponentDefaults.FirstOrDefault());
                    Assert.IsNotNull(actual.RelatedComponentDefaults.FirstOrDefault());
                    Assert.AreEqual(expected.RelatedComponentDefaults.FirstOrDefault().Id,
                        actual.RelatedComponentDefaults.FirstOrDefault().Id);
                }
                Assert.AreEqual(expected.TransactionStatus, actual.TransactionStatus);
                if (expected.Type != null)
                {
                    Assert.AreEqual(expected.Type.Account, actual.Type.Account);
                }
                Assert.AreEqual(expected.Value, actual.Value);
                if (actual.BudgetPools != null && actual.BudgetPools.Any())
                {
                    Assert.AreEqual(actual.BudgetPools.Count(), 1);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2Async_KeyNotFoundException()
        {
            _accountingStringServiceMock.Setup(repo => repo.GetAccountingStringComponentValues2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues2Async(new Paging(10, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2Async_PermissionsException()
        {
            _accountingStringServiceMock.Setup(repo => repo.GetAccountingStringComponentValues2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues2Async(new Paging(10, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2Async_ArgumentException()
        {
            _accountingStringServiceMock.Setup(repo => repo.GetAccountingStringComponentValues2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues2Async(new Paging(10, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2Async_RepositoryException()
        {
            _accountingStringServiceMock.Setup(repo => repo.GetAccountingStringComponentValues2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues2Async(new Paging(10, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2Async_IntegrationApiException()
        {
            _accountingStringServiceMock.Setup(repo => repo.GetAccountingStringComponentValues2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues2Async(new Paging(10, 0), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2Async_Exception()
        {
            _accountingStringServiceMock.Setup(repo => repo.GetAccountingStringComponentValues2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues2Async(new Paging(10, 0), It.IsAny<QueryStringFilter>());
        }

        //v15

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues3Async_KeyNotFoundException()
        {
            _accountingStringsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _accountingStringServiceMock.Setup(i => i.GetAccountingStringComponentValues3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.AccountingStringComponentValues3>(), default(DateTime?), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues3Async(new Paging(10, 0), null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3Async_PermissionsException()
        {
            _accountingStringServiceMock.Setup(i => i.GetAccountingStringComponentValues3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.AccountingStringComponentValues3>(), default(DateTime?), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues3Async(It.IsAny<Paging>(), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3Async_ArgumentException()
        {
            _accountingStringServiceMock.Setup(i => i.GetAccountingStringComponentValues3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.AccountingStringComponentValues3>(), default(DateTime?), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues3Async(new Paging(10, 0), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3Async_RepositoryException()
        {
            _accountingStringServiceMock.Setup(i => i.GetAccountingStringComponentValues3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.AccountingStringComponentValues3>(), default(DateTime?), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues3Async(new Paging(10, 0), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3Async_IntegrationApiException()
        {
            _accountingStringServiceMock.Setup(i => i.GetAccountingStringComponentValues3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.AccountingStringComponentValues3>(), default(DateTime?), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues3Async(new Paging(10, 0), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3Async_Exception()
        {
            _accountingStringServiceMock.Setup(i => i.GetAccountingStringComponentValues3Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Dtos.AccountingStringComponentValues3>(), default(DateTime?), It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            var accountingStringComponentValues = await _accountingStringsController.GetAccountingStringComponentValues3Async(new Paging(10, 0), It.IsAny<QueryStringFilter>(), It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2ByGuidAsync_ValidateFields()
        {
            var expected = _accountingStringComponentValuesCollection2.FirstOrDefault();
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues2ByGuidAsync(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);
            Assert.IsNotNull(expected);

            var actual = await _accountingStringsController.GetAccountingStringComponentValues2ByGuidAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            if (expected.Component != null)
            {
                Assert.AreEqual(expected.Component.Id, actual.Component.Id);
            }
            Assert.AreEqual(expected.Description, actual.Description);
            if (expected.DeterminingComponent != null)
            {
                Assert.AreEqual(expected.DeterminingComponent.Id, actual.DeterminingComponent.Id);
            }
            if (expected.ParentComponent != null)
            {
                Assert.AreEqual(expected.ParentComponent.Id, actual.ParentComponent.Id);
            }
            if (expected.RelatedComponentDefaults != null && expected.RelatedComponentDefaults.Any())
            {
                Assert.IsNotNull(expected.RelatedComponentDefaults.FirstOrDefault());
                Assert.IsNotNull(actual.RelatedComponentDefaults.FirstOrDefault());
                Assert.AreEqual(expected.RelatedComponentDefaults.FirstOrDefault().Id,
                    actual.RelatedComponentDefaults.FirstOrDefault().Id);
            }
            Assert.AreEqual(expected.TransactionStatus, actual.TransactionStatus);
            if (expected.Type != null)
            {
                Assert.AreEqual(expected.Type.Account, actual.Type.Account);
            }
            Assert.AreEqual(expected.Value, actual.Value);
            if (actual.BudgetPools != null && actual.BudgetPools.Any())
            {
                Assert.AreEqual(actual.BudgetPools.Count(), 1);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2ByGuidAsync_KeyNotFoundException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues2ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            await _accountingStringsController.GetAccountingStringComponentValues2ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2ByGuidAsync_PermissionsException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues2ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            await _accountingStringsController.GetAccountingStringComponentValues2ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2ByGuidAsync_ArgumentException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues2ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            await _accountingStringsController.GetAccountingStringComponentValues2ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2ByGuidAsync_RepositoryException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues2ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            await _accountingStringsController.GetAccountingStringComponentValues2ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2ByGuidAsync_IntegrationApiException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues2ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            await _accountingStringsController.GetAccountingStringComponentValues2ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_GetAccountingStringComponentValues2ByGuidAsync_Exception()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues2ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            await _accountingStringsController.GetAccountingStringComponentValues2ByGuidAsync("123");
        }

        //V15 get by guid
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3ByGuidAsync_NoGuid()
        {
            await _accountingStringsController.GetAccountingStringComponentValues3ByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3ByGuidAsync_KeyNotFoundException()
        {
            _accountingStringsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _accountingStringsController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues3ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            await _accountingStringsController.GetAccountingStringComponentValues3ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3ByGuidAsync_PermissionsException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues3ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            await _accountingStringsController.GetAccountingStringComponentValues3ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3ByGuidAsync_ArgumentException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues3ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            await _accountingStringsController.GetAccountingStringComponentValues3ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3ByGuidAsync_RepositoryException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues3ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            await _accountingStringsController.GetAccountingStringComponentValues3ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3ByGuidAsync_IntegrationApiException()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues3ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            await _accountingStringsController.GetAccountingStringComponentValues3ByGuidAsync("123");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AscvController_GetAccountingStringComponentValues3ByGuidAsync_Exception()
        {
            _accountingStringServiceMock.Setup(x => x.GetAccountingStringComponentValues3ByGuidAsync("123", It.IsAny<bool>()))
                .ThrowsAsync(new Exception());
            await _accountingStringsController.GetAccountingStringComponentValues3ByGuidAsync("123");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentValuesController_PostAccountingStringComponentValues2Async_Exception()
        {
            await _accountingStringsController.PostAccountingStringComponentValues2Async(It.IsAny<Dtos.AccountingStringComponentValues2>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountingStringComponentsController_PutAccountingStringComponents2Async_Exception()
        {
            var sourceContext = _accountingStringComponentsCollection.FirstOrDefault();
            Assert.IsNotNull(sourceContext);
            await _accountingStringsController.PutAccountingStringComponentValues2Async(It.IsAny<string>(), It.IsAny<Dtos.AccountingStringComponentValues2>());
        }

        #endregion
    }
}