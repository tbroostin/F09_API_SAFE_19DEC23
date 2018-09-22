// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Entities.AccountDue;
using Ellucian.Colleague.Domain.Finance.Entities.Payments;
using Ellucian.Colleague.Domain.Finance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using slf4net;

namespace Ellucian.Colleague.Domain.Finance.Tests.Services
{
    [TestClass]
    public class ImmediatePaymentServiceTests : BaseRepositorySetup
    {
        ImmediatePaymentService service;

        static RegistrationPaymentControl paymentControl;
        static Charge charge1;
        static Charge charge2;
        static List<ChargeCode> chargeCodes;
        static List<Charge> charges;
        static Invoice invoice1;
        static List<Invoice> invoices;

        static ReceiptPayment payment1;
        static List<ReceivablePayment> payments;

        static AccountTerm term1;
        static AccountTerm term1ZeroBalance;
        static AccountTerm term1LessThanRegBalance;

        static List<AccountTerm> terms;
        static List<AccountTerm> terms2;
        static List<AccountTerm> terms3;

        static Dictionary<string, string> distributionMap;

        static PaymentDeferralOption deferOption1;
        static PaymentDeferralOption deferOption2;
        static PaymentDeferralOption deferOption3;
        static PaymentDeferralOption deferOption4;
        static List<PaymentDeferralOption> deferOptions;

        static PaymentPlanOption planOption1;
        static List<PaymentPlanOption> planOptions;
        static PaymentPlanOption planOption2;
        static List<PaymentPlanOption> planOptions2;

        static PaymentRequirement payReq;
        static PaymentRequirement payReq2;
        static PaymentRequirement payReqNullPlans;
        static PaymentRequirement payReqEmptyPlans;

        static List<string> receivableTypeStrings;
        static List<ReceivableType> receivableTypes;

        static Dictionary<string, PaymentConfirmation> confirmationMap;

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();
            loggerMock = new Mock<ILogger>();

            paymentControl = new RegistrationPaymentControl("123", "0003315", "2014/FA", RegistrationPaymentStatus.New);
            charge1 = new Charge("124", "126", new List<string>() { "HIST-100-01 Tuition"}, "TUIPT", 3750m);
            charge2 = new Charge("125", "126", new List<string>() { "2014/FA Technology Fee"}, "TECFE", 250m);
            chargeCodes = new List<ChargeCode>() { new ChargeCode("TUIPT", "Part-Time Tuition", 1), new ChargeCode("TECFE", "Technology Fee", 50) };
            charges = new List<Charge>() { charge1, charge2 };
            invoice1 = new Invoice("126", "0003315", "01", "2014/FA", "124REF", DateTime.Today.AddMonths(-1),
            DateTime.Today.AddMonths(1), DateTime.Today.AddMonths(-2), DateTime.Today.AddMonths(2), "Registration - 2014/FA",
            charges);
            invoices = new List<Invoice>() { invoice1 };
            paymentControl.AddInvoice(invoice1.Id);

            payment1 = new ReceiptPayment("127", "0003315", "01", "2014/FA", "127REF", DateTime.Today.AddDays(-3), 4000m, "128");
            payments = new List<ReceivablePayment>() { payment1 };

            term1 = new AccountTerm() { TermId = "2014/FA", Description = "2014 Fall Term", Amount = 4000m };
            term1ZeroBalance = new AccountTerm() { TermId = "2014/FA", Description = "2014 Fall Term", Amount = 0m, AccountDetails = new List<AccountsReceivableDueItem>() { new AccountsReceivableDueItem() { AccountType = "01", AmountDue = 0m } } };
            term1LessThanRegBalance = new AccountTerm() { TermId = "2014/FA", Description = "2014 Fall Term", Amount = 0m, AccountDetails = new List<AccountsReceivableDueItem>() { new AccountsReceivableDueItem() { AccountType = "01", AmountDue = 3500m } } };

            terms = new List<AccountTerm>() { term1 };
            terms2 = new List<AccountTerm>() { term1ZeroBalance };
            terms3 = new List<AccountTerm>() { term1LessThanRegBalance };
            
            distributionMap = new Dictionary<string, string>() { {"01", "BANK"}, {"02", "TRAV"} };
            
            deferOption1 = new PaymentDeferralOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-15), 100m);
            deferOption2 = new PaymentDeferralOption(DateTime.Today.AddDays(-14), DateTime.Today.AddDays(-8), 75m);
            deferOption3 = new PaymentDeferralOption(DateTime.Today.AddDays(-7), DateTime.Today.AddDays(7), 75m);
            deferOption4 = new PaymentDeferralOption(DateTime.Today.AddDays(8), null, 0m);
            deferOptions = new List<PaymentDeferralOption>() { deferOption1, deferOption2, deferOption3, deferOption4 };

            planOption1 = new PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-15), "DEFAULT", DateTime.Parse("09/01/2014"));
            planOption2 = new PaymentPlanOption(DateTime.Today.AddDays(-14), DateTime.Today.AddDays(14), "DEFAULT", DateTime.Today.AddDays(21));
            planOptions = new List<PaymentPlanOption>() { planOption1 };
            planOptions2 = new List<PaymentPlanOption>() { planOption1, planOption2 };

            planOption1 = new PaymentPlanOption(DateTime.Today.AddDays(-21), DateTime.Today.AddDays(-15), "DEFAULT", DateTime.Parse("09/01/2014"));
            planOptions = new List<PaymentPlanOption>() { planOption1 };

            payReq = new PaymentRequirement("129", "2014/FA", null, 1, deferOptions, planOptions);
            payReq2 = new PaymentRequirement("129", "2014/FA", null, 1, deferOptions, planOptions2);
            payReqNullPlans = new PaymentRequirement("129", "2014/FA", null, 1, deferOptions, null);
            payReqEmptyPlans = new PaymentRequirement("129", "2014/FA", null, 1, deferOptions, new List<PaymentPlanOption>());


            receivableTypeStrings = new List<string>() { "01", "02" };
            receivableTypes = new List<ReceivableType>() { new ReceivableType("01", "Student Receivables"), new ReceivableType("02", "Continuing Ed Receivables") };

            confirmationMap = new Dictionary<string, PaymentConfirmation>() {
                { "BANK", new PaymentConfirmation() { ProviderAccount = "PPCC", ConvenienceFeeAmount = 3.5m, ConvenienceFeeCode = "CVFE", ConvenienceFeeDescription = "Convenience Fee 1"} },
                { "TRAV", new PaymentConfirmation() { ProviderAccount = "OPCCC", ConvenienceFeeAmount = 2.5m, ConvenienceFeeCode = "CVFE2", ConvenienceFeeDescription = "Convenience Fee 2"} }
            };

            service = new ImmediatePaymentService(loggerMock.Object);
        }

        [TestClass]
        public class ImmediatePaymentService_GetPaymentOptions : ImmediatePaymentServiceTests
        {
            [TestInitialize]
            public void ImmediatePaymentService_GetPaymentOptions_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_GetPaymentOptions_NullPaymentControl()
            {
                var options = service.GetPaymentOptions(null, invoices, null, terms, distributionMap, payReq, chargeCodes, receivableTypeStrings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_GetPaymentOptions_NullInvoices()
            {
                var options = service.GetPaymentOptions(paymentControl, null, null, terms, distributionMap, payReq, chargeCodes, receivableTypeStrings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_GetPaymentOptions_EmptyInvoices()
            {
                var options = service.GetPaymentOptions(paymentControl, new List<Invoice>(), null, terms, distributionMap, payReq, chargeCodes, receivableTypeStrings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_GetPaymentOptions_NullDistributionMap()
            {
                var options = service.GetPaymentOptions(paymentControl, invoices, null, terms, null, payReq, chargeCodes, receivableTypeStrings);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_GetPaymentOptions_EmptyDistributionMap()
            {
                var options = service.GetPaymentOptions(paymentControl, invoices, null, terms, new Dictionary<string, string>(), payReq, chargeCodes, receivableTypeStrings);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentOptions_NoInvoicesToPayAfterReceivableTypeFiltering()
            {
                var options = service.GetPaymentOptions(paymentControl, invoices, null, terms, distributionMap, payReq, chargeCodes, new List<string>() { "03" });
                Assert.IsFalse(options.ChargesOnPaymentPlan);
                Assert.AreEqual(0, options.RegistrationBalance);
                Assert.AreEqual(0, options.MinimumPayment);
                Assert.AreEqual(100, options.DeferralPercentage);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentOptions_AtLeastOneInvoiceAssignedToPaymentPlan()
            {
                invoices.First().Charges.First().AddPaymentPlan("128");
                var options = service.GetPaymentOptions(paymentControl, invoices, null, terms, distributionMap, payReq, chargeCodes, null);
                Assert.IsTrue(options.ChargesOnPaymentPlan);
                Assert.AreEqual(0, options.RegistrationBalance);
                Assert.AreEqual(0, options.MinimumPayment);
                Assert.AreEqual(100, options.DeferralPercentage);
                invoices.First().Charges.First().RemovePaymentPlan("128");
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentOptions_ZeroRegistrationBalance()
            {
                var options = service.GetPaymentOptions(paymentControl, invoices, payments, terms, distributionMap, payReq, chargeCodes, receivableTypeStrings);
                Assert.IsFalse(options.ChargesOnPaymentPlan);
                Assert.AreEqual(0, options.RegistrationBalance);
                Assert.AreEqual(0, options.MinimumPayment);
                Assert.AreEqual(100, options.DeferralPercentage);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentOptions_ZeroTermBalance()
            {
                var options = service.GetPaymentOptions(paymentControl, invoices, null, terms2, distributionMap, payReq, chargeCodes, receivableTypeStrings);
                Assert.IsFalse(options.ChargesOnPaymentPlan);
                Assert.AreEqual(0, options.RegistrationBalance);
                Assert.AreEqual(0, options.MinimumPayment);
                Assert.AreEqual(100, options.DeferralPercentage);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentOptions_NonzeroRegistrationBalanceAndTermBalance()
            {
                var options = service.GetPaymentOptions(paymentControl, invoices, null, terms3, distributionMap, payReq, chargeCodes, receivableTypeStrings);
                Assert.IsFalse(options.ChargesOnPaymentPlan);
                Assert.AreEqual(3500m, options.RegistrationBalance);
                Assert.AreEqual(500m, options.MinimumPayment);
                Assert.AreEqual(75m, options.DeferralPercentage);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentOptions_PreexistingPayments()
            {
                var payment2 = new ReceiptPayment("127", "0003315", "01", "2014/FA", "127REF", DateTime.Today.AddDays(-3), 2000m, "128");
                var payments2 = new List<ReceivablePayment>() { payment2 };
                var options = service.GetPaymentOptions(paymentControl, invoices, payments2, terms3, distributionMap, payReq, chargeCodes, receivableTypeStrings);
                Assert.IsFalse(options.ChargesOnPaymentPlan);
                Assert.AreEqual(2000m, options.RegistrationBalance);
                Assert.AreEqual(0m, options.MinimumPayment);
                Assert.AreEqual(75m, options.DeferralPercentage);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentOptions_NullPaymentRequirementUsesDefaultOptions()
            {
                var options = service.GetPaymentOptions(paymentControl, invoices, null, terms3, distributionMap, null, chargeCodes, receivableTypeStrings);
                Assert.IsFalse(options.ChargesOnPaymentPlan);
                Assert.AreEqual(3500m, options.RegistrationBalance);
                Assert.AreEqual(0m, options.MinimumPayment);
                Assert.AreEqual(100m, options.DeferralPercentage);
            }
        }

        [TestClass]
        public class ImmediatePaymentService_AddPaymentPlanOption : ImmediatePaymentServiceTests
        {
            static ImmediatePaymentOptions options = new ImmediatePaymentOptions(false, 4000m, 1000m, 75m);
            PaymentPlanTemplate activeTemplate = new PaymentPlanTemplate("DEFAULT", "Default Template", true, PlanFrequency.Weekly, 5, 0, null, null) { CalculatePlanAmountAutomatically = true, ModifyPlanAutomatically = true, TermsAndConditionsDocumentId = "IPCPLANTC", DownPaymentPercentage = 10m, DaysUntilDownPaymentIsDue = 5 };

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_AddPaymentPlanOption_NullTemplate()
            {
                service.AddPaymentPlanOption(options, null, DateTime.Today.AddDays(3), 4000m, "01");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_AddPaymentPlanOption_NullFirstDate()
            {
                service.AddPaymentPlanOption(options, activeTemplate, null, 4000m, "01");
            }

            [TestMethod]
            public void ImmediatePaymentService_AddPaymentPlanOption_Valid()
            {
                service.AddPaymentPlanOption(options, activeTemplate, DateTime.Today.AddDays(3), 4000m, "01");
                Assert.AreEqual(activeTemplate.Id, options.PaymentPlanTemplateId);
                Assert.AreEqual(DateTime.Today.AddDays(3), options.PaymentPlanFirstDueDate);
                Assert.AreEqual(4000m, options.PaymentPlanAmount);
                Assert.AreEqual("01", options.PaymentPlanReceivableTypeCode);
                Assert.AreEqual(activeTemplate.CalculateDownPaymentAmount(4000m), options.DownPaymentAmount);
                Assert.AreEqual(DateTime.Today.AddDays(activeTemplate.DaysUntilDownPaymentIsDue), options.DownPaymentDate);
            }
        }

        [TestClass]
        public class ImmediatePaymentService_GetPaymentSummary : ImmediatePaymentServiceTests
        {
            [TestInitialize]
            public void ImmediatePaymentService_GetPaymentOptions_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_GetPaymentSummary_NullPaymentControl()
            {
                var result = service.GetPaymentSummary(null, "PMTH", 5000m, invoices, payments, terms, distributionMap, payReq, confirmationMap, receivableTypes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_GetPaymentSummary_NullPaymentMethod()
            {
                var result = service.GetPaymentSummary(paymentControl, null, 5000m, invoices, payments, terms, distributionMap, payReq, confirmationMap, receivableTypes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_GetPaymentSummary_EmptyPaymentMethod()
            {
                var result = service.GetPaymentSummary(paymentControl, string.Empty, 5000m, invoices, payments, terms, distributionMap, payReq, confirmationMap, receivableTypes);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void ImmediatePaymentService_GetPaymentSummary_NegativeAmount()
            {
                var result = service.GetPaymentSummary(paymentControl, "PMTH", -5000m, invoices, payments, terms, distributionMap, payReq, confirmationMap, receivableTypes);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentSummary_Verify()
            {
                var result = service.GetPaymentSummary(paymentControl, "PMTH", 5000m, invoices, null, terms3, distributionMap, payReq, confirmationMap, receivableTypes);
                var resultList = result.ToList();
                Assert.AreEqual(1, resultList.Count);
                Assert.AreEqual(5003.5m, resultList[0].AmountToPay);
                Assert.AreEqual(null, resultList[0].CheckDetails);
                Assert.AreEqual("CVFE", resultList[0].ConvenienceFee);
                Assert.AreEqual(3.5m, resultList[0].ConvenienceFeeAmount);
                Assert.AreEqual(null, resultList[0].ConvenienceFeeGeneralLedgerNumber);
                Assert.AreEqual("BANK", resultList[0].Distribution);
                Assert.AreEqual(1, resultList[0].PaymentItems.Count);
                Assert.AreEqual("PMTH", resultList[0].PayMethod);
                Assert.AreEqual("0003315", resultList[0].PersonId);
                Assert.AreEqual("PPCC", resultList[0].ProviderAccount);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentSummary_NoInvoiceIdsOnPaymentControlRecord()
            {
                var rpc = new RegistrationPaymentControl("123", "0003315", "2014/FA", RegistrationPaymentStatus.New);
                var result = service.GetPaymentSummary(rpc, "PMTH", 5000m, invoices, null, terms3, distributionMap, payReq, confirmationMap, receivableTypes);
                var resultList = result.ToList();
                Assert.AreEqual(0, resultList.Count);
            }
        }

        [TestClass]
        public class ImmediatePaymentService_GetPaymentPlanOption : ImmediatePaymentServiceTests
        {
            [TestInitialize]
            public void ImmediatePaymentService_GetPaymentPlanOption_Initialize()
            {
                base.Initialize();
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentPlanOption_NullPaymentRequirement()
            {
                var option = service.GetPaymentPlanOption(null);
                Assert.IsNull(option);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentPlanOption_NullPlanOptionsForPaymentRequirement()
            {
                var option = service.GetPaymentPlanOption(payReqNullPlans);
                Assert.AreEqual(null, option);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentPlanOption_EmptyPlanOptionsForPaymentRequirement()
            {
                var option = service.GetPaymentPlanOption(payReqEmptyPlans);
                Assert.AreEqual(null, option);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentPlanOption_NoValidPlanOptionsForDate()
            {
                var option = service.GetPaymentPlanOption(payReq);
                Assert.AreEqual(null, option);
            }

            [TestMethod]
            public void ImmediatePaymentService_GetPaymentPlanOption_ValidPlanOptionForDate()
            {
                var option = service.GetPaymentPlanOption(payReq2);
                Assert.AreEqual(planOption2.EffectiveEnd, option.EffectiveEnd);
                Assert.AreEqual(planOption2.EffectiveStart, option.EffectiveStart);
                Assert.AreEqual(planOption2.FirstPaymentDate, option.FirstPaymentDate);
                Assert.AreEqual(planOption2.TemplateId, option.TemplateId);
            }
        }

        [TestClass]
        public class ImmediatePaymentService_IsValidTemplate : ImmediatePaymentServiceTests
        {
            PaymentPlanTemplate activeTemplate = new PaymentPlanTemplate("DEFAULT", "Default Template", true, PlanFrequency.Weekly, 5, 0, null, null) { CalculatePlanAmountAutomatically = true, ModifyPlanAutomatically = true, TermsAndConditionsDocumentId = "IPCPLANTC" };
            PaymentPlanTemplate inactiveTemplate = new PaymentPlanTemplate("DEFAULT", "Default Template", false, PlanFrequency.Weekly, 5, 0, null, null) { CalculatePlanAmountAutomatically = true, ModifyPlanAutomatically = true, TermsAndConditionsDocumentId = "IPCPLANTC" };
            PaymentPlanTemplate noPlanCalcTemplate = new PaymentPlanTemplate("DEFAULT", "Default Template", true, PlanFrequency.Weekly, 5, 0, null, null) { CalculatePlanAmountAutomatically = false, ModifyPlanAutomatically = false, TermsAndConditionsDocumentId = "IPCPLANTC" };
            PaymentPlanTemplate noAutoModTemplate = new PaymentPlanTemplate("DEFAULT", "Default Template", true, PlanFrequency.Weekly, 5, 0, null, null) { CalculatePlanAmountAutomatically = true, ModifyPlanAutomatically = false, TermsAndConditionsDocumentId = "IPCPLANTC" };

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ImmediatePaymentService_IsValidTemplate_Null()
            {
                var isValid = service.IsValidTemplate(null);
            }

            [TestMethod]
            public void ImmediatePaymentService_IsValidTemplate_True()
            {
                Assert.IsTrue(service.IsValidTemplate(activeTemplate));
            }

            [TestMethod]
            public void ImmediatePaymentService_IsValidTemplate_IsActive_False()
            {
                Assert.IsFalse(service.IsValidTemplate(inactiveTemplate));
            }

            [TestMethod]
            public void ImmediatePaymentService_IsValidTemplate_CalculatePlanAmountAutomatically_False()
            {
                Assert.IsFalse(service.IsValidTemplate(noPlanCalcTemplate));
            }

            [TestMethod]
            public void ImmediatePaymentService_IsValidTemplate_ModifyPlanAutomatically_False()
            {
                Assert.IsFalse(service.IsValidTemplate(noAutoModTemplate));
            }

            [TestMethod]
            public void ImmediatePaymentService_IsValidTemplate_TermsAndConditionsDocumentId_Null()
            {
                activeTemplate.TermsAndConditionsDocumentId = null;
                Assert.IsFalse(service.IsValidTemplate(activeTemplate));
            }

            [TestMethod]
            public void ImmediatePaymentService_IsValidTemplate_TermsAndConditionsDocumentId_Empty()
            {
                activeTemplate.TermsAndConditionsDocumentId = string.Empty;
                Assert.IsFalse(service.IsValidTemplate(activeTemplate));
            } 
        }
    }
}
