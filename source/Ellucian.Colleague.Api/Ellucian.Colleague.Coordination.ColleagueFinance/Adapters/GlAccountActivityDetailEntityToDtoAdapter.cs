// Copyright 2016 Ellucian Company L.P. and its affiliates.

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
    ///  Adapter for mapping the GL account activity detail entity to a DTO.
    /// </summary>
    public class GlAccountActivityDetailEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlAccountActivityDetail, Ellucian.Colleague.Dtos.ColleagueFinance.GlAccountActivityDetail>
    {
        public GlAccountActivityDetailEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a GL account activity detail domain entity into a DTO.
        /// </summary>
        /// <param name="Source">A GL account activity detail domain entity.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL accounts.</param>
        /// <returns>A GL account activity detail DTO.</returns>
        public Dtos.ColleagueFinance.GlAccountActivityDetail MapToType(Domain.ColleagueFinance.Entities.GlAccountActivityDetail Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var glAccountDto = new GlAccountActivityDetail();

            // Copy the GL account properties.
            glAccountDto.GlAccountNumber = Source.GlAccountNumber;
            glAccountDto.FormattedGlAccount = Source.GetFormattedGlAccount(glMajorComponentStartPositions);
            glAccountDto.Description = Source.GlAccountDescription;
            glAccountDto.CostCenterId = Source.CostCenterId;
            glAccountDto.UnitId = Source.UnitId;
            glAccountDto.Name = Source.Name;
            glAccountDto.Budget = Source.BudgetAmount;
            glAccountDto.MemoBudget = Source.MemoBudgetAmount;
            glAccountDto.Encumbrances = Source.EncumbranceAmount;
            glAccountDto.Actuals = Source.ActualAmount;
            glAccountDto.MemoActuals = Source.MemoActualsAmount;
            glAccountDto.EstimatedOpeningBalance = Source.EstimatedOpeningBalance;
            glAccountDto.ClosingYearAmount = Source.ClosingYearAmount;
            glAccountDto.JustificationNotes = Source.JustificationNotes;
            glAccountDto.ShowJustificationNotes = Source.ShowJustificationNotes;
            
            // Copy all of the GL transactions
            var glTransactionAdapter = new AutoMapperAdapter<Domain.ColleagueFinance.Entities.GlTransaction, Dtos.ColleagueFinance.GlTransaction>(adapterRegistry, logger);

            glAccountDto.BudgetTransactions = Source.Transactions.Where(x => x.GlTransactionType == Domain.ColleagueFinance.Entities.GlTransactionType.Budget)
                .Select(x => glTransactionAdapter.MapToType(x)).ToList();

            glAccountDto.ActualsTransactions = Source.Transactions.Where(x => x.GlTransactionType == Domain.ColleagueFinance.Entities.GlTransactionType.Actual)
                .Select(x => glTransactionAdapter.MapToType(x)).ToList();

            glAccountDto.EncumbranceTransactions = Source.Transactions.Where(x => x.GlTransactionType == Domain.ColleagueFinance.Entities.GlTransactionType.Encumbrance
                || x.GlTransactionType == Domain.ColleagueFinance.Entities.GlTransactionType.Requisition)
                .Select(x => glTransactionAdapter.MapToType(x)).ToList();

            return glAccountDto;
        }
    }
}
