/*Copyright 2019 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    public interface IPersonExternalEducationCredentialsRepository : IEthosExtended
    {      
        Task<Tuple<IEnumerable<ExternalEducation>, int>> GetExternalEducationCredentialsAsync(int offset, int limit, string[] filterPersonIds = null, string personFilter = "", bool bypassCache = false);    
        Task<ExternalEducation> GetExternalEducationCredentialsByGuidAsync(string guid);
        Task<string> GetExternalEducationCredentialsIdFromGuidAsync(string guid);
        Task<string> GetExternalEducationIdFromGuidAsync(string guid);
        Task<ExternalEducation> GetExternalEducationCredentialsByIdAsync(string id);
        Task<ExternalEducation> UpdateExternalEducationCredentialsAsync(ExternalEducation externalEducation);
        Task<ExternalEducation> CreateExternalEducationCredentialsAsync(ExternalEducation externalEducation);
    }
}