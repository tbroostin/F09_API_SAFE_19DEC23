using Ellucian.Colleague.Coordination.ColleagueFinance.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Tests.UserFactories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Tests.Services
{
    [TestClass]
    public class LedgerActivityServiceTests_V10
    {
        [TestClass]
        public class LedgerActivityServiceTests_GET : GeneralLedgerCurrentUser
        {
            #region DECLARATION

            protected Domain.Entities.Role getLedgerActivities = new Domain.Entities.Role(1, "VIEW.LEDGER.ACTIVITIES");

            private Mock<ILedgerActivityRepository> ledgerActivityRepositoryMock;
            private Mock<IGeneralLedgerConfigurationRepository> generalLedgerConfigurationRepositoryMock;
            private Mock<IColleagueFinanceReferenceDataRepository> referenceDataRepositoryMock;
            private Mock<IRoleRepository> roleRepositoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IAdapterRegistry> adapterRegistryMock;
            private Mock<IConfigurationRepository> configurationRepositoryMock;

            private GeneralLedgerUserAllAccounts currentUserFactory;

            private LedgerActivityService ledgerActivityService;

            private IEnumerable<Domain.Entities.Role> roles;
            private IEnumerable<Domain.ColleagueFinance.Entities.FiscalYear> fiscalYears;
            private IEnumerable<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg> fiscalPeriods;

            private Tuple<IEnumerable<Domain.ColleagueFinance.Entities.GeneralLedgerActivity>, int> tupleResult;
            private IEnumerable<Domain.ColleagueFinance.Entities.GeneralLedgerActivity> domainLedgerActivities;
            private IEnumerable<Domain.ColleagueFinance.Entities.GlSourceCodes> glSourceCodes;
            private Dictionary<string, string> accountingStrings;
            private List<Domain.Base.Entities.Person> persons;
            private List<Domain.Base.Entities.Institution> institutions;

            private Domain.ColleagueFinance.Entities.GeneralLedgerActivity ledgerActivity;
            private Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure accountStructure;

            private string guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874";

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                ledgerActivityRepositoryMock = new Mock<ILedgerActivityRepository>();
                generalLedgerConfigurationRepositoryMock = new Mock<IGeneralLedgerConfigurationRepository>();
                referenceDataRepositoryMock = new Mock<IColleagueFinanceReferenceDataRepository>();
                roleRepositoryMock = new Mock<IRoleRepository>();
                loggerMock = new Mock<ILogger>();
                adapterRegistryMock = new Mock<IAdapterRegistry>();
                configurationRepositoryMock = new Mock<IConfigurationRepository>();
                currentUserFactory = new GeneralLedgerCurrentUser.GeneralLedgerUserAllAccounts();

                ledgerActivityService = new LedgerActivityService(ledgerActivityRepositoryMock.Object, generalLedgerConfigurationRepositoryMock.Object,
                    referenceDataRepositoryMock.Object, adapterRegistryMock.Object, currentUserFactory, roleRepositoryMock.Object, loggerMock.Object);

                InitializeTestData();

                InitializeMock();
            }

            [TestCleanup]
            public void Cleanup()
            {
                ledgerActivityRepositoryMock = null;
                generalLedgerConfigurationRepositoryMock = null;
                referenceDataRepositoryMock = null;
                roleRepositoryMock = null;
                loggerMock = null;
                adapterRegistryMock = null;
                configurationRepositoryMock = null;
                currentUserFactory = null;

                ledgerActivityService = null;
            }

            private void InitializeTestData()
            {
                ledgerActivity = new Domain.ColleagueFinance.Entities.GeneralLedgerActivity(guid, "1*2", "desc", DateTime.Today, new DateTime(2016, 8, 20), 100, 1000)
                {
                    GlaSource = "AB*2",
                    HostCountry = "USA",
                    GlaAccountId = guid
                };

                institutions = new List<Domain.Base.Entities.Institution>()
                {
                    new Domain.Base.Entities.Institution("1a59eed8-5fe7-4120-b1cf-f23266b9e874", Domain.Base.Entities.InstType.College) { }
                };

                persons = new List<Domain.Base.Entities.Person>()
                {
                    new Domain.Base.Entities.Person("1", "last") { Guid = guid, PersonCorpIndicator = "N" },
                    new Domain.Base.Entities.Person("2", "last") { Guid = guid, PersonCorpIndicator = "Y" },
                    new Domain.Base.Entities.Person("3", "last") { Guid = "1a59eed8-5fe7-4120-b1cf-f23266b9e874", PersonCorpIndicator = "Y" }
                };

                accountStructure = new Domain.ColleagueFinance.Entities.GeneralLedgerAccountStructure()
                {
                    glDelimiter = "-",
                };
                accountStructure.SetMajorComponentStartPositions(new List<string>() { "1" });
                
                accountingStrings = new Dictionary<string, string>() { { "1", "2" }, { "2", "3" }, { "3", "4" } };

                glSourceCodes = new List<Domain.ColleagueFinance.Entities.GlSourceCodes>()
                {
                    new Domain.ColleagueFinance.Entities.GlSourceCodes(guid, "1", "desc", "sp3") { },
                    new Domain.ColleagueFinance.Entities.GlSourceCodes(guid, "YE", "desc", "sp3") { },
                    new Domain.ColleagueFinance.Entities.GlSourceCodes(guid, "AA", "desc", "sp3") { },
                    new Domain.ColleagueFinance.Entities.GlSourceCodes(guid, "AB", "desc", "sp3") { }
                };

                domainLedgerActivities = new List<Domain.ColleagueFinance.Entities.GeneralLedgerActivity>()
                {
                    new Domain.ColleagueFinance.Entities.GeneralLedgerActivity("1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1*2", "desc", DateTime.Today, new DateTime(2016, 8, 20), 100, null)
                    {
                        GlaSource = "YE*1",
                        HostCountry = "USA",
                        GlaAccountId = guid,
                        ProjectId = "1",
                        ProjectRefNo = "Ref Id",
                        ProjectGuid = "5a59eed8-5fe7-4120-b1cf-f23266b9e876"
                    },
                    new Domain.ColleagueFinance.Entities.GeneralLedgerActivity("1a59eed8-5fe7-4120-b1cf-f23266b9e875", "2*3", "desc", DateTime.Today, new DateTime(2014, 9, 20), null, 1000)
                    {
                        GlaSource = "AB*2",
                        //GlaSourceCode = "1",
                        HostCountry = "CAN",
                        GlaAccountId = guid,
                        GlaCorpFlag = "Y"
                    },
                    new Domain.ColleagueFinance.Entities.GeneralLedgerActivity("1a59eed8-5fe7-4120-b1cf-f23266b9e876", "3", "desc", DateTime.Today, new DateTime(2015, 9, 20), 100, 1000)
                    {
                        GlaSource = "AA*3",
                        HostCountry = "UK",
                        GlaAccountId = guid,
                        GlaCorpFlag = "Y",
                        GlaInstFlag = "Y"
                    }
                };

                tupleResult = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.GeneralLedgerActivity>, int>(domainLedgerActivities, 3);

                roles = new List<Domain.Entities.Role>()
                {
                    new Domain.Entities.Role(1, "VIEW.LEDGER.ACTIVITIES") { }
                };

                roles.FirstOrDefault().AddPermission(new Domain.Entities.Permission("VIEW.LEDGER.ACTIVITIES") { });

                fiscalYears = new List<Domain.ColleagueFinance.Entities.FiscalYear>()
                {
                    new Domain.ColleagueFinance.Entities.FiscalYear(guid, "2015") { CurrentFiscalYear = 2015, FiscalStartMonth = 7 },
                    new Domain.ColleagueFinance.Entities.FiscalYear(guid, "2017") { CurrentFiscalYear = 2017, FiscalStartMonth = 7 }
                };

                fiscalPeriods = new List<Domain.ColleagueFinance.Entities.FiscalPeriodsIntg>()
                {
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg(guid, "1") { },
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg("1a59eed8-5fe7-4120-b1cf-f23266b9e876", "2") { FiscalYear = 2016, Year = 2016, Month = 8 },
                    new Domain.ColleagueFinance.Entities.FiscalPeriodsIntg("1a59eed8-5fe7-4120-b1cf-f23266b9e877", "3") { FiscalYear = 2015, Year = 2015, Month = 9 }
                };
            }

            private void InitializeMock(bool bypassCache = false)
            {
                getLedgerActivities.AddPermission(new Domain.Entities.Permission(ColleagueFinancePermissionCodes.ViewLedgerActivities));
                roleRepositoryMock.Setup(rpm => rpm.Roles).Returns(new List<Domain.Entities.Role>() { getLedgerActivities });

                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(roles);

                referenceDataRepositoryMock.Setup(r => r.GetFiscalYearsAsync(bypassCache)).ReturnsAsync(fiscalYears);
                referenceDataRepositoryMock.Setup(r => r.GetFiscalPeriodsIntgAsync(bypassCache)).ReturnsAsync(fiscalPeriods);
                referenceDataRepositoryMock.Setup(r => r.GetGlSourceCodesValcodeAsync(bypassCache)).ReturnsAsync(glSourceCodes);

                ledgerActivityRepositoryMock.Setup(l => l.GetUnidataFormattedDate(It.IsAny<string>())).ReturnsAsync("2017-08-27");

                ledgerActivityRepositoryMock.Setup(l => l.GetGlaFyrByIdAsync(It.IsAny<string>())).ReturnsAsync(ledgerActivity);

                ledgerActivityRepositoryMock.Setup(l => l.GetGlaFyrAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<string>())).ReturnsAsync(tupleResult);
                generalLedgerConfigurationRepositoryMock.Setup(g => g.GetAccountStructureAsync()).ReturnsAsync(accountStructure);
            }

            #endregion

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync_PermissionsException()
            {
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
                await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "", "", "");
            }

            [TestMethod]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync_Sending_Empty_Filters()
            {
                var result = await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "", "", "");
                Assert.AreEqual(result.Item2, 3);
            }

            [TestMethod]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync_Invalid_FiscalYear_Filter()
            {
                var result =  await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "1a59eed8-5fe7-4120-b1cf-f23266b9e875", "", "", "");
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync_Invalid_FiscalPeriod_Filter()
            {
                var result = await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "1a59eed8-5fe7-4120-b1cf-f23266b9e875", "", "");
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task GetLedgerActivitiesAsync_FiscalYear_NotMatching_With_FiscalPeriod_FiscalYear()
            {
                var result = await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "1a59eed8-5fe7-4120-b1cf-f23266b9e874", "1a59eed8-5fe7-4120-b1cf-f23266b9e876", "", "");
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]          
            public async Task GetLedgerActivitiesAsync_FiscalYear_NotFound_For_TransactionDate_Filter()
            {
               var result =  await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "", "", "2017-08-27");
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]          
            public async Task GetLedgerActivitiesAsync_FiscalPeriod_NotMatching_With_TransactionDate_Filter_Month()
            {
               var result =  await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "1a59eed8-5fe7-4120-b1cf-f23266b9e877", "", "2017-08-27");
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync_Invalid_TransactionDate_Filter()
            {
                var result = await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "1a59eed8-5fe7-4120-b1cf-f23266b9e877", "", "2017-15-27");
                Assert.AreEqual(result.Item2, 0);
            }


            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync_FiscalYears_With_Empty_FiscalStartMonth()
            {
                fiscalYears = new List<Domain.ColleagueFinance.Entities.FiscalYear>() { new Domain.ColleagueFinance.Entities.FiscalYear(guid, "2015") { } };

                InitializeMock(true);

                await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "", "", "2017-08-27", true);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync_FiscalYears_With_Empty_FiscalYear()
            {
                fiscalYears = new List<Domain.ColleagueFinance.Entities.FiscalYear>() { new Domain.ColleagueFinance.Entities.FiscalYear(guid, "2015") { FiscalStartMonth = 8 } };

                InitializeMock(true);

                await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "", "", "2015-08-27", true);
            }


            [TestMethod]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync_Empty_Resultset()
            {
                tupleResult = new Tuple<IEnumerable<Domain.ColleagueFinance.Entities.GeneralLedgerActivity>, int>(new List<Domain.ColleagueFinance.Entities.GeneralLedgerActivity>() { }, 0);

                ledgerActivityRepositoryMock.Setup(l => l.GetGlaFyrAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>())).ReturnsAsync(tupleResult);

                var result = await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "", "", "2014-09-27");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetLedgerActivitiesAsync_EntityToDto_Invalid_LedgerCategoryEnum()
            {
                domainLedgerActivities.FirstOrDefault().GlaSource = "AB*5";
                await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "", "", "2016-08-27");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetLedgerActivitiesAsync_EntityToDto_Invalid_DocumentType()
            {
                domainLedgerActivities.FirstOrDefault().GlaSource = "BB*2";
                await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "1a59eed8-5fe7-4120-b1cf-f23266b9e877", "", "");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetLedgerActivitiesAsync_EntityToDto_TransactionDate_Null()
            {
                domainLedgerActivities.FirstOrDefault().TransactionDate = null;
                await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "1a59eed8-5fe7-4120-b1cf-f23266b9e877", "", "");
            }

            [TestMethod]        
            public async Task GetLedgerActivitiesAsync_EntityToDto_FiscalPeriod_NotFound_For_TransactionDate()
            {
                domainLedgerActivities.FirstOrDefault().TransactionDate = DateTime.Today;
                var result =  await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "1a59eed8-5fe7-4120-b1cf-f23266b9e878", "", "");
                Assert.AreEqual(result.Item2, 0);
            }

            [TestMethod]
            public async Task LedgerActivitiesService_GetLedgerActivitiesAsync()
            {

                var result = await ledgerActivityService.GetLedgerActivitiesAsync(0, 2, "", "1a59eed8-5fe7-4120-b1cf-f23266b9e877", "", "");

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Item2, 3);
            }

            [TestMethod]
            [ExpectedException(typeof(PermissionsException))]
            public async Task LedgerActivitiesService_GetLedgerActivityByGuidAsync_PermissionException()
            {
                roleRepositoryMock.Setup(r => r.GetRolesAsync()).ReturnsAsync(new List<Domain.Entities.Role>() { });
                await ledgerActivityService.GetLedgerActivityByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task GetLedgerActivityByGuidAsync_PermissionException_From_Repository()
            {
                ledgerActivityRepositoryMock.Setup(l => l.GetGlaFyrByIdAsync(It.IsAny<string>())).ThrowsAsync(new KeyNotFoundException());
                await ledgerActivityService.GetLedgerActivityByGuidAsync(guid);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetLedgerActivityByGuidAsync_InvalidOperationException_From_Repository()
            {
                ledgerActivityRepositoryMock.Setup(l => l.GetGlaFyrByIdAsync(It.IsAny<string>())).ThrowsAsync(new InvalidOperationException());
                await ledgerActivityService.GetLedgerActivityByGuidAsync(guid);
            }

            [TestMethod]
            public async Task LedgerActivitiesService_GetLedgerActivityByGuidAsync()
            {
                var result = await ledgerActivityService.GetLedgerActivityByGuidAsync(guid);

                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, guid);
            }
        }
    }
}
