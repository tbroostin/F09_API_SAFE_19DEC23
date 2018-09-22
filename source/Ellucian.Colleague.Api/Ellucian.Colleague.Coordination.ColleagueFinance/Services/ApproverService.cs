// Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    }
}