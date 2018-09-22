//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AssessmentCalculationMethods services
    /// </summary>
    public interface IAssessmentCalculationMethodsService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentCalculationMethods>> GetAssessmentCalculationMethodsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AssessmentCalculationMethods> GetAssessmentCalculationMethodsByGuidAsync(string id);
    }
}
