// Copyright 2014-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;

namespace Ellucian.Colleague.Data.Finance.Tests
{
    public static class TestPayPlanApprovalsRepository
    {
        private static Collection<PayPlanApprovals> _payPlanApprovals = new Collection<PayPlanApprovals>();
        public static Collection<PayPlanApprovals> PayPlanApprovals
        {
            get
            {
                if (_payPlanApprovals.Count == 0)
                {
                    GenerateDataContracts();
                }
                return _payPlanApprovals;
            }
        }

        /// <summary>
        /// Performs data setup for payment plans to be used in tests
        /// </summary>
        private static void GenerateDataContracts()
        {
            string[,] paymentPlanApprovalsData = GetPaymentPlanTermApprovalsData();
            int paymentPlanApprovalsCount = paymentPlanApprovalsData.Length / 19;
            for (int i = 0; i < paymentPlanApprovalsCount; i++)
            {
                // Parse out the data
                string id = paymentPlanApprovalsData[i, 0].Trim();
                string payControlId = paymentPlanApprovalsData[i, 1].Trim();
                string payPlanId = paymentPlanApprovalsData[i, 2].Trim();
                DateTime acknowledgementDate = DateTime.Parse(paymentPlanApprovalsData[i, 3].Trim());
                DateTime acknowledgementTime = DateTime.Parse(paymentPlanApprovalsData[i, 4].Trim());
                string templateId = paymentPlanApprovalsData[i, 5].Trim();
                decimal setupAmount = Decimal.Parse(paymentPlanApprovalsData[i, 6].Trim());
                decimal planAmount = Decimal.Parse(paymentPlanApprovalsData[i, 7].Trim());
                decimal lateAmount = Decimal.Parse(paymentPlanApprovalsData[i, 8].Trim());
                decimal latePercent = Decimal.Parse(paymentPlanApprovalsData[i, 9].Trim());
                decimal downPayAmount = Decimal.Parse(paymentPlanApprovalsData[i, 10].Trim());
                DateTime downPayDate = DateTime.Parse(paymentPlanApprovalsData[i, 11].Trim());
                string termsResponseId = paymentPlanApprovalsData[i, 12].Trim();
                string approvalDocId = paymentPlanApprovalsData[i, 13].Trim();
                string studentName = paymentPlanApprovalsData[i, 14].Trim();
                string frequency = paymentPlanApprovalsData[i, 15].Trim();
                int? numberOfPayments = !String.IsNullOrEmpty(paymentPlanApprovalsData[i, 16].Trim()) ? Int32.Parse(paymentPlanApprovalsData[i, 17]) : (Int32?)null;
                int? graceDays = !String.IsNullOrEmpty(paymentPlanApprovalsData[i, 17].Trim()) ? Int32.Parse(paymentPlanApprovalsData[i, 18]) : (Int32?)null;
                string studentId = paymentPlanApprovalsData[i, 18].Trim();

                PayPlanApprovals payPlanApproval = new PayPlanApprovals()
                {
                    Recordkey = id,
                    PpaIpcRegistration = payControlId,
                    PpaPayPlan = payPlanId,
                    PpaAckDate = acknowledgementDate,
                    PpaAckTime = acknowledgementTime,
                    PpaTemplateId = templateId,
                    PpaSetupChargeAmt = setupAmount,
                    PpaTotalPlanAmt = planAmount,
                    PpaLateChargeAmt = lateAmount,
                    PpaLateChargePct = latePercent,
                    PpaDownPaymentAmt = downPayAmount,
                    PpaDownPaymentDate = downPayDate,
                    PpaTermsApprDocResponse = termsResponseId,
                    PpaAckApprovalDocument = approvalDocId,
                    PpaStudentName = studentName,
                    PpaFrequency = frequency,
                    PpaNumberOfPayments = numberOfPayments,
                    PpaGraceDays = graceDays,
                    PpaStudent = studentId
                };

                string[,] paymentPlanApprovalScheduleData = GetTermApprovalsScheduleData();
                int scheduleCount = paymentPlanApprovalScheduleData.Length / 3;
                Dictionary<DateTime, decimal> scheduledPayments = new Dictionary<DateTime, decimal>();
                for (int j = 0; j < scheduleCount; j++)
                {
                    if (paymentPlanApprovalScheduleData[j, 0].Trim() == payPlanApproval.Recordkey)
                    {
                        DateTime payDate = DateTime.Parse(paymentPlanApprovalScheduleData[j, 1].Trim());
                        decimal payAmount = Decimal.Parse(paymentPlanApprovalScheduleData[j, 2].Trim());
                        scheduledPayments.Add(payDate, payAmount);
                    }
                }

                if (scheduledPayments != null && scheduledPayments.Count > 0)
                {
                    payPlanApproval.PpaSchedulesEntityAssociation = new List<PayPlanApprovalsPpaSchedules>();
                    foreach (var sp in scheduledPayments)
                    {
                        payPlanApproval.PpaSchedulesEntityAssociation.Add(new PayPlanApprovalsPpaSchedules()
                        {
                            PpaDueDateAssocMember = sp.Key,
                            PpaDueAmountAssocMember = sp.Value
                        });
                    }
                }
                _payPlanApprovals.Add(payPlanApproval);
            }
        }

        /// <summary>
        /// Gets payment plan term approvals raw data
        /// </summary>
        /// <returns>String array of raw payment plan term approvals data</returns>
        private static string[,] GetPaymentPlanTermApprovalsData()
        {
            string[,] paymentPlanApprovalsTable = {
                                                    {"123", "123","234","1/19/2022", "1/19/2022 12:34:56 PM", "DEFAULT", "50.00", "5000.00", "25.00", "2.50", "250.00", "1/21/2022", "345", "456", "Robert Ludlum", "W", "5", "3", "0003315"},
                                                    {"124", "124","235","2/19/2022", "2/19/2022 12:34:56 PM", "DEFAULT", "50.00", "5000.00", "25.00", "2.50", "250.00", "2/21/2022", "346", "457", "Johnny", "W", "5", "3", "0000895"}                                             
                                                  };
            return paymentPlanApprovalsTable;
        }

        /// <summary>
        /// Gets payment plan term approvals schedule raw data
        /// </summary>
        /// <returns>String array of raw payment plan term approvals schedule data</returns>
        private static string[,] GetTermApprovalsScheduleData()
        {
            string[,] paymentPlanApprovalsScheduleTable = {
                                                            {"123", "1/21/2022", "250.00"},
                                                            {"123", "1/28/2022", "950.00"},
                                                            {"123", "2/4/2022",  "950.00"},
                                                            {"123", "2/11/2022", "950.00"},
                                                            {"123", "2/18/2022", "950.00"},
                                                            {"123", "2/25/2022", "950.00"},
                                                            {"124", "2/21/2022", "250.00"},
                                                            {"124", "2/28/2022", "950.00"},
                                                            {"124", "3/4/2022",  "950.00"},
                                                            {"124", "3/11/2022", "950.00"},
                                                            {"124", "3/18/2022", "950.00"},
                                                            {"124", "3/25/2022", "950.00"},
                                                        };
            return paymentPlanApprovalsScheduleTable;
        }
    }
}
