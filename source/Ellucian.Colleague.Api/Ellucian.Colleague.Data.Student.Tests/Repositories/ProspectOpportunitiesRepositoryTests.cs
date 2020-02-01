// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Data.Student.Transactions;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class ProspectOpportunitiesRepositoryTests : BaseRepositorySetup
    {
        Mock<IProspectOpportunitiesRepository> _prospectOpportunitiesRepositoryMock;
        IProspectOpportunitiesRepository _prospectOpportunitiesRepository;
        ProspectOpportunity criteriaObj = null;
        Applications applicationsDC;

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();
            _prospectOpportunitiesRepositoryMock = new Mock<IProspectOpportunitiesRepository>();
            _prospectOpportunitiesRepository = BuildValidProspectOpportunitiesRepository();
        }

        [TestCleanup]
        public void Cleanup()
        {
            transFactoryMock = null;
            cacheProviderMock = null;
            localCacheMock = null;
        }

        #region GET ALL
        [TestMethod]
        public async Task GetProspectOpportunitiesAsync_PersonFilter_EmptyResults()
        {
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, criteriaObj, new string[1]{"1"}, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }        

        [TestMethod]
        public async Task GetProspectOpportunitiesAsync_CriteriaObj_WithProspectIds()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applicants>(It.IsAny<string>(), true)).ReturnsAsync(null);
            criteriaObj = new ProspectOpportunity()
            {
                ProspectId = "3c083c69-7f58-42b0-ac38-3effec7fc7bd"
            };
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, criteriaObj, null, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }

        [TestMethod]
        public async Task GetProspectOpportunitiesAsync_CriteriaObj_ProspectId_NullApplicants()
        {
            Applicants applicants = new Applicants()
            {
                Recordkey = "1"
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applicants>(It.IsAny<string>(), true)).ReturnsAsync(applicants);
            criteriaObj = new ProspectOpportunity()
            {
                ProspectId = "3c083c69-7f58-42b0-ac38-3effec7fc7bd"
            };
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, criteriaObj, null, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }

        [TestMethod]
        public async Task GetProspectOpportunitiesAsync_CriteriaObj_ProspectId_WithApplications()
        {
            Applicants applicants = new Applicants()
            {
                Recordkey = "1",
                AppApplications = new List<string>() { "1" }
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applicants>(It.IsAny<string>(), true)).ReturnsAsync(applicants);
            criteriaObj = new ProspectOpportunity()
            {
                ProspectId = "3c083c69-7f58-42b0-ac38-3effec7fc7bd"
            };
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, criteriaObj, null, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }

        [TestMethod]
        public async Task GetProspectOpportunitiesAsync_CriteriaObj_EntryAcademicPeriod_NullLimitingKeys()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { });
            criteriaObj = new ProspectOpportunity()
            {
                EntryAcademicPeriod = "3c083c69-7f58-42b0-ac38-3effec7fc7bd"
            };
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, criteriaObj, null, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }

        [TestMethod]
        public async Task GetProspectOpportunitiesAsync_Null_ApplStatusesSpCodes()
        {
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, It.IsAny<ProspectOpportunity>(), new string[1] { "1" }, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }

        [TestMethod]
        public async Task GetProspectOpportunitiesAsync_ApplStatusesSpCodes_Null_LimitingKeys()
        {

            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, It.IsAny<ProspectOpportunity>(), null, It.IsAny<bool>());

            Assert.IsNotNull(result);
            Assert.AreEqual(true, !result.Item1.Any());
            Assert.AreEqual(0, result.Item2);
        }

        [TestMethod]
        public async Task GetProspectOpportunitiesAsyn()
        {
            applicationsDC = new Applications()
            {
                Recordkey = "1",
                RecordGuid = "1c083c69-7f58-42b0-ac38-3effec7fc7bc"
            };
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
            dataReaderMock.Setup(repo => repo.BulkReadRecordAsync<Applications>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<Applications>() { applicationsDC });

            RecordKeyLookupResult lookups = new RecordKeyLookupResult()
            {
                Guid = "1c083c69-7f58-42b0-ac38-3effec7fc7bc",
                ModelName = "APPLICATIONS"
            };
            Dictionary<string, RecordKeyLookupResult> recordKeyLookups = new Dictionary<string, RecordKeyLookupResult>();
            recordKeyLookups.Add("APPLICATIONS+1+1", lookups);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookups);
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, It.IsAny<ProspectOpportunity>(), null, It.IsAny<bool>());
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Item2);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunitiesAsync_ApplStatusesSpCodes_With_LimitingKeys_Exception()
        {

            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1" });

            RecordKeyLookupResult lookups = new RecordKeyLookupResult()
            {
                Guid = "1c083c69-7f58-42b0-ac38-3effec7fc7bc",
                ModelName = "APPLICATIONS"
            };
            Dictionary<string, RecordKeyLookupResult> recordKeyLookups = new Dictionary<string, RecordKeyLookupResult>();
            recordKeyLookups.Add("1", lookups);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookups);
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, It.IsAny<ProspectOpportunity>(), null, It.IsAny<bool>());
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunitiesAsync_ApplStatusesSpCodes_Null_Dictionary()
        {

            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(0, 100, It.IsAny<ProspectOpportunity>(), null, It.IsAny<bool>());
        }

        [TestMethod]
        public async Task GetProspectOpportunitiesAsync_ApplStatusesSpCodes_Null_Dictionary_With_Ids_Null_Result_Null()
        {

            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<string>())).ReturnsAsync(new string[] { "" });
            Dictionary<string, RecordKeyLookupResult> recordKeyLookups = new Dictionary<string, RecordKeyLookupResult>();
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookups);
            var result = await _prospectOpportunitiesRepository.GetProspectOpportunitiesAsync(10, 100, It.IsAny<ProspectOpportunity>(), null, It.IsAny<bool>());
            Assert.AreEqual(result.Item1.Count(), 0);
            Assert.AreEqual(result.Item2, 0);
        }

        #endregion

        #region GET BY ID

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetProspectOpportunityByIdAsync_ArgumentNullException()
        {
            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetProspectOpportunityByIdAsync_KeyNotFoundException()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task GetProspectOpportunityByIdAsync_WrongEntity_KeyNotFoundException()
        {
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATION", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunityByIdAsync_WrongEntity_RepositoryException()
        {
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "", LdmGuidSecondaryKey = "", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunityByIdAsync_NoStatusSpCodes()
        {
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunityByIdAsync_NoStatusSpCodes_BypassCache_True()
        {
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(ldmGuid);
            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc", true);
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunityByIdAsync_Null_ApplIntgKeyIdx()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            Applications applications = new Applications() { ApplIntgKeyIdx = string.Empty };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applications);

            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunityByIdAsync_ApplStatus_NotAny()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            Applications applications = new Applications() { ApplIntgKeyIdx = "1", ApplStatus = new List<string>() };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applications);

            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunityByIdAsync_ApplStatus_NoIntersectResult()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            Applications applications = new Applications() { ApplIntgKeyIdx = "1", ApplStatus = new List<string>() { "ABC" } };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applications);

            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunityByIdAsync_Dictionary_RepositoryException()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            Applications applications = new Applications() { ApplIntgKeyIdx = "1", ApplStatus = new List<string>() { "PR" } };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applications);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ThrowsAsync(new Exception());

            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task GetProspectOpportunityByIdAsync_Null_Dictionary()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            Applications applications = new Applications() { ApplIntgKeyIdx = "1", ApplStatus = new List<string>() { "PR" } };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applications);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(null);

            await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
        }

        [TestMethod]
        public async Task GetProspectOpportunityByIdAsync()
        {
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "PR", "HP" });
            LdmGuid ldmGuid = new LdmGuid() { LdmGuidEntity = "APPLICATIONS", LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX", LdmGuidPrimaryKey = "1", LdmGuidSecondaryKey = "1", Recordkey = "1c083c69-7f58-42b0-ac38-3effec7fc7bc" };
            Applications applications = new Applications() { ApplIntgKeyIdx = "1", Recordkey = "1", ApplStatus = new List<string>() { "PR" } };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(applications);
            RecordKeyLookupResult lookups = new RecordKeyLookupResult()
            {
                Guid = "1c083c69-7f58-42b0-ac38-3effec7fc7bc",
                ModelName = "APPLICATIONS"
            };
            Dictionary<string, RecordKeyLookupResult> recordKeyLookups = new Dictionary<string, RecordKeyLookupResult>();
            recordKeyLookups.Add("APPLICATIONS+1+1", lookups);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(recordKeyLookups);

            var result = await _prospectOpportunitiesRepository.GetProspectOpportunityByIdAsync("1c083c69-7f58-42b0-ac38-3effec7fc7bc");
            Assert.IsNotNull(result);
        }

        #endregion

        #region prospect-opportunity-submissions        
        [TestClass]
        public class ProspectOpportunitiesSubmissionsRepositoryTests : BaseRepositorySetup
        {
            #region DECLARATIONS

            private ProspectOpportunitiesRepository prospectOpportunitiesRepository;
            private UpdateProspectOrApplicantResponse response;
            private Dictionary<string, GuidLookupResult> dicResult;
            private Dictionary<string, RecordKeyLookupResult> rKeyLookUpResult;
            private AdmissionApplication entity;
            private Applications application;
            private LdmGuid ldmGuid;
            private Collection<ApplicationStatuses> applicationStatuses;
            private Collection<StudentPrograms> studentPrograms;
            private StudentPrograms studentProgram;
            private string guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();
                InitializeTestData();
                InitializeTestMock();
                prospectOpportunitiesRepository = new ProspectOpportunitiesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
                prospectOpportunitiesRepository = null;
                response = null;
                dicResult = null;
                rKeyLookUpResult = null;
                entity = null;
                application = null;
                applicationStatuses = null;
                studentPrograms = null;
                studentProgram = null;
            }

            private void InitializeTestData()
            {
                entity = new AdmissionApplication("1a49eed8-5fe7-4120-b1cf-f23266b9e874", "1")
                {
                    ApplicationStudentLoadIntent = "1",
                    ApplicationStartTerm = "1",
                    ApplicationAdmitStatus = "active",
                    ApplicationOwnerId = "1",
                    ApplicationSchool = "1",
                    ApplicationIntgType = "1",
                    ApplicationAcadProgram = "1",
                    AdmittedOn = DateTime.Now,
                    ApplicantPersonId = "1",
                    AppliedOn = DateTime.Now.AddDays(-20),
                    ApplicationComments = "comments",
                    MatriculatedOn = DateTime.Now.AddDays(-100),
                    ApplicationNo = "1",
                    ApplicationResidencyStatus = "1",
                    ApplicationLocations = new List<string>() { "1" },
                    ApplicationSource = "1",
                    ApplicationAttendedInstead = "1",
                    ApplicationWithdrawReason = "1",
                    WithdrawnOn = DateTime.Now.AddDays(-10)
                };

                entity.ApplicationDisciplines = new List<ApplicationDiscipline>()
            {
                new ApplicationDiscipline()
                {
                    AdministeringInstitutionUnit = "1",
                    Code = "Disc 1",
                    DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Major,

                },
                new ApplicationDiscipline()
                {
                    AdministeringInstitutionUnit = "1",
                    Code = "Disc 2",
                    DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Minor,

                },
                new ApplicationDiscipline()
                {
                    AdministeringInstitutionUnit = "1",
                    Code = "Disc 3",
                    DisciplineType = Domain.Base.Entities.AcademicDisciplineType.Concentration,

                }
            };

                dicResult = new Dictionary<string, GuidLookupResult>()
                {
                    {
                        "1a49eed8-5fe7-4120-b1cf-f23266b9e874", new GuidLookupResult(){Entity = "APPLICATIONS", PrimaryKey = "1" }
                    }
                };

                rKeyLookUpResult = new Dictionary<string, RecordKeyLookupResult>() { { "APPLICATIONS+1+1", new RecordKeyLookupResult { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874" }}};
                
                applicationStatuses = new Collection<ApplicationStatuses>()
                {
                    new ApplicationStatuses() {RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "2"}
                };

                application = new Applications()
                {
                    RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874",
                    Recordkey = "1",
                    ApplApplicant = "1",
                    ApplAcadProgram = "1",
                    ApplStatusesEntityAssociation = new List<ApplicationsApplStatuses>()
                    {
                        new ApplicationsApplStatuses(){ ApplStatusAssocMember = "1" }
                    },
                    ApplStatus = new List<String> { "1a49eed8-5fe7-4120-b1cf-f23266b9e874" },
                    ApplIntgKeyIdx = "1"                    
                };

                ldmGuid = new LdmGuid()
                {
                    LdmGuidEntity = "APPLICATIONS",
                    LdmGuidSecondaryFld = "APPL.INTG.KEY.IDX",
                    LdmGuidPrimaryKey = "1",
                    LdmGuidSecondaryKey = "1"
                };

                studentPrograms = new Collection<StudentPrograms>()
                {
                    new StudentPrograms(){RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "1*1"}
                };

                studentProgram = new StudentPrograms() { RecordGuid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", Recordkey = "1*1" };
                       
                response = new UpdateProspectOrApplicantResponse() { Guid = "1a49eed8-5fe7-4120-b1cf-f23266b9e874", Application = "1" };
            }

            private void InitializeTestMock()
            {
                transManagerMock.Setup(t => t.ExecuteAsync<UpdateProspectOrApplicantRequest, UpdateProspectOrApplicantResponse>(It.IsAny<UpdateProspectOrApplicantRequest>()))
                    .ReturnsAsync(response);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(rKeyLookUpResult);
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new string[] { "1a49eed8-5fe7-4120-b1cf-f23266b9e874" });
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<ApplicationStatuses>(It.IsAny<string>(), true)).ReturnsAsync(applicationStatuses);
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<StudentPrograms>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(studentPrograms);
                dataReaderMock.Setup(d => d.ReadRecordAsync<StudentPrograms>(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(studentProgram);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(application);
                dataReaderMock.Setup(d => d.ReadRecordAsync<Applications>(It.IsAny<string>(), It.IsAny<GuidLookup>(), true)).ReturnsAsync(application);
                dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(ldmGuid);
            }
            
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateProspectOpportunitiesSubmissionAsync_ArgumentNullException_Null_Entity()
            {
                await prospectOpportunitiesRepository.UpdateProspectOpportunitiesSubmissionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task UpdateProspectOpportunitiesSubmissionAsync_ArgumentNullException_Null_Id()
            {
                AdmissionApplication entity = new AdmissionApplication(" ");
                await prospectOpportunitiesRepository.UpdateProspectOpportunitiesSubmissionsAsync(entity);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task UpdateProspectOpportunitiesSubmissionAsync_RepositoryException()
            {
                response.UpdateProspectApplicantErrors = new List<UpdateProspectApplicantErrors>()
            {
                new UpdateProspectApplicantErrors(){ ErrorCodes = "Error" }
            };
                await prospectOpportunitiesRepository.UpdateProspectOpportunitiesSubmissionsAsync(entity);
            }

            [TestMethod]
            public async Task UpdateProspectOpportunitiesSubmissionAsync()
            {
                prospectOpportunitiesRepository.EthosExtendedDataDictionary = new Dictionary<string, string>();
                prospectOpportunitiesRepository.EthosExtendedDataDictionary.Add("key", "value");
                var actual = await prospectOpportunitiesRepository.UpdateProspectOpportunitiesSubmissionsAsync(entity);
                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.Guid, guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task CreateProspectOpportunitiesSubmissionAsync_ArgumentNullException()
            {
                var actual = await prospectOpportunitiesRepository.UpdateProspectOpportunitiesSubmissionsAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(RepositoryException))]
            public async Task CreateProspectOpportunitiesSubmissionAsync_RepositoryException()
            {
                response.UpdateProspectApplicantErrors = new List<UpdateProspectApplicantErrors>()
            {
                new UpdateProspectApplicantErrors(){ ErrorCodes = "Error" }
            };
                var actual = await prospectOpportunitiesRepository.UpdateProspectOpportunitiesSubmissionsAsync(entity);
            }

            [TestMethod]
            public async Task CreateProspectOpportunitiesSubmissionAsync()
            {
                prospectOpportunitiesRepository.EthosExtendedDataDictionary = new Dictionary<string, string>();
                prospectOpportunitiesRepository.EthosExtendedDataDictionary.Add("key", "value");

                var actual = await prospectOpportunitiesRepository.UpdateProspectOpportunitiesSubmissionsAsync(entity);
                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.Guid, guid);
            }

            [TestMethod]
            public async Task GetProspectOpportunitiesSubmissionByIdAsync()
            {
                var actual = await prospectOpportunitiesRepository.GetProspectOpportunitiesSubmissionsByGuidAsync("1234", It.IsAny<bool>());
                Assert.IsNotNull(actual);
                Assert.AreEqual(actual.Guid, guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetProspectOpportunitiesSubmissionByIdAsync_KeyNotFoundException()
            {
                dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                var actual = await prospectOpportunitiesRepository.GetProspectOpportunitiesSubmissionsByGuidAsync("1234", It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetProspectOpportunitiesSubmissionByIdAsync_StatusNUll_KeyNotFoundException()
            {
                dataReaderMock.Setup(d => d.SelectAsync("APPLICATION.STATUSES", It.IsAny<string>())).ReturnsAsync(null);
                var actual = await prospectOpportunitiesRepository.GetProspectOpportunitiesSubmissionsByGuidAsync("1234", It.IsAny<bool>());
            }
            #endregion
        }

        #endregion

        private ProspectOpportunitiesRepository BuildValidProspectOpportunitiesRepository()
        {            
            ProspectOpportunitiesRepository repository = new ProspectOpportunitiesRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            return repository;
        }
    }
}
