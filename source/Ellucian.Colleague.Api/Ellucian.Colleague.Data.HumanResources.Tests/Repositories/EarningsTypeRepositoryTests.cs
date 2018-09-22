/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Colleague.Data.Base.Tests.Repositories;
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
    public class EarningsTypeRepositoryTests : BaseRepositorySetup
    {

        public TestEarningsTypeRepository testDataRepository;

        public EarningsTypeRepository repositoryUnderTest;

        public void EarningsTypeRepositoryTestsInitialize()
        {
            MockInitialize();
            testDataRepository = new TestEarningsTypeRepository();

            repositoryUnderTest = BuildRepository();
        }

        public EarningsTypeRepository BuildRepository()
        {
            dataReaderMock.Setup(d => d.SelectAsync("EARNTYPE", ""))
                .Returns<string, string>((f, c) => Task.FromResult(testDataRepository.earningsTypeRecords == null ? null :
                    testDataRepository.earningsTypeRecords.Select(et => et.id).ToArray()));

            dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Earntype>(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns<string, bool>((x, b) =>
                Task.FromResult(testDataRepository.earningsTypeRecords == null ? null :
                    new Collection<DataContracts.Earntype>(testDataRepository.earningsTypeRecords
                        .Select(record =>
                        (record == null) ? null : new DataContracts.Earntype()
                        {
                            Recordkey = record.id,
                            EtpDesc = record.description,
                            EtpActiveFlag = record.activeFlag,
                            EtpCategory = record.category,
                            EtpEarningMethod = record.method
                        }).ToList())
                ));

            loggerMock.Setup(l => l.IsErrorEnabled).Returns(true);

            return new EarningsTypeRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

        }

        [TestClass]
        public class GetEarningsTypesAsyncTests : EarningsTypeRepositoryTests
        {
            public async Task<IEnumerable<EarningsType>> getExpectedEarningsTypes()
            {
                return await testDataRepository.GetEarningsTypesAsync();
            }

            public async Task<IEnumerable<EarningsType>> getActualEarningsTypes()
            {
                return await repositoryUnderTest.GetEarningsTypesAsync();
            }

            [TestInitialize]
            public void Initialize()
            {
                EarningsTypeRepositoryTestsInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActualTest()
            {
                CollectionAssert.AreEqual((await getExpectedEarningsTypes()).ToList(), (await getActualEarningsTypes()).ToList());
            }

            [TestMethod]
            public async Task AttributesTest()
            {
                var expected = (await getExpectedEarningsTypes()).ToArray();
                var actual = (await getActualEarningsTypes()).ToArray();
                for (int i= 0; i < expected.Count(); i++)
                {
                    Assert.AreEqual(expected[i].Description, actual[i].Description);
                    Assert.AreEqual(expected[i].IsActive, actual[i].IsActive);
                    Assert.AreEqual(expected[i].Category, actual[i].Category);
                    Assert.AreEqual(expected[i].Method, actual[i].Method);
                }
            }

            [TestMethod]
            public async Task AssignNoneForEmptyEarningsMethodTest()
            {
                testDataRepository.earningsTypeRecords.ForEach(et => et.method = "");
                var actual = (await getActualEarningsTypes()).ToArray();
                Assert.AreEqual(actual[0].Method, EarningsMethod.None);
            }

            [TestMethod]
            public async Task AssignNoneForNullEarningsMethodTest()
            {
                testDataRepository.earningsTypeRecords.ForEach(et => et.method = null);
                var actual = (await getActualEarningsTypes()).ToArray();
                Assert.AreEqual(actual[0].Method, EarningsMethod.None);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task NullEarningsTypeRecordsFromDataReaderTest()
            {
                dataReaderMock.Setup(d => d.BulkReadRecordAsync<DataContracts.Earntype>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns(Task.FromResult<Collection<DataContracts.Earntype>>(null));

                try
                { 
                    await getActualEarningsTypes();
                }
                catch (Exception)
                {
                    loggerMock.Verify(l => l.Error(It.IsAny<string>()));
                    throw;
                }
            }

            [TestMethod]
            public async Task LogDataErrorIfEarningsTypeDescriptionIsNullTest()
            {
                testDataRepository.earningsTypeRecords.ForEach(et => et.description = null);
                Assert.IsFalse((await getActualEarningsTypes()).Any());
                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task LogDataErrorIfEarningsTypeCategoryIsNullTest()
            {
                testDataRepository.earningsTypeRecords.ForEach(et => et.category = null);
                Assert.IsFalse((await getActualEarningsTypes()).Any());
                loggerMock.Verify(l => l.Error(It.IsAny<ArgumentException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task LogDataErrorIfEarningsTypeCategoryIsInvalidTest()
            {
                testDataRepository.earningsTypeRecords.ForEach(et => et.category = "foo");
                Assert.IsFalse((await getActualEarningsTypes()).Any());
                loggerMock.Verify(l => l.Error(It.IsAny<ApplicationException>(), It.IsAny<string>()));
            }

            [TestMethod]
            public async Task LogDataErrorIfEarningsTypeMethodIsInvalidTest()
            {
                testDataRepository.earningsTypeRecords.ForEach(et => et.method = "foo");
                Assert.IsFalse((await getActualEarningsTypes()).Any());
                loggerMock.Verify(l => l.Error(It.IsAny<ApplicationException>(), It.IsAny<string>()));
            }

        }

    }
}
