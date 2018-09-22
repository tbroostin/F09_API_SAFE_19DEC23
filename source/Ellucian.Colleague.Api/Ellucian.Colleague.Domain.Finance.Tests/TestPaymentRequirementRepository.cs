using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Colleague.Data.Finance.Tests;

namespace Ellucian.Colleague.Domain.Finance.Tests
{
    public class TestPaymentRequirementRepository
    {
        private static List<PaymentRequirement> _paymentRequirements = new List<PaymentRequirement>();
        public static List<PaymentRequirement> PaymentRequirements
        {
            get
            {
                if (_paymentRequirements.Count == 0)
                {
                    GenerateEntities();
                }
                return _paymentRequirements;
            }
        }

        private static void GenerateEntities()
        {
            foreach (var record in TestIpcPaymentReqsRepository.IpcPaymentReqs)
            {
                // Build the deferral options
                var deferrals = new List<PaymentDeferralOption>();
                foreach (var def in record.IpcpDeferralsEntityAssociation)
                {
                    var defOption = new PaymentDeferralOption(def.IpcpDeferEffectiveStartAssocMember.GetValueOrDefault(), 
                        def.IpcpDeferEffectiveEndAssocMember, 
                        def.IpcpDeferPctAssocMember.GetValueOrDefault());
                    deferrals.Add(defOption);
                }

                // Build the pay plan options
                var plans = new List<PaymentPlanOption>();
                foreach (var pp in record.IpcpPayPlansEntityAssociation)
                {
                    var ppOption = new PaymentPlanOption(pp.IpcpPlanEffectiveStartAssocMember.GetValueOrDefault(), 
                        pp.IpcpPlanEffectiveEndAssocMember.GetValueOrDefault(), 
                        pp.IpcpPayPlanTemplateAssocMember, 
                        pp.IpcpPlanStartDateAssocMember.GetValueOrDefault());
                    plans.Add(ppOption);
                }

                // Build the payment requirement
                var entity = new PaymentRequirement(record.Recordkey, record.IpcpTerm, record.IpcpEligibilityRule, record.IpcpRuleEvalOrder.GetValueOrDefault(), 
                    deferrals, plans);
            }
        }
    }
}
