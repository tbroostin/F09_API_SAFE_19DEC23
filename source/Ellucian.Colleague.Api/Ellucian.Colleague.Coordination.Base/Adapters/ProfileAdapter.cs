using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using slf4net;

namespace Ellucian.Colleague.Coordination.Base.Adapters
{
    public class ProfileAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Base.Entities.Profile, Dtos.Base.Profile>
    {
        public ProfileAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.EmailAddress, Dtos.Base.EmailAddress>();
            AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.PhoneNumber, Dtos.Base.PhoneNumber>();
            AddMappingDependency<Ellucian.Colleague.Domain.Base.Entities.Address, Dtos.Base.Address>();
        }
    }
}
