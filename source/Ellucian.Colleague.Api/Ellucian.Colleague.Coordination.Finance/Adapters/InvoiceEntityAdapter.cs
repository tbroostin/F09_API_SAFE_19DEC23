// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    // Complex objects require additional dependency mappings
    public class InvoiceEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Invoice, Ellucian.Colleague.Dtos.Finance.Invoice>
    {
        public InvoiceEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Charge, Ellucian.Colleague.Dtos.Finance.Charge>();
        }
    }
}