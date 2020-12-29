// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    public class FinanceQuerySubtotalEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.FinanceQuerySubtotal, Ellucian.Colleague.Dtos.ColleagueFinance.FinanceQuerySubtotal>
    {
        public FinanceQuerySubtotalEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
           : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.FinanceQuerySubtotalComponent, Dtos.ColleagueFinance.FinanceQuerySubtotalComponent>();
        }

        public FinanceQuerySubtotal MapTotype(Domain.ColleagueFinance.Entities.FinanceQuerySubtotal source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var subTotals = new FinanceQuerySubtotal();
            // Initialize the adapter to convert the GL account line items within finance query subtotal entity.
            var adapter = new FinanceQueryGlAccountLineItemEntityToDtoAdapter(adapterRegistry, logger);

            var subTotalsAdapter = adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.FinanceQuerySubtotalComponent, Dtos.ColleagueFinance.FinanceQuerySubtotalComponent>();

            subTotals.SubtotalComponents = new List<FinanceQuerySubtotalComponent>();
            foreach (var subTotalComponent in source.FinanceQuerySubtotalComponents)
            {
                if (subTotalComponent != null)
                {
                    subTotals.SubtotalComponents.Add(subTotalsAdapter.MapToType(subTotalComponent));
                }
            }

            subTotals.FinanceQueryGlAccountLineItems = new List<FinanceQueryGlAccountLineItem>();
            // Convert the GL account line item domain entities into DTOs.
            foreach (var glAccount in source.FinanceQueryGlAccountLineItems)
            {
                if (glAccount != null)
                {
                    var financeQueryGlAccount = adapter.MapToType(glAccount, glMajorComponentStartPositions);
                    // Add the GL account DTO to the gl line item DTO.
                    subTotals.FinanceQueryGlAccountLineItems.Add(financeQueryGlAccount);
                }
            }

            return subTotals;
        }
    }
}
