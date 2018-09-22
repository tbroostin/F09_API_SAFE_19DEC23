// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Tests;
using slf4net;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Coordination.Student.Adapters;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class StudentAffiliationServiceTests
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
                            Name = "Sally",
                            PersonId = "S002",
                            SecurityToken = "321",
                            SessionTimeout = 30,
                            UserName = "Student",
                            Roles = new List<string>() { "Student" },
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
                            Roles = new List<string>() { "Advisor", "VIEW.STUDENT.INFORMATION" },
                            SessionFixationId = "abc123"
                        });
                    }
                }
            }
        }

        [TestClass]
        public class Get_StudentAffiliations : CurrentUserSetup
        {
            private StudentAffiliationService studentAffiliationService;
            private Mock<IStudentAffiliationRepository> studentAffiliationRepoMock;
            private IStudentAffiliationRepository studentAffiliationRepo;
            private ITermRepository termRepo;
            private Mock<ITermRepository> termRepoMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private List<Domain.Student.Entities.StudentAffiliation> studentAffiliation1 = new List<Domain.Student.Entities.StudentAffiliation>();
            private List<Domain.Student.Entities.StudentAffiliation> studentAffiliation2 = new List<Domain.Student.Entities.StudentAffiliation>();
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private ICurrentUserFactory currentUserFactory;
            private IStudentRepository studentRepo;
            private Mock<IStudentRepository> studentRepoMock;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;    


            [TestInitialize]
            public void Initialize()
            {
                studentAffiliationRepoMock = new Mock<IStudentAffiliationRepository>();
                studentAffiliationRepo = studentAffiliationRepoMock.Object;
                termRepoMock = new Mock<ITermRepository>();
                termRepo = termRepoMock.Object;

                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                logger = new Mock<ILogger>().Object;
                studentRepoMock = new Mock<IStudentRepository>();
                studentRepo = studentRepoMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;


                // Mock terms repository response
                List<Domain.Student.Entities.Term> termsList = new List<Domain.Student.Entities.Term>();
                termsList.Add(new Domain.Student.Entities.Term("2015/SP","Spring 2015 Term", DateTime.Parse("2015-01-13"), DateTime.Parse("2015-04-12"), 2015,1, false, false, "2015/SP", false));
                termsList.Add(new Domain.Student.Entities.Term("2015/SU", "Summer 2015 Term", DateTime.Parse("2015-04-13"), DateTime.Parse("2015-10-12"), 2015, 1, false, false, "2015/SU", false));
                termsList.Add(new Domain.Student.Entities.Term("2015/S1", "Summer I 2015 Term", DateTime.Parse("2015-04-13"), DateTime.Parse("2015-07-12"), 2015, 1, false, false, "2015/SU", false));
                termsList.Add(new Domain.Student.Entities.Term("2015/S2", "Summer II 2015 Term", DateTime.Parse("2015-06-13"), DateTime.Parse("2015-10-12"), 2015, 1, false, false, "2015/SU", false));
                termRepoMock.Setup(repo => repo.Get()).Returns(termsList);

                // Mock student affiliations repository responses
                studentAffiliation1 = new List<Domain.Student.Entities.StudentAffiliation>();
                studentAffiliation1.Add( new Domain.Student.Entities.StudentAffiliation("0000894", "SOCR")
                    {
                        AffiliationName = "Soccer",
                        StartDate = DateTime.Parse("2015-06-22"),
                        RoleCode = "PL",
                        RoleName = "Player",
                        StatusCode = "A",
                        StatusName = "Active"
                    });              
                
                studentAffiliation2 = new List<Domain.Student.Entities.StudentAffiliation>();
                studentAffiliation2.Add( new Domain.Student.Entities.StudentAffiliation("0004002", "FB")
                    {
                        AffiliationName = "Football",
                        StartDate = DateTime.Parse("2015-01-22"),
                        EndDate = DateTime.Parse("2015-12-01"),
                        RoleCode = "CP",
                        RoleName = "Captain",
                        StatusCode = "A",
                        StatusName = "Active"
                    });
                List<string> studentIds = new List<string>() { "0004002" };
                studentAffiliationRepoMock.Setup(repo => repo.GetStudentAffiliationsByStudentIdsAsync(studentIds, null,  null)).Returns(Task.FromResult(studentAffiliation2.AsEnumerable()));

                // Mock Adapters
                var studentAffiliationDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentAffiliation, Ellucian.Colleague.Dtos.Student.StudentAffiliation>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.StudentAffiliation, Ellucian.Colleague.Dtos.Student.StudentAffiliation>()).Returns(studentAffiliationDtoAdapter);

                // Set up current user
                currentUserFactory = new CurrentUserSetup.AdvisorUserFactory();

                studentAffiliationService = new StudentAffiliationService(adapterRegistry, studentAffiliationRepo, termRepo, currentUserFactory, roleRepo, logger, studentRepo, baseConfigurationRepository);
            }

            [TestCleanup]
            public void Cleanup()
            {
                studentAffiliationRepo = null;
                adapterRegistry = null;
                currentUserFactory = null;
                roleRepo = null;
                logger = null;
            }


            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task StudentNoViewPermissions_ThrowsPermissionError()
            {
                List<string> studentIds = new List<string>() { "0000894" };
                Dtos.Student.StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
                criteria.StudentIds = studentIds;
                criteria.Term = "2015/SP";

                var result = await studentAffiliationService.QueryStudentAffiliationsAsync(criteria);
            }

            [TestMethod]
            public async Task ReturnsEmptyStudentAffiliationsDto()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                var termData = new Domain.Student.Entities.Term("2015/SP", "Spring 2015 Term", DateTime.Parse("2015-01-13"), DateTime.Parse("2015-04-12"), 2015, 1, false, false, "2015/SP", false);
                termRepoMock.Setup(repo => repo.Get("2015/SP")).Returns(termData);

                List<string> studentIds = new List<string>() { "0000224" };
                List<Domain.Student.Entities.StudentAffiliation> studentAffiliation = new List<Domain.Student.Entities.StudentAffiliation>();
                studentAffiliationRepoMock.Setup(repo => repo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, null)).Returns(Task.FromResult(studentAffiliation.AsEnumerable()));
                                
                Dtos.Student.StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
                criteria.StudentIds = studentIds;
                criteria.Term = "2015/SP";

                var result =await studentAffiliationService.QueryStudentAffiliationsAsync(criteria);
                Assert.IsTrue(result.Count() == 0);
            }

            [TestMethod]
            public async Task ReturnsStudentAffiliationDto()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });
                
                var termData = new Domain.Student.Entities.Term("2015/SU", "Summer 2015 Term", DateTime.Parse("2015-04-13"), DateTime.Parse("2015-10-12"), 2015, 1, false, false, "2015/SU", false);
                termRepoMock.Setup(repo => repo.Get("2015/SU")).Returns(termData);

                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                studentAffiliationRepoMock.Setup(repo => repo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, null)).Returns(Task.FromResult(studentAffiliation1.AsEnumerable()));

                Dtos.Student.StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
                criteria.StudentIds = studentIds;
                criteria.Term = "2015/SU";

                var result = await studentAffiliationService.QueryStudentAffiliationsAsync(criteria);
                Assert.AreEqual(studentAffiliation1.ElementAt(0).StudentId, result.ElementAt(0).StudentId);
                Assert.AreEqual(studentAffiliation1.ElementAt(0).RoleCode, result.ElementAt(0).RoleCode);
                Assert.AreEqual(studentAffiliation1.ElementAt(0).RoleName, result.ElementAt(0).RoleName);
                Assert.AreEqual(studentAffiliation1.ElementAt(0).StartDate, result.ElementAt(0).StartDate);
                Assert.AreEqual(studentAffiliation1.ElementAt(0).EndDate, result.ElementAt(0).EndDate);
                Assert.AreEqual(result.ElementAt(0).Term, "2015/SU");
            }

            [TestMethod]
            public async Task ReturnsStudentsDto_FoundMatchingTerm()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                var termData = new Domain.Student.Entities.Term("2015/S2", "Summer II 2015 Term", DateTime.Parse("2015-06-13"), DateTime.Parse("2015-10-12"), 2015, 1, false, false, "2015/SU", false);
                termRepoMock.Setup(repo => repo.Get("2015/S2")).Returns(termData);
                
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                studentAffiliationRepoMock.Setup(repo => repo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, null)).Returns(Task.FromResult(studentAffiliation1.AsEnumerable()));

                Dtos.Student.StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
                criteria.StudentIds = studentIds;
                criteria.Term = "2015/S2";

                var result = await studentAffiliationService.QueryStudentAffiliationsAsync(criteria);
                Assert.AreEqual(result.ElementAt(0).Term, "2015/S2");
            }

            [TestMethod]
            public async Task ReturnsEmptyStudentsDto_NotFoundTerm()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                var termData = new Domain.Student.Entities.Term("2015/SP", "Spring 2015 Term", DateTime.Parse("2015-01-13"), DateTime.Parse("2015-04-12"), 2015, 1, false, false, "2015/SP", false);
                termRepoMock.Setup(repo => repo.Get("2015/SP")).Returns(termData);
                
                IEnumerable<string> studentIds = new List<string>() { "0000894" };
                studentAffiliationRepoMock.Setup(repo => repo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, null)).Returns(Task.FromResult(studentAffiliation1.AsEnumerable()));

                Dtos.Student.StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
                criteria.StudentIds = studentIds;
                criteria.Term = "2015/SP";

                var result = await studentAffiliationService.QueryStudentAffiliationsAsync(criteria);
                Assert.IsTrue(result.Count() == 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            //system.ArgumentNullException
            public async Task ThrowsExceptionForNoTerm()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                IEnumerable<string> studentIds = new List<string>() { "0004002" };
                studentAffiliationRepoMock.Setup(repo => repo.GetStudentAffiliationsByStudentIdsAsync(studentIds, null, null)).Returns(Task.FromResult(studentAffiliation2.AsEnumerable()));

                Dtos.Student.StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
                criteria.StudentIds = studentIds;

                var result = await studentAffiliationService.QueryStudentAffiliationsAsync(criteria);
                Assert.AreEqual(result.ElementAt(0).Term, "2015/SP");
            }

            [TestMethod]
            public async Task ReturnsStudentsDto_FoundSecondTerm()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                var termData = new Domain.Student.Entities.Term("2015/S1", "Summer I 2015 Term", DateTime.Parse("2015-04-13"), DateTime.Parse("2015-07-12"), 2015, 1, false, false, "2015/SU", false);
                termRepoMock.Setup(repo => repo.Get("2015/S1")).Returns(termData);

                IEnumerable<string> studentIds = new List<string>() { "0004002" };
                studentAffiliationRepoMock.Setup(repo => repo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, null)).Returns(Task.FromResult(studentAffiliation2.AsEnumerable()));

                Dtos.Student.StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
                criteria.StudentIds = studentIds;
                criteria.Term = "2015/S1";

                var result = await studentAffiliationService.QueryStudentAffiliationsAsync(criteria);
                Assert.AreEqual(result.ElementAt(0).Term, "2015/S1");
            }

            [TestMethod]
            public async Task ReturnsStudentsDto_FoundThirdTerm()
            {
                // Set up needed permission
                advisorRole.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewStudentInformation));
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Role>() { advisorRole });

                var termData = new Domain.Student.Entities.Term("2015/S2", "Summer II 2015 Term", DateTime.Parse("2015-06-13"), DateTime.Parse("2015-10-12"), 2015, 1, false, false, "2015/SU", false);
                termRepoMock.Setup(repo => repo.Get("2015/S2")).Returns(termData);

                IEnumerable<string> studentIds = new List<string>() { "0004002" };
                studentAffiliationRepoMock.Setup(repo => repo.GetStudentAffiliationsByStudentIdsAsync(studentIds, termData, null)).Returns(Task.FromResult(studentAffiliation2.AsEnumerable()));

                Dtos.Student.StudentAffiliationQueryCriteria criteria = new StudentAffiliationQueryCriteria();
                criteria.StudentIds = studentIds;
                criteria.Term = "2015/S2";

                var result = await studentAffiliationService.QueryStudentAffiliationsAsync(criteria);
                Assert.AreEqual(result.ElementAt(0).Term, "2015/S2");
            }
        }
    }
}
