// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System.Collections.Generic;
using System.Linq;
using Ellucian.Web.Adapters;
using slf4net;

namespace Ellucian.Colleague.Coordination.Finance.Adapters
{
    public class DepositDtoAdapter : AutoMapperAdapter<Dtos.Finance.Deposit, Domain.Finance.Entities.Deposit>
    {
        public DepositDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {

        }

        public override Domain.Finance.Entities.Deposit MapToType(Dtos.Finance.Deposit source)
        {
            var entity = new Domain.Finance.Entities.Deposit(source.Id,
                source.PersonId,
                source.Date,
                source.DepositType,
                source.Amount) 
                { 
                    TermId = source.TermId,
                    ReceiptId = source.ReceiptId
                };
            if (!string.IsNullOrEmpty(source.ExternalIdentifier) || !string.IsNullOrEmpty(source.ExternalSystem))
            {
                entity.AddExternalSystemAndId(source.ExternalSystem, source.ExternalIdentifier);
            }
            return entity;
        }
    }
}
