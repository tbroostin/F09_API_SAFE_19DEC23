//Copyright 2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using slf4net;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System.Threading.Tasks;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class VouchersControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IVoucherService> voucherServiceMock;
        private Mock<ILogger> loggerMock;
        private VouchersController voucherController;
        private List<VoucherSummary> voucherSummaryCollection;
        private Voucher voucher;
        private Voucher2 voucher2;
        private string personId = "0000100";

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            voucherServiceMock = new Mock<IVoucherService>();
            loggerMock = new Mock<ILogger>();
            voucherSummaryCollection = new List<VoucherSummary>();

            BuildVoucherSummaryData();

            voucherController = new VouchersController(voucherServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            voucherController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            voucherController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        private void BuildVoucherSummaryData()
        {
            voucherSummaryCollection = new List<VoucherSummary>()
            {
                new VoucherSummary()
                {
                   Id = "1",
                   Date = DateTime.Today.AddDays(2),
                   RequestorName = "Test User",
                   Status = VoucherStatus.InProgress,
                   VendorId = "0000190",
                   VendorName = "Basic Office Supply",
                   Amount = 10.00m,
                   
                   PurchaseOrders = new List<PurchaseOrderLinkSummary>()
                   {
                       new PurchaseOrderLinkSummary()
                       {
                           Id = "1",
                           Number = "0000001"
                       }
                   }

                },
                new VoucherSummary()
                {
                   Id = "2",
                   Date = DateTime.Today.AddDays(2),
                   RequestorName = "Test User",
                   Status = VoucherStatus.InProgress,
                   VendorId = "0000190",
                   VendorName = "Basic Office Supply",
                   Amount = 10.00m,
                   PurchaseOrders = new List<PurchaseOrderLinkSummary>()
                   {
                       new PurchaseOrderLinkSummary()
                       {
                           Id = "2",
                           Number = "0000002"
                       }
                   }
                }

            };
            voucher = new Voucher()
            {
                VoucherId = "V000001",
                Amount = 100,
                Date = DateTime.Today.AddDays(2),
                Status = VoucherStatus.InProgress
            };

            voucher2 = new Voucher2()
            {
                VoucherId = "V000001",
                Amount = 100,
                Date = DateTime.Today.AddDays(2),
                Status = VoucherStatus.InProgress
            };

            voucherServiceMock.Setup(r => r.GetVoucherSummariesAsync (It.IsAny<string>())).ReturnsAsync(voucherSummaryCollection);
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ReturnsAsync(voucher);
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ReturnsAsync(voucher2);

        }


        [TestCleanup]
        public void Cleanup()
        {
            voucherController = null;
            loggerMock = null;
            voucherServiceMock = null;
        }

        #region Voucher Summary

        [TestMethod]
        public async Task VouchersController_GetVoucherSummariesAsync()
        {
            var expected = voucherSummaryCollection.AsEnumerable();
            voucherServiceMock.Setup(r => r.GetVoucherSummariesAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var Vouchers = await voucherController.GetVoucherSummariesAsync(personId);
            Assert.AreEqual(Vouchers.ToList().Count, expected.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherSummariesAsync_PersonId_Null()
        {
            await voucherController.GetVoucherSummariesAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherSummariesAsync_ArgumentNullException()
        {
            voucherServiceMock.Setup(r => r.GetVoucherSummariesAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await voucherController.GetVoucherSummariesAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherSummariesAsync_Exception()
        {
            voucherServiceMock.Setup(r => r.GetVoucherSummariesAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await voucherController.GetVoucherSummariesAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherSummariesAsync_KeyNotFoundException()
        {
            voucherServiceMock.Setup(r => r.GetVoucherSummariesAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await voucherController.GetVoucherSummariesAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherSummariesAsync_ApplicationException()
        {
            voucherServiceMock.Setup(r => r.GetVoucherSummariesAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
            await voucherController.GetVoucherSummariesAsync(personId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherSummariesAsync_PermissionException()
        {
            voucherServiceMock.Setup(r => r.GetVoucherSummariesAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await voucherController.GetVoucherSummariesAsync(personId);
        }

        #endregion

        #region GET1
        [TestMethod]
        public async Task VouchersController_GetVoucherAsync()
        {
            var expected = voucher;
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var Voucher = await voucherController.GetVoucherAsync(voucher.VoucherId);
            Assert.IsNotNull(Voucher);
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherAsync_VoucherId_Asnull()
        {
            var expected = voucher;
            voucher.VoucherId = null;
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var Voucher = await voucherController.GetVoucherAsync(voucher.VoucherId);
            Assert.IsNotNull(Voucher);
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherAsync_VoucherId_AsEmpty()
        {
            var expected = voucher;
            voucher.VoucherId = null;
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ReturnsAsync(expected);
            var Voucher = await voucherController.GetVoucherAsync(voucher.VoucherId);
            Assert.IsNotNull(Voucher);
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherAsync_ArgumentNullException()
        {
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await voucherController.GetVoucherAsync(voucher.VoucherId);
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherAsync_KeyNotFoundException()
        {
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await voucherController.GetVoucherAsync(voucher.VoucherId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherAsync_PermissionException()
        {
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await voucherController.GetVoucherAsync(voucher.VoucherId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherAsync_ApplicationException()
        {
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            await voucherController.GetVoucherAsync(voucher.VoucherId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucherAsync_Exception()
        {
            voucherServiceMock.Setup(r => r.GetVoucherAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
            await voucherController.GetVoucherAsync(voucher.VoucherId);
        }

        #endregion

        #region GET2
        [TestMethod]
        public async Task VouchersController_GetVoucher2Async()
        {
            var expected = voucher2;
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ReturnsAsync(expected);
            var Voucher = await voucherController.GetVoucher2Async(voucher2.VoucherId);
            Assert.IsNotNull(Voucher);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucher2Async_VoucherId_Asnull()
        {
            var expected = voucher2;
            voucher2.VoucherId = null;
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ReturnsAsync(expected);
            var Voucher2 = await voucherController.GetVoucher2Async(voucher2.VoucherId);
            Assert.IsNotNull(Voucher2);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucher2Async_VoucherId_AsEmpty()
        {
            var expected = voucher2;
            voucher2.VoucherId = "";
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ReturnsAsync(expected);
            var Voucher2 = await voucherController.GetVoucher2Async(voucher2.VoucherId);
            Assert.IsNotNull(Voucher2);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucher2Async_ArgumentNullException()
        {
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
            await voucherController.GetVoucher2Async(voucher2.VoucherId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucher2Async_KeyNotFoundException()
        {
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
            await voucherController.GetVoucher2Async(voucher2.VoucherId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucher2Async_PermissionException()
        {
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
            await voucherController.GetVoucher2Async(voucher2.VoucherId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucher2Async_Exception()
        {
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ThrowsAsync(new Exception());
            await voucherController.GetVoucher2Async(voucher2.VoucherId);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetVoucher2Async_ApplicationException()
        {
            voucherServiceMock.Setup(r => r.GetVoucher2Async(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
            await voucherController.GetVoucher2Async(voucher2.VoucherId);
        }

        #endregion
    }


    #region CREATE/MODIFY
    [TestClass]
    public class PostVoucherAsyncTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private Mock<IVoucherService> voucherServiceMock;
        private Mock<ILogger> loggerMock;
        private VouchersController vouchersController;
        private Tuple<IEnumerable<Voucher2>, int> vouchersCollection;
        private Dtos.ColleagueFinance.VoucherCreateUpdateRequest createUpdateVoucherRequest;
        private VoucherCreateUpdateResponse createUpdateVoucherReponse;
        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            voucherServiceMock = new Mock<IVoucherService>();
            loggerMock = new Mock<ILogger>();
            createUpdateVoucherRequest = new VoucherCreateUpdateRequest();
            createUpdateVoucherRequest.Voucher = new Voucher2() { VoucherId = "V000001" };
            createUpdateVoucherRequest.PersonId = "0000100";

            createUpdateVoucherReponse = new VoucherCreateUpdateResponse() { VoucherId = "V000001", VoucherDate = DateTime.Now.Date };
            vouchersController = new VouchersController(voucherServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }



        [TestCleanup]
        public void Cleanup()
        {
            vouchersController = null;
            loggerMock = null;
            voucherServiceMock = null;
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_PostVoucherAsync_Dto_Null()
        {
            await vouchersController.PostVoucherAsync(null);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_PostRequisitionsAsync_PermissionsException()
        {
            voucherServiceMock.Setup(r => r.CreateUpdateVoucherAsync(It.IsAny<VoucherCreateUpdateRequest>())).ThrowsAsync(new PermissionsException());
            await vouchersController.PostVoucherAsync(createUpdateVoucherRequest);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_PostVoucherAsync_Exception()
        {
            voucherServiceMock.Setup(r => r.CreateUpdateVoucherAsync(It.IsAny<VoucherCreateUpdateRequest>())).ThrowsAsync(new Exception());
            await vouchersController.PostVoucherAsync(createUpdateVoucherRequest);
        }

        [TestMethod]
        public async Task VouchersController_PostVoucherAsync()
        {
            voucherServiceMock.Setup(r => r.CreateUpdateVoucherAsync(It.IsAny<VoucherCreateUpdateRequest>())).ReturnsAsync(createUpdateVoucherReponse);
            var result = await vouchersController.PostVoucherAsync(createUpdateVoucherRequest);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.VoucherId, createUpdateVoucherRequest.Voucher.VoucherId);
        }

    }

    #endregion
    
   #region Reimburse Myself Tests

    [TestClass]
    public class GetReimbursePersonAddressForVoucherAsyncTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IVoucherService> _vouchersServiceMock;
        private Mock<ILogger> _loggerMock;

        private VouchersController _vouchersController;
        
        private Dtos.ColleagueFinance.VendorsVoucherSearchResult resultsDto;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _vouchersServiceMock = new Mock<IVoucherService>();
            _loggerMock = new Mock<ILogger>();

            
            BuildData();
            _vouchersServiceMock.Setup(s => s.GetReimbursePersonAddressForVoucherAsync()).ReturnsAsync(resultsDto);
            _vouchersController = new VouchersController(_vouchersServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {
            
            resultsDto = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult
            {
                VendorId = "0000192",
                VendorNameLines = new List<string>() { "Blue Cross Office supply" },
                VendorMiscName = null,
                AddressLines = new List<string>() { "PO Box 69845" },
                City = "Minneapolis",
                State = "MN",
                Zip = "55430",
                Country = "",
                FormattedAddress = "PO Box 69845 Minneapolis MN 55430",
                AddressId = "143"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _vouchersController = null;
            _loggerMock = null;
            _vouchersServiceMock = null;
            resultsDto = null;
        }

        [TestMethod]
        public async Task VouchersController_GetReimbursePersonAddressForVoucherAsync_Success()
        {
            var results = await _vouchersController.GetReimbursePersonAddressForVoucherAsync();
            Assert.IsNotNull(results);
            
            var actualDto = results;
            Assert.AreEqual(resultsDto.VendorId, actualDto.VendorId);
            Assert.AreEqual(resultsDto.VendorNameLines, actualDto.VendorNameLines);
            Assert.AreEqual(resultsDto.VendorMiscName, actualDto.VendorMiscName);
            Assert.AreEqual(resultsDto.AddressLines, actualDto.AddressLines);
            Assert.AreEqual(resultsDto.City, actualDto.City);
            Assert.AreEqual(resultsDto.State, actualDto.State);
            Assert.AreEqual(resultsDto.Zip, actualDto.Zip);
            Assert.AreEqual(resultsDto.Country, actualDto.Country);
            Assert.AreEqual(resultsDto.FormattedAddress, actualDto.FormattedAddress);
            Assert.AreEqual(resultsDto.AddressId, actualDto.AddressId);
        }

        [TestMethod]
        public async Task VouchersController_GetReimbursePersonAddressForVoucherAsync_NullCriteria()
        {
            var results = await _vouchersController.GetReimbursePersonAddressForVoucherAsync();
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetReimbursePersonAddressForVoucherAsync_ArgumentNullException()
        {
            _vouchersServiceMock.Setup(s => s.GetReimbursePersonAddressForVoucherAsync()).Throws(new ArgumentNullException());
            _vouchersController = new VouchersController(_vouchersServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vouchersController.GetReimbursePersonAddressForVoucherAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetReimbursePersonAddressForVoucherAsync_Exception()
        {
            _vouchersServiceMock.Setup(s => s.GetReimbursePersonAddressForVoucherAsync()).Throws(new Exception());
            _vouchersController = new VouchersController(_vouchersServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vouchersController.GetReimbursePersonAddressForVoucherAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetReimbursePersonAddressForVoucherAsync_KeyNotFoundException()
        {
            _vouchersServiceMock.Setup(s => s.GetReimbursePersonAddressForVoucherAsync()).Throws(new KeyNotFoundException());
            _vouchersController = new VouchersController(_vouchersServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vouchersController.GetReimbursePersonAddressForVoucherAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_GetReimbursePersonAddressForVoucherAsync_PermissionsException()
        {
            _vouchersServiceMock.Setup(s => s.GetReimbursePersonAddressForVoucherAsync()).Throws(new PermissionsException());
            _vouchersController = new VouchersController(_vouchersServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            var results = await _vouchersController.GetReimbursePersonAddressForVoucherAsync();
        }


    }

    #endregion

   #region VOID
    [TestClass]
    public class VoucherVoidTests
    {
        public TestContext TestContext { get; set; }

        private Mock<IVoucherService> _vouchersServiceMock;
        private Mock<ILogger> _loggerMock;

        private VouchersController _vouchersController;

        private Dtos.ColleagueFinance.VendorsVoucherSearchResult resultsDto;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            _vouchersServiceMock = new Mock<IVoucherService>();
            _loggerMock = new Mock<ILogger>();


            BuildData();
            _vouchersServiceMock.Setup(s => s.GetReimbursePersonAddressForVoucherAsync()).ReturnsAsync(resultsDto);
            _vouchersController = new VouchersController(_vouchersServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
        }

        private void BuildData()
        {

            resultsDto = new Ellucian.Colleague.Dtos.ColleagueFinance.VendorsVoucherSearchResult
            {
                VendorId = "0000192",
                VendorNameLines = new List<string>() { "Blue Cross Office supply" },
                VendorMiscName = null,
                AddressLines = new List<string>() { "PO Box 69845" },
                City = "Minneapolis",
                State = "MN",
                Zip = "55430",
                Country = "",
                FormattedAddress = "PO Box 69845 Minneapolis MN 55430",
                AddressId = "143"
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            _vouchersController = null;
            _loggerMock = null;
            _vouchersServiceMock = null;
            resultsDto = null;
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_VoidVoucherAsync_ArgumentNullException()
        {
            _vouchersServiceMock.Setup(r => r.VoidVoucherAsync(It.IsAny<VoucherVoidRequest>())).ThrowsAsync(new ArgumentNullException());

            await _vouchersController.VoidVoucherAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_VoidVoucherAsync_Exception()
        {
            VoucherVoidRequest request = new VoucherVoidRequest()
            {
                PersonId = "0000123",
                VoucherId = "V00001",
                Comments = "Voucher Voided",
                ConfirmationEmailAddresses = "abc@mail.com"
            };
            _vouchersServiceMock.Setup(r => r.VoidVoucherAsync(It.IsAny<VoucherVoidRequest>())).ThrowsAsync(new Exception());

            await _vouchersController.VoidVoucherAsync(request);
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task VouchersController_VoidVoucherAsync_PermissionException()
        {
            VoucherVoidRequest request = new VoucherVoidRequest()
            {
                PersonId = "0000123",
                VoucherId = "V00001",
                Comments = "Voucher Voided",
                ConfirmationEmailAddresses = "abc@mail.com"
            };
            _vouchersServiceMock.Setup(r => r.VoidVoucherAsync(It.IsAny<VoucherVoidRequest>())).ThrowsAsync(new PermissionsException());

            await _vouchersController.VoidVoucherAsync(request);
        }

        [TestMethod]
        public async Task VouchersController_VoidVoucherAsync()
        {
            VoucherVoidRequest request = new VoucherVoidRequest()
            {
                PersonId = "0000123",
                VoucherId = "V00001",
                Comments = "Voucher Voided",
                ConfirmationEmailAddresses = "abc@mail.com"
            };

            VoucherVoidResponse response = new VoucherVoidResponse()
            {
                VoucherId = "V00001",
                ErrorOccured = false,
                ErrorMessages = null,
                WarningOccured = false,
                WarningMessages = null
            };

            _vouchersServiceMock.Setup(r => r.VoidVoucherAsync(It.IsAny<VoucherVoidRequest>())).ReturnsAsync(response);
            var result = await _vouchersController.VoidVoucherAsync(request);

            Assert.IsNotNull(result);
        }
    }
    #endregion

}
