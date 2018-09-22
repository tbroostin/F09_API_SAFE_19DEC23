// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using System.Collections.Generic;
using System;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Student Payments
    /// </summary>
    public interface IStudentPaymentService : IBaseService
    {
        Task<Dtos.StudentPayment> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<Dtos.StudentPayment>, int>> GetAsync(int offset, int limit, bool bypassCache, string personId = "", string academicPeriod = "", string accountingCode = "", string chargeType = "");

        Task<Dtos.StudentPayment> UpdateAsync(string id, Dtos.StudentPayment generalLedgerDto);

        Task<Dtos.StudentPayment> CreateAsync(Dtos.StudentPayment generalLedgerDto);

        Task<Dtos.StudentPayment2> GetByIdAsync2(string id);

        Task<Tuple<IEnumerable<Dtos.StudentPayment2>, int>> GetAsync2(int offset, int limit, bool bypassCache, string personId = "", string academicPeriod = "", string fundSource = "", string chargeType = "", string fundDestination = "");
        
        Task<Dtos.StudentPayment2> CreateAsync2(Dtos.StudentPayment2 generalLedgerDto);

        Task DeleteAsync(string id);
    }
}
