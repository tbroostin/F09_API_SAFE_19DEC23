// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    ///  Adapter for mapping the GL object code GL account entity to a DTO.
    /// </summary>
    public class FinanceQueryGlAccountEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.FinanceQueryGlAccount, Ellucian.Colleague.Dtos.ColleagueFinance.FinanceQueryGlAccount>
    {
        public FinanceQueryGlAccountEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a finance query GL account domain entity into a DTO.
        /// </summary>
        /// <param name="source">A GL account domain entity.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL accounts.</param>
        /// <returns>A GL object code GL account DTO.</returns>
        public Dtos.ColleagueFinance.FinanceQueryGlAccount MapToType(Domain.ColleagueFinance.Entities.FinanceQueryGlAccount source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var glAccountDto = new FinanceQueryGlAccount();

            // Copy the GL account properties.
            glAccountDto.GlAccountNumber = source.GlAccountNumber;
            glAccountDto.FormattedGlAccount = source.GetFormattedGlAccount(glMajorComponentStartPositions);
            glAccountDto.Description = source.GlAccountDescription;
            glAccountDto.Budget = source.BudgetAmount;
            glAccountDto.Encumbrances = source.EncumbranceAmount;
            glAccountDto.Actuals = source.ActualAmount;
            glAccountDto.Requisitions = source.RequisitionAmount;
            return glAccountDto;
        }
    }
}

