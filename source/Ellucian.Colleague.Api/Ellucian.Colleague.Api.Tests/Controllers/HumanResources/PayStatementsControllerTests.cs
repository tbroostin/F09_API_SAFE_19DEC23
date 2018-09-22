/* Copyright 2017-2018 Ellucian Company L.P. and its affiliates. */
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers.HumanResources;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.HumanResources.Services;
using Ellucian.Colleague.Dtos.HumanResources;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Api.Tests.Controllers.HumanResources
{
    [TestClass]
    public class PayStatementsControllerTests
    {
        public Mock<ILogger> loggerMock;
        public Mock<IPayStatementService> payStatementServiceMock;

        public PayStatementsController controllerUnderTest;

        public void PayStatementsControllerTestsInitialize()
        {
            loggerMock = new Mock<ILogger>();
            payStatementServiceMock = new Mock<IPayStatementService>();

            payStatementServiceMock.Setup(s => s.GetPayStatementPdf(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string, string>((id, path, logoPath) => Task.FromResult(new Tuple<string, byte[]>("byte",new byte[1] { byte.Parse("1") })));

            controllerUnderTest = new PayStatementsController(loggerMock.Object, payStatementServiceMock.Object, new ApiSettings("test"));

            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
                );
        }

        [TestClass]
        public class QueryMultiplePayStatementPdfTests : PayStatementsControllerTests
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

            public List<string> inputIds;

            public async Task<HttpResponseMessage> getActual()
            {
                return await controllerUnderTest.QueryPayStatementPdfs(inputIds);
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                PayStatementsControllerTestsInitialize();

                inputIds = new List<string>() { "foo", "bar" };
            }

            [TestMethod]
            public async Task ContentContainsByteArrayTest()
            {
                var actual = await getActual();

                Assert.IsInstanceOfType(actual.Content, typeof(ByteArrayContent));
            }

            [TestMethod]
            public async Task ContentContainsMediaTypeHeaderTest()
            {
                var actual = await getActual();

                Assert.AreEqual("application/pdf", actual.Content.Headers.ContentType.MediaType);
            }

            [TestMethod]
            public async Task ContentContainsDispositionHeaderTest()
            {
                var actual = await getActual();

                Assert.AreEqual("attachment", actual.Content.Headers.ContentDisposition.DispositionType);
            }

            [TestMethod]
            public async Task ContentContainsContentLengthHeaderTest()
            {
                var actual = await getActual();

                var bytes = await actual.Content.ReadAsByteArrayAsync();

                Assert.AreEqual(bytes.LongLength, actual.Content.Headers.ContentLength);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdsRequiredTest()
            {
                inputIds = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdsRequiredValuesTest()
            {
                inputIds = new List<string>();
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchArgumentNullExceptionTest()
            {

                payStatementServiceMock.Setup(s => s.GetPayStatementPdf(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentNullException());

                try
                {
                    await getActual();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchPermissionsExceptionTest()
            {

                payStatementServiceMock.Setup(s => s.GetPayStatementPdf(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new PermissionsException());

                try
                {
                    await getActual();
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
            public async Task CatchGenericExceptionTest()
            {
                payStatementServiceMock.Setup(s => s.GetPayStatementPdf(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());

                try
                {
                    await getActual();
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
        public class GetPayStatementPdfTests : PayStatementsControllerTests
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

            public string inputId;

            public async Task<HttpResponseMessage> getActual()
            {
                return await controllerUnderTest.GetPayStatementPdf(inputId);
            }

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                PayStatementsControllerTestsInitialize();

                inputId = "foobar";
            }

            [TestMethod]
            public async Task ContentContainsByteArrayTest()
            {
                var actual = await getActual();

                Assert.IsInstanceOfType(actual.Content, typeof(ByteArrayContent));
            }

            [TestMethod]
            public async Task ContentContainsMediaTypeHeaderTest()
            {
                var actual = await getActual();

                Assert.AreEqual("application/pdf", actual.Content.Headers.ContentType.MediaType);
            }

            [TestMethod]
            public async Task ContentContainsDispositionHeaderTest()
            {
                var actual = await getActual();

                Assert.AreEqual("attachment", actual.Content.Headers.ContentDisposition.DispositionType);
            }

            [TestMethod]
            public async Task ContentContainsContentLengthHeaderTest()
            {
                var actual = await getActual();

                var bytes = await actual.Content.ReadAsByteArrayAsync();

                Assert.AreEqual(bytes.LongLength, actual.Content.Headers.ContentLength);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task IdRequiredTest()
            {
                inputId = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchArgumentNullExceptionTest()
            {

                payStatementServiceMock.Setup(s => s.GetPayStatementPdf(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new ArgumentNullException());

                try
                {
                    await getActual();
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                payStatementServiceMock.Setup(s => s.GetPayStatementPdf(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Throws(new Exception());

                try
                {
                    await getActual();
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
        public class GetPayStatementSummariesTests : PayStatementsControllerTests
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

            public FunctionEqualityComparer<PayStatementSummary> summaryEqualityComparer;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                PayStatementsControllerTestsInitialize();
                summaryEqualityComparer = new FunctionEqualityComparer<PayStatementSummary>(
                    (a, b) =>
                        a.Id == b.Id &&
                        a.PayDate == b.PayDate,
                    (s) => s.Id.GetHashCode()
                );
            }

            //[TestMethod]
            //public async Task ListOfPayStatementSummariesIsReturnedAsExpected()
            //{
            //    var testData = new Collection<PayStatementSummary>() {
            //        new PayStatementSummary()
            //        {
            //            Id = "01",
            //            PayDate = DateTime.Now
            //        },
            //        new PayStatementSummary()
            //        {
            //            Id = "02",
            //            PayDate = DateTime.Now
            //        }
            //    };
            //    payStatementServiceMock.Setup(s => s.GetPayStatementSummariesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            //        .Returns(async () => await Task.FromResult(
            //                testData
            //        )
            //    );
            //    var actual = await controllerUnderTest.GetPayStatementSummariesAsync("employeeId", true, DateTime.Now, "PC");
            //    CollectionAssert.AreEqual(testData.ToList(), actual.ToList(), summaryEqualityComparer);
            //}

            //[TestMethod,ExpectedException(typeof(HttpResponseException))]
            //public async Task ArgumentNullExceptionIsCaught()
            //{
            //    payStatementServiceMock.Setup(s => s.GetPayStatementSummariesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).Throws(new ArgumentNullException("only a little bit null"));

            //    try
            //    {
            //        await controllerUnderTest.GetPayStatementSummariesAsync("employeeId", true, DateTime.Now, "PC");
            //    }
            //    catch (HttpResponseException hre)
            //    {
            //        loggerMock.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), It.IsAny<string>()));
            //        Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
            //        throw;
            //    }                
            //}

            //[TestMethod, ExpectedException(typeof(HttpResponseException))]
            //public async Task ExceptionIsCaught()
            //{
            //    payStatementServiceMock.Setup(s => s.GetPayStatementSummariesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), It.IsAny<DateTime?>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).Throws(new Exception("not that new"));

            //    try
            //    {
            //        await controllerUnderTest.GetPayStatementSummariesAsync("employeeId", true, DateTime.Now, "PC");
            //    }
            //    catch (HttpResponseException hre)
            //    {
            //        loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
            //        Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
            //        throw;
            //    }
            //}
        }
    }
}
