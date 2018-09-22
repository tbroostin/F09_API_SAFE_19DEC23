// Copyright 2012-2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class FinanceConfigurationEntityAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.Finance.Entities.Configuration.FinanceConfiguration, Ellucian.Colleague.Dtos.Finance.Configuration.FinanceConfiguration>
    {
        public FinanceConfigurationEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger)
        {
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Configuration.PartialPlanPayments, Ellucian.Colleague.Dtos.Finance.Configuration.PartialPlanPayments>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Configuration.ActivityDisplay, Ellucian.Colleague.Dtos.Finance.Configuration.ActivityDisplay>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Configuration.PaymentDisplay, Ellucian.Colleague.Dtos.Finance.Configuration.PaymentDisplay>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.Configuration.AvailablePaymentMethod, Ellucian.Colleague.Dtos.Finance.Configuration.AvailablePaymentMethod>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.FinancialPeriod, Ellucian.Colleague.Dtos.Finance.FinancialPeriod>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.StudentFinanceLink, Ellucian.Colleague.Dtos.Finance.StudentFinanceLink>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PayableReceivableType, Ellucian.Colleague.Dtos.Finance.PayableReceivableType>();
            AddMappingDependency<Ellucian.Colleague.Domain.Finance.Entities.PaymentRequirement, Ellucian.Colleague.Dtos.Finance.PaymentRequirement>();
        }
    }
}
