// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Base.Tests;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.Configuration;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestFinanceConfigurationRepository
    {
        public static FinanceConfiguration TermFinanceConfiguration
        {
            get
            {
                return new FinanceConfiguration()
                {
                    ActivityDisplay = ActivityDisplay.DisplayByTerm,
                    ECommercePaymentsAllowed = true,
                    IncludeDetail = true,
                    IncludeHistory = true,
                    IncludeSchedule = true,
                    InstitutionName = "Ellucian University",
                    NotificationText = "This is a notification.",
                    PartialAccountPaymentsAllowed = true,
                    PartialDepositPaymentsAllowed = true,
                    PartialPlanPaymentsAllowed = PartialPlanPayments.Allowed,
                    PaymentDisplay = PaymentDisplay.DisplayByTerm,
                    PaymentMethods = new List<AvailablePaymentMethod>()
                    {
                        new AvailablePaymentMethod(){Description = "Credit Card", InternalCode = "CC", Type = "Credit Card"},
                        new AvailablePaymentMethod(){Description = "E-Check", InternalCode = "ECHK", Type = "Electronic Check"},
                    },
                    PaymentReviewMessage = "Payment review message.",
                    Periods = TestFinancialPeriodRepository.FinancialPeriods,
                    RemittanceAddress = new List<string>() { "4375 Fair Lakes Court", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = true,
                    ShowCreditAmounts = true,
                    StatementTitle = "Student Statement",
                    SupportEmailAddress = "support@ellucian.edu",
                    UseGuaranteedChecks = true,
                    StatementMessage = (new List<string>(){"Statement Message Line 1", "Statement Message Line 2"}),
                };
            }
        }
        public static FinanceConfiguration PeriodFinanceConfiguration
        {
            get
            {
                return new FinanceConfiguration()
                {
                    ActivityDisplay = ActivityDisplay.DisplayByPeriod,
                    ECommercePaymentsAllowed = false,
                    IncludeDetail = false,
                    IncludeHistory = false,
                    IncludeSchedule = false,
                    InstitutionName = "Ellucian University",
                    NotificationText = "This is a notification.",
                    PartialAccountPaymentsAllowed = false,
                    PartialDepositPaymentsAllowed = false,
                    PartialPlanPaymentsAllowed = PartialPlanPayments.Denied,
                    PaymentDisplay = PaymentDisplay.DisplayByPeriod,
                    PaymentMethods = new List<AvailablePaymentMethod>()
                    {
                        new AvailablePaymentMethod(){Description = "Credit Card", InternalCode = "CC", Type = "Credit Card"},
                        new AvailablePaymentMethod(){Description = "E-Check", InternalCode = "ECHK", Type = "Electronic Check"},
                    },
                    PaymentReviewMessage = "Payment review message.",
                    Periods = TestFinancialPeriodRepository.FinancialPeriods,
                    RemittanceAddress = new List<string>() { "4375 Fair Lakes Court", "Fairfax, VA 22033" },
                    SelfServicePaymentsAllowed = false,
                    ShowCreditAmounts = false,
                    StatementTitle = "Student Statement",
                    SupportEmailAddress = "support@ellucian.edu",
                    UseGuaranteedChecks = false,
                    StatementMessage = (new List<string>() { "Statement Message Line 1", "Statement Message Line 2" }),
                };
            }
        }
    }
}
