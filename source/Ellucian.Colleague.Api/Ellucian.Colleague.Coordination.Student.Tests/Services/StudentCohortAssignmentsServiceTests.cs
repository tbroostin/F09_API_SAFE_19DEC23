//Copyright 2017-2020 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentCohortAssignmentsServiceTests
    {
        // sets up a current user
        public abstract class CurrentUserSetup
        {
            protected Domain.Entities.Role viewCohortsRole = new Domain.Entities.Role(1, "VIEW.STUDENT.COHORT.ASSIGNMENTS");

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
                            Roles = new List<string>() { "VIEW.STUDENT.COHORT.ASSIGNMENTS" },
                            SessionFixationId = "abc123",
                        });
                    }
                }
            }
        }

        [TestClass]
        public class StudentCohortAssignmentsServiceTests_Get : CurrentUserSetup
        {
            private StudentCohortAssignmentsService _studentCohortAssignmentsService;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<ICurrentUserFactory> currentUserFactoryMock;
            private ICurrentUserFactory currentUserFactory;
            private Mock<ILogger> _loggerMock;
            private Mock<IPersonRepository> _personRepositoryMock;
            private Mock<IStudentRepository> _studentRepositoryMock;
            private Mock<IStudentReferenceDataRepository> _studentReferenceRepositoryMock;
            private Mock<IStudentCohortAssignmentsRepository> _studentCohortAssignmentRepositoryMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            private const string studentCohortAssignmentsGuid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";
            private List<StudentCohortAssignments> _studentCohortAssignmentsDtos;
            private List<StudentCohortAssignment> _studentCohortAssignmentEntities;
            private Tuple<IEnumerable<StudentCohortAssignment>, int> _studentCohortAssignmentsTuple;
            private IEnumerable<Domain.Student.Entities.StudentCohort> _studentCohortEntities;
            
            int offset = 0;
            int limit = 200;

            private Domain.Entities.Permission permissionViewStudentCohortAssignment;


            [TestInitialize]
            public void Initialize()
            {
                _personRepositoryMock = new Mock<IPersonRepository>();
                _studentRepositoryMock = new Mock<IStudentRepository>();
                _studentReferenceRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                _studentCohortAssignmentRepositoryMock = new Mock<IStudentCohortAssignmentsRepository>();
                _loggerMock = new Mock<ILogger>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                currentUserFactoryMock = new Mock<ICurrentUserFactory>();
                currentUserFactory = currentUserFactoryMock.Object;
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                BuildData();
                BuildMocks();

                _studentCohortAssignmentsService = new StudentCohortAssignmentsService(
                    _studentCohortAssignmentRepositoryMock.Object, _personRepositoryMock.Object,
                    _studentReferenceRepositoryMock.Object,
                    adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object,
                    baseConfigurationRepository, _loggerMock.Object);
            }
            private void BuildData()
            {
                _studentCohortEntities = new List<Domain.Student.Entities.StudentCohort>()
                {
                   new Domain.Student.Entities.StudentCohort("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31", "2019", "2019 Cohort") { CohortType = "FED" },
                   new Domain.Student.Entities.StudentCohort("7e990bda-9427-4de6-b0ef-bba9b015e399", "OTHER", "Other Cohort"),
                   new Domain.Student.Entities.StudentCohort("8f3aac22-e0b5-4159-b4e2-da158362c41b", "LOCAL","Local Cohort")
                };

                #region dto

                _studentCohortAssignmentsDtos = new List<StudentCohortAssignments>()
                {
                    new Dtos.StudentCohortAssignments()
                    {
                        Id = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                        StartOn = new DateTime?(DateTime.Today),
                        Person = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Cohort = new GuidObject2("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31"),
                        AcademicLevel = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8")
                    },
                    new Dtos.StudentCohortAssignments()
                    {
                        Id = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                        StartOn = new DateTime?(DateTime.Today),
                        Person = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Cohort = new GuidObject2("7e990bda-9427-4de6-b0ef-bba9b015e399"),
                        AcademicLevel = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8")
                    },
                    new Dtos.StudentCohortAssignments()
                    {
                        Id = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                        StartOn = new DateTime?(DateTime.Today),
                        Person = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8"),
                        Cohort = new GuidObject2("8f3aac22-e0b5-4159-b4e2-da158362c41b"),
                        AcademicLevel = new GuidObject2("1df164eb-8178-4321-a9f7-24f27f3991d8")
                    }
                };
                #endregion

                #region Entity

                _studentCohortAssignmentEntities = new List<StudentCohortAssignment>();
                foreach (var dto in _studentCohortAssignmentsDtos)
                {
                    var entity = new StudentCohortAssignment(_studentCohortEntities.FirstOrDefault(sce => sce.Guid == dto.Cohort.Id).Code, dto.Id)
                    {
                        CohortId = _studentCohortEntities.FirstOrDefault(sce => sce.Guid == dto.Cohort.Id).Code,
                        AcadLevel = "UG",
                        PersonId = "0003784",
                        StartOn = dto.StartOn,
                        CohortType = ""
                    };
                    _studentCohortAssignmentEntities.Add(entity);
                }
                _studentCohortAssignmentsTuple = new Tuple<IEnumerable<StudentCohortAssignment>, int>(_studentCohortAssignmentEntities, _studentCohortAssignmentEntities.Count());

                #endregion
            }

            private void BuildMocks()
            {
                // Mock permissions
                permissionViewStudentCohortAssignment = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentCohortAssignments);
                viewCohortsRole.AddPermission(permissionViewStudentCohortAssignment);
                roleRepositoryMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { viewCohortsRole });

                _studentCohortAssignmentRepositoryMock.Setup(rp => rp.GetStudentCohortAssignmentsAsync(offset, limit, It.IsAny<StudentCohortAssignment>(), It.IsAny<Dictionary<string,string>>()))
                    .ReturnsAsync(_studentCohortAssignmentsTuple);
                _studentCohortAssignmentRepositoryMock.Setup(rp => rp.GetStudentCohortAssignmentByIdAsync(It.IsAny<string>())).ReturnsAsync(_studentCohortAssignmentEntities.FirstOrDefault(c => c.RecordGuid == studentCohortAssignmentsGuid));
                
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync("0003784")).ReturnsAsync("1df164eb-8178-4321-a9f7-24f27f3991d8");
                _personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1df164eb-8178-4321-a9f7-24f27f3991d8");
                var idCollection = new Dictionary<string, string>();
                idCollection.Add("0003784", "1df164eb-8178-4321-a9f7-24f27f3991d8");
                _personRepositoryMock.Setup(i => i.GetPersonGuidsCollectionAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(idCollection);
                
                _studentReferenceRepositoryMock.Setup(i => i.GetStudentCohortGuidAsync("2019")).ReturnsAsync("6eea82bc-c3f4-45c0-b0ef-a8f25b89ee31");
                _studentReferenceRepositoryMock.Setup(i => i.GetStudentCohortGuidAsync("OTHER")).ReturnsAsync("7e990bda-9427-4de6-b0ef-bba9b015e399");
                _studentReferenceRepositoryMock.Setup(i => i.GetStudentCohortGuidAsync("LOCAL")).ReturnsAsync("8f3aac22-e0b5-4159-b4e2-da158362c41b");

                _studentReferenceRepositoryMock.Setup(i => i.GetAcademicLevelsGuidAsync("UG")).ReturnsAsync("1df164eb-8178-4321-a9f7-24f27f3991d8");

            }

            [TestCleanup]
            public void Cleanup()
            {
                _personRepositoryMock = null;
                _studentRepositoryMock = null;
                _studentReferenceRepositoryMock = null;
                _studentCohortAssignmentRepositoryMock = null;
                _loggerMock = null;
                roleRepositoryMock = null;
                adapterRegistryMock = null;
                currentUserFactory = null;

                _studentCohortAssignmentsService = null;
                _studentCohortAssignmentsDtos = null;
                _studentCohortAssignmentEntities = null;
                _studentCohortAssignmentsTuple = null;
                _studentCohortEntities = null;
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsAsync()
            {
                var criteria = new StudentCohortAssignments();
                var filterQualifiers = new Dictionary<string, string>();
                var pageOfItems = await _studentCohortAssignmentsService.GetStudentCohortAssignmentsAsync(offset, limit, criteria, filterQualifiers, true);
                var results = pageOfItems.Item1;
                Assert.IsTrue(results is IEnumerable<StudentCohortAssignments>);
                Assert.IsNotNull(results);
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsAsync_Count()
            {
                var criteria = new StudentCohortAssignments();
                var filterQualifiers = new Dictionary<string, string>();
                var pageOfItems = await _studentCohortAssignmentsService.GetStudentCohortAssignmentsAsync(offset, limit, criteria, filterQualifiers, true);
                var results = pageOfItems.Item1;
                Assert.AreEqual(3, results.Count());
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsAsync_Properties()
            {
                var criteria = new StudentCohortAssignments();
                var filterQualifiers = new Dictionary<string, string>();
                var result =
                    (await _studentCohortAssignmentsService.GetStudentCohortAssignmentsAsync(offset, limit, criteria, filterQualifiers, true)).Item1.FirstOrDefault(x => x.Id == studentCohortAssignmentsGuid);
                Assert.IsNotNull(result.Id);
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsAsync_Expected()
            {
                var criteria = new StudentCohortAssignments();
                var filterQualifiers = new Dictionary<string, string>();
                var expectedResults = _studentCohortAssignmentsDtos.FirstOrDefault(c => c.Id == studentCohortAssignmentsGuid);
                var actualResult =
                    (await _studentCohortAssignmentsService.GetStudentCohortAssignmentsAsync(offset, limit, criteria, filterQualifiers, true)).Item1.FirstOrDefault(x => x.Id == studentCohortAssignmentsGuid);

                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Cohort.Id, actualResult.Cohort.Id);
                Assert.AreEqual(expectedResults.Person.Id, actualResult.Person.Id);
                Assert.AreEqual(expectedResults.StartOn, actualResult.StartOn);
                Assert.AreEqual(expectedResults.AcademicLevel.Id, actualResult.AcademicLevel.Id);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsByGuidAsync_Empty()
            {
                await _studentCohortAssignmentsService.GetStudentCohortAssignmentsByGuidAsync("");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsByGuidAsync_Null()
            {
                await _studentCohortAssignmentsService.GetStudentCohortAssignmentsByGuidAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsByGuidAsync_InvalidId()
            {
                _studentCohortAssignmentRepositoryMock.Setup(rp => rp.GetStudentCohortAssignmentByIdAsync("ABC")).ReturnsAsync(() => null);
                await _studentCohortAssignmentsService.GetStudentCohortAssignmentsByGuidAsync("ABC");
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsByGuidAsync_Expected()
            {
                var expectedResults =
                    _studentCohortAssignmentsDtos.First(c => c.Id == studentCohortAssignmentsGuid);
                var actualResult =
                    await _studentCohortAssignmentsService.GetStudentCohortAssignmentsByGuidAsync(studentCohortAssignmentsGuid);

                Assert.AreEqual(expectedResults.Id, actualResult.Id);
            }

            [TestMethod]
            public async Task StudentCohortAssignmentsService_GetStudentCohortAssignmentsByGuidAsync_Properties()
            {
                var expectedResults = _studentCohortAssignmentsDtos.First(nc => nc.Id == studentCohortAssignmentsGuid);
                var actualResult =
                    await _studentCohortAssignmentsService.GetStudentCohortAssignmentsByGuidAsync(studentCohortAssignmentsGuid);

                Assert.IsNotNull(actualResult.Id);
                Assert.AreEqual(expectedResults.Id, actualResult.Id);
                Assert.AreEqual(expectedResults.Cohort.Id, actualResult.Cohort.Id);
                Assert.AreEqual(expectedResults.Person.Id, actualResult.Person.Id);
                Assert.AreEqual(expectedResults.StartOn, actualResult.StartOn);
                Assert.AreEqual(expectedResults.AcademicLevel.Id, actualResult.AcademicLevel.Id);
            }
        }
    }
}
