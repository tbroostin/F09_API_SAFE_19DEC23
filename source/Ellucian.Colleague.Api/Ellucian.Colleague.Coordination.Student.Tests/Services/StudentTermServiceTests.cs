// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Moq;
using slf4net;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentTermServiceTests
    {
        // Sets up a Current user that is a student and one that is an advisor
        public abstract class CurrentUserSetup
        {
            protected Role advisorRole = new Role(105, "Advisor");

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
        }

        [TestClass]
        public class GetStudentTermsGpas : CurrentUserSetup
        {
            private AcademicHistoryService academicHistoryService;
            private StudentTermService studentTermService;
            private Mock<IAcademicCreditRepository> academicCreditRepoMock;
            private IAcademicCreditRepository academicCreditRepo;
            private Mock<IAcademicCreditRepository> pilotAcademicCreditRepoMock;
            private IAcademicCreditRepository pilotAcademicCreditRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IStudentRepository studentRepo;
            private Mock<IStudentTermRepository> studentTermRepoMock;
            private IStudentTermRepository studentTermRepo;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;            
            private ILogger logger;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private ITermRepository termRepo;
            private ISectionRepository sectionRepo;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;

            [TestInitialize]
            public void Initialize()
            {
                academicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                academicCreditRepo = academicCreditRepoMock.Object;
                pilotAcademicCreditRepoMock = new Mock<IAcademicCreditRepository>();
                pilotAcademicCreditRepo = pilotAcademicCreditRepoMock.Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                studentTermRepoMock = new Mock<IStudentTermRepository>();
                studentTermRepo = studentTermRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;               
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;

                
                // Mock adapter for Pilot academic history level to Pilot student term GPA
                var pilotAcademicHistoryLevelDtoAdapter = new PilotAcademicHistoryLevelToPilotStudentTermLevelGpaDtoAdapter(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.PilotAcademicHistoryLevel, Ellucian.Colleague.Dtos.Student.PilotStudentTermLevelGpa>()).Returns(pilotAcademicHistoryLevelDtoAdapter);
                
                // Mock credits
                //List<string> academicCreditIds = new List<string>() { "1", "2", "3", "4" };
                Dictionary<string, List<Domain.Student.Entities.PilotAcademicCredit>> dictAcademicCredits = new Dictionary<string, List<Ellucian.Colleague.Domain.Student.Entities.PilotAcademicCredit>>();
                List<Domain.Student.Entities.PilotAcademicCredit> pilotAcademicCredits = new List<PilotAcademicCredit>();
                var credit = new PilotAcademicCredit("12345");
                credit.GpaCredit = 3.0m;
                credit.GradePoints = 12.0m;
                credit.AcademicLevelCode = "UG";
                credit.StudentId = "0000894";
                credit.TermCode = "2015/FA";
                pilotAcademicCredits.Add(credit);                    
                dictAcademicCredits.Add("0000894", pilotAcademicCredits.ToList());                
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                pilotAcademicCreditRepoMock.Setup(repo => repo.GetPilotAcademicCreditsByStudentIdsAsync(It.IsAny<List<string>>(), AcademicCreditDataSubset.None, false, false, "2015/FA")).Returns(Task.FromResult<Dictionary<string, List<Domain.Student.Entities.PilotAcademicCredit>>>(dictAcademicCredits));                
                
                // Set up current advisor user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();
                termRepo = null;
                sectionRepo = null;
                academicHistoryService = new AcademicHistoryService(adapterRegistry, studentRepo, academicCreditRepo, termRepo, sectionRepo, currentUserFactory, roleRepo, logger, baseConfigurationRepository);
                studentTermService = new StudentTermService(adapterRegistry, studentTermRepo, currentUserFactory, roleRepo, logger, studentRepo, pilotAcademicCreditRepo, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentRepo = null;
                academicCreditRepo = null;
                studentTermRepo = null;
                termRepo = null;
                sectionRepo = null;
                pilotAcademicCreditRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ReturnStudentTermsGpa_NoPermissions()
            {
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                IEnumerable<PilotStudentTermLevelGpa> pilotStudentTermLevelGpas = await studentTermService.QueryPilotStudentTermsGpaAsync(studentIds, "2015/FA");
                var bma = "test";
                Assert.AreEqual(1, pilotStudentTermLevelGpas.Count());
                decimal gpaValue = 4.0m;
                Assert.AreEqual(gpaValue, pilotStudentTermLevelGpas.ElementAt(0).TermGpa);                
            }

            [TestMethod]
            public async Task ReturnStudentTermsGpa_WithPermissions()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission("VIEW.STUDENT.INFORMATION"));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                IEnumerable<PilotStudentTermLevelGpa> pilotStudentTermLevelGpas = await studentTermService.QueryPilotStudentTermsGpaAsync(studentIds, "2015/FA");
                var bma = "test";
                Assert.AreEqual(1, pilotStudentTermLevelGpas.Count());
                decimal gpaValue = 4.0m;
                Assert.AreEqual(gpaValue, pilotStudentTermLevelGpas.ElementAt(0).TermGpa);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task ReturnStudentTermsGpa_WithWrongPermissions()
            {
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(PlanningPermissionCodes.ViewAnyAdvisee));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                IEnumerable<PilotStudentTermLevelGpa> pilotStudentTermLevelGpas = await studentTermService.QueryPilotStudentTermsGpaAsync(studentIds, "2015/FA");
                var bma = "test";
                Assert.AreEqual(1, pilotStudentTermLevelGpas.Count());
                decimal gpaValue = 4.0m;
                Assert.AreEqual(gpaValue, pilotStudentTermLevelGpas.ElementAt(0).TermGpa);
            }
        }
    }
}
