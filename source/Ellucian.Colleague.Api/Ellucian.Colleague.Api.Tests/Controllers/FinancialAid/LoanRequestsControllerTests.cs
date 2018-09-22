/*Copyright 2014-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class LoanRequestsControllerTests
    {
        [TestClass]
        public class GetLoanRequestTests
        {
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

            private Mock<ILoanRequestService> loanRequestServiceMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;

            private string inputId;

            private LoanRequest inputLoanRequest;
            private LoanRequest expectedLoanRequest;
            private LoanRequest actualLoanRequest;

            private LoanRequestsController loanRequestsController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                loanRequestServiceMock = new Mock<ILoanRequestService>();

                inputId = "1";

                inputLoanRequest = new LoanRequest()
                {
                    Id = inputId,
                    StudentId = "0003914",
                    AwardYear = "2014",
                    RequestDate = DateTime.Today,
                    AssignedToId = "1111111",
                    TotalRequestAmount = 4530,
                    LoanRequestPeriods = new List<LoanRequestPeriod>()
                    {
                        new LoanRequestPeriod() { Code = "13/FA", LoanAmount = 2500 },
                        new LoanRequestPeriod() { Code = "14/SP", LoanAmount = 2500 }
                    },
                    Status = LoanRequestStatus.Accepted,
                    StatusDate = DateTime.Today,
                    StudentComments = "This is my comment"
                };                

                expectedLoanRequest = new LoanRequest()
                {
                    Id = inputId,
                    StudentId = "0003914",
                    AwardYear = "2014",
                    RequestDate = DateTime.Today,
                    AssignedToId = "1111111",
                    TotalRequestAmount = 4530,
                    LoanRequestPeriods = new List<LoanRequestPeriod>()
                    {
                        new LoanRequestPeriod() { Code = "13/FA", LoanAmount = 2500 },
                        new LoanRequestPeriod() { Code = "14/SP", LoanAmount = 2500 }
                    },
                    Status = LoanRequestStatus.Accepted,
                    StatusDate = DateTime.Today,
                    StudentComments = "This is my comment"
                };                

                loanRequestServiceMock = new Mock<ILoanRequestService>();
                loanRequestServiceMock.Setup(s => s.GetLoanRequestAsync(inputId)).ReturnsAsync(inputLoanRequest);

                loanRequestsController = new LoanRequestsController(adapterRegistryMock.Object, loanRequestServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                actualLoanRequest = await loanRequestsController.GetLoanRequestAsync(inputId);
                Assert.AreEqual(expectedLoanRequest.Id, actualLoanRequest.Id);
                Assert.AreEqual(expectedLoanRequest.StudentId, actualLoanRequest.StudentId);
                Assert.AreEqual(expectedLoanRequest.AwardYear, actualLoanRequest.AwardYear);
                Assert.AreEqual(expectedLoanRequest.RequestDate, actualLoanRequest.RequestDate);
                Assert.AreEqual(expectedLoanRequest.TotalRequestAmount, actualLoanRequest.TotalRequestAmount);
                Assert.AreEqual(expectedLoanRequest.LoanRequestPeriods.Count, actualLoanRequest.LoanRequestPeriods.Count);
                Assert.AreEqual(expectedLoanRequest.AssignedToId, actualLoanRequest.AssignedToId);
                Assert.AreEqual(expectedLoanRequest.Status, actualLoanRequest.Status);
                Assert.AreEqual(expectedLoanRequest.StatusDate, actualLoanRequest.StatusDate);
                Assert.AreEqual(expectedLoanRequest.StudentComments, actualLoanRequest.StudentComments);

                for ( var i = 0; i < expectedLoanRequest.LoanRequestPeriods.Count; i++ )
                {
                    var expectedLoanRequestPeriod = expectedLoanRequest.LoanRequestPeriods[i];
                    Assert.IsTrue(actualLoanRequest.LoanRequestPeriods.Select(lrp => lrp.Code).Contains(expectedLoanRequestPeriod.Code));
                    Assert.AreEqual(expectedLoanRequestPeriod.LoanAmount, actualLoanRequest.LoanRequestPeriods.First(lrp => lrp.Code == expectedLoanRequestPeriod.Code).LoanAmount);
                }
            }

            [TestMethod]
            public async Task NullInputIdThrowsExceptionTest()
            {
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.GetLoanRequestAsync(null);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught, "Expected (NullArgument) HttpResponseException was not caught");
            }

            [TestMethod]
            public async Task CatchPermissionsExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.GetLoanRequestAsync(inputId)).Throws(new PermissionsException("pex"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.GetLoanRequestAsync(inputId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught, "Expected PermissionsException was not caught");

                loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchKeyNotFoundExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.GetLoanRequestAsync(inputId)).Throws(new KeyNotFoundException("knfe"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.GetLoanRequestAsync(inputId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught, "Expected KeyNotFoundException was not caught");

                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchApplicationExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.GetLoanRequestAsync(inputId)).Throws(new ApplicationException("ae"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.GetLoanRequestAsync(inputId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught, "Expected ApplicationException was not caught");

                loggerMock.Verify(l => l.Error(It.IsAny<ApplicationException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchGenericExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.GetLoanRequestAsync(inputId)).Throws(new Exception("e"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.GetLoanRequestAsync(inputId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught, "Expected Exception was not caught");

                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }

        [TestClass]
        public class CreateLoanRequestTests
        {
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

            private Mock<ILoanRequestService> loanRequestServiceMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;

            //private string inputId;

            private LoanRequest inputLoanRequest;
            private LoanRequest newLoanRequest;
            private LoanRequest expectedLoanRequest;
            private LoanRequest actualLoanRequest;

            private LoanRequestsController loanRequestsController;
            private HttpResponse httpResponse;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                loanRequestServiceMock = new Mock<ILoanRequestService>();

                inputLoanRequest = new LoanRequest()
                {
                    StudentId = "0003914",
                    AwardYear = "2014",
                    TotalRequestAmount = 4530,
                    LoanRequestPeriods = new List<LoanRequestPeriod>()
                    {
                        new LoanRequestPeriod(){
                            Code = "13/FA",
                            LoanAmount = 2265
                        },

                        new LoanRequestPeriod(){
                            Code = "14/SP",
                            LoanAmount = 2265
                        }
                    },
                    StudentComments = "This is my comment"
                };

                newLoanRequest = new LoanRequest()
                {
                    Id = "foobar",
                    StudentId = "0003914",
                    AwardYear = "2014",
                    RequestDate = DateTime.Today,
                    AssignedToId = "1111111",
                    TotalRequestAmount = 4530,
                    LoanRequestPeriods = new List<LoanRequestPeriod>()
                    {
                        new LoanRequestPeriod(){
                            Code = "13/FA",
                            LoanAmount = 2265
                        },

                        new LoanRequestPeriod(){
                            Code = "14/SP",
                            LoanAmount = 2265
                        }
                    },
                    Status = LoanRequestStatus.Pending,
                    StatusDate = DateTime.Today,
                    StudentComments = "This is my comment"
                };

                expectedLoanRequest = new LoanRequest()
                {
                    Id = "foobar",
                    StudentId = "0003914",
                    AwardYear = "2014",
                    RequestDate = DateTime.Today,
                    AssignedToId = "1111111",
                    TotalRequestAmount = 4530,
                    LoanRequestPeriods = new List<LoanRequestPeriod>()
                    {
                        new LoanRequestPeriod(){
                            Code = "13/FA",
                            LoanAmount = 2265
                        },

                        new LoanRequestPeriod(){
                            Code = "14/SP",
                            LoanAmount = 2265
                        }
                    },
                    Status = LoanRequestStatus.Pending,
                    StatusDate = DateTime.Today,
                    StudentComments = "This is my comment"
                };

                loanRequestServiceMock = new Mock<ILoanRequestService>();
                loanRequestServiceMock.Setup(s =>
                    s.CreateLoanRequestAsync(It.Is<LoanRequest>(l => l.StudentId == newLoanRequest.StudentId && l.AwardYear == newLoanRequest.AwardYear)))
                    .ReturnsAsync(newLoanRequest);

                //this sets up the route for location link resolution
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/loan-requests/");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetLoanRequest",
                    routeTemplate: "api/loan-requests/{id}",
                    defaults: new { controller = "LoanRequests", action = "GetLoanRequest" }
                );
                var routeData = new HttpRouteData(getRoute);
                var context = new HttpControllerContext(config, routeData, request);

                httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                loanRequestsController = new LoanRequestsController(adapterRegistryMock.Object, loanRequestServiceMock.Object, loggerMock.Object);

                //this sets up the request
                loanRequestsController.ControllerContext = new HttpControllerContext(config, routeData, request);
                loanRequestsController.Request = request;
                loanRequestsController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                loanRequestsController.Url = new UrlHelper(request);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var response = await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                actualLoanRequest = JsonConvert.DeserializeObject<LoanRequest>(response.Content.ReadAsStringAsync().Result);

                Assert.AreEqual(expectedLoanRequest.Id, actualLoanRequest.Id);
                Assert.AreEqual(expectedLoanRequest.StudentId, actualLoanRequest.StudentId);
                Assert.AreEqual(expectedLoanRequest.AwardYear, actualLoanRequest.AwardYear);
                Assert.AreEqual(expectedLoanRequest.RequestDate, actualLoanRequest.RequestDate);
                Assert.AreEqual(expectedLoanRequest.AssignedToId, actualLoanRequest.AssignedToId);
                Assert.AreEqual(expectedLoanRequest.TotalRequestAmount, actualLoanRequest.TotalRequestAmount);
                Assert.AreEqual(expectedLoanRequest.LoanRequestPeriods.Count, actualLoanRequest.LoanRequestPeriods.Count);
                Assert.AreEqual(expectedLoanRequest.Status, actualLoanRequest.Status);
                Assert.AreEqual(expectedLoanRequest.StatusDate, actualLoanRequest.StatusDate);
                Assert.AreEqual(expectedLoanRequest.StudentComments, actualLoanRequest.StudentComments);

                for (var i = 0; i < expectedLoanRequest.LoanRequestPeriods.Count; i++)
                {
                    var expectedLoanRequestPeriod = expectedLoanRequest.LoanRequestPeriods[i];
                    Assert.IsTrue(actualLoanRequest.LoanRequestPeriods.Select(lrp => lrp.Code).Contains(expectedLoanRequestPeriod.Code));
                    Assert.AreEqual(expectedLoanRequestPeriod.LoanAmount, actualLoanRequest.LoanRequestPeriods.First(lrp => lrp.Code == expectedLoanRequestPeriod.Code).LoanAmount);
                }
            }

            [TestMethod]
            public async Task CreatedResponseCodeTest()
            {
                var response = await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
            }

            [TestMethod]
            public async Task NullInputThrowsExceptionTest()
            {
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.CreateLoanRequestAsync(null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task NullStudentIdInputThrowsExceptionTest()
            {
                bool exceptionCaught = false;
                try
                {
                    inputLoanRequest.StudentId = "";
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task NullAwardYearInputThrowsExceptionTest()
            {
                bool exceptionCaught = false;
                try
                {
                    inputLoanRequest.AwardYear = "";
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    exceptionCaught = true;
                }

                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public async Task CatchArgumentExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.CreateLoanRequestAsync(inputLoanRequest)).Throws(new ArgumentException("ae"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchPermissionsExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.CreateLoanRequestAsync(inputLoanRequest)).Throws(new PermissionsException("pe"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchExistingResourceExceptionTest()
            {
                var existingResourceId = "helloWorld";
                loanRequestServiceMock.Setup(s => s.CreateLoanRequestAsync(inputLoanRequest)).Throws(new ExistingResourceException("ere", existingResourceId));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.Conflict, hre.Response.StatusCode);

                    //Doesn't work because HttpContext is used
                    //var locationHeaderValue = hre.Response.Headers.Location;
                    //Assert.AreEqual(existingResourceId, locationHeaderValue.Segments.Last());
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<ExistingResourceException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchOperationCanceledExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.CreateLoanRequestAsync(inputLoanRequest)).Throws(new OperationCanceledException("oce"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.Conflict, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<OperationCanceledException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchInvalidOperationExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.CreateLoanRequestAsync(inputLoanRequest)).Throws(new InvalidOperationException("ioe"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchApplicationExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.CreateLoanRequestAsync(inputLoanRequest)).Throws(new ApplicationException("ae"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<ApplicationException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task CatchGenericExceptionTest()
            {
                loanRequestServiceMock.Setup(s => s.CreateLoanRequestAsync(inputLoanRequest)).Throws(new Exception("e"));
                bool exceptionCaught = false;
                try
                {
                    await loanRequestsController.CreateLoanRequestAsync(inputLoanRequest);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            }
        }
    }
}
