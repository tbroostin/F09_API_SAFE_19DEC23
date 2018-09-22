// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System.Linq;
using Ellucian.Colleague.Dtos.Finance;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    /// <summary>
    /// Adapter used to convert a PaymentPlan Dto to its corresponding domain entity
    /// </summary>
    public class PaymentPlanDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Dtos.Finance.PaymentPlan, Ellucian.Colleague.Domain.Finance.Entities.PaymentPlan>
    {
        /// <summary>
        /// Constructor for PaymentPlanDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">Base interface for adapter registries</param>
        /// <param name="logger">Interface for logging mechanisms</param>
        public PaymentPlanDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        /// <summary>
        /// Maps a PaymentPlan Dto to its corresponding domain entity
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Domain.Finance.Entities.PaymentPlan MapToType(PaymentPlan source)
        {
            var planCharges = new List<Domain.Finance.Entities.PlanCharge>();
            if (source.PlanCharges != null && source.PlanCharges.Count() > 0)
            {
                var planChargeDtoAdapter = new AutoMapperAdapter<PlanCharge, Domain.Finance.Entities.PlanCharge>(adapterRegistry, logger);
                foreach (var planCharge in source.PlanCharges)
                {
                    planCharges.Add(planChargeDtoAdapter.MapToType(planCharge));
                }
            }

            var scheduledPayments = new List<Domain.Finance.Entities.ScheduledPayment>();
            if (source.ScheduledPayments != null && source.ScheduledPayments.Count() > 0)
            {
                var scheduledPaymentDtoAdapter = new AutoMapperAdapter<ScheduledPayment, Domain.Finance.Entities.ScheduledPayment>(adapterRegistry, logger);
                foreach (var scheduledPayment in source.ScheduledPayments)
                {
                    scheduledPayments.Add(scheduledPaymentDtoAdapter.MapToType(scheduledPayment));
                }
            }

            var planStatuses = new List<Ellucian.Colleague.Domain.Finance.Entities.PlanStatus>();
            if (source.Statuses != null && source.Statuses.Count() > 0)
            {
                var planStatusDtoAdapter = new AutoMapperAdapter<PlanStatus, Domain.Finance.Entities.PlanStatus>(adapterRegistry, logger);
                foreach (var planStatus in source.Statuses)
                {
                    planStatuses.Add(planStatusDtoAdapter.MapToType(planStatus));
                }
            }

            var entity = new Domain.Finance.Entities.PaymentPlan(source.Id,
                source.TemplateId,
                source.PersonId,
                source.ReceivableTypeCode,
                source.TermId,
                source.OriginalAmount,
                source.FirstDueDate,
                planStatuses,
                scheduledPayments,
                planCharges)
                {
                    CurrentAmount = source.CurrentAmount,
                    DownPaymentPercentage = source.DownPaymentPercentage,
                    Frequency = (Ellucian.Colleague.Domain.Finance.Entities.PlanFrequency)source.Frequency,
                    GraceDays = source.GraceDays,
                    LateChargeAmount = source.LateChargeAmount,
                    LateChargePercentage = source.LateChargePercentage,
                    NumberOfPayments = source.NumberOfPayments,
                    SetupAmount = source.SetupAmount,
                    SetupPercentage= source.SetupPercentage
                };

            return entity;
        }
    }
}
