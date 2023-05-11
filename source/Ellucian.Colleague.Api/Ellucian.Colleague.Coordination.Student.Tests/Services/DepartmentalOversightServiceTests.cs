// Copyright 2021-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Student.Services;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Domain.Student;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    [TestClass]
    public class DepartmentalOversightServiceTests
    {       

        [TestClass]
        public class SearchAsync : CurrentUserSetup
        {
            
            private Mock<ISectionRepository> sectionRepoMock;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepository;
            private DepartmentalOversightService departmentalOversightService;
            private ISectionRepository sectionRepo;
            private Mock<IFacultyRepository> facultyRepositoryMock;
            private IFacultyRepository facultyRepository;
            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private ILogger logger;
            private IAdapterRegistry adapterRegistry;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private ICurrentUserFactory currentUserFactory;
            private IConfigurationRepository baseConfigurationRepository;
            private Mock<IConfigurationRepository> baseConfigurationRepositoryMock;
            private IStudentConfigurationRepository studentConfigurationRepository;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
            private IStudentReferenceDataRepository studentReferenceDataRepository;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Dtos.Student.DeptOversightSearchCriteria criteria;
            private IEnumerable<Dtos.Student.DeptOversightSearchResult> deptOversightSearchResults;
            private IEnumerable<Colleague.Domain.Student.Entities.DeptOversightSearchResult> deptOversightSearchResultsdomain;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            [TestInitialize]
            public void Initialize()
            {
                sectionRepoMock = new Mock<ISectionRepository>();
                sectionRepo = sectionRepoMock.Object;
                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepository = personBaseRepoMock.Object;
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                baseConfigurationRepositoryMock = new Mock<IConfigurationRepository>();
                baseConfigurationRepository = baseConfigurationRepositoryMock.Object;
                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepository = studentReferenceDataRepositoryMock.Object;
                studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepository = studentConfigurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;
                // Set up current user
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                logger = new Mock<ILogger>().Object;
                facultyRepositoryMock = new Mock<IFacultyRepository>();
                facultyRepository = facultyRepositoryMock.Object;
                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;
                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                deptOversightSearchResultsdomain = new List<Colleague.Domain.Student.Entities.DeptOversightSearchResult>()
                {
                    new Colleague.Domain.Student.Entities.DeptOversightSearchResult()
                    {
                         FacultyId = "001",
                         Department = "HIST",
                         SectionIds = new List<string>() { "Section1", "Section2" }
                    },
                    new Colleague.Domain.Student.Entities.DeptOversightSearchResult()
                    {
                         FacultyId = "002",
                         Department = "MATH",
                         SectionIds = new List<string>() { "Section3", "Section4" }
                    }
                };

                IEnumerable<Colleague.Domain.Student.Entities.Term> terms = new List<Colleague.Domain.Student.Entities.Term>()
                {
                     new Colleague.Domain.Student.Entities.Term("T1", "T1", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false),
                     new Colleague.Domain.Student.Entities.Term("T2", "T2", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false),
                     new Colleague.Domain.Student.Entities.Term("T3", "T3", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false)
                };
                termRepositoryMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Term>>>(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Term>>(terms));

                // Initialize the DepartmentalOversightSearchCriteria for all the existing search string tests
                criteria = new Dtos.Student.DeptOversightSearchCriteria();

                var deptOversightSearchResultsAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Student.Entities.DeptOversightSearchResult, Ellucian.Colleague.Dtos.Student.DeptOversightSearchResult>(adapterRegistry, logger);
                adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Student.Entities.DeptOversightSearchResult, Ellucian.Colleague.Dtos.Student.DeptOversightSearchResult>()).Returns(deptOversightSearchResultsAdapter);
                sectionRepoMock.Setup(repo => repo.GetDeptOversightSectionDetails(It.IsAny<string>(),terms,new List<string>())).Returns(Task.FromResult<IEnumerable<Domain.Student.Entities.DeptOversightSearchResult>>(deptOversightSearchResultsdomain));
                var _allDepartments = new List<Domain.Base.Entities.Department>()
                {
                    new Domain.Base.Entities.Department("1C3B805D-CFE6-483B-86C3-4C20562F8C14", "D1", "Department 1", true),
                    new Domain.Base.Entities.Department("1C3B805D-CFE6-483B-86C3-4C20562F8C15", "D2", "Department 2", true)
                };
                _allDepartments[0].DepartmentalOversightIds.AddRange(new List<string>() { "0000015" });
                _allDepartments[1].DepartmentalOversightIds.AddRange(new List<string>() { "0000015" });
                referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(_allDepartments);

                // Mock config repo response
                Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration2 config = new Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration2();
                studentConfigurationRepositoryMock.Setup(repo => repo.GetFacultyGradingConfiguration2Async()).Returns(Task.FromResult(config));

                List<Colleague.Domain.Student.Entities.ScheduleTerm> scheduleterms = new List<Colleague.Domain.Student.Entities.ScheduleTerm>();
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAllScheduleTermsAsync(false)).Returns(Task.FromResult(scheduleterms.AsEnumerable()));

                // Mock permissions
                Ellucian.Colleague.Domain.Entities.Permission permissionViewPerson = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation);
                personRole.AddPermission(permissionViewPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });

                departmentalOversightService = new DepartmentalOversightService(adapterRegistry, sectionRepo, facultyRepository, personBaseRepository, termRepository, referenceDataRepository, 
                    baseConfigurationRepository, studentConfigurationRepository, studentReferenceDataRepository,currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                sectionRepo = null;
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            //search criteria is null
            public async Task ThrowsErrorWhenCriteriaNull()
            {
                // Act--search 
                await departmentalOversightService.SearchAsync(null, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            //Faculty nor Section keyword is passed
            public async Task ThrowsErrorWhenCriteriaHasNeitherKeyword()
            {
                // Act--search            
                await departmentalOversightService.SearchAsync(criteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenCriteriaHasBothKeywords()
            {
                // Act--search
                criteria.FacultyKeyword = "xxx";
                criteria.SectionKeyword = "yyy";
                await departmentalOversightService.SearchAsync(criteria, 1, 1);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task ThrowsErrorWhenKeywordOnlyBlanks()
            {
                // Act--search
                criteria.FacultyKeyword = " ";
                criteria.SectionKeyword = " ";
                await departmentalOversightService.SearchAsync(criteria, 1, 1);
            }

            [TestMethod]
            public async Task SearchAsync_id_returns_Success()
            {
                DeptOversightSearchCriteria deptOversightSearchCriteria = new DeptOversightSearchCriteria()
                {
                    FacultyKeyword = "",
                    SectionKeyword = "Section"
                };
                var responseDtos = await departmentalOversightService.SearchAsync(deptOversightSearchCriteria,10,1);
                // Assert
                //Assert.IsTrue(responseDtos is IEnumerable<Dtos.Student.DeptOversightSearchResult>);
                //Assert.AreEqual(2, responseDtos.Count());
            }
        }

        [TestClass]
        public class SearchFacultyAsync
        {
            protected Domain.Entities.Role personRole = new Domain.Entities.Role(105, "Faculty");
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private Mock<ISectionRepository> sectionRepositoryMock;
            private ISectionRepository sectionRepository;
            private Mock<IFacultyRepository> facultyRepositoryMock;
            private IFacultyRepository facultyRepository;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepository;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;
            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IStudentConfigurationRepository studentConfigurationRepository;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
            private IStudentReferenceDataRepository studentReferenceDataRepository;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;
            private DeptOversightSearchCriteria deptOversightSearchCriteria;
            private DepartmentalOversightService departmentalOversightService;
            private IEnumerable<Dtos.Student.DeptOversightSearchResult> deptOversightSearchResults;
            private IEnumerable<Domain.Student.Entities.Faculty> faculties;
            private Domain.Student.Entities.Section section1;
            private Domain.Student.Entities.Section section2;
            private Domain.Student.Entities.Section section3;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;               
                logger = new Mock<ILogger>().Object;

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                facultyRepositoryMock = new Mock<IFacultyRepository>();
                facultyRepository = facultyRepositoryMock.Object;


                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepository = personBaseRepoMock.Object;

                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepository = studentReferenceDataRepositoryMock.Object;

                studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepository = studentConfigurationRepositoryMock.Object;

                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                deptOversightSearchResults = BuildDeptOversightResponse();
                IEnumerable<string> searchIds = new List<string> { "12345", "67890" };
                facultyRepositoryMock.Setup(repo => repo.SearchFacultyByNameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(searchIds));

                string paddedId = "0000002";
                facultyRepositoryMock.Setup(repo => repo.GetPID2FacultyIdAsync(It.IsAny<string>())).Returns(Task.FromResult(paddedId));

                faculties = new List<Domain.Student.Entities.Faculty>()
                {
                    new Domain.Student.Entities.Faculty("0000001", "Quincy")
                    {
                        FirstName = "John",
                        MiddleName = "Arnold",
                        ProfessionalName = "Dr Johnathan Barker DMD"
                    },
                    new Domain.Student.Entities.Faculty("0000002", "Jones")
                    {
                        FirstName = "William",
                        MiddleName = string.Empty,
                        ProfessionalName = null
                    },
                };
                facultyRepositoryMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>>(repo => repo.GetFacultyByIdsAsync(It.IsAny<IEnumerable<string>>())).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Faculty>>(faculties));

                ICollection<OfferingDepartment> depts = new List<OfferingDepartment>() { new OfferingDepartment("D1", 50m), new OfferingDepartment("D2", 50m) };
                ICollection<string> clcs = new List<string>() { "100", "200", "300", "400" };
                List<SectionStatusItem> statuses = new List<SectionStatusItem>() { new SectionStatusItem(SectionStatus.Active, "A", DateTime.Today.AddYears(-5)) };
                section1 = new Domain.Student.Entities.Section("11", "11", "01", new DateTime(2011, 1, 1), 3, null, "Section1 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section1.TermId = "T1"; section1.EndDate = new DateTime(2011, 1, 20); section1.AddFaculty("0000001");

                section2 = new Domain.Student.Entities.Section("12", "11", "02", new DateTime(2011, 2, 1), 3, null, "Section2 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section2.TermId = "T2"; section2.EndDate = new DateTime(2011, 2, 20); section2.AddFaculty("0000001");

                section3 = new Domain.Student.Entities.Section("13", "12", "01", new DateTime(2011, 3, 1), 3, null, "Section3 Title", "IN", depts, clcs, "UG", statuses, true, false, true, false, true, false);
                section3.TermId = "T3"; section3.EndDate = new DateTime(2011, 3, 20); section3.AddFaculty("0000002");

                IEnumerable<Domain.Student.Entities.Section> facultySectionsAsync = new List<Domain.Student.Entities.Section>() { section1, section2, section3 };
                sectionRepositoryMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Section>>>(repo => repo.GetFacultySectionsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<bool>())).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Section>>(facultySectionsAsync));

                IEnumerable<Colleague.Domain.Student.Entities.Term> terms = new List<Colleague.Domain.Student.Entities.Term>()
                {
                     new Colleague.Domain.Student.Entities.Term("T1", "T1", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false),
                     new Colleague.Domain.Student.Entities.Term("T2", "T2", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false),
                     new Colleague.Domain.Student.Entities.Term("T3", "T3", "2011 Fall", new DateTime(2011, 09, 01), new DateTime(2011, 12, 15), 2012,
                    0, false, false, "2011/FA", false)
                };
                termRepositoryMock.Setup<Task<IEnumerable<Colleague.Domain.Student.Entities.Term>>>(repo => repo.GetRegistrationTermsAsync()).Returns(Task.FromResult<IEnumerable<Colleague.Domain.Student.Entities.Term>>(terms));
                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
               
                var _allDepartments = new List<Domain.Base.Entities.Department>()
                {
                    new Domain.Base.Entities.Department("1C3B805D-CFE6-483B-86C3-4C20562F8C14", "D1", "Department 1", true),
                    new Domain.Base.Entities.Department("1C3B805D-CFE6-483B-86C3-4C20562F8C14", "D2", "Department 2", true)
                };
                _allDepartments[0].DepartmentalOversightIds.AddRange(new List<string>() { "0000015"});
                _allDepartments[1].DepartmentalOversightIds.AddRange(new List<string>() { "0000015" });
                referenceDataRepositoryMock.Setup(repo => repo.GetDepartmentsAsync(It.IsAny<bool>())).ReturnsAsync(_allDepartments);

                // Mock config repo response
                Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration2 config = new Ellucian.Colleague.Domain.Student.Entities.FacultyGradingConfiguration2();
                studentConfigurationRepositoryMock.Setup(repo => repo.GetFacultyGradingConfiguration2Async()).Returns(Task.FromResult(config));

                List<Colleague.Domain.Student.Entities.ScheduleTerm> scheduleterms = new List<Colleague.Domain.Student.Entities.ScheduleTerm>();
                studentReferenceDataRepositoryMock.Setup(repo => repo.GetAllScheduleTermsAsync(false)).Returns(Task.FromResult(scheduleterms.AsEnumerable()));

                // Mock permissions
                Ellucian.Colleague.Domain.Entities.Permission permissionViewPerson = new Ellucian.Colleague.Domain.Entities.Permission(StudentPermissionCodes.ViewPersonInformation);               
                personRole.AddPermission(permissionViewPerson);
                roleRepoMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { personRole });
                departmentalOversightService = new DepartmentalOversightService(adapterRegistry, sectionRepository, facultyRepository, personBaseRepository, termRepository,referenceDataRepository, 
                    configurationRepository, studentConfigurationRepository, studentReferenceDataRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                sectionRepository = null;
                facultyRepository = null;
                departmentalOversightService = null;
                sectionRepositoryMock = null;
                configurationRepository = null;
                facultyRepositoryMock = null;
            }

            [TestMethod]
            public async Task SearchAsync_id_returns_Success()
            {
                deptOversightSearchCriteria = new DeptOversightSearchCriteria()
                {
                    FacultyKeyword = "12345",
                    SectionKeyword = ""
                };
                var responseDtos = await departmentalOversightService.SearchAsync(deptOversightSearchCriteria);
                // Assert
                Assert.IsTrue(responseDtos is IEnumerable<Dtos.Student.DeptOversightSearchResult>);
                Assert.AreEqual(4, responseDtos.Count());
            }

            [TestMethod]
            public async Task SearchAsync_partial_id_returns_Success()
            {
                deptOversightSearchCriteria = new DeptOversightSearchCriteria()
                {
                    FacultyKeyword = "2",
                    SectionKeyword = ""
                };
                var responseDtos = await departmentalOversightService.SearchAsync(deptOversightSearchCriteria);
                // Assert
                Assert.IsTrue(responseDtos is IEnumerable<Dtos.Student.DeptOversightSearchResult>);
                Assert.AreEqual(4, responseDtos.Count());
            }

            [TestMethod]
            public async Task SearchAsync_name_returns_Success()
            {
                deptOversightSearchCriteria = new DeptOversightSearchCriteria()
                {
                    FacultyKeyword = "Quincy",
                    SectionKeyword = ""
                };
                var responseDtos = await departmentalOversightService.SearchAsync(deptOversightSearchCriteria);
                // Assert
                Assert.IsTrue(responseDtos is IEnumerable<Dtos.Student.DeptOversightSearchResult>);
                Assert.AreEqual(4, responseDtos.Count());
            }

            private IEnumerable<Dtos.Student.DeptOversightSearchResult> BuildDeptOversightResponse()
            {
                List<Dtos.Student.DeptOversightSearchResult> deptOversightSearchResults = new List<Dtos.Student.DeptOversightSearchResult>();
                deptOversightSearchResults.Add(new Dtos.Student.DeptOversightSearchResult() { FacultyId = "12345", Department = "MATH", SectionIds = new List<string>() { "222222", "333333" } });
                deptOversightSearchResults.Add(new Dtos.Student.DeptOversightSearchResult() { FacultyId = "67890", Department = "ART", SectionIds = new List<string>() { "444444", "555555" } });
                return deptOversightSearchResults;
            }
        }

        [TestClass]
        public class GetDepartmentalOversightPermissionsAsync : DepartmentalOversightServiceTests
        {
            protected Role deptOversightRole = new Role(2, "Faculty");
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private IAdapterRegistry adapterRegistry;
            private ILogger logger;
            private IDepartmentalOversightService departmentalOversightService;
            private Mock<IConfigurationRepository> configurationRepositoryMock;
            private IConfigurationRepository configurationRepository;
            private ICurrentUserFactory currentUserFactory;
            private Mock<IRoleRepository> roleRepoMock;
            private IRoleRepository roleRepo;        
            private Mock<ISectionRepository> sectionRepositoryMock;
            private ISectionRepository sectionRepository;
            private Mock<IFacultyRepository> facultyRepositoryMock;
            private IFacultyRepository facultyRepository;
            private Mock<IPersonBaseRepository> personBaseRepoMock;
            private IPersonBaseRepository personBaseRepository;
            private Mock<ITermRepository> termRepositoryMock;
            private ITermRepository termRepository;
            private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
            private IReferenceDataRepository referenceDataRepository;
            private IStudentConfigurationRepository studentConfigurationRepository;
            private Mock<IStudentConfigurationRepository> studentConfigurationRepositoryMock;
            private IStudentReferenceDataRepository studentReferenceDataRepository;
            private Mock<IStudentReferenceDataRepository> studentReferenceDataRepositoryMock;

            private Ellucian.Colleague.Domain.Base.Entities.DepartmentalOversightPermissions oversightPermissions;

            [TestInitialize]
            public void Initialize()
            {
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                adapterRegistry = adapterRegistryMock.Object;
                logger = new Mock<ILogger>().Object;

                sectionRepositoryMock = new Mock<ISectionRepository>();
                sectionRepository = sectionRepositoryMock.Object;

                facultyRepositoryMock = new Mock<IFacultyRepository>();
                facultyRepository = facultyRepositoryMock.Object;

                personBaseRepoMock = new Mock<IPersonBaseRepository>();
                personBaseRepository = personBaseRepoMock.Object;

                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                configurationRepository = configurationRepositoryMock.Object;
                roleRepoMock = new Mock<IRoleRepository>();
                roleRepo = roleRepoMock.Object;

                termRepositoryMock = new Mock<ITermRepository>();
                termRepository = termRepositoryMock.Object;

                referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                referenceDataRepository = referenceDataRepositoryMock.Object;

                studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
                studentReferenceDataRepository = studentReferenceDataRepositoryMock.Object;

                studentConfigurationRepositoryMock = new Mock<IStudentConfigurationRepository>();
                studentConfigurationRepository = studentConfigurationRepositoryMock.Object;

                IEnumerable<string> permissionCodes = new List<string> { 
                    DepartmentalOversightPermissionCodes.ViewSectionAddAuthorizations,
                    DepartmentalOversightPermissionCodes.ViewSectionAttendance,
                    DepartmentalOversightPermissionCodes.ViewSectionBooks,
                    DepartmentalOversightPermissionCodes.ViewSectionCensus, 
                    DepartmentalOversightPermissionCodes.ViewSectionDropRoster, 
                    DepartmentalOversightPermissionCodes.ViewSectionFacultyConsents, 
                    DepartmentalOversightPermissionCodes.ViewSectionGrading, 
                    DepartmentalOversightPermissionCodes.ViewSectionPrerequisiteWaiver,
                    DepartmentalOversightPermissionCodes.ViewSectionRoster, 
                    DepartmentalOversightPermissionCodes.ViewSectionStudentPetitions, 
                    DepartmentalOversightPermissionCodes.ViewSectionWaitlists
                };

                oversightPermissions = new Ellucian.Colleague.Domain.Base.Entities.DepartmentalOversightPermissions(permissionCodes);

                var permissionDtoAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.DepartmentalOversightPermissions, Ellucian.Colleague.Dtos.Student.DepartmentalOversightPermissions>(adapterRegistry, logger);
                adapterRegistryMock.Setup(x => x.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.DepartmentalOversightPermissions, Ellucian.Colleague.Dtos.Student.DepartmentalOversightPermissions>()).Returns(permissionDtoAdapter);

                currentUserFactory = new CurrentUserSetup.PersonUserFactory();
                departmentalOversightService = new DepartmentalOversightService(adapterRegistry, sectionRepository, facultyRepository, personBaseRepository, termRepository, referenceDataRepository, 
                    configurationRepository, studentConfigurationRepository, studentReferenceDataRepository, currentUserFactory, roleRepo, logger);
            }

            [TestCleanup]
            public void Cleanup()
            {
                adapterRegistry = null;
                roleRepo = null;
                sectionRepository = null;
                facultyRepository = null;
                departmentalOversightService = null;
                sectionRepositoryMock = null;
                configurationRepository = null;
                facultyRepositoryMock = null;
            }

            [TestMethod]
            public async Task GetDepartmentalOversightPermissionsAsync_returns_Success()
            {
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionRoster));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionGrading));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionCensus));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionAddAuthorizations));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionAttendance));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionBooks));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionDropRoster));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionFacultyConsents));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionPrerequisiteWaiver));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionStudentPetitions));
                deptOversightRole.AddPermission(new Permission(DepartmentalOversightPermissionCodes.ViewSectionWaitlists));
                roleRepoMock.Setup(rpm => rpm.GetRolesAsync()).ReturnsAsync(new List<Role>() { deptOversightRole });

                var result = await departmentalOversightService.GetDepartmentalOversightPermissionsAsync();
                Assert.IsTrue(result.CanViewSectionAddAuthorizations);
                Assert.IsTrue(result.CanViewSectionAttendance);
                Assert.IsTrue(result.CanViewSectionBooks);
                Assert.IsTrue(result.CanViewSectionCensus);
                Assert.IsTrue(result.CanViewSectionDropRoster);
                Assert.IsTrue(result.CanViewSectionFacultyConsents);
                Assert.IsTrue(result.CanViewSectionGrading);
                Assert.IsTrue(result.CanViewSectionPrerequisiteWaiver);
                Assert.IsTrue(result.CanViewSectionRoster);
                Assert.IsTrue(result.CanViewSectionStudentPetitions);
                Assert.IsTrue(result.CanViewSectionWaitlists);
            }
        }
    }
}
