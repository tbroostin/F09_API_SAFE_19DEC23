using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestPaymentPlanTemplateRepository
    {
        private static List<PaymentPlanTemplate> _payPlanTemplates = new List<PaymentPlanTemplate>();
        public static List<PaymentPlanTemplate> PayPlanTemplates
        {
            get
            {
                if (_payPlanTemplates.Count == 0)
                {
                    GenerateEntities();
                }
                return _payPlanTemplates;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var item in TestPayPlanTemplatesRepository.PayPlanTemplates)
            {
                _payPlanTemplates.Add(new PaymentPlanTemplate(item.Recordkey, item.PptDescription, (item.PptActiveFlag == "Y"), 
                    ConvertToFrequencyEntity(item.PptFrequency), item.PptNoPayments.GetValueOrDefault(), item.PptMinPlanAmt.GetValueOrDefault(), 
                    item.PptMaxPlanAmt, item.PptPayFrequencySubr)
                    {
                        TermsAndConditionsDocumentId = item.PptTermsAndConditionsDoc,
                        SetupChargeAmount = item.PptChargeAmt.GetValueOrDefault(),
                        SetupChargePercentage = item.PptChargePct.GetValueOrDefault(),
                        DownPaymentPercentage = item.PptDownPayPct.GetValueOrDefault(),
                        DaysUntilDownPaymentIsDue = item.PptDownPayNoDays.GetValueOrDefault(),
                        GraceDays = item.PptGraceNoDays.GetValueOrDefault(),
                        LateChargeAmount = item.PptLateChargeAmt.GetValueOrDefault(),
                        LateChargePercentage = item.PptLateChrgPct.GetValueOrDefault(),
                        IncludeSetupChargeInFirstPayment = (item.PptPrepaySetupFlag == "Y"),
                        SubtractAnticipatedFinancialAid = (item.PptSubtractAnticipatedFa == "Y"),
                        CalculatePlanAmountAutomatically = (item.PptCalcAmtFlag == "Y"),
                        ModifyPlanAutomatically = (item.PptRebillModifyFlag == "Y")
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
