// Copyright 2016-2019 Ellucian Company L.P. and its affiliates.
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Dmi.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class SessionControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<ISessionRepository> sessionRepositoryMock;
        public Mock<ISessionRecoveryService> sessionRecoveryServiceMock;
        public SessionController controllerUnderTest;
        public Credentials loginCredentials;
        public ProxyCredentials proxyCredentials;

        public void SessionControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            sessionRepositoryMock = new Mock<ISessionRepository>();
            sessionRecoveryServiceMock = new Mock<ISessionRecoveryService>();
            loginCredentials = new Credentials() { UserId = "abc", Password = "def" };
            proxyCredentials = new ProxyCredentials() { UserId = "abc", ProxyId = "123", ProxyPassword = "def" };
            string loginResponse = "Login was successful";

            sessionRepositoryMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(loginResponse);

            controllerUnderTest = new SessionController(loggerMock.Object, sessionRepositoryMock.Object, sessionRecoveryServiceMock.Object);
            controllerUnderTest.Request = new HttpRequestMessage();
            controllerUnderTest.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            controllerUnderTest.Request.Headers.Add("X-ProductName", "WebApi");
            controllerUnderTest.Request.Headers.Add("X-ProductVersion", "1.12");
        }

        [TestClass]
        public class PostLoginAsyncTests : SessionControllerTests
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
                base.SessionControllerTestsInitialize();
            }

            [TestMethod]
            public async Task PostLoginAsyncSuccessful()
            {
                SessionControllerTestsInitialize();
                var response = await controllerUnderTest.PostLoginAsync(loginCredentials);
            }

            [TestMethod]
            public async Task PostLoginAsyncThrowsColleagueDmiConnectionException_ReturnsUnauthorized()
            {
                sessionRepositoryMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ColleagueDmiConnectionException("dmi error occurred"));

                var result = await controllerUnderTest.PostLoginAsync(loginCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Unauthorized);
            }

            [TestMethod]
            public async Task PostLoginAsyncThrowsLoginException_ReturnsUnauthorized()
            {
                sessionRepositoryMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new LoginException("10019", "login error occurred"));

                var result = await controllerUnderTest.PostLoginAsync(loginCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Unauthorized);
            }

            [TestMethod]
            public async Task PostLoginAsyncThrowsExpiredPasswordException_ReturnsForbidden()
            {
                sessionRepositoryMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new LoginException("10017", "password has expired"));

                var result = await controllerUnderTest.PostLoginAsync(loginCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Forbidden);
            }
        }

        [TestClass]
        public class PostProxyLoginAsyncTests : SessionControllerTests
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
                base.SessionControllerTestsInitialize();
            }

            [TestMethod]
            public async Task PostProxyLoginAsyncSuccessful()
            {
                SessionControllerTestsInitialize();
                var response = await controllerUnderTest.PostProxyLoginAsync(proxyCredentials);
            }

            [TestMethod]
            public async Task PostProxyLoginAsyncThrowsColleagueDmiConnectionException_ReturnsUnauthorized()
            {
                sessionRepositoryMock.Setup(s => s.ProxyLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ColleagueDmiConnectionException("dmi error occurred"));

                var result = await controllerUnderTest.PostProxyLoginAsync(proxyCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Unauthorized);
            }

            [TestMethod]
            public async Task PostProxyLoginAsyncThrowsLoginException_ReturnsUnauthorized()
            {
                sessionRepositoryMock.Setup(s => s.ProxyLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new LoginException("10019", "login error occurred"));

                var result = await controllerUnderTest.PostProxyLoginAsync(proxyCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Unauthorized);
            }

            [TestMethod]
            public async Task PostProxyLoginAsyncThrowsExpiredPasswordException_ReturnsForbidden()
            {
                sessionRepositoryMock.Setup(s => s.ProxyLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new LoginException("10017", "password has expired"));

                var result = await controllerUnderTest.PostProxyLoginAsync(proxyCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Forbidden);
            }
        }

        [TestClass]
        public class PostLogin2AsyncTests : SessionControllerTests
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
                base.SessionControllerTestsInitialize();
            }

            [TestMethod]
            public async Task PostLogin2AsyncSuccessful()
            {
                SessionControllerTestsInitialize();
                var response = await controllerUnderTest.PostLogin2Async(loginCredentials);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostLogin2AsyncThrowsColleagueDmiConnectionException_ReturnsNotFound()
            {
                sessionRepositoryMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ColleagueDmiConnectionException("dmi error occurred"));

                try
                {
                    var result = await controllerUnderTest.PostLogin2Async(loginCredentials);
                }
                catch (HttpResponseException ex)
                {
                    Assert.IsTrue(ex.Response.StatusCode == HttpStatusCode.NotFound);
                    throw;
                }
            }

            [TestMethod]
            public async Task PostLogin2AsyncThrowsLoginException_ReturnsUnauthorized()
            {
                sessionRepositoryMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new LoginException("10019", "login error occurred"));

                var result = await controllerUnderTest.PostLogin2Async(loginCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Unauthorized);
            }

            [TestMethod]
            public async Task PostLogin2AsyncThrowsExpiredPasswordException_ReturnsForbidden()
            {
                sessionRepositoryMock.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new LoginException("10017", "password has expired"));

                var result = await controllerUnderTest.PostLogin2Async(loginCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Forbidden);
            }
        }

        [TestClass]
        public class PostProxyLogin2AsyncTests : SessionControllerTests
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
                base.SessionControllerTestsInitialize();
            }

            [TestMethod]
            public async Task PostProxyLogin2AsyncSuccessful()
            {
                SessionControllerTestsInitialize();
                var response = await controllerUnderTest.PostProxyLogin2Async(proxyCredentials);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProxyLogin2AsyncThrowsColleagueDmiConnectionException_ReturnsNotFound()
            {
                sessionRepositoryMock.Setup(s => s.ProxyLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ColleagueDmiConnectionException("dmi error occurred"));

                try
                {
                    var result = await controllerUnderTest.PostProxyLogin2Async(proxyCredentials);
                }
                catch (HttpResponseException ex)
                {
                    Assert.IsTrue(ex.Response.StatusCode == HttpStatusCode.NotFound);
                    throw;
                }
            }

            [TestMethod]
            public async Task PostProxyLogin2AsyncThrowsLoginException_ReturnsUnauthorized()
            {
                sessionRepositoryMock.Setup(s => s.ProxyLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new LoginException("10019", "login error occurred"));

                var result = await controllerUnderTest.PostProxyLogin2Async(proxyCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Unauthorized);
            }

            [TestMethod]
            public async Task PostProxyLogin2AsyncThrowsExpiredPasswordException_ReturnsForbidden()
            {
                sessionRepositoryMock.Setup(s => s.ProxyLoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new LoginException("10017", "password has expired"));

                var result = await controllerUnderTest.PostProxyLogin2Async(proxyCredentials);
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Forbidden);
            }

        }

        [TestClass]
        public class PasswordResetTests : SessionControllerTests
        {
            private TestContext testContextInstance;

            public TestContext TestContext
            {
                get { return testContextInstance; }
                set { testContextInstance = value; }
            }


            [TestInitialize]
            public void Initialize()
            {

                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                base.SessionControllerTestsInitialize();
            }

            [TestMethod]
            public async Task PostResetPasswordTokenRequestAsync_SuccessfulServiceCall_ReturnsAccepted()
            {
                sessionRecoveryServiceMock.Setup(srs => srs.RequestPasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.Delay(0));
                var result = await controllerUnderTest.PostResetPasswordTokenRequestAsync(new PasswordResetTokenRequest() { EmailAddress = "test@example.com", UserId = "testUserId" });
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            }

            [TestMethod]
            public async Task PostResetPasswordTokenRequestAsync_ServiceException_ReturnsAccepted()
            {
                sessionRecoveryServiceMock.Setup(srs => srs.RequestPasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new System.Exception("Test Service Exception"));
                var result = await controllerUnderTest.PostResetPasswordTokenRequestAsync(new PasswordResetTokenRequest() { EmailAddress = "test@example.com", UserId = "testUserId" });
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            }

            [TestMethod]
            public async Task PostUserIdRecoveryRequestAsync_ReturnsAccepted()
            {
                sessionRecoveryServiceMock.Setup(srs => srs.RequestUserIdRecoveryAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.Delay(0));
                var result = await controllerUnderTest.PostUserIdRecoveryRequestAsync(new UserIdRecoveryRequest() { EmailAddress = "test@example.com", FirstName = "Test", LastName = "Example" });
                Assert.AreEqual(result.StatusCode, HttpStatusCode.Accepted);
            }
            [TestMethod]
            public async Task PostResetPasswordAsync_ReturnsOK()
            {
                sessionRecoveryServiceMock.Setup(srs => srs.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.Delay(0));
                var result = await controllerUnderTest.PostResetPasswordAsync(new ResetPassword() { NewPassword = "testNewPassword", ResetToken = "testToken", UserId = "testUser" });
                Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            }
        }
    }
}
