// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PayrollDepositDirectivesControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IPayrollDepositDirectiveService> serviceMock;

        public PayrollDepositDirectivesController controllerUnderTest;

        public void PayrollDepositDirectivesControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            serviceMock = new Mock<IPayrollDepositDirectiveService>();

            controllerUnderTest = new PayrollDepositDirectivesController(loggerMock.Object, serviceMock.Object);
        }


        [TestClass]
        public class GetAllPayrollDepositDirectivesTests : PayrollDepositDirectivesControllerTests
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

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            
                PayrollDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.GetPayrollDepositDirectivesAsync()).Returns(() => Task.FromResult<IEnumerable<PayrollDepositDirective>>(new List<PayrollDepositDirective>() {
                    new PayrollDepositDirective()
                    {
                        Id = "foo",
                    },
                    new PayrollDepositDirective()
                    {
                        Id = "bar"
                    }
                }));
            }

            [TestMethod]
            public async Task Test()
            {
                var result = await controllerUnderTest.GetPayrollDepositDirectivesAsync();
                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class GetOnePayrollDepositDirectiveTests : PayrollDepositDirectivesControllerTests
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

            [TestInitialize]
            public void Initialize()
            {
                var testData = new List<PayrollDepositDirective>() {
                        new PayrollDepositDirective()
                        {
                            Id = "foo",
                        },
                        new PayrollDepositDirective()
                        {
                            Id = "bar"
                        }
                    };

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                PayrollDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.GetPayrollDepositDirectiveAsync(It.IsAny<string>()))
                    .Returns<string>((id) => Task.FromResult(testData.FirstOrDefault(dir => dir.Id == id)));
            }

            [TestMethod]
            public async Task Test()
            {
                var id = "bar";
                var result = await controllerUnderTest.GetPayrollDepositDirectiveAsync(id);
                Assert.AreEqual(id,result.Id);
            }
        }

        [TestClass]
        public class UpdateAllPayrollDepositDirectivesTests : PayrollDepositDirectivesControllerTests
        {
            List<PayrollDepositDirective> testData = new List<PayrollDepositDirective>() {
                    new PayrollDepositDirective()
                    {
                        Id = "foo",
                    },
                    new PayrollDepositDirective()
                    {
                        Id = "bar"
                    }
                };

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

                PayrollDepositDirectivesControllerTestsInitialize();

                var request = new HttpRequestMessage(HttpMethod.Post, "/payroll-deposit-directives");
                request.Headers.Add("X-Step-Up-Authentication", new Guid().ToString());
                controllerUnderTest.Request = request;

                serviceMock.Setup(s => s.UpdatePayrollDepositDirectivesAsync(It.IsAny<string>(),It.IsAny<IEnumerable<PayrollDepositDirective>>()))
                    .Returns<string,IEnumerable<PayrollDepositDirective>>((token, data) => Task.FromResult(data));
            }

            [TestMethod]
            public async Task Test()
            {
                var result = await controllerUnderTest.UpdatePayrollDepositDirectivesAsync(testData);
                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class UpdateOnePayrollDepositDirectiveTests : PayrollDepositDirectivesControllerTests
        {
            PayrollDepositDirective testData = new PayrollDepositDirective()
            {
                Id = "hexedall",
            };

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

                PayrollDepositDirectivesControllerTestsInitialize();

                var request = new HttpRequestMessage(HttpMethod.Post, "/payroll-deposit-directives/id");
                request.Headers.Add("X-Step-Up-Authentication", new Guid().ToString());
                controllerUnderTest.Request = request;

                serviceMock.Setup(s => s.UpdatePayrollDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayrollDepositDirective>()))
                    .Returns<string, PayrollDepositDirective>((token, data) => Task.FromResult(data));
            }

            [TestMethod]
            public async Task Test()
            {
                var result = await controllerUnderTest.UpdatePayrollDepositDirectiveAsync(testData.Id,testData);
                Assert.IsNotNull(result);
            }
        }

        [TestClass]
        public class CreateOnePayrollDepositDirectiveTests : PayrollDepositDirectivesControllerTests
        {
            PayrollDepositDirective testData = new PayrollDepositDirective()
            {
                Id = "hexedall",
            };

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

                PayrollDepositDirectivesControllerTestsInitialize();

                var httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);
                var config = new HttpConfiguration();
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetPayrollDepositDirectiveAsync",
                    routeTemplate: "payroll-deposit-directives/{id}",
                    defaults: new { controller = "PayrollDepositDirectives", action = "GetPayrollDepositDirectiveAsync" }
                );
                var routeData = new HttpRouteData(getRoute);
                var request = new HttpRequestMessage(HttpMethod.Post, "http://api/payroll-deposit-directives");
                request.SetConfiguration(new HttpConfiguration());
                request.Headers.Add("X-Step-Up-Authentication", new Guid().ToString());
                controllerUnderTest.Request = request;
                controllerUnderTest.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                controllerUnderTest.ControllerContext = new HttpControllerContext(config, routeData, request);
                controllerUnderTest.Url = new UrlHelper(request);
                serviceMock.Setup(s => s.CreatePayrollDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayrollDepositDirective>()))
                    .Returns<string, PayrollDepositDirective>((token, data) => Task.FromResult(data));

            }

            //[TestMethod]
            //public async Task Test()
            //{
            //    var result = await controllerUnderTest.CreatePayrollDepositDirectiveAsync(testData);
            //    Assert.IsNotNull(result);
            //    Assert.IsInstanceOfType(result, typeof(HttpResponseMessage));
            //    Assert.AreEqual(result.StatusCode, HttpStatusCode.Created);
            //    Assert.IsNotNull(result.Content);                
            //}
        }

        [TestClass]
        public class DeleteOnePayrollDepositDirectiveTests : PayrollDepositDirectivesControllerTests
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

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                PayrollDepositDirectivesControllerTestsInitialize();

                var request = new HttpRequestMessage(HttpMethod.Post, "/payroll-deposit-directives");
                request.Headers.Add("X-Step-Up-Authentication", new Guid().ToString());
                controllerUnderTest.Request = request;


                serviceMock.Setup(s => s.DeletePayrollDepositDirectiveAsync(It.IsAny<string>(),It.IsAny<string>()))
                    .Returns<string,string>((token, id) => Task.FromResult(true));
            }

            [TestMethod]
            public async Task Test()
            {
                var result = await controllerUnderTest.DeletePayrollDepositDirectiveAsync("janedoe");
                Assert.AreEqual(result.StatusCode,HttpStatusCode.NoContent);
            }
        }

        [TestClass]
        public class PostPayrollDepositDirectiveAuthorizationTests: PayrollDepositDirectivesControllerTests
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

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                PayrollDepositDirectivesControllerTestsInitialize();      
          
                serviceMock.Setup(s => s.AuthenticateCurrentUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync(new BankingAuthenticationToken() 
                    {
                        ExpirationDateTimeOffset = new DateTimeOffset(new DateTime(2017, 1, 21, 11, 40, 0), TimeSpan.FromHours(-4)),
                        Token = Guid.NewGuid()
                    });
            }

            [TestMethod]
            public async Task Test()
            {
                var result = await controllerUnderTest.PostPayrollDepositDirectiveAuthenticationAsync("foo", "bar");

                Assert.IsInstanceOfType(result, typeof(BankingAuthenticationToken));
            }
        }
    }
}
