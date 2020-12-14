using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.DataContracts;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Base.Exceptions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class GrantsRepositoryTests
    {
        [TestClass]
        public class GrantsRepositoryTests_V11
        {
            [TestClass]
            public class GrantsRepositoryTests_GETALL_GETBYID : BaseRepositorySetup
            {
                #region DECLARATIONS

                Collection<ProjectsCf> projectscf;

                Collection<DataContracts.Projects> projects;

                Collection<DataContracts.ProjectsLineItems> projectsln;

                private Defaults defaults;

                private LdmDefaults ldmDefaults;

                private Corp corp;

                private Dictionary<string, GuidLookupResult> lookUpResult;

                private GrantsRepository grantsRepository;

                private string guid = "adcbf49c-f129-470c-aa31-272493846751";

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {


                    MockInitialize();

                    InitializeTestData();

                    InitializeTestMock();

                    grantsRepository = new GrantsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object,apiSettings);
                }

                [TestCleanup]
                public void Cleanup()
                {
                    MockCleanup();

                    grantsRepository = null;

                }

                private void InitializeTestData() {

                    defaults = new Defaults() { DefaultHostCorpId = "1", Recordkey = "1" };

                    corp = new Corp() { CorpName = new List<string>() { "corp_001", "corp_002" }, CorpParents = new List<string>() { "corp_parent_001", "corp_parent_002" }, Recordkey = "1" };

                    ldmDefaults = new LdmDefaults() { Recordkey = "1", LdmdAddrDuplCriteria = "criteria_1", LdmdAddrTypeMapping = "mapping_1", LdmdArDefaultsEntityAssociation = new List<LdmDefaultsLdmdArDefaults>() { new LdmDefaultsLdmdArDefaults() { LdmdChargeTypesAssocMember = "charge_1", LdmdDefaultArCodesAssocMember = "ar_code_1" } }, LdmdChargeTypes = new List<string>() { "charge_1" }, LdmdPrinInvestigatorRole = "role_001" };
                    //projects = new Collection<ProjectCF>() {
                    //    new ProjectCF("adcbf49c-f129-470c-aa31-272493846751", "1", "proj_001", DateTime.Now){  AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, BudgetAmount=10, CurrentStatus="A", ProjectContactPerson = "1116968f-59f6-4a61-90f2-a94b7e1d6691", ProjectType= "proj_type_001" , EndOn = DateTime.Now,Title="Title_1", ReportingPeriods = new List<ReportingPeriod>(){ new ReportingPeriod() {  StartDate = DateTime.Now, EndDate = DateTime.Now } }, ReportingSegment = "Seg_01", SponsorReferenceCode = "Code_1"  },
                    //    new ProjectCF("adcbf49c-f129-470c-aa31-272493846752", "2", "proj_002", DateTime.Now) {AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, BudgetAmount=10, CurrentStatus="inactive", ProjectContactPerson = "1116968f-59f6-4a61-90f2-a94b7e1d6691", ProjectType= "proj_type_002" , EndOn = DateTime.Now,Title="Title_2", ReportingPeriods = new List<ReportingPeriod>(){ new ReportingPeriod() {  StartDate = DateTime.Now, EndDate = DateTime.Now } }, ReportingSegment = "Seg_02", SponsorReferenceCode = "Code_2"  }
                    //};

                    projectscf = new Collection<ProjectsCf>() {
                        new ProjectsCf(){ PrjcfFiscalYears = new List<string>(){ "2016", "2017" }, PrjcfLineItems = new List<string>(){ "ln_001", "ln_002" }, PrjcfPeriodEndDates = new List<DateTime?>(){ DateTime.Now, DateTime.Now.AddDays(4) }, PrjcfPeriodsEntityAssociation = new List<ProjectsCfPrjcfPeriods>(){ new ProjectsCfPrjcfPeriods() { PrjcfPeriodEndDatesAssocMember = DateTime.Now, PrjcfPeriodSeqNoAssocMember ="1", PrjcfPeriodStartDatesAssocMember = DateTime.Now }, new ProjectsCfPrjcfPeriods() { PrjcfPeriodEndDatesAssocMember = DateTime.Now, PrjcfPeriodSeqNoAssocMember="2", PrjcfPeriodStartDatesAssocMember= DateTime.Now }}, PrjcfPeriodSeqNo = new List<string>(){"1", "2" }, PrjcfPeriodStartDates = new List<DateTime?>(){ DateTime.Now, DateTime.Now }, RecordGuid="adcbf49c-f129-470c-aa31-272493846751", Recordkey="1", RecordModelName="" },
                        new ProjectsCf(){ PrjcfFiscalYears = new List<string>(){ "2016", "2017" }, PrjcfLineItems = new List<string>(){ "ln_001", "ln_002" }, PrjcfPeriodEndDates = new List<DateTime?>(){ DateTime.Now, DateTime.Now.AddDays(4) }, PrjcfPeriodsEntityAssociation = new List<ProjectsCfPrjcfPeriods>(){ new ProjectsCfPrjcfPeriods() { PrjcfPeriodEndDatesAssocMember = DateTime.Now, PrjcfPeriodSeqNoAssocMember ="1", PrjcfPeriodStartDatesAssocMember = DateTime.Now }, new ProjectsCfPrjcfPeriods() { PrjcfPeriodEndDatesAssocMember = DateTime.Now, PrjcfPeriodSeqNoAssocMember="2", PrjcfPeriodStartDatesAssocMember= DateTime.Now }}, PrjcfPeriodSeqNo = new List<string>(){"1", "2" }, PrjcfPeriodStartDates = new List<DateTime?>(){ DateTime.Now, DateTime.Now }, RecordGuid="adcbf49c-f129-470c-aa31-272493846752", Recordkey="2", RecordModelName="" }
                    };

                    projects = new Collection<DataContracts.Projects>() {
                        new DataContracts.Projects() { PrjAgencyRefNo = "ref_001", PrjContactPersonIds= new List<string>(){"1", "2" }, PrjContactRoles = new List<string>(){"role_001", "role_001" }, PrjContactsEntityAssociation = new List<ProjectsPrjContacts>(){ new ProjectsPrjContacts() { PrjContactPersonIdsAssocMember ="1", PrjContactRolesAssocMember="role_001" }, new ProjectsPrjContacts() { PrjContactPersonIdsAssocMember="2", PrjContactRolesAssocMember="role_002" } }, PrjCurrentStatus="Active", PrjEndDate = DateTime.Now, PrjRefNo="ref_001", PrjStartDate = DateTime.Now, PrjTitle ="title_001", PrjType ="proj_type_001", RecordGuid="adcbf49c-f129-470c-aa31-272493846751", Recordkey="1", RecordModelName=""  },
                        new DataContracts.Projects() { PrjAgencyRefNo = "ref_001", PrjContactPersonIds= new List<string>(){"1", "2" }, PrjContactRoles = new List<string>(){"role_001", "role_001" }, PrjContactsEntityAssociation = new List<ProjectsPrjContacts>(){ new ProjectsPrjContacts() { PrjContactPersonIdsAssocMember ="1", PrjContactRolesAssocMember="role_001" }, new ProjectsPrjContacts() { PrjContactPersonIdsAssocMember="2", PrjContactRolesAssocMember="role_002" } }, PrjCurrentStatus="Active", PrjEndDate = DateTime.Now, PrjRefNo="ref_002", PrjStartDate = DateTime.Now, PrjTitle ="title_002", PrjType ="proj_type_002", RecordGuid="adcbf49c-f129-470c-aa31-272493846752", Recordkey="2", RecordModelName=""  }
                    };

                    lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity= "PROJECTS.CF", PrimaryKey="1", SecondaryKey="1" } }, { "PROJECTS.CF", new GuidLookupResult() { Entity = "", PrimaryKey = "2", SecondaryKey = "2" } } };

                    projectsln = new Collection<DataContracts.ProjectsLineItems>()
                    {
                        new DataContracts.ProjectsLineItems(){ PrjlnProjectsCf = "1",  Recordkey = "1", PrjlnActualMemos = new List<decimal?>(){ 10,12}, PrjlnActualPosted = new List<decimal?>(){ 10 }, PrjlnAssetAmts = new List<decimal?>(){10 }, PrjlnBudgetAmts = new List<decimal?>(){10 }, PrjlnEncumbranceMemos = new List<decimal?>(){ 10}, PrjlnEncumbrancePosted = new List<decimal?>(){ 10}, PrjlnFundBalAmts = new List<decimal?>(){ 10}, PrjlnGlAcctEndDate = new List<DateTime?>(){ DateTime.Now.AddYears(1) }, PrjlnGlAccts = new List<string>(){ "111", "222" }, PrjlnGlAcctStartDates = new List<DateTime?>(){ DateTime.Now } , PrjlnGlEntityAssociation = new List<ProjectsLineItemsPrjlnGl>(){ new ProjectsLineItemsPrjlnGl() { PrjlnGlAcctEndDateAssocMember = DateTime.Now, PrjlnGlAcctsAssocMember = "Gl_mem", PrjlnGlAcctStartDatesAssocMember = DateTime.Now, PrjlnGlInactiveAssocMember = "inactive", PrjlnGlPrevInactiveAssocMember ="previnactive" } }, PrjlnGlClassType = "E",    }
                    };

                }

                private void InitializeTestMock() {

                    dataReaderMock.Setup(d => d.SelectAsync("PROJECTS.CF", It.IsAny<string>())).ReturnsAsync(new List<string>() { "1","2" }.ToArray<string>());
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<ProjectsCf>(It.IsAny<string[]>(), true)).ReturnsAsync(projectscf);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), true)).ReturnsAsync(projects);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.ProjectsLineItems>("PROJECTS.LINE.ITEMS", It.IsAny<string[]>(), true)).ReturnsAsync(projectsln);
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Defaults>(It.IsAny<string>(), It.IsAny<string>(),true)).ReturnsAsync(defaults);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Corp>("PERSON", It.IsAny<string>(), true)).ReturnsAsync(corp);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<LdmDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(ldmDefaults);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<ProjectsCf>(It.IsAny<string>(),true)).ReturnsAsync(projectscf.FirstOrDefault());
                    dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), true)).ReturnsAsync(projects.FirstOrDefault());

                }

                #endregion

                #region GETALL

                [TestMethod]
                public async Task GrantsRepository_GetGrantsAsync_FiscalYearId()
                {
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "", "2016", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                public async Task GrantsRepository_GetGrantsAsync_ReportingSeg()
                {
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "corp_001 corp_002", "", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                public async Task GrantsRepository_GetGrantsAsync_InvalidReportingSeg()
                {
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "rep_seg_001", "", false);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task GrantsRepository_GetGrantsAsync()
                {
                    var result = await grantsRepository.GetGrantsAsync(0, 10,"","" ,false);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                public async Task GrantsRepository_GetGrantsAsync_FiscalYearIdEmpty()
                {
                    lookUpResult = new Dictionary<string, GuidLookupResult>() { { "1", new GuidLookupResult() { Entity = "", PrimaryKey = "", SecondaryKey = "" } } };
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "", "2016", false);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GrantsRepository_GetGrantsAsync_FiscalYearId_ArgumentNullException()
                {
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "", "2016", false);
                }

                [TestMethod]
                [ExpectedException(typeof(ApplicationException))]
                public async Task GrantsRepository_GetGrantsAsync_ReportingSeg_ApplicationException()
                {
                    corp.CorpName = null;
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Corp>("PERSON", It.IsAny<string>(), true)).ReturnsAsync(corp);
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "repseg", "", false);
                }

                [TestMethod]
                [ExpectedException(typeof(ConfigurationException))]
                public async Task GrantsRepository_GetGrantsAsync_LdmDefaults_Null()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<LdmDefaults>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(null);
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "", "", false);
                }

                [TestMethod]
                public async Task GrantsRepository_GetGrantsAsync_Defaults_Null()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Defaults>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(null);
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "rep_seg_001", "", false);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task GrantsRepository_GetHostCountryAsync()
                {
                    var result = await grantsRepository.GetHostCountryAsync();
                    Assert.IsNotNull(result);
                }

                [TestMethod]
                public async Task GrantsRepository_GetHostCountryAsync_IntlParams_Null()
                {
                    var result = await grantsRepository.GetHostCountryAsync();
                    Assert.IsNotNull(result);
                }

                [TestMethod]
                public async Task GrantsRepository_GetGrantsAsync_ProjectIds_Empty()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("PROJECTS.CF", It.IsAny<string>())).ReturnsAsync(new List<string>() {  }.ToArray<string>());
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "", "", false);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GrantsRepository_GetGrantsAsync_ProjectRecordNotFound()
                {
                    projects[0].Recordkey = "3";
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string[]>(), true)).ReturnsAsync(projects);
                    var result = await grantsRepository.GetGrantsAsync(0, 10, "", "", false);
                }


                #endregion

                #region GETBYID


                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GrantsRepository_GetProjectsAsync_Guid_Null()
                {
                    await grantsRepository.GetProjectsAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GrantsRepository_GetProjectsAsync_PrimaryKey_Null()
                {
                    lookUpResult.FirstOrDefault().Value.PrimaryKey = null;
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                    await grantsRepository.GetProjectsAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GrantsRepository_GetProjectsAsync_Invalid_Entity()
                {
                    lookUpResult.FirstOrDefault().Value.Entity = "PROJECTS";
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(lookUpResult);
                    await grantsRepository.GetProjectsAsync(guid);
                }

                [TestMethod]
                public async Task GrantsRepository_GetProjectsAsync()
                {
                    var result = await grantsRepository.GetProjectsAsync(guid);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.RecordGuid, guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GrantsRepository_GetProjectsAsync_ProjectCFDC_Null()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<ProjectsCf>(It.IsAny<string>(), true)).ReturnsAsync(null);
                    await grantsRepository.GetProjectsAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GrantsRepository_GetProjectsAsync_ProjectDC_Null()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), true)).ReturnsAsync(null);
                    await grantsRepository.GetProjectsAsync(guid);
                }

                [TestMethod]
                public async Task GrantsRepository_GetProjectCFGuidsAsync() {

                    string[] projectIds = {"1", "2"};
                    var result = await grantsRepository.GetProjectCFGuidsAsync(projectIds);
                    Assert.IsNotNull(result);

                }

                #endregion
            }
        }
    }
}
