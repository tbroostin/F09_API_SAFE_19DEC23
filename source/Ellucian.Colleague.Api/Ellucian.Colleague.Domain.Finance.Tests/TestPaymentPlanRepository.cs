using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Data.Finance.DataContracts;
using Ellucian.Colleague.Data.Finance.Tests;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Dmi.Runtime;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public static class TestPaymentPlanRepository
    {
        private static List<PaymentPlan> _payPlans = new List<PaymentPlan>();
        public static List<PaymentPlan> PayPlans
        {
            get
            {
                if (_payPlans.Count == 0)
                {
                    GenerateEntities();
                }
                return _payPlans;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var item in TestArPayPlansRepository.ArPayPlans)
            {
                var planStatuses = new List<Domain.Finance.Entities.PlanStatus>();
                var scheduledPayments = new List<Domain.Finance.Entities.ScheduledPayment>();
                var planCharges = new List<Domain.Finance.Entities.PlanCharge>();

                if (item.ArplStatusesEntityAssociation != null && item.ArplStatusesEntityAssociation.Count > 0)
                {
                    foreach (var ps in item.ArplStatusesEntityAssociation)
                    {
                        planStatuses.Add(new PlanStatus(ConvertToPlanStatusEntity(ps.ArplStatusAssocMember), ps.ArplStatusDateAssocMember.GetValueOrDefault()));
                    }
                }

                if (item.ArplPayPlanItems != null && item.ArplPayPlanItems.Count > 0)
                {
                    foreach (var ps in item.ArplPayPlanItems)
                    {
                        var schedPay = TestArPayPlanItemsRepository.ArPayPlanItems.Where(appi => appi.Recordkey == ps).FirstOrDefault();
                        if (schedPay != null)
                        {
                            scheduledPayments.Add(new ScheduledPayment(schedPay.Recordkey, schedPay.ArpliPayPlan, schedPay.ArpliAmt.GetValueOrDefault(),
                                schedPay.ArpliDueDate.GetValueOrDefault(), schedPay.ArpliAmountPaid.GetValueOrDefault(), schedPay.ArpliDatePaid));
                        }
                    }
                }

                if (item.ArplInvoiceItems != null && item.ArplInvoiceItems.Count > 0)
                {
                    foreach (var ps in item.ArplInvoiceItems)
                    {
                        var planInvItem = TestArPayInvoiceItemsRepository.ArPayInvoiceItems.Where(apii => apii.Recordkey.Split('*')[1] == ps).FirstOrDefault();
                        if (planInvItem != null)
                        {
                            var invItem = TestArInvoiceItemsRepository.ArInvoiceItems.Where(arii => arii.Recordkey == planInvItem.Recordkey.Split('*')[1]).FirstOrDefault();
                            if (invItem != null)
                            {
                                var charge = new Charge(invItem.Recordkey, invItem.InviInvoice, invItem.InviDesc.Split(DmiString._VM), invItem.InviArCode,
                                    invItem.InviExtChargeAmt.GetValueOrDefault() - invItem.InviExtCrAmt.GetValueOrDefault());
                                decimal totalTax = 0;
                                if (invItem.InviArCodeTaxDistrs != null && invItem.InviArCodeTaxDistrs.Count > 0)
                                {
                                    foreach (var tax in invItem.InviArCodeTaxDistrs)
                                    {
                                        totalTax += TestArCodeTaxGlDistrRepository.ArCodeTaxGlDistrs.Where(tx => tx.Recordkey == tax).FirstOrDefault().ArctdGlTaxAmt.GetValueOrDefault();
                                    }
                                }
                                charge.TaxAmount = totalTax;

                                // Add any associated payment plan IDs to charges
                                if (invItem.InviArPayPlans != null && invItem.InviArPayPlans.Count > 0)
                                {
                                    foreach (var planId in invItem.InviArPayPlans)
                                    {
                                        charge.AddPaymentPlan(planId);
                                    }
                                }
                                planCharges.Add(new PlanCharge(planInvItem.Recordkey.Split('*')[0], charge, planInvItem.ArpliiAmt.GetValueOrDefault(), 
                                    (planInvItem.ArpliiSetupInvoiceFlag == "Y"), (planInvItem.ArpliiAllocationFlag == "Y")));
                            }
                        }
                    }
                }


                _payPlans.Add(new PaymentPlan(item.Recordkey,
                item.ArplPayPlanTemplate,
                item.ArplPersonId,
                item.ArplArType,
                item.ArplTerm,
                item.ArplOrigAmt.GetValueOrDefault(),
                item.ArplFirstDueDate.GetValueOrDefault(),
                planStatuses,
                scheduledPayments,
                planCharges)
                {
                    CurrentAmount = item.ArplAmt.GetValueOrDefault(),
                    Frequency = ConvertToFrequencyEntity(item.ArplFrequency),
                    NumberOfPayments = item.ArplNoPayments.GetValueOrDefault(),
                    SetupAmount = item.ArplChargeAmt.GetValueOrDefault(),
                    SetupPercentage = item.ArplChargePct.GetValueOrDefault(),
                    DownPaymentPercentage = item.ArplDownPayPct.GetValueOrDefault(),
                    GraceDays = item.ArplGraceNoDays.GetValueOrDefault(),
                    LateChargeAmount = item.ArplLateChargeAmt.GetValueOrDefault(),
                    LateChargePercentage = item.ArplPlanLateChrgPct.GetValueOrDefault(),
                });
            }
        }

        private static PlanFrequency ConvertToFrequencyEntity(string frequency)
        {
            switch (frequency.ToUpper())
            {
                case "W":
                    return PlanFrequency.Weekly;
                case "B":
                    return PlanFrequency.Biweekly;
                case "M":
                    return PlanFrequency.Monthly;
                case "C":
                    return PlanFrequency.Custom;
                default:
                    return PlanFrequency.Yearly;
            }
        }

        private static PlanStatusType ConvertToPlanStatusEntity(string planStatus)
        {
            switch (planStatus.ToUpper())
            {
                case "O":
                    return PlanStatusType.Open;
                case "P":
                    return PlanStatusType.Paid;
                default:
                    return PlanStatusType.Cancelled;
            }
        }
    }
}
