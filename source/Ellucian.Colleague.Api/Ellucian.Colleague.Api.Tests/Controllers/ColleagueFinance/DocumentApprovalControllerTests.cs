//Copyright 2020-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.ColleagueFinance;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.ColleagueFinance
{
    [TestClass]
    public class DocumentApprovalControllerTests
    {

        #region Test Context

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #endregion

        #region Initialize and Cleanup
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<ILogger> _loggerMock;
        private Mock<IDocumentApprovalService> _documentApprovalServiceMock;

        private HttpResponse _response;
        private DocumentApprovalController _documentApprovalController;

        private DocumentApprovalRequest documentApprovalRequestDto;
        private DocumentApprovalResponse documentApprovalResponseDto;
        private ApprovalDocumentRequest approvalDocumentRequestDto;
        private ApprovedDocumentFilterCriteria approvedDocumentFilterCriteriaDto;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _loggerMock = new Mock<ILogger>();
            _documentApprovalServiceMock = new Mock<IDocumentApprovalService>();

            _response = new HttpResponse(new StringWriter());

            _documentApprovalController = new DocumentApprovalController(_documentApprovalServiceMock.Object, _loggerMock.Object)
            {
                Request = new HttpRequestMessage() { RequestUri = new Uri("http://localhost") }
            };
            _documentApprovalController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            documentApprovalRequestDto = new DocumentApprovalRequest();
            documentApprovalRequestDto.ApprovalDocumentRequests = new List<ApprovalDocumentRequest>();
            approvalDocumentRequestDto = new ApprovalDocumentRequest() {  Approve = true, DocumentType = "REQ", DocumentId = "1325", DocumentNumber = "0001234"};
            documentApprovalRequestDto.ApprovalDocumentRequests.Add(approvalDocumentRequestDto);

            documentApprovalResponseDto = new DocumentApprovalResponse() { UpdatedApprovalDocumentResponses = new System.Collections.Generic.List<ApprovalDocumentResponse>(),
                                                                            NotUpdatedApprovalDocumentResponses = new System.Collections.Generic.List<ApprovalDocumentResponse>() };
            approvedDocumentFilterCriteriaDto = new ApprovedDocumentFilterCriteria();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _adapterRegistryMock = null;
            _loggerMock = null;
            _documentApprovalServiceMock = null;
            _documentApprovalController = null;
        }
        #endregion

        #region Get tests
        [TestMethod]
        public async Task DocumentApprovalController_GetAsync_Valid()
        {
            _documentApprovalServiceMock.Setup(x => x.GetAsync()).ReturnsAsync(new Dtos.ColleagueFinance.DocumentApproval());
            var documentApprovalDto = await _documentApprovalController.GetAsync();
            Assert.IsNotNull(documentApprovalDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_GetAsync_PermissionsException()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new PermissionsException());
                await _documentApprovalController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to get the document approval.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_GetAsync_ArgumentNullException()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new ArgumentNullException());
                await _documentApprovalController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the document approval.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_GetAsync_Exception()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new Exception());
                await _documentApprovalController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the document approval.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_GetAsync_ExpiredSessionException()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.GetAsync()).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _documentApprovalController.GetAsync();
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get the document approval.", responseJson.Message);
                throw;
            }
        }
        #endregion

        #region Update tests
        [TestMethod]
        public async Task DocumentApprovalController_PostDocumentApprovalAsync_Valid()
        {
            _documentApprovalServiceMock.Setup(x => x.UpdateDocumentApprovalRequestAsync(It.IsAny<DocumentApprovalRequest>())).ReturnsAsync(documentApprovalResponseDto);
            var documentApprovalDto = await _documentApprovalController.PostDocumentApprovalAsync(documentApprovalRequestDto);
            Assert.IsNotNull(documentApprovalDto);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_PostDocumentApprovalAsync_PermissionsException()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.UpdateDocumentApprovalRequestAsync(It.IsAny<DocumentApprovalRequest>())).ThrowsAsync(new PermissionsException());
                await _documentApprovalController.PostDocumentApprovalAsync(documentApprovalRequestDto);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Insufficient permissions to update document approvals.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_PostDocumentApprovalAsync_ArgumentNullException()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.UpdateDocumentApprovalRequestAsync(It.IsAny<DocumentApprovalRequest>())).ThrowsAsync(new ArgumentNullException());
                await _documentApprovalController.PostDocumentApprovalAsync(documentApprovalRequestDto);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Invalid argument to update a document approval.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_PostDocumentApprovalAsync_Exception()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.UpdateDocumentApprovalRequestAsync(It.IsAny<DocumentApprovalRequest>())).ThrowsAsync(new Exception());
                await _documentApprovalController.PostDocumentApprovalAsync(documentApprovalRequestDto);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to update a document approval.", responseJson.Message);
                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_PostDocumentApprovalAsync_NullDto()
        {
            await _documentApprovalController.PostDocumentApprovalAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_PostDocumentApprovalAsync_ExpiredSessionException()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.UpdateDocumentApprovalRequestAsync(It.IsAny<DocumentApprovalRequest>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _documentApprovalController.PostDocumentApprovalAsync(documentApprovalRequestDto);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to update a document approval.", responseJson.Message);
                throw;
            }
        }
        #endregion

        #region Query tests
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task DocumentApprovalController_QueryApprovedDocumentsAsync_ExpiredSessionException()
        {
            try
            {
                _documentApprovalServiceMock.Setup(x => x.QueryApprovedDocumentsAsync(It.IsAny<ApprovedDocumentFilterCriteria>())).ThrowsAsync(new ColleagueSessionExpiredException("timeout"));
                await _documentApprovalController.QueryApprovedDocumentsAsync(approvedDocumentFilterCriteriaDto);
            }
            catch (HttpResponseException ex)
            {
                var exceptionResponse = ex.Response.Content.ReadAsStringAsync().Result;
                WebApiException responseJson = JsonConvert.DeserializeObject<WebApiException>(exceptionResponse);
                Assert.IsNotNull(responseJson);
                Assert.AreEqual("Unable to get approved documents.", responseJson.Message);
                throw;
            }
        }
        #endregion
    }
}
