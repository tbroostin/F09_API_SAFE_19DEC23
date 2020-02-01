//Copyright 2019 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// Interface for TaxForm services
    /// </summary>
    public interface ITaxFormsService : IBaseService
    {
        /// <summary>
        /// Get Tax forms
        /// </summary>
        /// <returns>Collection of <see cref="TaxForm">TaxForm</see> objects</returns>
        Task<IEnumerable<Dtos.ColleagueFinance.TaxForm>> GetTaxFormsAsync();
    }
}
