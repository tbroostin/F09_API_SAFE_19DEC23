// Copyright 2018 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentSectionAttendancesServiceTests
    {
        // Sets up a Current user that is a faculty
        public abstract class CurrentUserSetup
        {
            protected Role facultyRole = new Role(105, "Faculty");
            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Samwise",
                            PersonId = "0000123",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Samwise",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
          
        }


        [TestClass]
        public class QueryStudentSectionAttendancesAsync : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentAttendanceRepository> studentAttendanceRepositoryMock;
            private IStudentAttendanceRepository studentAttendanceRepository;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private IStudentSectionAttendancesService studentAttendanceService;
            private Domain.Student.Entities.StudentSectionsAttendances studentAttendancesDataFor1Section;
            private Domain.Student.Entities.StudentSectionsAttendances studentAttendancesDataFor2Section;
            private Domain.Student.Entities.StudentSectionsAttendances studentAttendancesDataForNoKeys;
            private Domain.Student.Entities.StudentSectionsAttendances studentAttendancesDataForAll;
            private List<Domain.Student.Entities.Section> sectionData;
            private List<Domain.Student.Entities.Section> crossListedSectionData;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private DateTime attendanceDate;
            private string studentId = "0000123";

            [TestInitialize]
            public void Initialize()
            {
                attendanceDate = new DateTime(2017, 5, 1);
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                studentAttendanceRepositoryMock = new Mock<IStudentAttendanceRepository>();
                studentAttendanceRepository = studentAttendanceRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                // Mock student Attendance response
                studentAttendancesDataFor1Section = BuildStudentAttendancesRepositoryResponseFor1Section();
                studentAttendancesDataFor2Section = BuildStudentAttendancesRepositoryResponseFor2Section();
                studentAttendancesDataForNoKeys = BuildStudentAttendancesRepositoryResponseForNoKeys();
                studentAttendancesDataForAll = BuildStudentAttendancesRepositoryResponseFor2Section();

                studentAttendanceRepositoryMock.Setup(repository => repository.GetStudentSectionAttendancesAsync("0000123", new List<string>() { "1" })).Returns(Task.FromResult(studentAttendancesDataFor1Section));
                studentAttendanceRepositoryMock.Setup(repository => repository.GetStudentSectionAttendancesAsync("0000123", new List<string>() {"1", "2", "3" })).Returns(Task.FromResult(studentAttendancesDataFor2Section));

                //mock attendance as null
                studentAttendanceRepositoryMock.Setup(repository => repository.GetStudentSectionAttendancesAsync("0000123", new List<string>() { "SEC1" })).ReturnsAsync(null);

                //mock empty attendance
                studentAttendanceRepositoryMock.Setup(repository => repository.GetStudentSectionAttendancesAsync("0000123", new List<string>() { "SEC2" })).Returns(Task.FromResult(studentAttendancesDataForNoKeys));

                //mock when null sectionIds are passed 
                studentAttendanceRepositoryMock.Setup(repository => repository.GetStudentSectionAttendancesAsync("0000123", null)).Returns(Task.FromResult(studentAttendancesDataForAll));


                // Mock Adapters
                var studentSectionsAttendancesDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentSectionsAttendances, Dtos.Student.StudentSectionsAttendances>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.StudentSectionsAttendances, Dtos.Student.StudentSectionsAttendances>()).Returns(studentSectionsAttendancesDtoAdapter);

                var AttendanceDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>()).Returns(AttendanceDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                studentAttendanceService = new StudentSectionAttendancesService(adapterRegistry, studentAttendanceRepository, studentRepository, sectionRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                studentAttendanceRepository = null;
                studentRepository = null;
                sectionRepository = null;
                roleRepository = null;
                studentAttendanceService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentSectionAttendancesAsync_ThrowsExceptionIfCriteriaNull()
            {
                var studentAttendanceDto = await studentAttendanceService.QueryStudentSectionAttendancesAsync(null);
            }

            [TestMethod]
            public async Task GetStudentSectionAttendancesAsync_ExceptionIsNotThrownIfCriteriaSectionIdEmpty_ReturnsAll()
            {
                StudentSectionAttendancesQueryCriteria criteria = new StudentSectionAttendancesQueryCriteria() {StudentId= "0000123", SectionIds=null };
                StudentSectionsAttendances studentAttendanceDto = await studentAttendanceService.QueryStudentSectionAttendancesAsync(criteria);
                Assert.IsNotNull(studentAttendanceDto);
                Assert.AreEqual(3, studentAttendanceDto.SectionWiseAttendances.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetStudentSectionAttendancesAsync_ExceptionIfCriteriaStudentIdEmpty()
            {
                StudentSectionAttendancesQueryCriteria criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = string.Empty, SectionIds = new List<string>() { "1" } };
                var studentAttendanceDto = await studentAttendanceService.QueryStudentSectionAttendancesAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetStudentSectionAttendancesAsync_ExceptionIfCriteriaStudentIdNull()
            {
                StudentSectionAttendancesQueryCriteria criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = null, SectionIds = new List<string>() { "1" } };
                var studentAttendanceDto = await studentAttendanceService.QueryStudentSectionAttendancesAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentSectionAttendancesAsync_RethrowsExceptionFromSectionRepository()
            {
                StudentSectionAttendancesQueryCriteria criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = "0000123", SectionIds = new List<string>() { "SEC1" } };
                studentAttendanceRepositoryMock.Setup(repository => repository.GetStudentSectionAttendancesAsync(  studentId, It.IsAny<List<string>>())).Throws(new Exception());
                var studentAttendanceDto = await studentAttendanceService.QueryStudentSectionAttendancesAsync(criteria);
            }



            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentSectionAttendancesAsync_ThrowsExceptionIfCurrentUserIsNotSelf()
            {
                StudentSectionAttendancesQueryCriteria criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = "0000124", SectionIds = new List<string>() { "1" }};
                var studentAttendanceDto = await studentAttendanceService.QueryStudentSectionAttendancesAsync(criteria);
            }

            [TestMethod]
            public async Task GetStudentSectionAttendancesAsync_SectionsReturnedAsNullFromRepository_AttendanceReturnedNull()
            {
                StudentSectionAttendancesQueryCriteria criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = "0000123", SectionIds = new List<string>() { "SEC1" }};
                var studentAttendanceDto = await studentAttendanceService.QueryStudentSectionAttendancesAsync(criteria);
                Assert.IsNull(studentAttendanceDto);

            }

            [TestMethod]
            public async Task GetStudentSectionAttendancesAsync_SectionsReturnedAsEmptyFromRepository_AttendanceReturnedAsEmpty()
            {
                StudentSectionAttendancesQueryCriteria criteria = new StudentSectionAttendancesQueryCriteria() { StudentId = "0000123", SectionIds = new List<string>() { "SEC2" } };
                var studentAttendanceDto = await studentAttendanceService.QueryStudentSectionAttendancesAsync(criteria);
                Assert.AreEqual(0, studentAttendanceDto.SectionWiseAttendances.Count());

            }

            private Domain.Student.Entities.StudentSectionsAttendances BuildStudentAttendancesRepositoryResponseFor1Section()
            {
                List<Domain.Student.Entities.StudentAttendance> Attendances = new List<Domain.Student.Entities.StudentAttendance>();
                var startTime = new DateTimeOffset(2017, 1, 1, 8, 15, 0, new TimeSpan(1, 0, 0));
                var endTime = new DateTimeOffset(2017, 1, 1, 9, 15, 0, new TimeSpan(1, 0, 0));
                var otherTime = new DateTimeOffset(2017, 1, 1, 10, 15, 0, new TimeSpan(1, 0, 0));
                Domain.Student.Entities.StudentSectionsAttendances studentSectionAttendances = new Domain.Student.Entities.StudentSectionsAttendances("0000123");
                    var studentAttendance1 = new Domain.Student.Entities.StudentAttendance("0000123", "1", attendanceDate, "P", null, "Comment for student attendance 1") { StudentCourseSectionId = "StudentCourseSec1", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                    Attendances.Add(studentAttendance1);

                    var studentAttendance2 = new Domain.Student.Entities.StudentAttendance("0000123", "1", attendanceDate, "E", null, "Comment for student attendance 2") { StudentCourseSectionId = "StudentCourseSec2", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                    Attendances.Add(studentAttendance2);
                studentSectionAttendances.AddStudentAttendances(Attendances);
                    return studentSectionAttendances;
            }

            private Domain.Student.Entities.StudentSectionsAttendances BuildStudentAttendancesRepositoryResponseFor2Section()
            {
                List<Domain.Student.Entities.StudentAttendance> Attendances = new List<Domain.Student.Entities.StudentAttendance>();
                var startTime = new DateTimeOffset(2017, 1, 1, 8, 15, 0, new TimeSpan(1, 0, 0));
                var endTime = new DateTimeOffset(2017, 1, 1, 9, 15, 0, new TimeSpan(1, 0, 0));
                var otherTime = new DateTimeOffset(2017, 1, 1, 10, 15, 0, new TimeSpan(1, 0, 0));
                Domain.Student.Entities.StudentSectionsAttendances studentSectionAttendances = new Domain.Student.Entities.StudentSectionsAttendances("0000123");

                var studentAttendance1 = new Domain.Student.Entities.StudentAttendance("0000123", "1", attendanceDate, "P", null, "Comment for student attendance 1") { StudentCourseSectionId = "StudentCourseSec1", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                    Attendances.Add(studentAttendance1);

                    var studentAttendance2 = new Domain.Student.Entities.StudentAttendance("0000123", "1", attendanceDate, "E", null, "Comment for student attendance 2") { StudentCourseSectionId = "StudentCourseSec2", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                    Attendances.Add(studentAttendance2);

                var studentAttendance3 = new Domain.Student.Entities.StudentAttendance("0000123", "2", attendanceDate, "L", null, "Comment for student attendance 3") { StudentCourseSectionId = "StudentCourseSec3", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                Attendances.Add(studentAttendance3);

                var studentAttendance4 = new Domain.Student.Entities.StudentAttendance("0000123", "2", attendanceDate, "P", null);
                Attendances.Add(studentAttendance4);

                var studentAttendance5 = new Domain.Student.Entities.StudentAttendance("0000123", "3", attendanceDate, "P", null, "Comment for student attendance 5") { StudentCourseSectionId = "StudentCourseSec5", StartTime = startTime, EndTime = endTime, InstructionalMethod = "OM" };
                Attendances.Add(studentAttendance5);

                var studentAttendance6 = new Domain.Student.Entities.StudentAttendance("0000123", "3", attendanceDate, "E", null) { StudentCourseSectionId = "StudentCourseSec6" };
                Attendances.Add(studentAttendance6);

                var studentAttendance7 = new Domain.Student.Entities.StudentAttendance("0000123", "3", attendanceDate, "P", null, "Comment for student attendance 7") { StudentCourseSectionId = "StudentCourseSec7", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                Attendances.Add(studentAttendance7);

                var studentAttendance8 = new Domain.Student.Entities.StudentAttendance("0000123", "3", attendanceDate, null, 30, "Comment for student attendance 7") { StudentCourseSectionId = "StudentCourseSec7", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC", CumulativeMinutesAttended = 90, MinutesAttendedToDate = 60 };
                Attendances.Add(studentAttendance8);
                studentSectionAttendances.AddStudentAttendances(Attendances);
                return studentSectionAttendances;
            }

            private Domain.Student.Entities.StudentSectionsAttendances BuildStudentAttendancesRepositoryResponseForNoKeys()
            {
                Domain.Student.Entities.StudentSectionsAttendances studentSectionAttendances = new Domain.Student.Entities.StudentSectionsAttendances("0000123");
                List<Domain.Student.Entities.StudentAttendance> Attendances = new List<Domain.Student.Entities.StudentAttendance>();
                studentSectionAttendances.AddStudentAttendances(Attendances);
                return studentSectionAttendances;
            }
        }
    }
}