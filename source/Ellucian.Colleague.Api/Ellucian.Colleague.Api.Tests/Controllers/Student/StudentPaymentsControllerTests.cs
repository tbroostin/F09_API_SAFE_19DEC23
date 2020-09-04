// Copyright 2017-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;


namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    #region Student Payments V6
    [TestClass]
    public class StudentPaymentsControllerTests
    {

        public TestContext TestContext { get; set; }
        private StudentPaymentsController _studentPaymentController;
        private Mock<IStudentPaymentService> _studentPayementService;
        private Mock<ILogger> _loggerMock;

        private List<Dtos.StudentPayment> _studentPaymentsCollection;
        private Dtos.StudentPayment _studentPaymentsDto;

        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            _studentPayementService = new Mock<IStudentPaymentService>();
            _loggerMock = new Mock<ILogger>();


            _studentPaymentsCollection = new List<StudentPayment>();

            _studentPaymentsDto = new StudentPayment()
            {
                Id = "9a5a8793-c661-4c57-a47b-41a425c659c5",
                Person = new GuidObject2("e6857066-13a2-4316-981f-308d1474eabf"),
                AccountReceivableType = new GuidObject2("375c836b-cf4c-475e-bad4-c45d98bdc697"),
                AccountingCode = new GuidObject2("05cce1d0-c75c-40d7-9be0-88b61f2acfa6"),
                AcademicPeriod = new GuidObject2("1869dab7-12dc-4ea6-8c6d-8bedd36ebefe"),
                PaymentType = Dtos.EnumProperties.StudentPaymentTypes.sponsor,
                PaidOn = new DateTime(2015, 08, 31),
                Comments = new List<string>() { "this is a payment just testing the payment comment" },
                Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = 100m, Currency = Dtos.EnumProperties.CurrencyCodes.USD }
            };

            _studentPaymentsCollection.Add(_studentPaymentsDto);

            var tempDto = new StudentPayment()
            {
                Id = "0b9c531d-f3e7-4915-814b-ea6bdaec0907",
                Person = new GuidObject2("721d4f02-c7c9-4991-a8e9-8a3f755edadb"),
                AccountReceivableType = new GuidObject2("375c836b-cf4c-475e-bad4-c45d98bdc697"),
                AccountingCode = new GuidObject2("05cce1d0-c75c-40d7-9be0-88b61f2acfa6"),
                AcademicPeriod = new GuidObject2("1869dab7-12dc-4ea6-8c6d-8bedd36ebefe"),
                PaymentType = Dtos.EnumProperties.StudentPaymentTypes.sponsor,
                PaidOn = new DateTime(2015, 08, 31),
                Comments = new List<string>() { "this is a payment" },
                Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = 100m, Currency = Dtos.EnumProperties.CurrencyCodes.USD }
            };
            _studentPaymentsCollection.Add(tempDto);

            _studentPayementService.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            _studentPaymentController = new StudentPaymentsController(_studentPayementService.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentPaymentController = null;
            _studentPayementService = null;
            _loggerMock = null;
            _studentPaymentsCollection = null;
            _studentPaymentsDto = null;
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByIdAsync()
        {
            var expected = _studentPaymentsDto;
            _studentPayementService.Setup(x => x.GetByIdAsync(expected.Id)).ReturnsAsync(expected);

            var actual = await _studentPaymentController.GetByIdAsync(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
            Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
            Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
            Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
            Assert.AreEqual(expected.PaidOn, actual.PaidOn);
            Assert.AreEqual(expected.PaymentType, actual.PaymentType);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
            Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync_ValidateFields_Nocache()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var spTuple = new Tuple<IEnumerable<StudentPayment>, int>(_studentPaymentsCollection, _studentPaymentsCollection.Count);

            _studentPayementService.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(),
                false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync(It.IsAny<Paging>());

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync_ValidateFields_Cache()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var spTuple = new Tuple<IEnumerable<StudentPayment>, int>(_studentPaymentsCollection, _studentPaymentsCollection.Count);

            _studentPayementService.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(),
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync(It.IsAny<Paging>());

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            }
        }
        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync_ValidateFields_Paging()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            List<StudentPayment> newSp = new List<StudentPayment>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync(new Paging(1, 1));

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync_ValidateFields_Student()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            List<StudentPayment> newSp = new List<StudentPayment>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync(1, 1,
                true, _studentPaymentsDto.Person.Id, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync(new Paging(1, 1), _studentPaymentsDto.Person.Id);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync_ValidateFields_AcademicPeriod()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            List<StudentPayment> newSp = new List<StudentPayment>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync(1, 1,
                true, It.IsAny<string>(), _studentPaymentsDto.AcademicPeriod.Id, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync(new Paging(1, 1), "", _studentPaymentsDto.AcademicPeriod.Id);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync_ValidateFields_AccountingCode()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            List<StudentPayment> newSp = new List<StudentPayment>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), _studentPaymentsDto.AccountingCode.Id, It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync(new Paging(1, 1), "", "", _studentPaymentsDto.AccountingCode.Id);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync_ValidateFields_PaymentType()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            List<StudentPayment> newSp = new List<StudentPayment>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), "sponsor")).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync(new Paging(1, 1), "", "", "", "sponsor");

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
                Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_CreateAsync()
        {
            _studentPaymentsDto.Id = "00000000-0000-0000-0000-000000000000";
            _studentPayementService.Setup(x => x.CreateAsync(_studentPaymentsDto)).ReturnsAsync(_studentPaymentsDto);
            var actual = await _studentPaymentController.CreateAsync(_studentPaymentsDto);
            Assert.IsNotNull(actual);
            var expected = _studentPaymentsDto;

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
            Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
            Assert.AreEqual(expected.AccountingCode.Id, actual.AccountingCode.Id);
            Assert.AreEqual(expected.AccountReceivableType.Id, actual.AccountReceivableType.Id);
            Assert.AreEqual(expected.PaidOn, actual.PaidOn);
            Assert.AreEqual(expected.PaymentType, actual.PaymentType);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
            Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync_permissionError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Throws<PermissionsException>();
            await _studentPaymentController.GetByIdAsync(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync_ArgumentError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Throws<ArgumentException>();
            await _studentPaymentController.GetByIdAsync(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync_RepositoryError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Throws<RepositoryException>();
            await _studentPaymentController.GetByIdAsync(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync_UnknownError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync(It.IsAny<string>())).Throws<Exception>();
            await _studentPaymentController.GetByIdAsync(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync_PermissionError()
        {

            _studentPayementService.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<PermissionsException>();
            await _studentPaymentController.GetAsync(It.IsAny<Paging>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync_argumentError()
        {

            _studentPayementService.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<ArgumentException>();
            await _studentPaymentController.GetAsync(It.IsAny<Paging>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync_RepositoryError()
        {

            _studentPayementService.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<RepositoryException>();
            await _studentPaymentController.GetAsync(It.IsAny<Paging>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync_UnknownError()
        {

            _studentPayementService.Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();
            await _studentPaymentController.GetAsync(It.IsAny<Paging>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_PermissionError()
        {

            _studentPayementService.Setup(x => x.CreateAsync(It.IsAny<StudentPayment>())).Throws<PermissionsException>();
            await _studentPaymentController.CreateAsync(It.IsAny<StudentPayment>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_argumentError()
        {

            _studentPayementService.Setup(x => x.CreateAsync(It.IsAny<StudentPayment>())).Throws<ArgumentException>();
            await _studentPaymentController.CreateAsync(It.IsAny<StudentPayment>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_RepositoryError()
        {

            _studentPayementService.Setup(x => x.CreateAsync(It.IsAny<StudentPayment>())).Throws<RepositoryException>();
            await _studentPaymentController.CreateAsync(It.IsAny<StudentPayment>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_UnknownError()
        {

            _studentPayementService.Setup(x => x.CreateAsync(It.IsAny<StudentPayment>())).Throws<Exception>();
            await _studentPaymentController.CreateAsync(It.IsAny<StudentPayment>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_AcademicPeriodNull()
        {

            _studentPaymentsDto.AcademicPeriod = null;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_AcademicPeriodIdNull()
        {

            _studentPaymentsDto.AcademicPeriod.Id = null;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_AmountNull()
        {

            _studentPaymentsDto.Amount = null;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_AmtValueNull()
        {

            _studentPaymentsDto.Amount.Value = null;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_AmtZero()
        {

            _studentPaymentsDto.Amount.Value = 0;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_CurrencyCodes()
        {

            _studentPaymentsDto.Amount.Currency = null;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_PyamentType_notset()
        {

            _studentPaymentsDto.PaymentType = Dtos.EnumProperties.StudentPaymentTypes.notset;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_personNull()
        {

            _studentPaymentsDto.Person = null;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_personIdNull()
        {

            _studentPaymentsDto.Person = new GuidObject2();
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_accountingCode_IDnull()
        {

            _studentPaymentsDto.AccountingCode = new GuidObject2();
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync_Validate_accountingCode_sponsor()
        {
            _studentPaymentsDto.AccountingCode = null;
            _studentPaymentsDto.PaymentType = Dtos.EnumProperties.StudentPaymentTypes.sponsor;
            await _studentPaymentController.CreateAsync(_studentPaymentsDto);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_UpdateAsync()
        {
             await _studentPaymentController.UpdateAsync(_studentPaymentsDto.Id,_studentPaymentsDto);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_DeleteAsync()
        {
            await _studentPaymentController.DeleteAsync(_studentPaymentsDto.Id);

        }
    }

    #endregion

    #region Student Payments V11
    [TestClass]
    public class StudentPaymentsControllerTests_V11
    {

        public TestContext TestContext { get; set; }
        private StudentPaymentsController _studentPaymentController;
        private Mock<IStudentPaymentService> _studentPayementService;
        private Mock<ILogger> _loggerMock;

        private List<Dtos.StudentPayment2> _studentPaymentsCollection;
        private Dtos.StudentPayment2 _studentPaymentsDto;

        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            _studentPayementService = new Mock<IStudentPaymentService>();
            _loggerMock = new Mock<ILogger>();


            _studentPaymentsCollection = new List<StudentPayment2>();

            _studentPaymentsDto = new StudentPayment2()
            {
                Id = "9a5a8793-c661-4c57-a47b-41a425c659c5",
                Person = new GuidObject2("e6857066-13a2-4316-981f-308d1474eabf"),
                FundingDestination = new GuidObject2("375c836b-cf4c-475e-bad4-c45d98bdc697"),
                FundingSource = new GuidObject2("05cce1d0-c75c-40d7-9be0-88b61f2acfa6"),
                AcademicPeriod = new GuidObject2("1869dab7-12dc-4ea6-8c6d-8bedd36ebefe"),
                PaymentType = Dtos.EnumProperties.StudentPaymentTypes.sponsor,
                PaidOn = new DateTime(2015, 08, 31),
                Comments = new List<string>() { "this is a payment just testing the payment comment" },
                Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = 100m, Currency = Dtos.EnumProperties.CurrencyCodes.USD }
                //,GlPosting = Dtos.EnumProperties.GlPosting.notPosted
            };

            _studentPaymentsCollection.Add(_studentPaymentsDto);

            var tempDto = new StudentPayment2()
            {
                Id = "0b9c531d-f3e7-4915-814b-ea6bdaec0907",
                Person = new GuidObject2("721d4f02-c7c9-4991-a8e9-8a3f755edadb"),
                FundingDestination = new GuidObject2("375c836b-cf4c-475e-bad4-c45d98bdc697"),
                FundingSource = new GuidObject2("05cce1d0-c75c-40d7-9be0-88b61f2acfa6"),
                AcademicPeriod = new GuidObject2("1869dab7-12dc-4ea6-8c6d-8bedd36ebefe"),
                PaymentType = Dtos.EnumProperties.StudentPaymentTypes.sponsor,
                PaidOn = new DateTime(2015, 08, 31),
                Comments = new List<string>() { "this is a payment" },
                Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = 100m, Currency = Dtos.EnumProperties.CurrencyCodes.USD }
                //,GlPosting = Dtos.EnumProperties.GlPosting.posted
            };
            _studentPaymentsCollection.Add(tempDto);

            _studentPayementService.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            _studentPaymentController = new StudentPaymentsController(_studentPayementService.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentPaymentController = null;
            _studentPayementService = null;
            _loggerMock = null;
            _studentPaymentsCollection = null;
            _studentPaymentsDto = null;
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByIdAsync2()
        {
            var expected = _studentPaymentsDto;
            _studentPayementService.Setup(x => x.GetByIdAsync2(expected.Id)).ReturnsAsync(expected);

            var actual = await _studentPaymentController.GetByIdAsync2(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
            Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
            Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
            Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
            Assert.AreEqual(expected.PaidOn, actual.PaidOn);
            Assert.AreEqual(expected.PaymentType, actual.PaymentType);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
            Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_Nocache()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var spTuple = new Tuple<IEnumerable<StudentPayment2>, int>(_studentPaymentsCollection, _studentPaymentsCollection.Count);

            _studentPayementService.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(),
                false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync2(It.IsAny<Paging>(), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_Cache()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var spTuple = new Tuple<IEnumerable<StudentPayment2>, int>(_studentPaymentsCollection, _studentPaymentsCollection.Count);

            _studentPayementService.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(),
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync2(It.IsAny<Paging>(), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }
        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_Paging()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            List<StudentPayment2> newSp = new List<StudentPayment2>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment2>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync2(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync2(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_Student()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var filterGroupName = "criteria";
            _studentPaymentController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               string.Format("{{\"student\": {{\"id\":\"{0}\"}}}}", Guid.NewGuid()));

            List<StudentPayment2> newSp = new List<StudentPayment2>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment2>, int>(newSp, 1);
            
            _studentPayementService.Setup(x => x.GetAsync2(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync2(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_AcademicPeriod()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var filterGroupName = "criteria";
            _studentPaymentController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               string.Format("{{\"academicPeriod\": {{\"id\":\"{0}\"}}}}", Guid.NewGuid()));

            List<StudentPayment2> newSp = new List<StudentPayment2>()
            {
                _studentPaymentsDto
            };
            
            var spTuple = new Tuple<IEnumerable<StudentPayment2>, int>(newSp, 1);
         
            _studentPayementService.Setup(x => x.GetAsync2(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync2(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_FundingSource()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var filterGroupName = "criteria";
            _studentPaymentController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               string.Format("{{\"student\": {{\"id\":\"{0}\"}}}}", Guid.NewGuid()));

            List<StudentPayment2> newSp = new List<StudentPayment2>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment2>, int>(newSp, 1);
            
            _studentPayementService.Setup(x => x.GetAsync2(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync2(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_PaymentType()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var filterGroupName = "criteria";
            _studentPaymentController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               string.Format("{{\"paymentType\": \"sponsor\"}}"));
            List<StudentPayment2> newSp = new List<StudentPayment2>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment2>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync2(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync2(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment2>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment2>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_CreateAsync2()
        {
            _studentPaymentsDto.Id = "00000000-0000-0000-0000-000000000000";
            _studentPayementService.Setup(x => x.CreateAsync2(_studentPaymentsDto)).ReturnsAsync(_studentPaymentsDto);
            var actual = await _studentPaymentController.CreateAsync2(_studentPaymentsDto);
            Assert.IsNotNull(actual);
            var expected = _studentPaymentsDto;

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
            Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
            Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
            Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
            Assert.AreEqual(expected.PaidOn, actual.PaidOn);
            Assert.AreEqual(expected.PaymentType, actual.PaymentType);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
            Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            //Assert.AreEqual(expected.GlPosting, actual.GlPosting);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync2_permissionError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync2(It.IsAny<string>())).Throws<PermissionsException>();
            await _studentPaymentController.GetByIdAsync2(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync2_ArgumentError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync2(It.IsAny<string>())).Throws<ArgumentException>();
            await _studentPaymentController.GetByIdAsync2(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync2_RepositoryError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync2(It.IsAny<string>())).Throws<RepositoryException>();
            await _studentPaymentController.GetByIdAsync2(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync2_UnknownError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync2(It.IsAny<string>())).Throws<Exception>();
            await _studentPaymentController.GetByIdAsync2(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync2_PermissionError()
        {

            _studentPayementService.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<PermissionsException>();
            await _studentPaymentController.GetAsync2(It.IsAny<Paging>(), criteriaFilter);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync2_argumentError()
        {

            _studentPayementService.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<ArgumentException>();
            await _studentPaymentController.GetAsync2(It.IsAny<Paging>(), criteriaFilter);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync2_RepositoryError()
        {

            _studentPayementService.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<RepositoryException>();
            await _studentPaymentController.GetAsync2(It.IsAny<Paging>(), criteriaFilter);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync2_UnknownError()
        {

            _studentPayementService.Setup(x => x.GetAsync2(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();
            await _studentPaymentController.GetAsync2(It.IsAny<Paging>(), criteriaFilter);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_PermissionError()
        {

            _studentPayementService.Setup(x => x.CreateAsync2(It.IsAny<StudentPayment2>())).Throws<PermissionsException>();
            await _studentPaymentController.CreateAsync2(It.IsAny<StudentPayment2>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_argumentError()
        {

            _studentPayementService.Setup(x => x.CreateAsync2(It.IsAny<StudentPayment2>())).Throws<ArgumentException>();
            await _studentPaymentController.CreateAsync2(It.IsAny<StudentPayment2>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_RepositoryError()
        {

            _studentPayementService.Setup(x => x.CreateAsync2(It.IsAny<StudentPayment2>())).Throws<RepositoryException>();
            await _studentPaymentController.CreateAsync2(It.IsAny<StudentPayment2>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_UnknownError()
        {

            _studentPayementService.Setup(x => x.CreateAsync2(It.IsAny<StudentPayment2>())).Throws<Exception>();
            await _studentPaymentController.CreateAsync2(It.IsAny<StudentPayment2>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_AcademicPeriodNull()
        {

            _studentPaymentsDto.AcademicPeriod = null;
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_AcademicPeriodIdNull()
        {

            _studentPaymentsDto.AcademicPeriod.Id = null;
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_AmountNull()
        {

            _studentPaymentsDto.Amount = null;
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_AmtValueNull()
        {

            _studentPaymentsDto.Amount.Value = null;
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_AmtZero()
        {

            _studentPaymentsDto.Amount.Value = 0;
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_CurrencyCodes()
        {

            _studentPaymentsDto.Amount.Currency = null;
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_PyamentType_notset()
        {

            _studentPaymentsDto.PaymentType = Dtos.EnumProperties.StudentPaymentTypes.notset;
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_personNull()
        {

            _studentPaymentsDto.Person = null;
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_personIdNull()
        {

            _studentPaymentsDto.Person = new GuidObject2();
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync2_Validate_accountingCode_IDnull()
        {

            _studentPaymentsDto.FundingSource = new GuidObject2();
            await _studentPaymentController.CreateAsync2(_studentPaymentsDto);

        }
        
    }

    #endregion

    #region Student Payments V16.0.0
    [TestClass]
    public class StudentPaymentsControllerTests_V16_0_0
    {

        public TestContext TestContext { get; set; }
        private StudentPaymentsController _studentPaymentController;
        private Mock<IStudentPaymentService> _studentPayementService;
        private Mock<ILogger> _loggerMock;

        private List<Dtos.StudentPayment3> _studentPaymentsCollection;
        private Dtos.StudentPayment3 _studentPaymentsDto;

        private Ellucian.Web.Http.Models.QueryStringFilter criteriaFilter = new Web.Http.Models.QueryStringFilter("criteria", "");

        [TestInitialize]
        public async void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
            _studentPayementService = new Mock<IStudentPaymentService>();
            _loggerMock = new Mock<ILogger>();


            _studentPaymentsCollection = new List<StudentPayment3>();

            _studentPaymentsDto = new StudentPayment3()
            {
                Id = "9a5a8793-c661-4c57-a47b-41a425c659c5",
                Person = new GuidObject2("e6857066-13a2-4316-981f-308d1474eabf"),
                FundingDestination = new GuidObject2("375c836b-cf4c-475e-bad4-c45d98bdc697"),
                FundingSource = new GuidObject2("05cce1d0-c75c-40d7-9be0-88b61f2acfa6"),
                AcademicPeriod = new GuidObject2("1869dab7-12dc-4ea6-8c6d-8bedd36ebefe"),
                PaymentType = Dtos.EnumProperties.StudentPaymentTypes2.sponsor,
                PaidOn = new DateTime(2015, 08, 31),
                Comments = new List<string>() { "this is a payment just testing the payment comment" },
                Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = 100m, Currency = Dtos.EnumProperties.CurrencyCodes.USD }
                //,GlPosting = Dtos.EnumProperties.GlPosting.notPosted
            };

            _studentPaymentsCollection.Add(_studentPaymentsDto);

            var tempDto = new StudentPayment3()
            {
                Id = "0b9c531d-f3e7-4915-814b-ea6bdaec0907",
                Person = new GuidObject2("721d4f02-c7c9-4991-a8e9-8a3f755edadb"),
                FundingDestination = new GuidObject2("375c836b-cf4c-475e-bad4-c45d98bdc697"),
                FundingSource = new GuidObject2("05cce1d0-c75c-40d7-9be0-88b61f2acfa6"),
                AcademicPeriod = new GuidObject2("1869dab7-12dc-4ea6-8c6d-8bedd36ebefe"),
                PaymentType = Dtos.EnumProperties.StudentPaymentTypes2.sponsor,
                PaidOn = new DateTime(2015, 08, 31),
                Comments = new List<string>() { "this is a payment" },
                Amount = new Dtos.DtoProperties.AmountDtoProperty() { Value = 100m, Currency = Dtos.EnumProperties.CurrencyCodes.USD }
                //,GlPosting = Dtos.EnumProperties.GlPosting.posted
            };
            _studentPaymentsCollection.Add(tempDto);

            _studentPayementService.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());
            _studentPaymentController = new StudentPaymentsController(_studentPayementService.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _studentPaymentController = null;
            _studentPayementService = null;
            _loggerMock = null;
            _studentPaymentsCollection = null;
            _studentPaymentsDto = null;
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByIdAsync3()
        {
            var expected = _studentPaymentsDto;
            _studentPayementService.Setup(x => x.GetByIdAsync3(expected.Id)).ReturnsAsync(expected);

            var actual = await _studentPaymentController.GetByIdAsync3(expected.Id);

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
            Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
            Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
            Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
            Assert.AreEqual(expected.PaidOn, actual.PaidOn);
            Assert.AreEqual(expected.PaymentType, actual.PaymentType);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
            Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_Nocache()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var spTuple = new Tuple<IEnumerable<StudentPayment3>, int>(_studentPaymentsCollection, _studentPaymentsCollection.Count);

            _studentPayementService.Setup(x => x.GetAsync3(It.IsAny<int>(), It.IsAny<int>(),
                false, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync3(It.IsAny<Paging>(), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment3>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment3>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_Cache()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            var spTuple = new Tuple<IEnumerable<StudentPayment3>, int>(_studentPaymentsCollection, _studentPaymentsCollection.Count);

            _studentPayementService.Setup(x => x.GetAsync3(It.IsAny<int>(), It.IsAny<int>(),
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync3(It.IsAny<Paging>(), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment3>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment3>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }
        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_Paging()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            List<StudentPayment3> newSp = new List<StudentPayment3>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment3>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync3(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync3(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment3>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment3>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_Student()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var filterGroupName = "criteria";
            _studentPaymentController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               string.Format("{{\"student\": {{\"id\":\"{0}\"}}}}", Guid.NewGuid()));

            List<StudentPayment3> newSp = new List<StudentPayment3>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment3>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync3(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync3(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment3>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment3>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_AcademicPeriod()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var filterGroupName = "criteria";
            _studentPaymentController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               string.Format("{{\"academicPeriod\": {{\"id\":\"{0}\"}}}}", Guid.NewGuid()));

            List<StudentPayment3> newSp = new List<StudentPayment3>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment3>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync3(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync3(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment3>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment3>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_FundingSource()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var filterGroupName = "criteria";
            _studentPaymentController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               string.Format("{{\"student\": {{\"id\":\"{0}\"}}}}", Guid.NewGuid()));

            List<StudentPayment3> newSp = new List<StudentPayment3>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment3>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync3(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync3(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment3>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment3>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_GetByAsync2_ValidateFields_PaymentType()
        {
            _studentPaymentController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

            _studentPaymentController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            _studentPaymentController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            var filterGroupName = "criteria";
            _studentPaymentController.Request.Properties.Add(
               string.Format("FilterObject{0}", filterGroupName),
               string.Format("{{\"paymentType\": \"sponsor\"}}"));
            List<StudentPayment3> newSp = new List<StudentPayment3>()
            {
                _studentPaymentsDto
            };

            var spTuple = new Tuple<IEnumerable<StudentPayment3>, int>(newSp, 1);

            _studentPayementService.Setup(x => x.GetAsync3(1, 1,
                true, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(spTuple);

            var sourceContexts = await _studentPaymentController.GetAsync3(new Paging(1, 1), criteriaFilter);

            var cancelToken = new System.Threading.CancellationToken(false);
            HttpResponseMessage httpResponseMessage = await sourceContexts.ExecuteAsync(cancelToken);

            var actuals = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.StudentPayment3>>)httpResponseMessage.Content)
                .Value as IEnumerable<Dtos.StudentPayment3>;

            Assert.IsNotNull(actuals);
            foreach (var actual in actuals)
            {
                var expected = _studentPaymentsCollection.FirstOrDefault(i => i.Id == actual.Id);

                Assert.AreEqual(expected.Id, actual.Id);
                Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
                Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
                Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
                Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
                Assert.AreEqual(expected.PaidOn, actual.PaidOn);
                Assert.AreEqual(expected.PaymentType, actual.PaymentType);
                Assert.AreEqual(expected.Person.Id, actual.Person.Id);
                Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
                Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
                //Assert.AreEqual(expected.GlPosting, actual.GlPosting);
            }
        }

        [TestMethod]
        public async Task StudentPaymentsController_CreateAsync3()
        {
            _studentPaymentsDto.Id = "00000000-0000-0000-0000-000000000000";
            _studentPayementService.Setup(x => x.CreateAsync3(_studentPaymentsDto)).ReturnsAsync(_studentPaymentsDto);
            var actual = await _studentPaymentController.CreateAsync3(_studentPaymentsDto);
            Assert.IsNotNull(actual);
            var expected = _studentPaymentsDto;

            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Comments[0], actual.Comments[0]);
            Assert.AreEqual(expected.AcademicPeriod.Id, actual.AcademicPeriod.Id);
            Assert.AreEqual(expected.FundingDestination.Id, actual.FundingDestination.Id);
            Assert.AreEqual(expected.FundingSource.Id, actual.FundingSource.Id);
            Assert.AreEqual(expected.PaidOn, actual.PaidOn);
            Assert.AreEqual(expected.PaymentType, actual.PaymentType);
            Assert.AreEqual(expected.Person.Id, actual.Person.Id);
            Assert.AreEqual(expected.Amount.Value, actual.Amount.Value);
            Assert.AreEqual(expected.Amount.Currency, actual.Amount.Currency);
            //Assert.AreEqual(expected.GlPosting, actual.GlPosting);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync3_permissionError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync3(It.IsAny<string>())).Throws<PermissionsException>();
            await _studentPaymentController.GetByIdAsync3(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync3_ArgumentError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync3(It.IsAny<string>())).Throws<ArgumentException>();
            await _studentPaymentController.GetByIdAsync3(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync3_RepositoryError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync3(It.IsAny<string>())).Throws<RepositoryException>();
            await _studentPaymentController.GetByIdAsync3(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetByIdAsync3_UnknownError()
        {

            _studentPayementService.Setup(x => x.GetByIdAsync3(It.IsAny<string>())).Throws<Exception>();
            await _studentPaymentController.GetByIdAsync3(It.IsAny<string>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync3_PermissionError()
        {

            _studentPayementService.Setup(x => x.GetAsync3(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<PermissionsException>();
            await _studentPaymentController.GetAsync3(It.IsAny<Paging>(), criteriaFilter);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync3_argumentError()
        {

            _studentPayementService.Setup(x => x.GetAsync3(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<ArgumentException>();
            await _studentPaymentController.GetAsync3(It.IsAny<Paging>(), criteriaFilter);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync3_RepositoryError()
        {

            _studentPayementService.Setup(x => x.GetAsync3(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<RepositoryException>();
            await _studentPaymentController.GetAsync3(It.IsAny<Paging>(), criteriaFilter);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_GetAsync3_UnknownError()
        {

            _studentPayementService.Setup(x => x.GetAsync3(It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<Exception>();
            await _studentPaymentController.GetAsync3(It.IsAny<Paging>(), criteriaFilter);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_PermissionError()
        {

            _studentPayementService.Setup(x => x.CreateAsync3(It.IsAny<StudentPayment3>())).Throws<PermissionsException>();
            await _studentPaymentController.CreateAsync3(It.IsAny<StudentPayment3>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_argumentError()
        {

            _studentPayementService.Setup(x => x.CreateAsync3(It.IsAny<StudentPayment3>())).Throws<ArgumentException>();
            await _studentPaymentController.CreateAsync3(It.IsAny<StudentPayment3>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_RepositoryError()
        {

            _studentPayementService.Setup(x => x.CreateAsync3(It.IsAny<StudentPayment3>())).Throws<RepositoryException>();
            await _studentPaymentController.CreateAsync3(It.IsAny<StudentPayment3>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_UnknownError()
        {

            _studentPayementService.Setup(x => x.CreateAsync3(It.IsAny<StudentPayment3>())).Throws<Exception>();
            await _studentPaymentController.CreateAsync3(It.IsAny<StudentPayment3>());

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_AcademicPeriodNull()
        {

            _studentPaymentsDto.AcademicPeriod = null;
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_AcademicPeriodIdNull()
        {

            _studentPaymentsDto.AcademicPeriod.Id = null;
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_AmountNull()
        {

            _studentPaymentsDto.Amount = null;
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_AmtValueNull()
        {

            _studentPaymentsDto.Amount.Value = null;
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_AmtZero()
        {

            _studentPaymentsDto.Amount.Value = 0;
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_CurrencyCodes()
        {

            _studentPaymentsDto.Amount.Currency = null;
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_PyamentType_notset()
        {

            _studentPaymentsDto.PaymentType = Dtos.EnumProperties.StudentPaymentTypes2.notset;
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_personNull()
        {

            _studentPaymentsDto.Person = null;
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_personIdNull()
        {

            _studentPaymentsDto.Person = new GuidObject2();
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task StudentPaymentsController_CreateAsync3_Validate_accountingCode_IDnull()
        {

            _studentPaymentsDto.FundingSource = new GuidObject2();
            await _studentPaymentController.CreateAsync3(_studentPaymentsDto);

        }

    }

    #endregion
}
