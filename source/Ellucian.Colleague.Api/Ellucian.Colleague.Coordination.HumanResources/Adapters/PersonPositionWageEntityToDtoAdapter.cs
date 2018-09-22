/* Copyright 2016 Ellucian Company L.P. and its affiliates. */
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    /// <summary>
    /// Adapter converts PersonPositionWage from Domain Entity to DTO
    /// </summary>
    public class PersonPositionWageEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.PersonPositionWage, Dtos.HumanResources.PersonPositionWage>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency on PositionFundingSource
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public PersonPositionWageEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            :base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.PositionFundingSource, Dtos.HumanResources.PositionFundingSource>();
            AddMappingDependency<Domain.HumanResources.Entities.EarningsTypeGroup, Dtos.HumanResources.EarningsTypeGroupItem>();
        }
    }
}
