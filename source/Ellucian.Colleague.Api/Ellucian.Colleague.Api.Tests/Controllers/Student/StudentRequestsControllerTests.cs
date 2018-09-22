// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{

    [TestClass]
    public class StudentRequestsControllerTests
    {
        [TestClass]
        public class StudentTranscriptPostTests
        {
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

            private Mock<IStudentRequestService> studentRequestServiceMock;
            private Mock<ILogger> loggerMock;
            private IStudentRequestService requestService;
            private StudentRequestsController requestController;
            private ILogger logger;
            private Dtos.Student.StudentRequest returnedStudentRequest;
            private HttpRequestMessage httpRequest;

            [TestInitialize]
            public void Initilaize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentRequestServiceMock = new Mock<IStudentRequestService>();
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                requestService = studentRequestServiceMock.Object;
                returnedStudentRequest = ReturnedStudentTranscriptRequest();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Returns(Task.FromResult<StudentRequest>(returnedStudentRequest));
                requestController = new StudentRequestsController(requestService, logger);
                httpRequest=Initialize_HttpRequest_Controller_Context();
                //specify httprequestmessage for controller
                requestController.Request = httpRequest;
             
               //initialize httpContext current value to request and response objects
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://localhost/api/CreateStudentTranscriptRequests", ""), new HttpResponse(new StringWriter()));
            }

            private HttpRequestMessage Initialize_HttpRequest_Controller_Context()
            {
                //create new httpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/CreateStudentTranscriptRequests");
                //define the httpconfiguration only for routing to have url that will be defined as post response location header
                HttpConfiguration config = new HttpConfiguration();
                var route = config.Routes.MapHttpRoute("GetStudentTranscriptRequest", "students/student-transcript-request/{requestId}");
                var routeData = new System.Web.Http.Routing.HttpRouteData(route, new System.Web.Http.Routing.HttpRouteValueDictionary { { "controller", "studentRequests" } });
                //add the httpConfiguration to part of httpRequestMessage
                request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                return request;
            }
            private StudentTranscriptRequest Initialize_Transcript_Request_To_Send()
            {
                StudentTranscriptRequest sr = new StudentTranscriptRequest("11111", "my name", new List<string>() { "address line 1" });
                sr.Comments = "trial - TR";
                sr.HoldRequest = "GRADE";
                sr.TranscriptGrouping = "UG";
                sr.NumberOfCopies = 3;
                return sr;
            }

            private StudentRequest ReturnedStudentTranscriptRequest()
            {
                StudentRequest sr = new StudentTranscriptRequest("11111", "my name", new List<string>() { "line 1" }) { TranscriptGrouping = "UG" };
                sr.Comments = "trial - TR";
                sr.HoldRequest = "GRADE";
                sr.NumberOfCopies = 3;
                sr.Id = "1";
                return sr;
            }

            [TestMethod]
            public async Task Post_Student_Transcript_Request()
            {
                StudentTranscriptRequest sr = Initialize_Transcript_Request_To_Send();
                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(sr);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
                Assert.AreEqual(response.IsSuccessStatusCode, true);
                Assert.IsTrue(response.Content != null);
                Assert.IsTrue(HttpContext.Current.Response.RedirectLocation.Contains("students/student-transcript-request/1"));
                //deserialize response content and verify it is same as expected
                var responseTranscriptRequest = JsonConvert.DeserializeObject<StudentTranscriptRequest>(await response.Content.ReadAsStringAsync());
                StudentTranscriptRequest expectedSr= ReturnedStudentTranscriptRequest() as StudentTranscriptRequest;
                Assert.AreEqual(responseTranscriptRequest.StudentId, expectedSr.StudentId);
                Assert.AreEqual(responseTranscriptRequest.Id, expectedSr.Id);
                Assert.AreEqual(responseTranscriptRequest.RecipientName, expectedSr.RecipientName);
                Assert.AreEqual(responseTranscriptRequest.TranscriptGrouping, expectedSr.TranscriptGrouping);

            }
            //permission exceptions
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Post_Student_Transcript_Permission_Exception()
            {
                StudentTranscriptRequest sr = Initialize_Transcript_Request_To_Send();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Throws(new PermissionsException());
                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(sr);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.Forbidden);
                Assert.AreEqual(response.IsSuccessStatusCode, false);
                Assert.IsTrue(response.Content == null);
                Assert.IsTrue(string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

            }
            //existing resource exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Post_Student_Transcript_ExistingResource_Exception()
            {
                StudentTranscriptRequest sr = Initialize_Transcript_Request_To_Send();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Throws(new ExistingResourceException());
                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(sr);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.Conflict);
                Assert.AreEqual(response.IsSuccessStatusCode, false);
                Assert.IsTrue(response.Content == null);
                Assert.IsTrue(string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

            }
            //any other exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Post_Student_Transcript_Exception()
            {
                StudentTranscriptRequest sr = Initialize_Transcript_Request_To_Send();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Throws(new Exception());
                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(sr);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
                Assert.AreEqual(response.IsSuccessStatusCode, false);
                Assert.IsTrue(response.Content == null);
                Assert.IsTrue(string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

            }
            //if argument is null
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Post_Student_Transcript_Argument_Null_Exception()
            {
                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(null);
            }

            //validate missing required parameters

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentId_Transcript_Request_Is_Empty()
            {
                StudentTranscriptRequest str = new StudentTranscriptRequest("", "my name", new List<string>() { "address line 1" }) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(str);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ReciepientName_Transcript_Request_Is_Empty()
            {
                StudentTranscriptRequest str = new StudentTranscriptRequest("11111", "", new List<string>() { "address line 1" }) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "address line 1" };

                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(str);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentId_RecipientName_Transcript_Request_Is_Empty()
            {
                StudentTranscriptRequest str = new StudentTranscriptRequest(null, null, new List<string>() { "address line 1" }) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "address line 1" };

                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(str);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MailAddressLines_Transcript_Request_Is_Empty()
            {
                StudentTranscriptRequest str = new StudentTranscriptRequest("1111", "my name", null) { TranscriptGrouping = "UG" };
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                HttpResponseMessage response = await requestController.PostStudentTranscriptRequestAsync(str);
            }
        }

        [TestClass]
        public class StudentEnrollmentPostTests
        {
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

            private Mock<IStudentRequestService> studentRequestServiceMock;
            private Mock<ILogger> loggerMock;
            private IStudentRequestService requestService;
            private StudentRequestsController requestController;
            private ILogger logger;
            private Dtos.Student.StudentRequest returnedStudentRequest;
            private HttpRequestMessage httpRequest;

            [TestInitialize]
            public void Initilaize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentRequestServiceMock = new Mock<IStudentRequestService>();
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                requestService = studentRequestServiceMock.Object;
                returnedStudentRequest = ReturnedStudentEnrollmentRequest();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Returns(Task.FromResult<StudentRequest>(returnedStudentRequest));
                requestController = new StudentRequestsController(requestService, logger);
                httpRequest = Initialize_HttpRequest_Controller_Context();
                //specify httprequestmessage for controller
                requestController.Request = httpRequest;

                //initialize httpContext current value to request and response objects
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://localhost/api/CreateStudentEnrollmentRequests", ""), new HttpResponse(new StringWriter()));
            }

            private HttpRequestMessage Initialize_HttpRequest_Controller_Context()
            {
                //create new httpRequestMessage
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/CreateStudentEnrollmentRequests");
                //define the httpconfiguration only for routing to have url that will be defined as post response location header
                HttpConfiguration config = new HttpConfiguration();
                var route = config.Routes.MapHttpRoute("GetStudentEnrollmentRequest", "students/student-enrollment-request/{requestId}");
                var routeData = new System.Web.Http.Routing.HttpRouteData(route, new System.Web.Http.Routing.HttpRouteValueDictionary { { "controller", "studentRequests" } });
                //add the httpConfiguration to part of httpRequestMessage
                request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                return request;
            }
            private StudentEnrollmentRequest Initialize_Enrollment_Request_To_Send()
            {
                StudentEnrollmentRequest sr = new StudentEnrollmentRequest("11111", "my name", new List<string>() { "address line 1" });
                sr.Comments = "trial - TR";
                sr.HoldRequest = "GRADE";
                sr.NumberOfCopies = 3;
                return sr;
            }

            private StudentRequest ReturnedStudentEnrollmentRequest()
            {
                StudentRequest sr = new StudentEnrollmentRequest("11111", "my name", new List<string>() { "line 1" }) ;
                sr.Comments = "trial - TR";
                sr.HoldRequest = "GRADE";
                sr.NumberOfCopies = 3;
                sr.Id = "1";
                return sr;
            }

            [TestMethod]
            public async Task Post_Student_Enrollment_Request()
            {
                StudentEnrollmentRequest sr = Initialize_Enrollment_Request_To_Send();
                HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(sr);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
                Assert.AreEqual(response.IsSuccessStatusCode, true);
                Assert.IsTrue(response.Content != null);
                Assert.IsTrue(HttpContext.Current.Response.RedirectLocation.Contains("students/student-enrollment-request/1"));
                //deserialize response content and verify it is same as expected
                var responseEnrollmentRequest = JsonConvert.DeserializeObject<StudentEnrollmentRequest>(await response.Content.ReadAsStringAsync());
                StudentEnrollmentRequest expectedSr = ReturnedStudentEnrollmentRequest() as StudentEnrollmentRequest;
                Assert.AreEqual(responseEnrollmentRequest.StudentId, expectedSr.StudentId);
                Assert.AreEqual(responseEnrollmentRequest.Id, expectedSr.Id);
                Assert.AreEqual(responseEnrollmentRequest.RecipientName, expectedSr.RecipientName);

            }
            //permission exceptions
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Post_Student_Enrollment_Permission_Exception()
            {
                StudentEnrollmentRequest sr = Initialize_Enrollment_Request_To_Send();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Throws(new PermissionsException());
                HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(sr);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.Forbidden);
                Assert.AreEqual(response.IsSuccessStatusCode, false);
                Assert.IsTrue(response.Content == null);
                Assert.IsTrue(string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

            }
            //existing resource exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Post_Student_Enrollment_ExistingResource_Exception()
            {
                StudentEnrollmentRequest sr = Initialize_Enrollment_Request_To_Send();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Throws(new ExistingResourceException());
                HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(sr);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.Conflict);
                Assert.AreEqual(response.IsSuccessStatusCode, false);
                Assert.IsTrue(response.Content == null);
                Assert.IsTrue(string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

            }
            //any other exception
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Post_Student_Enrollment_Exception()
            {
                StudentEnrollmentRequest sr = Initialize_Enrollment_Request_To_Send();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.CreateStudentRequestAsync(It.IsAny<StudentRequest>())).Throws(new Exception());
                HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(sr);
                Assert.AreEqual(response.StatusCode, HttpStatusCode.BadRequest);
                Assert.AreEqual(response.IsSuccessStatusCode, false);
                Assert.IsTrue(response.Content == null);
                Assert.IsTrue(string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

            }
            //if argument is null
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Post_Student_Enrollment_Argument_Null_Exception()
            {
                HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(null);
            }

            //validate missing required parameters

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentId_Enrollment_Request_Is_Empty()
            {
                StudentEnrollmentRequest str = new StudentEnrollmentRequest("", "my name", new List<string>() { "address line 1" });
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(str);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ReciepientName_Enrollment_Request_Is_Empty()
            {
                StudentEnrollmentRequest str = new StudentEnrollmentRequest("11111", "", new List<string>() { "address line 1" }) ;
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "address line 1" };

                HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(str);
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task StudentId_RecipientName_Enrollment_Request_Is_Empty()
            {
                StudentEnrollmentRequest str = new StudentEnrollmentRequest(null, null, new List<string>() { "address line 1" });
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                str.MailToAddressLines = new List<string>() { "address line 1" };
                try
                {
                    HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(str);
                }
                catch (ArgumentException ex)
                {
                    Assert.IsTrue(ex.Message.Contains("StudentId"));
                    Assert.IsTrue(ex.Message.Contains("RecipientName"));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task MailAddressLines_Enrollment_Request_Is_Empty()
            {
                StudentEnrollmentRequest str = new StudentEnrollmentRequest("1111", "my name", null);
                str.HoldRequest = "GRADE";
                str.NumberOfCopies = 3;
                HttpResponseMessage response = await requestController.PostStudentEnrollmentRequestAsync(str);
            }
        }


        [TestClass]
        public class StudentTranscriptGetTests
        {
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

            private Mock<IStudentRequestService> studentRequestServiceMock;
            private Mock<ILogger> loggerMock;
            private IStudentRequestService requestService;
            private StudentRequestsController requestController;
            private ILogger logger;
            private Dtos.Student.StudentRequest returnedStudentRequest;

            [TestInitialize]
            public void Initilaize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentRequestServiceMock = new Mock<IStudentRequestService>();
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                requestService = studentRequestServiceMock.Object;
                returnedStudentRequest = ReturnedStudentTranscriptRequest();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.GetStudentRequestAsync(It.IsAny<string>())).Returns(Task.FromResult<StudentRequest>(returnedStudentRequest));
                requestController = new StudentRequestsController(requestService, logger);
            }

            private StudentRequest ReturnedStudentTranscriptRequest()
            {
                StudentRequest sr = new StudentTranscriptRequest("11111", "my name", new List<string>() { "line 1" }) { TranscriptGrouping = "UG" };
                sr.Comments = "trial - TR";
                sr.HoldRequest = "GRADE";
                sr.NumberOfCopies = 3;
                sr.Id = "1";
                return sr;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Request_Id_Is_Null()
            {
                await requestController.GetStudentTranscriptRequestAsync(null);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Permission_Transcript_Request_Exception()
            {
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.GetStudentRequestAsync(It.IsAny<string>())).Throws(new PermissionsException());
                try
                {
                    StudentTranscriptRequest str = await requestController.GetStudentTranscriptRequestAsync("1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFound_Transcript_Request_Exception()
            {
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.GetStudentRequestAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                try
                {
                    StudentTranscriptRequest str = await requestController.GetStudentTranscriptRequestAsync("1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AnyOther_Transcript_Request_Exception()
            {
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.GetStudentRequestAsync(It.IsAny<string>())).Throws(new Exception());
                try
                {
                    StudentTranscriptRequest str = await requestController.GetStudentTranscriptRequestAsync("1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            public async Task Valid_Transcript_Request_Returned()
            {
                StudentTranscriptRequest expected=ReturnedStudentTranscriptRequest() as StudentTranscriptRequest;
                    StudentTranscriptRequest str = await requestController.GetStudentTranscriptRequestAsync("1");
                Assert.AreEqual(str.StudentId,expected.StudentId);
                Assert.AreEqual(str.RecipientName, expected.RecipientName);
                Assert.AreEqual(str.TranscriptGrouping, expected.TranscriptGrouping);
            }
        }

        [TestClass]
        public class StudentEnrollmentGetTests
        {
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

            private Mock<IStudentRequestService> studentRequestServiceMock;
            private Mock<ILogger> loggerMock;
            private IStudentRequestService requestService;
            private StudentRequestsController requestController;
            private ILogger logger;
            private Dtos.Student.StudentRequest returnedStudentRequest;

            [TestInitialize]
            public void Initilaize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentRequestServiceMock = new Mock<IStudentRequestService>();
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                requestService = studentRequestServiceMock.Object;
                returnedStudentRequest = ReturnedStudentEnrollmentRequest();
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.GetStudentRequestAsync(It.IsAny<string>())).Returns(Task.FromResult<StudentRequest>(returnedStudentRequest));
                requestController = new StudentRequestsController(requestService, logger);
            }

            private StudentRequest ReturnedStudentEnrollmentRequest()
            {
                StudentRequest sr = new StudentEnrollmentRequest("11111", "my name", new List<string>() { "line 1" });
                sr.Comments = "trial - TR";
                sr.HoldRequest = "GRADE";
                sr.NumberOfCopies = 3;
                sr.Id = "1";
                return sr;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Request_Id_Is_Null()
            {
                await requestController.GetStudentEnrollmentRequestAsync(null);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Permission_Enrollment_Request_Exception()
            {
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.GetStudentRequestAsync(It.IsAny<string>())).Throws(new PermissionsException());
                try
                {
                    StudentEnrollmentRequest str = await requestController.GetStudentEnrollmentRequestAsync("1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFound_Enrollment_Request_Exception()
            {
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.GetStudentRequestAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                try
                {
                    StudentEnrollmentRequest str = await requestController.GetStudentEnrollmentRequestAsync("1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AnyOther_Enrollment_Request_Exception()
            {
                studentRequestServiceMock.Setup<Task<Dtos.Student.StudentRequest>>(s => s.GetStudentRequestAsync(It.IsAny<string>())).Throws(new Exception());
                try
                {
                    StudentEnrollmentRequest str = await requestController.GetStudentEnrollmentRequestAsync("1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            public async Task Valid_Enrollment_Request_Returned()
            {
                StudentEnrollmentRequest expected = ReturnedStudentEnrollmentRequest() as StudentEnrollmentRequest;
                StudentEnrollmentRequest str = await requestController.GetStudentEnrollmentRequestAsync("1");
                Assert.AreEqual(str.StudentId, expected.StudentId);
                Assert.AreEqual(str.RecipientName, expected.RecipientName);
            }
        }

        [TestClass]
        public class GetStudentEnrollmentRequestsTests
        {
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

            private Mock<IStudentRequestService> studentRequestServiceMock;
            private Mock<ILogger> loggerMock;
            private IStudentRequestService requestService;
            private StudentRequestsController requestController;
            private ILogger logger;
            private List<Dtos.Student.StudentRequest> returnedStudentRequests;

            [TestInitialize]
            public void Initilaize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentRequestServiceMock = new Mock<IStudentRequestService>();
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                requestService = studentRequestServiceMock.Object;
                returnedStudentRequests = ReturnedStudentEnrollmentRequests();
                studentRequestServiceMock.Setup<Task<List<Dtos.Student.StudentRequest>>>(s => s.GetStudentRequestsAsync(It.IsAny<string>(),It.IsAny<string>())).Returns(Task.FromResult<List<StudentRequest>>(returnedStudentRequests));
                requestController = new StudentRequestsController(requestService, logger);
            }

            private List<StudentRequest> ReturnedStudentEnrollmentRequests()
            {
                List<StudentRequest> returnedRequests = new List<StudentRequest>();
                StudentRequest sr = new StudentEnrollmentRequest("11111", "recipient 1 name", new List<string>() { "line 1" });
                sr.Comments = "comments";
                sr.NumberOfCopies = 3;
                sr.Id = "1";
                returnedRequests.Add(sr);
                
                StudentRequest sr2 = new StudentEnrollmentRequest("11111", "recipient 2 name", new List<string>() { "line 2" });
                sr2.Comments = "trial";
                sr2.NumberOfCopies = 2;
                sr2.Id = "2";
                returnedRequests.Add(sr2);
                return returnedRequests;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Student_Id_Is_Null()
            {
                await requestController.GetStudentEnrollmentRequestsAsync(null);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentEnrollmentRequests_ServiceThrowsPermissionException()
            {
                studentRequestServiceMock.Setup<Task<List<Dtos.Student.StudentRequest>>>(s => s.GetStudentRequestsAsync(It.IsAny<string>(),It.IsAny<string>())).Throws(new PermissionsException());
                try
                {
                    var requests = await requestController.GetStudentEnrollmentRequestsAsync("0000012");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentEnrollmentRequests_ServiceOtherException()
            {
                studentRequestServiceMock.Setup<Task<List<Dtos.Student.StudentRequest>>>(s => s.GetStudentRequestsAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
                try
                {
                    List<StudentEnrollmentRequest> str = await requestController.GetStudentEnrollmentRequestsAsync("0000011");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            public async Task Valid_StudentEnrollmentRequests_Returned()
            {
                var expected = ReturnedStudentEnrollmentRequests();
                    //as StudentEnrollmentRequest;

                var returnedRequests = await requestController.GetStudentEnrollmentRequestsAsync("0000011");
                Assert.AreEqual(expected.Count(), returnedRequests.Count());

            }
        }

        [TestClass]
        public class GetStudentTranscriptRequestsTests
        {
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

            private Mock<IStudentRequestService> studentRequestServiceMock;
            private Mock<ILogger> loggerMock;
            private IStudentRequestService requestService;
            private StudentRequestsController requestController;
            private ILogger logger;
            private List<Dtos.Student.StudentRequest> returnedStudentRequests;

            [TestInitialize]
            public void Initilaize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentRequestServiceMock = new Mock<IStudentRequestService>();
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                requestService = studentRequestServiceMock.Object;
                returnedStudentRequests = ReturnedStudentTranscriptRequests();
                studentRequestServiceMock.Setup<Task<List<Dtos.Student.StudentRequest>>>(s => s.GetStudentRequestsAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<List<StudentRequest>>(returnedStudentRequests));
                requestController = new StudentRequestsController(requestService, logger);
            }

            private List<StudentRequest> ReturnedStudentTranscriptRequests()
            {
                List<StudentRequest> returnedRequests = new List<StudentRequest>();
                StudentRequest sr = new StudentTranscriptRequest("11111", "recipient 1 name", new List<string>() { "line 1" });
                sr.Comments = "comments";
                sr.NumberOfCopies = 3;
                sr.Id = "1";
                returnedRequests.Add(sr);

                StudentRequest sr2 = new StudentTranscriptRequest("11111", "recipient 2 name", new List<string>() { "line 2" });
                sr2.Comments = "trial";
                sr2.NumberOfCopies = 2;
                sr2.Id = "2";
                returnedRequests.Add(sr2);
                return returnedRequests;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Student_Id_Is_Null()
            {
                await requestController.GetStudentTranscriptRequestsAsync(null);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentTranscriptRequests_ServiceThrowsPermissionException()
            {
                studentRequestServiceMock.Setup<Task<List<Dtos.Student.StudentRequest>>>(s => s.GetStudentRequestsAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new PermissionsException());
                try
                {
                    var requests = await requestController.GetStudentTranscriptRequestsAsync("0000012");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentTranscriptRequests_ServiceOtherException()
            {
                studentRequestServiceMock.Setup<Task<List<Dtos.Student.StudentRequest>>>(s => s.GetStudentRequestsAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());
                try
                {
                    List<StudentTranscriptRequest> str = await requestController.GetStudentTranscriptRequestsAsync("0000011");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            public async Task Valid_StudentTranscriptRequests_Returned()
            {
                var expected = ReturnedStudentTranscriptRequests();
                //as StudentTranscriptRequest;

                var returnedRequests = await requestController.GetStudentTranscriptRequestsAsync("0000011");
                Assert.AreEqual(expected.Count(), returnedRequests.Count());

            }
        }

        [TestClass]
        public class GetStudentRequestFeeTests
        {
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
            private IStudentRequestService studentRequestService;
            private Mock<IStudentRequestService> studentRequestServiceMock;
            private StudentRequestsController studentRequestController;
            private Ellucian.Colleague.Dtos.Student.StudentRequestFee studentRequestFeeDto;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                studentRequestServiceMock = new Mock<IStudentRequestService>();
                studentRequestService = studentRequestServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                studentRequestFeeDto = new Ellucian.Colleague.Dtos.Student.StudentRequestFee();
                studentRequestFeeDto.StudentId = "0004032";
                studentRequestFeeDto.RequestId = "12345";
                studentRequestFeeDto.Amount = 30m;
                studentRequestFeeDto.PaymentDistributionCode = "BANK";
                studentRequestController = new StudentRequestsController(studentRequestService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRequestController = null;
                studentRequestService = null;
            }

            [TestMethod]
            public async Task GetStudentRequestFeeAsync_ForGivenStudentId_ReturnsStudentRequestFeeDto()
            {
                studentRequestServiceMock.Setup(x => x.GetStudentRequestFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(studentRequestFeeDto));
                var studentRequestFee = await studentRequestController.GetStudentRequestFeeAsync("0004032", "12345");
                Assert.IsTrue(studentRequestFee is Dtos.Student.StudentRequestFee);
                Assert.AreEqual("0004032", studentRequestFeeDto.StudentId);
                Assert.AreEqual("12345", studentRequestFeeDto.RequestId);
                Assert.AreEqual("BANK", studentRequestFeeDto.PaymentDistributionCode);
                Assert.AreEqual(30m, studentRequestFeeDto.Amount);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentRequestFeeAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    studentRequestServiceMock.Setup(x => x.GetStudentRequestFeeAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new ApplicationException());
                    var studentRequest = await studentRequestController.GetStudentRequestFeeAsync("0004032", "12345");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

        }
    }
}