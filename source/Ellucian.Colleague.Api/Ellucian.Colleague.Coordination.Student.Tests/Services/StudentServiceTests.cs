// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.Student.Adapters;
using System.IO;
using System.Threading.Tasks;
using StudentCohortEntity = Ellucian.Colleague.Domain.Student.Entities.StudentCohort;
using StudentClassificationEntity = Ellucian.Colleague.Domain.Student.Entities.StudentClassification;
using Ellucian.Colleague.Coordination.Base;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentServiceTests
    {
        // Sets up a Current user that is a student and one that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role advisorRole = new Role(105, "Advisor");
            protected Role viewStudentRole = new Role(1, "VIEW.STUDENT.INFORMATION");

            public class StudentUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Johnny",
                            PersonId = "0000894",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class AdvisorUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000111",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Advisor",
                            Roles = new List<string>() { "Advisor" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            // Represents a Ethos system user
            public class EthosUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "Ethos",
                            PersonId = "Ethos",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Ethos",
                            Roles = new List<string>() { "VIEW.STUDENT.INFORMATION" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class GetEedmStudentTests : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private IEnumerable<Domain.Student.Entities.Student> studentList;
            private IEnumerable<Domain.Student.Entities.Student> oneStudentList;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepositoryMock;
            private IStaffRepository staffRepository;
            private ICurrentUserFactory currentUserFactory;


            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IEnumerable<StudentCohort> cohorts;
            private IEnumerable<StudentType> studentTypes;
            private IEnumerable<ResidencyStatus> residencyTypes;
            private Permission permissionViewAnyStudent;

            private string student1Guid = "6b227dcc-db1c-41a2-b809-8e400e5d0682";
            private string student2Guid = "b88342ca-03d3-4255-9d69-3dfd434c60ff";
            private string student1Id = "1234567";
            private string student2Id = "7654321";
            private string program1Guid = "cbac5aee-71e9-4f2d-ab44-3266d43390d4";
            private string program2Guid = "1f5d03d9-e3cb-43be-8ec9-dc606f5cf90f";
            private string academicCred1Guid = "911f1522-3fee-409e-a782-535f588a3419";
            private string academicCred2Guid = "4f2ead3b-210d-435c-a2c3-624e2683dbef";
            private string cohort1Guid = "5ed36f83-d3c4-400b-adb2-097d495153a0";
            private string cohort1Code = "cod";
            private string cohort2Guid = "82455ca0-e07e-49f5-a402-7948234b9ed1";
            private string cohort2Code = "mor";
            private string studentType1Guid = "8ce0fddc-b10d-4de6-885f-30d0aeaf9887";
            private string studentType1Code = "typ";
            private string studentType2Guid = "83b6a42b-4667-457b-93e8-8735ac4f6d3f";
            private string studentType2Code = "top";
            private string residency1Guid = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
            private string residency1Code = "code1";
            private string residency2Guid = "bd54668d-50d9-416c-81e9-2318e88571a1";
            private string residency2Code = "code2";


            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;

                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;

                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepository = staffRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                //Mock roles repo and permission
                permissionViewAnyStudent = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation);
                viewStudentRole.AddPermission(permissionViewAnyStudent);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewStudentRole });

                // Mock the repo call for cohort
                cohorts = new List<StudentCohort>()
                {
                    new StudentCohort(cohort1Guid, cohort1Code, "Test Data"),
                    new StudentCohort(cohort2Guid, cohort2Code, "Test Data")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAllStudentCohortAsync(It.IsAny<bool>())).ReturnsAsync(cohorts);

                // Mock the repo call for Student types
                studentTypes = new List<StudentType>()
                {
                    new StudentType(studentType1Guid, studentType1Code, "Test Data"),
                    new StudentType(studentType2Guid, studentType2Code, "Test Data")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetStudentTypesAsync(It.IsAny<bool>())).ReturnsAsync(studentTypes);

                // Mock the repo call for residency types
                residencyTypes = new List<ResidencyStatus>()
                {
                    new ResidencyStatus(residency1Guid, residency1Code, "Test Data1"),
                    new ResidencyStatus(residency2Guid, residency2Guid, "Test Data2")
                };
                studentRepoMock.Setup(repo => repo.GetResidencyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(residencyTypes);

                //mock the call to get the personid
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(student1Guid)).ReturnsAsync(student1Id);

                //setup student entity data
                student1 = new Domain.Student.Entities.Student(student1Guid, student1Id, new List<string>(){program1Guid, program2Guid}, new List<string>(){academicCred1Guid, academicCred2Guid},"Boyd", false);
                student2 = new Domain.Student.Entities.Student(student2Guid, student2Id, new List<string>(){program1Guid, program2Guid}, new List<string>(){academicCred1Guid, academicCred2Guid},"Boyd", false);


                studentList = new List<Domain.Student.Entities.Student>() {student1, student2};

                oneStudentList = new List<Domain.Student.Entities.Student>() { student1 };

                // Mock student repo response
                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "",
                            "", "")).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(studentList, 2));

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), student1Id, "",
                            "", "")).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(oneStudentList, 1));

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", studentType1Code,
                            "", "")).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(studentList, 2));

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "",
                            cohort1Code, "")).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(studentList, 2));

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudentsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), "", "",
                            "", residency1Code)).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(studentList, 2));



                // Set up current user
                currentUserFactory = new CurrentUserSetup.EthosUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo,
                    academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo,
                    referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo,
                    staffRepository, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetStudents_NoFitlers_V7()
            {
                var studentDtoList = await studentService.GetStudentsAsync(0, 100, false, null, null, null);

                Assert.AreEqual(2, studentDtoList.Item1.Count());
            }

            [TestMethod]
            public async Task GetStudents_PersonFitler_V7()
            {
                var studentDtoList = await studentService.GetStudentsAsync(0, 100, false, student1Guid, null, null);

                Assert.AreEqual(studentDtoList.Item2, studentDtoList.Item1.Count());
                Assert.AreEqual(student1Guid, studentDtoList.Item1.ToList()[0].Id);
            }

            [TestMethod]
            public async Task GetStudents_CohortFitler_V7()
            {
                var studentDtoList = await studentService.GetStudentsAsync(0, 100, false, null, null, cohort1Guid);

                Assert.AreEqual(2, studentDtoList.Item1.Count());
            }

            [TestMethod]
            public async Task GetStudents_StudentTypeFitler_V7()
            {
                var studentDtoList = await studentService.GetStudentsAsync(0, 100, false, null, studentType1Guid, null);

                Assert.AreEqual(2, studentDtoList.Item1.Count());
            }

            [TestMethod]
            public async Task GetStudents_StudentResidencyFitler_V7()
            {
                var studentDtoList = await studentService.GetStudentsAsync(0, 100, false, null, null,null, residency1Guid);

                Assert.AreEqual(2, studentDtoList.Item1.Count());
            }

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task GetStudentsBadCohortFilterTest_V7()
            //{
            //    var studentDto = await studentService.GetStudentsAsync(0, 100, false, null, null, "3456");
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task GetStudentsBadStudentTypeFilterTest_V7()
            //{
            //    var studentDto = await studentService.GetStudentsAsync(0, 100, false, null, "blah", null);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task GetStudentsBadPersonFilterTest_V7()
            //{
            //    personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync("blah")).ReturnsAsync("");

            //    var studentDto = await studentService.GetStudentsAsync(0, 100, false, "blah", null, null, null);
            //}

            //[TestMethod]
            //[ExpectedException(typeof(ArgumentException))]
            //public async Task GetStudentsBadResidencyFilterTest_V7()
            //{
            //    personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync("blah")).ReturnsAsync("");

            //    var studentDto = await studentService.GetStudentsAsync(0, 100, false, null, null, null, "3456");
            //}

        }

        [TestClass]
        public class Get_AsStudentUser : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;

            private Mock<IPersonRepository>personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;

            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;

                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;


                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock student repo response
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely", PersonalPronounCode = "XHE"};
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock Adapters
                var studentDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>()).Returns(studentDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo,logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }


            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task ThrowsErrorIfStudentNotFound()
            {
                Domain.Student.Entities.Student student = null;
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student);
                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task ReturnsStudentDto()
            {
                var result = await studentService.GetAsync("0000894");
                Assert.IsTrue(result is PrivacyWrapper<Dtos.Student.Student>);
                Assert.AreEqual(student1.Id, result.Dto.Id);
                Assert.AreEqual(student1.FirstName, result.Dto.FirstName);
                Assert.IsNotNull(result.Dto.PersonalPronounCode);
                Assert.AreEqual(student1.PersonalPronounCode, result.Dto.PersonalPronounCode);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfNotSelf()
            {
                studentRepoMock.Setup(repo => repo.GetAsync("0004002")).ReturnsAsync(student2);
                await studentService.GetAsync("0004002");
            }
        }

        [TestClass]
        public class Get_AsAdvisorUser : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Domain.Base.Entities.Staff staff;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Domain.Student.Entities.Student studentWithPrivacyCode;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;

                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;

                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock student repo response
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);
                //student with privacy code
                studentWithPrivacyCode = new Domain.Student.Entities.Student("00004003", "Jones", 803, new List<string>() { "BA.MATH" }, new List<string>() { "1", "2" },"A");
                studentRepoMock.Setup(repo => repo.GetAsync("00004003")).ReturnsAsync(studentWithPrivacyCode);

                studentWithPrivacyCode = new Domain.Student.Entities.Student("00004004", "Jones", 803, new List<string>() { "BA.MATH" }, new List<string>() { "1", "2" }, "B");
                studentRepoMock.Setup(repo => repo.GetAsync("00004004")).ReturnsAsync(studentWithPrivacyCode);
                //staff record for the advisor
                staff = new Domain.Base.Entities.Staff("0000111", "staff member");
                staff.IsActive = true;
                staff.PrivacyCodes = new List<string>() { "A" };
                staffRepoMock.Setup(r => r.Get("0000111")).Returns(staff);

                // Mock Adapters
                var studentDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>()).Returns(studentDtoAdapter);
                var advisementDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Advisement, Dtos.Student.Advisement>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Advisement, Dtos.Student.Advisement>()).Returns(advisementDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo,staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAnyAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAnyAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasUpdateAnyAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasAllAccessAnyAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowsAccessIfUserHasViewStudentInformationPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task NoAccessIfUserHasNoPermission()
            {
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorInStudentsAdvisorList()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfAdvisorNotInStudentsAdvisorList()
            {
                // Set up needed permission. Advisor is personid 0000111
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowedIfUpdateccessAndAdvisor()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfUpdateAccessButNotAdvisor()
            {
                // Set up needed permission. Advisor is personid 0000111
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowedIfReviewAccessAndAdvisor()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfReviewAccessButNotAdvisor()
            {
                // Set up needed permission. Advisor is personid 0000111
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowedIfAllAccessAndAdvisor()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                student1.AddAdvisor("0000111");
                student1.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
                
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrowsErrorIfAllAccessButNotAdvisor()
            {
                // Set up needed permission. Advisor is personid 0000111
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Add advisor to student's advisor list
                student1.AddAdvisor("0000896");
                student1.AddAdvisement("0000896", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task StaffRecordHaveSamePrivacyCodeAsStudent()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                var studentDtoWithWrapper= await studentService.GetAsync("00004003");
                Assert.AreEqual(studentDtoWithWrapper.Dto.Id, "00004003");
                Assert.AreEqual(studentDtoWithWrapper.Dto.DegreePlanId, studentWithPrivacyCode.DegreePlanId);
                Assert.IsNotNull(studentDtoWithWrapper.Dto.DegreePlanId);
                Assert.AreEqual(studentDtoWithWrapper.Dto.LastName, studentWithPrivacyCode.LastName);
                Assert.IsNotNull(studentDtoWithWrapper.Dto.ProgramIds);
                Assert.AreEqual(studentDtoWithWrapper.Dto.ProgramIds.Count, studentWithPrivacyCode.ProgramIds.Count);
            }

            [TestMethod]
            public async Task StaffRecordDoesNotHaveSamePrivacyCodeAsStudent()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                var studentDtoWithWrapper = await studentService.GetAsync("00004004");
                Assert.AreEqual(studentDtoWithWrapper.Dto.Id, "00004004");
                Assert.AreNotEqual(studentDtoWithWrapper.Dto.DegreePlanId, studentWithPrivacyCode.DegreePlanId);
                Assert.IsNull(studentDtoWithWrapper.Dto.DegreePlanId);
                Assert.AreEqual(studentDtoWithWrapper.Dto.LastName, studentWithPrivacyCode.LastName);
                Assert.IsNull(studentDtoWithWrapper.Dto.ProgramIds);
            }
        }

        [TestClass]
        public class CheckRegistrationEligibility_CheckRegistrationEligibility2_Advisor : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private ICurrentUserFactory currentUserFactory;

            private string id1 = "00001";
            private List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage> messages1;

            private string id2 = "0000894";
            private List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage> messages2;

            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;

                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock student repo response
                student1 = new Domain.Student.Entities.Student("00001", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student2 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student2.AddAdvisement("0000896", null, null, null);
                student2.AddAdvisement("0000111", null, null, null);
                var student2Access = student2.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(l => l.Contains("0000894")))).ReturnsAsync(new List<StudentAccess>() { student2Access }.AsEnumerable());

                messages1 = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true));
                messages2 = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage>() { new Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage() { Message = "Failed Registration Eligibility" } };
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id2)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages2, false));

                // Mock Adapters
                var regEligDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibility, Ellucian.Colleague.Dtos.Student.RegistrationEligibility>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibility, Ellucian.Colleague.Dtos.Student.RegistrationEligibility>()).Returns(regEligDtoAdapter);
                var regMessageDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage, Ellucian.Colleague.Dtos.Student.RegistrationMessage>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage, Ellucian.Colleague.Dtos.Student.RegistrationMessage>()).Returns(regMessageDtoAdapter);
                var studentDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>()).Returns(studentDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo,staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckRegistrationEligibility2_ViewStudentInformationPermission()
            {
                // A user with VIEW.STUDENT.INFORMATION - such as a faculty member - is not enough to see a student's registration eligibility
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(messages1.Count(), result.Messages.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility2_AdvisorViewAnyAdviseePermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await studentService.CheckRegistrationEligibility2Async(id2);
                Assert.AreEqual(messages2.Count(), result.Messages.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility2_ViewAssignedAdviseesPermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await studentService.CheckRegistrationEligibility2Async(id2);
                Assert.AreEqual(messages2.Count(), result.Messages.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility_ViewStudentInformationPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                var result = await studentService.CheckRegistrationEligibilityAsync(id1);
                Assert.AreEqual(messages1.Count(), result.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility_ViewAnyAdviseePermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await studentService.CheckRegistrationEligibilityAsync(id2);
                Assert.AreEqual(messages2.Count(), result.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility_ViewAssignedAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock get of studentAccess entity from repository since student is not read in this version
                var student2Access = student2.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<StudentAccess>() { student2Access }.AsEnumerable());
                var result = await studentService.CheckRegistrationEligibilityAsync(id2);
                Assert.AreEqual(messages2.Count(), result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckRegistrationEligibility_NoAccess_UserHasNoPermission()
            {
                var messages = await studentService.CheckRegistrationEligibilityAsync(id2);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckRegistrationEligibility2_NoAccess_UserHasNoPermission()
            {
                var messages = await studentService.CheckRegistrationEligibility2Async(id2);
            }
        }

        [TestClass]
        public class CheckRegistrationEligibility2_AsSelf : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private string id1 = "0000894";
            private List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage> messages1;
            private Domain.Student.Entities.Student student1;

            [TestInitialize]
            public async void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;

                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;


                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock student repo response
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                // add RegistrationPriorityIds to Student
                List<string> priorityIds = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
                foreach (var id in priorityIds)
                {
                    student1.AddRegistrationPriority(id);
                }
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                // Mock for check registration eligibility 2
                var regEligibility = await new TestStudentRepository().CheckRegistrationEligibilityAsync("studentId");
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(regEligibility);

                // Mock the term repository get for terms - instead of getting all just getting some that matter.
                List<Ellucian.Colleague.Domain.Student.Entities.Term> allTerms = new List<Ellucian.Colleague.Domain.Student.Entities.Term>();
                Ellucian.Colleague.Domain.Student.Entities.Term term3 = new Ellucian.Colleague.Domain.Student.Entities.Term("term3", "Term 3", DateTime.Now, DateTime.Now.AddDays(180), 2014, 1, true, true, "term3", true);
                allTerms.Add(term3);
                Ellucian.Colleague.Domain.Student.Entities.Term term4 = new Ellucian.Colleague.Domain.Student.Entities.Term("term4", "Term 4", DateTime.Now, DateTime.Now.AddDays(180), 2014, 1, true, true, "term4", false);
                allTerms.Add(term4);
                termRepoMock.Setup(trepo => trepo.GetAsync()).ReturnsAsync(allTerms);

                // Mock Adapters
                var regEligibilityDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibility, Ellucian.Colleague.Dtos.Student.RegistrationEligibility>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibility, Ellucian.Colleague.Dtos.Student.RegistrationEligibility>()).Returns(regEligibilityDtoAdapter);

                var studentDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>()).Returns(studentDtoAdapter);

                var registrationEligibilityDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibility, Ellucian.Colleague.Dtos.Student.RegistrationEligibility>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibility, Ellucian.Colleague.Dtos.Student.RegistrationEligibility>()).Returns(registrationEligibilityDtoAdapter);

                var registrationEligibilityTermDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibilityTerm, Ellucian.Colleague.Dtos.Student.RegistrationEligibilityTerm>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.RegistrationEligibilityTerm, Ellucian.Colleague.Dtos.Student.RegistrationEligibilityTerm>()).Returns(registrationEligibilityTermDtoAdapter);
                
                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo,staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task CheckRegistrationEligibilty_Original_ReturnsMessages()
            {
                messages1 = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                var result = await studentService.CheckRegistrationEligibilityAsync(id1);
                Assert.AreEqual(messages1.Count(), result.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Eligible2_NullStudentId_ThrowsException()
            {
                var result = await studentService.CheckRegistrationEligibility2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Eligible2_EmptyStudentId_ThrowsException()
            {
                var result = await studentService.CheckRegistrationEligibility2Async("");
            }

            [TestMethod]
            public async Task Eligible2_NoPriorities_NoTerms_Success()
            {
                messages1 = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(new List<RegistrationPriority>().AsEnumerable());
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(messages1.Count(), result.Messages.Count());
                Assert.AreEqual(0, result.Terms.Count());
            }

            [TestMethod]
            public async Task Eligible2_IsEligible()
            {
                messages1 = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(true, result.IsEligible);
            }

            [TestMethod]
            public async Task Eligible2_HasOverride()
            {
                messages1 = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(true, result.HasOverride);
            }

            [TestMethod]
            public async Task Eligible2_TermRequiresPriorities_NoPriorities_NotEligible()
            {
                // This student has no priorities but term3 requires one.
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(new List<RegistrationPriority>().AsEnumerable());
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(0, result.Messages.Count());
                Assert.AreEqual(6, result.Terms.Count());
                // Term 3 should be open
                var t3 = result.Terms.Where(t => t.TermCode == "term3").FirstOrDefault();
                Assert.AreEqual(Ellucian.Colleague.Dtos.Student.RegistrationEligibilityTermStatus.NotEligible, t3.Status);
        }

            [TestMethod]
            public async Task Eligible2_WithTerms_WithFuturePriorities()
            {
                // Setup necessary data
                List<RegistrationPriority> priorities = new List<RegistrationPriority>();
                RegistrationPriority priority = new RegistrationPriority("998", "0000894", "term3", DateTime.Today.AddYears(1), DateTime.Today.AddYears(2));
                priorities.Add(priority);
                RegistrationPriority priority2 = new RegistrationPriority("999", "0000894", "term4", new DateTime(2020, 9, 15, 15, 0, 0), new DateTime(2020, 9, 15, 16, 0, 0));
                priorities.Add(priority2);
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(priorities.AsEnumerable());

                // Take Action
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(6, result.Terms.Count());
                // Term 3 should be future now instead of not eligible.
                var t3 = result.Terms.Where(t => t.TermCode == "term3").FirstOrDefault();
                Assert.AreEqual(Ellucian.Colleague.Dtos.Student.RegistrationEligibilityTermStatus.Future, t3.Status);
                Assert.IsTrue(t3.FailedRegistrationPriorities);
            }

            [TestMethod]
            public async Task Eligible2_WithTerms_WithPastPriorities()
            {
                // Setup necessary data
                List<RegistrationPriority> priorities = new List<RegistrationPriority>();
                RegistrationPriority priority = new RegistrationPriority("998", "0000894", "term3", new DateTime(2010, 9, 15, 15, 0, 0), new DateTime(2010, 9, 15, 16, 0, 0));
                priorities.Add(priority);
                RegistrationPriority priority2 = new RegistrationPriority("999", "0000894", "term4", new DateTime(2010, 9, 15, 15, 0, 0), new DateTime(2010, 9, 15, 16, 0, 0));
                priorities.Add(priority2);
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(priorities.AsEnumerable());

                // Take Action
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(6, result.Terms.Count());
                // Term 3 should be future now instead of not eligible.
                var t3 = result.Terms.Where(t => t.TermCode == "term3").FirstOrDefault();
                Assert.AreEqual(Ellucian.Colleague.Dtos.Student.RegistrationEligibilityTermStatus.Past, t3.Status);
                Assert.IsTrue(t3.FailedRegistrationPriorities);
            }

            [TestMethod]
            public async Task CheckEligibility_FailsTermRules()
            {
                // Take Action
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                var t1 = result.Terms.Where(t => t.TermCode == "term1").FirstOrDefault();
                var t2 = result.Terms.Where(t => t.TermCode == "term2").FirstOrDefault();
                Assert.IsTrue(t1.FailedRegistrationTermRules);
                Assert.IsFalse(t2.FailedRegistrationTermRules);
            }
        }

        [TestClass]
        public class Search : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                 studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                 baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                 baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock student repo response

                // mock a valid student search response
                var student1 = new Ellucian.Colleague.Domain.Student.Entities.Student("00000001", "Dog", null, null, null, null) { FirstName = "Able" };
                var student2 = new Ellucian.Colleague.Domain.Student.Entities.Student("00000002", "Dog", null, null, null, null) { FirstName = "Baker" };
                var student3 = new Ellucian.Colleague.Domain.Student.Entities.Student("00000003", "Dog", null, null, null, null) { FirstName = "Charlie" };

                var justOne = new List<Ellucian.Colleague.Domain.Student.Entities.Student>() { student1 };
                var justTwo = new List<Ellucian.Colleague.Domain.Student.Entities.Student>() { student2 };
                var allThree = new List<Ellucian.Colleague.Domain.Student.Entities.Student>() { student1, student2, student3 };

                studentRepoMock.Setup(svc => svc.SearchAsync("Dog", null, null, null, null, null)).ReturnsAsync(allThree.AsEnumerable());
                studentRepoMock.Setup(svc => svc.SearchAsync("Dog", null, DateTime.Parse("3/3/33"), null, null, null)).ReturnsAsync(justTwo.AsEnumerable());
                studentRepoMock.Setup(svc => svc.SearchAsync("Dog", "Able", null, null, null, null)).ReturnsAsync(justOne.AsEnumerable());
                studentRepoMock.Setup(svc => svc.SearchAsync("Dog", "Baker", null, null, null, null)).ReturnsAsync(justTwo.AsEnumerable());
                studentRepoMock.Setup(svc => svc.SearchAsync("Smith", null, null, null, null, null)).Throws(new KeyNotFoundException("x"));
                studentRepoMock.Setup(svc => svc.SearchAsync(null, null, null, null, null, null)).Throws(new KeyNotFoundException("x"));

                // Mock Adapters
                var studentDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>()).Returns(studentDtoAdapter);

                // Mock advisor role so that permission exception not thrown
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            // needs permissions now

            [TestMethod]
            public async Task Search_single()
            {
                var students = await studentService.SearchAsync("Dog", null, "Able", null, null, null);
                Assert.AreEqual(1, students.Count());
            }

            [TestMethod]
            public async Task Search_single_two()
            {
                var students = await studentService.SearchAsync("Dog", DateTime.Parse("3/3/33"), null, null, null, null);
                Assert.AreEqual(1, students.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Search_single_allNull_throws()
            {
                var students = await studentService.SearchAsync(null, null, null, null, null, null);
            }

            [TestMethod]
            public async Task Search_multi()
            {
                var students = await studentService.SearchAsync("Dog", null, null, null, null, null);
                Assert.AreEqual(3, students.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task Search_empty()
            {
                var students = await studentService.SearchAsync("Smith", null, null, null, null, null);
            }
        }

        [TestClass]
        public class TrancriptRestrictions2 : CurrentUserSetup
        {
            private StudentService advisorStudentService;
            private StudentService studentStudentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Get Student data from the TestStudentRepository
                var studentId1 = "00004001";
                studentRepoMock.Setup(repo => repo.GetAsync(studentId1)).ReturnsAsync(new TestStudentRepository().Get(studentId1));

                var studentId2 = "0000894";
                studentRepoMock.Setup(repo => repo.GetAsync(studentId2)).ReturnsAsync(new TestStudentRepository().Get(studentId2));

                var studentId3 = "00004003";
                studentRepoMock.Setup(repo => repo.GetAsync(studentId3)).ReturnsAsync(new TestStudentRepository().Get(studentId3));

                // Mock the Student reposiory TranscriptRestriction responses
                studentRepoMock.Setup(repo => repo.GetTranscriptRestrictionsAsync("0000999")).Throws(new KeyNotFoundException());

                IEnumerable<Domain.Student.Entities.TranscriptRestriction> emptyRestrictions = new List<Domain.Student.Entities.TranscriptRestriction>();
                studentRepoMock.Setup(repo => repo.GetTranscriptRestrictionsAsync(studentId1)).ReturnsAsync(emptyRestrictions);

                IEnumerable<Domain.Student.Entities.TranscriptRestriction> oneRestriction = new List<Domain.Student.Entities.TranscriptRestriction>() { new Domain.Student.Entities.TranscriptRestriction() { Code = "TEST", Description = "TEST" } };
                studentRepoMock.Setup(repo => repo.GetTranscriptRestrictionsAsync(studentId2)).ReturnsAsync(oneRestriction);
                
                // Mock Adapters
                var restrictionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.TranscriptRestriction, Ellucian.Colleague.Dtos.Student.TranscriptRestriction>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.TranscriptRestriction, Ellucian.Colleague.Dtos.Student.TranscriptRestriction>()).Returns(restrictionDtoAdapter);

                // Mock advisor role so that permission exception not thrown
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                advisorStudentService = new StudentService(adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, new CurrentUserSetup.AdvisorUserFactory(), roleRepo,staffRepo, logger);
                studentStudentService = new StudentService(adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, new CurrentUserSetup.StudentUserFactory(), roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                studentConfigurationRepo = null;
                adapterRegistry = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetTranscriptRestrictions2_EnforceRestrictionsWithNoTranscriptRestrictions()
            {
                // ViewAnyAdvisee permission - allows access
                var studentConfigEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = true };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);

                var transcriptAccess = await advisorStudentService.GetTranscriptRestrictions2Async("00004001");
                Assert.AreEqual(0, transcriptAccess.TranscriptRestrictions.Count());
                Assert.IsTrue(transcriptAccess.EnforceTranscriptRestriction);
        }

            [TestMethod]
            public async Task GetTranscriptRestrictions2_UnenforceRestrictionsWithNoTranscriptRestrictions()
            {
                // ViewAnyAdvisee permission - allows access
                var studentConfigNotEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = false };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigNotEnforced);

                var transcriptAccess = await advisorStudentService.GetTranscriptRestrictions2Async("00004001");
                Assert.AreEqual(0, transcriptAccess.TranscriptRestrictions.Count());
                Assert.IsFalse(transcriptAccess.EnforceTranscriptRestriction);
            }

            [TestMethod]
            public async Task GetTranscriptRestrictions2_EnforceRestrictionsWithOneTranscriptRestrictions()
            {
                // ViewAnyAdvisee permission - allows access
                var studentConfigEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = true };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);

                var transcriptAccess = await advisorStudentService.GetTranscriptRestrictions2Async("0000894");
                Assert.AreEqual(1, transcriptAccess.TranscriptRestrictions.Count());
                Assert.IsTrue(transcriptAccess.EnforceTranscriptRestriction);
            }

            [TestMethod]
            public async Task GetTranscriptRestrictions2_UnenforceRestrictionsWithOneTranscriptRestrictions()
            {
                // ViewAnyAdvisee permission - allows access
                var studentConfigNotEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = false };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigNotEnforced);

                var transcriptAccess = await advisorStudentService.GetTranscriptRestrictions2Async("0000894");
                Assert.AreEqual(1, transcriptAccess.TranscriptRestrictions.Count());
                Assert.IsFalse(transcriptAccess.EnforceTranscriptRestriction);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetTranscriptRestrictions2_MissingStudent()
            {
                var studentConfigEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = true };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);
                var transcriptAccess = await advisorStudentService.GetTranscriptRestrictions2Async("0000999");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetTranscriptRestrictions2_NullStudentId()
            {
                var studentConfigEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = true };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);
                var transcriptAccess = await advisorStudentService.GetTranscriptRestrictions2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetTranscriptRestrictions2_EmptyStudentId()
            {
                var studentConfigEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = true };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);
                var transcriptAccess = await advisorStudentService.GetTranscriptRestrictions2Async(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetTranscriptRestrictions2_UnauthorizedUser()
            {
                // student cannot access anotherss
                var studentConfigEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = true };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);
                var transcriptAccess = await studentStudentService.GetTranscriptRestrictions2Async("00004003");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetTranscriptRestrictions2_Unauthorized_UserHasViewStudentInformation()
            {
                // Set up student 0000894 as the current user.
                //Mock roles repo and permission for VIEW.STUDENT.INFORMATION but this permission should not allow it.
                var permissionViewAnyStudent = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation);
                viewStudentRole.AddPermission(permissionViewAnyStudent);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewStudentRole });
                var studentConfigEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = true };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);
                var transcriptAccess = await studentStudentService.GetTranscriptRestrictions2Async("00004003");
            }



            [TestMethod]
            public async Task GetTranscriptRestrictions2_AuthorizedStudentUser()
            {
                var studentConfigEnforced = new StudentConfiguration() { FacultyPhoneTypeCode = "OFFICE", FacultyEmailTypeCode = "WORK", EnforceTranscriptRestriction = true };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);
                var transcriptAccess = await studentStudentService.GetTranscriptRestrictions2Async("0000894");
            }
        }

        [TestClass]
        public class GetUngradedTerms:CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Domain.Student.Entities.AcademicHistory historyEntity;
            private Colleague.Dtos.Student.AcademicHistory2 history;
            private string id1 = "00001";
            private string id2 = "00002";


            [TestInitialize]
            public async void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
           
            logger = new Mock<ILogger>().Object;


                // Mock acad history repo response
                var studentAcademicCredits = await new TestAcademicCreditRepository().GetAsync();
                Domain.Student.Entities.GradeRestriction gradeRestriction = new Domain.Student.Entities.GradeRestriction(false);
                historyEntity = new Domain.Student.Entities.AcademicHistory(studentAcademicCredits, gradeRestriction, null);
                history = new AcademicHistoryEntityToAcademicHistory2DtoAdapter(adapterRegistry, logger).MapToType(historyEntity);
                academicHistoryServiceMock.Setup(svc => svc.GetAcademicHistory2Async(id1, false, true, null)).Returns(Task.FromResult(history));
                academicHistoryServiceMock.Setup(svc => svc.GetAcademicHistory2Async(id2, false, true, null)).Throws(new PermissionsException());


                // Mock Adapters
                var studentDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>()).Returns(studentDtoAdapter);

                var termDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Ellucian.Colleague.Dtos.Student.Term>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Term, Ellucian.Colleague.Dtos.Student.Term>()).Returns(termDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UngradedTerms_NoPermissions_OnCurrentUser()
            {
                // Mock term response
                Domain.Student.Entities.Term term = new Domain.Student.Entities.Term("2011/FA", "ss", DateTime.Today, DateTime.Today, 9, 9, false, false, "x", true);
                Domain.Student.Entities.Term term1 = new Domain.Student.Entities.Term("2010/SP", "ss", new DateTime(2010, 01, 15), DateTime.Today, 9, 9, false, false, "x", true);
                termRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Term>() { term, term1 }.AsEnumerable());

                // There are actualy more ungraded terms returned, but this is the only one with current dates 
                var terms = await studentService.GetUngradedTermsAsync(id1);
                Assert.AreEqual(2, terms.Count());
            }

            [TestMethod]
            public async Task OldTermDiscarded()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("VIEW.STUDENT.INFORMATION"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
                //Mock roles repo and permission
                //permissionViewAnyStudent = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation);
                //viewStudentRole.AddPermission(permissionViewAnyStudent);
                //roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewStudentRole });
                // Mock term response

                DateTime SixtyOneDaysAgo = DateTime.Today.AddDays(-61);

                Domain.Student.Entities.Term term1 = new Domain.Student.Entities.Term("2010/SP", "ss", new DateTime(2010, 01, 15), new DateTime(2010, 05, 15), 9, 9, false, false, "x", true);
                Domain.Student.Entities.Term term = new Domain.Student.Entities.Term("2011/FA", "ss", SixtyOneDaysAgo, SixtyOneDaysAgo, 9, 9, false, false, "x", true);
                termRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Term>() { term, term1 }.AsEnumerable());

                var terms = await studentService.GetUngradedTermsAsync(id1);
                Assert.AreEqual(0, terms.Count());
            }

            [TestMethod]
            public async Task NewerTermKept()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                // Mock term response
                DateTime FiftyNineDaysAgo = DateTime.Today.AddDays(-59);
                Domain.Student.Entities.Term term1 = new Domain.Student.Entities.Term("2010/SP", "ss", new DateTime(2010, 01, 15), new DateTime(2010, 05, 15), 9, 9, false, false, "x", true);
                Domain.Student.Entities.Term term = new Domain.Student.Entities.Term("2011/FA", "ss", FiftyNineDaysAgo, FiftyNineDaysAgo, 9, 9, false, false, "x", true);
                termRepoMock.Setup(x => x.GetAsync(It.IsAny<List<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Term>() { term, term1 }.AsEnumerable());

                var terms = await studentService.GetUngradedTermsAsync(id1);
                Assert.AreEqual(1, terms.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ThrownPermissionsPassedThrough()
            {
                // Mock term response
                DateTime FiftyNineDaysAgo = DateTime.Today.AddDays(-59);
                IEnumerable<string> termids = new List<string>() { "2011/FA" };
                Domain.Student.Entities.Term term = new Domain.Student.Entities.Term("2011/FA", "ss", FiftyNineDaysAgo, FiftyNineDaysAgo, 9, 9, false, false, "x", true);
                termRepoMock.Setup(x => x.GetAsync(termids)).ReturnsAsync(new List<Domain.Student.Entities.Term>() { term }.AsEnumerable());

                var terms = await studentService.GetUngradedTermsAsync(id2);
            }
        }

        [TestClass]
        public class OrderTranscript : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            private ICurrentUserFactory currentUserFactory;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock student repo response
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                // Mock Adapter
                var requestDtoAdapter = new TranscriptRequestEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Dtos.Student.Transcripts.TranscriptRequest, Ellucian.Colleague.Domain.Student.Entities.Transcripts.TranscriptRequest>()).Returns(requestDtoAdapter);
                
                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public void CheckMapper()
            {
                Dtos.Student.Transcripts.TranscriptRequest source = new Dtos.Student.Transcripts.TranscriptRequest()
                {
                    TransmissionData = new Dtos.Student.Transcripts.TransmissionData()
                    {
                        DocumentID = "X"
                    },
                    Request = new Dtos.Student.Transcripts.Request()
                    {
                        Recipient = new Dtos.Student.Transcripts.Recipient()
                        {
                            Receiver = new Dtos.Student.Transcripts.Receiver()
                            {
                                RequestorReceiverOrganization = new Dtos.Student.Transcripts.RequestorReceiverOrganization()
                                {
                                    Contacts = new List<Dtos.Student.Transcripts.Contacts>()
                                    {
                                        new Dtos.Student.Transcripts.Contacts(){
                                            FaxPhone = new Dtos.Student.Transcripts.Phone(){
                                                AreaCityCode= "xxx",
                                                PhoneNumber = "yyy"
}
}    
                                    }
                                }
                            }
                          
                        },
                        RequestedStudent = new Dtos.Student.Transcripts.RequestedStudent()
                        {
                            Attendance = new Dtos.Student.Transcripts.Attendance()
                            {
                                AcademicAwardsReported = new List<Dtos.Student.Transcripts.AcademicAwardsReported>() {
                                    new Dtos.Student.Transcripts.AcademicAwardsReported() { AcademicAwardDate = DateTime.Now, AcademicAwardTitle = "X" }
                                }
                            }
                        }
                    },
                    DocumentID = "X",
                    NoteMessage = "X",
                    UserDefinedExtensions = new Dtos.Student.Transcripts.UserDefinedExtensions()
                };
                

                var adapter = (TranscriptRequestEntityAdapter)adapterRegistry.GetAdapter<Dtos.Student.Transcripts.TranscriptRequest, Domain.Student.Entities.Transcripts.TranscriptRequest>();
         
                Domain.Student.Entities.Transcripts.TranscriptRequest target = adapter.MapToType(source);

                Assert.IsNotNull(source.TransmissionData.DocumentID);
                
                Assert.AreEqual(source.TransmissionData.DocumentID, target.TransmissionData.DocumentID);
                Assert.AreEqual(source.Request.RequestedStudent.Attendance.AcademicAwardsReported.First().AcademicAwardTitle, target.Request.RequestedStudent.Attendance.AcademicAwardsReported.First().AcademicAwardTitle);


                Assert.AreEqual(source.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.First().FaxPhone.AreaCityCode,
                    target.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.First().FaxPhone.AreaCityCode);

                Assert.AreEqual(source.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.First().FaxPhone.PhoneNumber,
                    target.Request.Recipient.Receiver.RequestorReceiverOrganization.Contacts.First().FaxPhone.PhoneNumber);

            }

            public MemoryStream GetTestOrder()
            {
                #region Hardcoded test XML data

                string test = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                                "<ns1:TranscriptRequest xsi:schemaLocation=\"urn:org:pesc:message:TranscriptRequest:v1.2.0 TranscriptRequest_v1.2.0.xsd\" " +
                                "xmlns:n2=\"http://www.altova.com/samplexml/other-namespace\" " +
                                "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " +
                                "xmlns:ns1=\"urn:org:pesc:message:TranscriptRequest:v1.2.0\">" +
                                "  <ns1:TransmissionData>" +
                                "    <DocumentID>102004001</DocumentID>" +
                                "    <CreatedDateTime>2004-12-17T17:30:47-05:00</CreatedDateTime>" +
                                "    <DocumentTypeCode>Request</DocumentTypeCode>" +
                                "    <TransmissionType>Original</TransmissionType>" +
                                "    <Source>" +
                                "      <Organization>" +
                                "        <DUNS>827034414</DUNS>" +
                                "        <OrganizationName>National Student Clearinghouse</OrganizationName>" +
                                "      </Organization>" +
                                "    </Source>" +
                                "    <Destination>" +
                                "      <Organization>" +
                                "        <OPEID>00123400</OPEID>" +
                                "        <OrganizationName>ABC University</OrganizationName>" +
                                "      </Organization>" +
                                "    </Destination>" +
                                "  </ns1:TransmissionData >" +
                                "  <Request>" +
                                "    <CreatedDateTime>2004-12-17T09:30:47-05:00</CreatedDateTime>" +
                                "    <RequestedStudent>" +
                                "      <Person>" +
                                "        <SchoolAssignedPersonID>001234567</SchoolAssignedPersonID>" +
                                "        <SSN>111223333</SSN>" +
                                "        <Birth>" +
                                "          <BirthDate>1967-08-13</BirthDate>" +
                                "        </Birth>" +
                                "        <Name>" +
                                "          <FirstName>Jane</FirstName>" +
                                "          <MiddleName>C</MiddleName>" +
                                "          <LastName>Doe</LastName>" +
                                "        </Name>" +
                                "        <AlternateName>" +
                                "          <FirstName>Jane</FirstName>" +
                                "          <MiddleName>C</MiddleName>" +
                                "          <LastName>Smith</LastName>" +
                                "        </AlternateName>" +
                                "      </Person>" +
                                "      <Attendance>" +
                                "        <School>" +
                                "          <OrganizationName>ABC University</OrganizationName>" +
                                "          <OPEID>00123400</OPEID>" +
                                "        </School>" +
                                "        <EnrollDate>1985-01-01</EnrollDate>" +
                                "        <ExitDate>1989-12-31</ExitDate>" +
                                "        <AcademicAwardsReported>" +
                                "          <AcademicAwardTitle>Bachelor of Science</AcademicAwardTitle>" +
                                "          <AcademicAwardDate>1989-01-01</AcademicAwardDate>" +
                                "        </AcademicAwardsReported>" +
                                "      </Attendance>" +
                                "      <ReleaseAuthorizedIndicator>true</ReleaseAuthorizedIndicator>" +
                                "      <ReleaseAuthorizedMethod>Signature</ReleaseAuthorizedMethod>" +
                                "    </RequestedStudent>" +
                                "    <Recipient>" +
                                "      <Receiver>" +
                                "        <RequestorReceiverOrganization>" +
                                "          <OrganizationName>123 University</OrganizationName>" +
                                "          <OPEID>987654654</OPEID>" +
                                "          <Contacts>" +
                                "            <Address>" +
                                "              <AddressLine>123 College Avenue</AddressLine>" +
                                "              <City>College Park</City>" +
                                "              <StateProvinceCode>MD</StateProvinceCode>" +
                                "              <PostalCode>56241</PostalCode>" +
                                "              <AttentionLine>John Smith</AttentionLine>" +
                                "            </Address>" +
                                "            <Phone>" +
                                "              <PhoneNumber>3015551212</PhoneNumber>" +
                                "            </Phone>" +
                                "            <FaxPhone>" +
                                "              <PhoneNumber>3015551213</PhoneNumber>" +
                                "            </FaxPhone>" +
                                "            <Email>" +
                                "              <EmailAddress>john@123.edu</EmailAddress>" +
                                "            </Email>" +
                                "          </Contacts>" +
                                "        </RequestorReceiverOrganization>" +
                                "      </Receiver>" +
                                "      <TranscriptType>Undergraduate</TranscriptType>" +
                                "      <TranscriptPurpose>Admission</TranscriptPurpose>" +
                                "      <DeliveryMethod>Mail</DeliveryMethod>" +
                                "      <TranscriptCopies>2</TranscriptCopies>" +
                                "      <StampSealEnvelopeIndicator>true</StampSealEnvelopeIndicator>" +
                                "      <SpecialInstructions>Please print on red paper</SpecialInstructions>" +
                                "    </Recipient>" +
                                "  </Request>" +
                                "  <RequestTrackingID>145299-1</RequestTrackingID>" +
                                "  <UserDefinedExtensions>" +
                                "    <ReceivingInstitutionCeebId>002600</ReceivingInstitutionCeebId>" +
                                "    <AttachmentUrl>studentclearinghouse.org/attachments/1234567890/ASDASGWERWEREKASDJAKSJAJSNDUYQWISNSUE/</AttachmentUrl>" +
                                "    <AttachmentSpecialInstructions>Please print on purple paper</AttachmentSpecialInstructions>" +
                                "   <AttachmentFlag>Y</AttachmentFlag>	" +
                                "  </UserDefinedExtensions>" +
                                "</ns1:TranscriptRequest>";

                #endregion

                byte[] byteArray = Encoding.ASCII.GetBytes(test);
                MemoryStream stream = new MemoryStream(byteArray);
                return stream;
            }
        }

        [TestClass]
        public class Register : CurrentUserSetup
        { 
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;

            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Ellucian.Colleague.Dtos.Student.SectionRegistration> sectionRegistrations;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            // Used by Two_Section_Registration_Successful
            private List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage> messagesTwoElementRegRequest;
            private List<Ellucian.Colleague.Dtos.Student.SectionRegistration> sectionRegDtoTwoElementList;


            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                 staffRepoMock = new Mock<IStaffRepository>();
                  staffRepo = staffRepoMock.Object;
                logger = new Mock<ILogger>().Object;

            studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;

                // Mock student repo responses
                var studentId1 = "0000894";
                var student1 = new Domain.Student.Entities.Student(studentId1, "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync(studentId1)).ReturnsAsync(student1);
                var student1Access = student1.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(s => s.Contains(studentId1)))).ReturnsAsync(new List<StudentAccess>() { student1Access }.AsEnumerable());

                var studentId2 = "00004002";
                var student2 = new Domain.Student.Entities.Student(studentId2, "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                student2.AddAdvisement("0000111", new DateTime(2014, 12, 1), null, "major");
                student2.AddAdvisor("0000111");
                studentRepoMock.Setup(repo => repo.GetAsync(studentId2)).ReturnsAsync(student2);
                var student2Access = student2.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(s => s.Contains(studentId2)))).ReturnsAsync(new List<StudentAccess>() { student2Access }.AsEnumerable());

                var studentId3 = "00004003";
                var student3 = new Domain.Student.Entities.Student(studentId3, "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepoMock.Setup(repo => repo.GetAsync(studentId3)).ReturnsAsync(student3);
                var student3Access = student3.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(s => s.Contains(studentId3)))).ReturnsAsync(new List<StudentAccess>() { student3Access }.AsEnumerable());


                // Mock the student repository register method
                var messages = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage>() { new Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage() { Message = "Success", SectionId = "" } };
                var response = new Ellucian.Colleague.Domain.Student.Entities.RegistrationResponse(messages, null);
                studentRepoMock.Setup(x => x.RegisterAsync(It.IsAny<Ellucian.Colleague.Domain.Student.Entities.RegistrationRequest>())).ReturnsAsync(response);

                // Setup a SectionRegistration dto with two entries
                sectionRegDtoTwoElementList = new List<Ellucian.Colleague.Dtos.Student.SectionRegistration>();
                var sectionRegDto = new Ellucian.Colleague.Dtos.Student.SectionRegistration();
                sectionRegDto.Action = Dtos.Student.RegistrationAction.Drop;
                sectionRegDto.Credits = 3.5m;
                sectionRegDto.DropReasonCode = "DropCode1";
                sectionRegDto.SectionId = "SectionID1";
                sectionRegDtoTwoElementList.Add(sectionRegDto);
                var sectionRegDto2 = new Ellucian.Colleague.Dtos.Student.SectionRegistration();
                sectionRegDto2.Action = Dtos.Student.RegistrationAction.Add;
                sectionRegDto2.Credits = 3.0m;
                sectionRegDto2.DropReasonCode = null;
                sectionRegDto2.SectionId = "SectionID2";
                sectionRegDtoTwoElementList.Add(sectionRegDto2);

                // Mock the student repository RegisterAsync when passed a RegistrationRequest object with the exact same data that is contained int the
                // sectionRegDtoTwoElementList defined above. This will test that the RegisterAsync service method correctly translates the dto to the entity.
                messagesTwoElementRegRequest = new List<Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage>()
                    { new Ellucian.Colleague.Domain.Student.Entities.RegistrationMessage() { Message = "Successful Two Element Reg Request", SectionId = "" } };
                var responseTwoElementRegRequest = new Ellucian.Colleague.Domain.Student.Entities.RegistrationResponse(messagesTwoElementRegRequest, null);
                studentRepoMock.Setup(x => x.RegisterAsync(
                       It.Is<Ellucian.Colleague.Domain.Student.Entities.RegistrationRequest>(
                           r => (r.Sections.Count == sectionRegDtoTwoElementList.Count) && (r.Sections[0].SectionId == sectionRegDtoTwoElementList[0].SectionId) &&
                                (r.Sections[0].Action.ToString() == sectionRegDtoTwoElementList[0].Action.ToString()) && 
                                (r.Sections[0].Credits == sectionRegDtoTwoElementList[0].Credits) && 
                                (r.Sections[0].DropReasonCode == sectionRegDtoTwoElementList[0].DropReasonCode) && 
                                (r.Sections[1].SectionId == sectionRegDtoTwoElementList[1].SectionId) &&
                                (r.Sections[1].Action.ToString() == sectionRegDtoTwoElementList[1].Action.ToString()) &&
                                (r.Sections[1].Credits == sectionRegDtoTwoElementList[1].Credits) && 
                                (r.Sections[1].DropReasonCode == sectionRegDtoTwoElementList[1].DropReasonCode)
                                )))
                           .ReturnsAsync(responseTwoElementRegRequest);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Set up sectionRegistrations
                sectionRegistrations = new List<Dtos.Student.SectionRegistration>() { new Dtos.Student.SectionRegistration() { Action = Dtos.Student.RegistrationAction.Add, Credits = null, SectionId = "1111" } };

                studentService = new StudentService(adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                studentService = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullStudentId_ThrowsException()
            {
                await studentService.RegisterAsync(null, sectionRegistrations);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullSectionRegistrations_ThrowsException()
            {
                await studentService.RegisterAsync("0002222", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task ZeroSectionRegistrations_ThrowsException()
            {
                await studentService.RegisterAsync("00004002", new List<Ellucian.Colleague.Dtos.Student.SectionRegistration>());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task PersonWithoutPermission_ThrowsException()
            {
                // In this case we are not going to give 00004002 any permissions to allow registration.
                await studentService.RegisterAsync("00004002", sectionRegistrations);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AssignedAdviseePermission_NotAdvisee_ThrowsException()
            {
                // Add just the AllAccessAssignedAdvisees permission to advisor's role
                // Student 00004003 does not have 0000111 as an advisor so this should still not be allowed.
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);

                // Now try to register for a student who has this advisor as an assigned advisor.
                var registrationResponse = await studentService.RegisterAsync("00004003", sectionRegistrations);
            }

            [TestMethod]
            public async Task Self_RegistrationSuccessful()
            {
                var registrationResponse = await studentService.RegisterAsync("0000894", sectionRegistrations);
                Assert.AreEqual(1, registrationResponse.Messages.Count());
            }

            [TestMethod]
            public async Task Two_Section_Registration_Successful()
            {
                // When passed sectionRegDtoTwoElementList, the mocked student repository RegisterAsync method called by 
                // studentService.RegisterAsync should return messagesTwoElementRegRequest.
                var registrationResponse = await studentService.RegisterAsync("0000894", sectionRegDtoTwoElementList);
                Assert.AreEqual(1, registrationResponse.Messages.Count());
                Assert.IsTrue(registrationResponse.Messages[0].Message.Equals(messagesTwoElementRegRequest[0].Message));
            }

            [TestMethod]
            public async Task AssignedAdviseePermission_RegistrationSuccessful()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);

                // Now try to register for a student who has this advisor as an assigned advisor.
                var registrationResponse = await studentService.RegisterAsync("00004002", sectionRegistrations);
                Assert.AreEqual(1, registrationResponse.Messages.Count());
            }

            [TestMethod]
            public async Task AllAdviseePermission_RegistrationSuccessful()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);

                // Now try to register for a student who has this advisor as an assigned advisor.
                var registrationResponse = await studentService.RegisterAsync("00004003", sectionRegistrations);
                Assert.AreEqual(1, registrationResponse.Messages.Count());
            }
        }

        [TestClass]
        public class CheckUserAccess_Advisor : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            private ICurrentUserFactory currentUserFactory;
            private string id1;
            private Domain.Student.Entities.Student student1;
            private string id2;
            private Domain.Student.Entities.Student student2;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // Mock student repo response
                id1 = "0000001";
                student1 = new Domain.Student.Entities.Student(id1, "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync(id1)).ReturnsAsync(student1);
                var student1Access = student1.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(s => s.Contains(id1)))).ReturnsAsync(new List<StudentAccess>() { student1Access }.AsEnumerable());

                id2 = "0000894";
                student2 = new Domain.Student.Entities.Student(id2, "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student2.AddAdvisor("0000896");
                student2.AddAdvisement("0000896", null, null, null);
                student2.AddAdvisor("0000111");
                student2.AddAdvisement("0000111", null, null, null);
                studentRepoMock.Setup(repo => repo.GetAsync(id2)).ReturnsAsync(student2);
                var student2Access = student2.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(s => s.Contains(id2)))).ReturnsAsync(new List<StudentAccess>() { student2Access }.AsEnumerable());

                // Mock adapter
                var studentDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>()).Returns(studentDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task CheckUserAccess_ViewStudentInformation_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() {advisorRole});

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id, student1.ConvertToStudentAccess());
            }

            [TestMethod]
            public async Task CheckUserAccess_ViewAnyAdvisee_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id, student1.ConvertToStudentAccess());
            }

            [TestMethod]
            public async Task CheckUserAccess_ReviewAnyAdvisee_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_UpdateAnyAdvisee_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_AllAccessAnyAdvisee_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckUserAccess_ViewAssignedAdvisees_UnassignedAdvisee_ThrowsException()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_ViewAssignedAdvisee_AssignedAdvisee_AllowsAccess()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                // Student 2 is assigned to this advisor
                await studentService.CheckUserAccessAsync(student2.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckUserAccess_UpdateAssignedAdvisees_UnassignedAdvisee_ThrowsException()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_UpdateAssignedAdvisee_AssignedAdvisee_AllowsAccess()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                // Student 2 is assigned to this advisor
                await studentService.CheckUserAccessAsync(student2.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckUserAccess_AllAccessAssignedAdvisees_UnassignedAdvisee_ThrowsException()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_AllAccessAssignedAdvisee_AssignedAdvisee_AllowsAccess()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                // Student 2 is assigned to this advisor
                await studentService.CheckUserAccessAsync(student2.Id);
            }
        }

        [TestClass]
        public class CheckEmailAddresses_AsStudentUser : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;


            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;

                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;


                // Mock student repo response
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock Adapters
                var studentDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.Student, Ellucian.Colleague.Dtos.Student.Student>()).Returns(studentDtoAdapter);

                var emailDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Ellucian.Colleague.Dtos.Base.EmailAddress>()).Returns(emailDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }


            [TestMethod]
            public async Task DtoHasEmailAddresses()
            {
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student1.AddEmailAddress(new EmailAddress("aaa@a.com", "COL"));
                student1.AddEmailAddress(new EmailAddress("bbb@a.com", "PRI"));
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var studentDtoWrapper=await studentService.GetAsync("0000894");
                Assert.IsNotNull(studentDtoWrapper.Dto.EmailAddresses);
                Assert.AreEqual(studentDtoWrapper.Dto.EmailAddresses.Count(), student1.EmailAddresses.Count());
            }

            [TestMethod]
            public async Task DtoHasEmptyEmailAddresses()
            {
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                var studentDtoWrapper = await studentService.GetAsync("0000894");
                Assert.IsNotNull(studentDtoWrapper.Dto.EmailAddresses);
                Assert.AreEqual(studentDtoWrapper.Dto.EmailAddresses.Count(), 0);
                Assert.AreEqual(student1.EmailAddresses.Count(), student1.EmailAddresses.Count());
            }
        }

        [TestClass]
        public class GetUnofficialTranscriptAsync : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;


            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;


                // Mock student repo response
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                // Mock student when request is from someone else 
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Set up student 0000894 as the current user.
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Mock successful Get of a transcript
                string transcriptText = "Ellucian University Transcript";
                studentRepoMock.Setup(repo => repo.GetTranscriptAsync("0000894", "UG")).ReturnsAsync(transcriptText);

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentService = null;
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetUnofficialTranscript_ThrowsErrorIfNotSelfOrAdvisorWithPermissions()
            {
                 var result = await studentService.GetUnofficialTranscriptAsync("00004002", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetUnofficialTranscript_UserHasViewStudentInformationPermission()
            {
                // Set up student 0000894 as the current user.
                //Mock roles repo and permission
                var permissionViewAnyStudent = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation);
                viewStudentRole.AddPermission(permissionViewAnyStudent);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewStudentRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0004002")).ReturnsAsync(student2);
                var result = await studentService.GetUnofficialTranscriptAsync("0004002", "rdlc path", "UG", "water mark path", "device info path");
            }

            //[TestMethod]
            //public async Task GetUnofficialTranscript_UserIsSelf()
            //{
            //    // Set up student 0000894 as the current user.
                  // ToDo: figure out how to mock the Local Processing for these pdfs.
            //    var result = await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "UG", "water mark path", "device info path");
            //}


            //[TestMethod]
            //public async Task GetUnofficialTranscript_ViewAnyAdviseePermitted()
            //{
            //    // Set up needed permission
            //    // Set up student 0000894 as the current user.
            //    var currentAdvisorUserFactory = new CurrentUserSetup.AdvisorUserFactory();
            //    studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentAdvisorUserFactory, roleRepo, logger);
            //    advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
            //    roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
            //    studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
            //    var result = await studentService.GetUnofficialTranscriptAsync("00004002", "rdlc path", "UG", "water mark path", "device info path");
            //    //await studentService.GetAsync("0000894");

                
            //}
        }

        [TestClass]
        public class StudentCohort_GET : CurrentUserSetup
        {
            Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IPersonRepository> personRepoMock;        
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;         
            Mock<IAcademicHistoryService> acadHistServiceMock;
            Mock<ITermRepository> termRepositoryMock;
            Mock<IRegistrationPriorityRepository> priorityRepositoryMock;
            Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private ILogger logger;

            StudentService studentService;
            List<StudentCohort> studentCohortEntities = new List<StudentCohort>();
            List<Dtos.StudentCohort> studentCohortDtos = new List<Dtos.StudentCohort>();

            [TestInitialize]
            public void Initialize()
            {
                studentRepositoryMock = new Mock<IStudentRepository>();
                personRepoMock = new Mock<IPersonRepository>();
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();

                acadHistServiceMock = new Mock<IAcademicHistoryService>();
                termRepositoryMock = new Mock<ITermRepository>();
                priorityRepositoryMock = new Mock<IRegistrationPriorityRepository>();
                studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                BuildData();

                studentService = new StudentService(adapterRegistryMock.Object, studentRepositoryMock.Object, personRepoMock.Object, acadCreditRepoMock.Object, acadHistServiceMock.Object, termRepositoryMock.Object, priorityRepositoryMock.Object,
                                                    studentConfigurationRepositoryMock.Object, referenceDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepoMock.Object, staffRepo, logger);
            }           

            [TestCleanup]
            public void Cleanup()
            {
                studentRepositoryMock = null;
                acadHistServiceMock = null;
                termRepositoryMock = null;
                priorityRepositoryMock = null;
                studentConfigurationRepositoryMock = null;
                studentCohortEntities = null;
                roleRepoMock = null;
                studentCohortDtos = null;
            }

            [TestMethod]
            public async Task StudentCohort_GetAllStudentCohorts()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAllStudentCohortAsync(It.IsAny<bool>())).ReturnsAsync(studentCohortEntities);
                var actuals = await studentService.GetAllStudentCohortsAsync(It.IsAny<bool>());

                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentCohortEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                }
            }

            [TestMethod]
            public async Task StudentCohort_GetStudentCohortById()
            {
                string id = "f05a6c0f-3a56-4a87-b931-bc2901da5ef9";
                studentReferenceDataRepositoryMock.Setup(i => i.GetAllStudentCohortAsync(It.IsAny<bool>())).ReturnsAsync(studentCohortEntities);

                var actual = await studentService.GetStudentCohortByGuidAsync(id);

                Assert.IsNotNull(actual);
                var expected = studentCohortEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
                Assert.AreEqual(expected.Description, actual.Description);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentCohort_GetStudentCohortById_KeyNotFoundException()
            {
                string id = "badId";
                studentReferenceDataRepositoryMock.Setup(i => i.GetAllStudentCohortAsync(It.IsAny<bool>())).ReturnsAsync(studentCohortEntities);

                var actual = await studentService.GetStudentCohortByGuidAsync(id);
            }

            private void BuildData()
            {
                studentCohortEntities = new List<StudentCohortEntity>() 
                {
                    new StudentCohortEntity("e8dbcea5-ffb8-471e-87b7-ce5d36d5c2e7", "ATHL", "Athletes"),
                    new StudentCohortEntity("c2f57ee5-1c30-44a5-9d18-311f71f7b722", "FRAT", "Fraternity"),
                    new StudentCohortEntity("f05a6c0f-3a56-4a87-b931-bc2901da5ef9", "SORO", "Sorority"),
                    new StudentCohortEntity("05872218-f749-4cdc-b4f0-43200cc21335", "ROTC", "ROTC Participants"),
                    new StudentCohortEntity("827fffc4-3dd2-4492-8f51-4134597ec4bf", "VETS", "Military Veterans"),
                };

                studentCohortDtos = new List<Dtos.StudentCohort>() 
                {
                    new Dtos.StudentCohort(){ Id = "e8dbcea5-ffb8-471e-87b7-ce5d36d5c2e7", Code = "ATHL", Description = "Athletes", Title = "Athletes" },
                    new Dtos.StudentCohort(){ Id = "c2f57ee5-1c30-44a5-9d18-311f71f7b722", Code = "FRAT", Description = "Fraternity", Title = "Fraternity" },
                    new Dtos.StudentCohort(){ Id = "f05a6c0f-3a56-4a87-b931-bc2901da5ef9", Code = "SORO", Description = "Sorority", Title = "Sorority" },
                    new Dtos.StudentCohort(){ Id = "05872218-f749-4cdc-b4f0-43200cc21335", Code = "ROTC", Description = "ROTC Participants", Title = "ROTC Participants" },
                    new Dtos.StudentCohort(){ Id = "827fffc4-3dd2-4492-8f51-4134597ec4bf", Code = "VETS", Description = "Military Veterans", Title = "Military Veterans" }
                };
            }
        }

        [TestClass]
        public class StudentClassification_GET : CurrentUserSetup
        {
            Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IPersonRepository> personRepoMock;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;  
            Mock<IAcademicHistoryService> acadHistServiceMock;
            Mock<ITermRepository> termRepositoryMock;
            Mock<IRegistrationPriorityRepository> priorityRepositoryMock;
            Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;


            private Mock<IRoleRepository> roleRepoMock;
            private ILogger logger;

            StudentService studentService;
            List<StudentClassificationEntity> studentClassiicationEntities = new List<StudentClassificationEntity>();
            List<Dtos.StudentClassification> studentClassificationDtos = new List<Dtos.StudentClassification>();

            [TestInitialize]
            public void Initialize()
            {
                studentRepositoryMock = new Mock<IStudentRepository>();
                personRepoMock = new Mock<IPersonRepository>();
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadHistServiceMock = new Mock<IAcademicHistoryService>();
                termRepositoryMock = new Mock<ITermRepository>();
                priorityRepositoryMock = new Mock<IRegistrationPriorityRepository>();
                studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                roleRepoMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;


                BuildData();

                studentService = new StudentService(adapterRegistryMock.Object, studentRepositoryMock.Object, personRepoMock.Object, acadCreditRepoMock.Object, acadHistServiceMock.Object, termRepositoryMock.Object, priorityRepositoryMock.Object,
                                                   studentConfigurationRepositoryMock.Object, referenceDataRepositoryMock.Object, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepoMock.Object, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepositoryMock = null;
                acadHistServiceMock = null;
                termRepositoryMock = null;
                priorityRepositoryMock = null;
                studentConfigurationRepositoryMock = null;
                studentClassiicationEntities = null;
                roleRepoMock = null;
                studentClassificationDtos = null;
            }

            [TestMethod]
            public async Task StudentClassification_GetAllStudentClassifications()
            {
                studentReferenceDataRepositoryMock.Setup(i => i.GetAllStudentClassificationAsync(It.IsAny<bool>())).ReturnsAsync(studentClassiicationEntities);
                var actuals = await studentService.GetAllStudentClassificationsAsync(It.IsAny<bool>());

                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentClassiicationEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.Code, actual.Code);
                }
            }

            [TestMethod]
            public async Task StudentClassification_GetStudentClassificationById()
            {
                string id = "7b8c4ba7-ea28-4604-bca7-da7223f6e2b3";
                studentReferenceDataRepositoryMock.Setup(i => i.GetAllStudentClassificationAsync(It.IsAny<bool>())).ReturnsAsync(studentClassiicationEntities);

                var actual = await studentService.GetStudentClassificationByGuidAsync(id);

                Assert.IsNotNull(actual);
                var expected = studentClassiicationEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                Assert.IsNotNull(expected);

                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual(expected.Code, actual.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentClassification_GetStudentClassificationById_KeyNotFoundException()
            {
                string id = "badId";
                studentReferenceDataRepositoryMock.Setup(i => i.GetAllStudentClassificationAsync(It.IsAny<bool>())).ReturnsAsync(studentClassiicationEntities);

                var actual = await studentService.GetStudentClassificationByGuidAsync(id);
            }

            private void BuildData()
            {
                studentClassiicationEntities = new List<StudentClassificationEntity>() 
                {
                    new StudentClassificationEntity("3b8f02a3-d349-46b5-a0df-710121fa1f64", "1G", "First Year Graduate"),
                    new StudentClassificationEntity("7b8c4ba7-ea28-4604-bca7-da7223f6e2b3", "1L", "First Year Law"),
                    new StudentClassificationEntity("bd98c3ed-6adb-4c7c-bc80-7507ea868a23", "2A", "Second Year"),
                    new StudentClassificationEntity("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31", "2G", "Second Year Graduate"),
                    new StudentClassificationEntity("7e990bda-9427-4de6-b0ef-bba9b015e399", "2L", "Second Year Law"),
                };

                studentClassificationDtos = new List<Dtos.StudentClassification>() 
                {
                    new Dtos.StudentClassification(){ Id = "3b8f02a3-d349-46b5-a0df-710121fa1f64", Code = "1G", Description = "First Year Graduate", Title = "First Year Graduate" },
                    new Dtos.StudentClassification(){ Id = "7b8c4ba7-ea28-4604-bca7-da7223f6e2b3", Code = "1L", Description = "First Year Law", Title = "First Year Law" },
                    new Dtos.StudentClassification(){ Id = "bd98c3ed-6adb-4c7c-bc80-7507ea868a23", Code = "2A", Description = "Second Year", Title = "Second Year" },
                    new Dtos.StudentClassification(){ Id = "6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31", Code = "2G", Description = "Second Year Graduate", Title = "Second Year Graduate" },
                    new Dtos.StudentClassification(){ Id = "7e990bda-9427-4de6-b0ef-bba9b015e399", Code = "2L", Description = "Second Year Law", Title = "Second Year Law" }
                };
            }
        }

        [TestClass]
        public class GetResidentTypes
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepoMock;
            private IStudentReferenceDataRepository studentReferenceDataRepository;
            private Mock<IReferenceDataRepository> referenceDataRepoMock;
            private IReferenceDataRepository referenceDataRepository;

            private IAcademicHistoryService academicHistoryService;
            private Mock<IAcademicHistoryService> academicHistoryServiceMock;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;


            IEnumerable<Ellucian.Colleague.Domain.Student.Entities.ResidencyStatus> residencyStatuses;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;


                residencyStatuses = new TestStudentRepository().GetResidencyStatusesAsync(false).Result;

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task StudentService__GetAllAsync()
            {
                studentRepoMock.Setup(i => i.GetResidencyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(residencyStatuses);

                var results = await studentService.GetResidentTypesAsync(It.IsAny<bool>());
                Assert.AreEqual(residencyStatuses.ToList().Count, (results.Count()));

                foreach (var residentType in residencyStatuses)
                {
                    var result = results.FirstOrDefault(i => i.Id == residentType.Guid);

                    Assert.AreEqual(residentType.Code, result.Code);
                    Assert.AreEqual(residentType.Description, result.Title);
                    Assert.AreEqual(residentType.Guid, result.Id);
                }
            }

            [TestMethod]
            public async Task StudentService__GetByIdAsync()
            {
                studentRepoMock.Setup(i => i.GetResidencyStatusesAsync(It.IsAny<bool>())).ReturnsAsync(residencyStatuses);

                string id = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
                var residentType = residencyStatuses.FirstOrDefault(i => i.Guid == id);

                var result = await studentService.GetResidentTypeByIdAsync(id);

                Assert.AreEqual(residentType.Code, result.Code);
                Assert.AreEqual(residentType.Description, result.Title);
                Assert.AreEqual(residentType.Guid, result.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentService__GetByIdAsync_KeyNotFoundException()
            {
                studentRepoMock.Setup(i => i.GetResidencyStatusesAsync(true)).ReturnsAsync(residencyStatuses);
                var result = await studentService.GetResidentTypeByIdAsync("123");
            }
        }
    }
}
