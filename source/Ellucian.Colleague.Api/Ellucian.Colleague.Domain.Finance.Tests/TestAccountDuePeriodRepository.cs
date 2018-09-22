// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestAccountDuePeriodRepository
    {
        private static AccountDuePeriod _accountDuePeriod = new AccountDuePeriod();
        public static AccountDuePeriod AccountDuePeriod(string personId)
        {
            GenerateEntities(personId);
            return _accountDuePeriod;
        }

        private static void GenerateEntities(string personId)
        {
            _accountDuePeriod = new Domain.Finance.Entities.AccountDue.AccountDuePeriod()
            {
                Current = new Domain.Finance.Entities.AccountDue.AccountDue()
                {
                    AccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
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
                                    Overdue = true,
                                    PaymentPlanCurrent = false,
                                    PaymentPlanId = "Payment Plan ID1",
                                    UnpaidAmount = 0m,
                                    Period = "Current Period",
                                    PeriodDescription = "Current Period Description",
                                    Term = "Term",
                                    TermDescription = "Term Description",
                                },
                                new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                                {
                                    AccountDescription = "Account Description",
                                    AccountType = "Account Type",
                                    AmountDue = -4000m,
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
                                new Domain.Finance.Entities.AccountDue.InvoiceDueItem()
                                {
                                    AccountDescription = "Account Description",
                                    AccountType = "Account Type",
                                    AmountDue = 4000m,
                                    Description = "Description",
                                    Distribution = "Distribution",
                                    DueDate = DateTime.Today.AddDays(-10),
                                    InvoiceId = "Invoice ID",
                                    Overdue = true,
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
                    },
                    EndDate = DateTime.Today.AddMonths(-2),
                    PersonId = personId,
                    PersonName = "John Smith",
                    StartDate = DateTime.Today.AddMonths(2),
                },
                Past = new Domain.Finance.Entities.AccountDue.AccountDue()
                {
                    AccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
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
                                    DueDate = DateTime.Today.AddMonths(6),
                                    Overdue = true,
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
                                    DueDate = DateTime.Today.AddDays(70),
                                    InvoiceId = "Invoice ID",
                                    Overdue = true,
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
                                    Overdue = false,
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
                    },
                    EndDate = DateTime.Today.AddMonths(-2).AddDays(-1),
                    PersonId = personId,
                    PersonName = "John Smith",
                    StartDate = null
                },
                Future = new Domain.Finance.Entities.AccountDue.AccountDue()
                {
                    AccountTerms = new List<Domain.Finance.Entities.AccountDue.AccountTerm>()
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
                                    Overdue = false,
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
                    },
                    EndDate = null,
                    PersonId = personId,
                    PersonName = "John Smith",
                    StartDate = DateTime.Today.AddMonths(2).AddDays(1),
                },
                PersonName = "John Smith"
            };
        }
    }
}
