// Copyright 2015-2018 Ellucian Company L.P. and its affiliates.

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

        // This constructor initializes the private attributes
        public RecurringVoucherService(IRecurringVoucherRepository recurringVoucherRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.recurringVoucherRepository = recurringVoucherRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
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

            // Get the recurring voucher domain entity from the repository
            var recurringVoucherDomainEntity = await recurringVoucherRepository.GetRecurringVoucherAsync(id, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts);

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
