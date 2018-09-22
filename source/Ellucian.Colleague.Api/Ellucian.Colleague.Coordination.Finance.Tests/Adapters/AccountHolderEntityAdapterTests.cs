// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Colleague.Coordination.Finance.Adapters;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Colleague.Dtos.Finance.AccountDue;
using Ellucian.Web.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Tests.Adapters
{
    [TestClass]
    public class AccountHolderEntityAdapterTests
    {
        AccountHolder accountHolderDto;
        Ellucian.Colleague.Domain.Finance.Entities.AccountHolder accountHolderEntity;
        AccountHolderEntityAdapter accountHolderEntityAdapter;
        string id = "0001234";
        string lastName = "Lastname";

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            accountHolderEntityAdapter = new AccountHolderEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            
            var depositDueEntityAdapter = new DepositDueEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.DepositDue, DepositDue>()).Returns(depositDueEntityAdapter);

            accountHolderEntity = new Domain.Finance.Entities.AccountHolder(id, lastName, null)
            {
                BirthDate = DateTime.Today.AddYears(-18),
                DeceasedDate = null,
                EthnicCodes = new List<string>() { "NHS" },
                Ethnicities = new List<Domain.Base.Entities.EthnicOrigin>() { Domain.Base.Entities.EthnicOrigin.White },
                FirstName = "Firstname",
                Gender = "M",
                GovernmentId = "123-45-6789",
                Guid = "6c1091e2-4e54-4dbb-8b3c-6d186ae71d59",
                MaritalStatus = Domain.Base.Entities.MaritalState.Single,
                MaritalStatusCode = "S",
                MiddleName = "Middlename",
                Nickname = "Nickname",
                PreferredAddress = new List<string>() { "123 Main Street", "Fairfax, VA 22033" },
                PreferredName = "Nickname Lastname",
                Prefix = "Mr.",
                RaceCodes = new List<string>() { "WH" },
                Suffix = "III"
            };
            var depositDue = new Domain.Finance.Entities.DepositDue("234", "0001234", 500m, "MEALS", DateTime.Today.AddDays(7)) { TermId = "TermId" };
            depositDue.AddDeposit(new Domain.Finance.Entities.Deposit("123", "0001234", DateTime.Today.AddDays(-7), "MEALS", 300m) { TermId = "TermId" });
            accountHolderEntity.AddDepositDue(depositDue);
            accountHolderEntity.AddEmailAddress(new Domain.Base.Entities.EmailAddress("firstname.lastname@ellucian.edu", "PRI") { IsPreferred = true });
            accountHolderEntity.AddPersonAlt(new Domain.Base.Entities.PersonAlt("0001235", "ALT"));
            accountHolderDto = accountHolderEntityAdapter.MapToType(accountHolderEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_BirthDate()
        {
            Assert.AreEqual(accountHolderEntity.BirthDate, accountHolderDto.BirthDate);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_DepositsDue()
        {
            List<DepositDue> actual = new List<DepositDue>(accountHolderDto.DepositsDue);
            List<Deposit> actualDeposits;
            
            Assert.AreEqual(accountHolderEntity.DepositsDue.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].Amount, actual[i].Amount);
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].Balance, actual[i].Balance);
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].DepositType, actual[i].DepositType);
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].DueDate, actual[i].DueDate);
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].Id, actual[i].Id);
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].Overdue, actual[i].Overdue);
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].PersonId, actual[i].PersonId);
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].TermId, actual[i].TermId);

                actualDeposits = new List<Deposit>(actual[i].Deposits);
                Assert.AreEqual(accountHolderEntity.DepositsDue[i].Deposits.Count, actualDeposits.Count);
                for (int j = 0; j < actualDeposits.Count; j++) 
                {
                    Assert.AreEqual(accountHolderEntity.DepositsDue[i].Deposits[j].Amount, actualDeposits[j].Amount);
                    Assert.AreEqual(accountHolderEntity.DepositsDue[i].Deposits[j].Date, actualDeposits[j].Date);
                    Assert.AreEqual(accountHolderEntity.DepositsDue[i].Deposits[j].DepositType, actualDeposits[j].DepositType);
                    Assert.AreEqual(accountHolderEntity.DepositsDue[i].Deposits[j].Id, actualDeposits[j].Id);
                    Assert.AreEqual(accountHolderEntity.DepositsDue[i].Deposits[j].PersonId, actualDeposits[j].PersonId);
                }
            }
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_EthnicCodes()
        {
            CollectionAssert.AreEqual(accountHolderEntity.EthnicCodes, accountHolderDto.EthnicCodes);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_Ethnicities()
        {
            Assert.AreEqual(accountHolderEntity.Ethnicities.Count, accountHolderDto.Ethnicities.Count);
            for (int i = 0; i < accountHolderDto.Ethnicities.Count; i++)
            {
                Assert.AreEqual(accountHolderEntity.Ethnicities[i].ToString(), accountHolderDto.Ethnicities[i].ToString());
            }
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_FirstName()
        {
            Assert.AreEqual(accountHolderEntity.FirstName, accountHolderDto.FirstName);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_Genderd()
        {
            Assert.AreEqual(accountHolderEntity.Gender, accountHolderDto.Gender);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_GovernmentId()
        {
            Assert.AreEqual(accountHolderEntity.GovernmentId, accountHolderDto.GovernmentId);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_Id()
        {
            Assert.AreEqual(accountHolderEntity.Id, accountHolderDto.Id);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_LastName()
        {
            Assert.AreEqual(accountHolderEntity.LastName, accountHolderDto.LastName);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_MaritalStatus()
        {
            Assert.AreEqual(accountHolderEntity.MaritalStatus.ToString(), accountHolderDto.MaritalStatus.ToString());
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_MiddleName()
        {
            Assert.AreEqual(accountHolderEntity.MiddleName, accountHolderDto.MiddleName);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_PreferredAddress()
        {
            List<string> expected = new List<string>(accountHolderEntity.PreferredAddress);
            List<string> actual = new List<string>(accountHolderDto.PreferredAddress);
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_PreferredName()
        {
            Assert.AreEqual(accountHolderEntity.PreferredName, accountHolderDto.PreferredName);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_Prefix()
        {
            Assert.AreEqual(accountHolderEntity.Prefix, accountHolderDto.Prefix);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_RaceCodes()
        {
            CollectionAssert.AreEqual(accountHolderEntity.RaceCodes, accountHolderDto.RaceCodes);
        }

        [TestMethod]
        public void AccountHolderEntityAdapterTests_Suffix()
        {
            Assert.AreEqual(accountHolderEntity.Suffix, accountHolderDto.Suffix);
        }
    }
}