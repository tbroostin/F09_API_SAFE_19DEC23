// Copyright 2015 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IAcademicCredentialService : IBaseService
    {
        Task<IEnumerable<Dtos.AcademicCredential>> GetAcademicCredentialsAsync(bool bypassCache = false);
        Task<Dtos.AcademicCredential> GetAcademicCredentialByGuidAsync(string guid);
    }
}