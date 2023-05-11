// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student.QuickRegistration;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentQuickRegistrationControllerTests
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
        private Mock<IStudentQuickRegistrationService> _serviceMock;
        private IStudentQuickRegistrationService _service;
        private Mock<ILogger> _loggerMock;
        private ILogger _logger;
        private StudentQuickRegistrationController _controller;

        [TestInitialize]
        public void StudentQuickRegistrationControllerTests_Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            _serviceMock = new Mock<IStudentQuickRegistrationService>();
            _service = _serviceMock.Object;
            _loggerMock = new Mock<ILogger>();
            _logger = _loggerMock.Object;

            _controller = new StudentQuickRegistrationController(_service, _logger);
        }

        [TestClass]
        public class StudentQuickRegistrationControllerTests_GetStudentQuickRegistrationSectionsAsync_Tests : StudentQuickRegistrationControllerTests
        {
            [TestInitialize]
            public void StudentQuickRegistrationControllerTests_GetStudentQuickRegistrationSectionsAsync_Initialize()
            {
                base.StudentQuickRegistrationControllerTests_Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]

            public async Task StudentQuickRegistrationControllerTests_GetStudentQuickRegistrationSectionsAsync_null_student_ID_throws_exception()
            {
                var data = await _controller.GetStudentQuickRegistrationSectionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]

            public async Task StudentQuickRegistrationControllerTests_GetStudentQuickRegistrationSectionsAsync_empty_student_ID_throws_exception()
            {
                var data = await _controller.GetStudentQuickRegistrationSectionsAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentQuickRegistrationControllerTests_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    _serviceMock.Setup(x => x.GetStudentQuickRegistrationAsync(It.IsAny<string>())).ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    await _controller.GetStudentQuickRegistrationSectionsAsync("0001234");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw ex;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task StudentQuickRegistrationControllerTests_GetStudentQuickRegistrationSectionsAsync_service_PermissionsException()
            {
                try
                {
                    _serviceMock.Setup(x => x.GetStudentQuickRegistrationAsync(It.IsAny<string>())).ThrowsAsync(new PermissionsException());
                    await _controller.GetStudentQuickRegistrationSectionsAsync("0001234");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]

            public async Task StudentQuickRegistrationControllerTests_GetStudentQuickRegistrationSectionsAsync_service_GenericException()
            {
                try
                {
                    _serviceMock.Setup(x => x.GetStudentQuickRegistrationAsync(It.IsAny<string>())).ThrowsAsync(new ApplicationException());
                    await _controller.GetStudentQuickRegistrationSectionsAsync("0001234");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            public async Task StudentQuickRegistrationControllerTests_GetStudentQuickRegistrationSectionsAsync_valid()
            {
                _serviceMock.Setup(x => x.GetStudentQuickRegistrationAsync(It.IsAny<string>())).ReturnsAsync(new StudentQuickRegistration()
                {
                    StudentId = "0001234",
                    Terms = new List<QuickRegistrationTerm>()
                    {
                        new QuickRegistrationTerm()
                        {
                            TermCode = "2019/FA",
                            Sections = new List<QuickRegistrationSection>()
                            {
                                new QuickRegistrationSection() { SectionId = "123", Credits = 3m, GradingType = Dtos.Student.GradingType.Graded },
                                new QuickRegistrationSection() { SectionId = "234", Credits = 4m, GradingType = Dtos.Student.GradingType.Audit },
                                new QuickRegistrationSection() { SectionId = "345", Credits = 4m, GradingType = Dtos.Student.GradingType.PassFail }
                            }
                        }
                    }
                });
                var data = await _controller.GetStudentQuickRegistrationSectionsAsync("0001234");
                Assert.IsNotNull(data);
            }
        }
    }
}