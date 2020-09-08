// Copyright 2019 Ellucian Company L.P. and its affiliates
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using System.Net.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Dtos.Base;
using Newtonsoft.Json.Linq;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.EthosExtend;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class BulkLoadRequestControllerTests
    {
        [TestClass]
        public class BulkLoadPostTestsV1
        {
            private BulkLoadRequestController _bulkLoadRequestController;

            private IBulkLoadRequestService _bulkLoadRequestService;
            private Mock<IBulkLoadRequestService> _bulkLoadRequestServiceMock;
            
            private Dtos.BulkLoadRequest _bulkLoadRequestDtoFirstCall;
            private Dtos.BulkLoadRequest _bulkLoadRequestGoodReturn;
            private Dtos.BulkLoadRequest _bulkLoadRequestFailedReturn;

            private Dtos.BulkLoadGet _bulkLoadGet;

            private EthosResourceRouteInfo _ethosResourceRouteInfo;

            private const string BulkLoadRequestGuid = "62fa1539-d67f-4268-9ed2-272ddf9dc5a6";
            private const string ApplicationGuid = "5f5394e1-da5f-46c0-8fd3-ee2d67f416ea";
            private const string RepresentationVersion = "application/vnd.hedtech.integration.v12.1.0+json";
            private const string BulkRepresentation = "application/vnd.hedtech.integration.bulk-requests.v1+json";

            ILogger _logger = new Mock<ILogger>().Object;


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

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                _bulkLoadRequestServiceMock = new Mock<IBulkLoadRequestService>();
                _bulkLoadRequestService = _bulkLoadRequestServiceMock.Object;
                
                _bulkLoadRequestDtoFirstCall = new Dtos.BulkLoadRequest()
                {
                    ApplicationId = ApplicationGuid,
                    RequestorTrackingId = BulkLoadRequestGuid
                };

                _bulkLoadRequestGoodReturn = new BulkLoadRequest()
                {
                    ApplicationId = ApplicationGuid,
                    RequestorTrackingId = BulkLoadRequestGuid,
                    JobNumber = "4567566",
                    Message = "Bulk Load Request in progress.",
                    Representation = RepresentationVersion,
                    ResourceName = "person",
                    Status = BulkLoadRequestStatus.InProgress
                };

                _bulkLoadRequestFailedReturn = new BulkLoadRequest()
                {
                    ApplicationId = ApplicationGuid,
                    RequestorTrackingId = BulkLoadRequestGuid,
                    Message = "Bulk Load Request failed.",
                    Representation = RepresentationVersion,
                    ResourceName = "person",
                    Status = BulkLoadRequestStatus.Error
                };

                _ethosResourceRouteInfo = new EthosResourceRouteInfo()
                {
                    ResourceName = "student-transcript-grades",
                    ResourceVersionNumber = "1.0.0"
                };

                _bulkLoadGet = new BulkLoadGet()
                {
                    ApplicationId = ApplicationGuid,
                    JobNumber = "123456",
                    Representation = RepresentationVersion,
                    RequestorTrackingId = BulkLoadRequestGuid,
                    ResourceName = "student-transcript-grades",
                    Status = "In Progress",
                    XTotalCount = "560999",
                    ProcessingSteps = new List<BulkLoadProcessingStep>()
                    {
                        new BulkLoadProcessingStep() { Count = "5", JobNumber = "456", ElapsedTime = "00:04:55", Step = "Request"}, new BulkLoadProcessingStep() { Count = "58", JobNumber = "476", ElapsedTime = "00:05:55", Step = "Packaging" }
                    }
                };

                
                _bulkLoadRequestServiceMock.Setup(s => s.CreateBulkLoadRequestAsync(_bulkLoadRequestDtoFirstCall, "permissionCode")).ReturnsAsync(_bulkLoadRequestGoodReturn);
                _bulkLoadRequestServiceMock.Setup(s => s.GetBulkLoadRequestStatus(BulkLoadRequestGuid)).ReturnsAsync(_bulkLoadGet);

                _bulkLoadRequestController = new BulkLoadRequestController(_bulkLoadRequestService, _logger)
                {
                    Request = new HttpRequestMessage()
                };
                _bulkLoadRequestController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());

            }

            [TestMethod]
            public async Task GetBulkStatus()
            {
                var bulkRequestReturn = await _bulkLoadRequestController.GetBulkLoadRequestStatusAsync(BulkLoadRequestGuid);
                Assert.AreEqual(bulkRequestReturn, _bulkLoadGet);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuidAsync_Exception()
            {
                _bulkLoadRequestServiceMock.Setup(x => x.GetBulkLoadRequestStatus(It.IsAny<string>())).Throws<Exception>();
                await _bulkLoadRequestController.GetBulkLoadRequestStatusAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_KeyNotFoundException()
            {
                _bulkLoadRequestServiceMock.Setup(x => x.GetBulkLoadRequestStatus(It.IsAny<string>())).Throws<KeyNotFoundException>();
                await _bulkLoadRequestController.GetBulkLoadRequestStatusAsync(BulkLoadRequestGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_PermissionsException()
            {
                _bulkLoadRequestServiceMock.Setup(x => x.GetBulkLoadRequestStatus(It.IsAny<string>())).Throws<PermissionsException>();
                await _bulkLoadRequestController.GetBulkLoadRequestStatusAsync(BulkLoadRequestGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_ArgumentException()
            {
                _bulkLoadRequestServiceMock.Setup(x => x.GetBulkLoadRequestStatus(It.IsAny<string>())).Throws<ArgumentException>();
                await _bulkLoadRequestController.GetBulkLoadRequestStatusAsync(BulkLoadRequestGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_RepositoryException()
            {
                _bulkLoadRequestServiceMock.Setup(x => x.GetBulkLoadRequestStatus(It.IsAny<string>())).Throws<RepositoryException>();
                await _bulkLoadRequestController.GetBulkLoadRequestStatusAsync(BulkLoadRequestGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_IntegrationApiException()
            {
                _bulkLoadRequestServiceMock.Setup(x => x.GetBulkLoadRequestStatus(It.IsAny<string>())).Throws<IntegrationApiException>();
                await _bulkLoadRequestController.GetBulkLoadRequestStatusAsync(BulkLoadRequestGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentTranscriptGradesController_GetStudentTranscriptGradesByGuid_Exception()
            {
                _bulkLoadRequestServiceMock.Setup(x => x.GetBulkLoadRequestStatus(It.IsAny<string>())).Throws<Exception>();
                await _bulkLoadRequestController.GetBulkLoadRequestStatusAsync(BulkLoadRequestGuid);
            }

            [TestCleanup]
            public void Cleanup()
            {
                _bulkLoadRequestController = null;
                _bulkLoadRequestService = null;
                _logger = null;
                _bulkLoadRequestController = null;
            }

        }
    }
}
