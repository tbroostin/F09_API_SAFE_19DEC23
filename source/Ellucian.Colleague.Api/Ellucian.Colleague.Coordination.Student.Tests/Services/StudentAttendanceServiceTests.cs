// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.
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
    public class StudentAttendanceServiceTests
    {
        // Sets up a Current user that is a faculty
        public abstract class CurrentUserSetup
        {
            protected Role facultyRole = new Role(105, "Faculty");

            public class FacultyUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George Smith",
                            PersonId = "0000011",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "GSmith",
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }


        [TestClass]
        public class QueryStudentAttendancesAsync : CurrentUserSetup
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
            private IStudentAttendanceService studentAttendanceService;
            private List<Domain.Student.Entities.StudentAttendance> studentAttendancesData;
            private List<Domain.Student.Entities.Section> sectionData;
            private List<Domain.Student.Entities.Section> unassignedSectionData;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private DateTime attendanceDate;

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

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Mock section response
                sectionData = new List<Domain.Student.Entities.Section>();
                unassignedSectionData = new List<Domain.Student.Entities.Section>();
                var testSection1 = new TestSectionRepository().GetAsync().Result.First();
                testSection1.AddFaculty("0000011");
                sectionData.Add(testSection1);
                var testSection2 = new TestSectionRepository().GetAsync().Result.First();
                unassignedSectionData.Add(testSection2);
                List<string> queryIds1 = new List<string>() { "SEC1" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds1, It.IsAny<bool>())).Returns(Task.FromResult(sectionData.AsEnumerable()));
                List<string> queryIds2 = new List<string>() { "SEC2" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds2, It.IsAny<bool>())).Returns(Task.FromResult(unassignedSectionData.AsEnumerable()));
                List<string> queryIds3 = new List<string>() { "XXX" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds3, It.IsAny<bool>())).Returns(Task.FromResult(new List<Domain.Student.Entities.Section>().AsEnumerable()));
                // Mock student Attendance response
                studentAttendancesData = BuildStudentAttendancesRepositoryResponse("SEC1");
                studentAttendanceRepositoryMock.Setup(repository => repository.GetStudentAttendancesAsync(It.IsAny<List<string>>(), attendanceDate)).Returns(Task.FromResult(studentAttendancesData.AsEnumerable()));

                // Mock Adapters
                var AttendanceDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>()).Returns(AttendanceDtoAdapter);
                var MeetingDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>()).Returns(MeetingDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                studentAttendanceService = new StudentAttendanceService(adapterRegistry, studentAttendanceRepository, studentRepository, sectionRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository);
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
            public async Task GetStudentAttendanceAsync_ThrowsExceptionIfCriteriaNull()
            {
                var studentAttendanceDto = await studentAttendanceService.QueryStudentAttendancesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task GetStudentAttendanceAsync_ExceptionIfCriteriaSectionIdEmpty()
            {
                StudentAttendanceQueryCriteria criteria = new StudentAttendanceQueryCriteria() { AttendanceDate = new DateTime() };
                var studentAttendanceDto = await studentAttendanceService.QueryStudentAttendancesAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentAttendanceAsync_RethrowsExceptionFromSectionRepository()
            {
                StudentAttendanceQueryCriteria criteria = new StudentAttendanceQueryCriteria() { SectionId = "SEC1", AttendanceDate = attendanceDate };
                studentAttendanceRepositoryMock.Setup(repository => repository.GetStudentAttendancesAsync(It.IsAny<List<string>>(), attendanceDate)).Throws(new Exception());
                var studentAttendanceDto = await studentAttendanceService.QueryStudentAttendancesAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentAttendanceAsync_SectionNotfound()
            {
                StudentAttendanceQueryCriteria criteria = new StudentAttendanceQueryCriteria() { SectionId = "XXX", AttendanceDate = attendanceDate };
                var studentAttendanceDto = await studentAttendanceService.QueryStudentAttendancesAsync(criteria);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentAttendanceAsync_ThrowsExceptionIfCurrentUserIsNotSelf()
            {
                StudentAttendanceQueryCriteria criteria = new StudentAttendanceQueryCriteria() { SectionId = "SEC2", AttendanceDate = attendanceDate };
                var studentAttendanceDto = await studentAttendanceService.QueryStudentAttendancesAsync(criteria);
            }

            [TestMethod]
            public async Task GetStudentAttendanceAsync_ReturnsAttendancesIfCurrentUserIsSelf()
            {
                StudentAttendanceQueryCriteria criteria = new StudentAttendanceQueryCriteria() { SectionId = "SEC1", AttendanceDate = attendanceDate };
                var studentAttendanceDtos = await studentAttendanceService.QueryStudentAttendancesAsync(criteria);

                Assert.AreEqual(studentAttendancesData.Count(), studentAttendanceDtos.Count());
                foreach (var expectedEntity in studentAttendancesData)
                {
                    var saDto = studentAttendanceDtos.Where(sa => sa.StudentId == expectedEntity.StudentId).FirstOrDefault();
                    Assert.IsNotNull(saDto);
                    Assert.AreEqual(expectedEntity.AttendanceCategoryCode, saDto.AttendanceCategoryCode);
                    Assert.AreEqual(expectedEntity.MinutesAttended, saDto.MinutesAttended);
                    Assert.AreEqual(expectedEntity.Comment, saDto.Comment);
                    Assert.AreEqual(expectedEntity.StudentCourseSectionId, saDto.StudentCourseSectionId);
                    Assert.AreEqual(expectedEntity.MeetingDate, saDto.MeetingDate);
                    Assert.AreEqual(expectedEntity.SectionId, saDto.SectionId);
                    Assert.AreEqual(expectedEntity.InstructionalMethod, saDto.InstructionalMethod);
                    Assert.AreEqual(expectedEntity.MinutesAttended, saDto.MinutesAttended);
                    Assert.AreEqual(expectedEntity.MinutesAttendedToDate, saDto.MinutesAttendedToDate);
                    Assert.AreEqual(expectedEntity.CumulativeMinutesAttended, saDto.CumulativeMinutesAttended);
                }
            }

            private List<Domain.Student.Entities.StudentAttendance> BuildStudentAttendancesRepositoryResponse(string sectionId)
            {
                List<Domain.Student.Entities.StudentAttendance> Attendances = new List<Domain.Student.Entities.StudentAttendance>();
                var startTime = new DateTimeOffset(2017, 1, 1, 8, 15, 0, new TimeSpan(1, 0, 0));
                var endTime = new DateTimeOffset(2017, 1, 1, 9, 15, 0, new TimeSpan(1, 0, 0));
                var otherTime = new DateTimeOffset(2017, 1, 1, 10, 15, 0, new TimeSpan(1, 0, 0));
                var studentAttendance1 = new Domain.Student.Entities.StudentAttendance("0000123", "SEC1", attendanceDate, "P", null, "Comment for student attendance 1") { StudentCourseSectionId = "StudentCourseSec1", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                Attendances.Add(studentAttendance1);

                var studentAttendance2 = new Domain.Student.Entities.StudentAttendance("0000124", "SEC1", attendanceDate, "E", null, "Comment for student attendance 2") { StudentCourseSectionId = "StudentCourseSec2", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                Attendances.Add(studentAttendance2);

                var studentAttendance3 = new Domain.Student.Entities.StudentAttendance("0000125", "SEC1", attendanceDate, "L", null, "Comment for student attendance 3") { StudentCourseSectionId = "StudentCourseSec3", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                Attendances.Add(studentAttendance3);

                var studentAttendance4 = new Domain.Student.Entities.StudentAttendance("0000126", "SEC1", attendanceDate, "P", null);
                Attendances.Add(studentAttendance4);

                var studentAttendance5 = new Domain.Student.Entities.StudentAttendance("0000127", "SEC1", attendanceDate, "P", null, "Comment for student attendance 5") { StudentCourseSectionId = "StudentCourseSec5", StartTime = startTime, EndTime = endTime, InstructionalMethod = "OM" };
                Attendances.Add(studentAttendance5);

                var studentAttendance6 = new Domain.Student.Entities.StudentAttendance("0000128", "SEC1", attendanceDate, "E", null) { StudentCourseSectionId = "StudentCourseSec6" };
                Attendances.Add(studentAttendance6);

                var studentAttendance7 = new Domain.Student.Entities.StudentAttendance("0000129", "SEC1", attendanceDate, "P", null, "Comment for student attendance 7") { StudentCourseSectionId = "StudentCourseSec7", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC" };
                Attendances.Add(studentAttendance7);

                var studentAttendance8 = new Domain.Student.Entities.StudentAttendance("0000130", "SEC1", attendanceDate, null, 30, "Comment for student attendance 7") { StudentCourseSectionId = "StudentCourseSec7", StartTime = startTime, EndTime = endTime, InstructionalMethod = "LEC", CumulativeMinutesAttended = 90, MinutesAttendedToDate = 60 };
                Attendances.Add(studentAttendance8);
                return Attendances;
            }

        }

        [TestClass]
        public class UpdateSectionAttendanceAsync : CurrentUserSetup
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
            private IStudentAttendanceService studentAttendanceService;
            private Dtos.Student.SectionAttendance sectionAttendanceDto;
            private Domain.Student.Entities.SectionAttendanceResponse sectionAttendanceResponse;
            private Domain.Student.Entities.SectionAttendance sectionAttendanceEntity;
            private Dtos.Student.SectionAttendance secondaryAttendanceToUpdate;
            private Domain.Student.Entities.SectionAttendanceResponse secondaryAttendanceResponse;
            private List<Domain.Student.Entities.Section> sectionData;
            private List<Domain.Student.Entities.Section> unassignedSectionData;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IEnumerable<Domain.Student.Entities.SectionMeetingInstance> sectionMeetingInstanceResponse;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;
            private SectionMeetingInstance meetingInstance;
            private List<string> crossListIds;
            private List<StudentSectionAttendance> studentSectionAttendancesToUpdate;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                studentAttendanceRepositoryMock = new Mock<IStudentAttendanceRepository>();
                studentAttendanceRepository = studentAttendanceRepositoryMock.Object;

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                meetingDate = DateTime.Today;
                startTime = DateTimeOffset.Now.AddHours(-10);
                endTime = DateTimeOffset.Now;
                // Meeting Id matches one of the ones returned in the response
                meetingInstance = new SectionMeetingInstance()
                {
                    MeetingDate = meetingDate,
                    SectionId = "SEC1",
                    Id = "14",
                    StartTime = startTime,
                    EndTime = endTime,
                    InstructionalMethod = "LEC"
                };
                studentSectionAttendancesToUpdate = new List<StudentSectionAttendance>()
                    {
                        new StudentSectionAttendance()
                        {
                             StudentCourseSectionId = "courseSec1",
                              AttendanceCategoryCode = "L",
                               Comment = "Comment1"
                        },
                        new StudentSectionAttendance()
                        {
                             StudentCourseSectionId = "courseSec2",
                              AttendanceCategoryCode = "P"
                        },
                        new StudentSectionAttendance()
                        {
                             StudentCourseSectionId = "courseSec3",
                              MinutesAttended = 60
                        }
                    };
                sectionAttendanceDto = new Dtos.Student.SectionAttendance()
                {
                    SectionId = "SEC1",
                    MeetingInstance = meetingInstance,
                    StudentAttendances = studentSectionAttendancesToUpdate

                };

                secondaryAttendanceToUpdate = new Dtos.Student.SectionAttendance()
                {
                    SectionId = "SECXL1",
                    MeetingInstance = meetingInstance,
                    StudentAttendances = studentSectionAttendancesToUpdate

                };

                // Mock section repository responses

                // Section that is a secondary crosslisted section but is also for the correct faculty
                var secondarySectionData = new List<Domain.Student.Entities.Section>();
                var testSection3 = new TestSectionRepository().GetAsync().Result.ElementAt(1);
                var testSection4 = new TestSectionRepository().GetAsync().Result.ElementAt(2);
                crossListIds = new List<string>() { "something" };
                testSection3.PrimarySectionId = "SEC1";
                testSection3.AddCrossListedSection(testSection4);
                testSection3.AddFaculty("0000011");
                secondarySectionData.Add(testSection3);
                // Setup a good section's response
                sectionData = new List<Domain.Student.Entities.Section>();
                var testSection1 = new TestSectionRepository().GetAsync().Result.First();
                testSection1.AddFaculty("0000011");
                sectionData.Add(testSection1);
                // Setup an unassigned section's response - section does not belong to the faculty - permission problem
                unassignedSectionData = new List<Domain.Student.Entities.Section>();
                var testSection2 = new TestSectionRepository().GetAsync().Result.First();
                unassignedSectionData.Add(testSection2);

                List<string> queryIds1 = new List<string>() { "SEC1" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds1, It.IsAny<bool>())).Returns(Task.FromResult(sectionData.AsEnumerable()));
                List<string> queryIds2 = new List<string>() { "SEC2" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds2, It.IsAny<bool>())).Returns(Task.FromResult(unassignedSectionData.AsEnumerable()));
                List<string> queryIds3 = new List<string>() { "XXX" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds3, It.IsAny<bool>())).Returns(Task.FromResult(new List<Domain.Student.Entities.Section>().AsEnumerable()));
                List<string> queryIds4 = new List<string>() { "SECXL1" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds4, It.IsAny<bool>())).Returns(Task.FromResult(secondarySectionData.AsEnumerable()));
                ;

                sectionMeetingInstanceResponse = BuildSectionMeetingInstanceResponse();
                sectionRepositoryMock.Setup(repo => repo.GetSectionMeetingInstancesAsync("SEC1")).ReturnsAsync(sectionMeetingInstanceResponse);
                sectionRepositoryMock.Setup(repo => repo.GetSectionMeetingInstancesAsync("SECXL1")).ReturnsAsync(new List<Domain.Student.Entities.SectionMeetingInstance>());

                // Mock student Attendance response
                sectionAttendanceResponse = BuildStudentAttendancesRepositoryResponse(sectionAttendanceDto);

                // Mock secondary student Attendance response
                secondaryAttendanceResponse = BuildStudentAttendancesRepositoryResponse(secondaryAttendanceToUpdate);

                // Mock the many Adapters
                var SectionAttendanceDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionAttendanceResponse, Dtos.Student.SectionAttendanceResponse>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionAttendanceResponse, Dtos.Student.SectionAttendanceResponse>()).Returns(SectionAttendanceDtoAdapter);
                var MeetingDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>()).Returns(MeetingDtoAdapter);
                var AttendanceDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>()).Returns(AttendanceDtoAdapter);
                var AttendanceEntityAdapter = new AutoMapperAdapter<Dtos.Student.SectionAttendance, Domain.Student.Entities.SectionAttendance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.SectionAttendance, Domain.Student.Entities.SectionAttendance>()).Returns(AttendanceEntityAdapter);
                var MeetingEntityAdapter = new AutoMapperAdapter<Dtos.Student.SectionMeetingInstance, Domain.Student.Entities.SectionMeetingInstance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.SectionMeetingInstance, Domain.Student.Entities.SectionMeetingInstance>()).Returns(MeetingEntityAdapter);
                var StudentSectionAttendanceEntityAdapter = new AutoMapperAdapter<Dtos.Student.StudentSectionAttendance, Domain.Student.Entities.StudentSectionAttendance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.StudentSectionAttendance, Domain.Student.Entities.StudentSectionAttendance>()).Returns(StudentSectionAttendanceEntityAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();


                studentAttendanceService = new StudentAttendanceService(adapterRegistry, studentAttendanceRepository, studentRepository, sectionRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository);
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
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfStudentAttendanceNull()
            {
                var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfSectiontIdNull()
            {
                var tempSa = new Dtos.Student.SectionAttendance()
                {
                    MeetingInstance = meetingInstance,
                    StudentAttendances = studentSectionAttendancesToUpdate

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfSectiontIdEmpty()
            {
                var tempSa = new Dtos.Student.SectionAttendance()
                {
                    SectionId = string.Empty,
                    MeetingInstance = meetingInstance,
                    StudentAttendances = studentSectionAttendancesToUpdate

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfMeetingNull()
            {
                var tempSa = new Dtos.Student.SectionAttendance()
                {
                    SectionId = "sectionId",
                    MeetingInstance = null,
                    StudentAttendances = studentSectionAttendancesToUpdate

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfMeetingHasNoMeetingDate()
            {
                var tempSa = new Dtos.Student.SectionAttendance()
                {
                    SectionId = "sectionId",
                    MeetingInstance = new SectionMeetingInstance() { SectionId = "SectionId", StartTime = startTime, EndTime = endTime },
                    StudentAttendances = studentSectionAttendancesToUpdate

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfAttendancesNull()
            {
                {
                    var tempSa = new Dtos.Student.SectionAttendance()
                    {
                        SectionId = "sectionId",
                        MeetingInstance = new SectionMeetingInstance() { SectionId = "SectionId", StartTime = startTime, EndTime = endTime },

                    };
                    var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfAttendancesEmpty()
            {
                {
                    var tempSa = new Dtos.Student.SectionAttendance()
                    {
                        SectionId = "sectionId",
                        MeetingInstance = new SectionMeetingInstance() { SectionId = "SectionId", StartTime = startTime, EndTime = endTime },
                        StudentAttendances = new List<StudentSectionAttendance>()

                    };
                    var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
                }
            }

            /// Passed initial validations
            /// 
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfCurrentUserIsNotAssignedInstructor()
            {
                {
                    var tempSa = new Dtos.Student.SectionAttendance()
                    {
                        SectionId = "SEC2",
                        MeetingInstance = meetingInstance,
                        StudentAttendances = studentSectionAttendancesToUpdate

                    };
                    var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_ThrowsExceptionIfAdapterCannotConvert()
            {

                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.SectionAttendance, Domain.Student.Entities.SectionAttendance>()).Throws(new Exception());
                var result = await studentAttendanceService.UpdateSectionAttendanceAsync(sectionAttendanceDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateStudentAttendanceAsync_SectionNotfound()
            {
                {
                    var tempSa = new Dtos.Student.SectionAttendance()
                    {
                        SectionId = "XXX",
                        MeetingInstance = meetingInstance,
                        StudentAttendances = studentSectionAttendancesToUpdate

                    };
                    var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
                }

            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateSectionAttendanceAsync_NotAValidMeetingInstance()
            {
                {
                    var tempSa = new Dtos.Student.SectionAttendance()
                    {
                        SectionId = "SEC1",
                        MeetingInstance = new SectionMeetingInstance() { SectionId = "SEC1", Id = "test", MeetingDate = meetingDate, StartTime = startTime, EndTime = endTime, InstructionalMethod = "Y" },
                        StudentAttendances = studentSectionAttendancesToUpdate

                    };
                    var studentAttendanceDto = await studentAttendanceService.UpdateSectionAttendanceAsync(tempSa);
                }

            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdateSectionAttendanceAsync_RethrowsExceptionFromStudentAttendanceRepository()
            {
                studentAttendanceRepositoryMock.Setup(repository => repository.UpdateSectionAttendanceAsync(It.IsAny<Domain.Student.Entities.SectionAttendance>(), It.IsAny<List<string>>())).Throws(new Exception());
                var newResult = await studentAttendanceService.UpdateSectionAttendanceAsync(sectionAttendanceDto);
            }

            [TestMethod]
            public async Task UpdateStudentAttendanceAsync_SuccessIfCurrentUserIsInstructor()
            {
                studentAttendanceRepositoryMock.Setup(repository => repository.UpdateSectionAttendanceAsync(It.IsAny<Domain.Student.Entities.SectionAttendance>(), It.IsAny<List<string>>())).ReturnsAsync(sectionAttendanceResponse);
                var newResult = await studentAttendanceService.UpdateSectionAttendanceAsync(sectionAttendanceDto);
                Assert.IsInstanceOfType(newResult, typeof(SectionAttendanceResponse));
                Assert.IsNotNull(newResult);
                Assert.IsNotNull(newResult.MeetingInstance);
                Assert.AreEqual(sectionAttendanceDto.SectionId, newResult.SectionId);
                Assert.AreEqual(sectionAttendanceDto.MeetingInstance.Id, newResult.MeetingInstance.Id);
                Assert.AreEqual(sectionAttendanceDto.StudentAttendances.Count(), newResult.UpdatedStudentAttendances.Count());
                Assert.AreEqual(0, newResult.StudentAttendanceErrors.Count());
            }

            [TestMethod]
            public async Task UpdateStudentAttendanceAsync_SuccessWhenSecondarySection()
            {

                studentAttendanceRepositoryMock.Setup(repository => repository.UpdateSectionAttendanceAsync(It.IsAny<Domain.Student.Entities.SectionAttendance>(), It.IsAny<List<string>>())).ReturnsAsync(secondaryAttendanceResponse);
                var newResult = await studentAttendanceService.UpdateSectionAttendanceAsync(secondaryAttendanceToUpdate);
                Assert.IsInstanceOfType(newResult, typeof(SectionAttendanceResponse));
                Assert.IsNotNull(newResult);
                Assert.IsNotNull(newResult.MeetingInstance);
                Assert.AreEqual(secondaryAttendanceToUpdate.SectionId, newResult.SectionId);
                Assert.AreEqual(secondaryAttendanceToUpdate.MeetingInstance.Id, newResult.MeetingInstance.Id);
                Assert.AreEqual(secondaryAttendanceToUpdate.StudentAttendances.Count(), newResult.UpdatedStudentAttendances.Count());
                Assert.AreEqual(0, newResult.StudentAttendanceErrors.Count());

            }

            private Domain.Student.Entities.SectionAttendanceResponse BuildStudentAttendancesRepositoryResponse(Dtos.Student.SectionAttendance sa)
            {
                Domain.Student.Entities.SectionAttendanceResponse result = null;
                Domain.Student.Entities.SectionMeetingInstance sme = null;
                if (sa != null)
                {
                    sme = new Domain.Student.Entities.SectionMeetingInstance(sa.MeetingInstance.Id, sa.SectionId, sa.MeetingInstance.MeetingDate, sa.MeetingInstance.StartTime, sa.MeetingInstance.EndTime) { InstructionalMethod = sa.MeetingInstance.InstructionalMethod };

                    result = new Domain.Student.Entities.SectionAttendanceResponse(sa.SectionId, sme);
                    foreach (var saItem in sa.StudentAttendances)
                    {
                        result.AddUpdatedStudentAttendance(new Domain.Student.Entities.StudentAttendance("studentId", sa.SectionId, sa.MeetingInstance.MeetingDate, saItem.AttendanceCategoryCode, null, saItem.Comment) { StudentCourseSectionId = saItem.StudentCourseSectionId });
                    }
                }
                return result;
            }

            private IEnumerable<Domain.Student.Entities.SectionMeetingInstance> BuildSectionMeetingInstanceResponse()
            {
                var results = new List<Domain.Student.Entities.SectionMeetingInstance>()
                {
                    new Domain.Student.Entities.SectionMeetingInstance("12", "SEC1", DateTime.Today.AddDays(-30), startTime, endTime ) {InstructionalMethod = "LEC" },
                    new Domain.Student.Entities.SectionMeetingInstance("13", "SEC1", DateTime.Today.AddDays(-15), startTime, endTime ) {InstructionalMethod = "LEC" },
                    new Domain.Student.Entities.SectionMeetingInstance("14", "SEC1", meetingDate, startTime, endTime ) {InstructionalMethod = "LEC" },
                    new Domain.Student.Entities.SectionMeetingInstance("15", "SEC1", DateTime.Today.AddDays(15), startTime, endTime ) { InstructionalMethod = "LEC" },
                };

                return results;

            }

            private Domain.Student.Entities.SectionAttendance BuildSectionAttendanceEntity(Dtos.Student.SectionAttendance sa)
            {
                Domain.Student.Entities.SectionAttendance result = null;
                Domain.Student.Entities.SectionMeetingInstance sme = null;
                if (sa != null)
                {
                    sme = new Domain.Student.Entities.SectionMeetingInstance(sa.MeetingInstance.Id, sa.SectionId, sa.MeetingInstance.MeetingDate, sa.MeetingInstance.StartTime, sa.MeetingInstance.EndTime) { InstructionalMethod = sa.MeetingInstance.InstructionalMethod };

                    result = new Domain.Student.Entities.SectionAttendance(sa.SectionId, sme);
                    foreach (var saItem in sa.StudentAttendances)
                    {
                        result.AddStudentSectionAttendance(new Domain.Student.Entities.StudentSectionAttendance(saItem.StudentCourseSectionId, saItem.AttendanceCategoryCode, null, saItem.Comment));
                    }
                }
                return result;
            }
        }

        [TestClass]
        public class UpdateStudentAttendanceAsync : CurrentUserSetup
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
            private IStudentAttendanceService studentAttendanceService;
            private Dtos.Student.StudentAttendance studentAttendanceToUpdate;
            private Domain.Student.Entities.StudentAttendance studentAttendanceEntity;
            private Dtos.Student.StudentAttendance secondaryAttendanceToUpdate;
            private Domain.Student.Entities.StudentAttendance secondaryAttendanceEntity;
            private List<Domain.Student.Entities.Section> sectionData;
            private List<Domain.Student.Entities.Section> unassignedSectionData;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IEnumerable<Domain.Student.Entities.SectionMeetingInstance> sectionMeetingInstanceResponse;
            private DateTime meetingDate;
            private DateTimeOffset? startTime;
            private DateTimeOffset? endTime;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                studentAttendanceRepositoryMock = new Mock<IStudentAttendanceRepository>();
                studentAttendanceRepository = studentAttendanceRepositoryMock.Object;

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                meetingDate = DateTime.Today;
                startTime = DateTimeOffset.Now.AddHours(-10);
                endTime = DateTimeOffset.Now;

                studentAttendanceToUpdate = new Dtos.Student.StudentAttendance()
                {
                    SectionId = "SEC1",
                    StudentId = "StudentId",
                    MeetingDate = meetingDate,
                    AttendanceCategoryCode = "P",
                    InstructionalMethod = "LEC",
                    StartTime = startTime,
                    EndTime = endTime,
                    Comment = "This is the comment.",
                    StudentCourseSectionId = "StudentCourseSecId"

                };
                secondaryAttendanceToUpdate = new Dtos.Student.StudentAttendance()
                {
                    SectionId = "SECXL1",
                    StudentId = "studentId",
                    MeetingDate = meetingDate,
                    AttendanceCategoryCode = "P",
                    InstructionalMethod = "LEC",
                    StartTime = DateTimeOffset.Now.AddHours(-10),
                    EndTime = DateTimeOffset.Now,
                    Comment = "This is the comment.",
                    StudentCourseSectionId = "StudentCourseSecId"

                };

                // Mock section repository responses

                // Section that is a secondary crosslisted section but is for the correct faculty
                var secondarySectionData = new List<Domain.Student.Entities.Section>();
                var testSection3 = new TestSectionRepository().GetAsync().Result.ElementAt(1);
                testSection3.PrimarySectionId = "SEC1";
                testSection3.AddFaculty("0000011");
                secondarySectionData.Add(testSection3);
                // Setup a good section's response
                sectionData = new List<Domain.Student.Entities.Section>();
                var testSection1 = new TestSectionRepository().GetAsync().Result.First();
                testSection1.AddFaculty("0000011");
                sectionData.Add(testSection1);
                // Setup an unassigned section's response - section does not belong to the faculty - permission problem
                unassignedSectionData = new List<Domain.Student.Entities.Section>();
                var testSection2 = new TestSectionRepository().GetAsync().Result.First();
                unassignedSectionData.Add(testSection2);

                List<string> queryIds1 = new List<string>() { "SEC1" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds1, It.IsAny<bool>())).Returns(Task.FromResult(sectionData.AsEnumerable()));
                List<string> queryIds2 = new List<string>() { "SEC2" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds2, It.IsAny<bool>())).Returns(Task.FromResult(unassignedSectionData.AsEnumerable()));
                List<string> queryIds3 = new List<string>() { "XXX" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds3, It.IsAny<bool>())).Returns(Task.FromResult(new List<Domain.Student.Entities.Section>().AsEnumerable()));
                List<string> queryIds4 = new List<string>() { "SECXL1" };
                sectionRepositoryMock.Setup(repository => repository.GetCachedSectionsAsync(queryIds4, It.IsAny<bool>())).Returns(Task.FromResult(secondarySectionData.AsEnumerable()));
                ;

                sectionMeetingInstanceResponse = BuildSectionMeetingInstanceResponse();
                sectionRepositoryMock.Setup(repo => repo.GetSectionMeetingInstancesAsync("SEC1")).ReturnsAsync(sectionMeetingInstanceResponse);
                sectionRepositoryMock.Setup(repo => repo.GetSectionMeetingInstancesAsync("SECXL1")).ReturnsAsync(new List<Domain.Student.Entities.SectionMeetingInstance>());

                // Mock student Attendance response
                studentAttendanceEntity = BuildStudentAttendancesRepositoryResponse(studentAttendanceToUpdate);

                studentAttendanceRepositoryMock.Setup(repository => repository.UpdateStudentAttendanceAsync(studentAttendanceEntity, sectionMeetingInstanceResponse)).ReturnsAsync(studentAttendanceEntity);

                // Mock secondary student Attendance response
                secondaryAttendanceEntity = BuildStudentAttendancesRepositoryResponse(secondaryAttendanceToUpdate);

                studentAttendanceRepositoryMock.Setup(repository => repository.UpdateStudentAttendanceAsync(secondaryAttendanceEntity, sectionMeetingInstanceResponse)).ReturnsAsync(secondaryAttendanceEntity);

                // Mock Adapters
                var AttendanceDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.StudentAttendance, Dtos.Student.StudentAttendance>()).Returns(AttendanceDtoAdapter);
                var MeetingDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionMeetingInstance, Dtos.Student.SectionMeetingInstance>()).Returns(MeetingDtoAdapter);

                var AttendanceEntityAdapter = new AutoMapperAdapter<Dtos.Student.StudentAttendance, Domain.Student.Entities.StudentAttendance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.StudentAttendance, Domain.Student.Entities.StudentAttendance>()).Returns(AttendanceEntityAdapter);
                var MeetingEntityAdapter = new AutoMapperAdapter<Dtos.Student.SectionMeetingInstance, Domain.Student.Entities.SectionMeetingInstance>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.SectionMeetingInstance, Domain.Student.Entities.SectionMeetingInstance>()).Returns(MeetingEntityAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();


                studentAttendanceService = new StudentAttendanceService(adapterRegistry, studentAttendanceRepository, studentRepository, sectionRepository, currentUserFactory, roleRepository, logger, baseConfigurationRepository);
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
            public async Task UpdateStudentAttendanceAsync_ThrowsExceptionIfStudentAttendanceNull()
            {
                var studentAttendanceDto = await studentAttendanceService.UpdateStudentAttendanceAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentAttendanceAsync_ThrowsExceptionIfStudentIdInAttendanceNull()
            {
                var tempSa = new Dtos.Student.StudentAttendance()
                {
                    SectionId = "SEC1",
                    StudentId = null,
                    MeetingDate = DateTime.Now,
                    AttendanceCategoryCode = "P",
                    InstructionalMethod = "LEC",
                    StartTime = DateTimeOffset.Now.AddHours(-10),
                    EndTime = DateTimeOffset.Now,
                    Comment = "This is the comment.",
                    StudentCourseSectionId = "StudentCourseSecId"

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateStudentAttendanceAsync(tempSa);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentAttendanceAsync_ThrowsExceptionIfSectionIdInAttendanceNull()
            {
                var tempSa = new Dtos.Student.StudentAttendance()
                {
                    SectionId = null,
                    StudentId = "studentId",
                    MeetingDate = DateTime.Now,
                    AttendanceCategoryCode = "P",
                    InstructionalMethod = "LEC",
                    StartTime = DateTimeOffset.Now.AddHours(-10),
                    EndTime = DateTimeOffset.Now,
                    Comment = "This is the comment.",
                    StudentCourseSectionId = "StudentCourseSecId"

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateStudentAttendanceAsync(tempSa);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentAttendanceAsync_ThrowsExceptionIfDefaultDateInAttendanceNull()
            {
                var tempSa = new Dtos.Student.StudentAttendance()
                {
                    SectionId = "sectionId",
                    StudentId = "studentId",
                    MeetingDate = default(DateTime),
                    AttendanceCategoryCode = "P",
                    InstructionalMethod = "LEC",
                    StartTime = DateTimeOffset.Now.AddHours(-10),
                    EndTime = DateTimeOffset.Now,
                    Comment = "This is the comment.",
                    StudentCourseSectionId = "StudentCourseSecId"

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateStudentAttendanceAsync(tempSa);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdateStudentAttendanceAsync_RethrowsExceptionFromStudentAttendanceRepository()
            {
                studentAttendanceRepositoryMock.Setup(repository => repository.UpdateStudentAttendanceAsync(studentAttendanceEntity, sectionMeetingInstanceResponse)).Throws(new Exception());
                var studentAttendanceDto = await studentAttendanceService.UpdateStudentAttendanceAsync(studentAttendanceToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentAttendanceAsync_ThrowsExceptionIfAdapterCannotConvert()
            {
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.StudentAttendance, Domain.Student.Entities.StudentAttendance>()).Throws(new Exception());
                var studentAttendanceDto = await studentAttendanceService.UpdateStudentAttendanceAsync(studentAttendanceToUpdate);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task UpdateStudentAttendanceAsync_SectionNotfound()
            {
                var tempSa = new Dtos.Student.StudentAttendance()
                {
                    SectionId = "XXX",
                    StudentId = "studentId",
                    MeetingDate = meetingDate,
                    AttendanceCategoryCode = "P",
                    InstructionalMethod = "LEC",
                    StartTime = DateTimeOffset.Now.AddHours(-10),
                    EndTime = DateTimeOffset.Now,
                    Comment = "This is the comment.",
                    StudentCourseSectionId = "StudentCourseSecId"

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateStudentAttendanceAsync(tempSa);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateStudentAttendanceAsync_ThrowsExceptionIfCurrentUserIsNotAssignedInstructor()
            {
                var tempSa = new Dtos.Student.StudentAttendance()
                {
                    SectionId = "SEC2",
                    StudentId = "studentId",
                    MeetingDate = meetingDate,
                    AttendanceCategoryCode = "P",
                    InstructionalMethod = "LEC",
                    StartTime = DateTimeOffset.Now.AddHours(-10),
                    EndTime = DateTimeOffset.Now,
                    Comment = "This is the comment.",
                    StudentCourseSectionId = "StudentCourseSecId"

                };
                var studentAttendanceDto = await studentAttendanceService.UpdateStudentAttendanceAsync(tempSa);
            }

            [TestMethod]
            public async Task UpdateStudentAttendanceAsync_SuccessIfCurrentUserIsInstructor()
            {
                var saDto = await studentAttendanceService.UpdateStudentAttendanceAsync(studentAttendanceToUpdate);

                Assert.AreEqual(studentAttendanceToUpdate.AttendanceCategoryCode, saDto.AttendanceCategoryCode);
                Assert.AreEqual(studentAttendanceToUpdate.Comment, saDto.Comment);
                Assert.AreEqual(studentAttendanceToUpdate.StudentCourseSectionId, saDto.StudentCourseSectionId);
                Assert.AreEqual(studentAttendanceToUpdate.MeetingDate, saDto.MeetingDate);
                Assert.AreEqual(studentAttendanceToUpdate.SectionId, saDto.SectionId);
                Assert.AreEqual(studentAttendanceToUpdate.InstructionalMethod, saDto.InstructionalMethod);

            }

            [TestMethod]
            public async Task UpdateStudentAttendanceAsync_SuccessWhenSecondarySection()
            {

                var saDto = await studentAttendanceService.UpdateStudentAttendanceAsync(secondaryAttendanceToUpdate);

                Assert.AreEqual(secondaryAttendanceToUpdate.AttendanceCategoryCode, saDto.AttendanceCategoryCode);
                Assert.AreEqual(secondaryAttendanceToUpdate.Comment, saDto.Comment);
                Assert.AreEqual(secondaryAttendanceToUpdate.StudentCourseSectionId, saDto.StudentCourseSectionId);
                Assert.AreEqual(secondaryAttendanceToUpdate.MeetingDate, saDto.MeetingDate);
                Assert.AreEqual(secondaryAttendanceToUpdate.SectionId, saDto.SectionId);
                Assert.AreEqual(secondaryAttendanceToUpdate.InstructionalMethod, saDto.InstructionalMethod);

            }

            private Domain.Student.Entities.StudentAttendance BuildStudentAttendancesRepositoryResponse(Dtos.Student.StudentAttendance sa)
            {
                Domain.Student.Entities.StudentAttendance result = new Domain.Student.Entities.StudentAttendance(sa.StudentId, sa.SectionId, sa.MeetingDate, sa.AttendanceCategoryCode, null, sa.Comment);
                result.InstructionalMethod = sa.InstructionalMethod;
                result.StartTime = sa.StartTime;
                result.EndTime = sa.EndTime;
                result.StudentCourseSectionId = sa.StudentCourseSectionId;
                return result;
            }

            private IEnumerable<Domain.Student.Entities.SectionMeetingInstance> BuildSectionMeetingInstanceResponse()
            {
                var results = new List<Domain.Student.Entities.SectionMeetingInstance>()
                {
                    new Domain.Student.Entities.SectionMeetingInstance("12", "SEC1", DateTime.Today.AddDays(-30), startTime, endTime ) {InstructionalMethod = "LEC" },
                    new Domain.Student.Entities.SectionMeetingInstance("13", "SEC1", DateTime.Today.AddDays(-15), startTime, endTime ) {InstructionalMethod = "LEC" },
                    new Domain.Student.Entities.SectionMeetingInstance("14", "SEC1", meetingDate, startTime, endTime ) {InstructionalMethod = "LEC" },
                    new Domain.Student.Entities.SectionMeetingInstance("15", "SEC1", DateTime.Today.AddDays(15), startTime, endTime ) { InstructionalMethod = "LEC" },
                };

                return results;

            }
        }
    }
}