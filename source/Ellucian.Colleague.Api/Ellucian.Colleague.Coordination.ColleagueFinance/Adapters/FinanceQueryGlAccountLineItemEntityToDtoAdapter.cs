// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    ///  Adapter for mapping the GL object code GL account entity to a DTO.
    /// </summary>
    public class FinanceQueryGlAccountLineItemEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.FinanceQueryGlAccountLineItem, Ellucian.Colleague.Dtos.ColleagueFinance.FinanceQueryGlAccountLineItem>
    {
        public FinanceQueryGlAccountLineItemEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a finance query GL account domain entity into a DTO.
        /// </summary>
        /// <param name="source">A GL account domain entity.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL accounts.</param>
        /// <returns>A GL object code GL account DTO.</returns>
        public Dtos.ColleagueFinance.FinanceQueryGlAccountLineItem MapToType(Domain.ColleagueFinance.Entities.FinanceQueryGlAccountLineItem source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var lineItemDto = new FinanceQueryGlAccountLineItem { IsUmbrellaAccount = source.IsUmbrellaAccount, IsUmbrellaVisible = source.IsUmbrellaVisible };
            
            // Initialize the adapter to convert the GL accounts within the subtotal.
            var glAccountDtoAdapter = new FinanceQueryGlAccountEntityToDtoAdapter(adapterRegistry, logger);

            var glAccount = glAccountDtoAdapter.MapToType(source.GlAccount, glMajorComponentStartPositions);

            // If the umbrella is not visible to the user, null out the amounts, so they will be displayed masked in SS.
            if (!source.IsUmbrellaVisible)
            {
                glAccount.Actuals = 0;
                glAccount.Budget = 0;
                glAccount.Encumbrances = 0;
                glAccount.Requisitions = 0;
            }
            lineItemDto.GlAccount = glAccount;
            lineItemDto.Poolees = new List<FinanceQueryGlAccount>();
            lineItemDto.Poolees = source.Poolees.Select(x => glAccountDtoAdapter.MapToType(x, glMajorComponentStartPositions)).ToList();

            return lineItemDto;
        }
    }
}


