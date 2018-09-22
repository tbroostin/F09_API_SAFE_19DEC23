//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for BillingOverrideReasons services
    /// </summary>
    public interface IBillingOverrideReasonsService : IBaseService
    {

        Task<IEnumerable<Ellucian.Colleague.Dtos.BillingOverrideReasons>> GetBillingOverrideReasonsAsync(bool bypassCache = false);

        Task<Ellucian.Colleague.Dtos.BillingOverrideReasons> GetBillingOverrideReasonsByGuidAsync(string id);
    }
}