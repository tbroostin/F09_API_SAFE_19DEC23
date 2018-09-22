/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class AwardPackageChangeRequestsControllerTests
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

        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<IAwardPackageChangeRequestService> awardPackageChangeRequestServiceMock;
        public Mock<ILogger> loggerMock;

        public string studentId;
        public string changeRequestId;

        public List<AwardPackageChangeRequest> inputChangeRequests;

        public AwardPackageChangeRequestsController actualController;

        public FunctionEqualityComparer<AwardPackageChangeRequest> awardPackageChangeRequestComparer;

        public void AwardPackageChangeRequestsControllerInitialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            awardPackageChangeRequestServiceMock = new Mock<IAwardPackageChangeRequestService>();

            awardPackageChangeRequestComparer = new FunctionEqualityComparer<AwardPackageChangeRequest>(
                (cr1, cr2) => cr1.StudentId == cr2.StudentId && cr1.AwardYearId == cr2.AwardYearId && cr1.AwardId == cr2.AwardId,
                cr => cr.StudentId.GetHashCode() ^ cr.AwardId.GetHashCode() ^ cr.AwardYearId.GetHashCode());

            inputChangeRequests = BuildAwardPackageChangeRequests();

            studentId = inputChangeRequests.First().StudentId;
            changeRequestId = inputChangeRequests.First().Id;

            awardPackageChangeRequestServiceMock.Setup(s => s.GetAwardPackageChangeRequestsAsync(It.IsAny<string>()))
                .Returns<string>(stuId => Task.FromResult(inputChangeRequests.Where(cr => cr.StudentId == stuId)));

            awardPackageChangeRequestServiceMock.Setup(s => s.GetAwardPackageChangeRequestAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((stuId, id) => Task.FromResult(inputChangeRequests.FirstOrDefault(cr => cr.StudentId == stuId && cr.Id == id)));

            awardPackageChangeRequestServiceMock.Setup(s => s.CreateAwardPackageChangeRequestAsync(It.IsAny<string>(), It.IsAny<AwardPackageChangeRequest>()))
                .Returns<string, AwardPackageChangeRequest>(async (stuId, cr) =>
                    {
                        return await Task.FromResult(new AwardPackageChangeRequest()
                        {
                            StudentId = stuId,
                            AwardId = cr.AwardId,
                            AwardYearId = cr.AwardYearId,
                            AwardPeriodChangeRequests = cr.AwardPeriodChangeRequests.Select(inCr => new AwardPeriodChangeRequest()
                            {
                                AwardPeriodId = inCr.AwardPeriodId,
                                NewAmount = inCr.NewAmount,
                                NewAwardStatusId = inCr.NewAwardStatusId,
                                Status = AwardPackageChangeRequestStatus.Pending
                            }),
                            AssignedToCounselorId = "counselorId",
                            CreateDateTime = new DateTimeOffset(DateTime.Now),
                            Id = cr.Id
                        });
                    });

            actualController = new AwardPackageChangeRequestsController(adapterRegistryMock.Object, loggerMock.Object, awardPackageChangeRequestServiceMock.Object);

        }

        private List<AwardPackageChangeRequest> BuildAwardPackageChangeRequests()
        {
            return new List<AwardPackageChangeRequest>()
            {
                new AwardPackageChangeRequest()
                {
                    Id = "1234",
                    StudentId = "0003914",
                    AwardYearId = "2014",
                    AwardId = "WOOFY",
                    AssignedToCounselorId = "0010479",
                    CreateDateTime = new DateTimeOffset(DateTime.Now),
                    AwardPeriodChangeRequests = new List<AwardPeriodChangeRequest>()
                    {
                        new AwardPeriodChangeRequest()
                        {
                            AwardPeriodId = "14/FA",
                            NewAmount = 500,
                            Status = AwardPackageChangeRequestStatus.Pending
                        }
                    }
                },
                new AwardPackageChangeRequest()
                {
                    Id = "1235",
                    StudentId = "0003914",
                    AwardYearId = "2014",
                    AwardId = "LOOPY",
                    AssignedToCounselorId = "0010479",
                    CreateDateTime = new DateTimeOffset(DateTime.Now),
                    AwardPeriodChangeRequests = new List<AwardPeriodChangeRequest>()
                    {
                        new AwardPeriodChangeRequest()
                        {
                            AwardPeriodId = "14/FA",
                            NewAwardStatusId = "A",
                            Status = AwardPackageChangeRequestStatus.Pending
                        }
                    }
                },
                new AwardPackageChangeRequest()
                {
                    Id = "1236",
                    StudentId = "0004791",
                    AwardYearId = "2013",
                    AwardId = "LOOPY",
                    AssignedToCounselorId = "0010479",
                    CreateDateTime = new DateTimeOffset(DateTime.Now),
                    AwardPeriodChangeRequests = new List<AwardPeriodChangeRequest>()
                    {
                        new AwardPeriodChangeRequest()
                        {
                            AwardPeriodId = "13/FA",
                            NewAwardStatusId = "A",
                            Status = AwardPackageChangeRequestStatus.Accepted
                        }
                    }
                },
                new AwardPackageChangeRequest()
                {
                    Id = "1237",
                    StudentId = "0004791",
                    AwardYearId = "2013",
                    AwardId = "SNEEZY",
                    AssignedToCounselorId = "0010479",
                    CreateDateTime = new DateTimeOffset(DateTime.Now),
                    AwardPeriodChangeRequests = new List<AwardPeriodChangeRequest>()
                    {
                        new AwardPeriodChangeRequest()
                        {
                            AwardPeriodId = "13/FA",
                            NewAwardStatusId = "A",
                            Status = AwardPackageChangeRequestStatus.RejectedByCounselor
                        }
                    }
                },
            };
        }

        [TestClass]
        public class GetStudentAwardPackageChangeRequestsTests : AwardPackageChangeRequestsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestsControllerInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actual = await actualController.GetAwardPackageChangeRequestsAsync(studentId);
                var expected = inputChangeRequests.Where(cr => cr.StudentId == studentId);
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList(), awardPackageChangeRequestComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                try
                {
                    await actualController.GetAwardPackageChangeRequestsAsync(null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermisisonsExceptionLogsMessageTest()
            {
                awardPackageChangeRequestServiceMock.Setup(s => s.GetAwardPackageChangeRequestsAsync(It.IsAny<string>())).Throws(new PermissionsException("pe"));

                try
                {
                    await actualController.GetAwardPackageChangeRequestsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionLogsMessageTest()
            {
                awardPackageChangeRequestServiceMock.Setup(s => s.GetAwardPackageChangeRequestsAsync(It.IsAny<string>())).Throws(new Exception("ex"));

                try
                {
                    await actualController.GetAwardPackageChangeRequestsAsync(studentId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class GetSingleAwardPackageChangeRequestTests : AwardPackageChangeRequestsControllerTests
        {
            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestsControllerInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var actual = await actualController.GetAwardPackageChangeRequestAsync(studentId, changeRequestId);

                Assert.IsTrue(inputChangeRequests.Contains(actual, awardPackageChangeRequestComparer));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                try
                {
                    await actualController.GetAwardPackageChangeRequestAsync(null, changeRequestId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ChangeRequestIdRequiredTest()
            {
                try
                {
                    await actualController.GetAwardPackageChangeRequestAsync(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermisisonsExceptionLogsMessageTest()
            {
                awardPackageChangeRequestServiceMock.Setup(s => s.GetAwardPackageChangeRequestAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new PermissionsException("pe"));

                try
                {
                    await actualController.GetAwardPackageChangeRequestAsync(studentId, changeRequestId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionLogsMessageTest()
            {
                awardPackageChangeRequestServiceMock.Setup(s => s.GetAwardPackageChangeRequestAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("ex"));

                try
                {
                    await actualController.GetAwardPackageChangeRequestAsync(studentId, changeRequestId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class CreateAwardPackageChangeRequestAsyncTest : AwardPackageChangeRequestsControllerTests
        {
            public HttpResponse httpResponse;
            public AwardPackageChangeRequest inputChangeRequest;
            public AwardPackageChangeRequest actualChangeRequest;

            [TestInitialize]
            public void Initialize()
            {
                AwardPackageChangeRequestsControllerInitialize();

                inputChangeRequest = inputChangeRequests.First();

                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/students/" + studentId + "/award-package-change-requests");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetStudentAwardPackageChangeRequest",
                    routeTemplate: "api/students/{studentId}/award-package-change-requests/{id}",
                    defaults: new { controller = "AwardPackageChangeRequests", action = "GetAwardPackageChangeRequest" }
                );
                var routeData = new HttpRouteData(getRoute);
                var context = new HttpControllerContext(config, routeData, request);

                httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                actualController = new AwardPackageChangeRequestsController(adapterRegistryMock.Object, loggerMock.Object, awardPackageChangeRequestServiceMock.Object);

                //this sets up the request
                actualController.ControllerContext = new HttpControllerContext(config, routeData, request);
                actualController.Request = request;
                actualController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                actualController.Url = new UrlHelper(request);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var response = await actualController.PostAwardPackageChangeRequestAsync(studentId, inputChangeRequest);
                actualChangeRequest = JsonConvert.DeserializeObject<AwardPackageChangeRequest>(response.Content.ReadAsStringAsync().Result);

                Assert.IsTrue(awardPackageChangeRequestComparer.Equals(inputChangeRequest, actualChangeRequest));
            }

            [TestMethod]
            public async Task CreatedResponseCodeTest()
            {
                inputChangeRequest.Id = "newId";
                var response = await actualController.PostAwardPackageChangeRequestAsync(studentId, inputChangeRequest);
                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                try
                {
                    await actualController.PostAwardPackageChangeRequestAsync(null, inputChangeRequest);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ChangeRequestRequiredTest()
            {
                try
                {
                    await actualController.PostAwardPackageChangeRequestAsync(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionTest()
            {
                awardPackageChangeRequestServiceMock.Setup(s => s.CreateAwardPackageChangeRequestAsync(It.IsAny<string>(), It.IsAny<AwardPackageChangeRequest>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await actualController.PostAwardPackageChangeRequestAsync(studentId, inputChangeRequest);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ExistingResourceExceptionTest()
            {
                var existingId = "foo";
                awardPackageChangeRequestServiceMock.Setup(s => s.CreateAwardPackageChangeRequestAsync(It.IsAny<string>(), It.IsAny<AwardPackageChangeRequest>()))
                    .Throws(new ExistingResourceException("erex", existingId));

                try
                {
                    await actualController.PostAwardPackageChangeRequestAsync(studentId, inputChangeRequest);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<ExistingResourceException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Conflict, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionTest()
            {
                awardPackageChangeRequestServiceMock.Setup(s => s.CreateAwardPackageChangeRequestAsync(It.IsAny<string>(), It.IsAny<AwardPackageChangeRequest>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await actualController.PostAwardPackageChangeRequestAsync(studentId, inputChangeRequest);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

        }
    }
}
