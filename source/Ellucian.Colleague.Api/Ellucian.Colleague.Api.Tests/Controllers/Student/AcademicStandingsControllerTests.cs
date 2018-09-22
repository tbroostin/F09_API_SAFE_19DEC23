// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AcademicStandingsControllerTests
    {
        [TestClass]
        public class GET
        {
            /// <summary>
            ///     Gets or sets the test context which provides
            ///     information about and functionality for the current test run.
            /// </summary>
            public TestContext TestContext { get; set; }

            Mock<IAcademicStandingsService> academicStandingsServiceMock;
            Mock<IStudentReferenceDataRepository> studentReferenceDataRepoMock;
            Mock<IAdapterRegistry> adapterRegistryMock;
            Mock<ILogger> loggerMock;

            AcademicStandingsController academicStandingsController;
            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.AcademicStanding2> academicStandingsEntities;
            List<Dtos.AcademicStanding> academicStandingsDto = new List<Dtos.AcademicStanding>();

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(Path.Combine(TestContext.DeploymentDirectory, "App_Data"));
                studentReferenceDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                academicStandingsServiceMock = new Mock<IAcademicStandingsService>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                loggerMock = new Mock<ILogger>();

                academicStandingsEntities = new TestStudentReferenceDataRepository().GetAcademicStandings2Async(false).Result;
                foreach (var academicStandingsEntity in academicStandingsEntities)
                {
                    Dtos.AcademicStanding academicStandingDto = new Dtos.AcademicStanding();
                    academicStandingDto.Id = academicStandingsEntity.Guid;
                    academicStandingDto.Code = academicStandingsEntity.Code;
                    academicStandingDto.Title = academicStandingsEntity.Description;
                    academicStandingDto.Description = string.Empty;
                    academicStandingsDto.Add(academicStandingDto);
                }

                academicStandingsController = new AcademicStandingsController(adapterRegistryMock.Object, academicStandingsServiceMock.Object, studentReferenceDataRepoMock.Object, loggerMock.Object) { Request = new HttpRequestMessage() };
                academicStandingsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey,
                new HttpConfiguration());               
            }

            [TestCleanup]
            public void Cleanup()
            {
                academicStandingsController = null;
                academicStandingsEntities = null;
                academicStandingsDto = null;
            }

            [TestMethod]
            public async Task AcademicStandingsController_GetAll_NoCache_True()
            {
                academicStandingsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    Public = true
                };

                academicStandingsServiceMock.Setup(ac => ac.GetAcademicStandingsAsync(It.IsAny<bool>())).ReturnsAsync(academicStandingsDto);

                var results = await academicStandingsController.GetAcademicStandingsAsync();
                Assert.AreEqual(academicStandingsDto.Count, results.Count());

                foreach (var academicStandingDto in academicStandingsDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == academicStandingDto.Id);
                    Assert.AreEqual(result.Id, academicStandingDto.Id);
                    Assert.AreEqual(result.Code, academicStandingDto.Code);
                    Assert.AreEqual(result.Title, academicStandingDto.Title);
                    Assert.AreEqual(result.Description, academicStandingDto.Description);
                }
            }

            [TestMethod]
            public async Task AcademicStandingsController_GetAll_NoCache_False()
            {
                academicStandingsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = false,
                    Public = true
                };

                academicStandingsServiceMock.Setup(ac => ac.GetAcademicStandingsAsync(It.IsAny<bool>())).ReturnsAsync(academicStandingsDto);

                var results = await academicStandingsController.GetAcademicStandingsAsync();
                Assert.AreEqual(academicStandingsDto.Count, results.Count());                
                
                foreach (var academicStandingDto in academicStandingsDto)
                {
                    var result = results.FirstOrDefault(i => i.Id == academicStandingDto.Id);
                    Assert.AreEqual(result.Id, academicStandingDto.Id);
                    Assert.AreEqual(result.Code, academicStandingDto.Code);
                    Assert.AreEqual(result.Title, academicStandingDto.Title);
                    Assert.AreEqual(result.Description, academicStandingDto.Description);
                }

            }

            [TestMethod]
            public async Task AcademicStandingsController_GetById()
            {
                string id = "9c3b805d-cfe6-483b-86c3-4c20562f8c15";
                var academicStandingDto = academicStandingsDto.FirstOrDefault(i => i.Id == id);
                academicStandingsController.Request.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = It.IsAny<bool>(),
                    Public = true
                };

                academicStandingsServiceMock.Setup(ac => ac.GetAcademicStandingByIdAsync(id)).ReturnsAsync(academicStandingDto);

                var result = await academicStandingsController.GetAcademicStandingByIdAsync(id);
                Assert.AreEqual(result.Id, academicStandingDto.Id);
                Assert.AreEqual(result.Code, academicStandingDto.Code);
                Assert.AreEqual(result.Title, academicStandingDto.Title);
                Assert.AreEqual(result.Description, academicStandingDto.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AcademicStandingsController_GetAll_Exception()
            {
                academicStandingsServiceMock.Setup(ac => ac.GetAcademicStandingsAsync(It.IsAny<bool>())).ThrowsAsync(new Exception());
                var result = await academicStandingsController.GetAcademicStandingsAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AcademicStandingsController_GetById_KeyNotFoundException()
            {
                academicStandingsServiceMock.Setup(ac => ac.GetAcademicStandingByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                var result = await academicStandingsController.GetAcademicStandingByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AcademicStandingsController_GetById_Exception()
            {
                academicStandingsServiceMock.Setup(ac => ac.GetAcademicStandingByIdAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
                var result = await academicStandingsController.GetAcademicStandingByIdAsync(It.IsAny<string>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AcademicStandingsController_PUT_Exception()
            {
                var result = await academicStandingsController.PutAcademicStandingAsync(It.IsAny<string>(), new Dtos.AcademicStanding());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AcademicStandingsController_POST_Exception()
            {
                var result = await academicStandingsController.PostAcademicStandingAsync(new Dtos.AcademicStanding());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AcademicStandingsController_DELETE_Exception()
            {
                await academicStandingsController.DeleteAcademicStandingAsync(It.IsAny<string>());
            }
        }
    }
}
