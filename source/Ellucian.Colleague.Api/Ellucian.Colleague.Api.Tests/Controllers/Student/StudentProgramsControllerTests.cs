// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Ellucian.Colleague.Api.Controllers;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student.Requirements;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Dtos.Student;
using System;
using Ellucian.Web.Security;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Tests.Controllers.Student
{
    [TestClass]
    public class StudentProgramsTests
    {
        [TestClass]
        public class StudentProgramsGetAsync
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

            private StudentProgramsController ProgramController;
            private Mock<IStudentProgramService> StudentProgramServiceMock;
            private IStudentProgramService studentprogramService;

            //private Mock<IProgramRepository> ProgramRepositoryMock;
            //private IProgramRepository ProgramRepository;

            //private Mock<IProgramRequirementsRepository> ProgramRequirementsRepositoryMock;
            //private IProgramRequirementsRepository ProgramRequirmentsRepository;

            //private IAdapterRegistry AdapterRegistry;

            //private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program> allPrograms;

            ILogger logger = new Mock<ILogger>().Object;

            [TestInitialize]
            public  void Initialize()
            {
                EllucianLicenseProvider.RefreshLicense(System.IO.Path.Combine(TestContext.TestDeploymentDir, "App_Data"));

                StudentProgramServiceMock = new Mock<IStudentProgramService>();
                studentprogramService = StudentProgramServiceMock.Object;

                ProgramController = new StudentProgramsController(studentprogramService, logger);
                
            }

            [TestCleanup]
            public void Cleanup()
            {
                ProgramController = null;
            }

            [TestMethod]
            public async Task StudentProgram_Get()
            {
                StudentProgram2 givenStudentProgram = new StudentProgram2();
                givenStudentProgram.StudentId = "0000011";
                givenStudentProgram.ProgramCode = "MATH.BA";
                givenStudentProgram.Location = "MC";
                StudentProgramServiceMock.Setup(svc => svc.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(givenStudentProgram));
                var studentProgram = await ProgramController.GetAsync(givenStudentProgram.StudentId, givenStudentProgram.ProgramCode);
                Assert.AreEqual(studentProgram.StudentId, givenStudentProgram.StudentId);
                Assert.AreEqual(studentProgram.ProgramCode, givenStudentProgram.ProgramCode);
                Assert.AreEqual(studentProgram.Location, givenStudentProgram.Location);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentId_Is_Null()
            {
                var studentProgram = await ProgramController.GetAsync(null,"MATH.BA");

            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ProgramCode_Is_Null()
            {

                var studentProgram = await ProgramController.GetAsync("0000111",null);
            }
            [TestMethod]
            [ExpectedException(typeof(HttpResponseException))]
            public async Task Permission_Exception()
            {
                StudentProgramServiceMock.Setup(svc => svc.GetAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new PermissionsException());
                var studentProgram = await ProgramController.GetAsync("000011", "MATH.BA");
            }
        }
      
    }
}
