// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a StudentCharges repository
    /// </summary>
    public interface IStudentChargeRepository : IEthosExtended
    {
        Task<StudentCharge> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<StudentCharge>, int>> GetAsync(int offset, int limit, bool bypassCache, string personId = "", string term = "", string arCode = "", string arType = "", string chargeType = "", string usage = "");

        Task<StudentCharge> UpdateAsync(string id, StudentCharge studentCharge);

        Task<StudentCharge> CreateAsync(StudentCharge studentChargesEntity);

        Task<StudentCharge> DeleteAsync(string id);
    }
}
