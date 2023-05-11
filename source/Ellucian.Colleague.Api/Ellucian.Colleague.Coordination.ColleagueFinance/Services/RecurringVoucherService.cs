// Copyright 2015-2022 Ellucian Company L.P. and its affiliates.

using System;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Coordination.ColleagueFinance.Adapters;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using Ellucian.Colleague.Domain.ColleagueFinance;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IRecurringVoucherService interface
    /// </summary>
    [RegisterType]
    public class RecurringVoucherService : BaseCoordinationService, IRecurringVoucherService
    {
        private IRecurringVoucherRepository recurringVoucherRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerUserRepository generalLedgerUserRepository;
        private IApprovalConfigurationRepository approvalConfigurationRepository;

        // This constructor initializes the private attributes
        public RecurringVoucherService(IRecurringVoucherRepository recurringVoucherRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IApprovalConfigurationRepository approvalConfigurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.recurringVoucherRepository = recurringVoucherRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
            this.approvalConfigurationRepository = approvalConfigurationRepository;
        }

        /// <summary>
        /// Returns the DTO for the specified recurring voucher
        /// </summary>
        /// <param name="id">ID of the requested recurring voucher</param>
        /// <returns>Recurring Voucher DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.RecurringVoucher> GetRecurringVoucherAsync(string id)
        {
            // Check the permission code to view a recurring voucher.
            CheckViewRecurringVoucherPermission();

            // Get the GL Configuration to get the name of the full GL account access role
            // and also provides the information to format the GL accounts
            var glConfiguration = await generalLedgerConfigurationRepository.GetAccountStructureAsync();
            if (glConfiguration == null)
            {
                throw new ArgumentNullException("glConfiguration", "glConfiguration cannot be null");
            }

            // Get the GL class configuration because it is used by the GL user repository.
            var glClassConfiguration = await generalLedgerConfigurationRepository.GetClassConfigurationAsync();
            if (glClassConfiguration == null)
            {
                throw new ArgumentNullException("glClassConfiguration", "glClassConfiguration cannot be null");
            }

            // Get the ID for the person who is logged in, and use the ID to get their GL access level.
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration.ClassificationName, glClassConfiguration.ExpenseClassValues);
            if (generalLedgerUser == null)
            {
                throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
            }

            // If the user has access to all the GL accounts through the GL access role,
            // don't bother adding any additional GL accounts they may have access through
            // approval roles.
            IEnumerable<string> allAccessAndApprovalAccounts = new List<string>();
            if (generalLedgerUser.GlAccessLevel != GlAccessLevel.Full_Access)
            {
                ApprovalConfiguration approvalConfiguration = null;
                try
                {
                    approvalConfiguration = await approvalConfigurationRepository.GetApprovalConfigurationAsync();
                }
                catch (Exception e)
                {
                    logger.Debug(e, "Approval Roles are not configured.");
                }

                // Check if recurring vouchers use approval roles.
                if (approvalConfiguration != null && approvalConfiguration.RecurringVouchersUseApprovalRoles)
                {
                    // Use the approval roles to see if they have access to additional GL accounts.
                    allAccessAndApprovalAccounts = await generalLedgerUserRepository.GetGlUserApprovalAndGlAccessAccountsAsync(CurrentUser.PersonId, generalLedgerUser.AllAccounts);
                }
                else
                {
                    // If approval roles aren't being used, just use the GL Access
                    allAccessAndApprovalAccounts = generalLedgerUser.AllAccounts;
                }

                // If the user has access to some GL accounts via approval 
                // roles, change the GL access level to possible access.
                if (allAccessAndApprovalAccounts != null && allAccessAndApprovalAccounts.Any())
                {
                    generalLedgerUser.SetGlAccessLevel(GlAccessLevel.Possible_Access);
                }
            }
            // Get the recurring voucher domain entity from the repository
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync(id, generalLedgerUser.GlAccessLevel, allAccessAndApprovalAccounts);

            if (recurringVoucherDomainEntity == null)
            {
                throw new ArgumentNullException("recurringVoucherDomainEntity", "recurringVoucherDomainEntity cannot be null.");
            }

            // Convert the recurring voucher and all its child objects into DTOs
            var recurringVoucherDtoAdapter = new RecurringVoucherEntityToDtoAdapter(_adapterRegistry, logger);
            var recurringVoucherDto = recurringVoucherDtoAdapter.MapToType(recurringVoucherDomainEntity);

            return recurringVoucherDto;
        }

        /// <summary>
        /// Permission code that allows a READ operation on a recurring voucher.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewRecurringVoucherPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewRecurringVoucher);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view recurring vouchers.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }
}
