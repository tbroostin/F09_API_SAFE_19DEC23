// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Adapters
{

    /// <summary>
    /// Adapter for line item entity to Dto mapping.
    /// </summary>
    public class LineItemEntityToDtoAdapter : AutoMapperAdapter<Domain.BudgetManagement.Entities.LineItem, Dtos.BudgetManagement.LineItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LineItemEntityToDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public LineItemEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.BudgetManagement.Entities.BudgetLineItem, Dtos.BudgetManagement.BudgetLineItem>();
            AddMappingDependency<Domain.BudgetManagement.Entities.SubtotalLineItem, Dtos.BudgetManagement.SubtotalLineItem>();
        }
    }
}