using Ellucian.Colleague.Data.Base.DataContracts;
/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Data.Colleague;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PositionRepositoryTests : BaseRepositorySetup
    {
        public TestPositionRepository testDataRepository;

        public PositionRepository repositoryUnderTest;

        public void PositionRepositoryTestsInitialize()
        {
            MockInitialize();
            testDataRepository = new TestPositionRepository();

            repositoryUnderTest = BuildRepository();
        }

        public PositionRepository BuildRepository()
        {
            dataReaderMock.Setup(d => d.SelectAsync("POSITION", ""))
                .Returns<string, string>((f, c) => Task.FromResult(testDataRepository.positionRecords == null ? null :
                    testDataRepository.positionRecords.Select(pos => pos.id).ToArray()));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Position>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) =>
                    Task.FromResult(testDataRepository.positionRecords == null ? null :
                        new Collection<DataContracts.Position>(testDataRepository.positionRecords
                            .Where(record => ids.Contains(record.id))
                            .Select(record =>
                                (record == null) ? null : new DataContracts.Position()
                                {
                                    Recordkey = record.id,
                                    AllPospay = record.positionPayIds,
                                    PosAltSuperPosId = record.alternateSupervisorPositionId,
                                    PosEndDate = record.endDate,
                                    PosExemptOrNot = record.exemptOrNot,
                                    PosHrlyOrSlry = record.hourlyOrSalary,
                                    PosShortTitle = record.shortTitle,
                                    PosDept = record.positionDept,
                                    PosLocation = record.positionLocation,
                                    PosStartDate = record.startDate,
                                    PosSupervisorPosId = record.supervisorPositionId,
                                    PosTimeEntryForm = record.timeEntryForm,
                                    PosTitle = record.title
                                }).ToList())
                    ));

            apiSettings.BulkReadSize = 1;

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new PositionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestClass]
        public class GetPositionsAsyncTests : PositionRepositoryTests
        {
            public async Task<IEnumerable<Position>> getExpectedPositions()
            {
                return await testDataRepository.GetPositionsAsync();
            }

            public async Task<IEnumerable<Position>> getActualPositions()
            {
                return await repositoryUnderTest.GetPositionsAsync();
            }

            [TestInitialize]
            public void Initialize()
            {
                PositionRepositoryTestsInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await getExpectedPositions()).ToList();
                var actual = (await getActualPositions()).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

            [TestMethod]
            public async Task AttributesTest()
            {
                var expected = (await getExpectedPositions()).ToArray();
                var actual = (await getActualPositions()).ToArray();
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected[i].Title, actual[i].Title);
                    Assert.AreEqual(expected[i].ShortTitle, actual[i].ShortTitle);
                    Assert.AreEqual(expected[i].PositionDept, actual[i].PositionDept);
                    Assert.AreEqual(expected[i].PositionLocation, actual[i].PositionLocation);
                    Assert.AreEqual(expected[i].StartDate, actual[i].StartDate);
                    Assert.AreEqual(expected[i].IsSalary, actual[i].IsSalary);
                    Assert.AreEqual(expected[i].EndDate, actual[i].EndDate);
                    Assert.AreEqual(expected[i].IsExempt, actual[i].IsExempt);
                    Assert.AreEqual(expected[i].SupervisorPositionId, actual[i].SupervisorPositionId);
                    Assert.AreEqual(expected[i].AlternateSupervisorPositionId, actual[i].AlternateSupervisorPositionId);
                    CollectionAssert.AreEqual(expected[i].PositionPayScheduleIds, actual[i].PositionPayScheduleIds);
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullPositionIdsTest()
            {
                testDataRepository.positionRecords = null;
                try
                {
                    await getActualPositions();
                }
                catch(Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            public async Task BulkReadLimitIsUsedTest()
            {
                Assert.AreEqual(1, apiSettings.BulkReadSize);
                await getActualPositions();
                dataReaderMock.Verify(r => r.BulkReadRecordAsync<DataContracts.Position>(It.IsAny<string[]>(), true), Times.Exactly(2));
            }

            [TestMethod]
            public async Task NullPositionRecordsFromDataReaderTest()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Position>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns(Task.FromResult<Collection<DataContracts.Position>>(null));

                await getActualPositions();

                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task LogDataErrorIfPositionStartDateIsNullTest()
            {
                testDataRepository.positionRecords.ForEach(p => p.startDate = null);
                Assert.IsFalse((await getActualPositions()).Any());
                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task LogDataErrorIfSalaryOrHourlyNotSpecifiedTest()
            {
                testDataRepository.positionRecords.ForEach(p => p.hourlyOrSalary = null);
                Assert.IsFalse((await getActualPositions()).Any());
                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));           
            }



        }

        [TestClass]
        public class PositionRepositoryTests_GET : BaseRepositorySetup
        {
            public TestPositionRepository testDataRepository;
            public PositionRepository positionRepository;

            Collection<DataContracts.Position> positions;
            IEnumerable<DataContracts.Pospay> positionPayDataContracts;

            int offset = 0;
            int limit = 4;

            string criteria = "WITH POS.START.DATE NE ''";

            [TestInitialize]
            public void Initialize() 
            {
                MockInitialize();
                positionRepository = BuildRepository();
                BuildData();
            }

            private void BuildData()
            {
                positions = new Collection<DataContracts.Position>() 
                {
                    new DataContracts.Position()
                    { 
                        RecordGuid = "f0b4ee37-a939-47bd-af01-60ea40c73b11", 
                        Recordkey = "0000071813", 
                        PosExemptOrNot = "E", 
                        PosAltSuperPosId = "FWWREGI",
                        PosDept = "REG",
                        PosLocation = "HC",
                        AllPospay = new List<string>(){ "1" },
                        PosShortTitle = "Assistant Registrar",
                        PosStartDate = new DateTime(2016, 07, 01),
                        PosSupervisorPosId = "ZREG40110REGI",
                        PosTitle = "Assistant Registrar",
                        PosHrlyOrSlry = "H",
                        PosAuthorizedDate = DateTime.Now,
                        PosClass = "PosClass1"
                    },
                     new DataContracts.Position()
                    { 
                        RecordGuid = "2091962f-bc2b-44d9-af03-56f94c453475", 
                        Recordkey = "0000071913", 
                        PosExemptOrNot = "NE", 
                        PosAltSuperPosId = "SB1",
                        PosDept = "REG",
                        PosLocation = "HC",
                        AllPospay = new List<string>(){ "2" },
                        PosShortTitle = "Registrar",
                        PosStartDate = new DateTime(2016, 09, 01),
                        PosSupervisorPosId = "XREG40110REGJ",
                        PosTitle = "Registrar",
                        PosHrlyOrSlry = "S",
                        PosAuthorizedDate = DateTime.Now,
                        PosClass = "PosClass2"
                    }, 
                    new DataContracts.Position()
                    { 
                        RecordGuid = "5bc2d86c-6a0c-46b1-824d-485ccb27dc67", 
                        Recordkey = "0000072013", 
                        PosExemptOrNot = "E", 
                        PosAltSuperPosId = "SB3",
                        PosDept = "MUSC",
                        PosLocation = "HC",
                        AllPospay = new List<string>(){ "3" },
                        PosShortTitle = "Instrumental Music Inst",
                        PosStartDate = new DateTime(2016, 07, 01),
                        PosSupervisorPosId = "ZREG40110REGI",
                        PosTitle = "Instrumental Music Inst",
                        PosHrlyOrSlry = "H",
                        PosAuthorizedDate = DateTime.Now,
                        PosClass = "PosClass3"
                    }, 
                    new DataContracts.Position()
                    { 
                        RecordGuid = "f0b4ee37-a939-47bd-af01-60ea40c73b11", 
                        Recordkey = "0000072113", 
                        PosExemptOrNot = "NE", 
                        PosAltSuperPosId = "FWWREGI",
                        PosDept = "REG",
                        PosLocation = "HC",
                        AllPospay = new List<string>(){ "1" },
                        PosShortTitle = "Associate Registrar",
                        PosStartDate = new DateTime(2016, 07, 01),
                        PosSupervisorPosId = "ZREG40110REGI",
                        PosTitle = "Associate Registrar",
                        PosHrlyOrSlry = "S",
                        PosAuthorizedDate = DateTime.Now,
                        PosClass = "PosClass4"
                    },                
                };
                positionPayDataContracts = new List<DataContracts.Pospay>() 
                {
                    new DataContracts.Pospay()
                    {
                        Recordkey = "1",                        
                        PospayAuthorizedDate = new DateTime(2016, 08, 01),
                        PospayBargainingUnit = "PospayBargainingUnit1",
                        PospayCycleWorkTimeAmt = 1,
                        PospayCycleWorkTimeUnits = "PospayCycleWorkTimeUnits1",
                        PospayEndDate = new DateTime(2016, 12, 31),
                        PospayFndgEntityAssociation = new List<DataContracts.PospayPospayFndg>()
                        { 
                            new DataContracts.PospayPospayFndg()
                            {
                                PospayFndgGlNoAssocMember = "11-01-01-00-10408-51001",
                                PospayFndgPctAssocMember = 1,
                                PospayFndgPrjItemIdAssocMember = "PospayFndgPrjItemIdAssocMember1",
                                PospayFndgProjIdAssocMember = "PospayFndgProjIdAssocMember",
                                PospayFndgSourceAssocMember = "PospayFndgSourceAssocMember1"
                            }
                        },
                        PospayFndgGlNo = new List<string>(){ "11-01-01-00-10408-51001" },
                        PospayFndgPct = new List<decimal?>(){1, 2},
                        PospayFndgPrjItemId = new List<string>(){ "1", "2" },
                        PospayStartDate = new DateTime(2016, 10, 01),
                        PospaySalaryMin = "45000",
                        PospaySalaryMax = "75000",
                        PospayYearWorkTimeAmt = 2016
                    }
                };
            }

            [TestMethod]
            public async Task PositionRepository_GET_ALL()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("POSITION", criteria)).ReturnsAsync(new List<string>() { "0000071813", "0000071913", "0000072013", "0000072113" }.ToArray());
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DataContracts.Position>("POSITION", new string[] { "0000071813", "0000071913", "0000072013", "0000072113" }, It.IsAny<bool>())).ReturnsAsync(positions);
                var actuals =
                    await
                        positionRepository.GetPositionsAsync(offset, limit, It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());

                Assert.IsNotNull(actuals.Item1);

                for (int i = 0; i < actuals.Item1.Count(); i++)
                {
                    var expected = positions[i];
                    var actual = actuals.Item1.ToList()[i];

                    Assert.AreEqual(expected.AllPospay.Count(), actual.PositionPayScheduleIds.Count);
                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.PosAltSuperPosId, actual.AlternateSupervisorPositionId);
                    Assert.AreEqual(expected.PosAuthorizedDate, actual.PositionAuthorizedDate);
                    Assert.AreEqual(expected.PosClass, actual.PositionClass);
                    Assert.AreEqual(expected.PosDept, actual.PositionDept);
                    Assert.AreEqual(expected.PosEndDate, actual.EndDate);
                    Assert.AreEqual(expected.PosExemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase), actual.IsExempt);
                    Assert.AreEqual(expected.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase), actual.IsSalary);
                    Assert.AreEqual(expected.PosJobDesc, actual.PositionJobDesc);
                    Assert.AreEqual(expected.PosLocation, actual.PositionLocation);
                    Assert.AreEqual(expected.PosShortTitle, actual.ShortTitle);
                    Assert.AreEqual(expected.PosStartDate, actual.StartDate);
                    Assert.AreEqual(expected.PosSupervisorPosId, actual.SupervisorPositionId);
                    Assert.AreEqual(expected.PosTitle, actual.Title);
                }
            }

            [TestMethod]
            public async Task PositionRepository_GET_ALL_Filtered()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("POSITION", It.IsAny<string>())).ReturnsAsync(new List<string>() { "0000071813", "0000071913", "0000072013", "0000072113" }.ToArray());
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DataContracts.Position>("POSITION", new string[] { "0000071813", "0000071913", "0000072013", "0000072113" }, It.IsAny<bool>())).ReturnsAsync(positions);
                var actuals =
                    await
                        positionRepository.GetPositionsAsync(offset, limit, "campus", "active",
                            "bargainingUnit", new List<string>() { "reportsToPosition" }, "exempt", "salary",
                            "startOn", "endOn", It.IsAny<bool>());

                Assert.IsNotNull(actuals.Item1);

                for (int i = 0; i < actuals.Item1.Count(); i++)
                {
                    var expected = positions[i];
                    var actual = actuals.Item1.ToList()[i];

                    Assert.AreEqual(expected.AllPospay.Count(), actual.PositionPayScheduleIds.Count);
                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.PosAltSuperPosId, actual.AlternateSupervisorPositionId);
                    Assert.AreEqual(expected.PosAuthorizedDate, actual.PositionAuthorizedDate);
                    Assert.AreEqual(expected.PosClass, actual.PositionClass);
                    Assert.AreEqual(expected.PosDept, actual.PositionDept);
                    Assert.AreEqual(expected.PosEndDate, actual.EndDate);
                    Assert.AreEqual(expected.PosExemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase), actual.IsExempt);
                    Assert.AreEqual(expected.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase), actual.IsSalary);
                    Assert.AreEqual(expected.PosJobDesc, actual.PositionJobDesc);
                    Assert.AreEqual(expected.PosLocation, actual.PositionLocation);
                    Assert.AreEqual(expected.PosShortTitle, actual.ShortTitle);
                    Assert.AreEqual(expected.PosStartDate, actual.StartDate);
                    Assert.AreEqual(expected.PosSupervisorPosId, actual.SupervisorPositionId);
                    Assert.AreEqual(expected.PosTitle, actual.Title);
                }
            }

            [TestMethod]
            public async Task PositionRepository_GET_ALL_Filtered_2()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("POSITION", It.IsAny<string>())).ReturnsAsync(new List<string>() { "0000071813", "0000071913", "0000072013", "0000072113" }.ToArray());
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DataContracts.Position>("POSITION", new string[] { "0000071813", "0000071913", "0000072013", "0000072113" }, It.IsAny<bool>())).ReturnsAsync(positions);
                var actuals =
                    await
                        positionRepository.GetPositionsAsync(offset, limit, "campus", "inactive",
                            "bargainingUnit", new List<string>() { "reportsToPosition" }, "nonexempt", "wages",
                            "startOn", "endOn", It.IsAny<bool>());

                Assert.IsNotNull(actuals.Item1);

                for (int i = 0; i < actuals.Item1.Count(); i++)
                {
                    var expected = positions[i];
                    var actual = actuals.Item1.ToList()[i];

                    Assert.AreEqual(expected.AllPospay.Count(), actual.PositionPayScheduleIds.Count);
                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.PosAltSuperPosId, actual.AlternateSupervisorPositionId);
                    Assert.AreEqual(expected.PosAuthorizedDate, actual.PositionAuthorizedDate);
                    Assert.AreEqual(expected.PosClass, actual.PositionClass);
                    Assert.AreEqual(expected.PosDept, actual.PositionDept);
                    Assert.AreEqual(expected.PosEndDate, actual.EndDate);
                    Assert.AreEqual(expected.PosExemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase), actual.IsExempt);
                    Assert.AreEqual(expected.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase), actual.IsSalary);
                    Assert.AreEqual(expected.PosJobDesc, actual.PositionJobDesc);
                    Assert.AreEqual(expected.PosLocation, actual.PositionLocation);
                    Assert.AreEqual(expected.PosShortTitle, actual.ShortTitle);
                    Assert.AreEqual(expected.PosStartDate, actual.StartDate);
                    Assert.AreEqual(expected.PosSupervisorPosId, actual.SupervisorPositionId);
                    Assert.AreEqual(expected.PosTitle, actual.Title);
                }
            }

            [TestMethod]
            public async Task PositionRepository_GET_ALL_Filtered_3()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("POSITION", It.IsAny<string>())).ReturnsAsync(new List<string>() { "0000071813", "0000071913", "0000072013", "0000072113" }.ToArray());
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DataContracts.Position>("POSITION", new string[] { "0000071813", "0000071913", "0000072013", "0000072113" }, It.IsAny<bool>())).ReturnsAsync(positions);
                var actuals =
                    await
                        positionRepository.GetPositionsAsync(offset, limit, "campus", "cancelled",
                            "bargainingUnit", new List<string>() { "reportsToPosition" }, "nonexempt", "wages",
                            "startOn", "endOn", It.IsAny<bool>());

                Assert.IsNotNull(actuals.Item1);

                for (int i = 0; i < actuals.Item1.Count(); i++)
                {
                    var expected = positions[i];
                    var actual = actuals.Item1.ToList()[i];

                    Assert.AreEqual(expected.AllPospay.Count(), actual.PositionPayScheduleIds.Count);
                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.PosAltSuperPosId, actual.AlternateSupervisorPositionId);
                    Assert.AreEqual(expected.PosAuthorizedDate, actual.PositionAuthorizedDate);
                    Assert.AreEqual(expected.PosClass, actual.PositionClass);
                    Assert.AreEqual(expected.PosDept, actual.PositionDept);
                    Assert.AreEqual(expected.PosEndDate, actual.EndDate);
                    Assert.AreEqual(expected.PosExemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase), actual.IsExempt);
                    Assert.AreEqual(expected.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase), actual.IsSalary);
                    Assert.AreEqual(expected.PosJobDesc, actual.PositionJobDesc);
                    Assert.AreEqual(expected.PosLocation, actual.PositionLocation);
                    Assert.AreEqual(expected.PosShortTitle, actual.ShortTitle);
                    Assert.AreEqual(expected.PosStartDate, actual.StartDate);
                    Assert.AreEqual(expected.PosSupervisorPosId, actual.SupervisorPositionId);
                    Assert.AreEqual(expected.PosTitle, actual.Title);
                }
            }

            [TestMethod]
            public async Task PositionRepository_GET_ALL_Filtered_4()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("POSITION", It.IsAny<string>())).ReturnsAsync(new List<string>() { "0000071813", "0000071913", "0000072013", "0000072113" }.ToArray());
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DataContracts.Position>("POSITION", new string[] { "0000071813", "0000071913", "0000072013", "0000072113" }, It.IsAny<bool>())).ReturnsAsync(positions);
                var actuals =
                    await
                        positionRepository.GetPositionsAsync(offset, limit, "campus", "defaultStatus",
                            "bargainingUnit", new List<string>() { "reportsToPosition" }, "defaultExemption", "defaultCompensation",
                            "startOn", "endOn", It.IsAny<bool>());

                Assert.IsNotNull(actuals.Item1);

                for (int i = 0; i < actuals.Item1.Count(); i++)
                {
                    var expected = positions[i];
                    var actual = actuals.Item1.ToList()[i];

                    Assert.AreEqual(expected.AllPospay.Count(), actual.PositionPayScheduleIds.Count);
                    Assert.AreEqual(expected.RecordGuid, actual.Guid);
                    Assert.AreEqual(expected.PosAltSuperPosId, actual.AlternateSupervisorPositionId);
                    Assert.AreEqual(expected.PosAuthorizedDate, actual.PositionAuthorizedDate);
                    Assert.AreEqual(expected.PosClass, actual.PositionClass);
                    Assert.AreEqual(expected.PosDept, actual.PositionDept);
                    Assert.AreEqual(expected.PosEndDate, actual.EndDate);
                    Assert.AreEqual(expected.PosExemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase), actual.IsExempt);
                    Assert.AreEqual(expected.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase), actual.IsSalary);
                    Assert.AreEqual(expected.PosJobDesc, actual.PositionJobDesc);
                    Assert.AreEqual(expected.PosLocation, actual.PositionLocation);
                    Assert.AreEqual(expected.PosShortTitle, actual.ShortTitle);
                    Assert.AreEqual(expected.PosStartDate, actual.StartDate);
                    Assert.AreEqual(expected.PosSupervisorPosId, actual.SupervisorPositionId);
                    Assert.AreEqual(expected.PosTitle, actual.Title);
                }
            }

            [TestMethod]
            public async Task PositionRepository_GET_ById()
            {
                var guid = "f0b4ee37-a939-47bd-af01-60ea40c73b11";
                var id = "0000071813";
                Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
                result.Add("POSITION", new GuidLookupResult() { Entity = "POSITION", PrimaryKey = id, SecondaryKey = id }); ;
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Position>("POSITION", id, It.IsAny<bool>())).ReturnsAsync(positions.ToList()[0]);
                var actual = await positionRepository.GetPositionByGuidAsync(guid);
                Assert.IsNotNull(actual);

                var expected = positions.ToList()[0];

                Assert.AreEqual(expected.AllPospay.Count(), actual.PositionPayScheduleIds.Count);
                Assert.AreEqual(expected.RecordGuid, actual.Guid);
                Assert.AreEqual(expected.PosAltSuperPosId, actual.AlternateSupervisorPositionId);
                Assert.AreEqual(expected.PosAuthorizedDate, actual.PositionAuthorizedDate);
                Assert.AreEqual(expected.PosClass, actual.PositionClass);
                Assert.AreEqual(expected.PosDept, actual.PositionDept);
                Assert.AreEqual(expected.PosEndDate, actual.EndDate);
                Assert.AreEqual(expected.PosExemptOrNot.Equals("E", StringComparison.InvariantCultureIgnoreCase), actual.IsExempt);
                Assert.AreEqual(expected.PosHrlyOrSlry.Equals("S", StringComparison.InvariantCultureIgnoreCase), actual.IsSalary);
                Assert.AreEqual(expected.PosJobDesc, actual.PositionJobDesc);
                Assert.AreEqual(expected.PosLocation, actual.PositionLocation);
                Assert.AreEqual(expected.PosShortTitle, actual.ShortTitle);
                Assert.AreEqual(expected.PosStartDate, actual.StartDate);
                Assert.AreEqual(expected.PosSupervisorPosId, actual.SupervisorPositionId);
                Assert.AreEqual(expected.PosTitle, actual.Title);
            }
            
            [TestMethod]
            public async Task PositionRepository_GET_ById_EmptyId_Returns_Null()
            {
                var guid = "";
                var actual = await positionRepository.GetPositionByGuidAsync(guid);
                Assert.IsNull(actual);
            }

            [TestMethod]            
            public async Task PositionRepository_GET_ById_Exception_Returns_Null()
            {
                var guid = "f0b4ee37-a939-47bd-af01-60ea40c73b11";
                var id = "0000071813";
                Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
                result.Add("POSITION", new GuidLookupResult() { Entity = "POSITION", PrimaryKey = id, SecondaryKey = id }); ;
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Position>("POSITION", id, It.IsAny<bool>())).ThrowsAsync(new Exception());
                var actual = await positionRepository.GetPositionByGuidAsync(guid);
                Assert.IsNull(actual);
            }

            [TestMethod]
            public async Task PositionRepository_GET_ById_KeyNotFoundException_ReturnsNull()
            {
                var guid = "f0b4ee37-a939-47bd-af01-60ea40c73b11";
                var id = "0000071813";
                Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
                result.Add("POSITION", new GuidLookupResult() { Entity = "POSITION", PrimaryKey = id, SecondaryKey = id }); ;
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(null);
                try
                {
                    var actual = await positionRepository.GetPositionByGuidAsync(guid);
                }
                catch (Exception e)
                {
                    Assert.IsInstanceOfType(e, typeof(KeyNotFoundException));
                }
            }

            [TestMethod]
            public async Task PositionRepository_GET_ById_FoundEntryNull_KeyNotFoundException_ReturnsNull()
            {
                var guid = "f0b4ee37-a939-47bd-af01-60ea40c73b11";
                Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
                result.Add("POSITION", null);
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);
                try
                {
                    var actual = await positionRepository.GetPositionByGuidAsync(guid);
                }
                catch (Exception e)
                {
                    Assert.IsInstanceOfType(e, typeof(KeyNotFoundException));
                }
            }

            [TestMethod]
            public async Task PositionRepository_GET_ById_RepositoryException_Returns_Null()
            {
                var guid = "f0b4ee37-a939-47bd-af01-60ea40c73b11";
                var id = "0000071813";
                Dictionary<string, GuidLookupResult> result = new Dictionary<string, GuidLookupResult>();
                result.Add("POSITION", new GuidLookupResult() { Entity = "POSITIO", PrimaryKey = id, SecondaryKey = id }); ;
                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<GuidLookup[]>())).ReturnsAsync(result);

                dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Position>("POSITION", id, It.IsAny<bool>())).ReturnsAsync(positions.ToList()[0]);
                try
                {
                    var actual = await positionRepository.GetPositionByGuidAsync(guid);
                }
                catch (Exception e)
                {
                    Assert.IsInstanceOfType(e, typeof(RepositoryException));
                }
            }

            [TestMethod]
            public async Task PositionRepository_PositionGuidFromIdAsync()
            {
                var id = "ZREG40110REGI";
                var expectedGuid = "f0b4ee37-a939-47bd-af01-60ea40c73b11";
                var lookup = new RecordKeyLookup("POSITION", id, false);
                Dictionary<string, RecordKeyLookupResult> result = new Dictionary<string, RecordKeyLookupResult>();
                result.Add("POSITION+ZREG40110REGI", new RecordKeyLookupResult() { Guid = expectedGuid, ModelName = "POSITION" });

                dataReaderMock.Setup(dr => dr.SelectAsync(It.IsAny<RecordKeyLookup[]>())).ReturnsAsync(result);
                var actual = await positionRepository.GetPositionGuidFromIdAsync(id);
                Assert.AreEqual(expectedGuid, actual);
            }

            [TestMethod]
            public async Task PositionRepository_PositionPay_GetPositionPayByIdsAsync()
            {
                Projects proj = new Projects(){ PrjRefNo = "1" };
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Pospay>("POSPAY", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(positionPayDataContracts.ToList()[0]);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Projects>("PROJECTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proj);

                var actual = await positionRepository.GetPositionPayByIdsAsync(new List<string>() { "1" });

                Assert.IsNotNull(actual);
                Assert.AreEqual(1, actual.Count());
            }

            [TestMethod]
            public async Task PositionRepository_PositionPay_GetPositionPayByIdsAsync_IdNull_Log_Error()
            {
                Projects proj = new Projects() { PrjRefNo = "1" };
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Pospay>("POSPAY", It.IsAny<string>(), It.IsAny<bool>())).ThrowsAsync(new Exception());
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Projects>("PROJECTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proj);

                var actual = await positionRepository.GetPositionPayByIdsAsync(new List<string>() { "1" });
                Assert.IsNull(actual.ToList()[0]);
            }

            [TestMethod]
            public async Task PositionRepository_PositionPay_GetPositionPayByIdsAsync_NullPositionPay_Log_Error()
            {
                Projects proj = new Projects() { PrjRefNo = "1" };
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<DataContracts.Pospay>("POSPAY", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(null);
                dataReaderMock.Setup(dr => dr.ReadRecordAsync<Projects>("PROJECTS", It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(proj);

                var actual = await positionRepository.GetPositionPayByIdsAsync(new List<string>() { "1" });
                Assert.IsNull(actual.ToList()[0]);
            }

            [TestMethod]
            [ExpectedExceptionAttribute(typeof(ArgumentNullException))]
            public async Task PositionRepository_PositionPay_GetPositionPayByIdsAsync_ArgumentNullException()
            {
                var actual = await positionRepository.GetPositionPayByIdsAsync(null);
            }

            [TestMethod]
            [ExpectedExceptionAttribute(typeof(ArgumentNullException))]
            public async Task PositionRepository_PositionGuidFromIdAsync_ArgumentNullException()
            {
                var actual = await positionRepository.GetPositionGuidFromIdAsync("");
            }

            [TestMethod]
            [ExpectedExceptionAttribute(typeof(ArgumentOutOfRangeException))]
            public async Task PositionRepository_PositionGuidFromIdAsync_ArgumentOutOfRangeException()
            {
                var actual = await positionRepository.GetPositionGuidFromIdAsync("ZREG40110REGI");
            }

            [TestMethod]
            [ExpectedExceptionAttribute(typeof(RepositoryException))]
            public async Task PositionRepository_GET_ALL_RepositoryException()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("POSITION", criteria)).ThrowsAsync(new RepositoryException());
                var actuals = await positionRepository.GetPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            [TestMethod]
            [ExpectedExceptionAttribute(typeof(ArgumentNullException))]
            public async Task PositionRepository_GET_ALL_NUllEntity_RepositoryException()
            {
                dataReaderMock.Setup(dr => dr.SelectAsync("POSITION", criteria)).ReturnsAsync(new List<string>() { "0000071813", "0000071913", "0000072013", "0000072113" }.ToArray());
                dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<DataContracts.Position>("POSITION", new string[] { "0000071813", "0000071913", "0000072013", "0000072113" }, It.IsAny<bool>()))
                    .ReturnsAsync(new Collection<DataContracts.Position>() { null });
                var actuals = await positionRepository.GetPositionsAsync(offset, limit, It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(),
                            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>());
            }

            public PositionRepository BuildRepository()
            {
                apiSettings.BulkReadSize = 1;

                loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

                return new PositionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            }
        }
    }
}
