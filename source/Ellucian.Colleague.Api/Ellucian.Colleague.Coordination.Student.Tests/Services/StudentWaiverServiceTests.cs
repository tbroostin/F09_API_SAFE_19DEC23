// Copyright 2015-2020 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentWaiverServiceTests
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
        public class GetSectionStudentWaivers : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentWaiverRepository> waiverRepoMock;
            private IStudentWaiverRepository waiverRepository;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private IStudentReferenceDataRepository referenceDataRepository;
            private Mock<IReferenceDataRepository> baseReferenceDataRepositoryMock;
            private IReferenceDataRepository baseReferenceDataRepository;
            private Mock<ICourseRepository> courseRepositoryMock;
            private ICourseRepository courseRepository;
            private IStudentWaiverService waiverService;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Section section;
            private List<StudentWaiver> waiversResponse;
            private string studentId;



            [TestInitialize]
            public async void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                waiverRepoMock = new Mock<IStudentWaiverRepository>();
                waiverRepository = waiverRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;
                baseReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                baseReferenceDataRepository = baseReferenceDataRepositoryMock.Object;
                courseRepositoryMock = new Mock<ICourseRepository>();
                courseRepository = courseRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                studentId = "student1";

                // Mock section response
                section = (await new TestSectionRepository().GetAsync()).First();
                section.AddFaculty("1111100");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1", false)).Returns(Task.FromResult(section));

                // Mock Waivers response
                waiversResponse = BuildWaiverRepoResponse();
                waiverRepoMock.Setup(repo => repo.GetSectionWaiversAsync(It.IsAny<string>())).Returns(Task.FromResult(waiversResponse));

                // Mock Adapters
                var waiverDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiver, Ellucian.Colleague.Dtos.Student.StudentWaiver>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiver, Ellucian.Colleague.Dtos.Student.StudentWaiver>()).Returns(waiverDtoAdapter);
                var requisiteWaiverDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>()).Returns(requisiteWaiverDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                waiverService = new StudentWaiverService(adapterRegistry, waiverRepository, sectionRepository, studentProgramRepository, referenceDataRepository, baseReferenceDataRepository, courseRepository, currentUserFactory, roleRepo, logger, studentRepository, configurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                waiverService = null;
                waiverRepository = null;
                sectionRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionStudentWaivers_ThrowsExceptionIfSectionStringNull()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>(), false)).Throws(new ArgumentNullException());
                var waiverDto = await waiverService.GetSectionStudentWaiversAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionStudentWaivers_ThrowsExceptionIfSectionStringEmpty()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>(), false)).Throws(new ArgumentNullException());
                var waiverDto = await waiverService.GetSectionStudentWaiversAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task GetSectionStudentWaivers_RethrowsExceptionFromSectionRepository()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>(), false)).Throws(new ApplicationException());
                var waiverDto = await waiverService.GetSectionStudentWaiversAsync("SEC1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSectionStudentWaivers_ThrowsKeyNotFoundExceptionIfSectionNotFound()
            {
                Section nullSection = null;
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>(), false)).Returns(Task.FromResult(nullSection));
                var waiverDto = await waiverService.GetSectionStudentWaiversAsync("SEC1");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetSectionStudentWaivers_ThrowsExceptionIfCurrentUserIsNotSectionFaculty()
            {
                var waiverDto = await waiverService.GetSectionStudentWaiversAsync("SEC1");
            }


            [TestMethod]
            public async Task GetSectionStudentWaivers_ReturnsWaiversIfCurrentUserIsSectionFaculty()
            {
                // Add this faculty to the mocked section response
                section.AddFaculty("0000011");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1", false)).Returns(Task.FromResult(section));

                // Act
                var waiverDto = await waiverService.GetSectionStudentWaiversAsync("SEC1");

                // Spot-check resulting dto
                Assert.AreEqual(2, waiverDto.Count());
                Assert.AreEqual(waiversResponse.ElementAt(0).StudentId, waiverDto.ElementAt(0).StudentId);
                Assert.AreEqual(3, waiverDto.ElementAt(0).RequisiteWaivers.Count());
                Assert.AreEqual(2, waiverDto.ElementAt(1).RequisiteWaivers.Count());
                Assert.AreEqual(Ellucian.Colleague.Dtos.Student.WaiverStatus.Denied, waiverDto.ElementAt(1).RequisiteWaivers.ElementAt(1).Status);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task GetSectionStudentWaivers_ThrowsExceptionIfAdapterErrorOccurs()
            {
                // Add this faculty to the mocked section response
                section.AddFaculty("0000011");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1", false)).Returns(Task.FromResult(section));
                // Null adapter registry to force adapter error
                ITypeAdapter<Domain.Student.Entities.StudentWaiver, Dtos.Student.StudentWaiver> nullAdapter = null;
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.StudentWaiver, Dtos.Student.StudentWaiver>()).Returns(nullAdapter);
                waiverService = new StudentWaiverService(adapterRegistry, waiverRepository, sectionRepository, studentProgramRepository, referenceDataRepository, baseReferenceDataRepository, courseRepository, currentUserFactory, roleRepo, logger, studentRepository, configurationRepository);
                // Act
                var waiverDto = await waiverService.GetSectionStudentWaiversAsync("SEC1");
            }

            [TestMethod]
            public async Task GetSectionStudentWaivers_ReturnsEmptyListIfRepositoryReturnsNullList()
            {
                // Add this faculty to the mocked section response
                section.AddFaculty("0000011");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1", false)).Returns(Task.FromResult(section));
                // Mock empty response from waiver repository
                List<StudentWaiver> nullResponse = null;
                waiverRepoMock.Setup(repo => repo.GetSectionWaiversAsync(It.IsAny<string>())).Returns(Task.FromResult(nullResponse));
                // Act
                var waiverDto = await waiverService.GetSectionStudentWaiversAsync("SEC1");
                // Assert
                Assert.IsTrue(waiverDto is IEnumerable<Dtos.Student.StudentWaiver>);
                Assert.AreEqual(0, waiverDto.Count());
            }


            private List<StudentWaiver> BuildWaiverRepoResponse()
            {
                List<StudentWaiver> waivers = new List<StudentWaiver>();

                var waiver1 = new StudentWaiver("1", studentId, "", "SEC1", "OTHER", "2014/FA");
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW1", WaiverStatus.Denied));
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW2", WaiverStatus.NotSelected));
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW3", WaiverStatus.Waived));

                waivers.Add(waiver1);

                var waiver2 = new StudentWaiver("2", "Student2", "", "SEC1", "LIFE", "2014/FA");
                waiver2.AddRequisiteWaiver(new RequisiteWaiver("RW2", WaiverStatus.Denied));
                waiver2.AddRequisiteWaiver(new RequisiteWaiver("RW3", WaiverStatus.Denied));

                waivers.Add(waiver2);

                return waivers;
            }
        }

        [TestClass]
        public class GetStudentWaiver : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentWaiverRepository> waiverRepoMock;
            private IStudentWaiverRepository waiverRepository;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private IStudentReferenceDataRepository referenceDataRepository;
            private Mock<IReferenceDataRepository> baseReferenceDataRepositoryMock;
            private IReferenceDataRepository baseReferenceDataRepository;
            private Mock<ICourseRepository> courseRepositoryMock;
            private ICourseRepository courseRepository;
            private IStudentWaiverService waiverService;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Section section;
            private StudentWaiver waiverResponse;
            private string studentId;
            private string waiverId;

            [TestInitialize]
            public async void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                waiverRepoMock = new Mock<IStudentWaiverRepository>();
                waiverRepository = waiverRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;
                baseReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                baseReferenceDataRepository = baseReferenceDataRepositoryMock.Object;
                courseRepositoryMock = new Mock<ICourseRepository>();
                courseRepository = courseRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                waiverId = "3";
                studentId = "student1";

                // Mock section response
                section = (await new TestSectionRepository().GetAsync()).First();
                section.AddFaculty("1111100");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1", false)).Returns(Task.FromResult(section));

                // Mock Waivers response
                waiverResponse = BuildWaiverRepoResponse();
                waiverRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Returns(Task.FromResult(waiverResponse));

                // Mock Adapters
                var waiverDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiver, Ellucian.Colleague.Dtos.Student.StudentWaiver>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiver, Ellucian.Colleague.Dtos.Student.StudentWaiver>()).Returns(waiverDtoAdapter);
                var requisiteWaiverDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>()).Returns(requisiteWaiverDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                waiverService = new StudentWaiverService(adapterRegistry, waiverRepository, sectionRepository, studentProgramRepository, referenceDataRepository, baseReferenceDataRepository, courseRepository, currentUserFactory, roleRepo, logger, studentRepository, configurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                waiverService = null;
                waiverRepository = null;
                sectionRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentWaiver_ThrowsExceptionIfWaiverIdNull()
            {
                var waiverDto = await waiverService.GetStudentWaiverAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentWaiver_ThrowsExceptionIfWaiverIdEmpty()
            {
                var waiverDto = await waiverService.GetStudentWaiverAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentWaiver_ThrowsKeyNotFoundExceptionIfThrownByRepositoryGet()
            {
                waiverRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                var waiverDto = await waiverService.GetStudentWaiverAsync(waiverId);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task GetStudentWaiver_ThrowsExceptionIfOtherExceptionThrownByRepositoryGet()
            {
                waiverRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Throws(new Exception());
                var waiverDto = await waiverService.GetStudentWaiverAsync(waiverId);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task GetStudentWaiver_RethrowsExceptionFromSectionRepository()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>(), false)).Throws(new ApplicationException());
                var waiverDto = await waiverService.GetStudentWaiverAsync(waiverId);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task GetStudentWaiver_ThrowsExceptionIfSectionNotFound()
            {
                Section nullSection = null;
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>(), false)).Returns(Task.FromResult(nullSection));
                var waiverDto = await waiverService.GetStudentWaiverAsync(waiverId);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentWaiver_ThrowsPermissionsExceptionIfCurrentUserIsNotSectionFaculty()
            {
                var waiverDto = await waiverService.GetStudentWaiverAsync(waiverId);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentWaiver_ThrowsExceptionIfWaiverRepoResponseNull()
            {
                waiverRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).Throws(new KeyNotFoundException());
                var waiverDto = await waiverService.GetStudentWaiverAsync(waiverId);
            }

            [TestMethod]
            public async Task GetStudentWaiver_ReturnsWaiver()
            {
                // Add this faculty to the mocked section response
                section.AddFaculty("0000011");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1", false)).Returns(Task.FromResult(section));

                // Act
                var waiverDto = await waiverService.GetStudentWaiverAsync(waiverId);

                // Spot-check resulting dto
                Assert.AreEqual(waiverResponse.StudentId, waiverDto.StudentId);
                Assert.AreEqual(3, waiverDto.RequisiteWaivers.Count());
                Assert.AreEqual(Ellucian.Colleague.Dtos.Student.WaiverStatus.Denied, waiverDto.RequisiteWaivers.ElementAt(0).Status);
            }


            private StudentWaiver BuildWaiverRepoResponse()
            {
                var waiver1 = new StudentWaiver(waiverId, studentId, "", "SEC1", "OTHER", "2014/FA");
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW1", WaiverStatus.Denied));
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW2", WaiverStatus.NotSelected));
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW3", WaiverStatus.Waived));
                return waiver1;
            }
        }

        [TestClass]
        public class CreateStudentWaiver : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentWaiverRepository> waiverRepoMock;
            private IStudentWaiverRepository waiverRepository;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private IStudentReferenceDataRepository referenceDataRepository;
            private Mock<IReferenceDataRepository> baseReferenceDataRepositoryMock;
            private IReferenceDataRepository baseReferenceDataRepository;
            private Mock<ICourseRepository> courseRepositoryMock;
            private ICourseRepository courseRepository;
            private IStudentWaiverService waiverService;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Section section;
            private Course course;
            private List<StudentWaiver> waiversResponse;
            private Dtos.Student.StudentWaiver waiverDto;
            private string studentId;
            private List<StudentProgram> studentPrograms;
            private Domain.Student.Entities.StudentWaiver studentWaiver;

            [TestInitialize]
            public async void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                waiverRepoMock = new Mock<IStudentWaiverRepository>();
                waiverRepository = waiverRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;
                baseReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                baseReferenceDataRepository = baseReferenceDataRepositoryMock.Object;
                courseRepositoryMock = new Mock<ICourseRepository>();
                courseRepository = courseRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                // Set up student ID
                studentId = "Student1";

                // Set up faculty user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Permission(Ellucian.Colleague.Domain.Student.StudentPermissionCodes.CreatePrerequisiteWaiver));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });

                // Mock a section/course response that includes required pre-requisites
                // Get section from the test section repository
                section = (await new TestSectionRepository().GetAsync()).First();
                // set up requisites for the section
                section.Requisites.Add(new Requisite("RW1", true, RequisiteCompletionOrder.Previous, false));
                section.Requisites.Add(new Requisite("RW2", true, RequisiteCompletionOrder.PreviousOrConcurrent, false));
                section.Requisites.Add(new Requisite("RW3", true, RequisiteCompletionOrder.Previous, false));
                section.OverridesCourseRequisites = true;
                // set up faculty for the section
                section.AddFaculty(currentUserFactory.CurrentUser.PersonId);
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1", false)).Returns(Task.FromResult(section));
                // Mock course response (for requisite checking)--get the course from the test course repository using the course identified in the section
                course = await new TestCourseRepository().GetAsync(section.CourseId);
                courseRepositoryMock.Setup(repo => repo.GetAsync(section.CourseId)).Returns(Task.FromResult(course));

                // Mock entity to dto Adapters
                var waiverDtoAdapter = new StudentWaiverEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiver, Ellucian.Colleague.Dtos.Student.StudentWaiver>()).Returns(waiverDtoAdapter);
                var requisiteWaiverDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>()).Returns(requisiteWaiverDtoAdapter);

                // Mock dto to entity adapters
                var waiverEntityAdapter = new StudentWaiverDtoToEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentWaiver, Ellucian.Colleague.Domain.Student.Entities.StudentWaiver>()).Returns(waiverEntityAdapter);
                var requisiteWaiverEntityAdapter = new AutoMapperAdapter<Ellucian.Colleague.Dtos.Student.RequisiteWaiver, Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>()).Returns(requisiteWaiverDtoAdapter);

                // Mock GetSectionWaivers response
                waiversResponse = BuildWaiverRepoResponse();
                waiverRepoMock.Setup(repo => repo.GetSectionWaiversAsync(It.IsAny<string>())).Returns(Task.FromResult(waiversResponse));

                // Build an incoming waiver dto
                waiverDto = waiverDtoAdapter.MapToType(waiversResponse.ElementAt(0));

                // Mock repository CreateWaiver response
                studentWaiver = waiversResponse.Where(w => w.StudentId == studentId).FirstOrDefault();
                waiverRepoMock.Setup(repo => repo.CreateSectionWaiverAsync(It.IsAny<StudentWaiver>())).Returns(Task.FromResult(studentWaiver));

                // Set up reference data repository value for WaiverReasons
                var waiverReasons = BuildWaiverReasons();
                referenceDataRepositoryMock.Setup(x => x.GetStudentWaiverReasonsAsync()).Returns(Task.FromResult(waiverReasons.AsEnumerable()));

                // Set up student program repository response
                studentPrograms = new List<StudentProgram>() { new StudentProgram(studentId, "MATH.BA", "2012") };
                studentProgramRepositoryMock.Setup(x => x.GetAsync(studentId)).ReturnsAsync(studentPrograms);

                waiverService = new StudentWaiverService(adapterRegistry, waiverRepository, sectionRepository, studentProgramRepository, referenceDataRepository, baseReferenceDataRepository, courseRepository, currentUserFactory, roleRepo, logger, studentRepository, configurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                waiverService = null;
                waiverRepository = null;
                sectionRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CreateStudentWaiver_ThrowsExceptionIfUserDoesNotHavePermission()
            {
                // Remove any existing permission from the faculty to test this scenario
                var permission = facultyRole.Permissions.Where(p => p.Code == Domain.Student.StudentPermissionCodes.CreatePrerequisiteWaiver).FirstOrDefault();
                if (permission != null)
                {
                    facultyRole.RemovePermission(permission);
                }
                await waiverService.CreateStudentWaiverAsync(waiverDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateStudentWaiver_ThrowsExceptionIfWaiverIsNull()
            {
                await waiverService.CreateStudentWaiverAsync(null);
            }

            [TestMethod]
            public async Task CreateStudentWaiver_CreatesWaiver()
            {
                var createdWaiver = await waiverService.CreateStudentWaiverAsync(waiverDto);
                Assert.AreEqual(waiverDto.Comment, createdWaiver.Comment);
                Assert.AreEqual(waiverDto.ReasonCode, createdWaiver.ReasonCode);
                Assert.AreEqual(waiverDto.RequisiteWaivers.Count(), createdWaiver.RequisiteWaivers.Count());
                for (int i = 0; i < waiverDto.RequisiteWaivers.Count(); i++)
                {
                    Assert.AreEqual(waiverDto.RequisiteWaivers.ElementAt(i).RequisiteId, createdWaiver.RequisiteWaivers.ElementAt(i).RequisiteId);
                    Assert.AreEqual(waiverDto.RequisiteWaivers.ElementAt(i).Status, createdWaiver.RequisiteWaivers.ElementAt(i).Status);
                }
                Assert.AreEqual(waiverDto.SectionId, createdWaiver.SectionId);
                Assert.AreEqual(waiverDto.StudentId, createdWaiver.StudentId);
            }

            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateStudentWaiver_ThrowsExceptionIfReasonCodeNotValid()
            {
                waiverDto.ReasonCode = "blah";
                await waiverService.CreateStudentWaiverAsync(waiverDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task CreateStudentWaiver_ThrowsExceptionIfSectionIdEmpty()
            {
                waiverDto.SectionId = string.Empty;
                await waiverService.CreateStudentWaiverAsync(waiverDto);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task CreateStudentWaiver_ExceptionIfSectionThrowsKeyNotFoundException()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>(), false)).Throws(new KeyNotFoundException());
                await waiverService.CreateStudentWaiverAsync(waiverDto);
            }


            [TestMethod]
            [ExpectedException(typeof(ColleagueWebApiException))]
            public async Task CreateStudentWaiver_ThrowsExceptionIfUnwaiverableRequisitesInWaiver()
            {
                section.Requisites = new List<Requisite>();
                section.Requisites.Add(new Requisite("RW1", true, RequisiteCompletionOrder.Previous, false));
                waiverDto.RequisiteWaivers = new List<Dtos.Student.RequisiteWaiver>();
                waiverDto.RequisiteWaivers.Add(new Dtos.Student.RequisiteWaiver() { RequisiteId = "RW2", Status = Dtos.Student.WaiverStatus.Denied });
                await waiverService.CreateStudentWaiverAsync(waiverDto);
            }


            private List<StudentWaiver> BuildWaiverRepoResponse()
            {
                List<StudentWaiver> waivers = new List<StudentWaiver>();

                var waiver1 = new StudentWaiver("1", "Student1", "", "SEC1", "OTHER", "This is a waiver comment\r\ncoming from either user or\rColleague", "2014/FA");
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW1", WaiverStatus.Denied));
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW2", WaiverStatus.NotSelected));
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW3", WaiverStatus.Waived));

                waivers.Add(waiver1);

                var waiver2 = new StudentWaiver("2", "Student2", "", "SEC1", "LIFE", "2014/FA");
                waiver2.AddRequisiteWaiver(new RequisiteWaiver("RW2", WaiverStatus.Denied));
                waiver2.AddRequisiteWaiver(new RequisiteWaiver("RW3", WaiverStatus.Denied));

                waivers.Add(waiver2);

                return waivers;
            }

            private List<StudentWaiverReason> BuildWaiverReasons()
            {
                List<StudentWaiverReason> reasons = new List<StudentWaiverReason>();
                reasons.Add(new StudentWaiverReason("OTHER", "Other reason"));
                reasons.Add(new StudentWaiverReason("LIFE", "Life experience"));
                reasons.Add(new StudentWaiverReason("DISC", "Faculty discretion"));
                return reasons;
            }
        }

        [TestClass]
        public class GetStudentWaivers : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStudentWaiverRepository> waiverRepoMock;
            private IStudentWaiverRepository waiverRepository;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private IStudentReferenceDataRepository referenceDataRepository;
            private Mock<IReferenceDataRepository> baseReferenceDataRepositoryMock;
            private IReferenceDataRepository baseReferenceDataRepository;
            private Mock<ICourseRepository> courseRepositoryMock;
            private ICourseRepository courseRepository;
            private IStudentWaiverService waiverService;
            private List<StudentWaiver> waiversResponse;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private string studentId;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                waiverRepoMock = new Mock<IStudentWaiverRepository>();
                waiverRepository = waiverRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;
                baseReferenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                baseReferenceDataRepository = baseReferenceDataRepositoryMock.Object;
                courseRepositoryMock = new Mock<ICourseRepository>();
                courseRepository = courseRepositoryMock.Object;
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                studentId = "0000011";
                // Mock Waivers response
                waiversResponse = BuildWaiverRepoResponse();
                waiverRepoMock.Setup(repo => repo.GetStudentWaiversAsync(It.IsAny<string>())).Returns(Task.FromResult(waiversResponse));

                // Mock Adapters
                var waiverDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiver, Ellucian.Colleague.Dtos.Student.StudentWaiver>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentWaiver, Ellucian.Colleague.Dtos.Student.StudentWaiver>()).Returns(waiverDtoAdapter);
                var requisiteWaiverDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RequisiteWaiver, Ellucian.Colleague.Dtos.Student.RequisiteWaiver>()).Returns(requisiteWaiverDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                waiverService = new StudentWaiverService(adapterRegistry, waiverRepository, sectionRepository, studentProgramRepository, referenceDataRepository, baseReferenceDataRepository, courseRepository, currentUserFactory, roleRepo, logger, studentRepository, configurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                waiverService = null;
                waiverRepository = null;
                sectionRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentWaivers_ThrowsExceptionIfStudentIdNull()
            {
                var waiverDto = await waiverService.GetStudentWaiversAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentWaivers_ThrowsExceptionIfWaiverIdEmpty()
            {
                var waiverDto = await waiverService.GetStudentWaiversAsync(string.Empty);
            }


            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentWaivers_ThrowsExceptionIfOtherExceptionThrownByRepositoryGet()
            {
                waiverRepoMock.Setup(repo => repo.GetStudentWaiversAsync(It.IsAny<string>())).Throws(new Exception());
                var waiverDto = await waiverService.GetStudentWaiversAsync(studentId);
            }

            [TestMethod]
            public async Task GetStudentWaivers_EmptyStudentWaiversEntityByRepositoryGet()
            {
                waiverRepoMock.Setup(repo => repo.GetStudentWaiversAsync(It.IsAny<string>())).ReturnsAsync(new List<StudentWaiver>());
                var waiverDto = await waiverService.GetStudentWaiversAsync(studentId);
                Assert.AreEqual(waiverDto.Count(), 0);
            }

            [TestMethod]
            public async Task GetStudentWaivers_NullStudentWaiversEntityByRepositoryGet()
            {
                waiverRepoMock.Setup(repo => repo.GetStudentWaiversAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                var waiverDto = await waiverService.GetStudentWaiversAsync(studentId);
                Assert.AreEqual(waiverDto.Count(), 0);
            }

            //check adapter worked fine and mappping happened good

            [TestMethod]
            public async Task GetStudentWaivers_ReturnValidStudentWaivers()
            {
                waiverRepoMock.Setup(repo => repo.GetStudentWaiversAsync(It.IsAny<string>())).ReturnsAsync(waiversResponse);
                var waiverDto = (await waiverService.GetStudentWaiversAsync(studentId)).ToList();
                Assert.AreEqual(waiverDto.Count(), 2);
                Assert.AreEqual(waiverDto[0].RequisiteWaivers.Count(), 3);
                Assert.AreEqual(waiverDto[1].RequisiteWaivers.Count(), 2);
            }

            private List<StudentWaiver> BuildWaiverRepoResponse()
            {
                List<StudentWaiver> waivers = new List<StudentWaiver>();

                var waiver1 = new StudentWaiver("1", studentId, "", "SEC1", "OTHER", "2014/FA");
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW1", WaiverStatus.Denied));
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW2", WaiverStatus.NotSelected));
                waiver1.AddRequisiteWaiver(new RequisiteWaiver("RW3", WaiverStatus.Waived));

                waivers.Add(waiver1);

                var waiver2 = new StudentWaiver("2", studentId, "", "SEC1", "LIFE", "2014/FA");
                waiver2.AddRequisiteWaiver(new RequisiteWaiver("RW2", WaiverStatus.Denied));
                waiver2.AddRequisiteWaiver(new RequisiteWaiver("RW3", WaiverStatus.Denied));

                waivers.Add(waiver2);

                return waivers;
            }

        }
    }
}