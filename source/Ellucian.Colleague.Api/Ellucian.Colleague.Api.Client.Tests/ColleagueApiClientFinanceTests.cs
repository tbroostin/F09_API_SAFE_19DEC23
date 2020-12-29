// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountActivity;
using Ellucian.Rest.Client.Exceptions;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Infrastructure.TestUtil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Client.Tests
{
    [TestClass]
    public class ColleagueApiClientFinanceTests
    {
        [TestClass]
        public class GetAccountHolder2
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private string id;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public void GetAccountHolder2_ReturnsSerializedAccountHolder()
            {
                // Arrange
                var accountHolder = new AccountHolder() { Id = "1", FirstName = "Joe", LastName = "Smith"};
                id = accountHolder.Id;

                var serializedResponse = JsonConvert.SerializeObject(accountHolder);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = client.GetAccountHolder2(id);

                // Assert
                Assert.AreEqual(accountHolder.Id, clientResponse.Id);
                Assert.AreEqual(accountHolder.FirstName, clientResponse.FirstName);
                Assert.AreEqual(accountHolder.LastName, clientResponse.LastName);
            }
        }

        [TestClass]
        public class QueryInvoicesAsync
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private IEnumerable<string> invoiceIds;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task QueryInvoice_ReturnsSerializedInvoice()
            {
                // Arrange
                var invoiceDtos = new List<Invoice>()
                {
                    new Invoice() { Id = "1", Amount = 1000, Description = "Invoice Description", PersonId = "0000001"},
                    new Invoice() { Id = "2", Amount = 2000, Description = "Invoice Description 2", PersonId = "0000001"}
                };
                invoiceIds = invoiceDtos.Select(f => f.Id);

                var serializedResponse = JsonConvert.SerializeObject(invoiceDtos.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var criteria = new InvoiceQueryCriteria() { InvoiceIds = invoiceIds };
                var clientResponse = await client.QueryInvoicesAsync(criteria);

                // Assert
                Assert.AreEqual(invoiceDtos.Count(), clientResponse.Count());
                foreach (var id in invoiceIds)
                {
                    var invoiceDto = invoiceDtos.Where(f => f.Id == id).First();
                    var invoiceResponse = clientResponse.Where(f => f.Id == id).First();

                    Assert.AreEqual(invoiceDto.Id, invoiceResponse.Id);
                    Assert.AreEqual(invoiceDto.Description, invoiceResponse.Description);
                }
            }
        }

        [TestClass]
        public class QueryInvoicePaymentsAsync
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private IEnumerable<string> invoiceIds;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task QueryInvoicePayment_ReturnsSerializedInvoicePayments()
            {
                // Arrange
                var invoicePaymentDtos = new List<InvoicePayment>()
                {
                    new InvoicePayment() { Id = "1", Amount = 1000, Description = "Invoice Description", PersonId = "0000001", AmountPaid = 5000},
                    new InvoicePayment() { Id = "2", Amount = 2000, Description = "Invoice Description 2", PersonId = "0000001", AmountPaid = 1000}
                };
                invoiceIds = invoicePaymentDtos.Select(f => f.Id);

                var serializedResponse = JsonConvert.SerializeObject(invoicePaymentDtos.AsEnumerable());

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var criteria = new InvoiceQueryCriteria() { InvoiceIds = invoiceIds };
                var clientResponse = await client.QueryInvoicePaymentsAsync(criteria);

                // Assert
                Assert.AreEqual(invoicePaymentDtos.Count(), clientResponse.Count());
                foreach (var id in invoiceIds)
                {
                    var invoiceDto = invoicePaymentDtos.Where(f => f.Id == id).First();
                    var invoiceResponse = clientResponse.Where(f => f.Id == id).First();

                    Assert.AreEqual(invoiceDto.Id, invoiceResponse.Id);
                    Assert.AreEqual(invoiceDto.Description, invoiceResponse.Description);
                    Assert.AreEqual(invoiceDto.AmountPaid, invoiceResponse.AmountPaid);
                }
            }
        }

        [TestClass]
        public class QueryAccountHolders2Async
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private IEnumerable<string> ids;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task QueryAccountHolders2Async_ReturnsSerializedAccountHolders()
            {
                // Arrange
                var accountHolderDtos = new List<AccountHolder>()
                {
                    new AccountHolder() { Id = "1", FirstName = "Joe", LastName = "Smith"},
                    new AccountHolder() { Id = "2", FirstName = "John", LastName = "Smith"}
                };
                ids = accountHolderDtos.Select(f => f.Id);

                var serializedResponse = JsonConvert.SerializeObject(accountHolderDtos.AsEnumerable());

                var client = serviceClientSetup(serializedResponse);

                // Act
                var criteria = "Smith,J";
                var clientResponse = await client.QueryAccountHoldersByPostAsync2(criteria);

                // Assert
                Assert.AreEqual(accountHolderDtos.Count(), clientResponse.Count());
                foreach (var id in ids)
                {
                    var accountHolderDto = accountHolderDtos.Where(f => f.Id == id).First();
                    var accountHolderResponse = clientResponse.Where(f => f.Id == id).First();

                    Assert.AreEqual(accountHolderDto.Id, accountHolderResponse.Id);
                    Assert.AreEqual(accountHolderDto.FirstName, accountHolderResponse.FirstName);
                    Assert.AreEqual(accountHolderDto.LastName, accountHolderResponse.LastName);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryAccountHolders2Async_ThrowsArgumentNullExceptionTest()
            {
                var client = serviceClientSetup(string.Empty);
                await client.QueryAccountHoldersByPostAsync2(null);
            }
            
            private ColleagueApiClient serviceClientSetup(string serializedResponse)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);
                return client;
            }
        }

        [TestClass]
        public class QueryAccountHolders3Async
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private IEnumerable<string> ids;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task QueryAccountHolders3Async_ReturnsSerializedAccountHolders()
            {
                // Arrange
                var accountHolderDtos = new List<AccountHolder>()
                {
                    new AccountHolder() { Id = "1", FirstName = "Joe", LastName = "Smith"},
                    new AccountHolder() { Id = "2", FirstName = "John", LastName = "Smith"}
                };
                ids = accountHolderDtos.Select(f => f.Id);

                var serializedResponse = JsonConvert.SerializeObject(accountHolderDtos.AsEnumerable());

                var client = serviceClientSetup(serializedResponse);

                // Act
                var criteria = new AccountHolderQueryCriteria() { QueryKeyword = "Smith,J" };
                var clientResponse = await client.QueryAccountHoldersByPost3Async(criteria);

                // Assert
                Assert.AreEqual(accountHolderDtos.Count(), clientResponse.Count());
                foreach (var id in ids)
                {
                    var accountHolderDto = accountHolderDtos.Where(f => f.Id == id).First();
                    var accountHolderResponse = clientResponse.Where(f => f.Id == id).First();

                    Assert.AreEqual(accountHolderDto.Id, accountHolderResponse.Id);
                    Assert.AreEqual(accountHolderDto.FirstName, accountHolderResponse.FirstName);
                    Assert.AreEqual(accountHolderDto.LastName, accountHolderResponse.LastName);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryAccountHolders3Async_ThrowsArgumentNullExceptionTest()
            {
                var client = serviceClientSetup(string.Empty);
                await client.QueryAccountHoldersByPost3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task QueryAccountHolders3Async_ThrowsArgumentExceptionTest()
            {
                var client = serviceClientSetup(string.Empty);
                await client.QueryAccountHoldersByPost3Async(new AccountHolderQueryCriteria());
            }

            private ColleagueApiClient serviceClientSetup(string serializedResponse)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);
                return client;
            }
        }

        [TestClass]
        public class QueryAccountHolderPaymentPlanOptionsAsync
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private IEnumerable<string> ids;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task QueryAccountHolderPaymentPlanOptionsAsync_ReturnsSerializedBillingTermPaymentPlanInformations()
            {
                // Arrange
                var terms = new List<BillingTermPaymentPlanInformation>()
                {
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM1", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM2", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM3", ReceivableTypeCode = "03", PaymentPlanAmount = 10000m },
                };

                var elig = new PaymentPlanEligibility()
                {
                    EligibleItems = terms
                };

                var serializedResponse = JsonConvert.SerializeObject(elig);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAccountHolderPaymentPlanOptionsAsync(terms);

                // Assert
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(terms.Count(), clientResponse.EligibleItems.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryCriteriaIsNull_QueryAccountHolderPaymentPlanOptionsAsync_ThrowsArgumentNullExceptionTest()
            {
                var serializedResponse = JsonConvert.SerializeObject("");

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);
                await client.QueryAccountHolderPaymentPlanOptionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task BillingTermsEmpty_QueryAccountHolderPaymentPlanOptionsAsync_ThrowsArgumentNullExceptionTest()
            {
                var serializedResponse = JsonConvert.SerializeObject("");

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);
                await client.QueryAccountHolderPaymentPlanOptionsAsync(new List<BillingTermPaymentPlanInformation>());
            }
        }

        [TestClass]
        public class QueryAccountHolderPaymentPlanOptions2Async
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private IEnumerable<string> ids;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task QueryAccountHolderPaymentPlanOptionsAsync_ReturnsSerializedBillingTermPaymentPlanInformations()
            {
                // Arrange
                var terms = new PaymentPlanQueryCriteria()
                {
                    BillingTerms = new List<BillingTermPaymentPlanInformation>()
                {
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM1", ReceivableTypeCode = "01", PaymentPlanAmount = 1000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM2", ReceivableTypeCode = "02", PaymentPlanAmount = 5000m },
                    new BillingTermPaymentPlanInformation() { PersonId = "0001234", TermId = "TERM3", ReceivableTypeCode = "03", PaymentPlanAmount = 10000m },
                }
                };

                var elig = new PaymentPlanEligibility()
                {
                    EligibleItems = terms.BillingTerms
                };

                var serializedResponse = JsonConvert.SerializeObject(elig);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryAccountHolderPaymentPlanOptions2Async(terms);

                // Assert
                Assert.IsNotNull(clientResponse);
                Assert.AreEqual(terms.BillingTerms.Count(), clientResponse.EligibleItems.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryCriteriaIsNull_QueryAccountHolderPaymentPlanOptionsAsync_ThrowsArgumentNullExceptionTest()
            {
                var serializedResponse = JsonConvert.SerializeObject("");

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);
                await client.QueryAccountHolderPaymentPlanOptions2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task BillingTermsEmpty_QueryAccountHolderPaymentPlanOptionsAsync_ThrowsArgumentExceptionTest()
            {
                var serializedResponse = JsonConvert.SerializeObject("");

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);
                await client.QueryAccountHolderPaymentPlanOptions2Async(new PaymentPlanQueryCriteria() { BillingTerms = new List<BillingTermPaymentPlanInformation>() });
            }
        }

        [TestClass]
        public class GetProposedPaymentPlanAsync
        {
            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private IEnumerable<string> ids;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetProposedPaymentPlanAsync_ReturnsSerializedPaymentPlan()
            {
                string personId = "0001234";
                string termId = "2016/FA";
                string receivableTypeCode = "01";
                decimal planAmount = 1000m;

                // Arrange
                var plan = new PaymentPlan()
                {
                    Id = "1234",
                    PersonId = "0001234",
                    TermId = "2016/FA",
                    TemplateId = "DEFAULT"
                };

                var serializedResponse = JsonConvert.SerializeObject(plan);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, planAmount);

                // Assert
                Assert.IsNotNull(clientResponse);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetProposedPaymentPlanAsync_Null_PersonId()
            {
                string personId = "0001234";
                string termId = "2016/FA";
                string receivableTypeCode = "01";
                decimal planAmount = 1000m;

                // Arrange
                var plan = new PaymentPlan()
                {
                    Id = "1234",
                    PersonId = "0001234",
                    TermId = "2016/FA",
                    TemplateId = "DEFAULT"
                };

                var serializedResponse = JsonConvert.SerializeObject(plan);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetProposedPaymentPlanAsync(null, termId, receivableTypeCode, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetProposedPaymentPlanAsync_Null_TermId()
            {
                string personId = "0001234";
                string termId = "2016/FA";
                string receivableTypeCode = "01";
                decimal planAmount = 1000m;

                // Arrange
                var plan = new PaymentPlan()
                {
                    Id = "1234",
                    PersonId = "0001234",
                    TermId = "2016/FA",
                    TemplateId = "DEFAULT"
                };

                var serializedResponse = JsonConvert.SerializeObject(plan);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetProposedPaymentPlanAsync(personId, null, receivableTypeCode, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetProposedPaymentPlanAsync_Null_ReceivableTypeCode()
            {
                string personId = "0001234";
                string termId = "2016/FA";
                string receivableTypeCode = "01";
                decimal planAmount = 1000m;

                // Arrange
                var plan = new PaymentPlan()
                {
                    Id = "1234",
                    PersonId = "0001234",
                    TermId = "2016/FA",
                    TemplateId = "DEFAULT"
                };

                var serializedResponse = JsonConvert.SerializeObject(plan);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetProposedPaymentPlanAsync(personId, termId, null, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetProposedPaymentPlanAsync_Invalid_PlanAMount()
            {
                string personId = "0001234";
                string termId = "2016/FA";
                string receivableTypeCode = "01";
                decimal planAmount = 1000m;

                // Arrange
                var plan = new PaymentPlan()
                {
                    Id = "1234",
                    PersonId = "0001234",
                    TermId = "2016/FA",
                    TemplateId = "DEFAULT"
                };

                var serializedResponse = JsonConvert.SerializeObject(plan);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, -100m);
            }

        }

        [TestClass]
        public class GetChargeCodesAsync
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private string id;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;
            }

            [TestMethod]
            public async Task GetChargeCodesAsync_ReturnsSerializedChargeCodes()
            {
                // Arrange
                var chargeCodes = new List<ChargeCode>() { new ChargeCode() { Code = "MATFE", Description = "Materials Fee", Priority = 99, ChargeGroup = 2 } };

                var serializedResponse = JsonConvert.SerializeObject(chargeCodes);

                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetChargeCodesAsync();

                // Assert
                Assert.AreEqual(chargeCodes[0].Code, clientResponse.ToList()[0].Code);
                Assert.AreEqual(chargeCodes[0].Description, clientResponse.ToList()[0].Description);
                Assert.AreEqual(chargeCodes[0].ChargeGroup, clientResponse.ToList()[0].ChargeGroup);
                Assert.AreEqual(chargeCodes[0].Priority, clientResponse.ToList()[0].Priority);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task GetChargeCodesAsync_BadRequest()
            {
                // Arrange
                var chargeCodes = new List<ChargeCode>() { new ChargeCode() { Code = "MATFE", Description = "Materials Fee", Priority = 99, ChargeGroup = 2 } };

                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.GetChargeCodesAsync();
                _loggerMock.Verify();
            }
        }

        [TestClass]
        public class QueryStudentPotentialD7FinancialAid
        {

            private const string _serviceUrl = "http://service.url";
            private const string _contentType = "application/json";
            private const string _token = "1234567890";
            private string id;

            private Mock<ILogger> _loggerMock;
            private ILogger _logger;

            PotentialD7FinancialAidCriteria criteria;
            List<PotentialD7FinancialAid> potentialD7s;
            string serializedResponse;

            [TestInitialize]
            public void Initialize()
            {
                _loggerMock = MockLogger.Instance;

                _logger = _loggerMock.Object;

                criteria = new PotentialD7FinancialAidCriteria()
                {
                    StudentId = "Valid",
                    TermId = "Term",
                    AwardPeriodAwardsToEvaluate = new List<AwardPeriodAwardTransmitExcessStatus>()
                    {
                        new AwardPeriodAwardTransmitExcessStatus()
                        {
                           AwardPeriodAward = "Foo",
                           TransmitExcessIndicator = false,
                        }
                    }
                };

                potentialD7s = new List<PotentialD7FinancialAid>()
                {
                    new PotentialD7FinancialAid()
                    {
                        AwardPeriodAward = "Awdp*Award1",
                        AwardDescription = "Description for Awdp*Award1",
                        AwardAmount = 50m,
                    },
                };

                serializedResponse = JsonConvert.SerializeObject(potentialD7s);
            }

            /// <summary>
            /// Verify that valid input results in a valid response
            /// </summary>
            [TestMethod]
            public async Task QueryStudentPotentialD7FinancialAid_ValidResponse()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentPotentialD7FinancialAidAsync(criteria);

                // Assert
                Assert.AreEqual(potentialD7s[0].AwardPeriodAward, clientResponse.ToList()[0].AwardPeriodAward);
                Assert.AreEqual(potentialD7s[0].AwardDescription, clientResponse.ToList()[0].AwardDescription);
                Assert.AreEqual(potentialD7s[0].AwardAmount, clientResponse.ToList()[0].AwardAmount);
            }

            /// <summary>
            /// Verify that a bad request results in an exception
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(HttpRequestFailedException))]
            public async Task QueryStudentPotentialD7FinancialAid_BadRequest()
            {
                // Arrange
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                response.Content = new StringContent(string.Empty, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentPotentialD7FinancialAidAsync(criteria);
                _loggerMock.Verify();
            }

            /// <summary>
            /// Verify that a null input parameter results in an exception
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task QueryStudentPotentialD7FinancialAid_NullCriteria()
            {
                // Arrange
                var student = criteria.StudentId;
                criteria.StudentId = null;
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(serializedResponse, Encoding.UTF8, _contentType);
                response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://test");
                var mockHandler = new MockHandler();
                mockHandler.Responses.Enqueue(response);

                var testHttpClient = new HttpClient(mockHandler);
                testHttpClient.BaseAddress = new Uri(_serviceUrl);

                var client = new ColleagueApiClient(testHttpClient, _logger);

                // Act
                var clientResponse = await client.QueryStudentPotentialD7FinancialAidAsync(null);
                _loggerMock.Verify();
            }
        }
    }
}


