// Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Base.Entities;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Base.Repositories
{
    /// <summary>
    /// Interface for InstitutionsAttendRepository Repository
    /// </summary>
    public interface IInstitutionsAttendRepository: IEthosExtended
    {
        Task<string> GetInstitutionsAttendIdFromGuidAsync(string guid);

        Task<Domain.Base.Entities.InstitutionsAttend> GetInstitutionAttendByIdAsync(string id);

        Task<Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>> GetInstitutionsAttendAsync(int offset, int limit,
            string personId = "", string[] filterPersonIds = null, string personFilter = "", string personByInstitutionTypePersonId = "",
          InstType? typeFilter = null, bool bypassCache = false);

        Task<Dictionary<string, string>> GetInsAttendGuidsCollectionAsync(IEnumerable<string> insAttendIds);

        Task<InstitutionsAttend> UpdateExternalEducationAsync(InstitutionsAttend personExternalEducationEntity);
       
    }
}