// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
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
    public class DepositDueEntityAdapterTests
    {
        DepositDue depositDueDto;
        Ellucian.Colleague.Domain.Finance.Entities.DepositDue depositDueEntity;
        DepositDueEntityAdapter depositDueEntityAdapter;

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            depositDueEntityAdapter = new DepositDueEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var depositEntityAdapter = new AutoMapperAdapter<Domain.Finance.Entities.Deposit, Deposit>(adapterRegistryMock.Object, loggerMock.Object);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.Deposit, Deposit>()).Returns(depositEntityAdapter);

            depositDueEntity = new Domain.Finance.Entities.DepositDue("123", "0001234", 500m, "MEALS", DateTime.Today.AddDays(7))
            {
                TermId = "TermId"
            };
            depositDueEntity.AddDeposit(new Domain.Finance.Entities.Deposit("123", "0001234", DateTime.Today.AddDays(-7), "MEALS", 300m) { TermId = "TermId" });

            depositDueDto = depositDueEntityAdapter.MapToType(depositDueEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_Amount()
        {
            Assert.AreEqual(depositDueEntity.Amount, depositDueDto.Amount);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_AmountPaid()
        {
            Assert.AreEqual(depositDueEntity.AmountPaid, depositDueDto.AmountPaid);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_Balance()
        {
            Assert.AreEqual(depositDueEntity.Balance, depositDueDto.Balance);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_Deposits()
        {
            List<Deposit> actualDeposits = new List<Deposit>(depositDueDto.Deposits);
            Assert.AreEqual(depositDueEntity.Deposits.Count, actualDeposits.Count);
            for (int i = 0; i < actualDeposits.Count; i++)
            {
                Assert.AreEqual(depositDueEntity.Deposits[i].Amount, actualDeposits[i].Amount);
                Assert.AreEqual(depositDueEntity.Deposits[i].Date, actualDeposits[i].Date);
                Assert.AreEqual(depositDueEntity.Deposits[i].DepositType, actualDeposits[i].DepositType);
                Assert.AreEqual(depositDueEntity.Deposits[i].Id, actualDeposits[i].Id);
                Assert.AreEqual(depositDueEntity.Deposits[i].PersonId, actualDeposits[i].PersonId);
            }
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_DepositType()
        {
            Assert.AreEqual(depositDueEntity.DepositType, depositDueDto.DepositType);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_DepositTypeDescription()
        {
            Assert.AreEqual(depositDueEntity.DepositTypeDescription, depositDueDto.DepositTypeDescription);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_DueDate()
        {
            Assert.AreEqual(depositDueEntity.DueDate, depositDueDto.DueDate);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_Id()
        {
            Assert.AreEqual(depositDueEntity.Id, depositDueDto.Id);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_Overdue()
        {
            Assert.AreEqual(depositDueEntity.Overdue, depositDueDto.Overdue);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_PersonId()
        {
            Assert.AreEqual(depositDueEntity.PersonId, depositDueDto.PersonId);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_SortOrder()
        {
            Assert.AreEqual(depositDueEntity.SortOrder, depositDueDto.SortOrder);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_TermId()
        {
            Assert.AreEqual(depositDueEntity.TermId, depositDueDto.TermId);
        }

        [TestMethod]
        public void DepositDueEntityAdapterTests_TermDescription()
        {
            Assert.AreEqual(depositDueEntity.TermDescription, depositDueDto.TermDescription);
        }
    }
}