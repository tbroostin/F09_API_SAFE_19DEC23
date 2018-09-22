//Copyright 2016 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Admission Populations services
    /// </summary>
    public interface IAdmissionPopulationsService: IBaseService
    {
        Task<IEnumerable<AdmissionPopulations>> GetAdmissionPopulationsAsync(bool bypassCache = false);
        Task<AdmissionPopulations> GetAdmissionPopulationsByGuidAsync(string id);
    }
}
