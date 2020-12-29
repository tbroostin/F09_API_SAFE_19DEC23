// Copyright 2019 Ellucian Company L.P. and its affiliates.

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
    ///  Adapter for mapping the GL account entity to a DTO.
    /// </summary>
    public class GlAccountEntityToDtoAdapter2 : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.GlAccount, Ellucian.Colleague.Dtos.ColleagueFinance.GlAccount>
    {
        public GlAccountEntityToDtoAdapter2(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }

        /// <summary>
        /// Convert a GL account domain entity into a DTO.
        /// </summary>
        /// <param name="Source">A GL account domain entity.</param>
        /// <param name="glMajorComponentStartPositions">List of GL major component start positions, used to format GL accounts.</param>
        /// <returns>A GL account DTO.</returns>
        public Dtos.ColleagueFinance.GlAccount MapToType(Domain.ColleagueFinance.Entities.GlAccount Source, IEnumerable<string> glMajorComponentStartPositions)
        {
            var glAccountDto = new GlAccount();

            glAccountDto.GlAccountNumber = Source.GlAccountNumber;
            glAccountDto.FormattedGlAccount = Source.GetFormattedGlAccount(glMajorComponentStartPositions);
            glAccountDto.GlAccountDescription = Source.GlAccountDescription;

            return glAccountDto;
        }
    }
}
