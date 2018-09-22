// Copyright 2015 Ellucian Company L.P. and its affiliates.

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
    /// Adapter for mapping from the Blanket Purchase Order GL Distribution entity to DTO
    /// </summary>
    public class BlanketPurchaseOrderGlDistributionEntityToDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.BlanketPurchaseOrderGlDistribution, Ellucian.Colleague.Dtos.ColleagueFinance.BlanketPurchaseOrderGlDistribution>
    {

        public BlanketPurchaseOrderGlDistributionEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a Blanket Purchase Order GL distribution domain entity into a DTO
        /// </summary>
        /// <param name="Source"> GL distribution domain entity to be converted</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL numbers</param>
        /// <returns>Blanket Purchase Order GL distribution DTO</returns>
        public Dtos.ColleagueFinance.BlanketPurchaseOrderGlDistribution MapToType(Domain.ColleagueFinance.Entities.BlanketPurchaseOrderGlDistribution Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            // Copy the blanket purchase order GL distribution properties
            var bpoGlDistributionDto = new BlanketPurchaseOrderGlDistribution();

            bpoGlDistributionDto.GlAccount = Source.GlAccountNumber;
            bpoGlDistributionDto.FormattedGlAccount = Source.GetFormattedGlAccount(glMajorComponentStartPositions);
            bpoGlDistributionDto.Description = Source.GlAccountDescription;
            bpoGlDistributionDto.ProjectNumber = Source.ProjectNumber;
            bpoGlDistributionDto.ProjectLineItemCode = Source.ProjectLineItemCode;
            bpoGlDistributionDto.EncumberedAmount = Source.EncumberedAmount;
            bpoGlDistributionDto.ExpensedAmount = Source.ExpensedAmount;

            return bpoGlDistributionDto;
        }
    }
}
