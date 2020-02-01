// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System.Threading.Tasks;
using System.Collections.Generic;
using Ellucian.Colleague.Domain.Planning.Entities;

namespace Ellucian.Colleague.Domain.Planning.Repositories
{
    public interface IDegreePlanRepository
    {
        Task<IEnumerable<DegreePlanReviewRequest>> GetReviewReqestedAsync();
        Task<DegreePlanReviewRequest> UpdateAdvisorAssignment(DegreePlanReviewRequest degreePlanReviewRequest);
    }
}
