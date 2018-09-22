// Copyright 2012-2013 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class AccountDuePeriodEntityAdapter : BaseAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDuePeriod, Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDuePeriod>
    {
        public AccountDuePeriodEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger) : base(adapterRegistry, logger) { }

        public override Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDuePeriod MapToType(Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDuePeriod Source)
        {
            var accountPeriodDto = new Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDuePeriod();

            var accountDueDtoAdapter = adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Finance.Entities.AccountDue.AccountDue, Ellucian.Colleague.Dtos.Finance.AccountDue.AccountDue>();

            accountPeriodDto.Current = accountDueDtoAdapter.MapToType(Source.Current);
            accountPeriodDto.Future = accountDueDtoAdapter.MapToType(Source.Future);
            accountPeriodDto.Past = accountDueDtoAdapter.MapToType(Source.Past);
            accountPeriodDto.PersonName = Source.PersonName;

            return accountPeriodDto;
        }
    }
}
