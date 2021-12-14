/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.HumanResources.DataContracts;
using Ellucian.Colleague.Data.HumanResources.Repositories;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Tests;
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
    public class PersonPositionWageRepositoryTests : BaseRepositorySetup
    {
        public TestPersonPositionWageRepository testDataRepository;

        public PersonPositionWageRepository repositoryUnderTest;

        public void PersonPositionWageRepositoryTestsInitialize()
        {
            MockInitialize();
            testDataRepository = new TestPersonPositionWageRepository();
            repositoryUnderTest = BuildRepository();
        }

        public PersonPositionWageRepository BuildRepository()
        {
            dataReaderMock.Setup(d => d.SelectAsync("PERPOSWG", "WITH PPWG.HRP.ID EQ ?", It.IsAny<string[]>(), "?", true, 425))
                .Returns<string, string, string[], string, bool, int>((f, c, personIds, p, b, s) =>
                    Task.FromResult(testDataRepository.personPositionWageRecords == null ? null :
                        testDataRepository.personPositionWageRecords
                            .Where(r => personIds.Select(v => v.Replace("\"", "").Replace("\\", "")).Contains(r.personId))
                            .Select(r => r.id)
                            .ToArray()));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) =>
                    Task.FromResult(testDataRepository.personPositionWageRecords == null ? null :
                        new Collection<Perposwg>(
                            testDataRepository.personPositionWageRecords
                            .Where(r => ids.Contains(r.id))
                            .Select(r => new Perposwg()
                            {
                                Recordkey = r.id,
                                PpwgBaseEt = r.regularWorkEarningsType,
                                PpwgEndDate = r.endDate,
                                PpwgStartDate = r.startDate,
                                PpwgSuspendPayFlag = r.paySuspendedFlag,
                                PpwgPaycycleId = r.payCycleId,
                                PpwgPayclassId = r.payClassId,
                                PpwgPospayId = r.PpwgPospayId,
                                PpwgPerposId = r.personPositionId,
                                PpwgPositionId = r.positionId,
                                PpwgHrpId = r.personId,
                                PpwgType = r.wageType,
                                PpwitemsEntityAssociation = r.payItems.Select(p => new PerposwgPpwitems()
                                {
                                    PpwgPiFndsrcIdAssocMember = p.fundSourceId,
                                    PpwgProjectsIdsAssocMember = p.projectId
                                }).ToList()
                            }).ToList())));
            MockRecordsAsync<Pospay>("POSPAY", testDataRepository.positionPayRecords);
            //MockRecordsAsync<EarntypeGroupings>("EARNTYPE.GROUPINGS", testDataRepository.earnTypeGroupingsRecords);
            apiSettings.BulkReadSize = 1;
            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new PersonPositionWageRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestClass]
        public class GetPersonPositionWagesAsyncTests : PersonPositionWageRepositoryTests
        {
            public List<string> inputPersonIds;
            public List<string> payCycleIds;

            public async Task<List<PersonPositionWage>> getExpected()
            {
                return (await testDataRepository.GetPersonPositionWagesAsync(inputPersonIds, null, payCycleIds)).ToList();
            }

            public async Task<List<PersonPositionWage>> getActual()
            {
                return (await repositoryUnderTest.GetPersonPositionWagesAsync(inputPersonIds, null, payCycleIds)).ToList();
            }

            [TestInitialize]
            public void Initialize()
            {
                PersonPositionWageRepositoryTestsInitialize();
                inputPersonIds = testDataRepository.PersonIdsUsedInTestData.ToList();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual(await getExpected(), await getActual());
            }

            [TestMethod]
            public async Task AttributesTest()
            {
                var expected = await getExpected();
                var actual = await getActual();

                for (int i = 0; i < expected.Count; i++)
                {
                    Assert.AreEqual(expected[i].EndDate, actual[i].EndDate);
                    Assert.AreEqual(expected[i].Id, actual[i].Id);
                    Assert.AreEqual(expected[i].IsPaySuspended, actual[i].IsPaySuspended);
                    Assert.AreEqual(expected[i].PayClassId, actual[i].PayClassId);
                    Assert.AreEqual(expected[i].PayCycleId, actual[i].PayCycleId);
                    Assert.AreEqual(expected[i].PersonId, actual[i].PersonId);
                    Assert.AreEqual(expected[i].PersonPositionId, actual[i].PersonPositionId);
                    Assert.AreEqual(expected[i].PositionId, actual[i].PositionId);
                    Assert.AreEqual(expected[i].PositionPayDefaultId, actual[i].PositionPayDefaultId);
                    Assert.AreEqual(expected[i].RegularWorkEarningsTypeId, actual[i].RegularWorkEarningsTypeId);
                    Assert.AreEqual(expected[i].StartDate, actual[i].StartDate);
                    CollectionAssert.AreEqual(expected[i].FundingSources, actual[i].FundingSources);
                    for (int j = 0; j < expected[i].FundingSources.Count; j++)
                    {
                        Assert.AreEqual(expected[i].FundingSources[j].FundingOrder, actual[i].FundingSources[j].FundingOrder);
                        Assert.AreEqual(expected[i].FundingSources[j].FundingSourceId, actual[i].FundingSources[j].FundingSourceId);
                        Assert.AreEqual(expected[i].FundingSources[j].ProjectId, actual[i].FundingSources[j].ProjectId);
                    }
                    Assert.AreEqual(expected[i].EarningsTypeGroupId, actual[i].EarningsTypeGroupId);
                    
                }
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonIdsRequiredTest()
            {
                inputPersonIds = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public async Task PersonIdsMustHaveValueTest()
            {
                inputPersonIds = new List<string>();
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullReturnedBySelectAsyncTest()
            {
                testDataRepository.personPositionWageRecords = null;
                try
                {
                    await getActual();
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            public async Task NoRecordsExistForGivenPersonIdsTest()
            {
                inputPersonIds = new List<string>() { "foobar" };
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
                loggerMock.Verify(l => l.Info(It.Is<string>(s => s.Contains("foobar"))));
            }

            [TestMethod]
            public async Task BulkRecordReadLimitIsUsedTest()
            {
                Assert.AreEqual(1, apiSettings.BulkReadSize);
                var expectedReadTimes = testDataRepository.personPositionWageRecords.Count();
                var actual = await getActual();

                dataReaderMock.Verify(r => r.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), true), Times.Exactly(expectedReadTimes));
            }

            [TestMethod]
            public async Task NullReturnedByBulkRecordReadTest()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), true))
                    .Returns(Task.FromResult<Collection<Perposwg>>(null));

                var actual = await getActual();
                Assert.IsFalse(actual.Any());
                loggerMock.Verify(l => l.Error(It.IsAny<string>()));
            }

            [TestMethod]
            public async Task StartDateInRecordIsNullTest()
            {
                testDataRepository.personPositionWageRecords.ForEach(r => r.startDate = null);

                var actual = await getActual();
                Assert.IsFalse(actual.Any());

                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task SuspendPayFlagIsNull_ResultIsFalseTest()
            {
                testDataRepository.personPositionWageRecords.ForEach(r => r.paySuspendedFlag = null);

                var actual = await getActual();

                Assert.IsTrue(actual.All(ppw => !ppw.IsPaySuspended));
            }


            [TestMethod]
            public async Task SuspendPayFlagIsNotYes_ResultIsFalseTest()
            {
                testDataRepository.personPositionWageRecords.ForEach(r => r.paySuspendedFlag = "FOO");

                var actual = await getActual();

                Assert.IsTrue(actual.All(ppw => !ppw.IsPaySuspended));
            }

            [TestMethod]
            public async Task SuspendPayFlagIsYes_ResultIsTrueTest()
            {
                testDataRepository.personPositionWageRecords.ForEach(r => r.paySuspendedFlag = "y");

                var actual = await getActual();

                Assert.IsTrue(actual.All(ppw => ppw.IsPaySuspended));
            }

            [TestMethod]
            public async Task VerifyActualWithNullEarnTypeGroupId()
            {
                testDataRepository.positionPayRecords.ForEach(rec => rec.PospayEarntypeGrouping = null);

                var actual = await getActual();

                Assert.AreEqual(6, actual.Count);
                Assert.AreEqual(null, actual.ElementAt(0).EarningsTypeGroupId);
            }

            [TestMethod]
            public async Task PassInPayCycleId_ReturnsExpectedResultTest()
            {
                payCycleIds = new List<string>() { "MW" };
                var expected = await getExpected();
                var actual = await getActual();
                CollectionAssert.AreEqual(expected, actual);
                Assert.IsTrue(actual.All(ppw => payCycleIds.Contains(ppw.PayCycleId)));
            }

            [TestMethod]
            public async Task PassInPayCycleIds_ReturnsExpectedResultTest()
            {
                payCycleIds = new List<string>() { "MW", "BM" };
                var expected = await getExpected();
                var actual = await getActual();
                CollectionAssert.AreEqual(expected, actual);
                Assert.IsTrue(actual.All(ppw => payCycleIds.Contains(ppw.PayCycleId)));
            }

            [TestMethod]
            public async Task PassInInvalidPayCycleIds_ReturnsExpectedResultTest()
            {
                payCycleIds = new List<string>() { "MW1", "BM2" };
                var expected = await getExpected();
                var actual = await getActual();
                CollectionAssert.AreEqual(expected, actual);
                Assert.IsFalse(actual.Any());
            }
        }
    }
}
