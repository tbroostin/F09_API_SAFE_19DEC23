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
    public class AccountDuePeriodEntityAdapterTests
    {
        AccountDuePeriod accountDuePeriodDto;
        Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDuePeriod accountDuePeriodEntity;
        AccountDuePeriodEntityAdapter accountDuePeriodEntityAdapter;
        List<Domain.Finance.Entities.AccountDue.AccountTerm> currentAccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
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
                        Period = "Current Period",
                        PeriodDescription = "Current Period Description",
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
                        Period = "Current Period",
                        PeriodDescription = "Current Period Description",
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
                        Period = "Current Period",
                        PeriodDescription = "Current Period Description",
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
                        Period = "Current Period",
                        PeriodDescription = "Current Period Description",
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
                        Period = "Current Period",
                        PeriodDescription = "Current Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    }
                },
                Amount = 9500m,
                Description = "Account Details Description",
                TermId = "Current Term"
            }
        };
        List<Domain.Finance.Entities.AccountDue.AccountTerm> pastAccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
        {
            new Domain.Finance.Entities.AccountDue.AccountTerm()
            {
                AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                {
                    new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 50000m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(-6),
                        Overdue = false,
                        Period = "Past Period",
                        PeriodDescription = "Past Period Description",
                        Term = "Term",
                        TermDescription = "Term Description"
                    },
                    new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 40000m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddDays(-70),
                        InvoiceId = "Invoice ID",
                        Overdue = false,
                        Period = "Past Period",
                        PeriodDescription = "Past Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 2500m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(-4),
                        Overdue = false,
                        PaymentPlanCurrent = true,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 2500m,
                        Period = "Past Period",
                        PeriodDescription = "Past Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 1500m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(-4),
                        Overdue = true,
                        PaymentPlanCurrent = false,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 1000m,
                        Period = "Past Period",
                        PeriodDescription = "Past Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 1000m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(-4),
                        Overdue = false,
                        PaymentPlanCurrent = false,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 0m,
                        Period = "Past Period",
                        PeriodDescription = "Past Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    }
                },
                Amount = 95000m,
                Description = "Past Account Details Description",
                TermId = "Past Term"
            }
        };
        List<Domain.Finance.Entities.AccountDue.AccountTerm> futureAccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
        {
            new Domain.Finance.Entities.AccountDue.AccountTerm()
            {
                AccountDetails = new List<Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem>()
                {
                    new Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 500m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(6),
                        Overdue = false,
                        Period = "Future Period",
                        PeriodDescription = "Future Period Description",
                        Term = "Term",
                        TermDescription = "Term Description"
                    },
                    new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 400m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddDays(70),
                        InvoiceId = "Invoice ID",
                        Overdue = false,
                        Period = "Future Period",
                        PeriodDescription = "Future Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 25m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(4),
                        Overdue = false,
                        PaymentPlanCurrent = true,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 25m,
                        Period = "Future Period",
                        PeriodDescription = "Future Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 15m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(4),
                        Overdue = true,
                        PaymentPlanCurrent = false,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 10m,
                        Period = "Future Period",
                        PeriodDescription = "Future Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    },
                    new Domain.Finance.Entities.AccountDue.PaymentPlanDueItem()
                    {
                        AccountDescription = "Account Description",
                        AccountType = "Account Type",
                        AmountDue = 10m,
                        Description = "Description",
                        Distribution = "Distribution",
                        DueDate = DateTime.Today.AddMonths(4),
                        Overdue = false,
                        PaymentPlanCurrent = false,
                        PaymentPlanId = "Payment Plan ID1",
                        UnpaidAmount = 0m,
                        Period = "Future Period",
                        PeriodDescription = "Future Period Description",
                        Term = "Term",
                        TermDescription = "Term Description",
                    }
                },
                Amount = 950m,
                Description = "Future Account Details Description",
                TermId = "Future Term"
            }
        };

        [TestInitialize]
        public void Initialize()
        {
            var loggerMock = new Mock<ILogger>();
            var adapterRegistryMock = new Mock<IAdapterRegistry>();
            
            accountDuePeriodEntityAdapter = new AccountDuePeriodEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);
            var accountDueEntityAdapter = new AccountDueEntityAdapter(adapterRegistryMock.Object, loggerMock.Object);

            var accountsReceivableDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem, AccountsReceivableDueItem>(adapterRegistryMock.Object, loggerMock.Object);
            var invoiceDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem, InvoiceDueItem>(adapterRegistryMock.Object, loggerMock.Object);
            var paymentPlanDueItemAdapter = new AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem, PaymentPlanDueItem>(adapterRegistryMock.Object, loggerMock.Object);

            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue, AccountDue>()).Returns(accountDueEntityAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountsReceivableDueItem, AccountsReceivableDueItem>()).Returns(accountsReceivableDueItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.InvoiceDueItem, InvoiceDueItem>()).Returns(invoiceDueItemAdapter);
            adapterRegistryMock.Setup(reg => reg.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.PaymentPlanDueItem, PaymentPlanDueItem>()).Returns(paymentPlanDueItemAdapter);

            var currentPeriodEntity = new Domain.Finance.Entities.AccountDue.AccountDue()
            {
                AccountTerms = currentAccountTerms,
                EndDate = DateTime.Today.AddMonths(-2),
                PersonId = "0001234",
                PersonName = "John Smith",
                StartDate = DateTime.Today.AddMonths(2),
            };

            var pastPeriodEntity = new Domain.Finance.Entities.AccountDue.AccountDue()
            {
                AccountTerms = pastAccountTerms,
                EndDate = DateTime.Today.AddMonths(-2).AddDays(-1),
                PersonId = "0001234",
                PersonName = "John Smith",
                StartDate = null
            };

            var futurePeriodEntity = new Domain.Finance.Entities.AccountDue.AccountDue()
            {
                AccountTerms = futureAccountTerms,
                EndDate = null,
                PersonId = "0001234",
                PersonName = "John Smith",
                StartDate = DateTime.Today.AddMonths(2).AddDays(1),
            };

            accountDuePeriodEntity = new Domain.Finance.Entities.AccountDue.AccountDuePeriod()
            {
                Current = currentPeriodEntity,
                Future = futurePeriodEntity,
                Past = pastPeriodEntity,
                PersonName = "John Smith"
            };

            accountDuePeriodDto = accountDuePeriodEntityAdapter.MapToType(accountDuePeriodEntity);
        }

        [TestCleanup]
        public void Cleanup()
        {

        }

        [TestMethod]
        public void AccountDuePeriodEntityAdapterTests_PersonName()
        {
            Assert.AreEqual(accountDuePeriodEntity.PersonName, accountDuePeriodDto.PersonName);
        }

        [TestMethod]
        public void AccountDuePeriodEntityAdapterTests_Past()
        {
            Assert.AreEqual(accountDuePeriodEntity.Past.EndDate, accountDuePeriodDto.Past.EndDate);
            Assert.AreEqual(accountDuePeriodEntity.Past.PersonName, accountDuePeriodDto.Past.PersonName);
            Assert.AreEqual(accountDuePeriodEntity.Past.StartDate, accountDuePeriodDto.Past.StartDate);
            Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms.Count, accountDuePeriodDto.Past.AccountTerms.Count);
            for (int i = 0; i < accountDuePeriodEntity.Past.AccountTerms.Count; i++)
            {
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].Amount, accountDuePeriodDto.Past.AccountTerms[i].Amount);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].Description, accountDuePeriodDto.Past.AccountTerms[i].Description);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].TermId, accountDuePeriodDto.Past.AccountTerms[i].TermId);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails.Count, accountDuePeriodDto.Past.AccountTerms[i].DepositDueItems.Count +
                    accountDuePeriodDto.Past.AccountTerms[i].GeneralItems.Count +
                    accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems.Count +
                    accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems.Count);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].AccountDescription, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].AccountDescription);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].AccountType, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].AccountType);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].AmountDue, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].AmountDue);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].Description, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].Description);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].Distribution, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].Distribution);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].DueDate, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].DueDate);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].Overdue, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].Overdue);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].Period, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].Period);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].PeriodDescription, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].PeriodDescription);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].Term, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].Term);
                Assert.AreEqual(accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[0].TermDescription, accountDuePeriodDto.Past.AccountTerms[i].GeneralItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).AccountDescription, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).AccountType, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).AmountDue, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).Description, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).Distribution, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).InvoiceId, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].InvoiceId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).DueDate, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).Overdue, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).Period, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).PeriodDescription, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).Term, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[1]).TermDescription, accountDuePeriodDto.Past.AccountTerms[i].InvoiceItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).AccountDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).AccountType, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).AmountDue, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).Description, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).Distribution, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).DueDate, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).Overdue, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).PaymentPlanCurrent, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).PaymentPlanId, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).Period, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).PeriodDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).Term, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[2]).TermDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).AccountDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).AccountType, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).AmountDue, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).Description, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).Distribution, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).DueDate, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).Overdue, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).PaymentPlanCurrent, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).PaymentPlanId, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).Period, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).PeriodDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).Term, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[3]).TermDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[1].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).AccountDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).AccountType, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).AmountDue, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).Description, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).Distribution, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).DueDate, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).Overdue, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).PaymentPlanCurrent, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).PaymentPlanId, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).Period, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).PeriodDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).Term, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Past.AccountTerms[i].AccountDetails[4]).TermDescription, accountDuePeriodDto.Past.AccountTerms[i].PaymentPlanItems[2].TermDescription);
            }
        }

        [TestMethod]
        public void AccountDuePeriodEntityAdapterTests_Current()
        {
            Assert.AreEqual(accountDuePeriodEntity.Current.EndDate, accountDuePeriodDto.Current.EndDate);
            Assert.AreEqual(accountDuePeriodEntity.Current.PersonName, accountDuePeriodDto.Current.PersonName);
            Assert.AreEqual(accountDuePeriodEntity.Current.StartDate, accountDuePeriodDto.Current.StartDate);
            Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms.Count, accountDuePeriodDto.Current.AccountTerms.Count);
            for (int i = 0; i < accountDuePeriodEntity.Current.AccountTerms.Count; i++)
            {
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].Amount, accountDuePeriodDto.Current.AccountTerms[i].Amount);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].Description, accountDuePeriodDto.Current.AccountTerms[i].Description);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].TermId, accountDuePeriodDto.Current.AccountTerms[i].TermId);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails.Count, accountDuePeriodDto.Current.AccountTerms[i].DepositDueItems.Count +
                    accountDuePeriodDto.Current.AccountTerms[i].GeneralItems.Count +
                    accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems.Count +
                    accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems.Count);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].AccountDescription, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].AccountDescription);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].AccountType, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].AccountType);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].AmountDue, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].AmountDue);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].Description, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].Description);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].Distribution, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].Distribution);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].DueDate, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].DueDate);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].Overdue, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].Overdue);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].Period, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].Period);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].PeriodDescription, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].PeriodDescription);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].Term, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].Term);
                Assert.AreEqual(accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[0].TermDescription, accountDuePeriodDto.Current.AccountTerms[i].GeneralItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).AccountDescription, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).AccountType, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).AmountDue, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).Description, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).Distribution, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).InvoiceId, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].InvoiceId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).DueDate, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).Overdue, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).Period, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).PeriodDescription, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).Term, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[1]).TermDescription, accountDuePeriodDto.Current.AccountTerms[i].InvoiceItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).AccountDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).AccountType, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).AmountDue, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).Description, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).Distribution, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).DueDate, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).Overdue, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).PaymentPlanCurrent, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).PaymentPlanId, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).Period, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).PeriodDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).Term, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[2]).TermDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).AccountDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).AccountType, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).AmountDue, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).Description, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).Distribution, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).DueDate, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).Overdue, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).PaymentPlanCurrent, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).PaymentPlanId, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).Period, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).PeriodDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).Term, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[3]).TermDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[1].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).AccountDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).AccountType, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).AmountDue, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).Description, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).Distribution, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).DueDate, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).Overdue, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).PaymentPlanCurrent, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).PaymentPlanId, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).Period, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).PeriodDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).Term, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Current.AccountTerms[i].AccountDetails[4]).TermDescription, accountDuePeriodDto.Current.AccountTerms[i].PaymentPlanItems[2].TermDescription);

            }
        }

        [TestMethod]
        public void AccountDuePeriodEntityAdapterTests_Future()
        {
            Assert.AreEqual(accountDuePeriodEntity.Future.EndDate, accountDuePeriodDto.Future.EndDate);
            Assert.AreEqual(accountDuePeriodEntity.Future.PersonName, accountDuePeriodDto.Future.PersonName);
            Assert.AreEqual(accountDuePeriodEntity.Future.StartDate, accountDuePeriodDto.Future.StartDate);
            Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms.Count, accountDuePeriodDto.Future.AccountTerms.Count);
            for (int i = 0; i < accountDuePeriodEntity.Future.AccountTerms.Count; i++)
            {
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].Amount, accountDuePeriodDto.Future.AccountTerms[i].Amount);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].Description, accountDuePeriodDto.Future.AccountTerms[i].Description);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].TermId, accountDuePeriodDto.Future.AccountTerms[i].TermId);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails.Count, accountDuePeriodDto.Future.AccountTerms[i].DepositDueItems.Count +
                    accountDuePeriodDto.Future.AccountTerms[i].GeneralItems.Count +
                    accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems.Count +
                    accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems.Count);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].AccountDescription, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].AccountDescription);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].AccountType, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].AccountType);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].AmountDue, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].AmountDue);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].Description, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].Description);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].Distribution, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].Distribution);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].DueDate, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].DueDate);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].Overdue, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].Overdue);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].Period, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].Period);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].PeriodDescription, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].PeriodDescription);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].Term, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].Term);
                Assert.AreEqual(accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[0].TermDescription, accountDuePeriodDto.Future.AccountTerms[i].GeneralItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).AccountDescription, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).AccountType, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).AmountDue, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).Description, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).Distribution, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).InvoiceId, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].InvoiceId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).DueDate, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).Overdue, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).Period, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).PeriodDescription, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).Term, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.InvoiceDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[1]).TermDescription, accountDuePeriodDto.Future.AccountTerms[i].InvoiceItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).AccountDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).AccountType, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).AmountDue, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).Description, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).Distribution, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).DueDate, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).Overdue, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).PaymentPlanCurrent, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).PaymentPlanId, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).Period, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).PeriodDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).Term, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[2]).TermDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[0].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).AccountDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).AccountType, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).AmountDue, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).Description, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).Distribution, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).DueDate, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).Overdue, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).PaymentPlanCurrent, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).PaymentPlanId, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).Period, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).PeriodDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).Term, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[3]).TermDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[1].TermDescription);

                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).AccountDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].AccountDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).AccountType, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].AccountType);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).AmountDue, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].AmountDue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).Description, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].Description);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).Distribution, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].Distribution);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).DueDate, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].DueDate);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).Overdue, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].Overdue);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).PaymentPlanCurrent, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].PaymentPlanCurrent);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).PaymentPlanId, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].PaymentPlanId);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).Period, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].Period);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).PeriodDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].PeriodDescription);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).Term, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].Term);
                Assert.AreEqual(((Domain.Finance.Entities.AccountDue.PaymentPlanDueItem)accountDuePeriodEntity.Future.AccountTerms[i].AccountDetails[4]).TermDescription, accountDuePeriodDto.Future.AccountTerms[i].PaymentPlanItems[2].TermDescription);
            }
        }
    }
}