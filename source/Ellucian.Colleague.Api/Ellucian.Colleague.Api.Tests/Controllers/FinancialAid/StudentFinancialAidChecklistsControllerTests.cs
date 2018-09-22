/*Copyright 2015-2018 Ellucian Company L.P. and its affiliates.*/
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Ellucian.Colleague.Api.Tests.Controllers.FinancialAid
{
    [TestClass]
    public class StudentFinancialAidChecklistsControllerTests
    {
        public Mock<IAdapterRegistry> adapterRegistryMock;
        public Mock<ILogger> loggerMock;
        public Mock<IStudentChecklistService> studentChecklistServiceMock;

        public StudentFinancialAidChecklistsController actualController;

        public FunctionEqualityComparer<StudentFinancialAidChecklist> studentChecklistDtoComparer;
        public FunctionEqualityComparer<StudentChecklistItem> studentChecklistItemDtoComparer;

        public void StudentFinancialAidChecklistsControllerTestsInitialize()
        {
            adapterRegistryMock = new Mock<IAdapterRegistry>();
            loggerMock = new Mock<ILogger>();
            studentChecklistServiceMock = new Mock<IStudentChecklistService>();

            studentChecklistDtoComparer = new FunctionEqualityComparer<StudentFinancialAidChecklist>(
                (c1, c2) => c1.AwardYear == c2.AwardYear && c1.StudentId == c2.StudentId && c1.ChecklistItems.Count == c2.ChecklistItems.Count,
                (c) => c.AwardYear.GetHashCode() ^ c.StudentId.GetHashCode() ^ c.ChecklistItems.Count);

            studentChecklistItemDtoComparer = new FunctionEqualityComparer<StudentChecklistItem>(
                (i1, i2) => i1.Code == i2.Code && i1.ControlStatus == i2.ControlStatus,
                (i) => i.Code.GetHashCode() ^ i.ControlStatus.GetHashCode());
        }

        [TestClass]
        public class CreateStudentFinancialAidChecklistTests : StudentFinancialAidChecklistsControllerTests
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

            public string studentId;
            public string awardYear;

            public StudentFinancialAidChecklist expectedChecklistDto;
            public HttpResponseMessage actualResponseMessage;

            [TestInitialize]
            public async void Initialize()
            {
                StudentFinancialAidChecklistsControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0003914";
                awardYear = "2015";

                expectedChecklistDto = new StudentFinancialAidChecklist()
                {
                    StudentId = studentId,
                    AwardYear = awardYear,
                    ChecklistItems = new List<StudentChecklistItem>() { new StudentChecklistItem() { Code = "CDDDD", ControlStatus = ChecklistItemControlStatus.CompletionRequired } }
                };

                studentChecklistServiceMock.Setup(s => s.CreateStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, string, bool>((id, year, b) => Task.FromResult(expectedChecklistDto));

                //this sets up the route for location link resolution
                var config = new HttpConfiguration();
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/api/loan-requests/");
                var getRoute = config.Routes.MapHttpRoute(

                    name: "GetStudentFinancialAidChecklistAsync",
                    routeTemplate: "api/students/{studentId}/financial-aid-checklists/{year}",
                    defaults: new { controller = "StudentFinancialAidChecklists", action = "GetStudentFinancialAidChecklistAsync" }
                );
                var routeData = new HttpRouteData(getRoute);

                var httpResponse = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), httpResponse);

                actualController = new StudentFinancialAidChecklistsController(adapterRegistryMock.Object, studentChecklistServiceMock.Object, loggerMock.Object);
                actualController.ControllerContext = new HttpControllerContext(config, routeData, request);
                actualController.Request = request;
                actualController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
                actualController.Url = new UrlHelper(request);
                actualResponseMessage = await actualController.CreateStudentFinancialAidChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                var actualDto = JsonConvert.DeserializeObject<StudentFinancialAidChecklist>(actualResponseMessage.Content.ReadAsStringAsync().Result);

                Assert.IsTrue(studentChecklistDtoComparer.Equals(expectedChecklistDto, actualDto));
                CollectionAssert.AreEqual(expectedChecklistDto.ChecklistItems, actualDto.ChecklistItems, studentChecklistItemDtoComparer);
            }

            [TestMethod]
            public void CreatedResponseCodeTest()
            {
                Assert.AreEqual(HttpStatusCode.Created, actualResponseMessage.StatusCode);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = null;
                await actualController.CreateStudentFinancialAidChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AwardYearRequiredTest()
            {
                awardYear = null;
                await actualController.CreateStudentFinancialAidChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchPermissionsExceptionTest()
            {
                studentChecklistServiceMock.Setup(s => s.CreateStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await actualController.CreateStudentFinancialAidChecklistAsync(studentId, awardYear);
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
            public async Task CatchExistingResourceExceptionTest()
            {
                studentChecklistServiceMock.Setup(s => s.CreateStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new ExistingResourceException("ere", awardYear));
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), new HttpResponse(new StringWriter()));

                try
                {
                    await actualController.CreateStudentFinancialAidChecklistAsync(studentId, awardYear);
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
            public async Task CatchApplicationExceptionTest()
            {
                studentChecklistServiceMock.Setup(s => s.CreateStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new ApplicationException("aex"));

                try
                {
                    await actualController.CreateStudentFinancialAidChecklistAsync(studentId, awardYear);
                }
                catch (HttpResponseException hre)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<ApplicationException>(), It.IsAny<string>()));
                    Assert.AreEqual(HttpStatusCode.BadRequest, hre.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchGenericExceptionTest()
            {
                studentChecklistServiceMock.Setup(s => s.CreateStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await actualController.CreateStudentFinancialAidChecklistAsync(studentId, awardYear);
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
        public class GetSingleStudentFinancialAidChecklistTests : StudentFinancialAidChecklistsControllerTests
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

            public string studentId;
            public string awardYear;

            public StudentFinancialAidChecklist expectedChecklistDto;
            public StudentFinancialAidChecklist actualChecklistDto;

            [TestInitialize]
            public async void Initialize()
            {
                StudentFinancialAidChecklistsControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0003914";
                awardYear = "2015";

                expectedChecklistDto = new StudentFinancialAidChecklist()
                {
                    StudentId = studentId,
                    AwardYear = awardYear,
                    ChecklistItems = new List<StudentChecklistItem>()
                        {
                            new StudentChecklistItem() {Code = "F", ControlStatus = ChecklistItemControlStatus.CompletionRequired},
                            new StudentChecklistItem() {Code = "R", ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater}
                        }
                };

                studentChecklistServiceMock.Setup(s => s.GetStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, string, bool>((id, year, b) => Task.FromResult(expectedChecklistDto));

                actualController = new StudentFinancialAidChecklistsController(adapterRegistryMock.Object, studentChecklistServiceMock.Object, loggerMock.Object);
                actualChecklistDto = await actualController.GetStudentFinancialAidChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                Assert.IsTrue(studentChecklistDtoComparer.Equals(expectedChecklistDto, actualChecklistDto));
                CollectionAssert.AreEqual(expectedChecklistDto.ChecklistItems, actualChecklistDto.ChecklistItems, studentChecklistItemDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = string.Empty;
                await actualController.GetStudentFinancialAidChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AwardYearRequiredTest()
            {
                awardYear = string.Empty;
                await actualController.GetStudentFinancialAidChecklistAsync(studentId, awardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchPermissionsExceptionTest()
            {
                studentChecklistServiceMock.Setup(s => s.GetStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await actualController.GetStudentFinancialAidChecklistAsync(studentId, awardYear);
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
                studentChecklistServiceMock.Setup(s => s.GetStudentChecklistAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await actualController.GetStudentFinancialAidChecklistAsync(studentId, awardYear);
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
        public class GetAllStudentFinancialAidChecklistsTests : StudentFinancialAidChecklistsControllerTests
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

            public string studentId;

            public List<StudentFinancialAidChecklist> expectedChecklistDtos;
            public List<StudentFinancialAidChecklist> actualChecklistDtos;

            [TestInitialize]
            public async void Initialize()
            {
                StudentFinancialAidChecklistsControllerTestsInitialize();
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                studentId = "0003914";

                expectedChecklistDtos = new List<StudentFinancialAidChecklist>()
                {
                    new StudentFinancialAidChecklist()
                    {
                        StudentId = studentId,
                        AwardYear = "2014",
                        ChecklistItems = new List<StudentChecklistItem>()
                        {
                            new StudentChecklistItem() {Code = "F", ControlStatus = ChecklistItemControlStatus.CompletionRequired},
                            new StudentChecklistItem() {Code = "R", ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater}
                        }
                    },
                    new StudentFinancialAidChecklist()
                    {
                        StudentId = studentId,
                        AwardYear = "2015",
                        ChecklistItems = new List<StudentChecklistItem>()
                        {
                            new StudentChecklistItem() {Code = "F", ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater},
                            new StudentChecklistItem() {Code = "R", ControlStatus = ChecklistItemControlStatus.CompletionRequiredLater}
                        }
                    },
                };

                studentChecklistServiceMock.Setup(s => s.GetAllStudentChecklistsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, b) => Task.FromResult(expectedChecklistDtos.AsEnumerable()));

                actualController = new StudentFinancialAidChecklistsController(adapterRegistryMock.Object, studentChecklistServiceMock.Object, loggerMock.Object);
                actualChecklistDtos = (await actualController.GetAllStudentFinancialAidChecklistsAsync(studentId)).ToList();
            }

            [TestMethod]
            public void ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(expectedChecklistDtos, actualChecklistDtos, studentChecklistDtoComparer);
                CollectionAssert.AreEqual(expectedChecklistDtos.SelectMany(c => c.ChecklistItems).ToList(), actualChecklistDtos.SelectMany(c => c.ChecklistItems).ToList(), studentChecklistItemDtoComparer);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentIdRequiredTest()
            {
                studentId = string.Empty;
                await actualController.GetAllStudentFinancialAidChecklistsAsync(studentId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task CatchPermissionsExceptionTest()
            {
                studentChecklistServiceMock.Setup(s => s.GetAllStudentChecklistsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new PermissionsException("pex"));

                try
                {
                    await actualController.GetAllStudentFinancialAidChecklistsAsync(studentId);
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
                studentChecklistServiceMock.Setup(s => s.GetAllStudentChecklistsAsync(It.IsAny<string>(), It.IsAny<bool>()))
                    .Throws(new Exception("ex"));

                try
                {
                    await actualController.GetAllStudentFinancialAidChecklistsAsync(studentId);
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
