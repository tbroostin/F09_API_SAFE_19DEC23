// Copyright 2020 Ellucian Company L.P. and its affiliates.
using Moq;
using System;
using slf4net;
using System.Linq;
using System.Web.Http;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using System.Collections.Generic;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Dtos.Student.TransferWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Student.Services;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentTransferWorkControllerTests
    {
        [TestClass]
        public class StudentTransferWorkController_GetStudentTransferWorkAsync_Tests
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

            private StudentTransferWorkController transferWorkController;
            private Mock<ITransferWorkService> transferWorkServiceMock;
            private ITransferWorkService transferWorkService;
            private ILogger logger;

            private List<TransferEquivalencies> transferEquivalencies;

            [TestInitialize]
            public void Initialize()
            {
                LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                transferWorkServiceMock = new Mock<ITransferWorkService>();
                transferWorkService = transferWorkServiceMock.Object;
                logger = new Mock<ILogger>().Object;

                List<ExternalCourseWork> externalCourseWork = new List<ExternalCourseWork>()
                {
                    new  ExternalCourseWork()
                    {
                        EndDate = null,
                        Course = "ENGL 100",
                        Title = "Engl 100",
                        Credits = Convert.ToDecimal(3),
                        GradeId = "14",
                        Score = 0
                    }
                };

                List<ExternalNonCourseWork> externalNonCourseWork = new List<ExternalNonCourseWork>()
                {
                    new ExternalNonCourseWork()
                    {
                        Name = "ENGL-101",
                        Title = "ENGL-101",
                        GradeId = "14",
                        Score = Convert.ToDecimal(3),
                        Status = "Active",
                        EndDate = null
                    }
                };

                List<EquivalentCourseCredit> equivalentCoursCredit = new List<EquivalentCourseCredit>()
                {
                    new EquivalentCourseCredit()
                    {
                        CourseId = "626",
                        Name = "ENGL-100",
                        Title = "The Short Story",
                        Credits = Convert.ToDecimal(3),
                        GradeId = "14",
                        AcademicLevelId = "UG",
                        CreditType = "TR",
                        CreditStatus = "TR"
                    }
                };

                List<EquivalentGeneralCredit> equivalentGeneralCredit = new List<EquivalentGeneralCredit>()
                {
                    new EquivalentGeneralCredit()
                    {
                        Name = "Math-english",
                        Title = "Math-mathenglish",
                        Subject = "HIST",
                        CourseLevel = "100",
                        AcademicLevelId = "UG",
                        CreditStatus = "TR",
                        CreditType = "TR",
                        Credits = Convert.ToDecimal(3),
                        DepartmentCodes = new List<string>() { "MATH" }
                    }
                };

                List<Equivalency> equivalency = new List<Equivalency>()
                {
                    new Equivalency()
                    {
                        AcademicPrograms = new List<string>() { "BUSN.BA" },
                        ExternalCourseWork = externalCourseWork,
                        EquivalentCourseCredits = equivalentCoursCredit,
                        EquivalentGeneralCredits = equivalentGeneralCredit,
                        ExternalNonCourseWork = externalNonCourseWork
                    }
                };


                //EquivalentCoursCredit equivalentCoursCredit = new EquivalentCoursCredit("626", "ENGL-100", "The Short Story", Convert.ToDecimal(3), "14", "UG", "TR", "TR");
                //EquivalentGeneralCredit equivalentGeneralCredit = new EquivalentGeneralCredit("Math-mathenglish", "Math-mathenglish", "HIST", "100", Convert.ToDecimal(3), "UG", "TR", "TR", new List<string> { "MATH" });

                //Equivalency equivalency = new Equivalency();
                //equivalency.AddAcademicProgram("BUSN.BA");
                //equivalency.AddExternalCourseWork(externalCourseWork);
                //equivalency.AddExternalNonCourseWork(externalNonCourseWork);
                //equivalency.AddEquivalentCourseCredit(equivalentCoursCredit);
                //equivalency.AddEquivalentGeneralCredit(equivalentGeneralCredit);

                //transferEquivalency.AddTransferCourseWork(equivalency);
                //transferEquivalencies = new List<TransferEquivalencies>();
                //transferEquivalencies.Add(transferEquivalency);

                transferEquivalencies = new List<TransferEquivalencies>()
                {
                    new TransferEquivalencies()
                    {
                         Equivalencies = equivalency,
                         InstitutionId = "0000129"
                    }
                };

                transferWorkController = new StudentTransferWorkController(transferWorkService, logger);
                transferWorkController.Request = new System.Net.Http.HttpRequestMessage() { RequestUri = new Uri("http://localhost") };
                transferWorkController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            }

            [TestCleanup]
            public void Cleanup()
            {
                transferWorkController = null;
                transferWorkService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentTransferWorkAsync_NullStudentId()
            {
                transferWorkServiceMock.Setup(x => x.GetStudentTransferWorkAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var transferSummaryDtos = await transferWorkController.GetStudentTransferWorkAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentTransferWorkAsync_PermissionsException()
            {
                transferWorkServiceMock.Setup(x => x.GetStudentTransferWorkAsync(It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                var transferSummaryDtos = await transferWorkController.GetStudentTransferWorkAsync("31");
            }

            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task GetStudentTransferWorkAsync_Exception()
            {
                transferWorkServiceMock.Setup(x => x.GetStudentTransferWorkAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var transferSummaryDtos = await transferWorkController.GetStudentTransferWorkAsync("");
            }

            [TestMethod]
            public async Task GetStudentTransferWorkAsync_ReturnsSuccess()
            {
                transferWorkServiceMock.Setup(s => s.GetStudentTransferWorkAsync(It.IsAny<string>())).ReturnsAsync(transferEquivalencies);
                var transferSummaryDtos = await transferWorkController.GetStudentTransferWorkAsync("31");
                Assert.IsNotNull(transferSummaryDtos);
                Assert.AreEqual(transferEquivalencies.Count(), transferSummaryDtos.Count());
                var tsdto = transferSummaryDtos.ToList();
                Assert.AreEqual(transferEquivalencies[0].InstitutionId, tsdto[0].InstitutionId);
                Assert.AreEqual(transferEquivalencies[0].Equivalencies.Count(), tsdto[0].Equivalencies.Count());
            }
        }
    }
}
