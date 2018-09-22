//Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsGet
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction _generalLedgerTransaction;
        private List<GeneralLedgerTransaction> _generalLedgerTransactions;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 }
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty>() { generalLedgerDetailDtoProperty }
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                Transactions = new List<GeneralLedgerTransactionDtoProperty>() { gltDtoProperty }
            };

            _generalLedgerTransactions = new List<GeneralLedgerTransaction>() { _generalLedgerTransaction };

            _response = new HttpResponse(new StringWriter());

            _glServiceMock.Setup(x => x.GetAsync()).ReturnsAsync(_generalLedgerTransactions);
            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            _generalLedgerTransactionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_GetAsync_Valid()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ReturnsAsync(_generalLedgerTransactions);
            var generalLedgerTransaction = await _generalLedgerTransactionsController.GetAsync();
            Assert.IsNotNull(generalLedgerTransaction);
        }



        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_PermissionsException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new PermissionsException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_KeyNotFoundException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new KeyNotFoundException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_ArgumentNullException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new ArgumentNullException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_IntegrationApiException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new IntegrationApiException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_ConfigurationException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new ConfigurationException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_ArgumentOutOfRangeException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new ArgumentOutOfRangeException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_ArgumentException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new ArgumentException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_InvalidOperationException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new InvalidOperationException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_FormatException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new FormatException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_RepositoryException()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new RepositoryException());
            await _generalLedgerTransactionsController.GetAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetAsync_Exception()
        {
            _glServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new Exception());
            await _generalLedgerTransactionsController.GetAsync();
        }


    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsGet2
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction2 _generalLedgerTransaction;
        private List<GeneralLedgerTransaction2> _generalLedgerTransactions;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 },
                SubmittedBy = new GuidObject2() { Id= "123456" }
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty2()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty2>() { generalLedgerDetailDtoProperty }
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction2
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                Transactions = new List<GeneralLedgerTransactionDtoProperty2>() { gltDtoProperty },
                SubmittedBy = new GuidObject2() { Id= "123456" }
            };

            _generalLedgerTransactions = new List<GeneralLedgerTransaction2>() { _generalLedgerTransaction };
            
            _response = new HttpResponse(new StringWriter());

            _glServiceMock.Setup(x => x.Get2Async()).ReturnsAsync(_generalLedgerTransactions);
            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            _generalLedgerTransactionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_Get2Async_Valid()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ReturnsAsync(_generalLedgerTransactions);
            var generalLedgerTransaction = await _generalLedgerTransactionsController.GetAsync();
            Assert.IsNotNull(generalLedgerTransaction);
        }



        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_PermissionsException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new PermissionsException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_KeyNotFoundException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new KeyNotFoundException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_ArgumentNullException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new ArgumentNullException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_IntegrationApiException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new IntegrationApiException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_ConfigurationException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new ConfigurationException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_ArgumentOutOfRangeException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new ArgumentOutOfRangeException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_ArgumentException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new ArgumentException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_InvalidOperationException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new InvalidOperationException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_FormatException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new FormatException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_RepositoryException()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new RepositoryException());
            await _generalLedgerTransactionsController.Get2Async();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Get2Async_Exception()
        {
            _glServiceMock.Setup(x => x.Get2Async()).ThrowsAsync(new Exception());
            await _generalLedgerTransactionsController.Get2Async();
        }


    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsGetById
    {

        #region Test Context

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction _generalLedgerTransaction;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() {Currency = CurrencyCodes.USD, Value = 25}
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty>() {generalLedgerDetailDtoProperty}
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                Transactions = new List<GeneralLedgerTransactionDtoProperty>() {gltDtoProperty}
            };

            _response = new HttpResponse(new StringWriter());

            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_generalLedgerTransaction);
            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            _generalLedgerTransactionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_Valid()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_generalLedgerTransaction);
            var generalLedgerTransaction = await _generalLedgerTransactionsController.GetByIdAsync("0001234");
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_EmptyID()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_generalLedgerTransaction);
            await _generalLedgerTransactionsController.GetByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_NullID()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(_generalLedgerTransaction);
            await _generalLedgerTransactionsController.GetByIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_PermissionsException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_KeyNotFoundException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_ArgumentNullException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_IntegrationApiException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_ConfigurationException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new ConfigurationException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_ArgumentOutOfRangeException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentOutOfRangeException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_ArgumentException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_InvalidOperationException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_FormatException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new FormatException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_RepositoryException()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetByIdAsync_Exception()
        {
            _glServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await _generalLedgerTransactionsController.GetByIdAsync("0001234");
        }

    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsGetById2
    {

        #region Test Context

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction2 _generalLedgerTransaction;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 },
                SubmittedBy = new GuidObject2() { Id = "123456" }
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty2()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty2>() { generalLedgerDetailDtoProperty }
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction2
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                SubmittedBy = new GuidObject2() { Id= "123456" },
                Transactions = new List<GeneralLedgerTransactionDtoProperty2>() { gltDtoProperty }
            };

            _response = new HttpResponse(new StringWriter());

            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ReturnsAsync(_generalLedgerTransaction);
            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            _generalLedgerTransactionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_GetById2Async_Valid()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ReturnsAsync(_generalLedgerTransaction);
            var generalLedgerTransaction = await _generalLedgerTransactionsController.GetById2Async("0001234");
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_EmptyID()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ReturnsAsync(_generalLedgerTransaction);
            await _generalLedgerTransactionsController.GetById2Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_NullID()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ReturnsAsync(_generalLedgerTransaction);
            await _generalLedgerTransactionsController.GetById2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_PermissionsException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_KeyNotFoundException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_ArgumentNullException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_IntegrationApiException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_ConfigurationException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new ConfigurationException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_ArgumentOutOfRangeException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new ArgumentOutOfRangeException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_ArgumentException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_InvalidOperationException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_FormatException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new FormatException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_RepositoryException()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_GetById2Async_Exception()
        {
            _glServiceMock.Setup(x => x.GetById2Async(It.IsAny<string>())).ThrowsAsync(new Exception());
            await _generalLedgerTransactionsController.GetById2Async("0001234");
        }

    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsCreate
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction _generalLedgerTransaction;
        private List<GeneralLedgerTransaction> _generalLedgerTransactions;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 }
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty>() { generalLedgerDetailDtoProperty }
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                Transactions = new List<GeneralLedgerTransactionDtoProperty>() { gltDtoProperty }
            };

            _generalLedgerTransactions = new List<GeneralLedgerTransaction>() { _generalLedgerTransaction };

            _response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), _response);

            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ReturnsAsync(_generalLedgerTransaction);
            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_CreateAsync_Valid()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_NullBody()
        {
            await _generalLedgerTransactionsController.CreateAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_PermissionsException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new PermissionsException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_KeyNotFoundException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new KeyNotFoundException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_ArgumentNullException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new ArgumentNullException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_IntegrationApiException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new IntegrationApiException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_ConfigurationException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new ConfigurationException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_ArgumentOutOfRangeException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new ArgumentOutOfRangeException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_ArgumentException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new ArgumentException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_InvalidOperationException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new InvalidOperationException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_FormatException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new FormatException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_RepositoryException()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new RepositoryException());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_CreateAsync_Exception()
        {
            _glServiceMock.Setup(x => x.CreateAsync(It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new Exception());
            await _generalLedgerTransactionsController.CreateAsync(_generalLedgerTransaction);
        }

    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsCreate2
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction2 _generalLedgerTransaction;
        private List<GeneralLedgerTransaction2> _generalLedgerTransactions;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 }
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty2()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty2>() { generalLedgerDetailDtoProperty }
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction2
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Transactions = new List<GeneralLedgerTransactionDtoProperty2>() { gltDtoProperty }
            };

            _generalLedgerTransactions = new List<GeneralLedgerTransaction2>() { _generalLedgerTransaction };

            _response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), _response);

            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ReturnsAsync(_generalLedgerTransaction);
            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_Create2Async_Valid()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_NullBody()
        {
            await _generalLedgerTransactionsController.Create2Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_PermissionsException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new PermissionsException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_KeyNotFoundException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new KeyNotFoundException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_ArgumentNullException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new ArgumentNullException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_IntegrationApiException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new IntegrationApiException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Creat2eAsync_ConfigurationException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new ConfigurationException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_ArgumentOutOfRangeException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new ArgumentOutOfRangeException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_ArgumentException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new ArgumentException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_InvalidOperationException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new InvalidOperationException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_FormatException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new FormatException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_RepositoryException()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new RepositoryException());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Create2Async_Exception()
        {
            _glServiceMock.Setup(x => x.Create2Async(It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new Exception());
            await _generalLedgerTransactionsController.Create2Async(_generalLedgerTransaction);
        }

    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsUpdate
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction _generalLedgerTransaction;
        private List<GeneralLedgerTransaction> _generalLedgerTransactions;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 }
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty>() { generalLedgerDetailDtoProperty }
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                Transactions = new List<GeneralLedgerTransactionDtoProperty>() { gltDtoProperty }
            };

            _generalLedgerTransactions = new List<GeneralLedgerTransaction>() { _generalLedgerTransaction };

            _response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), _response);

            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ReturnsAsync(_generalLedgerTransaction);
            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_Valid()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_Valid_NoGUD()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ReturnsAsync(_generalLedgerTransaction);
            var id = _generalLedgerTransaction.Id;
            _generalLedgerTransaction.Id = null;
            var generalLedgerTransaction = await _generalLedgerTransactionsController.UpdateAsync(id, _generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_EmptyId()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.UpdateAsync("", _generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_NullId()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.UpdateAsync(null, _generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_NullBody()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, null);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_PermissionsException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new PermissionsException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_KeyNotFoundException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new KeyNotFoundException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_ArgumentNullException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new ArgumentNullException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_IntegrationApiException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new IntegrationApiException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_ConfigurationException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new ConfigurationException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_ArgumentOutOfRangeException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new ArgumentOutOfRangeException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_ArgumentException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new ArgumentException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_InvalidOperationException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new InvalidOperationException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_FormatException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new FormatException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_RepositoryException()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new RepositoryException());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_UpdateAsync_Exception()
        {
            _glServiceMock.Setup(x => x.UpdateAsync(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction>())).ThrowsAsync(new Exception());
            await _generalLedgerTransactionsController.UpdateAsync(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsUpdate2
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction2 _generalLedgerTransaction;
        private List<GeneralLedgerTransaction2> _generalLedgerTransactions;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty2()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 }
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty2()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty2>() { generalLedgerDetailDtoProperty }
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction2
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                SubmittedBy = new GuidObject2() { Id = "123456" },
                Transactions = new List<GeneralLedgerTransactionDtoProperty2>() { gltDtoProperty }
            };

            _generalLedgerTransactions = new List<GeneralLedgerTransaction2>() { _generalLedgerTransaction };

            _response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), _response);

            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ReturnsAsync(_generalLedgerTransaction);
            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_Update2Async_Valid()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        public async Task GeneralLedgerTransactionsController_Update2Async_Valid_NoGUD()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ReturnsAsync(_generalLedgerTransaction);
            var id = _generalLedgerTransaction.Id;
            _generalLedgerTransaction.Id = null;
            var generalLedgerTransaction = await _generalLedgerTransactionsController.Update2Async(id, _generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_EmptyId()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.Update2Async("", _generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_NullId()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.Update2Async(null, _generalLedgerTransaction);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_NullBody()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ReturnsAsync(_generalLedgerTransaction);

            var generalLedgerTransaction = await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, null);
            Assert.IsNotNull(generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_PermissionsException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new PermissionsException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_KeyNotFoundException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new KeyNotFoundException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_ArgumentNullException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new ArgumentNullException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_IntegrationApiException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new IntegrationApiException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_ConfigurationException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new ConfigurationException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_ArgumentOutOfRangeException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new ArgumentOutOfRangeException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_ArgumentException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new ArgumentException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_InvalidOperationException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new InvalidOperationException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_FormatException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new FormatException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_RepositoryException()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new RepositoryException());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_Update2Async_Exception()
        {
            _glServiceMock.Setup(x => x.Update2Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction2>())).ThrowsAsync(new Exception());
            await _generalLedgerTransactionsController.Update2Async(_generalLedgerTransaction.Id, _generalLedgerTransaction);
        }

    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTestsDelete
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IGeneralLedgerTransactionService> _glServiceMock;

        private GeneralLedgerTransaction _generalLedgerTransaction;
        private List<GeneralLedgerTransaction> _generalLedgerTransactions;
        private HttpResponse _response;

        private GeneralLedgerTransactionsController _generalLedgerTransactionsController;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _glServiceMock = new Mock<IGeneralLedgerTransactionService>();

            var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty()
            {
                AccountingString = "784545",
                Description = "description",
                SequenceNumber = 1,
                Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 }
            };

            var gltDtoProperty = new GeneralLedgerTransactionDtoProperty()
            {
                LedgerDate = DateTimeOffset.Now.DateTime,
                ReferenceNumber = "GL122312321",
                Reference = new GeneralLedgerReferenceDtoProperty()
                {
                    Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                    Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                },
                TransactionNumber = "1",
                TransactionTypeReferenceDate = DateTimeOffset.Now,
                Type = GeneralLedgerTransactionType.ActualOpenBalance,
                TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty>() { generalLedgerDetailDtoProperty }
            };

            _generalLedgerTransaction = new GeneralLedgerTransaction
            {
                Id = "0001234",
                ProcessMode = ProcessMode.Update,
                Transactions = new List<GeneralLedgerTransactionDtoProperty>() { gltDtoProperty }
            };

            _generalLedgerTransactions = new List<GeneralLedgerTransaction>() { _generalLedgerTransaction };

            _response = new HttpResponse(new StringWriter());
            HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), _response);

            _generalLedgerTransactionsController = new GeneralLedgerTransactionsController(_glServiceMock.Object,
                _loggerMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _glServiceMock = null;
            _generalLedgerTransactionsController = null;
        }

        [TestMethod]
        [ExpectedException(typeof (HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_DeleteAsync_EmptyId()
        {
            await _generalLedgerTransactionsController.DeleteAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GeneralLedgerTransactionsController_DeleteAsync_NullId()
        {
            await _generalLedgerTransactionsController.DeleteAsync(null);
        }
    }

    [TestClass]
    public class GeneralLedgerTransactionsControllerTests_V12
    {
        [TestClass]
        public class GeneralLedgerTransactionsControllerTests_GET_POST_PUT
        {
            #region DECLARATIONS
            public TestContext TestContext { get; set; }
            private GeneralLedgerTransactionsController generalLedgerTransactionsController;
            private Mock<IGeneralLedgerTransactionService> generalLedgerTransactionServiceMock;
            private Mock<ILogger> loggerMock;

            private IEnumerable<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerTransaction> allGeneralLedgerTransactions;
            private List<Dtos.GeneralLedgerTransaction3> generalLedgerTransactionCollection;
            
            private Dtos.GeneralLedgerTransaction3 generalLedgerTransaction;
            private List<GeneralLedgerTransaction3> generalLedgerTransactions;

            private IEnumerable<Dtos.GeneralLedgerTransaction3> generalLedgerTransactionsCollection;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                generalLedgerTransactionServiceMock = new Mock<IGeneralLedgerTransactionService>();
                loggerMock = new Mock<ILogger>();

                InitializeTestData();
                
                generalLedgerTransactionsController = new GeneralLedgerTransactionsController(generalLedgerTransactionServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                generalLedgerTransactionsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                generalLedgerTransactionsController.Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);
                generalLedgerTransactionServiceMock.Setup(x => x.GetById3Async(It.IsAny<string>())).ReturnsAsync(generalLedgerTransaction);
                generalLedgerTransactionServiceMock.Setup(x => x.Get3Async(It.IsAny<bool>())).ReturnsAsync(generalLedgerTransactionCollection);

            }
            [TestCleanup]
            public void Cleanup()
            {
                generalLedgerTransactionsController = null;
                generalLedgerTransactionServiceMock = null;
                loggerMock = null;
                TestContext = null;
            }

            private void InitializeTestData()
            {
                generalLedgerTransactionCollection = new List<Dtos.GeneralLedgerTransaction3>();

                allGeneralLedgerTransactions = new List<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerTransaction>()
                {
                    new Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerTransaction(){ Id="849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d" },
                    new Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerTransaction(){ Id="849e6a7c-6cd4-4f98-8a73-ab0aa3627f01" },
                    new Ellucian.Colleague.Domain.ColleagueFinance.Entities.GeneralLedgerTransaction(){ Id="849e6a7c-6cd4-4f98-8a73-ab0aa3627f02" }
                };

                
                generalLedgerTransactionsCollection = new List<GeneralLedgerTransaction3>() {
                    new GeneralLedgerTransaction3(){ Id="2a082180-b897-46f3-8435-df25caaca921"},
                    new GeneralLedgerTransaction3(){ Id="2a082180-b897-46f3-8435-df25caaca922"},
                    new GeneralLedgerTransaction3(){ Id="2a082180-b897-46f3-8435-df25caaca923"}
                };

                

                foreach (var source in allGeneralLedgerTransactions)
                {
                    var _generalLedgerTransactions = new Ellucian.Colleague.Dtos.GeneralLedgerTransaction3
                    {
                        Id = source.Id
                    };
                    generalLedgerTransactionCollection.Add(_generalLedgerTransactions);
                }


                var generalLedgerDetailDtoProperty = new GeneralLedgerDetailDtoProperty3()
                {
                    AccountingString = "784545",
                    Description = "description",
                    SequenceNumber = 1,
                    Amount = new AmountDtoProperty() { Currency = CurrencyCodes.USD, Value = 25 },
                    SubmittedBy = new GuidObject2() { Id = "123456" }
                };

                var gltDtoProperty = new GeneralLedgerTransactionDtoProperty3()
                {
                    LedgerDate = DateTimeOffset.Now.DateTime,
                    ReferenceNumber = "GL122312321",
                    Reference = new GeneralLedgerReferenceDtoProperty()
                    {
                        Organization = new GuidObject2("B17F7796-53D1-403C-A883-934D4DE04F1D"),
                        Person = new GuidObject2("C17F7796-53D1-403C-A883-934D4DE04F1D")
                    },
                    TransactionNumber = "1",
                    TransactionTypeReferenceDate = DateTimeOffset.Now,
                    Type = GeneralLedgerTransactionType.ActualOpenBalance,
                    TransactionDetailLines = new List<GeneralLedgerDetailDtoProperty3>() { generalLedgerDetailDtoProperty }
                };
                
                generalLedgerTransaction = new GeneralLedgerTransaction3
                { 
                    Id = "2a082180-b897-46f3-8435-df25caaca922"
                };

                generalLedgerTransactions = new List<GeneralLedgerTransaction3>() {  new GeneralLedgerTransaction3
                {
                    Id = "2a082180-b897-46f3-8435-df25caaca921"
                },
                 new GeneralLedgerTransaction3
                {
                    Id = "2a082180-b897-46f3-8435-df25caaca922"
                }
            };

            }

            #endregion

            #region GETALL

            [TestMethod]
            public async Task GeneralLedgerTransactionsController_Get3Async()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ReturnsAsync(generalLedgerTransactionsCollection);
                var generalLedgerTransactionRes = await generalLedgerTransactionsController.Get3Async();
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_PermissionsException()
            {
                generalLedgerTransactionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_KeyNotFoundException()
            {
                generalLedgerTransactionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = false, Public = true };
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_ArgumentNullException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new ArgumentNullException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_IntegrationApiException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new IntegrationApiException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_ConfigurationException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new ConfigurationException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_ArgumentOutOfRangeException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new ArgumentOutOfRangeException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_ArgumentException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new ArgumentException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_InvalidOperationException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new InvalidOperationException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_FormatException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new FormatException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_RepositoryException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new RepositoryException());
                await generalLedgerTransactionsController.Get3Async();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Get3Async_Exception()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.Get3Async(It.IsAny<bool>())).ThrowsAsync(new Exception());
                await generalLedgerTransactionsController.Get3Async();
            }

            #endregion

            #region GETBYID

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_EmptyId()
            {
                generalLedgerTransactionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ReturnsAsync(generalLedgerTransaction);
                await generalLedgerTransactionsController.GetById3Async("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_PermissionException()
            {
                generalLedgerTransactionsController.Request.Headers.CacheControl = new CacheControlHeaderValue { NoCache = true, Public = true };
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_KeyNotFoundException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionsController_GetById3Async()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.GetById3Async(It.IsAny<string>())).ReturnsAsync(generalLedgerTransaction);
                var generalLedgerTransactionRes = await generalLedgerTransactionsController.GetById3Async("0001234");
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_ArgumentNullException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_IntegrationApiException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new IntegrationApiException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_ConfigurationException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new ConfigurationException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_ArgumentOutOfRangeException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new ArgumentOutOfRangeException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_ArgumentException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new ArgumentException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_InvalidOperationException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_FormatException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new FormatException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_RepositoryException()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_GetById3Async_Exception()
            {
                generalLedgerTransactionServiceMock.Setup(e => e.GetById3Async(It.IsAny<string>())).ThrowsAsync(new Exception());
                await generalLedgerTransactionsController.GetById3Async("2a082180-b897-46f3-8435-df25caaca922");
            }

            #endregion

            #region POST

            [TestMethod]
            public async Task GeneralLedgerTransactionsController_Create3Async_Valid()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };

                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);

                var generalLedgerTransactionRes = await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_EmptyGuidValid()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = ""
                };

                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);

                var generalLedgerTransactionRes = await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_NullBody()
            {
                await generalLedgerTransactionsController.Create3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_PermissionsException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };

                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new PermissionsException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_KeyNotFoundException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new KeyNotFoundException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_ArgumentNullException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new ArgumentNullException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_IntegrationApiException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new IntegrationApiException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_ConfigurationException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new ConfigurationException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_ArgumentOutOfRangeException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new ArgumentOutOfRangeException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_ArgumentException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new ArgumentException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_InvalidOperationException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new InvalidOperationException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_FormatException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new FormatException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_RepositoryException()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new RepositoryException());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Create3Async_Exception()
            {
                generalLedgerTransaction = new GeneralLedgerTransaction3
                {
                    Id = "00000000-0000-0000-0000-000000000000"
                };
                generalLedgerTransactionServiceMock.Setup(x => x.Create3Async(It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new Exception());
                await generalLedgerTransactionsController.Create3Async(generalLedgerTransaction);
            }


            #endregion

            #region PUT

            [TestMethod]
            public async Task GeneralLedgerTransactionsController_Update3Async_Valid()
            {

                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);

                var generalLedgerTransactionRes = await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            public async Task GeneralLedgerTransactionsController_Update3Async_Valid_NoGUD()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);
                var id = generalLedgerTransaction.Id;
                generalLedgerTransaction.Id = null;
                var generalLedgerTransactionRes = await generalLedgerTransactionsController.Update3Async(id, generalLedgerTransaction);
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_Valid_EmptyGUID()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);
                
                await generalLedgerTransactionsController.Update3Async("00000000-0000-0000-0000-000000000000", generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_EmptyId()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);

                var generalLedgerTransactionRes = await generalLedgerTransactionsController.Update3Async("", generalLedgerTransaction);
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_NullId()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);

                var generalLedgerTransactionRes = await generalLedgerTransactionsController.Update3Async(null, generalLedgerTransaction);
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_NullBody()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ReturnsAsync(generalLedgerTransaction);

                var generalLedgerTransactionRes = await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, null);
                Assert.IsNotNull(generalLedgerTransactionRes);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_PermissionsException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new PermissionsException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_KeyNotFoundException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new KeyNotFoundException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_ArgumentNullException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new ArgumentNullException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_IntegrationApiException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new IntegrationApiException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_ConfigurationException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new ConfigurationException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update2Async_ArgumentOutOfRangeException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new ArgumentOutOfRangeException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_ArgumentException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new ArgumentException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_InvalidOperationException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new InvalidOperationException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_FormatException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new FormatException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_RepositoryException()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new RepositoryException());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GeneralLedgerTransactionsController_Update3Async_Exception()
            {
                generalLedgerTransactionServiceMock.Setup(x => x.Update3Async(It.IsAny<string>(), It.IsAny<GeneralLedgerTransaction3>())).ThrowsAsync(new Exception());
                await generalLedgerTransactionsController.Update3Async(generalLedgerTransaction.Id, generalLedgerTransaction);
            }
            #endregion

        }
    }
}

