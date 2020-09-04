// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Generic;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.ColleagueFinance;
using System;
using Ellucian.Colleague.Domain.ColleagueFinance;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    /// <summary>
    /// This class implements the IVendorCommoditiesService interface
    /// </summary>
    [RegisterType]
    public class VendorCommodityService : BaseCoordinationService, IVendorCommodityService
    {
        private IVendorCommodityRepository vendorCommoditiesRepository;
        private IColleagueFinanceWebConfigurationsRepository cfWebConfigurationsRepository;


        // This constructor initializes the private attributes
        public VendorCommodityService(
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IVendorCommodityRepository vendorCommoditiesRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            this.vendorCommoditiesRepository = vendorCommoditiesRepository;
        }

        /// <summary>
        /// Retrieves vendor commodity code DTO.
        /// </summary>
        /// <param name="vendorId">vendor id.</param>        
        /// <param name="commodityCode">Commodity code.</param>
        /// <returns>VendorCommodity Dto.</returns>
        public async Task<VendorCommodity> GetVendorCommodityAsync(string vendorId, string commodityCode)
        {            
            if (string.IsNullOrEmpty(vendorId) && string.IsNullOrEmpty(commodityCode))
            {
                throw new ArgumentNullException("vendorId and commodityCode must be specified.");
            }

            // Check the permission code to view vendor information.
            CheckViewVendorPermissions();

            var vendorCommoditiesEntity = await vendorCommoditiesRepository.GetVendorCommodityAsync(vendorId, commodityCode);
            // Convert the vendor commodity entity to DTO
            var dtoAdapter = _adapterRegistry.GetAdapter<Domain.ColleagueFinance.Entities.VendorCommodity, Dtos.ColleagueFinance.VendorCommodity>();

            VendorCommodity vendorCommodityDto = new VendorCommodity();
            if (vendorCommoditiesEntity != null)
            {
                vendorCommodityDto = dtoAdapter.MapToType(vendorCommoditiesEntity);
            }
            return vendorCommodityDto;
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view vendor information.
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private void CheckViewVendorPermissions()
        {
            var hasPermission = HasPermission(ColleagueFinancePermissionCodes.ViewVendor)
                || HasPermission(ColleagueFinancePermissionCodes.CreateUpdateRequisition)
                || HasPermission(ColleagueFinancePermissionCodes.CreateUpdatePurchaseOrder);

            if (!hasPermission)
            {
                var message = string.Format("{0} does not have permission to view vendor information.", CurrentUser.PersonId);
                logger.Error(message);
                throw new PermissionsException(message);
            }
        }
    }
}
