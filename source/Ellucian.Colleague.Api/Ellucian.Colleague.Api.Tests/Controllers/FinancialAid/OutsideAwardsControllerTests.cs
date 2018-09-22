/*Copyright 2016-2018 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class OutsideAwardsControllerTests
    {
        [TestClass]
        public class CreateOutsideAwardAsyncTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IOutsideAwardsService> outsideAwardsServiceMock;

            private OutsideAward expectedOutsideAward;
            private OutsideAward actualOutsideAward;

            private OutsideAwardsController outsideAwardsController;
            private HttpResponse httpResponse;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                outsideAwardsServiceMock = new Mock<IOutsideAwardsService>();

                expectedOutsideAward = new OutsideAward()
                {
                    StudentId = "0003914",
                    AwardYearCode = "2016",
                    AwardName = "AwardName",
                    AwardType = "Loan",
                    AwardAmount = 4678,
                    AwardFundingSource = "Self"
                };

                outsideAwardsServiceMock.Setup(s => s.CreateOutsideAwardAsync(It.IsAny<OutsideAward>())).ReturnsAsync(expectedOutsideAward);

                //this sets up the route for location link resolution
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/outside-awards/");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetOutsideAwards",
                    routeTemplate: "api/students/{studentId}/outside-awards/{year}",
                    defaults: new { controller = "OutsideAwards", action = "GetOutsideAwardsAsync" }
                );
                var routeData = new HttpRouteData(getRoute);
                var context = new HttpControllerContext(config, routeData, request);

                httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                outsideAwardsController = new OutsideAwardsController(adapterRegistryMock.Object, outsideAwardsServiceMock.Object, loggerMock.Object);

                outsideAwardsController.ControllerContext = new HttpControllerContext(config, routeData, request);
                outsideAwardsController.Request = request;
                outsideAwardsController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                outsideAwardsController.Url = new UrlHelper(request);

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                outsideAwardsServiceMock = null;
                outsideAwardsController = null;
                httpResponse = null;
                expectedOutsideAward = null;
                actualOutsideAward = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullOutsideAwardPassed_HttpResponseExceptionThrownTest()
            {
                await outsideAwardsController.CreateOutsideAwardAsync(null);
            }

            [TestMethod]
            public async Task ActualEqualsExpectedTest()
            {
                var response = await outsideAwardsController.CreateOutsideAwardAsync(expectedOutsideAward);
                actualOutsideAward = JsonConvert.DeserializeObject<OutsideAward>(await response.Content.ReadAsStringAsync());

                Assert.AreEqual(expectedOutsideAward.StudentId, actualOutsideAward.StudentId);
                Assert.AreEqual(expectedOutsideAward.AwardName, actualOutsideAward.AwardName);
                Assert.AreEqual(expectedOutsideAward.AwardType, actualOutsideAward.AwardType);
                Assert.AreEqual(expectedOutsideAward.AwardYearCode, actualOutsideAward.AwardYearCode);
                Assert.AreEqual(expectedOutsideAward.AwardAmount, actualOutsideAward.AwardAmount);
                Assert.AreEqual(expectedOutsideAward.AwardFundingSource, actualOutsideAward.AwardFundingSource);
            }

            [TestMethod]
            public async Task CreatedResponseCode_EqualsExpectedTest()
            {
                var response = await outsideAwardsController.CreateOutsideAwardAsync(expectedOutsideAward);
                Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentNullExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.CreateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new ArgumentNullException("ex"));
                try
                {
                    await outsideAwardsController.CreateOutsideAwardAsync(expectedOutsideAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.CreateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new ArgumentException("ex"));
                try
                {
                    await outsideAwardsController.CreateOutsideAwardAsync(expectedOutsideAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.CreateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new PermissionsException("ex"));
                try
                {
                    await outsideAwardsController.CreateOutsideAwardAsync(expectedOutsideAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ApplicationExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.CreateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new ApplicationException("ex"));
                try
                {
                    await outsideAwardsController.CreateOutsideAwardAsync(expectedOutsideAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
            
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.CreateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new Exception("ex"));
                try
                {
                    await outsideAwardsController.CreateOutsideAwardAsync(expectedOutsideAward);
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
        public class GetOutsideAwardsAsyncTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IOutsideAwardsService> outsideAwardsServiceMock;

            private List<OutsideAward> expectedOutsideAwards;
            private List<OutsideAward> actualOutsideAwards;

            private OutsideAwardsController outsideAwardsController;
            private HttpResponse httpResponse;

            private string studentId, awardYearCode;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                outsideAwardsServiceMock = new Mock<IOutsideAwardsService>();

                expectedOutsideAwards = new List<OutsideAward>(){
                    new OutsideAward()
                    {
                        StudentId = "0003914",
                        AwardYearCode = "2016",
                        AwardName = "AwardName",
                        AwardType = "Scholarship",
                        AwardAmount = 4678.67m,
                        AwardFundingSource = "Self"
                    },
                    new OutsideAward()
                    {
                        StudentId = "0003914",
                        AwardYearCode = "2016",
                        AwardName = "LoanName",
                        AwardType = "Loan",
                        AwardAmount = 5000,
                        AwardFundingSource = "Bank"
                    }
                };

                studentId = "0003914";
                awardYearCode = "2016";

                outsideAwardsServiceMock.Setup(s => s.GetOutsideAwardsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(expectedOutsideAwards);

                //this sets up the route for location link resolution
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/outside-awards/");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetOutsideAwards",
                    routeTemplate: "api/students/{studentId}/outside-awards/{year}",
                    defaults: new { controller = "OutsideAwards", action = "GetOutsideAwardsAsync" }
                );
                var routeData = new HttpRouteData(getRoute);
                var context = new HttpControllerContext(config, routeData, request);

                httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                outsideAwardsController = new OutsideAwardsController(adapterRegistryMock.Object, outsideAwardsServiceMock.Object, loggerMock.Object);

                outsideAwardsController.ControllerContext = new HttpControllerContext(config, routeData, request);
                outsideAwardsController.Request = request;
                outsideAwardsController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                outsideAwardsController.Url = new UrlHelper(request);

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                outsideAwardsServiceMock = null;
                outsideAwardsController = null;
                httpResponse = null;
                expectedOutsideAwards = null;
                actualOutsideAwards = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullStudentIdPassed_HttpResponseExceptionThrownTest()
            {
                await outsideAwardsController.GetOutsideAwardsAsync(null, awardYearCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullAwardYearCodePassed_HttpResponseExceptionThrownTest()
            {
                await outsideAwardsController.GetOutsideAwardsAsync(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentNullExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.GetOutsideAwardsAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentNullException("ex"));
                try
                {
                    await outsideAwardsController.GetOutsideAwardsAsync(studentId, awardYearCode);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.GetOutsideAwardsAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new PermissionsException("ex"));
                try
                {
                    await outsideAwardsController.GetOutsideAwardsAsync(studentId, awardYearCode);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.GetOutsideAwardsAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception("ex"));
                try
                {
                    await outsideAwardsController.GetOutsideAwardsAsync(studentId, awardYearCode);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public async Task NoRecordsForStudentIdYear_EmptyListReturnedTest()
            {
                outsideAwardsServiceMock.Setup(s => s.GetOutsideAwardsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<OutsideAward>());
                actualOutsideAwards = (await outsideAwardsController.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                
                Assert.IsFalse(actualOutsideAwards.Any());
            }

            [TestMethod]
            public async Task ExpectedOutsideAwardRecordsNumberReturnedTest()
            {
                actualOutsideAwards = (await outsideAwardsController.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                Assert.AreEqual(expectedOutsideAwards.Count, actualOutsideAwards.Count);
            }

            [TestMethod]
            public async Task ActualOutsideAwards_EqualExpectedRecordsTest()
            {
                actualOutsideAwards = (await outsideAwardsController.GetOutsideAwardsAsync(studentId, awardYearCode)).ToList();
                CollectionAssert.AreEqual(expectedOutsideAwards, actualOutsideAwards);
            }

        }

        [TestClass]
        public class DeleteOutsideAwardAsyncTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IOutsideAwardsService> outsideAwardsServiceMock;

            private OutsideAwardsController outsideAwardsController;
            private HttpResponse httpResponse;

            private string studentId, recordId;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                outsideAwardsServiceMock = new Mock<IOutsideAwardsService>();
                

                outsideAwardsServiceMock.Setup(s => s.DeleteOutsideAwardAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(""));

                //this sets up the route for location link resolution
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/outside-awards/");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "DeleteOutsideAward",
                    routeTemplate: "api/students/{studentId}/outside-awards/{id}",
                    defaults: new { controller = "OutsideAwards", action = "DeleteOutsideAwardAsync" }
                );
                var routeData = new HttpRouteData(getRoute);
                var context = new HttpControllerContext(config, routeData, request);

                httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                outsideAwardsController = new OutsideAwardsController(adapterRegistryMock.Object, outsideAwardsServiceMock.Object, loggerMock.Object);

                outsideAwardsController.ControllerContext = new HttpControllerContext(config, routeData, request);
                outsideAwardsController.Request = request;
                outsideAwardsController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                outsideAwardsController.Url = new UrlHelper(request);

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

                studentId = "0004791";
                recordId = "n/a";
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                outsideAwardsServiceMock = null;
                outsideAwardsController = null;
                httpResponse = null;                
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullRecordId_HttpResponseExceptionThrownTest()
            {
                await outsideAwardsController.DeleteOutsideAwardAsync(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullStudentId_HttpResponseExceptionThrownTest()
            {
                await outsideAwardsController.DeleteOutsideAwardAsync(null, recordId);
            }

            [TestMethod]
            public async Task DeleteOutsideAwardAsync_ReturnsExpectedResponseTest()
            {
                var actualResponse = await outsideAwardsController.DeleteOutsideAwardAsync(studentId, recordId);
                Assert.AreEqual(HttpStatusCode.OK, actualResponse.StatusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentNullExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.DeleteOutsideAwardAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentNullException("ex"));
                try
                {
                    await outsideAwardsController.DeleteOutsideAwardAsync(studentId, recordId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.DeleteOutsideAwardAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new PermissionsException("ex"));
                try
                {
                    await outsideAwardsController.DeleteOutsideAwardAsync(studentId, recordId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.DeleteOutsideAwardAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new KeyNotFoundException("ex"));
                try
                {
                    await outsideAwardsController.DeleteOutsideAwardAsync(studentId, recordId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ApplicationExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.DeleteOutsideAwardAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ApplicationException("ex"));
                try
                {
                    await outsideAwardsController.DeleteOutsideAwardAsync(studentId, recordId);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.DeleteOutsideAwardAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception("ex"));
                try
                {
                    await outsideAwardsController.DeleteOutsideAwardAsync(studentId, recordId);
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
        public class UpdateOutsideAwardAsyncTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IOutsideAwardsService> outsideAwardsServiceMock;

            private OutsideAward expectedOutsideAward;
            private OutsideAward actualOutsideAward;

            private OutsideAwardsController outsideAwardsController;
            private HttpResponse httpResponse;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                outsideAwardsServiceMock = new Mock<IOutsideAwardsService>();

                expectedOutsideAward = new OutsideAward()
                {
                    Id = "1",
                    StudentId = "0003914",
                    AwardYearCode = "2016",
                    AwardName = "AwardName",
                    AwardType = "Loan",
                    AwardAmount = 4678,
                    AwardFundingSource = "Self"
                };

                outsideAwardsServiceMock.Setup(s => s.UpdateOutsideAwardAsync(It.IsAny<OutsideAward>())).ReturnsAsync(expectedOutsideAward);

                //this sets up the route for location link resolution
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/outside-awards/");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "UpdateOutsideAward",
                    routeTemplate: "api/outside-awards",
                    defaults: new { controller = "OutsideAwards", action = "UpdateOutsideAwardAsync" }
                );
                var routeData = new HttpRouteData(getRoute);
                var context = new HttpControllerContext(config, routeData, request);

                httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                outsideAwardsController = new OutsideAwardsController(adapterRegistryMock.Object, outsideAwardsServiceMock.Object, loggerMock.Object);

                outsideAwardsController.ControllerContext = new HttpControllerContext(config, routeData, request);
                outsideAwardsController.Request = request;
                outsideAwardsController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                outsideAwardsController.Url = new UrlHelper(request);

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            }
           
            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                outsideAwardsServiceMock = null;
                outsideAwardsController = null;
                httpResponse = null;
                expectedOutsideAward = null;
                actualOutsideAward = null;
            }
            
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullOutsideAwardPassed_HttpResponseExceptionThrownTest()
            {
                await outsideAwardsController.UpdateOutsideAwardAsync(null);
            }

            [TestMethod]
            public async Task ActualEqualsExpectedTest()
            {
                var response = await outsideAwardsController.UpdateOutsideAwardAsync(expectedOutsideAward);
                actualOutsideAward = JsonConvert.DeserializeObject<OutsideAward>(await response.Content.ReadAsStringAsync());

                Assert.AreEqual(expectedOutsideAward.Id, actualOutsideAward.Id);
                Assert.AreEqual(expectedOutsideAward.StudentId, actualOutsideAward.StudentId);
                Assert.AreEqual(expectedOutsideAward.AwardName, actualOutsideAward.AwardName);
                Assert.AreEqual(expectedOutsideAward.AwardType, actualOutsideAward.AwardType);
                Assert.AreEqual(expectedOutsideAward.AwardYearCode, actualOutsideAward.AwardYearCode);
                Assert.AreEqual(expectedOutsideAward.AwardAmount, actualOutsideAward.AwardAmount);
                Assert.AreEqual(expectedOutsideAward.AwardFundingSource, actualOutsideAward.AwardFundingSource);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentNullExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.UpdateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new ArgumentNullException("ex"));
                try
                {
                    await outsideAwardsController.UpdateOutsideAwardAsync(expectedOutsideAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.UpdateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new PermissionsException("ex"));
                try
                {
                    await outsideAwardsController.UpdateOutsideAwardAsync(expectedOutsideAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.UpdateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new KeyNotFoundException("ex"));
                try
                {
                    await outsideAwardsController.UpdateOutsideAwardAsync(expectedOutsideAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ApplicationExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.UpdateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new ApplicationException("ex"));
                try
                {
                    await outsideAwardsController.UpdateOutsideAwardAsync(expectedOutsideAward);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
            
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionCaughtTest()
            {
                outsideAwardsServiceMock.Setup(s => s.UpdateOutsideAwardAsync(It.IsAny<OutsideAward>()))
                    .Throws(new Exception("ex"));
                try
                {
                    await outsideAwardsController.UpdateOutsideAwardAsync(expectedOutsideAward);
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
