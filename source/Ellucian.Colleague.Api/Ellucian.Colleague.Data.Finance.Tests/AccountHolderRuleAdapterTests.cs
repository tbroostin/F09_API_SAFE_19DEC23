// Copyright 2015-2016 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Data.Finance;
using System;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    [TestClass]
    public class AccountHolderRuleAdapterTests
    {
        public AccountHolder context;

        public AccountHolderRuleAdapter ruleAdapter;

        public RuleDescriptor descriptor;

        public string studentId;
        public string lastName;
        public string privacyStatusCode;

        [TestInitialize]
        public void Initialize()
        {
            studentId = "0003914";
            lastName = "Smith";
            privacyStatusCode = null;

            context = new AccountHolder(studentId, lastName, privacyStatusCode)
            {
                BirthDate = DateTime.Today.AddYears(-19),
                EthnicCodes = new List<string>() { "LAT" },
                Ethnicities = new List<EthnicOrigin>() { EthnicOrigin.HispanicOrLatino },
                FirstName = "Kevin",
                Gender = "M",
                GovernmentId = "123456789",
                Guid = "e54dc41c-63cf-4b59-8852-7548cdf17ea2",
                MaritalStatus = MaritalState.Single,
                MaritalStatusCode = "S",
                MiddleName = "Patrick",
                Nickname = "Dusty Smith",
                PreferredAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                PreferredName = "Kevin Smith",
                Prefix = "Mr.",
                RaceCodes = new List<string>() { "RACE" },
                Suffix = "III"
            };
            context.AddDepositDue(new DepositDue("123", studentId, 500m, "MEALS", DateTime.Today.AddMonths(2)));
            context.AddEmailAddress(new EmailAddress("kevin.smith@ellucianuniversity.edu", "PER"));
            context.AddPersonAlt(new PersonAlt("0003915", "ALT"));

            //rule descriptor called FOO that checks for non-Male account holders
            descriptor = new RuleDescriptor()
            {
                Id = "FOO",
                PrimaryView = "PERSON.AR",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "GENDER", "NE", "M")
                }
            };

            ruleAdapter = new AccountHolderRuleAdapter();
        }

        [TestMethod]
        public void GetRecordIdTest()
        {
            Assert.AreEqual(studentId, ruleAdapter.GetRecordId(context));
        }

        [TestMethod]
        public void ContextTypeTest()
        {
            Assert.AreEqual(typeof(AccountHolder), ruleAdapter.ContextType);
        }

        [TestMethod]
        public void ExpectedPrimaryViewTest()
        {
            Assert.AreEqual("PERSON.AR", ruleAdapter.ExpectedPrimaryView);
        }

        [TestMethod]
        public void NoExpressionForRuleTest()
        {
            descriptor.Expressions = new List<RuleExpressionDescriptor>();
            var rule = ruleAdapter.Create(descriptor) as Rule<AccountHolder>;
            Assert.IsFalse(rule.HasExpression);
        }

        [TestMethod]
        public void PersonIdTest()
        {
            descriptor = new RuleDescriptor()
            {
                Id = "0001234",
                PrimaryView = "PERSON.AR",
                Expressions = new List<RuleExpressionDescriptor>()
                {
                    new RuleExpressionDescriptor("WITH", "PERSON.ID", "EQ", "\""+context.Id+"\"")
                }
            };
            var rule = ruleAdapter.Create(descriptor) as Rule<AccountHolder>;
            Assert.IsTrue(rule.HasExpression);
            Assert.IsTrue(rule.Passes(context));
        }

        [TestMethod]
        public void UnsupportedFieldForRuleTest()
        {
            descriptor.Expressions.Add(new RuleExpressionDescriptor("WITH", new RuleDataElement() { Id = "FOOBAR" }, "NE", "\"\""));
            var rule = ruleAdapter.Create(descriptor) as Rule<AccountHolder>;
            Assert.IsFalse(rule.HasExpression);
        }
    }
}
