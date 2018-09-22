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
    public class PersonPositionRepositoryTests : BaseRepositorySetup
    {
        public TestPersonPositionRepository testDataRepository;

        public PersonPositionRepository repositoryUnderTest;

        public void PersonPositionRepositoryTestsInitialize()
        {
            MockInitialize();
            testDataRepository = new TestPersonPositionRepository();
            repositoryUnderTest = BuildRepository();
        }

        public PersonPositionRepository BuildRepository()
        {
            //dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>()))
            //    .Returns<string, string>((f, critiera) =>
            //    {
            //        if (testDataRepository.personPositionRecords == null) return Task.FromResult<string[]>(null);
            //        var personId = critiera.Substring(22).Trim(); //length of criteria before personId;
            //        var records = testDataRepository.personPositionRecords.Where(rec => rec.personId == personId);
            //        return Task.FromResult(records.Select(rec => rec.id).ToArray());
            //    });

            dataReaderMock.Setup(d => d.SelectAsync("PERPOS", "WITH PERPOS.HRP.ID EQ ?", It.IsAny<string[]>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()))
                .Returns<string, string, string[], string, bool, int>((f, c, values, p, r, s) =>
                        Task.FromResult((testDataRepository.personPositionRecords == null) ? null :
                            testDataRepository.personPositionRecords
                            .Where(rec => values.Select(v => v.Replace("\"", "").Replace("\\", "")).Contains(rec.personId))
                            .Select(rec => rec.id).ToArray()
                        ));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perpos>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testDataRepository.personPositionRecords == null ? null :
                    new Collection<Perpos>(
                        testDataRepository.personPositionRecords
                        .Where(rec => ids.Contains(rec.id))
                        .Select(rec => new Perpos()
                        {
                            Recordkey = rec.id,
                            PerposAltSupervisorId = rec.alternateSupervisorId,
                            PerposSupervisorHrpId = rec.supervisorId,
                            PerposStartDate = rec.startDate,
                            PerposPositionId = rec.positionId,
                            PerposHrpId = rec.personId,
                            PerposEndDate = rec.endDate
                        }).ToList()
                    )));

            apiSettings.BulkReadSize = 1;

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new PersonPositionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestClass]
        public class GetPersonPositionsAsyncTests : PersonPositionRepositoryTests
        {
            public List<string> inputPersonIds;

            //public List<string> selectedPersonIds;

            public async Task<List<PersonPosition>> getExpected()
            {
                return (await testDataRepository.GetPersonPositionsAsync(inputPersonIds)).ToList();
            }

            public async Task<List<PersonPosition>> getActual()
            {
                return (await repositoryUnderTest.GetPersonPositionsAsync(inputPersonIds)).ToList();
            }

            [TestInitialize]
            public void Initialize()
            {
                PersonPositionRepositoryTestsInitialize();
                inputPersonIds = testDataRepository.personIdsUsedInTestData.ToList();

                //selectedPersonIds = new List<string>();
                //dataReaderMock.Setup(d => d.SelectAsync(It.IsAny<string>(), It.IsAny<string>()))
                //    .Callback<string, string>((f, criteria) =>
                //    {
                //        if (testDataRepository.personPositionRecords != null)
                //            selectedPersonIds.Add(criteria.Substring(22).Trim()); //length of criteria before personId;
                //    });
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
                    Assert.AreEqual(expected[i].AlternateSupervisorId, actual[i].AlternateSupervisorId);
                    Assert.AreEqual(expected[i].EndDate, actual[i].EndDate);
                    Assert.AreEqual(expected[i].PersonId, actual[i].PersonId);
                    Assert.AreEqual(expected[i].PositionId, actual[i].PositionId);
                    Assert.AreEqual(expected[i].StartDate, actual[i].StartDate);
                    Assert.AreEqual(expected[i].SupervisorId, actual[i].SupervisorId);
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
                    Verify(d => d.SelectAsync("PERPOS", "WITH PERPOS.HRP.ID EQ ?", It.Is<string[]>(a => a.All(id => inputPersonIds.Contains(id.Replace("\"", "").Replace("\\", "")))), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<int>()));
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task MessageLoggedIfSelectReturnsNullTest()
            {
                testDataRepository.personPositionRecords = null;
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
                loggerMock.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);
            }

            [TestMethod]
            public async Task BulkReadLimitIsUsedTest()
            {
                Assert.AreEqual(1, apiSettings.BulkReadSize);
                await getActual();
                dataReaderMock.Verify(d => d.BulkReadRecordAsync<Perpos>(It.IsAny<string[]>(), true), Times.Exactly(testDataRepository.personPositionRecords.Count));
            }

            [TestMethod]
            public async Task PerposRecordMustHaveStartDateTest()
            {
                testDataRepository.personPositionRecords.ForEach(rec => rec.startDate = null);
                var actual = await getActual();
                Assert.IsFalse(actual.Any());
                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            }
        }
    }
}
