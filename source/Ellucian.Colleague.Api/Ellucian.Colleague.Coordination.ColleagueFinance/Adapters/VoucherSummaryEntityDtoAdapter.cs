// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Web.Adapters;
using AutoMapper;
using slf4net;
using Ellucian.Colleague.Dtos.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for mapping the Voucher entity into DTOs
    /// </summary>
    public class VoucherSummaryEntityDtoAdapter : AutoMapperAdapter<Ellucian.Colleague.Domain.ColleagueFinance.Entities.VoucherSummary, Ellucian.Colleague.Dtos.ColleagueFinance.VoucherSummary>
    {

        public VoucherSummaryEntityDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.VoucherStatus, Dtos.ColleagueFinance.VoucherStatus>();
            AddMappingDependency<Domain.ColleagueFinance.Entities.PurchaseOrderSummary, Dtos.ColleagueFinance.PurchaseOrderLinkSummary>();
            AddMappingDependency<Domain.ColleagueFinance.Entities.Approver, Dtos.ColleagueFinance.Approver>();
        }
    }
}
