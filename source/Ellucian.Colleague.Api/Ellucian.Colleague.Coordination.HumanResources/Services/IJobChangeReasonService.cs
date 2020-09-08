/* Copyright 2016-2020 Ellucian Company L.P. and its affiliates. */

using Ellucian.Colleague.Coordination.Base.Services;
using System;
using System.Threading.Tasks;
namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    public interface IJobChangeReasonService : IBaseService 
    {
        Task<Ellucian.Colleague.Dtos.JobChangeReason> GetJobChangeReasonByGuidAsync(string guid);
        Task<System.Collections.Generic.IEnumerable<Ellucian.Colleague.Dtos.JobChangeReason>> GetJobChangeReasonsAsync(bool bypassCache = false);
    }
}
