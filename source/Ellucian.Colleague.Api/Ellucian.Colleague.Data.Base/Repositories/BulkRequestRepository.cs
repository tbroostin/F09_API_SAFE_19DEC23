//Copyright 2020-2022 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ellucian.Colleague.Data.Base.Transactions;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Entities;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using slf4net;

namespace Ellucian.Colleague.Data.Base.Repositories
{   
    /// <summary>
    /// Repository for bulk request
    /// </summary>
    [RegisterType(Lifetime = RegistrationLifetime.Hierarchy)]
    public class BulkRequestRepository : BaseColleagueRepository, IBulkRequestRepository
    {
        public BulkRequestRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger) : base(cacheProvider, transactionFactory, logger)
        {
            CacheTimeout = Level1CacheTimeoutValue;
        }

        /// <summary>
        /// Create the bulk request
        /// </summary>
        /// <param name="bulkRequest"></param>
        /// <param name="dataPrivacyList"></param>
        /// <returns>created bulk request details</returns>
        public async Task<BulkRequest> CreateBulkLoadRequestAsync(BulkRequest bulkRequest, IEnumerable<string> dataPrivacyList)
        {
            try
            {
                var createBulkRequest = new CreateBulkRequestRequest
                {
                    Representation = bulkRequest.Representation,
                    ApplicationId = bulkRequest.ApplicationId,
                    RequestorTrackingId = bulkRequest.RequestorTrackingId,
                    ResourceName = bulkRequest.ResourceName,
                    RestrictedContent = dataPrivacyList.ToList()
                };
                
                var response = await transactionInvoker.ExecuteAsync<CreateBulkRequestRequest, CreateBulkRequestResponse>(createBulkRequest);
                
                var bulkRequestResponse = new BulkRequest();
                
                if (response != null)
                {
                    bulkRequestResponse.Representation = bulkRequest.Representation;
                    bulkRequestResponse.ApplicationId = bulkRequest.ApplicationId;
                    bulkRequestResponse.RequestorTrackingId = bulkRequest.RequestorTrackingId;
                    bulkRequestResponse.ResourceName = bulkRequest.ResourceName;

                    bulkRequestResponse.JobNumber = response.JobNumber;
                    bulkRequestResponse.Message = response.Message;

                    if (response.Status.Equals("Error", StringComparison.OrdinalIgnoreCase))
                    {
                        bulkRequestResponse.Status = BulkRequestStatus.Error;
                    }
                    else if (response.Status.Equals("launched", StringComparison.OrdinalIgnoreCase))
                    {
                        bulkRequestResponse.Status = BulkRequestStatus.InProgress;
                    }
                    else if (response.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        bulkRequestResponse.Status = BulkRequestStatus.Completed;
                    }
                    else if (response.Status.Equals("PackagingCompleted", StringComparison.OrdinalIgnoreCase))
                    {
                        bulkRequestResponse.Status = BulkRequestStatus.PackagingCompleted;
                    }
                    else
                    {
                        bulkRequestResponse.Status = BulkRequestStatus.Error;
                    }
                }
                else
                {
                    throw new ColleagueWebApiException("Create Bulk Failed to send response");
                }

                return bulkRequestResponse;

            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<BulkRequestDetails> GetBulkRequestDetails(string resourceName, string id)
        {
            try
            {
                var getBulkRequestStatus = new GetBulkRequestStatusRequest()
                {
                    requestorTrackingId = id,
                    resourceName = resourceName
                };

                var response = await transactionInvoker.ExecuteAsync<GetBulkRequestStatusRequest, GetBulkRequestStatusResponse>(getBulkRequestStatus);

                var bulkRequestStatusResponse = new BulkRequestDetails();

                if (response != null)
                {
                    bulkRequestStatusResponse.Representation = !string.IsNullOrEmpty(response.representation) ? response.representation : null;
                    bulkRequestStatusResponse.ApplicationId = !string.IsNullOrEmpty(response.applicationId) ? response.applicationId : null;
                    bulkRequestStatusResponse.JobNumber = !string.IsNullOrEmpty(response.jobNumber) ? response.jobNumber : null;
                    bulkRequestStatusResponse.RequestorTrackingId = !string.IsNullOrEmpty(response.requestorTrackingId) ? response.requestorTrackingId : null;
                    bulkRequestStatusResponse.ResourceName = !string.IsNullOrEmpty(response.resourceName) ? response.resourceName : null;
                    bulkRequestStatusResponse.Status = !string.IsNullOrEmpty(response.jobStatus) ? response.jobStatus : null;
                    bulkRequestStatusResponse.TenantId = !string.IsNullOrEmpty(response.tenantId) ? response.tenantId : null;
                    bulkRequestStatusResponse.XTotalCount = !string.IsNullOrEmpty(response.xTotalCount) ? response.xTotalCount : null;
                    if (response.errors != null && response.errors.Any())
                    {
                        var bulkRequestErrorList = new List<BulkRequestGetError>();
                        foreach (var error in response.errors)
                        {
                            var bulkError = new BulkRequestGetError()
                            {
                                Code = !string.IsNullOrEmpty(error.code) ? error.code : null,
                                Id = !string.IsNullOrEmpty(error.id) ? error.id : null,
                                Message = !string.IsNullOrEmpty(error.message) ? error.message : null
                            };
                            bulkRequestErrorList.Add(bulkError);
                        }
                        bulkRequestStatusResponse.Errors = bulkRequestErrorList;
                    }
                    if (response.processingSteps != null && response.processingSteps.Any())
                    {
                        var bulkRequestProcessList = new List<BulkRequestProcessingStep>();
                        foreach (var process in response.processingSteps)
                        {
                            var bulkProcessStep = new BulkRequestProcessingStep()
                            {
                                Count = !string.IsNullOrEmpty(process.count) ? process.count : null,
                                ElapsedTime = !string.IsNullOrEmpty(process.elapsedTime) ? process.elapsedTime : null,
                                JobNumber = !string.IsNullOrEmpty(process.stepJobNumber) ? process.stepJobNumber : null,
                                Seq = !string.IsNullOrEmpty(process.seq) ? process.seq : null,
                                StartTime = !string.IsNullOrEmpty(process.startTime) ? process.startTime : null,
                                Status = !string.IsNullOrEmpty(process.status) ? process.status : null,
                                Step = !string.IsNullOrEmpty(process.step) ? process.step : null
                            };
                            bulkRequestProcessList.Add(bulkProcessStep);
                        }
                        bulkRequestStatusResponse.ProcessingSteps = bulkRequestProcessList;
                    }
                }
                else
                {
                    throw new KeyNotFoundException(string.Concat("Bulk Load was not found with requestor tracking id ", id));
                }

                return bulkRequestStatusResponse;

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Gets the status of bulk load support
        /// </summary>
        /// <returns></returns>
        public bool IsBulkLoadSupported()
        {
            bool bulkStatus = false;

            try
            {
                var getBulkSupport = new GetBulkRequestEnabledRequest();

                var response = transactionInvoker.Execute<GetBulkRequestEnabledRequest, GetBulkRequestEnabledResponse>(getBulkSupport);

                if (response != null)
                {
                    bulkStatus = response.Enabled;
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return bulkStatus;
        }
    }
}
