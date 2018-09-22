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
    /// This class implements the IBlanketPurchaseOrderService interface
    /// </summary>
    [RegisterType]
    public class BlanketPurchaseOrderService : BaseCoordinationService, IBlanketPurchaseOrderService
    {
        private IBlanketPurchaseOrderRepository blanketPurchaseOrderRepository;
        private IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository;
        private IGeneralLedgerUserRepository generalLedgerUserRepository;

        // Constructor to initialize the private attributes
        public BlanketPurchaseOrderService(IBlanketPurchaseOrderRepository blanketPurchaseOrderRepository,
            IGeneralLedgerConfigurationRepository generalLedgerConfigurationRepository,
            IGeneralLedgerUserRepository generalLedgerUserRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this.blanketPurchaseOrderRepository = blanketPurchaseOrderRepository;
            this.generalLedgerConfigurationRepository = generalLedgerConfigurationRepository;
            this.generalLedgerUserRepository = generalLedgerUserRepository;
        }

        /// <summary>
        /// Returns the DTO for the specified blanket purchase order
        /// </summary>
        /// <param name="id">ID of the requested blanket purchase order</param>
        /// <returns>Blanket purchase order DTO</returns>
        public async Task<Ellucian.Colleague.Dtos.ColleagueFinance.BlanketPurchaseOrder> GetBlanketPurchaseOrderAsync(string id)
        {
            // Check the permission code to view a blanket purchase order.
            CheckViewBlanketPurchaseOrderPermission();

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

            // Get the blanket purchase order domain entity from the repository
            var blanketPurchaseOrderDomainEntity = await blanketPurchaseOrderRepository.GetBlanketPurchaseOrderAsync(id, CurrentUser.PersonId, generalLedgerUser.GlAccessLevel, generalLedgerUser.AllAccounts);

            if (blanketPurchaseOrderDomainEntity == null)
            {
                throw new ArgumentNullException("blanketPurchaseOrderDomainEntity", "blanketPurchaseOrderDomainEntity cannot be null.");
            }

            // Convert the blanket purchase order and all its child objects into DTOs
            var blanketPurchaseOrderDtoAdapter = new BlanketPurchaseOrderEntityToDtoAdapter(_adapterRegistry, logger);
            var bpoDto = blanketPurchaseOrderDtoAdapter.MapToType(blanketPurchaseOrderDomainEntity, glConfiguration.MajorComponentStartPositions);

            if (bpoDto.GlDistributions == null || bpoDto.GlDistributions.Count < 1)
            {
                throw new PermissionsException("Insufficient permission to access blanket purchase order.");
            }

            return bpoDto;
        }

        /// <summary>
        /// Permission code that allows a READ operation on a blanket purchase order.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewBlanketPurchaseOrderPermission()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewBlanketPurchaseOrder);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view blanket purchase orders.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }
}

