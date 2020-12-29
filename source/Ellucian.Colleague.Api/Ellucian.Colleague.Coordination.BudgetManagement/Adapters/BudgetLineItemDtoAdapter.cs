// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.BudgetManagement;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.BudgetManagement.Adapters
{

    /// <summary>
    /// Adapter for budget line item entity to Dto mapping.
    /// </summary>
    public class BudgetLineItemDtoAdapter : AutoMapperAdapter<Domain.BudgetManagement.Entities.BudgetLineItem, Dtos.BudgetManagement.BudgetLineItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BudgetLineItemDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public BudgetLineItemDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.BudgetManagement.Entities.BudgetComparable, Dtos.BudgetManagement.BudgetComparable>();
            AddMappingDependency<Domain.BudgetManagement.Entities.BudgetOfficer, Dtos.BudgetManagement.BudgetOfficer>();
            AddMappingDependency<Domain.BudgetManagement.Entities.BudgetReportingUnit, Dtos.BudgetManagement.BudgetReportingUnit>();
        }
    }
}