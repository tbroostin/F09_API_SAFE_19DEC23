//Copyright 2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for SectionStatuses services
    /// </summary>
    public interface ISectionStatusesService : IBaseService
    {
          
         Task<IEnumerable<Ellucian.Colleague.Dtos.SectionStatuses>> GetSectionStatusesAsync(bool bypassCache = false);
               
        Task<Ellucian.Colleague.Dtos.SectionStatuses> GetSectionStatusesByGuidAsync(string id);
    }
}
