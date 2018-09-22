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
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.HumanResources.Tests.Repositories
{
    [TestClass]
    public class PersonBenefitDeductionRepositoryTests : BaseRepositorySetup
    {

        public PersonBenefitDeductionRepository repositoryUnderTest;

        public TestPersonBenefitDeductionRepository testData;

        public void InitializePersonBenefitDeductionRepositoryTests()
        {
            MockInitialize();

            testData = new TestPersonBenefitDeductionRepository();

            dataReaderMock.Setup(d => d.ReadRecordAsync<Hrper>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((id, b) => Task.FromResult(testData.personBenefitDeductionSummaryRecords == null ? null : new Hrper()
                {
                    Recordkey = id,
                    AllBenefits = testData.personBenefitDeductionSummaryRecords.Select(rec => rec.benefitDeductionId).ToList(),
                    PerbenInfoEntityAssociation = testData.personBenefitDeductionSummaryRecords.Select(rec =>
                        new HrperPerbenInfo()
                        {
                            HrpPerbenBdIdAssocMember = rec.benefitDeductionId,
                            HrpPerbenCancelDateAssocMember = rec.cancelDate,
                            HrpPerbenEnrollDateAssocMember = rec.enrollmentDate,
                            HrpPerbenLastPayDateAssocMember = rec.lastPayDate
                        }).ToList()
                }));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<Hrper>(It.IsAny<string[]>(), It.IsAny<bool>()))
                .Returns<string[], bool>((ids, b) => Task.FromResult(testData.personBenefitDeductionSummaryRecords == null ? null :
                    new Collection<Hrper>(ids.Select(id => new Hrper()
                    {
                        Recordkey = id,
                        AllBenefits = testData.personBenefitDeductionSummaryRecords.Select(rec => rec.benefitDeductionId).ToList(),
                        PerbenInfoEntityAssociation = testData.personBenefitDeductionSummaryRecords.Select(rec =>
                            new HrperPerbenInfo()
                            {
                                HrpPerbenBdIdAssocMember = rec.benefitDeductionId,
                                HrpPerbenCancelDateAssocMember = rec.cancelDate,
                                HrpPerbenEnrollDateAssocMember = rec.enrollmentDate,
                                HrpPerbenLastPayDateAssocMember = rec.lastPayDate
                            }).ToList()
                    }).ToList())));


            repositoryUnderTest = new PersonBenefitDeductionRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            
        }

        [TestClass]
        public class GetPersonBenefitDeductionForMultipleIdsTests : PersonBenefitDeductionRepositoryTests
        {
            public List<string> inputPersonIds;

            public bool simulateCachedRecords;

            public async Task<IEnumerable<PersonBenefitDeduction>> getActual()
            {
                return await repositoryUnderTest.GetPersonBenefitDeductionsAsync(inputPersonIds);
            }

            [TestInitialize]
            public void Initialize()
            {
                InitializePersonBenefitDeductionRepositoryTests();

                inputPersonIds = new List<string>() { "0003914", "0004451" };

                //initialize the cacheProvider to simulate that nothing is cached.
                simulateCachedRecords = false;

                cacheProviderMock.Setup(c => c.Contains(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>((key, region) => 
                        simulateCachedRecords);

                cacheProviderMock.Setup(c => c.Get(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns<string, string>((key, region) =>
                    {
                        var personId = key.Substring(0, key.IndexOf('-')); //extract personId from beginning of cache key
                        return testData.GetPersonBenefitDeductions(personId);
                    });                
            }

            [TestMethod]
            public async Task BenefitDeductionsForMultipleIdsTest()
            {
                var actual = await getActual();

                Assert.IsTrue(actual.Any(a => a.PersonId == inputPersonIds[0]));
                Assert.IsTrue(actual.Any(a => a.PersonId == inputPersonIds[1]));
            }

            [TestMethod]
            public async Task BenefitDeductionsForUniquePersonIdsTest()
            {
                var expected = await getActual(); //get actual with original input person ids

                //add duplicate ids
                inputPersonIds.Add(inputPersonIds[0]);
                inputPersonIds.Add(inputPersonIds[1]);

                var actual = await getActual(); //get actual with duplicate input ids

                //assert the number of objects returned are the same
                Assert.AreEqual(expected.Count(), actual.Count());
            }

            [TestMethod]
            public async Task RecordsRetreivedFromDatabaseTest()
            {
                simulateCachedRecords = false;
                var actual = await getActual();

                cacheProviderMock.Verify(c => c.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [TestMethod]
            public async Task RecordsFromDatabaseAddedToCacheTest()
            {
                simulateCachedRecords = false;
                var actual = await getActual();

                cacheProviderMock.Verify(c => c.Add(It.IsAny<string>(), It.IsAny<IEnumerable<PersonBenefitDeduction>>(), It.IsAny<CacheItemPolicy>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task RecordsRetrievedFromCacheTest()
            {
                simulateCachedRecords = true;
                var actual = await getActual();

                dataReaderMock.Verify(d => d.BulkReadRecordAsync<Hrper>(It.IsAny<string[]>(), It.IsAny<bool>()), Times.Never);
            }

            [TestMethod]
            public async Task NullRecordsInCacheTest()
            {
                simulateCachedRecords = true;
                testData.personBenefitDeductionSummaryRecords = null;

                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            public async Task NullRecordsInDatabase()
            {
                simulateCachedRecords = false;
                testData.personBenefitDeductionSummaryRecords = null;

                var actual = await getActual();
                Assert.IsFalse(actual.Any());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonIdsRequiredTest()
            {
                inputPersonIds = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonIdsEmptyTest()
            {
                inputPersonIds = new List<string>();
                await getActual();
            }


        }

        [TestClass]
        public class GetPersonBenefitDeductionForSinglePersonIdTests : PersonBenefitDeductionRepositoryTests
        {
            public string inputPersonId;

            public async Task<IEnumerable<PersonBenefitDeduction>> getActual()
            {
                return await repositoryUnderTest.GetPersonBenefitDeductionsAsync(inputPersonId);
            }

            [TestInitialize]
            public void Initialize()
            {
                InitializePersonBenefitDeductionRepositoryTests();
                inputPersonId = "0003914";
            }

            [TestMethod]
            public async Task NoErrorTest()
            {
                var expected = await testData.GetPersonBenefitDeductionsAsync(inputPersonId);
                var actual = await getActual();

                Assert.IsTrue(actual.Any());
                Assert.AreEqual(expected.Count(), actual.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PersonIdRequiredTest()
            {
                inputPersonId = null;
                await getActual();
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public async Task HrperRecordNotFoundTest()
            {
                testData.personBenefitDeductionSummaryRecords = null;
                await getActual();
            }

            [TestMethod]
            public async Task InvalidRecordDataTest()
            {
                testData.personBenefitDeductionSummaryRecords.ForEach(pbd => pbd.enrollmentDate = null);

                Assert.IsFalse((await getActual()).Any());
            }
        }
    }
}
