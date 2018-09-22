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
    [RegisterType]
    public class EarningsTypeGroupEntityToDtoAdapter : AutoMapperAdapter<Domain.HumanResources.Entities.EarningsTypeGroup, Dtos.HumanResources.EarningsTypeGroup>
    {
        public EarningsTypeGroupEntityToDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.HumanResources.Entities.EarningsTypeGroupItem, Dtos.HumanResources.EarningsTypeGroupItem>();
        }
    }
}
