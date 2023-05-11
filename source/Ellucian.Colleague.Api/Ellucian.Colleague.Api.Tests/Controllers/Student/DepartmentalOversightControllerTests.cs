// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Hosting;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class DepartmentalOversightControllerTests
    {
        [TestClass]
        public class DepartmentatlOversightQueryAsync
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

            private DepartmentalOversightController departmentalOversightController;
            private Mock<IDepartmentalOversightService> departmentalOversightServiceMock;
            private IDepartmentalOversightService departmentalOversightService;
            private ILogger logger;
            private IAdapterRegistry adapterRegistry;
            private HttpResponse response;



            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                departmentalOversightServiceMock = new Mock<IDepartmentalOversightService>();
                departmentalOversightService = departmentalOversightServiceMock.Object;
                logger = new Mock<ILogger>().Object;



                // controller that will be tested using mock objects
                departmentalOversightController = new DepartmentalOversightController(departmentalOversightService, logger);
                departmentalOversightController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                departmentalOversightController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());


                HashSet<ITypeAdapter> adapters = new HashSet<ITypeAdapter>();
                adapterRegistry = new AdapterRegistry(adapters, logger);
                var testAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.DeptOversightSearchResult, DeptOversightSearchResult>(adapterRegistry, logger);
                adapterRegistry.AddAdapter(testAdapter);


                Mapper.CreateMap<Domain.Student.Entities.DeptOversightSearchResult, Dtos.Student.DeptOversightSearchResult>();

                // mock an deptOverSightSearchResult dtos to return
                List<DeptOversightSearchResult> SearchResults = new List<DeptOversightSearchResult>() { new DeptOversightSearchResult() { FacultyId = "0001", Department = "MATH", SectionIds = new List<string>() { "Section1", "Section2" } }, new DeptOversightSearchResult() { FacultyId = "0002", Department = "HIST", SectionIds = new List<string>() { "Section3", "Section4" } } };


                departmentalOversightServiceMock.Setup(x => x.SearchAsync(It.IsAny<DeptOversightSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(SearchResults);

                // Set up an Http Context
                response = new HttpResponse(new StringWriter());
                HttpContext.Current = new HttpContext(new HttpRequest("", "http://doesntMatter.com", ""), response);
            }

            [TestCleanup]
            public void Cleanup()
            {
                departmentalOversightController = null;
                departmentalOversightService = null;
            }

            [TestMethod]
            public async Task QueryDepartmentalOversightByPostAsync()
            {
                DeptOversightSearchCriteria criteria = new DeptOversightSearchCriteria() { FacultyKeyword = "",SectionKeyword = "Section1" };
                var searchList = await departmentalOversightController.QueryDepartmentalOversightByPostAsync(criteria);
                Assert.IsTrue(searchList.Count() == 2);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Search_ExceptionReturnsHttpResponseException()
            {
                var crit = new DeptOversightSearchCriteria();
                departmentalOversightServiceMock.Setup(x => x.SearchAsync(crit, It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new Exception("some kind of error"));
                var adv = await departmentalOversightController.QueryDepartmentalOversightByPostAsync(crit);
            }
        }

        [TestClass]
        public class DepartmentalOversightController_GetDepartmentalOversightPermissionsAsync_Tests
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

            private DepartmentalOversightController departmentalOversightController;
            private Mock<IDepartmentalOversightService> departmentalOversightServiceMock;
            private IDepartmentalOversightService departmentalOversightService;
            private ILogger logger;

            private DepartmentalOversightPermissions permissions;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                departmentalOversightServiceMock = new Mock<IDepartmentalOversightService>();
                departmentalOversightService = departmentalOversightServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                permissions = new DepartmentalOversightPermissions()
                {
                    CanViewSectionAddAuthorizations = true,
                    CanViewSectionAttendance = true,
                    CanViewSectionBooks = true,
                    CanViewSectionCensus = true,
                    CanViewSectionDropRoster = true,
                    CanViewSectionFacultyConsents = true,
                    CanViewSectionGrading = true,
                    CanViewSectionPrerequisiteWaiver = true,
                    CanViewSectionRoster = true,
                    CanViewSectionStudentPetitions = true,
                    CanViewSectionWaitlists = true,
                    CanCreateSectionBooks = true,
                    CanCreateSectionGrading = true,
                    CanCreateSectionAddAuthorization = true,
                    CanCreateSectionPrerequisiteWaiver = true,
                    CanCreateSectionStudentPetition = true,
                    CanCreateSectionFacultyConsent = true,
                    CanSearchStudents = true,
                    CanCreateSectionAttendance = true,
                    CanCreateSectionCensus = true,
                    CanCreateSectionDropRoster=true
                };

                departmentalOversightController = new DepartmentalOversightController(departmentalOversightService, logger);
                departmentalOversightController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                departmentalOversightController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                departmentalOversightController = null;
                departmentalOversightService = null;
            }

            [TestMethod]
            public async Task GetDepartmentalOversightPermissionsAsync_Success()
            {
                departmentalOversightServiceMock.Setup(s => s.GetDepartmentalOversightPermissionsAsync()).ReturnsAsync(permissions);
                var response = await departmentalOversightService.GetDepartmentalOversightPermissionsAsync();
                Assert.IsNotNull(response);
                Assert.IsTrue(response.CanViewSectionAddAuthorizations);
                Assert.IsTrue(response.CanViewSectionAttendance);
                Assert.IsTrue(response.CanViewSectionBooks);
                Assert.IsTrue(response.CanViewSectionCensus);
                Assert.IsTrue(response.CanViewSectionDropRoster);
                Assert.IsTrue(response.CanViewSectionFacultyConsents);
                Assert.IsTrue(response.CanViewSectionGrading);
                Assert.IsTrue(response.CanViewSectionPrerequisiteWaiver);
                Assert.IsTrue(response.CanViewSectionRoster);
                Assert.IsTrue(response.CanViewSectionStudentPetitions);
                Assert.IsTrue(response.CanViewSectionWaitlists);
                Assert.IsTrue(response.CanCreateSectionBooks);
                Assert.IsTrue(response.CanCreateSectionAddAuthorization);
                Assert.IsTrue(response.CanCreateSectionPrerequisiteWaiver);
                Assert.IsTrue(response.CanCreateSectionStudentPetition);
                Assert.IsTrue(response.CanCreateSectionFacultyConsent);
                Assert.IsTrue(response.CanSearchStudents);
                Assert.IsTrue(response.CanCreateSectionGrading);
                Assert.IsTrue(response.CanCreateSectionCensus);
                Assert.IsTrue(response.CanCreateSectionAttendance);
                Assert.IsTrue(response.CanCreateSectionDropRoster);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetDepartmentalOversightPermissionsAsync_Exception()
            {
                departmentalOversightServiceMock.Setup(s => s.GetDepartmentalOversightPermissionsAsync()).ThrowsAsync(new Exception());
                var reesponse = await departmentalOversightController.GetDepartmentalOversightPermissionsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetDepartmentalOversightPermissionsAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    departmentalOversightServiceMock.Setup(s => s.GetDepartmentalOversightPermissionsAsync())
                        .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    await departmentalOversightController.GetDepartmentalOversightPermissionsAsync();
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

        }
    }
}
