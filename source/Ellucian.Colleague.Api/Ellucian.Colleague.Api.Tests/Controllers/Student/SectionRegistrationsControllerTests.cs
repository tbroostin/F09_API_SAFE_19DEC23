// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Web.Http;
using Ellucian.Web.Security;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Web.Http.Models;
using System.Web.Http.Hosting;
using Newtonsoft.Json.Linq;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class SectionRegistrationControllerTests
    {
        [TestClass]
        public class Get
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration2> allSectionRegistrationsDtos;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration3> allSectionRegistrations3Dtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                sectionRegistrationServiceMock.Setup(s => s.GetDataPrivacyListByApi(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new List<string>());

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations();
                allSectionRegistrations3Dtos = SectionRegistrationControllerTests.BuildSectionRegistrations3();
                string guid = allSectionRegistrationsDtos.ElementAt(0).Id;

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }


            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrationsAsync()
            {
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration2>, int>(allSectionRegistrationsDtos, 5);

                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrationsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var sectionRegistrations = await sectionRegistrationsController.GetSectionRegistrationsAsync(new Paging(10, 0), "", "");
               
                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionRegistrations.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.SectionRegistration2> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration2>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionRegistration2>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(sectionRegistrations is IHttpActionResult);

                foreach (var sectionRegistrationsDto in allSectionRegistrationsDtos)
                {
                    var sectReg = results.FirstOrDefault(i => i.Id == sectionRegistrationsDto.Id);

                    Assert.AreEqual(sectionRegistrationsDto.Id, sectReg.Id);
                    Assert.AreEqual(sectionRegistrationsDto.AwardGradeScheme, sectReg.AwardGradeScheme);
                    Assert.AreEqual(sectionRegistrationsDto.Section.Id, sectReg.Section.Id);
                    
                }
            }

            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrations2Async()
            {
                sectionRegistrationsController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };

                var tuple = new Tuple<IEnumerable<Dtos.SectionRegistration3>, int>(allSectionRegistrations3Dtos, 5);

                sectionRegistrationServiceMock.Setup(s => s.GetSectionRegistrations2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(tuple);
                var sectionRegistrations = await sectionRegistrationsController.GetSectionRegistrations2Async(new Paging(10, 0), "", "");

                var cancelToken = new System.Threading.CancellationToken(false);

                System.Net.Http.HttpResponseMessage httpResponseMessage = await sectionRegistrations.ExecuteAsync(cancelToken);

                IEnumerable<Dtos.SectionRegistration3> results = ((ObjectContent<IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration3>>)httpResponseMessage.Content).Value as IEnumerable<Dtos.SectionRegistration3>;

                var result = results.FirstOrDefault();

                Assert.IsTrue(sectionRegistrations is IHttpActionResult);

                foreach (var sectionRegistrationsDto in allSectionRegistrations3Dtos)
                {
                    var sectReg = results.FirstOrDefault(i => i.Id == sectionRegistrationsDto.Id);

                    Assert.AreEqual(sectionRegistrationsDto.Id, sectReg.Id);
                    Assert.AreEqual(sectionRegistrationsDto.AwardGradeScheme, sectReg.AwardGradeScheme);
                    Assert.AreEqual(sectionRegistrationsDto.Section.Id, sectReg.Section.Id);

                }
            }


            [TestMethod]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync()
            {
                string guid = allSectionRegistrationsDtos.ElementAt(0).Id;
                sectionRegistrationServiceMock.Setup(x => x.GetSectionRegistrationAsync(guid)).Returns(Task.FromResult(allSectionRegistrationsDtos.ElementAt(0)));
                var sectionRegistration = await sectionRegistrationsController.GetSectionRegistrationAsync(guid);
                Assert.AreEqual(sectionRegistration.Id, allSectionRegistrationsDtos.ElementAt(0).Id);
                Assert.AreEqual(sectionRegistration.AwardGradeScheme.Id, allSectionRegistrationsDtos.ElementAt(0).AwardGradeScheme.Id);
                Assert.AreEqual(sectionRegistration.Section.Id, allSectionRegistrationsDtos.ElementAt(0).Section.Id);
            }

            #region Exception Tests
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_GetSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.GetSectionRegistrationAsync("asdf"))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.GetSectionRegistrationAsync("asdf");
            }
            #endregion

        }

        [TestClass]
        public class Put
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration2> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations();
                Dtos.SectionRegistration2 registration = allSectionRegistrationsDtos.ElementAt(0);

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                sectionRegistrationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(registration));
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task UpdatesSectionRegistrationByGuid()
            {
                Dtos.SectionRegistration2 registration = allSectionRegistrationsDtos.ElementAt(0);
                string guid = registration.Id;
                sectionRegistrationServiceMock.Setup(x => x.UpdateSectionRegistrationAsync(guid, It.IsAny<SectionRegistration2>())).ReturnsAsync(registration);
                sectionRegistrationServiceMock.Setup(x => x.GetSectionRegistrationAsync(guid)).ReturnsAsync(registration);
                var result = await sectionRegistrationsController.PutSectionRegistrationAsync(guid, registration);
                Assert.AreEqual(result.Id, registration.Id);
                Assert.AreEqual(result.Registrant, registration.Registrant);
                Assert.AreEqual(result.Section, registration.Section);
                Assert.AreEqual(result.Status.RegistrationStatus, registration.Status.RegistrationStatus);
            }                   
            #region Exception Test PUT
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }
            #endregion

        }

        [TestClass]
        public class Put_V7
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration3> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations3();
                Dtos.SectionRegistration3 registration = allSectionRegistrationsDtos.ElementAt(0);

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger)
                {
                    Request = new HttpRequestMessage()
                };
                sectionRegistrationsController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
                sectionRegistrationsController.Request.Properties.Add("PartialInputJsonObject", JObject.FromObject(registration));
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task UpdatesSectionRegistrationByGuid()
            {
                Dtos.SectionRegistration3 registration = allSectionRegistrationsDtos.ElementAt(0);
                string guid = registration.Id;
                sectionRegistrationServiceMock.Setup(x => x.UpdateSectionRegistration2Async(guid, It.IsAny<SectionRegistration3>())).ReturnsAsync(registration);
                sectionRegistrationServiceMock.Setup(x => x.GetSectionRegistration2Async(guid)).ReturnsAsync(registration);
                var result = await sectionRegistrationsController.PutSectionRegistration2Async(guid, registration);
                Assert.AreEqual(result.Id, registration.Id);
                Assert.AreEqual(result.Registrant, registration.Registrant);
                Assert.AreEqual(result.Section, registration.Section);
                Assert.AreEqual(result.Status.RegistrationStatus, registration.Status.RegistrationStatus);
                Assert.AreEqual(result.AcademicLevel, registration.AcademicLevel);
            }
            #region Exception Test PUT
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PutSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.UpdateSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PutSectionRegistrationAsync("asdf", It.IsAny<SectionRegistration2>());
            }
            #endregion

        }

        [TestClass]
        public class Post
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration2> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations();

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task CreatesSectionRegistration()
            {
                Dtos.SectionRegistration2 registration = allSectionRegistrationsDtos.ElementAt(0);
                sectionRegistrationServiceMock.Setup(x => x.CreateSectionRegistrationAsync(registration)).ReturnsAsync(allSectionRegistrationsDtos.ElementAt(0));

                var sectionRegistration = await sectionRegistrationsController.PostSectionRegistrationAsync(registration);
                Assert.AreEqual(sectionRegistration.Id, registration.Id);
                Assert.AreEqual(sectionRegistration.Registrant, registration.Registrant);
                Assert.AreEqual(sectionRegistration.Section, registration.Section);
                Assert.AreEqual(sectionRegistration.Status.RegistrationStatus, registration.Status.RegistrationStatus);
            }

            #region Exception Test POST
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_FormatException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new FormatException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }
            #endregion
        }

        [TestClass]
        public class PostV7
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

            private SectionRegistrationsController sectionRegistrationsController;

            private Mock<ISectionRegistrationService> sectionRegistrationServiceMock;
            private ISectionRegistrationService sectionRegistrationService;
            private IStudentReferenceDataRepository studentReferenceDataRepository;

            private IAdapterRegistry AdapterRegistry = null;

            private IEnumerable<Ellucian.Colleague.Dtos.SectionRegistration3> allSectionRegistrationsDtos;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                sectionRegistrationServiceMock = new Mock<ISectionRegistrationService>();
                sectionRegistrationService = sectionRegistrationServiceMock.Object;

                allSectionRegistrationsDtos = SectionRegistrationControllerTests.BuildSectionRegistrations3();

                sectionRegistrationsController = new SectionRegistrationsController(AdapterRegistry, studentReferenceDataRepository, sectionRegistrationService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRegistrationsController = null;
                sectionRegistrationService = null;
                studentReferenceDataRepository = null;
            }

            [TestMethod]
            public async Task CreatesSectionRegistration()
            {
                Dtos.SectionRegistration3 registration = allSectionRegistrationsDtos.ElementAt(0);
                sectionRegistrationServiceMock.Setup(x => x.CreateSectionRegistration2Async(registration)).ReturnsAsync(allSectionRegistrationsDtos.ElementAt(0));

                var sectionRegistration = await sectionRegistrationsController.PostSectionRegistration2Async(registration);
                Assert.AreEqual(sectionRegistration.Id, registration.Id);
                Assert.AreEqual(sectionRegistration.Registrant, registration.Registrant);
                Assert.AreEqual(sectionRegistration.Section, registration.Section);
                Assert.AreEqual(sectionRegistration.Status.RegistrationStatus, registration.Status.RegistrationStatus);
                Assert.AreEqual(sectionRegistration.AcademicLevel, registration.AcademicLevel);

            }

            #region Exception Test POST
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_PermissionsException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new PermissionsException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_KeyNotFoundException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new KeyNotFoundException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentNullException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentNullException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentOutOfRangeException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentOutOfRangeException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ArgumentException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ArgumentException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_IntegrationApiException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new IntegrationApiException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_InvalidOperationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new InvalidOperationException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_FormatException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new FormatException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_ConfigurationException()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new ConfigurationException());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task SectionRegistrationsController_PostSectionRegistrationAsync_Exception()
            {
                sectionRegistrationServiceMock
                    .Setup(s => s.CreateSectionRegistrationAsync(It.IsAny<SectionRegistration2>()))
                    .ThrowsAsync(new Exception());
                await sectionRegistrationsController.PostSectionRegistrationAsync(It.IsAny<SectionRegistration2>());
            }
            #endregion
        }


        internal static IEnumerable<Dtos.SectionRegistration2> BuildSectionRegistrations()
        {
            var sectionRegistrationDtos = new List<Dtos.SectionRegistration2>();

            var sectionRegistrationDto1 = new Dtos.SectionRegistration2()
            {
                Id = "abcdefghijklmnop",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                Registrant = new Dtos.GuidObject2() { Id = "abc123def456" },
                Status = new Dtos.SectionRegistrationStatus2()
                {
                    RegistrationStatus = Dtos.RegistrationStatus2.Registered,
                    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Registered
                },
                Transcript = new Dtos.SectionRegistrationTranscript()
                {
                    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                    Mode = Dtos.TranscriptMode.Standard
                },
                AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" }
            };

            var sectionRegistrationDto2 = new Dtos.SectionRegistration2()
            {
                Id = "a1b2c383748akdfj817382",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                Registrant = new Dtos.GuidObject2() { Id = "abc123def456" },
                Status = new Dtos.SectionRegistrationStatus2()
                {
                    RegistrationStatus = Dtos.RegistrationStatus2.NotRegistered,
                    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Dropped
                },
                Transcript = new Dtos.SectionRegistrationTranscript()
                {
                    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                    Mode = Dtos.TranscriptMode.Standard
                },
                AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" }
            };

            sectionRegistrationDtos.Add(sectionRegistrationDto1);
            sectionRegistrationDtos.Add(sectionRegistrationDto2);

            return sectionRegistrationDtos;
        }



        internal static IEnumerable<Dtos.SectionRegistration3> BuildSectionRegistrations3()
        {
            var sectionRegistrationDtos = new List<Dtos.SectionRegistration3>();

            var sectionRegistrationDto1 = new Dtos.SectionRegistration3()
            {
                Id = "abcdefghijklmnop",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                Registrant = new Dtos.GuidObject2() { Id = "abc123def456" },
                Status = new Dtos.SectionRegistrationStatus2()
                {
                    RegistrationStatus = Dtos.RegistrationStatus2.Registered,
                    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Registered
                },
                Transcript = new Dtos.SectionRegistrationTranscript()
                {
                    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                    Mode = Dtos.TranscriptMode.Standard
                },
                AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                AcademicLevel = new GuidObject2(){ Id = "qwertyuiop" },
            };

            var sectionRegistrationDto2 = new Dtos.SectionRegistration3()
            {
                Id = "a1b2c383748akdfj817382",
                Section = new Dtos.GuidObject2() { Id = "12345678910" },
                Registrant = new Dtos.GuidObject2() { Id = "abc123def456" },
                Status = new Dtos.SectionRegistrationStatus2()
                {
                    RegistrationStatus = Dtos.RegistrationStatus2.NotRegistered,
                    SectionRegistrationStatusReason = Dtos.RegistrationStatusReason2.Dropped
                },
                Transcript = new Dtos.SectionRegistrationTranscript()
                {
                    GradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                    Mode = Dtos.TranscriptMode.Standard
                },
                AwardGradeScheme = new Dtos.GuidObject2() { Id = "098975864tgu17637ajhdf" },
                AcademicLevel = new GuidObject2() { Id = "qwertyuiop" },
            };

            sectionRegistrationDtos.Add(sectionRegistrationDto1);
            sectionRegistrationDtos.Add(sectionRegistrationDto2);

            return sectionRegistrationDtos;
        }
    }
}
