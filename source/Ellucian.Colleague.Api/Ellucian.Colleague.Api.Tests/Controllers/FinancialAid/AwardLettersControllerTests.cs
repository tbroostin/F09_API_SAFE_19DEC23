//Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using System.Linq;
using Ellucian.Colleague.Api.Controllers.FinancialAid;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.FinancialAid.Services;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Ellucian.Colleague.Dtos.FinancialAid;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Configuration;
using System.Web;
using System.IO;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Net.Http;
using System.Web.Http.Hosting;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Net.Http.Headers;
using Ellucian.Web.Http.Routes;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{

    [TestClass]
    public class AwardLettersControllerTests
    {
        [TestClass]
        public class UpdateAwardLetterTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;
            private string awardYear;

            private AwardLetter inputAwardLetter;
            private AwardLetter expectedUpdatedAwardLetter;
            private AwardLetter actualUpdatedAwardLetter;

            private AwardLettersController AwardLettersController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");


                studentId = "0003914";
                awardYear = "2014";

                inputAwardLetter = new AwardLetter()
                {
                    AwardYearCode = awardYear,
                    StudentId = studentId,
                    AcceptedDate = DateTime.Today,
                };

                expectedUpdatedAwardLetter = new AwardLetter()
                {
                    AwardYearCode = awardYear,
                    StudentId = studentId,
                    AcceptedDate = DateTime.Today,
                };

                awardLetterServiceMock.Setup(l => l.UpdateAwardLetter(inputAwardLetter)).Returns(inputAwardLetter);

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);

                actualUpdatedAwardLetter = AwardLettersController.UpdateAwardLetter(studentId, awardYear, inputAwardLetter);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                awardLetterServiceMock = null;
                studentId = null;
                expectedUpdatedAwardLetter = null;
                inputAwardLetter = null;
                actualUpdatedAwardLetter = null;
                AwardLettersController = null;
            }

            [TestMethod]
            public void TypeTest()
            {
                Assert.AreEqual(expectedUpdatedAwardLetter.GetType(), actualUpdatedAwardLetter.GetType());
            }

            [TestMethod]
            public void EqualAttributesTest()
            {
                Assert.AreEqual(expectedUpdatedAwardLetter.StudentId, actualUpdatedAwardLetter.StudentId);
                Assert.AreEqual(expectedUpdatedAwardLetter.AwardYearCode, actualUpdatedAwardLetter.AwardYearCode);
                Assert.AreEqual(expectedUpdatedAwardLetter.AcceptedDate, actualUpdatedAwardLetter.AcceptedDate);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void StudentIdArgumentRequiredTest()
            {
                AwardLettersController.UpdateAwardLetter(null, awardYear, inputAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AwardYearArgumentRequiredTest()
            {
                AwardLettersController.UpdateAwardLetter(studentId, null, inputAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AwardLetterArgumentRequiredTest()
            {
                AwardLettersController.UpdateAwardLetter(studentId, awardYear, null);
            }

            [TestMethod]
            public void AwardYearArgumentMustMatchAwardLetterAwardYearTest()
            {
                awardYear = "foobar";
                var exceptionCaught = false;
                try
                {
                    AwardLettersController.UpdateAwardLetter(studentId, awardYear, inputAwardLetter);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(string.Format("AwardYear {0} in URI does not match AwardYear {1} of awardLetter in request body", awardYear, inputAwardLetter.AwardYearCode)));
            }

            [TestMethod]
            public void CatchArgumentExceptionTest()
            {
                awardLetterServiceMock.Setup(s => s.UpdateAwardLetter(inputAwardLetter)).Throws(new ArgumentException("Argument Exception message"));

                var exceptionCaught = false;
                try
                {
                    AwardLettersController.UpdateAwardLetter(studentId, awardYear, inputAwardLetter);
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
            public void CatchPermissionsExceptionTest()
            {
                awardLetterServiceMock.Setup(s => s.UpdateAwardLetter(inputAwardLetter)).Throws(new PermissionsException("Permission Exception message"));

                var exceptionCaught = false;
                try
                {
                    AwardLettersController.UpdateAwardLetter(studentId, awardYear, inputAwardLetter);
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
            public void CatchKeyNotFoundExceptionTest()
            {
                awardLetterServiceMock.Setup(s => s.UpdateAwardLetter(inputAwardLetter)).Throws(new KeyNotFoundException("Not Found Exception message"));

                var exceptionCaught = false;
                try
                {
                    AwardLettersController.UpdateAwardLetter(studentId, awardYear, inputAwardLetter);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public void CatchUnknownExceptionTest()
            {
                awardLetterServiceMock.Setup(s => s.UpdateAwardLetter(inputAwardLetter)).Throws(new Exception("Unknown Exception message"));

                var exceptionCaught = false;
                try
                {
                    AwardLettersController.UpdateAwardLetter(studentId, awardYear, inputAwardLetter);
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

        [TestClass]
        public class GetSingleAwardLetterReportTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;
            private string awardYear;

            private byte[] expectedAwardLetterByteArray;
            private AwardLetter expectedAwardLetter;

            private AwardLettersController AwardLettersController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0003914";
                awardYear = "2014";

                expectedAwardLetter = new AwardLetter()
                {
                    AcceptedDate = DateTime.Today,
                    AwardColumnHeader = "Award",
                    AwardPeriod1ColumnHeader = "Period1",
                    AwardPeriod2ColumnHeader = "Period2",
                    AwardPeriod3ColumnHeader = "Period3",
                    AwardPeriod4ColumnHeader = "Period4",
                    AwardPeriod5ColumnHeader = "Period5",
                    AwardPeriod6ColumnHeader = "Period6",
                    AwardTableRows = new List<AwardLetterAward>() { new AwardLetterAward() { AwardId = "FOO", AwardDescription = "BAR", Period1Amount = 1000 } },
                    AwardYearCode = awardYear,
                    BudgetAmount = 1000,
                    ClosingParagraph = "This is the closing paragraph",
                    ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    Date = DateTime.Today,
                    EstimatedFamilyContributionAmount = 500,
                    IsContactBlockActive = true,
                    IsNeedBlockActive = true,
                    NeedAmount = 500,
                    NumberAwardPeriodColumns = 2,
                    OpeningParagraph = "This is the opening paragraph",
                    StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    StudentId = studentId,
                    StudentName = "Preferred Name",
                    TotalColumnHeader = "Total"
                };

                awardLetterServiceMock.Setup(l => l.GetAwardLetters(studentId, awardYear)).Returns(expectedAwardLetter);

                var binaryFormatter = new BinaryFormatter();
                var memoryStream = new MemoryStream();
                binaryFormatter.Serialize(memoryStream, "This is the awardLetter report pdf");
                expectedAwardLetterByteArray = memoryStream.ToArray();

                //awardLetterServiceMock.Setup(l => l.GetAwardLetters(studentId, awardYear, It.IsAny<string>(), It.IsAny<string>())).Returns(expectedAwardLetterByteArray);
                awardLetterServiceMock.Setup(l => l.GetAwardLetters(It.IsAny<AwardLetter>(), It.IsAny<string>(), It.IsAny<string>())).Returns(expectedAwardLetterByteArray);

                //this sets up the route for PDF processing
                // var config = new HttpConfiguration();
                // var url = string.Format("http://localhost/api/students/{0}/award-letters/{1}", studentId, awardYear);
                // var request = new HttpRequestMessage(HttpMethod.Get, url);

                //var route = config.Routes.MapHttpRoute(
                //    name: "GetAwardLetterReport",
                //    routeTemplate: "api/students/{id}/award-letters/{awardYear}",
                //    defaults: new { controller = "AwardLetters", action = "GetAwardLetterReport" },
                //    constraints: new
                //    {
                //        httpMethod = new HttpMethodConstraint(HttpMethod.Get),
                //        headerVersion = new HeaderVersionConstraint(1, false, "application\vnd.ellucian.v1+pdf")
                //    }
                //);     request.Headers.Add("X-Ellucian-Media-Type", "application\vnd.ellucian.v1+pdf");

                // var routeData = new HttpRouteData(route);
                //var context = new HttpControllerContext(config, routeData, request);
                var httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);


                //AwardLettersController.ControllerContext = context;
                // AwardLettersController.Request = request;
                // AwardLettersController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;

            }

            [TestMethod]
            public void PdfReportAsByteArrayTest()
            {
                var response = AwardLettersController.GetAwardLetterReport(studentId, awardYear);
                var byteArray = response.Content.ReadAsByteArrayAsync().Result;

                CollectionAssert.AreEqual(expectedAwardLetterByteArray, byteArray);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void StudentIdRequiredTest()
            {
                try
                {
                    AwardLettersController.GetAwardLetterReport(null, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AwardYearRequiredTest()
            {

                try
                {
                    AwardLettersController.GetAwardLetterReport(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullAwardLetterDtoThrowsExceptionTest()
            {
                expectedAwardLetter = null;
                awardLetterServiceMock.Setup(l => l.GetAwardLetters(studentId, awardYear)).Returns(expectedAwardLetter);
                try
                {
                    AwardLettersController.GetAwardLetterReport(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public void EmptyReportLogoPathTest()
            {
                apiSettings.ReportLogoPath = string.Empty;
                var actualLogoPath = string.Empty;
                awardLetterServiceMock.Setup(l => l.GetAwardLetters(It.IsAny<AwardLetter>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(expectedAwardLetterByteArray)
                    .Callback<AwardLetter, string, string>((l, p, i) => actualLogoPath = i);

                var response = AwardLettersController.GetAwardLetter(studentId, awardYear);

                Assert.AreEqual(apiSettings.ReportLogoPath, actualLogoPath);
            }

            [TestMethod]
            public void PdfResponseContentHeadersTest()
            {

                var response = AwardLettersController.GetAwardLetterReport(studentId, awardYear);

                var expectedContentType = new MediaTypeHeaderValue("application/pdf");
                var expectedContentLength = expectedAwardLetterByteArray.Length;
                var expectedContentDisposition = new ContentDispositionHeaderValue("attachment");
                var expectedStartOfFileName = "AwardLetter_" + studentId + "_" + awardYear + "_";

                Assert.AreEqual(expectedContentType, response.Content.Headers.ContentType);
                Assert.AreEqual(expectedContentLength, response.Content.Headers.ContentLength);
                Assert.AreEqual(expectedContentDisposition.DispositionType, response.Content.Headers.ContentDisposition.DispositionType);
                Assert.AreEqual(expectedStartOfFileName, response.Content.Headers.ContentDisposition.FileName.Substring(0, expectedStartOfFileName.Length));
            }
        }

        [TestClass]
        public class GetSingleAwardLetterReport2Tests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;
            private string awardYear;

            private byte[] expectedAwardLetterByteArray;
            private AwardLetter expectedAwardLetter;

            private AwardLettersController AwardLettersController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0003914";
                awardYear = "2014";

                expectedAwardLetter = new AwardLetter()
                {
                    AcceptedDate = DateTime.Today,
                    AwardColumnHeader = "Award",
                    AwardPeriod1ColumnHeader = "Period1",
                    AwardPeriod2ColumnHeader = "Period2",
                    AwardPeriod3ColumnHeader = "Period3",
                    AwardPeriod4ColumnHeader = "Period4",
                    AwardPeriod5ColumnHeader = "Period5",
                    AwardPeriod6ColumnHeader = "Period6",
                    AwardTableRows = new List<AwardLetterAward>() { new AwardLetterAward() { AwardId = "FOO", AwardDescription = "BAR", Period1Amount = 1000 } },
                    AwardYearCode = awardYear,
                    BudgetAmount = 1000,
                    ClosingParagraph = "This is the closing paragraph",
                    ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    Date = DateTime.Today,
                    EstimatedFamilyContributionAmount = 500,
                    IsContactBlockActive = true,
                    IsNeedBlockActive = true,
                    NeedAmount = 500,
                    NumberAwardPeriodColumns = 2,
                    OpeningParagraph = "This is the opening paragraph",
                    StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    StudentId = studentId,
                    StudentName = "Preferred Name",
                    TotalColumnHeader = "Total"
                };

                awardLetterServiceMock.Setup(l => l.GetAwardLetters2(studentId, awardYear)).Returns(expectedAwardLetter);

                var binaryFormatter = new BinaryFormatter();
                var memoryStream = new MemoryStream();
                binaryFormatter.Serialize(memoryStream, "This is the awardLetter report pdf");
                expectedAwardLetterByteArray = memoryStream.ToArray();

                awardLetterServiceMock.Setup(l => l.GetAwardLetters(It.IsAny<AwardLetter>(), It.IsAny<string>(), It.IsAny<string>())).Returns(expectedAwardLetterByteArray);


                var httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);

            }

            [TestMethod]
            public void PdfReportAsByteArrayTest()
            {
                var response = AwardLettersController.GetAwardLetterReport2(studentId, awardYear);
                var byteArray = response.Content.ReadAsByteArrayAsync().Result;

                CollectionAssert.AreEqual(expectedAwardLetterByteArray, byteArray);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void StudentIdRequiredTest()
            {
                try
                {
                    AwardLettersController.GetAwardLetterReport2(null, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AwardYearRequiredTest()
            {

                try
                {
                    AwardLettersController.GetAwardLetterReport2(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void NullAwardLetterDtoThrowsExceptionTest()
            {
                expectedAwardLetter = null;
                awardLetterServiceMock.Setup(l => l.GetAwardLetters2(studentId, awardYear)).Returns(expectedAwardLetter);
                try
                {
                    AwardLettersController.GetAwardLetterReport2(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public void EmptyReportLogoPathTest()
            {
                apiSettings.ReportLogoPath = string.Empty;
                var actualLogoPath = string.Empty;
                awardLetterServiceMock.Setup(l => l.GetAwardLetters(It.IsAny<AwardLetter>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(expectedAwardLetterByteArray)
                    .Callback<AwardLetter, string, string>((l, p, i) => actualLogoPath = i);

                var response = AwardLettersController.GetAwardLetter2(studentId, awardYear);

                Assert.AreEqual(apiSettings.ReportLogoPath, actualLogoPath);
            }

            [TestMethod]
            public void PdfResponseContentHeadersTest()
            {

                var response = AwardLettersController.GetAwardLetterReport2(studentId, awardYear);

                var expectedContentType = new MediaTypeHeaderValue("application/pdf");
                var expectedContentLength = expectedAwardLetterByteArray.Length;
                var expectedContentDisposition = new ContentDispositionHeaderValue("attachment");
                var expectedStartOfFileName = "AwardLetter_" + studentId + "_" + awardYear + "_";

                Assert.AreEqual(expectedContentType, response.Content.Headers.ContentType);
                Assert.AreEqual(expectedContentLength, response.Content.Headers.ContentLength);
                Assert.AreEqual(expectedContentDisposition.DispositionType, response.Content.Headers.ContentDisposition.DispositionType);
                Assert.AreEqual(expectedStartOfFileName, response.Content.Headers.ContentDisposition.FileName.Substring(0, expectedStartOfFileName.Length));
            }
        }

        [TestClass]
        public class GetSingleAwardLetterTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;
            private string awardYear;

            private AwardLetter expectedAwardLetter;
            private AwardLetter actualAwardLetter;

            private AwardLettersController AwardLettersController;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0003914";
                awardYear = "2014";

                expectedAwardLetter = new AwardLetter()
                    {
                        AcceptedDate = DateTime.Today,
                        AwardColumnHeader = "Award",
                        AwardPeriod1ColumnHeader = "Period1",
                        AwardPeriod2ColumnHeader = "Period2",
                        AwardPeriod3ColumnHeader = "Period3",
                        AwardPeriod4ColumnHeader = "Period4",
                        AwardPeriod5ColumnHeader = "Period5",
                        AwardPeriod6ColumnHeader = "Period6",
                        AwardTableRows = new List<AwardLetterAward>() { new AwardLetterAward() { AwardId = "FOO", AwardDescription = "BAR", Period1Amount = 1000 } },
                        AwardYearCode = awardYear,
                        BudgetAmount = 1000,
                        ClosingParagraph = "This is the closing paragraph",
                        ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        Date = DateTime.Today,
                        EstimatedFamilyContributionAmount = 500,
                        IsContactBlockActive = true,
                        IsNeedBlockActive = true,
                        NeedAmount = 500,
                        NumberAwardPeriodColumns = 2,
                        OpeningParagraph = "This is the opening paragraph",
                        StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        StudentId = studentId,
                        StudentName = "Preferred Name",
                        TotalColumnHeader = "Total"
                    };

                awardLetterServiceMock.Setup(l => l.GetAwardLetters(studentId, awardYear)).Returns(expectedAwardLetter);

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);

            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                awardLetterServiceMock = null;
                studentId = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                AwardLettersController = null;
            }

            [TestMethod]
            public void AwardLetterDtoTest()
            {
                actualAwardLetter = AwardLettersController.GetAwardLetter(studentId, awardYear);

                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void StudentIdRequired_BadRequestResponseTest()
            {
                try
                {
                    AwardLettersController.GetAwardLetter(null, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AwardYearRequiredTest()
            {
                try
                {
                    AwardLettersController.GetAwardLetter(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PermissionsExceptionThrowsForbiddenResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters(studentId, awardYear)).Throws(new PermissionsException("PermissionsException Message"));
                try
                {
                    AwardLettersController.GetAwardLetter(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void KeyNotFoundExceptionThrowsNotFoundResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters(studentId, awardYear)).Throws(new KeyNotFoundException("KeyNotFoundException Message"));
                try
                {
                    AwardLettersController.GetAwardLetter(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InvalidOperationExceptionThrowsBadRequestResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters(studentId, awardYear)).Throws(new InvalidOperationException("ioe Message"));
                try
                {
                    AwardLettersController.GetAwardLetter(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void UnknownExceptionThrowsBadRequestResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters(studentId, awardYear)).Throws(new Exception("Exception Message"));
                try
                {
                    AwardLettersController.GetAwardLetter(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetSingleAwardLetter2Tests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;
            private string awardYear;

            private AwardLetter expectedAwardLetter;
            private AwardLetter actualAwardLetter;

            private AwardLettersController AwardLettersController;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0003914";
                awardYear = "2014";

                expectedAwardLetter = new AwardLetter()
                {
                    AcceptedDate = DateTime.Today,
                    AwardColumnHeader = "Award",
                    AwardPeriod1ColumnHeader = "Period1",
                    AwardPeriod2ColumnHeader = "Period2",
                    AwardPeriod3ColumnHeader = "Period3",
                    AwardPeriod4ColumnHeader = "Period4",
                    AwardPeriod5ColumnHeader = "Period5",
                    AwardPeriod6ColumnHeader = "Period6",
                    AwardTableRows = new List<AwardLetterAward>() { new AwardLetterAward() { AwardId = "FOO", AwardDescription = "BAR", Period1Amount = 1000 } },
                    AwardYearCode = awardYear,
                    BudgetAmount = 1000,
                    ClosingParagraph = "This is the closing paragraph",
                    ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    Date = DateTime.Today,
                    EstimatedFamilyContributionAmount = 500,
                    IsContactBlockActive = true,
                    IsNeedBlockActive = true,
                    NeedAmount = 500,
                    NumberAwardPeriodColumns = 2,
                    OpeningParagraph = "This is the opening paragraph",
                    StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    StudentId = studentId,
                    StudentName = "Preferred Name",
                    TotalColumnHeader = "Total"
                };

                awardLetterServiceMock.Setup(l => l.GetAwardLetters2(studentId, awardYear)).Returns(expectedAwardLetter);

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);

            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                awardLetterServiceMock = null;
                studentId = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                AwardLettersController = null;
            }

            [TestMethod]
            public void AwardLetterDtoTest()
            {
                actualAwardLetter = AwardLettersController.GetAwardLetter2(studentId, awardYear);

                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void StudentIdRequired_BadRequestResponseTest()
            {
                try
                {
                    AwardLettersController.GetAwardLetter2(null, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void AwardYearRequiredTest()
            {
                try
                {
                    AwardLettersController.GetAwardLetter2(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void PermissionsExceptionThrowsForbiddenResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters2(studentId, awardYear)).Throws(new PermissionsException("PermissionsException Message"));
                try
                {
                    AwardLettersController.GetAwardLetter2(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void KeyNotFoundExceptionThrowsNotFoundResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters2(studentId, awardYear)).Throws(new KeyNotFoundException("KeyNotFoundException Message"));
                try
                {
                    AwardLettersController.GetAwardLetter2(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void InvalidOperationExceptionThrowsBadRequestResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters2(studentId, awardYear)).Throws(new InvalidOperationException("ioe Message"));
                try
                {
                    AwardLettersController.GetAwardLetter2(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void UnknownExceptionThrowsBadRequestResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters2(studentId, awardYear)).Throws(new Exception("Exception Message"));
                try
                {
                    AwardLettersController.GetAwardLetter2(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetAllAwardLettersTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;

            private IEnumerable<AwardLetter> expectedAwardLetters;
            private List<AwardLetter> testAwardLetters;
            private IEnumerable<AwardLetter> actualAwardLetters;

            private AwardLettersController AwardLettersController;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0003914";
                expectedAwardLetters = new List<AwardLetter>()
                {
                    new AwardLetter()
                    {
                        AcceptedDate = DateTime.Today,
                        AwardColumnHeader = "Award",
                        AwardPeriod1ColumnHeader = "Period1",
                        AwardPeriod2ColumnHeader = "Period2",
                        AwardPeriod3ColumnHeader = "Period3",
                        AwardPeriod4ColumnHeader = "Period4",
                        AwardPeriod5ColumnHeader = "Period5",
                        AwardPeriod6ColumnHeader = "Period6",
                        AwardTableRows = new List<AwardLetterAward>() { new AwardLetterAward() { AwardId = "FOO", AwardDescription = "BAR", Period1Amount = 1000 } },
                        AwardYearCode = "2014",
                        BudgetAmount = 1000,
                        ClosingParagraph = "This is the closing paragraph",
                        ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        Date = DateTime.Today,
                        EstimatedFamilyContributionAmount = 500,
                        IsContactBlockActive = true,
                        IsNeedBlockActive = true,
                        NeedAmount = 500,
                        NumberAwardPeriodColumns = 2,
                        OpeningParagraph = "This is the opening paragraph",
                        StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        StudentId = studentId,
                        StudentName = "Preferred Name",
                        TotalColumnHeader = "Total"
                    },
                    new AwardLetter()
                    {
                        AcceptedDate = DateTime.Today,
                        AwardColumnHeader = "Award",
                        AwardPeriod1ColumnHeader = "Period1",
                        AwardPeriod2ColumnHeader = "Period2",
                        AwardPeriod3ColumnHeader = "Period3",
                        AwardPeriod4ColumnHeader = "Period4",
                        AwardPeriod5ColumnHeader = "Period5",
                        AwardPeriod6ColumnHeader = "Period6",
                        AwardTableRows = new List<AwardLetterAward>() { new AwardLetterAward() { AwardId = "FOO", AwardDescription = "BAR", Period1Amount = 1000 } },
                        AwardYearCode = "2013",
                        BudgetAmount = 1000,
                        ClosingParagraph = "This is the closing paragraph",
                        ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        Date = DateTime.Today,
                        EstimatedFamilyContributionAmount = 500,
                        IsContactBlockActive = true,
                        IsNeedBlockActive = true,
                        NeedAmount = 500,
                        NumberAwardPeriodColumns = 2,
                        OpeningParagraph = "This is the opening paragraph",
                        StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        StudentId = studentId,
                        StudentName = "Preferred Name",
                        TotalColumnHeader = "Total"
                    }
                };

                testAwardLetters = new List<AwardLetter>();
                foreach (var letter in expectedAwardLetters)
                {
                    var testLetter = new AwardLetter();
                    foreach (var property in typeof(AwardLetter).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        property.SetValue(testLetter, property.GetValue(letter, null), null);
                    }
                    testAwardLetters.Add(testLetter);
                }
                awardLetterServiceMock.Setup<IEnumerable<AwardLetter>>(l => l.GetAwardLetters(studentId)).Returns(testAwardLetters);

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);

                actualAwardLetters = AwardLettersController.GetAwardLetters(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                awardLetterServiceMock = null;
                studentId = null;
                expectedAwardLetters = null;
                testAwardLetters = null;
                actualAwardLetters = null;
                AwardLettersController = null;
            }

            [TestMethod]
            public void AwardLetterTypeTest()
            {
                Assert.AreEqual(expectedAwardLetters.GetType(), actualAwardLetters.GetType());
                foreach (var actualLetter in actualAwardLetters)
                {
                    Assert.AreEqual(typeof(AwardLetter), actualLetter.GetType());
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void StudentIdRequiredTest()
            {
                AwardLettersController.GetAwardLetters(null);
            }

            [TestMethod]
            public void StudentIdRequired_BadRequestResponseTest()
            {
                var exceptionCaught = false;
                try
                {
                    AwardLettersController.GetAwardLetters(null);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public void PermissionsExceptionThrowsForbiddenResponseTest()
            {
                var exceptionCaught = false;
                awardLetterServiceMock.Setup(s => s.GetAwardLetters(studentId)).Throws(new PermissionsException("PermissionsException Message"));
                try
                {
                    AwardLettersController.GetAwardLetters(studentId);
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
            public void KeyNotFoundExceptionThrowsNotFoundResponseTest()
            {
                var exceptionCaught = false;
                awardLetterServiceMock.Setup(s => s.GetAwardLetters(studentId)).Throws(new KeyNotFoundException("KeyNotFoundException Message"));
                try
                {
                    AwardLettersController.GetAwardLetters(studentId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public void UnknownExceptionThrowsBadRequestResponseTest()
            {
                var exceptionCaught = false;
                awardLetterServiceMock.Setup(s => s.GetAwardLetters(studentId)).Throws(new Exception("Exception Message"));
                try
                {
                    AwardLettersController.GetAwardLetters(studentId);
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

        [TestClass]
        public class GetAllAwardLetters2Tests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;

            private IEnumerable<AwardLetter> expectedAwardLetters;
            private List<AwardLetter> testAwardLetters;
            private IEnumerable<AwardLetter> actualAwardLetters;

            private AwardLettersController AwardLettersController;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0003914";
                expectedAwardLetters = new List<AwardLetter>()
                {
                    new AwardLetter()
                    {
                        AcceptedDate = DateTime.Today,
                        AwardColumnHeader = "Award",
                        AwardPeriod1ColumnHeader = "Period1",
                        AwardPeriod2ColumnHeader = "Period2",
                        AwardPeriod3ColumnHeader = "Period3",
                        AwardPeriod4ColumnHeader = "Period4",
                        AwardPeriod5ColumnHeader = "Period5",
                        AwardPeriod6ColumnHeader = "Period6",
                        AwardTableRows = new List<AwardLetterAward>() { new AwardLetterAward() { AwardId = "FOO", AwardDescription = "BAR", Period1Amount = 1000 } },
                        AwardYearCode = "2014",
                        BudgetAmount = 1000,
                        ClosingParagraph = "This is the closing paragraph",
                        ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        Date = DateTime.Today,
                        EstimatedFamilyContributionAmount = 500,
                        IsContactBlockActive = true,
                        IsNeedBlockActive = true,
                        NeedAmount = 500,
                        NumberAwardPeriodColumns = 2,
                        OpeningParagraph = "This is the opening paragraph",
                        StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        StudentId = studentId,
                        StudentName = "Preferred Name",
                        TotalColumnHeader = "Total"
                    },
                    new AwardLetter()
                    {
                        AcceptedDate = DateTime.Today,
                        AwardColumnHeader = "Award",
                        AwardPeriod1ColumnHeader = "Period1",
                        AwardPeriod2ColumnHeader = "Period2",
                        AwardPeriod3ColumnHeader = "Period3",
                        AwardPeriod4ColumnHeader = "Period4",
                        AwardPeriod5ColumnHeader = "Period5",
                        AwardPeriod6ColumnHeader = "Period6",
                        AwardTableRows = new List<AwardLetterAward>() { new AwardLetterAward() { AwardId = "FOO", AwardDescription = "BAR", Period1Amount = 1000 } },
                        AwardYearCode = "2013",
                        BudgetAmount = 1000,
                        ClosingParagraph = "This is the closing paragraph",
                        ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        Date = DateTime.Today,
                        EstimatedFamilyContributionAmount = 500,
                        IsContactBlockActive = true,
                        IsNeedBlockActive = true,
                        NeedAmount = 500,
                        NumberAwardPeriodColumns = 2,
                        OpeningParagraph = "This is the opening paragraph",
                        StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        StudentId = studentId,
                        StudentName = "Preferred Name",
                        TotalColumnHeader = "Total"
                    }
                };

                testAwardLetters = new List<AwardLetter>();
                foreach (var letter in expectedAwardLetters)
                {
                    var testLetter = new AwardLetter();
                    foreach (var property in typeof(AwardLetter).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        property.SetValue(testLetter, property.GetValue(letter, null), null);
                    }
                    testAwardLetters.Add(testLetter);
                }
                awardLetterServiceMock.Setup<IEnumerable<AwardLetter>>(l => l.GetAwardLetters2(studentId)).Returns(testAwardLetters);

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);

                actualAwardLetters = AwardLettersController.GetAwardLetters2(studentId);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                awardLetterServiceMock = null;
                studentId = null;
                expectedAwardLetters = null;
                testAwardLetters = null;
                actualAwardLetters = null;
                AwardLettersController = null;
            }

            [TestMethod]
            public void AwardLetterTypeTest()
            {
                Assert.AreEqual(expectedAwardLetters.GetType(), actualAwardLetters.GetType());
                foreach (var actualLetter in actualAwardLetters)
                {
                    Assert.AreEqual(typeof(AwardLetter), actualLetter.GetType());
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public void StudentIdRequiredTest()
            {
                AwardLettersController.GetAwardLetters(null);
            }

            [TestMethod]
            public void StudentIdRequired_BadRequestResponseTest()
            {
                var exceptionCaught = false;
                try
                {
                    AwardLettersController.GetAwardLetters(null);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);
            }

            [TestMethod]
            public void PermissionsExceptionThrowsForbiddenResponseTest()
            {
                var exceptionCaught = false;
                awardLetterServiceMock.Setup(s => s.GetAwardLetters2(studentId)).Throws(new PermissionsException("PermissionsException Message"));
                try
                {
                    AwardLettersController.GetAwardLetters2(studentId);
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
            public void KeyNotFoundExceptionThrowsNotFoundResponseTest()
            {
                var exceptionCaught = false;
                awardLetterServiceMock.Setup(s => s.GetAwardLetters2(studentId)).Throws(new KeyNotFoundException("KeyNotFoundException Message"));
                try
                {
                    AwardLettersController.GetAwardLetters2(studentId);
                }
                catch (HttpResponseException hre)
                {
                    exceptionCaught = true;
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                }

                Assert.IsTrue(exceptionCaught);

                loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public void UnknownExceptionThrowsBadRequestResponseTest()
            {
                var exceptionCaught = false;
                awardLetterServiceMock.Setup(s => s.GetAwardLetters2(studentId)).Throws(new Exception("Exception Message"));
                try
                {
                    AwardLettersController.GetAwardLetters2(studentId);
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

        [TestClass]
        public class GetSingleAwardLetter3Tests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;
            private string awardYear;

            private AwardLetter2 expectedAwardLetter;
            private AwardLetter2 actualAwardLetter;

            private AwardLettersController AwardLettersController;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0003914";
                awardYear = "2014";

                expectedAwardLetter = new AwardLetter2()
                {
                    AcceptedDate = DateTime.Today,
                    AwardLetterYear = awardYear,
                    BudgetAmount = 1000,
                    ClosingParagraph = "This is the closing paragraph",
                    ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    CreatedDate = DateTime.Today,
                    EstimatedFamilyContributionAmount = 500,
                    NeedAmount = 500,
                    OpeningParagraph = "This is the opening paragraph",
                    StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    StudentId = studentId,
                    StudentName = "Preferred Name",
                    AwardYearDescription = "2014 Award Year",
                    StudentOfficeCode = "MAIN"
                };

                awardLetterServiceMock.Setup(l => l.GetAwardLetter3Async(studentId, awardYear)).Returns(Task.FromResult(expectedAwardLetter));

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);

            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                awardLetterServiceMock = null;
                studentId = null;
                expectedAwardLetter = null;
                actualAwardLetter = null;
                AwardLettersController = null;
            }

            [TestMethod]
            public async Task AwardLetterDtoTest()
            {
                actualAwardLetter = await AwardLettersController.GetAwardLetter3Async(studentId, awardYear);

                Assert.IsNotNull(actualAwardLetter);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequired_BadRequestResponseTest()
            {
                try
                {
                    await AwardLettersController.GetAwardLetter3Async(null, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AwardYearRequiredTest()
            {
                try
                {
                    await AwardLettersController.GetAwardLetter3Async(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionThrowsForbiddenResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetter3Async(studentId, awardYear)).Throws(new PermissionsException("PermissionsException Message"));
                try
                {
                    await AwardLettersController.GetAwardLetter3Async(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionThrowsNotFoundResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetter3Async(studentId, awardYear)).Throws(new KeyNotFoundException("KeyNotFoundException Message"));
                try
                {
                    await AwardLettersController.GetAwardLetter3Async(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InvalidOperationExceptionThrowsBadRequestResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetter3Async(studentId, awardYear)).Throws(new InvalidOperationException("ioe Message"));
                try
                {
                    await AwardLettersController.GetAwardLetter3Async(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UnknownExceptionThrowsBadRequestResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetter3Async(studentId, awardYear)).Throws(new Exception("Exception Message"));
                try
                {
                    await AwardLettersController.GetAwardLetter3Async(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetAllAwardLetters3Tests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;

            private IEnumerable<AwardLetter2> expectedAwardLetters;
            private List<AwardLetter2> testAwardLetters;
            private IEnumerable<AwardLetter2> actualAwardLetters;

            private AwardLettersController AwardLettersController;


            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0003914";
                expectedAwardLetters = new List<AwardLetter2>()
                {
                    new AwardLetter2()
                    {
                        AcceptedDate = DateTime.Today,
                        AwardLetterYear = "2014",
                        BudgetAmount = 1000,
                        ClosingParagraph = "This is the closing paragraph",
                        ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        CreatedDate = DateTime.Today,
                        EstimatedFamilyContributionAmount = 500,
                        NeedAmount = 500,
                        OpeningParagraph = "This is the opening paragraph",
                        StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        StudentId = studentId,
                        StudentName = "Preferred Name",
                        AwardYearDescription = "2014 Award Year",
                        StudentOfficeCode = "MAIN"
                    },
                    new AwardLetter2()
                    {
                        AcceptedDate = DateTime.Today,
                        AwardLetterYear = "2015",
                        BudgetAmount = 1000,
                        ClosingParagraph = "This is the closing paragraph",
                        ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        CreatedDate = DateTime.Today,
                        EstimatedFamilyContributionAmount = 500,
                        NeedAmount = 500,
                        OpeningParagraph = "This is the opening paragraph",
                        StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                        StudentId = studentId,
                        StudentName = "Preferred Name",
                        AwardYearDescription = "2014 Award Year",
                        StudentOfficeCode = "MAIN"
                    }
                };

                testAwardLetters = new List<AwardLetter2>();
                foreach (var letter in expectedAwardLetters)
                {
                    var testLetter = new AwardLetter2();
                    foreach (var property in typeof(AwardLetter2).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        property.SetValue(testLetter, property.GetValue(letter, null), null);
                    }
                    testAwardLetters.Add(testLetter);
                }
                awardLetterServiceMock.Setup(l => l.GetAwardLetters3Async(studentId)).Returns(Task.FromResult(testAwardLetters.AsEnumerable()));

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);

                actualAwardLetters = AwardLettersController.GetAwardLetters3Async(studentId).Result;
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistryMock = null;
                loggerMock = null;
                awardLetterServiceMock = null;
                studentId = null;
                expectedAwardLetters = null;
                testAwardLetters = null;
                actualAwardLetters = null;
                AwardLettersController = null;
            }

            [TestMethod]
            public void AwardLetterTypeTest()
            {
                Assert.AreEqual(expectedAwardLetters.GetType(), actualAwardLetters.GetType());
                foreach (var actualLetter in actualAwardLetters)
                {
                    Assert.AreEqual(typeof(AwardLetter2), actualLetter.GetType());
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequired_BadRequestResponseTest()
            {
                try
                {
                    await AwardLettersController.GetAwardLetters3Async(null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AwardYearRequiredTest()
            {
                try
                {
                    await AwardLettersController.GetAwardLetter3Async(studentId, null);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PermissionsExceptionThrowsForbiddenResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters3Async(studentId)).Throws(new PermissionsException("PermissionsException Message"));
                try
                {
                    await AwardLettersController.GetAwardLetters3Async(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task KeyNotFoundExceptionThrowsNotFoundResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters3Async(studentId)).Throws(new KeyNotFoundException("KeyNotFoundException Message"));
                try
                {
                    await AwardLettersController.GetAwardLetters3Async(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<KeyNotFoundException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task InvalidOperationExceptionThrowsBadRequestResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters3Async(studentId)).Throws(new InvalidOperationException("ioe Message"));
                try
                {
                    await AwardLettersController.GetAwardLetters3Async(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<InvalidOperationException>(), It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UnknownExceptionThrowsBadRequestResponseTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetters3Async(studentId)).Throws(new Exception("Exception Message"));
                try
                {
                    await AwardLettersController.GetAwardLetters3Async(studentId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    loggerMock.Verify(l => l.Error(It.IsAny<Exception>(), It.IsAny<string>()));
                    throw;
                }
            }
        }

        [TestClass]
        public class GetAwardLetterReport3AsyncTests
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

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAwardLetterService> awardLetterServiceMock;
            private ApiSettings apiSettings;

            private string studentId;
            private string awardLetterId;

            private byte[] expectedAwardLetterByteArray;
            private AwardLetter2 expectedAwardLetter;

            private AwardLettersController AwardLettersController;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();
                awardLetterServiceMock = new Mock<IAwardLetterService>();
                apiSettings = new ApiSettings("TEST");

                studentId = "0004791";
                awardLetterId = "56";

                expectedAwardLetter = new AwardLetter2()
                {
                    AcceptedDate = DateTime.Today,
                    BudgetAmount = 1000,
                    ClosingParagraph = "This is the closing paragraph",
                    ContactAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    EstimatedFamilyContributionAmount = 500,
                    NeedAmount = 500,
                    OpeningParagraph = "This is the opening paragraph",
                    StudentAddress = new List<AwardLetterAddress>() { new AwardLetterAddress() { AddressLine = "AddressLine1" } },
                    StudentId = studentId,
                    StudentName = "Preferred Name",
                    AwardLetterParameterId = "ALTR2",
                    AwardLetterYear = "2015",
                    AwardYearDescription = "2015 description",
                    CreatedDate = DateTime.Today,
                    Id = "56",
                    StudentOfficeCode = "LAW",
                    HousingCode = HousingCode.OffCampus,
                    AwardLetterAnnualAwards = new List<AwardLetterAnnualAward>(){
                        new AwardLetterAnnualAward(){
                            AnnualAwardAmount = 1000,
                            AwardDescription = "Award 1 description",
                            AwardId = "AWARD1",
                            AwardLetterAwardPeriods = new List<AwardLetterAwardPeriod>(){
                                new AwardLetterAwardPeriod(){
                                    AwardDescription = "Award 1 description",
                                    AwardId = "AWARD1",
                                    AwardPeriodAmount = 1000,
                                    ColumnName = "Fall 2015",
                                    ColumnNumber = 1,
                                    GroupName = "Awards",
                                    GroupNumber = 1
                                }
                            },
                            GroupName = "Awards",
                            GroupNumber = 1
                        }
                    }
                };

                awardLetterServiceMock.Setup(l => l.GetAwardLetterByIdAsync(studentId, awardLetterId)).Returns(Task.FromResult(expectedAwardLetter));

                var binaryFormatter = new BinaryFormatter();
                var memoryStream = new MemoryStream();
                binaryFormatter.Serialize(memoryStream, "This is the awardLetter report pdf");
                expectedAwardLetterByteArray = memoryStream.ToArray();

                awardLetterServiceMock.Setup(l => l.GetAwardLetterReport3Async(It.IsAny<AwardLetter2>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(expectedAwardLetterByteArray));

                var httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                AwardLettersController = new AwardLettersController(adapterRegistryMock.Object, awardLetterServiceMock.Object, loggerMock.Object, apiSettings);
            }

            [TestMethod]
            public async Task PdfReportAsByteArrayTest()
            {
                var response = await AwardLettersController.GetAwardLetterReport3Async(studentId, awardLetterId);
                var byteArray = response.Content.ReadAsByteArrayAsync().Result;

                CollectionAssert.AreEqual(expectedAwardLetterByteArray, byteArray);
            }

            [TestMethod]
            public async Task PdfResponseContentHeadersTest()
            {

                var response = await AwardLettersController.GetAwardLetterReport3Async(studentId, awardLetterId);

                var expectedContentType = new MediaTypeHeaderValue("application/pdf");
                var expectedContentLength = expectedAwardLetterByteArray.Length;
                var expectedContentDisposition = new ContentDispositionHeaderValue("attachment");
                var expectedStartOfFileName = "AwardLetter_" + studentId + "_" + expectedAwardLetter.AwardLetterYear + "_";

                Assert.AreEqual(expectedContentType, response.Content.Headers.ContentType);
                Assert.AreEqual(expectedContentLength, response.Content.Headers.ContentLength);
                Assert.AreEqual(expectedContentDisposition.DispositionType, response.Content.Headers.ContentDisposition.DispositionType);
                Assert.AreEqual(expectedStartOfFileName, response.Content.Headers.ContentDisposition.FileName.Substring(0, expectedStartOfFileName.Length));
            }

            [TestMethod]
            public async Task EmptyReportLogoPathTest()
            {
                apiSettings.ReportLogoPath = string.Empty;
                var actualLogoPath = string.Empty;
                awardLetterServiceMock.Setup(l => l.GetAwardLetterReport3Async(It.IsAny<AwardLetter2>(), It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(Task.FromResult(expectedAwardLetterByteArray))
                    .Callback<AwardLetter2, string, string>((l, p, i) => actualLogoPath = i);

                var response = await AwardLettersController.GetAwardLetterReport3Async(studentId, awardLetterId);

                Assert.AreEqual(apiSettings.ReportLogoPath, actualLogoPath);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullStudentId_ThrowsExceptionTest()
            {
                await AwardLettersController.GetAwardLetterReport3Async(null, awardLetterId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NullAwardLetterId_ThrowsExceptionTest()
            {
                await AwardLettersController.GetAwardLetterReport3Async(studentId, null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task NoAwardLetterDtoReturned_ThrowsExceptionTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetterByIdAsync(studentId, awardLetterId)).ReturnsAsync(null);
                try
                {
                    await AwardLettersController.GetAwardLetterReport3Async(studentId, awardLetterId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.NotFound, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task UserDoesNotHavePermissions_ThrowsExceptionTest(){
                awardLetterServiceMock.Setup(s => s.GetAwardLetterByIdAsync(studentId, awardLetterId)).Throws(new PermissionsException());
                try{
                    await AwardLettersController.GetAwardLetterReport3Async(studentId, awardLetterId);
                }
                catch(HttpResponseException hre){
                    Assert.AreEqual(HttpStatusCode.Forbidden, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PersonNotStudentOrApplicant_ThrowsExceptionTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetterByIdAsync(studentId, awardLetterId)).Throws(new InvalidOperationException());
                try
                {
                    await AwardLettersController.GetAwardLetterReport3Async(studentId, awardLetterId);
                }
                catch (HttpResponseException hre)
                {
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GenericExceptionThrownByService_ThrowsExceptionTest()
            {
                awardLetterServiceMock.Setup(s => s.GetAwardLetterByIdAsync(studentId, awardLetterId)).Throws(new Exception());
                
                await AwardLettersController.GetAwardLetterReport3Async(studentId, awardLetterId);
                
            }
        }
    }
}

