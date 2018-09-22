// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    public class RestrictionConfigurationAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.RestrictionConfiguration, Dtos.Base.RestrictionConfiguration>
    {
        public RestrictionConfigurationAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.SeverityStyleMapping, Dtos.Base.SeverityStyleMapping>();
        }
    }
}
