//Copyright 2019 Ellucian Company L.P. and its affiliates.

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
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Student.Tests.Services
{
    /// <summary>
    /// Tests the prospect-opportunities service layer
    /// </summary>
    [TestClass]
    public class ProspectOpportunitiesServiceTests
    {
        private Mock<IProspectOpportunitiesRepository> _prospectOpportunitiesRepositoryMock;
        private Mock<IPersonRepository> _personRepositoryMock;
        private Mock<IStudentAcademicProgramRepository> _studentAcademicProgramRepositoryMock;
        private Mock<ITermRepository> _termRepositoryMock;
        private Mock<IReferenceDataRepository> _referenceDataRepositoryMock;
        private Mock<IStudentReferenceDataRepository> _studentReferenceDataRepositoryMock;

        private Mock<IAdapterRegistry> _adapterRegistryMock;
        private Mock<IRoleRepository> _roleRepositoryMock;
        private Mock<IConfigurationRepository> _configurationRepoMock;
        private Mock<ILogger> _loggerMock;

        private IProspectOpportunitiesRepository _prospectOpportunitiesRepository;
        private IPersonRepository _personRepository;
        private IStudentAcademicProgramRepository _studentAcademicProgramRepository;
        private ITermRepository _termRepository;
        private IReferenceDataRepository _referenceDataRepository;
        private IStudentReferenceDataRepository _studentReferenceDataRepository;

        private IAdapterRegistry _adapterRegistry;
        private IRoleRepository _roleRepository;
        private IConfigurationRepository _configurationRepository;
        private ILogger logger;
        private ICurrentUserFactory userFactory;

        private IProspectOpportunitiesService _prospectOpportunitiesService;

        private Domain.Entities.Role _roleView = new Domain.Entities.Role(1, "VIEW.PROSPECT.OPPORTUNITY");
        private Domain.Entities.Role _roleUpdate = new Domain.Entities.Role(2, "UPDATE.PROSPECT.OPPORTUNITY");
        
        private IEnumerable<ProspectOpportunity> prospectOpportunityEntities;

        private ProspectOpportunity prospectOpp1;
        private ProspectOpportunity prospectOpp2;
        private ProspectOpportunity prospectOpp3;

        private ProspectOpportunitiesSubmissions prospectOppSubmissionDto1;
        private ProspectOpportunities prospectOppDto1;
        private Domain.Student.Entities.AdmissionApplication admissionApplication1;

        private List<Domain.Student.Entities.AcademicProgram> academicPrograms;
        private List<Domain.Student.Entities.AdmissionPopulation> admissionPopulations;
        private List<Domain.Base.Entities.Location> locations;

        private Dictionary<string, string> studentProgramsDict;

        private int offset = 0, limit = 100;

        [TestInitialize]
        public void Initialize()
        {

            _prospectOpportunitiesRepositoryMock = new Mock<IProspectOpportunitiesRepository>();
            _prospectOpportunitiesRepository = _prospectOpportunitiesRepositoryMock.Object;
            _personRepositoryMock = new Mock<IPersonRepository>();
            _personRepository = _personRepositoryMock.Object;
            _studentAcademicProgramRepositoryMock = new Mock<IStudentAcademicProgramRepository>();
            _studentAcademicProgramRepository = _studentAcademicProgramRepositoryMock.Object;
            _termRepositoryMock = new Mock<ITermRepository>();
            _termRepository = _termRepositoryMock.Object;
            _referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
            _referenceDataRepository = _referenceDataRepositoryMock.Object;
            _studentReferenceDataRepositoryMock = new Mock<IStudentReferenceDataRepository>();
            _studentReferenceDataRepository = _studentReferenceDataRepositoryMock.Object;
            _adapterRegistryMock = new Mock<IAdapterRegistry>();
            _adapterRegistry = _adapterRegistryMock.Object;
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _roleRepository = _roleRepositoryMock.Object;
            _configurationRepoMock = new Mock<IConfigurationRepository>();
            _configurationRepository = _configurationRepoMock.Object;
            _loggerMock = new Mock<ILogger>();
            logger = _loggerMock.Object;
            
            userFactory = new UserFactories.StudentUserFactory.ProspectOpportunitiesUser();

            InitializeData();
            InitializeMocks();


            _prospectOpportunitiesService = new ProspectOpportunitiesService(_prospectOpportunitiesRepository, _personRepository, _studentAcademicProgramRepository, _termRepository,
                                                                           _referenceDataRepository, _studentReferenceDataRepository, _adapterRegistry, userFactory, _roleRepository,
                                                                           _configurationRepository, logger);

        }

        private void InitializeMocks()
        {
            // prospect opportunity repo
            // Get all
            Tuple<IEnumerable<ProspectOpportunity>, int> tuple = new Tuple<IEnumerable<ProspectOpportunity>, int>(prospectOpportunityEntities, prospectOpportunityEntities.Count());
            _prospectOpportunitiesRepositoryMock.Setup(por => por.GetProspectOpportunitiesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ProspectOpportunity>(), It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(tuple);

            // Get
            _prospectOpportunitiesRepositoryMock.Setup(por => por.GetProspectOpportunityByIdAsync(prospectOpp1.Guid, It.IsAny<bool>())).ReturnsAsync(prospectOpp1);
            _prospectOpportunitiesRepositoryMock.Setup(por => por.GetProspectOpportunityByIdAsync(prospectOpp2.Guid, It.IsAny<bool>())).ReturnsAsync(prospectOpp2);
            _prospectOpportunitiesRepositoryMock.Setup(por => por.GetProspectOpportunityByIdAsync(prospectOpp3.Guid, It.IsAny<bool>())).ReturnsAsync(prospectOpp3);            
            _prospectOpportunitiesRepositoryMock.Setup(por => por.GetProspectOpportunitiesSubmissionsByGuidAsync(admissionApplication1.Guid, It.IsAny<bool>())).ReturnsAsync(admissionApplication1);
            _prospectOpportunitiesRepositoryMock.Setup(por => por.GetProspectOpportunityIdFromGuidAsync(prospectOppSubmissionDto1.Id)).ReturnsAsync("1");
            _prospectOpportunitiesRepositoryMock.Setup(por => por.UpdateProspectOpportunitiesSubmissionsAsync(admissionApplication1)).ReturnsAsync(prospectOpp1);
            _prospectOpportunitiesRepositoryMock.Setup(por => por.UpdateProspectOpportunitiesSubmissionsAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(prospectOpp1);
            _prospectOpportunitiesRepositoryMock.Setup(por => por.CreateProspectOpportunitiesSubmissionsAsync(It.IsAny<Domain.Student.Entities.AdmissionApplication>())).ReturnsAsync(prospectOpp1);

            // person repo
            _personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(prospectOpp1.ProspectId)).ReturnsAsync(prospectOpp1.Guid);
            _personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(prospectOpp2.ProspectId)).ReturnsAsync(prospectOpp2.Guid);
            _personRepositoryMock.Setup(pr => pr.GetPersonIdFromGuidAsync(prospectOpp3.ProspectId)).ReturnsAsync(prospectOpp3.Guid);

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add(prospectOpp1.ProspectId, prospectOpp1.Guid);
            dict.Add(prospectOpp2.ProspectId, prospectOpp2.Guid);
            dict.Add(prospectOpp3.ProspectId, prospectOpp3.Guid);
            _personRepositoryMock.Setup(pr => pr.GetPersonGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(dict);

            // student acad program repo
            _studentAcademicProgramRepositoryMock.Setup(sapr => sapr.GetStudentAcademicProgramGuidsCollectionAsync(It.IsAny<List<string>>())).ReturnsAsync(studentProgramsDict);

            // term repo
            _termRepositoryMock.Setup(tr => tr.GetAcademicPeriodsCodeFromGuidAsync(It.IsAny<string>())).ReturnsAsync("6592e027-2ff6-4ab9-b128-6fbb9f7e9119");
            _termRepositoryMock.Setup(tr => tr.GetAcademicPeriodsGuidAsync(It.IsAny<string>())).ReturnsAsync("2019/FA");
            var term = new Term("4201e71a-f601-4b3a-9ca9-7b5e43485dbf", "2019/FA","2019 Fall", new DateTime(2019, 09, 01), new DateTime(2019, 12, 15), 2020,
                    0, false, false, "2019/FA", false);
            _termRepositoryMock.Setup(tr => tr.GetAsync(It.IsAny<bool>())).ReturnsAsync(new List<Term>() { term });

            // reference data repo
            Domain.Base.Entities.PersonFilter filter = new Domain.Base.Entities.PersonFilter("personfilterguid", "personfiltercode", "a person filter");
            _referenceDataRepositoryMock.Setup(rd => rd.GetPersonFiltersAsync(It.IsAny<bool>())).ReturnsAsync(new List<Domain.Base.Entities.PersonFilter>() { filter });
            _referenceDataRepositoryMock.Setup(rd => rd.GetLocationsGuidAsync(It.IsAny<string>())).ReturnsAsync("64510e64-29fa-440c-8cf4-670ec3d3e095");
            _referenceDataRepositoryMock.Setup(rd => rd.GetLocationsAsync(It.IsAny<bool>())).ReturnsAsync(locations);

            // student reference data repo
            _studentReferenceDataRepositoryMock.Setup(rd => rd.GetAdmissionPopulationsGuidAsync(It.IsAny<string>())).ReturnsAsync("0ccfab36-eb32-4e94-9180-457e7096785c");
            _studentReferenceDataRepositoryMock.Setup(rd => rd.GetAcademicProgramsAsync(It.IsAny<bool>())).ReturnsAsync(academicPrograms);
            _studentReferenceDataRepositoryMock.Setup(rd => rd.GetAdmissionPopulationsAsync(It.IsAny<bool>())).ReturnsAsync(admissionPopulations);

            // role repo
            _roleView.AddPermission(new Domain.Entities.Permission("VIEW.PROSPECT.OPPORTUNITY"));
            _roleUpdate.AddPermission(new Domain.Entities.Permission("UPDATE.PROSPECT.OPPORTUNITY")); 
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleView });
        }

        private void InitializeData()
        {
            // For clarity:
            // Dtos.ProspectOpportunities
            // Domain.Student.Entities.ProspectOpportunity 

            prospectOpp1 = new ProspectOpportunity("6518d26e-ab8d-4aa0-95f0-f415fa7c0001", "APP001")
            {
                ProspectId = "STU001",
                EntryAcademicPeriod = "2019/FA",
                AdmissionPopulation = "FRESH",
                Site = "CAMPUS1",
                StudentAcadProgId = "MATH.BS"
            };

            prospectOpp2 = new ProspectOpportunity("6518d26e-ab8d-4aa0-95f0-f415fa7c0002", "APP002")
            {
                ProspectId = "STU002",
                EntryAcademicPeriod = "2019/FA",
                AdmissionPopulation = "FRESH",
                Site = "CAMPUS1",
                StudentAcadProgId = "BIOL.BS"
            };
            prospectOpp3 = new ProspectOpportunity("6518d26e-ab8d-4aa0-95f0-f415fa7c0003", "APP003")
            {
                ProspectId = "STU003",
                EntryAcademicPeriod = "2019/FA",
                AdmissionPopulation = "FRESH",
                Site = "CAMPUS1",
                StudentAcadProgId = "ART.BA"
            };
            prospectOpportunityEntities = new List<ProspectOpportunity>() { prospectOpp1, prospectOpp2, prospectOpp3 };
            
            var recruitAcademicProgramSubmission = new RecruitAcademicProgram() { Program = new GuidObject2() { Id = "45ef8c79-a2ed-43b1-810b-b93ea3deaf1d" } };
            prospectOppSubmissionDto1 = new ProspectOpportunitiesSubmissions()
            {
                Id = "6518d26e-ab8d-4aa0-95f0-f415fa7c0001",
                Prospect = new GuidObject2() { Id = "STU001" },
                EntryAcademicPeriod = new GuidObject2() { Id = "4201e71a-f601-4b3a-9ca9-7b5e43485dbf" },
                AdmissionPopulation = new GuidObject2() { Id = "0ccfab36-eb32-4e94-9180-457e7096785c" },
                Site = new GuidObject2() { Id = "64510e64-29fa-440c-8cf4-670ec3d3e095" },
                RecruitAcademicPrograms = new List<RecruitAcademicProgram> { recruitAcademicProgramSubmission }
            };

            var recruitAcademicProgram = new GuidObject2() { Id = "45ef8c79-a2ed-43b1-810b-b93ea3deaf1d" };
            prospectOppDto1 = new ProspectOpportunities()
            {
                Id = "6518d26e-ab8d-4aa0-95f0-f415fa7c0001",
                Prospect = new GuidObject2() { Id = "STU001" },
                EntryAcademicPeriod = new GuidObject2() { Id = "4201e71a-f601-4b3a-9ca9-7b5e43485dbf" },
                AdmissionPopulation = new GuidObject2() { Id = "0ccfab36-eb32-4e94-9180-457e7096785c" },
                Site = new GuidObject2() { Id = "64510e64-29fa-440c-8cf4-670ec3d3e095" },
                RecruitAcademicPrograms = new List<GuidObject2> { recruitAcademicProgram }
            };
            
            admissionApplication1 = new Domain.Student.Entities.AdmissionApplication("1668a39c-7716-47e6-ab9d-8c53c2d0f716")
            {
                ApplicantPersonId = "STU001",
                ApplicationStartTerm = "2019/FA",
                ApplicationAdmitStatus = "FRESH",
                ApplicationLocations = new List<string> { "CAMPUS1" },
                ApplicationAcadProgram = "MATH.BS"
            };
            
            studentProgramsDict = new Dictionary<string, string>();
            studentProgramsDict.Add("MATH.BS", "1da6bb4a-21c6-4b06-92a2-53171607813b");
            studentProgramsDict.Add("BIOL.BS", "fef80534-4094-40ba-91e4-c39fa74c5479");
            studentProgramsDict.Add("ART.BA", "fffcc007-51c2-4631-b940-7642a921fb6d");

            var academicProgram = new Domain.Student.Entities.AcademicProgram("45ef8c79-a2ed-43b1-810b-b93ea3deaf1d", "MATH.BS", "Bachelor of Science - Math") {};
            academicPrograms = new List<Domain.Student.Entities.AcademicProgram>(){ academicProgram };

            var admissionPopulation = new Domain.Student.Entities.AdmissionPopulation("0ccfab36-eb32-4e94-9180-457e7096785c", "FRESH", "Freshmen") { };
            admissionPopulations = new List<Domain.Student.Entities.AdmissionPopulation>() { admissionPopulation };

            var location = new Domain.Base.Entities.Location("64510e64-29fa-440c-8cf4-670ec3d3e095", "CAMPUS1", "Campus One");
            locations = new List<Domain.Base.Entities.Location>() { location };           
        }


        [TestCleanup]
        public void Cleanup()
        {
            _prospectOpportunitiesRepositoryMock = null;
            _prospectOpportunitiesRepository = null;
            _personRepositoryMock = null;
            _personRepository = null;
            _studentAcademicProgramRepositoryMock = null;
            _studentAcademicProgramRepository = null;
            _termRepositoryMock = null;
            _termRepository = null;
            _referenceDataRepositoryMock = null;
            _referenceDataRepository = null;
            _studentReferenceDataRepositoryMock = null;
            _studentReferenceDataRepository = null;
            _adapterRegistryMock = null;
            _adapterRegistry = null;
            _roleRepositoryMock = null;
            _roleRepository = null;
            _configurationRepoMock = null;
            _configurationRepository = null;
            _loggerMock = null;
            logger = null;
            userFactory = null;

        }


        [TestMethod]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesAsync()
        {
            var results = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(offset, limit, null, null, true);
            Assert.IsTrue(results is Tuple<IEnumerable<ProspectOpportunities>, int>);
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesAsync_roleUpdate_acceptable()
        {
            _roleUpdate.AddPermission(new Domain.Entities.Permission("UPDATE.PROSPECT.OPPORTUNITY"));
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleUpdate });
            var results = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(offset, limit, null, null, true);
        }


        [TestMethod]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesAsync_Count()
        {
            var results = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(offset, limit, null, null, true);
            Assert.AreEqual(3, results.Item2);
        }
        [TestMethod]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesAsync_Properties()
        {
            var result = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(offset, limit, null, null, true);

            foreach (var actual in result.Item1)
            {
                var expected = prospectOpportunityEntities.FirstOrDefault(poe => poe.Guid == actual.Id);
                Assert.IsNotNull(expected);
                Assert.AreEqual(expected.Guid, actual.Id);
                Assert.AreEqual("0ccfab36-eb32-4e94-9180-457e7096785c", actual.AdmissionPopulation.Id);
                Assert.AreEqual("2019/FA", actual.EntryAcademicPeriod.Id);
                Assert.AreEqual(expected.Guid, actual.Prospect.Id);
                Assert.AreEqual("64510e64-29fa-440c-8cf4-670ec3d3e095", actual.Site.Id);
                Assert.AreEqual(studentProgramsDict[expected.StudentAcadProgId], actual.RecruitAcademicPrograms.First().Id);
            }

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesByGuidAsync_Empty()
        {
            await _prospectOpportunitiesService.GetProspectOpportunitiesByGuidAsync("");
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesByGuidAsync_Null()
        {
            await _prospectOpportunitiesService.GetProspectOpportunitiesByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_PermissionsException()
        {
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
            await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(0, 100, null, null);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_Empty_Tuple_Invalid_Prospect_Id()
        {
            var criteria = new ProspectOpportunities()
            {
                Id = "64510e64-29fa-440c-8cf4-670ec3d3e095",
                Prospect = new GuidObject2(Guid.NewGuid().ToString())
            };
            //GetPersonIdFromGuidAsync
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(0, 100, criteria, null);
            Assert.AreEqual(actual.Item2, 0);
            Assert.AreEqual(actual.Item1.Count(), 0);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_Empty_Tuple_Valid_Prospect_Id()
        {
            var criteria = new ProspectOpportunities()
            {
                Id = "64510e64-29fa-440c-8cf4-670ec3d3e095",
                Prospect = new GuidObject2(Guid.NewGuid().ToString())
            };
            //GetPersonIdFromGuidAsync
            _personRepositoryMock.Setup(repo => repo.GetPersonIdFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(0, 100, criteria, null);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_Empty_Tuple_Invalid_EntryAcademicPeriod()
        {
            var criteria = new ProspectOpportunities()
            {
                Id = "64510e64-29fa-440c-8cf4-670ec3d3e095",
                EntryAcademicPeriod = new GuidObject2(Guid.NewGuid().ToString())
            };
            
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsCodeFromGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(0, 100, criteria, null);
            Assert.AreEqual(actual.Item2, 0);
            Assert.AreEqual(actual.Item1.Count(), 0);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_Empty_Tuple_Valid_EntryAcademicPeriod()
        {
            var criteria = new ProspectOpportunities()
            {
                Id = "64510e64-29fa-440c-8cf4-670ec3d3e095",
                EntryAcademicPeriod = new GuidObject2(Guid.NewGuid().ToString())
            };
            
            _termRepositoryMock.Setup(repo => repo.GetAcademicPeriodsCodeFromGuidAsync(It.IsAny<string>())).ReturnsAsync("1");
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(0, 100, criteria, null);
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_Empty_Tuple_Invalid_PersonFilter()
        {
            var criteria = new ProspectOpportunities()
            {
                Id = "64510e64-29fa-440c-8cf4-670ec3d3e095",
                Prospect = new GuidObject2(Guid.NewGuid().ToString())
            };

            _referenceDataRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(0, 100, null, Guid.NewGuid().ToString());
            Assert.AreEqual(actual.Item2, 0);
            Assert.AreEqual(actual.Item1.Count(), 0);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_Empty_Tuple_Valid_PersonFilter()
        {
            var criteria = new ProspectOpportunities()
            {
                Id = "64510e64-29fa-440c-8cf4-670ec3d3e095",
                Prospect = new GuidObject2(Guid.NewGuid().ToString())
            };

            _referenceDataRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(0, 100, null, Guid.NewGuid().ToString());
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task AdmissionDecisionsService_GetAdmissionDecisionsAsync_Empty_Tuple_Valid_PersonFilter_EmptyResults()
        {
            var criteria = new ProspectOpportunities()
            {
                Id = "64510e64-29fa-440c-8cf4-670ec3d3e095",
                Prospect = new GuidObject2(Guid.NewGuid().ToString())
            };

            _referenceDataRepositoryMock.Setup(repo => repo.GetPersonIdsByPersonFilterGuidAsync(It.IsAny<string>())).ReturnsAsync(new string[] {});
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesAsync(0, 100, null, Guid.NewGuid().ToString());
            Assert.IsNotNull(actual);
        }

        #region prospect-opportunities-submissions       

        [TestMethod]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesSubmissionsByGuidAsync()
        {
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesSubmissionsByGuidAsync(admissionApplication1.Guid);
            var expected = admissionApplication1;
            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Guid, actual.Id, "Guid");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesSubmissionsByGuidAsync_EmptyString()
        {
            await _prospectOpportunitiesService.GetProspectOpportunitiesSubmissionsByGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesSubmissionsByGuidAsync_Null()
        {
            await _prospectOpportunitiesService.GetProspectOpportunitiesSubmissionsByGuidAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task ProspectOpportunitiesService_GetProspectOpportunitiesSubmissionsByGuidAsync_KeyNotFoundException()
        {
            _prospectOpportunitiesRepositoryMock.Setup(por => por.GetProspectOpportunitiesSubmissionsByGuidAsync(admissionApplication1.Guid, It.IsAny<bool>()))
                .ThrowsAsync(new KeyNotFoundException());
            var actual = await _prospectOpportunitiesService.GetProspectOpportunitiesSubmissionsByGuidAsync(admissionApplication1.Guid);
        }
        
        // PUT
        [TestMethod]
        public async Task ProspectOpportunitiesService_UpdateProspectOpportunitiesSubmissionsAsync()
        {
            _roleUpdate.AddPermission(new Domain.Entities.Permission("UPDATE.PROSPECT.OPPORTUNITY"));
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleUpdate });
            var actual = await _prospectOpportunitiesService.UpdateProspectOpportunitiesSubmissionsAsync(prospectOppSubmissionDto1);
            var expected = prospectOppDto1;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id, "Guid");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ProspectOpportunitiesService_UpdateProspectOpportunitiesSubmissionsAsync_NoPermissions()
        {
            var actual = await _prospectOpportunitiesService.UpdateProspectOpportunitiesSubmissionsAsync(prospectOppSubmissionDto1);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task ProspectOpportunitiesService_UpdateAdmissionApplicationsSubmissionAsync_Validation_Null_Dto()
        {
            var actual = await _prospectOpportunitiesService.UpdateProspectOpportunitiesSubmissionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task ProspectOpportunitiesService_UpdateAdmissionApplicationsSubmissionAsync_Validation_Empty_DtoId()
        {
            _roleUpdate.AddPermission(new Domain.Entities.Permission("UPDATE.PROSPECT.OPPORTUNITY"));
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleUpdate });
            prospectOppSubmissionDto1.Id = " ";
            var actual = await _prospectOpportunitiesService.UpdateProspectOpportunitiesSubmissionsAsync(prospectOppSubmissionDto1);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task ProspectOpportunitiesService_UpdateAdmissionApplicationsSubmissionAsync_Validation_Applicant_Null()
        {
            prospectOppSubmissionDto1.Prospect = null;
            var actual = await _prospectOpportunitiesService.UpdateProspectOpportunitiesSubmissionsAsync(prospectOppSubmissionDto1);
        }
      
        // POST
        [TestMethod]
        public async Task ProspectOpportunitiesService_CreateProspectOpportunitiesSubmissionsAsync()
        {
            _roleUpdate.AddPermission(new Domain.Entities.Permission("UPDATE.PROSPECT.OPPORTUNITY"));
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleUpdate });
            var actual = await _prospectOpportunitiesService.CreateProspectOpportunitiesSubmissionsAsync(prospectOppSubmissionDto1);
            var expected = prospectOppDto1;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Id, actual.Id, "Guid");
        }

        [TestMethod]
        [ExpectedException(typeof(PermissionsException))]
        public async Task ProspectOpportunitiesService_CreateProspectOpportunitiesSubmissionsAsync_NoPermissions()
        {
            var actual = await _prospectOpportunitiesService.CreateProspectOpportunitiesSubmissionsAsync(prospectOppSubmissionDto1);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task ProspectOpportunitiesService_CreateAdmissionApplicationsSubmissionAsync_Validation_Null_Dto()
        {
            var actual = await _prospectOpportunitiesService.UpdateProspectOpportunitiesSubmissionsAsync(null);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task ProspectOpportunitiesService_CreateAdmissionApplicationsSubmissionAsync_Validation_Empty_DtoId()
        {
            _roleUpdate.AddPermission(new Domain.Entities.Permission("UPDATE.PROSPECT.OPPORTUNITY"));
            _roleRepositoryMock.Setup(i => i.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { _roleUpdate });
            prospectOppSubmissionDto1.Id = " ";
            var actual = await _prospectOpportunitiesService.CreateProspectOpportunitiesSubmissionsAsync(prospectOppSubmissionDto1);
        }

        [TestMethod]
        [ExpectedException(typeof(IntegrationApiException))]
        public async Task ProspectOpportunitiesService_CreateAdmissionApplicationsSubmissionAsync_Validation_Prospect_Null()
        {
            prospectOppSubmissionDto1.Prospect = null;
            var actual = await _prospectOpportunitiesService.CreateProspectOpportunitiesSubmissionsAsync(prospectOppSubmissionDto1);
        }
        
        #endregion
    }
}
