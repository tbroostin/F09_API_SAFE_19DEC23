//Copyright 2019-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Exceptions;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Web.Http.Controllers;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class BulkLoadRequestService : BaseCoordinationService, IBulkLoadRequestService
    {
        private readonly IBulkRequestRepository _bulkLoadRequestRepository;

        public BulkLoadRequestService(
            IBulkRequestRepository bulkRequestRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _bulkLoadRequestRepository = bulkRequestRepository;
        }

        /// <summary>
        /// Create a BulkLoadRequest.
        /// </summary>
        /// <param name="bulkLoadRequest">The <see cref="bulkLoadRequest">BulkLoadRequest</see> entity to create in the database.</param>
        /// <param name="permissionCode"></param>
        /// <param name="dataPrivacyList"></param>
        /// <returns>The newly created <see cref="bulkLoadRequest">BulkLoadRequest</see></returns>
        public async Task<BulkLoadRequest> CreateBulkLoadRequestAsync(BulkLoadRequest bulkLoadRequest, string permissionCode)
        {
            if (bulkLoadRequest == null)
                throw new ArgumentNullException("BulkLoadRequest", "Must provide a BulkLoadRequest payload to start Bulk Load.");
            if (string.IsNullOrEmpty(bulkLoadRequest.RequestorTrackingId))
                throw new ArgumentNullException("BulkLoadRequest", "Must provide the Requestor Tracking Id to start a Bulk Load.");

            CheckCreateBulkLoadRequestPermission(permissionCode, bulkLoadRequest.ResourceName);

            try
            {
                var dataPrivacyList = await GetDataPrivacyListByApi(bulkLoadRequest.ResourceName);

                var bulkLoadRequestEntity = ConvertBulkLoadRequestDtoToEntityAsync(bulkLoadRequest.RequestorTrackingId, bulkLoadRequest);
                
                var createdBulkLoadRequestEntity = await _bulkLoadRequestRepository.CreateBulkLoadRequestAsync(bulkLoadRequestEntity, dataPrivacyList);
                
                return ConvertBulkLoadRequestEntityToDtoAsync(createdBulkLoadRequestEntity);

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message, ex.InnerException);
            }
        }

        public bool IsBulkLoadSupported()
        {
            try
            {
                return _bulkLoadRequestRepository.IsBulkLoadSupported();
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new ColleagueWebApiException(ex.Message, ex.InnerException);
            }
        }


        private BulkRequest ConvertBulkLoadRequestDtoToEntityAsync(string bulkLoadRequestId, BulkLoadRequest source)
        {
            var bulkRequestEntity = new BulkRequest
            {
                RequestorTrackingId = source.RequestorTrackingId,
                ApplicationId = source.ApplicationId,
                JobNumber = source.JobNumber,
                Message = source.Message,
                Representation = source.Representation,
                ResourceName = source.ResourceName
            };

            return bulkRequestEntity;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to create/update BulkLoadRequest.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateBulkLoadRequestPermission(string permissionCode, string resourceName)
        {
            bool hasPermission = HasPermission(permissionCode);

            // User is not allowed to create or update Bulkloadrequest without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to create/update BulkLoadRequest for the resource " + resourceName);
            }
        }


        /// <summary>
        /// Helper method to determine if the user has permission to view BulkLoadRequest.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewBulkLoadRequestPermission(string permissionCode, string resourceName)
        {
            bool hasPermission = HasPermission(permissionCode);

            // User is not allowed to create or update Bulkloadrequest without the appropriate permissions
            if (!hasPermission)
            {
                throw new PermissionsException("User " + CurrentUser.UserId + " does not have permission to view BulkLoadRequest for the resource " + resourceName);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Bulkloadrequest domain entity to its corresponding Bulkloadrequest DTO
        /// </summary>
        /// <param name="source">Bulkloadrequest domain entity</param>
        /// <returns>Bulkloadrequest DTO</returns>
        private BulkLoadRequest ConvertBulkLoadRequestEntityToDtoAsync(BulkRequest source)
        {
            var bulkLoadRequest = new BulkLoadRequest
            {
                RequestorTrackingId = source.RequestorTrackingId,
                ApplicationId = source.ApplicationId,
                JobNumber = source.JobNumber,
                Message = source.Message,
                Representation = source.Representation,
                ResourceName = source.ResourceName,
                Status = ConvertBulkLoadRequestStatusDomainEnumToBulkLoadRequestStatusDtoEnum(source.Status)
            };

            return bulkLoadRequest;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BulkloadrequestStatus domain enumeration value to its corresponding BulkloadrequestStatus DTO enumeration value
        /// </summary>
        /// <param name="source">BulkloadrequestStatus domain enumeration value</param>
        /// <returns>BulkloadrequestStatus DTO enumeration value</returns>
        private BulkLoadRequestStatus ConvertBulkLoadRequestStatusDomainEnumToBulkLoadRequestStatusDtoEnum(BulkRequestStatus source)
        {
            switch (source)
            {
                case BulkRequestStatus.InProgress:
                    return BulkLoadRequestStatus.InProgress;
                case BulkRequestStatus.Completed:
                    return BulkLoadRequestStatus.Completed;
                case BulkRequestStatus.PackagingCompleted:
                    return BulkLoadRequestStatus.PackagingCompleted;
                case BulkRequestStatus.Error:
                    return BulkLoadRequestStatus.Error;
                default:
                    return BulkLoadRequestStatus.InProgress;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BulkloadrequestStatus DTO enumeration value to its corresponding BulkloadrequestStatus entity enumeration value
        /// </summary>
        /// <param name="source">BulkloadrequestStatus DTO enumeration value</param>
        /// <returns>BulkloadrequestStatus entity enumeration value</returns>
        private BulkRequestStatus ConvertBulkLoadRequestStatusDtoEnumToBulkLoadRequestStatusEntityEnum(BulkLoadRequestStatus source)
        {
            switch (source)
            {

                case BulkLoadRequestStatus.InProgress:
                    return BulkRequestStatus.InProgress;
                case BulkLoadRequestStatus.Completed:
                    return BulkRequestStatus.Completed;
                case BulkLoadRequestStatus.PackagingCompleted:
                    return BulkRequestStatus.PackagingCompleted;
                case BulkLoadRequestStatus.Error:
                    return BulkRequestStatus.Error;
                default:
                    return BulkRequestStatus.InProgress;
            }
        }


        /// <summary>
        /// Get the status of the bulk request
        /// </summary>
        /// <param name="resourceName">Name of the bulk resource</param>
        /// <param name="id">id of the bulk request</param>
        /// <param name="permissionCode">Permission</param>
        /// <returns></returns>
        public async Task<BulkLoadGet> GetBulkLoadRequestStatus(string resourceName, string id, string permissionCode)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ColleagueWebApiException("Id must have a value");
            }
            if (string.IsNullOrEmpty(resourceName))
            {
                throw new ColleagueWebApiException("resourceName must have a value");
            }

            CheckViewBulkLoadRequestPermission(permissionCode, resourceName);

            try
            {
                var bulkRequestEntity = await _bulkLoadRequestRepository.GetBulkRequestDetails(resourceName, id);
                return ConvertBulkRequestEntityToBulkLoadGetDto(bulkRequestEntity);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private BulkLoadGet ConvertBulkRequestEntityToBulkLoadGetDto(BulkRequestDetails source)
        {
            var bulkLoadReturn = new BulkLoadGet();

            if (source == null) return bulkLoadReturn;

            bulkLoadReturn.ApplicationId = source.ApplicationId;
            bulkLoadReturn.JobNumber = source.JobNumber;
            bulkLoadReturn.Representation = source.Representation;
            bulkLoadReturn.RequestorTrackingId = source.RequestorTrackingId;
            bulkLoadReturn.ResourceName = source.ResourceName;
            bulkLoadReturn.Status = source.Status;
            bulkLoadReturn.TenantId = source.TenantId;
            bulkLoadReturn.XTotalCount = source.XTotalCount;

            if (source.Errors != null && source.Errors.Any())
            {
                var bulkRequestErrorList = source.Errors.Select(error => new BulkLoadGetError()
                {
                    Code = error.Code, Id = error.Id, Message = error.Message

                }).ToList();
                bulkLoadReturn.Errors = bulkRequestErrorList;
            }
            if (source.ProcessingSteps != null && source.ProcessingSteps.Any())
            {
                var bulkRequestProcessList = source.ProcessingSteps.Select(process => new BulkLoadProcessingStep()
                {
                    Count = process.Count,
                    ElapsedTime = process.ElapsedTime,
                    JobNumber = process.JobNumber,
                    Seq = process.Seq,
                    StartTime = process.StartTime,
                    Status = process.Status,
                    Step = process.Step
                }).ToList();

                bulkLoadReturn.ProcessingSteps = bulkRequestProcessList;
            }

            return bulkLoadReturn;
        }
    }
}