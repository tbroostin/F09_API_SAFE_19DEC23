/*Copyright 2015-2017 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.FinancialAid.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.FinancialAid.Tests.Entities
{
    [TestClass]
    public class ShoppingSheetRuleTableTests
    {
        public string code;
        public string awardYear;
        public string description;
        public string defaultResult;
        public bool alwaysUseDefault;
        public string ruleId;
        public string result;
        public Rule<StudentAwardYear> ruleObject;
        public StudentAwardYear ruleTableContext;
        public ShoppingSheetRuleTable shoppingSheetRuleTable;
        public Func<IEnumerable<RuleRequest<StudentAwardYear>>, Task<IEnumerable<RuleResult>>> ruleProcessor;

        public void ShoppingSheetRuleTableTestsInitialize()
        {
            code = "CODE";
            awardYear = "2015";
            description = "This is a rule table";
            defaultResult = "This is the default result";
            alwaysUseDefault = true;
            ruleId = "NOISIR";
            result = "You have no ISIR";
            ruleObject = new Rule<StudentAwardYear>(ruleId, (year) => string.IsNullOrEmpty(year.FederallyFlaggedIsirId));
            ruleTableContext = new StudentAwardYear("0003914", "2015", new FinancialAidOffice("office"));
            ruleProcessor = new Func<IEnumerable<RuleRequest<StudentAwardYear>>, Task<IEnumerable<RuleResult>>>(
                (ruleRequests) => Task.FromResult(ruleRequests.Select(rr => new RuleResult() { RuleId = rr.Rule.Id, Passed = rr.Rule.Passes(rr.Context), Context = rr.Context })));
            shoppingSheetRuleTable = new ShoppingSheetRuleTable(code, awardYear, defaultResult);
        }

        [TestClass]
        public class ShoppingSheetRuleTableConstructorTests : ShoppingSheetRuleTableTests
        {
            [TestInitialize]
            public void Initialize()
            {
                ShoppingSheetRuleTableTestsInitialize();
            }

            [TestMethod]
            public void ShoppingSheetRuleTableConstructor_CodeTest()
            {
                Assert.AreEqual(code, shoppingSheetRuleTable.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShoppingSheetRuleTableConstructor_CodeRequiredTest()
            {
                new ShoppingSheetRuleTable(null, awardYear, defaultResult);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableConstructor_AwardYearTest()
            {
                Assert.AreEqual(awardYear, shoppingSheetRuleTable.AwardYear);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ShoppingSheetRuleTableConstructor_AwardYearRequiredTest()
            {
                new ShoppingSheetRuleTable(code, string.Empty, defaultResult);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableConstructor_DefaultResultTest()
            {
                Assert.AreEqual(defaultResult, shoppingSheetRuleTable.DefaultResult);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableConstructor_DefaultResultRequiredTest()
            {
                var shoppingSheetRuleTableResult = new ShoppingSheetRuleTable(code, awardYear, " ");
                Assert.AreEqual(shoppingSheetRuleTableResult.DefaultResult, " ");
            }

            [TestMethod]
            public void ShoppingSheetRuleTableConstructor_RuleResultPairsInitializedTest()
            {
                //RuleResultPairs is private so we should be able to add pair with no exception thrown
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableConstructor_RuleObjectsDictionaryInitializedTest()
            {
                //Rule Objects is private so we should be able to link RuleObject with no exception thrown
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.LinkRuleObjects(ruleObject);
            }
        }

        [TestClass]
        public class ShoppingSheetRuleTableAttributesTests : ShoppingSheetRuleTableTests
        {
            [TestInitialize]
            public void Initialize()
            {
                ShoppingSheetRuleTableTestsInitialize();
            }

            [TestMethod]
            public void ShoppingSheetRuleTableAttributes_DescriptionGetSetTest()
            {
                shoppingSheetRuleTable.Description = description;
                Assert.AreEqual(description, shoppingSheetRuleTable.Description);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableAttributes_DefaultResultGetSetTest()
            {
                var newDefaultResult = "foobar";
                shoppingSheetRuleTable.DefaultResult = newDefaultResult;
                Assert.AreEqual(newDefaultResult, shoppingSheetRuleTable.DefaultResult);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableAttributes_DefaultResultNullIgnoredTest()
            {
                shoppingSheetRuleTable.DefaultResult = null;
                Assert.AreEqual(defaultResult, shoppingSheetRuleTable.DefaultResult);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableAttributes_DefaultResultEmptyIgnoredTest()
            {
                shoppingSheetRuleTable.DefaultResult = string.Empty;
                Assert.AreEqual(defaultResult, shoppingSheetRuleTable.DefaultResult);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableAttributes_DefaultResultWhiteSpaceIgnoredTest()
            {
                shoppingSheetRuleTable.DefaultResult = "  ";
                Assert.AreEqual(defaultResult, shoppingSheetRuleTable.DefaultResult);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableAttributes_AlwaysUseDefaultTest()
            {
                shoppingSheetRuleTable.AlwaysUseDefault = alwaysUseDefault;
                Assert.AreEqual(alwaysUseDefault, shoppingSheetRuleTable.AlwaysUseDefault);
            }

            [TestMethod]
            public void ShoppingSheetRuleTableAttributes_RuleProcessorSetTest()
            {
                shoppingSheetRuleTable.RuleProcessor = ruleProcessor;
            }
        }

        [TestClass]
        public class RulesTests : ShoppingSheetRuleTableTests
        {
            [TestInitialize]
            public void Initialize()
            {
                ShoppingSheetRuleTableTestsInitialize();
            }

            [TestMethod]
            public void AddRuleResultPairTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                Assert.AreEqual(ruleId, shoppingSheetRuleTable.RuleResultPairs[0].Key);
                Assert.AreEqual(result, shoppingSheetRuleTable.RuleResultPairs[0].Value);
            }

            [TestMethod]
            public void DuplicateRuleResultsAllowedTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                Assert.AreEqual(2, shoppingSheetRuleTable.RuleResultPairs.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void RuleIdRequiredTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(null, result);
            }

            [TestMethod]
            public void ResultNotRequiredTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, null);
                Assert.AreEqual(null, shoppingSheetRuleTable.RuleResultPairs.First(p => p.Key == ruleId).Value);
            }

            [TestMethod]
            public void RuleIdsGetTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);

                CollectionAssert.AreEqual(new List<string>() { ruleId, ruleId, ruleId }, shoppingSheetRuleTable.RuleIds);
            }


            [TestMethod]
            public void NoRuleIdsTest()
            {
                Assert.AreEqual(0, shoppingSheetRuleTable.RuleIds.Count());
            }

            [TestMethod]
            public void LinkSingleRuleObjectsTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.LinkRuleObjects(ruleObject);

                //ruleRequest rule should equal ruleObject
                var ruleRequest = shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext).ElementAt(0);
                Assert.AreEqual(ruleObject.Id, ruleRequest.Rule.Id);
                Assert.IsTrue(ruleRequest.Rule.HasExpression);
            }

            [TestMethod]
            public void NullRuleObjectsNotLinkedTest()
            {
                List<Rule> nullRules = null;
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.LinkRuleObjects(nullRules);

                //ruleRequest rule should equal ruleObject
                var ruleRequest = shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext).ElementAt(0);
                Assert.AreEqual(ruleObject.Id, ruleRequest.Rule.Id);
                Assert.IsFalse(ruleRequest.Rule.HasExpression);
            }

            [TestMethod]
            public void OnlyStudentAwardYearRulesAreLinkedTest()
            {
                var otherRule = new Rule<Department>("id", dept => !string.IsNullOrEmpty(dept.Code));
                shoppingSheetRuleTable.AddRuleResultPair("id", "result");
                shoppingSheetRuleTable.LinkRuleObjects(otherRule);

                var ruleRequest = shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext).ElementAt(0);
                Assert.AreEqual("id", ruleRequest.Rule.Id);
                Assert.IsFalse(ruleRequest.Rule.HasExpression);
            }

            [TestMethod]
            public void OnlyLinkRuleIfIdInRuleResultPairs()
            {
                shoppingSheetRuleTable.LinkRuleObjects(ruleObject);

                var ruleRequests = shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext);
                Assert.AreEqual(0, ruleRequests.Count());
            }

            [TestMethod]
            public void DoNotLinkDuplicateRuleObjects()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.LinkRuleObjects(ruleObject);

                //rule object passes if federeally flagged id is null or empty. 
                //rule object converse passes if federally flagged id has value
                var ruleObjectConverse = new Rule<StudentAwardYear>(ruleId, (year) => !string.IsNullOrEmpty(year.FederallyFlaggedIsirId));

                //however rule object converse has same id as ruleobject, so it is considered a duplicate and will not be linked
                shoppingSheetRuleTable.LinkRuleObjects(ruleObjectConverse);

                //setup ruletable context to pass ruleObject and fail ruleObjectConverse
                ruleTableContext.FederallyFlaggedIsirId = string.Empty;

                var ruleRequest = shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext).ElementAt(0);
                Assert.AreEqual(ruleObject.Id, ruleRequest.Rule.Id);
                Assert.IsTrue(ruleRequest.Rule.HasExpression);

                //assert that the ruleRequest expression passes, which means only ruleObject was added. not ruleObjectConverse
                Assert.IsTrue(ruleRequest.Rule.Passes(ruleTableContext));
            }

            [TestMethod]
            public void CreateRuleRequests_NumberOfRuleResultPairsTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);

                var ruleRequests = shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext);

                Assert.AreEqual(3, ruleRequests.Count());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void CreateRuleRequests_RuleTableContextRequiredTest()
            {
                shoppingSheetRuleTable.CreateRuleRequests(null);
            }

            [TestMethod]
            public void CreateRuleRequests_UseLinkedRuleObjectTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.LinkRuleObjects(ruleObject);

                var ruleRequest = shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext).ElementAt(0);
                Assert.AreEqual(ruleObject.Id, ruleRequest.Rule.Id);
                Assert.IsTrue(ruleRequest.Rule.HasExpression);

                Assert.AreEqual(ruleTableContext, ruleRequest.Context);
            }

            [TestMethod]
            public void CreateRuleRequests_NoLinkedRuleObjectTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);

                var ruleRequest = shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext).ElementAt(0);
                Assert.AreEqual(ruleObject.Id, ruleRequest.Rule.Id);
                Assert.IsFalse(ruleRequest.Rule.HasExpression);

                Assert.AreEqual(ruleTableContext, ruleRequest.Context);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void GetRuleTableResultFromResults_RuleResultsRequiredTest()
            {
                List<RuleResult> nullResults = null;
                shoppingSheetRuleTable.GetRuleTableResult(nullResults);
            }

            [TestMethod]
            public void GetRuleTableResultFromResults_AlwaysUseDefaultTrue_DefaultResultFirstTest()
            {
                shoppingSheetRuleTable.AlwaysUseDefault = true;
                var results = shoppingSheetRuleTable.GetRuleTableResult(new List<RuleResult>());
                Assert.AreEqual(1, results.Count());
                Assert.AreEqual(defaultResult, results.ElementAt(0));
            }

            [TestMethod]
            public void GetRuleTableResultFromResults_NoRuleResults_DefaultResultFirstTest()
            {
                shoppingSheetRuleTable.AlwaysUseDefault = false;
                var results = shoppingSheetRuleTable.GetRuleTableResult(new List<RuleResult>());
                Assert.AreEqual(1, results.Count());
                Assert.AreEqual(defaultResult, results.ElementAt(0));
            }

            [TestMethod]
            public void GetRuleTableResultFromResults_NoRulesPass_DefaultResultFirstTest()
            {
                var rule1 = new Rule<StudentAwardYear>("id1", (year) => false);
                var rule2 = new Rule<StudentAwardYear>("id2", (year) => false);
                shoppingSheetRuleTable.AlwaysUseDefault = false;
                shoppingSheetRuleTable.AddRuleResultPair("id1", "foo");
                shoppingSheetRuleTable.AddRuleResultPair("id2", "bar");
                shoppingSheetRuleTable.LinkRuleObjects(new List<Rule>() { rule1, rule2 });
                var results = shoppingSheetRuleTable.GetRuleTableResult(ruleProcessor.Invoke(shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext)).Result);
                Assert.AreEqual(1, results.Count());
                Assert.AreEqual(defaultResult, results.ElementAt(0));
            }

            [TestMethod]
            public void GetRuleTableResultFromResults_OnlyResultsFromRulesInRuleResultPairsTest()
            {
                var rule1 = new Rule<StudentAwardYear>("id1", (year) => true);
                var rule2 = new Rule<StudentAwardYear>("id2", (year) => true);
                var rule3 = new Rule<StudentAwardYear>("id3", (year) => true);
                shoppingSheetRuleTable.AlwaysUseDefault = false;
                shoppingSheetRuleTable.AddRuleResultPair("id1", "foo");
                shoppingSheetRuleTable.AddRuleResultPair("id2", "bar");
                shoppingSheetRuleTable.LinkRuleObjects(new List<Rule>() { rule1, rule2, rule3 });

                var results = shoppingSheetRuleTable.GetRuleTableResult(ruleProcessor.Invoke(shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext)).Result);

                Assert.AreEqual(2, results.Count());
                Assert.AreEqual("foo", results.ElementAt(0));
                Assert.AreEqual("bar", results.ElementAt(1));
            }

            [TestMethod]
            public void GetRuleTableResultFromResults_OnlyResultsFromPassingRulesTest()
            {
                var rule1 = new Rule<StudentAwardYear>("id1", (year) => true);
                var rule2 = new Rule<StudentAwardYear>("id2", (year) => true);
                var rule3 = new Rule<StudentAwardYear>("id3", (year) => false);
                shoppingSheetRuleTable.AlwaysUseDefault = false;
                shoppingSheetRuleTable.AddRuleResultPair("id1", "foo");
                shoppingSheetRuleTable.AddRuleResultPair("id2", "bar");
                shoppingSheetRuleTable.AddRuleResultPair("id3", "whatever");
                shoppingSheetRuleTable.LinkRuleObjects(new List<Rule>() { rule1, rule2, rule3 });

                var results = shoppingSheetRuleTable.GetRuleTableResult(ruleProcessor.Invoke(shoppingSheetRuleTable.CreateRuleRequests(ruleTableContext)).Result);

                Assert.AreEqual(2, results.Count());
                Assert.AreEqual("foo", results.ElementAt(0));
                Assert.AreEqual("bar", results.ElementAt(1));
                Assert.IsFalse(results.Contains("whatever"));
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task GetRuleTableResultFromContext_ContextRequired()
            {
                ruleTableContext = null;
                await shoppingSheetRuleTable.GetRuleTableResultAsync(ruleTableContext);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task GetRuleTableResultFromContext_RuleProcessorRequired()
            {
                shoppingSheetRuleTable.RuleProcessor = null;
                await shoppingSheetRuleTable.GetRuleTableResultAsync(ruleTableContext);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task GetRuleTableResultFromContext_ThrowAnyExceptionFromRuleProcessor()
            {
                shoppingSheetRuleTable.RuleProcessor = new Func<IEnumerable<RuleRequest<StudentAwardYear>>, Task<IEnumerable<RuleResult>>>(
                    (requests) => { throw new ApplicationException(); });

                await shoppingSheetRuleTable.GetRuleTableResultAsync(ruleTableContext);
            }

            [TestMethod]
            public async Task GetRuleTableResultFromContext_RuleProcessorIsInvokedTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.LinkRuleObjects(ruleObject);

                var ruleProcessorInvoked = false;
                shoppingSheetRuleTable.RuleProcessor = new Func<IEnumerable<RuleRequest<StudentAwardYear>>, Task<IEnumerable<RuleResult>>>(
                    (ruleRequests) =>
                    {
                        ruleProcessorInvoked = true;
                        return Task.FromResult(ruleRequests.Select(rr => new RuleResult() { RuleId = rr.Rule.Id, Passed = rr.Rule.Passes(rr.Context), Context = rr.Context }));
                    });


                await shoppingSheetRuleTable.GetRuleTableResultAsync(ruleTableContext);
                Assert.IsTrue(ruleProcessorInvoked);
            }

            [TestMethod]
            public async Task GetRuleTableResultFromContext_RuleResultsFromRuleProcessorTest()
            {
                shoppingSheetRuleTable.AddRuleResultPair(ruleId, result);
                shoppingSheetRuleTable.LinkRuleObjects(ruleObject);
                shoppingSheetRuleTable.AlwaysUseDefault = true;

                shoppingSheetRuleTable.RuleProcessor = ruleProcessor;
                var results = await shoppingSheetRuleTable.GetRuleTableResultAsync(ruleTableContext);
                Assert.AreEqual(2, results.Count());
                Assert.AreEqual(defaultResult, results.ElementAt(0));
                Assert.AreEqual(result, results.ElementAt(1));
            }
        }
    }
}
