// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class EmployeeLeaveRequestControllerTests
    {
        #region Variables_Declaration
        private EmployeeLeaveRequestController controllerUnderTest;
        private Mock<ILogger> loggerMock;
        private Mock<IEmployeeLeaveRequestService> employeeLeaveRequestServiceMock;
        private HttpResponse httpResponse;

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
        #endregion

        [TestClass]
        public class GetLeaveRequestsTests : EmployeeLeaveRequestControllerTests
        {
            private IEnumerable<LeaveRequest> leaveRequests;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                loggerMock = new Mock<ILogger>();
                employeeLeaveRequestServiceMock = new Mock<IEmployeeLeaveRequestService>();

                #region Data_SetUp
                leaveRequests = new List<LeaveRequest>()
                {
                    new LeaveRequest() {
                        Id = "13",
                        PerLeaveId ="697",
                        EmployeeId ="0011560",
                        ApproverId ="0010351",
                        ApproverName ="Hadrian O. Racz",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today,
                        Status = LeaveStatusAction.Draft,
                        LeaveRequestDetails = new List<LeaveRequestDetail>()
                        {
                            new LeaveRequestDetail() { Id = "38", LeaveDate = DateTime.Today, LeaveHours = 4.00m, LeaveRequestId = "13"}
                        }
                    },
                        new LeaveRequest() {
                        Id = "14",
                        PerLeaveId ="698",
                        EmployeeId ="0011560",
                        ApproverId ="0010351",
                        ApproverName ="Hadrian O. Racz",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(1),
                        Status = LeaveStatusAction.Draft,
                        LeaveRequestDetails = new List<LeaveRequestDetail>()
                        {
                            new LeaveRequestDetail() { Id = "39", LeaveDate = DateTime.Today, LeaveHours = 4.00m, LeaveRequestId = "14"},
                            new LeaveRequestDetail() { Id = "40", LeaveDate = DateTime.Today.AddDays(1), LeaveHours = 8.00m, LeaveRequestId = "14"}
                        }
                    },
                        new LeaveRequest() {
                        Id = "15",
                        PerLeaveId ="697",
                        EmployeeId ="0011560",
                        ApproverId ="0010351",
                        ApproverName ="Hadrian O. Racz",
                        StartDate = DateTime.Today.AddDays(2),
                        EndDate = DateTime.Today.AddDays(2),
                        Status = LeaveStatusAction.Submitted,
                        LeaveRequestDetails = new List<LeaveRequestDetail>()
                        {
                            new LeaveRequestDetail() { Id = "41", LeaveDate = DateTime.Today.AddDays(2), LeaveHours = 8.00m, LeaveRequestId = "15"}
                        }
                    }
                };

                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetLeaveRequestsAsync(null)).ReturnsAsync(leaveRequests);
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetLeaveRequestsAsync("0012458")).Throws(new PermissionsException());
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetLeaveRequestsAsync("test")).Throws(new Exception());
                #endregion

                controllerUnderTest = new EmployeeLeaveRequestController(employeeLeaveRequestServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task GetLeaveRequestsAsync_MethodExecutesWithoutErrors()
            {
                var result = await controllerUnderTest.GetLeaveRequestsAsync();
                Assert.IsInstanceOfType(result, typeof(List<LeaveRequest>));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetLeaveRequestsAsync_PermissionException()
            {
                var result = await controllerUnderTest.GetLeaveRequestsAsync("0012458");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetLeaveRequestsAsync_Exception()
            {
                var result = await controllerUnderTest.GetLeaveRequestsAsync("test");
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                leaveRequests = null;
                employeeLeaveRequestServiceMock = null;
                controllerUnderTest = null;
            }
        }

        [TestClass]
        public class GetLeaveRequestInfoByLeaveRequestIdTests : EmployeeLeaveRequestControllerTests
        {
            private LeaveRequest leaveRequest;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                loggerMock = new Mock<ILogger>();
                employeeLeaveRequestServiceMock = new Mock<IEmployeeLeaveRequestService>();

                #region Data_SetUp
                leaveRequest = new LeaveRequest()
                {
                    Id = "14",
                    PerLeaveId = "698",
                    EmployeeId = "0011560",
                    ApproverId = "0010351",
                    ApproverName = "Hadrian O. Racz",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    Status = LeaveStatusAction.Draft,
                    LeaveRequestDetails = new List<LeaveRequestDetail>()
                        {
                            new LeaveRequestDetail() { Id = "39", LeaveDate = DateTime.Today, LeaveHours = 4.00m, LeaveRequestId = "14"},
                            new LeaveRequestDetail() { Id = "40", LeaveDate = DateTime.Today.AddDays(1), LeaveHours = 8.00m, LeaveRequestId = "14"}
                        }
                };

                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetLeaveRequestInfoByLeaveRequestIdAsync("14", null)).ReturnsAsync(leaveRequest);
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetLeaveRequestInfoByLeaveRequestIdAsync("18", null)).Throws(new PermissionsException());
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetLeaveRequestInfoByLeaveRequestIdAsync(null, null)).Throws(new Exception());
                #endregion

                controllerUnderTest = new EmployeeLeaveRequestController(employeeLeaveRequestServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task GetLeaveRequestInfoByLeaveRequestIdAsync_MethodExecutesWithoutErrors()
            {
                var result = await controllerUnderTest.GetLeaveRequestInfoByLeaveRequestIdAsync("14");
                Assert.IsInstanceOfType(result, typeof(LeaveRequest));
                Assert.AreEqual("14", result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetLeaveRequestInfoByLeaveRequestIdAsync_PermissionException()
            {
                var result = await controllerUnderTest.GetLeaveRequestInfoByLeaveRequestIdAsync("18", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetLeaveRequestInfoByLeaveRequestIdAsync_Exception()
            {
                var result = await controllerUnderTest.GetLeaveRequestInfoByLeaveRequestIdAsync(null);
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                leaveRequest = null;
                employeeLeaveRequestServiceMock = null;
                controllerUnderTest = null;
            }
        }

        [TestClass]
        public class CreateLeaveRequestTests : EmployeeLeaveRequestControllerTests
        {
            private LeaveRequest leaveRequestToBeCreated;
            private LeaveRequest createdLeaveRequest;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                loggerMock = new Mock<ILogger>();
                employeeLeaveRequestServiceMock = new Mock<IEmployeeLeaveRequestService>();

                // Resource location link set-up.
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/leave-requests/");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetLeaveRequestInfoByLeaveRequestId",
                    routeTemplate: "api/leave-requests/{id}",
                    defaults: new { controller = "EmployeeLeaveRequest", action = "GetLeaveRequestInfoByLeaveRequestIdAsync" }
                );
                var routeData = new HttpRouteData(getRoute);
                var context = new HttpControllerContext(config, routeData, request);

                httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                #region Data_SetUp
                leaveRequestToBeCreated = new LeaveRequest()
                {
                    Id = "",
                    PerLeaveId = "698",
                    EmployeeId = "0011560",
                    ApproverId = "0010351",
                    ApproverName = "Hadrian O. Racz",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today,
                    Status = LeaveStatusAction.Draft,
                    LeaveRequestDetails = new List<LeaveRequestDetail>()
                        {
                            new LeaveRequestDetail() { Id = "", LeaveDate = DateTime.Today, LeaveHours = 4.00m, LeaveRequestId = ""}
                        }
                };

                createdLeaveRequest = new LeaveRequest()
                {
                    Id = "14",
                    PerLeaveId = "698",
                    EmployeeId = "0011560",
                    ApproverId = "0010351",
                    ApproverName = "Hadrian O. Racz",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today,
                    Status = LeaveStatusAction.Draft,
                    LeaveRequestDetails = new List<LeaveRequestDetail>()
                        {
                            new LeaveRequestDetail() { Id = "39", LeaveDate = DateTime.Today, LeaveHours = 4.00m, LeaveRequestId = "14"}
                        }
                };

                employeeLeaveRequestServiceMock.Setup(elrs => elrs.CreateLeaveRequestAsync(leaveRequestToBeCreated, null)).ReturnsAsync(createdLeaveRequest);
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.CreateLeaveRequestAsync(null, null)).Throws(new Exception());
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.CreateLeaveRequestAsync(leaveRequestToBeCreated, "0011895")).Throws(new PermissionsException());

                controllerUnderTest = new EmployeeLeaveRequestController(employeeLeaveRequestServiceMock.Object, loggerMock.Object);
                #endregion

                // Request set-up
                controllerUnderTest.ControllerContext = new HttpControllerContext(config, routeData, request);
                controllerUnderTest.Request = request;
                controllerUnderTest.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                controllerUnderTest.Url = new UrlHelper(request);
            }

            [TestMethod]
            public async Task CreateLeaveRequest_MethodExecutesWithoutErrors()
            {
                var response = await controllerUnderTest.CreateLeaveRequestAsync(leaveRequestToBeCreated, null);
                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

                var actualCreatedLeaveRequest = JsonConvert.DeserializeObject<LeaveRequest>(response.Content.ReadAsStringAsync().Result);
                Assert.AreEqual(HttpContext.Current.Response.RedirectLocation, string.Format("http://localhost/api/leave-requests/{0}", actualCreatedLeaveRequest.Id));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CreateLeaveRequest_NullLeaveRequestObject_Exception()
            {
                var result = await controllerUnderTest.CreateLeaveRequestAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CreateLeaveRequest_PermissionException()
            {
                var result = await controllerUnderTest.CreateLeaveRequestAsync(leaveRequestToBeCreated, "0011895");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CreateLeaveRequest_ExistingResourceException()
            {
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.CreateLeaveRequestAsync(leaveRequestToBeCreated, null)).Throws(new ExistingResourceException());
                var result = await controllerUnderTest.CreateLeaveRequestAsync(leaveRequestToBeCreated, null);
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                leaveRequestToBeCreated = null;
                createdLeaveRequest = null;
                employeeLeaveRequestServiceMock = null;
                controllerUnderTest = null;
            }
        }

        [TestClass]
        public class UpdateLeaveRequestTests : EmployeeLeaveRequestControllerTests
        {
            private LeaveRequest leaveRequestToBeUpdated;
            private LeaveRequest updatedLeaveRequest;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                employeeLeaveRequestServiceMock = new Mock<IEmployeeLeaveRequestService>();
                // Resource location link set-up.
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/leave-requests/");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetLeaveRequestInfoByLeaveRequestId",
                    routeTemplate: "api/leave-requests/{id}",
                    defaults: new { controller = "EmployeeLeaveRequest", action = "GetLeaveRequestInfoByLeaveRequestIdAsync" }
                );
                var routeData = new HttpRouteData(getRoute);
                var context = new HttpControllerContext(config, routeData, request);

                httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                #region Data_SetUp
                leaveRequestToBeUpdated = new LeaveRequest()
                {
                    Id = "14",
                    PerLeaveId = "698",
                    EmployeeId = "0011560",
                    ApproverId = "0010351",
                    ApproverName = "Hadrian O. Racz",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    Status = LeaveStatusAction.Draft,
                    LeaveRequestDetails = new List<LeaveRequestDetail>()
                        {
                            new LeaveRequestDetail() { Id = "39", LeaveDate = DateTime.Today, LeaveHours = 8.00m, LeaveRequestId = "14"},
                            new LeaveRequestDetail() { Id = "40", LeaveDate = DateTime.Today.AddDays(1), LeaveHours = 8.00m, LeaveRequestId = "14"}
                        }
                };

                updatedLeaveRequest = new LeaveRequest()
                {
                    Id = "14",
                    PerLeaveId = "698",
                    EmployeeId = "0011560",
                    ApproverId = "0010351",
                    ApproverName = "Hadrian O. Racz",
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    Status = LeaveStatusAction.Draft,
                    LeaveRequestDetails = new List<LeaveRequestDetail>()
                        {
                            new LeaveRequestDetail() { Id = "39", LeaveDate = DateTime.Today, LeaveHours = 8.00m, LeaveRequestId = "14"},
                            new LeaveRequestDetail() { Id = "40", LeaveDate = DateTime.Today.AddDays(1), LeaveHours = 8.00m, LeaveRequestId = "14"}
                        }
                };

                employeeLeaveRequestServiceMock.Setup(elrs => elrs.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, null)).ReturnsAsync(updatedLeaveRequest);
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.UpdateLeaveRequestAsync(null, null)).Throws(new Exception());
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, "0011895")).Throws(new PermissionsException());

                controllerUnderTest = new EmployeeLeaveRequestController(employeeLeaveRequestServiceMock.Object, loggerMock.Object);
                #endregion

                // Request set-up
                controllerUnderTest.ControllerContext = new HttpControllerContext(config, routeData, request);
                controllerUnderTest.Request = request;
                controllerUnderTest.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                controllerUnderTest.Url = new UrlHelper(request);
            }

            [TestMethod]
            public async Task UpdateLeaveRequest_MethodExecutesWithoutErrors()
            {
                var actual = await controllerUnderTest.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, null);
                Assert.AreEqual(leaveRequestToBeUpdated.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateLeaveRequest_NullLeaveRequestObject_Exception()
            {
                var result = await controllerUnderTest.UpdateLeaveRequestAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateLeaveRequest_PermissionException()
            {
                var result = await controllerUnderTest.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, "0011895");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateLeaveRequest_KeyNotFoundException()
            {
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, null)).Throws(new KeyNotFoundException());
                var result = await controllerUnderTest.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateLeaveRequest_RecordLockException()
            {
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, null)).Throws(new RecordLockException());
                var result = await controllerUnderTest.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UpdateLeaveRequest_ExistingResourceException()
            {
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, null)).Throws(new ExistingResourceException());
                var result = await controllerUnderTest.UpdateLeaveRequestAsync(leaveRequestToBeUpdated, null);
            }
        }

        [TestClass]
        public class CreateLeaveRequestStatusTests : EmployeeLeaveRequestControllerTests
        {
            private LeaveRequestStatus leaveRequestStatusToBeCreated;
            private LeaveRequestStatus createdLeaveRequestStatus;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                loggerMock = new Mock<ILogger>();
                employeeLeaveRequestServiceMock = new Mock<IEmployeeLeaveRequestService>();

                #region Data_SetUp
                leaveRequestStatusToBeCreated = new LeaveRequestStatus()
                {
                    Id = "",
                    LeaveRequestId = "14",
                    ActionType = LeaveStatusAction.Approved,
                    ActionerId = "0010351"
                };

                createdLeaveRequestStatus = new LeaveRequestStatus()
                {
                    Id = "2",
                    LeaveRequestId = "14",
                    ActionType = LeaveStatusAction.Approved,
                    ActionerId = "0010351"
                };

                employeeLeaveRequestServiceMock.Setup(elrs => elrs.CreateLeaveRequestStatusAsync(leaveRequestStatusToBeCreated, null)).ReturnsAsync(createdLeaveRequestStatus);
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.CreateLeaveRequestStatusAsync(null, null)).Throws(new Exception());
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.CreateLeaveRequestStatusAsync(leaveRequestStatusToBeCreated, "0011895")).Throws(new PermissionsException());

                controllerUnderTest = new EmployeeLeaveRequestController(employeeLeaveRequestServiceMock.Object, loggerMock.Object);
                #endregion               
            }

            [TestMethod]
            public async Task CreateLeaveRequestStatus_MethodExecutesWithoutErrors()
            {
                var result = await controllerUnderTest.CreateLeaveRequestStatusAsync(leaveRequestStatusToBeCreated, null);
                Assert.IsInstanceOfType(result, typeof(LeaveRequestStatus));
                Assert.AreEqual("2", result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CreateLeaveRequestStatus_NullLeaveRequestObject_Exception()
            {
                var result = await controllerUnderTest.CreateLeaveRequestStatusAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CreateLeaveRequestStatus_PermissionException()
            {
                var result = await controllerUnderTest.CreateLeaveRequestStatusAsync(leaveRequestStatusToBeCreated, "0011895");
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                leaveRequestStatusToBeCreated = null;
                createdLeaveRequestStatus = null;
                employeeLeaveRequestServiceMock = null;
                controllerUnderTest = null;
            }
        }

        [TestClass]
        public class GetSupervisorsByPositionIdTests : EmployeeLeaveRequestControllerTests
        {
            private IEnumerable<HumanResourceDemographics> supervisors;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                loggerMock = new Mock<ILogger>();
                employeeLeaveRequestServiceMock = new Mock<IEmployeeLeaveRequestService>();

                #region Data_SetUp
                supervisors = new List<HumanResourceDemographics>()
                {
                    new HumanResourceDemographics()
                    {
                        Id = "0011958",
                        FirstName = "Rekha",
                        MiddleName = "S",
                        LastName = "Shetty",
                        PreferredName = "Mrs. Rekha Shetty"
                    },
                    new HumanResourceDemographics()
                    {
                        Id = "0018458",
                        FirstName = "Diya",
                        MiddleName = "",
                        LastName = "Hulekal",
                        PreferredName = "Mrs. Diya Hulekal"
                    }
                };

                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetSupervisorsByPositionIdAsync("ZPRIN62104ASST", null)).ReturnsAsync(supervisors);
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetSupervisorsByPositionIdAsync(null, null)).Throws(new Exception());
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetSupervisorsByPositionIdAsync("", null)).Throws(new Exception());
                employeeLeaveRequestServiceMock.Setup(elrs => elrs.GetSupervisorsByPositionIdAsync("ZPRIN62104ASST", "0011895")).Throws(new PermissionsException());
                #endregion

                controllerUnderTest = new EmployeeLeaveRequestController(employeeLeaveRequestServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task GetSupervisorsByPositionIdsAsync_MethodExecutesWithoutErrors()
            {
                var result = await controllerUnderTest.GetSupervisorsByPositionIdAsync("ZPRIN62104ASST", null);
                Assert.IsInstanceOfType(result, typeof(List<HumanResourceDemographics>));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSupervisorsByPositionIdsAsync_NullPositionId_Exception()
            {
                var result = await controllerUnderTest.GetSupervisorsByPositionIdAsync(null, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSupervisorsByPositionIdsAsync_EmptyPositionId_Exception()
            {
                var result = await controllerUnderTest.GetSupervisorsByPositionIdAsync("", null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSupervisorsByPositionIdsAsync_PermissionException()
            {
                var result = await controllerUnderTest.GetSupervisorsByPositionIdAsync("ZPRIN62104ASST", "0011895");
            }

            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                supervisors = null;
                employeeLeaveRequestServiceMock = null;
                controllerUnderTest = null;
            }
        }

        [TestClass]
        public class GetSuperviseesByPrimaryPositionForSupervisorAsyncTests : EmployeeLeaveRequestControllerTests
        {
            private IEnumerable<HumanResourceDemographics> supervisees;
            
            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                loggerMock = new Mock<ILogger>();
                employeeLeaveRequestServiceMock = new Mock<IEmployeeLeaveRequestService>();
              
           
                #region Data_SetUp
                supervisees = new List<HumanResourceDemographics>()
                {
                    new HumanResourceDemographics()
                    {
                        Id = "0012225",
                        FirstName = "Akash",
                        MiddleName = "",
                        LastName = "Hulekal",
                        PreferredName = "Mr. Akash Hulekal"
                    },
                    new HumanResourceDemographics()
                    {
                        Id = "0018458",
                        FirstName = "Diya",
                        MiddleName = "",
                        LastName = "Hulekal",
                        PreferredName = "Ms. Diya Hulekal"
                    }
                };
               
                employeeLeaveRequestServiceMock.Setup(s => s.GetSuperviseesByPrimaryPositionForSupervisorAsync()).ReturnsAsync(new List<Dtos.HumanResources.HumanResourceDemographics>());
                              
                #endregion

                controllerUnderTest = new EmployeeLeaveRequestController(employeeLeaveRequestServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task GetSuperviseesByPrimaryPositionForSupervisorAsync_MethodExecutesWithoutErrors()
            {
                var result = await controllerUnderTest.GetSuperviseesByPrimaryPositionForSupervisorAsync();
                Assert.IsInstanceOfType(result, typeof(List<HumanResourceDemographics>));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSuperviseesByPrimaryPositionForSupervisorAsync_PermissionException()
            {
                employeeLeaveRequestServiceMock.Setup(s => s.GetSuperviseesByPrimaryPositionForSupervisorAsync()).ThrowsAsync(new PermissionsException());
                var result = await controllerUnderTest.GetSuperviseesByPrimaryPositionForSupervisorAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSuperviseesByPrimaryPositionForSupervisorAsync_GenericException()
            {
                employeeLeaveRequestServiceMock.Setup(s => s.GetSuperviseesByPrimaryPositionForSupervisorAsync()).ThrowsAsync(new Exception());
                var result = await controllerUnderTest.GetSuperviseesByPrimaryPositionForSupervisorAsync();
            }



            [TestCleanup]
            public void Cleanup()
            {
                loggerMock = null;
                supervisees = null;
                employeeLeaveRequestServiceMock = null;
                controllerUnderTest = null;
            }
        }
    }
}