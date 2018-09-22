/*Copyright 2014-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.FinancialAid.Adapters
{
    /// <summary>
    /// Custom Adapter to convert AwardPackageChangeRequest DTO to AwardPackageChangeRequest Entity
    /// </summary>
    public class AwardPackageChangeRequestDtoToEntityAdapter : AutoMapperAdapter<Dtos.FinancialAid.AwardPackageChangeRequest, Domain.FinancialAid.Entities.AwardPackageChangeRequest>
    {
        /// <summary>
        /// Instantiate a new AwardPackageChangeRequestDtoToEntityAdapter
        /// </summary>
        /// <param name="adapterRegistry">AdapterRegistry</param>
        /// <param name="logger">Logger</param>
        public AwardPackageChangeRequestDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Dtos.FinancialAid.AwardPeriodChangeRequest, Domain.FinancialAid.Entities.AwardPeriodChangeRequest>();
        }
    }
}
