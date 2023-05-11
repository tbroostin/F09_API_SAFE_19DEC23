// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos.Student.InstantEnrollment;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class InstantEnrollmentControllerTest
    {
        [TestClass]
        public class PostProposedRegistrationForClassesAsyncTests
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

            private InstantEnrollmentController _ieController;
            private Mock<IInstantEnrollmentService> _ieServiceMock;
            private Mock<ICourseService> _courseServiceMock;
            private Mock<ILogger> _loggerMock;
            private InstantEnrollmentProposedRegistration _proposedRegistration;
            private InstantEnrollmentProposedRegistrationResult _proposedRegistrationResult;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                _loggerMock = new Mock<ILogger>();
                _courseServiceMock = new Mock<ICourseService>();
                _ieServiceMock = new Mock<IInstantEnrollmentService>();
                _ieController = new InstantEnrollmentController(_courseServiceMock.Object, _ieServiceMock.Object, _loggerMock.Object);

                _proposedRegistration = new InstantEnrollmentProposedRegistration();
                _proposedRegistration.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();
                _proposedRegistration.ProposedSections.Add(new InstantEnrollmentRegistrationBaseSectionToRegister()
                {
                    SectionId = "sect1"
                });

                _proposedRegistrationResult = new InstantEnrollmentProposedRegistrationResult();
                _proposedRegistrationResult.RegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
                _proposedRegistrationResult.RegisteredSections.Add(
                 new InstantEnrollmentRegistrationBaseRegisteredSection
                 {
                     SectionId = "sect1",
                     SectionCost = 0
                 });
            }

            [TestMethod]
            public async Task PostProposedRegistrationForClassesAsync_Valid_proposedRegistration_Success()
            {
                _ieServiceMock.Setup(ies => ies.ProposedRegistrationForClassesAsync(It.IsAny<InstantEnrollmentProposedRegistration>())).ReturnsAsync(_proposedRegistrationResult);
                var result = await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                Assert.AreEqual(result.RegisteredSections[0].SectionId, _proposedRegistration.ProposedSections[0].SectionId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_NullProposedRegistration_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    await _ieController.PostProposedRegistrationForClassesAsync(null);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_NullProposedSections_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _proposedRegistration.ProposedSections = null;
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_EmptyProposedSections_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _proposedRegistration.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public async Task PostProposedRegistrationForClassesAsync_ServiceThrows_ArgumentNullException()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ProposedRegistrationForClassesAsync(It.IsAny<InstantEnrollmentProposedRegistration>())).Throws(new ArgumentNullException());
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    Assert.IsTrue(exMsg.Contains("Proposed registration argument was not provided in order to complete proposed registration for classes selected for instant enrollment"));
                }
            }

            [TestMethod]
            public async Task PostProposedRegistrationForClassesAsync_ServiceThrows_ArgumentException()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ProposedRegistrationForClassesAsync(It.IsAny<InstantEnrollmentProposedRegistration>())).Throws(new ArgumentException());
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    Assert.IsTrue(exMsg.Contains("An invalid argument was supplied for proposed registrations."));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ProposedRegistrationForClassesAsync(It.IsAny<InstantEnrollmentProposedRegistration>())).Throws(new ColleagueSessionExpiredException("session expired"));
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ProposedRegistrationForClassesAsync(It.IsAny<InstantEnrollmentProposedRegistration>())).Throws(new PermissionsException());
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_ArgumentNullException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ProposedRegistrationForClassesAsync(It.IsAny<InstantEnrollmentProposedRegistration>())).Throws(new ArgumentNullException());
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_ArgumentException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ProposedRegistrationForClassesAsync(It.IsAny<InstantEnrollmentProposedRegistration>())).Throws(new ArgumentException());
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ProposedRegistrationForClassesAsync(It.IsAny<InstantEnrollmentProposedRegistration>())).Throws(new Exception());
                    await _ieController.PostProposedRegistrationForClassesAsync(_proposedRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class StartPaymentGatewayRegistrationTests
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

            private InstantEnrollmentController ieController;
            private Mock<IInstantEnrollmentService> ieServiceMock;
            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;
            private InstantEnrollmentStartPaymentGatewayRegistrationResult ieStartPGResult;
            private InstantEnrollmentPaymentGatewayRegistration goodDto;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();
                ieServiceMock = new Mock<IInstantEnrollmentService>();
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);

                goodDto = new InstantEnrollmentPaymentGatewayRegistration();
                goodDto.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();
                goodDto.ProposedSections.Add(new InstantEnrollmentRegistrationBaseSectionToRegister() { SectionId = "sect1" });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Dto_Null()
            {
                await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_ProposedSections_Null()
            {
                InstantEnrollmentPaymentGatewayRegistration dto = new InstantEnrollmentPaymentGatewayRegistration();
                dto.ProposedSections = null;
                await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(dto);
            }

            [TestMethod]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Success()
            {
                ieStartPGResult = new Dtos.Student.InstantEnrollment.InstantEnrollmentStartPaymentGatewayRegistrationResult();
                ieStartPGResult.PaymentProviderRedirectUrl = "Url";
                ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).ReturnsAsync(ieStartPGResult);

                var result = await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                Assert.AreEqual(result.PaymentProviderRedirectUrl, "Url");
            }

            [TestMethod]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Service_ArgNull_Exception()
            {
                try
                {
                    ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).Throws(new ArgumentNullException());
                    await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("A required argument was not provided."));
                }
            }

            [TestMethod]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Service_ArgException()
            {
                try
                {
                    await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                    ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).Throws(new ArgumentException());
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("An invalid argument was supplied."));
                }
            }

            [TestMethod]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Service_Generic_Exception()
            {
                try
                {
                    ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).Throws(new Exception());
                    await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("Unable to start the payment gateway instant enrollment registration"));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).Throws(new ColleagueSessionExpiredException("session expired"));
                    await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).Throws(new PermissionsException());
                    await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_ArgumentNullException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).Throws(new ArgumentNullException());
                    await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_ArgumentException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).Throws(new ArgumentException());
                    await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostStartInstantEnrollmentPaymentGatewayTransaction_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    ieServiceMock.Setup(ies => ies.StartInstantEnrollmentPaymentGatewayTransaction(It.IsAny<InstantEnrollmentPaymentGatewayRegistration>())).Throws(new Exception());
                    await ieController.PostStartInstantEnrollmentPaymentGatewayTransaction(goodDto);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class ZeroCostRegistrationForClassesTests
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

            private InstantEnrollmentController _ieController;
            private Mock<IInstantEnrollmentService> _ieServiceMock;
            private Mock<ICourseService> _courseServiceMock;
            private Mock<ILogger> _loggerMock;
            private Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration _zeroCostRegistration;
            private Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult _zeroCostRegistrationResult;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                _loggerMock = new Mock<ILogger>();
                _courseServiceMock = new Mock<ICourseService>();
                _ieServiceMock = new Mock<IInstantEnrollmentService>();
                _ieController = new InstantEnrollmentController(_courseServiceMock.Object, _ieServiceMock.Object, _loggerMock.Object);

                _zeroCostRegistration = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration();
                _zeroCostRegistration.ProposedSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>();
                _zeroCostRegistration.ProposedSections.Add(new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister()
                {
                    SectionId = "sect1"
                });

                _zeroCostRegistrationResult = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistrationResult();
                _zeroCostRegistrationResult.RegisteredSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection>();
                _zeroCostRegistrationResult.RegisteredSections.Add(
                 new Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseRegisteredSection
                 {
                     SectionId = "sect1",
                     SectionCost = 0
                 });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_Dto_Null()
            {
                await _ieController.PostZeroCostRegistrationForClassesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_ProposedSections_Null()
            {
                _zeroCostRegistration = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration();
                await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_ProposedSections_Empty()
            {
                _zeroCostRegistration = new Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration();
                _zeroCostRegistration.ProposedSections = new List<Dtos.Student.InstantEnrollment.InstantEnrollmentRegistrationBaseSectionToRegister>();
                await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
            }

            [TestMethod]
            public async Task PostZeroCostRegistrationForClassesAsync_Success()
            {
                _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).ReturnsAsync(_zeroCostRegistrationResult);

                var result = await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                Assert.AreEqual(result.RegisteredSections[0].SectionId, _zeroCostRegistration.ProposedSections[0].SectionId);
            }

            [TestMethod]
            public async Task PostZeroCostRegistrationForClassesAsync_ServiceThrows_ArgumentNullException()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).Throws(new ArgumentNullException());
                    await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("A required zero cost registration argument was not provided to register for classes for instant enrollment."));
                }
            }

            [TestMethod]
            public async Task PostZeroCostRegistrationForClassesAsync_ServiceThrows_ArgumentException()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).Throws(new ArgumentException());
                    await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("An invalid argument was supplied for the zero cost registration."));
                }
            }

            [TestMethod]
            public async Task PostZeroCostRegistrationForClassesAsync_ServiceThrows_GenericException()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<Dtos.Student.InstantEnrollment.InstantEnrollmentZeroCostRegistration>())).Throws(new Exception());
                    await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("Could not complete the zero cost registration for selected classes for instant enrollment."));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<InstantEnrollmentZeroCostRegistration>())).Throws(new ColleagueSessionExpiredException("session expired"));
                    await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<InstantEnrollmentZeroCostRegistration>())).Throws(new PermissionsException());
                    await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_ArgumentNullException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<InstantEnrollmentZeroCostRegistration>())).Throws(new ArgumentNullException());
                    await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostZeroCostRegistrationForClassesAsync_ArgumentException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<InstantEnrollmentZeroCostRegistration>())).Throws(new ArgumentException());
                    await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostProposedRegistrationForClassesAsync_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.ZeroCostRegistrationForClassesAsync(It.IsAny<InstantEnrollmentZeroCostRegistration>())).Throws(new Exception());
                    await _ieController.PostZeroCostRegistrationForClassesAsync(_zeroCostRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class EchecktRegistrationForClassesTests
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

            private InstantEnrollmentController _ieController;
            private Mock<IInstantEnrollmentService> _ieServiceMock;
            private Mock<ICourseService> _courseServiceMock;
            private Mock<ILogger> _loggerMock;
            private InstantEnrollmentEcheckRegistration _echeckRegistration;
            private InstantEnrollmentEcheckRegistrationResult _echeckRegistrationResult;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                _loggerMock = new Mock<ILogger>();
                _courseServiceMock = new Mock<ICourseService>();
                _ieServiceMock = new Mock<IInstantEnrollmentService>();
                _ieController = new InstantEnrollmentController(_courseServiceMock.Object, _ieServiceMock.Object, _loggerMock.Object);

                _echeckRegistration = new InstantEnrollmentEcheckRegistration();
                _echeckRegistration.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();
                _echeckRegistration.ProposedSections.Add(new InstantEnrollmentRegistrationBaseSectionToRegister()
                {
                    SectionId = "sect1"
                });

                _echeckRegistrationResult = new InstantEnrollmentEcheckRegistrationResult();
                _echeckRegistrationResult.RegisteredSections = new List<InstantEnrollmentRegistrationBaseRegisteredSection>();
                _echeckRegistrationResult.RegisteredSections.Add(
                 new InstantEnrollmentRegistrationBaseRegisteredSection
                 {
                     SectionId = "sect1",
                     SectionCost = 0
                 });
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_Dto_Null()
            {
                await _ieController.PostEcheckRegistrationForClassesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_ProposedSections_Null()
            {
                _echeckRegistration = new InstantEnrollmentEcheckRegistration();
                await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_ProposedSections_Empty()
            {
                _echeckRegistration = new InstantEnrollmentEcheckRegistration();
                _echeckRegistration.ProposedSections = new List<InstantEnrollmentRegistrationBaseSectionToRegister>();
                await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
            }

            [TestMethod]
            public async Task PostEcheckRegistrationForClassesAsync_Success()
            {
                _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).ReturnsAsync(_echeckRegistrationResult);

                var result = await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                Assert.AreEqual(result.RegisteredSections[0].SectionId, _echeckRegistration.ProposedSections[0].SectionId);
            }

            [TestMethod]
            public async Task PostEcheckRegistrationForClassesAsync_ServiceThrows_ArgumentNullException()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).Throws(new ArgumentNullException());
                    await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("Echeck registration argument was not provided in order to complete registration for classes selected for instant enrollment"));
                }
            }

            [TestMethod]
            public async Task PostEcheckRegistrationForClassesAsync_ServiceThrows_ArgumentException()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).Throws(new ArgumentException());
                    await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("An invalid argument was supplied"));
                }
            }

            [TestMethod]
            public async Task PostEcheckRegistrationForClassesAsync_ServiceThrows_GenericException()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).Throws(new Exception());
                    await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                }
                catch (HttpResponseException ex)
                {
                    var exMsg = await ex.Response.Content.ReadAsStringAsync();
                    // The message is contained within the string along with other elements, so check that it contains the expected message
                    Assert.IsTrue(exMsg.Contains("ouldn't complete the echeck registration for classes selected for instant enrollment"));
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).Throws(new ColleagueSessionExpiredException("session expired"));
                    await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).Throws(new PermissionsException());
                    await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_ArgumentNullException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).Throws(new ArgumentNullException());
                    await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_ArgumentException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).Throws(new ArgumentException());
                    await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(ies => ies.EcheckRegistrationForClassesAsync(It.IsAny<InstantEnrollmentEcheckRegistration>())).Throws(new Exception());
                    await _ieController.PostEcheckRegistrationForClassesAsync(_echeckRegistration);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests
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

            private InstantEnrollmentController ieController;
            private Mock<IInstantEnrollmentService> ieServiceMock;
            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public void GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();
                ieServiceMock = new Mock<IInstantEnrollmentService>();
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_Null_Request()
            {
                await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_Permissions_Exception()
            {
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                    ThrowsAsync(new PermissionsException());
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
                {
                    CashReceiptId = "123",
                    PersonId = "0001234"
                };
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);

                await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_general_Exception()
            {
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                    ThrowsAsync(new ApplicationException());
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
                {
                    CashReceiptId = "123",
                    PersonId = "0001234"
                };
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);

                await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    ieServiceMock.Setup(svc => svc.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                        ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
                    {
                        CashReceiptId = "123",
                        PersonId = "0001234"
                    };
                    ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
                    await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    ieServiceMock.Setup(svc => svc.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                        ThrowsAsync(new ApplicationException());
                    var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
                    {
                        CashReceiptId = "123",
                        PersonId = "0001234"
                    };
                    ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
                    await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public async Task GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsyncTests_valid()
            {
                var response = new List<string>()
                {
                    "Line 1",
                    "Line 2"
                };
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(It.IsAny<InstantEnrollmentPaymentAcknowledgementParagraphRequest>())).
                    ReturnsAsync(response);
                var request = new InstantEnrollmentPaymentAcknowledgementParagraphRequest()
                {
                    CashReceiptId = "123",
                    PersonId = "0001234"
                };
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);

                var text = await ieController.GetInstantEnrollmentPaymentAcknowledgementParagraphTextAsync(request);
                CollectionAssert.AreEqual(response, text.ToList());
            }
        }

        [TestClass]
        public class QueryPersonMatchResultsInstantEnrollmentByPostAsyncTests
        {
            private Mock<IInstantEnrollmentService> _ieServiceMock;
            private Mock<ICourseService> _courseServiceMock;
            private InstantEnrollmentController _ieController;
            ILogger logger = new Mock<ILogger>().Object;

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

                _ieServiceMock = new Mock<IInstantEnrollmentService>();
                _courseServiceMock = new Mock<ICourseService>();
                _ieController = new InstantEnrollmentController(_courseServiceMock.Object, _ieServiceMock.Object, logger);

            }

            [TestCleanup]
            public void Cleanup()
            {
                _ieController = null;
                _ieServiceMock = null;
                logger = null;
            }

            [TestMethod]
            public async Task QueryPersonMatchResultsInstantEnrollmentByPostAsync_Success()
            {
                PersonMatchCriteriaInstantEnrollment criteriaDto;
                var resultDto = new InstantEnrollmentPersonMatchResult()
                {
                    HasPotentialMatches = true,
                    PersonId = null
                };

                criteriaDto = new PersonMatchCriteriaInstantEnrollment()
                {
                    LastName = "Enrollment",
                    FirstName = "Instant"
                };

                _ieServiceMock.Setup(s => s.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteriaDto)).ReturnsAsync(resultDto);

                var results = await _ieController.QueryPersonMatchResultsInstantEnrollmentByPostAsync(criteriaDto);
                Assert.AreEqual(resultDto.HasPotentialMatches, results.HasPotentialMatches);
                Assert.AreEqual(resultDto.PersonId, results.PersonId);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task QueryPersonMatchResultsByPostAsync_NullCriteriaException()
            {
                _ieServiceMock.Setup(s => s.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null)).Throws(new Exception("An error occurred"));
                var results = await _ieController.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    _ieServiceMock.Setup(s => s.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null)).Throws(new ColleagueSessionExpiredException("session expired"));
                    var results = await _ieController.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    _ieServiceMock.Setup(s => s.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null)).Throws(new Exception());
                    var results = await _ieController.QueryPersonMatchResultsInstantEnrollmentByPostAsync(null);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests
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

            private InstantEnrollmentController ieController;
            private Mock<IInstantEnrollmentService> ieServiceMock;
            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public void GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                courseServiceMock = new Mock<ICourseService>();
                ieServiceMock = new Mock<IInstantEnrollmentService>();
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_Null_Request()
            {
                await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_CashReceiptId_Permissions_Exception()
            {
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                    ThrowsAsync(new PermissionsException());
                var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                {
                    TransactionId = "",
                    CashReceiptId = "123",
                    PersonId = ""
                };
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
                await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_TransactionId_Permissions_Exception()
            {
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                    ThrowsAsync(new PermissionsException());
                var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                {
                    TransactionId = "123",
                    CashReceiptId = "",
                    PersonId = ""
                };
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
                await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                        ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                    {
                        TransactionId = "123",
                        CashReceiptId = "",
                        PersonId = ""
                    };
                    ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
                    await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostEcheckRegistrationForClassesAsync_Exception_ReturnsHttpResponseException_BadRequest()
            {
                try
                {

                    ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                        Throws(new Exception());
                    var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                    {
                        TransactionId = "123",
                        CashReceiptId = "",
                        PersonId = ""
                    };
                    ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
                    await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_CashReceiptId_valid()
            {
                var convenienceFees = new List<ConvenienceFee>();
                convenienceFees.Add(new ConvenienceFee() { Code = "code", Amount = 1.2M, Description = "convience fee" });

                var paymentMethods = new List<PaymentMethod>();
                paymentMethods.Add(new PaymentMethod()
                {
                    PayMethodCode = "e-check",
                    PayMethodDescription = "e-Check",
                    ControlNumber = "8858",
                    ConfirmationNumber = "",
                    TransactionNumber = "A45D8542",
                    TransactionDescription = "Payment on account",
                    TransactionAmount = 81.2M
                });

                var response = new InstantEnrollmentCashReceiptAcknowledgement()
                {
                    CashReceiptsId = "123",
                    ReceiptDate = new DateTime(2020, 01, 01),
                    MerchantNameAddress = new List<string>() { "Mechant Name", "Address Line 1" },
                    ReceiptPayerId = "0001",
                    ReceiptPayerName = "John Smith",
                    Status = EcommerceProcessStatus.None,
                    Username = "",
                    UsernameCreationErrors = new List<string>(),
                    ConvenienceFees = convenienceFees,
                    PaymentMethods = paymentMethods,
                    RegisteredSections = new List<InstantEnrollmentRegistrationPaymentGatewayRegisteredSection>(),
                    FailedSections = new List<InstantEnrollmentRegistrationPaymentGatewayFailedSection>()
                };
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                        ReturnsAsync(response);
                var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                {
                    TransactionId = "",
                    CashReceiptId = "123",
                    PersonId = ""
                };
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);

                var cashReceiptAcknowledgement = await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);

                Assert.AreEqual(cashReceiptAcknowledgement.CashReceiptsId, response.CashReceiptsId);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptDate, response.ReceiptDate);
                CollectionAssert.AreEqual(cashReceiptAcknowledgement.MerchantNameAddress, response.MerchantNameAddress);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptPayerId, response.ReceiptPayerId);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptPayerName, response.ReceiptPayerName);
                Assert.AreEqual(cashReceiptAcknowledgement.Status, response.Status);
                Assert.AreEqual(cashReceiptAcknowledgement.Username, response.Username);
                CollectionAssert.AreEqual(cashReceiptAcknowledgement.UsernameCreationErrors, response.UsernameCreationErrors);
                Assert.AreEqual(cashReceiptAcknowledgement.ConvenienceFees.Count(), response.ConvenienceFees.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.PaymentMethods.Count(), response.PaymentMethods.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.RegisteredSections.Count(), response.RegisteredSections.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.FailedSections.Count(), response.FailedSections.Count());
            }

            [TestMethod]
            public async Task GetInstantEnrollmentCashReceiptAcknowledgementAsyncTests_TransactionId_valid()
            {
                var convenienceFees = new List<ConvenienceFee>();
                convenienceFees.Add(new ConvenienceFee() { Code = "code", Amount = 1.2M, Description = "convience fee" });

                var paymentMethods = new List<PaymentMethod>();
                paymentMethods.Add(new PaymentMethod()
                {
                    PayMethodCode = "VSA",
                    PayMethodDescription = "Visa",
                    ControlNumber = "8858",
                    ConfirmationNumber = "A45D8542",
                    TransactionNumber = "123",
                    TransactionDescription = "Payment on account",
                    TransactionAmount = 81.2M
                });

                var registeredSections = new List<InstantEnrollmentRegistrationPaymentGatewayRegisteredSection>();
                registeredSections.Add(new InstantEnrollmentRegistrationPaymentGatewayRegisteredSection()
                {
                    SectionId = "251",
                    Ceus = 1.5M,
                    SectionCost = 80.12M
                });
                var response = new InstantEnrollmentCashReceiptAcknowledgement()
                {
                    CashReceiptsId = "123",
                    ReceiptDate = new DateTime(2020, 01, 01),
                    MerchantNameAddress = new List<string>() { "Mechant Name", "Address Line 1" },
                    ReceiptPayerId = "0001",
                    ReceiptPayerName = "John Smith",
                    Status = EcommerceProcessStatus.None,
                    Username = "john_smith",
                    UsernameCreationErrors = new List<string>(),
                    ConvenienceFees = convenienceFees,
                    PaymentMethods = paymentMethods,
                    RegisteredSections = registeredSections,
                    FailedSections = new List<InstantEnrollmentRegistrationPaymentGatewayFailedSection>()
                };
                ieServiceMock.Setup(svc => svc.GetInstantEnrollmentCashReceiptAcknowledgementAsync(It.IsAny<InstantEnrollmentCashReceiptAcknowledgementRequest>())).
                        ReturnsAsync(response);
                var request = new InstantEnrollmentCashReceiptAcknowledgementRequest()
                {
                    TransactionId = "123",
                    CashReceiptId = "",
                    PersonId = ""
                };
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);

                var cashReceiptAcknowledgement = await ieController.GetInstantEnrollmentCashReceiptAcknowledgementAsync(request);

                Assert.AreEqual(cashReceiptAcknowledgement.CashReceiptsId, response.CashReceiptsId);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptDate, response.ReceiptDate);
                CollectionAssert.AreEqual(cashReceiptAcknowledgement.MerchantNameAddress, response.MerchantNameAddress);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptPayerId, response.ReceiptPayerId);
                Assert.AreEqual(cashReceiptAcknowledgement.ReceiptPayerName, response.ReceiptPayerName);
                Assert.AreEqual(cashReceiptAcknowledgement.Status, response.Status);
                Assert.AreEqual(cashReceiptAcknowledgement.Username, response.Username);
                CollectionAssert.AreEqual(cashReceiptAcknowledgement.UsernameCreationErrors, response.UsernameCreationErrors);
                Assert.AreEqual(cashReceiptAcknowledgement.ConvenienceFees.Count(), response.ConvenienceFees.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.PaymentMethods.Count(), response.PaymentMethods.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.RegisteredSections.Count(), response.RegisteredSections.Count());
                Assert.AreEqual(cashReceiptAcknowledgement.FailedSections.Count(), response.FailedSections.Count());
            }
        }

        [TestClass]
        public class GetInstantEnrollmentStudentPrograms2AsyncTests
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

            private InstantEnrollmentController ieController;
            private Mock<IInstantEnrollmentService> ieServiceMock;
            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;

            private IEnumerable<StudentProgram> sprog;

            [TestInitialize]
            public void GetInstantEnrollmentStudentPrograms2AsyncTests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                ieServiceMock = new Mock<IInstantEnrollmentService>();
                courseServiceMock = new Mock<ICourseService>();
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentStudentPrograms2AsyncTests_Null_StudentId_Paramter()
            {
                await ieController.GetInstantEnrollmentStudentPrograms2Async(null, true);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentStudentPrograms2AsyncTests_Empty_StudentId_Paramter()
            {
                await ieController.GetInstantEnrollmentStudentPrograms2Async(string.Empty, true);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentStudentPrograms2AsyncTests_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    ieServiceMock.Setup(svc => svc.GetInstantEnrollmentStudentPrograms2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
                    await ieController.GetInstantEnrollmentStudentPrograms2Async("0001", true);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetInstantEnrollmentStudentPrograms2AsyncTests_StudentId_PermissionsException_Forbidden()
            {
                try
                {
                    ieServiceMock.Setup(svc => svc.GetInstantEnrollmentStudentPrograms2Async(It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new PermissionsException());
                    ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
                    await ieController.GetInstantEnrollmentStudentPrograms2Async("0001", true);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw;
                }
            }
        }

        [TestClass]
        public class PostInstantEnrollmentCourseSearch2AsyncTests
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

            private InstantEnrollmentController ieController;
            private Mock<IInstantEnrollmentService> ieServiceMock;
            private Mock<ICourseService> courseServiceMock;
            private Mock<ILogger> loggerMock;

            [TestInitialize]
            public void PostInstantEnrollmentCourseSearch2AsyncTests_Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                ieServiceMock = new Mock<IInstantEnrollmentService>();
                courseServiceMock = new Mock<ICourseService>();
                ieController = new InstantEnrollmentController(courseServiceMock.Object, ieServiceMock.Object, loggerMock.Object);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostInstantEnrollmentCourseSearch2Async_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                var criteria = new InstantEnrollmentCourseSearchCriteria() { Keyword = "ART-100" };
                try
                {
                    courseServiceMock.Setup(svc => svc.InstantEnrollmentSearch2Async(It.IsAny<InstantEnrollmentCourseSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    await ieController.PostInstantEnrollmentCourseSearch2Async(criteria, 0, 0);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task PostInstantEnrollmentCourseSearch2Async_Exception_ReturnsHttpResponseException_BadRequest()
            {
                var criteria = new InstantEnrollmentCourseSearchCriteria() { Keyword = "ART-100" };
                try
                {
                    courseServiceMock.Setup(svc => svc.InstantEnrollmentSearch2Async(It.IsAny<InstantEnrollmentCourseSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).ThrowsAsync(new ApplicationException());
                    await ieController.PostInstantEnrollmentCourseSearch2Async(criteria, 0, 0);
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw;
                }
            }
        }
    }
}