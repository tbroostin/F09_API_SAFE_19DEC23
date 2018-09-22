//Copyright 2016 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.Student;
using Ellucian.Colleague.Dtos;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Admission Application Types services
    /// </summary>
    public interface IAdmissionApplicationTypesService
    {
        Task<IEnumerable<AdmissionApplicationTypes>> GetAdmissionApplicationTypesAsync(bool bypassCache = false);
        Task<AdmissionApplicationTypes> GetAdmissionApplicationTypesByGuidAsync(string id);
    }
}
