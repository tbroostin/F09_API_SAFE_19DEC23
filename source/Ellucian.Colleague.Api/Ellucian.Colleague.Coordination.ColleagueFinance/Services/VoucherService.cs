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
    /// This class implements the IVoucherService interface.
    /// </summary>
    [RegisterType]
    public class VoucherService : BaseCoordinationService, IVoucherService
    {
        private IVoucherRepository voucherRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerUserRepository generalLedgerUserRepository;

        // This constructor initializes the private attributes.
        public VoucherService(IVoucherRepository voucherRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.voucherRepository = voucherRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
        }

        /// <summary>
        /// Returns the DTO for the specified voucher.
        /// </summary>
        /// <param name="id">ID of the requested voucher.</param>
        /// <returns>Voucher DTO.</returns>
        [Obsolete("OBSOLETE as of API 1.15. Please use GetVoucher2Async")]
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.Voucher> GetVoucherAsync(string id)
        {
            // Check the permission code to view a voucher.
            CheckViewVoucherPermission();

            // Get the GL Configuration so we know how to format the GL numbers, and get the full GL access role
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

            // Get the voucher domain entity from the repository.
            int versionNumber = 1;
            var voucherDomainEntity = await voucherRepository.GetVoucherAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts, versionNumber);

            if (voucherDomainEntity == null)
            {
                throw new ArgumentNullException("voucherDomainEntity", "voucherDomainEntity cannot be null.");
            }

            // Convert the project and all its child objects into DTOs.
            var voucherDtoAdapter = new VoucherEntityToDtoAdapter(_adapterRegistry, logger);
            var voucherDto = voucherDtoAdapter.MapToType(voucherDomainEntity, glConfiguration.MajorComponentStartPositions);

            // Throw an exception if there are no line items being returned since access to the document
            // is governed by access to the GL numbers on the line items, and a line item will not be returned
            // if the user does not have access to at least one of the line items.
            if (voucherDto.LineItems == null || voucherDto.LineItems.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access voucher.");
            }

            return voucherDto;
        }

        /// <summary>
        /// Returns the DTO for the specified voucher.
        /// </summary>
        /// <param name="id">ID of the requested voucher.</param>
        /// <returns>Voucher DTO.</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.Voucher2> GetVoucher2Async(string id)
        {
            // Check the permission code to view a voucher.
            CheckViewVoucherPermission();

            // Get the GL Configuration so we know how to format the GL numbers, and get the full GL access role
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
            var generalLedgerUser = await generalLedgerUserRepository.GetGeneralLedgerUserAsync2(CurrentUser.PersonId, glConfiguration.FullAccessRole, glClassConfiguration);
            if (generalLedgerUser == null)
            {
                throw new ArgumentNullException("generalLedgerUser", "generalLedgerUser cannot be null");
            }

            // Get the voucher domain entity from the repository.
            int versionNumber = 2;
            var voucherDomainEntity = await voucherRepository.GetVoucherAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts, versionNumber);

            if (voucherDomainEntity == null)
            {
                throw new ArgumentNullException("voucherDomainEntity", "voucherDomainEntity cannot be null.");
            }

            // Convert the project and all its child objects into DTOs.
            var voucherDtoAdapter = new Voucher2EntityToDtoAdapter(_adapterRegistry, logger);
            var voucherDto = voucherDtoAdapter.MapToType(voucherDomainEntity, glConfiguration.MajorComponentStartPositions);

            // Throw an exception if there are no line items being returned since access to the document
            // is governed by access to the GL numbers on the line items, and a line item will not be returned
            // if the user does not have access to at least one of the line items.
            if (voucherDto.LineItems == null || voucherDto.LineItems.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access voucher.");
            }

            return voucherDto;
        }

        /// <summary>
        /// Permission code that allows a READ operation on a voucher.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewVoucherPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewVoucher);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view vouchers.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }

    }
}
