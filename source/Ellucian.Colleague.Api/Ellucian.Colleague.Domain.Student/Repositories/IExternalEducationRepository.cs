/*Copyright 2016-2017 Ellucian Company L.P. and its affiliates.*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    public interface IExternalEducationRepository
    {      
        Task<Tuple<IEnumerable<ExternalEducation>, int>> GetExternalEducationAsync(int offset, int limit, bool bypassCache = false, string personId = "");    
        Task<ExternalEducation> GetExternalEducationByGuidAsync(string guid);
        Task<string> GetExternalEducationIdFromGuidAsync(string guid);
        Task<ExternalEducation> GetExternalEducationByIdAsync(string id);
    }
}