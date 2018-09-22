// Copyright 2016-2017 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Interface for EducationalInstitutionUnits services
    /// </summary>
    public interface IEducationalInstitutionUnitsService : IBaseService
    {
        Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitutionUnits>> GetEducationalInstitutionUnitsAsync(bool ignoreCache);
        Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitutionUnits>> GetEducationalInstitutionUnitsByTypeAsync(string type, bool ignoreCache = false);
        Task<Ellucian.Colleague.Dtos.EducationalInstitutionUnits> GetEducationalInstitutionUnitsByGuidAsync(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitutionUnits2>> GetEducationalInstitutionUnits2Async(bool ignoreCache);
        Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitutionUnits2>> GetEducationalInstitutionUnitsByType2Async(string type, bool ignoreCache = false);
        Task<Ellucian.Colleague.Dtos.EducationalInstitutionUnits2> GetEducationalInstitutionUnitsByGuid2Async(string id);

        Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitutionUnits3>> GetEducationalInstitutionUnits3Async(
            bool ignoreCache = false, 
            Dtos.EnumProperties.EducationalInstitutionUnitType educationalInstitutionUnitType = Dtos.EnumProperties.EducationalInstitutionUnitType.NotSet,
            Dtos.EnumProperties.Status departmentStatus = Dtos.EnumProperties.Status.NotSet);
       // Task<IEnumerable<Ellucian.Colleague.Dtos.EducationalInstitutionUnits3>> GetEducationalInstitutionUnitsByType3Async(string type, bool ignoreCache = false);
        Task<Ellucian.Colleague.Dtos.EducationalInstitutionUnits3> GetEducationalInstitutionUnitsByGuid3Async(string guid, bool ignoreCache);
    }
}