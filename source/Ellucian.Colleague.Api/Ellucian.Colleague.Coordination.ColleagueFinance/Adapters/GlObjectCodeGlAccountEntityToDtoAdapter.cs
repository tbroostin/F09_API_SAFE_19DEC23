// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    ///  Adapter for mapping the GL object code GL account entity to a DTO.
    /// </summary>
    public class GlObjectCodeGlAccountEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlObjectCodeGlAccount, Ellucian.Colleague.Dtos.ColleagueFinance.GlObjectCodeGlAccount>
    {
        public GlObjectCodeGlAccountEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a GL account domain entity into a DTO.
        /// </summary>
        /// <param name="Source">A GL account domain entity.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL accounts.</param>
        /// <returns>A GL object code GL account DTO.</returns>
        public Dtos.ColleagueFinance.GlObjectCodeGlAccount MapToType(Domain.ColleagueFinance.Entities.GlObjectCodeGlAccount Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var glAccountDto = new GlObjectCodeGlAccount();

            // Copy the GL account properties.
            glAccountDto.GlAccountNumber = Source.GlAccountNumber;
            glAccountDto.FormattedGlAccount = Source.GetFormattedGlAccount(glMajorComponentStartPositions);
            glAccountDto.Description = Source.GlAccountDescription;
            glAccountDto.Budget = Source.BudgetAmount;
            glAccountDto.Encumbrances = Source.EncumbranceAmount;
            glAccountDto.Actuals = Source.ActualAmount;

            // Copy the pool type
            switch (Source.PoolType)
            {
                case Domain.ColleagueFinance.Entities.GlBudgetPoolType.None:
                    glAccountDto.PoolType = GlBudgetPoolType.None;
                    break;
                case Domain.ColleagueFinance.Entities.GlBudgetPoolType.Poolee:
                    glAccountDto.PoolType = GlBudgetPoolType.Poolee;
                    break;
                case Domain.ColleagueFinance.Entities.GlBudgetPoolType.Umbrella:
                    glAccountDto.PoolType = GlBudgetPoolType.Umbrella;
                    break;
            } 
            
            return glAccountDto;
        }
    }
}
