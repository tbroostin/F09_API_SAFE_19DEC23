// Copyright 2012-2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Tests;
using Ellucian.Data.Colleague;
using Ellucian.Web.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Base.Tests.Repositories
{
    [TestClass]
    public class RuleRepositoryTests
    {
        [TestClass]
        public class RuleRepositoryGetManyAsyncTests
        {
            MemoryCacheProvider cacheProvider;
            Mock<IColleagueTransactionFactory> txFactoryMock;
            Mock<IColleagueTransactionInvoker> ctxMock;

            Mock<IColleagueDataReader> dataReaderMock;
            ILogger logger;
            RuleAdapterRegistry registry;
            RuleConfiguration config;
            RuleRepository repo;

            [TestInitialize]
            public void Initialize()
            {
                cacheProvider = new MemoryCacheProvider();
                txFactoryMock = new Mock<IColleagueTransactionFactory>();

                dataReaderMock = new Mock<IColleagueDataReader>();
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(new DataContracts.IntlParams() { HostDateDelimiter = "/", HostShortDateFormat = "MDY" });

                txFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                logger = new Mock<ILogger>().Object;
                registry = new RuleAdapterRegistry();
                registry.Register<Department>("DEPTS", new TestRuleAdapter());
                config = new RuleConfiguration();
                repo = new RuleRepository(cacheProvider, txFactoryMock.Object, logger, registry, config);
            }

            [TestMethod]
            public async Task ReturnsEmptyListWhenRuleIdsNull()
            {
                var result = await repo.GetManyAsync(null);
                Assert.IsTrue(result.Count() == 0);
            }

            [TestMethod]
            public async Task ReturnsEmptyListWhenRuleIdsNotRetreived()
            {
                dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Rules>("RULES", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<Rules>() { });
                var result = await repo.GetManyAsync(new List<string>() { "1234", "5678" });
                Assert.IsTrue(result.Count() == 0);
            }
        }

        [TestClass]
        public class RuleRepositoryGetAsyncTests
        {
            MemoryCacheProvider cacheProvider;
            Mock<IColleagueTransactionFactory> txFactoryMock;
            Mock<IColleagueTransactionInvoker> ctxMock;

            Mock<IColleagueDataReader> dataReaderMock;
            ILogger logger;
            RuleAdapterRegistry registry;
            RuleConfiguration config;
            RuleRepository repo;

            [TestInitialize]
            public void Initialize()
            {
                cacheProvider = new MemoryCacheProvider();
                txFactoryMock = new Mock<IColleagueTransactionFactory>();

                dataReaderMock = new Mock<IColleagueDataReader>();
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(new DataContracts.IntlParams() { HostDateDelimiter = "/", HostShortDateFormat = "MDY" });

                txFactoryMock.Setup(transFac => transFac.GetDataReader()).Returns(dataReaderMock.Object);

                logger = new Mock<ILogger>().Object;
                registry = new RuleAdapterRegistry();
                registry.Register<Department>("DEPTS", new TestRuleAdapter());
                config = new RuleConfiguration();
                repo = new RuleRepository(cacheProvider, txFactoryMock.Object, logger, registry, config);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task ReturnsNullWhenRuleIdNull()
            {
                dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Rules>("RULES", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<Rules>() { });
                var result = await repo.GetAsync(null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public async Task ReturnsNullWhenRuleIdNotRetreived()
            {
                dataReaderMock.Setup(reader => reader.BulkReadRecordAsync<Rules>("RULES", It.IsAny<string[]>(), It.IsAny<bool>())).ReturnsAsync(new Collection<Rules>() { });
                var result = await repo.GetAsync("1234");
            }
        }

        [TestClass]
        public class RuleRepositoryExecuteAsyncTests
        {
            private IEnumerable<Department> depts = new TestDepartmentRepository().Get();

            [TestMethod]
            public async Task ReturnsResultsInCorrectOrder()
            {
                var cacheProvider = new MemoryCacheProvider();
                var txFactoryMock = new Mock<IColleagueTransactionFactory>();
                var ctxMock = new Mock<IColleagueTransactionInvoker>();

                var dataReaderMock = new Mock<IColleagueDataReader>();
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(new DataContracts.IntlParams() { HostDateDelimiter = "/", HostShortDateFormat = "MDY" });

                txFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(ctxMock.Object);
                var logger = new Mock<ILogger>().Object;
                var registry = new RuleAdapterRegistry();
                registry.Register<Department>("DEPTS", new TestRuleAdapter());
                var config = new RuleConfiguration();
                var repo = new RuleRepository(cacheProvider, txFactoryMock.Object, logger, registry, config);
                var ruleRequests = new List<RuleRequest<Department>>();
                var math = depts.First(d => d.Code == "MATH");
                var biol = depts.First(d => d.Code == "BIOL");

                var rule1 = new Rule<Department>("R1");
                var rule2 = new Rule<Department>("R2");
                var rule3 = new Rule<Department>("R3");

                ruleRequests.Add(new RuleRequest<Department>(rule1, math));
                ruleRequests.Add(new RuleRequest<Department>(rule1, biol));
                ruleRequests.Add(new RuleRequest<Department>(rule2, math));
                ruleRequests.Add(new RuleRequest<Department>(rule2, biol));
                ruleRequests.Add(new RuleRequest<Department>(rule3, math));
                ruleRequests.Add(new RuleRequest<Department>(rule3, biol));

                var req1 = new ExecuteRulesRequest();
                req1.ARuleId = rule1.Id;
                req1.AFileSuiteInstance = "";
                req1.AlRecordIds = new List<string>() { "MATH", "BIOL" };
                var resp1 = new ExecuteRulesResponse();
                resp1.Results = new List<bool>() { true, false };

                var req2 = new ExecuteRulesRequest();
                req2.ARuleId = rule2.Id;
                req2.AFileSuiteInstance = "";
                req2.AlRecordIds = new List<string>() { "MATH", "BIOL" };
                var resp2 = new ExecuteRulesResponse();
                resp2.Results = new List<bool>() { false, true };

                var req3 = new ExecuteRulesRequest();
                req3.ARuleId = rule3.Id;
                req3.AFileSuiteInstance = "";
                req3.AlRecordIds = new List<string>() { "MATH", "BIOL" };
                var resp3 = new ExecuteRulesResponse();
                resp3.Results = new List<bool>() { true, true };

                ctxMock.Setup(cti => cti.ExecuteAsync<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req1)))).ReturnsAsync(resp1);
                ctxMock.Setup(cti => cti.ExecuteAsync<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req2)))).ReturnsAsync(resp2);
                ctxMock.Setup(cti => cti.ExecuteAsync<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req3)))).ReturnsAsync(resp3);

                var results = await repo.ExecuteAsync<Department>(ruleRequests);
                Assert.AreEqual(6, results.Count());

                Assert.IsTrue(results.First(rr => rr.RuleId == rule1.Id && rr.Context == math).Passed);
                Assert.IsFalse(results.First(rr => rr.RuleId == rule1.Id && rr.Context == biol).Passed);

                Assert.IsFalse(results.First(rr => rr.RuleId == rule2.Id && rr.Context == math).Passed);
                Assert.IsTrue(results.First(rr => rr.RuleId == rule2.Id && rr.Context == biol).Passed);

                Assert.IsTrue(results.First(rr => rr.RuleId == rule3.Id && rr.Context == math).Passed);
                Assert.IsTrue(results.First(rr => rr.RuleId == rule3.Id && rr.Context == biol).Passed);
            }

            private bool IsSameRequest(ExecuteRulesRequest a, ExecuteRulesRequest b)
            {
                return a.ARuleId == b.ARuleId && a.AFileSuiteInstance == b.AFileSuiteInstance && Enumerable.SequenceEqual(a.AlRecordIds, b.AlRecordIds);
            }

            [TestMethod]
            public async Task ReturnsResultsForAllRuleRequests()
            {
                var cacheProvider = new MemoryCacheProvider();
                var txFactoryMock = new Mock<IColleagueTransactionFactory>();
                var ctxMock = new Mock<IColleagueTransactionInvoker>();
                txFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(ctxMock.Object);
                var logger = new Mock<ILogger>().Object;
                var registry = new RuleAdapterRegistry();
                registry.Register<Department>("DEPTS", new TestRuleAdapter());
                var config = new RuleConfiguration();
                var repo = new RuleRepository(cacheProvider, txFactoryMock.Object, logger, registry, config);
                var ruleRequests = new List<RuleRequest<Department>>();
                var math = depts.First(d => d.Code == "MATH");
                var biol = depts.First(d => d.Code == "BIOL");

                var rule1 = new Rule<Department>("R1");
                var rule2 = new Rule<Department>("R2");

                // Create a rule that can be evaluated in .NET
                var rd = new RuleDescriptor()
                {
                    Id = "R3",
                    PrimaryView = "DEPTS"
                };
                var red = new RuleExpressionDescriptor();
                red.Connector = "WITH";
                red.DataElement = new RuleDataElement() { Id = "DEPTS.ID" };
                red.Operator = "EQ";
                red.Literal = "\'MATH\'";
                rd.Expressions.Add(red);
                var adapter = registry.Get(rd.PrimaryView);
                var rule3 = (Rule<Department>)adapter.Create(rd);


                // Add two rule requests for each rule.
                ruleRequests.Add(new RuleRequest<Department>(rule1, math));
                ruleRequests.Add(new RuleRequest<Department>(rule1, biol));
                ruleRequests.Add(new RuleRequest<Department>(rule2, math));
                ruleRequests.Add(new RuleRequest<Department>(rule2, biol));
                ruleRequests.Add(new RuleRequest<Department>(rule3, math));
                ruleRequests.Add(new RuleRequest<Department>(rule3, biol));

                // Setup responses for the requests that must go to Colleague.
                var req1 = new ExecuteRulesRequest();
                req1.ARuleId = rule1.Id;
                req1.AFileSuiteInstance = "";
                req1.AlRecordIds = new List<string>() { "MATH", "BIOL" };

                var resp1 = new ExecuteRulesResponse();
                resp1.Results = new List<bool>() { true, false };

                var req2 = new ExecuteRulesRequest();
                req2.ARuleId = rule2.Id;
                req2.AFileSuiteInstance = "";
                req2.AlRecordIds = new List<string>() { "MATH", "BIOL" };
                var resp2 = new ExecuteRulesResponse();
                resp2.Results = new List<bool>() { false, true };

                ctxMock.Setup(cti => cti.ExecuteAsync<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req1)))).ReturnsAsync(resp1);
                ctxMock.Setup(cti => cti.ExecuteAsync<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req2)))).ReturnsAsync(resp2);

                var results = await repo.ExecuteAsync<Department>(ruleRequests);
                Assert.AreEqual(6, results.Count());

                Assert.IsTrue(results.First(rr => rr.RuleId == rule1.Id && rr.Context == math).Passed);
                Assert.IsFalse(results.First(rr => rr.RuleId == rule1.Id && rr.Context == biol).Passed);

                Assert.IsFalse(results.First(rr => rr.RuleId == rule2.Id && rr.Context == math).Passed);
                Assert.IsTrue(results.First(rr => rr.RuleId == rule2.Id && rr.Context == biol).Passed);

                Assert.IsTrue(results.First(rr => rr.RuleId == rule3.Id && rr.Context == math).Passed);
                Assert.IsFalse(results.First(rr => rr.RuleId == rule3.Id && rr.Context == biol).Passed);
            }

            [TestMethod]
            public async Task ExecutesFileSuiteRulesSeperately()
            {
                var cacheProvider = new MemoryCacheProvider();
                var txFactoryMock = new Mock<IColleagueTransactionFactory>();
                var ctxMock = new Mock<IColleagueTransactionInvoker>();
                txFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(ctxMock.Object);
                var logger = new Mock<ILogger>().Object;
                var registry = new RuleAdapterRegistry();
                registry.Register<TestFileSuite>("FILE.SUITE.ACYR", new TestFileSuiteRuleAdapter());
                var config = new RuleConfiguration();
                var repo = new RuleRepository(cacheProvider, txFactoryMock.Object, logger, registry, config);
                var ruleRequests = new List<RuleRequest<TestFileSuite>>();

                var rule1 = new Rule<TestFileSuite>("R1");

                //context objects have the same file suite instance, and different ids. one ctx for these
                var fooInstance1 = new TestFileSuite() { FileSuiteInstance = "2014", Id = "FOO" };
                var fooInstance2 = new TestFileSuite() { FileSuiteInstance = "2014", Id = "BAR" };

                //context object has different file suite instance, but same id as fooInstance1. different ctx for this one.
                var fooInstance3 = new TestFileSuite() { FileSuiteInstance = "2015", Id = "FOO" };

                ruleRequests.Add(new RuleRequest<TestFileSuite>(rule1, fooInstance1));
                ruleRequests.Add(new RuleRequest<TestFileSuite>(rule1, fooInstance2));
                ruleRequests.Add(new RuleRequest<TestFileSuite>(rule1, fooInstance3));

                var ctxRequest1 = new ExecuteRulesRequest()
                {
                    ARuleId = rule1.Id,
                    AFileSuiteInstance = "2014",
                    AlRecordIds = new List<string>() { "FOO", "BAR" }
                };
                var ctxRequest2 = new ExecuteRulesRequest()
                {
                    ARuleId = rule1.Id,
                    AFileSuiteInstance = "2015",
                    AlRecordIds = new List<string>() { "FOO" }
                };

                //mock the transaction so that all the rules pass, and save off the actual requests
                var actualExecuteRulesRequests = new List<ExecuteRulesRequest>();
                ctxMock.Setup(t => t.ExecuteAsync<ExecuteRulesRequest, ExecuteRulesResponse>(It.IsAny<ExecuteRulesRequest>()))
                    .Callback<ExecuteRulesRequest>(req => actualExecuteRulesRequests.Add(req))
                    .Returns((ExecuteRulesRequest req) =>
                        {
                            return Task.FromResult(new ExecuteRulesResponse()
                                {
                                    Results = req.AlRecordIds.Select(id => true).ToList()
                                });
                        });

                var ruleResults = await repo.ExecuteAsync<TestFileSuite>(ruleRequests);

                //Number of inputRuleRequests should equal number of results
                Assert.AreEqual(ruleRequests.Count(), ruleResults.Count());

                Assert.AreEqual(2, actualExecuteRulesRequests.Count());

                Assert.IsTrue(IsSameRequest(ctxRequest1, actualExecuteRulesRequests[0]));
                Assert.IsTrue(IsSameRequest(ctxRequest2, actualExecuteRulesRequests[1]));

            }
        }

        [TestClass]
        public class RuleRepositoryExecuteTests
        {
            private IEnumerable<Department> depts = new TestDepartmentRepository().Get();

            [TestMethod]
            public void ReturnsResultsInCorrectOrder()
            {
                var cacheProvider = new MemoryCacheProvider();
                var txFactoryMock = new Mock<IColleagueTransactionFactory>();
                var ctxMock = new Mock<IColleagueTransactionInvoker>();

                var dataReaderMock = new Mock<IColleagueDataReader>();
                dataReaderMock.Setup(acc => acc.ReadRecordAsync<Data.Base.DataContracts.IntlParams>("INTL.PARAMS", "INTERNATIONAL", true)).ReturnsAsync(new DataContracts.IntlParams() { HostDateDelimiter = "/", HostShortDateFormat = "MDY" });

                txFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(ctxMock.Object);
                var logger = new Mock<ILogger>().Object;
                var registry = new RuleAdapterRegistry();
                registry.Register<Department>("DEPTS", new TestRuleAdapter());
                var config = new RuleConfiguration();
                var repo = new RuleRepository(cacheProvider, txFactoryMock.Object, logger, registry, config);
                var ruleRequests = new List<RuleRequest<Department>>();
                var math = depts.First(d => d.Code == "MATH");
                var biol = depts.First(d => d.Code == "BIOL");

                var rule1 = new Rule<Department>("R1");
                var rule2 = new Rule<Department>("R2");
                var rule3 = new Rule<Department>("R3");

                ruleRequests.Add(new RuleRequest<Department>(rule1, math));
                ruleRequests.Add(new RuleRequest<Department>(rule1, biol));
                ruleRequests.Add(new RuleRequest<Department>(rule2, math));
                ruleRequests.Add(new RuleRequest<Department>(rule2, biol));
                ruleRequests.Add(new RuleRequest<Department>(rule3, math));
                ruleRequests.Add(new RuleRequest<Department>(rule3, biol));

                var req1 = new ExecuteRulesRequest();
                req1.ARuleId = rule1.Id;
                req1.AFileSuiteInstance = "";
                req1.AlRecordIds = new List<string>() { "MATH", "BIOL" };
                var resp1 = new ExecuteRulesResponse();
                resp1.Results = new List<bool>() { true, false };

                var req2 = new ExecuteRulesRequest();
                req2.ARuleId = rule2.Id;
                req2.AFileSuiteInstance = "";
                req2.AlRecordIds = new List<string>() { "MATH", "BIOL" };
                var resp2 = new ExecuteRulesResponse();
                resp2.Results = new List<bool>() { false, true };

                var req3 = new ExecuteRulesRequest();
                req3.ARuleId = rule3.Id;
                req3.AFileSuiteInstance = "";
                req3.AlRecordIds = new List<string>() { "MATH", "BIOL" };
                var resp3 = new ExecuteRulesResponse();
                resp3.Results = new List<bool>() { true, true };

                ctxMock.Setup(cti => cti.Execute<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req1)))).Returns(resp1);
                ctxMock.Setup(cti => cti.Execute<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req2)))).Returns(resp2);
                ctxMock.Setup(cti => cti.Execute<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req3)))).Returns(resp3);

                var results = repo.Execute<Department>(ruleRequests);
                Assert.AreEqual(6, results.Count());

                Assert.IsTrue(results.First(rr => rr.RuleId == rule1.Id && rr.Context == math).Passed);
                Assert.IsFalse(results.First(rr => rr.RuleId == rule1.Id && rr.Context == biol).Passed);

                Assert.IsFalse(results.First(rr => rr.RuleId == rule2.Id && rr.Context == math).Passed);
                Assert.IsTrue(results.First(rr => rr.RuleId == rule2.Id && rr.Context == biol).Passed);

                Assert.IsTrue(results.First(rr => rr.RuleId == rule3.Id && rr.Context == math).Passed);
                Assert.IsTrue(results.First(rr => rr.RuleId == rule3.Id && rr.Context == biol).Passed);
            }

            private bool IsSameRequest(ExecuteRulesRequest a, ExecuteRulesRequest b)
            {
                return a.ARuleId == b.ARuleId && a.AFileSuiteInstance == b.AFileSuiteInstance && Enumerable.SequenceEqual(a.AlRecordIds, b.AlRecordIds);
            }

            [TestMethod]
            public void ReturnsResultsForAllRuleRequests()
            {
                var cacheProvider = new MemoryCacheProvider();
                var txFactoryMock = new Mock<IColleagueTransactionFactory>();
                var ctxMock = new Mock<IColleagueTransactionInvoker>();
                txFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(ctxMock.Object);
                var logger = new Mock<ILogger>().Object;
                var registry = new RuleAdapterRegistry();
                registry.Register<Department>("DEPTS", new TestRuleAdapter());
                var config = new RuleConfiguration();
                var repo = new RuleRepository(cacheProvider, txFactoryMock.Object, logger, registry, config);
                var ruleRequests = new List<RuleRequest<Department>>();
                var math = depts.First(d => d.Code == "MATH");
                var biol = depts.First(d => d.Code == "BIOL");

                var rule1 = new Rule<Department>("R1");
                var rule2 = new Rule<Department>("R2");

                // Create a rule that can be evaluated in .NET
                var rd = new RuleDescriptor()
                {
                    Id = "R3",
                    PrimaryView = "DEPTS"
                };
                var red = new RuleExpressionDescriptor();
                red.Connector = "WITH";
                red.DataElement = new RuleDataElement() { Id = "DEPTS.ID" };
                red.Operator = "EQ";
                red.Literal = "\'MATH\'";
                rd.Expressions.Add(red);
                var adapter = registry.Get(rd.PrimaryView);
                var rule3 = (Rule<Department>)adapter.Create(rd);


                // Add two rule requests for each rule.
                ruleRequests.Add(new RuleRequest<Department>(rule1, math));
                ruleRequests.Add(new RuleRequest<Department>(rule1, biol));
                ruleRequests.Add(new RuleRequest<Department>(rule2, math));
                ruleRequests.Add(new RuleRequest<Department>(rule2, biol));
                ruleRequests.Add(new RuleRequest<Department>(rule3, math));
                ruleRequests.Add(new RuleRequest<Department>(rule3, biol));

                // Setup responses for the requests that must go to Colleague.
                var req1 = new ExecuteRulesRequest();
                req1.ARuleId = rule1.Id;
                req1.AFileSuiteInstance = "";
                req1.AlRecordIds = new List<string>() { "MATH", "BIOL" };

                var resp1 = new ExecuteRulesResponse();
                resp1.Results = new List<bool>() { true, false };

                var req2 = new ExecuteRulesRequest();
                req2.ARuleId = rule2.Id;
                req2.AFileSuiteInstance = "";
                req2.AlRecordIds = new List<string>() { "MATH", "BIOL" };
                var resp2 = new ExecuteRulesResponse();
                resp2.Results = new List<bool>() { false, true };

                ctxMock.Setup(cti => cti.Execute<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req1)))).Returns(resp1);
                ctxMock.Setup(cti => cti.Execute<ExecuteRulesRequest, ExecuteRulesResponse>(It.Is<ExecuteRulesRequest>(r => IsSameRequest(r, req2)))).Returns(resp2);

                var results = repo.Execute<Department>(ruleRequests);
                Assert.AreEqual(6, results.Count());

                Assert.IsTrue(results.First(rr => rr.RuleId == rule1.Id && rr.Context == math).Passed);
                Assert.IsFalse(results.First(rr => rr.RuleId == rule1.Id && rr.Context == biol).Passed);

                Assert.IsFalse(results.First(rr => rr.RuleId == rule2.Id && rr.Context == math).Passed);
                Assert.IsTrue(results.First(rr => rr.RuleId == rule2.Id && rr.Context == biol).Passed);

                Assert.IsTrue(results.First(rr => rr.RuleId == rule3.Id && rr.Context == math).Passed);
                Assert.IsFalse(results.First(rr => rr.RuleId == rule3.Id && rr.Context == biol).Passed);
            }

            [TestMethod]
            public void ExecutesFileSuiteRulesSeperately()
            {
                var cacheProvider = new MemoryCacheProvider();
                var txFactoryMock = new Mock<IColleagueTransactionFactory>();
                var ctxMock = new Mock<IColleagueTransactionInvoker>();
                txFactoryMock.Setup(tf => tf.GetTransactionInvoker()).Returns(ctxMock.Object);
                var logger = new Mock<ILogger>().Object;
                var registry = new RuleAdapterRegistry();
                registry.Register<TestFileSuite>("FILE.SUITE.ACYR", new TestFileSuiteRuleAdapter());
                var config = new RuleConfiguration();
                var repo = new RuleRepository(cacheProvider, txFactoryMock.Object, logger, registry, config);
                var ruleRequests = new List<RuleRequest<TestFileSuite>>();

                var rule1 = new Rule<TestFileSuite>("R1");

                //context objects have the same file suite instance, and different ids. one ctx for these
                var fooInstance1 = new TestFileSuite() { FileSuiteInstance = "2014", Id = "FOO" };
                var fooInstance2 = new TestFileSuite() { FileSuiteInstance = "2014", Id = "BAR" };

                //context object has different file suite instance, but same id as fooInstance1. different ctx for this one.
                var fooInstance3 = new TestFileSuite() { FileSuiteInstance = "2015", Id = "FOO" };

                ruleRequests.Add(new RuleRequest<TestFileSuite>(rule1, fooInstance1));
                ruleRequests.Add(new RuleRequest<TestFileSuite>(rule1, fooInstance2));
                ruleRequests.Add(new RuleRequest<TestFileSuite>(rule1, fooInstance3));

                var ctxRequest1 = new ExecuteRulesRequest()
                {
                    ARuleId = rule1.Id,
                    AFileSuiteInstance = "2014",
                    AlRecordIds = new List<string>() { "FOO", "BAR" }
                };
                var ctxRequest2 = new ExecuteRulesRequest()
                {
                    ARuleId = rule1.Id,
                    AFileSuiteInstance = "2015",
                    AlRecordIds = new List<string>() { "FOO" }
                };

                //mock the transaction so that all the rules pass, and save off the actual requests
                var actualExecuteRulesRequests = new List<ExecuteRulesRequest>();
                ctxMock.Setup(t => t.Execute<ExecuteRulesRequest, ExecuteRulesResponse>(It.IsAny<ExecuteRulesRequest>()))
                    .Callback<ExecuteRulesRequest>(req => actualExecuteRulesRequests.Add(req))
                    .Returns<ExecuteRulesRequest>(req => new ExecuteRulesResponse()
                    {
                        Results = req.AlRecordIds.Select(id => true).ToList()
                    });

                var ruleResults = repo.Execute<TestFileSuite>(ruleRequests);

                //Number of inputRuleRequests should equal number of results
                Assert.AreEqual(ruleRequests.Count(), ruleResults.Count());

                Assert.AreEqual(2, actualExecuteRulesRequests.Count());

                Assert.IsTrue(IsSameRequest(ctxRequest1, actualExecuteRulesRequests[0]));
                Assert.IsTrue(IsSameRequest(ctxRequest2, actualExecuteRulesRequests[1]));

            }
        }
    }

    // Create a test rule adapter for departments, a base entity, for testing purposes.
    class TestRuleAdapter : RuleAdapter
    {
        public override string GetRecordId(object context)
        {
            return (context as Department).Code;
        }

        public override Type ContextType { get { return typeof(Department); } }

        protected override Expression CreateDataElementExpression(RuleDataElement dataElement, Expression param, out string unsupportedMessage)
        {
            Expression lhs = null;
            string unsupported = null;
            switch (dataElement.Id)
            {
                case "DEPTS.ID":
                    lhs = Expression.Property(param, "Code");
                    break;
                default:
                    unsupported = "The field " + dataElement.Id + " is not supported yet";
                    break;
            }
            unsupportedMessage = unsupported;
            return lhs;
        }

        protected override Rule CreateExpressionAndRule(string ruleId, Expression finalExpression, ParameterExpression contextExpression)
        {
            Expression<Func<Department, bool>> lambdaExpression = null;

            if (finalExpression != null && contextExpression != null)
            {
                lambdaExpression = Expression.Lambda<Func<Department, bool>>(finalExpression, contextExpression);
            }

            return new Rule<Department>(ruleId, lambdaExpression);
        }

        public override string ExpectedPrimaryView
        {
            get { return "DEPTS"; }
        }
    }

    // Create a test rule adapter with a file suite override for TestFileSuite, an entity specific to this test class
    class TestFileSuiteRuleAdapter : RuleAdapter
    {

        public override string GetRecordId(object context)
        {
            return (context as TestFileSuite).Id;
        }

        public override string GetFileSuiteInstance(object context)
        {
            return (context as TestFileSuite).FileSuiteInstance;
        }

        public override Type ContextType
        {
            get { return typeof(TestFileSuite); }
        }

        public override string ExpectedPrimaryView
        {
            get { return "FILE.SUITE.ACYR"; }
        }

        protected override Rule CreateExpressionAndRule(string ruleId, Expression finalExpression, ParameterExpression contextExpression)
        {
            Expression<Func<TestFileSuite, bool>> lambdaExpression = null;

            if (finalExpression != null && contextExpression != null)
            {
                lambdaExpression = Expression.Lambda<Func<TestFileSuite, bool>>(finalExpression, contextExpression);
            }

            return new Rule<TestFileSuite>(ruleId, lambdaExpression);
        }

        protected override Expression CreateDataElementExpression(RuleDataElement dataElement, Expression param, out string unsupportedMessage)
        {
            Expression lhs = null;
            string unsupported = null;
            switch (dataElement.Id)
            {
                case "FILE.SUITE.ID":
                    lhs = Expression.Property(param, "Id");
                    break;
                default:
                    unsupported = "The field " + dataElement.Id + " is not supported yet";
                    break;
            }
            unsupportedMessage = unsupported;
            return lhs;
        }
    }

    // Create a test class that represents a file suite table in Colleague
    public class TestFileSuite
    {
        public string FileSuiteInstance;
        public string Id;
    }

}
