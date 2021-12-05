// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using Moq;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class SectionPermissionServiceTests
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
        public class GetSectionPermissions : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<ISectionPermissionRepository> sectionPermissionRepositoryMock;
            private ISectionPermissionRepository sectionPermissionRepository;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private IStudentReferenceDataRepository referenceDataRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private ISectionPermissionService sectionPermissionService;
            private SectionPermission sectionPermissionResponseData;
            private Section sectionData;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                sectionPermissionRepositoryMock = new Mock<ISectionPermissionRepository>();
                sectionPermissionRepository = sectionPermissionRepositoryMock.Object;

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                // Mock section response
                sectionData = new TestSectionRepository().GetAsync().Result.First();
                sectionData.AddFaculty("1111100");
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync("SEC1")).Returns(Task.FromResult(sectionData));

                // Mock Section Permission response
                sectionPermissionResponseData = BuildSectionPermissionRepositoryResponse();
                sectionPermissionRepositoryMock.Setup(repository => repository.GetSectionPermissionAsync(It.IsAny<string>())).Returns(Task.FromResult(sectionPermissionResponseData));

                // Mock Adapters
                var sectionPermissionEntitytoDtoAdapter = new SectionPermissionEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionPermission, Ellucian.Colleague.Dtos.Student.SectionPermission>()).Returns(sectionPermissionEntitytoDtoAdapter);
                var studentPetitionEntityToDtoAdapter = new StudentPetitionEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>()).Returns(studentPetitionEntityToDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                sectionPermissionService = new SectionPermissionService(adapterRegistry, sectionPermissionRepository, studentRepository, sectionRepository, referenceDataRepository, currentUserFactory, roleRepository, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                sectionPermissionRepository = null;
                studentRepository = null;
                sectionRepository = null;
                roleRepository = null;
                sectionPermissionService = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionPermissionAsync_ThrowsExceptionIfSectionStringNull()
            {
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
                var sectionPermissionDto = await sectionPermissionService.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetSectionPermissionAsync_ThrowsExceptionIfSectionStringEmpty()
            {
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
                var sectionPermissionDto = await sectionPermissionService.GetAsync(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetSectionPermissionAsync_RethrowsExceptionFromSectionRepository()
            {
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync(It.IsAny<string>())).Throws(new ApplicationException());
                var sectionPermissionDto = await sectionPermissionService.GetAsync("SEC1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetSectionPermissionAsync_ThrowsKeyNotFoundExceptionIfSectionNotFound()
            {
                Section nullSection = null;
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync(It.IsAny<string>())).Returns(Task.FromResult(nullSection));
                var sectionPermissionDto = await sectionPermissionService.GetAsync("SEC1");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetSectionPermissionAsync_ThrowsExceptionIfCurrentUserIsNotSectionFaculty()
            {
                var sectionPermissionDto = await sectionPermissionService.GetAsync("SEC1");
            }

            [TestMethod]
            public async Task GetSectionPermissionAsync_ReturnsPermissionsIfCurrentUserIsSectionFaculty()
            {
                // Add this faculty to the mocked section response
                sectionData.AddFaculty("0000011");
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync("SEC1")).Returns(Task.FromResult(sectionData));

                var sectionPermissionDto = await sectionPermissionService.GetAsync("SEC1");

                Assert.AreEqual(sectionPermissionResponseData.StudentPetitions.Count(), sectionPermissionDto.StudentPetitions.Count());
                Assert.AreEqual(sectionPermissionResponseData.FacultyConsents.Count(), sectionPermissionDto.FacultyConsents.Count());
                Assert.AreEqual(sectionPermissionResponseData.StudentPetitions.ElementAt(0).StudentId, sectionPermissionDto.StudentPetitions.ElementAt(0).StudentId);
                Assert.AreEqual(sectionPermissionResponseData.FacultyConsents.ElementAt(0).StudentId, sectionPermissionDto.FacultyConsents.ElementAt(0).StudentId);
                Assert.AreEqual(sectionPermissionResponseData.StudentPetitions.ElementAt(3).ReasonCode, sectionPermissionDto.StudentPetitions.ElementAt(3).ReasonCode);
                Assert.AreEqual(sectionPermissionResponseData.FacultyConsents.ElementAt(2).ReasonCode, sectionPermissionDto.FacultyConsents.ElementAt(2).ReasonCode);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetSectionPermissionAsync_ThrowsExceptionIfAdapterErrorOccurs()
            {
                // Add this faculty to the mocked section response
                sectionData.AddFaculty("0000011");
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync("SEC1")).Returns(Task.FromResult(sectionData));

                // Null adapter registry to force adapter error
                ITypeAdapter<Domain.Student.Entities.SectionPermission, Dtos.Student.SectionPermission> nullAdapter = null;
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.SectionPermission, Dtos.Student.SectionPermission>()).Returns(nullAdapter);
                sectionPermissionService = new SectionPermissionService(adapterRegistry, sectionPermissionRepository, studentRepository, sectionRepository, referenceDataRepository, currentUserFactory, roleRepository, logger);

                var sectionPermissionDto = await sectionPermissionService.GetAsync("SEC1");
            }

            [TestMethod]
            public async Task GetSectionPermissionAsync_ReturnsEmptyListIfRepositoryReturnsNullList()
            {
                // Add this faculty to the mocked section response
                sectionData.AddFaculty("0000011");
                sectionRepositoryMock.Setup(repository => repository.GetSectionAsync("SEC1")).Returns(Task.FromResult(sectionData)); ;

                // Mock empty response from section permission respository
                //Task<Dtos.Student.SectionPermission> nullResponse = null;
                SectionPermission response = null;
                sectionPermissionRepositoryMock.Setup(repository => repository.GetSectionPermissionAsync(It.IsAny<string>())).Returns(Task.FromResult(response));

                // Act
                var sectionPermissionDto = await sectionPermissionService.GetAsync("SEC1");

                // Assert
                Assert.IsTrue(sectionPermissionDto is Dtos.Student.SectionPermission);
                Assert.AreEqual(0, sectionPermissionDto.StudentPetitions.Count());
                Assert.AreEqual(0, sectionPermissionDto.FacultyConsents.Count());
            }

            private SectionPermission BuildSectionPermissionRepositoryResponse()
            {
                var sectionPermission = new SectionPermission("SEC1");

                var studentPetition1 = new StudentPetition(id: "1", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.StudentPetition, statusCode: "status") { ReasonCode = "OVHM" };
                sectionPermission.AddStudentPetition(studentPetition1);

                var studentPetition2 = new StudentPetition(id: "2", courseId: "ART-101", sectionId: "SEC1", studentId: "0000456", type: StudentPetitionType.StudentPetition, statusCode: "status") { Comment = "Student 456 ART-101 Petition comment.", ReasonCode = null };
                sectionPermission.AddStudentPetition(studentPetition2);

                var studentPetition3 = new StudentPetition(id: "3", courseId: "ART-101", sectionId: "SEC1", studentId: "0000111", type: StudentPetitionType.StudentPetition, statusCode: "status") { Comment = "Student 111 ART-101 Petition comment.", ReasonCode = string.Empty };
                sectionPermission.AddStudentPetition(studentPetition3);

                var studentPetition4 = new StudentPetition(id: "4", courseId: "ART-101", sectionId: "SEC1", studentId: "0000789", type: StudentPetitionType.StudentPetition, statusCode: "status") {Comment = "Student 789 ART-101 Petition comment. Line1 \ncomment line2\ncomment line3 the end", ReasonCode = "ICHI"};
                sectionPermission.AddStudentPetition(studentPetition4);

                var facultyConsent1 = new StudentPetition(id: "1", courseId: "ART-101", sectionId: "SEC1", studentId: "0000123", type: StudentPetitionType.FacultyConsent, statusCode: "status") {Comment =  null,  ReasonCode = "ICHI"};
                sectionPermission.AddFacultyConsent(facultyConsent1);

                var facultyConsent2 = new StudentPetition(id: "2", courseId: "ART-101", sectionId: "SEC1", studentId: "0000456", type: StudentPetitionType.FacultyConsent, statusCode: "status") {Comment = "Student 456 ART-101 Consent comment.",  ReasonCode = null};
                sectionPermission.AddFacultyConsent(facultyConsent2);

                var facultyConsent4 = new StudentPetition(id: "4", courseId: "ART-101", sectionId: "SEC1", studentId: "0000789", type: StudentPetitionType.FacultyConsent, statusCode: "status") { Comment = "Student 789 ART-101 Consent comment. Line1 \ncomment line2\ncomment line3 the end", ReasonCode = "OVHM" };
                sectionPermission.AddFacultyConsent(facultyConsent4);

                return sectionPermission;
            }

        }

        [TestClass]
        public class AddStudentPetition : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<ISectionPermissionRepository> sectionPermissionRepositoryMock;
            private ISectionPermissionRepository sectionPermissionRepository;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private IStudentReferenceDataRepository referenceDataRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private ISectionPermissionService sectionPermissionService;
            private Dtos.Student.StudentPetition badPetitionDto;
            private Dtos.Student.StudentPetition goodPetitionDto;
            private Dtos.Student.StudentPetition goodConsentDto;
            private Domain.Student.Entities.StudentPetition studentPetitionEntityAdded;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                sectionPermissionRepositoryMock = new Mock<ISectionPermissionRepository>();
                sectionPermissionRepository = sectionPermissionRepositoryMock.Object;

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                goodPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "sectionId",
                    StudentId = "studentId",
                    StatusCode = "A",
                    ReasonCode = "REASON",
                    Comment = "MultiLineComment/nLine2",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };

                goodConsentDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "sectionId",
                    StudentId = "studentId",
                    StatusCode = "A",
                    ReasonCode = "REASON",
                    Comment = "MultiLineComment/nLine2",
                    Type = Dtos.Student.StudentPetitionType.FacultyConsent
                };
                // Set up reference data repository value for Reasons and statuses
                var petitionReasons = BuildPetitionReasons();
                referenceDataRepositoryMock.Setup(x => x.GetStudentPetitionReasonsAsync()).Returns(Task.FromResult(petitionReasons.AsEnumerable()));
                var statusCodes = BuildStatusCodes();
                referenceDataRepositoryMock.Setup(x => x.GetPetitionStatusesAsync()).Returns(Task.FromResult(statusCodes.AsEnumerable()));

                // Mock Section Permission response
                studentPetitionEntityAdded = new StudentPetition("1", "courseId", "sectionId", "studentId", StudentPetitionType.FacultyConsent, "A") { Comment = "MultiLineComment/nLine2", ReasonCode = "REASON" };
                sectionPermissionRepositoryMock.Setup(repository => repository.AddStudentPetitionAsync(studentPetitionEntityAdded)).Returns(Task.FromResult(studentPetitionEntityAdded));

                // Mock Adapters
                var studentPetitionDtoToEntityAdapter = new StudentPetitionDtoToEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentPetition, Ellucian.Colleague.Domain.Student.Entities.StudentPetition>()).Returns(studentPetitionDtoToEntityAdapter);
                var studentPetitionEntityToDtoAdapter = new StudentPetitionEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>()).Returns(studentPetitionEntityToDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                sectionPermissionService = new SectionPermissionService(adapterRegistry, sectionPermissionRepository, studentRepository, sectionRepository, referenceDataRepository, currentUserFactory, roleRepository, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                sectionPermissionRepository = null;
                studentRepository = null;
                sectionRepository = null;
                roleRepository = null;
                sectionPermissionService = null;
                badPetitionDto = null;
                goodPetitionDto = null;
                currentUserFactory = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task AddStudentPetition_ThrowsExceptionIfStudentPetitionNull()
            {
                //sectionRepositoryMock.Setup(repository => repository.GetSectionAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AddStudentPetition_ThrowsExceptionIfUserWithoutCreateStudentPetitionPermission()
            {

                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                await sectionPermissionService.AddStudentPetitionAsync(goodPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task AddStudentPetition_ThrowsExceptionIfUserWithoutCreateFacultyConsentPermission()
            {

                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                await sectionPermissionService.AddStudentPetitionAsync(goodConsentDto);
            }
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AddStudentPetition_ThrowsExceptionIfProvidedPetitionHasNoSectionId()
            {

                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateFacultyConsent));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = null,
                    StudentId = "12345",
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.FacultyConsent
                };

                var sectionPermissionDto =await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AddStudentPetitionc_ThrowsExceptionIfProvidedPetitionHasEmptySectionId()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = string.Empty,
                    StudentId = "12345",
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };

                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AddStudentPetitionc_ThrowsExceptionIfProvidedPetitionHasNoStudentId()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = null,
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };

                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AddStudentPetition_ThrowsArgumentExceptionIfStatusCodeNull()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = null,
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AddStudentPetition_ThrowsArgumentExceptionIfStatusCodeEmpty()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = string.Empty,
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AddStudentPetition_ThrowsArgumentExceptionIfStatusCodeInvalid()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = "JUNK",
                    ReasonCode = "REASON",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AddStudentPetition_ThrowsArgumentExceptionIfPetitionReasonInvalid()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = "A",
                    ReasonCode = "JUNK",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task AddStudentPetition_ThrowsExceptionIfNoReasonOrComment()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task AddStudentPetition_ThrowsExceptionIfAdapterErrorOccurs()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var newPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = "A",
                    ReasonCode = "REASON",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };

                // Null adapter registry to force adapter error
                ITypeAdapter<Dtos.Student.StudentPetition, Domain.Student.Entities.StudentPetition> nullAdapter = null;
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.StudentPetition, Domain.Student.Entities.StudentPetition>()).Returns(nullAdapter);
                sectionPermissionService = new SectionPermissionService(adapterRegistry, sectionPermissionRepository, studentRepository, sectionRepository, referenceDataRepository, currentUserFactory, roleRepository, logger);

                var sectionPermissionDto = await sectionPermissionService.AddStudentPetitionAsync(newPetitionDto);
            }

            [TestMethod]
            public async Task AddStudentPetition_SuccessfulAdd()
            {

                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });

                var addedPetitionDto = await sectionPermissionService.AddStudentPetitionAsync(goodPetitionDto);
                Assert.AreEqual(goodPetitionDto.StudentId, addedPetitionDto.StudentId);
                Assert.AreEqual(goodPetitionDto.SectionId, addedPetitionDto.SectionId);
                Assert.AreEqual(goodPetitionDto.StatusCode, addedPetitionDto.StatusCode);
                Assert.AreEqual(goodPetitionDto.ReasonCode, addedPetitionDto.ReasonCode);
                Assert.AreEqual(goodPetitionDto.Comment, addedPetitionDto.Comment);
            }

            private List<PetitionStatus> BuildStatusCodes()
            {
                var results = new List<PetitionStatus>();
                results.Add(new PetitionStatus("A", "Accepted", true));
                results.Add(new PetitionStatus("D", "Denied", false));
                return results;
            }

            private List<StudentPetitionReason> BuildPetitionReasons()
            {
                var results = new List<StudentPetitionReason>();
                results.Add(new StudentPetitionReason("REASON", "Audition"));
                results.Add(new StudentPetitionReason("NO", "No Audition"));
                return results;
            }

        }

        [TestClass]
        public class GetStudentPetition : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private Mock<ISectionPermissionRepository> sectionPermissionRepoMock;
            private ISectionPermissionRepository sectionPermissionRepository;
            private Mock<ISectionRepository> sectionRepoMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private IStudentReferenceDataRepository referenceDataRepository;
            private ISectionPermissionService sectionPermissionService;
            private Section section;
            private StudentPetition petitionResponse;
            private string studentId;
            private string petitionId;
            private Dtos.Student.StudentPetitionType type;
            private string sectionId;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                sectionPermissionRepoMock = new Mock<ISectionPermissionRepository>();
                sectionPermissionRepository = sectionPermissionRepoMock.Object;
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepoMock.Object;
                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;
                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;
                petitionId = "3";
                studentId = "student1";
                type = Dtos.Student.StudentPetitionType.StudentPetition;
                sectionId = "SEC1";

                // Mock section response
                section = new TestSectionRepository().GetAsync().Result.First();
                section.AddFaculty("1111100");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1")).Returns(Task.FromResult(section));

                // Mock Section  Permissions response
                petitionResponse = BuildSectionPermissionRepoResponse();
                sectionPermissionRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), StudentPetitionType.StudentPetition)).Returns(Task.FromResult(petitionResponse));

                // Mock Adapters
                var sectionPermissionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionPermission, Ellucian.Colleague.Dtos.Student.SectionPermission>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.SectionPermission, Ellucian.Colleague.Dtos.Student.SectionPermission>()).Returns(sectionPermissionDtoAdapter);
                var studentPetitionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>()).Returns(studentPetitionDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                sectionPermissionService = new SectionPermissionService(adapterRegistry, sectionPermissionRepository, studentRepository, sectionRepository, referenceDataRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                sectionPermissionService = null;
                sectionPermissionRepository = null;
                sectionRepository = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPetition_ThrowsExceptionIfpetitionIdNull()
            {
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(null, sectionId, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPetition_ThrowsExceptionIfpetitionIdEmpty()
            {
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(string.Empty, sectionId, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPetition_ThrowsExceptionIfSectionIdNull()
            {
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, null, type);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetStudentPetition_ThrowsExceptionIfSectionIdEmpty()
            {
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, string.Empty, type);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetStudentPetition_ThrowsKeyNotFoundExceptionIfThrownByRepositoryGet()
            {
                sectionPermissionRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StudentPetitionType>())).Throws(new KeyNotFoundException());
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, sectionId, type);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentPetition_ThrowsExceptionIfOtherExceptionThrownByRepositoryGet()
            {
                sectionPermissionRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StudentPetitionType>())).Throws(new Exception());
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, sectionId, type);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentPetition_RethrowsExceptionFromSectionRepository()
            {
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).Throws(new ApplicationException());
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, sectionId, type);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentPetition_ThrowsExceptionIfSectionNotFound()
            {
                Section nullSection = null;
                sectionRepoMock.Setup(repo => repo.GetSectionAsync(It.IsAny<string>())).Returns(Task.FromResult(nullSection));
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, sectionId, type);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task GetStudentPetition_ThrowsPermissionsExceptionIfCurrentUserIsNotSectionFaculty()
            {
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, sectionId, type);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task GetStudentPetition_ThrowsExceptionIfsectionPermissionRepoThrowsException()
            {
                sectionPermissionRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), StudentPetitionType.StudentPetition)).Throws(new ArgumentNullException());
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, sectionId, type);
            }

            [TestMethod]
            public async Task GetStudentPetition_ReturnsPetition()
            {
                // Add this faculty to the mocked section response
                section.AddFaculty("0000011");
                sectionRepoMock.Setup(repo => repo.GetSectionAsync("SEC1")).Returns(Task.FromResult(section));

                // Act
                var petitionDto = await sectionPermissionService.GetStudentPetitionAsync(petitionId, sectionId, type);

                // Spot-check resulting dto
                Assert.AreEqual(petitionResponse.StudentId, petitionDto.StudentId);
                Assert.AreEqual(petitionResponse.SectionId, petitionDto.SectionId);
                Assert.AreEqual(petitionResponse.Comment, petitionDto.Comment);
                Assert.AreEqual(petitionResponse.DateTimeChanged, petitionDto.DateTimeChanged);
                Assert.AreEqual(petitionResponse.UpdatedBy, petitionDto.UpdatedBy);
                Assert.AreEqual(petitionResponse.StatusCode, petitionDto.StatusCode);
                Assert.AreEqual(petitionResponse.ReasonCode, petitionDto.ReasonCode);
                Assert.AreEqual(petitionResponse.Type.ToString(), petitionDto.Type.ToString());
            }


            private StudentPetition BuildSectionPermissionRepoResponse()
            {
                var petition1 = new StudentPetition(petitionId, "COURSE1", sectionId, studentId, StudentPetitionType.StudentPetition, "STATUS");
                petition1.Comment = "Comments";
                petition1.TermCode = "TERM1";
                petition1.ReasonCode = "REASON";
                petition1.DateTimeChanged = DateTime.Now.AddHours(-1);
                petition1.EndDate = DateTime.Today.AddDays(-1);
                petition1.StartDate = DateTime.Today.AddDays(-5);
                petition1.UpdatedBy = "UpdateBy";
                return petition1;
            }
        }

        [TestClass]
        public class UpdateStudentPetition : CurrentUserSetup
        {
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<IRoleRepository> roleRepositoryMock;
            private IRoleRepository roleRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<ISectionPermissionRepository> sectionPermissionRepositoryMock;
            private ISectionPermissionRepository sectionPermissionRepository;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private ISectionRepository sectionRepository;
            private Mock<IStudentRepository> studentRepositoryMock;
            private IStudentRepository studentRepository;
            private IStudentReferenceDataRepository referenceDataRepository;
            private Mock<IStudentReferenceDataRepository> referenceDataRepositoryMock;
            private ISectionPermissionService sectionPermissionService;
            private Dtos.Student.StudentPetition badPetitionDto;
            private Dtos.Student.StudentPetition goodPetitionDto;
            private Dtos.Student.StudentPetition goodConsentDto;
            private Domain.Student.Entities.StudentPetition studentPetitionEntityUpdated;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepositoryMock = new Mock<IRoleRepository>();
                roleRepository = roleRepositoryMock.Object;

                sectionPermissionRepositoryMock = new Mock<ISectionPermissionRepository>();
                sectionPermissionRepository = sectionPermissionRepositoryMock.Object;

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                studentRepositoryMock = new Mock<IStudentRepository>();
                studentRepository = studentRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                logger = new Mock<ILogger>().Object;

                goodPetitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "sectionId",
                    StudentId = "studentId",
                    StatusCode = "A",
                    ReasonCode = "REASON",
                    Comment = "MultiLineComment/nLine2",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };

                goodConsentDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "sectionId",
                    StudentId = "studentId",
                    StatusCode = "A",
                    ReasonCode = "REASON",
                    Comment = "MultiLineComment/nLine2",
                    Type = Dtos.Student.StudentPetitionType.FacultyConsent
                };
                // Set up reference data repository value for Reasons and statuses
                var petitionReasons = BuildPetitionReasons();
                referenceDataRepositoryMock.Setup(x => x.GetStudentPetitionReasonsAsync()).Returns(Task.FromResult(petitionReasons.AsEnumerable()));
                var statusCodes = BuildStatusCodes();
                referenceDataRepositoryMock.Setup(x => x.GetPetitionStatusesAsync()).Returns(Task.FromResult(statusCodes.AsEnumerable()));

                // Mock Section Permission response
                studentPetitionEntityUpdated = new StudentPetition("1", "courseId", "sectionId", "studentId", StudentPetitionType.FacultyConsent, "A") { Comment = "MultiLineComment/nLine2", ReasonCode = "REASON" };
                sectionPermissionRepositoryMock.Setup(repository => repository.UpdateStudentPetitionAsync(studentPetitionEntityUpdated)).Returns(Task.FromResult(studentPetitionEntityUpdated));

                // Mock Adapters
                var studentPetitionDtoToEntityAdapter = new StudentPetitionDtoToEntityAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Dtos.Student.StudentPetition, Ellucian.Colleague.Domain.Student.Entities.StudentPetition>()).Returns(studentPetitionDtoToEntityAdapter);
                var studentPetitionEntityToDtoAdapter = new StudentPetitionEntityToDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentPetition, Ellucian.Colleague.Dtos.Student.StudentPetition>()).Returns(studentPetitionEntityToDtoAdapter);
                // Set up current user
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();

                sectionPermissionService = new SectionPermissionService(adapterRegistry, sectionPermissionRepository, studentRepository, sectionRepository, referenceDataRepository, currentUserFactory, roleRepository, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                sectionPermissionRepository = null;
                studentRepository = null;
                sectionRepository = null;
                roleRepository = null;
                sectionPermissionService = null;
                badPetitionDto = null;
                goodPetitionDto = null;
                currentUserFactory = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateStudentPetition_ThrowsExceptionIfStudentPetitionNull()
            {
                //sectionRepositoryMock.Setup(repository => repository.GetSectionAsync(It.IsAny<string>())).Throws(new ArgumentNullException());
                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateStudentPetition_ThrowsExceptionIfUserWithoutCreateStudentPetitionPermission()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                await sectionPermissionService.UpdateStudentPetitionAsync(goodPetitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task UpdateStudentPetition_ThrowsExceptionIfUserWithoutCreateFacultyConsentPermission()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                await sectionPermissionService.UpdateStudentPetitionAsync(goodConsentDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentPetition_ThrowsExceptionIfProvidedPetitionHasNoSectionId()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateFacultyConsent));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = null,
                    StudentId = "12345",
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.FacultyConsent
                };

                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentPetitionc_ThrowsExceptionIfProvidedPetitionHasEmptySectionId()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = string.Empty,
                    StudentId = "12345",
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };

                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentPetitionc_ThrowsExceptionIfProvidedPetitionHasNoStudentId()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = null,
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };

                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentPetition_ThrowsArgumentExceptionIfStatusCodeNull()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = null,
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentPetition_ThrowsArgumentExceptionIfStatusCodeEmpty()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = string.Empty,
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdateStudentPetition_ThrowsArgumentExceptionIfStatusCodeInvalid()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = "JUNK",
                    ReasonCode = "REASON",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdateStudentPetition_ThrowsArgumentExceptionIfPetitionReasonInvalid()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = "A",
                    ReasonCode = "JUNK",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task UpdateStudentPetition_ThrowsExceptionIfNoReasonOrComment()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = "A",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };
                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task UpdateStudentPetition_ThrowsExceptionIfAdapterErrorOccurs()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });
                var petitionDto = new Dtos.Student.StudentPetition()
                {
                    SectionId = "12345",
                    StudentId = "456",
                    StatusCode = "A",
                    ReasonCode = "REASON",
                    Type = Dtos.Student.StudentPetitionType.StudentPetition
                };

                // Null adapter registry to force adapter error
                ITypeAdapter<Dtos.Student.StudentPetition, Domain.Student.Entities.StudentPetition> nullAdapter = null;
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.StudentPetition, Domain.Student.Entities.StudentPetition>()).Returns(nullAdapter);
                sectionPermissionService = new SectionPermissionService(adapterRegistry, sectionPermissionRepository, studentRepository, sectionRepository, referenceDataRepository, currentUserFactory, roleRepository, logger);

                var sectionPermissionDto = await sectionPermissionService.UpdateStudentPetitionAsync(petitionDto);
            }

            [TestMethod]
            public async Task UpdateStudentPetition_Successful()
            {
                // Set up faculty as the current user.
                currentUserFactory = new CurrentUserSetup.FacultyUserFactory();
                facultyRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.CreateStudentPetition));
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { facultyRole });

                var updatedPetitionDto = await sectionPermissionService.UpdateStudentPetitionAsync(goodPetitionDto);
                Assert.AreEqual(goodPetitionDto.StudentId, updatedPetitionDto.StudentId);
                Assert.AreEqual(goodPetitionDto.SectionId, updatedPetitionDto.SectionId);
                Assert.AreEqual(goodPetitionDto.StatusCode, updatedPetitionDto.StatusCode);
                Assert.AreEqual(goodPetitionDto.ReasonCode, updatedPetitionDto.ReasonCode);
                Assert.AreEqual(goodPetitionDto.Comment, updatedPetitionDto.Comment);
            }

            private List<PetitionStatus> BuildStatusCodes()
            {
                var results = new List<PetitionStatus>();
                results.Add(new PetitionStatus("A", "Accepted", true));
                results.Add(new PetitionStatus("D", "Denied", false));
                return results;
            }

            private List<StudentPetitionReason> BuildPetitionReasons()
            {
                var results = new List<StudentPetitionReason>();
                results.Add(new StudentPetitionReason("REASON", "Audition"));
                results.Add(new StudentPetitionReason("NO", "No Audition"));
                return results;
            }

        }
    }
}
