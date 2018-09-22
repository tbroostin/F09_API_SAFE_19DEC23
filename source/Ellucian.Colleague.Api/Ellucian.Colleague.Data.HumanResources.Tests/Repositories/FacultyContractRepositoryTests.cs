using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
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

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class FacultyContractRepositoryTests
    {
        [TestClass]
        public class FacultyContractTests : BaseRepositorySetup
        {
            #region DECLARATIONS
            Collection<DataContracts.PacLoadPeriods> _pacLoadPeriodsCollection;
            Collection<DataContracts.PerAsgmtContract> _perAsgmtContractCollection;
            Collection<DataContracts.PacLpAsgmts> _pacLpAsgmtsCollection;
            Collection<DataContracts.PacLpPositions> _pacLpPositionsCollection;
            Collection<DataContracts.HrssDefaults> _hrssDefaultsCollection;
            DataContracts.PerAsgmtContract pac2;
            DataContracts.PacLoadPeriods pacLp;
            DataContracts.PacLoadPeriods pacLp2;
            DataContracts.HrssDefaults hrssDef2;
            FacultyContractRepository facContractRepository;

            [TestInitialize]
            public void Initialize()
            {
                MockInitialize();

                //PerAsgmtContract fields
                //Recordkey, PacDesc, PacHrpId, PacNo, PacType, PacStartDate, PacEndDate, PacAllLoadPeriods
                string id4, hrpid, desc4, type, pacNo;
                DateTime? startDate4, endDate4;
                List<string> pacAllLps;
                //PAC 1
                id4 = "000";
                desc4 = "Automatically created FPA";
                hrpid = "0014076";
                pacNo = "";
                type = "FPA";
                startDate4 = new DateTime(2016, 9, 1);
                endDate4 = new DateTime(2016, 12, 31);
                pacAllLps = new List<string>()
                {
                    "175"
                };

                //PAC 2
                pac2 = new DataContracts.PerAsgmtContract()
                {
                    Recordkey = "001",
                    PacDesc = "Automatically created FPA",
                    PacHrpId = "0014076",
                    PacNo = "",
                    PacType = "FPA",
                    PacStartDate = new DateTime(2016, 9, 1),
                    PacEndDate = new DateTime(2016, 12, 31),
                    PacAllLoadPeriods = new List<string>() { "571" }

                };


                //HrssDefaults fields
                //_AppServerVersion, Recordkey, HrssOtCalcDefinitionId, HrssHourlyRule, HrssSalaryRule, HrssDfltWorkWeekStartDy, HrssDfltWorkWeekStartTm, HrssFacContWebVisible
                int appSV;
                string hrssRK, hrssCalcID, hrssHR, hrssSalR, hrssWWStartD;
                DateTime? hrssWWStartTi;
                List<string> hrssVisible;
                //HrssDefaults
                appSV = 0;
                hrssRK = "HRSS.DEFAULTS";
                hrssCalcID = "BASIC";
                hrssHR = "";
                hrssSalR = "";
                hrssWWStartD = "S";
                hrssWWStartTi = null;
                hrssVisible = new List<string>()
                {
                    "AP"
                };

                hrssDef2 = new DataContracts.HrssDefaults()
                {
                    _AppServerVersion = 0,
                    Recordkey = "HRSS.DEFAULTS",
                    HrssOtCalcDefinitionId = "BASIC",
                    HrssHourlyRule = "",
                    HrssSalaryRule = "",
                    HrssDfltWorkWeekStartDy = "S",
                    HrssDfltWorkWeekStartTm = null,
                    HrssFacContWebVisible = new List<string>() { "AP" }
                };

                // PACLP
                string plpHrpId, plpLoadPeriod, plpPerAsgmtContractId, recordKey;
                Decimal? plpIntendedTotalLoad, plpTotalValue;
                List<string> plpPaclLpPositionIds = new List<string>() { "246" };
                List<string> plpStatuses;
                List<string> plpStatusChgopr;
                List<DateTime?> plpStatusDates;
                plpHrpId = "0014076";
                plpLoadPeriod = "16FL";
                plpPerAsgmtContractId = "000";
                recordKey = "175";
                plpIntendedTotalLoad = 60;
                plpStatuses = new List<String>() { "AP" };
                plpStatusDates = new List<DateTime?>() { null };
                plpStatusChgopr = new List<String>() { "" };
                plpTotalValue = null;

                //PACLP2
                pacLp2 = new DataContracts.PacLoadPeriods()
                {
                    PlpHrpId = "0014076",
                    PlpIntendedTotalLoad = 100,
                    PlpLoadPeriod = "11SP",
                    PlpPacLpPositionIds = new List<string>() { "222" },
                    PlpPerAsgmtContractId = "001",
                    PlpTotalValue = null,
                    PlpStatuses = new List<String>() { "FakeStatus" },
                    PlpStatusDates = new List<DateTime?>() { null },
                    PlpStatusChgopr = new List<String>() { "" },
                    Recordkey = "571"
                };

                // PacLpAsgmts
                // Recordkey, plpahrpid, plpapaclppositionsid, type, plpaasgmtid, 
                var plpaHrpId = "0014076";
                var plpaPacLpPositionsId = "246";
                var plpaType = "V";
                var plpaAsgmtId = "BIOS*0014076";
                DataContracts.PacLpAsgmts pacLpAsgmt1 = new DataContracts.PacLpAsgmts()
                {
                    Recordkey = "274",
                    PlpaHrpId = plpaHrpId,
                    PlpaPacLpPositionsId = plpaPacLpPositionsId,
                    PlpaAsgmtId = plpaAsgmtId,
                    PlpaAsgmtType = plpaType
                };
                //paclpasgmt2
                var plpaHrpId2 = "0014076";
                var plpaPacLpPositionsId2 = "246";
                var plpaType2 = "S";
                var plpaAsgmtId2 = "19277";
                DataContracts.PacLpAsgmts pacLpAsgmt2 = new DataContracts.PacLpAsgmts()
                {
                    Recordkey = "289",
                    PlpaHrpId = plpaHrpId2,
                    PlpaPacLpPositionsId = plpaPacLpPositionsId2,
                    PlpaAsgmtId = plpaAsgmtId2,
                    PlpaAsgmtType = plpaType2
                };
                //paclpasgmt3
                var plpaHrpId3 = "0014076";
                var plpaPacLpPositionsId3 = "246";
                var plpaType3 = "M";
                var plpaAsgmtId3 = "BIOS*0014076";
                DataContracts.PacLpAsgmts pacLpAsgmt3 = new DataContracts.PacLpAsgmts()
                {
                    Recordkey = "292",
                    PlpaHrpId = plpaHrpId3,
                    PlpaPacLpPositionsId = plpaPacLpPositionsId3,
                    PlpaAsgmtId = plpaAsgmtId3,
                    PlpaAsgmtType = plpaType3
                };
                DataContracts.PacLpAsgmts pacLpAsgmt4 = new DataContracts.PacLpAsgmts()
                {
                    Recordkey = "123",
                    PlpaHrpId = plpaHrpId3,
                    PlpaPacLpPositionsId = plpaPacLpPositionsId3,
                    PlpaAsgmtId = plpaAsgmtId3,
                    PlpaAsgmtType = plpaType3
                };

                //pacLpPosition
                DataContracts.PacLpPositions pacLpPosition = new DataContracts.PacLpPositions()
                {
                    Recordkey = "246",
                    PlppIntendedLoad = 60,
                    PlppPacLoadPeriodsId = "175",
                    PlppPacLpAsgmtsIds = new List<string>() { "292", "289", "274", "123" },
                    PlppPositionId = "Professor of Everything"

                };
                DataContracts.PacLpPositions pacLpPosition2 = new DataContracts.PacLpPositions()
                {
                    Recordkey = "246",
                    PlppIntendedLoad = 60,
                    PlppPacLoadPeriodsId = "571",
                    PlppPacLpAsgmtsIds = new List<string>() { "292", "289", "274", "123" },
                    PlppPositionId = "Professor of Everything"

                };
                _pacLpPositionsCollection = new Collection<DataContracts.PacLpPositions>()
                {
                    pacLpPosition,
                    pacLpPosition2
                };
                _pacLpAsgmtsCollection = new Collection<DataContracts.PacLpAsgmts>()
                {
                    pacLpAsgmt1,
                    pacLpAsgmt2,
                    pacLpAsgmt3,
                    pacLpAsgmt4
                };

                _perAsgmtContractCollection = new Collection<DataContracts.PerAsgmtContract>()
                {
                    new DataContracts.PerAsgmtContract()
                    {
                        Recordkey = id4,
                        PacDesc = desc4,
                        PacHrpId = hrpid,
                        PacNo = pacNo,
                        PacType = type,
                        PacStartDate = startDate4,
                        PacEndDate = endDate4,
                        PacAllLoadPeriods = pacAllLps
                    },
                    {
                        pac2
                    }
                };

                _pacLoadPeriodsCollection = new Collection<DataContracts.PacLoadPeriods>() {
                    new DataContracts.PacLoadPeriods()
                    {
                        PlpHrpId = plpHrpId,
                        PlpIntendedTotalLoad = plpIntendedTotalLoad,
                        PlpLoadPeriod = plpLoadPeriod,
                        PlpPacLpPositionIds = plpPaclLpPositionIds,
                        PlpPerAsgmtContractId = plpPerAsgmtContractId,
                        PlpTotalValue = plpTotalValue,
                        PlpStatuses = plpStatuses,
                        PlpStatusDates = plpStatusDates,
                        PlpStatusChgopr = plpStatusChgopr,
                        Recordkey = recordKey
                    },
                    {
                        pacLp2
                    }
                };

                _hrssDefaultsCollection = new Collection<DataContracts.HrssDefaults>() {
                    new DataContracts.HrssDefaults()
                    {
                        _AppServerVersion = appSV,
                        Recordkey = hrssRK,
                        HrssOtCalcDefinitionId = hrssCalcID,
                        HrssHourlyRule = hrssHR,
                        HrssSalaryRule = hrssSalR,
                        HrssDfltWorkWeekStartDy = hrssWWStartD,
                        HrssDfltWorkWeekStartTm = hrssWWStartTi,
                        HrssFacContWebVisible = hrssVisible
                    }
                };

                MockRecords<DataContracts.PerAsgmtContract>("PER.ASGMT.CONTRACT", _perAsgmtContractCollection);
                MockRecords<DataContracts.PacLoadPeriods>("PAC.LOAD.PERIODS", _pacLoadPeriodsCollection);
                MockRecords<DataContracts.PacLpAsgmts>("PAC.LP.ASGMTS", _pacLpAsgmtsCollection);
                MockRecords<DataContracts.PacLpPositions>("PAC.LP.POSITIONS", _pacLpPositionsCollection);
                MockRecords<DataContracts.HrssDefaults>("HR.PARMS", _hrssDefaultsCollection);

                // Transaction involving the datacontract
                Transactions.AssignmentAmounts asgmt274 = new Transactions.AssignmentAmounts()
                {
                    AsgmtAmount = "274,000",
                    AssocEntryId = "",
                    PacLpAsgmtsId = "274"
                };
                Transactions.AssignmentAmounts asgmt289 = new Transactions.AssignmentAmounts()
                {
                    AsgmtAmount = "289,000",
                    AssocEntryId = "",
                    PacLpAsgmtsId = "289"
                };
                Transactions.AssignmentAmounts asgmt292 = new Transactions.AssignmentAmounts()
                {
                    AsgmtAmount = "292,000",
                    AssocEntryId = "",
                    PacLpAsgmtsId = "292"
                };

                var transactionResponse = new Transactions.GetFacultyAsgmtAmtsResponse()
                {
                    AssignmentAmounts = new List<Transactions.AssignmentAmounts>()
                    {
                        asgmt274,
                        asgmt289,
                        asgmt292
                    }
                };

                transManagerMock.Setup(r => r.ExecuteAsync<Transactions.GetFacultyAsgmtAmtsRequest, Transactions.GetFacultyAsgmtAmtsResponse>(It.IsAny<Transactions.GetFacultyAsgmtAmtsRequest>
                                    ())).ReturnsAsync(transactionResponse);
                dataReaderMock.Setup(r => r.SelectAsync("PER.ASGMT.CONTRACT", "WITH PAC.HRP.ID EQ '?'", new string[] { hrpid }, "?", true, 425)).ReturnsAsync(new string[] { id4, pac2.Recordkey });
                dataReaderMock.Setup(r => r.ReadRecordAsync<Data.HumanResources.DataContracts.HrssDefaults>("HR.PARMS", "HRSS.DEFAULTS", true)).ReturnsAsync(hrssDef2);
                facContractRepository = new FacultyContractRepository(cacheProvider, transFactory, logger, apiSettings);

            }
            #endregion

            [TestCleanup]
            public void Cleanup()
            {
                MockCleanup();
            }

            [TestMethod]
            public async Task FacultyContractRepository_GetFacultyContractsByFacultyIdsAsync()
            {
                var id = "0014076";
                var result = await facContractRepository.GetFacultyContractsByFacultyIdAsync(id);
                Assert.AreEqual(_pacLoadPeriodsCollection.First().Recordkey, result.First().Id);
                Assert.AreEqual("246", result.First().FacultyContractPositions.First().Id);
                Assert.AreEqual(4, result.First().FacultyContractPositions.First().FacultyContractAssignments.Count());
            }

            [TestMethod]
            public async Task FacultyContractRepository_NoTransactionDefaultValue()
            {
                var id = "0014076";
                var expectedAmount = "0";
                var result = await facContractRepository.GetFacultyContractsByFacultyIdAsync(id);
                var count = result.Count();
                Assert.AreEqual(expectedAmount, result.First().FacultyContractPositions.First().FacultyContractAssignments.Where(asgmt => asgmt.Id.Equals("123")).First().Amount);

            }

            [TestMethod]
            public async Task FacultyContractRepository_NoContracts()
            {
                var id = "01010101010";
                var result = await facContractRepository.GetFacultyContractsByFacultyIdAsync(id);
                Assert.AreEqual(0, result.Count());
            }

            [TestMethod]
            public async Task FacultyContractRepository_AllContractsShowWithValidStatus()
            {
                var id = "0014076";
                var result = await facContractRepository.GetFacultyContractsByFacultyIdAsync(id);

                //Resets the Status of the pacLp2 Contract to a visible status
                pacLp2.PlpStatuses = hrssDef2.HrssFacContWebVisible;
                
                var secondResult = await facContractRepository.GetFacultyContractsByFacultyIdAsync(id);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(2, secondResult.Count());
            }

            [TestMethod]
            public async Task FacultyContractRepository_NoContractsShowWithValidStatus()
            {
                var id = "0014076";
                var result = await facContractRepository.GetFacultyContractsByFacultyIdAsync(id);

                //Resets the Status of the pacLp2 Contract to a visible status
                pacLp2.PlpStatuses = hrssDef2.HrssFacContWebVisible;

                //Resets the list of valid statuses to a new status
                hrssDef2.HrssFacContWebVisible = new List<string>() {"No Status"};

                var secondResult = await facContractRepository.GetFacultyContractsByFacultyIdAsync(id);
                Assert.AreEqual(1, result.Count());
                Assert.AreEqual(0, secondResult.Count());
            }

        }
    }
}
