/* Copyright 2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
using Ellucian.Data.Colleague.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PayCycleRepositoryTests : BaseRepositorySetup
    {
        public TestPayCycleRepository testDataRepository;

        public PayCycleRepository repositoryUnderTest;

        public string fullCacheKey;

        public void PayCycleRepositoryTestsInitialize()
        {
            MockInitialize();
            testDataRepository = new TestPayCycleRepository();
            repositoryUnderTest = BuildRepository();
            var cacheKey = "PayCycleFrequency";
            fullCacheKey = repositoryUnderTest.BuildFullCacheKey(cacheKey);
        }

        public PayCycleRepository BuildRepository()
        {
            dataReaderMock.Setup(d => d.SelectAsync("PAYCYCLE", ""))
             .Returns<string, string>((f, c) => Task.FromResult(testDataRepository.payCycleRecords == null ? null :
                 testDataRepository.payCycleRecords.Select(pc => pc.id).ToArray()));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Paycycle>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((x, b) =>
                Task.FromResult(testDataRepository.payCycleRecords == null ? null :
                    new Collection<DataContracts.Paycycle>(testDataRepository.payCycleRecords
                        .Select(record =>
                        (record == null) ? null : new DataContracts.Paycycle()
                        {
                            Recordkey = record.id,
                            PcyDesc = record.description,
                            PcyPayclasses = record.payClassIds,
                            PcyFrequency = record.payFrequency,
                            PcyWorkWeekStartDy = record.workWeekStartDay,
                            PcyBendedPeriods = record.benDedPeriods,
                            PcyEndDate = record.endDate,
                            PcyExcludeEarnTypes = record.excludeEarnTypes,
                            PcyPaycheckDate = record.paycheckDate,
                            PcyPeriodStatus = record.periodStatus,
                            PcyStartDate = record.startDate,
                            PcyTakeBenefits = record.takeBenefits,
                            PayperiodsEntityAssociation = record.payPeriods.Select(dateRange => new PaycyclePayperiods()
                            {
                                PcyStartDateAssocMember = dateRange.startDate,
                                PcyEndDateAssocMember = dateRange.endDate
                            }).ToList()
                        }).ToList())));

            // pay control mock
            dataReaderMock.Setup(d => d.SelectAsync("PAYCNTRL", ""))
                .Returns<string, string>((f, c) => Task.FromResult(testDataRepository.payControlRecords == null ? null :
                    testDataRepository.payControlRecords.Select(pc => pc.Recordkey).ToArray()));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Paycntrl>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((x, b) =>
                Task.FromResult(testDataRepository.payControlRecords == null ? null :
                    new Collection<DataContracts.Paycntrl>(testDataRepository.payControlRecords
                        .Select(record =>
                        (record == null) ? null : new DataContracts.Paycntrl()
                        {
                            Recordkey = record.Recordkey,
                            PclCurrProgram = record.CurrentProgram,
                            PclLastProgram = record.LastProgram,
                            PclPeriodStartDate = record.PeriodStartDate,
                            PclTimeHistoryUpdated = record.TimeHistoryUpdated,
                            PclEmployeeCutoffDate = record.PeriodStartDate,
                            PclEmployeeCutoffTime = record.PeriodStartDate,
                            PclSupervisorCutoffDate = record.PeriodStartDate,
                            PclSupervisorCutoffTime = record.PeriodStartDate

                        }).ToList())));


            dataReaderMock.Setup(acc => acc.ReadRecordAsync<ApplValcodes>(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(Task.FromResult<ApplValcodes>(testDataRepository.validationCodeRecords));

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new PayCycleRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestClass]
        public class GetPayCyclesAsyncTests : PayCycleRepositoryTests
        {
            public async Task<IEnumerable<PayCycle>> getExpectedPayCycles()
            {
                return await testDataRepository.GetPayCyclesAsync();
            }

            public async Task<IEnumerable<PayCycle>> getActualPayCycles()
            {
                return await repositoryUnderTest.GetPayCyclesAsync();
            }

            public async Task<IEnumerable<PayCycle>> getExpectedPayCyclesWithLookback()
            {
                return await testDataRepository.GetPayCyclesAsync(new DateTime(2016, 1, 1));
            }

            public async Task<IEnumerable<PayCycle>> getActualPayCyclesWithLookback()
            {
                return await repositoryUnderTest.GetPayCyclesAsync(new DateTime(2016, 1, 1));
            }

            [TestInitialize]
            public void Initialize()
            {
                PayCycleRepositoryTestsInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                var expected = (await getExpectedPayCycles()).ToList();
                var actual = (await getActualPayCycles()).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest_Lookback()
            {
                var expected = (await getExpectedPayCyclesWithLookback()).ToList();
                var actual = (await getActualPayCyclesWithLookback()).ToList();
                CollectionAssert.AreEqual(expected, actual);
            }

            [TestMethod]
            public async Task ValidationCodesCachedTest()
            {
                cacheProviderMock.Setup(x => x.AddAndUnlockSemaphore(fullCacheKey, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();
                await getActualPayCycles();
                cacheProviderMock.Verify(m => m.AddAndUnlockSemaphore(fullCacheKey, It.IsAny<Object>(), It.IsAny<SemaphoreSlim>(), It.IsAny<CacheItemPolicy>(), null));
            }

            [TestMethod]
            public async Task AttributesTest()
            {
                var expected = (await getExpectedPayCycles()).ToArray();
                var actual = (await getActualPayCycles()).ToArray();
                for (int i = 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected[i].Description, actual[i].Description);
                    Assert.AreEqual(expected[i].AnnualPayFrequency, actual[i].AnnualPayFrequency);
                    CollectionAssert.AreEqual(expected[i].PayClassIds, actual[i].PayClassIds);
                    CollectionAssert.AreEqual(expected[i].PayPeriods, actual[i].PayPeriods);
                    for (int j = 0; j < expected[i].PayPeriods.Count; j++)
                    {
                        Assert.AreEqual(expected[i].PayPeriods[j].StartDate, actual[i].PayPeriods[j].StartDate);
                        Assert.AreEqual(expected[i].PayPeriods[j].EndDate, actual[i].PayPeriods[j].EndDate);
                    }
                }
            }

            [TestMethod]
            public async Task NullReturnedByBulkRecordReadTest()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Paycycle>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(Task.FromResult<Collection<Paycycle>>(null));

                var actual = await getActualPayCycles();
                Assert.IsFalse(actual.Any());
            }


            [TestMethod]
            [Ignore]
            public async Task PayPeriodDataErrorsAreHandledTest()
            {
                testDataRepository.payCycleRecords.ForEach(pc => pc.payPeriods.ForEach(pp => pp.endDate = null));
                var actual = await getActualPayCycles();
                Assert.IsTrue(actual.All(pc => !pc.PayPeriods.Any()));
                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task PayCycleRecordIsNullTest()
            {
                var nullId = testDataRepository.payCycleRecords[0].id;
                testDataRepository.payCycleRecords[0] = null;
                var actual = await getActualPayCycles();
                Assert.IsFalse(actual.Any(pc => pc.Id == nullId));
            }

            [TestMethod]
            public async Task PayCycleDataErrorsAreHandledTest()
            {
                testDataRepository.payCycleRecords.ForEach(pc => pc.description = null);
                Assert.IsFalse((await getActualPayCycles()).Any());
            }
        }
    }
}
