// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
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

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentWaiversControllerTests
    {
        [TestClass]
        public class StudentWaiversControllerTests_GetSectionWaivers
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

            private IStudentWaiverService waiverService;
            private Mock<IStudentWaiverService> waiverServiceMock;
            private StudentWaiversController waiversController;
            private List<StudentWaiver> waiverDtos;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                waiverServiceMock = new Mock<IStudentWaiverService>();
                waiverService = waiverServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;

                waiverDtos = BuildWaiverDtos();

                waiversController = new StudentWaiversController(waiverService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                waiversController = null;
                waiverService = null;
            }

            [TestMethod]
            public async Task GetSectionWaivers_ReturnsWaiverDtos()
            {
                waiverServiceMock.Setup(x => x.GetSectionStudentWaiversAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<StudentWaiver>>(waiverDtos));
                var waivers = await waiversController.GetSectionStudentWaiversAsync("SEC1");
                Assert.IsTrue(waivers is IEnumerable<Dtos.Student.StudentWaiver>);
                Assert.AreEqual(2, waivers.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionWaivers_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    waiverServiceMock.Setup(x => x.GetSectionStudentWaiversAsync(It.IsAny<string>())).Throws(new PermissionsException());
                    var waivers = await waiversController.GetSectionStudentWaiversAsync("SEC1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionWaivers_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
            {
                try
                {
                    waiverServiceMock.Setup(x => x.GetSectionStudentWaiversAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                    var waivers = await waiversController.GetSectionStudentWaiversAsync("SEC1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionWaivers_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    waiverServiceMock.Setup(x => x.GetSectionStudentWaiversAsync(It.IsAny<string>())).Throws(new ApplicationException());
                    var waivers = await waiversController.GetSectionStudentWaiversAsync("SEC1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetSectionWaivers_ColleagueSessionExpiredException_ReturnsHttpResponseException_Unauthorized()
            {
                try
                {
                    waiverServiceMock.Setup(x => x.GetSectionStudentWaiversAsync(It.IsAny<string>()))
                        .ThrowsAsync(new ColleagueSessionExpiredException("session expired"));
                    await waiversController.GetSectionStudentWaiversAsync("SEC1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, ex.Response.StatusCode);
                    throw;
                }
            }

            private List<Dtos.Student.StudentWaiver> BuildWaiverDtos()
            {
                List<StudentWaiver> waivers = new List<StudentWaiver>();

                var waiver1 = new StudentWaiver()
                {
                    Id = "1",
                    StudentId = "Student1",
                    SectionId = "SEC1",
                    ReasonCode = "OTHER",
                    RequisiteWaivers = new List<RequisiteWaiver>()
                };
                waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW1", Status = WaiverStatus.Denied });
                waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW2", Status = WaiverStatus.NotSelected });
                waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW3", Status = WaiverStatus.Waived });

                waivers.Add(waiver1);

                var waiver2 = new StudentWaiver()
                {
                    Id = "2",
                    StudentId = "Student2",
                    SectionId = "SEC1",
                    ReasonCode = "LIFE",
                    RequisiteWaivers = new List<RequisiteWaiver>()
                };
                waiver2.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW2", Status = WaiverStatus.Denied });
                waiver2.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW3", Status = WaiverStatus.Denied });

                waivers.Add(waiver2);

                return waivers;
            }
        }

        [TestClass]
        public class StudentWaiversControllerTests_GetStudentWaivers
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

            private IStudentWaiverService waiverService;
            private Mock<IStudentWaiverService> waiverServiceMock;
            private StudentWaiversController waiversController;
            private List<StudentWaiver> waiverDtos;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
                waiverServiceMock = new Mock<IStudentWaiverService>();
                waiverService = waiverServiceMock.Object;
                adapterRegistry = new Mock<IAdapterRegistry>().Object;
                logger = new Mock<ILogger>().Object;
                waiverDtos = BuildWaiverDtos();
                waiversController = new StudentWaiversController(waiverService, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                waiversController = null;
                waiverService = null;
            }

            [TestMethod]
            public async Task GetStudentWaivers_ReturnsWaiverDtos()
            {
                waiverServiceMock.Setup(x => x.GetStudentWaiversAsync(It.IsAny<string>())).Returns(Task.FromResult<IEnumerable<StudentWaiver>>(waiverDtos));
                var waivers = await waiversController.GetStudentWaiversAsync("Student1");
                Assert.IsTrue(waivers is IEnumerable<Dtos.Student.StudentWaiver>);
                Assert.AreEqual(2, waivers.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentWaivers_PermissionsException_ReturnsHttpResponseException_Forbidden()
            {
                try
                {
                    waiverServiceMock.Setup(x => x.GetStudentWaiversAsync(It.IsAny<string>())).Throws(new PermissionsException());
                    var waivers = await waiversController.GetStudentWaiversAsync("Student1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
                    throw ex;
                }
            }


            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentWaivers_AnyOtherException_ReturnsHttpResponseException_BadRequest()
            {
                try
                {
                    waiverServiceMock.Setup(x => x.GetStudentWaiversAsync(It.IsAny<string>())).Throws(new ApplicationException());
                    var waivers = await waiversController.GetStudentWaiversAsync("Student1");
                }
                catch (HttpResponseException ex)
                {
                    Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
                    throw ex;
                }
            }

            private List<Dtos.Student.StudentWaiver> BuildWaiverDtos()
            {
                List<StudentWaiver> waivers = new List<StudentWaiver>();

                var waiver1 = new StudentWaiver()
                {
                    Id = "1",
                    StudentId = "Student1",
                    SectionId = "SEC1",
                    ReasonCode = "OTHER",
                    RequisiteWaivers = new List<RequisiteWaiver>()
                };
                waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW1", Status = WaiverStatus.Denied });
                waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW2", Status = WaiverStatus.NotSelected });
                waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW3", Status = WaiverStatus.Waived });

                waivers.Add(waiver1);

                var waiver2 = new StudentWaiver()
                {
                    Id = "2",
                    StudentId = "Student1",
                    SectionId = "SEC2",
                    ReasonCode = "LIFE",
                    RequisiteWaivers = new List<RequisiteWaiver>()
                };
                waiver2.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW2", Status = WaiverStatus.Denied });
                waiver2.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW3", Status = WaiverStatus.Denied });

                waivers.Add(waiver2);

                return waivers;
            }
        }



        // TODO SSS: Figure out how to mock these post requests
        //    [TestClass][Ignore]
        //    public class WaiversControllerTests_PostSectionWaiver
        //    {
        //        private TestContext testContextInstance;

        //        /// <summary>
        //        ///Gets or sets the test context which provides
        //        ///information about and functionality for the current test run.
        //        ///</summary>
        //        public TestContext TestContext
        //        {
        //            get
        //            {
        //                return testContextInstance;
        //            }
        //            set
        //            {
        //                testContextInstance = value;
        //            }
        //        }

        //        private IWaiverService waiverService;
        //        private Mock<IWaiverService> waiverServiceMock;
        //        private WaiversController waiversController;
        //        private Waiver waiverDto;
        //        private IAdapterRegistry adapterRegistry;
        //        private ILogger logger;

        //        [TestInitialize]
        //        public void Initialize()
        //        {
        //            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
        //            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
        //            waiverServiceMock = new Mock<IWaiverService>();
        //            waiverService = waiverServiceMock.Object;
        //            adapterRegistry = new Mock<IAdapterRegistry>().Object;
        //            logger = new Mock<ILogger>().Object;

        //            waiverDto = BuildWaiverDto();

        //            waiversController = new WaiversController(waiverService, logger);
        //            waiversController.Request = new HttpRequestMessage();
        //            waiversController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        //            waiversController.Configuration = new HttpConfiguration();

        //        }

        //        [TestCleanup]
        //        public void Cleanup()
        //        {
        //            waiversController = null;
        //            waiverService = null;
        //        }

        //        [TestMethod]
        //        public void PostSectionWaiver_ReturnsWaiverDtoWithResourceLocator()
        //        {
        //            waiverServiceMock.Setup(x => x.CreateWaiver(It.IsAny<Waiver>())).Returns(waiverDto);
        //            var response = waiversController.PostWaiver(waiverDto);
        //            Assert.IsTrue(response is HttpResponseMessage);
        //        }

        //        [TestMethod]
        //        [ExpectedException(typeof(HttpResponseException))]
        //        public void PostSectionWaiver_PermissionsException_ReturnsHttpResponseException_Forbidden()
        //        {
        //            try
        //            {
        //                waiverServiceMock.Setup(x => x.CreateWaiver(It.IsAny<Waiver>())).Throws(new PermissionsException());
        //                var waivers = waiversController.PostWaiver(waiverDto);
        //            }
        //            catch (HttpResponseException ex)
        //            {
        //                Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, ex.Response.StatusCode);
        //                throw ex;
        //            }
        //        }

        //        [TestMethod]
        //        [ExpectedException(typeof(HttpResponseException))]
        //        public void PostSectionWaiver_KeyNotFoundException_ReturnsHttpResponseException_NotFound()
        //        {
        //            try
        //            {
        //                waiverServiceMock.Setup(x => x.CreateWaiver(It.IsAny<Waiver>())).Throws(new KeyNotFoundException());
        //                var waivers = waiversController.PostWaiver(waiverDto);
        //            }
        //            catch (HttpResponseException ex)
        //            {
        //                Assert.AreEqual(System.Net.HttpStatusCode.NotFound, ex.Response.StatusCode);
        //                throw ex;
        //            }
        //        }

        //        [TestMethod]
        //        [ExpectedException(typeof(HttpResponseException))]
        //        public void PostSectionWaiver_AnyOtherException_ReturnsHttpResponseException_BadRequest()
        //        {
        //            try
        //            {
        //                waiverServiceMock.Setup(x => x.GetSectionWaivers(It.IsAny<string>())).Throws(new ApplicationException());
        //                var waivers = waiversController.GetSectionWaivers("SEC1");
        //            }
        //            catch (HttpResponseException ex)
        //            {
        //                Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, ex.Response.StatusCode);
        //                throw ex;
        //            }
        //        }

        //        private Dtos.Student.Waiver BuildWaiverDto()
        //        {
        //            var waiver1 = new Waiver()
        //            {
        //                Id = "1",
        //                StudentId = "Student1",
        //                SectionId = "SEC1",
        //                ReasonCode = "OTHER",
        //                RequisiteWaivers = new List<RequisiteWaiver>()
        //            };
        //            waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW1", Status = WaiverStatus.Denied });
        //            waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW2", Status = WaiverStatus.NotSelected });
        //            waiver1.RequisiteWaivers.Add(new RequisiteWaiver() { RequisiteId = "RW3", Status = WaiverStatus.Waived });

        //            return waiver1;
        //        }
        //    }
        //}
    }
}