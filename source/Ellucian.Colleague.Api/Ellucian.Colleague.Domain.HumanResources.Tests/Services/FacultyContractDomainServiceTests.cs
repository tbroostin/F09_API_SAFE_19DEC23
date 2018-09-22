using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Services;
using Ellucian.Colleague.Domain.Student.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using System.Collections.ObjectModel;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Domain.HumanResources.Tests.Services
{
    [TestClass]
    public class FacultyContractDomainServiceTests : BaseRepositorySetup
    {
        #region DECLARATION
        private Mock<IFacultyContractRepository> facultyContractRepoMock;
        private Mock<ICampusOrganizationRepository> campusOrgRepoMock;
        private Mock<ISectionRepository> sectionRepoMock;
        private Mock<IPositionRepository> positionRepoMock;
        private Mock<IStudentReferenceDataRepository> studentReferenceDataRepoMock;
        private Mock<IHumanResourcesReferenceDataRepository> humanResourcesDataReferenceMock;

        private IFacultyContractRepository facultyContractRepo;
        private ICampusOrganizationRepository campusOrgRepo;
        private ISectionRepository sectionRepo;
        private IPositionRepository positionRepo;
        private IStudentReferenceDataRepository studentReferenceDataRepo;
        private IHumanResourcesReferenceDataRepository humanResourcesDataReferenceRepo;

        private FacultyContractDomainService facultyContractDomainService;

        private string facultyId = "0014076";
        private FacultyContract contract1;
        private FacultyContractPosition facContractPosition1;
        private FacultyContractPosition facContractPosition2;


        private Student.Entities.CampusOrgMemberRole memAssignment;
        private Student.Entities.CampusOrgAdvisorRole advAssignment1;
        private Student.Entities.CampusOrgAdvisorRole advAssignment2;
        private Student.Entities.CampusOrgRole orgRoleDesc;
        private CampusOrgRole orgRoleDesc2;
        private Student.Entities.CampusOrganization campusOrg1;
        private Student.Entities.CampusOrganization campusOrg2;
        private List<string> userHrpId;
        private Tuple<IEnumerable<Student.Entities.SectionFaculty>, int> facultyCountTuple;
        private List<SectionFaculty> sectionFac;
        private SectionFaculty faculty;
        private SectionFaculty faculty2;
        private FacultyContractAssignment facultyContractAssignment1;
        private FacultyContractAssignment facultyContractAssignment2;
        private FacultyContractAssignment facultyContractAssignment3;
        private FacultyContractAssignment facultyContractAssignment4;
        private FacultyContractAssignment facultyContractAssignment5;
        private Collection<Domain.HumanResources.Entities.AsgmtContractTypes> _assignmentContractTypesCollection;
        private Collection<Domain.Student.Entities.InstructionalMethod> _instructionalMethodsCollection;
        private Position position2;
        private Section sectionsRequested;
        private Section sectionsRequested2;
        private List<string> courseSecIds;
        private ICollection<OfferingDepartment> deptList;
        private OfferingDepartment dept;
        private IEnumerable<SectionStatusItem> statuses;
        private string courseLvlCode;
        private ICollection<string> courseLvlCodesList;


        #endregion

        #region TEST SETUP

        [TestInitialize]
        public void Initialize()
        {
            MockInitialize();
            facultyContractRepoMock = new Mock<IFacultyContractRepository>();
            campusOrgRepoMock = new Mock<ICampusOrganizationRepository>();
            sectionRepoMock = new Mock<ISectionRepository>();
            positionRepoMock = new Mock<IPositionRepository>();
            studentReferenceDataRepoMock = new Mock<IStudentReferenceDataRepository>();
            humanResourcesDataReferenceMock = new Mock<IHumanResourcesReferenceDataRepository>();

            facultyContractRepo = facultyContractRepoMock.Object;
            campusOrgRepo = campusOrgRepoMock.Object;
            sectionRepo = sectionRepoMock.Object;
            positionRepo = positionRepoMock.Object;
            studentReferenceDataRepo = studentReferenceDataRepoMock.Object;
            humanResourcesDataReferenceRepo = humanResourcesDataReferenceMock.Object;

            // Fill in with correct data
            // Links to facultycontractAssignment3
            memAssignment = new CampusOrgMemberRole("BIOS*0014076", "CA", 100, "292", new DateTime(2016, 12, 01), new DateTime(2016, 12, 01));
            orgRoleDesc = new CampusOrgRole("LAD", "Limited Advisor", "");
            orgRoleDesc2 = new CampusOrgRole("CA", "Captain", "2");
            advAssignment1 = new CampusOrgAdvisorRole("BIOS*0014076", "LAD", 100, "274", new DateTime(2016, 9, 1), new DateTime(2016, 12, 01));
            advAssignment2 = new CampusOrgAdvisorRole("HIKE*0014076", "LAD", 100, "246", new DateTime(2016, 9, 1), new DateTime(2016, 12, 01));

            // Create first contract
            contract1 = new FacultyContract("175", "Automatically Created FPA", "", "FPA", new DateTime(2016, 9, 1), new DateTime(2016, 12, 31), "16FL", 60, null);
            position2 = new Position("101", "title", "shortTitle", "positionDept", new DateTime(1991, 01, 01), true);

            // Create first position in contract 1
            facContractPosition1 = new FacultyContractPosition("246", "175", 60, "CALLIGRAPHER");
            // Create assignments in position 1
            facultyContractAssignment1 = new FacultyContractAssignment("274", "0014076", FacultyContractAssignmentType.CampusOrganizationAdvisor, "246", "BIOS*0014076", "312,500.00");
            facultyContractAssignment2 = new FacultyContractAssignment("289", "0014076", FacultyContractAssignmentType.CourseSectionFaculty, "246", "19277", "250,000.00");
            
            // Add assignments to position 1
            facContractPosition1.FacultyContractAssignments.Add(facultyContractAssignment1);
            facContractPosition1.FacultyContractAssignments.Add(facultyContractAssignment2);
            // Create position 2
            facContractPosition2 = new FacultyContractPosition("264", "175", 60, "ASTRONAUT");
            // Create assignments for position 2
            facultyContractAssignment3 = new FacultyContractAssignment("292", "0014076", FacultyContractAssignmentType.CampusOrganizationMember, "264", "BIOS*0014076", "1,250,000.00");
            facultyContractAssignment4 = new FacultyContractAssignment("299", "0014076", FacultyContractAssignmentType.CampusOrganizationAdvisor, "246", "HIKE*0014076", "");
            ///CourseSectionFaculty with no instructional method
            facultyContractAssignment5 = new FacultyContractAssignment("123", "0014076", FacultyContractAssignmentType.CourseSectionFaculty, "246", "19288", "250,000.00");
            facContractPosition1.FacultyContractAssignments.Add(facultyContractAssignment5);
            // Add assignments to position 2
            facContractPosition2.FacultyContractAssignments.Add(facultyContractAssignment3);
            facContractPosition2.FacultyContractAssignments.Add(facultyContractAssignment4);
            // Add positions to contract
            contract1.FacultyContractPositions.Add(facContractPosition1);
            contract1.FacultyContractPositions.Add(facContractPosition2);



            campusOrg1 = new Student.Entities.CampusOrganization("BIOS", "f2bba7af-e660-499e-bf50-8068fc40e5e2", "Amateur Bioinformatics Group", "", "ACAD");
            campusOrg2 = new CampusOrganization("HIKE", "3bb3a243-adf0-4fa2-bdba-5d33ad10acc9", "The Hiking Club", "0012283", "ATHL");
            userHrpId = new List<string>();
            sectionFac = new List<SectionFaculty>();
            faculty = new SectionFaculty("19277", "20050", "0014076", "LEC", new DateTime(2016, 9, 25), new DateTime(2016, 12, 14), 100);
            // Faculty for insturctional method without a type
            faculty2 = new SectionFaculty("19288", "20080", "0014076", "RANDOM", new DateTime(2016, 9, 25), new DateTime(2016, 12, 14), 100);
            sectionFac.Add(faculty);
            sectionFac.Add(faculty2);
            facultyCountTuple = new Tuple<IEnumerable<Student.Entities.SectionFaculty>, int>(sectionFac, 0);
            facultyContractDomainService = new FacultyContractDomainService(facultyContractRepo, campusOrgRepo, sectionRepo, positionRepo, studentReferenceDataRepo, humanResourcesDataReferenceRepo, logger);
            courseSecIds = new List<string>() { "20050","20080" };
            dept = new OfferingDepartment("BIOL", 100);
            courseLvlCode = "100";
            deptList = new List<OfferingDepartment>() { dept };
            statuses = new List<SectionStatusItem>();
            courseLvlCodesList = new List<string>() { courseLvlCode };

            userHrpId.Add("0014076");
            // Mock out a section
            sectionsRequested = new Section("20050", "110", "89", new DateTime(2016, 8, 25), 3, null, "Molecular Biology", "IN", deptList, courseLvlCodesList, "UG", statuses, true, true, true, true, false, false, false);
            sectionsRequested2 = new Section("20080", "110", "89", new DateTime(2016, 8, 25), 3, null, "Biology", "NI", deptList, courseLvlCodesList, "GU", statuses, true, true, true, true, false, false, false);

            _assignmentContractTypesCollection = new Collection<Domain.HumanResources.Entities.AsgmtContractTypes>()
            {
                    new Domain.HumanResources.Entities.AsgmtContractTypes("FPA","Fixed by Assignment w/ Subr")
            };
            _instructionalMethodsCollection = new Collection<Domain.Student.Entities.InstructionalMethod>()
            {
                    new Domain.Student.Entities.InstructionalMethod("0","LEC","Lecture",true)
                    
            };
        }


        [TestCleanup]
        public void Cleanup()
        {
            MockCleanup();
        }

        #endregion

        #region TEST METHODS
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task FacultyContractDomainService_GetFacultyContractsByFacultyIdAsync_FacultyIdNullCheck()
        {
            await facultyContractDomainService.GetFacultyContractsByFacultyIdAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task FacultyContractDomainService_GetFacultyContractsByFacultyIdAsync_FacultyIdEmpty()
        {
            var facultyId = string.Empty;
            await facultyContractDomainService.GetFacultyContractsByFacultyIdAsync(facultyId);
        }

        [TestMethod]
        public async Task FacultyContractDomainService_GetFacultyContractsByFacultyIdAsync_ContractCheck()
        {

            // Mocking out repo for facultyContractEntities declartion (contract1)
            facultyContractRepoMock.Setup(repo => repo.GetFacultyContractsByFacultyIdAsync(facultyId)).ReturnsAsync(new List<Domain.HumanResources.Entities.FacultyContract>() { contract1 });
            // Mocking out repo for positions (position1)

            // TODO: Fix below
            positionRepoMock.Setup(repo => repo.GetPositionsAsync()).ReturnsAsync(new List<Position>() { position2 });
            // Mocking out repo for campusOrganizationsEntities (campusOrg1)
            campusOrgRepoMock.Setup(org => org.GetCampusOrganizationsAsync(false)).ReturnsAsync(new List<Student.Entities.CampusOrganization>() { campusOrg1 });
            // Mocking out the repo for assignments


            // Mocking out the repo for memAssignmentInfoEntities

            campusOrgRepoMock.Setup(org => org.GetCampusOrgMembersAsync(userHrpId)).ReturnsAsync(new List<Student.Entities.CampusOrgMemberRole>() { memAssignment });

            campusOrgRepoMock.Setup(org => org.GetCampusOrgAdvisorsAsync(userHrpId)).ReturnsAsync(new List<Student.Entities.CampusOrgAdvisorRole>() { advAssignment1, advAssignment2 });
            // define orgRoleEntity
            studentReferenceDataRepoMock.Setup(repo => repo.CampusOrgRolesAsync()).ReturnsAsync(new List<Student.Entities.CampusOrgRole>() { orgRoleDesc, orgRoleDesc2 });

            sectionRepoMock.Setup(sec => sec.GetSectionFacultyAsync(0, 0, "", userHrpId.First(), new List<string>())).ReturnsAsync(facultyCountTuple);

            sectionRepoMock.Setup(sec => sec.GetNonCachedSectionsAsync(courseSecIds, false)).ReturnsAsync(new List<Section>() { sectionsRequested, sectionsRequested2 });

            humanResourcesDataReferenceMock.Setup(repo => repo.GetAssignmentContractTypesAsync(false)).ReturnsAsync(_assignmentContractTypesCollection);

            studentReferenceDataRepoMock.Setup(repo => repo.GetInstructionalMethodsAsync()).ReturnsAsync(_instructionalMethodsCollection);

            var contracts = await facultyContractDomainService.GetFacultyContractsByFacultyIdAsync(facultyId);

            // Test first assignment details of position 1 are correctly attached to assignment
            Assert.AreEqual(100, contracts.ElementAt(0).FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(0).IntendedLoad);
            Assert.AreEqual(new DateTime(2016, 9, 1), contracts.ElementAt(0).FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(0).StartDate);
            Assert.AreEqual(new DateTime(2016, 12, 1), contracts.ElementAt(0).FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(0).EndDate);
            Assert.AreEqual("Amateur Bioinformatics Group", contracts.ElementAt(0).FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(0).AssignmentDescription);

            // Test second assignment details of position 1 are correctly attached to assignment
            Assert.IsNull(contracts.ElementAt(0).FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(1).IntendedLoad);
            Assert.AreEqual(new DateTime(2016, 9, 25),contracts.ElementAt(0).FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(1).StartDate);
            Assert.AreEqual(new DateTime(2016, 12, 14),contracts.ElementAt(0).FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(1).EndDate);
            Assert.AreEqual(" Molecular Biology",contracts.ElementAt(0).FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(1).AssignmentDescription);

            // Test first assignment details of position 1 are correctly attached to assignment
            Assert.AreEqual(100,contracts.ElementAt(0).FacultyContractPositions.ElementAt(1).FacultyContractAssignments.ElementAt(0).IntendedLoad);
            Assert.AreEqual(new DateTime(2016, 12, 1), contracts.ElementAt(0).FacultyContractPositions.ElementAt(1).FacultyContractAssignments.ElementAt(0).StartDate);
            Assert.AreEqual(new DateTime(2016, 12, 1),contracts.ElementAt(0).FacultyContractPositions.ElementAt(1).FacultyContractAssignments.ElementAt(0).EndDate);
            Assert.AreEqual("Amateur Bioinformatics Group", contracts.ElementAt(0).FacultyContractPositions.ElementAt(1).FacultyContractAssignments.ElementAt(0).AssignmentDescription);

            // Verify the ContractType has changed from the code ("FT"), to the description (fully written out version, "Full Time") 
            Assert.AreEqual("Fixed by Assignment w/ Subr", contracts.First().ContractType);

            // Verify role on the course sec faculty is correct and not empty
            Assert.AreEqual("Lecture", contracts.First().FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(1).Role);
            Assert.AreEqual(null, contracts.First().FacultyContractPositions.ElementAt(0).FacultyContractAssignments.ElementAt(2).Role);
        }
        #endregion

    }
}
