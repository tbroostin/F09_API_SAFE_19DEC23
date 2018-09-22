/*Copyright 2016 Ellucian Company L.P. and its affiliates.*/
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.HumanResources.Adapters
{
    public class PayCycleEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.PayCycle, Dtos.HumanResources.PayCycle>
    {
        /// <summary>
        /// Constructor adds Mapping Dependency for PayPeriod
        /// </summary>
        /// <param name="adapterRegistry"></param>
        /// <param name="logger"></param>
        public PayCycleEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.PayPeriod, Dtos.HumanResources.PayPeriod>();
        }
    }
}
