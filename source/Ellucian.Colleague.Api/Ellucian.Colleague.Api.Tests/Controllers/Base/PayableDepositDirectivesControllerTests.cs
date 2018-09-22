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
using Ellucian.Colleague.Api.Controllers.Base;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class PayableDepositDirectivesControllerTests
    {

        public const string stepUpAuthenticationHeaderKey = "X-Step-Up-Authentication";
        public string securityToken;

        public Mock<ILogger> loggerMock;
        public Mock<IPayableDepositDirectiveService> serviceMock;

        public PayableDepositDirectivesController controllerUnderTest;

        public PayableDepositDirective payableDepositDirective;
        public BankingAuthenticationToken bankingAuthenticationToken;

        public void PayableDepositDirectivesControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            serviceMock = new Mock<IPayableDepositDirectiveService>();

            controllerUnderTest = new PayableDepositDirectivesController(loggerMock.Object, serviceMock.Object);

            payableDepositDirective = new PayableDepositDirective()
            {
                Id = "500",
            };

            bankingAuthenticationToken = new BankingAuthenticationToken()
            {
                ExpirationDateTimeOffset = new DateTimeOffset(new DateTime(2017, 3, 27, 11, 17, 0), TimeSpan.FromHours(-4)),
                Token = Guid.NewGuid(),

            };

            securityToken = bankingAuthenticationToken.Token.ToString();
        }

        [TestClass]
        public class GetAllPayableDepositDirectivesTests : PayableDepositDirectivesControllerTests
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

                PayableDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.GetPayableDepositDirectivesAsync())
                    .Returns(() => Task.FromResult<IEnumerable<PayableDepositDirective>>(new List<PayableDepositDirective>() {
                        payableDepositDirective
                    }));
            }

            [TestMethod]
            public async Task GetAllTest()
            {
                var actual = await controllerUnderTest.GetPayableDepositDirectivesAsync();

                Assert.AreEqual(1, actual.Count());
                Assert.AreEqual(payableDepositDirective.Id, actual.First().Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OtherErrorTest()
            {
                serviceMock.Setup(s => s.GetPayableDepositDirectivesAsync())
                    .Throws(new Exception());

                try
                {
                    await controllerUnderTest.GetPayableDepositDirectivesAsync();
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }


        }

        [TestClass]
        public class GetSinglePayableDepositDirectivesTests : PayableDepositDirectivesControllerTests
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

                PayableDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.GetPayableDepositDirectiveAsync(It.IsAny<string>()))
                    .Returns<string>((id) => Task.FromResult<PayableDepositDirective>(payableDepositDirective));
            }

            [TestMethod]
            public async Task GetSingleTest()
            {
                var actual = await controllerUnderTest.GetPayableDepositDirectiveAsync(payableDepositDirective.Id);

                serviceMock.Verify(s => s.GetPayableDepositDirectiveAsync(It.Is<string>(id => id.Equals(payableDepositDirective.Id))));

                Assert.AreEqual(payableDepositDirective.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InputIdRequiredTest()
            {
                await controllerUnderTest.GetPayableDepositDirectiveAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionErrorTest()
            {
                serviceMock.Setup(s => s.GetPayableDepositDirectiveAsync(It.IsAny<string>()))
                    .Throws(new PermissionsException());

                try
                {
                    await controllerUnderTest.GetPayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NotFoundErrorTest()
            {
                serviceMock.Setup(s => s.GetPayableDepositDirectiveAsync(It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());

                try
                {
                    await controllerUnderTest.GetPayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OtherErrorTest()
            {
                serviceMock.Setup(s => s.GetPayableDepositDirectiveAsync(It.IsAny<string>()))
                    .Throws(new Exception());

                try
                {
                    await controllerUnderTest.GetPayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class UpdatePayableDepositDirectiveTests : PayableDepositDirectivesControllerTests
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

                PayableDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.UpdatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Returns<string, PayableDepositDirective>((token, directive) => Task.FromResult(payableDepositDirective));

                var request = new HttpRequestMessage(HttpMethod.Post, "/payable-deposit-directives/id");
                request.Headers.Add(stepUpAuthenticationHeaderKey, securityToken);
                controllerUnderTest.Request = request;
            }

            [TestMethod]
            public async Task UpdateSingleTest()
            {
                var actual = await controllerUnderTest.UpdatePayableDepositDirectiveAsync(payableDepositDirective);

                serviceMock.Verify(s => s.UpdatePayableDepositDirectiveAsync(It.Is<string>(t => t.Equals(securityToken)), It.Is<PayableDepositDirective>(p => p.Id == payableDepositDirective.Id)));

                Assert.AreEqual(payableDepositDirective.Id, actual.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InputDirectiveRequiredTest()
            {
                await controllerUnderTest.UpdatePayableDepositDirectiveAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentErrorTest()
            {
                serviceMock.Setup(s => s.UpdatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new ArgumentException());
                try
                {
                    await controllerUnderTest.UpdatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionErrorTest()
            {
                serviceMock.Setup(s => s.UpdatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new PermissionsException());
                try
                {
                    await controllerUnderTest.UpdatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NotFoundTest()
            {
                serviceMock.Setup(s => s.UpdatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new KeyNotFoundException());
                try
                {
                    await controllerUnderTest.UpdatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecordLockTest()
            {
                serviceMock.Setup(s => s.UpdatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new RecordLockException());
                try
                {
                    await controllerUnderTest.UpdatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Conflict, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OtherErrorTest()
            {
                serviceMock.Setup(s => s.UpdatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new Exception());
                try
                {
                    await controllerUnderTest.UpdatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class CreatePayableDepositDirectiveTests : PayableDepositDirectivesControllerTests
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

                var httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                PayableDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.CreatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Returns<string, PayableDepositDirective>((token, directive) => Task.FromResult(payableDepositDirective));

                var config = new HttpConfiguration();
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetPayableDepositDirective",
                    routeTemplate: "payable-deposit-directives/{id}",
                    defaults: new { controller = "PayableDepositDirectives", action = "GetPayableDepositDirective" }
                );
                var routeData = new HttpRouteData(getRoute);


                var request = new HttpRequestMessage(HttpMethod.Post, "http://api/payable-deposit-directives");
                request.SetConfiguration(new HttpConfiguration());
                request.Headers.Add(stepUpAuthenticationHeaderKey, securityToken);
                controllerUnderTest.Request = request;
                controllerUnderTest.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                controllerUnderTest.ControllerContext = new HttpControllerContext(config, routeData, request);
                controllerUnderTest.Url = new UrlHelper(request);
            }

            [TestMethod]
            public async Task CreateTest()
            {
                var actual = await controllerUnderTest.CreatePayableDepositDirectiveAsync(payableDepositDirective);

                Assert.AreEqual(HttpStatusCode.Created, actual.StatusCode);
                Assert.AreEqual(payableDepositDirective, await actual.Content.ReadAsAsync<PayableDepositDirective>());
                serviceMock.Verify(s => s.CreatePayableDepositDirectiveAsync(It.Is<string>(t => t.Equals(securityToken)), It.IsAny<PayableDepositDirective>()));

            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InputDirectiveRequiredTest()
            {
                await controllerUnderTest.CreatePayableDepositDirectiveAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BadArgumentsTest()
            {
                serviceMock.Setup(s => s.CreatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new ArgumentException());

                try
                {
                    await controllerUnderTest.CreatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsErrorTest()
            {
                serviceMock.Setup(s => s.CreatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new PermissionsException());

                try
                {
                    await controllerUnderTest.CreatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ApplicationErrorTest()
            {
                serviceMock.Setup(s => s.CreatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new ApplicationException());

                try
                {
                    await controllerUnderTest.CreatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OtherErrorTest()
            {
                serviceMock.Setup(s => s.CreatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<PayableDepositDirective>()))
                    .Throws(new Exception());

                try
                {
                    await controllerUnderTest.CreatePayableDepositDirectiveAsync(payableDepositDirective);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class DeletePayableDepositDirectiveTests : PayableDepositDirectivesControllerTests
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

                PayableDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.DeletePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult<int>(1));

                var request = new HttpRequestMessage(HttpMethod.Delete, "http://api/payable-deposit-directives/id");
                request.SetConfiguration(new HttpConfiguration());
                request.Headers.Add(stepUpAuthenticationHeaderKey, securityToken);
                controllerUnderTest.Request = request;
                controllerUnderTest.Url = new UrlHelper(request);
            }

            [TestMethod]
            public async Task DeleteTest()
            {
                var actual = await controllerUnderTest.DeletePayableDepositDirectiveAsync(payableDepositDirective.Id);

                Assert.AreEqual(HttpStatusCode.NoContent, actual.StatusCode);
                serviceMock.Verify(s => s.DeletePayableDepositDirectiveAsync(It.Is<string>(t => t.Equals(securityToken)), It.Is<string>(id => id.Equals(payableDepositDirective.Id))));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task DirectiveIdRequiredTest()
            {
                await controllerUnderTest.DeletePayableDepositDirectiveAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BadArgumentsTest()
            {
                serviceMock.Setup(s => s.DeletePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentException());
                try
                {
                    await controllerUnderTest.DeletePayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsTest()
            {
                serviceMock.Setup(s => s.DeletePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new PermissionsException());
                try
                {
                    await controllerUnderTest.DeletePayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NotFoundTest()
            {
                serviceMock.Setup(s => s.DeletePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());
                try
                {
                    await controllerUnderTest.DeletePayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecordLockTest()
            {
                serviceMock.Setup(s => s.DeletePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new RecordLockException());
                try
                {
                    await controllerUnderTest.DeletePayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Conflict, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ApplicationErrorTest()
            {
                serviceMock.Setup(s => s.DeletePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ApplicationException());
                try
                {
                    await controllerUnderTest.DeletePayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task OtherErrorTest()
            {
                serviceMock.Setup(s => s.DeletePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());
                try
                {
                    await controllerUnderTest.DeletePayableDepositDirectiveAsync(payableDepositDirective.Id);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }
        }


        [TestClass]
        public class AuthenticatePayableDepositDirectiveWithIdTests : PayableDepositDirectivesControllerTests
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


            public PayableDepositDirectiveAuthenticationChallenge inputChallenge;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                PayableDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string, string>((id, accountId, addressId) => Task.FromResult(bankingAuthenticationToken));

                inputChallenge = new PayableDepositDirectiveAuthenticationChallenge()
                {
                    PayableDepositDirectiveId = payableDepositDirective.Id,
                    ChallengeValue = "123456789",
                    AddressId = "foo",
                };

            }

            [TestMethod]
            public async Task AuthenticateTest()
            {
                var actual = await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);

                serviceMock.Verify(s => s.AuthenticatePayableDepositDirectiveAsync(It.Is<string>(id => id.Equals(inputChallenge.PayableDepositDirectiveId)), It.Is<string>(v => v.Equals(inputChallenge.ChallengeValue)), It.Is<string>(a => a.Equals(inputChallenge.AddressId))));

                Assert.AreEqual(bankingAuthenticationToken.Token, actual.Token);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InputChallengeRequiredTest()
            {
                inputChallenge = null;
                await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NotFoundTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new KeyNotFoundException());

                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BadRequestTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());

                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentNullTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentNullException());

                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new PermissionsException());

                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AuthenticationFailedTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new BankingAuthenticationException("foo", "bar"));
                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class AuthenticatePayableDepositDirectiveWithoutIdTests : PayableDepositDirectivesControllerTests
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

            public PayableDepositDirectiveAuthenticationChallenge inputChallenge;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                PayableDepositDirectivesControllerTestsInitialize();

                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string, string>((id, accountId, addressId) => Task.FromResult(bankingAuthenticationToken));

                inputChallenge = new PayableDepositDirectiveAuthenticationChallenge();
            }

            [TestMethod]
            public async Task AuthenticateTest()
            {
                var actual = await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);

                serviceMock.Verify(s => s.AuthenticatePayableDepositDirectiveAsync(It.Is<string>(id => string.IsNullOrEmpty(id)), It.Is<string>(v => string.IsNullOrEmpty(v)), It.Is<string>(a => string.IsNullOrEmpty(a))));

                Assert.AreEqual(bankingAuthenticationToken.Token, actual.Token);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task BadRequestTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());

                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task ArgumentNullTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentNullException());

                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new PermissionsException());

                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AuthenticationFailedTest()
            {
                serviceMock.Setup(s => s.AuthenticatePayableDepositDirectiveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new BankingAuthenticationException("foo", "bar"));
                try
                {
                    await controllerUnderTest.AuthenticatePayableDepositDirectiveAsync(inputChallenge);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}
