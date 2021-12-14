// Copyright 2015-2021 Ellucian Company L.P. and its affiliates.

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
    /// Adapter for mapping from the Line Item GL Distribution entity to DTO.
    /// </summary>
    public class LineItemGlDistributionEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.LineItemGlDistribution, Ellucian.Colleague.Dtos.ColleagueFinance.LineItemGlDistribution>
    {
     
        public LineItemGlDistributionEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a line item GL distribution domain entity into a DTO.
        /// </summary>
        /// <param name="Source">Line item GL distribution domain entity to be converted.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL numbers.</param>
        /// <returns>Line item GL distribution DTO.</returns>
        public Dtos.ColleagueFinance.LineItemGlDistribution MapToType(Domain.ColleagueFinance.Entities.LineItemGlDistribution Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            // Copy line item GL distribution properties.
            var lineItemGlDistributionDto = new LineItemGlDistribution();

            // Only populate the DTO with the GL distribution data if the account isn't masked
            if (Source.Masked)
            {
                lineItemGlDistributionDto.GlAccount = null;
                lineItemGlDistributionDto.FormattedGlAccount = Source.GetFormattedMaskedGlAccount(glMajorComponentStartPositions);
                lineItemGlDistributionDto.ProjectNumber = null;
                lineItemGlDistributionDto.ProjectLineItemCode = null;
                lineItemGlDistributionDto.Quantity = 0.00m;
                lineItemGlDistributionDto.Amount = 0.00m;
                lineItemGlDistributionDto.GlAccountDescription = null;
                lineItemGlDistributionDto.BudgetAmount = 0.00m;
                lineItemGlDistributionDto.EncumbranceAmount = 0.00m;
                lineItemGlDistributionDto.RequisitionAmount = 0.00m;
                lineItemGlDistributionDto.ActualAmount = 0.00m;
            }
            else
            {
                lineItemGlDistributionDto.GlAccount = Source.GlAccountNumber;
                lineItemGlDistributionDto.FormattedGlAccount = Source.GetFormattedMaskedGlAccount(glMajorComponentStartPositions);
                lineItemGlDistributionDto.ProjectNumber = Source.ProjectNumber;
                lineItemGlDistributionDto.ProjectLineItemCode = Source.ProjectLineItemCode;
                lineItemGlDistributionDto.Quantity = Source.Quantity;
                lineItemGlDistributionDto.Amount = Source.Amount;
                lineItemGlDistributionDto.GlAccountDescription = Source.GlAccountDescription;
                lineItemGlDistributionDto.BudgetAmount = Source.BudgetAmount;
                lineItemGlDistributionDto.EncumbranceAmount = Source.EncumbranceAmount;
                lineItemGlDistributionDto.RequisitionAmount = Source.RequisitionAmount;
                lineItemGlDistributionDto.ActualAmount = Source.ActualAmount;
            }

            lineItemGlDistributionDto.IsMasked = Source.Masked;
            return lineItemGlDistributionDto;
        }
    }
}
