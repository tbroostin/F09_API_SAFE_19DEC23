//Copyright 2015-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.FinancialAid.DataContracts;
using Ellucian.Colleague.Data.FinancialAid.Repositories;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Colleague.Data.FinancialAid.Tests.Repositories
{
    [TestClass]
    public class RuleTableRepositoryTests : BaseRepositorySetup
    {

        public IEnumerable<string> inputAwardYears;

        public TestRuleTableRepository expectedRepository;
        public RuleTableRepository actualRepository;

        public void RuleTableRepositoryTestsInitialize()
        {
            MockInitialize();

            expectedRepository = new TestRuleTableRepository();
            actualRepository = BuildRepository();

            inputAwardYears = expectedRepository.ruleTableData.Select(rt => rt.awardYear);
        }

        private RuleTableRepository BuildRepository()
        {
            dataReaderMock.Setup(dr => dr.BulkReadRecordAsync<RuleTableAcyr>(It.IsAny<string>(), It.IsAny<string>(), true))
                .Returns<string, string, bool>((acyrFile, c, b) => Task.FromResult(
                    new Collection<RuleTableAcyr>(expectedRepository.ruleTableData
                        .Where(record => record.awardYear == acyrFile.Split('.')[2])
                        .Select(record =>
                            new RuleTableAcyr()
                            {
                                Recordkey = record.code,
                                RtFixLiteral = record.defaultResultString,
                                RtDescription = record.description,
                                RtDefaultValueCondition = record.defaultValueCondition,
                                RtType = record.type,
                                RtRuleTableEntityAssociation = record.ruleTableList.Select(ruleResult =>
                                    new RuleTableAcyrRtRuleTable()
                                    {
                                        RtRuleIdsAssocMember = ruleResult.ruleId,
                                        RtResultsAssocMember = ruleResult.result,
                                    }
                                ).ToList(),
                            }).ToList()
                        )));
            loggerMock.Setup(l => l.IsInfoEnabled).Returns(true);

            return new RuleTableRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object);
        }

        [TestClass]
        public class GetShoppingSheetRuleTableTests : RuleTableRepositoryTests
        {
            public List<ShoppingSheetRuleTable> expectedRuleTables
            { get { return expectedRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears).Result.ToList(); } }

            public IEnumerable<ShoppingSheetRuleTable> actualRuleTables;
           
            [TestInitialize]
            public void Initialize()
            {
                RuleTableRepositoryTestsInitialize();
            }

            [TestMethod]
            public async Task ExpectedEqualsActual()
            {
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);
                CollectionAssert.AreEqual(expectedRuleTables, actualRuleTables.ToList());
            }

            [TestMethod]
            public async Task NullAwardYearsReturnsEmptyListTest()
            {
                inputAwardYears = null;
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);
                Assert.AreEqual(0, actualRuleTables.Count());
            }

            [TestMethod]
            public async Task EmptyAwardYearsReturnsEmptyListTest()
            {
                inputAwardYears = new List<string>();
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);
                Assert.AreEqual(0, actualRuleTables.Count());
            }

            [TestMethod]
            public async Task NoRuleTableAcyrRecordsTest()
            {
                expectedRepository.ruleTableData = new List<TestRuleTableRepository.RuleTableRecord>();
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);
                Assert.AreEqual(0, actualRuleTables.Count());
            }

            [TestMethod]
            public async Task NullRuleTableTypeTest()
            {
                expectedRepository.ruleTableData.ForEach(rt => rt.type = null);
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);
                Assert.AreEqual(0, actualRuleTables.Count());
            }

            [TestMethod]
            public async Task NoTextTypeRuleTablesTest()
            {
                expectedRepository.ruleTableData.ForEach(rt => rt.type = "FOO");
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);
                Assert.AreEqual(0, actualRuleTables.Count());
            }

            [TestMethod]
            public async Task DefaultValueTest()
            {
                var defaultValue = "FOOBAR";
                expectedRepository.ruleTableData.ForEach(rt => rt.defaultResultString = defaultValue);
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);

                Assert.IsTrue(actualRuleTables.All(rt => rt.DefaultResult == defaultValue));
            }

            [TestMethod]
            public async Task DescriptionTest()
            {
                var description = "FOOBAR";
                expectedRepository.ruleTableData.ForEach(rt => rt.description = description);
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);

                Assert.IsTrue(actualRuleTables.All(rt => rt.Description == description));
            }

            [TestMethod]
            public async Task AlwaysUseDefaultTrueTest()
            {
                var alwaysUseDefault = "y";
                expectedRepository.ruleTableData.ForEach(rt => rt.defaultValueCondition = alwaysUseDefault);
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);

                Assert.IsTrue(actualRuleTables.All(rt => rt.AlwaysUseDefault));
            }

            [TestMethod]
            public async Task AlwaysUseDefaultFalseTest()
            {
                var alwaysUseDefault = "n";
                expectedRepository.ruleTableData.ForEach(rt => rt.defaultValueCondition = alwaysUseDefault);
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);

                Assert.IsTrue(actualRuleTables.All(rt => !rt.AlwaysUseDefault));
            }

            [TestMethod]
            public async Task NoRuleTableEntityAssociationTest()
            {
                expectedRepository.ruleTableData.ForEach(rt => rt.ruleTableList = new List<TestRuleTableRepository.RuleTableRecord.RuleTable>());
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);

                Assert.AreEqual(0, actualRuleTables.Sum(rt => rt.RuleResultPairs.Count()));
            }

            [TestMethod]
            public async Task RuleResultPairsTest()
            {
                var expectedRuleId = "FOO";
                var expectedResult = "BAR";
                expectedRepository.ruleTableData.ForEach(rt => rt.ruleTableList = new List<TestRuleTableRepository.RuleTableRecord.RuleTable>()
                    {
                        new TestRuleTableRepository.RuleTableRecord.RuleTable() { ruleId = expectedRuleId, result = expectedResult},
                        new TestRuleTableRepository.RuleTableRecord.RuleTable() { ruleId = expectedRuleId, result = expectedResult},
                    });
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);

                Assert.IsTrue(actualRuleTables.All(rt => rt.RuleResultPairs.Count() == 2));
                Assert.IsTrue(actualRuleTables.All(rt => rt.RuleResultPairs[0].Key == expectedRuleId && rt.RuleResultPairs[0].Value == expectedResult));
                Assert.IsTrue(actualRuleTables.All(rt => rt.RuleResultPairs[1].Key == expectedRuleId && rt.RuleResultPairs[1].Value == expectedResult));
            }

            [TestMethod]
            public async Task ErrorCreatingRuleTableTest()
            {
                expectedRepository.ruleTableData.ForEach(rt => rt.defaultResultString = string.Empty);
                actualRuleTables = await actualRepository.GetShoppingSheetRuleTablesAsync(inputAwardYears);

                Assert.AreEqual(0, actualRuleTables.Count());
                loggerMock.Verify(l => l.Info(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()));
            }
        }
    }
}
