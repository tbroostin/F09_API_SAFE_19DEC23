// Copyright 2014 Ellucian Company L.P. and its affiliates.
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
    public class AccountDueEntityAdapterTests
    {
        AccountDue accountDueDto;
        Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue accountDueEntity;
        AccountDueEntityAdapter accountDueEntityAdapter;
        List<Domain.Finance.Entities.AccountDue.AccountTerm> accountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
        {
            new Domain.Finance.Entities.AccountDue.AccountTerm()
            {
                AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                {
                    new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 5000m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(1),
                        Overdue = false,
                        Period = "Period",
                        PeriodDescription = "Period Description",
                        Term = "Term",
                        TermDescription = "Term Description"
                    },
                    new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 4000m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddDays(10),
                        InvoiceId = "Invoice ID",
                        Overdue = false,
                        Period = "Period",
                        PeriodDescription = "Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 250m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(1),
                        Overdue = false,
                        PaymentPlanCurrent = true,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 250m,
                        Period = "Period",
                        PeriodDescription = "Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 150m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(-1),
                        Overdue = true,
                        PaymentPlanCurrent = false,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 100m,
                        Period = "Period",
                        PeriodDescription = "Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 100m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(-2),
                        Overdue = false,
                        PaymentPlanCurrent = false,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 0m,
                        Period = "Period",
                        PeriodDescription = "Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    }
                },
                Amount = 10000m,
                Description = "Account Details Description",
                TermId = "Term"
            }
        };

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();

            accountDueEntityAdapter = new AccountDueEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var accountsReceivableDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem, AccountsReceivableDueItem>(adapterRegistryMock.Object, loggerMock.Object);
            var invoiceDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem, InvoiceDueItem>(adapterRegistryMock.Object, loggerMock.Object);
            var paymentPlanDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem, PaymentPlanDueItem>(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem, AccountsReceivableDueItem>()).Returns(accountsReceivableDueItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem, InvoiceDueItem>()).Returns(invoiceDueItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem, PaymentPlanDueItem>()).Returns(paymentPlanDueItemAdapter);

            accountDueEntity = new Domain.Finance.Entities.AccountDue.AccountDue()
            {
                AccountTerms = accountTerms,
                EndDate = DateTime.Today.AddMonths(-2),
                PersonId = "0001234",
                PersonName = "John Smith",
                StartDate = DateTime.Today.AddMonths(2),
            };

            accountDueDto = accountDueEntityAdapter.MapToType(accountDueEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void AccountDueEntityAdapterTests_AccountTerms()
        {
            Assert.AreEqual(accountDueEntity.AccountTerms.Count, accountDueDto.AccountTerms.Count);
            for (int i = 0; i < accountDueEntity.AccountTerms.Count; i++)
            {
                Assert.AreEqual(accountDueEntity.AccountTerms[i].Amount, accountDueDto.AccountTerms[i].Amount);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].Description, accountDueDto.AccountTerms[i].Description);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].TermId, accountDueDto.AccountTerms[i].TermId);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails.Count, accountDueDto.AccountTerms[i].DepositDueItems.Count +
                    accountDueDto.AccountTerms[i].GeneralItems.Count +
                    accountDueDto.AccountTerms[i].InvoiceItems.Count +
                    accountDueDto.AccountTerms[i].PaymentPlanItems.Count);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].AccountDescription, accountDueDto.AccountTerms[i].GeneralItems[0].AccountDescription);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].AccountType, accountDueDto.AccountTerms[i].GeneralItems[0].AccountType);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].AmountDue, accountDueDto.AccountTerms[i].GeneralItems[0].AmountDue);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].Description, accountDueDto.AccountTerms[i].GeneralItems[0].Description);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].Distribution, accountDueDto.AccountTerms[i].GeneralItems[0].Distribution);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].DueDate, accountDueDto.AccountTerms[i].GeneralItems[0].DueDate);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].Overdue, accountDueDto.AccountTerms[i].GeneralItems[0].Overdue);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].Period, accountDueDto.AccountTerms[i].GeneralItems[0].Period);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].PeriodDescription, accountDueDto.AccountTerms[i].GeneralItems[0].PeriodDescription);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].Term, accountDueDto.AccountTerms[i].GeneralItems[0].Term);
                Assert.AreEqual(accountDueEntity.AccountTerms[i].AccountDetails[0].TermDescription, accountDueDto.AccountTerms[i].GeneralItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).AccountDescription, accountDueDto.AccountTerms[i].InvoiceItems[0].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).AccountType, accountDueDto.AccountTerms[i].InvoiceItems[0].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).AmountDue, accountDueDto.AccountTerms[i].InvoiceItems[0].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).Description, accountDueDto.AccountTerms[i].InvoiceItems[0].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).Distribution, accountDueDto.AccountTerms[i].InvoiceItems[0].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).InvoiceId, accountDueDto.AccountTerms[i].InvoiceItems[0].InvoiceId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).DueDate, accountDueDto.AccountTerms[i].InvoiceItems[0].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).Overdue, accountDueDto.AccountTerms[i].InvoiceItems[0].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).Period, accountDueDto.AccountTerms[i].InvoiceItems[0].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).PeriodDescription, accountDueDto.AccountTerms[i].InvoiceItems[0].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).Term, accountDueDto.AccountTerms[i].InvoiceItems[0].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDueEntity.AccountTerms[i].AccountDetails[1]).TermDescription, accountDueDto.AccountTerms[i].InvoiceItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).AccountDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[0].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).AccountType, accountDueDto.AccountTerms[i].PaymentPlanItems[0].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).AmountDue, accountDueDto.AccountTerms[i].PaymentPlanItems[0].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).Description, accountDueDto.AccountTerms[i].PaymentPlanItems[0].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).Distribution, accountDueDto.AccountTerms[i].PaymentPlanItems[0].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).DueDate, accountDueDto.AccountTerms[i].PaymentPlanItems[0].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).Overdue, accountDueDto.AccountTerms[i].PaymentPlanItems[0].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).PaymentPlanCurrent, accountDueDto.AccountTerms[i].PaymentPlanItems[0].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).PaymentPlanId, accountDueDto.AccountTerms[i].PaymentPlanItems[0].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).Period, accountDueDto.AccountTerms[i].PaymentPlanItems[0].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).PeriodDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[0].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).Term, accountDueDto.AccountTerms[i].PaymentPlanItems[0].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[2]).TermDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).AccountDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[1].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).AccountType, accountDueDto.AccountTerms[i].PaymentPlanItems[1].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).AmountDue, accountDueDto.AccountTerms[i].PaymentPlanItems[1].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).Description, accountDueDto.AccountTerms[i].PaymentPlanItems[1].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).Distribution, accountDueDto.AccountTerms[i].PaymentPlanItems[1].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).DueDate, accountDueDto.AccountTerms[i].PaymentPlanItems[1].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).Overdue, accountDueDto.AccountTerms[i].PaymentPlanItems[1].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).PaymentPlanCurrent, accountDueDto.AccountTerms[i].PaymentPlanItems[1].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).PaymentPlanId, accountDueDto.AccountTerms[i].PaymentPlanItems[1].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).Period, accountDueDto.AccountTerms[i].PaymentPlanItems[1].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).PeriodDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[1].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).Term, accountDueDto.AccountTerms[i].PaymentPlanItems[1].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[3]).TermDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[1].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).AccountDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[2].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).AccountType, accountDueDto.AccountTerms[i].PaymentPlanItems[2].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).AmountDue, accountDueDto.AccountTerms[i].PaymentPlanItems[2].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).Description, accountDueDto.AccountTerms[i].PaymentPlanItems[2].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).Distribution, accountDueDto.AccountTerms[i].PaymentPlanItems[2].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).DueDate, accountDueDto.AccountTerms[i].PaymentPlanItems[2].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).Overdue, accountDueDto.AccountTerms[i].PaymentPlanItems[2].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).PaymentPlanCurrent, accountDueDto.AccountTerms[i].PaymentPlanItems[2].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).PaymentPlanId, accountDueDto.AccountTerms[i].PaymentPlanItems[2].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).Period, accountDueDto.AccountTerms[i].PaymentPlanItems[2].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).PeriodDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[2].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).Term, accountDueDto.AccountTerms[i].PaymentPlanItems[2].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDueEntity.AccountTerms[i].AccountDetails[4]).TermDescription, accountDueDto.AccountTerms[i].PaymentPlanItems[2].TermDescription);
            }
        }

        [TestMethod]
        public void AccountDueEntityAdapterTests_EndDate()
        {
            Assert.AreEqual(accountDueEntity.EndDate, accountDueDto.EndDate);
        }

        [TestMethod]
        public void AccountDueEntityAdapterTests_PersonName()
        {
            Assert.AreEqual(accountDueEntity.PersonName, accountDueDto.PersonName);
        }

        [TestMethod]
        public void AccountDueEntityAdapterTests_StartDate()
        {
            Assert.AreEqual(accountDueEntity.StartDate, accountDueDto.StartDate);
        }

       
    }
}