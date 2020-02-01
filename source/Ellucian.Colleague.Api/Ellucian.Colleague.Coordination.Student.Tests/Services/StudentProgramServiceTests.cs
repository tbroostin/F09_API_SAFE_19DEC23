// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class StudentProgramServiceTests
    {
        private Ellucian.Colleague.Domain.Entities.Role viewStudentInformationRole = new Ellucian.Colleague.Domain.Entities.Role(1, "ViewStudentInformation");
        private Ellucian.Colleague.Domain.Entities.Role advisorAnyAdviseeRole = new Ellucian.Colleague.Domain.Entities.Role(2, "AnyAdvisees");
        private Ellucian.Colleague.Domain.Entities.Role advisorAssignedAdviseeRole = new Ellucian.Colleague.Domain.Entities.Role(2, "AssignedAdvisees");
        public abstract class CurrentUserSetup
        {

            public class AdvisorUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "STU1",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { "ViewStudentInformation", "AnyAdvisees", "AssignedAdvisees" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentProgramService_GetStudentProgramsByIdsAsync_Tests : StudentProgramServiceTests
        {
            private IEnumerable<string> studentIds;
            private bool includeInactivePrograms;
            private string term;
            private bool includeHistory;
            private IEnumerable<StudentProgram> studentPrograms;
            private IEnumerable<StudentAccess> studentAccesses;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;

            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;

            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;

            private ICurrentUserFactory currentUserFactory;

            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;

            private Mock<ILogger> loggerMock;
            private ILogger logger;

            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;

            private StudentProgramService service;

            [TestInitialize]
            public void StudentProgramService_GetStudentProgramsByIdsAsync_Initialize()
            {
                studentIds = new List<string>()
                {
                    "0001234",
                    "0001235",
                    "0001236"
                };
                includeInactivePrograms = false;
                term = null;
                includeHistory = false;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                var studentProgramDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentProgram, Ellucian.Colleague.Dtos.Student.StudentProgram2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(ar => ar.GetAdapter<StudentProgram, Dtos.Student.StudentProgram2>()).Returns(studentProgramDtoAdapter);
                adapterRegistry = adapterRegistryMock.Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;

                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;

                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                service = new StudentProgramService(adapterRegistry, studentRepository, studentProgramRepository, termRepository, currentUserFactory, roleRepository, logger, configurationRepository);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentProgramService_GetStudentProgramsByIdsAsync_null_StudentIds_throws_exception()
            {
                var programs = await service.GetStudentProgramsByIdsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentProgramService_GetStudentProgramsByIdsAsync_empty_StudentIds_throws_exception()
            {
                var programs = await service.GetStudentProgramsByIdsAsync(new List<string>());
            }

            [TestMethod]
            public async Task StudentProgramService_GetStudentProgramsByIdsAsync_permissions_exception_on_programs()
            {
                // Setup
                roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>());
                roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>());

                studentPrograms = new List<StudentProgram>()
                {
                    new StudentProgram("0001234", "PROG", "2018") { ProgramName = "No Start Date", StartDate = null, EndDate = DateTime.Today.AddMonths(6) }
                };
                studentProgramRepositoryMock.Setup(spr => spr.GetStudentProgramsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<Term>(), It.IsAny<bool>())).ReturnsAsync(studentPrograms);

                studentRepositoryMock.Setup(sr => sr.GetStudentAccessAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(null);

                service = new StudentProgramService(adapterRegistry, studentRepository, studentProgramRepository, termRepository, currentUserFactory, roleRepository, logger, configurationRepository);

                // Test
                var programs = await service.GetStudentProgramsByIdsAsync(studentIds);
                CollectionAssert.AreEqual(new List<Dtos.Student.StudentProgram2>(), programs.ToList());
                loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), "Unable to retrieve student academic program data."));
            }

            /// <summary>
            /// Authenticated user is an advisor with the "any" advisee permissions, requesting student program information for students who are/are not an assigned advisee.
            /// Expected Result: the student program data for each student is returned
            /// </summary>
            [TestMethod]
            public async Task StudentProgramService_GetStudentProgramsByIdsAsync_all_students_allowed_ANYADVISEE_permissions()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                studentPrograms = new List<StudentProgram>()
                {
                    new StudentProgram(studentIds.ElementAt(0), "PROG", "2018") { ProgramName = "No Start Date", StartDate = null, EndDate = DateTime.Today.AddMonths(6) },
                    new StudentProgram(studentIds.ElementAt(1), "PROG2", "2018") { ProgramName = "No End Date", StartDate = DateTime.Today.AddMonths(-6), EndDate = null },
                    new StudentProgram(studentIds.ElementAt(2), "PROG3", "2018") { ProgramName = "Start and End Date", StartDate = DateTime.Today.AddMonths(-6), EndDate = DateTime.Today.AddMonths(6) }

                };
                studentProgramRepositoryMock.Setup(spr => spr.GetStudentProgramsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<Term>(), It.IsAny<bool>())).ReturnsAsync(studentPrograms);

                studentAccesses = new List<StudentAccess>()
                {
                    new StudentAccess(studentIds.ElementAt(0)),
                    new StudentAccess(studentIds.ElementAt(1)),
                    new StudentAccess(studentIds.ElementAt(2)),
                };
                studentAccesses.ElementAt(0).AddAdvisement("NOT_CURRENT_USER", null, null, "MAJ");
                studentAccesses.ElementAt(1).AddAdvisement(currentUserFactory.CurrentUser.PersonId, null, null, "MAJ");
                studentAccesses.ElementAt(2).AddAdvisement(currentUserFactory.CurrentUser.PersonId, null, null, "MAJ");
                studentRepositoryMock.Setup(sr => sr.GetStudentAccessAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(studentAccesses);

                service = new StudentProgramService(adapterRegistry, studentRepository, studentProgramRepository, termRepository, currentUserFactory, roleRepository, logger, configurationRepository);

                // Test
                var programs = await service.GetStudentProgramsByIdsAsync(studentIds);
                Assert.AreEqual(3, programs.Count());
            }

            /// <summary>
            /// Authenticated user is an advisor with the VIEW.STUDENT.INFORMATION permissions, requesting student program information for several students.
            /// Expected Result: the student program data for each student is returned
            /// </summary>
            [TestMethod]
            public async Task StudentProgramService_GetStudentProgramsByIdsAsync_all_students_allowed_VIEWSTUDENTINFORMATION()
            {
                // Setup
                viewStudentInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { viewStudentInformationRole });
                roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { viewStudentInformationRole });

                studentPrograms = new List<StudentProgram>()
                {
                    new StudentProgram(studentIds.ElementAt(0), "PROG", "2018") { ProgramName = "No Start Date", StartDate = null, EndDate = DateTime.Today.AddMonths(6) },
                    new StudentProgram(studentIds.ElementAt(1), "PROG2", "2018") { ProgramName = "No End Date", StartDate = DateTime.Today.AddMonths(-6), EndDate = null },
                    new StudentProgram(studentIds.ElementAt(2), "PROG3", "2018") { ProgramName = "Start and End Date", StartDate = DateTime.Today.AddMonths(-6), EndDate = DateTime.Today.AddMonths(6) }

                };
                studentProgramRepositoryMock.Setup(spr => spr.GetStudentProgramsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<Term>(), It.IsAny<bool>())).ReturnsAsync(studentPrograms);

                studentAccesses = new List<StudentAccess>()
                {
                    new StudentAccess(studentIds.ElementAt(0)),
                    new StudentAccess(studentIds.ElementAt(1)),
                    new StudentAccess(studentIds.ElementAt(2)),
                };
                studentAccesses.ElementAt(0).AddAdvisement(currentUserFactory.CurrentUser.PersonId, null, null, "MAJ");
                studentAccesses.ElementAt(1).AddAdvisement(currentUserFactory.CurrentUser.PersonId, null, null, "MAJ");
                studentAccesses.ElementAt(2).AddAdvisement("NOT_CURRENT_USER", null, null, "MAJ");
                studentRepositoryMock.Setup(sr => sr.GetStudentAccessAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(studentAccesses);

                service = new StudentProgramService(adapterRegistry, studentRepository, studentProgramRepository, termRepository, currentUserFactory, roleRepository, logger, configurationRepository);

                // Test
                var programs = await service.GetStudentProgramsByIdsAsync(studentIds);
                Assert.AreEqual(3, programs.Count());
            }

            /// <summary>
            /// Authenticated user is an advisor with the "assigned" advisee permissions, requesting student program information for a student who is not an assigned advisee.
            /// Expected Result: the student program data for the unassigned advisee is not returned
            /// </summary>
            [TestMethod]
            public async Task StudentProgramService_GetStudentProgramsByIdsAsync_students_not_allowed_ASSIGNEDADVISEES()
            {
                // Setup
                advisorAssignedAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                advisorAssignedAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                advisorAssignedAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                advisorAssignedAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAssignedAdviseeRole });
                roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAssignedAdviseeRole });

                studentPrograms = new List<StudentProgram>()
                {
                    new StudentProgram(studentIds.ElementAt(2), "PROG3", "2018") { ProgramName = "Start and End Date", StartDate = DateTime.Today.AddMonths(-6), EndDate = DateTime.Today.AddMonths(6) }

                };
                studentProgramRepositoryMock.Setup(spr => spr.GetStudentProgramsByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>(), It.IsAny<Term>(), It.IsAny<bool>())).ReturnsAsync(studentPrograms);

                studentAccesses = new List<StudentAccess>()
                {
                    new StudentAccess(studentIds.ElementAt(2)),
                };
                studentAccesses.ElementAt(0).AddAdvisement("NOT_CURRENT_USER", null, null, "MAJ");
                studentRepositoryMock.Setup(sr => sr.GetStudentAccessAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(studentAccesses);

                service = new StudentProgramService(adapterRegistry, studentRepository, studentProgramRepository, termRepository, currentUserFactory, roleRepository, logger, configurationRepository);

                // Test
                var programs = await service.GetStudentProgramsByIdsAsync(studentIds);
                Assert.AreEqual(0, programs.Count());
                loggerMock.Verify(l => l.Error(It.IsAny<PermissionsException>(), "Unable to retrieve student academic program data."));
            }
        }

        [TestClass]
        public class StudentProgramServiceTest_AddStudentProgram_Tests : StudentProgramServiceTests
        {
            private Dtos.Student.StudentAcademicProgram addProgram;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            private StudentProgram studentProgram;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;

            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;

            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;

            private ICurrentUserFactory currentUserFactory;

            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;

            private Mock<ILogger> loggerMock;
            private ILogger logger;

            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;

            private StudentProgramService service;

            [TestInitialize]
            public void StudentProgramService_AddStudentProgram_Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;

                var addProgramDtoAdapter = new Student.Adapters.StudentAcademicProgramEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(ar => ar.GetAdapter<Dtos.Student.StudentAcademicProgram, StudentAcademicProgram>()).Returns(addProgramDtoAdapter);

                var programDtoAdapter = new AutoMapperAdapter<StudentProgram, Dtos.Student.StudentProgram2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(ar => ar.GetAdapter<StudentProgram, Dtos.Student.StudentProgram2>()).Returns(programDtoAdapter);

                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;

                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;

                studentProgram = new StudentProgram("0001234", "PROG", "2018")
                {
                    ProgramName = "No Start Date",
                    StartDate = null,
                    EndDate = DateTime.Today.AddMonths(6)
                };
                //studentProgramRepositoryMock.Setup(spr => spr.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentProgram);

                // Mock return from the repo
                studentProgramRepositoryMock.Setup(repo => repo.AddStudentProgram(It.IsAny<StudentAcademicProgram>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(studentProgram);

                service = new StudentProgramService(adapterRegistryMock.Object, studentRepository, studentProgramRepository, termRepository, currentUserFactory, roleRepository, logger, configurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                service = null;
                adapterRegistryMock = null;
                adapterRegistry = null;
                logger = null;
                roleRepositoryMock = null;
                roleRepository = null;
                currentUserFactory = null;
                studentProgramRepositoryMock = null;
                studentProgramRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddStudentProgramAsync_NullArgument()
            {
                var serviceResult = await service.AddStudentProgram(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AddStudentProgramAsync_InvalidArgument_NullSectionId()
            {
                var serviceResult = await service.AddStudentProgram(new Dtos.Student.StudentAcademicProgram() { StudentId = "" });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AddStudentProgramAsync_NoPermission()
            {
                addProgram = new Dtos.Student.StudentAcademicProgram
                {
                    StudentId = "studentId",
                    AcadamicProgramId = "AddProg1",
                    ActivePrograms = null,
                    CatalogYear = "2012",
                    Department = "",
                    Location = "",
                    StartDate = DateTime.Now.ToShortDateString()
                };

                // Setup
                roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>());
                roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>());
                // Test
                var programs = await service.AddStudentProgram(addProgram);
            }

            [TestMethod]
            public async Task AddStudentProgram_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                addProgram = new Dtos.Student.StudentAcademicProgram
                {
                    StudentId = "studentId",
                    AcadamicProgramId = "AddProg1",
                    ActivePrograms = null,
                    CatalogYear = "2012",
                    Department = "",
                    Location = "",
                    StartDate = DateTime.Now.ToShortDateString()
                };

                var serviceResult = await service.AddStudentProgram(addProgram);
                Assert.IsNotNull(serviceResult);
                Assert.AreEqual("PROG", serviceResult.ProgramCode);
            }
        }

        [TestClass]
        public class StudentProgramServiceTest_UpdateStudentProgram_Tests : StudentProgramServiceTests
        {
            private Dtos.Student.StudentAcademicProgram updateProgram;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;

            private StudentProgram studentProgram;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;

            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;

            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;

            private ICurrentUserFactory currentUserFactory;

            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;

            private Mock<ILogger> loggerMock;
            private ILogger logger;

            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;

            private StudentProgramService service;

            [TestInitialize]
            public void StudentProgramService_UpdateStudentProgram_Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;

                var addProgramDtoAdapter = new Student.Adapters.StudentAcademicProgramEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
                adapterRegistryMock.Setup(ar => ar.GetAdapter<Dtos.Student.StudentAcademicProgram, StudentAcademicProgram>()).Returns(addProgramDtoAdapter);

                var programDtoAdapter = new AutoMapperAdapter<StudentProgram, Dtos.Student.StudentProgram2>(adapterRegistry, logger);
                adapterRegistryMock.Setup(ar => ar.GetAdapter<StudentProgram, Dtos.Student.StudentProgram2>()).Returns(programDtoAdapter);

                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;

                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                studentProgram = new StudentProgram("0001234", "PROG", "2018")
                {
                    ProgramName = "No Start Date",
                    StartDate = null,
                    EndDate = DateTime.Today.AddMonths(6)
                };
                //studentProgramRepositoryMock.Setup(spr => spr.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(studentProgram);

                // Mock return from the repo
                studentProgramRepositoryMock.Setup(repo => repo.UpdateStudentProgram(It.IsAny<StudentAcademicProgram>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(studentProgram);
              
                service = new StudentProgramService(adapterRegistryMock.Object, studentRepository, studentProgramRepository, termRepository, currentUserFactory, roleRepository, logger, configurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                service = null;
                adapterRegistryMock = null;
                adapterRegistry = null;
                logger = null;
                roleRepositoryMock = null;
                roleRepository = null;
                currentUserFactory = null;
                studentProgramRepositoryMock = null;
                studentProgramRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateStudentProgramAsync_NullArgument()
            {
                var serviceResult = await service.UpdateStudentProgram(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdatetudentProgramAsync_InvalidArgument_NullSectionId()
            {
                var serviceResult = await service.UpdateStudentProgram(new Dtos.Student.StudentAcademicProgram() { StudentId = "" });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateStudentProgramAsync_NoPermission()
            {
                updateProgram = new Dtos.Student.StudentAcademicProgram
                {
                    StudentId = "studentId",
                    AcadamicProgramId = "AddProg1",
                    ActivePrograms = null,
                    CatalogYear = "2012",
                    Department = "",
                    Location = "",
                    StartDate = DateTime.Now.ToShortDateString()
                };

                // Setup
                roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>());
                roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>());
                // Test
                var programs = await service.UpdateStudentProgram(updateProgram);
            }

            [TestMethod]
            public async Task UpdateStudentProgram_Success()
            {
                // Setup
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                advisorAnyAdviseeRole.AddPermission(new Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });
                roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorAnyAdviseeRole });

                updateProgram = new Dtos.Student.StudentAcademicProgram
                {
                    StudentId = "studentId",
                    AcadamicProgramId = "AddProg1",
                    ActivePrograms = null,
                    CatalogYear = "2011",
                    Department = "",
                    Location = "",
                    StartDate = DateTime.Now.ToShortDateString()
                };

                var serviceResult = await service.UpdateStudentProgram(updateProgram);
                Assert.IsNotNull(serviceResult);
                Assert.AreEqual("PROG", serviceResult.ProgramCode);
            }
        }
    }
}