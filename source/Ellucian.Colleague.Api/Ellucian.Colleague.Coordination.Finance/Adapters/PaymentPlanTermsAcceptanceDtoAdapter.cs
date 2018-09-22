// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class PaymentPlanTermsAcceptanceDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Dtos.Finance.PaymentPlanTermsAcceptance, Ellucian.Colleague.Domain.Finance.Entities.PaymentPlanTermsAcceptance>
    {
        public PaymentPlanTermsAcceptanceDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger) { }

        public override Domain.Finance.Entities.PaymentPlanTermsAcceptance MapToType(PaymentPlanTermsAcceptance Source)
        {
            var planCharges = new List<Domain.Finance.Entities.PlanCharge>();
            if (Source.ProposedPlan.PlanCharges != null && Source.ProposedPlan.PlanCharges.Count() > 0)
            {
                foreach (var planCharge in Source.ProposedPlan.PlanCharges)
                {
                    var charge = new Domain.Finance.Entities.Charge(planCharge.Charge.Id, planCharge.Charge.InvoiceId, planCharge.Charge.Description,
                    planCharge.Charge.Code, planCharge.Charge.Amount)
                    {
                        TaxAmount = planCharge.Charge.TaxAmount,
                    };
                    var planChargeEntity = new Domain.Finance.Entities.PlanCharge(planCharge.PlanId, charge, planCharge.Amount, 
                        planCharge.IsSetupCharge, planCharge.IsAutomaticallyModifiable);
                    planCharges.Add(planChargeEntity);
                }
            }

            var scheduledPayments = new List<Domain.Finance.Entities.ScheduledPayment>();
            if (Source.ProposedPlan.ScheduledPayments != null && Source.ProposedPlan.ScheduledPayments.Count() > 0)
            {
                var scheduledPaymentDtoAdapter = new AutoMapperAdapter<ScheduledPayment, Domain.Finance.Entities.ScheduledPayment>(adapterRegistry, logger);
                foreach (var scheduledPayment in Source.ProposedPlan.ScheduledPayments)
                {
                    var scheduledPaymentEntity = scheduledPaymentDtoAdapter.MapToType(scheduledPayment);
                    scheduledPayments.Add(scheduledPaymentEntity);
                }
            }

            var planStatuses = new List<Domain.Finance.Entities.PlanStatus>();
            if (Source.ProposedPlan.Statuses != null && Source.ProposedPlan.Statuses.Count() > 0)
            {
                var planStatusDtoAdapter = new AutoMapperAdapter<PlanStatus, Domain.Finance.Entities.PlanStatus>(adapterRegistry, logger);
                foreach (var status in Source.ProposedPlan.Statuses)
                {
                    var statusEntity = planStatusDtoAdapter.MapToType(status);
                    planStatuses.Add(statusEntity);
                }
            }

            var planDtoAdapter = new AutoMapperAdapter<PaymentPlan, Domain.Finance.Entities.PaymentPlan>(adapterRegistry, logger);
            var planEntity = new Domain.Finance.Entities.PaymentPlan(Source.ProposedPlan.Id, Source.ProposedPlan.TemplateId, Source.ProposedPlan.PersonId, 
                Source.ProposedPlan.ReceivableTypeCode, Source.ProposedPlan.TermId, Source.ProposedPlan.OriginalAmount, Source.ProposedPlan.FirstDueDate, 
                planStatuses, scheduledPayments, planCharges)
                {
                    CurrentAmount = Source.ProposedPlan.CurrentAmount,
                    DownPaymentPercentage = Source.ProposedPlan.DownPaymentPercentage,
                    Frequency = (Domain.Finance.Entities.PlanFrequency)Source.ProposedPlan.Frequency,
                    GraceDays = Source.ProposedPlan.GraceDays,
                    LateChargeAmount = Source.ProposedPlan.LateChargeAmount,
                    LateChargePercentage = Source.ProposedPlan.LateChargePercentage,
                    NumberOfPayments = Source.ProposedPlan.NumberOfPayments,
                    SetupAmount = Source.ProposedPlan.SetupAmount,
                    SetupPercentage = Source.ProposedPlan.SetupPercentage
                };

            Domain.Finance.Entities.PaymentPlanTermsAcceptance entity;
            if (!string.IsNullOrEmpty(Source.PaymentControlId))
            {
                entity = new Domain.Finance.Entities.PaymentPlanTermsAcceptance(Source.StudentId, Source.AcknowledgementDateTime,
                    Source.StudentName, planEntity, Source.DownPaymentAmount, Source.DownPaymentDate, Source.ApprovalReceived, Source.ApprovalUserId, Source.TermsText, Source.RegistrationApprovalId)
                {
                    PaymentControlId = Source.PaymentControlId,
                    AcknowledgementText = Source.AcknowledgementText,
                };
            }
            else
            {
                entity = new Domain.Finance.Entities.PaymentPlanTermsAcceptance(Source.StudentId, Source.AcknowledgementDateTime,
                    Source.StudentName, planEntity, Source.DownPaymentAmount, Source.DownPaymentDate, Source.ApprovalReceived, Source.ApprovalUserId, Source.TermsText)
                {
                    AcknowledgementText = Source.AcknowledgementText
                };
            }

            return entity;
        }
    }
}
