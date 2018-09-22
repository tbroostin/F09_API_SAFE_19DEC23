// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests.Entities
{
    [TestClass]
    public class AccountHolderTests
    {
        private string id;
        private string lastName;
        private string privacyCode;

        [TestInitialize]
        public void AccountHolder_Initialize()
        {
            id = "0003315";
            lastName = "Smith";
            privacyCode = "X";
        }

        [TestClass]
        public class AccountHolder_ConstructorTests : AccountHolderTests
        {
            [TestMethod]
            public void AccountHolder_Constructor_ValidId()
            {
                var result = new AccountHolder(id, lastName);
                Assert.AreEqual(id, result.Id);
            }

            [TestMethod]
            public void AccountHolder_Constructor_DefaultPreferredAddress()
            {
                var result = new AccountHolder(id, lastName);
                Assert.AreEqual(0, result.PreferredAddress.Count);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountHolder_Constructor_NullLastName()
            {
                var result = new AccountHolder(id, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountHolder_Constructor_EmptyLastName()
            {
                var result = new AccountHolder(id, string.Empty);
            }

            [TestMethod]
            public void AccountHolder_Constructor_PrivacyStatusCode_Default()
            {
                var result = new AccountHolder(id, lastName);
                Assert.IsNull(result.PrivacyStatusCode);
            }

            [TestMethod]
            public void AccountHolder_Constructor_PrivacyStatusCode_HasValue()
            {
                var result = new AccountHolder(id, lastName, privacyCode);
                Assert.AreEqual(privacyCode, result.PrivacyStatusCode);
            }
        }

        [TestClass]
        public class AccountHolder_AddDepositDueTests : AccountHolderTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void AccountHolder_AddDepositDue_NullDepositDue()
            {
                var accountHolder = new AccountHolder(id, lastName);
                accountHolder.AddDepositDue(null);
            }

            [TestMethod]
            public void AccountHolder_AddDepositDue_ValidDepositDue()
            {
                var depositDue = new DepositDue("123", "0003315", 200, "MEALS", DateTime.Today.AddDays(14));
                var accountHolder = new AccountHolder(id, lastName);
                accountHolder.AddDepositDue(depositDue);
                Assert.AreEqual(depositDue, accountHolder.DepositsDue.First());
            }

            [TestMethod]
            public void AccountHolder_AddDepositDue_DuplicateDepositDue()
            {
                var depositDue = new DepositDue("123", "0003315", 200, "MEALS", DateTime.Today.AddDays(14));
                var accountHolder = new AccountHolder(id, lastName);
                accountHolder.AddDepositDue(depositDue);
                accountHolder.AddDepositDue(depositDue);
                Assert.AreEqual(1, accountHolder.DepositsDue.Count);
            }
        }

        [TestClass]
        public class AccountHolder_AddDepositsDueTests : AccountHolderTests
        {
            [TestMethod]
            public void AccountHolder_AddDepositsDue_NullCollection()
            {
                var accountHolder = new AccountHolder(id, lastName);
                accountHolder.AddDepositsDue(null);
                Assert.AreEqual(0, accountHolder.DepositsDue.Count);
            }

            [TestMethod]
            public void AccountHolder_AddDepositsDue_EmptyCollection()
            {
                var accountHolder = new AccountHolder(id, lastName);
                accountHolder.AddDepositsDue(new List<DepositDue>());
                Assert.AreEqual(0, accountHolder.DepositsDue.Count);
            }

            [TestMethod]
            public void AccountHolder_AddDepositsDue_ValidCollection()
            {
                var depositDue1 = new DepositDue("123", "0003315", 200, "MEALS", DateTime.Today.AddDays(14));
                var depositDue2 = new DepositDue("124", "0003315", 300, "RESHL", DateTime.Today.AddDays(30));
                var accountHolder = new AccountHolder(id, lastName);
                accountHolder.AddDepositsDue(new List<DepositDue>() { depositDue1, depositDue2 });
                Assert.AreEqual(2, accountHolder.DepositsDue.Count);
                Assert.AreEqual(depositDue1, accountHolder.DepositsDue[0]);
                Assert.AreEqual(depositDue2, accountHolder.DepositsDue[1]);
            }
        }
    }
}