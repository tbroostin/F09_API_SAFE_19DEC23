// Copyright 2014-2017 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ellucian.Colleague.Data.Base.Tests.Repositories;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Repositories;
using Ellucian.Colleague.Data.Student.DataContracts;
using Ellucian.Colleague.Data.Student.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Domain.Finance.Repositories;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Student.Tests;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Data.Finance.Transactions;
using Ellucian.Web.Http.Configuration;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Finance.Tests.Repositories
{
    [TestClass]
    public class PaymentPlanRepositoryTests : BaseRepositorySetup
    {
        PaymentPlanRepository repository;
        private string _colleagueTimeZone;

        #region Setup variable definition

        static Collection<PayPlanTemplates> payPlanTemplates = TestPayPlanTemplatesRepository.PayPlanTemplates;
        static Collection<ArPayPlans> arPayPlans = TestArPayPlansRepository.ArPayPlans;
        static Collection<ArPayPlanItems> arPayPlanItems = TestArPayPlanItemsRepository.ArPayPlanItems;
        static Collection<ArPayInvoiceItems> arPayInvoiceItems = TestArPayInvoiceItemsRepository.ArPayInvoiceItems;
        static Collection<DataContracts.ArInvoiceItems> arInvoiceItems = TestArInvoiceItemsRepository.ArInvoiceItems;
        static Collection<ArCodeTaxGlDistr> arCodeTaxGlDistrs = TestArCodeTaxGlDistrRepository.ArCodeTaxGlDistrs;
        static Collection<PayPlanApprovals> payPlanApprovals = new Collection<PayPlanApprovals>();

        static string planId1 = "1";
        static string planId2 = "1006";
        static string planId3 = "1111";
        static string planId4 = "2222";
        static ArPayPlans source1 = arPayPlans.FirstOrDefault(x => x.Recordkey == planId1);
        static ArPayPlans source2 = arPayPlans.FirstOrDefault(x => x.Recordkey == planId2);
        static ArPayPlans source3 = arPayPlans.FirstOrDefault(x => x.Recordkey == planId3);
        static ArPayPlans source4 = arPayPlans.FirstOrDefault(x => x.Recordkey == planId4);

        static string personId = "0003315";
        static string receivableType = "01";
        static string errorReceivableType = "02";
        static string termId = "2024/SP";
        static string templateId = "DEFAULT";
        static DateTime? downPaymentDate = DateTime.Today;
        static DateTime firstPaymentDate = DateTime.Today.AddDays(3);
        static List<DateTime?> otherDates = new List<DateTime?>() { DateTime.Today.AddDays(10), DateTime.Today.AddDays(17), DateTime.Today.AddDays(24) };

        static GetPlanCustomScheduleDatesResponse validResponseWithDownPayment = new GetPlanCustomScheduleDatesResponse() { DownPmtDate = DateTime.Today, FirstPmtDate = DateTime.Today.AddDays(3), ErrorMsg = null, OutAddnlPmtDates = otherDates };
        static GetPlanCustomScheduleDatesResponse validResponseWithoutDownPayment = new GetPlanCustomScheduleDatesResponse() { DownPmtDate = null, FirstPmtDate = DateTime.Today.AddDays(3), ErrorMsg = null, OutAddnlPmtDates = otherDates };
        static GetPlanCustomScheduleDatesResponse errorResponse = new GetPlanCustomScheduleDatesResponse() { DownPmtDate = null, FirstPmtDate = null, ErrorMsg = "There was an error building the list of custom schedule dates.", OutAddnlPmtDates = null };

        static GetProposedPaymentPlanInfoResponse gpppirError = new GetProposedPaymentPlanInfoResponse() { OutErrorOccurred = true, OutErrorMessages = new List<string>() { "Error executing CTX." } };
        static GetProposedPaymentPlanInfoResponse gpppirData = new GetProposedPaymentPlanInfoResponse()
        {
            OutPlanId = "200",
            OutErrorOccurred = false,
            OutErrorMessages = new List<string>(),
            OutPlanAmount = 1600m,
            OutPlanDownPayPct = 0m,
            OutPlanFrequency = "W",
            OutPlanGraceDays = 3,
            OutPlanLateChgPct = 10m,
            OutPlanLateChgAmt = 15m,
            OutPlanPmtsCount = 5,
            OutPlanSetupChgAmt = 100m,
            OutPlanSetupChgPct = 3m,
            ProposedInvoiceItems = new List<ProposedInvoiceItems>()
            {
                new ProposedInvoiceItems() 
                {
                    OutArInvoiceItemArCodes = "MATFE",
                    OutArInvoiceItemChargeAmts = 500m,
                    OutArInvoiceItemCreditAmts = null,
                    OutArInvoiceItemDescs = "Materials Fee",
                    OutArInvoiceItemIds = "123",
                    OutArInvoiceItemInvoiceIds = "2313"
                },
                new ProposedInvoiceItems() 
                {
                    OutArInvoiceItemArCodes = "MATFE",
                    OutArInvoiceItemChargeAmts = 400m,
                    OutArInvoiceItemCreditAmts = null,
                    OutArInvoiceItemDescs = "Materials Fee",
                    OutArInvoiceItemIds = "124",
                    OutArInvoiceItemInvoiceIds = "2313"
                },
                new ProposedInvoiceItems() 
                {
                    OutArInvoiceItemArCodes = "MATFE",
                    OutArInvoiceItemChargeAmts = 400m,
                    OutArInvoiceItemCreditAmts = null,
                    OutArInvoiceItemDescs = "Materials Fee",
                    OutArInvoiceItemIds = "125",
                    OutArInvoiceItemInvoiceIds = "2313"
                },
                new ProposedInvoiceItems() 
                {
                    OutArInvoiceItemArCodes = "MATFE",
                    OutArInvoiceItemChargeAmts = 300m,
                    OutArInvoiceItemCreditAmts = null,
                    OutArInvoiceItemDescs = "Materials Fee",
                    OutArInvoiceItemIds = "126",
                    OutArInvoiceItemInvoiceIds = "2313"
                }
            },
            ProposedPaymentPlanCharges = new List<ProposedPaymentPlanCharges>()
            {
                new ProposedPaymentPlanCharges() 
                {
                    OutPlanItemAmounts = 500m,
                    OutPlanItemAutoModFlags = "Y",
                    OutPlanItemInvItemIds = "200*123",
                    OutPlanItemSetupChgFlags = "N"
                },
                new ProposedPaymentPlanCharges() 
                {
                    OutPlanItemAmounts = 400m,
                    OutPlanItemAutoModFlags = "Y",
                    OutPlanItemInvItemIds = "200*124",
                    OutPlanItemSetupChgFlags = "N"
                },
                new ProposedPaymentPlanCharges() 
                {
                    OutPlanItemAmounts = 400m,
                    OutPlanItemAutoModFlags = "Y",
                    OutPlanItemInvItemIds = "200*125",
                    OutPlanItemSetupChgFlags = "N"
                },
                new ProposedPaymentPlanCharges() 
                {
                    OutPlanItemAmounts = 300m,
                    OutPlanItemAutoModFlags = "Y",
                    OutPlanItemInvItemIds = "200*126",
                    OutPlanItemSetupChgFlags = "N"
                }
            },
            ProposedScheduledPayments = new List<ProposedScheduledPayments>()
            {
                new ProposedScheduledPayments() 
                {
                    OutSchedPmtAmts = 400m,
                    OutSchedPmtDueDates = DateTime.Today.AddDays(3),
                    OutSchedPmtIds = "200"
                },
                new ProposedScheduledPayments() 
                {
                    OutSchedPmtAmts = 400m,
                    OutSchedPmtDueDates = DateTime.Today.AddDays(13),
                    OutSchedPmtIds = "200"
                },
                new ProposedScheduledPayments() 
                {
                    OutSchedPmtAmts = 400m,
                    OutSchedPmtDueDates = DateTime.Today.AddDays(23),
                    OutSchedPmtIds = "200"
                },
                new ProposedScheduledPayments() 
                {
                    OutSchedPmtAmts = 400m,
                    OutSchedPmtDueDates = DateTime.Today.AddDays(33),
                    OutSchedPmtIds = "200"
                }              
            }
        };
        static GetProposedPaymentPlanInfoResponse gpppirCorruptData = new GetProposedPaymentPlanInfoResponse()
        {
            OutErrorOccurred = false,
            OutErrorMessages = new List<string>(),
            OutPlanAmount = 1600m,
            OutPlanDownPayPct = 0m,
            OutPlanFrequency = "W",
            OutPlanGraceDays = 3,
            OutPlanLateChgPct = 10m,
            OutPlanLateChgAmt = 15m,
            OutPlanPmtsCount = 5,
            OutPlanSetupChgAmt = 100m,
            OutPlanSetupChgPct = 3m,
            ProposedInvoiceItems = new List<ProposedInvoiceItems>()
            {
                new ProposedInvoiceItems() 
                {
                    OutArInvoiceItemArCodes = "MATFE",
                    OutArInvoiceItemChargeAmts = 500m,
                    OutArInvoiceItemCreditAmts = null,
                    OutArInvoiceItemDescs = "Materials Fee",
                    OutArInvoiceItemIds = "123",
                    OutArInvoiceItemInvoiceIds = "2313"
                },
                new ProposedInvoiceItems() 
                {
                    OutArInvoiceItemArCodes = "MATFE",
                    OutArInvoiceItemChargeAmts = 400m,
                    OutArInvoiceItemCreditAmts = null,
                    OutArInvoiceItemDescs = "Materials Fee",
                    OutArInvoiceItemIds = "124",
                    OutArInvoiceItemInvoiceIds = "2313"
                },
                new ProposedInvoiceItems() 
                {
                    OutArInvoiceItemArCodes = "MATFE",
                    OutArInvoiceItemChargeAmts = 400m,
                    OutArInvoiceItemCreditAmts = null,
                    OutArInvoiceItemDescs = "Materials Fee",
                    OutArInvoiceItemIds = "125",
                    OutArInvoiceItemInvoiceIds = "2313"
                },
                new ProposedInvoiceItems() 
                {
                    OutArInvoiceItemArCodes = "MATFE",
                    OutArInvoiceItemChargeAmts = 300m,
                    OutArInvoiceItemCreditAmts = null,
                    OutArInvoiceItemDescs = "Materials Fee",
                    OutArInvoiceItemIds = "126",
                    OutArInvoiceItemInvoiceIds = "2313"
                }
            },
            ProposedPaymentPlanCharges = new List<ProposedPaymentPlanCharges>()
            {
                new ProposedPaymentPlanCharges() 
                {
                    OutPlanItemAmounts = 500m,
                    OutPlanItemAutoModFlags = "Y",
                    OutPlanItemInvItemIds = "20*123",
                    OutPlanItemSetupChgFlags = "N"
                },
                new ProposedPaymentPlanCharges() 
                {
                    OutPlanItemAmounts = 400m,
                    OutPlanItemAutoModFlags = "Y",
                    OutPlanItemInvItemIds = "20*124",
                    OutPlanItemSetupChgFlags = "N"
                },
                new ProposedPaymentPlanCharges() 
                {
                    OutPlanItemAmounts = 400m,
                    OutPlanItemAutoModFlags = "Y",
                    OutPlanItemInvItemIds = "20*125",
                    OutPlanItemSetupChgFlags = "N"
                },
                new ProposedPaymentPlanCharges() 
                {
                    OutPlanItemAmounts = 300m,
                    OutPlanItemAutoModFlags = "Y",
                    OutPlanItemInvItemIds = "20*126",
                    OutPlanItemSetupChgFlags = "N"
                }
            },
            ProposedScheduledPayments = new List<ProposedScheduledPayments>()
            {
                new ProposedScheduledPayments() 
                {
                    OutSchedPmtAmts = 400m,
                    OutSchedPmtDueDates = DateTime.Today.AddDays(3),
                },
                new ProposedScheduledPayments() 
                {
                    OutSchedPmtAmts = 400m,
                    OutSchedPmtDueDates = DateTime.Today.AddDays(13),
                },
                new ProposedScheduledPayments() 
                {
                    OutSchedPmtAmts = 400m,
                    OutSchedPmtDueDates = DateTime.Today.AddDays(23),
                },
                new ProposedScheduledPayments() 
                {
                    OutSchedPmtAmts = 400m,
                    OutSchedPmtDueDates = DateTime.Today.AddDays(33),
                }              
            }
        };

        static List<string> acknowledgementText = new List<string>() { "Review the terms and conditions below" };
        static List<PlanSchedule> planSchedules = new List<PlanSchedule>() { new PlanSchedule(new DateTime(2021, 12, 1), 2288.34m), new PlanSchedule(new DateTime(2022, 1, 19), 2263.33m), new PlanSchedule(new DateTime(2022, 2, 19), 2263.33m) };

        static TimeSpan localOffset = new TimeSpan((DateTime.Now - DateTime.UtcNow).Hours, (DateTime.Now - DateTime.UtcNow).Minutes, 0);
        static DateTimeOffset ackDateTime1 = new DateTimeOffset(2024, 1, 1, 13, 30, 45, localOffset);
        static DateTimeOffset ackDateTime2 = new DateTimeOffset(2024, 2, 2, 14, 20, 25, localOffset);
        static DateTime downPaymentDate1 = new DateTime(2022, 1, 19);
        static DateTimeOffset approvalDate1 = new DateTimeOffset(2024, 1, 1, 13, 31, 55, localOffset);
        static DateTime downPaymentDate2 = new DateTime(2022, 1, 19);
        static DateTimeOffset approvalDate2 = new DateTimeOffset(2024, 1, 1, 13, 31, 55, localOffset);

        static PaymentPlan proposedPlan1 = new PaymentPlan(null, "DEFAULT", "0000304", "01", "2022/SP", 6790.00m, downPaymentDate1, null, null, null)
        {
            Frequency = PlanFrequency.Monthly,
            NumberOfPayments = 2,
            SetupAmount = 25.00m,
            SetupPercentage = 0m,
            DownPaymentPercentage = 33.33m,
            GraceDays = 0,
            LateChargeAmount = 20.00m,
            LateChargePercentage = 0m
        };
        static PlanStatus planStatus1 = new PlanStatus(PlanStatusType.Open, DateTime.Today);
        static List<PlanStatus> planStatuses1 = new List<PlanStatus>() { planStatus1 };
        static Charge charge1 = new Charge("1", "1", new List<string>() { "Tuition & Fees" }, "TUIF", 6790.00m);
        static Charge charge2 = new Charge("11", "2", new List<string>() { "Setup charge" }, "PLSU", 25.00m);
        static PlanCharge proposedCharge1 = new PlanCharge("", charge1, 6790.00m, false, true);
        static PlanCharge proposedCharge2 = new PlanCharge("", charge2, 25.00m, true, true);
        static List<PlanCharge> proposedCharges1 = new List<PlanCharge>() { proposedCharge1, proposedCharge2 };

        static PlanInvoiceItems planInvoiceItem1 = new PlanInvoiceItems() { PlanInvoiceItemIds = proposedCharge1.Charge.Id, PlanInvoices = proposedCharge1.Charge.InvoiceId, PlanInvoiceItemBalances = proposedCharge1.Charge.Amount, PlanInvoiceItemPlanAmounts = proposedCharge1.Amount };
        static PlanInvoiceItems planInvoiceItem2 = new PlanInvoiceItems() { PlanInvoiceItemIds = proposedCharge2.Charge.Id, PlanInvoices = proposedCharge2.Charge.InvoiceId, PlanInvoiceItemBalances = proposedCharge2.Charge.Amount, PlanInvoiceItemPlanAmounts = proposedCharge2.Amount };
        static List<PlanInvoiceItems> validTermsRequestCharges = new List<PlanInvoiceItems>() { planInvoiceItem1, planInvoiceItem2 };

        static ScheduledPayment proposedPayment1 = new ScheduledPayment("", "", 2288.34m, new DateTime(2021, 12, 1), 0m, null);
        static ScheduledPayment proposedPayment2 = new ScheduledPayment("", "", 2263.33m, new DateTime(2022, 1, 19), 0m, null);
        static ScheduledPayment proposedPayment3 = new ScheduledPayment("", "", 2263.33m, new DateTime(2022, 2, 19), 0m, null);
        static List<ScheduledPayment> proposedPayments1 = new List<ScheduledPayment>() { proposedPayment1, proposedPayment2, proposedPayment3 };

        static ScheduledPayments validTermsRequestSchedule1 = new ScheduledPayments() { ScheduledPaymentDates = proposedPayment1.DueDate, ScheduledPaymentAmounts = proposedPayment1.Amount };
        static ScheduledPayments validTermsRequestSchedule2 = new ScheduledPayments() { ScheduledPaymentDates = proposedPayment2.DueDate, ScheduledPaymentAmounts = proposedPayment2.Amount };
        static ScheduledPayments validTermsRequestSchedule3 = new ScheduledPayments() { ScheduledPaymentDates = proposedPayment3.DueDate, ScheduledPaymentAmounts = proposedPayment3.Amount };
        static List<ScheduledPayments> validTermsRequestSchedule = new List<ScheduledPayments>() { validTermsRequestSchedule1, validTermsRequestSchedule2, validTermsRequestSchedule3 };

        static PaymentPlanTermsAcceptance termsAcceptance1 = new PaymentPlanTermsAcceptance(proposedPlan1.PersonId, ackDateTime1, "Wail Khalil", proposedPlan1, 2263.11m,
            downPaymentDate1, approvalDate1, "wkhalil", new List<string>() { "I hereby agree to the terms and conditions." }, "2595") { PaymentControlId = "1234", AcknowledgementText = acknowledgementText };
        static ApprovalDocument ackDocument = new ApprovalDocument("1", acknowledgementText);
        static ApprovalResponse termsResponse = new ApprovalResponse("456", "345", termsAcceptance1.StudentId, termsAcceptance1.ApprovalUserId,
            termsAcceptance1.ApprovalReceived.UtcDateTime, true);
        static ApprovePaymentPlanTermsRequest validTermsRequest = new ApprovePaymentPlanTermsRequest()
        {
            AcknowledgementDate = termsAcceptance1.AcknowledgementDateTime.ToLocalTime().DateTime,
            AcknowledgementTime = termsAcceptance1.AcknowledgementDateTime.ToLocalTime().DateTime,
            AcknowledgementText = termsAcceptance1.AcknowledgementText,
            ApprovalDate = termsAcceptance1.ApprovalReceived.ToLocalTime().DateTime,
            ApprovalTime = termsAcceptance1.ApprovalReceived.ToLocalTime().DateTime,
            ApprovalUserid = termsAcceptance1.ApprovalUserId,
            PaymentControlId = termsAcceptance1.PaymentControlId,
            PlanDownPaymentAmount = termsAcceptance1.DownPaymentAmount,
            PlanDownPaymentDate = termsAcceptance1.DownPaymentDate,
            PlanFirstPaymentDate = termsAcceptance1.ProposedPlan.FirstDueDate,
            PlanFrequency = ConvertFromPlanFrequency(termsAcceptance1.ProposedPlan.Frequency),
            PlanGraceDays = termsAcceptance1.ProposedPlan.GraceDays,
            PlanLateChargeAmount = termsAcceptance1.ProposedPlan.LateChargeAmount,
            PlanLateChargePercent = termsAcceptance1.ProposedPlan.LateChargePercentage,
            PlanNumberOfPayments = termsAcceptance1.ProposedPlan.NumberOfPayments,
            PlanReceivableType = termsAcceptance1.ProposedPlan.ReceivableTypeCode,
            PlanSetupChargeAmount = termsAcceptance1.ProposedPlan.TotalSetupChargeAmount,
            PlanTemplateId = termsAcceptance1.ProposedPlan.TemplateId,
            PlanTerm = termsAcceptance1.ProposedPlan.TermId,
            PlanTotalAmount = termsAcceptance1.ProposedPlan.OriginalAmount,
            StudentId = termsAcceptance1.StudentId,
            StudentName = termsAcceptance1.StudentName,
            TermsText = termsAcceptance1.TermsText.ToList(),
            PlanInvoiceItems = validTermsRequestCharges,
            ScheduledPayments = validTermsRequestSchedule
        };
        static ApprovePaymentPlanTermsResponse validTermsResponse = new ApprovePaymentPlanTermsResponse()
        {
            PayPlanApprovalsId = "123",
            AcknowledgementDocumentId = "234",
            ErrorMsg = null,
            PayPlansId = "1",
            TermsDocumentId = "345",
            TermsResponseId = "456"
        };
        static PaymentPlanApproval termsApproval1 = new PaymentPlanApproval(validTermsResponse.PayPlanApprovalsId, termsAcceptance1.StudentId, termsAcceptance1.StudentName,
            termsAcceptance1.AcknowledgementDateTime, termsAcceptance1.ProposedPlan.TemplateId, validTermsResponse.PayPlansId,
            validTermsResponse.TermsResponseId, validTermsRequest.PlanTotalAmount.Value, planSchedules) { PaymentControlId = termsAcceptance1.PaymentControlId, AcknowledgementDocumentId = validTermsResponse.AcknowledgementDocumentId };
        static PayPlanApprovals planApproval1 = new PayPlanApprovals()
        {
            Recordkey = validTermsResponse.PayPlanApprovalsId,
            PpaAckApprovalDocument = validTermsResponse.AcknowledgementDocumentId,
            PpaPayPlan = validTermsResponse.PayPlansId,
            PpaTermsApprDocResponse = validTermsResponse.TermsResponseId,
            PpaAckDate = validTermsRequest.AcknowledgementDate,
            PpaAckTime = validTermsRequest.AcknowledgementTime,
            PpaDownPaymentAmt = validTermsRequest.PlanDownPaymentAmount,
            PpaDownPaymentDate = validTermsRequest.PlanDownPaymentDate,
            PpaFrequency = validTermsRequest.PlanFrequency,
            PpaGraceDays = validTermsRequest.PlanGraceDays,
            PpaIpcRegistration = validTermsRequest.PaymentControlId,
            PpaLateChargeAmt = validTermsRequest.PlanLateChargeAmount,
            PpaLateChargePct = validTermsRequest.PlanLateChargePercent,
            PpaNumberOfPayments = validTermsRequest.PlanNumberOfPayments,
            PpaSetupChargeAmt = validTermsRequest.PlanSetupChargeAmount,
            PpaStudentName = validTermsRequest.StudentName,
            PpaTemplateId = validTermsRequest.PlanTemplateId,
            PpaTotalPlanAmt = validTermsRequest.PlanTotalAmount,
            PpaDueDate = validTermsRequest.ScheduledPayments.Select(x => x.ScheduledPaymentDates).ToList(),
            PpaDueAmount = validTermsRequest.ScheduledPayments.Select(x => x.ScheduledPaymentAmounts).ToList(),
            PpaStudent = validTermsRequest.StudentId
        };


        static PaymentPlan proposedPlan2 = new PaymentPlan(null, "WMK", "0010456", "01", "2024/SP", 550.00m, new DateTime(2024, 4, 1), null, new List<ScheduledPayment>(), new List<PlanCharge>()) { Frequency = PlanFrequency.Custom, NumberOfPayments = 3, SetupAmount = 60.00m, SetupPercentage = 0m, DownPaymentPercentage = 10.00m, GraceDays = 3, LateChargeAmount = 5.00m, LateChargePercentage = 0m };
        static PaymentPlanTermsAcceptance termsAcceptance2 = new PaymentPlanTermsAcceptance("0000304", ackDateTime2, "Jason Mansfield", proposedPlan2, 2263.11m,
            downPaymentDate2, approvalDate2, "jmansfield", new List<string>() { "I hereby agree to the terms and conditions." }, "2595") { PaymentControlId = "1234", AcknowledgementText = acknowledgementText };
        static ApprovePaymentPlanTermsRequest errorTermsRequest = new ApprovePaymentPlanTermsRequest()
        {
            AcknowledgementDate = termsAcceptance2.AcknowledgementDateTime.ToLocalTime().DateTime,
            AcknowledgementTime = termsAcceptance2.AcknowledgementDateTime.ToLocalTime().DateTime,
            AcknowledgementText = termsAcceptance2.AcknowledgementText,
            ApprovalDate = termsAcceptance2.ApprovalReceived.ToLocalTime().DateTime,
            ApprovalTime = termsAcceptance2.ApprovalReceived.ToLocalTime().DateTime,
            ApprovalUserid = termsAcceptance2.ApprovalUserId,
            PaymentControlId = termsAcceptance2.PaymentControlId,
            PlanDownPaymentAmount = termsAcceptance2.DownPaymentAmount,
            PlanDownPaymentDate = termsAcceptance2.DownPaymentDate,
            PlanFirstPaymentDate = termsAcceptance2.ProposedPlan.FirstDueDate,
            PlanFrequency = ConvertFromPlanFrequency(termsAcceptance2.ProposedPlan.Frequency),
            PlanGraceDays = termsAcceptance2.ProposedPlan.GraceDays,
            PlanLateChargeAmount = termsAcceptance2.ProposedPlan.LateChargeAmount,
            PlanLateChargePercent = termsAcceptance2.ProposedPlan.LateChargePercentage,
            PlanNumberOfPayments = termsAcceptance2.ProposedPlan.NumberOfPayments,
            PlanReceivableType = termsAcceptance2.ProposedPlan.ReceivableTypeCode,
            PlanSetupChargeAmount = termsAcceptance2.ProposedPlan.TotalSetupChargeAmount,
            PlanTemplateId = termsAcceptance2.ProposedPlan.TemplateId,
            PlanTerm = termsAcceptance2.ProposedPlan.TermId,
            PlanTotalAmount = termsAcceptance2.ProposedPlan.OriginalAmount,
            StudentId = termsAcceptance2.StudentId,
            StudentName = termsAcceptance2.StudentName,
            TermsText = termsAcceptance2.TermsText.ToList()
        };
        static ApprovePaymentPlanTermsResponse errorTermsResponse = new ApprovePaymentPlanTermsResponse()
        {
            AcknowledgementDocumentId = null,
            ErrorMsg = "Could not accept the terms.",
            PayPlanApprovalsId = null,
            PayPlansId = null,
            TermsDocumentId = null,
            TermsResponseId = null
        };

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            if (termsAcceptance1.ProposedPlan.PlanCharges.Count == 0)
            {
                foreach (var planCharge in proposedCharges1)
                {
                    termsAcceptance1.ProposedPlan.AddPlanCharge(planCharge);
                }
            }
            if (termsAcceptance1.ProposedPlan.ScheduledPayments.Count == 0)
            {
                foreach (var scheduledPayment in proposedPayments1)
                {
                    termsAcceptance1.ProposedPlan.AddScheduledPayment(scheduledPayment);
                }
            }

            // Add the "new" approval record to the collection if it isn't already there
            if (payPlanApprovals.Count == 0 || !payPlanApprovals.Select(x => x.Recordkey).Contains(planApproval1.Recordkey))
            {
                planApproval1.buildAssociations();
                payPlanApprovals.Add(planApproval1);
            }

            // Initialize Mock framework
            MockInitialize();

            // Build the test repository
            this.repository = new PaymentPlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
            _colleagueTimeZone = apiSettings.ColleagueTimeZone;
        }

        #region Tests of PaymentPlanTemplates

        [TestClass]
        public class PaymentPlanRepository_PaymentPlanTemplates : PaymentPlanRepositoryTests
        {
            [TestInitialize]
            public void Initialize_GetTemplate()
            {
                base.Initialize();
                SetupPaymentPlanTemplates();
            }

            [TestMethod]
            public void PaymentPlanRepository_PaymentPlanTemplates_Verify()
            {
                var result = this.repository.PaymentPlanTemplates.ToList();
                Assert.AreEqual(payPlanTemplates.Count, result.Count);
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    Assert.AreEqual(payPlanTemplates[i].Recordkey, result[i].Id);
                }
            }

            [TestMethod]
            public void PaymentPlanRepository_PaymentPlanTemplates_Corrupted()
            {
                var corruptTemplates = TestPayPlanTemplatesRepository.PayPlanTemplates;
                PayPlanTemplates corrupt = new PayPlanTemplates() { Recordkey = "CORRUPT" };
                corruptTemplates.Insert(0, corrupt);
                dataReaderMock.Setup<Collection<PayPlanTemplates>>(
                    reader => reader.BulkReadRecord<PayPlanTemplates>(string.Empty, It.IsAny<bool>())).Returns(corruptTemplates);

                this.repository = new PaymentPlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);

                var result = this.repository.PaymentPlanTemplates.ToList();
                Assert.AreEqual(corruptTemplates.Count - 1, result.Count);
                corruptTemplates.Remove(corrupt);
            }

            [TestMethod]
            public void PaymentPlanRepository_PaymentPlanTemplates_NoTemplatesExist()
            {
                dataReaderMock.Setup<Collection<PayPlanTemplates>>(
                    reader => reader.BulkReadRecord<PayPlanTemplates>(string.Empty, It.IsAny<bool>())).Returns(new Collection<PayPlanTemplates>());
                base.Initialize();
                var result = this.repository.PaymentPlanTemplates.ToList();
                Assert.AreEqual(0, result.Count);
            }

            [TestMethod]
            public void PaymentPlanRepository_PaymentPlanTemplates_VerifyCache()
            {
                string cacheKey = this.repository.BuildFullCacheKey("PaymentPlanTemplates");
                cacheProviderMock.Setup(x => x.Contains(cacheKey, null)).Returns(false);
                cacheProviderMock.Setup(x => x.Get(cacheKey, null)).Returns(null);

                // Make sure we can verify that it's in the cache
                cacheProviderMock.Setup(x => x.Add(cacheKey, It.IsAny<IEnumerable<PaymentPlanTemplate>>(), It.IsAny<CacheItemPolicy>(), null)).Verifiable();

                // Get the templates
                var result = this.repository.PaymentPlanTemplates;

                // Verify that the config is now in the cache
                cacheProviderMock.Verify(x => x.Add(cacheKey, It.IsAny<IEnumerable<PaymentPlanTemplate>>(), It.IsAny<CacheItemPolicy>(), null));
            }
        }

        #endregion

        #region Tests of GetTemplate

        [TestClass]
        public class PaymentPlanRepository_GetTemplate : PaymentPlanRepositoryTests
        {
            [TestInitialize]
            public void Initialize_GetTemplate()
            {
                base.Initialize();
                SetupPaymentPlanTemplates();
            }

            [TestCleanup]
            public void Cleanup_GetTemplate()
            {
                SetupPaymentPlanTemplates();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetTemplate_NullTemplateId()
            {
                var result = this.repository.GetTemplate(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetTemplate_EmptyTemplateId()
            {
                var result = this.repository.GetTemplate(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void PaymentPlanRepository_GetTemplate_InvalidTemplateId()
            {
                var result = this.repository.GetTemplate("INVALIDID");
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplates_AllTemplates()
            {
                var result = this.repository.PaymentPlanTemplates;

                Assert.AreEqual(TestPayPlanTemplatesRepository.PayPlanTemplates.Count, result.Count());
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplate_VerifyTemplateId()
            {
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    var source = payPlanTemplates[i];
                    var result = this.repository.GetTemplate(source.Recordkey);
                    Assert.AreEqual(source.Recordkey, result.Id, "IDs do not match for Template " + source.Recordkey);
                }
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplate_VerifyActiveFlag()
            {
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    var source = payPlanTemplates[i];
                    var result = this.repository.GetTemplate(source.Recordkey);
                    Assert.AreEqual((source.PptActiveFlag == "Y"), result.IsActive, "Active flags do not match for Template " + source.Recordkey);
                }
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplate_VerifyDescription()
            {
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    var source = payPlanTemplates[i];
                    var result = this.repository.GetTemplate(source.Recordkey);
                    Assert.AreEqual(source.PptDescription, result.Description, "Descriptions do not match for Template " + source.Recordkey);
                }
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplate_VerifyFrequency()
            {
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    var source = payPlanTemplates[i];
                    var result = this.repository.GetTemplate(source.Recordkey);
                    Assert.AreEqual(ConvertToPlanFrequency(source.PptFrequency), result.Frequency, "Frequencies do not match for Template " + source.Recordkey);
                }
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplate_VerifyMinimumPlanAmount()
            {
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    var source = payPlanTemplates[i];
                    var result = this.repository.GetTemplate(source.Recordkey);
                    Assert.AreEqual(source.PptMinPlanAmt, result.MinimumPlanAmount, "Minimum Plan Amounts do not match for Template " + source.Recordkey);
                }
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplate_VerifyIncludeSetupFeeInFirstPayment()
            {
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    var source = payPlanTemplates[i];
                    var result = this.repository.GetTemplate(source.Recordkey);
                    Assert.AreEqual((source.PptPrepaySetupFlag == "Y"), result.IncludeSetupChargeInFirstPayment,
                        "Included Setup Fee in First Payment flags do not match for Template " + source.Recordkey);
                }
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplate_VerifySubtractAnticipatedFinancialAid()
            {
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    var source = payPlanTemplates[i];
                    var result = this.repository.GetTemplate(source.Recordkey);
                    Assert.AreEqual((source.PptSubtractAnticipatedFa == "Y"), result.SubtractAnticipatedFinancialAid,
                        "Subtracts Anticipated Financial Aid flags do not match for Template " + source.Recordkey);
                }
            }

            [TestMethod]
            public void PaymentPlanRepository_GetTemplate_VerifyCalculatePlanAmountAutomatically()
            {
                for (int i = 0; i < payPlanTemplates.Count; i++)
                {
                    var source = payPlanTemplates[i];
                    var result = this.repository.GetTemplate(source.Recordkey);
                    Assert.AreEqual((source.PptCalcAmtFlag == "Y"), result.CalculatePlanAmountAutomatically,
                        "Calculates Plan Amount Automatically flags do not match for Template " + source.Recordkey);
                }
            }
        }

        #endregion

        #region Tests of GetPaymentPlan

        [TestClass]
        public class PaymentPlanRepository_GetPaymentPlan : PaymentPlanRepositoryTests
        {
            [TestInitialize]
            public void Initialize_GetPaymentPlan()
            {
                base.Initialize();
                SetupPaymentPlans();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPaymentPlan_NullTemplateId()
            {
                var result = this.repository.GetPaymentPlan(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPaymentPlan_EmptyTemplateId()
            {
                var result = this.repository.GetPaymentPlan(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void PaymentPlanRepository_GetPaymentPlan_InvalidPlanId()
            {
                var result = this.repository.GetPaymentPlan("INVALIDID");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void PaymentPlanRepository_GetPaymentPlan_InvalidPlanStatus()
            {
                var result = this.repository.GetPaymentPlan("3333");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentOutOfRangeException))]
            public void PaymentPlanRepository_GetPaymentPlan_InvalidPlanFrequency()
            {
                var result = this.repository.GetPaymentPlan("4444");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPaymentPlan_NullPlanStatus()
            {
                var result = this.repository.GetPaymentPlan("6666");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPaymentPlan_NullPlanFrequency()
            {
                var result = this.repository.GetPaymentPlan("5555");
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyId()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(planId1, result.Id);
                Assert.AreEqual(planId2, result2.Id);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyTemplateId()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplPayPlanTemplate, result.TemplateId);
                Assert.AreEqual(source2.ArplPayPlanTemplate, result2.TemplateId);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyPersonId()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplPersonId, result.PersonId);
                Assert.AreEqual(source2.ArplPersonId, result2.PersonId);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyReceivableTypeCode()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplArType, result.ReceivableTypeCode);
                Assert.AreEqual(source2.ArplArType, result2.ReceivableTypeCode);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyCurrentAmount()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplAmt, result.CurrentAmount);
                Assert.AreEqual(source2.ArplAmt, result2.CurrentAmount);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyFirstDueDate()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplFirstDueDate.GetValueOrDefault().Date, result.FirstDueDate.Date);
                Assert.AreEqual(source2.ArplFirstDueDate.GetValueOrDefault().Date, result2.FirstDueDate.Date);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyOriginalAmount()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplOrigAmt, result.OriginalAmount);
                Assert.AreEqual(source2.ArplOrigAmt, result2.OriginalAmount);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyFrequency()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                var result3 = this.repository.GetPaymentPlan(planId3);
                var result4 = this.repository.GetPaymentPlan(planId4);
                Assert.AreEqual(ConvertToPlanFrequency(source1.ArplFrequency), result.Frequency);
                Assert.AreEqual(ConvertToPlanFrequency(source2.ArplFrequency), result2.Frequency);
                Assert.AreEqual(ConvertToPlanFrequency(source3.ArplFrequency), result3.Frequency);
                Assert.AreEqual(ConvertToPlanFrequency(source4.ArplFrequency), result4.Frequency);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyNumberOfPayments()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplNoPayments, result.NumberOfPayments);
                Assert.AreEqual(source2.ArplNoPayments, result2.NumberOfPayments);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifySetupChargeAmount()
            {
                decimal setup1 = Math.Round((source1.ArplOrigAmt ?? 0) * (source1.ArplChargePct ?? 0) / 100, 2, MidpointRounding.AwayFromZero) + source1.ArplChargeAmt ?? 0;
                decimal setup2 = Math.Round((source2.ArplOrigAmt ?? 0) * (source2.ArplChargePct ?? 0) / 100, 2, MidpointRounding.AwayFromZero) + source2.ArplChargeAmt ?? 0;
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplChargeAmt, result.SetupAmount);
                Assert.AreEqual(source2.ArplChargeAmt, result2.SetupAmount);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifySetupPercentage()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplChargePct ?? 0, result.SetupPercentage);
                Assert.AreEqual(source2.ArplChargePct ?? 0, result2.SetupPercentage);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyDownPaymentPercentage()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplDownPayPct, result.DownPaymentPercentage);
                Assert.AreEqual(source2.ArplDownPayPct, result2.DownPaymentPercentage);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyGraceDays()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplGraceNoDays, result.GraceDays);
                Assert.AreEqual(source2.ArplGraceNoDays, result2.GraceDays);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyLateChargeAmount()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplLateChargeAmt, result.LateChargeAmount);
                Assert.AreEqual(source2.ArplLateChargeAmt, result2.LateChargeAmount);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPaymentPlan_VerifyLatePercentage()
            {
                var result = this.repository.GetPaymentPlan(planId1);
                var result2 = this.repository.GetPaymentPlan(planId2);
                Assert.AreEqual(source1.ArplPlanLateChrgPct ?? 0, result.LateChargePercentage);
                Assert.AreEqual(source2.ArplPlanLateChrgPct ?? 0, result2.LateChargePercentage);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void PaymentPlanRepository_GetPaymentPlan_ScheduledPaymentMismatch()
            {
                // Set up corrupt AR.PAY.PLAN.ITEMS
                dataReaderMock.Setup<Collection<ArPayPlanItems>>(
                    reader => reader.BulkReadRecord<ArPayPlanItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                        .Returns<string[], bool>((ids, flag) =>
                        {
                            var planItems = new List<ArPayPlanItems>();
                            foreach (string id in ids)
                            {
                                var item = arPayPlanItems.FirstOrDefault(x => x.Recordkey == id);
                                if (item != null)
                                {
                                    planItems.Add(item);
                                }
                            }
                            planItems.RemoveAt(0);
                            return new Collection<ArPayPlanItems>(planItems);
                        }
                    );

                var result = this.repository.GetPaymentPlan("1111");
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPaymentPlan_NullScheduledPayments()
            {
                // Set up corrupt AR.PAY.PLAN.ITEMS
                dataReaderMock.Setup<Collection<ArPayPlanItems>>(
                    reader => reader.BulkReadRecord<ArPayPlanItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                        .Returns<string[], bool>((ids, flag) =>
                        {
                            var planItems = new List<ArPayPlanItems>();
                            foreach (string id in ids)
                            {
                                var item = arPayPlanItems.FirstOrDefault(x => x.Recordkey == id);
                                if (item != null)
                                {
                                    planItems.Add(item);
                                }
                            }
                            planItems[0] = null;
                            return new Collection<ArPayPlanItems>(planItems);
                        }
                    );

                var result = this.repository.GetPaymentPlan("1111");
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void PaymentPlanRepository_GetPaymentPlan_PlanItemsMismatch()
            {
                // AR.INVOICE.ITEMS
                dataReaderMock.Setup<Collection<DataContracts.ArInvoiceItems>>(
                    reader => reader.BulkReadRecord<DataContracts.ArInvoiceItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                        .Returns<string[], bool>((ids, flag) =>
                        {
                            var planCharges = new List<DataContracts.ArInvoiceItems>();
                            foreach (string id in ids)
                            {
                                var item = arInvoiceItems.FirstOrDefault(x => x.Recordkey == id);
                                if (item != null)
                                {
                                    planCharges.Add(item);
                                }
                            }
                            planCharges.RemoveAt(0);
                            return new Collection<DataContracts.ArInvoiceItems>(planCharges);
                        }
                    );
                var result = this.repository.GetPaymentPlan("1111");
            }
        }

        #endregion

        #region Tests of GetPlanCustomScheduleDates

        [TestClass]
        public class PaymentPlanRepository_GetPlanCustomScheduleDates : PaymentPlanRepositoryTests
        {
            [TestInitialize]
            public void Initialize_GetPlanCustomScheduleDates()
            {
                //base.Initialize();
                SetupPaymentPlanTemplates();
                SetupCustomSchedules();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_NullPersonId()
            {
                var result = this.repository.GetPlanCustomScheduleDates(null, receivableType, termId, templateId, downPaymentDate, firstPaymentDate, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_EmptyPersonId()
            {
                var result = this.repository.GetPlanCustomScheduleDates(string.Empty, receivableType, termId, templateId, downPaymentDate, firstPaymentDate, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_NullReceivableType()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, null, termId, templateId, downPaymentDate, firstPaymentDate, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_EmptyReceivableType()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, string.Empty, termId, templateId, downPaymentDate, firstPaymentDate, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_NullTermId()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, receivableType, null, templateId, downPaymentDate, firstPaymentDate, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_EmptyTermId()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, receivableType, string.Empty, templateId, downPaymentDate, firstPaymentDate, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_NullTemplateId()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, receivableType, termId, null, downPaymentDate, firstPaymentDate, null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_EmptyTemplateId()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, receivableType, termId, string.Empty, downPaymentDate, firstPaymentDate, null);
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_RequestReturnsValidListOfDatesWithDownPayment()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, receivableType, termId, templateId, downPaymentDate, firstPaymentDate, null);
                var source = new List<DateTime?>();
                source.Add(downPaymentDate);
                source.Add(firstPaymentDate);
                source.AddRange(otherDates);

                Assert.AreEqual(source.Count, result.Count());
                CollectionAssert.AllItemsAreNotNull(result.ToList());
                CollectionAssert.AllItemsAreInstancesOfType(result.ToList(), typeof(DateTime));
                CollectionAssert.AreEqual(source, result.ToList());
            }

            [TestMethod]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_RequestReturnsValidListOfDatesNoDownPayment()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, receivableType, termId, templateId, null, firstPaymentDate, null);
                var source = new List<DateTime?>();
                source.Add(null);
                source.Add(firstPaymentDate);
                source.AddRange(otherDates);

                Assert.AreEqual(source.Count, result.Count());
                CollectionAssert.AreEqual(source, result.ToList());
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PaymentPlanRepository_GetPlanCustomScheduleDates_RequestReturnsErrorMessage()
            {
                var result = this.repository.GetPlanCustomScheduleDates(personId, errorReceivableType, termId, templateId, downPaymentDate, firstPaymentDate, null);
            }

        }

        #endregion

        #region Tests of ApprovePaymentPlanTerms

        [TestClass]
        public class PaymentPlanRepository_ApprovePaymentPlanTerms : PaymentPlanRepositoryTests
        {
            [TestInitialize]
            public void Initialize_ApprovePaymentPlanTerms()
            {
                base.Initialize();

                SetupPaymentPlans();
                SetupPaymentPlanApprovals();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_ApprovePaymentPlanTerms_NullAcceptance()
            {
                var result = this.repository.ApprovePaymentPlanTerms(null);
            }

            [TestMethod]
            [ExpectedException(typeof(InvalidOperationException))]
            public void PaymentPlanRepository_ApprovePaymentPlanTerms_ErrorResponse()
            {
                var result = this.repository.ApprovePaymentPlanTerms(termsAcceptance2);
            }

            [TestMethod]
            public void PaymentPlanRepository_ApprovePaymentPlanTerms_ValidResponse()
            {
                // First, build the response and add it to the setup

                var result = this.repository.ApprovePaymentPlanTerms(termsAcceptance1);

                Assert.AreEqual(termsApproval1.AcknowledgementDocumentId, result.AcknowledgementDocumentId);

                Assert.AreEqual(termsApproval1.PaymentPlanId, result.PaymentPlanId);
                Assert.AreEqual(termsApproval1.Id, result.Id);
                Assert.AreEqual(termsApproval1.PaymentControlId, result.PaymentControlId);
                Assert.AreEqual(termsApproval1.TermsResponseId, result.TermsResponseId);
                Assert.AreEqual(termsApproval1.Timestamp, result.Timestamp);
            }
        }

        #endregion

        #region Tests of GetPlanTermsApproval

        [TestClass]
        public class PaymentPlanRepository_GetPlanTermsApproval : PaymentPlanRepositoryTests
        {
            [TestInitialize]
            public void Initialize_GetPlanTermsApproval()
            {
                base.Initialize();
                SetupPaymentPlanApprovals();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanTermsApproval_NullId()
            {
                var result = this.repository.GetPaymentPlanApproval(null);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void PaymentPlanRepository_GetPlanTermsApproval_EmptyId()
            {
                var result = this.repository.GetPaymentPlanApproval(string.Empty);
            }

            [TestMethod]
            [ExpectedException(typeof(KeyNotFoundException))]
            public void PaymentPlanRepository_GetPlanTermsApproval_InvalidId()
            {
                var result = this.repository.GetPaymentPlanApproval("INVALID");
            }
        }

        #endregion

        #region Tests of GetProposedPaymentPlanAsync

        [TestClass]
        public class PaymentPlanRepository_GetProposedPaymentPlanAsync : PaymentPlanRepositoryTests
        {
            private string receivableTypeCode = "01";
            private decimal planAmount = 1000m;

            [TestInitialize]
            public void Initialize_GetProposedPaymentPlanAsync()
            {
                base.Initialize();
                SetupGetProposedPaymentPlanAsync();
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanRepository_GetProposedPaymentPlanAsync_PersonId_Null()
            {
                var result = await this.repository.GetProposedPaymentPlanAsync(null, termId, receivableTypeCode, templateId, firstPaymentDate, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanRepository_GetProposedPaymentPlanAsync_TermId_Null()
            {
                var result = await this.repository.GetProposedPaymentPlanAsync(personId, null, receivableTypeCode, templateId, firstPaymentDate, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanRepository_GetProposedPaymentPlanAsync_ReceivableTypeCode_Null()
            {
                var result = await this.repository.GetProposedPaymentPlanAsync(personId, termId, null, templateId, firstPaymentDate, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public async Task PaymentPlanRepository_GetProposedPaymentPlanAsync_TemplateId_Null()
            {
                var result = await this.repository.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, null, firstPaymentDate, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PaymentPlanRepository_GetProposedPaymentPlanAsync_Null_CtxResponse()
            {
                transManagerMock.Setup<Task<GetProposedPaymentPlanInfoResponse>>(trans =>
                    trans.ExecuteAsync<GetProposedPaymentPlanInfoRequest, GetProposedPaymentPlanInfoResponse>(It.IsAny<GetProposedPaymentPlanInfoRequest>()))
                        .Returns<GetProposedPaymentPlanInfoRequest>(req =>
                        {
                            GetProposedPaymentPlanInfoResponse response = null;
                            return Task.FromResult(response);
                        }
                    );
                this.repository = new PaymentPlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                _colleagueTimeZone = apiSettings.ColleagueTimeZone;
                var result = await this.repository.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, templateId, firstPaymentDate, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PaymentPlanRepository_GetProposedPaymentPlanAsync_ErrorMessage()
            {
                transManagerMock.Setup<Task<GetProposedPaymentPlanInfoResponse>>(trans =>
                    trans.ExecuteAsync<GetProposedPaymentPlanInfoRequest, GetProposedPaymentPlanInfoResponse>(It.IsAny<GetProposedPaymentPlanInfoRequest>()))
                        .Returns<GetProposedPaymentPlanInfoRequest>(req =>
                        {
                            GetProposedPaymentPlanInfoResponse response = new GetProposedPaymentPlanInfoResponse()
                            {
                                OutErrorOccurred = true,
                                OutErrorMessages = new List<string>() { "Error 1" }
                            };
                            return Task.FromResult(response);
                        }
                    );
                this.repository = new PaymentPlanRepository(cacheProviderMock.Object, transFactoryMock.Object, loggerMock.Object, apiSettings);
                _colleagueTimeZone = apiSettings.ColleagueTimeZone;
                var result = await this.repository.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, templateId, firstPaymentDate, planAmount);
            }

            [TestMethod]
            [ExpectedException(typeof(ApplicationException))]
            public async Task PaymentPlanRepository_GetProposedPaymentPlanAsync_DataError()
            {
                var result = await this.repository.GetProposedPaymentPlanAsync(personId, termId, "02", templateId, firstPaymentDate, planAmount);
            }

            [TestMethod]
            public async Task PaymentPlanRepository_GetProposedPaymentPlanAsync_Valid()
            {
                var result = await this.repository.GetProposedPaymentPlanAsync(personId, termId, receivableTypeCode, templateId, firstPaymentDate, planAmount);
                Assert.IsNotNull(result);
            }
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Converts a payment plan frequency string to the appropriate enumeration value
        /// </summary>
        /// <param name="frequency">Plan Frequency code</param>
        /// <returns>Payment Plan Frequency enumeration value</returns>
        private static PlanFrequency ConvertToPlanFrequency(string frequency)
        {
            if (string.IsNullOrEmpty(frequency))
            {
                throw new ArgumentNullException("frequency", "Payment Plan Template must have a frequency specified.");
            }

            switch (frequency)
            {
                case "W":
                    return PlanFrequency.Weekly;
                case "B":
                    return PlanFrequency.Biweekly;
                case "M":
                    return PlanFrequency.Monthly;
                case "Y":
                    return PlanFrequency.Yearly;
                case "C":
                    return PlanFrequency.Custom;
                default:
                    throw new ArgumentOutOfRangeException("frequency", "Frequency " + frequency + " is not valid.");
            }
        }

        /// <summary>
        /// Converts a payment plan frequency value to the corresponding code in Colleague
        /// </summary>
        /// <param name="frequency">Plan frequency</param>
        /// <returns>Frequency code</returns>
        private static string ConvertFromPlanFrequency(PlanFrequency frequency)
        {
            switch (frequency)
            {
                case PlanFrequency.Weekly:
                    return "W";
                case PlanFrequency.Biweekly:
                    return "B";
                case PlanFrequency.Monthly:
                    return "M";
                case PlanFrequency.Yearly:
                    return "Y";
                case PlanFrequency.Custom:
                    return "C";
                default:
                    throw new ArgumentOutOfRangeException("frequency", "Plan frequency " + frequency.ToString() + " is not valid.");
            }
        }

        #endregion

        #region Payment Plan Template setup

        /// <summary>
        /// Performs data setup for payment plan templates to be used in tests
        /// </summary>
        private void SetupPaymentPlanTemplates()
        {
            // Read one PAY.PLAN.TEMPLATES record
            dataReaderMock.Setup<PayPlanTemplates>(
                reader => reader.ReadRecord<PayPlanTemplates>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) => { return payPlanTemplates.FirstOrDefault(x => x.Recordkey == id); });

            // Read all PAY.PLAN.TEMPLATES records
            dataReaderMock.Setup<Collection<PayPlanTemplates>>(
                reader => reader.BulkReadRecord<PayPlanTemplates>(string.Empty, It.IsAny<bool>())).Returns(payPlanTemplates);
        }

        #endregion

        #region Payment Plans setup

        private void SetupPaymentPlans()
        {
            // Read an AR.PAY.PLANS record and its associated records
            dataReaderMock.Setup<ArPayPlans>(
                reader => reader.ReadRecord<ArPayPlans>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) => { return arPayPlans.FirstOrDefault(x => x.Recordkey == id); });
            // AR.PAY.PLAN.ITEMS
            dataReaderMock.Setup<Collection<ArPayPlanItems>>(
                reader => reader.BulkReadRecord<ArPayPlanItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var planItems = new List<ArPayPlanItems>();
                        foreach (string id in ids)
                        {
                            var item = arPayPlanItems.FirstOrDefault(x => x.Recordkey == id);
                            if (item != null)
                            {
                                planItems.Add(item);
                            }
                        }
                        return new Collection<ArPayPlanItems>(planItems);
                    }
                );
            // AR.INVOICE.ITEMS
            dataReaderMock.Setup<Collection<DataContracts.ArInvoiceItems>>(
                reader => reader.BulkReadRecord<DataContracts.ArInvoiceItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var planCharges = new List<DataContracts.ArInvoiceItems>();
                        foreach (string id in ids)
                        {
                            var item = arInvoiceItems.FirstOrDefault(x => x.Recordkey == id);
                            if (item != null)
                            {
                                planCharges.Add(item);
                            }
                        }
                        return new Collection<DataContracts.ArInvoiceItems>(planCharges);
                    }
                );
            // AR.PAY.INVOICE.ITEMS
            dataReaderMock.Setup<Collection<ArPayInvoiceItems>>(
                reader => reader.BulkReadRecord<ArPayInvoiceItems>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var planCharges = new List<ArPayInvoiceItems>();
                        foreach (string id in ids)
                        {
                            var item = arPayInvoiceItems.FirstOrDefault(x => x.Recordkey == id);
                            if (item != null)
                            {
                                planCharges.Add(item);
                            }
                        }
                        return new Collection<ArPayInvoiceItems>(planCharges);
                    }
                );
            // Set up corrupt AR.PAY.PLAN.ITEMS
            dataReaderMock.Setup<Collection<ArCodeTaxGlDistr>>(
                reader => reader.BulkReadRecord<ArCodeTaxGlDistr>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var taxes = new List<ArCodeTaxGlDistr>();
                        foreach (string id in ids)
                        {
                            var item = arCodeTaxGlDistrs.FirstOrDefault(x => x.Recordkey == id);
                            if (item != null)
                            {
                                taxes.Add(item);
                            }
                        }
                        return new Collection<ArCodeTaxGlDistr>(taxes);
                    }
                );
        }

        #endregion

        #region Payment Plan Approvals setup

        private void SetupPaymentPlanApprovals()
        {
            // Read a single record
            dataReaderMock.Setup<PayPlanApprovals>(
                reader => reader.ReadRecord<PayPlanApprovals>(It.IsAny<string>(), It.IsAny<bool>()))
                    .Returns<string, bool>((id, flag) => { return payPlanApprovals.FirstOrDefault(x => x.Recordkey == id); });
            // Read multiple records
            dataReaderMock.Setup<Collection<PayPlanApprovals>>(
                accessor => accessor.BulkReadRecord<PayPlanApprovals>(It.IsAny<string[]>(), It.IsAny<bool>()))
                    .Returns<string[], bool>((ids, flag) =>
                    {
                        var planApprovals = new Collection<PayPlanApprovals>();
                        foreach (var id in ids)
                        {
                            planApprovals.Add(payPlanApprovals.FirstOrDefault(x => x.Recordkey == id));
                        }
                        return planApprovals;
                    }
                );
            //// Setup for the approval response
            //dataReaderMock.Setup<PayPlanApprovals>(
            //    reader => reader.ReadRecord<PayPlanApprovals>(validTermsResponse.PayPlanApprovalsId, It.IsAny<bool>())).Returns(planApproval1);
            // If selection criteria were supplied, return everything
            dataReaderMock.Setup<Collection<PayPlanApprovals>>(
                reader => reader.BulkReadRecord<PayPlanApprovals>(It.IsAny<string>(), It.IsAny<bool>())).Returns(payPlanApprovals);

            transManagerMock.Setup<ApprovePaymentPlanTermsResponse>(
                trans => trans.Execute<ApprovePaymentPlanTermsRequest, ApprovePaymentPlanTermsResponse>(It.IsAny<ApprovePaymentPlanTermsRequest>()))
                    .Returns<ApprovePaymentPlanTermsRequest>(request =>
                    {
                        if (request.ApprovalUserid == "wkhalil")
                        {
                            return validTermsResponse;
                        }
                        return errorTermsResponse;
                    });

            //transManagerMock.Setup<ApprovePaymentPlanTermsResponse>(trans =>
            //    trans.Execute<ApprovePaymentPlanTermsRequest, ApprovePaymentPlanTermsResponse>(It.Is<ApprovePaymentPlanTermsRequest>(req => req.PlanFrequency != "C")))
            //        .Returns(validTermsResponse);
            //transManagerMock.Setup<ApprovePaymentPlanTermsResponse>(trans =>
            //    trans.Execute<ApprovePaymentPlanTermsRequest, ApprovePaymentPlanTermsResponse>(It.Is<ApprovePaymentPlanTermsRequest>(req => req.PlanFrequency == "C")))
            //        .Returns(errorTermsResponse);

        }

        #endregion

        #region Payment Plan Custom Schedules setup

        private void SetupCustomSchedules()
        {
            transManagerMock.Setup<GetPlanCustomScheduleDatesResponse>(trans =>
                trans.Execute<GetPlanCustomScheduleDatesRequest, GetPlanCustomScheduleDatesResponse>(It.IsAny<GetPlanCustomScheduleDatesRequest>()))
                    .Returns<GetPlanCustomScheduleDatesRequest>(req =>
                    {
                        var response = new GetPlanCustomScheduleDatesResponse();
                        if (req.ArType == errorReceivableType)
                        {
                            response = errorResponse;
                        }
                        else if (req.ArType == receivableType && req.DownPmtDate == null)
                        {
                            response = validResponseWithoutDownPayment;
                        }
                        else if (req.ArType == receivableType)
                        {
                            response = validResponseWithDownPayment;
                        }
                        return response;
                    }
                    );
        }

        #endregion

        #region GetProposedPaymentPlanAsync Setup

        private void SetupGetProposedPaymentPlanAsync()
        {
            transManagerMock.Setup<Task<GetProposedPaymentPlanInfoResponse>>(trans =>
                trans.ExecuteAsync<GetProposedPaymentPlanInfoRequest, GetProposedPaymentPlanInfoResponse>(It.IsAny<GetProposedPaymentPlanInfoRequest>()))
                    .Returns<GetProposedPaymentPlanInfoRequest>(req =>
                    {
                        var response = new GetProposedPaymentPlanInfoResponse();
                        if (req.InArType == errorReceivableType)
                        {
                            response = gpppirError;
                        }
                        else if (req.InArType == receivableType && req.InPayPlanTemplateId == "DEFAULT")
                        {
                            response = gpppirData;
                        }
                        else if (req.InArType == "02")
                        {
                            response = gpppirCorruptData;
                        }
                        return Task.FromResult(response);
                    }
                );

        }

        #endregion
    }
}