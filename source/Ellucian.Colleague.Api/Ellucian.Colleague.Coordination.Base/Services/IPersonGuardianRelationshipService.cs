// Copyright 2016 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    public interface IPersonGuardianRelationshipService : IBaseService
    {
        /// <summary>
        /// Gets person guardian relationship by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Dtos.PersonGuardianRelationship> GetPersonGuardianRelationshipByIdAsync(string id);

        /// <summary>
        /// Gets all person guardian relationships or by filter
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        Task<Tuple<IEnumerable<Dtos.PersonGuardianRelationship>, int>> GetPersonGuardianRelationshipsAllAndFilterAsync(int offset, int limit, string person = "");
    }
}
