// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Adapters;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Adapters
{
    /// <summary>
    /// Adapter for CF Web configuration entity to Dto mapping.
    /// </summary>
    public class ColleagueFinanceWebConfigurationsEntityDtoAdapter : AutoMapperAdapter<Domain.ColleagueFinance.Entities.ColleagueFinanceWebConfiguration, Dtos.ColleagueFinance.ColleagueFinanceWebConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CFWebDefaultsEntityDtoAdapter"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public ColleagueFinanceWebConfigurationsEntityDtoAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
            AddMappingDependency<Domain.ColleagueFinance.Entities.PurchasingDefaults, Dtos.ColleagueFinance.PurchasingDefaults>();
            AddMappingDependency<Domain.ColleagueFinance.Entities.VoucherWebConfiguration, Dtos.ColleagueFinance.VoucherWebConfiguration>();
        }
    }
}
