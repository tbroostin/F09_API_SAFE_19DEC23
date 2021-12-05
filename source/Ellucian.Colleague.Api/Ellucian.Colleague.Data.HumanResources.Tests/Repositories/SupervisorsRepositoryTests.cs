/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class SupervisorsRepositoryTests : BaseRepositorySetup
    {
        // test data
        public TestSupervisorsRepository testRepository;
        public SupervisorsRepository actualRepository;

        public string supervisorId;
        public string superviseeId;
        public DateTime? lookbackDate;

        // mocks
        public SupervisorsRepository BuildRepository()
        {
            // select perpos with hrpId
            dataReaderMock.Setup(d => d.SelectAsync("PERPOS", It.Is<string>(s => s.Contains("WITH PERPOS.HRP.ID EQ"))))
                .Returns<string, string>((f, criteria) =>
                        Task.FromResult((testRepository.PerposRecords == null) ? null : lookbackDate.HasValue ?
                            testRepository.PerposRecords
                            .Where(rec => criteria.Contains(rec.PerposSupervisorHrpId) && (!rec.PerposEndDate.HasValue || rec.PerposEndDate.Value >= lookbackDate))
                            .Select(rec => rec.RecordKey).ToArray() : testRepository.PerposRecords
                            .Where(rec => criteria.Contains(rec.PerposSupervisorHrpId))
                            .Select(rec => rec.RecordKey).ToArray()
                        ));

            // select perpos supervisor id
            dataReaderMock.Setup(d => d.SelectAsync("PERPOS", "WITH PERPOS.SUPERVISOR.HRP.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                        Task.FromResult((testRepository.PerposRecords == null) ? null :
                            testRepository.PerposRecords
                            .Where(rec => values.Contains(rec.PerposHrpId))
                            .Select(rec => rec.RecordKey).ToArray()
                        ));

            //select perpos by position id
            dataReaderMock.Setup(d => d.SelectAsync("PERPOS", It.Is<string>(c => c.Contains("AND WITH PERPOS.POSITION.INDEX EQ ?")), It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                    Task.FromResult((testRepository.PerposRecords == null) ? null :
                        testRepository.PerposRecords
                        .Where(rec => values.Contains(rec.PerposPositionId))
                        .Select(rec => rec.RecordKey).ToArray()
                    ));

            //select perpos by direct supervisor id
            dataReaderMock.Setup(d => d.SelectAsync("PERPOS", It.Is<string>(c => c.Contains("WITH INDEX.PERPOS.SUPERVISOR EQ " + It.IsAny<string>()))))
                .Returns<string, string>((f, criteria) =>
                    Task.FromResult((testRepository.PerposRecords == null) ? null : lookbackDate.HasValue ?
                        testRepository.PerposRecords
                        .Where(rec => !string.IsNullOrEmpty(rec.PerposSupervisorHrpId) && criteria.Contains(rec.PerposSupervisorHrpId)
                        && (!rec.PerposEndDate.HasValue || rec.PerposEndDate.Value >= lookbackDate))
                        .Select(rec => rec.RecordKey).ToArray() : testRepository.PerposRecords
                        .Where(rec => criteria.Contains(rec.PerposSupervisorHrpId))
                        .Select(rec => rec.RecordKey).ToArray()
                    ));

            //select perpos by position id without direct supervisor
            dataReaderMock.Setup(d => d.SelectAsync("PERPOS", It.Is<string>(c => c.Contains("WITH PERPOS.POSITION.INDEX EQ ? AND WITH INDEX.PERPOS.SUPERVISOR EQ ''"
                + It.IsAny<string>())),
                It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, b, i) =>
                    {
                        var idsList = values.ToList();
                        return Task.FromResult(testRepository.PerposRecords == null ? null : lookbackDate.HasValue ?
                        testRepository.PerposRecords
                        .Where(rec => idsList.Contains(string.Format("\"{0}\"", rec.PerposPositionId)) && string.IsNullOrEmpty(rec.PerposSupervisorHrpId)
                        && (!rec.PerposEndDate.HasValue || rec.PerposEndDate.Value >= lookbackDate))
                        .Select(rec => rec.RecordKey).ToArray() :
                        testRepository.PerposRecords
                        .Where(rec => idsList.Contains(string.Format("\"{0}\"", rec.PerposPositionId)) && string.IsNullOrEmpty(rec.PerposSupervisorHrpId))
                        .Select(rec => rec.RecordKey).ToArray());
                    });

            // read perpos with list of keys
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perpos>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testRepository.PerposRecords == null ? null :
                    new Collection<Perpos>(
                        testRepository.PerposRecords
                        .Where(rec => ids.Contains(rec.RecordKey))
                        .Select(rec => new Perpos()
                        {
                            Recordkey = rec.RecordKey,
                            PerposAltSupervisorId = rec.PerposAltSupervisorId,
                            PerposSupervisorHrpId = rec.PerposSupervisorHrpId,
                            PerposStartDate = rec.PerposStartDate,
                            PerposPositionId = rec.PerposPositionId,
                            PerposHrpId = rec.PerposHrpId,
                            PerposEndDate = rec.PerposEndDate,
                        }).ToList()
                    )));

            // read from perpos with selection critiera
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perpos>(It.Is<string>(c => c.Contains("WITH PERPOS.HRP.ID EQ")), It.IsAny<bool>()))
                .Returns<string, bool>((ids, b) => Task.FromResult(testRepository.PerposRecords == null ? null : lookbackDate.HasValue ?
                new Collection<Perpos>(
                        testRepository.PerposRecords
                        .Where(rec => rec.PerposHrpId == supervisorId && (!rec.PerposEndDate.HasValue || rec.PerposEndDate.Value >= lookbackDate))
                        .Select(rec => new Perpos()
                        {
                            Recordkey = rec.RecordKey,
                            PerposAltSupervisorId = rec.PerposAltSupervisorId,
                            PerposSupervisorHrpId = rec.PerposSupervisorHrpId,
                            PerposStartDate = rec.PerposStartDate,
                            PerposPositionId = rec.PerposPositionId,
                            PerposHrpId = rec.PerposHrpId,
                            PerposEndDate = rec.PerposEndDate
                        }).ToList()
                    )
                    : new Collection<Perpos>(
                        testRepository.PerposRecords
                        .Where(rec => rec.PerposHrpId == supervisorId)
                        .Select(rec => new Perpos()
                        {
                            Recordkey = rec.RecordKey,
                            PerposAltSupervisorId = rec.PerposAltSupervisorId,
                            PerposSupervisorHrpId = rec.PerposSupervisorHrpId,
                            PerposStartDate = rec.PerposStartDate,
                            PerposPositionId = rec.PerposPositionId,
                            PerposHrpId = rec.PerposHrpId,
                            PerposEndDate = rec.PerposEndDate
                        }).ToList()
                    )));



            //read single record from HRPER
            dataReaderMock.Setup(d => d.ReadRecordAsync<Hrper>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((supervisorId, b) => Task.FromResult(new Hrper()
                {
                    Recordkey = supervisorId,
                    HrpNonempPosition = "ZNONEMPLYPOS"
                }));

            //select Positions by supervisory position
            dataReaderMock.Setup(d => d.SelectAsync("POSITION", "WITH INDEX.POS.SUPER EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, b, i) =>
                {
                    if (testRepository.PositionRecords != null)
                    {
                        var idsList = values.ToList();
                        var selectedRecords = testRepository.PositionRecords.Where(pr => !string.IsNullOrEmpty(pr.PosSupervisorPosId)
                            && idsList.Contains(string.Format("\"{0}\"", pr.PosSupervisorPosId)));
                        var ids = selectedRecords.Select(rec => rec.Recordkey).ToArray();
                        return Task.FromResult(ids);
                    }
                    else return Task.FromResult((string[])null);

                });



            //read from Positions using keys
            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Position>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((keys, b) => Task.FromResult(testRepository.PositionRecords == null ? null :
                    new Collection<DataContracts.Position>(testRepository.PositionRecords
                        .Where(rec => keys.Contains(rec.Recordkey))
                        .Select(rec => new DataContracts.Position()
                        {
                            Recordkey = rec.Recordkey,
                            AllPospay = rec.AllPospay,
                            PosDept = rec.PosDept,
                            PosEndDate = rec.PosEndDate,
                            PosExemptOrNot = rec.PosExemptOrNot,
                            PosHrlyOrSlry = rec.PosHrlyOrSlry,
                            PosLocation = rec.PosLocation,
                            PosShortTitle = rec.PosShortTitle,
                            PosStartDate = rec.PosStartDate,
                            PosSupervisorPosId = rec.PosSupervisorPosId,
                            PosTimeEntryForm = rec.TimeEntryType,
                            PosTitle = rec.PosTitle,
                            PosAltSuperPosId = rec.PosAltSuperPosId,
                        }).ToList())));




            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);
            apiSettings.BulkReadSize = 1;

            return new SupervisorsRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        // test setup
        public void SupervisorsRepositoryTestsInitialize()
        {
            supervisorId = "24601";

            base.MockInitialize();
            testRepository = new TestSupervisorsRepository();
            actualRepository = BuildRepository();

        }

        // tests
        [TestClass]
        public class GetSuperviseesBySupervisorAsyncTests : SupervisorsRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                base.SupervisorsRepositoryTestsInitialize();
            }

            #region DIRECT SUPERVISORS

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task NullOrEmptySupervisorIdArgTest()
            {
                supervisorId = string.Empty;
                await actualRepository.GetSuperviseesBySupervisorAsync(supervisorId);
            }

            [TestMethod]
            public async Task NullPerposKeysTest()
            {
                dataReaderMock.Setup(d => d.SelectAsync("PERPOS", It.IsAny<string>())).Returns(Task.FromResult((string[])null));
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perpos>(It.IsAny<string[]>(), true)).ReturnsAsync(() => null);
                var emptyList = await actualRepository.GetSuperviseesBySupervisorAsync(supervisorId);

                Assert.IsFalse(emptyList.Any());


            }

            // no perpos keys noted logger info 54

            // null bulkrecordread error 66

            // null perposhrpid logdataerror 74

            [TestMethod]
            public async Task ExpectedSubordinateIdsReturnedTest()
            {
                var expected = await testRepository.GetSuperviseesBySupervisorAsync(supervisorId);
                var actual = await actualRepository.GetSuperviseesBySupervisorAsync(supervisorId);
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
            }

            [TestMethod]
            public async Task LookbackDatePassedIn_LaterThanEndDate_ExpectedSubordinateIdsReturnedTest()
            {
                lookbackDate = new DateTime(2020, 01, 02);
                var expected = await testRepository.GetSuperviseesBySupervisorAsync(supervisorId, lookbackDate);
                var actual = await actualRepository.GetSuperviseesBySupervisorAsync(supervisorId, lookbackDate);
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
            }

            [TestMethod]
            public async Task LookbackDatePassedIn_EarlierThanEndDate_ExpectedSubordinateIdsReturnedTest()
            {
                lookbackDate = new DateTime(1999, 01, 01);
                var expected = await testRepository.GetSuperviseesBySupervisorAsync(supervisorId, lookbackDate);
                var actual = await actualRepository.GetSuperviseesBySupervisorAsync(supervisorId, lookbackDate);
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
            }

            #endregion

            #region POSITION SUPERVISORS


            #endregion
        }


        [TestClass]
        public class GetSupervisorIdsForPositionsAsyncTests : SupervisorsRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                SupervisorsRepositoryTestsInitialize();
                superviseeId = "24601";

                // mock call to getSuperviseePerposIds to simulate lookback
                dataReaderMock.Setup(d => d.SelectAsync("PERPOS", It.Is<string>(s => s.Contains("WITH PERPOS.HRP.ID EQ"))))
                .Returns<string, string>((f, criteria) =>
                        Task.FromResult(
                            testRepository.PerposRecords
                            .Where(rec => criteria.Contains(superviseeId) && rec.PerposHrpId == superviseeId && (!rec.PerposEndDate.HasValue || rec.PerposEndDate.Value >= lookbackDate))
                            .Select(rec => rec.RecordKey).ToArray()
                        ));
            }

            [TestMethod]
            public async Task ExecuteTest()
            {
                var result = await actualRepository.GetSupervisorIdsForPositionsAsync(new string[1] { "TMA001" });
            }

            [TestMethod]
            public async Task LookbackDatePassedIn_LaterThanEndDate_GetSupervisorsBySuperviseeAsync()
            {
                lookbackDate = new DateTime(2020, 01, 02);
                var expected = await testRepository.GetSupervisorsBySuperviseeAsync(superviseeId, lookbackDate);
                var actual = await actualRepository.GetSupervisorsBySuperviseeAsync(superviseeId, lookbackDate);
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
            }

            [TestMethod]
            public async Task LookbackDatePassedIn_EarlierThanEndDate_GetSupervisorsBySuperviseeAsync()
            {
                lookbackDate = new DateTime(1999, 01, 01);
                var expected = await testRepository.GetSupervisorsBySuperviseeAsync(superviseeId, lookbackDate);
                var actual = await actualRepository.GetSupervisorsBySuperviseeAsync(superviseeId, lookbackDate);
                CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
            }
        }

        [TestClass]
        public class GetSupervisorsBySuperviseeAsyncTests : SupervisorsRepositoryTests
        {
            [TestInitialize]
            public void Initialize()
            {
                SupervisorsRepositoryTestsInitialize();
            }

            [TestMethod]
            public async Task ExecuteTest()
            {
                var result = await actualRepository.GetSupervisorsBySuperviseeAsync("ANDRE3000");
            }
        }



    }
}
