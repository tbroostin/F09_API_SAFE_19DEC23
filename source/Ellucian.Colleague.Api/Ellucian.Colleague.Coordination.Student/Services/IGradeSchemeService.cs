// Copyright 2015-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Grade Scheme services
    /// </summary>
    public interface IGradeSchemeService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.GradeScheme>> GetGradeSchemesAsync();
        Task<Ellucian.Colleague.Dtos.GradeScheme> GetGradeSchemeByGuidAsync(string guid);

        Task<IEnumerable<Ellucian.Colleague.Dtos.GradeScheme2>> GetGradeSchemes2Async(bool bypassCache);
        Task<Ellucian.Colleague.Dtos.GradeScheme2> GetGradeSchemeByIdAsync(string id);

        Task<Ellucian.Colleague.Dtos.Student.GradeScheme> GetNonEthosGradeSchemeByIdAsync(string id);

        Task<Ellucian.Colleague.Dtos.Student.GradeSubscheme> GetGradeSubschemeByIdAsync(string id);

    }
}
