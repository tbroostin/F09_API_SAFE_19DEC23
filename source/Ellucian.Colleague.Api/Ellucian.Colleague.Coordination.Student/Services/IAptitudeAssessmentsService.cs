//Copyright 2017 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AptitudeAssessments services
    /// </summary>
    public interface IAptitudeAssessmentsService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.AptitudeAssessment>> GetAptitudeAssessmentsAsync(bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.AptitudeAssessment> GetAptitudeAssessmentsByGuidAsync(string id);
    }
}