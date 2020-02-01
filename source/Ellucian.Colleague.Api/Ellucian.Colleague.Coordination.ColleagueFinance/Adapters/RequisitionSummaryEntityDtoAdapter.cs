// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{

    /// <summary>
    /// Adapter for requisition summary entity to Dto mapping.
    /// </summary>
    public class RequisitionSummaryEntityDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.RequisitionSummary, Dtos.ColleagueFinance.RequisitionSummary>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequisitionSummaryEntityDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public RequisitionSummaryEntityDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.RequisitionStatus, Dtos.ColleagueFinance.RequisitionStatus>();

            AddMappingDependency<Domain.ColleagueFinance.Entities.PurchaseOrderSummary, Dtos.ColleagueFinance.PurchaseOrderLinkSummary>();
            
        }
    }
}
