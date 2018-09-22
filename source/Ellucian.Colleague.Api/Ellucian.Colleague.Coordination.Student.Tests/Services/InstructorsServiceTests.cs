//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class InstructorsServiceTests
    {
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");

            public class PersonUserFactory : ICurrentUserFactory
            {
                public ICurrentUser CurrentUser
                {
                    get
                    {
                        return new CurrentUser(new Claims()
                        {
                            ControlId = "123",
                            Name = "George",
                            PersonId = "0000015",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Faculty",
                            Roles = new List<string>() { "Faculty", "VIEW.INSTRUCTORS" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }
        [TestClass]
        public class InstructorsServiceTests_GET : CurrentUserSetup
        {
            private const string instructorsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string instructorsCode = "AT";
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Instructor> _instructorEntitiesCollection;
            private Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Instructor>, int> _instructorTuple;
            private Domain.Entities.Permission permissionViewInstrucors;
            string locationId = "66698e2f-1bd2-496b-90f0-23c77acda34e";
            IEnumerable<Domain.Base.Entities.Location> _locations;
            ICurrentUserFactory userFactory;

            private InstructorsService _instructorsService;
            private Mock<ILogger> _loggerMock;
            private Mock<IPersoRepository> _instructorRepositoryMock;
            private Mock<IReferenceDataRepository> _referenceRepositoryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            [TestInitialize]
            public void Initialize()
            {
                _instructorRepositoryMock = new Mock<IPersoRepository>();
                _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Set up current user
                userFactory = _currentUserFactoryMock.Object;
                userFactory = new CurrentUserSetup.PersonUserFactory();
                // Mock permissions
                permissionViewInstrucors = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewInstructors);
                personRole.AddPermission(permissionViewInstrucors);
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                BuildData();

                _instructorsService = new InstructorsService(_instructorRepositoryMock.Object, _referenceRepositoryMock.Object, _studentReferenceDataRepositoryMock.Object,
                   _adapterRegistryMock.Object, userFactory, _roleRepositoryMock.Object, _loggerMock.Object, baseConfigurationRepository);
            }

            private void BuildData()
            {
                _locations = new List<Domain.Base.Entities.Location>() 
                {
                    new Domain.Base.Entities.Location("66698e2f-1bd2-496b-90f0-23c77acda34e", "Code1", "Desc 1"),
                    new Domain.Base.Entities.Location("94bad9e4-902c-49b9-9763-7f2817e7824d", "Code2", "Desc 2"),
                    new Domain.Base.Entities.Location("1763bb9c-a261-4f90-a8d9-6ab3ebff2be6", "Code3", "Desc 3")
                };
                _referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_locations);

                IEnumerable<Domain.Base.Entities.Department> _depts = new List<Domain.Base.Entities.Department>() 
                {
                    new Domain.Base.Entities.Department("3977673d-41e8-4401-a36c-c3101f333d23", "Code1", "Desc 1", true),
                    new Domain.Base.Entities.Department("0edd7609-d459-4dd9-9148-4f3e23d670a9", "Code2", "Desc 2", true),
                    new Domain.Base.Entities.Department("25ef8441-dbd4-41a0-964e-110b1c2c99f4", "Code3", "Desc 3", true)
                };
                _referenceRepositoryMock.Setup(i => i.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(_depts);

                IEnumerable<Domain.Student.Entities.FacultyContractTypes> _facultyContractTypes = new List<Domain.Student.Entities.FacultyContractTypes>() 
                {
                    new Domain.Student.Entities.FacultyContractTypes("935f5aa0-bb22-4069-ac27-f895eb895f34", "Code1", "Desc 1"),
                    new Domain.Student.Entities.FacultyContractTypes("add64774-db22-44de-be3b-3517baabb1c8", "Code2", "Desc 2"),
                    new Domain.Student.Entities.FacultyContractTypes("29dc2163-ab6d-4ba4-8bec-50bb00b28be2", "Code3", "Desc 3")
                };
                _studentReferenceDataRepositoryMock.Setup(i => i.GetFacultyContractTypesAsync(It.IsAny<bool>())).ReturnsAsync(_facultyContractTypes);

                IEnumerable<Domain.Student.Entities.FacultySpecialStatuses> _facultySpecialStatuses = new List<Domain.Student.Entities.FacultySpecialStatuses>() 
                {
                    new Domain.Student.Entities.FacultySpecialStatuses("1cc6499a-90c1-422f-8710-45a9b34d2373", "Code1", "Desc 1"),
                    new Domain.Student.Entities.FacultySpecialStatuses("b68c40cb-11a4-49d4-ae3b-0d08d1f5fac3", "Code2", "Desc 2"),
                    new Domain.Student.Entities.FacultySpecialStatuses("991d3818-8467-44f0-8f8e-777ca8567b53", "Code3", "Desc 3")
                };
                _studentReferenceDataRepositoryMock.Setup(i => i.GetFacultySpecialStatusesAsync(It.IsAny<bool>())).ReturnsAsync(_facultySpecialStatuses);

                Dictionary<string, string> _personKeyGuids = new Dictionary<string, string>();
                List<string> ids = new List<string>() { "AT", "AC", "CU" };
                _personKeyGuids.Add("AT", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                _personKeyGuids.Add("AC", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d");
                _personKeyGuids.Add("CU", "d2253ac7-9931-4560-b42f-1fccd43c952e");
                _instructorRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(_personKeyGuids);

                _instructorEntitiesCollection = new List<Ellucian.Colleague.Domain.Student.Entities.Instructor>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                    {
                        Departments = new List<FacultyDeptLoad>()
                        {
                            new FacultyDeptLoad()
                            {
                                DeptPcts = 50,
                                FacultyDepartment = "Code1"
                            }
                        }
                    },
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC"),
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU")
                };
                _instructorTuple = new Tuple<IEnumerable<Domain.Student.Entities.Instructor>, int>(_instructorEntitiesCollection, _instructorEntitiesCollection.Count());
            }

            [TestCleanup]
            public void Cleanup()
            {
                _instructorRepositoryMock = null;
                _studentReferenceDataRepositoryMock = null;
                _adapterRegistryMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _instructorsService = null;
                _instructorEntitiesCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorsAsync()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(_instructorTuple);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorsAsync_WithLocationGuid()
            {
                
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>()))
                    .ReturnsAsync(_instructorTuple);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuidAsync()
            {
                var expected = _instructorEntitiesCollection.First();
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuidAsync_NoFacultyDepartment()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    Departments = new List<FacultyDeptLoad>() 
                    {
                        new FacultyDeptLoad()
                        {
                            DeptPcts = 50,
                            FacultyDepartment = ""
                        }
                    }
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuidAsync_WithFacultyStatuses()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    SpecialStatus = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuidAsync_WithLocation()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    HomeLocation = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]            
            public async Task InstructorsService_GetInstructorByGuidAsync_FacultyContractType()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }
            
            [TestMethod]
            public async Task InstructorsService_GetInstructorsAsync_WithLocationGuid_ArgumentNullException()
            {
                _referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(null);
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>()))
                    .ReturnsAsync(_instructorTuple);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorsAsync_WithLocationGuid_ArgumentException()
            {                
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>()))
                    .ReturnsAsync(_instructorTuple);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "1", It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_FacultyContractType_ArgumentNullException()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                _instructorRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny <List<string>>())).ThrowsAsync(new ArgumentNullException());
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstructorsService_GetInstructorByGuidAsync_FacultyContractType_Exception()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                _instructorRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<List<string>>())).ThrowsAsync(new Exception());
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task InstructorsService_GetInstructorsAsync_NoPerms()
            {
                personRole.RemovePermission(permissionViewInstrucors);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructorsService_GetInstructorsAsync_ArgumentNullException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_NoKey()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "");
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_WrongKey()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", " ");
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_WrongDept()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    Departments = new List<FacultyDeptLoad>() 
                    {
                        new FacultyDeptLoad()
                        {
                            DeptPcts = 50,
                            FacultyDepartment = "ABC"
                        }
                    }
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_WrongLocation()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    HomeLocation = "ABC"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_WrongFacultyStatuses()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    SpecialStatus = "ABC"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_WrongFacultyContractType()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "ABC"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuidAsync(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorsAsync_KeyNotFoundException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task InstructorsService_GetInstructorsAsync_PermissionsException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstructorsService_GetInstructorsAsync_Exception()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_ArgumentNullException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var results = await _instructorsService.GetInstructorByGuidAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_KeyNotFoundException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var results = await _instructorsService.GetInstructorByGuidAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task InstructorsService_GetInstructorByGuidAsync_PermissionsException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                var results = await _instructorsService.GetInstructorByGuidAsync("1");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstructorsService_GetInstructorByGuidAsync_Exception()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var results = await _instructorsService.GetInstructorByGuidAsync("1");
            }            
        }

        [TestClass]
        public class InstructorsServiceTests_GET_v9 : CurrentUserSetup
        {
            private const string instructorsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private const string instructorsCode = "AT";
            private IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Instructor> _instructorEntitiesCollection;
            private Tuple<IEnumerable<Ellucian.Colleague.Domain.Student.Entities.Instructor>, int> _instructorTuple;
            private Domain.Entities.Permission permissionViewInstrucors;
            string locationId = "66698e2f-1bd2-496b-90f0-23c77acda34e";
            IEnumerable<Domain.Base.Entities.Location> _locations;
            ICurrentUserFactory userFactory;

            private InstructorsService _instructorsService;
            private Mock<ILogger> _loggerMock;
            private Mock<IPersoRepository> _instructorRepositoryMock;
            private Mock<IReferenceDataRepository> _referenceRepositoryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;
            private Mock<IAdapterRegistry> _adapterRegistryMock;
            private Mock<ICurrentUserFactory> _currentUserFactoryMock;
            private Mock<IRoleRepository> _roleRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;


            [TestInitialize]
            public void Initialize()
            {
                _instructorRepositoryMock = new Mock<IPersoRepository>();
                _referenceRepositoryMock = new Mock<IReferenceDataRepository>();
                _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _adapterRegistryMock = new Mock<IAdapterRegistry>();
                _currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                _roleRepositoryMock = new Mock<IRoleRepository>();
                _loggerMock = new Mock<ILogger>();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                // Set up current user
                userFactory = _currentUserFactoryMock.Object;
                userFactory = new CurrentUserSetup.PersonUserFactory();
                // Mock permissions
                permissionViewInstrucors = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewInstructors);
                personRole.AddPermission(permissionViewInstrucors);
                _roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                BuildData();

                _instructorsService = new InstructorsService(_instructorRepositoryMock.Object, _referenceRepositoryMock.Object, _studentReferenceDataRepositoryMock.Object,
                   _adapterRegistryMock.Object, userFactory, _roleRepositoryMock.Object, _loggerMock.Object, baseConfigurationRepository);
            }

            private void BuildData()
            {
                _locations = new List<Domain.Base.Entities.Location>()
                {
                    new Domain.Base.Entities.Location("66698e2f-1bd2-496b-90f0-23c77acda34e", "Code1", "Desc 1"),
                    new Domain.Base.Entities.Location("94bad9e4-902c-49b9-9763-7f2817e7824d", "Code2", "Desc 2"),
                    new Domain.Base.Entities.Location("1763bb9c-a261-4f90-a8d9-6ab3ebff2be6", "Code3", "Desc 3")
                };
                _referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(_locations);

                IEnumerable<Domain.Base.Entities.Department> _depts = new List<Domain.Base.Entities.Department>()
                {
                    new Domain.Base.Entities.Department("3977673d-41e8-4401-a36c-c3101f333d23", "Code1", "Desc 1", true),
                    new Domain.Base.Entities.Department("0edd7609-d459-4dd9-9148-4f3e23d670a9", "Code2", "Desc 2", true),
                    new Domain.Base.Entities.Department("25ef8441-dbd4-41a0-964e-110b1c2c99f4", "Code3", "Desc 3", true)
                };
                _referenceRepositoryMock.Setup(i => i.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(_depts);

                IEnumerable<Domain.Student.Entities.FacultyContractTypes> _facultyContractTypes = new List<Domain.Student.Entities.FacultyContractTypes>()
                {
                    new Domain.Student.Entities.FacultyContractTypes("935f5aa0-bb22-4069-ac27-f895eb895f34", "Code1", "Desc 1"),
                    new Domain.Student.Entities.FacultyContractTypes("add64774-db22-44de-be3b-3517baabb1c8", "Code2", "Desc 2"),
                    new Domain.Student.Entities.FacultyContractTypes("29dc2163-ab6d-4ba4-8bec-50bb00b28be2", "Code3", "Desc 3")
                };
                _studentReferenceDataRepositoryMock.Setup(i => i.GetFacultyContractTypesAsync(It.IsAny<bool>())).ReturnsAsync(_facultyContractTypes);

                IEnumerable<Domain.Student.Entities.FacultySpecialStatuses> _facultySpecialStatuses = new List<Domain.Student.Entities.FacultySpecialStatuses>()
                {
                    new Domain.Student.Entities.FacultySpecialStatuses("1cc6499a-90c1-422f-8710-45a9b34d2373", "Code1", "Desc 1"),
                    new Domain.Student.Entities.FacultySpecialStatuses("b68c40cb-11a4-49d4-ae3b-0d08d1f5fac3", "Code2", "Desc 2"),
                    new Domain.Student.Entities.FacultySpecialStatuses("991d3818-8467-44f0-8f8e-777ca8567b53", "Code3", "Desc 3")
                };
                _studentReferenceDataRepositoryMock.Setup(i => i.GetFacultySpecialStatusesAsync(It.IsAny<bool>())).ReturnsAsync(_facultySpecialStatuses);

                _instructorRepositoryMock.Setup(i => i.GetTenureTypesAsync(It.IsAny<bool>())).ReturnsAsync(new List<TenureTypes>()
                {
                    new TenureTypes("412c87d3-bbb5-4c61-971d-91ea6da1e3a1", "T1", "Tenure Type 1"),
                    new TenureTypes("b749862b-2fdd-4dac-9354-32e87100a40d", "T2", "Tenure Type 2"),
                    new TenureTypes("1588bb67-b6e5-44f2-bf8e-fff78070c8e1", "T3", "Tenure Type 3")
                }


                );

                Dictionary<string, string> _personKeyGuids = new Dictionary<string, string>();
                List<string> ids = new List<string>() { "AT", "AC", "CU" };
                _personKeyGuids.Add("AT", "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc");
                _personKeyGuids.Add("AC", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d");
                _personKeyGuids.Add("CU", "d2253ac7-9931-4560-b42f-1fccd43c952e");
                _instructorRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(_personKeyGuids);

                _instructorEntitiesCollection = new List<Ellucian.Colleague.Domain.Student.Entities.Instructor>()
                {
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                    {
                        TentureType = "",
                        Departments = new List<FacultyDeptLoad>()
                        {
                            new FacultyDeptLoad()
                            {
                                DeptPcts = 50,
                                FacultyDepartment = "Code1"
                            }
                        }
                    },
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "AC"),
                    new Ellucian.Colleague.Domain.Student.Entities.Instructor("d2253ac7-9931-4560-b42f-1fccd43c952e", "CU")
                };
                _instructorTuple = new Tuple<IEnumerable<Domain.Student.Entities.Instructor>, int>(_instructorEntitiesCollection, _instructorEntitiesCollection.Count());
            }

            [TestCleanup]
            public void Cleanup()
            {
                _instructorRepositoryMock = null;
                _studentReferenceDataRepositoryMock = null;
                _adapterRegistryMock = null;
                _currentUserFactoryMock = null;
                _roleRepositoryMock = null;
                _instructorsService = null;
                _instructorEntitiesCollection = null;
                _referenceRepositoryMock = null;
                _loggerMock = null;
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorsAsync()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ReturnsAsync(_instructorTuple);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorsAsync_WithLocationGuid()
            {

                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>()))
                    .ReturnsAsync(_instructorTuple);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuid2Async()
            {
                var expected = _instructorEntitiesCollection.First();
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuid2Async_NoFacultyDepartment()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    Departments = new List<FacultyDeptLoad>()
                    {
                        new FacultyDeptLoad()
                        {
                            DeptPcts = 50,
                            FacultyDepartment = ""
                        }
                    }
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuid2Async_WithFacultyStatuses()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    SpecialStatus = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuid2Async_WithLocation()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    HomeLocation = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuid2Async_FacultyContractType()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorByGuid2Async_TenureType()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "Code1",
                    TentureType = "T1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
                Assert.AreEqual("412c87d3-bbb5-4c61-971d-91ea6da1e3a1", results.Tenure.TenureType.Id);
            }
            [TestMethod]
            public async Task InstructorsService_GetInstructorsAsync_WithLocationGuid_ArgumentNullException()
            {
                _referenceRepositoryMock.Setup(i => i.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(null);
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>()))
                    .ReturnsAsync(_instructorTuple);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task InstructorsService_GetInstructorsAsync_WithLocationGuid_ArgumentException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), locationId, It.IsAny<bool>()))
                    .ReturnsAsync(_instructorTuple);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), "1", It.IsAny<bool>());
                Assert.IsNotNull(results);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_FacultyContractType_ArgumentNullException()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                _instructorRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<List<string>>())).ThrowsAsync(new ArgumentNullException());
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstructorsService_GetInstructorByGuid2Async_FacultyContractType_Exception()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "Code1"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                _instructorRepositoryMock.Setup(i => i.GetPersonGuidsAsync(It.IsAny<List<string>>())).ThrowsAsync(new Exception());
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task InstructorsService_GetInstructorsAsync_NoPerms()
            {
                personRole.RemovePermission(permissionViewInstrucors);
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructorsService_GetInstructorsAsync_ArgumentNullException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new ArgumentNullException());
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_NoKey()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "");
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_WrongKey()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", " ");
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_WrongDept()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    Departments = new List<FacultyDeptLoad>()
                    {
                        new FacultyDeptLoad()
                        {
                            DeptPcts = 50,
                            FacultyDepartment = "ABC"
                        }
                    }
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_WrongLocation()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    HomeLocation = "ABC"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_WrongFacultyStatuses()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    SpecialStatus = "ABC"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_WrongFacultyContractType()
            {
                var expected = new Ellucian.Colleague.Domain.Student.Entities.Instructor("7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "AT")
                {
                    ContractType = "ABC"
                };
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(expected);
                var results = await _instructorsService.GetInstructorByGuid2Async(expected.RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorsAsync_KeyNotFoundException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task InstructorsService_GetInstructorsAsync_PermissionsException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new PermissionsException());
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstructorsService_GetInstructorsAsync_Exception()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
                var results = await _instructorsService.GetInstructorsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_ArgumentNullException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new ArgumentNullException());
                var results = await _instructorsService.GetInstructorByGuid2Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_KeyNotFoundException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new KeyNotFoundException());
                var results = await _instructorsService.GetInstructorByGuid2Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task InstructorsService_GetInstructorByGuid2Async_PermissionsException()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new PermissionsException());
                var results = await _instructorsService.GetInstructorByGuid2Async("1");
            }

            [TestMethod]
            [ExpectedException(typeof(Exception))]
            public async Task InstructorsService_GetInstructorByGuid2Async_Exception()
            {
                _instructorRepositoryMock.Setup(i => i.GetInstructorByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception());
                var results = await _instructorsService.GetInstructorByGuid2Async("1");
            }
        }

    }
}