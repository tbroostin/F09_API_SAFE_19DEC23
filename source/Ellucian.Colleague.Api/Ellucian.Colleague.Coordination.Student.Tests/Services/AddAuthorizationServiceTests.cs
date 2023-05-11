// Copyright 2018-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Coordination.Student.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Tests.Services
{
    [TestClass]
    public class AddAuthorizationServiceTests_UpdateAddAuthorizationAsync : CurrentUserFactorySetup
    {
        private AddAuthorizationService _addAuthorizationService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ICurrentUserFactory _currentUserFactory;
        private IRoleRepository _roleRepository;
        private ILogger _logger;
        private Mock<IAddAuthorizationRepository> addAuthorizationRepoMock;
        private IAddAuthorizationRepository _addAuthorizationRepo;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository _sectionRepo;
        private IReferenceDataRepository _referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Dtos.Student.AddAuthorization authorizationToUpdate;
        private Ellucian.Colleague.Domain.Student.Entities.AddAuthorization authorizationEntity;
        private Domain.Student.Entities.Section section;


        [TestInitialize]
        public void Initialize()
        {
            sectionRepoMock = new Mock<ISectionRepository>();
            _sectionRepo = sectionRepoMock.Object;
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _referenceDataRepository = referenceDataRepositoryMock.Object;
            addAuthorizationRepoMock = new Mock<IAddAuthorizationRepository>();
            _addAuthorizationRepo = addAuthorizationRepoMock.Object;
            var _roleRepositoryMock = new Mock<IRoleRepository>();
            var facultyRole = new Domain.Entities.Role(1, "Faculty");
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { facultyRole });
            _roleRepository = _roleRepositoryMock.Object;
            _logger = new Mock<ILogger>().Object;
            _currentUserFactory = new CurrentUserFactorySetup.StudentUserFactory();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                Id = "AddAuth1",
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "studentId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),
                IsRevoked = true,
                RevokedBy = "otherId",
                RevokedTime = DateTime.Now

            };

            authorizationEntity = new Domain.Student.Entities.AddAuthorization(authorizationToUpdate.Id, authorizationToUpdate.SectionId)
            {
                AddAuthorizationCode = authorizationToUpdate.AddAuthorizationCode,
                StudentId = authorizationToUpdate.StudentId,
                AssignedBy = authorizationToUpdate.AssignedBy,
                AssignedTime = authorizationToUpdate.AssignedTime,
                IsRevoked = authorizationToUpdate.IsRevoked,
                RevokedBy = authorizationToUpdate.RevokedBy,
                RevokedTime = authorizationToUpdate.RevokedTime
            };

            section = new Domain.Student.Entities.Section("sectionId", "courseId", "01", new DateTime(2018, 01, 01), 3m, null, "Section Title", "I", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") }, new List<string>() { "l1" }, "UG", new List<SectionStatusItem>(), true, true, false, true, false, false, false);
            section.AddFaculty("facultyId");

            //Register adapters

            // var AddAuthorizationDtoToEntityAdapter = new AddAuthorizationDtoToEntityAdapter(_adapterRegistry, _logger);
            var addAuthorizationDtoToEntityAdapter = new Student.Adapters.AddAuthorizationDtoToEntityAdapter(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.AddAuthorization, Domain.Student.Entities.AddAuthorization>()).Returns(addAuthorizationDtoToEntityAdapter);

            var AddAuthorizationEntityToDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>()).Returns(AddAuthorizationEntityToDtoAdapter);

            // Mock return from the repo on update
            addAuthorizationRepoMock.Setup(repo => repo.UpdateAddAuthorizationAsync(It.IsAny<AddAuthorization>())).ReturnsAsync(authorizationEntity);

            // Mock return from repo for GetAsync
            addAuthorizationRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(authorizationEntity);

            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, null, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _addAuthorizationService = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _currentUserFactory = null;
            _roleRepository = null;
            _logger = null;
            addAuthorizationRepoMock = null;
            _addAuthorizationRepo = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateAddAuthorizationAsync_NullArgument()
        {
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(null);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateAddAuthorizationAsync_InvalidArgument_NullSectionId()
        {
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(new Dtos.Student.AddAuthorization() { Id = "Id" });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateAddAuthorizationAsync_InvalidArgument_EmptyMissingSectionId()
        {
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(new Dtos.Student.AddAuthorization() { Id = "Id", SectionId = string.Empty });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateAddAuthorizationAsync_InvalidArgument_NoIdNoAddCode()
        {
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(new Dtos.Student.AddAuthorization() { SectionId = "SectionId", StudentId = "StudentId" });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UpdateAddAuthorizationAsync_InvalidArgument_NoIdNoStudentId()
        {
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(new Dtos.Student.AddAuthorization() { SectionId = "SectionId", AddAuthorizationCode = "AddCode" });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task NotStudent_UpdateAddAuthorizationAsync_SectionNullFromRepo()
        {
            // Advisor is current user (not faculty on the section.)
            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            // Section will come back null from section repo
            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                Id = "AddAuth1",
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "OtherId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),
                IsRevoked = true,
                RevokedBy = "otherId",
                RevokedTime = DateTime.Now

            };
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task NotStudentOrFaculty_UpdateAddAuthorizationAsync()
        {
            // Advisor is current user (not faculty on the section.)
            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            var sectionEntities = new List<Domain.Student.Entities.Section>() { section };
            sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).ReturnsAsync(sectionEntities);
            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                Id = "AddAuth1",
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "OtherId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),
                IsRevoked = true,
                RevokedBy = "otherId",
                RevokedTime = DateTime.Now

            };
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, null, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task Student_AssignAddAuthorizationAsync_NullRepoResponse()
        {
            //

            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                Id = "",
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "StudentId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),
                IsRevoked = true,
                RevokedBy = "otherId",
                RevokedTime = DateTime.Now

            };

            AddAuthorization addAuthorizationEntity = null;
            addAuthorizationRepoMock.Setup(x => x.GetAddAuthorizationByAddCodeAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(addAuthorizationEntity);

            // Student is current user
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);

 
        }

        [TestMethod]
        public async Task Student_AssignAddAuthorizationAsync_Success()
        {
            //

            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "studentId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),
                IsRevoked = true,
                RevokedBy = "otherId",
                RevokedTime = DateTime.Now

            };
            // Student is null Ok to update
            AddAuthorization addAuthorizationEntity = new AddAuthorization("CorrectId", authorizationToUpdate.SectionId)
            {
                AddAuthorizationCode = "abCD1234",
            };


            addAuthorizationRepoMock.Setup(x => x.GetAddAuthorizationByAddCodeAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(addAuthorizationEntity);
            // Mock return from the repo on update
            var newAuthorizationEntity = new Domain.Student.Entities.AddAuthorization("CorrectId", authorizationToUpdate.SectionId)
            {
                AddAuthorizationCode = authorizationToUpdate.AddAuthorizationCode,
                StudentId = authorizationToUpdate.StudentId,
                AssignedBy = authorizationToUpdate.AssignedBy,
                AssignedTime = authorizationToUpdate.AssignedTime,
                IsRevoked = authorizationToUpdate.IsRevoked,
                RevokedBy = authorizationToUpdate.RevokedBy,
                RevokedTime = authorizationToUpdate.RevokedTime
            };
            addAuthorizationRepoMock.Setup(repo => repo.UpdateAddAuthorizationAsync(It.IsAny<AddAuthorization>())).ReturnsAsync(newAuthorizationEntity);

            // Student is current user
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);

            Assert.AreEqual("CorrectId", serviceResult.Id);
            Assert.AreEqual(authorizationToUpdate.SectionId, serviceResult.SectionId);
            Assert.AreEqual(authorizationToUpdate.AddAuthorizationCode, serviceResult.AddAuthorizationCode);
            Assert.AreEqual(authorizationToUpdate.IsRevoked, serviceResult.IsRevoked);
            Assert.AreEqual(authorizationToUpdate.RevokedBy, serviceResult.RevokedBy);
            Assert.AreEqual(authorizationToUpdate.RevokedTime, serviceResult.RevokedTime);
            Assert.AreEqual(authorizationToUpdate.StudentId, serviceResult.StudentId);
            Assert.AreEqual(authorizationToUpdate.AssignedBy, serviceResult.AssignedBy);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Student_AssignAddAuthorizationAsync_AlreadyAssigned()
        {
            //

            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                Id = "",
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "studentId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),
                IsRevoked = true,
                RevokedBy = "otherId",
                RevokedTime = DateTime.Now

            };

            // Student is another - update not allowed
            AddAuthorization addAuthorizationEntity = new AddAuthorization("BadId", authorizationToUpdate.SectionId) { StudentId = "AnotherStudent" };

            addAuthorizationRepoMock.Setup(x => x.GetAddAuthorizationByAddCodeAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(addAuthorizationEntity);

            // Student is current user
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);


        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Student_AssignAddAuthorizationAsync_CantUnrevoke()
        {
            //

            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                Id = "",
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "studentId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),

            };

            // Student is another - update not allowed
            AddAuthorization addAuthorizationEntity = new AddAuthorization("BadId", authorizationToUpdate.SectionId)
            {
                AddAuthorizationCode = "abCD1234",
                IsRevoked = true
            };

            addAuthorizationRepoMock.Setup(x => x.GetAddAuthorizationByAddCodeAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(addAuthorizationEntity);

            // Student is current user
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);


        }

        [TestMethod]
        public async Task Student_UpdateAddAuthorizationAsync_SuccessfullForStudent()
        {
            // Student is current user
             var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);

            Assert.AreEqual(authorizationToUpdate.Id, serviceResult.Id);
            Assert.AreEqual(authorizationToUpdate.SectionId, serviceResult.SectionId);
            Assert.AreEqual(authorizationToUpdate.AddAuthorizationCode, serviceResult.AddAuthorizationCode);
            Assert.AreEqual(authorizationToUpdate.IsRevoked, serviceResult.IsRevoked);
            Assert.AreEqual(authorizationToUpdate.RevokedBy, serviceResult.RevokedBy);
            Assert.AreEqual(authorizationToUpdate.RevokedTime, serviceResult.RevokedTime);
            Assert.AreEqual(authorizationToUpdate.StudentId, serviceResult.StudentId);
            Assert.AreEqual(authorizationToUpdate.AssignedBy, serviceResult.AssignedBy);
        }

        [TestMethod]
        public async Task UpdateAddAuthorizationAsync_AllowedForFacultyOfSection()
        {
            // Set up faculty as the current user.
            _currentUserFactory = new CurrentUserFactorySetup.FacultyUserFactory();
            var sectionEntities = new List<Domain.Student.Entities.Section>() { section };
            sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).ReturnsAsync(sectionEntities);
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, null, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);

            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);
            Assert.AreEqual(authorizationToUpdate.Id, serviceResult.Id);
            Assert.AreEqual(authorizationToUpdate.SectionId, serviceResult.SectionId);
            Assert.AreEqual(authorizationToUpdate.AddAuthorizationCode, serviceResult.AddAuthorizationCode);
            Assert.AreEqual(authorizationToUpdate.IsRevoked, serviceResult.IsRevoked);
            Assert.AreEqual(authorizationToUpdate.RevokedBy, serviceResult.RevokedBy);
            Assert.AreEqual(authorizationToUpdate.RevokedTime, serviceResult.RevokedTime);
            Assert.AreEqual(authorizationToUpdate.StudentId, serviceResult.StudentId);
            Assert.AreEqual(authorizationToUpdate.AssignedBy, serviceResult.AssignedBy);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Student_UpdateAddAuthorizationAsync_AdapterException()
        {
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.AddAuthorization, Domain.Student.Entities.AddAuthorization>()).Throws(new Exception());
            var serviceResult = await _addAuthorizationService.UpdateAddAuthorizationAsync(authorizationToUpdate);
        }
    }

    [TestClass]
    public class AddAuthorizationServiceTests_GetSectionAddAuthorizationsAsync : CurrentUserFactorySetup
    {
        private AddAuthorizationService _addAuthorizationService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ICurrentUserFactory _currentUserFactory;
        private IRoleRepository _roleRepository;
        private ILogger _logger;
        private Mock<IAddAuthorizationRepository> addAuthorizationRepoMock;
        private IAddAuthorizationRepository _addAuthorizationRepo;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository _sectionRepo;
        private IReferenceDataRepository _referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Dtos.Student.AddAuthorization authorizationToUpdate;
        private IEnumerable<AddAuthorization> addAuthorizationEntities;
        private Domain.Student.Entities.Section section;
        private string sectionId;


        [TestInitialize]
        public void Initialize()
        {
            sectionRepoMock = new Mock<ISectionRepository>();
            _sectionRepo = sectionRepoMock.Object;
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _referenceDataRepository = referenceDataRepositoryMock.Object;
            addAuthorizationRepoMock = new Mock<IAddAuthorizationRepository>();
            _addAuthorizationRepo = addAuthorizationRepoMock.Object;
            var _roleRepositoryMock = new Mock<IRoleRepository>();
            var facultyRole = new Domain.Entities.Role(1, "Faculty");
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { facultyRole });
            _roleRepository = _roleRepositoryMock.Object;
            _logger = new Mock<ILogger>().Object;
            _currentUserFactory = new CurrentUserFactorySetup.FacultyUserFactory();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                Id = "AddAuth1",
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "studentId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),
                IsRevoked = true,
                RevokedBy = "otherId",
                RevokedTime = DateTime.Now

            };

            addAuthorizationEntities = new List<AddAuthorization>() {
                new Domain.Student.Entities.AddAuthorization("AddAuth1", "SectionId")
                    {
                    AddAuthorizationCode = "abCD1234",
                    StudentId = "studentId",
                    AssignedBy = "FacultyId",
                    AssignedTime = DateTime.Now.AddDays(-1),
                    IsRevoked = true,
                    RevokedBy = "otherId",
                    RevokedTime = DateTime.Now
                    },
                new Domain.Student.Entities.AddAuthorization("AddAuth2", "SectionId")
                    {
                    AddAuthorizationCode = "abCD1111",
                    StudentId = "studentId2",
                    AssignedBy = "FacultyId2",
                    AssignedTime = DateTime.Now.AddDays(-2),

                    }
            };

            section = new Domain.Student.Entities.Section("sectionId", "courseId", "01", new DateTime(2018, 01, 01), 3m, null, "Section Title", "I", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") }, new List<string>() { "l1" }, "UG", new List<SectionStatusItem>(), true, true, false, true, false, false, false);
            section.AddFaculty("facultyId");
            sectionId = section.Id;

            var sectionEntities = new List<Domain.Student.Entities.Section>() { section };
            sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).ReturnsAsync(sectionEntities);

            var AddAuthorizationEntityToDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>()).Returns(AddAuthorizationEntityToDtoAdapter);

            // Mock return from the repo on update
            addAuthorizationRepoMock.Setup(repo => repo.GetSectionAddAuthorizationsAsync(It.IsAny<string>())).ReturnsAsync(addAuthorizationEntities);


            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, null, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _addAuthorizationService = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _currentUserFactory = null;
            _roleRepository = null;
            _logger = null;
            addAuthorizationRepoMock = null;
            _addAuthorizationRepo = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetSectionAddAuthorizationsAsync_NullSectionId()
        {
            var serviceResult = await _addAuthorizationService.GetSectionAddAuthorizationsAsync(null);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetSectionAddAuthorizationsAsync_EmptySectionId()
        {
            var serviceResult = await _addAuthorizationService.GetSectionAddAuthorizationsAsync(string.Empty);

        }



        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetSectionAddAuthorizationsAsync_NotFacultyOnSection()
        {
            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();

            var sectionEntities = new List<Domain.Student.Entities.Section>() { section };
            sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).ReturnsAsync(sectionEntities);
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, null, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetSectionAddAuthorizationsAsync(sectionId);
        }

        [TestMethod]
        public async Task GetSectionAddAuthorizationsAsync_AsFaculty_NullRepoResponse()
        { 
            IEnumerable<AddAuthorization> addAuthorizationEntities = null;
            addAuthorizationRepoMock.Setup(x => x.GetSectionAddAuthorizationsAsync(It.IsAny<string>())).ReturnsAsync(addAuthorizationEntities);

            // Student is current user
            var serviceResult = await _addAuthorizationService.GetSectionAddAuthorizationsAsync(sectionId);

            Assert.AreEqual(0, serviceResult.Count());
        }

        [TestMethod]
        public async Task GetSectionAddAuthorizationsAsync_AsFaculty_Success()
        {


            var serviceResult = await _addAuthorizationService.GetSectionAddAuthorizationsAsync(sectionId);
            Assert.AreEqual(2, serviceResult.Count());
            foreach (var auth in serviceResult)
            {
                var expectedAuth = addAuthorizationEntities.Where(aa => aa.Id == auth.Id).FirstOrDefault();
                Assert.IsNotNull(expectedAuth);
                Assert.AreEqual(expectedAuth.Id, auth.Id);
                Assert.AreEqual(expectedAuth.StudentId, auth.StudentId);
                Assert.AreEqual(expectedAuth.SectionId, auth.SectionId);
                Assert.AreEqual(expectedAuth.AddAuthorizationCode, auth.AddAuthorizationCode);
                Assert.AreEqual(expectedAuth.AssignedBy, auth.AssignedBy);
                Assert.AreEqual(expectedAuth.AssignedTime, auth.AssignedTime);
                Assert.AreEqual(expectedAuth.IsRevoked, auth.IsRevoked);
                Assert.AreEqual(expectedAuth.RevokedBy, auth.RevokedBy);
                Assert.AreEqual(expectedAuth.RevokedTime, auth.RevokedTime);
            }

        }

       

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetSectionAddAuthorizationsAsync_AdapterException()
        {
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>()).Throws(new Exception());
            var serviceResult = await _addAuthorizationService.GetSectionAddAuthorizationsAsync(sectionId);
        }
    }

    [TestClass]
    public class AddAuthorizationServiceTests_CreateAddAuthorizationAsync : CurrentUserFactorySetup
    {
        private AddAuthorizationService _addAuthorizationService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ICurrentUserFactory _currentUserFactory;
        private IRoleRepository _roleRepository;
        private ILogger _logger;
        private Mock<IAddAuthorizationRepository> addAuthorizationRepoMock;
        private IAddAuthorizationRepository _addAuthorizationRepo;
        private Mock<ISectionRepository> sectionRepoMock;
        private ISectionRepository _sectionRepo;
        private IReferenceDataRepository _referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private Dtos.Student.AddAuthorizationInput authorizationToCreate;
        private Ellucian.Colleague.Domain.Student.Entities.AddAuthorization authorizationEntity;
        private Domain.Student.Entities.Section section;


        [TestInitialize]
        public void Initialize()
        {
            sectionRepoMock = new Mock<ISectionRepository>();
            _sectionRepo = sectionRepoMock.Object;
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _referenceDataRepository = referenceDataRepositoryMock.Object;
            addAuthorizationRepoMock = new Mock<IAddAuthorizationRepository>();
            _addAuthorizationRepo = addAuthorizationRepoMock.Object;
            var _roleRepositoryMock = new Mock<IRoleRepository>();
            var facultyRole = new Domain.Entities.Role(1, "Faculty");
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { facultyRole });
            _roleRepository = _roleRepositoryMock.Object;
            _logger = new Mock<ILogger>().Object;
            _currentUserFactory = new CurrentUserFactorySetup.FacultyUserFactory();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            authorizationToCreate = new Dtos.Student.AddAuthorizationInput
            {
                SectionId = "SectionId",
                StudentId = "studentId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1)


            };

            authorizationEntity = new Domain.Student.Entities.AddAuthorization("addAuthorizationId", authorizationToCreate.SectionId)
            {
                StudentId = authorizationToCreate.StudentId,
                AssignedBy = authorizationToCreate.AssignedBy,
                AssignedTime = authorizationToCreate.AssignedTime
            };

            section = new Domain.Student.Entities.Section("sectionId", "courseId", "01", new DateTime(2018, 01, 01), 3m, null, "Section Title", "I", new List<Domain.Student.Entities.OfferingDepartment>() { new Domain.Student.Entities.OfferingDepartment("MATH") }, new List<string>() { "l1" }, "UG", new List<SectionStatusItem>(), true, true, false, true, false, false, false);
            section.AddFaculty("facultyId");

            //Register adapters

            // var AddAuthorizationDtoToEntityAdapter = new AddAuthorizationDtoToEntityAdapter(_adapterRegistry, _logger);
            var addAuthorizationInputDtoToEntityAdapter = new Student.Adapters.AddAuthorizationInputDtoToEntityAdapter(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.AddAuthorizationInput, Domain.Student.Entities.AddAuthorization>()).Returns(addAuthorizationInputDtoToEntityAdapter);

            var AddAuthorizationEntityToDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>()).Returns(AddAuthorizationEntityToDtoAdapter);

            // Mock return from the repo on update
            addAuthorizationRepoMock.Setup(repo => repo.CreateAddAuthorizationAsync(It.IsAny<AddAuthorization>())).ReturnsAsync(authorizationEntity);

            //// Mock return from repo for GetAsync
            //addAuthorizationRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>())).ReturnsAsync(authorizationEntity);

            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, null, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _addAuthorizationService = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _currentUserFactory = null;
            _roleRepository = null;
            _logger = null;
            addAuthorizationRepoMock = null;
            _addAuthorizationRepo = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task CreateAddAuthorizationAsync_NullArgument()
        {
            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(null);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateAddAuthorizationAsync_InvalidArgument_NullSectionId()
        {
            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(new Dtos.Student.AddAuthorizationInput() { StudentId = "studentId" });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateAddAuthorizationAsync_InvalidArgument_EmptySectionId()
        {
            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(new Dtos.Student.AddAuthorizationInput() { StudentId = "studentId", SectionId = string.Empty });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateAddAuthorizationAsync_InvalidArgument_NullStudent()
        {
            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(new Dtos.Student.AddAuthorizationInput() { SectionId = "SectionId"  });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateAddAuthorizationAsync_InvalidArgument_EmptyStudentId()
        {
            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(new Dtos.Student.AddAuthorizationInput() { SectionId = "SectionId", StudentId = string.Empty });

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task NotFaculty_CreateAddAuthorizationAsync_SectionNullFromRepo()
        {
            // Advisor is current user (not faculty on the section.)
            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            // Section will come back null from section repo
            authorizationToCreate = new Dtos.Student.AddAuthorizationInput
            {
                SectionId = "SectionId",
                StudentId = "OtherId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1)


            };
            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(authorizationToCreate);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task NotFaculty_CreateAddAuthorizationAsync()
        {
            // Advisor is current user (not faculty on the section.)
            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            var sectionEntities = new List<Domain.Student.Entities.Section>() { section };
            sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).ReturnsAsync(sectionEntities);
            authorizationToCreate = new Dtos.Student.AddAuthorizationInput
            {
                SectionId = "SectionId",
                StudentId = "OtherId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1)


            };
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, null, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(authorizationToCreate);
        }


        [TestMethod]
        public async Task CreateAddAuthorizationAsync_AllowedForFacultyOfSection()
        {
            // Set up faculty as the current user.
            var sectionEntities = new List<Domain.Student.Entities.Section>() { section };
            sectionRepoMock.Setup(x => x.GetCachedSectionsAsync(It.IsAny<IEnumerable<string>>(), false)).ReturnsAsync(sectionEntities);
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, null, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);

            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(authorizationToCreate);
            Assert.AreEqual("addAuthorizationId", serviceResult.Id);
            Assert.AreEqual(authorizationToCreate.SectionId, serviceResult.SectionId);
            Assert.AreEqual(authorizationToCreate.StudentId, serviceResult.StudentId);
            Assert.AreEqual(authorizationToCreate.AssignedBy, serviceResult.AssignedBy);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task Student_CreateAddAuthorizationAsync_AdapterException()
        {
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Dtos.Student.AddAuthorizationInput, Domain.Student.Entities.AddAuthorization>()).Throws(new Exception());
            var serviceResult = await _addAuthorizationService.CreateAddAuthorizationAsync(authorizationToCreate);
        }
    }

    [TestClass]
    public class AddAuthorizationServiceTests_GetStudentAddAuthorizationsAsync : CurrentUserFactorySetup
    {
        private AddAuthorizationService _addAuthorizationService;
        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private IAdapterRegistry _adapterRegistry;
        private ICurrentUserFactory _currentUserFactory;
        private IRoleRepository _roleRepository;
        private ILogger _logger;
        private Mock<IAddAuthorizationRepository> addAuthorizationRepoMock;
        private IAddAuthorizationRepository _addAuthorizationRepo;
        private Mock<ISectionRepository> sectionRepoMock;
        private Mock<IStudentRepository> studentRepoMock;
        private Mock<IRoleRepository> _roleRepositoryMock;

        private ISectionRepository _sectionRepo;
        private IStudentRepository _studentRepo;
        private Dtos.Student.AddAuthorization authorizationToUpdate;
        private IEnumerable<AddAuthorization> addAuthorizationEntities;
        private IReferenceDataRepository _referenceDataRepository;
        private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
        private string sectionId;

        [TestInitialize]
        public void Initialize()
        {
            sectionRepoMock = new Mock<ISectionRepository>();
            _sectionRepo = sectionRepoMock.Object;
            referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _referenceDataRepository = referenceDataRepositoryMock.Object;
            studentRepoMock = new Mock<IStudentRepository>();
            _studentRepo = studentRepoMock.Object;
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });
            _roleRepository = _roleRepositoryMock.Object;
            addAuthorizationRepoMock = new Mock<IAddAuthorizationRepository>();
            _addAuthorizationRepo = addAuthorizationRepoMock.Object;
            _logger = new Mock<ILogger>().Object;
            _currentUserFactory = new CurrentUserFactorySetup.StudentUserFactory();
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;

            authorizationToUpdate = new Dtos.Student.AddAuthorization
            {
                Id = "AddAuth1",
                SectionId = "SectionId",
                AddAuthorizationCode = "abCD1234",
                StudentId = "studentId",
                AssignedBy = "FacultyId",
                AssignedTime = DateTime.Now.AddDays(-1),
                IsRevoked = true,
                RevokedBy = "otherId",
                RevokedTime = DateTime.Now

            };

            addAuthorizationEntities = new List<AddAuthorization>() {
                new Domain.Student.Entities.AddAuthorization("AddAuth1", "SectionId")
                    {
                    AddAuthorizationCode = "abCD1234",
                    StudentId = "studentId",
                    AssignedBy = "FacultyId",
                    AssignedTime = DateTime.Now.AddDays(-1),
                    IsRevoked = true,
                    RevokedBy = "otherId",
                    RevokedTime = DateTime.Now
                    },
                new Domain.Student.Entities.AddAuthorization("AddAuth2", "SectionId")
                    {
                    AddAuthorizationCode = "abCD1111",
                    StudentId = "studentId2",
                    AssignedBy = "FacultyId2",
                    AssignedTime = DateTime.Now.AddDays(-2),

                    }
            };

            sectionId = _currentUserFactory.CurrentUser.PersonId;

            var studentEntity = new Domain.Student.Entities.StudentAccess("1234567");
            studentEntity.AddAdvisement(new CurrentUserFactorySetup.AdvisorUserFactory().CurrentUser.PersonId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), null);
            studentRepoMock.Setup(x => x.GetStudentAccessAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<StudentAccess>() { studentEntity });

            var AddAuthorizationEntityToDtoAdapter = new AutoMapperAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>(_adapterRegistry, _logger);
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>()).Returns(AddAuthorizationEntityToDtoAdapter);

            // Mock return from the repo on update
            addAuthorizationRepoMock.Setup(repo => repo.GetStudentAddAuthorizationsAsync(It.IsAny<string>())).ReturnsAsync(addAuthorizationEntities);

            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);

        }

        [TestCleanup]
        public void Cleanup()
        {
            _addAuthorizationService = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _currentUserFactory = null;
            _roleRepository = null;
            _logger = null;
            addAuthorizationRepoMock = null;
            _addAuthorizationRepo = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentAddAuthorizationsAsync_NullStudentId()
        {
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync(null);

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetStudentAddAuthorizationsAsync_EmptyStudentId()
        {
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync(string.Empty);

        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_NotAdvisor()
        {
            _currentUserFactory = new CurrentUserFactorySetup.StudentUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync(_currentUserFactory.CurrentUser.PersonId + "1");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Assigned_to_Student_with_AllAccessAssignedAdvisees()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAssignedAdvisees));
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Assigned_to_Student_with_UpdateAssignedAdvisees()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAssignedAdvisees));
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Assigned_to_Student_with_ReviewAssignedAdvisees()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAssignedAdvisees));
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Assigned_to_Student_with_ViewAssignedAdvisees()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAssignedAdvisees));
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Assigned_to_Student_insufficient_permission()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Not_Assigned_to_Student_insufficient_permissions()
        {
            var studentEntity = new Domain.Student.Entities.StudentAccess("1234567");
            studentRepoMock.Setup(x => x.GetStudentAccessAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(new List<StudentAccess>() { studentEntity });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Not_Assigned_to_Student_with_AllAccessAnyAdvisee()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.AllAccessAnyAdvisee));
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Not_Assigned_to_Student_with_UpdateAnyAdvisee()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.UpdateAnyAdvisee));
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Not_Assigned_to_Student_with_ReviewAnyAdvisee()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ReviewAnyAdvisee));
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NotRequestedStudent_Advisor_Not_Assigned_to_Student_with_ViewAnyAdvisee()
        {
            var advisorRole = new Domain.Entities.Role(1, "Advisor");
            advisorRole.AddPermission(new Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
            _roleRepositoryMock.Setup(rr => rr.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role> { advisorRole });
            _roleRepositoryMock.Setup(rr => rr.Roles).Returns(new List<Domain.Entities.Role> { advisorRole });

            _currentUserFactory = new CurrentUserFactorySetup.AdvisorUserFactory();
            _addAuthorizationService = new AddAuthorizationService(_addAuthorizationRepo, _sectionRepo, _studentRepo, _referenceDataRepository, null, _adapterRegistry, _currentUserFactory, _roleRepository, _logger);
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync("1234567");
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_NullRepoResponse()
        {
            IEnumerable<AddAuthorization> addAuthorizationEntities = null;
            addAuthorizationRepoMock.Setup(x => x.GetStudentAddAuthorizationsAsync(It.IsAny<string>())).ReturnsAsync(addAuthorizationEntities);

            // Student is current user
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync(sectionId);

            Assert.AreEqual(0, serviceResult.Count());
        }

        [TestMethod]
        public async Task GetStudentAddAuthorizationsAsync_Success()
        {
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync(sectionId);
            Assert.AreEqual(2, serviceResult.Count());
            foreach (var auth in serviceResult)
            {
                var expectedAuth = addAuthorizationEntities.Where(aa => aa.Id == auth.Id).FirstOrDefault();
                Assert.IsNotNull(expectedAuth);
                Assert.AreEqual(expectedAuth.Id, auth.Id);
                Assert.AreEqual(expectedAuth.StudentId, auth.StudentId);
                Assert.AreEqual(expectedAuth.SectionId, auth.SectionId);
                Assert.AreEqual(expectedAuth.AddAuthorizationCode, auth.AddAuthorizationCode);
                Assert.AreEqual(expectedAuth.AssignedBy, auth.AssignedBy);
                Assert.AreEqual(expectedAuth.AssignedTime, auth.AssignedTime);
                Assert.AreEqual(expectedAuth.IsRevoked, auth.IsRevoked);
                Assert.AreEqual(expectedAuth.RevokedBy, auth.RevokedBy);
                Assert.AreEqual(expectedAuth.RevokedTime, auth.RevokedTime);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task GetStudentAddAuthorizationsAsync_AdapterException()
        {
            _adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.AddAuthorization, Dtos.Student.AddAuthorization>()).Throws(new Exception());
            var serviceResult = await _addAuthorizationService.GetStudentAddAuthorizationsAsync(sectionId);
        }
    }

    public abstract class CurrentUserFactorySetup
    {
        protected Domain.Entities.Role advisorRole = new Domain.Entities.Role(105, "Advisor");
        protected Domain.Entities.Role facultyRole = new Domain.Entities.Role(999, "Faculty");

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
                        PersonId = "studentId",
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

        public class FacultyUserFactory : ICurrentUserFactory
        {
            public ICurrentUser CurrentUser
            {
                get
                {
                    return new CurrentUser(new Claims()
                    {
                        ControlId = "123",
                        Name = "George",
                        PersonId = "facultyId",
                        SecurityToken = "321",
                        SessionTimeout = 30,
                        UserName = "Faculty Name",
                        Roles = new List<string>() { "Faculty" },
                        SessionFixationId = "abc123"
                    });
                }
            }
        }
    }
}
