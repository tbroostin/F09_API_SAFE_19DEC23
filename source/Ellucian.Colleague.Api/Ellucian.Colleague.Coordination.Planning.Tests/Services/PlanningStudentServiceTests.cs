// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Coordination.Planning.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;

namespace Ellucian.Colleague.Coordination.Planning.Tests.Services
{
    [TestClass]
    public class PlanningStudentServiceTests
    {
        [TestClass]
        public class PlanningStudentServiceTests_QueryPlanningStudent : CurrentUserSetup
        {
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IPlanningStudentRepository> planningStudentRepoMock;
            private IPlanningStudentRepository planningStudentRepo;
            private IRoleRepository roleRepository;
            private Mock<IRoleRepository> roleRepoMock;
            private PlanningStudentService planningStudentService;
            private Dumper dumper;
            private ILogger logger;
            private ICurrentUserFactory currentUserFactory;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private Mock<IStaffRepository> staffRepoMock;
            private IStaffRepository staffRepo;

            string studentId;
            Domain.Student.Entities.PlanningStudent planningStudent;

            [TestInitialize]
            public void Initialize()
            {
                planningStudentRepoMock = new Mock<IPlanningStudentRepository>();
                planningStudentRepo = planningStudentRepoMock.Object;
                dumper = new Dumper();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;
                currentUserFactory = new StudentUserFactory();
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepository = roleRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                staffRepoMock = new Mock<IStaffRepository>();
                staffRepo = staffRepoMock.Object;

                var planningStudentAdapter = new AutoMapperAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Student.PlanningStudent>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.PlanningStudent, Dtos.Student.PlanningStudent>()).Returns(planningStudentAdapter);
                var advisementAdapter = new AutoMapperAdapter<Domain.Student.Entities.Advisement, Dtos.Student.Advisement>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Domain.Student.Entities.Advisement, Dtos.Student.Advisement>()).Returns(advisementAdapter);

                studentId = "0000894";
                planningStudent = new Domain.Student.Entities.PlanningStudent(studentId, "brown", 3, new List<string>() { "ENGL.BA", "MATH.BS" });
                // This will be the response regardless of the student requested--it's ok.
                planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudent));

                planningStudentService = new PlanningStudentService(adapterRegistry, planningStudentRepo, currentUserFactory, roleRepository, logger, studentRepo, baseConfigurationRepository,staffRepo);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task QueryPlanningStudents_WithOnlyViewPersonInformation_Permission()
            {
                List<string> planningStudents = new List<string>() { "0004723", "0011902" };
                var planningStudentsEntity = new List<Domain.Student.Entities.PlanningStudent>() {
                new Domain.Student.Entities.PlanningStudent("0004723","last name",null,null),
                new Domain.Student.Entities.PlanningStudent("0011902","last name",null,null)
                };
                planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudentsEntity.AsEnumerable()));
                currentUserFactory = new FacultyUserFactory();
                planningStudentService = new PlanningStudentService(adapterRegistry, planningStudentRepo, currentUserFactory, roleRepository, logger, studentRepo, baseConfigurationRepository, staffRepo);
                // Set up view assigned advisee permissions on advisor's role--so that advisor cannot access this student
                facultyRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });
                var result = await planningStudentService.QueryPlanningStudentsAsync(planningStudents);
                Assert.IsTrue(result.Dto is List<Dtos.Student.PlanningStudent>);
                Assert.AreEqual(2, result.Dto.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task QueryPlanningStudents_WithOnlyViewStudentInformation_Permission()
            {
                List<string> planningStudents = new List<string>() { "0004723", "0011902" };
                var planningStudentsEntity = new List<Domain.Student.Entities.PlanningStudent>() {
                new Domain.Student.Entities.PlanningStudent("0004723","last name",null,null),
                new Domain.Student.Entities.PlanningStudent("0011902","last name",null,null)
                };
                planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudentsEntity.AsEnumerable()));
                currentUserFactory = new FacultyUserFactory();
                planningStudentService = new PlanningStudentService(adapterRegistry, planningStudentRepo, currentUserFactory, roleRepository, logger, studentRepo, baseConfigurationRepository, staffRepo);
                // Set up view assigned advisee permissions on advisor's role--so that advisor cannot access this student
                facultyRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });
                var result = await planningStudentService.QueryPlanningStudentsAsync(planningStudents);
                Assert.IsTrue(result.Dto is List<Dtos.Student.PlanningStudent>);
                Assert.AreEqual(2, result.Dto.Count());
            }
            [TestMethod]
            public async Task QueryPlanningStudents_WithBoth_Permissions()
            {
                List<string> planningStudents = new List<string>() { "0004723", "0011902" };
                var planningStudentsEntity = new List<Domain.Student.Entities.PlanningStudent>() {
                new Domain.Student.Entities.PlanningStudent("0004723","last name",null,null),
                new Domain.Student.Entities.PlanningStudent("0011902","last name",null,null)
                };
                planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudentsEntity.AsEnumerable()));
                currentUserFactory = new FacultyUserFactory();
                planningStudentService = new PlanningStudentService(adapterRegistry, planningStudentRepo, currentUserFactory, roleRepository, logger, studentRepo, baseConfigurationRepository, staffRepo);
                // Set up view assigned advisee permissions on advisor's role--so that advisor cannot access this student
                facultyRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation));
                facultyRole.AddPermission(new Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { facultyRole });
                var result = await planningStudentService.QueryPlanningStudentsAsync(planningStudents);
                Assert.IsTrue(result.Dto is List<Dtos.Student.PlanningStudent>);
                Assert.AreEqual(2, result.Dto.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task  QueryPlanningStudents_NoPermission_ThrowsException()
            {
                List<string> planningStudents = new List<string>() { "0004723", "0011902" };
                var planningStudentsEntity = new List<Domain.Student.Entities.PlanningStudent>() {
                new Domain.Student.Entities.PlanningStudent("0004723","last name",null,null),
                new Domain.Student.Entities.PlanningStudent("0011902","last name",null,null)
                };
                planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudentsEntity.AsEnumerable()));
                currentUserFactory = new FacultyUserFactory();
                planningStudentService = new PlanningStudentService(adapterRegistry, planningStudentRepo, currentUserFactory, roleRepository, logger, studentRepo, baseConfigurationRepository, staffRepo);
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { facultyRole });
                var result = await planningStudentService.QueryPlanningStudentsAsync(planningStudents);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task QueryPlanningStudents_EmptyIds()
            {
                List<string> planningStudents = new List<string>();
                var planningStudentsEntity = new List<Domain.Student.Entities.PlanningStudent>() {
                new Domain.Student.Entities.PlanningStudent("0004723","last name",null,null),
                new Domain.Student.Entities.PlanningStudent("0011902","last name",null,null)
                };
                planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudentsEntity.AsEnumerable()));
                currentUserFactory = new FacultyUserFactory();
                planningStudentService = new PlanningStudentService(adapterRegistry, planningStudentRepo, currentUserFactory, roleRepository, logger, studentRepo, baseConfigurationRepository, staffRepo);
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { facultyRole });
                var result = await planningStudentService.QueryPlanningStudentsAsync(planningStudents);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task QueryPlanningStudents_NullIds()
            {
                List<string> planningStudents = null;
                var planningStudentsEntity = new List<Domain.Student.Entities.PlanningStudent>() {
                new Domain.Student.Entities.PlanningStudent("0004723","last name",null,null),
                new Domain.Student.Entities.PlanningStudent("0011902","last name",null,null)
                };
                planningStudentRepoMock.Setup(repo => repo.GetAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).Returns(Task.FromResult(planningStudentsEntity.AsEnumerable()));
                currentUserFactory = new FacultyUserFactory();
                planningStudentService = new PlanningStudentService(adapterRegistry, planningStudentRepo, currentUserFactory, roleRepository, logger, studentRepo, baseConfigurationRepository, staffRepo);
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { facultyRole });
                var result = await planningStudentService.QueryPlanningStudentsAsync(planningStudents);
            }

        }
    }
}
