// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Ellucian.Colleague.Domain.Base.Exceptions;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class AddAuthorizationsControllerTests
    {

        [TestClass]
        public class AddAuthorizationsControllerTests_PutAddAuthorizationAsync
        {
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
            private IAddAuthorizationService addAuthorizationService;
            private Mock<IAddAuthorizationService> addAuthorizationServiceMock;
            private AddAuthorizationsController addAuthorizationsController;
            private Ellucian.Colleague.Dtos.Student.AddAuthorization addAuthorizationDto;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                addAuthorizationServiceMock = new Mock<IAddAuthorizationService>();
                addAuthorizationService = addAuthorizationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                addAuthorizationDto = new Ellucian.Colleague.Dtos.Student.AddAuthorization();
                addAuthorizationDto.StudentId = "studentId";
                addAuthorizationDto.SectionId = "sectionId";
                addAuthorizationDto.Id = "authID";
                addAuthorizationDto.AddAuthorizationCode = "addCode";
                addAuthorizationDto.AssignedBy = "facultyId";

                addAuthorizationsController = new AddAuthorizationsController(addAuthorizationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                addAuthorizationsController = null;
                addAuthorizationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAddAuthorization_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.UpdateAddAuthorizationAsync(addAuthorizationDto)).Throws(new PermissionsException());
                    var addAuthorization = await addAuthorizationsController.PutAddAuthorizationAsync(addAuthorizationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAddAuthorization_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.UpdateAddAuthorizationAsync(addAuthorizationDto)).Throws(new KeyNotFoundException());
                    var addAuthorization = await addAuthorizationsController.PutAddAuthorizationAsync(addAuthorizationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PutAddAuthorization_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.UpdateAddAuthorizationAsync(addAuthorizationDto)).Throws(new ArgumentException());
                    var addAuthorization = await addAuthorizationsController.PutAddAuthorizationAsync(addAuthorizationDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

        }

        [TestClass]
        public class AddAuthorizationsControllerTests_GetSectionAddAuthorizationsAsync
        {
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
            private IAddAuthorizationService addAuthorizationService;
            private Mock<IAddAuthorizationService> addAuthorizationServiceMock;
            private AddAuthorizationsController addAuthorizationsController;
            private List<Dtos.Student.AddAuthorization> addAuthorizationDtos;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private string sectionId;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                addAuthorizationServiceMock = new Mock<IAddAuthorizationService>();
                addAuthorizationService = addAuthorizationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                addAuthorizationDtos = new List<Dtos.Student.AddAuthorization>();
                sectionId = "section/Id";
                var addAuthorizationDto1 = new Ellucian.Colleague.Dtos.Student.AddAuthorization()
                {
                    StudentId = "studentId1",
                    SectionId = "section/Id",
                    Id = "authID1",
                    AddAuthorizationCode = "addCode",
                    AssignedBy = "facultyId"
                };
                addAuthorizationDtos.Add(addAuthorizationDto1);
                var addAuthorizationDto2 = new Ellucian.Colleague.Dtos.Student.AddAuthorization()
                {
                    StudentId = "studentId2",
                    SectionId = "section/Id",
                    Id = "authID2",
                    AddAuthorizationCode = "addCode",
                    AssignedBy = "facultyId"
                };
                addAuthorizationDtos.Add(addAuthorizationDto2);
                addAuthorizationsController = new AddAuthorizationsController(addAuthorizationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                addAuthorizationsController = null;
                addAuthorizationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionAddAuthorizationsAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.GetSectionAddAuthorizationsAsync(It.IsAny<string>())).Throws(new PermissionsException());
                    var response = await addAuthorizationsController.GetSectionAddAuthorizationsAsync(sectionId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionAddAuthorizationsAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.GetSectionAddAuthorizationsAsync(It.IsAny<string>())).Throws(new ArgumentException());
                    var response = await addAuthorizationsController.GetSectionAddAuthorizationsAsync(sectionId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task GetSectionAddAuthorizationsAsync_Success()
            {

                addAuthorizationServiceMock.Setup(x => x.GetSectionAddAuthorizationsAsync(It.IsAny<string>())).ReturnsAsync(addAuthorizationDtos);
                var response = await addAuthorizationsController.GetSectionAddAuthorizationsAsync(sectionId);
                Assert.AreEqual(addAuthorizationDtos.Count(), response.Count());
                foreach (var resultDto in response)
                {
                    var expectedDto = addAuthorizationDtos.Where(aa => aa.Id == resultDto.Id).FirstOrDefault();
                    Assert.AreEqual(expectedDto.StudentId, resultDto.StudentId);
                    Assert.AreEqual(expectedDto.SectionId, resultDto.SectionId);
                }


            }
        }

        [TestClass]
        public class AddAuthorizationsControllerTests_GetAsync
        {
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
            private IAddAuthorizationService addAuthorizationService;
            private Mock<IAddAuthorizationService> addAuthorizationServiceMock;
            private AddAuthorizationsController addAuthorizationsController;
            private Dtos.Student.AddAuthorization addAuthorizationDto;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private string authId;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                addAuthorizationServiceMock = new Mock<IAddAuthorizationService>();
                addAuthorizationService = addAuthorizationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                authId = "1111111";
                addAuthorizationDto = new Dtos.Student.AddAuthorization()
                {
                    StudentId = "studentId1",
                    SectionId = "section/Id",
                    Id = "1111111",
                    AddAuthorizationCode = "addCode",
                    AssignedBy = "facultyId",
                    AssignedTime = DateTime.Now.AddDays(-10),
                    IsRevoked = true,
                    RevokedBy = "Otherfaculty",
                    RevokedTime = DateTime.Now.AddDays(-1)
                };

                addAuthorizationsController = new AddAuthorizationsController(addAuthorizationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                addAuthorizationsController = null;
                addAuthorizationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AddAuthorizationsController_GetAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Throws(new PermissionsException());
                    var response = await addAuthorizationsController.GetAsync(authId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task AddAuthorizationsController_GetAsync_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).Throws(new ArgumentException());
                    var response = await addAuthorizationsController.GetAsync(authId);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task GAddAuthorizationsController_GetAsync_Success()
            {

                addAuthorizationServiceMock.Setup(x => x.GetAsync(It.IsAny<string>())).ReturnsAsync(addAuthorizationDto);
                var resultDto = await addAuthorizationsController.GetAsync(authId);

                Assert.AreEqual(addAuthorizationDto.StudentId, resultDto.StudentId);
                Assert.AreEqual(addAuthorizationDto.SectionId, resultDto.SectionId);
                Assert.AreEqual(addAuthorizationDto.Id, resultDto.Id);
                Assert.AreEqual(addAuthorizationDto.AddAuthorizationCode, resultDto.AddAuthorizationCode);
                Assert.AreEqual(addAuthorizationDto.AssignedBy, resultDto.AssignedBy);
                Assert.AreEqual(addAuthorizationDto.AssignedTime, resultDto.AssignedTime);
                Assert.AreEqual(addAuthorizationDto.IsRevoked, resultDto.IsRevoked);
                Assert.AreEqual(addAuthorizationDto.RevokedTime, resultDto.RevokedTime);
                Assert.AreEqual(addAuthorizationDto.RevokedBy, resultDto.RevokedBy);
            }
        }

        [TestClass]
        public class AddAuthorizationsControllerTests_PosAddAuthorizationAsync
        {
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
            private IAddAuthorizationService addAuthorizationService;
            private Mock<IAddAuthorizationService> addAuthorizationServiceMock;
            private AddAuthorizationsController addAuthorizationsController;
            private Dtos.Student.AddAuthorizationInput addAuthorizationInput;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                addAuthorizationServiceMock = new Mock<IAddAuthorizationService>();
                addAuthorizationService = addAuthorizationServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                addAuthorizationInput = new Dtos.Student.AddAuthorizationInput();
                addAuthorizationInput.StudentId = "studentId";
                addAuthorizationInput.SectionId = "sectionId";
                addAuthorizationInput.AssignedBy = "facultyId";
                addAuthorizationInput.AssignedTime = DateTime.Now.AddDays(-1);

                addAuthorizationsController = new AddAuthorizationsController(addAuthorizationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                addAuthorizationsController = null;
                addAuthorizationService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAddAuthorization_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.CreateAddAuthorizationAsync(addAuthorizationInput)).Throws(new PermissionsException());
                    var addAuthorization = await addAuthorizationsController.PostAddAuthorizationAsync(addAuthorizationInput);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAddAuthorization_AnyOtherException_ReturnsHttpResponseException_Conflict()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.CreateAddAuthorizationAsync(addAuthorizationInput)).Throws(new ExistingResourceException());
                    var addAuthorization = await addAuthorizationsController.PostAddAuthorizationAsync(addAuthorizationInput);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Conflict, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostAddAuthorization_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    addAuthorizationServiceMock.Setup(x => x.CreateAddAuthorizationAsync(addAuthorizationInput)).Throws(new ArgumentException());
                    var addAuthorization = await addAuthorizationsController.PostAddAuthorizationAsync(addAuthorizationInput);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

        }
    }
}