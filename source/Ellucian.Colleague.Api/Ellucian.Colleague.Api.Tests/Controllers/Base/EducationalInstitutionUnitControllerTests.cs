// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Colleague.Coordination.Base.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Tests;
using System.Net.Http;
using System.Web.Http.Hosting;
using System.Web.Http;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Http.Models;

namespace Ellucian.Colleague.Api.Tests.Controllers.Base
{
    [TestClass]
    public class EducationalInstitutionUnitsControllerTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        private Mock<IEducationalInstitutionUnitsService> educationalInstitutionUnitsServiceMock;
        private Mock<ILogger> loggerMock;

        private EducationalInstitutionUnitsController educationalInstitutionUnitsController;
        private List<Dtos.EducationalInstitutionUnits> educationalInstitutionUnitsCollection;

        private const string departmentGuid = "6d6040a5-1a98-4614-943d-ad20101ff057"; //BIOLOGY

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            educationalInstitutionUnitsServiceMock = new Mock<IEducationalInstitutionUnitsService>();
            loggerMock = new Mock<ILogger>();

            educationalInstitutionUnitsCollection = new List<Dtos.EducationalInstitutionUnits>();

            var allDepartments = new TestDepartmentRepository().Get();
            var allDivisions = new TestDivisionRepository().GetDivisions();
            var allSchools = new TestSchoolRepository().GetSchools();

            foreach (var source in allDepartments)
            {
                var department = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits
                {
                    Id = source.Guid,
                    EducationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                    Title = source.Description,
                    Description = null                
                };
                educationalInstitutionUnitsCollection.Add(department);
            }

            educationalInstitutionUnitsController = new EducationalInstitutionUnitsController(educationalInstitutionUnitsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            educationalInstitutionUnitsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            educationalInstitutionUnitsController = null;
            educationalInstitutionUnitsCollection = null;
            loggerMock = null;
            educationalInstitutionUnitsServiceMock = null;
        }

        #region EducationalInstitutionUnits

        [TestMethod]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_CommentSubjectArea()
        {
            educationalInstitutionUnitsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expectedCollection = educationalInstitutionUnitsCollection.Where(x => x.Id == departmentGuid).ToList();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(expectedCollection);

            var educationalInstitutionUnits = (await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync()).ToList();
            Assert.AreEqual(expectedCollection.Count, educationalInstitutionUnits.Count);
            for (var i = 0; i < educationalInstitutionUnits.Count; i++)
            {
                var expected = expectedCollection[i];
                var actual = educationalInstitutionUnits[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
                     }
        }

       
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_PermissionsException()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<PermissionsException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_ArgumentException()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<ArgumentException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_RepositoryException()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<RepositoryException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_IntegrationApiException()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<IntegrationApiException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync();
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_Exception()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(It.IsAny<string>(), It.IsAny<bool>())).Throws<Exception>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync();
        }

        #endregion GetEducationalInstitutionUnits

        #region GetEducationalInstitutionUnitsByGuid

        [TestMethod]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuidAsync(expected.Id)).ReturnsAsync(expected);

            var actual = (await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuidAsync(expected.Id));
            Assert.AreEqual(expected.Id, actual.Id, "Id");
               }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid_NullArgument()
        {
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid_EmptyArgument()
        {
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid_PermissionsException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuidAsync(expected.Id)).Throws<PermissionsException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuidAsync(expected.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid_ArgumentException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuidAsync(expected.Id)).Throws<ArgumentException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid_RepositoryException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuidAsync(expected.Id)).Throws<RepositoryException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid_IntegrationApiException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuidAsync(expected.Id)).Throws<IntegrationApiException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuidAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid_Exception()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuidAsync(expected.Id)).Throws<Exception>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuidAsync(expected.Id);
        }

        #endregion GetEducationalInstitutionUnitsByGuid

        #region GetEducationalInstitutionUnitsByType

        [TestMethod]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByType()
        {
            var educationalInstitutionUnits = educationalInstitutionUnitsCollection
                .Where(x => x.EducationalInstitutionUnitType == Dtos.EnumProperties.EducationalInstitutionUnitType.Department).ToList();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync("department", It.IsAny<bool>())).ReturnsAsync(educationalInstitutionUnits);

            var educationalInstitutionUnitsByType = (await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync("department")).ToList();
            for (var i = 0; i < educationalInstitutionUnits.Count; i++)
            {
                var expected = educationalInstitutionUnitsByType[i];
                var actual = educationalInstitutionUnits[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }
        
        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByType_PermissionsException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(expected.Id, It.IsAny<bool>())).Throws<PermissionsException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByType_ArgumentException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(expected.Id, It.IsAny<bool>())).Throws<ArgumentException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByType_RepositoryException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(expected.Id, It.IsAny<bool>())).Throws<RepositoryException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByType_IntegrationApiException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(expected.Id, It.IsAny<bool>())).Throws<IntegrationApiException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByType_Exception()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByTypeAsync(expected.Id, It.IsAny<bool>())).Throws<Exception>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsAsync(expected.Id);
        }

        #endregion GetEducationalInstitutionUnitsByType

        #region Put

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_PutEducationalInstitutionUnits_NullArgument()
        {
            await educationalInstitutionUnitsController.PutEducationalInstitutionUnitsAsync(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_PutEducationalInstitutionUnits_EmptyArgument()
        {
            await educationalInstitutionUnitsController.PutEducationalInstitutionUnitsAsync("", null);
        }
      
        #endregion

        #region Post

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_PostEducationalInstitutionUnits_NullArgument()
        {
            await educationalInstitutionUnitsController.PostEducationalInstitutionUnitsAsync(null);
        }
 
        #endregion

        #region Delete

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_DeleteEducationalInstitutionUnits_EmptyArgument()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            await educationalInstitutionUnitsController.DeleteEducationalInstitutionUnitsByGuidAsync("");
        }
 
        #endregion
    }

    [TestClass]
    public class EducationalInstitutionUnitsControllerTestsV12
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }
        private Mock<IEducationalInstitutionUnitsService> educationalInstitutionUnitsServiceMock;
        private Mock<ILogger> loggerMock;

        private EducationalInstitutionUnitsController educationalInstitutionUnitsController;
        private List<Dtos.EducationalInstitutionUnits3> educationalInstitutionUnitsCollection;
        private QueryStringFilter criteriaFilter = new QueryStringFilter("criteria", "");
        private QueryStringFilter departmentFilter = new QueryStringFilter("department", "");

        private const string departmentGuid = "6d6040a5-1a98-4614-943d-ad20101ff057"; //BIOLOGY

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.DeploymentDirectory, "App_Data"));

            educationalInstitutionUnitsServiceMock = new Mock<IEducationalInstitutionUnitsService>();
            loggerMock = new Mock<ILogger>();

            educationalInstitutionUnitsCollection = new List<Dtos.EducationalInstitutionUnits3>();

            var allDepartments = new TestDepartmentRepository().Get();
            var allDivisions = new TestDivisionRepository().GetDivisions();
            var allSchools = new TestSchoolRepository().GetSchools();

            foreach (var source in allDepartments)
            {
                var department = new Ellucian.Colleague.Dtos.EducationalInstitutionUnits3
                {
                    Id = source.Guid,
                    Type = Dtos.EnumProperties.EducationalInstitutionUnitType.Department,
                    Title = source.Description,
                    Description = null
                };
                educationalInstitutionUnitsCollection.Add(department);
            }

            educationalInstitutionUnitsController = new EducationalInstitutionUnitsController(educationalInstitutionUnitsServiceMock.Object, loggerMock.Object)
            {
                Request = new HttpRequestMessage()
            };
            educationalInstitutionUnitsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup()
        {
            educationalInstitutionUnitsController = null;
            educationalInstitutionUnitsCollection = null;
            loggerMock = null;
            educationalInstitutionUnitsServiceMock = null;
        }

        #region EducationalInstitutionUnits

        [TestMethod]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits()
        {
            educationalInstitutionUnitsController.Request.Headers.CacheControl =
                 new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = false };

            var expectedCollection = educationalInstitutionUnitsCollection.Where(x => x.Id == departmentGuid).ToList();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnits3Async(It.IsAny<bool>(), 
                It.IsAny<Dtos.EnumProperties.EducationalInstitutionUnitType>(), 
                It.IsAny<Dtos.EnumProperties.Status>())).ReturnsAsync(expectedCollection);

            var educationalInstitutionUnits = (await educationalInstitutionUnitsController.GetEducationalInstitutionUnits3Async(criteriaFilter, departmentFilter)).ToList();
            Assert.AreEqual(expectedCollection.Count, educationalInstitutionUnits.Count);
            for (var i = 0; i < educationalInstitutionUnits.Count; i++)
            {
                var expected = expectedCollection[i];
                var actual = educationalInstitutionUnits[i];
                Assert.AreEqual(expected.Id, actual.Id, "Id, Index=" + i.ToString());
            }
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_PermissionsException()
        {
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnits3Async(It.IsAny<bool>(),
                It.IsAny<Dtos.EnumProperties.EducationalInstitutionUnitType>(),
                It.IsAny<Dtos.EnumProperties.Status>())).Throws<PermissionsException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnits3Async(criteriaFilter, departmentFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_ArgumentException()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnits3Async(It.IsAny<bool>(),
                It.IsAny<Dtos.EnumProperties.EducationalInstitutionUnitType>(),
                It.IsAny<Dtos.EnumProperties.Status>())).Throws<ArgumentException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnits3Async(criteriaFilter, departmentFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_RepositoryException()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnits3Async(It.IsAny<bool>(),
                It.IsAny<Dtos.EnumProperties.EducationalInstitutionUnitType>(),
                It.IsAny<Dtos.EnumProperties.Status>())).Throws<RepositoryException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnits3Async(criteriaFilter, departmentFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits_IntegrationApiException()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnits3Async(It.IsAny<bool>(),
                It.IsAny<Dtos.EnumProperties.EducationalInstitutionUnitType>(),
                It.IsAny<Dtos.EnumProperties.Status>())).Throws<IntegrationApiException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnits3Async(criteriaFilter, departmentFilter);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnits3_Exception()
        {

            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnits3Async(It.IsAny<bool>(),
                It.IsAny<Dtos.EnumProperties.EducationalInstitutionUnitType>(),
                It.IsAny<Dtos.EnumProperties.Status>())).Throws<Exception>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnits3Async(criteriaFilter, departmentFilter);
        }

        #endregion GetEducationalInstitutionUnits

        #region GetEducationalInstitutionUnitsByGuid

        [TestMethod]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid3()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuid3Async(expected.Id, It.IsAny<bool>())).ReturnsAsync(expected);

            var actual = (await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuid3Async(expected.Id));
            Assert.AreEqual(expected.Id, actual.Id, "Id");
        }


        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid3_NullArgument()
        {
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuid3Async(null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid3_EmptyArgument()
        {
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuid3Async("");
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid3_PermissionsException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuid3Async(expected.Id, It.IsAny<bool>())).Throws<PermissionsException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuid3Async(expected.Id);

        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid3_ArgumentException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuid3Async(expected.Id, It.IsAny<bool>())).Throws<ArgumentException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuid3Async(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid3_RepositoryException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuid3Async(expected.Id, It.IsAny<bool>())).Throws<RepositoryException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuid3Async(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid3_IntegrationApiException()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuid3Async(expected.Id, It.IsAny<bool>())).Throws<IntegrationApiException>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuid3Async(expected.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task EducationalInstitutionUnitsController_GetEducationalInstitutionUnitsByGuid3_Exception()
        {
            var expected = educationalInstitutionUnitsCollection.FirstOrDefault();
            educationalInstitutionUnitsServiceMock.Setup(x => x.GetEducationalInstitutionUnitsByGuid3Async(expected.Id, It.IsAny<bool>())).Throws<Exception>();
            await educationalInstitutionUnitsController.GetEducationalInstitutionUnitsByGuid3Async(expected.Id);
        }

        #endregion GetEducationalInstitutionUnitsByGuid

    }
}