// Copyright 2018-2020 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class ApproverService : BaseCoordinationService, IApproverService
    {
        private IApproverRepository approverRepository;

        // This constructor initializes the private attributes.
        public ApproverService(IApproverRepository approverRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.approverRepository = approverRepository;
        }

        /// <summary>
        /// Validate an approver ID. 
        /// A next approver ID and an approver ID are the same. They are just
        /// populated under different circumstances.
        /// </summary>
        /// <param name="approverId">Approver ID.</param>
        /// <returns>An approver validation response DTO.</returns>
        public async Task<NextApproverValidationResponse> ValidateApproverAsync(string approverId)
        {
            if (string.IsNullOrEmpty(approverId))
            {
                throw new ArgumentNullException("approverId", "approverId is required.");
            }

            // Check the permission code to create or update a budget adjustment because we are using
            // this validation API in budget adjustments. May need to add more permissions later or 
            // create its own permission.
            CheckCreateUpdateBudgetAdjustmentPermission();

            // Call the repository to validate the approver ID.
            var approverValidationResponseEntity = await approverRepository.ValidateApproverAsync(approverId);

            if (approverValidationResponseEntity == null)
            {
                throw new ApplicationException("No approver validation response entity returned.");
            }

            // Get the adapter
            var adapter = new NextAppoverValidationResponseEntityToDtoAdapter(_adapterRegistry, logger);

            // Initialize the DTO.
            var nextApproverValidationResponseDto = new NextApproverValidationResponse();

            // Convert the domain entity into a DTO
            nextApproverValidationResponseDto = adapter.MapToType(approverValidationResponseEntity);

            return nextApproverValidationResponseDto;
        }

        /// <summary>
        /// Get the list of next approver based on keyword search.
        /// </summary>
        /// <param name="queryKeyword"> The search criteria containing keyword for NextApprover search.</param>
        /// <returns> The staff search results</returns> 
        public async Task<IEnumerable<NextApprover>> QueryNextApproverByKeywordAsync(string queryKeyword)
        {
            List<NextApprover> nextApproverDtos = new List<NextApprover>();

            if (string.IsNullOrEmpty(queryKeyword))
            {
                string message = "query keyword is required to query.";
                throw new ArgumentNullException(message);
            }

            // Check the permission code to view next approver information.
            CheckViewNextApproverPermissions();

            // Get the list of vendor search result domain entity from the repository
            var nextApproverDomainEntities = await approverRepository.QueryNextApproverByKeywordAsync(queryKeyword.Trim());

            if (nextApproverDomainEntities == null || !nextApproverDomainEntities.Any())
            {
                return nextApproverDtos;
            }

            //sorting
            nextApproverDomainEntities = nextApproverDomainEntities.OrderBy(item => item.NextApproverName).ThenBy(x => x.NextApproverId);

            // Convert the vendor search result into DTOs
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.NextApprover, NextApprover>();
            foreach (var nextApproverDomainEntity in nextApproverDomainEntities)
            {
                nextApproverDtos.Add(dtoAdapter.MapToType(nextApproverDomainEntity));
            }

            return nextApproverDtos;
        }

        /// <summary>
        /// Permission code that allows a CREATE and UPDATE operation
        /// on both a draft budget adjustment and a budget adjutment.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckCreateUpdateBudgetAdjustmentPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.CreateUpdateBudgetAdjustments);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to create or update budget adjustments.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view Next Approver information.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewNextApproverPermissions()
        {
            var hasPermission = (HasPermission(BasePermissionCodes.ViewAnyPerson) ||    
                                    HasPermission(ColleagueFinancePermissionCodes.CreateUpdateRequisition) ||
                                    HasPermission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder) || 
                                    HasPermission(ColleagueFinancePermissionCodes.CreateUpdateVoucher) ||
                                    HasPermission(ColleagueFinancePermissionCodes.CreateUpdateBudgetAdjustments));
            
            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view next approver information.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }
}