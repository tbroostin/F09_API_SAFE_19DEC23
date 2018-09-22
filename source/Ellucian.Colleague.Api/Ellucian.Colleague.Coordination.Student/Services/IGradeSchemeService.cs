// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using Ellucian.Colleague.Dtos.Student;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

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
    }
}
