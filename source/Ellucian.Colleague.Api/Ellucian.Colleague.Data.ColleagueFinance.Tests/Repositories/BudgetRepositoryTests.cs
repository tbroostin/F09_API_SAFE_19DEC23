﻿// Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.ColleagueFinance.Tests.Repositories
{
    [TestClass]
    public class BudgetRepositoryTests
    {
        [TestClass]
        public class BudgetRepositoryTests_BudgetPhases
        {
            #region DECLARATIONS

            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueTransactionFactory> transactionFactoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IColleagueDataReader> dataReaderMock;
            private BudgetRepository budgetRepository;

            private Collection<DataContracts.Budget> _budgetCollection = null;

            private string[] budgetIds;
            private string[] budgetGuids;
               
            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                cacheProviderMock = new Mock<ICacheProvider>();
                transactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                dataReaderMock = new Mock<IColleagueDataReader>();

                transactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                InitializeTestData();

                budgetRepository = new BudgetRepository(cacheProviderMock.Object, transactionFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                transactionFactoryMock = null;
                loggerMock = null;
                dataReaderMock = null;

                budgetRepository = null;
            }

            private void InitializeTestData()
            {
                _budgetCollection = new Collection<DataContracts.Budget>
           {
                    new DataContracts.Budget()
                    {  Recordkey = "1",
                        RecordGuid =  "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                     BuTitle = "2015 Operating Budget", BuStatus = "O" },
                    new DataContracts.Budget() {
                        Recordkey = "2", RecordGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                     BuTitle = "2016 Operating Budget", BuStatus = "O" },
                    new DataContracts.Budget()
                    {  Recordkey = "3", RecordGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                     BuTitle = "2017 Operating Budget", BuStatus = "O" }
                };

                budgetIds = new string[3] { "1", "2", "3" };
                budgetGuids = new string[3] { "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e" };
                
            }

            #endregion

            [TestMethod]
            public async Task BudgetRepositoryTests_GetBudgetsAsync()
            {
                dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ReturnsAsync(budgetIds);
                dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(budgetGuids);


                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Budget>(It.IsAny<string[]>(), It.IsAny<bool>()))
                              .ReturnsAsync(_budgetCollection);

                var result = await budgetRepository.GetBudgetPhasesAsync(It.IsAny<string>(), It.IsAny<bool>());

                Assert.AreEqual(_budgetCollection.Count, result.Item2);
                Assert.AreEqual(_budgetCollection.FirstOrDefault().RecordGuid, result.Item1.FirstOrDefault().BudgetPhaseGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(System.ArgumentNullException))]
            public async Task BudgetRepositoryTests_GetBudgetByGuidAsync_ArgumentNullException()
            {
                await budgetRepository.GetBudgetPhasesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BudgetRepositoryTests_GetBudgetByGuidAsync_KeyNotFoundException()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", null } }));

                await budgetRepository.GetBudgetPhasesAsync(_budgetCollection.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BudgetRepositoryTests_GetBudgetByGuidAsync_KeyNotFoundException_When_Budget_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "KEY" } } }));

                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(null);

                await budgetRepository.GetBudgetPhasesAsync(_budgetCollection.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BudgetRepositoryTests_GetBudgetByGuidAsync_KeyNotFoundException_When_RmasRoom_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "KEY" } } }));

                var record = _budgetCollection.FirstOrDefault();
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(record);

                await budgetRepository.GetBudgetPhasesAsync(_budgetCollection.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            public async Task BudgetRepositoryTests_GetBudgetByGuidAsync()
            {
                var expected =  _budgetCollection.FirstOrDefault();
                expected.BuLocation = "O";
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(expected);
                dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(new LdmGuid() { LdmGuidEntity = "BUDGET", LdmGuidPrimaryKey = expected.Recordkey });
                dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ReturnsAsync(budgetIds);
                dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(budgetGuids);
                var result = await budgetRepository.GetBudgetPhasesAsync(_budgetCollection.FirstOrDefault().RecordGuid);

                Assert.IsNotNull(result);

                Assert.AreEqual(result.BudgetPhaseGuid, _budgetCollection.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task BudgetRepositoryTests_GetBudgetByGuidAsync_NotOnline()
            {
                var expected = _budgetCollection.FirstOrDefault();
                expected.BuLocation = "X";
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(expected);
                dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(new LdmGuid() { LdmGuidEntity = "BUDGET", LdmGuidPrimaryKey = expected.Recordkey });
                dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ReturnsAsync(budgetIds);
                dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(budgetGuids);
                await budgetRepository.GetBudgetPhasesAsync(_budgetCollection.FirstOrDefault().RecordGuid);
            }
        }

        [TestClass]
        public class BudgetRepositoryTests_BudgetCodes
        {
            #region DECLARATIONS

            private Mock<ICacheProvider> cacheProviderMock;
            private Mock<IColleagueTransactionFactory> transactionFactoryMock;
            private Mock<ILogger> loggerMock;
            private Mock<IColleagueDataReader> dataReaderMock;
            private BudgetRepository budgetRepository;

            private Collection<DataContracts.Budget> _budgetCollection = null;

            private string[] budgetIds;
            private string[] budgetGuids;

            #endregion

            #region TEST SETUP

            [TestInitialize]
            public void Initialize()
            {
                cacheProviderMock = new Mock<ICacheProvider>();
                transactionFactoryMock = new Mock<IColleagueTransactionFactory>();
                loggerMock = new Mock<ILogger>();
                dataReaderMock = new Mock<IColleagueDataReader>();

                transactionFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                InitializeTestData();

                budgetRepository = new BudgetRepository(cacheProviderMock.Object, transactionFactoryMock.Object, loggerMock.Object);
            }

            [TestCleanup]
            public void Cleanup()
            {
                cacheProviderMock = null;
                transactionFactoryMock = null;
                loggerMock = null;
                dataReaderMock = null;

                budgetRepository = null;
            }

            private void InitializeTestData()
            {
                _budgetCollection = new Collection<DataContracts.Budget>
           {
                    new DataContracts.Budget()
                    {  Recordkey = "1",
                        RecordGuid =  "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                     BuTitle = "2015 Operating Budget", BuStatus = "O",
                    BuBudgetCodesIntgIdx = "1"},
                    new DataContracts.Budget() {
                        Recordkey = "2", RecordGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                     BuTitle = "2016 Operating Budget", BuStatus = "O",
                    BuBudgetCodesIntgIdx = "2"},
                    new DataContracts.Budget()
                    {  Recordkey = "3", RecordGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                     BuTitle = "2017 Operating Budget", BuStatus = "O",
                    BuBudgetCodesIntgIdx = "3"}
                };

                budgetIds = new string[3] { "1", "2", "3" };
                budgetGuids = new string[3] { "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e" };

            }

            #endregion

            [TestMethod]
            public async Task BudgetRepositoryTests_GetBudgetCodesAsync()
            {
                dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ReturnsAsync(budgetIds);
                dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(budgetGuids);


                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Budget>(It.IsAny<string[]>(), It.IsAny<bool>()))
                              .ReturnsAsync(_budgetCollection);

                var result = await budgetRepository.GetBudgetCodesAsync(It.IsAny<bool>());

                Assert.AreEqual(_budgetCollection.Count, result.Item2);
                Assert.AreEqual(_budgetCollection.FirstOrDefault().RecordGuid, result.Item1.FirstOrDefault().BudgetCodeGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(System.ArgumentNullException))]
            public async Task BudgetRepositoryTests_GetBudgetCodeByGuidAsync_ArgumentNullException()
            {
                await budgetRepository.GetBudgetCodesAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BudgetRepositoryTests_GetBudgetCodeByGuidAsync_KeyNotFoundException()
            {
                dataReaderMock.Setup(acc => acc.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", null } }));

                await budgetRepository.GetBudgetCodesAsync(_budgetCollection.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BudgetRepositoryTests_GetBudgetCodeByGuidAsync_KeyNotFoundException_When_Budget_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "KEY" } } }));

                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(null);

                await budgetRepository.GetBudgetCodesAsync(_budgetCollection.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task BudgetRepositoryTests_GetBudgetCodeByGuidAsync_KeyNotFoundException_When_RmasRoom_Null()
            {
                dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<GuidLookup[]>()))
                  .Returns(Task.FromResult(new Dictionary<string, GuidLookupResult>() { { "KEY", new GuidLookupResult() { Entity = "ROOM.ASSIGNMENT", PrimaryKey = "KEY" } } }));

                var record = _budgetCollection.FirstOrDefault();
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(record);

                await budgetRepository.GetBudgetCodesAsync(_budgetCollection.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            public async Task BudgetRepositoryTests_GetBudgetCodeByGuidAsync()
            {
                var expected = _budgetCollection.FirstOrDefault();
                expected.BuLocation = "O";
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(expected);
                dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(new LdmGuid() {
                                  LdmGuidEntity = "BUDGET",
                                  LdmGuidPrimaryKey = expected.Recordkey,
                                  LdmGuidSecondaryFld = "BU.BUDGET.CODES.INTG.IDX"
                              });
                dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ReturnsAsync(budgetIds);
                dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(budgetGuids);
                var result = await budgetRepository.GetBudgetCodesAsync(_budgetCollection.FirstOrDefault().RecordGuid);

                Assert.IsNotNull(result);

                Assert.AreEqual(result.BudgetCodeGuid, _budgetCollection.FirstOrDefault().RecordGuid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task BudgetRepositoryTests_GetBudgetCodeByGuidAsync_NotOnline()
            {
                var expected = _budgetCollection.FirstOrDefault();
                expected.BuLocation = "X";
                dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.Budget>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(expected);
                dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>("LDM.GUID", It.IsAny<string>(), It.IsAny<bool>()))
                              .ReturnsAsync(new LdmGuid() {
                                  LdmGuidEntity = "BUDGET",
                                  LdmGuidPrimaryKey = expected.Recordkey,
                                  LdmGuidSecondaryFld = "BU.BUDGET.CODES.INTG.IDX" });
                dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ReturnsAsync(budgetIds);
                dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(budgetGuids);
                await budgetRepository.GetBudgetCodesAsync(_budgetCollection.FirstOrDefault().RecordGuid);
            }
        }

        [TestClass]
        public class BudgetRepositoryTests_BudgetPhaseLineItems_V12
        {
            [TestClass]
            public class BudgetRepositoryTests_BudgetPhaseLineItems_GET_GETALL_V12 : BaseRepositorySetup
            {
                #region DECLARATIONS

                private BudgetRepository budgetRepository;

                private Collection<DataContracts.Budget> budgetCollection = null;
                private Collection<DataContracts.BudWork> budworkCollection = null;
                private LdmGuid ldmGuid;
                private Base.DataContracts.IntlParams intlParams;
                private Dictionary<string, GuidLookupResult> dicResult;

                private string guid = "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc";

                private string[] budgetIds;
                private string[] budgetFieldIds;
                private string[] budgetGuids;

                #endregion

                #region TEST SETUP

                [TestInitialize]
                public void Initialize()
                {
                    MockInitialize();

                    InitializeTestData();

                    InitializeTestMock();

                    budgetRepository = new BudgetRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
                }

                [TestCleanup]
                public void Cleanup()
                {
                    MockCleanup();

                    budgetRepository = null;
                }

                private void InitializeTestMock()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(ldmGuid);
                    dataReaderMock.Setup(d => d.SelectAsync("BUDGET", "WITH BU.LOCATION EQ 'O'")).ReturnsAsync(budgetIds);
                    dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(budgetFieldIds);
                    dataReaderMock.Setup(d => d.ReadRecordAsync<Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(intlParams);
                    dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.BudWork>(It.IsAny<string>(), It.IsAny<string[]>(), true)).ReturnsAsync(budworkCollection);
                    dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                }

                private void InitializeTestData()
                {
                    dicResult = new Dictionary<string, GuidLookupResult>()
                    {
                        { guid, new GuidLookupResult() { Entity = "BUD.WORK", PrimaryKey = "1" } }
                    };

                    intlParams = new Base.DataContracts.IntlParams()
                    {
                        HostCountry = "USA",
                        HostDateDelimiter = "/",
                        HostShortDateFormat = "MDY"
                    };

                    budworkCollection = new Collection<DataContracts.BudWork>()
                    {
                        new DataContracts.BudWork()
                        {
                            RecordGuid = guid,
                            Recordkey = "1"
                        }
                    };

                    budgetCollection = new Collection<DataContracts.Budget>
                    {
                        new DataContracts.Budget()
                        {
                            Recordkey = "1",
                            RecordGuid =  "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc",
                            BuTitle = "2015 Operating Budget",
                            BuStatus = "O"
                        },
                        new DataContracts.Budget()
                        {
                            Recordkey = "2",
                            RecordGuid = "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d",
                            BuTitle = "2016 Operating Budget",
                            BuStatus = "O"
                        },
                        new DataContracts.Budget()
                        {
                            Recordkey = "3",
                            RecordGuid = "d2253ac7-9931-4560-b42f-1fccd43c952e",
                            BuTitle = "2017 Operating Budget",
                            BuStatus = "O"
                        }
                    };

                    ldmGuid = new LdmGuid() { LdmGuidEntity = "BUDGET", LdmGuidPrimaryKey = "1" };

                    budgetIds = new string[3] { "1", "2", "3" };
                    budgetFieldIds = new string[1] { "1"};
                    budgetGuids = new string[3] { "7a2bf6b5-cdcd-4c8f-b5d8-3053bf5b3fbc", "849e6a7c-6cd4-4f98-8a73-ab0aa3627f0d", "d2253ac7-9931-4560-b42f-1fccd43c952e" };

                }

                #endregion

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task BudgetPhases_GetBudgetPhasesIdFromGuidAsync_Guid_Null()
                {
                    await budgetRepository.GetBudgetPhasesIdFromGuidAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task BudgetPhases_GetBudgetPhasesIdFromGuidAsync_Guid_NotFound()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(null);
                    await budgetRepository.GetBudgetPhasesIdFromGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task BudgetPhases_GetBudgetPhasesIdFromGuidAsync_Invalid_Entity_Name()
                {
                    ldmGuid = new LdmGuid()
                    {
                        LdmGuidEntity = "BUDGET1",
                        LdmGuidSecondaryFld = "1"
                    };
                    dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(ldmGuid);
                    await budgetRepository.GetBudgetPhasesIdFromGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task BudgetPhases_GetBudgetPhasesIdFromGuidAsync_Invalid_SecondaryFld()
                {
                    ldmGuid = new LdmGuid()
                    {
                        LdmGuidEntity = "BUDGET",
                        LdmGuidSecondaryFld = "1"
                    };
                    dataReaderMock.Setup(d => d.ReadRecordAsync<LdmGuid>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(ldmGuid);
                    await budgetRepository.GetBudgetPhasesIdFromGuidAsync(guid);
                }

                [TestMethod]
                public async Task BudgetPhases_GetBudgetPhasesIdFromGuidAsync()
                {
                    var result = await budgetRepository.GetBudgetPhasesIdFromGuidAsync(guid);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result, ldmGuid.LdmGuidPrimaryKey);
                }

                [TestMethod]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsAsync_Empty_BudgetIds()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ReturnsAsync((new List<string>() { }).ToArray());

                    var result = await budgetRepository.GetBudgetPhaseLineItemsAsync(0, 2, null, null);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsAsync_Returns_Null_BudgetIds()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ReturnsAsync(null);

                    var result = await budgetRepository.GetBudgetPhaseLineItemsAsync(0, 2, null, null);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 0);
                }

                [TestMethod]
                [ExpectedException(typeof(Exception))]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsAsync_Exception()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("BUDGET", It.IsAny<string>())).ThrowsAsync(new Exception());
                    await budgetRepository.GetBudgetPhaseLineItemsAsync(0, 2, null, null);
                }

                [TestMethod]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsAsync()
                {
                    var result = await budgetRepository.GetBudgetPhaseLineItemsAsync(0, 2, null, null);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 1);
                }

                [TestMethod]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsAsync_With_BudgetPhase_Filter()
                {
                    var result = await budgetRepository.GetBudgetPhaseLineItemsAsync(0, 2, "1", null);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 1);
                }

                [TestMethod]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsAsync_With_Accouting_Filter()
                {
                    var result = await budgetRepository.GetBudgetPhaseLineItemsAsync(0, 2, null, "1");

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.Item2, 1);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task BudgetPhases_GetBudgetPhasesGuidFromIdAsync_Guid_NotFound()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(null);
                    await budgetRepository.GetBudgetPhasesGuidFromIdAsync("1");
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task BudgetPhases_GetBudgetPhasesGuidFromIdAsync_Guid_MoreThanOne()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ReturnsAsync(new List<string>() { guid, guid }.ToArray());
                    await budgetRepository.GetBudgetPhasesGuidFromIdAsync("1");
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task BudgetPhases_GetBudgetPhasesGuidFromIdAsync_ArgumentNullException()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ThrowsAsync(new ArgumentNullException());
                    await budgetRepository.GetBudgetPhasesGuidFromIdAsync("1");
                }

                [TestMethod]
                [ExpectedException(typeof(RepositoryException))]
                public async Task BudgetPhases_GetBudgetPhasesGuidFromIdAsync_RepositoryException()
                {
                    dataReaderMock.Setup(d => d.SelectAsync("LDM.GUID", It.IsAny<string>())).ThrowsAsync(new RepositoryException());
                    await budgetRepository.GetBudgetPhasesGuidFromIdAsync("1");
                }

                [TestMethod]
                [ExpectedException(typeof(ArgumentNullException))]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsByGuidAsync_Null_Guid()
                {
                    await budgetRepository.GetBudgetPhaseLineItemsByGuidAsync(null);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsByGuid_Record_NotFound_For_Guid()
                {
                    dicResult = new Dictionary<string, GuidLookupResult>()
                    {
                        { guid, null }
                    };
                    dataReaderMock.Setup(repo => repo.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(dicResult);
                    await budgetRepository.GetBudgetPhaseLineItemsByGuidAsync(guid);
                }

                [TestMethod]
                [ExpectedException(typeof(KeyNotFoundException))]
                public async Task BudgetPhases_GetByGuid_Record_NotFound_For_PrimaryKey()
                {
                    dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.BudWork>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(null);
                    await budgetRepository.GetBudgetPhaseLineItemsByGuidAsync(guid);
                }

                [TestMethod]
                public async Task BudgetPhases_GetBudgetPhaseLineItemsByGuidAsync()
                {
                    var budWork = new DataContracts.BudWork()
                    {
                        RecordGuid = guid,
                        Recordkey = "1"
                    };
                    dataReaderMock.Setup(d => d.ReadRecordAsync<DataContracts.BudWork>(It.IsAny<string>(), It.IsAny<string>(), true)).ReturnsAsync(budWork);

                    var result = await budgetRepository.GetBudgetPhaseLineItemsByGuidAsync(guid);

                    Assert.IsNotNull(result);
                    Assert.AreEqual(result.RecordGuid, guid);
                }
            }
        }
    }
}