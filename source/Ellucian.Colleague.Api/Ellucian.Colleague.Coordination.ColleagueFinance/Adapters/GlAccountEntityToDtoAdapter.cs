// Copyright 2016-2021 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    ///  Adapter for mapping the cost center GL account entity to a DTO.
    /// </summary>
    public class GlAccountEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.CostCenterGlAccount, Ellucian.Colleague.Dtos.ColleagueFinance.CostCenterGlAccount>
    {
        public GlAccountEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a GL account domain entity into a DTO.
        /// </summary>
        /// <param name="Source">A GL account domain entity.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL accounts.</param>
        /// <returns>A Cost Center GL account DTO.</returns>
        public Dtos.ColleagueFinance.CostCenterGlAccount MapToType(Domain.ColleagueFinance.Entities.CostCenterGlAccount Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var glAccountDto = new CostCenterGlAccount();

            // Copy the GL account properties.
            glAccountDto.GlAccountNumber = Source.GlAccountNumber;
            glAccountDto.FormattedGlAccount = Source.GetFormattedGlAccount(glMajorComponentStartPositions);
            glAccountDto.Description = Source.GlAccountDescription;
            glAccountDto.Budget = Source.BudgetAmount;
            glAccountDto.Encumbrances = Source.EncumbranceAmount;
            glAccountDto.Actuals = Source.ActualAmount;
            glAccountDto.JustificationNotes = Source.JustificationNotes;

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
