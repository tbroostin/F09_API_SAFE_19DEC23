// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Exceptions;
using System.Web.Http.Hosting;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class AccountsPayableInvoicesControllerGetTestsV11
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IAccountsPayableInvoicesService> accountsPayableInvoicesServiceMock;
        private Mock<ILogger> loggerMock;
        private AccountsPayableInvoicesController accountsPayableInvoicesController;

        private AccountsPayableInvoices2 accountsPayableInvoices;

        private string guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";
        private Paging page;
        private int limit;
        private int offset;
        private Tuple<IEnumerable<AccountsPayableInvoices2>, int> accountsPayableInvoicesTuple;

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            accountsPayableInvoicesServiceMock = new Mock<IAccountsPayableInvoicesService>();

            InitializeTestData();

            limit = 100;
            offset = 0;
            page = new Paging(limit, offset);

            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ReturnsAsync(accountsPayableInvoices);
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ReturnsAsync(accountsPayableInvoices);
            accountsPayableInvoicesServiceMock.Setup(s => s.GetAccountsPayableInvoices2ByGuidAsync(It.IsAny<string>())).ReturnsAsync(accountsPayableInvoices);

            accountsPayableInvoicesController = new AccountsPayableInvoicesController(accountsPayableInvoicesServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            accountsPayableInvoicesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            accountsPayableInvoicesController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(accountsPayableInvoices));
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            accountsPayableInvoicesServiceMock = null;
            accountsPayableInvoicesController = null;
        }

        private void InitializeTestData()
        {
            //accountsPayableInvoices = new AccountsPayableInvoices2()
            //{
            //    Id = guid,
            //    TransactionDate = DateTime.Now.Date
            //};
            accountsPayableInvoices = new AccountsPayableInvoices2()
            {
                Id = guid,
                //Vendor = new GuidObject2("0123VendorGuid"),
                //VendorAddress = new GuidObject2("02344AddressGuid"),
                ReferenceNumber = "refNo012",
                VendorInvoiceNumber = "VIN021",
                TransactionDate = new DateTime(2017, 1, 12),
                VendorInvoiceDate = new DateTime(2017, 1, 12),
                VoidDate = new DateTime(2017, 1, 25),
                ProcessState = Dtos.EnumProperties.AccountsPayableInvoicesProcessState.NotSet,
                VendorBilledAmount =
                    new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Value = 40m,
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                    },
                InvoiceDiscountAmount =
                    new Dtos.DtoProperties.Amount2DtoProperty()
                    {
                        Value = 5m,
                        Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                    },
                Taxes = new List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty()
                    {
                        TaxCode = new GuidObject2("TaxCodeGuid"),
                        VendorAmount =
                            new Dtos.DtoProperties.Amount2DtoProperty()
                            {
                                Value = 1m,
                                Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                            }
                    }
                },
                InvoiceType = Dtos.EnumProperties.AccountsPayableInvoicesInvoiceType.Invoice,
                Payment = new Dtos.DtoProperties.AccountsPayableInvoicesPaymentDtoProperty()
                {
                    Source = new GuidObject2("SourceGuid0123"),
                    PaymentDueOn = new DateTime(2017, 1, 17),
                    PaymentTerms = new GuidObject2("TermsGuid321")
                },
                InvoiceComment = "This is a Comment 321",
                GovernmentReporting = new List<Dtos.DtoProperties.GovernmentReportingDtoProperty>()
                {
                    new Dtos.DtoProperties.GovernmentReportingDtoProperty()
                    {
                        Code = CountryCodeType.USA,
                        TransactionType = Dtos.EnumProperties.AccountsPayableInvoicesTransactionType.NotSet
                    }
                },
                LineItems = new List<Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2>()
                {
                    new Dtos.DtoProperties.AccountsPayableInvoicesLineItemDtoProperty2()
                    {
                        AccountDetails = new List<Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty>()
                        {
                            new Dtos.DtoProperties.AccountsPayableInvoicesAccountDetailDtoProperty()
                            {
                                AccountingString = "10-10-1000-400",
                                Allocation = new Dtos.DtoProperties.AccountsPayableInvoicesAllocationDtoProperty()
                                {
                                    Allocated = new Dtos.DtoProperties.AccountsPayableInvoicesAllocatedDtoProperty()
                                    {
                                        Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                                        {
                                            Value = 10m,
                                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                                        },
                                        Percentage = 100m,
                                        Quantity = 2m
                                    }
                                },
                                BudgetCheck = Dtos.EnumProperties.AccountsPayableInvoicesAccountBudgetCheck.NotRequired,
                                SequenceNumber = 1,
                                Source = new GuidObject2("asbc-321"),
                                SubmittedBy = new GuidObject2("submit-guid"),

                            }
                        },
                        Comment = "LineItem comment",
                        Description = "line item Description",
                        CommodityCode = new GuidObject2("Commodity-guid"),
                        Quantity = 2m,
                        UnitofMeasure = new GuidObject2("um-guid"),
                        UnitPrice = new Dtos.DtoProperties.Amount2DtoProperty()
                        {
                            Value = 5m,
                            Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                        },
                        Taxes = new List<Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty>()
                        {
                            new Dtos.DtoProperties.AccountsPayableInvoicesTaxesDtoProperty()
                            {
                                TaxCode = new GuidObject2("taxCode-guid"),
                                VendorAmount = new Dtos.DtoProperties.Amount2DtoProperty()
                                {
                                    Value = 1m,
                                    Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                                }
                            }
                        },
                        Discount = new Dtos.DtoProperties.AccountsPayableInvoicesDiscountDtoProperty()
                        {
                            Amount = new Dtos.DtoProperties.Amount2DtoProperty()
                            {
                                Value = 1m,
                                Currency = Dtos.EnumProperties.CurrencyIsoCode.USD
                            },
                            Percent = 1m
                        }
                        ,
                        PaymentStatus = Dtos.EnumProperties.AccountsPayableInvoicesPaymentStatus.Nohold

                    }
                }
            };
        }

        #endregion

        [TestMethod]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2ByGuidAsync()
        {
            //_mockAccountsPayableInvoicesService.Setup(x => x.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>()))
            //    .ReturnsAsync(_accountsPayableInvoices);

            var actual = await accountsPayableInvoicesController.GetAccountsPayableInvoices2ByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async()
        {
            var DtosAPI = new List<Dtos.AccountsPayableInvoices2>();
            DtosAPI.Add(accountsPayableInvoices);
            accountsPayableInvoices.Id = "NewGuid2";
            DtosAPI.Add(accountsPayableInvoices);
            accountsPayableInvoices.Id = "NewGuid3";
            DtosAPI.Add(accountsPayableInvoices);

            accountsPayableInvoicesTuple = new Tuple<IEnumerable<AccountsPayableInvoices2>, int>(DtosAPI, 3);

            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ReturnsAsync(accountsPayableInvoicesTuple);
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<AccountsPayableInvoices2> ActualsAPI =
                ((ObjectContent<IEnumerable<AccountsPayableInvoices2>>)httpResponseMessage.Content).Value as
                    List<AccountsPayableInvoices2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = DtosAPI.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.GovernmentReporting, actual.GovernmentReporting);
                Assert.AreEqual(expected.InvoiceComment, actual.InvoiceComment);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Currency, actual.InvoiceDiscountAmount.Currency);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Value, actual.InvoiceDiscountAmount.Value);
                Assert.AreEqual(expected.InvoiceType, actual.InvoiceType);
                Assert.AreEqual(expected.LineItems, actual.LineItems);
                Assert.AreEqual(expected.Payment.DirectDepositOverride, actual.Payment.DirectDepositOverride);
                Assert.AreEqual(expected.Payment.PaymentDueOn, actual.Payment.PaymentDueOn);
                Assert.AreEqual(expected.Payment.PaymentTerms, actual.Payment.PaymentTerms);
                Assert.AreEqual(expected.Payment.Source, actual.Payment.Source);
                Assert.AreEqual(expected.PaymentStatus, actual.PaymentStatus);
                Assert.AreEqual(expected.ProcessState, actual.ProcessState);
                Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
                Assert.AreEqual(expected.Taxes[0].TaxCode, actual.Taxes[0].TaxCode);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Currency, actual.Taxes[0].VendorAmount.Currency);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Value, actual.Taxes[0].VendorAmount.Value);
                //Assert.AreEqual(expected.Vendor, actual.Vendor);
                Assert.AreEqual(expected.TransactionDate, actual.TransactionDate);
                Assert.AreEqual(expected.VendorBilledAmount.Currency, actual.VendorBilledAmount.Currency);
                Assert.AreEqual(expected.VendorBilledAmount.Value, actual.VendorBilledAmount.Value);
                Assert.AreEqual(expected.VendorInvoiceDate, actual.VendorInvoiceDate);
                Assert.AreEqual(expected.VendorInvoiceNumber, actual.VendorInvoiceNumber);
                Assert.AreEqual(expected.VoidDate, actual.VoidDate);
                Assert.AreEqual(expected.LineItems.Count(), actual.LineItems.Count());
                for (int x = 0; x < expected.LineItems.Count(); x++)
                {
                    var lineItem = actual.LineItems[x];
                    var expectedLi = expected.LineItems[x];

                    Assert.AreEqual(expectedLi.Description, lineItem.Description);
                    Assert.AreEqual(expectedLi.Comment, lineItem.Comment);
                    Assert.AreEqual(expectedLi.CommodityCode.Id, lineItem.CommodityCode.Id);
                    Assert.AreEqual(expectedLi.Quantity, lineItem.Quantity);
                    Assert.AreEqual(expectedLi.UnitofMeasure.Id, lineItem.UnitofMeasure.Id);
                    Assert.AreEqual(expectedLi.UnitPrice.Value, lineItem.UnitPrice.Value);
                    Assert.AreEqual(expectedLi.UnitPrice.Currency, lineItem.UnitPrice.Currency);

                    Assert.AreEqual(expectedLi.Taxes.Count(), lineItem.Taxes.Count());
                    Assert.AreEqual(expectedLi.Taxes[0].TaxCode.Id, lineItem.Taxes[0].TaxCode.Id);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Currency, lineItem.Taxes[0].VendorAmount.Currency);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Value, lineItem.Taxes[0].VendorAmount.Value);

                    Assert.AreEqual(expectedLi.Discount.Amount.Value, lineItem.Discount.Amount.Value);
                    Assert.AreEqual(expectedLi.Discount.Amount.Currency, lineItem.Discount.Amount.Currency);
                    Assert.AreEqual(expectedLi.Discount.Percent, lineItem.Discount.Percent);

                    Assert.AreEqual(expectedLi.PaymentStatus, lineItem.PaymentStatus);

                    Assert.AreEqual(expectedLi.AccountDetails.Count(), lineItem.AccountDetails.Count());
                    Assert.AreEqual(expectedLi.AccountDetails[0].SequenceNumber,
                        lineItem.AccountDetails[0].SequenceNumber);
                    Assert.AreEqual(expectedLi.AccountDetails[0].AccountingString,
                        lineItem.AccountDetails[0].AccountingString);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Value,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Value);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Currency,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Currency);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Percentage,
                        lineItem.AccountDetails[0].Allocation.Allocated.Percentage);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Quantity,
                        lineItem.AccountDetails[0].Allocation.Allocated.Quantity);
                }
            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_cache()
        {
            var DtosAPI = new List<Dtos.AccountsPayableInvoices2>();
            DtosAPI.Add(accountsPayableInvoices);
            accountsPayableInvoices.Id = "NewGuid2";
            DtosAPI.Add(accountsPayableInvoices);
            accountsPayableInvoices.Id = "NewGuid3";
            DtosAPI.Add(accountsPayableInvoices);

            accountsPayableInvoicesTuple = new Tuple<IEnumerable<AccountsPayableInvoices2>, int>(DtosAPI, 3);

            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ReturnsAsync(accountsPayableInvoicesTuple);
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<AccountsPayableInvoices2> ActualsAPI =
                ((ObjectContent<IEnumerable<AccountsPayableInvoices2>>)httpResponseMessage.Content).Value as
                    List<AccountsPayableInvoices2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = DtosAPI.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.GovernmentReporting, actual.GovernmentReporting);
                Assert.AreEqual(expected.InvoiceComment, actual.InvoiceComment);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Currency, actual.InvoiceDiscountAmount.Currency);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Value, actual.InvoiceDiscountAmount.Value);
                Assert.AreEqual(expected.InvoiceType, actual.InvoiceType);
                Assert.AreEqual(expected.LineItems, actual.LineItems);
                Assert.AreEqual(expected.Payment.DirectDepositOverride, actual.Payment.DirectDepositOverride);
                Assert.AreEqual(expected.Payment.PaymentDueOn, actual.Payment.PaymentDueOn);
                Assert.AreEqual(expected.Payment.PaymentTerms, actual.Payment.PaymentTerms);
                Assert.AreEqual(expected.Payment.Source, actual.Payment.Source);
                Assert.AreEqual(expected.PaymentStatus, actual.PaymentStatus);
                Assert.AreEqual(expected.ProcessState, actual.ProcessState);
                Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
                Assert.AreEqual(expected.Taxes[0].TaxCode, actual.Taxes[0].TaxCode);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Currency, actual.Taxes[0].VendorAmount.Currency);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Value, actual.Taxes[0].VendorAmount.Value);
                //Assert.AreEqual(expected.Vendor, actual.Vendor);
                Assert.AreEqual(expected.TransactionDate, actual.TransactionDate);
                Assert.AreEqual(expected.VendorBilledAmount.Currency, actual.VendorBilledAmount.Currency);
                Assert.AreEqual(expected.VendorBilledAmount.Value, actual.VendorBilledAmount.Value);
                Assert.AreEqual(expected.VendorInvoiceDate, actual.VendorInvoiceDate);
                Assert.AreEqual(expected.VendorInvoiceNumber, actual.VendorInvoiceNumber);
                Assert.AreEqual(expected.VoidDate, actual.VoidDate);

                Assert.AreEqual(expected.LineItems.Count(), actual.LineItems.Count());
                for (int x = 0; x < expected.LineItems.Count(); x++)
                {
                    var lineItem = actual.LineItems[x];
                    var expectedLi = expected.LineItems[x];

                    Assert.AreEqual(expectedLi.Description, lineItem.Description);
                    Assert.AreEqual(expectedLi.Comment, lineItem.Comment);
                    Assert.AreEqual(expectedLi.CommodityCode.Id, lineItem.CommodityCode.Id);
                    Assert.AreEqual(expectedLi.Quantity, lineItem.Quantity);
                    Assert.AreEqual(expectedLi.UnitofMeasure.Id, lineItem.UnitofMeasure.Id);
                    Assert.AreEqual(expectedLi.UnitPrice.Value, lineItem.UnitPrice.Value);
                    Assert.AreEqual(expectedLi.UnitPrice.Currency, lineItem.UnitPrice.Currency);

                    Assert.AreEqual(expectedLi.Taxes.Count(), lineItem.Taxes.Count());
                    Assert.AreEqual(expectedLi.Taxes[0].TaxCode.Id, lineItem.Taxes[0].TaxCode.Id);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Currency, lineItem.Taxes[0].VendorAmount.Currency);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Value, lineItem.Taxes[0].VendorAmount.Value);

                    Assert.AreEqual(expectedLi.Discount.Amount.Value, lineItem.Discount.Amount.Value);
                    Assert.AreEqual(expectedLi.Discount.Amount.Currency, lineItem.Discount.Amount.Currency);
                    Assert.AreEqual(expectedLi.Discount.Percent, lineItem.Discount.Percent);

                    Assert.AreEqual(expectedLi.PaymentStatus, lineItem.PaymentStatus);

                    Assert.AreEqual(expectedLi.AccountDetails.Count(), lineItem.AccountDetails.Count());
                    Assert.AreEqual(expectedLi.AccountDetails[0].SequenceNumber,
                        lineItem.AccountDetails[0].SequenceNumber);
                    Assert.AreEqual(expectedLi.AccountDetails[0].AccountingString,
                        lineItem.AccountDetails[0].AccountingString);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Value,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Value);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Currency,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Currency);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Percentage,
                        lineItem.AccountDetails[0].Allocation.Allocated.Percentage);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Quantity,
                        lineItem.AccountDetails[0].Allocation.Allocated.Quantity);
                }
            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_NoCache()
        {
            var DtosAPI = new List<Dtos.AccountsPayableInvoices2>();
            DtosAPI.Add(accountsPayableInvoices);
            accountsPayableInvoices.Id = "NewGuid2";
            DtosAPI.Add(accountsPayableInvoices);
            accountsPayableInvoices.Id = "NewGuid3";
            DtosAPI.Add(accountsPayableInvoices);

            accountsPayableInvoicesTuple = new Tuple<IEnumerable<AccountsPayableInvoices2>, int>(DtosAPI, 3);

            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ReturnsAsync(accountsPayableInvoicesTuple);
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<AccountsPayableInvoices2> ActualsAPI =
                ((ObjectContent<IEnumerable<AccountsPayableInvoices2>>)httpResponseMessage.Content).Value as
                    List<AccountsPayableInvoices2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = DtosAPI.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.GovernmentReporting, actual.GovernmentReporting);
                Assert.AreEqual(expected.InvoiceComment, actual.InvoiceComment);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Currency, actual.InvoiceDiscountAmount.Currency);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Value, actual.InvoiceDiscountAmount.Value);
                Assert.AreEqual(expected.InvoiceType, actual.InvoiceType);
                Assert.AreEqual(expected.LineItems, actual.LineItems);
                Assert.AreEqual(expected.Payment.DirectDepositOverride, actual.Payment.DirectDepositOverride);
                Assert.AreEqual(expected.Payment.PaymentDueOn, actual.Payment.PaymentDueOn);
                Assert.AreEqual(expected.Payment.PaymentTerms, actual.Payment.PaymentTerms);
                Assert.AreEqual(expected.Payment.Source, actual.Payment.Source);
                Assert.AreEqual(expected.PaymentStatus, actual.PaymentStatus);
                Assert.AreEqual(expected.ProcessState, actual.ProcessState);
                Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
                Assert.AreEqual(expected.Taxes[0].TaxCode, actual.Taxes[0].TaxCode);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Currency, actual.Taxes[0].VendorAmount.Currency);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Value, actual.Taxes[0].VendorAmount.Value);
                //Assert.AreEqual(expected.Vendor, actual.Vendor);
                Assert.AreEqual(expected.TransactionDate, actual.TransactionDate);
                Assert.AreEqual(expected.VendorBilledAmount.Currency, actual.VendorBilledAmount.Currency);
                Assert.AreEqual(expected.VendorBilledAmount.Value, actual.VendorBilledAmount.Value);
                Assert.AreEqual(expected.VendorInvoiceDate, actual.VendorInvoiceDate);
                Assert.AreEqual(expected.VendorInvoiceNumber, actual.VendorInvoiceNumber);
                Assert.AreEqual(expected.VoidDate, actual.VoidDate);

                Assert.AreEqual(expected.LineItems.Count(), actual.LineItems.Count());
                for (int x = 0; x < expected.LineItems.Count(); x++)
                {
                    var lineItem = actual.LineItems[x];
                    var expectedLi = expected.LineItems[x];

                    Assert.AreEqual(expectedLi.Description, lineItem.Description);
                    Assert.AreEqual(expectedLi.Comment, lineItem.Comment);
                    Assert.AreEqual(expectedLi.CommodityCode.Id, lineItem.CommodityCode.Id);
                    Assert.AreEqual(expectedLi.Quantity, lineItem.Quantity);
                    Assert.AreEqual(expectedLi.UnitofMeasure.Id, lineItem.UnitofMeasure.Id);
                    Assert.AreEqual(expectedLi.UnitPrice.Value, lineItem.UnitPrice.Value);
                    Assert.AreEqual(expectedLi.UnitPrice.Currency, lineItem.UnitPrice.Currency);

                    Assert.AreEqual(expectedLi.Taxes.Count(), lineItem.Taxes.Count());
                    Assert.AreEqual(expectedLi.Taxes[0].TaxCode.Id, lineItem.Taxes[0].TaxCode.Id);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Currency, lineItem.Taxes[0].VendorAmount.Currency);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Value, lineItem.Taxes[0].VendorAmount.Value);

                    Assert.AreEqual(expectedLi.Discount.Amount.Value, lineItem.Discount.Amount.Value);
                    Assert.AreEqual(expectedLi.Discount.Amount.Currency, lineItem.Discount.Amount.Currency);
                    Assert.AreEqual(expectedLi.Discount.Percent, lineItem.Discount.Percent);

                    Assert.AreEqual(expectedLi.PaymentStatus, lineItem.PaymentStatus);

                    Assert.AreEqual(expectedLi.AccountDetails.Count(), lineItem.AccountDetails.Count());
                    Assert.AreEqual(expectedLi.AccountDetails[0].SequenceNumber,
                        lineItem.AccountDetails[0].SequenceNumber);
                    Assert.AreEqual(expectedLi.AccountDetails[0].AccountingString,
                        lineItem.AccountDetails[0].AccountingString);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Value,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Value);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Currency,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Currency);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Percentage,
                        lineItem.AccountDetails[0].Allocation.Allocated.Percentage);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Quantity,
                        lineItem.AccountDetails[0].Allocation.Allocated.Quantity);
                }
            }
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_Paging()
        {
            var DtosAPI = new List<Dtos.AccountsPayableInvoices2>();
            DtosAPI.Add(accountsPayableInvoices);

            page = new Paging(1, 1);

            accountsPayableInvoicesTuple = new Tuple<IEnumerable<AccountsPayableInvoices2>, int>(DtosAPI, 1);

            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(x => x.GetAccountsPayableInvoices2Async(1, 1, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ReturnsAsync(accountsPayableInvoicesTuple);
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<AccountsPayableInvoices2> ActualsAPI =
                ((ObjectContent<IEnumerable<AccountsPayableInvoices2>>)httpResponseMessage.Content).Value as
                    List<AccountsPayableInvoices2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = DtosAPI.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.GovernmentReporting, actual.GovernmentReporting);
                Assert.AreEqual(expected.InvoiceComment, actual.InvoiceComment);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Currency, actual.InvoiceDiscountAmount.Currency);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Value, actual.InvoiceDiscountAmount.Value);
                Assert.AreEqual(expected.InvoiceType, actual.InvoiceType);
                Assert.AreEqual(expected.LineItems, actual.LineItems);
                Assert.AreEqual(expected.Payment.DirectDepositOverride, actual.Payment.DirectDepositOverride);
                Assert.AreEqual(expected.Payment.PaymentDueOn, actual.Payment.PaymentDueOn);
                Assert.AreEqual(expected.Payment.PaymentTerms, actual.Payment.PaymentTerms);
                Assert.AreEqual(expected.Payment.Source, actual.Payment.Source);
                Assert.AreEqual(expected.PaymentStatus, actual.PaymentStatus);
                Assert.AreEqual(expected.ProcessState, actual.ProcessState);
                Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
                Assert.AreEqual(expected.Taxes[0].TaxCode, actual.Taxes[0].TaxCode);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Currency, actual.Taxes[0].VendorAmount.Currency);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Value, actual.Taxes[0].VendorAmount.Value);
                //Assert.AreEqual(expected.Vendor, actual.Vendor);
                Assert.AreEqual(expected.TransactionDate, actual.TransactionDate);
                Assert.AreEqual(expected.VendorBilledAmount.Currency, actual.VendorBilledAmount.Currency);
                Assert.AreEqual(expected.VendorBilledAmount.Value, actual.VendorBilledAmount.Value);
                Assert.AreEqual(expected.VendorInvoiceDate, actual.VendorInvoiceDate);
                Assert.AreEqual(expected.VendorInvoiceNumber, actual.VendorInvoiceNumber);
                Assert.AreEqual(expected.VoidDate, actual.VoidDate);

                Assert.AreEqual(expected.LineItems.Count(), actual.LineItems.Count());
                for (int x = 0; x < expected.LineItems.Count(); x++)
                {
                    var lineItem = actual.LineItems[x];
                    var expectedLi = expected.LineItems[x];

                    Assert.AreEqual(expectedLi.Description, lineItem.Description);
                    Assert.AreEqual(expectedLi.Comment, lineItem.Comment);
                    Assert.AreEqual(expectedLi.CommodityCode.Id, lineItem.CommodityCode.Id);
                    Assert.AreEqual(expectedLi.Quantity, lineItem.Quantity);
                    Assert.AreEqual(expectedLi.UnitofMeasure.Id, lineItem.UnitofMeasure.Id);
                    Assert.AreEqual(expectedLi.UnitPrice.Value, lineItem.UnitPrice.Value);
                    Assert.AreEqual(expectedLi.UnitPrice.Currency, lineItem.UnitPrice.Currency);

                    Assert.AreEqual(expectedLi.Taxes.Count(), lineItem.Taxes.Count());
                    Assert.AreEqual(expectedLi.Taxes[0].TaxCode.Id, lineItem.Taxes[0].TaxCode.Id);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Currency, lineItem.Taxes[0].VendorAmount.Currency);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Value, lineItem.Taxes[0].VendorAmount.Value);

                    Assert.AreEqual(expectedLi.Discount.Amount.Value, lineItem.Discount.Amount.Value);
                    Assert.AreEqual(expectedLi.Discount.Amount.Currency, lineItem.Discount.Amount.Currency);
                    Assert.AreEqual(expectedLi.Discount.Percent, lineItem.Discount.Percent);

                    Assert.AreEqual(expectedLi.PaymentStatus, lineItem.PaymentStatus);

                    Assert.AreEqual(expectedLi.AccountDetails.Count(), lineItem.AccountDetails.Count());
                    Assert.AreEqual(expectedLi.AccountDetails[0].SequenceNumber,
                        lineItem.AccountDetails[0].SequenceNumber);
                    Assert.AreEqual(expectedLi.AccountDetails[0].AccountingString,
                        lineItem.AccountDetails[0].AccountingString);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Value,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Value);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Currency,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Currency);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Percentage,
                        lineItem.AccountDetails[0].Allocation.Allocated.Percentage);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Quantity,
                        lineItem.AccountDetails[0].Allocation.Allocated.Quantity);
                }
            }
        }


        [TestMethod]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_NoPaging()
        {
            var DtosAPI = new List<Dtos.AccountsPayableInvoices2>();
            DtosAPI.Add(accountsPayableInvoices);

            page = new Paging(1, 1);

            accountsPayableInvoicesTuple = new Tuple<IEnumerable<AccountsPayableInvoices2>, int>(DtosAPI, 1);

            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(x => x.GetAccountsPayableInvoices2Async(0, 100, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ReturnsAsync(accountsPayableInvoicesTuple);
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(null, It.IsAny<QueryStringFilter>());
            var cancelToken = new System.Threading.CancellationToken(false);
            System.Net.Http.HttpResponseMessage httpResponseMessage = await actuals.ExecuteAsync(cancelToken);
            List<AccountsPayableInvoices2> ActualsAPI =
                ((ObjectContent<IEnumerable<AccountsPayableInvoices2>>)httpResponseMessage.Content).Value as
                    List<AccountsPayableInvoices2>;
            for (var i = 0; i < ActualsAPI.Count; i++)
            {
                var expected = DtosAPI.ToList()[i];
                var actual = ActualsAPI[i];
                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.GovernmentReporting, actual.GovernmentReporting);
                Assert.AreEqual(expected.InvoiceComment, actual.InvoiceComment);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Currency, actual.InvoiceDiscountAmount.Currency);
                Assert.AreEqual(expected.InvoiceDiscountAmount.Value, actual.InvoiceDiscountAmount.Value);
                Assert.AreEqual(expected.InvoiceType, actual.InvoiceType);
                Assert.AreEqual(expected.LineItems, actual.LineItems);
                Assert.AreEqual(expected.Payment.DirectDepositOverride, actual.Payment.DirectDepositOverride);
                Assert.AreEqual(expected.Payment.PaymentDueOn, actual.Payment.PaymentDueOn);
                Assert.AreEqual(expected.Payment.PaymentTerms, actual.Payment.PaymentTerms);
                Assert.AreEqual(expected.Payment.Source, actual.Payment.Source);
                Assert.AreEqual(expected.PaymentStatus, actual.PaymentStatus);
                Assert.AreEqual(expected.ProcessState, actual.ProcessState);
                Assert.AreEqual(expected.ReferenceNumber, actual.ReferenceNumber);
                Assert.AreEqual(expected.Taxes[0].TaxCode, actual.Taxes[0].TaxCode);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Currency, actual.Taxes[0].VendorAmount.Currency);
                Assert.AreEqual(expected.Taxes[0].VendorAmount.Value, actual.Taxes[0].VendorAmount.Value);
                //Assert.AreEqual(expected.Vendor, actual.Vendor);
                Assert.AreEqual(expected.TransactionDate, actual.TransactionDate);
                Assert.AreEqual(expected.VendorBilledAmount.Currency, actual.VendorBilledAmount.Currency);
                Assert.AreEqual(expected.VendorBilledAmount.Value, actual.VendorBilledAmount.Value);
                Assert.AreEqual(expected.VendorInvoiceDate, actual.VendorInvoiceDate);
                Assert.AreEqual(expected.VendorInvoiceNumber, actual.VendorInvoiceNumber);
                Assert.AreEqual(expected.VoidDate, actual.VoidDate);

                Assert.AreEqual(expected.LineItems.Count(), actual.LineItems.Count());
                for (int x = 0; x < expected.LineItems.Count(); x++)
                {
                    var lineItem = actual.LineItems[x];
                    var expectedLi = expected.LineItems[x];

                    Assert.AreEqual(expectedLi.Description, lineItem.Description);
                    Assert.AreEqual(expectedLi.Comment, lineItem.Comment);
                    Assert.AreEqual(expectedLi.CommodityCode.Id, lineItem.CommodityCode.Id);
                    Assert.AreEqual(expectedLi.Quantity, lineItem.Quantity);
                    Assert.AreEqual(expectedLi.UnitofMeasure.Id, lineItem.UnitofMeasure.Id);
                    Assert.AreEqual(expectedLi.UnitPrice.Value, lineItem.UnitPrice.Value);
                    Assert.AreEqual(expectedLi.UnitPrice.Currency, lineItem.UnitPrice.Currency);

                    Assert.AreEqual(expectedLi.Taxes.Count(), lineItem.Taxes.Count());
                    Assert.AreEqual(expectedLi.Taxes[0].TaxCode.Id, lineItem.Taxes[0].TaxCode.Id);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Currency, lineItem.Taxes[0].VendorAmount.Currency);
                    Assert.AreEqual(expectedLi.Taxes[0].VendorAmount.Value, lineItem.Taxes[0].VendorAmount.Value);

                    Assert.AreEqual(expectedLi.Discount.Amount.Value, lineItem.Discount.Amount.Value);
                    Assert.AreEqual(expectedLi.Discount.Amount.Currency, lineItem.Discount.Amount.Currency);
                    Assert.AreEqual(expectedLi.Discount.Percent, lineItem.Discount.Percent);

                    Assert.AreEqual(expectedLi.PaymentStatus, lineItem.PaymentStatus);

                    Assert.AreEqual(expectedLi.AccountDetails.Count(), lineItem.AccountDetails.Count());
                    Assert.AreEqual(expectedLi.AccountDetails[0].SequenceNumber,
                        lineItem.AccountDetails[0].SequenceNumber);
                    Assert.AreEqual(expectedLi.AccountDetails[0].AccountingString,
                        lineItem.AccountDetails[0].AccountingString);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Value,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Value);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Amount.Currency,
                        lineItem.AccountDetails[0].Allocation.Allocated.Amount.Currency);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Percentage,
                        lineItem.AccountDetails[0].Allocation.Allocated.Percentage);
                    Assert.AreEqual(expectedLi.AccountDetails[0].Allocation.Allocated.Quantity,
                        lineItem.AccountDetails[0].Allocation.Allocated.Quantity);
                }
            }
        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2ByGuidAsync_KeyNotFoundExecpt
            ()
        {
            accountsPayableInvoicesServiceMock.Setup(x => x.GetAccountsPayableInvoices2ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new KeyNotFoundException());

            var actual = await accountsPayableInvoicesController.GetAccountsPayableInvoices2ByGuidAsync(guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task
            AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2ByGuidAsync_PermissionsException()
        {
            accountsPayableInvoicesServiceMock.Setup(x => x.GetAccountsPayableInvoices2ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new PermissionsException());

            var actual = await accountsPayableInvoicesController.GetAccountsPayableInvoices2ByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2ByGuidAsync_ArgumentException
            ()
        {
            accountsPayableInvoicesServiceMock.Setup(x => x.GetAccountsPayableInvoices2ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException());

            var actual = await accountsPayableInvoicesController.GetAccountsPayableInvoices2ByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task
            AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2ByGuidAsync_RepositoryException()
        {
            accountsPayableInvoicesServiceMock.Setup(x => x.GetAccountsPayableInvoices2ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new RepositoryException());

            var actual = await accountsPayableInvoicesController.GetAccountsPayableInvoices2ByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task
            AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2ByGuidAsync_IntegrationApiException()
        {
            accountsPayableInvoicesServiceMock.Setup(x => x.GetAccountsPayableInvoices2ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new IntegrationApiException());

            var actual = await accountsPayableInvoicesController.GetAccountsPayableInvoices2ByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2ByGuidAsync_Exception()
        {
            accountsPayableInvoicesServiceMock.Setup(x => x.GetAccountsPayableInvoices2ByGuidAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var actual = await accountsPayableInvoicesController.GetAccountsPayableInvoices2ByGuidAsync(guid);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2ByGuidAsync_nullGuid()
        {
            // _mockAccountsPayableInvoicesService.Setup(x => x.GetAccountsPayableInvoicesByGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            var actual = await accountsPayableInvoicesController.GetAccountsPayableInvoices2ByGuidAsync(null);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_KeyNotFoundExecpt()
        {
            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_PermissionsException()
        {
            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ThrowsAsync(new PermissionsException());
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_ArgumentException()
        {
            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ThrowsAsync(new ArgumentException());
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_RepositoryException()
        {
            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ThrowsAsync(new RepositoryException());
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_IntegrationApiException
            ()
        {
            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>()))
                .ThrowsAsync(new IntegrationApiException());
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesControllerTests_GetAccountsPayableInvoices2Async_Exception()
        {
            accountsPayableInvoicesController.Request = new System.Net.Http.HttpRequestMessage()
            {
                RequestUri = new Uri("http://localhost")
            };
            accountsPayableInvoicesController.Request.Headers.CacheControl =
                new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };
            accountsPayableInvoicesServiceMock.Setup(
                x => x.GetAccountsPayableInvoices2Async(offset, limit, It.IsAny<AccountsPayableInvoices2>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
            var actuals = await accountsPayableInvoicesController.GetAccountsPayableInvoices2Async(page, It.IsAny<QueryStringFilter>());
        }


    }

    [TestClass]
    public class AccountsPayableInvoicesControllerTests_PUT_POST_V11
    {
        #region DECLARATIONS

        public TestContext TestContext { get; set; }
        private Mock<IAccountsPayableInvoicesService> accountsPayableInvoicesServiceMock;
        private Mock<ILogger> loggerMock;
        private AccountsPayableInvoicesController accountsPayableInvoicesController;

        private AccountsPayableInvoices2 accountsPayableInvoices;
        private AccountsPayableInvoices2 accountsPayableInvoicesPOST;

        private string guid = "02dc2629-e8a7-410e-b4df-572d02822f8b";

        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            loggerMock = new Mock<ILogger>();
            accountsPayableInvoicesServiceMock = new Mock<IAccountsPayableInvoicesService>();

            InitializeTestData();

            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ReturnsAsync(accountsPayableInvoices);
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ReturnsAsync(accountsPayableInvoices);
            accountsPayableInvoicesServiceMock.Setup(s => s.GetAccountsPayableInvoices2ByGuidAsync(guid)).ReturnsAsync(accountsPayableInvoices);

            accountsPayableInvoicesController = new AccountsPayableInvoicesController(accountsPayableInvoicesServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
            accountsPayableInvoicesController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            accountsPayableInvoicesController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(accountsPayableInvoices));
        }

        [TestCleanup]
        public void Cleanup()
        {
            loggerMock = null;
            accountsPayableInvoicesServiceMock = null;
            accountsPayableInvoicesController = null;
        }

        private void InitializeTestData()
        {
            accountsPayableInvoices = new AccountsPayableInvoices2()
            {
                Id = guid,
                TransactionDate = DateTime.Now.Date
            };
            accountsPayableInvoicesPOST = new AccountsPayableInvoices2()
            {
                Id = Guid.Empty.ToString(),
                TransactionDate = DateTime.Now.Date
            };
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Guid_As_Null()
        {
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(null, new AccountsPayableInvoices2() { });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Dto_As_Null()
        {
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Guid_As_Empty()
        {
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(Guid.Empty.ToString(), new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Dto_Id_As_Empty()
        {
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = Guid.Empty.ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Dto_Id_Not_Same_As_Id()
        {
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = Guid.NewGuid().ToString() });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_PermissionException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new PermissionsException());
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_KeyNotFoundException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new KeyNotFoundException());
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_ArgumentException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new ArgumentException());
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_ArgumentNullException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new ArgumentNullException());
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_RepositoryException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new RepositoryException());
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_RepositoryException_With_Errors()
        {
            var exception = new RepositoryException() { };

            exception.AddError(new Domain.Entities.RepositoryError("ERROR", "Repository Exception"));

            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(exception);
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_IntegrationApiException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new IntegrationApiException());
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_ConfigurationException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new ConfigurationException());
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_Put_Throws_Exception()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PutAccountsPayableInvoices2Async(It.IsAny<string>(), It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new Exception());
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = guid });
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesController_Put()
        {
            var result = await accountsPayableInvoicesController.PutAccountsPayableInvoices2Async(guid, new AccountsPayableInvoices2() { Id = String.Empty });

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, guid);
        }

        [TestMethod]
        public async Task AccountsPayableInvoicesController_Post()
        {

            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, guid);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Dto_As_Null()
        {
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_ArgumentNullException_Dto_NotNullGUID()
        {
            accountsPayableInvoicesPOST.Id = "1234";

            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_KeyNotFoundException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new KeyNotFoundException());
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_PermissionsException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new PermissionsException());
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_ArgumentException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new ArgumentException());
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_ArgumentNullException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new ArgumentNullException());
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_RepositoryException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new RepositoryException());
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_IntegrationApiException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new IntegrationApiException());
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_ConfigurationException()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new ConfigurationException());
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_Exception()
        {
            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(new Exception());
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_DeleteT_Not_Supported()
        {
            await accountsPayableInvoicesController.DeleteAccountsPayableInvoicesAsync(It.IsAny<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task AccountsPayableInvoicesController_POST_Throws_RepositoryException_With_Errors()
        {
            var exception = new RepositoryException() { };

            exception.AddError(new Domain.Entities.RepositoryError("ERROR", "Repository Exception"));

            accountsPayableInvoicesServiceMock.Setup(s => s.PostAccountsPayableInvoices2Async(It.IsAny<AccountsPayableInvoices2>())).ThrowsAsync(exception);
            var result = await accountsPayableInvoicesController.PostAccountsPayableInvoices2Async(accountsPayableInvoicesPOST);
        }
    }
}






