// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.Finance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    /// <summary>
    /// Adapts a ReceivableCharge DTO to a ReceivableCharge entity
    /// </summary>
    public class ReceivableChargeDtoAdapter : BaseAdapter<ReceivableCharge, Ellucian.Colleague.Domain.Finance.Entities.ReceivableCharge>
    {
        /// <summary>
        /// Constructor for ReceivableChargeDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">Base interface for adapter registries</param>
        /// <param name="logger">Interface for logging mechanisms</param>
        public ReceivableChargeDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Maps a ReceivableCharge Dto to its corresponding domain entity
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public override Domain.Finance.Entities.ReceivableCharge MapToType(ReceivableCharge source)
        {
            var entity = new Domain.Finance.Entities.ReceivableCharge(source.Id, source.InvoiceId, source.Description,
                source.Code, source.BaseAmount);
            entity.TaxAmount = source.TaxAmount;

            if (source.AllocationIds != null && source.AllocationIds.Count > 0)
            {
                foreach (var alloc in source.AllocationIds)
                {
                    entity.AddAllocation(alloc);
                }
            }

            if (source.PaymentPlanIds != null && source.PaymentPlanIds.Count > 0)
            {
                foreach (var plan in source.PaymentPlanIds)
                {
                    entity.AddPaymentPlan(plan);
                }
            }

            return entity;
        }
    }
}
