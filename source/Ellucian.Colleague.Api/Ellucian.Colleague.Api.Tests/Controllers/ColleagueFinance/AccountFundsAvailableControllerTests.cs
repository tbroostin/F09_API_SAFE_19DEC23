// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Http.Models;
using System.Net.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class AccountFundsAvailableControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        private Mock<IAccountFundsAvailableService> _accountFundsAvailableServiceMock;
        private Mock<ILogger> _loggerMock;
        private AccountFundsAvailableController _accountFundsAvailableController;

        private AccountFundsAvailable _accountFundsAvailable;
        private Dtos.AccountFundsAvailable_Transactions acctFundsAvailabaleTransDto;
        private Dtos.AccountFundsAvailable_Transactions2 acctFundsAvailabaleTrans2Dto;
        private HttpResponse _response;

        private string accountingString = "11-00-01-00-00000-10110";
        private DateTime? balanceOn = new DateTime(2016, 12, 31);
        private decimal amount = 2000m;
        private string submittedByGuid = "02dc2629-e8a7-410e-b4df-572d02822f8b";
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");
        private QueryStringFilter accountSpecificationFilter = new QueryStringFilter("accountSpecification", "");
        private Dtos.Filters.AccountFundsAvailableFilter criteria = new Dtos.Filters.AccountFundsAvailableFilter();

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _loggerMock = new Mock<ILogger>();
            _accountFundsAvailableServiceMock = new Mock<IAccountFundsAvailableService>();

            _response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), _response);

            BuildData();

            _accountFundsAvailableController = new AccountFundsAvailableController(_accountFundsAvailableServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {
            _accountFundsAvailable = new AccountFundsAvailable()
            {
                AccountingStringValue = accountingString,
                BalanceOn = balanceOn,
                FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available
            };
            acctFundsAvailabaleTransDto = new AccountFundsAvailable_Transactions()
            {
                Transactions = new List<AccountFundsAvailable_Transactionstransactions>() 
                {
                    new AccountFundsAvailable_Transactionstransactions()
                    {
                        TransactionDate = DateTime.Today.AddDays(-10),
                        TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines>()
                        {
                           new AccountFundsAvailable_TransactionstransactionDetailLines()
                           {
                               AccountingString = "11_00_01_00_00000_10110",
                               Amount = new Dtos.DtoProperties.AmountDtoProperty()
                               {
                                   Currency = CurrencyCodes.USD,
                                   Value = 100
                               },
                               FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available,
                               SubmittedBy = new GuidObject2("2c0ee889-2db9-4625-a1ca-7ad307152d60"),
                               Type = CreditOrDebit.Credit
                           }
                        }
                    },
                     new AccountFundsAvailable_Transactionstransactions()
                    {
                        TransactionDate = DateTime.Today.AddDays(-10),
                        TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines>()
                        {
                           new AccountFundsAvailable_TransactionstransactionDetailLines()
                           {
                               AccountingString = "11_00_01_00_00000_10220",
                               Amount = new Dtos.DtoProperties.AmountDtoProperty()
                               {
                                   Currency = CurrencyCodes.CAD,
                                   Value = 50
                               },
                               FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available,
                               SubmittedBy = new GuidObject2("a386bf6a-25c2-4d5c-932e-bb4513b25cd9"),
                               Type = CreditOrDebit.Debit
                           }
                        }
                    }
                }
            };

            acctFundsAvailabaleTrans2Dto = new AccountFundsAvailable_Transactions2()
            {
                Transactions = new List<AccountFundsAvailable_Transactionstransactions2>()
                {
                    new AccountFundsAvailable_Transactionstransactions2()
                    {
                        TransactionDate = DateTime.Today.AddDays(-10),
                        TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines2>()
                        {
                           new AccountFundsAvailable_TransactionstransactionDetailLines2()
                           {
                               AccountingString = "11_00_01_00_00000_10110",
                               Amount = new Dtos.DtoProperties.AmountDtoProperty()
                               {
                                   Currency = CurrencyCodes.USD,
                                   Value = 100
                               },
                               FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available,
                               SubmittedBy = new GuidObject2("2c0ee889-2db9-4625-a1ca-7ad307152d60"),
                               Type = CreditOrDebit.Credit,
                               ReferenceDocument = new Dtos.ReferenceDocumentDtoProperty()
                               {
                                   ItemNumber = "123"
                               }
                           }
                        }
                    },
                     new AccountFundsAvailable_Transactionstransactions2()
                    {
                        TransactionDate = DateTime.Today.AddDays(-10),
                        TransactionDetailLines = new List<AccountFundsAvailable_TransactionstransactionDetailLines2>()
                        {
                           new AccountFundsAvailable_TransactionstransactionDetailLines2()
                           {
                               AccountingString = "11_00_01_00_00000_10220",
                               Amount = new Dtos.DtoProperties.AmountDtoProperty()
                               {
                                   Currency = CurrencyCodes.CAD,
                                   Value = 50
                               },
                               FundsAvailable = Dtos.EnumProperties.FundsAvailable.Available,
                               SubmittedBy = new GuidObject2("a386bf6a-25c2-4d5c-932e-bb4513b25cd9"),
                               Type = CreditOrDebit.Debit,
                               ReferenceDocument = new Dtos.ReferenceDocumentDtoProperty()
                               {
                                   ItemNumber = "456"
                               }
                           }
                        }
                    }
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _loggerMock = null;
            _accountFundsAvailableServiceMock = null;
            _accountFundsAvailableController = null;
        }

        [TestMethod]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 2000,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            _accountFundsAvailableServiceMock.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(_accountFundsAvailable);
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
            Assert.IsNotNull(actual);

            Assert.AreEqual(_accountFundsAvailable.AccountingStringValue, actual.AccountingStringValue);
            Assert.AreEqual(_accountFundsAvailable.BalanceOn, actual.BalanceOn);
            Assert.AreEqual(_accountFundsAvailable.FundsAvailable, actual.FundsAvailable);
        }

        [TestMethod]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_EmptyBalanceAndSubmittedBy()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 2000
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"),
                criteria);

            // criteria = "{ 'accountingString':'11_00_01_00_20603_52010', 'amount': '2000', 'balanceOn': '', 'submittedBy': '' }";
            _accountFundsAvailableServiceMock.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(), DateTime.Today.Date, "")).ReturnsAsync(_accountFundsAvailable);
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByAccountSpecificationAsync()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 2000,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "accountSpecification"), criteria);

            _accountFundsAvailableServiceMock.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(_accountFundsAvailable);
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
            Assert.IsNotNull(actual);

            Assert.AreEqual(_accountFundsAvailable.AccountingStringValue, actual.AccountingStringValue);
            Assert.AreEqual(_accountFundsAvailable.BalanceOn, actual.BalanceOn);
            Assert.AreEqual(_accountFundsAvailable.FundsAvailable, actual.FundsAvailable);
        }

        [TestMethod]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_ZeroAmount()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 0,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            // criteria = "{ 'accountingString':'11_00_01_00_20603_52010', 'amount': '0', 'balanceOn': '12/31/2016', 'submittedBy': '02dc2629-e8a7-410e-b4df-572d02822f8b' }";
            _accountFundsAvailableServiceMock.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(_accountFundsAvailable);
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
            Assert.IsNotNull(actual);

            Assert.AreEqual(_accountFundsAvailable.AccountingStringValue, actual.AccountingStringValue);
            Assert.AreEqual(_accountFundsAvailable.BalanceOn, actual.BalanceOn);
            Assert.AreEqual(_accountFundsAvailable.FundsAvailable, actual.FundsAvailable);
        }

        [TestMethod]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByAccountSpecificationAsync_EmptyBalanceAndSubmittedBy()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 2000
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "accountSpecification"),
                criteria);

            // criteria = "{ 'accountingString':'11_00_01_00_20603_52010', 'amount': '2000', 'balanceOn': '', 'submittedBy': '' }";
            _accountFundsAvailableServiceMock.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(), DateTime.Today.Date, "")).ReturnsAsync(_accountFundsAvailable);
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsync()
        {
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_TransactionsAsync(It.IsAny<AccountFundsAvailable_Transactions>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsyncV11()
        {
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions2>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsync_IntegrationApiException()
        {
            _accountFundsAvailableServiceMock.Setup(i => i.CheckAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions>()))
                .ThrowsAsync(new IntegrationApiException());
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_TransactionsAsync(It.IsAny<AccountFundsAvailable_Transactions>());
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsync_ArgumentException()
        {
            _accountFundsAvailableServiceMock.Setup(i => i.CheckAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions>()))
                .ThrowsAsync(new ArgumentException());
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_TransactionsAsync(It.IsAny<AccountFundsAvailable_Transactions>());
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsync_Exception()
        {
            _accountFundsAvailableServiceMock.Setup(i => i.CheckAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions>()))
                .ThrowsAsync(new Exception());
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_TransactionsAsync(It.IsAny<AccountFundsAvailable_Transactions>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsync_InvalidOperationException()
        {
            _accountFundsAvailableServiceMock.Setup(i => i.CheckAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions>()))
                .ThrowsAsync(new InvalidOperationException());
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_TransactionsAsync(It.IsAny<AccountFundsAvailable_Transactions>());
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsyncV11_IntegrationApiException()
        {
            _accountFundsAvailableServiceMock.Setup(i => i.CheckAccountFundsAvailable_Transactions3Async(It.IsAny<AccountFundsAvailable_Transactions2>()))
                .ThrowsAsync(new IntegrationApiException());
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions2>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsyncV11_ArgumentException()
        {
            _accountFundsAvailableServiceMock.Setup(i => i.CheckAccountFundsAvailable_Transactions3Async(It.IsAny<AccountFundsAvailable_Transactions2>()))
                .ThrowsAsync(new ArgumentException());
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions2>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsyncV11_Exception()
        {
            _accountFundsAvailableServiceMock.Setup(i => i.CheckAccountFundsAvailable_Transactions3Async(It.IsAny<AccountFundsAvailable_Transactions2>()))
                .ThrowsAsync(new Exception());
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions2>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_QueryAccountFundsAvailable_TransactionsAsyncV11_InvalidOperationException()
        {
            _accountFundsAvailableServiceMock.Setup(i => i.CheckAccountFundsAvailable_Transactions3Async(It.IsAny<AccountFundsAvailable_Transactions2>()))
                .ThrowsAsync(new InvalidOperationException());
            var actual = await _accountFundsAvailableController.QueryAccountFundsAvailable_Transactions2Async(It.IsAny<AccountFundsAvailable_Transactions2>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_EmptyCriteria()
        {
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_EmptyAccountingString()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                Amount = 2000,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            // criteria = "{ 'accountingString':'', 'amount': '2000', 'balanceOn': '12/31/2016', 'submittedBy': '02dc2629-e8a7-410e-b4df-572d02822f8b' }";
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_EmptyAmount()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            // criteria = "{ 'accountingString':'11_00_01_00_20603_52010', 'amount': '', 'balanceOn': '12/31/2016', 'submittedBy': '02dc2629-e8a7-410e-b4df-572d02822f8b' }";
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_EmptyGuid()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 2000,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("00000000-0000-0000-0000-000000000000")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            // criteria = "{ 'accountingString':'11_00_01_00_20603_52010', 'amount': '2000', 'balanceOn': '12/31/2016', 'submittedBy': '00000000-0000-0000-0000-000000000000' }";
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_BadGuid()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 0,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            // criteria = "{ 'accountingString':'11_00_01_00_20603_52010', 'amount': '2000', 'balanceOn': '12/31/2016', 'submittedBy': '02dc2629-e8a7-410e-b4df-572d02822f8' }";
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_KeyNotFoundException()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 0,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            _accountFundsAvailableServiceMock.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime?>(), It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_RepositoryException()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 0,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            _accountFundsAvailableServiceMock.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime?>(), It.IsAny<string>()))
                .ThrowsAsync(new RepositoryException());
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailableControllerByFilterCriteriaAsync_PermissionsException()
        {
            criteria = new Dtos.Filters.AccountFundsAvailableFilter()
            {
                AccountingString = "11_00_01_00_20603_52010",
                Amount = 0,
                BalanceOn = new DateTime(2016, 12, 31),
                SubmittedBy = new GuidObject2("02dc2629-e8a7-410e-b4df-572d02822f8b")
            };

            _accountFundsAvailableController.Request.Properties.Add(
                string.Format("FilterObject{0}", "criteria"), criteria);

            _accountFundsAvailableServiceMock.Setup(x => x.GetAccountFundsAvailableByFilterCriteriaAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<DateTime?>(), It.IsAny<string>()))
                .ThrowsAsync(new PermissionsException());
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailableAsync(criteriaFilter, accountSpecificationFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_PostAccountFundsAvailableAsync_NotImplemented()
        {
            var actual = await _accountFundsAvailableController.PostAccountFundsAvailableAsync(It.IsAny<AccountFundsAvailable>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_PutAccountFundsAvailableAsync_NotImplemented()
        {
            var actual = await _accountFundsAvailableController.PutAccountFundsAvailableAsync(It.IsAny<string>(), It.IsAny<AccountFundsAvailable>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_DeleteAccountFundsAvailableAsync_NotImplemented()
        {
            await _accountFundsAvailableController.DeleteAccountFundsAvailableAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailable_TransactionsAsync_NotSupported()
        {
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailable_TransactionsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_GetAccountFundsAvailable_TransactionsByGuidAsync_NotSupported()
        {
            var actual = await _accountFundsAvailableController.GetAccountFundsAvailable_TransactionsByGuidAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_PostAccountFundsAvailable_TransactionsAsync_NotSupported()
        {
            var actual = await _accountFundsAvailableController.PostAccountFundsAvailable_TransactionsAsync(It.IsAny<AccountFundsAvailable_Transactions>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_PutAccountFundsAvailable_TransactionsAsync_NotSupported()
        {
            var actual = await _accountFundsAvailableController.PutAccountFundsAvailable_TransactionsAsync(It.IsAny<string>(), It.IsAny<AccountFundsAvailable_Transactions>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountFundsAvailableController_DeleteAccountFundsAvailable_TransactionsAsync_NotSupported()
        {
            await _accountFundsAvailableController.DeleteAccountFundsAvailable_TransactionsAsync(It.IsAny<string>());
        }
    }
}