// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for EducationalInstitutionUnits services
    /// </summary>
    public interface IEducationalInstitutionsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitution>, int>> GetEducationalInstitutionsByTypeAsync(int offset, int limit, Dtos.EnumProperties.EducationalInstitutionType? type, bool bypassCache = false);
        Task<Ellucian.Colleague.Dtos.EducationalInstitution> GetEducationalInstitutionByGuidAsync(string id, bool bypassCache = false);
    }
}