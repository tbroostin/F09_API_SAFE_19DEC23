// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    public interface IAcademicDisciplineService : IBaseService
    {
        Task<IEnumerable<Dtos.AcademicDiscipline>> GetAcademicDisciplinesAsync(bool bypassCache = false);
        Task<IEnumerable<Dtos.AcademicDiscipline2>> GetAcademicDisciplines2Async(bool bypassCache = false);
        Task<IEnumerable<Dtos.AcademicDiscipline3>> GetAcademicDisciplines3Async(Dtos.EnumProperties.MajorStatus status, string type, bool bypassCache = false);
        Task<Dtos.AcademicDiscipline> GetAcademicDisciplineByGuidAsync(string guid);
        Task<Dtos.AcademicDiscipline2> GetAcademicDiscipline2ByGuidAsync(string guid, bool bypassCache = false);
    }
}
