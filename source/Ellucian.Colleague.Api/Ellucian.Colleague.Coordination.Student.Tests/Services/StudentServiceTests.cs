// Copyright 2012-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Entities.Transcripts;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StudentClassificationEntity = Ellucian.Colleague.Domain.Student.Entities.StudentClassification;
using StudentCohortEntity = Ellucian.Colleague.Domain.Student.Entities.StudentCohort;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentServiceTests
    {
        // Sets up a Current user that is a student and one that is an advisor
        public abstract class CurrentUserSetup
        {

            protected Role advisorRole = new Role(105, "Advisor");
            protected Role advisorRole1 = new Role(106, "Advisor1");
            protected Role viewStudentRole = new Role(1, "VIEW.STUDENT.INFORMATION");
            protected Role viewAnyAdviseeRole = new Role(18, "Advisor");
            protected Role crossRegUserRole = new Role(111, "CrossReg");


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

            // A user with permissions to run the validation-only and skip-validation APIs in support of cross-reg
            public class RegisterValidationOnlySkipValidationUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "RegisterCrossReg",
                            PersonId = "RegisterCrossReg",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "RegisterCrossReg",
                            Roles = new List<string>() { "CrossReg" },
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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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
            private IEnumerable<AdmissionResidencyType> residencyTypes;
            private IEnumerable<AcademicLevel> academicLevels;
            private IEnumerable<StudentClassification> studentClassifications;
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

            private string academicLevel1Guid = "0dbf5ac4-b25d-46e2-97a9-8353edd63b63";
            private string academicLevel1Code = "UG";
            private string academicLevel2Guid = "d0543c34-d3e1-45a9-a67d-b25ad3e0a4683";
            private string academicLevel2Code = "GR";
            private string studentClassification1Guid = "531c5ce4-580c-4868-91fb-24d5080566c2";
            private string studentClassification1Code = "FR";
            private string studentClassification2Guid = "d97c6abc-e6fc-40aa-bf91-84bd1d30090c";
            private string studentClassification2Code = "SO";

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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

                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

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
                permissionViewAnyStudent = new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation);
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
                residencyTypes = new List<AdmissionResidencyType>()
                {
                    new AdmissionResidencyType(residency1Guid, residency1Code, "Test Data1"),
                    new AdmissionResidencyType(residency2Guid, residency2Guid, "Test Data2")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyTypes);

                // Mock the repo call for academic levels
                academicLevels = new List<AcademicLevel>()
                {
                    new AcademicLevel(academicLevel1Guid, academicLevel1Code, "Undergraduate"),
                    new AcademicLevel(academicLevel2Guid, academicLevel2Code, "Graduate")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevels);

                // Mock the repo call for student classifications
                studentClassifications = new List<StudentClassification>()
                {
                    new StudentClassification(studentClassification1Guid, studentClassification1Code, "Freshman"),
                    new StudentClassification(studentClassification2Guid, studentClassification2Code, "Sophmore")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevels);

                //mock the call to get the personid
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(student1Guid)).ReturnsAsync(student1Id);

                //setup student entity data
                student1 = new Domain.Student.Entities.Student(student1Guid, student1Id, new List<string>() { program1Guid, program2Guid }, new List<string>() { academicCred1Guid, academicCred2Guid }, "Boyd", false);
                student2 = new Domain.Student.Entities.Student(student2Guid, student2Id, new List<string>() { program1Guid, program2Guid }, new List<string>() { academicCred1Guid, academicCred2Guid }, "Boyd", false);


                studentList = new List<Domain.Student.Entities.Student>() { student1, student2 };

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
                    referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepository, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
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
                var studentDtoList = await studentService.GetStudentsAsync(0, 100, false, null, null, null, residency1Guid);

                Assert.AreEqual(2, studentDtoList.Item1.Count());
            }
        }

        [TestClass]
        public class GetEedmStudentTests2 : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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

            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;

            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private IEnumerable<Domain.Student.Entities.Student> studentList;
            private IEnumerable<Domain.Student.Entities.Student> oneStudentList;
            private IEnumerable<Domain.Student.Entities.Student> twoStudentList;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepositoryMock;
            private IStaffRepository staffRepository;
            private ICurrentUserFactory currentUserFactory;


            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IEnumerable<StudentCohort> cohorts;
            private IEnumerable<StudentType> studentTypes;
            private IEnumerable<AdmissionResidencyType> residencyTypes;
            private Permission permissionViewAnyStudent;

            private string student1Guid = "6b227dcc-db1c-41a2-b809-8e400e5d0682";
            private string student2Guid = "b88342ca-03d3-4255-9d69-3dfd434c60ff";
            private string student1Id = "1234567";
            private string student2Id = "7654321";
            private string program1Guid = "cbac5aee-71e9-4f2d-ab44-3266d43390d4";
            private string program2Guid = "1f5d03d9-e3cb-43be-8ec9-dc606f5cf90f";
            private string academicCred1Guid = "911f1522-3fee-409e-a782-535f588a3419";
            private string academicCred2Guid = "4f2ead3b-210d-435c-a2c3-624e2683dbef";
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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;


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
                permissionViewAnyStudent = new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation);
                viewStudentRole.AddPermission(permissionViewAnyStudent);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewStudentRole });

                // Mock the repo call for Student types
                studentTypes = new List<StudentType>()
                {
                    new StudentType(studentType1Guid, studentType1Code, "Test Data"),
                    new StudentType(studentType2Guid, studentType2Code, "Test Data")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetStudentTypesAsync(It.IsAny<bool>())).ReturnsAsync(studentTypes);

                // Mock the repo call for residency types
                residencyTypes = new List<AdmissionResidencyType>()
                {
                    new AdmissionResidencyType(residency1Guid, residency1Code, "Test Data1"),
                    new AdmissionResidencyType(residency2Guid, residency2Guid, "Test Data2")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyTypes);

                //mock the call to get the personid
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(student1Guid)).ReturnsAsync(student1Id);

                //setup student entity data
                var studentResidency1 = new StudentResidency(residency1Code, new DateTime(2012, 05, 01));
                var studentResidencies1 = new List<StudentResidency> { studentResidency1 };
                student1 = new Domain.Student.Entities.Student(student1Guid, student1Id, new List<string>() { program1Guid, program2Guid }, new List<string>() { academicCred1Guid, academicCred2Guid }, "Boyd", false) { StudentResidencies = studentResidencies1 };

                var studentTypeInfo2 = new StudentTypeInfo(studentType1Code, new DateTime(2013, 06, 01));
                var studentTypeInfos2 = new List<StudentTypeInfo> { studentTypeInfo2 };
                student2 = new Domain.Student.Entities.Student(student2Guid, student2Id, new List<string>() { program1Guid, program2Guid }, new List<string>() { academicCred1Guid, academicCred2Guid }, "Boyd", false) { StudentTypeInfo = studentTypeInfos2 };


                studentList = new List<Domain.Student.Entities.Student>() { student1, student2 };

                oneStudentList = new List<Domain.Student.Entities.Student>() { student1 };

                twoStudentList = new List<Domain.Student.Entities.Student>() { student2 };


                // Mock student repo response

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string[]>(), "",
                        It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(studentList, 2));

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string[]>(), student1Id,
                        It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(oneStudentList, 1));


                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string[]>(), "",
                        new List<string>() { studentType2Code }, It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(twoStudentList, 2));

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string[]>(), "",
                        It.IsAny<List<string>>(), new List<string>() { residency1Code })).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(oneStudentList, 2));

                // Set up current user
                currentUserFactory = new CurrentUserSetup.EthosUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo,
                    academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo,
                    referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepository, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task GetStudents_NoFilters_V16()
            {
                //var students2Dto = new Dtos.Students2();
                var studentDtoList = await studentService.GetStudents2Async(0, 100, null, null, false);

                Assert.AreEqual(2, studentDtoList.Item1.Count());
            }

            [TestMethod]
            public async Task GetStudents_PersonFilter_V16()
            {
                var criteriaStudentFilter = new Dtos.Students2()
                {
                    Person = new Dtos.GuidObject2(student1Guid)
                };
                var studentDtoList = await studentService.GetStudents2Async(0, 100, criteriaStudentFilter, null, false);

                Assert.AreEqual(studentDtoList.Item2, studentDtoList.Item1.Count());
                Assert.AreEqual(student1Guid, studentDtoList.Item1.ToList()[0].Id);
            }

            [TestMethod]
            public async Task GetStudents_StudentTypeFilter_V16()
            {
                var criteriaType = new Dtos.GuidObject2(studentType2Guid);
                var criteriaTypes = new Dtos.StudentTypesDtoProperty() { Type = criteriaType, StartOn = new DateTime(2013, 06, 01) };
                var criteriaTypesCollection = new List<Dtos.StudentTypesDtoProperty>() { criteriaTypes };
                var criteriaStudentTypeFilter = new Dtos.Students2()
                {
                    Types = criteriaTypesCollection
                };
                var studentDtoList = await studentService.GetStudents2Async(0, 100, criteriaStudentTypeFilter, null, false);

                Assert.AreEqual(1, studentDtoList.Item1.Count());
                Assert.AreEqual(student2Guid, studentDtoList.Item1.ToList()[0].Id);
            }

            [TestMethod]
            public async Task GetStudents_StudentResidencyFilter_V16()
            {
                var criteriaResidency = new Dtos.GuidObject2(residency1Guid);
                var criteriaResidencies = new Dtos.StudentResidenciesDtoProperty() { Residency = criteriaResidency, StartOn = new DateTime(2013, 06, 01) };
                var criteriaResidenciesCollection = new List<Dtos.StudentResidenciesDtoProperty>() { criteriaResidencies };
                var criteriaStudentResidenciesFilter = new Dtos.Students2()
                {
                    Residencies = criteriaResidenciesCollection
                };
                var studentDtoList = await studentService.GetStudents2Async(0, 100, criteriaStudentResidenciesFilter, null, false);

                Assert.AreEqual(1, studentDtoList.Item1.Count());
                Assert.AreEqual(student1Guid, studentDtoList.Item1.ToList()[0].Id);
            }
        }

        [TestClass]
        public class Get_AsStudentUser : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;

            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

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
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely", PersonalPronounCode = "XHE" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock Adapters
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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
                studentWithPrivacyCode = new Domain.Student.Entities.Student("00004003", "Jones", 803, new List<string>() { "BA.MATH" }, new List<string>() { "1", "2" }, "A");
                studentRepoMock.Setup(repo => repo.GetAsync("00004003")).ReturnsAsync(studentWithPrivacyCode);

                studentWithPrivacyCode = new Domain.Student.Entities.Student("00004004", "Jones", 803, new List<string>() { "BA.MATH" }, new List<string>() { "1", "2" }, "B");
                studentRepoMock.Setup(repo => repo.GetAsync("00004004")).ReturnsAsync(studentWithPrivacyCode);
                //staff record for the advisor
                staff = new Domain.Base.Entities.Staff("0000111", "staff member");
                staff.IsActive = true;
                staff.PrivacyCodes = new List<string>() { "A" };
                staffRepoMock.Setup(r => r.Get("0000111")).Returns(staff);

                // Mock Adapters
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);
                var advisementDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Advisement, Dtos.Student.Advisement>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Advisement, Dtos.Student.Advisement>()).Returns(advisementDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService,
                    termRepo, regPriorityRepo, studentConfigurationRepo, referenceDataRepositoryRepo,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory,
                    roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasViewAnyAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasReviewAnyAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasUpdateAnyAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            public async Task AllowsAccessIfAdvisorHasAllAccessAnyAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AllowsAccessIfUserHasViewStudentInformationPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                await studentService.GetAsync("0000894");
            }
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DoNotAllowsAccess_IfUserHasViewStudentInformationPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                await studentService.GetAsync("0000894");
            }
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task DoNotAllowsAccess_IfUserHasViewPersonInformationPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                await studentService.GetAsync("0000894");
            }
            [TestMethod]
            public async Task AllowsAccess_IfUserHasViewPersonInformation_ViewStudentInformation_Permissions()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation));
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation));
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });
                var studentDtoWithWrapper = await studentService.GetAsync("00004003");
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
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation));
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));

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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
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
            private List<Domain.Student.Entities.RegistrationMessage> messages1;

            private string id2 = "0000894";
            private List<Domain.Student.Entities.RegistrationMessage> messages2;

            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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

                messages1 = new List<Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true));
                messages2 = new List<Domain.Student.Entities.RegistrationMessage>() { new Domain.Student.Entities.RegistrationMessage() { Message = "Failed Registration Eligibility" } };
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id2)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages2, false));

                // Mock Adapters
                var regEligDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>()).Returns(regEligDtoAdapter);
                var regMessageDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationMessage, Dtos.Student.RegistrationMessage>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.RegistrationMessage, Dtos.Student.RegistrationMessage>()).Returns(regMessageDtoAdapter);
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckRegistrationEligibility3_ViewStudentInformationPermission()
            {
                // A user with VIEW.STUDENT.INFORMATION - such as a faculty member - is not enough to see a student's registration eligibility
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                var result = await studentService.CheckRegistrationEligibility3Async(id1);
                Assert.AreEqual(messages1.Count(), result.Messages.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility3_AdvisorViewAnyAdviseePermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await studentService.CheckRegistrationEligibility3Async(id2);
                Assert.AreEqual(messages2.Count(), result.Messages.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility3_ViewAssignedAdviseesPermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await studentService.CheckRegistrationEligibility3Async(id2);
                Assert.AreEqual(messages2.Count(), result.Messages.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckRegistrationEligibility3_NoAccess_UserHasNoPermission()
            {
                var messages = await studentService.CheckRegistrationEligibility3Async(id2);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckRegistrationEligibility2_ViewStudentInformationPermission()
            {
                // A user with VIEW.STUDENT.INFORMATION - such as a faculty member - is not enough to see a student's registration eligibility
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(messages1.Count(), result.Messages.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility2_AdvisorViewAnyAdviseePermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await studentService.CheckRegistrationEligibility2Async(id2);
                Assert.AreEqual(messages2.Count(), result.Messages.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility2_ViewAssignedAdviseesPermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await studentService.CheckRegistrationEligibility2Async(id2);
                Assert.AreEqual(messages2.Count(), result.Messages.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility_ViewStudentInformationPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                var result = await studentService.CheckRegistrationEligibilityAsync(id1);
                Assert.AreEqual(messages1.Count(), result.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility_ViewAnyAdviseePermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                var result = await studentService.CheckRegistrationEligibilityAsync(id2);
                Assert.AreEqual(messages2.Count(), result.Count());
            }

            [TestMethod]
            public async Task CheckRegistrationEligibility_ViewAssignedAdviseesPermission()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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
            private List<Domain.Student.Entities.RegistrationMessage> messages1;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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
                List<Domain.Student.Entities.Term> allTerms = new List<Domain.Student.Entities.Term>();
                Domain.Student.Entities.Term term3 = new Domain.Student.Entities.Term("term3", "Term 3", DateTime.Now, DateTime.Now.AddDays(180), 2014, 1, true, true, "term3", true);
                allTerms.Add(term3);
                Domain.Student.Entities.Term term4 = new Domain.Student.Entities.Term("term4", "Term 4", DateTime.Now, DateTime.Now.AddDays(180), 2014, 1, true, true, "term4", false);
                allTerms.Add(term4);
                termRepoMock.Setup(trepo => trepo.GetAsync()).ReturnsAsync(allTerms);

                // Mock Adapters
                var regEligibilityDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>()).Returns(regEligibilityDtoAdapter);

                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);

                var registrationEligibilityDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.RegistrationEligibility, Dtos.Student.RegistrationEligibility>()).Returns(registrationEligibilityDtoAdapter);

                var registrationEligibilityTermDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.RegistrationEligibilityTerm, Dtos.Student.RegistrationEligibilityTerm>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.RegistrationEligibilityTerm, Dtos.Student.RegistrationEligibilityTerm>()).Returns(registrationEligibilityTermDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task CheckRegistrationEligibilty_Original_ReturnsMessages()
            {
                messages1 = new List<Domain.Student.Entities.RegistrationMessage>();
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
                messages1 = new List<Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(new List<RegistrationPriority>().AsEnumerable());
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(messages1.Count(), result.Messages.Count());
                Assert.AreEqual(0, result.Terms.Count());
            }

            [TestMethod]
            public async Task Eligible2_IsEligible()
            {
                messages1 = new List<Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                Assert.AreEqual(true, result.IsEligible);
            }

            [TestMethod]
            public async Task Eligible2_HasOverride()
            {
                messages1 = new List<Domain.Student.Entities.RegistrationMessage>();
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
                Assert.AreEqual(Dtos.Student.RegistrationEligibilityTermStatus.NotEligible, t3.Status);
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
                Assert.AreEqual(Dtos.Student.RegistrationEligibilityTermStatus.Future, t3.Status);
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
                Assert.AreEqual(Dtos.Student.RegistrationEligibilityTermStatus.Past, t3.Status);
                Assert.IsTrue(t3.FailedRegistrationPriorities);
            }

            [TestMethod]
            public async Task CheckEligibility2_FailsTermRules()
            {
                // Take Action
                var result = await studentService.CheckRegistrationEligibility2Async(id1);
                var t1 = result.Terms.Where(t => t.TermCode == "term1").FirstOrDefault();
                var t2 = result.Terms.Where(t => t.TermCode == "term2").FirstOrDefault();
                Assert.IsTrue(t1.FailedRegistrationTermRules);
                Assert.IsFalse(t2.FailedRegistrationTermRules);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Eligible3_NullStudentId_ThrowsException()
            {
                var result = await studentService.CheckRegistrationEligibility3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Eligible3_EmptyStudentId_ThrowsException()
            {
                var result = await studentService.CheckRegistrationEligibility3Async("");
            }

            [TestMethod]
            public async Task Eligible3_NoPriorities_NoTerms_Success()
            {
                messages1 = new List<Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(new List<RegistrationPriority>().AsEnumerable());
                var result = await studentService.CheckRegistrationEligibility3Async(id1);
                Assert.AreEqual(messages1.Count(), result.Messages.Count());
                Assert.AreEqual(0, result.Terms.Count());
            }

            [TestMethod]
            public async Task Eligible3_IsEligible()
            {
                messages1 = new List<Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                var result = await studentService.CheckRegistrationEligibility3Async(id1);
                Assert.AreEqual(true, result.IsEligible);
            }

            [TestMethod]
            public async Task Eligible3_HasOverride()
            {
                messages1 = new List<Domain.Student.Entities.RegistrationMessage>();
                studentRepoMock.Setup(repo => repo.CheckRegistrationEligibilityAsync(id1)).ReturnsAsync(new Domain.Student.Entities.RegistrationEligibility(messages1, true, true));
                var result = await studentService.CheckRegistrationEligibility3Async(id1);
                Assert.AreEqual(true, result.HasOverride);
            }

            [TestMethod]
            public async Task Eligible3_TermRequiresPriorities_NoPriorities_NotEligible()
            {
                // This student has no priorities but term3 requires one.
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(new List<RegistrationPriority>().AsEnumerable());
                var result = await studentService.CheckRegistrationEligibility3Async(id1);
                Assert.AreEqual(0, result.Messages.Count());
                Assert.AreEqual(6, result.Terms.Count());
                // Term 3 should be open
                var t3 = result.Terms.Where(t => t.TermCode == "term3").FirstOrDefault();
                Assert.AreEqual(Dtos.Student.RegistrationEligibilityTermStatus.NotEligible, t3.Status);
            }

            [TestMethod]
            public async Task Eligible3_WithTerms_WithFuturePriorities()
            {
                // Setup necessary data
                List<RegistrationPriority> priorities = new List<RegistrationPriority>();
                RegistrationPriority priority = new RegistrationPriority("998", "0000894", "term3", DateTime.Today.AddYears(1), DateTime.Today.AddYears(2));
                priorities.Add(priority);
                RegistrationPriority priority2 = new RegistrationPriority("999", "0000894", "term4", new DateTime(2020, 9, 15, 15, 0, 0), new DateTime(2020, 9, 15, 16, 0, 0));
                priorities.Add(priority2);
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(priorities.AsEnumerable());

                // Take Action
                var result = await studentService.CheckRegistrationEligibility3Async(id1);
                Assert.AreEqual(6, result.Terms.Count());
                // Term 3 should be future now instead of not eligible.
                var t3 = result.Terms.Where(t => t.TermCode == "term3").FirstOrDefault();
                Assert.AreEqual(Dtos.Student.RegistrationEligibilityTermStatus.Future, t3.Status);
                Assert.IsTrue(t3.FailedRegistrationPriorities);
            }

            [TestMethod]
            public async Task Eligible3_WithTerms_WithPastPriorities()
            {
                // Setup necessary data
                List<RegistrationPriority> priorities = new List<RegistrationPriority>();
                RegistrationPriority priority = new RegistrationPriority("998", "0000894", "term3", new DateTime(2010, 9, 15, 15, 0, 0), new DateTime(2010, 9, 15, 16, 0, 0));
                priorities.Add(priority);
                RegistrationPriority priority2 = new RegistrationPriority("999", "0000894", "term4", new DateTime(2010, 9, 15, 15, 0, 0), new DateTime(2010, 9, 15, 16, 0, 0));
                priorities.Add(priority2);
                regPriorityRepoMock.Setup(rrepo => rrepo.GetAsync(student1.Id)).ReturnsAsync(priorities.AsEnumerable());

                // Take Action
                var result = await studentService.CheckRegistrationEligibility3Async(id1);
                Assert.AreEqual(6, result.Terms.Count());
                // Term 3 should be future now instead of not eligible.
                var t3 = result.Terms.Where(t => t.TermCode == "term3").FirstOrDefault();
                Assert.AreEqual(Dtos.Student.RegistrationEligibilityTermStatus.Past, t3.Status);
                Assert.IsTrue(t3.FailedRegistrationPriorities);
            }

            [TestMethod]
            public async Task CheckEligibility3_FailsTermRules()
            {
                // Take Action
                var result = await studentService.CheckRegistrationEligibility3Async(id1);
                var t1 = result.Terms.Where(t => t.TermCode == "term1").FirstOrDefault();
                var t2 = result.Terms.Where(t => t.TermCode == "term2").FirstOrDefault();
                Assert.IsTrue(t1.FailedRegistrationTermRules);
                Assert.IsFalse(t2.FailedRegistrationTermRules);
            }
        }

        #region GetRegistrationPrioritiesAsync
        [TestClass]
        public class GetRegistrationPrioritiesAsync_Student : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;

            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
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

            private IEnumerable<RegistrationPriority> registrationPriorities;

            [TestInitialize]
            public async void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;

                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;

                // Mock registration repository response
                registrationPriorities = new List<RegistrationPriority>()
                { 
                    new RegistrationPriority(id: "1", studentId: "0000894", termCode: "2019/FA", start: new DateTimeOffset(new DateTime(2019,05,12, 07,00,0)), end: new DateTimeOffset(new DateTime(2019,05,12, 17,00,0))),
                    new RegistrationPriority(id: "2", studentId: "0000894", termCode: null,      start: new DateTimeOffset(new DateTime(2018,04,01, 00,00,0)), end: new DateTimeOffset(new DateTime(2018,05,12, 20,00,0))),
                    new RegistrationPriority(id: "3", studentId: "0000894", termCode: "2022/FA", start: null,                                                  end: new DateTimeOffset(new DateTime(2019,05,12, 07,19,0))),
                    new RegistrationPriority(id: "10",studentId: "0000894", termCode: "2023/SP", start: new DateTimeOffset(new DateTime(2022,11,05, 06,00,0)), end: new DateTimeOffset(new DateTime(2022,11,06, 21,00,0))),
                    new RegistrationPriority(id: "13",studentId: "0000894", termCode: "2023/FA", start: new DateTimeOffset(new DateTime(2023,05,20, 08,00,0)), end: null),
                    new RegistrationPriority(id: "20",studentId: "0000894", termCode: "2023/SU", start: new DateTimeOffset(new DateTime(2023,03,02, 05,00,0)), end: new DateTimeOffset(new DateTime(2023,04,15, 19,00,0))),
                    new RegistrationPriority(id: "33",studentId: "0000894", termCode: null,      start: new DateTimeOffset(new DateTime(2019,05,25, 00,00,0)), end: new DateTimeOffset(new DateTime(2019,05,31, 15,30,0))),
                };
                regPriorityRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(registrationPriorities);

                // Mock Adapters
                var registrationPriorityDtoAdapter = new AutoMapperAdapter<RegistrationPriority, Dtos.Student.RegistrationPriority>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<RegistrationPriority, Dtos.Student.RegistrationPriority>()).Returns(registrationPriorityDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                regPriorityRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRegistrationPrioritiesAsync_NullStudentId_ThrowsException()
            {
                await studentService.GetRegistrationPrioritiesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRegistrationPrioritiesAsync_EmptyStudentId_ThrowsException()
            {
                await studentService.GetRegistrationPrioritiesAsync(String.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetRegistrationPrioritiesAsync_StudentIdNotSelf_ThrowsException()
            {
                await studentService.GetRegistrationPrioritiesAsync("0000001");
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_StudentWithNoPriorities_EmptyCollection()
            {
                regPriorityRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(new List<RegistrationPriority>());

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_StudentWithPriorities_ReturnsPriorities()
            {
                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
                for (int i = 0; i < registrationPriorities.Count(); i++)
                {
                    Assert.AreEqual(registrationPriorities.ElementAt(i).Id, result.ElementAt(i).Id);
                    Assert.AreEqual(registrationPriorities.ElementAt(i).StudentId, result.ElementAt(i).StudentId);
                    Assert.AreEqual(registrationPriorities.ElementAt(i).TermCode, result.ElementAt(i).TermCode);
                    Assert.AreEqual(registrationPriorities.ElementAt(i).Start, result.ElementAt(i).Start);
                    Assert.AreEqual(registrationPriorities.ElementAt(i).End, result.ElementAt(i).End);
                }
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_ReturnsNullPriorityRecords_DoesNotThrow()
            {
                var regPriorities = new List<RegistrationPriority>()
                {
                    new RegistrationPriority(id: "1", studentId: "0000894", termCode: "2019/FA", start: new DateTimeOffset(new DateTime(2019,05,12, 07,00,0)), end: new DateTimeOffset(new DateTime(2019,05,12, 17,00,0))),
                    null, //handles nulls gracefully
                    new RegistrationPriority(id: "2", studentId: "0000894", termCode: null,      start: new DateTimeOffset(new DateTime(2018,04,01, 00,00,0)), end: new DateTimeOffset(new DateTime(2018,05,12, 20,00,0))),
                    new RegistrationPriority(id: "3", studentId: "0000894", termCode: "2022/FA", start: null,                                                  end: new DateTimeOffset(new DateTime(2019,05,12, 07,19,0))),
                    new RegistrationPriority(id: "10",studentId: "0000894", termCode: "2023/SP", start: new DateTimeOffset(new DateTime(2022,11,05, 06,00,0)), end: new DateTimeOffset(new DateTime(2022,11,06, 21,00,0))),
                    new RegistrationPriority(id: "13",studentId: "0000894", termCode: "2023/FA", start: new DateTimeOffset(new DateTime(2023,05,20, 08,00,0)), end: null),
                    new RegistrationPriority(id: "20",studentId: "0000894", termCode: "2023/SU", start: new DateTimeOffset(new DateTime(2023,03,02, 05,00,0)), end: new DateTimeOffset(new DateTime(2023,04,15, 19,00,0))),
                    new RegistrationPriority(id: "33",studentId: "0000894", termCode: null,      start: new DateTimeOffset(new DateTime(2019,05,25, 00,00,0)), end: new DateTimeOffset(new DateTime(2019,05,31, 15,30,0))),
                };
                regPriorityRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(regPriorities);

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(regPriorities.Count(), result.Count());
            }
        }

        [TestClass]
        public class GetRegistrationPrioritiesAsync_Advisor : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private IRegistrationPriorityRepository regPriorityRepo;
            private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;

            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
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

            private IEnumerable<RegistrationPriority> registrationPriorities;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;

                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;

                logger = new Mock<ILogger>().Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;

                // Mock registration repository response
                registrationPriorities = new List<RegistrationPriority>()
                {
                    new RegistrationPriority(id: "1", studentId: "0000894", termCode: "2019/FA", start: new DateTimeOffset(new DateTime(2019,05,12, 07,00,0)), end: new DateTimeOffset(new DateTime(2019,05,12, 17,00,0))),
                    new RegistrationPriority(id: "2", studentId: "0000894", termCode: null,      start: new DateTimeOffset(new DateTime(2018,04,01, 00,00,0)), end: new DateTimeOffset(new DateTime(2018,05,12, 20,00,0))),
                    new RegistrationPriority(id: "3", studentId: "0000894", termCode: "2022/FA", start: null,                                                  end: new DateTimeOffset(new DateTime(2019,05,12, 07,19,0))),
                    new RegistrationPriority(id: "10",studentId: "0000894", termCode: "2023/SP", start: new DateTimeOffset(new DateTime(2022,11,05, 06,00,0)), end: new DateTimeOffset(new DateTime(2022,11,06, 21,00,0))),
                    new RegistrationPriority(id: "13",studentId: "0000894", termCode: "2023/FA", start: new DateTimeOffset(new DateTime(2023,05,20, 08,00,0)), end: null),
                    new RegistrationPriority(id: "20",studentId: "0000894", termCode: "2023/SU", start: new DateTimeOffset(new DateTime(2023,03,02, 05,00,0)), end: new DateTimeOffset(new DateTime(2023,04,15, 19,00,0))),
                    new RegistrationPriority(id: "33",studentId: "0000894", termCode: null,      start: new DateTimeOffset(new DateTime(2019,05,25, 00,00,0)), end: new DateTimeOffset(new DateTime(2019,05,31, 15,30,0))),
                };
                regPriorityRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(registrationPriorities);

                // Mock Adapters
                var registrationPriorityDtoAdapter = new AutoMapperAdapter<RegistrationPriority, Dtos.Student.RegistrationPriority>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<RegistrationPriority, Dtos.Student.RegistrationPriority>()).Returns(registrationPriorityDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                regPriorityRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetRegistrationPrioritiesAsync_NoAccess_UserHasNoPermission()
            {
                await studentService.GetRegistrationPrioritiesAsync("0000894");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetRegistrationPrioritiesAsync_ViewStudentInformationPermission()
            {
                // A user with VIEW.STUDENT.INFORMATION - such as a faculty member - is not enough to see a student's registration eligibility
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                await studentService.GetRegistrationPrioritiesAsync("0000894");
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_ViewAnyAdviseePermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_ReviewAnyAdviseePermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_UpdateAnyAdviseePermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_AllAccessAnyAdviseePermission_Allowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_ViewAssignedAdviseesPermission_Allowed()
            {
                // Set up advisor assignment
                var student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student.AddAdvisement("0000896", null, null, null);
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(l => l.Contains("0000894")))).ReturnsAsync(new List<StudentAccess>() { studentAccess }.AsEnumerable());

                // add assigned advisor permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_ReviewAssignedAdviseesPermission_Allowed()
            {
                // Set up advisor assignment
                var student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student.AddAdvisement("0000896", null, null, null);
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(l => l.Contains("0000894")))).ReturnsAsync(new List<StudentAccess>() { studentAccess }.AsEnumerable());

                // add assigned advisor permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_UpdateAssignedAdviseesPermission_Allowed()
            {
                // Set up advisor assignment
                var student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student.AddAdvisement("0000896", null, null, null);
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(l => l.Contains("0000894")))).ReturnsAsync(new List<StudentAccess>() { studentAccess }.AsEnumerable());

                // add assigned advisor permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_AllAccessAssignedAdviseesPermission_Allowed()
            {
                // Set up advisor assignment
                var student = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                student.AddAdvisement("0000896", null, null, null);
                student.AddAdvisement("0000111", null, null, null);
                var studentAccess = student.ConvertToStudentAccess();
                studentRepoMock.Setup(repo => repo.GetStudentAccessAsync(It.Is<List<string>>(l => l.Contains("0000894")))).ReturnsAsync(new List<StudentAccess>() { studentAccess }.AsEnumerable());

                // add assigned advisor permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
            }


            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRegistrationPrioritiesAsync_NullStudentId_ThrowsException()
            {
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                await studentService.GetRegistrationPrioritiesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRegistrationPrioritiesAsync_EmptyStudentId_ThrowsException()
            {
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                await studentService.GetRegistrationPrioritiesAsync(String.Empty);
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_StudentWithNoPriorities_EmptyCollection()
            {
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                regPriorityRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(new List<RegistrationPriority>());

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_StudentWithPriorities_ReturnsPriorities()
            {
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(registrationPriorities.Count(), result.Count());
                for (int i = 0; i < registrationPriorities.Count(); i++)
                {
                    Assert.AreEqual(registrationPriorities.ElementAt(i).Id, result.ElementAt(i).Id);
                    Assert.AreEqual(registrationPriorities.ElementAt(i).StudentId, result.ElementAt(i).StudentId);
                    Assert.AreEqual(registrationPriorities.ElementAt(i).TermCode, result.ElementAt(i).TermCode);
                    Assert.AreEqual(registrationPriorities.ElementAt(i).Start, result.ElementAt(i).Start);
                    Assert.AreEqual(registrationPriorities.ElementAt(i).End, result.ElementAt(i).End);
                }
            }

            [TestMethod]
            public async Task GetRegistrationPrioritiesAsync_ReturnsNullPriorityRecords_DoesNotThrow()
            {
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                var regPriorities = new List<RegistrationPriority>()
                {
                    new RegistrationPriority(id: "1", studentId: "0000894", termCode: "2019/FA", start: new DateTimeOffset(new DateTime(2019,05,12, 07,00,0)), end: new DateTimeOffset(new DateTime(2019,05,12, 17,00,0))),
                    null, //handles nulls gracefully
                    new RegistrationPriority(id: "2", studentId: "0000894", termCode: null,      start: new DateTimeOffset(new DateTime(2018,04,01, 00,00,0)), end: new DateTimeOffset(new DateTime(2018,05,12, 20,00,0))),
                    new RegistrationPriority(id: "3", studentId: "0000894", termCode: "2022/FA", start: null,                                                  end: new DateTimeOffset(new DateTime(2019,05,12, 07,19,0))),
                    new RegistrationPriority(id: "10",studentId: "0000894", termCode: "2023/SP", start: new DateTimeOffset(new DateTime(2022,11,05, 06,00,0)), end: new DateTimeOffset(new DateTime(2022,11,06, 21,00,0))),
                    new RegistrationPriority(id: "13",studentId: "0000894", termCode: "2023/FA", start: new DateTimeOffset(new DateTime(2023,05,20, 08,00,0)), end: null),
                    new RegistrationPriority(id: "20",studentId: "0000894", termCode: "2023/SU", start: new DateTimeOffset(new DateTime(2023,03,02, 05,00,0)), end: new DateTimeOffset(new DateTime(2023,04,15, 19,00,0))),
                    new RegistrationPriority(id: "33",studentId: "0000894", termCode: null,      start: new DateTimeOffset(new DateTime(2019,05,25, 00,00,0)), end: new DateTimeOffset(new DateTime(2019,05,31, 15,30,0))),
                };
                regPriorityRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(regPriorities);

                var result = await studentService.GetRegistrationPrioritiesAsync("0000894");

                Assert.IsNotNull(result);
                Assert.AreEqual(regPriorities.Count(), result.Count());
            }
        }
        #endregion GetRegistrationPrioritiesAsync

        [TestClass]
        public class Search : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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
                var student1 = new Domain.Student.Entities.Student("00000001", "Dog", null, null, null, null) { FirstName = "Able" };
                var student2 = new Domain.Student.Entities.Student("00000002", "Dog", null, null, null, null) { FirstName = "Baker" };
                var student3 = new Domain.Student.Entities.Student("00000003", "Dog", null, null, null, null) { FirstName = "Charlie" };

                var justOne = new List<Domain.Student.Entities.Student>() { student1 };
                var justTwo = new List<Domain.Student.Entities.Student>() { student2 };
                var allThree = new List<Domain.Student.Entities.Student>() { student1, student2, student3 };

                studentRepoMock.Setup(svc => svc.SearchAsync("Dog", null, null, null, null, null)).ReturnsAsync(allThree.AsEnumerable());
                studentRepoMock.Setup(svc => svc.SearchAsync("Dog", null, DateTime.Parse("3/3/33"), null, null, null)).ReturnsAsync(justTwo.AsEnumerable());
                studentRepoMock.Setup(svc => svc.SearchAsync("Dog", "Able", null, null, null, null)).ReturnsAsync(justOne.AsEnumerable());
                studentRepoMock.Setup(svc => svc.SearchAsync("Dog", "Baker", null, null, null, null)).ReturnsAsync(justTwo.AsEnumerable());
                studentRepoMock.Setup(svc => svc.SearchAsync("Smith", null, null, null, null, null)).Throws(new KeyNotFoundException("x"));
                studentRepoMock.Setup(svc => svc.SearchAsync(null, null, null, null, null, null)).Throws(new KeyNotFoundException("x"));

                // Mock Adapters
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);

                // Mock advisor role so that permission exception not thrown
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IAcademicCreditRepository> acadCreditRepoMock;
            private IAcademicCreditRepository acadCreditRepo;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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
                var restrictionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.TranscriptRestriction, Dtos.Student.TranscriptRestriction>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.TranscriptRestriction, Dtos.Student.TranscriptRestriction>()).Returns(restrictionDtoAdapter);

                // Mock advisor role so that permission exception not thrown
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                advisorStudentService = new StudentService(
                    adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, new CurrentUserSetup.AdvisorUserFactory(),
                    roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
                studentStudentService = new StudentService(
                    adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, new CurrentUserSetup.StudentUserFactory(),
                    roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
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
                var permissionViewAnyStudent = new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation);
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
        public class GetUngradedTerms : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);

                var termDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Term, Dtos.Student.Term>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Term, Dtos.Student.Term>()).Returns(termDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
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
                advisorRole.AddPermission(new Domain.Entities.Permission("VIEW.STUDENT.INFORMATION"));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
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
        public class OrderTranscriptAsync : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.Transcripts.TranscriptRequest, Domain.Student.Entities.Transcripts.TranscriptRequest>()).Returns(requestDtoAdapter);

                // Mock advisor role so that permission exception not thrown
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
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

            /// <summary>
            /// In this test we call OrderTranscriptAsync with a user who has the VIEW.ANY.ADVISEE permission, so the request should be successful
            /// </summary>
            [TestMethod]
            public async Task StudentService_OrderTranscriptAsync_Valid()
            {
                // Return "success" to coordination service when repository method is called
                studentRepoMock.Setup(repo => repo.OrderTranscriptAsync(It.IsAny<TranscriptRequest>())).ReturnsAsync("success");
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                // Create TranscriptRequest DTO to call OrderTranscriptAsync
                var transcriptRequestDto = new Dtos.Student.Transcripts.TranscriptRequest()
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

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                // Call OrderTranscriptAsync with DTO
                var result = await studentService.OrderTranscriptAsync(transcriptRequestDto);

                // Verify that the repository returned the correct response
                Assert.AreEqual("success", result);
            }

            /// <summary>
            /// In this test we call OrderTranscriptAsync but the user does not have the VIEW.ANY.ADVISEE permission, so a PermissionsException should be thrown
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentService_OrderTranscriptAsync_PermissionsException()
            {
                // Set up the advisorRole so that it does NOT have the VIEW.ANY.ADVISEE permission code (which is needed when calling OrderTranscriptAsync)
                advisorRole1 = new Role(advisorRole1.Id, advisorRole1.Title);
                advisorRole1.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAdviseeDegreePlan));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole1 });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole1 });

                // Create TranscriptRequest DTO to call OrderTranscriptAsync
                var transcriptRequestDto = new Dtos.Student.Transcripts.TranscriptRequest()
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

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                // Call OrderTranscriptAsync with DTO
                var result = await studentService.OrderTranscriptAsync(transcriptRequestDto);
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
        public class CheckTranscriptStatusAsync : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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
                adapterRegistryMock.Setup(x => x.GetAdapter<Dtos.Student.Transcripts.TranscriptRequest, Domain.Student.Entities.Transcripts.TranscriptRequest>()).Returns(requestDtoAdapter);

                // Mock advisor role so that permission exception not thrown
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            /// <summary>
            /// In this test we call CheckTranscriptStatusAsync with a user who has the VIEW.ANY.ADVISEE permission, so the request should be successful
            /// </summary>
            [TestMethod]
            public async Task StudentService_CheckTranscriptStatusAsync_Valid()
            {
                // Return "success" to coordination service when repository method is called
                studentRepoMock.Setup(repo => repo.CheckTranscriptStatusAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("success");

                // Call CheckTranscriptStatusAsync with arguments
                var result = await studentService.CheckTranscriptStatusAsync("1", "PROCESSING");

                // Verify that the repository returned the correct response
                Assert.AreEqual("success", result);
            }

            /// <summary>
            /// In this test we call CheckTranscriptStatusAsync but the user does not have the VIEW.ANY.ADVISEE permission, so a PermissionsException should be thrown
            /// </summary>
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentService_CheckTranscriptStatusAsync_PermissionsException()
            {
                // Set up the advisorRole so that it does NOT have the VIEW.ANY.ADVISEE permission code (which is needed when calling CheckTranscriptStatusAsync)
                advisorRole = new Role(advisorRole.Id, advisorRole.Title);
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAdviseeDegreePlan));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                // Call CheckTranscriptStatusAsync with arguments
                var result = await studentService.CheckTranscriptStatusAsync("1", "PROCESSING");
            }
        }

        [TestClass]
        public class Register : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Dtos.Student.SectionRegistration> sectionRegistrations;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            // Used by Two_Section_Registration_Successful
            private List<Domain.Student.Entities.RegistrationMessage> messagesTwoElementRegRequest;
            private List<Dtos.Student.SectionRegistration> sectionRegDtoTwoElementList;


            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
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
                var messages = new List<Domain.Student.Entities.RegistrationMessage>() { new Domain.Student.Entities.RegistrationMessage() { Message = "Success", SectionId = "" } };
                var response = new Domain.Student.Entities.RegistrationResponse(messages, null, null);
                studentRepoMock.Setup(x => x.RegisterAsync(It.IsAny<Domain.Student.Entities.RegistrationRequest>())).ReturnsAsync(response);

                // Setup a SectionRegistration dto with two entries
                sectionRegDtoTwoElementList = new List<Dtos.Student.SectionRegistration>();
                var sectionRegDto = new Dtos.Student.SectionRegistration();
                sectionRegDto.Action = Dtos.Student.RegistrationAction.Drop;
                sectionRegDto.Credits = 3.5m;
                sectionRegDto.DropReasonCode = "DropCode1";
                sectionRegDto.IntentToWithdrawId = null;
                sectionRegDto.SectionId = "SectionID1";
                sectionRegDtoTwoElementList.Add(sectionRegDto);
                var sectionRegDto2 = new Dtos.Student.SectionRegistration();
                sectionRegDto2.Action = Dtos.Student.RegistrationAction.Add;
                sectionRegDto2.Credits = 3.0m;
                sectionRegDto2.DropReasonCode = null;
                sectionRegDto2.IntentToWithdrawId = "7654";
                sectionRegDto2.SectionId = "SectionID2";
                sectionRegDtoTwoElementList.Add(sectionRegDto2);

                // Mock the student repository RegisterAsync when passed a RegistrationRequest object with the exact same data that is contained int the
                // sectionRegDtoTwoElementList defined above. This will test that the RegisterAsync service method correctly translates the dto to the entity.
                messagesTwoElementRegRequest = new List<Domain.Student.Entities.RegistrationMessage>()
                    { new Domain.Student.Entities.RegistrationMessage() { Message = "Successful Two Element Reg Request", SectionId = "" } };
                var responseTwoElementRegRequest = new Domain.Student.Entities.RegistrationResponse(messagesTwoElementRegRequest, null, null);
                studentRepoMock.Setup(x => x.RegisterAsync(
                       It.Is<Domain.Student.Entities.RegistrationRequest>(
                           r => (r.Sections.Count == sectionRegDtoTwoElementList.Count) && (r.Sections[0].SectionId == sectionRegDtoTwoElementList[0].SectionId) &&
                                (r.Sections[0].Action.ToString() == sectionRegDtoTwoElementList[0].Action.ToString()) &&
                                (r.Sections[0].Credits == sectionRegDtoTwoElementList[0].Credits) &&
                                (r.Sections[0].DropReasonCode == sectionRegDtoTwoElementList[0].DropReasonCode) &&
                                (r.Sections[0].IntentToWithdrawId == sectionRegDtoTwoElementList[0].IntentToWithdrawId) &&
                                (r.Sections[1].SectionId == sectionRegDtoTwoElementList[1].SectionId) &&
                                (r.Sections[1].Action.ToString() == sectionRegDtoTwoElementList[1].Action.ToString()) &&
                                (r.Sections[1].Credits == sectionRegDtoTwoElementList[1].Credits) &&
                                (r.Sections[1].DropReasonCode == sectionRegDtoTwoElementList[1].DropReasonCode) &&
                                (r.Sections[1].IntentToWithdrawId == sectionRegDtoTwoElementList[1].IntentToWithdrawId)
                                )))
                           .ReturnsAsync(responseTwoElementRegRequest);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Set up sectionRegistrations
                sectionRegistrations = new List<Dtos.Student.SectionRegistration>() { new Dtos.Student.SectionRegistration() { Action = Dtos.Student.RegistrationAction.Add, Credits = null, SectionId = "1111" } };

                studentService = new StudentService(
                    adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo,
                    logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
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
                await studentService.RegisterAsync("00004002", new List<Dtos.Student.SectionRegistration>());
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo,
                    logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo,
                    logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                // Now try to register for a student who has this advisor as an assigned advisor.
                var registrationResponse = await studentService.RegisterAsync("00004002", sectionRegistrations);
                Assert.AreEqual(1, registrationResponse.Messages.Count());
            }

            [TestMethod]
            public async Task AllAdviseePermission_RegistrationSuccessful()
            {
                // Set up update permissions on advisor's role
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo,
                    logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                // Now try to register for a student who has this advisor as an assigned advisor.
                var registrationResponse = await studentService.RegisterAsync("00004003", sectionRegistrations);
                Assert.AreEqual(1, registrationResponse.Messages.Count());
            }

            [TestMethod]
            public async Task StudentService_RegisterAsync_Entity_Properties_Copied_to_DTO()
            {
                List<RegistrationMessage> regMessages = new List<RegistrationMessage>() { new RegistrationMessage() { SectionId = "123", Message = "Warning" }, new RegistrationMessage() { SectionId = "234", Message = "Error" } };
                string rpcId = "123";
                List<string> registeredSections = new List<string>() { "123", "456", "789" };
                var response = new Domain.Student.Entities.RegistrationResponse(regMessages, rpcId, registeredSections);
                studentRepoMock.Setup(x => x.RegisterAsync(It.IsAny<Domain.Student.Entities.RegistrationRequest>())).ReturnsAsync(response);

                var registrationResponse = await studentService.RegisterAsync("0000894", sectionRegistrations);

                Assert.AreEqual(regMessages.Count, registrationResponse.Messages.Count);
                for (int i = 0; i < regMessages.Count; i++)
                {
                    Assert.AreEqual(regMessages[i].Message, registrationResponse.Messages[i].Message);
                    Assert.AreEqual(regMessages[i].SectionId, registrationResponse.Messages[i].SectionId);
                }
                Assert.AreEqual(rpcId, registrationResponse.PaymentControlId);
                Assert.AreEqual(registeredSections.Count, registrationResponse.RegisteredSectionIds.Count);
                for (int i = 0; i < registeredSections.Count; i++)
                {
                    Assert.AreEqual(registeredSections[i], registrationResponse.RegisteredSectionIds[i]);
                }
            }
        }

        [TestClass]
        public class RegisterValidationOnly : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;

            // The following mocks are needed to construct the StudentService object, though most are not used by the 
            // registration methods that will be tested.
            private IPlanningStudentRepository planningStudentRepo;
            private IStudentConfigurationRepository studentConfigurationRepo;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;
            private Mock<IPersonRepository> personRepoMock;
            private IPersonRepository personRepo;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepositoryRepo;
            private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IEnumerable<Dtos.Student.SectionRegistration> sectionRegistrations;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            // Used by Two_Section_Registration_Successful
            private List<Domain.Student.Entities.RegistrationMessage> messagesTwoElementRegRequest;
            private List<Dtos.Student.SectionRegistration> sectionRegDtoTwoElementList;

            private string personGuidNotExists = "{notExistPerson}";
            private string personGuidExists = "{existsGuid}";
            private string personIDExists = "existPerson";

            private string sectionGuidNotExists = "{notExistSection}";
            private string sectionGuidAExists = "{sectionGuidA}";
            private string sectionIDAExists = "sectionIDA";
            private string sectionGuidBExists = "{sectionGuidB}";
            private string sectionIDBExists = "sectionIDB";


            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;
                logger = new Mock<ILogger>().Object;

                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;

                // Setup a SectionRegistration dto with two entries
                sectionRegDtoTwoElementList = new List<Dtos.Student.SectionRegistration>();
                var sectionRegDto = new Dtos.Student.SectionRegistration();
                sectionRegDto.Action = Dtos.Student.RegistrationAction.Drop;
                sectionRegDto.Credits = 3.5m;
                sectionRegDto.DropReasonCode = "DropCode1";
                sectionRegDto.IntentToWithdrawId = null;
                sectionRegDto.SectionId = "SectionID1";
                sectionRegDtoTwoElementList.Add(sectionRegDto);
                var sectionRegDto2 = new Dtos.Student.SectionRegistration();
                sectionRegDto2.Action = Dtos.Student.RegistrationAction.Add;
                sectionRegDto2.Credits = 3.0m;
                sectionRegDto2.DropReasonCode = null;
                sectionRegDto2.IntentToWithdrawId = "7654";
                sectionRegDto2.SectionId = "SectionID2";
                sectionRegDtoTwoElementList.Add(sectionRegDto2);

                // Mock the student repository RegisterAsync when passed a RegistrationRequest object with the exact same data that is contained int the
                // sectionRegDtoTwoElementList defined above. This will test that the RegisterAsync service method correctly translates the dto to the entity.
                messagesTwoElementRegRequest = new List<Domain.Student.Entities.RegistrationMessage>()
                    { new Domain.Student.Entities.RegistrationMessage() { Message = "Successful Two Element Reg Request", SectionId = "" } };
                var responseTwoElementRegRequest = new Domain.Student.Entities.RegistrationResponse(messagesTwoElementRegRequest, null, null);
                studentRepoMock.Setup(x => x.RegisterAsync(
                       It.Is<Domain.Student.Entities.RegistrationRequest>(
                           r => (r.Sections.Count == sectionRegDtoTwoElementList.Count) && (r.Sections[0].SectionId == sectionRegDtoTwoElementList[0].SectionId) &&
                                (r.Sections[0].Action.ToString() == sectionRegDtoTwoElementList[0].Action.ToString()) &&
                                (r.Sections[0].Credits == sectionRegDtoTwoElementList[0].Credits) &&
                                (r.Sections[0].DropReasonCode == sectionRegDtoTwoElementList[0].DropReasonCode) &&
                                (r.Sections[0].IntentToWithdrawId == sectionRegDtoTwoElementList[0].IntentToWithdrawId) &&
                                (r.Sections[1].SectionId == sectionRegDtoTwoElementList[1].SectionId) &&
                                (r.Sections[1].Action.ToString() == sectionRegDtoTwoElementList[1].Action.ToString()) &&
                                (r.Sections[1].Credits == sectionRegDtoTwoElementList[1].Credits) &&
                                (r.Sections[1].DropReasonCode == sectionRegDtoTwoElementList[1].DropReasonCode) &&
                                (r.Sections[1].IntentToWithdrawId == sectionRegDtoTwoElementList[1].IntentToWithdrawId)
                                )))
                           .ReturnsAsync(responseTwoElementRegRequest);

                // Set up current user with permission to run the register validation-only API
                currentUserFactory = new CurrentUserSetup.RegisterValidationOnlySkipValidationUserFactory();
                crossRegUserRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.RegisterSkipValidation));
                crossRegUserRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.RegisterValidationOnly));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { crossRegUserRole });

                // Set up sectionRegistrations
                sectionRegistrations = new List<Dtos.Student.SectionRegistration>() { new Dtos.Student.SectionRegistration() { Action = Dtos.Student.RegistrationAction.Add, Credits = null, SectionId = "1111" } };

                // Mock person repo person guid translations
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuidNotExists)).ReturnsAsync(String.Empty);
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(personGuidExists)).ReturnsAsync(personIDExists);

                // Mock section repo for guids that exist and do not
                sectionRepoMock.Setup(repo => repo.GetSectionIdFromGuidAsync(sectionGuidNotExists)).ReturnsAsync(String.Empty);
                sectionRepoMock.Setup(repo => repo.GetSectionIdFromGuidAsync(sectionGuidAExists)).ReturnsAsync(sectionIDAExists);
                sectionRepoMock.Setup(repo => repo.GetSectionIdFromGuidAsync(sectionGuidBExists)).ReturnsAsync(sectionIDBExists);

                studentService = new StudentService(
                        adapterRegistry, studentRepo, personRepo, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo,
                        studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo,
                        logger, planningStudentRepo, sectionRepo,
                        programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                studentService = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RegisterValidationOnly_NullStudentGuid()
            {
                var response = await studentService.RegisterValidationOnlyAsync(null, new Dtos.Student.StudentRegistrationValidationOnlyRequest());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RegisterValidationOnly_NullRequestDto()
            {
                var response = await studentService.RegisterValidationOnlyAsync("{aguid}", null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RegisterValidationOnly_RequestDtoWithNullActions()
            {
                var response = await studentService.RegisterValidationOnlyAsync("{aguid}", new Dtos.Student.StudentRegistrationValidationOnlyRequest());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task RegisterValidationOnly_RequestDtoWithZeroActions()
            {
                var request = new Dtos.Student.StudentRegistrationValidationOnlyRequest();
                request.SectionActionRequests = new List<Dtos.Student.SectionRegistrationGuid>();
                var response = await studentService.RegisterValidationOnlyAsync("{aguid}", request);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task RegisterValidationOnlyMissingPermission()
            {
                // Create a studentService instance with a CurrentUser that does not have the necessary permission.
                // (The studentService that is global to this test class does have the required permission )
                var currentUserFactoryNoPermission = new CurrentUserSetup.StudentUserFactory();

                var studentServiceNoPermission = new StudentService(
                    adapterRegistry, studentRepo, null, null, null, null, null, studentConfigurationRepo, referenceDataRepositoryRepo,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactoryNoPermission, roleRepo, staffRepo,
                    logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var request = new Dtos.Student.StudentRegistrationValidationOnlyRequest();
                request.SectionActionRequests = new List<Dtos.Student.SectionRegistrationGuid>();
                request.SectionActionRequests.Add(new Dtos.Student.SectionRegistrationGuid());
                var messages = await studentServiceNoPermission.RegisterValidationOnlyAsync("{aguid}", request);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegisterValidationBadPersonGuid()
            {
                var request = new Dtos.Student.StudentRegistrationValidationOnlyRequest();
                request.SectionActionRequests = new List<Dtos.Student.SectionRegistrationGuid>();
                request.SectionActionRequests.Add(new Dtos.Student.SectionRegistrationGuid());
                var messages = await studentService.RegisterValidationOnlyAsync(personGuidNotExists, request);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task RegisterValidationBadSectionGuid()
            {
                var request = new Dtos.Student.StudentRegistrationValidationOnlyRequest();
                request.SectionActionRequests = new List<Dtos.Student.SectionRegistrationGuid>();
                request.SectionActionRequests.Add(new Dtos.Student.SectionRegistrationGuid() { SectionGuid = sectionGuidNotExists });
                var messages = await studentService.RegisterValidationOnlyAsync(personGuidExists, request);
            }

            [TestMethod]
            public async Task RegisterValidationSuccessTwoActions()
            {
                var request = new Dtos.Student.StudentRegistrationValidationOnlyRequest();

                Dtos.Student.SectionRegistrationGuid actOne = new Dtos.Student.SectionRegistrationGuid();
                Dtos.Student.SectionRegistrationGuid actTwo = new Dtos.Student.SectionRegistrationGuid();
                request.SectionActionRequests = new List<Dtos.Student.SectionRegistrationGuid>();

                actOne.SectionGuid = sectionGuidAExists;
                actOne.Action = Dtos.Student.RegistrationAction.Add;
                actOne.Credits = 3.2M;
                actOne.DropReasonCode = "D1";
                actOne.IntentToWithdrawId = "22";
                request.SectionActionRequests.Add(actOne);

                actTwo.SectionGuid = sectionGuidBExists;
                actTwo.Action = Dtos.Student.RegistrationAction.Drop;
                actTwo.Credits = 3.0M;
                actTwo.DropReasonCode = "D2";
                actTwo.IntentToWithdrawId = "33";
                request.SectionActionRequests.Add(actTwo);

                // Mock the student repository registerasync method with a response that will test that the service method prepares its response entity correctly.
                var regMsg1 = new RegistrationMessage() { Message = "SectAMessage", SectionId = sectionIDAExists };
                var regMsg2 = new RegistrationMessage() { Message = "SectBMessage", SectionId = sectionIDBExists };
                var resMessages = new List<RegistrationMessage>();
                resMessages.Add(regMsg1);
                resMessages.Add(regMsg2);

                var regActResult1 = new SectionRegistrationActionResult() { SectionId = sectionIDAExists, Action = RegistrationAction.Add, RegistrationActionSuccess = true };
                var regActResult2 = new SectionRegistrationActionResult() { SectionId = sectionIDBExists, Action = RegistrationAction.Drop, RegistrationActionSuccess = false };

                var responseEntity = new Domain.Student.Entities.RegistrationResponse(resMessages, null, null);
                responseEntity.RegistrationActionResults.Add(regActResult1);
                responseEntity.RegistrationActionResults.Add(regActResult2);
                responseEntity.ValidationToken = "TheToken";

                //TODO BFE. Change to expect specific attributes on the request entity as passed by the service.
                // Will test that the service is setting those attributes correctly.
                studentRepoMock.Setup(x => x.RegisterAsync(It.IsAny<Domain.Student.Entities.RegistrationRequest>())).ReturnsAsync(responseEntity);

                Dtos.Student.StudentRegistrationValidationOnlyResponse resDto = await studentService.RegisterValidationOnlyAsync(personGuidExists, request);
                Assert.IsTrue(resDto.Messages.Count == 2);
                Assert.AreEqual(resDto.Messages[0].Message, regMsg1.Message);
                Assert.AreEqual(resDto.Messages[0].SectionId, regMsg1.SectionId);
                Assert.AreEqual(resDto.Messages[1].Message, regMsg2.Message);
                Assert.AreEqual(resDto.Messages[1].SectionId, regMsg2.SectionId);
                Assert.AreEqual(resDto.ValidationToken, responseEntity.ValidationToken);
                Assert.AreEqual(resDto.SectionActionResponses.Count, 2);
                Assert.AreEqual(resDto.SectionActionResponses[0].SectionGuid, sectionGuidAExists);
                Assert.AreEqual(resDto.SectionActionResponses[0].Action.ToString(), regActResult1.Action.ToString());
                Assert.AreEqual(resDto.SectionActionResponses[0].SectionActionSuccess, regActResult1.RegistrationActionSuccess);
                Assert.AreEqual(resDto.SectionActionResponses[1].SectionGuid, sectionGuidBExists);
                Assert.AreEqual(resDto.SectionActionResponses[1].Action.ToString(), regActResult2.Action.ToString());
                Assert.AreEqual(resDto.SectionActionResponses[1].SectionActionSuccess, regActResult2.RegistrationActionSuccess);
            }

            //[TestMethod]
            //public async Task Two_Section_Registration_Successful()
            //{
            //    // When passed sectionRegDtoTwoElementList, the mocked student repository RegisterAsync method called by 
            //    // studentService.RegisterAsync should return messagesTwoElementRegRequest.
            //    var registrationResponse = await studentService.RegisterAsync("0000894", sectionRegDtoTwoElementList);
            //    Assert.AreEqual(1, registrationResponse.Messages.Count());
            //    Assert.IsTrue(registrationResponse.Messages[0].Message.Equals(messagesTwoElementRegRequest[0].Message));
            //}

            //[TestMethod]
            //public async Task StudentService_RegisterAsync_Entity_Properties_Copied_to_DTO()
            //{
            //    List<RegistrationMessage> regMessages = new List<RegistrationMessage>() { new RegistrationMessage() { SectionId = "123", Message = "Warning" }, new RegistrationMessage() { SectionId = "234", Message = "Error" } };
            //    string rpcId = "123";
            //    List<string> registeredSections = new List<string>() { "123", "456", "789" };
            //    var response = new Domain.Student.Entities.RegistrationResponse(regMessages, rpcId, registeredSections);
            //    studentRepoMock.Setup(x => x.RegisterAsync(It.IsAny<Domain.Student.Entities.RegistrationRequest>())).ReturnsAsync(response);

            //    var registrationResponse = await studentService.RegisterAsync("0000894", sectionRegistrations);

            //    Assert.AreEqual(regMessages.Count, registrationResponse.Messages.Count);
            //    for (int i = 0; i < regMessages.Count; i++)
            //    {
            //        Assert.AreEqual(regMessages[i].Message, registrationResponse.Messages[i].Message);
            //        Assert.AreEqual(regMessages[i].SectionId, registrationResponse.Messages[i].SectionId);
            //    }
            //    Assert.AreEqual(rpcId, registrationResponse.PaymentControlId);
            //    Assert.AreEqual(registeredSections.Count, registrationResponse.RegisteredSectionIds.Count);
            //    for (int i = 0; i < registeredSections.Count; i++)
            //    {
            //        Assert.AreEqual(registeredSections[i], registrationResponse.RegisteredSectionIds[i]);
            //    }
            //}
        }

        [TestClass]
        public class CheckUserAccess_Advisor : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;
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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

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
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, null,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task CheckUserAccess_ViewStudentInformation_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id, student1.ConvertToStudentAccess());
            }

            [TestMethod]
            public async Task CheckUserAccess_ViewAnyAdvisee_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id, student1.ConvertToStudentAccess());
            }

            [TestMethod]
            public async Task CheckUserAccess_ReviewAnyAdvisee_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_UpdateAnyAdvisee_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_AllAccessAnyAdvisee_AccessAllowed()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task CheckUserAccess_ViewAssignedAdvisees_UnassignedAdvisee_ThrowsException()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_ViewAssignedAdvisee_AssignedAdvisee_AllowsAccess()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_UpdateAssignedAdvisee_AssignedAdvisee_AllowsAccess()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
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
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });

                // Act -- Call async Task method: CheckUserAccess throws exception if access not allowed
                await studentService.CheckUserAccessAsync(student1.Id);
            }

            [TestMethod]
            public async Task CheckUserAccess_AllAccessAssignedAdvisee_AssignedAdvisee_AllowsAccess()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

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
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);

                var emailDtoAdapter = new AutoMapperAdapter<Domain.Base.Entities.EmailAddress, Dtos.Base.EmailAddress>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Base.Entities.EmailAddress, Dtos.Base.EmailAddress>()).Returns(emailDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
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
                var studentDtoWrapper = await studentService.GetAsync("0000894");
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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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

            private List<TranscriptGrouping> allTranscriptGroupings;
            private List<Domain.Student.Entities.Requirements.Program> programs;
            private List<StudentProgram> studentPrograms;
            private StudentConfiguration studentConfigEnforced;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

                // Mock student repo response
                student1 = new Domain.Student.Entities.Student("0000894", "Smith", 2, new List<string>() { "BA.ENGL" }, new List<string>()) { FirstName = "Bob", MiddleName = "Blakely" };
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);

                // Mock student when request is from someone else 
                student2 = new Domain.Student.Entities.Student("00004002", "Jones", 802, new List<string>() { "BA.MATH" }, new List<string>());
                studentRepoMock.Setup(repo => repo.GetAsync("00004002")).ReturnsAsync(student2);

                // Mock advisor role so that permission exception not thrown
                advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { advisorRole });
                roleRepoMock.Setup(repo => repo.Roles).Returns(new List<Role>() { advisorRole });

                // Mock the Student reposiory TranscriptRestriction responses
                studentRepoMock.Setup(repo => repo.GetTranscriptRestrictionsAsync("0000999")).Throws(new KeyNotFoundException());

                // Mock Adapters
                var restrictionDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.TranscriptRestriction, Dtos.Student.TranscriptRestriction>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.TranscriptRestriction, Dtos.Student.TranscriptRestriction>()).Returns(restrictionDtoAdapter);


                // Set up student 0000894 as the current user.
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                // Mock student restrictions
                studentConfigEnforced = new StudentConfiguration()
                {
                    FacultyPhoneTypeCode = "OFFICE",
                    FacultyEmailTypeCode = "WORK",
                    EnforceTranscriptRestriction = true
                };
                studentConfigurationRepoMock.Setup(x => x.GetStudentConfigurationAsync()).ReturnsAsync(studentConfigEnforced);

                // mock transcript groupings
                allTranscriptGroupings = new List<TranscriptGrouping>();
                allTranscriptGroupings.Add(new TranscriptGrouping("CE", "Continuing Education", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("DA", "Degree Audit", true));
                allTranscriptGroupings.Add(new TranscriptGrouping("GR", "Graduate", true));
                allTranscriptGroupings.Add(new TranscriptGrouping("FA", "Finacial Aid", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("UG", "Undergraduate", true));
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(allTranscriptGroupings));

                // mock programs
                var programBA = new Domain.Student.Entities.Requirements.Program("ENGL.BA", "english ba", new List<string>() { "english" }, true, "2", new CreditFilter(), true, null);
                var programMA = new Domain.Student.Entities.Requirements.Program("ENGL.MA", "english ma", new List<string>() { "english" }, true, "2", new CreditFilter(), true, null);
                var programCE = new Domain.Student.Entities.Requirements.Program("ENGL.CE", "english ce", new List<string>() { "english" }, true, "2", new CreditFilter(), true, null);
                programBA.TranscriptGrouping = "UG";
                programMA.TranscriptGrouping = "GR";
                programCE.TranscriptGrouping = "CE";

                programs = new List<Ellucian.Colleague.Domain.Student.Entities.Requirements.Program>();
                programs.Add(programBA);
                programs.Add(programMA);
                programs.Add(programCE);
                programRepositoryMock.Setup(x => x.GetAsync()).ReturnsAsync(programs);

                // mock student programs
                studentPrograms = new List<StudentProgram>()
                {
                    new StudentProgram("0000894", "ENGL.BA", "2018") { ProgramName = "English BA", StartDate = DateTime.Today.AddMonths(-48), EndDate = DateTime.Today.AddMonths(50) },
                    new StudentProgram("0000894", "ENGL.MA", "2020") { ProgramName = "English MA", StartDate = DateTime.Today.AddMonths(-12), EndDate = DateTime.Today.AddMonths(24) },
                    new StudentProgram("0000894", "ENGL.CE", "2020") { ProgramName = "English Continuing Education", StartDate = DateTime.Today, EndDate = DateTime.Today.AddMonths(36) },
                };
                studentProgramRepositoryMock.Setup(spr => spr.GetAsync(It.IsAny<string>())).ReturnsAsync(studentPrograms);


                // Mock successful Get of a transcript
                string transcriptText = "Ellucian University Transcript";
                studentRepoMock.Setup(repo => repo.GetTranscriptAsync("0000894", "UG")).ReturnsAsync(transcriptText);

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentService = null;
                planningStudentRepo = null;
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetUnofficialTranscriptAsync_ThrowsErrorIfNotSelfOrAdvisorWithPermissions()
            {
                await studentService.GetUnofficialTranscriptAsync("00004002", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetUnofficialTranscriptAsync_UserHasViewStudentInformationPermission()
            {
                // Set up student 0000894 as the current user.
                //Mock roles repo and permission
                var permissionViewAnyStudent = new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation);
                viewStudentRole.AddPermission(permissionViewAnyStudent);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewStudentRole });
                studentRepoMock.Setup(repo => repo.GetAsync("0004002")).ReturnsAsync(student2);
                await studentService.GetUnofficialTranscriptAsync("0004002", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelfWithEnforableRestrictionsNoRestrictions_Returns_Pdf()
            {
                // student 0000894 as the current user.
                IEnumerable<Domain.Student.Entities.TranscriptRestriction> emptyRestrictions = new List<Domain.Student.Entities.TranscriptRestriction>();
                studentRepoMock.Setup(repo => repo.GetTranscriptRestrictionsAsync("0000894")).ReturnsAsync(emptyRestrictions);

                var expectedPdfFileName = Regex.Replace(
                    (student2.LastName +
                    " " + student2.FirstName +
                    " " + student2.Id +
                    " " + DateTime.Now.ToShortDateString()),
                    "[^a-zA-Z0-9_]", "_")
                    + ".pdf";
                studentRepoMock.Setup(repo => repo.GetAsync(currentUserFactory.CurrentUser.PersonId)).ReturnsAsync(student1);
                await studentService.GetUnofficialTranscriptAsync(currentUserFactory.CurrentUser.PersonId,
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscript.rdlc", "UG", "water mark path",
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscriptDeviceInfo.txt");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelfWithEnforableRestrictions_Throws_PermissionsException()
            {
                // student 0000894 as the current user.
                IEnumerable<Domain.Student.Entities.TranscriptRestriction> oneRestriction =
                    new List<Domain.Student.Entities.TranscriptRestriction>() { new Domain.Student.Entities.TranscriptRestriction() { Code = "TEST", Description = "TEST" } };
                studentRepoMock.Setup(repo => repo.GetTranscriptRestrictionsAsync("0000894")).ReturnsAsync(oneRestriction);

                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student2);
                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelfWithoutEnforableRestrictions__Returns_Pdf()
            {
                // student 0000894 as the current user.
                studentConfigEnforced.EnforceTranscriptRestriction = false;
                IEnumerable<Domain.Student.Entities.TranscriptRestriction> oneRestriction =
                    new List<Domain.Student.Entities.TranscriptRestriction>() { new Domain.Student.Entities.TranscriptRestriction() { Code = "TEST", Description = "TEST" } };
                studentRepoMock.Setup(repo => repo.GetTranscriptRestrictionsAsync("0000894")).ReturnsAsync(oneRestriction);

                var expectedPdfFileName = Regex.Replace(
                    (student2.LastName +
                    " " + student2.FirstName +
                    " " + student2.Id +
                    " " + DateTime.Now.ToShortDateString()),
                    "[^a-zA-Z0-9_]", "_")
                    + ".pdf";
                studentRepoMock.Setup(repo => repo.GetAsync(currentUserFactory.CurrentUser.PersonId)).ReturnsAsync(student1);
                await studentService.GetUnofficialTranscriptAsync(currentUserFactory.CurrentUser.PersonId,
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscript.rdlc", "UG", "water mark path",
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscriptDeviceInfo.txt");
            }

            [TestMethod]
            public async Task GetUnofficialTranscriptAsync_ReturnsPdf_AdvisorViewsStudentWithRestrictionsEnforced_Returns_Pdf()
            {
                // Set up advisor 0000111 as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                IEnumerable<Domain.Student.Entities.TranscriptRestriction> oneRestriction =
                    new List<Domain.Student.Entities.TranscriptRestriction>() { new Domain.Student.Entities.TranscriptRestriction() { Code = "TEST", Description = "TEST" } };
                studentRepoMock.Setup(repo => repo.GetTranscriptRestrictionsAsync("0000894")).ReturnsAsync(oneRestriction);

                var expectedPdfFileName = Regex.Replace(
                    (student2.LastName +
                    " " + student2.FirstName +
                    " " + student2.Id +
                    " " + DateTime.Now.ToShortDateString()),
                    "[^a-zA-Z0-9_]", "_")
                    + ".pdf";
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                await studentService.GetUnofficialTranscriptAsync("0000894",
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscript.rdlc", "UG", "water mark path",
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscriptDeviceInfo.txt");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelf_InvalidTranscriptGrouping_Throws_KeyNotFoundException()
            {
                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "ABC123", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_AdvisorViewStudent_InvalidTranscriptGrouping_Throws_KeyNotFoundException()
            {
                // Set up advisor 0000111 as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "ABC123", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelf_ValidTranscriptGroupingNotOnProgram_Throws_KeyNotFoundException()
            {
                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "DA", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_AdvisorViewStudent_ValidTranscriptGroupingNotOnPrograM_Throws_KeyNotFoundException()
            {
                // Set up advisor 0000111 as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "DA", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelf_NoTranscriptGroupings_Throws_KeyNotFoundException()
            {
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(new List<TranscriptGrouping>()));
                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_AdvisorViewStudent_NoTranscriptGroupings_Throws_KeyNotFoundException()
            {
                // Set up advisor 0000111 as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(new List<TranscriptGrouping>()));

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelf_NoSelectableTranscriptGroupings_Throws_KeyNotFoundException()
            {
                allTranscriptGroupings = new List<TranscriptGrouping>();
                allTranscriptGroupings.Add(new TranscriptGrouping("CE", "Continuing Education", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("DA", "Degree Audit", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("GR", "Graduate", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("FA", "Finacial Aid", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("UG", "Undergraduate", false));
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(allTranscriptGroupings));

                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_AdvisorViewStudent_NoSelectableTranscriptGroupings_Throws_KeyNotFoundException()
            {
                allTranscriptGroupings = new List<TranscriptGrouping>();
                allTranscriptGroupings.Add(new TranscriptGrouping("CE", "Continuing Education", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("DA", "Degree Audit", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("GR", "Graduate", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("FA", "Finacial Aid", false));
                allTranscriptGroupings.Add(new TranscriptGrouping("UG", "Undergraduate", false));
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(allTranscriptGroupings));

                // Set up advisor 0000111 as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelf_TranscriptGroupingsNull_Throws_KeyNotFoundException()
            {
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(null));
                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetUnofficialTranscriptAsync_AdvisorViewStudent_TranscriptGroupingsNull_Throws_KeyNotFoundException()
            {
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(null));

                // Set up advisor 0000111 as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                await studentService.GetUnofficialTranscriptAsync("0000894", "rdlc path", "UG", "water mark path", "device info path");
            }

            [TestMethod]
            public async Task GetUnofficialTranscriptAsync_StudentIsSelf_TranscriptGroupingsHasNullRecord_Returns_Pdf()
            {
                allTranscriptGroupings.Add(null);
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(allTranscriptGroupings));

                var expectedPdfFileName = Regex.Replace(
                    (student2.LastName +
                    " " + student2.FirstName +
                    " " + student2.Id +
                    " " + DateTime.Now.ToShortDateString()),
                    "[^a-zA-Z0-9_]", "_")
                    + ".pdf";
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                await studentService.GetUnofficialTranscriptAsync("0000894",
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscript.rdlc", "UG", "water mark path",
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscriptDeviceInfo.txt");
            }

            [TestMethod]
            public async Task GetUnofficialTranscriptAsync_AdvisorViewStudent_TranscriptGroupingsNullRecord_Returns_Pdf()
            {
                //TranscriptGrouping nullGrouping = null;
                allTranscriptGroupings.Add(null);
                transcriptGroupingRepositoryMock.Setup(x => x.GetAsync()).Returns(Task.FromResult<IEnumerable<TranscriptGrouping>>(allTranscriptGroupings));

                // Set up advisor 0000111 as the current user.
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var expectedPdfFileName = Regex.Replace(
                    (student2.LastName +
                    " " + student2.FirstName +
                    " " + student2.Id +
                    " " + DateTime.Now.ToShortDateString()),
                    "[^a-zA-Z0-9_]", "_")
                    + ".pdf";
                studentRepoMock.Setup(repo => repo.GetAsync("0000894")).ReturnsAsync(student1);
                await studentService.GetUnofficialTranscriptAsync("0000894",
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscript.rdlc", "UG", "water mark path",
                    "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscriptDeviceInfo.txt");
            }

            [TestMethod]
            public async Task GetUnofficialTranscriptAsync_Returns_Pdf_for_student_requesting_own_data()
            {
                var expectedPdfFileName = Regex.Replace(
                    (student2.LastName +
                    " " + student2.FirstName +
                    " " + student2.Id +
                    " " + DateTime.Now.ToShortDateString()),
                    "[^a-zA-Z0-9_]", "_")
                    + ".pdf";
                studentRepoMock.Setup(repo => repo.GetAsync(currentUserFactory.CurrentUser.PersonId)).ReturnsAsync(student2);
                var result = await studentService.GetUnofficialTranscriptAsync(currentUserFactory.CurrentUser.PersonId, "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscript.rdlc", "UG", "water mark path", "../../../Ellucian.Colleague.Coordination.Student/Reports/UnofficialTranscriptDeviceInfo.txt");

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Item1);
                Assert.AreEqual(expectedPdfFileName, result.Item2);
            }
        }

        [TestClass]
        public class StudentCohort_GET : CurrentUserSetup
        {
            Mock<IStudentRepository> studentRepositoryMock;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;


            private Mock<IRoleRepository> roleRepoMock;
            private ILogger logger;

            StudentService studentService;
            List<StudentCohort> studentCohortEntities = new List<StudentCohort>();
            List<Dtos.StudentCohort> studentCohortDtos = new List<Dtos.StudentCohort>();

            [TestInitialize]
            public void Initialize()
            {
                studentRepositoryMock = new Mock<IStudentRepository>();
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;


                roleRepoMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                BuildData();

                studentService = new StudentService(
                    adapterRegistryMock.Object, studentRepositoryMock.Object, personRepoMock.Object, acadCreditRepoMock.Object,
                    acadHistServiceMock.Object, termRepositoryMock.Object, priorityRepositoryMock.Object,
                    studentConfigurationRepositoryMock.Object, referenceDataRepositoryMock.Object,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory,
                    roleRepoMock.Object, staffRepo, logger, planningStudentRepoMock.Object, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepositoryMock = null;
                planningStudentRepoMock = null;
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
                var actuals = await studentService.GetAllStudentCohortsAsync(It.IsAny<Dtos.Filters.CodeItemFilter>(), It.IsAny<bool>());

                Assert.IsNotNull(actuals);

                foreach (var actual in actuals)
                {
                    var expected = studentCohortEntities.FirstOrDefault(i => i.Guid.Equals(actual.Id, StringComparison.OrdinalIgnoreCase));
                    Assert.IsNotNull(expected);

                    Assert.AreEqual(expected.Guid, actual.Id);
                    Assert.AreEqual(expected.Code, actual.Code);
                    Assert.AreEqual(expected.Description, actual.Description);
                    if (!string.IsNullOrEmpty(expected.CohortType) && expected.CohortType.Equals("FED", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.AreEqual(CohortType.Federal, actual.StudentCohortType);
                    }
                    else
                    {
                        Assert.AreEqual(CohortType.NotSet, actual.StudentCohortType);
                    }
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
                if (!string.IsNullOrEmpty(expected.CohortType) && expected.CohortType.Equals("FED", StringComparison.OrdinalIgnoreCase))
                {
                    Assert.AreEqual(CohortType.Federal, actual.StudentCohortType);
                }
                else
                {
                    Assert.AreEqual(CohortType.NotSet, actual.StudentCohortType);
                }
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
                    new StudentCohortEntity("e8dbcea5-ffb8-471e-87b7-ce5d36d5c2e7", "ATHL", "Athletes"){ CohortType = "FED" },
                    new StudentCohortEntity("c2f57ee5-1c30-44a5-9d18-311f71f7b722", "FRAT", "Fraternity"),
                    new StudentCohortEntity("f05a6c0f-3a56-4a87-b931-bc2901da5ef9", "SORO", "Sorority"),
                    new StudentCohortEntity("05872218-f749-4cdc-b4f0-43200cc21335", "ROTC", "ROTC Participants"),
                    new StudentCohortEntity("827fffc4-3dd2-4492-8f51-4134597ec4bf", "VETS", "Military Veterans"){ CohortType = "FED" }
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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

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
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

                BuildData();

                studentService = new StudentService(
                    adapterRegistryMock.Object, studentRepositoryMock.Object, personRepoMock.Object, acadCreditRepoMock.Object,
                    acadHistServiceMock.Object, termRepositoryMock.Object, priorityRepositoryMock.Object,
                    studentConfigurationRepositoryMock.Object, referenceDataRepositoryMock.Object,
                    studentReferenceDataRepositoryMock.Object, baseConfigurationRepository, currentUserFactory,
                    roleRepoMock.Object, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepositoryMock = null;
                planningStudentRepo = null;
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
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;


            IEnumerable<Domain.Student.Entities.AdmissionResidencyType> residencyStatuses;

            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                studentReferenceDataRepoMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepository = studentReferenceDataRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;


                residencyStatuses = new TestStudentRepository().GetResidencyStatusesAsync(false).Result;

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            public async Task StudentService__GetAllAsync()
            {
                studentReferenceDataRepoMock.Setup(i => i.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyStatuses);

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
                studentReferenceDataRepoMock.Setup(i => i.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyStatuses);

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
                studentReferenceDataRepoMock.Setup(i => i.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyStatuses);
                var result = await studentService.GetResidentTypeByIdAsync("123");
            }
        }

        [TestClass]
        public class QueryStudentsById4Async_Tests : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

            List<Domain.Student.Entities.Student> students;
            List<Domain.Student.Entities.Student> restrictedStudents;
            List<Domain.Student.Entities.Student> privilegedStudents;
            List<string> studentIds;
            Role viewPersonInformationRole;

            public class ViewStudentUserFactory : ICurrentUserFactory
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
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            [TestInitialize]
            public void QueryStudentsById4Async_Tests_Initialize()
            {
                students = new List<Domain.Student.Entities.Student>() {
                    new Domain.Student.Entities.Student("0001234", "Smith", null, null, null, null)
                };
                restrictedStudents = new List<Domain.Student.Entities.Student>() {
                    new Domain.Student.Entities.Student("0001234", "Smith", null, null, null, "X")
                };
                privilegedStudents = new List<Domain.Student.Entities.Student>() {
                    new Domain.Student.Entities.Student("0001234", "Smith", null, null, null, "S")
                };
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepoMock.Setup(r => r.GetStudentsByIdAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<Term>(), It.IsAny<IEnumerable<CitizenshipStatus>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(students);
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepoMock.Setup(r => r.GetCitizenshipStatusesAsync(false)).ReturnsAsync(new List<CitizenshipStatus>());
                referenceDataRepository = referenceDataRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepoMock.Setup(r => r.Get(It.IsAny<string>())).Returns<string>((termId) => new TestTermRepository().Get(termId));
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                viewPersonInformationRole = new Role(109, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });

                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

                var studentBatch3DtoAdapter = new StudentEntityToStudentBatch3DtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.StudentBatch3>()).Returns(studentBatch3DtoAdapter);
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentIds = new List<string>() { currentUserFactory.CurrentUser.PersonId };

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void QueryStudentsById4Async_Tests_Cleanup()
            {
                studentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task QueryStudentsById4Async_User_without_ViewStudentInformation_Permission_throws_PermissionsException()
            {
                currentUserFactory = new ViewStudentUserFactory();
                viewPersonInformationRole = new Role(109, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                studentService = new StudentService(
                   adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                   studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                   currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                   programRepository, studentProgramRepository, transcriptGroupingRepository);

                var students = await studentService.QueryStudentsById4Async(studentIds);
                Assert.IsNotNull(students);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task QueryStudentsById4Async_User_without_ViewPersonInformation_Permission_throws_PermissionsException()
            {
                currentUserFactory = new ViewStudentUserFactory();
                viewPersonInformationRole = new Role(109, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                studentService = new StudentService(
                   adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                   studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                   currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var students = await studentService.QueryStudentsById4Async(studentIds);
            }

            [TestMethod]
            public async Task QueryStudentsById4Async_Valid()
            {
                currentUserFactory = new ViewStudentUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var result = await studentService.QueryStudentsById4Async(studentIds);
                Assert.AreEqual(students.Count, result.Dto.Count());
                Assert.AreEqual(students.First().Id, result.Dto.First().Id);
            }

            [TestMethod]
            public async Task QueryStudentsById4Async_Valid_StudentRepository_returns_null()
            {
                currentUserFactory = new ViewStudentUserFactory();
                studentRepoMock.Setup(r => r.GetStudentsByIdAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<Term>(), It.IsAny<IEnumerable<CitizenshipStatus>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var result = await studentService.QueryStudentsById4Async(studentIds);
                Assert.AreEqual(0, result.Dto.Count());
            }

            [TestMethod]
            public async Task QueryStudentsById4Async_Valid_StudentRepository_returns_empty_list()
            {
                currentUserFactory = new ViewStudentUserFactory();
                studentRepoMock.Setup(r => r.GetStudentsByIdAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<Term>(), It.IsAny<IEnumerable<CitizenshipStatus>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(new List<Domain.Student.Entities.Student>());
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var result = await studentService.QueryStudentsById4Async(studentIds);
                Assert.AreEqual(0, result.Dto.Count());
            }


            [TestMethod]
            public async Task QueryStudentsById4Async_Valid_student_has_privacy_code_and_user_does_not()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                studentRepoMock.Setup(r => r.GetStudentsByIdAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<Term>(), It.IsAny<IEnumerable<CitizenshipStatus>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(restrictedStudents);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                staffRepoMock.Setup(rpm => rpm.Get(It.IsAny<string>())).Returns(new Staff(currentUserFactory.CurrentUser.PersonId, "Smith") { PrivacyCodes = new List<string>() { "S" } });
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var result = await studentService.QueryStudentsById4Async(studentIds);
                Assert.AreEqual(1, result.Dto.Count());
                Assert.IsTrue(result.HasPrivacyRestrictions);
            }

            [TestMethod]
            public async Task QueryStudentsById4Async_Valid_student_has_privacy_code_and_user_can_see()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                studentRepoMock.Setup(r => r.GetStudentsByIdAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<Term>(), It.IsAny<IEnumerable<CitizenshipStatus>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).ReturnsAsync(privilegedStudents);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                staffRepoMock.Setup(rpm => rpm.Get(It.IsAny<string>())).Returns(new Staff(currentUserFactory.CurrentUser.PersonId, "Smith") { PrivacyCodes = new List<string>() { "S" } });
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var result = await studentService.QueryStudentsById4Async(studentIds);
                Assert.AreEqual(1, result.Dto.Count());
                Assert.IsFalse(result.HasPrivacyRestrictions);
            }
        }

        [TestClass]
        public class Search3Async_Tests : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

            public class ViewStudentUserFactory : ICurrentUserFactory
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
                            Roles = new List<string>() { "Faculty" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }

            public class ProxyStudentUserFactory : ICurrentUserFactory
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
                            Roles = new List<string>() { "ViewStudentInformation" },
                            SessionFixationId = "abc123",
                            ProxySubjectClaims = new ProxySubjectClaims()
                            {
                                PersonId = "0001234",
                            }
                        });
                    }
                }
            }

            [TestInitialize]
            public void Search3Async_Tests_Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepoMock.Setup(repo => repo.GetStudentsSearchAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Student>()
                {
                    new Domain.Student.Entities.Student(null, "0001212", "Smith", null, null, null)
                });
                studentRepoMock.Setup(repo => repo.GetStudentSearchByNameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<Domain.Student.Entities.Student>()
                {
                    new Domain.Student.Entities.Student(null, "0001212", "Smith", null, null, null)
                });
                studentRepo = studentRepoMock.Object;
                personRepoMock = new Mock<IPersonRepository>();
                personRepo = personRepoMock.Object;
                acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                acadCreditRepo = acadCreditRepoMock.Object;
                academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                academicHistoryService = academicHistoryServiceMock.Object;
                referenceDataRepoMock = new Mock<IReferenceDataRepository>();
                referenceDataRepoMock.Setup(r => r.GetCitizenshipStatusesAsync(false)).ReturnsAsync(new List<CitizenshipStatus>());
                referenceDataRepository = referenceDataRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepoMock.Setup(r => r.Get(It.IsAny<string>())).Returns<string>((termId) => new TestTermRepository().Get(termId));
                termRepo = termRepoMock.Object;
                regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                regPriorityRepo = regPriorityRepoMock.Object;
                studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepo = studentConfigurationRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.Student, Dtos.Student.Student>()).Returns(studentDtoAdapter);

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Search3Async_Tests_Cleanup()
            {
                studentService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task Search3Async_null_criteria_throws_exception()
            {
                var student = await studentService.Search3Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task Search3Async_criteria_StudentKeyword_is_Student_ID_invalid_permissions()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = "1234567" });
            }
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task Search3Async_criteria_StudentKeyword_WithOnlyViewPersonInformation_permission()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = "1234567" });
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task Search3Async_criteria_StudentKeyword_WithOnlyViewStudentInformation_permission()
            {
                currentUserFactory = new CurrentUserSetup.StudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = "1234567" });
            }


            [TestMethod]
            public async Task Search3Async_criteria_StudentKeyword_is_Student_ID_student_has_privacy_code_and_user_does_not()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                studentRepoMock.Setup(repo => repo.GetStudentsSearchAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Student>()
                {
                    new Domain.Student.Entities.Student(null, "0001212", "Smith", null, null, null, "X")
                });
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                staffRepoMock.Setup(rpm => rpm.Get(It.IsAny<string>())).Returns(new Staff(currentUserFactory.CurrentUser.PersonId, "Smith") { PrivacyCodes = new List<string>() { "S" } });
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = currentUserFactory.CurrentUser.PersonId });
                Assert.AreEqual(1, student.Dto.Count);
                Assert.IsTrue(student.HasPrivacyRestrictions);
            }

            [TestMethod]
            public async Task Search3Async_criteria_StudentKeyword_is_Student_ID_student_has_privacy_code_and_user_can_see()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                studentRepoMock.Setup(repo => repo.GetStudentsSearchAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<Domain.Student.Entities.Student>()
                {
                    new Domain.Student.Entities.Student(null, "0001212", "Smith", null, null, null, "S")
                });
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                staffRepoMock.Setup(rpm => rpm.Get(It.IsAny<string>())).Returns(new Staff(currentUserFactory.CurrentUser.PersonId, "Smith") { PrivacyCodes = new List<string>() { "S" } });
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = currentUserFactory.CurrentUser.PersonId });
                Assert.AreEqual(1, student.Dto.Count);
                Assert.IsFalse(student.HasPrivacyRestrictions);
            }

            [TestMethod]
            public async Task Search3Async_criteria_StudentKeyword_is_Student_ID_repo_returns_null()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                studentRepoMock.Setup(repo => repo.GetStudentsSearchAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(() => null);
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository,
                    baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = currentUserFactory.CurrentUser.PersonId });
                Assert.AreEqual(0, student.Dto.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task Search3Async_criteria_StudentKeyword_is_1_part_name_throws_exception()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = "John" });
            }

            [TestMethod]
            public async Task Search3Async_criteria_StudentKeyword_is_2_part_name()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = "Smith, John" });
                Assert.AreEqual(1, student.Dto.Count);
            }

            [TestMethod]
            public async Task Search3Async_criteria_StudentKeyword_is_name_student_has_privacy_code_and_user_can_see()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                studentRepoMock.Setup(repo => repo.GetStudentSearchByNameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<Domain.Student.Entities.Student>()
                {
                    new Domain.Student.Entities.Student("0001212", "Smith", null, null, null, "S")
                });
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                staffRepoMock.Setup(rpm => rpm.Get(It.IsAny<string>())).Returns(new Staff(currentUserFactory.CurrentUser.PersonId, "Smith") { PrivacyCodes = new List<string>() { "S" } });
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = "Smith, John" });
                Assert.AreEqual(1, student.Dto.Count);
                Assert.IsFalse(student.HasPrivacyRestrictions);
            }

            [TestMethod]
            public async Task Search3Async_criteria_StudentKeyword_is_name_student_has_privacy_code_and_user_does_not()
            {
                currentUserFactory = new ViewStudentUserFactory();
                Role viewPersonInformationRole = new Role(108, "Faculty");
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewPersonInformation));
                viewPersonInformationRole.AddPermission(new Permission(StudentPermissionCodes.ViewStudentInformation));
                studentRepoMock.Setup(repo => repo.GetStudentSearchByNameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<Domain.Student.Entities.Student>()
                {
                    new Domain.Student.Entities.Student("0001212", "Smith", null, null, null, "X")
                });
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewPersonInformationRole });
                staffRepoMock.Setup(rpm => rpm.Get(It.IsAny<string>())).Returns(new Staff(currentUserFactory.CurrentUser.PersonId, "Smith") { PrivacyCodes = new List<string>() { "S" } });
                studentService = new StudentService(
                    adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                    studentConfigurationRepo, referenceDataRepository, studentReferenceDataRepository, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);

                var student = await studentService.Search3Async(new Dtos.Student.StudentSearchCriteria() { StudentKeyword = "John Michael Smith" });
                Assert.AreEqual(1, student.Dto.Count);
                Assert.IsTrue(student.HasPrivacyRestrictions);
            }
        }

        [TestClass]
        public class PlanningStudentServiceTests
        {
            [TestClass]
            public class PlanningStudentServiceTests_GetPlanningStudent : CurrentUserSetup
            {
                private IAdapterRegistry adapterRegistry;
                private Mock<IAdapterRegistry> adapterRegistryMock;

                private IStudentRepository studentRepo;
                private Mock<IStudentRepository> studentRepoMock;

                private IPlanningStudentRepository planningStudentRepo;
                private Mock<IPlanningStudentRepository> planningStudentRepoMock;

                private IPersonRepository personRepo;
                private Mock<IPersonRepository> personRepoMock;

                private IAcademicCreditRepository acadCreditRepo;
                private Mock<IAcademicCreditRepository> acadCreditRepoMock;

                private ITermRepository termRepo;
                private Mock<ITermRepository> termRepoMock;

                private IRegistrationPriorityRepository regPriorityRepo;
                private Mock<IRegistrationPriorityRepository> regPriorityRepoMock;

                private IConfigurationRepository baseConfigurationRepository;
                private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

                private IStudentConfigurationRepository studentConfigurationRepo;
                private Mock<IStudentConfigurationRepository> studentConfigurationRepoMock;

                private IRoleRepository roleRepo;
                private Mock<IRoleRepository> roleRepoMock;

                private IStaffRepository staffRepo;
                private Mock<IStaffRepository> staffRepoMock;

                private Mock<ISectionRepository> sectionRepoMock;
                private ISectionRepository sectionRepo;
                private IProgramRepository programRepository;
                private Mock<IProgramRepository> programRepositoryMock;
                private IStudentProgramRepository studentProgramRepository;
                private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
                private ITranscriptGroupingRepository transcriptGroupingRepository;
                private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

                private IReferenceDataRepository referenceDataRepositoryRepo;
                private Mock<IReferenceDataRepository> referenceDataRepositoryRepoMock;
                private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

                private StudentService studentService;
                private IAcademicHistoryService academicHistoryService;
                private Mock<IAcademicHistoryService> academicHistoryServiceMock;

                private ICurrentUserFactory currentUserFactory;
                private ILogger logger;

                string studentId;
                PlanningStudent planningStudent;

                [TestInitialize]
                public void Initialize()
                {
                    logger = new Mock<ILogger>().Object;

                    adapterRegistryMock = new Mock<IAdapterRegistry>();
                    adapterRegistry = adapterRegistryMock.Object;

                    studentRepoMock = new Mock<IStudentRepository>();
                    studentRepo = studentRepoMock.Object;

                    planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                    planningStudentRepo = planningStudentRepoMock.Object;

                    personRepoMock = new Mock<IPersonRepository>();
                    personRepo = personRepoMock.Object;

                    acadCreditRepoMock = new Mock<IAcademicCreditRepository>();
                    acadCreditRepo = acadCreditRepoMock.Object;

                    termRepoMock = new Mock<ITermRepository>();
                    termRepo = termRepoMock.Object;

                    regPriorityRepoMock = new Mock<IRegistrationPriorityRepository>();
                    regPriorityRepo = regPriorityRepoMock.Object;

                    baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                    baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                    studentConfigurationRepoMock = new Mock<IStudentConfigurationRepository>();
                    studentConfigurationRepo = studentConfigurationRepoMock.Object;

                    roleRepoMock = new Mock<IRoleRepository>();
                    roleRepo = roleRepoMock.Object;

                    staffRepoMock = new Mock<IStaffRepository>();
                    staffRepo = staffRepoMock.Object;

                    sectionRepoMock = new Mock<ISectionRepository>();
                    sectionRepo = sectionRepoMock.Object;
                    programRepositoryMock = new Mock<IProgramRepository>();
                    programRepository = programRepositoryMock.Object;
                    studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                    studentProgramRepository = studentProgramRepositoryMock.Object;
                    transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                    transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

                    referenceDataRepositoryRepoMock = new Mock<IReferenceDataRepository>();
                    referenceDataRepositoryRepo = referenceDataRepositoryRepoMock.Object;
                    studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();

                    academicHistoryServiceMock = new Mock<IAcademicHistoryService>();
                    academicHistoryService = academicHistoryServiceMock.Object;

                    currentUserFactory = new StudentUserFactory();

                    var planningStudentAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Student.PlanningStudent>(adapterRegistry, logger);
                    adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Student.PlanningStudent>()).Returns(planningStudentAdapter);
                    var advisementAdapter = new AutoMapperAdapter<Domain.Student.Entities.Advisement, Dtos.Student.Advisement>(adapterRegistry, logger);
                    adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Advisement, Dtos.Student.Advisement>()).Returns(advisementAdapter);
                    var completedAdvisementAdapter = new AutoMapperAdapter<Domain.Student.Entities.CompletedAdvisement, Dtos.Student.CompletedAdvisement>(adapterRegistry, logger);
                    adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.CompletedAdvisement, Dtos.Student.CompletedAdvisement>()).Returns(completedAdvisementAdapter);

                    studentId = "0000894";
                    planningStudent = new Domain.Student.Entities.PlanningStudent(studentId, "brown", 3, new List<string>() { "ENGL.BA", "MATH.BS" });
                    // This will be the response regardless of the student requested--it's ok.
                    planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                    studentService = new StudentService(
                        adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                        studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                        baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                        programRepository, studentProgramRepository, transcriptGroupingRepository);
                }

                [TestMethod]
                public async Task UserIsSelf_ReturnsPlanningStudent()
                {
                    // Act
                    var planningStudentResponse = await studentService.GetPlanningStudentAsync(studentId);

                    // Assert - spot-check the response
                    Assert.AreEqual(planningStudent.Id, planningStudentResponse.Dto.Id);
                    Assert.AreEqual(planningStudent.LastName, planningStudentResponse.Dto.LastName);
                    Assert.AreEqual(planningStudent.ProgramIds.Count(), planningStudentResponse.Dto.ProgramIds.Count());
                }

                [TestMethod]
                public async Task PlanningStudentWithCompletedAdvisements()
                {
                    planningStudent.AddCompletedAdvisement("advisor1", DateTime.Today, DateTime.Now);
                    planningStudent.AddCompletedAdvisement("advisor2", DateTime.Today.AddDays(-1), DateTime.Now);
                    planningStudent.AddCompletedAdvisement("advisor3", DateTime.Today.AddDays(-2), DateTime.Now);
                    var planningStudentResponse = await studentService.GetPlanningStudentAsync(studentId);
                    Assert.AreEqual(planningStudentResponse.Dto.CompletedAdvisements.Count(), 3);
                    Assert.AreEqual(planningStudentResponse.Dto.CompletedAdvisements.FirstOrDefault().AdvisorId, "advisor1");
                    Assert.AreEqual(planningStudentResponse.Dto.CompletedAdvisements.FirstOrDefault().CompletionDate.ToShortDateString(), DateTime.Today.ToShortDateString());

                }

                [TestMethod]
                public async Task PlanningStudent_With_PrivacyStatusCode()
                {
                    planningStudent = new Domain.Student.Entities.PlanningStudent(studentId, "brown", 3, new List<string>() { "ENGL.BA", "MATH.BS" }, "S");
                    planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                    studentService = new StudentService(
                        adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                        studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                        baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                        programRepository, studentProgramRepository, transcriptGroupingRepository);

                    var planningStudentResponse = await studentService.GetPlanningStudentAsync(studentId);
                    Assert.AreEqual(planningStudent.PrivacyStatusCode, planningStudentResponse.Dto.PrivacyStatusCode);
                }

                [TestMethod]
                [ExpectedException(typeof(PermissionsException))]
                public async Task AdvisorWithViewAssignedAccess_NotAssigned_ThrowsException()
                {
                    // Arrange
                    currentUserFactory = new AdvisorUserFactory();

                    studentService = new StudentService(
                        adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                        studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                        baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                        programRepository, studentProgramRepository, transcriptGroupingRepository);

                    // Set up view assigned advisee permissions on advisor's role--so that advisor cannot access this student
                    advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                    roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                    // Act
                    var result = await studentService.GetPlanningStudentAsync("0000894");
                }

                [TestMethod]
                public async Task AdvisorWithViewAssignedAccess_AssignedToStudent_ReturnsResult()
                {
                    // Arrange
                    // In TestStudentRepository, Student 0004012 has advisor 0000111 (Id for advisor from AdvisorUserFactory) set as current advisor
                    studentId = "00004012";
                    planningStudent = new Domain.Student.Entities.PlanningStudent(studentId, "brown", 3, new List<string>() { "ENGL.BA", "MATH.BS" });
                    planningStudent.AddAdvisement("0000111", null, null, null);
                    planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                    // View Assigned advisees permissions
                    advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                    roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                    currentUserFactory = new AdvisorUserFactory();

                    studentService = new StudentService(
                        adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                        studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                        baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                        programRepository, studentProgramRepository, transcriptGroupingRepository);

                    // Act
                    var planningStudentResponse = await studentService.GetPlanningStudentAsync(studentId);

                    // Assert - spot-check the response
                    Assert.AreEqual(planningStudent.Id, planningStudentResponse.Dto.Id);
                    Assert.AreEqual(planningStudent.LastName, planningStudentResponse.Dto.LastName);
                    Assert.AreEqual(planningStudent.ProgramIds.Count(), planningStudentResponse.Dto.ProgramIds.Count());
                    Assert.AreEqual(planningStudent.AdvisorIds.ElementAt(0), planningStudentResponse.Dto.AdvisorIds.ElementAt(0));
                }

                [TestMethod]
                public async Task AdvisorWithViewAssignedAccess_AssignedToStudent_With_PrivacyCode()
                {
                    // Arrange
                    // In TestStudentRepository, Student 0004012 has advisor 0000111 (Id for advisor from AdvisorUserFactory) set as current advisor
                    studentId = "00004012";
                    planningStudent = new Domain.Student.Entities.PlanningStudent(studentId, "brown", 3, new List<string>() { "ENGL.BA", "MATH.BS" }, "S");
                    planningStudent.AddAdvisement("0000111", null, null, null);
                    planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                    // View Assigned advisees permissions
                    advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                    roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                    currentUserFactory = new AdvisorUserFactory();

                    studentService = new StudentService(
                        adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                        studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                        baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                        programRepository, studentProgramRepository, transcriptGroupingRepository);

                    // Act
                    var planningStudentResponse = await studentService.GetPlanningStudentAsync(studentId);

                    // Assert - spot-check the response
                    Assert.AreEqual(planningStudent.PrivacyStatusCode, planningStudentResponse.Dto.PrivacyStatusCode);
                }

                [TestMethod]
                public async Task AdvisorWithViewAssignedAccess_AssignedToStudent_With_StaffRecord_SamePrivacyCode()
                {
                    Domain.Base.Entities.Staff staff;
                    // Arrange
                    // In TestStudentRepository, Student 0004012 has advisor 0000111 (Id for advisor from AdvisorUserFactory) set as current advisor
                    studentId = "00004012";
                    planningStudent = new Domain.Student.Entities.PlanningStudent(studentId, "brown", 3, new List<string>() { "ENGL.BA", "MATH.BS" }, "S");
                    planningStudent.AddAdvisement("0000111", null, null, null);
                    planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                    //staff record for the advisor
                    staff = new Domain.Base.Entities.Staff("0000111", "staff member");
                    staff.IsActive = true;
                    staff.PrivacyCodes = new List<string>() { "S" };
                    staffRepoMock.Setup(r => r.Get("0000111")).Returns(staff);

                    // View Assigned advisees permissions
                    advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                    roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                    currentUserFactory = new AdvisorUserFactory();

                    studentService = new StudentService(
                        adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                        studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                        baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                        programRepository, studentProgramRepository, transcriptGroupingRepository);

                    // Act
                    var planningStudentResponse = await studentService.GetPlanningStudentAsync(studentId);

                    // Assert - spot-check the response
                    Assert.AreEqual(planningStudent.PrivacyStatusCode, planningStudentResponse.Dto.PrivacyStatusCode);
                    Assert.AreEqual(planningStudentResponse.Dto.Id, studentId);
                    Assert.AreEqual(planningStudentResponse.Dto.DegreePlanId, planningStudent.DegreePlanId);
                    Assert.IsNotNull(planningStudentResponse.Dto.DegreePlanId);
                    Assert.AreEqual(planningStudentResponse.Dto.LastName, planningStudent.LastName);
                    Assert.IsNotNull(planningStudentResponse.Dto.ProgramIds);
                    Assert.AreEqual(planningStudentResponse.Dto.ProgramIds.Count, planningStudent.ProgramIds.Count);
                }
                [TestMethod]
                public async Task AdvisorWithViewAssignedAccess_AssignedToStudent_With_StaffRecord_Different_PrivacyCode()
                {
                    Domain.Base.Entities.Staff staff;
                    // Arrange
                    // In TestStudentRepository, Student 0004012 has advisor 0000111 (Id for advisor from AdvisorUserFactory) set as current advisor
                    studentId = "00004012";
                    planningStudent = new Domain.Student.Entities.PlanningStudent(studentId, "brown", 3, new List<string>() { "ENGL.BA", "MATH.BS" }, "S");
                    planningStudent.AddAdvisement("0000111", null, null, null);
                    planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));
                    //staff record for the advisor
                    staff = new Domain.Base.Entities.Staff("0000111", "staff member");
                    staff.IsActive = true;
                    staff.PrivacyCodes = new List<string>() { "A" };
                    staffRepoMock.Setup(r => r.Get("0000111")).Returns(staff);

                    // View Assigned advisees permissions
                    advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
                    roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { advisorRole });

                    currentUserFactory = new AdvisorUserFactory();

                    studentService = new StudentService(
                        adapterRegistry, studentRepo, personRepo, acadCreditRepo, academicHistoryService, termRepo, regPriorityRepo,
                        studentConfigurationRepo, referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object,
                        baseConfigurationRepository, currentUserFactory, roleRepo, staffRepo, logger, planningStudentRepo, sectionRepo,
                        programRepository, studentProgramRepository, transcriptGroupingRepository);

                    // Act
                    var planningStudentResponse = await studentService.GetPlanningStudentAsync(studentId);

                    // Assert - spot-check the response
                    Assert.AreEqual(planningStudent.PrivacyStatusCode, planningStudentResponse.Dto.PrivacyStatusCode);
                    Assert.AreEqual(planningStudentResponse.Dto.Id, studentId);
                    Assert.IsNull(planningStudentResponse.Dto.DegreePlanId);
                    Assert.AreEqual(planningStudentResponse.Dto.LastName, planningStudent.LastName);
                    Assert.IsNull(planningStudentResponse.Dto.ProgramIds);
                }
            }
        }

        [TestClass]
        public class GetStudentAcademicLevelsTests : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private Mock<ILogger> loggerMock;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;
            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;


            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepoMock.Setup(repo => repo.GetStudentAcademicLevelsAsync("0000894")).ReturnsAsync(new List<Domain.Student.Entities.StudentAcademicLevel>()
                {
                    new Domain.Student.Entities.StudentAcademicLevel("UG","admitted","100","2012/fa",null,false,DateTime.Today.AddYears(-2),DateTime.Today.AddYears(-1)),
                    new Domain.Student.Entities.StudentAcademicLevel("GR","admitted","100","2012/fa",new List<string>(),false),
                    new Domain.Student.Entities.StudentAcademicLevel("CE","admitted","100","2012/fa",new List<string>(){"1","2" },false,DateTime.Today.AddYears(-2),DateTime.Today.AddYears(-1))

                });
                studentRepo = studentRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                var studentDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.StudentAcademicLevel, Dtos.Student.StudentAcademicLevel>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentAcademicLevel, Dtos.Student.StudentAcademicLevel>()).Returns(studentDtoAdapter);

                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                loggerMock = new Mock<ILogger>();
                logger = loggerMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;

                currentUserFactory = new CurrentUserSetup.StudentUserFactory();

                studentService = new StudentService(
                    adapterRegistry, studentRepo, null, null, null, null, null,
                    null, null, null, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepo, logger, null, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentService = null;
            }

            //student id is null or empty
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentAcademicLevelsTests_StudentIdNull()
            {
                await studentService.GetStudentAcademicLevelsAsync(null);
            }
            //student is not self
            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentAcademicLevelsTests_StudentIdIsNotSelf()
            {
                await studentService.GetStudentAcademicLevelsAsync("student-id-is-not-self");
            }
            //repository threw exception
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentAcademicLevelsTests_RepositoryThrewException()
            {
                studentRepoMock.Setup(repo => repo.GetStudentAcademicLevelsAsync("0000894")).Throws(new Exception("junk"));
                await studentService.GetStudentAcademicLevelsAsync("0000894");
            }
            //repository returns null academic levels
            [TestMethod]
            public async Task GetStudentAcademicLevelsTests_ReturnsNullCollection()
            {
                studentRepoMock.Setup(repo => repo.GetStudentAcademicLevelsAsync("0000894")).ReturnsAsync(() => null);
                var result = await studentService.GetStudentAcademicLevelsAsync("0000894");
                loggerMock.Verify(l => l.Warn("Repository call to retrieve student's academic levels returns null or empty entity"));
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());

            }
            //repository returns empty academic levels
            [TestMethod]
            public async Task GetStudentAcademicLevelsTests_ReturnsEmptyCollection()
            {
                studentRepoMock.Setup(repo => repo.GetStudentAcademicLevelsAsync("0000894")).ReturnsAsync(new List<StudentAcademicLevel>());
                var result = await studentService.GetStudentAcademicLevelsAsync("0000894");
                loggerMock.Verify(l => l.Warn("Repository call to retrieve student's academic levels returns null or empty entity"));
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count());

            }
            //adapter throws exception
            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentAcademicLevelsTests_AdapterThrowsException()
            {
                adapterRegistryMock.Setup(x => x.GetAdapter<Domain.Student.Entities.StudentAcademicLevel, Dtos.Student.StudentAcademicLevel>()).Throws(new Exception("adapter issue"));
                var result = await studentService.GetStudentAcademicLevelsAsync("0000894");
            }

            //happy path
            public async Task GetStudentAcademicLevelsTests_HappyPath()
            {

                var result = (await studentService.GetStudentAcademicLevelsAsync("0000894")).ToList();
                Assert.IsNotNull(result);
                Assert.AreEqual(3, result.Count);
                Assert.AreEqual("UG", result[0].AcademicLevel);
                Assert.AreEqual(null, result[0].AcademicCredits);

                Assert.AreEqual("GR", result[1].AcademicLevel);
                Assert.AreEqual(0, result[1].AcademicCredits);

                Assert.AreEqual("CE", result[2].AcademicLevel);
                Assert.AreEqual(2, result[2].AcademicCredits);
            }
        }

        [TestClass]
        public class UpdateStudents2Tests : CurrentUserSetup
        {
            private StudentService studentService;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
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

            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepo;

            private IProgramRepository programRepository;
            private Mock<IProgramRepository> programRepositoryMock;
            private IStudentProgramRepository studentProgramRepository;
            private Mock<IStudentProgramRepository> studentProgramRepositoryMock;
            private ITranscriptGroupingRepository transcriptGroupingRepository;
            private Mock<ITranscriptGroupingRepository> transcriptGroupingRepositoryMock;

            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Domain.Student.Entities.Student student1;
            private Domain.Student.Entities.Student student2;
            private IEnumerable<Domain.Student.Entities.Student> studentList;
            private IEnumerable<Domain.Student.Entities.Student> oneStudentList;
            private IEnumerable<Domain.Student.Entities.Student> twoStudentList;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<IStaffRepository> staffRepositoryMock;
            private IStaffRepository staffRepository;
            private ICurrentUserFactory currentUserFactory;


            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private IEnumerable<StudentCohort> cohorts;
            private IEnumerable<StudentType> studentTypes;
            private IEnumerable<AdmissionResidencyType> residencyTypes;
            private IEnumerable<AcademicLevel> academicLevels;
            private IEnumerable<StudentClassification> studentClassifications;
            private Permission permissionUpdateStudentInfo;
            private Ellucian.Colleague.Dtos.Students2 studentsDto;

            private string student1Guid = "6b227dcc-db1c-41a2-b809-8e400e5d0682";
            private string student2Guid = "b88342ca-03d3-4255-9d69-3dfd434c60ff";
            private string student1Id = "1234567";
            private string student2Id = "7654321";
            private string program1Guid = "cbac5aee-71e9-4f2d-ab44-3266d43390d4";
            private string program2Guid = "1f5d03d9-e3cb-43be-8ec9-dc606f5cf90f";
            private string academicCred1Guid = "911f1522-3fee-409e-a782-535f588a3419";
            private string academicCred2Guid = "4f2ead3b-210d-435c-a2c3-624e2683dbef";
            private string studentType1Guid = "8ce0fddc-b10d-4de6-885f-30d0aeaf9887";
            private string studentType1Code = "typ";
            private string studentType2Guid = "83b6a42b-4667-457b-93e8-8735ac4f6d3f";
            private string studentType2Code = "top";
            private string residency1Guid = "b4bcb3a0-2e8d-4643-bd17-ba93f36e8f09";
            private string residency1Code = "code1";
            private string residency2Guid = "bd54668d-50d9-416c-81e9-2318e88571a1";
            private string residency2Code = "code2";
            private string personFilter = "6b227dcc-db1c-41a2-b809-8e400e5d0682";            
            private string academicLevel1Guid = "6b227dcc-db1c-41a2-b809-8e400e5d0682";
            private string classification1Guid = "6b227dcc-db1c-41a2-b809-8e400e5d0682";



            [TestInitialize]
            public void Initialize()
            {
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
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
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                programRepositoryMock = new Mock<IProgramRepository>();
                programRepository = programRepositoryMock.Object;
                studentProgramRepositoryMock = new Mock<IStudentProgramRepository>();
                studentProgramRepository = studentProgramRepositoryMock.Object;
                transcriptGroupingRepositoryMock = new Mock<ITranscriptGroupingRepository>();
                transcriptGroupingRepository = transcriptGroupingRepositoryMock.Object;


                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                staffRepositoryMock = new Mock<IStaffRepository>();
                staffRepository = staffRepositoryMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;

                // setup students Dto object                
                studentsDto = new Dtos.Students2();
                studentsDto.Id = student1Guid;
                studentsDto.Person = new Dtos.GuidObject2(personFilter);
                studentsDto.Residencies = new List<Dtos.StudentResidenciesDtoProperty>()
                {  new Dtos.StudentResidenciesDtoProperty()
                    { Residency = new Dtos.GuidObject2(residency1Guid) } };
                studentsDto.Types = new List<Dtos.StudentTypesDtoProperty>()
                {  new Dtos.StudentTypesDtoProperty() { Type  =  new Dtos.GuidObject2(studentType1Guid) } };
                studentsDto.LevelClassifications = new List<Dtos.StudentLevelClassificationsDtoProperty>()
                {  new Dtos.StudentLevelClassificationsDtoProperty() { Level = new Dtos.GuidObject2(academicLevel1Guid) }};

                studentsDto.LevelClassifications[0].LatestClassification = new Dtos.GuidObject2(classification1Guid);
                
                //Mock roles repo and permission
                permissionUpdateStudentInfo = new Domain.Entities.Permission(StudentPermissionCodes.UpdateStudentInformation);
                viewStudentRole.AddPermission(permissionUpdateStudentInfo);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { viewStudentRole });

                // Mock the repo call for Student types
                studentTypes = new List<StudentType>()
                {
                    new StudentType(studentType1Guid, studentType1Code, "Test Data"),
                    new StudentType(studentType2Guid, studentType2Code, "Test Data")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetStudentTypesAsync(It.IsAny<bool>())).ReturnsAsync(studentTypes);

                // Mock the repo call for Academic level types
                academicLevels = new List<AcademicLevel>()
                {
                    new AcademicLevel(academicLevel1Guid, "test", "Test Data")               
                };

                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAcademicLevelsAsync(It.IsAny<bool>())).ReturnsAsync(academicLevels);
                                
                // Mock the repo call for Student Classification
                var studentClassifications = new List<StudentClassification>()
                {
                    new StudentClassification(classification1Guid, "test", "Test Data")
                };

                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAllStudentClassificationAsync(It.IsAny<bool>())).ReturnsAsync(studentClassifications);
                

                // Mock the repo call for residency types
                residencyTypes = new List<AdmissionResidencyType>()
                {
                    new AdmissionResidencyType(residency1Guid, residency1Code, "Test Data1"),
                    new AdmissionResidencyType(residency2Guid, residency2Guid, "Test Data2")
                };
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyTypes);

                //mock the call to get the personid
                personRepoMock.Setup(repo => repo.GetPersonIdFromGuidAsync(student1Guid)).ReturnsAsync(student1Id);

                //setup student entity data
                var studentResidency1 = new StudentResidency(residency1Code, new DateTime(2012, 05, 01));
                var studentResidencies1 = new List<StudentResidency> { studentResidency1 };
                student1 = new Domain.Student.Entities.Student(student1Guid, student1Id, new List<string>() { program1Guid, program2Guid }, new List<string>() { academicCred1Guid, academicCred2Guid }, "Boyd", false) { StudentResidencies = studentResidencies1 };

                var studentTypeInfo2 = new StudentTypeInfo(studentType1Code, new DateTime(2013, 06, 01));
                var studentTypeInfos2 = new List<StudentTypeInfo> { studentTypeInfo2 };
                student2 = new Domain.Student.Entities.Student(student2Guid, student2Id, new List<string>() { program1Guid, program2Guid }, new List<string>() { academicCred1Guid, academicCred2Guid }, "Boyd", false) { StudentTypeInfo = studentTypeInfos2 };


                studentList = new List<Domain.Student.Entities.Student>() { student1, student2 };

                oneStudentList = new List<Domain.Student.Entities.Student>() { student1 };

                twoStudentList = new List<Domain.Student.Entities.Student>() { student2 };


                // Mock student repo response

                studentRepoMock.Setup(repo => repo.GetStudentIdFromGuidAsync(student1Guid)).ReturnsAsync(student1Id);

                studentRepoMock.Setup(repo => repo.UpdateStudentAsync(It.IsAny<Domain.Student.Entities.Student>())).ReturnsAsync(student1);

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string[]>(), "",
                        It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(studentList, 2));

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string[]>(), student1Id,
                        It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(oneStudentList, 1));


                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string[]>(), "",
                        new List<string>() { studentType2Code }, It.IsAny<List<string>>())).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(twoStudentList, 2));

                studentRepoMock.Setup(
                    repo =>
                        repo.GetDataModelStudents2Async(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string[]>(), "",
                        It.IsAny<List<string>>(), new List<string>() { residency1Code })).ReturnsAsync(new Tuple<IEnumerable<Domain.Student.Entities.Student>, int>(oneStudentList, 2));

                // Set up current user
                currentUserFactory = new CurrentUserSetup.EthosUserFactory();

                studentService = new StudentService(adapterRegistry, studentRepo, personRepo, acadCreditRepo,
                    academicHistoryService, termRepo, regPriorityRepo, studentConfigurationRepo,
                    referenceDataRepositoryRepo, studentReferenceDataRepositoryMock.Object, baseConfigurationRepository,
                    currentUserFactory, roleRepo, staffRepository, logger, planningStudentRepo, sectionRepo,
                    programRepository, studentProgramRepository, transcriptGroupingRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                planningStudentRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateStudents2_Student_Null_Exception()
            {
                await studentService.UpdateStudents2Async(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateStudents2_StudentId_Null_Exception()
            {
                var student2Dto = new Dtos.Students2();
                student2Dto.Id = null;
                await studentService.UpdateStudents2Async(student2Dto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateStudents2_GetStudentIdFromGuidAsync_ReturnsNull()
            {
                studentRepoMock.Setup(repo => repo.GetStudentIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync((string)null);
                await studentService.UpdateStudents2Async(studentsDto);
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateStudents2_ConvertStudents2ToStudentsEntity_PersonId_NullException()
            {
                studentsDto = new Dtos.Students2();
                studentsDto.Id = student1Guid;
                studentsDto.Person = null;

                await studentService.UpdateStudents2Async(studentsDto);                
            }

            [TestMethod]
            [ExpectedException(typeof(IntegrationApiException))]
            public async Task UpdateStudents2_ConvertStudents2ToStudentsEntity_Residencies_NullOrEmptyException()
            {
                residencyTypes = new List<AdmissionResidencyType>();
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAdmissionResidencyTypesAsync(It.IsAny<bool>())).ReturnsAsync(residencyTypes);
                await studentService.UpdateStudents2Async(studentsDto);
            }

            [TestMethod]            
            public async Task UpdateStudents2_Success()
            {                
                var actual = await studentService.UpdateStudents2Async(studentsDto);
                Assert.IsNotNull(actual);
                Assert.AreSame(studentsDto.Id, actual.Id);
            }
        }
    }
}
