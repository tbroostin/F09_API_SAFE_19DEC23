using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Data.Colleague;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestPaymentPlanApprovalRepository
    {
        private static List<PaymentPlanApproval> _payPlanApprovals = new List<PaymentPlanApproval>();
        public static List<PaymentPlanApproval> PayPlanApprovals
        {
            get
            {
                if (_payPlanApprovals.Count == 0)
                {
                    GenerateEntities();
                }
                return _payPlanApprovals;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var item in TestPayPlanApprovalsRepository.PayPlanApprovals)
            {
                DateTimeOffset ackDateTime = item.PpaAckTime.ToPointInTimeDateTimeOffset(item.PpaAckDate, TimeZoneInfo.Local.Id).GetValueOrDefault();
                var planSchedule = new List<PlanSchedule>();
                foreach (var schedule in item.PpaSchedulesEntityAssociation)
                {
                    planSchedule.Add(new PlanSchedule(schedule.PpaDueDateAssocMember.GetValueOrDefault(), schedule.PpaDueAmountAssocMember.GetValueOrDefault()));
                }
                
                _payPlanApprovals.Add(new PaymentPlanApproval(item.Recordkey, item.PpaStudent, item.PpaStudentName, ackDateTime, item.PpaTemplateId,
                item.PpaPayPlan, item.PpaTermsApprDocResponse, item.PpaTotalPlanAmt.GetValueOrDefault(), 
                planSchedule)
                {
                    PaymentControlId = item.PpaIpcRegistration,
                    AcknowledgementDocumentId = item.PpaAckApprovalDocument,
                    DownPaymentAmount = item.PpaDownPaymentAmt.GetValueOrDefault(),
                    DownPaymentDate = item.PpaDownPaymentDate.GetValueOrDefault(),
                    SetupChargeAmount = item.PpaSetupChargeAmt.GetValueOrDefault(),
                    Frequency = ConvertToFrequencyEntity(item.PpaFrequency),
                    NumberOfPayments = item.PpaNumberOfPayments.GetValueOrDefault(),
                    GraceDays = item.PpaGraceDays.GetValueOrDefault(),
                    LateChargeAmount = item.PpaLateChargeAmt.GetValueOrDefault(), 
                    LateChargePercentage = item.PpaLateChargePct.GetValueOrDefault(),
                });
            }
        }

        private static PlanFrequency ConvertToFrequencyEntity(string frequency)
        {
            if (string.IsNullOrEmpty(frequency))
            {
                throw new ArgumentNullException("frequency", "Payment Plan Template must have a frequency specified.");
            }

            switch (frequency.ToUpper())
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
    }
}
