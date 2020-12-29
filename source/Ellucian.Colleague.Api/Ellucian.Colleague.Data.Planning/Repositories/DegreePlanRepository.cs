// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ellucian.Colleague.Domain.Planning.Entities;
using Ellucian.Colleague.Domain.Planning.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using slf4net;
using Ellucian.Web.Dependency;
using Ellucian.Web.Cache;
using Ellucian.Web.Http.Configuration;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Data.Planning.Repositories
{
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class DegreePlanRepository : BaseColleagueRepository, IDegreePlanRepository
    {
        //private ApplValcodes waitlistStatuses;
        private readonly string colleagueTimeZone;
        readonly int readSize;

        public DegreePlanRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings settings)
            : base(cacheProvider, transactionFactory, logger)
        {
            // Caching degree plans only for verification purposes
            CacheTimeout = Level1CacheTimeoutValue;
            colleagueTimeZone = settings.ColleagueTimeZone;
            readSize = settings.BulkReadSize;
        }

        /// <summary>
        /// Get requested degree plans for review
        /// </summary>
        /// <returns>DegreePlanReviewRequest Collection</returns>
        public async Task<IEnumerable<DegreePlanReviewRequest>> GetReviewReqestedAsync()
        {
            string searchString = "DP.REVIEW.REQUESTED EQ 'Y'";
            var degreePlans = await DataReader.BulkReadRecordAsync<Student.DataContracts.DegreePlan>("DEGREE_PLAN", searchString);

            if (degreePlans == null)
            {
                throw new ArgumentException("No Degree Plan found which requested for review");
            }
            else
            {
                //Get Degree Plans review assigned data
                var degreePlanIds = degreePlans.Select(d => d.Recordkey).Distinct().ToArray();
                var degreePlanReviewAssginments = await DataReader.BulkReadRecordAsync<DataContracts.DegreePlanRvwAsgn>(degreePlanIds);

                var degreePlanReviewRequests = await BuildDegreePlanReviewRequestsAsync(degreePlans, degreePlanReviewAssginments);

                return degreePlanReviewRequests;
            }
        }

        /// <summary>
        /// Create or Update degree plan review assignments
        /// </summary>
        /// <param name="degreePlanReviewRequest"></param>
        /// <returns></returns>
        public async Task<DegreePlanReviewRequest> UpdateAdvisorAssignment(DegreePlanReviewRequest degreePlanReviewRequest)
        {
            var updateDegreePlanReviewAssignment = new Student.Transactions.MaintDegreePlanRvwAsgnRequest();
            updateDegreePlanReviewAssignment.ADegreePlanId = degreePlanReviewRequest.Id;
            updateDegreePlanReviewAssignment.AReviewerId = degreePlanReviewRequest.AssignedReviewer;

            if (!string.IsNullOrEmpty(degreePlanReviewRequest.AssignedReviewer))
            {
                updateDegreePlanReviewAssignment.AAction = "C";
                var createResponse = await transactionInvoker.ExecuteAsync<Student.Transactions.MaintDegreePlanRvwAsgnRequest, Student.Transactions.MaintDegreePlanRvwAsgnResponse>(updateDegreePlanReviewAssignment);
                if (!string.IsNullOrEmpty(createResponse.AErrorMessage))
                {
                    throw new ArgumentException("Unable to create degree plan review request");
                }
            }
            else
            {
                updateDegreePlanReviewAssignment.AAction = "D";
                var deleteResponse = await transactionInvoker.ExecuteAsync<Student.Transactions.MaintDegreePlanRvwAsgnRequest, Student.Transactions.MaintDegreePlanRvwAsgnResponse>(updateDegreePlanReviewAssignment);
                if (!string.IsNullOrEmpty(deleteResponse.AErrorMessage))
                {
                    throw new ArgumentException("Unable to delete degree plan review request");
                }
            }
            return degreePlanReviewRequest;
        }

        /// <summary>
        /// Build degree plan review request collection
        /// </summary>
        /// <param name="plans"></param>
        /// <param name="degreePlanReviewAssginments"></param>
        /// <returns></returns>
        private async Task<IEnumerable<DegreePlanReviewRequest>> BuildDegreePlanReviewRequestsAsync(ICollection<Student.DataContracts.DegreePlan> plans, Collection<DataContracts.DegreePlanRvwAsgn> degreePlanReviewAssginments)
        {
            Collection<DegreePlanReviewRequest> degreePlanReviewRequests = new Collection<DegreePlanReviewRequest>();

            foreach (var plan in plans)
            {
                DegreePlanReviewRequest degreePlanReviewRequest = new DegreePlanReviewRequest();

                var dpReviewAssgnContract = degreePlanReviewAssginments.FirstOrDefault(d => d.Recordkey == plan.Recordkey);

                degreePlanReviewRequest.Id = plan.Recordkey;
                degreePlanReviewRequest.PersonId = plan.DpStudentId;
                degreePlanReviewRequest.ReviewRequestedDate = plan.DpReviewRequestedDate;

                if (dpReviewAssgnContract != null)
                {
                    degreePlanReviewRequest.AssignedReviewer = dpReviewAssgnContract.DpraAssignedReviewer;
                }
                degreePlanReviewRequests.Add(degreePlanReviewRequest);
            }
            return degreePlanReviewRequests;
        }
    }
}
