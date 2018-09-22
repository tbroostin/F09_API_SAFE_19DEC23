using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Transactions;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Tests;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class AccountFundsAvailableRepositoryTests : BaseRepositorySetup
    {
        AccountFundsAvailableRepository accountFundsAvailableRepository;
        TestGeneralLedgerConfigurationRepository testGeneralLedgerConfigurationRepository;
        DataContracts.GlAccts glAcctsDataContract;

        [TestInitialize]
        public void Initialize()
        {
            testGeneralLedgerConfigurationRepository = new TestGeneralLedgerConfigurationRepository();
            MockInitialize();
            BuldData();
            accountFundsAvailableRepository = new AccountFundsAvailableRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestCleanup]
        public void Cleanup() 
        {
            cacheProviderMock = null;
            transFactoryMock = null;
            transManagerMock = null;
            loggerMock = null;
            dataReaderMock = null;
            testGeneralLedgerConfigurationRepository = null;
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_CheckAvailableFundsAsync_Available()
        {
            DataContracts.Glstruct glStruct = testGeneralLedgerConfigurationRepository.GlStructDataContract;
            DataContracts.Glclsdef glClassDef = testGeneralLedgerConfigurationRepository.GlClassDefDataContract;

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE", It.IsAny<bool>()))
                .ReturnsAsync(glStruct);

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", It.IsAny<bool>()))
                .ReturnsAsync(glClassDef);

            var accountingString = "11-01-01-00-10501-53011";

            var fundsAvailable = new Domain.ColleagueFinance.Entities.FundsAvailable(accountingString)
            { TransactionDate = DateTime.Now, Amount = 150 };

            var checkAvailableFundsResponse = new CheckAvailableFundsResponse()
            {
                GlAvailableList = new List<GlAvailableList>()
                    { new GlAvailableList () {
                        AccountingStrings = accountingString,
                        AvailableStatus = "AVAILABLE",
                        Currency = "USD",
                        GlAmts = 150,
                        ItemsId = "100",
                        RecordKey = "10",
                        SubmittedBy = "00001",
                        TransactionDate = DateTime.Now
                    } }
            };

            transManagerMock.Setup(i => i.ExecuteAsync<CheckAvailableFundsRequest, CheckAvailableFundsResponse>(It.IsAny<CheckAvailableFundsRequest>()))
                .ReturnsAsync(checkAvailableFundsResponse);

            var result = await accountFundsAvailableRepository.CheckAvailableFundsAsync
                (new List<Domain.ColleagueFinance.Entities.FundsAvailable>() { fundsAvailable });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(fundsAvailable.Amount, result[0].Amount);
            Assert.AreEqual(fundsAvailable.AvailableStatus, result[0].AvailableStatus);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_CheckAvailableFundsAsync_NotAvailable()
        {
            DataContracts.Glstruct glStruct = testGeneralLedgerConfigurationRepository.GlStructDataContract;
            DataContracts.Glclsdef glClassDef = testGeneralLedgerConfigurationRepository.GlClassDefDataContract;

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE", It.IsAny<bool>()))
                .ReturnsAsync(glStruct);

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", It.IsAny<bool>()))
                .ReturnsAsync(glClassDef);

            var accountingString = "11-01-01-00-10501-53011";

            var fundsAvailable = new Domain.ColleagueFinance.Entities.FundsAvailable(accountingString)
            { TransactionDate = DateTime.Now, Amount = 150 };

            var checkAvailableFundsResponse = new CheckAvailableFundsResponse()
            {
                GlAvailableList = new List<GlAvailableList>()
                    { new GlAvailableList () {
                        AccountingStrings = accountingString,
                        AvailableStatus = "NOT.AVAILABLE",
                        Currency = "USD",
                        GlAmts = 150,
                        ItemsId = "100",
                        RecordKey = "10",
                        SubmittedBy = "00001",
                        TransactionDate = DateTime.Now
                    } }
            };

            transManagerMock.Setup(i => i.ExecuteAsync<CheckAvailableFundsRequest, CheckAvailableFundsResponse>(It.IsAny<CheckAvailableFundsRequest>()))
                .ReturnsAsync(checkAvailableFundsResponse);

            var result = await accountFundsAvailableRepository.CheckAvailableFundsAsync
                (new List<Domain.ColleagueFinance.Entities.FundsAvailable>() { fundsAvailable });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(fundsAvailable.Amount, result[0].Amount);
            Assert.AreEqual(FundsAvailableStatus.NotAvailable, result[0].AvailableStatus);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_CheckAvailableFundsAsync_Override()
        {
            DataContracts.Glstruct glStruct = testGeneralLedgerConfigurationRepository.GlStructDataContract;
            DataContracts.Glclsdef glClassDef = testGeneralLedgerConfigurationRepository.GlClassDefDataContract;

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE", It.IsAny<bool>()))
                .ReturnsAsync(glStruct);

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", It.IsAny<bool>()))
                .ReturnsAsync(glClassDef);

            var accountingString = "11-01-01-00-10501-53011";

            var fundsAvailable = new Domain.ColleagueFinance.Entities.FundsAvailable(accountingString)
            { TransactionDate = DateTime.Now, Amount = 150 };

            var checkAvailableFundsResponse = new CheckAvailableFundsResponse()
            {
                GlAvailableList = new List<GlAvailableList>()
                    { new GlAvailableList () {
                        AccountingStrings = accountingString,
                        AvailableStatus = "OVERRIDE",
                        Currency = "USD",
                        GlAmts = 150,
                        ItemsId = "100",
                        RecordKey = "10",
                        SubmittedBy = "00001",
                        TransactionDate = DateTime.Now
                    } }
            };

            transManagerMock.Setup(i => i.ExecuteAsync<CheckAvailableFundsRequest, CheckAvailableFundsResponse>(It.IsAny<CheckAvailableFundsRequest>()))
                .ReturnsAsync(checkAvailableFundsResponse);

            var result = await accountFundsAvailableRepository.CheckAvailableFundsAsync
                (new List<Domain.ColleagueFinance.Entities.FundsAvailable>() { fundsAvailable });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(fundsAvailable.Amount, result[0].Amount);
            Assert.AreEqual(FundsAvailableStatus.Override, result[0].AvailableStatus);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_CheckAvailableFundsAsync_NotApplicable()
        {
            DataContracts.Glstruct glStruct = testGeneralLedgerConfigurationRepository.GlStructDataContract;
           
            DataContracts.Glclsdef glClassDef = testGeneralLedgerConfigurationRepository.GlClassDefDataContract;
            glClassDef.GlClassExpenseValues = new List<string> { "9" };
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE", It.IsAny<bool>()))
                .ReturnsAsync(glStruct);

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", It.IsAny<bool>()))
                .ReturnsAsync(glClassDef);

            var accountingString = "11-01-01-00-10501-53011";

            var fundsAvailable = new Domain.ColleagueFinance.Entities.FundsAvailable(accountingString)
            { TransactionDate = DateTime.Now, Amount = 150 };

            var checkAvailableFundsResponse = new CheckAvailableFundsResponse()
            {
                GlAvailableList = new List<GlAvailableList>()
                    { new GlAvailableList () {
                        AccountingStrings = accountingString,
                        AvailableStatus = "OVERRIDE",
                        Currency = "USD",
                        GlAmts = 150,
                        ItemsId = "100",
                        RecordKey = "10",
                        SubmittedBy = "00001",
                        TransactionDate = DateTime.Now
                    } }
            };

            transManagerMock.Setup(i => i.ExecuteAsync<CheckAvailableFundsRequest, CheckAvailableFundsResponse>(It.IsAny<CheckAvailableFundsRequest>()))
                .ReturnsAsync(checkAvailableFundsResponse);

            var result = await accountFundsAvailableRepository.CheckAvailableFundsAsync
                (new List<Domain.ColleagueFinance.Entities.FundsAvailable>() { fundsAvailable });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(FundsAvailableStatus.NotApplicable, result[0].AvailableStatus);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_AcctCheckAvailFunds()
        {
            DataContracts.Glstruct glStruct = testGeneralLedgerConfigurationRepository.GlStructDataContract;
            glStruct.AcctCheckAvailFunds = "N";
            DataContracts.Glclsdef glClassDef = testGeneralLedgerConfigurationRepository.GlClassDefDataContract;

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glstruct>("ACCOUNT.PARAMETERS", "ACCT.STRUCTURE", It.IsAny<bool>()))
                .ReturnsAsync(glStruct);

            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.Glclsdef>("ACCOUNT.PARAMETERS", "GL.CLASS.DEF", It.IsAny<bool>()))
                .ReturnsAsync(glClassDef);

            var accountingString = "11-01-01-00-10501-53011";

            var fundsAvailable = new Domain.ColleagueFinance.Entities.FundsAvailable(accountingString)
            { TransactionDate = DateTime.Now, Amount = 150 };

            var checkAvailableFundsResponse = new CheckAvailableFundsResponse()
            {
                GlAvailableList = new List<GlAvailableList>()
                    { new GlAvailableList () {
                        AccountingStrings = accountingString,
                        AvailableStatus = "AVAILABLE",
                        Currency = "USD",
                        GlAmts = 150,
                        ItemsId = "100",
                        RecordKey = "10",
                        SubmittedBy = "00001",
                        TransactionDate = DateTime.Now
                    } }
            };

            transManagerMock.Setup(i => i.ExecuteAsync<CheckAvailableFundsRequest, CheckAvailableFundsResponse>(It.IsAny<CheckAvailableFundsRequest>()))
                .ReturnsAsync(checkAvailableFundsResponse);

            var result = await accountFundsAvailableRepository.CheckAvailableFundsAsync
                (new List<Domain.ColleagueFinance.Entities.FundsAvailable>() { fundsAvailable });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(FundsAvailableStatus.NotApplicable, result[0].AvailableStatus);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetAvailableFundsAsync() 
        {
            var actual = await accountFundsAvailableRepository.GetAvailableFundsAsync("11_00_01_00_20603_52010", 25, "2017");
            Assert.IsNotNull(actual);
        }
        
        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetPersonIdFromGuidAsync()
        {
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("1", new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "1" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);

            var actual = await accountFundsAvailableRepository.GetPersonIdFromGuidAsync("2882a0e4-9860-4bf9-aebc-a5a7cac02b62");
            Assert.AreEqual("1", actual);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetPersonIdFromGuidAsync_Null_Result()
        {
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("1", null);
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);

            var actual = await accountFundsAvailableRepository.GetPersonIdFromGuidAsync("2882a0e4-9860-4bf9-aebc-a5a7cac02b62");
            Assert.AreEqual(null, actual);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_With_Project_Number()
        {
            DataContracts.Projects resultProject = new DataContracts.Projects() 
            {
                Recordkey = "1",
            };
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), true)).ReturnsAsync(new Collection<DataContracts.Projects>(){ resultProject } );
            DataContracts.ProjectsLineItems resultProjectLineItem = new DataContracts.ProjectsLineItems()
            {
                PrjlnActualMemos = new List<decimal?>() { 10 },
                PrjlnActualPosted = new List<decimal?>() { 20 },
                PrjlnBudgetAmts = new List<decimal?>() { 500 },
                PrjlnEncumbranceMemos = new List<decimal?>() { 20 },
                PrjlnEncumbrancePosted = new List<decimal?>() { 10, 20},
                PrjlnRequisitionMemos = new List<decimal?>(){ 10, 50 },
                Recordkey = "1",
                PrjlnProjectsCf = "1"
            };
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(It.IsAny<string>(), true)).ReturnsAsync(new Collection<DataContracts.ProjectsLineItems>() { resultProjectLineItem });

            DataContracts.ProjectsCf resultProjectCf = new DataContracts.ProjectsCf()
            {
                Recordkey = "1",
                PrjcfPeriodStartDates = new List<DateTime?>() { DateTime.Today },
                PrjcfPeriodEndDates = new List<DateTime?>() { DateTime.Today.AddDays(10)}
            };
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.ProjectsCf>(It.IsAny<string>(), true)).ReturnsAsync(resultProjectCf);
            var actual = await accountFundsAvailableRepository.GetProjectAvailableFundsAsync("11_00_01_00_20603_52010", 25, "SALIL-TEST", It.IsAny<DateTime?>());
            Assert.IsNotNull(actual);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetBpoAsync_EmptyString()
        {
            var result = await accountFundsAvailableRepository.GetBpoAsync(string.Empty);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetBpoAsync_With_Result()
        {
            DataContracts.Items item = new DataContracts.Items() { ItmBpoId = "5" };
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(item);
            var result = await accountFundsAvailableRepository.GetBpoAsync("1");
            Assert.IsNotNull(result);
            Assert.AreEqual("5", result);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetBpoAsync_GetPOStatusByItemNumber_EmptyString()
        {
            var result = await accountFundsAvailableRepository.GetPOStatusByItemNumber("");
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetBpoAsync_GetPOStatusByItemNumber()
        {
            DataContracts.Items item = new DataContracts.Items() { ItmPoId = "5" };
            Collection<DataContracts.PurchaseOrders> pos = new Collection<DataContracts.PurchaseOrders>()
            {
                new DataContracts.PurchaseOrders()
                {
                    PoStatEntityAssociation = new List<DataContracts.PurchaseOrdersPoStat>()
                    {
                        new DataContracts.PurchaseOrdersPoStat()
                        {
                            PoStatusAssocMember = "ABC"
                        }
                    }
                }
            };
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(item);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(pos);
            var result = await accountFundsAvailableRepository.GetPOStatusByItemNumber("1");
            Assert.IsNotNull(result);
            Assert.AreEqual("ABC", result);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetBpoAsync_GetPOStatusByItemNumber_No_POID()
        {
            DataContracts.Items item = new DataContracts.Items() { ItmPoId = "" };
            Collection<DataContracts.PurchaseOrders> pos = new Collection<DataContracts.PurchaseOrders>()
            {
                new DataContracts.PurchaseOrders()
                {
                    PoStatEntityAssociation = new List<DataContracts.PurchaseOrdersPoStat>()
                    {
                        new DataContracts.PurchaseOrdersPoStat()
                        {
                            PoStatusAssocMember = "ABC"
                        }
                    }
                }
            };
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(item);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(pos);
            var result = await accountFundsAvailableRepository.GetPOStatusByItemNumber("1");
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetReqStatusByItemNumber()
        {
            DataContracts.Items item = new DataContracts.Items() { ItmReqId = "5" };
            Collection<DataContracts.Requisitions> req = new Collection<DataContracts.Requisitions>()
            {
                new DataContracts.Requisitions()
                {
                    ReqStatusesEntityAssociation = new List<DataContracts.RequisitionsReqStatuses>()
                    {
                        new DataContracts.RequisitionsReqStatuses()
                        {
                            ReqStatusAssocMember = "ABC"
                        }
                    }
                }
            };
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(item);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.Requisitions>("REQUISITIONS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(req);
            var result = await accountFundsAvailableRepository.GetReqStatusByItemNumber("1");
            Assert.IsNotNull(result);
            Assert.AreEqual("ABC", result);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetReqStatusByItemNumber_ReturnEmptyString()
        {
            DataContracts.Items item = new DataContracts.Items() { ItmReqId = "" };
            Collection<DataContracts.Requisitions> req = new Collection<DataContracts.Requisitions>()
            {
                new DataContracts.Requisitions()
                {
                    ReqStatusesEntityAssociation = new List<DataContracts.RequisitionsReqStatuses>()
                    {
                        new DataContracts.RequisitionsReqStatuses()
                        {
                            ReqStatusAssocMember = "ABC"
                        }
                    }
                }
            };
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(item);
            var result = await accountFundsAvailableRepository.GetReqStatusByItemNumber("1");
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task AccountFundsAvailableRepository_GetReqStatusByItemNumber_EmptyItemNumber()
        {
            var result = await accountFundsAvailableRepository.GetReqStatusByItemNumber("");
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_GetReqStatusByItemNumber_NullItems()
        {
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(null);
            var result = await accountFundsAvailableRepository.GetReqStatusByItemNumber("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_GetReqStatusByItemNumber_NullReq()
        {
            DataContracts.Items item = new DataContracts.Items() { ItmReqId = "5" };
            Collection<DataContracts.Requisitions> req = new Collection<DataContracts.Requisitions>()
            {
                new DataContracts.Requisitions()
                {
                    ReqStatusesEntityAssociation = new List<DataContracts.RequisitionsReqStatuses>()
                    {
                        new DataContracts.RequisitionsReqStatuses()
                        {
                            ReqStatusAssocMember = "ABC"
                        }
                    }
                }
            };
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(item);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.Requisitions>("REQUISITIONS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
            var result = await accountFundsAvailableRepository.GetReqStatusByItemNumber("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_GetBpoAsync_GetPOStatusByItemNumber_KeyNotFoundException()
        {
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(null);
            var result = await accountFundsAvailableRepository.GetPOStatusByItemNumber("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_GetBpoAsync_GetPOStatusByItemNumber_Null_PO_KeyNotFoundException()
        {
            DataContracts.Items item = new DataContracts.Items() { ItmPoId = "5" };
            Collection<DataContracts.PurchaseOrders> pos = new Collection<DataContracts.PurchaseOrders>()
            {
                new DataContracts.PurchaseOrders()
                {
                    PoStatEntityAssociation = new List<DataContracts.PurchaseOrdersPoStat>()
                    {
                        new DataContracts.PurchaseOrdersPoStat()
                        {
                            PoStatusAssocMember = "ABC"
                        }
                    }
                }
            };
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(item);
            dataReaderMock.Setup(i => i.BulkReadRecordAsync<DataContracts.PurchaseOrders>("PURCHASE.ORDERS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
            var result = await accountFundsAvailableRepository.GetPOStatusByItemNumber("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_GetBpoAsync_With_Result_KeyNotFoundException()
        {
            dataReaderMock.Setup(i => i.ReadRecordAsync<DataContracts.Items>("1", It.IsAny<bool>())).ReturnsAsync(null);
            var result = await accountFundsAvailableRepository.GetBpoAsync("1");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_With_Project_Null()
        {
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), true)).ReturnsAsync(null);

            var actual = await accountFundsAvailableRepository.GetProjectAvailableFundsAsync("11_00_01_00_20603_52010", 25, "SALIL-TEST", It.IsAny<DateTime?>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_With_ProjectLineItem_Null()
        {
            DataContracts.Projects resultProject = new DataContracts.Projects()
            {
                Recordkey = "1",
            };
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), true)).ReturnsAsync(new Collection<DataContracts.Projects>() { resultProject });
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(It.IsAny<string>(), true)).ReturnsAsync(null);

            var actual = await accountFundsAvailableRepository.GetProjectAvailableFundsAsync("11_00_01_00_20603_52010", 25, "SALIL-TEST", It.IsAny<DateTime?>());
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_With_ProjectCf_Null()
        {
            DataContracts.Projects resultProject = new DataContracts.Projects()
            {
                Recordkey = "1",
            };
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<DataContracts.Projects>(It.IsAny<string>(), true)).ReturnsAsync(new Collection<DataContracts.Projects>() { resultProject });
            DataContracts.ProjectsLineItems resultProjectLineItem = new DataContracts.ProjectsLineItems()
            {
                PrjlnActualMemos = new List<decimal?>() { 10 },
                PrjlnActualPosted = new List<decimal?>() { 20 },
                PrjlnBudgetAmts = new List<decimal?>() { 500 },
                PrjlnEncumbranceMemos = new List<decimal?>() { 20 },
                PrjlnEncumbrancePosted = new List<decimal?>() { 10, 20 },
                PrjlnRequisitionMemos = new List<decimal?>() { 10, 50 },
                Recordkey = "1",
                PrjlnProjectsCf = "1"
            };
            dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<DataContracts.ProjectsLineItems>(It.IsAny<string>(), true)).ReturnsAsync(new Collection<DataContracts.ProjectsLineItems>() { resultProjectLineItem });

            DataContracts.ProjectsCf resultProjectCf = null;
            dataReaderMock.Setup(reader => reader.ReadRecordAsync<DataContracts.ProjectsCf>(It.IsAny<string>(), true)).ReturnsAsync(resultProjectCf);

            var actual = await accountFundsAvailableRepository.GetProjectAvailableFundsAsync("11_00_01_00_20603_52010", 25, "SALIL-TEST", It.IsAny<DateTime?>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountFundsAvailableRepository_GetPersonIdFromGuidAsync_ArgumentNullException()
        {
            Dictionary<string, GuidLookupResult> guidLookupResult = new Dictionary<string, GuidLookupResult>();
            guidLookupResult.Add("1", new GuidLookupResult() { Entity = "GL.ACCTS", PrimaryKey = "1" });
            dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(guidLookupResult);

            var actual = await accountFundsAvailableRepository.GetPersonIdFromGuidAsync("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountFundsAvailableRepository_Null_AccountingString_ArgumentNullException()
        {
            var actual = await accountFundsAvailableRepository.GetAvailableFundsAsync("", 25, "2017");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountFundsAvailableRepository_Zero_Amount_ArgumentNullException()
        {
            var actual = await accountFundsAvailableRepository.GetAvailableFundsAsync("11_00_01_00_20603_52010", 0, "2017");
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public async Task AccountFundsAvailableRepository_Null_Results_KeyNotFoundException()
        {
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), true)).ReturnsAsync(null);
            var actual = await accountFundsAvailableRepository.GetAvailableFundsAsync("11_00_01_00_20603_52010", 25, "2017");
        }

        [TestMethod]
        [ExpectedException(typeof(RepositoryException))]
        public async Task AccountFundsAvailableRepository_Closed_Year_RepositoryException()
        {
            var actual = await accountFundsAvailableRepository.GetAvailableFundsAsync("11_00_01_00_20603_52010", 25, "2016");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountFundsAvailableRepository_Null_AcctString_ArgumentNullException()
        {
            var actual = await accountFundsAvailableRepository.GetProjectAvailableFundsAsync("", 25, It.IsAny<string>(), It.IsAny<DateTime?>()); ;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task AccountFundsAvailableRepository_Null_Project_Number_ArgumentNullException()
        {
            var actual = await accountFundsAvailableRepository.GetProjectAvailableFundsAsync("11_00_01_00_20603_52010", 25, It.IsAny<string>(), It.IsAny<DateTime?>()); ;
        }  

        private void BuldData()
        {
            glAcctsDataContract = new DataContracts.GlAccts()
            {
                Recordkey = "11_00_01_00_20603_52010",
                AvailFundsController = new List<string>() { "2016", "2017" },
                MemosEntityAssociation = new List<DataContracts.GlAcctsMemos>() 
                {
                    new DataContracts.GlAcctsMemos()
                    {
                        AvailFundsControllerAssocMember = "2017",
                        GlFreezeFlagsAssocMember = "O",
                        GlBudgetPostedAssocMember = 100,
                        GlBudgetMemosAssocMember = 500,
                        GlActualPostedAssocMember = 10,
                        GlActualMemosAssocMember = 50,
                        GlEncumbrancePostedAssocMember = 20,
                        GlEncumbranceMemosAssocMember = 30,
                        GlRequisitionMemosAssocMember = 100
                    },
                    new DataContracts.GlAcctsMemos()
                    {
                        AvailFundsControllerAssocMember = "2016",
                        GlFreezeFlagsAssocMember = "C",
                        GlBudgetPostedAssocMember = 100,
                        GlBudgetMemosAssocMember = 500,
                        GlActualPostedAssocMember = 10,
                        GlActualMemosAssocMember = 50,
                        GlEncumbrancePostedAssocMember = 20,
                        GlEncumbranceMemosAssocMember = 30,
                        GlRequisitionMemosAssocMember = 100
                    }
                }
            };
            dataReaderMock.Setup(repo => repo.ReadRecordAsync<DataContracts.GlAccts>(It.IsAny<string>(), true)).ReturnsAsync(glAcctsDataContract);
        }
    }
}
