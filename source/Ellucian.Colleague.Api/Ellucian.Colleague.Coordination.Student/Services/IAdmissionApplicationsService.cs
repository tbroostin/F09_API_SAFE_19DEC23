//Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for AdmissionApplications services
    /// </summary>
    public interface IAdmissionApplicationsService : IBaseService
    {
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication>, int>> GetAdmissionApplicationsAsync(int offset, int limit, bool bypassCache);
        Task<Ellucian.Colleague.Dtos.AdmissionApplication> GetAdmissionApplicationsByGuidAsync(string id);
        Task<Tuple<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplication2>, int>> GetAdmissionApplications2Async(int offset, int limit, bool bypassCache);
        Task<Ellucian.Colleague.Dtos.AdmissionApplication2> GetAdmissionApplicationsByGuid2Async(string guid);
        Task<AdmissionApplication2> UpdateAdmissionApplicationAsync(string guid, AdmissionApplication2 admissionApplication, bool bypassCache);
        Task<AdmissionApplication2> CreateAdmissionApplicationAsync(AdmissionApplication2 admissionApplication, bool bypassCache);
    }
}