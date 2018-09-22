//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AssessmentPercentileTypes services
    /// </summary>
    public interface IAssessmentPercentileTypesService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AssessmentPercentileTypes>> GetAssessmentPercentileTypesAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AssessmentPercentileTypes> GetAssessmentPercentileTypesByGuidAsync(string id);
    }
}