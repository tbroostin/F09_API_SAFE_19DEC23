// Copyright 2014-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Data.Base;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Finance;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Finance.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ellucian.Colleague.Domain.Finance.Tests.Services
{
    [TestClass]
    public class PaymentPlanProcessorTests : BaseRepositorySetup
    {
        Mock<RuleAdapterRegistry> ruleAdapterRegistryMock;
        Mock<RuleConfiguration> ruleConfigurationMock;
        Mock<IRuleRepository> ruleRepositoryMock;
        Mock<IPaymentPlanRepository> payPlanRepositoryMock;

        PaymentPlanProcessor payPlanProcessor;

        static string personId;
        static Charge charge1_1;
        static Charge charge1_2;
        static Charge charge1_3;
        static Charge charge1_4;
        static Charge charge1_5;
        static Charge charge1_6;
        static Charge charge1_7;
        static List<Charge> charges1;
        static Invoice invoice1;

        static Charge charge2_1;
        static List<Charge> charges2;
        static Invoice invoice2;

        static Charge charge3_1;
        static List<Charge> charges3;
        static Invoice invoice3;
        static List<Invoice> invoices;

        static ReceiptPayment payment1;
        static List<ReceivablePayment> payments;

        static List<string> allowedReceivableTypes;
        static List<string> _invExclRules1;
        static ReadOnlyCollection<string> invExclRules1;

        static List<string> _exclRules1;
        static ReadOnlyCollection<string> exclRules1;
        static PaymentPlanTemplate ppTemplate1; 
        
        decimal regBalance;
        decimal termBalance;
        static ReadOnlyCollection<ChargeCode> chargeCodes = TestChargeCodesRepository.ChargeCodes.AsReadOnly();

        // Function called for rule processing
        public List<Invoice> ProcessRules(List<Invoice> invoices, IEnumerable<string> rules)
        {
            var results = new List<Invoice>();
            foreach (var invoice in invoices)
            {
                bool passed = false;
                foreach (string rule in rules)
                {
                    switch (rule)
                    {
                        case "EXCLALL":
                            passed = true;
                            break;
                        case "EXCLNONE":
                            passed = false;
                            break;
                        case "EXCLSOME":
                            passed = invoice.Description == "Payment Plan Setup Charge";
                            break;
                        case "INVDROOM":
                            passed = invoice.Description.ToUpper().StartsWith("FOX");
                            break;
                        case "INVNO02":
                            passed = invoice.ReceivableTypeCode == "02";
                            break;
                        default:
                            passed = false;
                            break;
                    }
                    if (passed) break;
                }
                if (!passed) results.Add(invoice);
            }
            return results;
        }

        [TestInitialize]
        public void Initialize()
        {
            base.MockInitialize();
            ruleAdapterRegistryMock = new Mock<RuleAdapterRegistry>();
            ruleAdapterRegistryMock.Object.Register<Invoice>("AR.INVOICES", new InvoiceRuleAdapter());
            ruleConfigurationMock = new Mock<RuleConfiguration>();

            payPlanRepositoryMock = new Mock<IPaymentPlanRepository>();
            ruleRepositoryMock = new Mock<IRuleRepository>();

            payPlanProcessor = new PaymentPlanProcessor(ProcessRules, loggerMock.Object);

            personId = "0005343";
            charge1_1 = new Charge("27818", "6516", new List<string>() { "Activity Fee" }, "ACTFE", 325.00m);
            charge1_2 = new Charge("27819", "6516", new List<string>() { "Health Fee" }, "HLTFE", 1225.00m);
            charge1_3 = new Charge("27820", "6516", new List<string>() { "Technology Fee" }, "TECFE", 175.00m);
            charge1_4 = new Charge("27821", "6516", new List<string>() { "Tuition, Full Time" }, "TUIFT", 3750.00m);
            charge1_5 = new Charge("27822", "6516", new List<string>() { "Tuition, Full Time" }, "TUIFT", 3750.00m);
            charge1_6 = new Charge("27823", "6516", new List<string>() { "Tuition, Full Time" }, "TUIFT", 3750.00m);
            charge1_7 = new Charge("27824", "6516", new List<string>() { "Tuition, Full Time" }, "TUIFT", 3750.00m);
            charges1 = new List<Charge>() { charge1_1, charge1_2, charge1_3, charge1_4, charge1_5, charge1_6, charge1_7 };
            invoice1 = new Invoice("6516", personId, "01", "2014/SP", "000005565", new DateTime(2014, 4, 15), new DateTime(2014, 4, 22), new DateTime(2014, 1, 23), new DateTime(2014, 5, 14), "Registration - 2014/SP", charges1);

            charge2_1 = new Charge("28957", "6760", new List<string>() { "Payment Plan Setup Charge" }, "PPLAN", 250.00m);
            charges2 = new List<Charge>() { charge2_1 };
            invoice2 = new Invoice("6760", personId, "01", "2014/SP", "0000005790", new DateTime(2014, 5, 21), new DateTime(2014, 5, 21), new DateTime(2014, 5, 21), new DateTime(2014, 5, 21), "Payment Plan Setup Charge", charges2);

            charge3_1 = new Charge("28963", "6766", new List<string>() { "Payment Plan Setup Charge" }, "PPLAN", -250.00m);
            charges3 = new List<Charge>() { charge3_1 };
            invoice3 = new Invoice("6766", personId, "01", "2014/SP", "0000005796", new DateTime(2014, 5, 30), new DateTime(2014, 5, 30), new DateTime(2014, 5, 21), new DateTime(2014, 5, 21), "Payment Plan Setup Charge", charges3);
            invoices = new List<Invoice>() { invoice1, invoice2, invoice3 };

            payment1 = new ReceiptPayment("32423", personId, "01", "2014/SP", "000006547", new DateTime(2014, 4, 20), 250.00m, "000000654");
            payments = new List<ReceivablePayment>() { payment1 };

            allowedReceivableTypes = new List<string>() { "01", "02" };
            _invExclRules1 = new List<string>() { "INVDROOM", "INVNO02" };
            invExclRules1 = _invExclRules1.AsReadOnly();

            _exclRules1 = new List<string>() { "ACTFE", "HLTFE" };
            exclRules1 = _exclRules1.AsReadOnly();
            ppTemplate1 = new PaymentPlanTemplate("SETUPDPNO", "Setup Not in Down Payment", true, PlanFrequency.Monthly, 3, 0.00m, 10000.00m, "")
            { SetupChargeAmount = 250.00m, SetupChargePercentage = 0.00m, DownPaymentPercentage = 10.00m, DaysUntilDownPaymentIsDue = 0, IncludeSetupChargeInFirstPayment = false, GraceDays = 3, LateChargeAmount = 150.00m, LateChargePercentage = 3.00m, CalculatePlanAmountAutomatically = true }; 
        }

        [TestClass]
        public class FilterInvoices : PaymentPlanProcessorTests
        {
            PaymentPlanTemplate template1;

            [TestInitialize]
            public void Initialize_FilterInvoices()
            {
                base.Initialize();
                template1 = new PaymentPlanTemplate(ppTemplate1.Id, ppTemplate1.Description, ppTemplate1.IsActive, ppTemplate1.Frequency, ppTemplate1.NumberOfPayments,
                    ppTemplate1.MinimumPlanAmount, ppTemplate1.MaximumPlanAmount, ppTemplate1.CustomFrequencySubroutine)
                    {
                        CalculatePlanAmountAutomatically = ppTemplate1.CalculatePlanAmountAutomatically,
                        DaysUntilDownPaymentIsDue = ppTemplate1.DaysUntilDownPaymentIsDue,
                        DownPaymentPercentage = ppTemplate1.DownPaymentPercentage,
                        GraceDays = ppTemplate1.GraceDays,
                        IncludeSetupChargeInFirstPayment = ppTemplate1.IncludeSetupChargeInFirstPayment,
                        LateChargeAmount = ppTemplate1.LateChargeAmount,
                        LateChargePercentage = ppTemplate1.LateChargePercentage,
                        SetupChargeAmount = ppTemplate1.SetupChargeAmount,
                        SetupChargePercentage = ppTemplate1.SetupChargePercentage,
                        SubtractAnticipatedFinancialAid = ppTemplate1.SubtractAnticipatedFinancialAid,
                        TermsAndConditionsDocumentId = ppTemplate1.TermsAndConditionsDocumentId
                    };
                    template1.AddAllowedReceivableTypeCode("01");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_FilterInvoices_NullTemplate()
            {
                var result = payPlanProcessor.FilterInvoices(null, invoices);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_FilterInvoices_NullInvoices()
            {
                var result = payPlanProcessor.FilterInvoices(template1, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_FilterInvoices_ZeroInvoices()
            {
                var result = payPlanProcessor.FilterInvoices(template1, new List<Invoice>());
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_ZeroInvoiceExclusionRules()
            {
                var result = payPlanProcessor.FilterInvoices(template1, invoices);

                CollectionAssert.AreEqual(invoices, result.ToList());
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_NoAllowedReceivableTypeCodes()
            {
                template1 = new PaymentPlanTemplate(ppTemplate1.Id, ppTemplate1.Description, ppTemplate1.IsActive, ppTemplate1.Frequency, ppTemplate1.NumberOfPayments,
                    ppTemplate1.MinimumPlanAmount, ppTemplate1.MaximumPlanAmount, ppTemplate1.CustomFrequencySubroutine)
                {
                    CalculatePlanAmountAutomatically = ppTemplate1.CalculatePlanAmountAutomatically,
                    DaysUntilDownPaymentIsDue = ppTemplate1.DaysUntilDownPaymentIsDue,
                    DownPaymentPercentage = ppTemplate1.DownPaymentPercentage,
                    GraceDays = ppTemplate1.GraceDays,
                    IncludeSetupChargeInFirstPayment = ppTemplate1.IncludeSetupChargeInFirstPayment,
                    LateChargeAmount = ppTemplate1.LateChargeAmount,
                    LateChargePercentage = ppTemplate1.LateChargePercentage,
                    SetupChargeAmount = ppTemplate1.SetupChargeAmount,
                    SetupChargePercentage = ppTemplate1.SetupChargePercentage,
                    SubtractAnticipatedFinancialAid = ppTemplate1.SubtractAnticipatedFinancialAid,
                    TermsAndConditionsDocumentId = ppTemplate1.TermsAndConditionsDocumentId
                };
                var result = payPlanProcessor.FilterInvoices(template1, invoices);
                CollectionAssert.AreEqual(invoices, result.ToList());
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_AllInvoicesExcluded()
            {
                template1.AddInvoiceExclusionRuleId("EXCLALL");
                var result = payPlanProcessor.FilterInvoices(template1, invoices);

                Assert.AreEqual(0, result.Count());
                CollectionAssert.AreEqual(new List<Invoice>(), result.ToList());
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_NoInvoicesExcluded()
            {
                template1.AddInvoiceExclusionRuleId("EXCLNONE");
                var result = payPlanProcessor.FilterInvoices(template1, invoices).ToList();

                Assert.AreEqual(invoices.Count, result.Count);
                CollectionAssert.AreEqual(invoices, result);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_SomeInvoicesExcluded()
            {
                template1.AddInvoiceExclusionRuleId("EXCLSOME");
                var result = payPlanProcessor.FilterInvoices(template1, invoices);
                var source = new List<Invoice>() { invoice1 };

                Assert.AreEqual(source.Count, result.Count());
                CollectionAssert.AreEqual(source, result.ToList());
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_AllMatchingReceivableTypes()
            {
                template1.AddAllowedReceivableTypeCode("01");
                var result = payPlanProcessor.FilterInvoices(template1, invoices).ToList();

                Assert.AreEqual(invoices.Count, result.Count);
                CollectionAssert.AreEqual(invoices, result);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_NoMatchingReceivableTypes()
            {
                template1 = new PaymentPlanTemplate(ppTemplate1.Id, ppTemplate1.Description, ppTemplate1.IsActive, ppTemplate1.Frequency, ppTemplate1.NumberOfPayments,
                    ppTemplate1.MinimumPlanAmount, ppTemplate1.MaximumPlanAmount, ppTemplate1.CustomFrequencySubroutine)
                {
                    CalculatePlanAmountAutomatically = ppTemplate1.CalculatePlanAmountAutomatically,
                    DaysUntilDownPaymentIsDue = ppTemplate1.DaysUntilDownPaymentIsDue,
                    DownPaymentPercentage = ppTemplate1.DownPaymentPercentage,
                    GraceDays = ppTemplate1.GraceDays,
                    IncludeSetupChargeInFirstPayment = ppTemplate1.IncludeSetupChargeInFirstPayment,
                    LateChargeAmount = ppTemplate1.LateChargeAmount,
                    LateChargePercentage = ppTemplate1.LateChargePercentage,
                    SetupChargeAmount = ppTemplate1.SetupChargeAmount,
                    SetupChargePercentage = ppTemplate1.SetupChargePercentage,
                    SubtractAnticipatedFinancialAid = ppTemplate1.SubtractAnticipatedFinancialAid,
                    TermsAndConditionsDocumentId = ppTemplate1.TermsAndConditionsDocumentId
                };
                template1.AddAllowedReceivableTypeCode("02");
                var result = payPlanProcessor.FilterInvoices(template1, invoices).ToList();

                Assert.AreEqual(0, result.Count);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_SomeMatchingReceivableTypes()
            {
                foreach (string rule in invExclRules1)
                {
                    template1.AddInvoiceExclusionRuleId(rule);
                }
                var result = payPlanProcessor.FilterInvoices(template1, invoices);

                Assert.AreEqual(invoices.Count, result.Count());
                CollectionAssert.AreEqual(invoices, result.ToList());
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_FilterInvoices_ComplexScenario()
            {
                foreach (string type in allowedReceivableTypes)
                {
                    template1.AddAllowedReceivableTypeCode(type);
                }
                foreach (string rule in invExclRules1)
                {
                    template1.AddInvoiceExclusionRuleId(rule);
                }

                var result = payPlanProcessor.FilterInvoices(template1, invoices).ToList();
                var source = invoices.Where(x => !x.Charges.Any(y => template1.IncludedChargeCodes.Contains(y.Code))).ToList();

                Assert.AreEqual(source.Count(), result.Count());
                CollectionAssert.AreEqual(source.ToList(), result.ToList());
            }
        }

        [TestClass]
        public class GetPlanAmount : PaymentPlanProcessorTests
        {
            string receivableType = null;
            List<PlanCharge> planCharges = new List<PlanCharge>();
            decimal planAmount = 0m;

            [TestInitialize]
            public void Initialize_GetPlanAmount()
            {
                base.Initialize();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetPlanAmount_NullChargeCodes()
            {
                planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, null, true, out receivableType);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetPlanAmount_EmptyChargeCodes()
            {
                planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, new List<ChargeCode>(), true, out receivableType);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_GetPlanAmount_VerifyPlanAmount_RegistrationBalanceLessThanTermBalance()
            {
                regBalance = 6725;
                termBalance = 16725;
                planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, chargeCodes, true, out receivableType);
                Assert.AreEqual(regBalance, planAmount);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_GetPlanAmount_VerifyPlanAmount_RegistrationBalanceGreaterThanTermBalance()
            {
                regBalance = 16725;
                termBalance = 5725;
                planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, chargeCodes, true, out receivableType);
                Assert.AreEqual(termBalance, planAmount);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_GetPlanAmount_VerifyPlanAmount_RegistrationBalanceEqualsTermBalance()
            {
                regBalance = 16725;
                termBalance = 16725;
                planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, chargeCodes, true, out receivableType);
                Assert.AreEqual(10000m, planAmount);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_GetPlanAmount_VerifyPlanAmount_TemplateHasIncludedChargeCodes()
            {
                regBalance = 16725;
                termBalance = 16725;
                ppTemplate1.AddIncludedChargeCode("ACTFE");
                ppTemplate1.AddIncludedChargeCode("HLTFE");
                ppTemplate1.AddIncludedChargeCode("TECFE");
                planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, chargeCodes, true, out receivableType);
                Assert.AreEqual(1725m, planAmount);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_GetPlanAmount_VerifyPlanAmount_TemplateHasExcludedChargeCodes()
            {
                regBalance = 1525;
                termBalance = 1525;
                ppTemplate1.AddExcludedChargeCode("TUIFT");
                ppTemplate1.AddExcludedChargeCode("HLTFE");
                planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, chargeCodes, true, out receivableType);
                Assert.AreEqual(500m, planAmount);
            }
        }

        [TestClass]
        public class AddPlanSchedule : PaymentPlanProcessorTests
        {
            static PaymentPlanTemplate activeTemplate;
            static PaymentPlan plan;
            static List<DateTime?> planDates;

            [TestInitialize]
            public void Initialize_FilterInvoices()
            {
                base.Initialize();
                activeTemplate = new PaymentPlanTemplate("DEFAULT", "Default Template", true, PlanFrequency.Weekly, 5, 0, null, null) { CalculatePlanAmountAutomatically = true, ModifyPlanAutomatically = true, TermsAndConditionsDocumentId = "IPCPLANTC", DownPaymentPercentage = 10m, DaysUntilDownPaymentIsDue = 5 };
                plan = new PaymentPlan(null, "DEFAULT", "0003315", "01", "2014/FA", 5000m, DateTime.Today.AddDays(-7),
                    null, null, null)
                    {
                        NumberOfPayments = 4
                    };
                planDates = new List<DateTime?>() {
                    DateTime.Today.AddDays(-7),
                    DateTime.Today,
                    DateTime.Today.AddDays(7),
                    DateTime.Today.AddDays(14),
                    DateTime.Today.AddDays(21)
                };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_AddPlanSchedule_NullTemplate()
            {
                payPlanProcessor.AddPlanSchedules(null, plan, planDates);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_AddPlanSchedule_NullProposedPlan()
            {
                payPlanProcessor.AddPlanSchedules(activeTemplate, null, planDates);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_AddPlanSchedule_NullPlanDates()
            {
                payPlanProcessor.AddPlanSchedules(activeTemplate, plan, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_AddPlanSchedule_EmptyPlanDates()
            {
                payPlanProcessor.AddPlanSchedules(activeTemplate, plan, new List<DateTime?>());
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void TestPaymentPlanProcessor_AddPlanSchedule_NumberOfPaymentsMismatch()
            {
                plan.NumberOfPayments = 10;
                payPlanProcessor.AddPlanSchedules(activeTemplate, plan, planDates);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_AddPlanSchedule_VerifySchedule_DownPayment()
            {
                payPlanProcessor.AddPlanSchedules(activeTemplate, plan, planDates);
                Assert.AreEqual(5, plan.ScheduledPayments.Count);
                Assert.AreEqual(plan.OriginalAmount, plan.ScheduledPayments.Sum(sp => sp.Amount));
                for (int i = 0; i < plan.ScheduledPayments.Count; i++)
                {
                    Assert.AreEqual(planDates[i], plan.ScheduledPayments[i].DueDate);
                    switch(i)
                    {
                        case 0:
                            Assert.AreEqual(500m, plan.ScheduledPayments[i].Amount);
                            break;
                        default:
                            Assert.AreEqual(1125m, plan.ScheduledPayments[i].Amount);
                            break;
                    }
                }
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_AddPlanSchedule_VerifySchedule_NoDownPayment()
            {
                planDates[0] = null;
                activeTemplate.DownPaymentPercentage = 0m;
                payPlanProcessor.AddPlanSchedules(activeTemplate, plan, planDates);
                Assert.AreEqual(4, plan.ScheduledPayments.Count);
                Assert.AreEqual(plan.OriginalAmount, plan.ScheduledPayments.Sum(sp => sp.Amount));
                Assert.IsTrue(plan.ScheduledPayments.All(sp => sp.Amount == 1250m));
                for (int i = 1; i <= plan.ScheduledPayments.Count; i++)
                {
                    Assert.AreEqual(planDates[i], plan.ScheduledPayments[i-1].DueDate);
                }
            }
        }

        [TestClass]
        public class GetProposedPlan : PaymentPlanProcessorTests
        {
            static PaymentPlanTemplate activeTemplate;
            static string receivableType;
            static string termId;
            static decimal planAmount;
            static DateTime firstPaymentDate;
            static PlanCharge charge1;
            static List<PlanCharge> planCharges;

            [TestInitialize]
            public void Initialize_GetProposedPlan()
            {
                activeTemplate = new PaymentPlanTemplate("DEFAULT", "Default Template", true, PlanFrequency.Weekly, 5, 0, null, null) { CalculatePlanAmountAutomatically = true, ModifyPlanAutomatically = true, TermsAndConditionsDocumentId = "IPCPLANTC", DownPaymentPercentage = 10m, DaysUntilDownPaymentIsDue = 5 };
                personId = "0003315";
                receivableType = "01";
                termId = "2014/FA";
                planAmount = 5000;
                firstPaymentDate = DateTime.Today.AddDays(7);
                charge1 = new PlanCharge(null, new Charge("12345", "23456", new List<string>() { "Desc" }, "TUIFT", planAmount), planAmount, false, true);
                planCharges = new List<PlanCharge>() { charge1 };
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetProposedPlan_NullTemplate()
            {
                var plan = payPlanProcessor.GetProposedPlan(null, personId, receivableType, termId, planAmount, firstPaymentDate, planCharges);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetProposedPlan_NullPersonId()
            {
                var plan = payPlanProcessor.GetProposedPlan(activeTemplate, null, receivableType, termId, planAmount, firstPaymentDate, planCharges);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetProposedPlan_EmptyPersonId()
            {
                var plan = payPlanProcessor.GetProposedPlan(activeTemplate, string.Empty, receivableType, termId, planAmount, firstPaymentDate, planCharges);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetProposedPlan_NullReceivableType()
            {
                var plan = payPlanProcessor.GetProposedPlan(activeTemplate, personId, null, termId, planAmount, firstPaymentDate, planCharges);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetProposedPlan_EmptyReceivableType()
            {
                var plan = payPlanProcessor.GetProposedPlan(activeTemplate, personId, string.Empty, termId, planAmount, firstPaymentDate, planCharges);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetProposedPlan_NullTermId()
            {
                var plan = payPlanProcessor.GetProposedPlan(activeTemplate, personId, receivableType, null, planAmount, firstPaymentDate, planCharges);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void TestPaymentPlanProcessor_GetProposedPlan_EmptyTermId()
            {
                var plan = payPlanProcessor.GetProposedPlan(activeTemplate, personId, receivableType, string.Empty, planAmount, firstPaymentDate, planCharges);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void TestPaymentPlanProcessor_GetProposedPlan_PlanAmountLessThanZero()
            {
                var plan = payPlanProcessor.GetProposedPlan(activeTemplate, personId, receivableType, termId, -1, firstPaymentDate, planCharges);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_GetProposedPlan_VerifyPlan()
            {
                var plan = payPlanProcessor.GetProposedPlan(activeTemplate, personId, receivableType, termId, planAmount, firstPaymentDate, planCharges);
                Assert.AreEqual(planAmount, plan.CurrentAmount);
                Assert.AreEqual(activeTemplate.DownPaymentPercentage, plan.DownPaymentPercentage);
                Assert.AreEqual(activeTemplate.Frequency, plan.Frequency);
                Assert.AreEqual(activeTemplate.GraceDays, plan.GraceDays);
                Assert.AreEqual(activeTemplate.LateChargeAmount, plan.LateChargeAmount);
                Assert.AreEqual(activeTemplate.LateChargePercentage, plan.LateChargePercentage);
                Assert.AreEqual(activeTemplate.NumberOfPayments, plan.NumberOfPayments);
                Assert.AreEqual(activeTemplate.SetupChargeAmount, plan.SetupAmount);
                Assert.AreEqual(activeTemplate.SetupChargePercentage, plan.SetupPercentage);
                Assert.AreEqual(1, plan.PlanCharges.Count);
                Assert.AreEqual(charge1, plan.PlanCharges[0]);
            }
        }

        [TestClass]
        public class GetPlanCharges : PaymentPlanProcessorTests
        {
            string receivableType = null;
            List<PlanCharge> planCharges = new List<PlanCharge>();

            [TestMethod]
            public void TestPaymentPlanProcessor_GetPlanCharges_TemplateFiltersAllInvoices_VerifyNoEligibleCharges()
            {
                ppTemplate1.AddInvoiceExclusionRuleId("EXCLALL");
                regBalance = 6725;
                termBalance = 16725;
                var planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, chargeCodes, false, out receivableType);

                var charges = payPlanProcessor.GetPlanCharges("01", 5000m, ppTemplate1, invoices, chargeCodes);
                Assert.AreEqual(0, charges.Count);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_GetPlanCharges_TemplateHasIncludedChargeCodes_VerifyEligibleCharges()
            {
                ppTemplate1.AddIncludedChargeCode("ACTFE");
                ppTemplate1.AddIncludedChargeCode("HLTFE");
                ppTemplate1.AddIncludedChargeCode("TECFE");
                regBalance = 6725;
                termBalance = 16725;
                // Need to call GetPlanAmount to populate _chargeCodes on processor
                var planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, chargeCodes, false, out receivableType);

                var charges = payPlanProcessor.GetPlanCharges("01", 5000m, ppTemplate1, invoices, chargeCodes);
                Assert.AreEqual(3, charges.Count);
            }

            [TestMethod]
            public void TestPaymentPlanProcessor_GetPlanCharges_TemplateHasExcludedChargeCodes_VerifyEligibleCharges()
            {
                ppTemplate1.AddExcludedChargeCode("TUIFT");
                ppTemplate1.AddExcludedChargeCode("HLTFE");
                regBalance = 6725;
                termBalance = 16725;
                // Need to call GetPlanAmount to populate _chargeCodes on processor
                var planAmount = payPlanProcessor.GetPlanAmount(regBalance, termBalance, ppTemplate1, invoices, payments, chargeCodes, false, out receivableType);

                var charges = payPlanProcessor.GetPlanCharges("01", 5000m, ppTemplate1, invoices, chargeCodes);
                Assert.AreEqual(4, charges.Count);
            }
        }

    }
}
