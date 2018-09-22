// Copyright 2016 Ellucian Company L.P. and its affiliates.

using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using System.Collections.Generic;
using System;
using Ellucian.Colleague.Coordination.Base.Services;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    /// <summary>
    /// Interface for Student Charges
    /// </summary>
    public interface IStudentChargeService : IBaseService
    {
        Task<Dtos.StudentCharge> GetByIdAsync(string id);

        Task<Tuple<IEnumerable<Dtos.StudentCharge>, int>> GetAsync(int offset, int limit, bool bypassCache, string personId = "", string academicPeriod = "", string accountingCode = "", string chargeType = "");

        Task<Dtos.StudentCharge> UpdateAsync(string id, Dtos.StudentCharge generalLedgerDto);

        Task<Dtos.StudentCharge> CreateAsync(Dtos.StudentCharge generalLedgerDto);

        Task<Dtos.StudentCharge1> GetByIdAsync1(string id);

        Task<Tuple<IEnumerable<Dtos.StudentCharge1>, int>> GetAsync1(int offset, int limit, bool bypassCache, string personId = "", string academicPeriod = "", string fundingDestination = "", string fundingSource = "", string chargeType = "");
        
        Task<Dtos.StudentCharge1> CreateAsync1(Dtos.StudentCharge1 generalLedgerDto);

        Task DeleteAsync(string id);
    }
}
