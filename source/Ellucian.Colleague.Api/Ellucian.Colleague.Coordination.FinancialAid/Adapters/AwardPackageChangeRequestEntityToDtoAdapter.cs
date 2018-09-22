/*Copyright 2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom Adapter to convert AwardPackageChangeRequest entity to AwardPackageChangeRequest DTO
    /// </summary>
    public class AwardPackageChangeRequestEntityToDtoAdapter : AutoMapperAdapter<Domain.FinancialAid.Entities.AwardPackageChangeRequest, Dtos.FinancialAid.AwardPackageChangeRequest>
    {
        /// <summary>
        /// Constructor for the custom AwardPackageChangeRequestEntityToDtoAdapter
        /// </summary>
        /// <param name="adapterRegistry">Adapter Registry</param>
        /// <param name="logger">Logger</param>
        public AwardPackageChangeRequestEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.FinancialAid.Entities.AwardPeriodChangeRequest, Dtos.FinancialAid.AwardPeriodChangeRequest>();
        }
    }
}
