using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
   public class GrantsServiceTests
    {
        [TestClass]
        public class GrantsServiceTests_V11 {

            [TestClass]
            public class GrantsServiceTests_GETALL_GETBYID: GeneralLedgerCurrentUser
            {
                #region DECLARATIONS

                protected Domain.Entities.Role viewGrants = new Domain.Entities.Role(1, "VIEW.GRANTS");

                private Mock<IGrantsRepository> grantsRepositoryMock;
                private Mock<IPersonRepository> personRepositoryMock;
                private Mock<IColleagueFinanceReferenceDataRepository> financeReferenceDataRepositoryMock;
                private Mock<IReferenceDataRepository> referenceDataRepositoryMock;
                private Mock<IAdapterRegistry> adapterRegistryMock;
                private Mock<IRoleRepository> roleRepositoryMock;
                private Mock<IConfigurationRepository> configurationRepositoryMock;
                private Mock<ILogger> loggerMock;

                private GrantsUser currentUserFactory;

                private GrantsService grantsService;

                private IEnumerable<Grant> grant;

                private IEnumerable<ProjectType> projectType;

                private IEnumerable<Domain.Entities.Role> roles;

                private Tuple<IEnumerable<Grant>, int> grantTuple;

                private IEnumerable<ProjectCF> project;

                private Tuple<IEnumerable<ProjectCF>, int> projectTuple;

                private string guid = "adcbf49c-f129-470c-aa31-272493846751";

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                
                    grantsRepositoryMock = new Mock<IGrantsRepository>();
                    personRepositoryMock = new Mock<IPersonRepository>();
                    financeReferenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                    referenceDataRepositoryMock = new Mock<IReferenceDataRepository>();
                    
                    adapterRegistryMock = new Mock<IAdapterRegistry>();
                    roleRepositoryMock = new Mock<IRoleRepository>();
                    configurationRepositoryMock = new Mock<IConfigurationRepository>();
                    loggerMock = new Mock<ILogger>();

                    currentUserFactory = new GrantsUser();

                    InitializeTestData();

                    InitializeTestMock();

                    grantsService = new GrantsService(grantsRepositoryMock.Object, personRepositoryMock.Object, financeReferenceDataRepositoryMock.Object, referenceDataRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, configurationRepositoryMock.Object, loggerMock.Object);

                }

                private void InitializeTestData()
                {
                    grant = new List<Grant>() {
                        new Grant(){  AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, Amount = new Dtos.DtoProperties.AmountDtoProperty(){ Currency = Dtos.EnumProperties.CurrencyCodes.USD, Value=10 }, Category="Category_1", EndOn = DateTime.Now, Id = "21535293-fb77-48a7-9b41-7f34d5763a41", StartOn = DateTime.Now, Title="Title_1", Status = Dtos.EnumProperties.GrantsStatus.Active, ReferenceCode = "ref_001", PrincipalInvestigator = new GuidObject2("1116968f-59f6-4a61-90f2-a94b7e1d6691"), ReportingPeriods = new List<GrantsReportingPeriodProperty>(){ new GrantsReportingPeriodProperty() { Start = DateTime.Now, End = DateTime.Now } }, ReportingSegment = "Seg_01", SponsorReferenceCode = "Code_1" },
                        new Grant(){  AccountingStrings = new List<string>(){ "11-02-01-04-10506-53010*SENSING","11-02-01-04-10506-53011*SENSING","11-02-01-04-10506-53012*SENSING","11-02-01-04-10506-53013*SENSING","11-02-01-04-10506-53017*SENSING" }, Amount = new Dtos.DtoProperties.AmountDtoProperty(){ Currency = Dtos.EnumProperties.CurrencyCodes.CAD, Value=10 }, Category="Category_2", EndOn = DateTime.Now, Id = "21535293-fb77-48a7-9b41-7f34d5763a42", StartOn = DateTime.Now, Title="Title_2", Status = Dtos.EnumProperties.GrantsStatus.Inactive, ReferenceCode = "ref_002", PrincipalInvestigator = new GuidObject2("1116968f-59f6-4a61-90f2-a94b7e1d6692"), ReportingPeriods = new List<GrantsReportingPeriodProperty>(){ new GrantsReportingPeriodProperty() { Start = DateTime.Now, End = DateTime.Now } }, ReportingSegment = "Seg_02", SponsorReferenceCode = "Code_2" }
                };
                    grantTuple = new Tuple<IEnumerable<Grant>, int>(grant, 2);

                    project = new List<ProjectCF>() {
                        new ProjectCF("adcbf49c-f129-470c-aa31-272493846751", "1", "proj_001", DateTime.Now){  AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, BudgetAmount=10, CurrentStatus="A", ProjectContactPerson = new List<string>() { "1116968f-59f6-4a61-90f2-a94b7e1d6691" }, ProjectType= "proj_type_001" , EndOn = DateTime.Now,Title="Title_1", ReportingPeriods = new List<ReportingPeriod>(){ new ReportingPeriod() {  StartDate = DateTime.Now, EndDate = DateTime.Now } }, ReportingSegment = "Seg_01", SponsorReferenceCode = "Code_1"  },
                        new ProjectCF("adcbf49c-f129-470c-aa31-272493846752", "2", "proj_002", DateTime.Now) {AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, BudgetAmount=10, CurrentStatus="inactive", ProjectContactPerson = new List<string>() {"1116968f-59f6-4a61-90f2-a94b7e1d6691" }, ProjectType= "proj_type_002" , EndOn = DateTime.Now,Title="Title_2", ReportingPeriods = new List<ReportingPeriod>(){ new ReportingPeriod() {  StartDate = DateTime.Now, EndDate = DateTime.Now } }, ReportingSegment = "Seg_02", SponsorReferenceCode = "Code_2"  },
                        new ProjectCF("adcbf49c-f129-470c-aa31-272493846752", "2", "proj_002", DateTime.Now) {AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, BudgetAmount=10, CurrentStatus="inactive" , EndOn = DateTime.Now,Title="Title_2", ReportingPeriods = new List<ReportingPeriod>(){ new ReportingPeriod() {  StartDate = DateTime.Now, EndDate = DateTime.Now } }, ReportingSegment = "Seg_02", SponsorReferenceCode = "Code_2"  },
                        new ProjectCF("adcbf49c-f129-470c-aa31-272493846752", "2", "proj_002", DateTime.Now) {AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, CurrentStatus="inactive", ProjectContactPerson = new List<string>() {"1116968f-59f6-4a61-90f2-a94b7e1d6691" }, ProjectType= "proj_type_002" , EndOn = DateTime.Now,Title="Title_2", ReportingPeriods = new List<ReportingPeriod>(){ new ReportingPeriod() {  StartDate = DateTime.Now, EndDate = DateTime.Now } }, ReportingSegment = "Seg_02", SponsorReferenceCode = "Code_2"  }
                };
                    projectTuple = new Tuple<IEnumerable<ProjectCF>, int>(project, 2);

                    projectType = new List<ProjectType>() {
                        new ProjectType("proj_type_001", "project type 1"),
                        new ProjectType("proj_type_002", "project type 2")
                    };

                    roles = new List<Domain.Entities.Role>()
                    {
                        new Domain.Entities.Role(1,"VIEW.GRANTS")
                    };
                    
                    roles.FirstOrDefault().AddPermission(new Permission(ColleagueFinancePermissionCodes.ViewGrants));

                }

                private void InitializeTestMock()
                {
                    viewGrants.AddPermission(new Ellucian.Colleague.Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewGrants));
                    roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { viewGrants });
                    roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);
                    grantsRepositoryMock.Setup(g => g.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(projectTuple);
                    referenceDataRepositoryMock.Setup(i => i.GetProjectTypesAsync()).ReturnsAsync(projectType);
                    personRepositoryMock.Setup(i => i.IsCorpAsync(It.IsAny<string>())).ReturnsAsync(false);
                    personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync("1116968f-59f6-4a61-90f2-a94b7e1d6691");
                    grantsRepositoryMock.Setup(i => i.GetProjectsAsync(It.IsAny<string>())).ReturnsAsync(project.FirstOrDefault());

                }

                [TestCleanup]
                public void Cleanup()
                {
                    grantsService = null;
                    grantsRepositoryMock = null;
                    personRepositoryMock = null;
                    financeReferenceDataRepositoryMock = null;
                    referenceDataRepositoryMock = null;
                    adapterRegistryMock = null;
                    roleRepositoryMock = null;
                    configurationRepositoryMock = null;
                    loggerMock = null;
                    currentUserFactory = null;
                }

                #endregion

                #region GETALL
                
                [TestMethod]
                public async Task GrantService_GetGrantAsync_When_There_Are_No_Project_Records()
                {
                    grantsRepositoryMock.Setup(i => i.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), false)).ReturnsAsync(() => null);
                    var result = await grantsService.GetGrantsAsync(0, 100,"","", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task GrantsService_GetGrantsAsync()
                {
                    var result = await grantsService.GetGrantsAsync(0, 100,"","", false);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 2);
                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task GrantsService_GetGrantsAsync_TitleAsNull()
                {
                    projectTuple.Item1.FirstOrDefault().Title = null;
                    grantsRepositoryMock.Setup(g => g.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(projectTuple);
                    try
                    {
                        await grantsService.GetGrantsAsync(0, 100, "", "", false);
                    }
                    catch (IntegrationApiException ex)
                    {
                        Assert.IsTrue(ex.Errors != null && ex.Errors.Any(), "Errors are present with exception.");
                        Assert.AreEqual(string.Format("Title is required. Id: {0}", projectTuple.Item1.FirstOrDefault().RecordGuid), ex.Errors.FirstOrDefault().Message);
                        throw ex;
                    }
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GrantsService_GetGrantsAsync_CodeAsNull()
                {

                    project = new List<ProjectCF>() {
                        new ProjectCF("adcbf49c-f129-470c-aa31-272493846752", "2", "", DateTime.Now) {AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, BudgetAmount=10, CurrentStatus="inactive", ProjectContactPerson = new List<string>() {"1116968f-59f6-4a61-90f2-a94b7e1d6691" }, ProjectType= "proj_type_002" , EndOn = DateTime.Now,Title="Title_2", ReportingPeriods = new List<ReportingPeriod>(){ new ReportingPeriod() {  StartDate = DateTime.Now, EndDate = DateTime.Now } }, ReportingSegment = "Seg_02", SponsorReferenceCode = "Code_2"  }
                    };
                    projectTuple = new Tuple<IEnumerable<ProjectCF>, int>(project, 2);

                    grantsRepositoryMock.Setup(g => g.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(projectTuple);
                    await grantsService.GetGrantsAsync(0, 100, "", "", false);
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task GrantsService_GetGrantsAsync_StartOnAsNull()
                {
                    project = new List<ProjectCF>() {
                        new ProjectCF("adcbf49c-f129-470c-aa31-272493846752", "2", "proj_001", null) {AccountingStrings = new List<string>(){ "21-02-01-04-10506-53010*SENSING","21-02-01-04-10506-53011*SENSING","21-02-01-04-10506-53012*SENSING","21-02-01-04-10506-53013*SENSING","21-02-01-04-10506-53017*SENSING" }, BudgetAmount=10, CurrentStatus="inactive", ProjectContactPerson = new List<string>() {"1116968f-59f6-4a61-90f2-a94b7e1d6691" }, ProjectType= "proj_type_002" , EndOn = DateTime.Now,Title="Title_2", ReportingPeriods = new List<ReportingPeriod>(){ new ReportingPeriod() {  StartDate = DateTime.Now, EndDate = DateTime.Now } }, ReportingSegment = "Seg_02", SponsorReferenceCode = "Code_2"  }
                    };
                    projectTuple = new Tuple<IEnumerable<ProjectCF>, int>(project, 2);

                    grantsRepositoryMock.Setup(g => g.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(projectTuple);
                    await grantsService.GetGrantsAsync(0, 100, "", "", false);
                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task GrantsService_GetGrantsAsync_InvallidProjectType()
                {
                    grantsRepositoryMock.Setup(i => i.GetHostCountryAsync()).ReturnsAsync("CANADA");
                    projectTuple.Item1.FirstOrDefault().ProjectType = "Proj_0001";
                    grantsRepositoryMock.Setup(g => g.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(projectTuple);
                    try
                    {
                        await grantsService.GetGrantsAsync(0, 100, "", "", false);
                    }
                    catch (IntegrationApiException ex)
                    {
                        Assert.IsTrue(ex.Errors != null && ex.Errors.Any(), "Errors are present with exception.");
                        Assert.AreEqual(string.Format("Project type not found for code {0}.", projectTuple.Item1.FirstOrDefault().ProjectType), ex.Errors.FirstOrDefault().Message);
                        throw ex;
                    }
                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task GrantsService_GetGrantsAsync_ReportingPeriodStartDateAsNull()
                {
                    projectTuple.Item1.FirstOrDefault().ReportingPeriods.FirstOrDefault().StartDate = null;
                    grantsRepositoryMock.Setup(g => g.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(projectTuple);
                    try
                    {
                        await grantsService.GetGrantsAsync(0, 100, "", "", false);
                    }
                    catch (IntegrationApiException ex)
                    {
                        Assert.IsTrue(ex.Errors != null && ex.Errors.Any(), "Errors are present with exception.");
                        Assert.AreEqual(string.Format("Start date is required for reporting period. Guid: {0}", projectTuple.Item1.FirstOrDefault().RecordGuid), ex.Errors.FirstOrDefault().Message);
                        throw ex;
                    }
                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task GrantsService_GetGrantsAsync_ReportingPeriodEndDateAsNull()
                {
                    projectTuple.Item1.FirstOrDefault().ReportingPeriods.FirstOrDefault().EndDate = null;
                    grantsRepositoryMock.Setup(g => g.GetGrantsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(projectTuple);
                    try
                    {
                        await grantsService.GetGrantsAsync(0, 100, "", "", false);
                    }
                    catch (IntegrationApiException ex)
                    {
                        Assert.IsTrue(ex.Errors != null && ex.Errors.Any(), "Errors are present with exception.");
                        Assert.AreEqual(string.Format("End date is required for reporting period. Guid: {0}", projectTuple.Item1.FirstOrDefault().RecordGuid), ex.Errors.FirstOrDefault().Message);
                        throw ex;
                    }
                }

                [TestMethod]
                [ExpectedException(typeof(IntegrationApiException))]
                public async Task GrantsService_GetGrantsAsync_PersonAsNull()
                {
                    personRepositoryMock.Setup(i => i.GetPersonGuidFromIdAsync(It.IsAny<string>())).ReturnsAsync(() => null);
                    try
                    {
                        await grantsService.GetGrantsAsync(0, 100, "", "", false);
                    }
                    catch (IntegrationApiException ex)
                    {
                        Assert.IsTrue(ex.Errors != null && ex.Errors.Any(), "Errors are present with exception.");
                        Assert.AreEqual(string.Format("No GUID found for person ID {0}.", projectTuple.Item1.FirstOrDefault().ProjectContactPerson.FirstOrDefault()), ex.Errors.FirstOrDefault().Message);
                        throw ex;
                    }
                }

                #endregion

                #region GETBYID

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GrantsService_GetGrantsByGuidAsync_KeyNotFoundException()
                {

                    grantsRepositoryMock.Setup(i => i.GetProjectsAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                    try
                    {
                        await grantsService.GetGrantsByGuidAsync(guid);
                    }
                    catch (KeyNotFoundException ex)
                    {
                        Assert.IsTrue(ex.Message != null, "Errors are present with exception.");
                        Assert.AreEqual(string.Format("Grants record not found for GUID {0}", projectTuple.Item1.FirstOrDefault().RecordGuid), ex.Message);
                        throw ex;
                    }
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task GrantsService_GetGrantsByGuidAsync_InvalidOperationException()
                {

                    grantsRepositoryMock.Setup(i => i.GetProjectsAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                    await grantsService.GetGrantsByGuidAsync(guid);
                }

                #endregion


            }
        }

    }
}
