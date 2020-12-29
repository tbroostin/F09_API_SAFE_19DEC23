// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Adapters
{

    /// <summary>
    /// Adapter for the subtotal line item entity to Dto mapping.
    /// </summary>
    public class SubtotalLineItemEntityToDtoAdapter : AutoMapperAdapter<Domain.BudgetManagement.Entities.SubtotalLineItem, Dtos.BudgetManagement.SubtotalLineItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubtotalLineItemEntityToDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public SubtotalLineItemEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.BudgetManagement.Entities.BudgetComparable, Dtos.BudgetManagement.BudgetComparable>();
        }
    }
}