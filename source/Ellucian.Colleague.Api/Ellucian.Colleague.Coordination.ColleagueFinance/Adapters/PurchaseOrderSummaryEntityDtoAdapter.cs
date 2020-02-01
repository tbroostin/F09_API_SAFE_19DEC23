// Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    public class PurchaseOrderSummaryEntityDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.PurchaseOrderSummary, Dtos.ColleagueFinance.PurchaseOrderSummary>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PurchaseOrderSummaryEntityDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public PurchaseOrderSummaryEntityDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.PurchaseOrderStatus, Dtos.ColleagueFinance.PurchaseOrderStatus>();
            AddMappingDependency<Domain.ColleagueFinance.Entities.RequisitionSummary, Dtos.ColleagueFinance.RequisitionLinkSummary>();

        }
    }
}
