// Copyright 2012-2023 Ellucian Company L.P. and its affiliates.
using System;
using System.Diagnostics;
using System.Linq;
using Ellucian.Colleague.Configuration;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Dmi.Client;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.TestUtil;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Security;
using Microsoft.IdentityModel.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using slf4net;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Student.Tests.Repositories
{
    [TestClass]
    public class UnitTest1
    {
        /// This harness can be used to test against live environments.  
        /// 
        /// 1. DO NOT CHECK THIS FILE IN
        ///  1a. If you must check this file in, make sure any system information is blanked before delivery.
        ///  1b. Make sure the Ignore keyword is not commented out on checkin.  These tests should not be allowed
        ///      to run as part of a "run all tests in solution" because actually hitting an environment for data is 
        ///      slower than test data.
        /// </summary>
        /// 

        private ApiSettings apiSettingsMock;

        [TestMethod]
        [Ignore]
        public async Task SectionRepositoryRegistrationTestAgainstEnvironment()
        {
            var settings = new DmiSettings();
            settings.AccountName = "test_account";
            settings.IpAddress = "yourserver.yourschool.com";
            settings.Port = 9999;
            settings.SharedSecret = "sharedsecret123";
            var token = new ColleagueSessionRepository(settings, new MemoryCacheProvider()).LoginAsync("student_userid", "student_password");

            var principal = JwtHelper.CreatePrincipal(token.Result);
            var sessionClaim = (principal as IClaimsPrincipal).Identities.First().Claims.FirstOrDefault(c => c.ClaimType == "sid");
            var securityToken = sessionClaim.Value.Split('*')[0];
            var controlId = sessionClaim.Value.Split('*')[1];
            var session = new StandardDmiSession() { SecurityToken = securityToken, SenderControlId = controlId };

            var cacheProvider = new MemoryCacheProvider();
            var logger = new ConsoleLogger() { IsDebugEnabled = true, IsErrorEnabled = true, IsInfoEnabled = true, IsTraceEnabled = true, IsWarnEnabled = true };
            var txFactory = new TestTransactionFactory(session, logger, settings);
            apiSettingsMock = new ApiSettings("null");

            var regTerms = await new TermRepository(cacheProvider, txFactory, logger).GetRegistrationTermsAsync();
            var sectionRepo = new SectionRepository(cacheProvider, txFactory, logger, new Ellucian.Colleague.Domain.Student.Tests.TestFacultyRepository(), new Ellucian.Colleague.Domain.Student.Tests.TestTermRepository(), apiSettingsMock);
            var sw = new Stopwatch();
            sw.Start();
            var count = (await sectionRepo.GetRegistrationSectionsAsync(regTerms)).Count();
            sw.Stop();
            Console.WriteLine("Read " + count + " registration sections in " + sw.ElapsedMilliseconds + "ms");

            new ColleagueSessionRepository(settings, new MemoryCacheProvider()).LogoutAsync(token.Result);
        }
    }

    public class ConsoleLogger : ILogger
    {
        public bool IsDebugEnabled { get; set; }
        public bool IsErrorEnabled { get; set; }
        public bool IsInfoEnabled { get; set; }
        public bool IsTraceEnabled { get; set; }
        public bool IsWarnEnabled { get; set; }
        public string Name { get; set; }


        public void Debug(string message)
        {
            if (IsDebugEnabled)
            {
                Console.WriteLine(message);
            }
        }

        public void Debug(Exception exception, string message)
        {
            if (IsDebugEnabled)
            {
                Console.WriteLine(exception);
            }
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
}
