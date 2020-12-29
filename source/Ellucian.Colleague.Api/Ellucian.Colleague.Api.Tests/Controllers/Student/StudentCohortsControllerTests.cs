// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using StudentCohort = Ellucian.Colleague.Dtos.StudentCohort;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentCohortsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IStudentService> studentServiceMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            StudentCohortsController studentCohortsController;
            List<Dtos.StudentCohort> studentCohorts;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

                studentServiceMock = new Mock<IStudentService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                BuildData();

                studentCohortsController = new StudentCohortsController(adapterRegistryMock.Object, studentServiceMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                studentCohortsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                studentCohortsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
            }

            private void BuildData()
            {
                studentCohorts = new List<StudentCohort>() 
                {
                    new StudentCohort(){ Id = "e8dbcea5-ffb8-471e-87b7-ce5d36d5c2e7", Code = "ATHL", Description = "Athletes", Title = "Athletes" },
                    new StudentCohort(){ Id = "c2f57ee5-1c30-44a5-9d18-311f71f7b722", Code = "FRAT", Description = "Fraternity", Title = "Fraternity" },
                    new StudentCohort(){ Id = "f05a6c0f-3a56-4a87-b931-bc2901da5ef9", Code = "SORO", Description = "Sorority", Title = "Sorority" },
                    new StudentCohort(){ Id = "05872218-f749-4cdc-b4f0-43200cc21335", Code = "ROTC", Description = "ROTC Participants", Title = "ROTC Participants" },
                    new StudentCohort(){ Id = "827fffc4-3dd2-4492-8f51-4134597ec4bf", Code = "VETS", Description = "Military Veterans", Title = "Military Veterans" }
                };
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentCohortsController = null;
                studentCohorts = null;
                studentServiceMock = null;
                adapterRegistryMock = null;
                loggerMock = null;
            }

            [TestMethod]
            public async Task StudentCohortsController_GetAll_NoCache_True()
            {
                studentCohortsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };
                studentServiceMock.Setup( i => i.GetAllStudentCohortsAsync( It.IsAny<Dtos.Filters.CodeItemFilter>(), true ) ).ReturnsAsync( studentCohorts );

                var actuals = await studentCohortsController.GetStudentCohortsAsync( It.IsAny<QueryStringFilter>() );

                Assert.IsNotNull( actuals );

                foreach( var actual in actuals )
                {
                    var expected = studentCohorts.FirstOrDefault( i => i.Id.Equals( actual.Id, StringComparison.OrdinalIgnoreCase ) );
                    Assert.IsNotNull( expected );

                    Assert.AreEqual( expected.Id, actual.Id );
                    Assert.AreEqual( expected.Code, actual.Code );
                    Assert.AreEqual( expected.Description, actual.Description );
                    Assert.AreEqual( expected.Title, actual.Title );
                }
            }

            [TestMethod]
            public async Task StudentCohortsController_GetAll_NoCache_False()
            {
                studentCohortsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };
                studentServiceMock.Setup( i => i.GetAllStudentCohortsAsync( It.IsAny<Dtos.Filters.CodeItemFilter>(), false ) ).ReturnsAsync( studentCohorts );

                var actuals = await studentCohortsController.GetStudentCohortsAsync(It.IsAny<QueryStringFilter>());

                Assert.IsNotNull( actuals );

                foreach( var actual in actuals )
                {
                    var expected = studentCohorts.FirstOrDefault( i => i.Id.Equals( actual.Id, StringComparison.OrdinalIgnoreCase ) );

                    Assert.IsNotNull( expected );
                    Assert.AreEqual( expected.Id, actual.Id );
                    Assert.AreEqual( expected.Code, actual.Code );
                    Assert.AreEqual( expected.Description, actual.Description );
                    Assert.AreEqual( expected.Title, actual.Title );
                }
            }

            [TestMethod]
            public async Task StudentCohortsController_GetById()
            {
                string id = "f05a6c0f-3a56-4a87-b931-bc2901da5ef9";
                var expected = studentCohorts.FirstOrDefault( i => i.Id.Equals( id, StringComparison.OrdinalIgnoreCase ) );
                studentServiceMock.Setup( i => i.GetStudentCohortByGuidAsync( id, It.IsAny<bool>() ) ).ReturnsAsync( expected );

                var actual = await studentCohortsController.GetStudentCohortByIdAsync( id );

                Assert.IsNotNull( expected );
                Assert.IsNotNull( actual );
                Assert.AreEqual( expected.Id, actual.Id );
                Assert.AreEqual( expected.Code, actual.Code );
                Assert.AreEqual( expected.Description, actual.Description );
                Assert.AreEqual( expected.Title, actual.Title );
            }

            [TestMethod]
            [ExpectedException( typeof( HttpResponseException ) )]
            public async Task StudentCohortsController_GetAll_Exception()
            {
                studentServiceMock.Setup( i => i.GetAllStudentCohortsAsync( It.IsAny<Dtos.Filters.CodeItemFilter>(), It.IsAny<bool>() ) ).ThrowsAsync( new Exception() );

                var actuals = await studentCohortsController.GetStudentCohortsAsync(It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentCohortsController_GetById_Exception()
            {
                string id = "f05a6c0f-3a56-4a87-b931-bc2901da5ef9";
                studentServiceMock.Setup(i => i.GetStudentCohortByGuidAsync(id, It.IsAny<bool>())).ThrowsAsync(new Exception());

                var actual = await studentCohortsController.GetStudentCohortByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentCohortsController_GetById_KeyNotFoundException()
            {
                string id = "f05a6c0f-3a56-4a87-b931-bc2901da5ef9";
                studentServiceMock.Setup(i => i.GetStudentCohortByGuidAsync(id, It.IsAny<bool>())).ThrowsAsync(new KeyNotFoundException());

                var actual = await studentCohortsController.GetStudentCohortByIdAsync(id);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentCohortsController_PUT_Not_Supported()
            {
                var actual = await studentCohortsController.PutStudentCohortAsync(It.IsAny<string>(), It.IsAny<Dtos.StudentCohort>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentCohortsController_POST_Not_Supported()
            {
                var actual = await studentCohortsController.PostStudentCohortAsync(It.IsAny<Dtos.StudentCohort>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentCohortsController_DELETE_Not_Supported()
            {
                await studentCohortsController.DeleteStudentCohortAsync(It.IsAny<string>());
            }
          
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentCohortsController_GetByIdThrowsIntAppiPermissionExc()
            {
                studentServiceMock.Setup(gc => gc.GetStudentCohortByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
                await studentCohortsController.GetStudentCohortByIdAsync("invalid");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentCohortsController_GetByIdThrowsIntAppiKeyNotFoundExc()
            {
                studentServiceMock.Setup(gc => gc.GetStudentCohortByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<KeyNotFoundException>();
                await studentCohortsController.GetStudentCohortByIdAsync("invalid");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentCohortsController_GetByIdThrowsIntAppiIntegrationExc()
            {
                studentServiceMock.Setup(gc => gc.GetStudentCohortByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
                await studentCohortsController.GetStudentCohortByIdAsync("invalid");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentCohortsController_GetByIdThrowsIntAppiArgumentExc()
            {
                studentServiceMock.Setup(gc => gc.GetStudentCohortByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();
                await studentCohortsController.GetStudentCohortByIdAsync("invalid");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task studentCohortsController_GetByIdThrowsIntAppiRepositoryExc()
            {
                studentServiceMock.Setup(gc => gc.GetStudentCohortByGuidAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
                await studentCohortsController.GetStudentCohortByIdAsync("invalid");
            }

            [TestMethod]
            [ExpectedException( typeof( HttpResponseException ) )]
            public async Task studentCohortsController_GetThrowsIntAppiPermissionExc()
            {
                studentServiceMock.Setup( gc => gc.GetAllStudentCohortsAsync( It.IsAny<Dtos.Filters.CodeItemFilter>(), It.IsAny<bool>() ) ).Throws<PermissionsException>();
                await studentCohortsController.GetStudentCohortsAsync(It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException( typeof( HttpResponseException ) )]
            public async Task studentCohortsController_GetThrowsIntAppiKeyNotFoundExc()
            {
                studentServiceMock.Setup( gc => gc.GetAllStudentCohortsAsync( It.IsAny<Dtos.Filters.CodeItemFilter>(), It.IsAny<bool>() ) ).Throws<KeyNotFoundException>();
                await studentCohortsController.GetStudentCohortsAsync(It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException( typeof( HttpResponseException ) )]
            public async Task studentCohortsController_GetThrowsIntAppiIntegrationExc()
            {
                studentServiceMock.Setup( gc => gc.GetAllStudentCohortsAsync( It.IsAny<Dtos.Filters.CodeItemFilter>(), It.IsAny<bool>() ) ).Throws<IntegrationApiException>();
                await studentCohortsController.GetStudentCohortsAsync(It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException( typeof( HttpResponseException ) )]
            public async Task studentCohortsController_GetThrowsIntAppiArgumentExc()
            {
                studentServiceMock.Setup( gc => gc.GetAllStudentCohortsAsync( It.IsAny<Dtos.Filters.CodeItemFilter>(), It.IsAny<bool>() ) ).Throws<ArgumentException>();
                await studentCohortsController.GetStudentCohortsAsync(It.IsAny<QueryStringFilter>());
            }

            [TestMethod]
            [ExpectedException( typeof( HttpResponseException ) )]
            public async Task studentCohortsController_GetThrowsIntAppiRepositoryExc()
            {
                studentServiceMock.Setup( gc => gc.GetAllStudentCohortsAsync( It.IsAny<Dtos.Filters.CodeItemFilter>(), It.IsAny<bool>() ) ).Throws<RepositoryException>();
                await studentCohortsController.GetStudentCohortsAsync(It.IsAny<QueryStringFilter>()); ;
            }
        }
    }
}