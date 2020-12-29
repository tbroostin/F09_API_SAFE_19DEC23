//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AptitudeAssessmentTypes services
    /// </summary>
    public interface IAptitudeAssessmentTypesService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AptitudeAssessmentTypes>> GetAptitudeAssessmentTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AptitudeAssessmentTypes> GetAptitudeAssessmentTypesByGuidAsync(string id);
    }
}