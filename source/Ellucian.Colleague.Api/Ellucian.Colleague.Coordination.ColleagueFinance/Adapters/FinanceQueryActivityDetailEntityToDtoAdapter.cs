// Copyright 2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using slf4net;
using System.Collections.Generic;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for a Finance Query Activity Detail entity to Dto mapping.
    /// </summary>
    public class FinanceQueryActivityDetailEntityToDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.FinanceQueryActivityDetail, Dtos.ColleagueFinance.FinanceQueryActivityDetail>
    {
        public FinanceQueryActivityDetailEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
    : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a finance query activity detail domain entity and all of its descendent objects into DTOs
        /// </summary>
        /// <param name="Source">Finance query activity detail domain entity to be converted</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL numbers</param>
        /// <returns>Finance query activity detail DTO</returns>
        public FinanceQueryActivityDetail MapToType(Domain.ColleagueFinance.Entities.FinanceQueryActivityDetail Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            // Copy the FinanceQueryActivityDetail properties
            var financeQueryActivityDetailDto = new FinanceQueryActivityDetail();
            financeQueryActivityDetailDto.GlAccountNumber = Source.GlAccountNumber;
            financeQueryActivityDetailDto.FormattedGlAccount = Source.GetFormattedGlAccount(glMajorComponentStartPositions);
            financeQueryActivityDetailDto.Description = Source.GlAccountDescription;
            financeQueryActivityDetailDto.BudgetPoolIndicator = Source.BudgetPoolIndicator;

            financeQueryActivityDetailDto.Transactions = new List<FinanceQueryGlTransaction>();
       
            // Convert the finance query activity detail transaction domain entities into finance query Gl transaction DTOs
            foreach (var transaction in Source.Transactions)
            {
                if (transaction != null)
                {
                    var transactionDto = new Dtos.ColleagueFinance.FinanceQueryGlTransaction();
                    transactionDto.ReferenceNumber = transaction.ReferenceNumber;
                    transactionDto.DocumentId = transaction.DocumentId;
                    transactionDto.Source = transaction.Source;

                    // Translate the domain entity into the DTO status
                    switch (transaction.GlTransactionType)
                    {
                        case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlTransactionType.Actual:
                            transactionDto.TransactionType = "Actuals";
                            break;
                        case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlTransactionType.Budget:
                            transactionDto.TransactionType = "Budget";
                            break;
                        case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlTransactionType.Encumbrance:
                            transactionDto.TransactionType = "Encumbrances";
                            break;
                        case Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlTransactionType.Requisition:
                            transactionDto.TransactionType = "Requisitions";
                            break;
                    }

                    transactionDto.TransactionDate = transaction.TransactionDate;
                    transactionDto.Description = transaction.Description;
                    transactionDto.Amount = transaction.Amount;

                    // Add the finance query gl transaction DTO to the finance query activity detail DTO
                    financeQueryActivityDetailDto.Transactions.Add(transactionDto);
                }
            }

            return financeQueryActivityDetailDto;
        }
    }
}
