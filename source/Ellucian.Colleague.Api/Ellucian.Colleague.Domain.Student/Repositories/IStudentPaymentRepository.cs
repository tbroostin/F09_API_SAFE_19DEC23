// Copyright 2017 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Student.Entities;
using System.Collections.Generic;
using System;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Domain.Student.Repositories
{
    /// <summary>
    /// Definition of methods implemented for a StudentPayment repository
    /// </summary>
    public interface IStudentPaymentRepository : IEthosExtended
    {
        Task<StudentPayment> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<StudentPayment>, int>> GetAsync(int offset, int limit, bool bypassCache, string personId = "", string term = "", string arCode = "", string chargeType = "");

        Task<Tuple<IEnumerable<StudentPayment>, int>> GetAsync2(int offset, int limit, bool bypassCache, string personId = "", string term = "", string distrCode = "", string chargeType = "", string arType = "", string usage = "");

        Task<StudentPayment> UpdateAsync(string id, StudentPayment studentPayment);

        Task<StudentPayment> CreateAsync(StudentPayment studentPaymentsEntity);

        Task<StudentPayment> CreateAsync2(StudentPayment studentPaymentsEntity);

        Task<StudentPayment> DeleteAsync(string id);
    }
}
