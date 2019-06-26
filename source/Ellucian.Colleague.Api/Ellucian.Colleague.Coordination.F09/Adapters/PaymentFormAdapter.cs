using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.F09.Entities;
using Ellucian.Colleague.Domain.F09.Repositories;
using Ellucian.Colleague.Dtos.F09;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.F09.Adapters
{
    public class PaymentFormAdapter : AutoMapperAdapter<Domain.F09.Entities.F09PaymentForm, Dtos.F09.F09PaymentFormDto>
    {
        public PaymentFormAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            AddMappingDependency<F09PaymentOption, F09PaymentOptionDto>();
        }
    }
}
