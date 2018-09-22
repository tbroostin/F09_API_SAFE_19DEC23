// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Interface for InstitutionsAttend Repository
    /// </summary>
    public interface IInstitutionsAttendRepository
    {
        Task<string> GetInstitutionsAttendIdFromGuidAsync(string guid);

        Task<Domain.Base.Entities.InstitutionsAttend> GetInstitutionAttendByIdAsync(string id);

        Task<Tuple<IEnumerable<Domain.Base.Entities.InstitutionsAttend>, int>> GetInstitutionsAttendAsync(int offset, int limit, bool bypassCache = false, string personId = "");

        Task<Dictionary<string, string>> GetInsAttendGuidsCollectionAsync(IEnumerable<string> insAttendIds);

    }
}