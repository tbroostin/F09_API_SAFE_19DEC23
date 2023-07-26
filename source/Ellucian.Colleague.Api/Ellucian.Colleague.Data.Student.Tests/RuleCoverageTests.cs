// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Client;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Security;
using Microsoft.IdentityModel.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests
{
    /// This harness can be used to test against live environments.  
    /// 
    /// 1. DO NOT CHECK THIS FILE IN
    ///  1a. If you must check this file in, make sure any system information is blanked before delivery.
    ///  1b. Make sure the Ignore keyword is not commented out on checkin.  These tests should not be allowed
    ///      to run as part of a "run all tests in solution" because actually hitting an environment for data is 
    ///      slower than test data.
    /// </summary>
    [TestClass]
    [Ignore]
    public class RuleCoverageTests
    {
        private ConsoleLogger logger = new ConsoleLogger() { IsDebugEnabled = true, IsErrorEnabled = true, IsInfoEnabled = true, IsTraceEnabled = true, IsWarnEnabled = true };
        private DmiSettings settings;

        [TestMethod]
        public async Task RuleCoverageTestAgainstEnvironment()
        {
            var environment1 = await Environment1Async();
            var environment2 = await Environment2Async();


            DumpResults(environment1);
            DumpResults(environment2);

        }

        private static void DumpResults(EnvironmentResults results)
        {
            Console.WriteLine();
            Console.WriteLine(results.Name);
            Console.WriteLine("\t" + results.Supported + " rules supported");
            Console.WriteLine("\t" + results.Count + " rules total");
            Console.WriteLine("\t" + ((float)results.Supported / (float)results.Count) * 100 + "% covered");
            Console.WriteLine();
        }

        private async Task<EnvironmentResults> Environment1Async()
        {
            settings = new DmiSettings();
            settings.AccountName = "test_account";
            settings.IpAddress = "yourserver.yourschool.com";
            settings.Port = 9999;
            settings.SharedSecret = "sharedsecret123";
            var token = new ColleagueSessionRepository(settings, new MemoryCacheProvider()).LoginAsync("student_id", "student_password");

            var principal = JwtHelper.CreatePrincipal(token.Result);
            var sessionClaim = (principal as IClaimsPrincipal).Identities.First().Claims.FirstOrDefault(c => c.ClaimType == "sid");
            var securityToken = sessionClaim.Value.Split('*')[0];
            var controlId = sessionClaim.Value.Split('*')[1];
            var session = new StandardDmiSession() { SecurityToken = securityToken, SenderControlId = controlId };
            var txFactory = new TestTransactionFactory(session, logger, settings);

            var results = await QueryRulesAsync("environment1", txFactory);

            await new ColleagueSessionRepository(settings, new MemoryCacheProvider()).LogoutAsync(token.Result);

            return results;
        }

        private async Task<EnvironmentResults> Environment2Async()
        {
            settings = new DmiSettings();
            settings.AccountName = "live_account";
            settings.IpAddress = "yourserver.yourschool.com";
            settings.Port = 9999;
            settings.SharedSecret = "sharedsecret123";
            var token = new ColleagueSessionRepository(settings, new MemoryCacheProvider()).LoginAsync("student_id", "student_password");

            var principal = JwtHelper.CreatePrincipal(token.Result);
            var sessionClaim = (principal as IClaimsPrincipal).Identities.First().Claims.FirstOrDefault(c => c.ClaimType == "sid");
            var securityToken = sessionClaim.Value.Split('*')[0];
            var controlId = sessionClaim.Value.Split('*')[1];
            var session = new StandardDmiSession() { SecurityToken = securityToken, SenderControlId = controlId };
            var txFactory = new TestTransactionFactory(session, logger, settings);

            var results = await QueryRulesAsync("environment2", txFactory);

            new ColleagueSessionRepository(settings, new MemoryCacheProvider()).LogoutAsync(token.Result);

            return results;
        }


        private async Task<EnvironmentResults> QueryRulesAsync(string env, IColleagueTransactionFactory txFactory)
        {
            var results = new EnvironmentResults();
            results.Name = env;
            var cacheProvider = new MemoryCacheProvider();
            var ruleIds = txFactory.GetDataReader().Select("ACAD.REQMT.BLOCKS", "ACRB.ACAD.CRED.RULES NE '' SAVING ACRB.ACAD.CRED.RULES");
            var ruleAdapterRegistry = new RuleAdapterRegistry();
            ruleAdapterRegistry.Register<AcademicCredit>("STUDENT.ACAD.CRED", new AcademicCreditRuleAdapter());
            ruleAdapterRegistry.Register<Course>("COURSES", new CourseRuleAdapter());
            var ruleRepository = new RuleRepository(cacheProvider, txFactory, logger, ruleAdapterRegistry, new RuleConfiguration());
            var rules = await ruleRepository.GetManyAsync(ruleIds);
            results.Count = rules.Count();
            foreach (var rule in rules)
            {
                if (rule.GetType() == typeof(Rule<Course>))
                {
                    var crule = (Rule<Course>)rule;
                    if (crule.HasExpression)
                    {
                        results.Supported++;
                    }
                    else
                    {
                        results.NotSupportedNames.Add(crule);
                    }
                }
                else if (rule.GetType() == typeof(Rule<AcademicCredit>))
                {
                    var stcRule = (Rule<AcademicCredit>)rule;
                    if (stcRule.HasExpression)
                    {
                        results.Supported++;
                    }
                    else
                    {
                        results.NotSupportedNames.Add(stcRule);
                    }
                }
            }

            Console.WriteLine("Begin unsupported rules for " + env);
            var rawRecords = txFactory.GetDataReader().BulkReadRecord<Rules>("RULES", results.NotSupportedNames.Select(rr => rr.Id).ToArray());
            foreach (var record in rawRecords)
            {
                Console.WriteLine(RuleRepository.Dump(record));
                Console.WriteLine("Not supported because: " + results.NotSupportedNames.First(rr => rr.Id == record.Recordkey).NotSupportedMessage);
            }
            Console.WriteLine("End unsupported rules for " + env);
            Console.WriteLine();
            return results;
        }

        class ConsoleLogger : ILogger
        {
            public bool IsDebugEnabled { get; set; }
            public bool IsErrorEnabled { get; set; }
            public bool IsInfoEnabled { get; set; }
            public bool IsTraceEnabled { get; set; }
            public bool IsWarnEnabled { get; set; }
            public string Name { get; set; }

            public void Debug(string message)
            {
            }

            public void Debug(Exception exception, string message)
            {
            }

            public void Debug(string format, params object[] args)
            {
            }

            public void Debug(Exception exception, string format, params object[] args)
            {
            }

            public void Debug(IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Debug(Exception exception, IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Error(string message)
            {
            }

            public void Error(Exception exception, string message)
            {
            }

            public void Error(string format, params object[] args)
            {
            }

            public void Error(Exception exception, string format, params object[] args)
            {
            }

            public void Error(IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Error(Exception exception, IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Info(string message)
            {
            }

            public void Info(Exception exception, string message)
            {
            }

            public void Info(string format, params object[] args)
            {
            }

            public void Info(Exception exception, string format, params object[] args)
            {
            }

            public void Info(IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Info(Exception exception, IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Trace(string message)
            {
            }

            public void Trace(Exception exception, string message)
            {
            }

            public void Trace(string format, params object[] args)
            {
            }

            public void Trace(Exception exception, string format, params object[] args)
            {
            }

            public void Trace(IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Trace(Exception exception, IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Warn(string message)
            {
            }

            public void Warn(Exception exception, string message)
            {
            }

            public void Warn(string format, params object[] args)
            {
            }

            public void Warn(Exception exception, string format, params object[] args)
            {
            }

            public void Warn(IFormatProvider provider, string format, params object[] args)
            {
            }

            public void Warn(Exception exception, IFormatProvider provider, string format, params object[] args)
            {
            }
        }

        class EnvironmentResults
        {
            public string Name { get; set; }
            public int Supported { get; set; }
            public int Count { get; set; }
            public List<Rule> NotSupportedNames = new List<Rule>();
        }
    }
}
