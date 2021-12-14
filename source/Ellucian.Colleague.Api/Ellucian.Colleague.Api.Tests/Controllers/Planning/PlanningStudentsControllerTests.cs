// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Controllers.Planning;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Planning;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Planning
{
    [TestClass]
    public class PlanningStudentsControllerTests
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

        private IProgramEvaluationService programEvaluationService;
        private IStudentProgramRepository studentProgramRepo;
        private IPlanningStudentService planningStudentService;
        private IAdapterRegistry adapterRegistry;
        private PlanningStudentsController planningStudentsController;

        Mock<IProgramEvaluationService> programEvaluationServiceMock = new Mock<IProgramEvaluationService>();
        Mock<IStudentProgramRepository> studentProgramRepoMock = new Mock<IStudentProgramRepository>();
        Mock<IPlanningStudentService> planningStudentServiceMock = new Mock<IPlanningStudentService>();
        Mock<IAdapterRegistry> adapterRegistryMock = new Mock<IAdapterRegistry>();

        private ILogger logger;

        [TestInitialize]
        public void Initialize()
        {
            LicenseHelper.CopyLicenseFile(TestContext.TestDeploymentDir);
            EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));
            programEvaluationService = programEvaluationServiceMock.Object;
            studentProgramRepo = studentProgramRepoMock.Object;
            planningStudentService = planningStudentServiceMock.Object;
            adapterRegistry = adapterRegistryMock.Object;
            logger = new Mock<ILogger>().Object;

            // mock controller
            planningStudentsController = new PlanningStudentsController(adapterRegistry, programEvaluationService, planningStudentService, logger);
        }

        [TestCleanup]
        public void Cleanup()
        {
            planningStudentsController = null;
            adapterRegistry = null;
            studentProgramRepo = null;
            programEvaluationService = null;
        }

        [TestMethod]
        public async Task GetEvaluation()
        {
            // arrange
            var studentId = "0000001";
            var programCode = "ENGL.BA";
            var programEvaluation = new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013");
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), null)).Returns(Task.FromResult((new List<Domain.Student.Entities.ProgramEvaluation>() { programEvaluation }).AsEnumerable()));
            var programEvaluationDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation>()).Returns(programEvaluationDtoAdapter);

            // act
            var result = await planningStudentsController.GetEvaluationAsync(studentId, programCode);

            // assert
            Assert.IsTrue(result is ProgramEvaluation);
        }

        [TestMethod]
        public async Task QueryEvaluations()
        {
            // arrange
            var studentId = "0000001";
            var programCodes = new List<string>() { "ENGL.BA", "MATH.BA" };
            var programEvaluations = new List<Domain.Student.Entities.ProgramEvaluation>() {
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013"),
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "MATH.BA", "2014")
            };
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), null)).Returns(Task.FromResult(programEvaluations.AsEnumerable()));
            var programEvaluationDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation>()).Returns(programEvaluationDtoAdapter);

            // act
            var result = await planningStudentsController.QueryEvaluationsAsync(studentId, programCodes);

            // assert
            Assert.IsTrue(result is List<ProgramEvaluation>);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task GetEvaluationNotices()
        {
            // arrange
            var studentId = "0000001";
            var programCode = "ENGL.BA";
            var text1 = new List<string>() { "line1", "line2" };
            var type1 = Dtos.Student.EvaluationNoticeType.StudentProgram;
            var noticeDto1 = new Dtos.Student.EvaluationNotice() { StudentId = studentId, ProgramCode = programCode, Text = text1, Type = type1 };
            var text2 = new List<string>() { "line3", "line4" };
            var type2 = Dtos.Student.EvaluationNoticeType.End;
            var noticeDto2 = new Dtos.Student.EvaluationNotice() { StudentId = studentId, ProgramCode = programCode, Text = text2, Type = type2 };
            programEvaluationServiceMock.Setup(svc => svc.GetEvaluationNoticesAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new List<Dtos.Student.EvaluationNotice>() { noticeDto1, noticeDto2 }.AsEnumerable()));

            // act
            var result = await planningStudentsController.GetEvaluationNoticesAsync("0000001", "XYZ");

            // assert
            Assert.IsTrue(result is List<Dtos.Student.EvaluationNotice>);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task GetEvaluationNotices_ConvertsExceptionToHttpResponse()
        {
            // arrange--Mock permissions exception from student service Get
            programEvaluationServiceMock.Setup(svc => svc.GetEvaluationNoticesAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new PermissionsException());

            // act
            var result = await planningStudentsController.GetEvaluationNoticesAsync("NOPERMISSION", "X");
        }

        [TestMethod]
        public async Task QueryPlanningStudents()
        {
            var planningStudentCriteria = new PlanningStudentCriteria();
            planningStudentCriteria.StudentIds = new List<string>() { "0004723", "0011902" };
            PrivacyWrapper<IEnumerable<Dtos.Student.PlanningStudent>> privacyPlanningStudent = new PrivacyWrapper<IEnumerable<Dtos.Student.PlanningStudent>>();
            var planningStudents = new List<Dtos.Student.PlanningStudent>() {
            new Dtos.Student.PlanningStudent(){Id="0004723"},
            new Dtos.Student.PlanningStudent(){Id="0011902"}
            };
            privacyPlanningStudent.Dto = planningStudents;
             
            planningStudentServiceMock.Setup(s => s.QueryPlanningStudentsAsync(It.IsAny<List<string>>())).Returns(Task.FromResult(privacyPlanningStudent));
            var result = await planningStudentsController.QueryPlanningStudentsAsync(planningStudentCriteria);
            Assert.IsTrue(result is List<Dtos.Student.PlanningStudent>);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryPlanningStudents_ConvertsPermissionExceptionToHttpResponse()
        {
            var planningStudentCriteria = new PlanningStudentCriteria();
            planningStudentCriteria.StudentIds = new List<string>() { "0004723", "0011902" };
            var planningStudents = new List<Dtos.Student.PlanningStudent>() {
            new Dtos.Student.PlanningStudent(){Id="0004723"},
            new Dtos.Student.PlanningStudent(){Id="0011902"}
            };
            planningStudentServiceMock.Setup(svc => svc.QueryPlanningStudentsAsync(It.IsAny<List<string>>())).Throws(new PermissionsException());
            var result = await planningStudentsController.QueryPlanningStudentsAsync(planningStudentCriteria);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpResponseException))]
        public async Task QueryPlanningStudents_ConvertsArgumentExceptionToHttpResponse()
        {
            var planningStudentCriteria = new PlanningStudentCriteria();
            planningStudentCriteria.StudentIds = new List<string>() { "0004723", "0011902" };
            var planningStudents = new List<Dtos.Student.PlanningStudent>() {
            new Dtos.Student.PlanningStudent(){Id="0004723"},
            new Dtos.Student.PlanningStudent(){Id="0011902"}
            };
            planningStudentServiceMock.Setup(svc => svc.QueryPlanningStudentsAsync(It.IsAny<List<string>>())).Throws(new ArgumentException());
            var result = await planningStudentsController.QueryPlanningStudentsAsync(planningStudentCriteria);
        }

        //Tests for versioned Controller Methods
        [TestMethod]
        public async Task GetEvaluation2Async()
        {
            // arrange
            var studentId = "0000001";
            var programCode = "ENGL.BA";
            var programEvaluation = new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013");
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), null)).Returns(Task.FromResult((new List<Domain.Student.Entities.ProgramEvaluation>() { programEvaluation }).AsEnumerable()));
            var programEvaluationDto2Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>()).Returns(programEvaluationDto2Adapter);

            // act
            var result = await planningStudentsController.GetEvaluation2Async(studentId, programCode);

            // assert
            Assert.IsTrue(result is ProgramEvaluation2);
        }

        [TestMethod]
        public async Task QueryEvaluations2Async()
        {
            // arrange
            var studentId = "0000001";
            var programCodes = new List<string>() { "ENGL.BA", "MATH.BA" };
            var programEvaluations = new List<Domain.Student.Entities.ProgramEvaluation>() {
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013"),
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "MATH.BA", "2014")
            };
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), null)).Returns(Task.FromResult(programEvaluations.AsEnumerable()));
            var programEvaluationDto2Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation2>()).Returns(programEvaluationDto2Adapter);

            // act
            var result = await planningStudentsController.QueryEvaluations2Async(studentId, programCodes);

            // assert
            Assert.IsTrue(result is List<ProgramEvaluation2>);
            Assert.AreEqual(2, result.Count());
        }

        //Tests for versioned Controller Methods
        [TestMethod]
        public async Task GetEvaluation3Async()
        {
            // arrange
            var studentId = "0000001";
            var programCode = "ENGL.BA";
            var programEvaluation = new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013");
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), null)).Returns(Task.FromResult((new List<Domain.Student.Entities.ProgramEvaluation>() { programEvaluation }).AsEnumerable()));
            var programEvaluationDto3Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>()).Returns(programEvaluationDto3Adapter);

            // act
            var result = await planningStudentsController.GetEvaluation3Async(studentId, programCode, null);

            // assert
            Assert.IsTrue(result is ProgramEvaluation3);
        }
  

        [TestMethod]
        public async Task GetEvaluation3WithCatalogAsync()
        {
            // arrange
            var studentId = "0000001";
            var programCode = "ENGL.BA";
            var programEvaluation = new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013");
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).Returns(Task.FromResult((new List<Domain.Student.Entities.ProgramEvaluation>() { programEvaluation }).AsEnumerable()));
            var programEvaluationDto3Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>()).Returns(programEvaluationDto3Adapter);

            // act
            var result = await planningStudentsController.GetEvaluation3Async(studentId, programCode, null);

            // assert
            Assert.IsTrue(result is ProgramEvaluation3);
        }


        [TestMethod]
        public async Task QueryEvaluations3Async()
        {
            // arrange
            var studentId = "0000001";
            var programCodes = new List<string>() { "ENGL.BA", "MATH.BA" };
            var programEvaluations = new List<Domain.Student.Entities.ProgramEvaluation>() {
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013"),
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "MATH.BA", "2014")
            };
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), null)).Returns(Task.FromResult(programEvaluations.AsEnumerable()));
            var programEvaluationDto3Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>()).Returns(programEvaluationDto3Adapter);

            // act
            var result = await planningStudentsController.QueryEvaluations3Async(studentId, programCodes);

            // assert
            Assert.IsTrue(result is List<ProgramEvaluation3>);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task QueryEvaluations3WithCatalogAsync()
        {
            // arrange
            var studentId = "0000001";
            var programCodes = new List<string>() { "ENGL.BA", "MATH.BA" };
            var programEvaluations = new List<Domain.Student.Entities.ProgramEvaluation>() {
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013"),
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "MATH.BA", "2014")
            };
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).Returns(Task.FromResult(programEvaluations.AsEnumerable()));
            var programEvaluationDto3Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation3>()).Returns(programEvaluationDto3Adapter);

            // act
            var result = await planningStudentsController.QueryEvaluations3Async(studentId, programCodes);

            // assert
            Assert.IsTrue(result is List<ProgramEvaluation3>);
            Assert.AreEqual(2, result.Count());
        }

        //Tests for versioned Controller Methods for version 4
        [TestMethod]
        public async Task GetEvaluation4Async()
        {
            // arrange
            var studentId = "0000001";
            var programCode = "ENGL.BA";
            var programEvaluation = new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013");
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), null)).Returns(Task.FromResult((new List<Domain.Student.Entities.ProgramEvaluation>() { programEvaluation }).AsEnumerable()));
            var programEvaluationDto4Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>()).Returns(programEvaluationDto4Adapter);

            // act
            var result = await planningStudentsController.GetEvaluation4Async(studentId, programCode, null);

            // assert
            Assert.IsTrue(result is ProgramEvaluation4);
        }


        [TestMethod]
        public async Task GetEvaluation4WithCatalogAsync()
        {
            // arrange
            var studentId = "0000001";
            var programCode = "ENGL.BA";
            var programEvaluation = new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013");
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).Returns(Task.FromResult((new List<Domain.Student.Entities.ProgramEvaluation>() { programEvaluation }).AsEnumerable()));
            var programEvaluationDto4Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>()).Returns(programEvaluationDto4Adapter);

            // act
            var result = await planningStudentsController.GetEvaluation4Async(studentId, programCode, null);

            // assert
            Assert.IsTrue(result is ProgramEvaluation4);
        }


        [TestMethod]
        public async Task QueryEvaluations4Async()
        {
            // arrange
            var studentId = "0000001";
            var programCodes = new List<string>() { "ENGL.BA", "MATH.BA" };
            var programEvaluations = new List<Domain.Student.Entities.ProgramEvaluation>() {
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013"),
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "MATH.BA", "2014")
            };
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), null)).Returns(Task.FromResult(programEvaluations.AsEnumerable()));
            var programEvaluationDto4Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>()).Returns(programEvaluationDto4Adapter);

            // act
            var result = await planningStudentsController.QueryEvaluations4Async(studentId, programCodes);

            // assert
            Assert.IsTrue(result is List<ProgramEvaluation4>);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public async Task QueryEvaluations4WithCatalogAsync()
        {
            // arrange
            var studentId = "0000001";
            var programCodes = new List<string>() { "ENGL.BA", "MATH.BA" };
            var programEvaluations = new List<Domain.Student.Entities.ProgramEvaluation>() {
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "ENGL.BA", "2013"),
                new Domain.Student.Entities.ProgramEvaluation(new List<Domain.Student.Entities.AcademicCredit>(), "MATH.BA", "2014")
            };
            programEvaluationServiceMock.Setup(svc => svc.EvaluateAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>())).Returns(Task.FromResult(programEvaluations.AsEnumerable()));
            var programEvaluationDto4Adapter = new AutoMapperAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>(adapterRegistry, logger);
            adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.ProgramEvaluation, Dtos.Planning.ProgramEvaluation4>()).Returns(programEvaluationDto4Adapter);

            // act
            var result = await planningStudentsController.QueryEvaluations4Async(studentId, programCodes);

            // assert
            Assert.IsTrue(result is List<ProgramEvaluation4>);
            Assert.AreEqual(2, result.Count());
        }

    }
}
