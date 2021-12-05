/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
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
    public class PersonEmploymentStatusRepositoryTests : BaseRepositorySetup
    {
        public TestPersonEmploymentStatusRepository testDataRepository;

        public PersonEmploymentStatusRepository repositoryUnderTest;

        public void PersonEmploymentStatusRepositoryTestsInitialize()
        {
            MockInitialize();
            testDataRepository = new TestPersonEmploymentStatusRepository();
            repositoryUnderTest = BuildRepository();
        }

        public PersonEmploymentStatusRepository BuildRepository()
        {

            dataReaderMock.Setup(d => d.SelectAsync("PERSTAT", "WITH PERSTAT.HRP.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                        Task.FromResult((testDataRepository.personEmploymentStatusRecords == null) ? null :
                            testDataRepository.personEmploymentStatusRecords
                            .Where(rec => values.Select(v => v.Replace("\"","").Replace("\\","")).Contains(rec.personId))
                            .Select(rec => rec.id).ToArray()
                        ));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testDataRepository.personEmploymentStatusRecords == null ? null :
                    new Collection<Perstat>(
                        testDataRepository.personEmploymentStatusRecords
                        .Where(rec => ids.Contains(rec.id))
                        .Select(rec => new Perstat()
                        {
                            Recordkey = rec.id,
                            PerstatHrpId = rec.personId,
                            PerstatPrimaryPosId = rec.primaryPositionId,
                            PerstatPrimaryPerposId = rec.personPositionId,
                            PerstatStartDate = rec.startDate,
                            PerstatEndDate = rec.endDate,
                        }).ToList()
                    )));

            apiSettings.BulkReadSize = 1;

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new PersonEmploymentStatusRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestClass]
        public class GetPersonEmploymentStatussAsyncTests : PersonEmploymentStatusRepositoryTests
        {
            public List<string> inputPersonIds;

            //public List<string> selectedPersonIds;

            public async Task<List<PersonEmploymentStatus>> getExpected()
            {
                return (await testDataRepository.GetPersonEmploymentStatusesAsync(inputPersonIds)).ToList();
            }

            public async Task<List<PersonEmploymentStatus>> getActual()
            {
                return (await repositoryUnderTest.GetPersonEmploymentStatusesAsync(inputPersonIds)).ToList();
            }

            [TestInitialize]
            public void Initialize()
            {
                PersonEmploymentStatusRepositoryTestsInitialize();
                inputPersonIds = testDataRepository.personIdsUsedInTestData.ToList();
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
                    Assert.AreEqual(expected[i].Id, actual[i].Id);
                    Assert.AreEqual(expected[i].PersonId, actual[i].PersonId);
                    Assert.AreEqual(expected[i].PersonPositionId, actual[i].PersonPositionId);
                    Assert.AreEqual(expected[i].PrimaryPositionId, actual[i].PrimaryPositionId);
                    Assert.AreEqual(expected[i].StartDate, actual[i].StartDate);
                    Assert.AreEqual(expected[i].EndDate, actual[i].EndDate);
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
            public async Task PersonIdsCannotBeEmptyTest()
            {
                inputPersonIds = new List<string>();
                await getActual();
            }

            [TestMethod]
            public async Task RecordsSelectedForEachPersonIdTest()
            {
                await getActual();
                dataReaderMock.
                    Verify(d => d.SelectAsync("PERSTAT", "WITH PERSTAT.HRP.ID EQ ?", It.Is<string[]>(a => a.All(id => inputPersonIds.Contains(id.Replace("\"", "").Replace("\\", "")))), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task MessageLoggedIfSelectReturnsNullTest()
            {
                testDataRepository.personEmploymentStatusRecords = null;
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
            public async Task MessageLoggedIfSelectReturnsEmptyTest()
            {
                //add personId with no Perpos records. no records will be selected for this person
                inputPersonIds = new List<string>() { "foobar" };
                var actual = await getActual();
                Assert.IsFalse(actual.Any(pps => pps.PersonId == "foobar"));
                loggerMock.Verify(l => l.Info(It.IsAny<string>()), Times.Once);
            }

            [TestMethod]
            public async Task NullPerstatRecordsFromReadLoggedTest()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(() => null);
                await getActual();
                // loggerMock.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
                // this error will be logged for each perstat key, which may be multiple -- it is unreasonable to expect it to be logged once
                loggerMock.Verify(l => l.Error(It.IsAny<string>()), Times.AtLeastOnce);
            }

            //[TestMethod]
            //public async Task BadDataInDBRecordCreatesErrorInMapping_DataErrorIsLoggged_AndRecordSkipped_Test()
            //{
            //    testDataRepository.
            //}

            [TestMethod]
            public async Task BulkReadLimitIsUsedTest()
            {
                Assert.AreEqual(1, apiSettings.BulkReadSize);
                await getActual();
                dataReaderMock.Verify(d => d.BulkReadRecordAsync<Perstat>(It.IsAny<string[]>(), true), Times.Exactly(testDataRepository.personEmploymentStatusRecords.Count));
            }
        }
    }
}
