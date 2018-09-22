//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for SectionInstructors services
    /// </summary>
    public interface ISectionInstructorsService : IBaseService
    {
        Task<Tuple<IEnumerable<Dtos.SectionInstructors>, int>> GetSectionInstructorsAsync(int offset, int limit, string section, string instructor, List<string> instructionalEvents, bool bypassCache = false);
             
        Task<Dtos.SectionInstructors> GetSectionInstructorsByGuidAsync(string id);

        Task<Dtos.SectionInstructors> CreateSectionInstructorsAsync(Dtos.SectionInstructors sectionInstructors);

        Task<Dtos.SectionInstructors> UpdateSectionInstructorsAsync(string guid, Dtos.SectionInstructors sectionInstructors);

        Task DeleteSectionInstructorsAsync(string guid);
    }
}
