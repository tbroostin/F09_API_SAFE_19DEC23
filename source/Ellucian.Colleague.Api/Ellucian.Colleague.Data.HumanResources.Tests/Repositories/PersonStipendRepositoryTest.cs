/* Copyright 2019 Ellucian Company L.P. and its affiliates. */
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
    public class PersonStipendRepositoryTests : BaseRepositorySetup
    {
        public TestPersonStipendRepository testDataRepository;

        public PersonStipendRepository repositoryUnderTest;

        public void PersonStipendRepositoryTestsInitialize()
        {
            MockInitialize();
            testDataRepository = new TestPersonStipendRepository();
            repositoryUnderTest = BuildRepository();
        }

        private PersonStipendRepository BuildRepository()
        {
            dataReaderMock.Setup(d => d.SelectAsync("PERPOSWG", "WITH PPWG.HRP.ID EQ ?", It.IsAny<string[]>(), "?", true, 425))
            .Returns<string, string, string[], string, bool, int>((f, c, personIds, p, b, s) =>
                Task.FromResult(testDataRepository.personStipendRecords == null ? null :
                    testDataRepository.personStipendRecords
                        .Where(r => personIds.Select(v => v.Replace("\"", "").Replace("\\", "")).Contains(r.personId))
                        .Select(r => r.id)
                        .ToArray()));


            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), true))
                .Returns<string[], bool>((ids, b) =>
                    Task.FromResult(testDataRepository.personStipendRecords == null ? null :
                        new Collection<Perposwg>(
                            testDataRepository.personStipendRecords
                            .Where(r => ids.Contains(r.id))
                            .Select(r => new Perposwg()
                            {
                                Recordkey = r.id,
                                PpwgHrpId = r.personId,
                                PpwgPositionId = r.positionId,
                                PpwgStartDate = r.startDate,
                                PpwgEndDate = r.endDate,
                                PpwgDesc = r.description,
                                PpwgBaseAmt = r.baseAmount,
                                PpwgPayrollDesignation = r.payrollDesignation,
                                PpwgNoPayments = r.numberOfPayments,
                                PpwgNoPaymentsTaken = r.paymentsTaken,
                                PpwgCourseSecAsgmt = r.courseSectionAssignments,
                                PpwgAdvisorAsgmt = r.advisorAssignments,
                                PpwgMembershipAsgmt = r.membershipAssignment
                            }).ToList())));    

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new PersonStipendRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
        }

        [TestClass]
        public class GetPersonStipendAsyncTests : PersonStipendRepositoryTests
        {
            public List<string> inputPersonIds;

            public async Task<List<PersonStipend>> getExpected()
            {
                return (await testDataRepository.GetPersonStipendAsync(inputPersonIds)).ToList();
            }

            public async Task<List<PersonStipend>> getActual()
            {
                return (await repositoryUnderTest.GetPersonStipendAsync(inputPersonIds)).ToList();
            }

            [TestInitialize]
            public void Initialize()
            {
                PersonStipendRepositoryTestsInitialize();
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
                    Assert.AreEqual(expected[i].Id, actual[i].Id);                 
                    Assert.AreEqual(expected[i].PersonId, actual[i].PersonId);
                    Assert.AreEqual(expected[i].PositionId, actual[i].PositionId);               
                    Assert.AreEqual(expected[i].StartDate, actual[i].StartDate);
                    Assert.AreEqual(expected[i].EndDate, actual[i].EndDate);
                    Assert.AreEqual(expected[i].Description, actual[i].Description);
                    Assert.AreEqual(expected[i].BaseAmount, actual[i].BaseAmount);
                    Assert.AreEqual(expected[i].PayrollDesignation, actual[i].PayrollDesignation);
                    Assert.AreEqual(expected[i].NumberOfPayments, actual[i].NumberOfPayments);
                    Assert.AreEqual(expected[i].PaymentsTaken, actual[i].PaymentsTaken);
                    CollectionAssert.AreEqual(expected[i].CourseSectionAssignments, actual[i].CourseSectionAssignments);
                    CollectionAssert.AreEqual(expected[i].AdvisorAssignments, actual[i].AdvisorAssignments);
                    CollectionAssert.AreEqual(expected[i].MembershipAssignments, actual[i].MembershipAssignments);
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
                testDataRepository.personStipendRecords = null;
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
            [ExpectedException(typeof(ApplicationException))]
            public async Task NoRecordsExistForGivenPersonIdsTest()
            {
                inputPersonIds = new List<string>() { "dummyId" };
                try
                {
                    var actual = await getActual();
                }
                catch(Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }              
             }

            [TestMethod]
            public async Task BulkRecordReadLimitIsUsedTest()
            {
                var expectedReadTimes = testDataRepository.personStipendRecords.Count();
                var actual = await getActual();

                dataReaderMock.Verify(r => r.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), true), Times.Once);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullReturnedByBulkRecordReadTest()
            {
                dataReaderMock.Setup(r => r.BulkReadRecordAsync<Perposwg>(It.IsAny<string[]>(), true))
                    .Returns(Task.FromResult<Collection<Perposwg>>(null));
                try
                {
                    var actual = await getActual();
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }          

            }

            [TestMethod]
            public async Task StartDateInRecordIsNullTest()
            {
                testDataRepository.personStipendRecords.ForEach(r => r.startDate = null);

                var actual = await getActual();
                Assert.IsFalse(actual.Any());

                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            }
        }
    }
}
