// Copyright 2020 Ellucian Company L.P. and its affiliates.

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
    /// Adapter for VendorInfo Dto to Entity mapping.
    /// </summary>
    public class VendorsVoucherSearchResultDtoToEntityAdapter : AutoMapperAdapter<Dtos.ColleagueFinance.VendorsVoucherSearchResult, Domain.ColleagueFinance.Entities.VendorsVoucherSearchResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VendorsVoucherSearchResult"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="logger">The logger.</param>
        public VendorsVoucherSearchResultDtoToEntityAdapter(IAdapterRegistry adapterRegistry, ILogger logger)
            : base(adapterRegistry, logger)
        {
        }
    }
}
