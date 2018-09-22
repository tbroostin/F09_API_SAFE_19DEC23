// Copyright 2014 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Finance.Entities;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    /// <summary>
    /// Adapts a ReceivableInvoice entity to a ReceivableInvoice DTO
    /// </summary>
    public class ReceivableInvoiceEntityAdapter : AutoMapperAdapter<ReceivableInvoice, Ellucian.Colleague.Dtos.Finance.ReceivableInvoice>
    {
        public ReceivableInvoiceEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            // Mapping dependency
            AddMappingDependency<ReceivableCharge, Ellucian.Colleague.Dtos.Finance.ReceivableCharge>();
        }
    }
}
