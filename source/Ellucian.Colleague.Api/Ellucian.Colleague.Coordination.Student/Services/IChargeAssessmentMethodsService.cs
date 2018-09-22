//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for ChargeAssessmentMethods services
    /// </summary>
    public interface IChargeAssessmentMethodsService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.ChargeAssessmentMethods>> GetChargeAssessmentMethodsAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.ChargeAssessmentMethods> GetChargeAssessmentMethodsByGuidAsync(string id);
    }
}
