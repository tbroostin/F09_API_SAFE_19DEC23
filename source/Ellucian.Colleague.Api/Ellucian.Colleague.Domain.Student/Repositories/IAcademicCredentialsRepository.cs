// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for AcademicCredentials Repository
    /// </summary>
    public interface IAcademicCredentialsRepository 
    {
        
        Task<string> GetAcademicCredentialsIdFromGuidAsync(string guid);

        Task<Domain.Base.Entities.AcademicCredential> GetAcademicCredentialsByIdAsync(string id);

        Task<Tuple<IEnumerable<Domain.Base.Entities.AcademicCredential>, int>> GetAcademicCredentialsAsync(int offset, int limit, bool bypassCache = false);

    }
}