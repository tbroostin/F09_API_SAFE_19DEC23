// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Student;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class RecruiterControllerTests
    {
        /// <summary>
        /// Set up class to use for each faculty controller test class
        /// </summary>
        public abstract class RecruiterControllerTestsSetup
        {
            #region Test Context

            protected TestContext testContextInstance;

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

            public RecruiterController recruiterController;
            public Mock<IRecruiterService> recruiterServiceMock;
            public IRecruiterService recruiterService;
            public Mock<ILogger> loggerMock;

            public void InitializeRecruiterController()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                loggerMock = new Mock<ILogger>();
                recruiterServiceMock = new Mock<IRecruiterService>();
                recruiterService = recruiterServiceMock.Object;

                recruiterController = new RecruiterController(recruiterService, loggerMock.Object)
                {
                    Request = new HttpRequestMessage()
                };
            }
        }

        [TestClass]
        public class RecruiterController_PostApplicationAsync_Tests : RecruiterControllerTestsSetup
        {
            private Application permissionsExceptionApp;
            private Application genericExceptionApp;
            private Application successApp;


            [TestInitialize]
            public void RecruiterController_PostApplicationAsync_Initialize()
            {
                base.InitializeRecruiterController();

                permissionsExceptionApp = new Application()
                {
                    CrmApplicationId = "PERMISSIONS_EXCEPTION"
                };
                genericExceptionApp = new Application()
                {
                    CrmApplicationId = "EXCEPTION"
                };
                successApp = new Application()
                {
                    CrmApplicationId = "SUCCESS"
                };

                recruiterServiceMock.Setup(svc => svc.ImportApplicationAsync(permissionsExceptionApp)).Throws(new PermissionsException());
                recruiterServiceMock.Setup(svc => svc.ImportApplicationAsync(genericExceptionApp)).Throws(new ApplicationException());
                recruiterServiceMock.Setup(svc => svc.ImportApplicationAsync(successApp)).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostApplicationAsync_returns_Forbidden_status_code_on_PermissionsException()
            {
                var result = await recruiterController.PostApplicationAsync(permissionsExceptionApp);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostApplicationAsync_returns_BadRequest_status_code_on_non_permissions_Exception()
            {
                var result = await recruiterController.PostApplicationAsync(genericExceptionApp);
            }

            [TestMethod]
            public async Task RecruiterController_PostApplicationAsync_returns_OK_status_code_on_success()
            {
                var result = await recruiterController.PostApplicationAsync(successApp);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [TestClass]
        public class RecruiterController_PostApplicationStatusAsync_Tests : RecruiterControllerTestsSetup
        {
            private Application permissionsExceptionApp;
            private Application genericExceptionApp;
            private Application successApp;


            [TestInitialize]
            public void RecruiterController_PostApplicationStatusAsync_Initialize()
            {
                base.InitializeRecruiterController();

                permissionsExceptionApp = new Application()
                {
                    CrmApplicationId = "PERMISSIONS_EXCEPTION"
                };
                genericExceptionApp = new Application()
                {
                    CrmApplicationId = "EXCEPTION"
                };
                successApp = new Application()
                {
                    CrmApplicationId = "SUCCESS"
                };

                recruiterServiceMock.Setup(svc => svc.UpdateApplicationStatusAsync(permissionsExceptionApp)).Throws(new PermissionsException());
                recruiterServiceMock.Setup(svc => svc.UpdateApplicationStatusAsync(genericExceptionApp)).Throws(new ApplicationException());
                recruiterServiceMock.Setup(svc => svc.UpdateApplicationStatusAsync(successApp)).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostApplicationStatusAsync_returns_Forbidden_status_code_on_PermissionsException()
            {
                var result = await recruiterController.PostApplicationStatusAsync(permissionsExceptionApp);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostApplicationStatusAsync_returns_BadRequest_status_code_on_non_permissions_Exception()
            {
                var result = await recruiterController.PostApplicationStatusAsync(genericExceptionApp);
            }

            [TestMethod]
            public async Task RecruiterController_PostApplicationStatusAsync_returns_OK_status_code_on_success()
            {
                var result = await recruiterController.PostApplicationStatusAsync(successApp);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [TestClass]
        public class RecruiterController_PostTestScoresAsync_Tests : RecruiterControllerTestsSetup
        {
            private TestScore permissionsExceptionTestScore;
            private TestScore genericExceptionTestScore;
            private TestScore successTestScore;


            [TestInitialize]
            public void RecruiterController_PostTestScoresAsync_Initialize()
            {
                base.InitializeRecruiterController();

                permissionsExceptionTestScore = new TestScore()
                {
                    ErpProspectId = "PERMISSIONS_EXCEPTION"
                };
                genericExceptionTestScore = new TestScore()
                {
                    ErpProspectId = "EXCEPTION"
                };
                successTestScore = new TestScore()
                {
                    ErpProspectId = "SUCCESS"
                };

                recruiterServiceMock.Setup(svc => svc.ImportTestScoresAsync(permissionsExceptionTestScore)).Throws(new PermissionsException());
                recruiterServiceMock.Setup(svc => svc.ImportTestScoresAsync(genericExceptionTestScore)).Throws(new ApplicationException());
                recruiterServiceMock.Setup(svc => svc.ImportTestScoresAsync(successTestScore)).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostTestScoresAsync_returns_Forbidden_status_code_on_PermissionsException()
            {
                var result = await recruiterController.PostTestScoresAsync(permissionsExceptionTestScore);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostTestScoresAsync_returns_BadRequest_status_code_on_non_permissions_Exception()
            {
                var result = await recruiterController.PostTestScoresAsync(genericExceptionTestScore);
            }

            [TestMethod]
            public async Task RecruiterController_PostTestScoresAsync_returns_OK_status_code_on_success()
            {
                var result = await recruiterController.PostTestScoresAsync(successTestScore);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [TestClass]
        public class RecruiterController_PostTranscriptCoursesAsync_Tests : RecruiterControllerTestsSetup
        {
            private TranscriptCourse permissionsExceptionTranscriptCourse;
            private TranscriptCourse genericExceptionTranscriptCourse;
            private TranscriptCourse successTranscriptCourse;


            [TestInitialize]
            public void RecruiterController_PostTranscriptCoursesAsync_Initialize()
            {
                base.InitializeRecruiterController();

                permissionsExceptionTranscriptCourse = new TranscriptCourse()
                {
                    ErpProspectId = "PERMISSIONS_EXCEPTION"
                };
                genericExceptionTranscriptCourse = new TranscriptCourse()
                {
                    ErpProspectId = "EXCEPTION"
                };
                successTranscriptCourse = new TranscriptCourse()
                {
                    ErpProspectId = "SUCCESS"
                };

                recruiterServiceMock.Setup(svc => svc.ImportTranscriptCoursesAsync(permissionsExceptionTranscriptCourse)).Throws(new PermissionsException());
                recruiterServiceMock.Setup(svc => svc.ImportTranscriptCoursesAsync(genericExceptionTranscriptCourse)).Throws(new ApplicationException());
                recruiterServiceMock.Setup(svc => svc.ImportTranscriptCoursesAsync(successTranscriptCourse)).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostTranscriptCoursesAsync_returns_Forbidden_status_code_on_PermissionsException()
            {
                var result = await recruiterController.PostTranscriptCoursesAsync(permissionsExceptionTranscriptCourse);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostTranscriptCoursesAsync_returns_BadRequest_status_code_on_non_permissions_Exception()
            {
                var result = await recruiterController.PostTranscriptCoursesAsync(genericExceptionTranscriptCourse);
            }

            [TestMethod]
            public async Task RecruiterController_PostTranscriptCoursesAsync_returns_OK_status_code_on_success()
            {
                var result = await recruiterController.PostTranscriptCoursesAsync(successTranscriptCourse);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [TestClass]
        public class RecruiterController_PostCommunicationHistoryAsync_Tests : RecruiterControllerTestsSetup
        {
            private CommunicationHistory permissionsExceptionCommunicationHistory;
            private CommunicationHistory genericExceptionCommunicationHistory;
            private CommunicationHistory successCommunicationHistory;


            [TestInitialize]
            public void RecruiterController_PostCommunicationHistoryAsync_Initialize()
            {
                base.InitializeRecruiterController();

                permissionsExceptionCommunicationHistory = new CommunicationHistory()
                {
                    ErpProspectId = "PERMISSIONS_EXCEPTION"
                };
                genericExceptionCommunicationHistory = new CommunicationHistory()
                {
                    ErpProspectId = "EXCEPTION"
                };
                successCommunicationHistory = new CommunicationHistory()
                {
                    ErpProspectId = "SUCCESS"
                };

                recruiterServiceMock.Setup(svc => svc.ImportCommunicationHistoryAsync(permissionsExceptionCommunicationHistory)).Throws(new PermissionsException());
                recruiterServiceMock.Setup(svc => svc.ImportCommunicationHistoryAsync(genericExceptionCommunicationHistory)).Throws(new ApplicationException());
                recruiterServiceMock.Setup(svc => svc.ImportCommunicationHistoryAsync(successCommunicationHistory)).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostCommunicationHistoryAsync_returns_Forbidden_status_code_on_PermissionsException()
            {
                var result = await recruiterController.PostCommunicationHistoryAsync(permissionsExceptionCommunicationHistory);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostCommunicationHistoryAsync_returns_BadRequest_status_code_on_non_permissions_Exception()
            {
                var result = await recruiterController.PostCommunicationHistoryAsync(genericExceptionCommunicationHistory);
            }

            [TestMethod]
            public async Task RecruiterController_PostCommunicationHistoryAsync_returns_OK_status_code_on_success()
            {
                var result = await recruiterController.PostCommunicationHistoryAsync(successCommunicationHistory);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [TestClass]
        public class RecruiterController_PostCommunicationHistoryRequestAsync_Tests : RecruiterControllerTestsSetup
        {
            private CommunicationHistory permissionsExceptionCommunicationHistory;
            private CommunicationHistory genericExceptionCommunicationHistory;
            private CommunicationHistory successCommunicationHistory;


            [TestInitialize]
            public void RecruiterController_PostCommunicationHistoryRequestAsync_Initialize()
            {
                base.InitializeRecruiterController();

                permissionsExceptionCommunicationHistory = new CommunicationHistory()
                {
                    ErpProspectId = "PERMISSIONS_EXCEPTION"
                };
                genericExceptionCommunicationHistory = new CommunicationHistory()
                {
                    ErpProspectId = "EXCEPTION"
                };
                successCommunicationHistory = new CommunicationHistory()
                {
                    ErpProspectId = "SUCCESS"
                };

                recruiterServiceMock.Setup(svc => svc.RequestCommunicationHistoryAsync(permissionsExceptionCommunicationHistory)).Throws(new PermissionsException());
                recruiterServiceMock.Setup(svc => svc.RequestCommunicationHistoryAsync(genericExceptionCommunicationHistory)).Throws(new ApplicationException());
                recruiterServiceMock.Setup(svc => svc.RequestCommunicationHistoryAsync(successCommunicationHistory)).Returns(Task.FromResult(default(object)));
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostCommunicationHistoryRequestAsync_returns_Forbidden_status_code_on_PermissionsException()
            {
                var result = await recruiterController.PostCommunicationHistoryRequestAsync(permissionsExceptionCommunicationHistory);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostCommunicationHistoryRequestAsync_returns_BadRequest_status_code_on_non_permissions_Exception()
            {
                var result = await recruiterController.PostCommunicationHistoryRequestAsync(genericExceptionCommunicationHistory);
            }

            [TestMethod]
            public async Task RecruiterController_PostCommunicationHistoryRequestAsync_returns_OK_status_code_on_success()
            {
                var result = await recruiterController.PostCommunicationHistoryRequestAsync(successCommunicationHistory);
                Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [TestClass]
        public class RecruiterController_PostConnectionStatusAsync_Tests : RecruiterControllerTestsSetup
        {
            private ConnectionStatus permissionsExceptionConnectionStatus;
            private ConnectionStatus genericExceptionConnectionStatus;
            private ConnectionStatus successConnectionStatus;


            [TestInitialize]
            public void RecruiterController_PostConnectionStatusAsync_Initialize()
            {
                base.InitializeRecruiterController();

                permissionsExceptionConnectionStatus = new ConnectionStatus()
                {
                    Message = "PERMISSIONS_EXCEPTION"
                };
                genericExceptionConnectionStatus = new ConnectionStatus()
                {
                    Message = "EXCEPTION"
                };
                successConnectionStatus = new ConnectionStatus()
                {
                    Message = "SUCCESS"
                };

                recruiterServiceMock.Setup(svc => svc.PostConnectionStatusAsync(permissionsExceptionConnectionStatus)).Throws(new PermissionsException());
                recruiterServiceMock.Setup(svc => svc.PostConnectionStatusAsync(genericExceptionConnectionStatus)).Throws(new ApplicationException());
                recruiterServiceMock.Setup(svc => svc.PostConnectionStatusAsync(successConnectionStatus)).ReturnsAsync(successConnectionStatus);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostConnectionStatusAsync_returns_Forbidden_status_code_on_PermissionsException()
            {
                var result = await recruiterController.PostConnectionStatusAsync(permissionsExceptionConnectionStatus);
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task RecruiterController_PostConnectionStatusAsync_returns_BadRequest_status_code_on_non_permissions_Exception()
            {
                var result = await recruiterController.PostConnectionStatusAsync(genericExceptionConnectionStatus);
            }

            [TestMethod]
            public async Task RecruiterController_PostConnectionStatusAsync_returns_OK_status_code_on_success()
            {
                var result = await recruiterController.PostConnectionStatusAsync(successConnectionStatus);
                Assert.AreEqual(successConnectionStatus, result);
            }
        }

    }
}
