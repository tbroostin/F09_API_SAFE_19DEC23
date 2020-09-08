//Copyright 2020 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.ColleagueFinance.Entities;
using Ellucian.Colleague.Domain.ColleagueFinance.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.ColleagueFinance.Services
{
    [RegisterType]
    public class VendorAddressUsagesService : BaseCoordinationService, IVendorAddressUsagesService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public VendorAddressUsagesService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all vendor-address-usages
        /// </summary>
        /// <returns>Collection of VendorAddressUsages DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.VendorAddressUsages>> GetVendorAddressUsagesAsync(bool bypassCache = false)
        {
            var vendorAddressUsagesCollection = new List<Ellucian.Colleague.Dtos.VendorAddressUsages>();

            var vendorAddressUsagesEntities = await _referenceDataRepository.GetIntgVendorAddressUsagesAsync(bypassCache);
            if (vendorAddressUsagesEntities != null && vendorAddressUsagesEntities.Any())
            {
                foreach (var vendorAddressUsages in vendorAddressUsagesEntities)
                {
                    vendorAddressUsagesCollection.Add(ConvertVendorAddressUsagesEntityToDto(vendorAddressUsages));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return vendorAddressUsagesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a VendorAddressUsages from its GUID
        /// </summary>
        /// <returns>VendorAddressUsages DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.VendorAddressUsages> GetVendorAddressUsagesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertVendorAddressUsagesEntityToDto((await _referenceDataRepository.GetIntgVendorAddressUsagesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No vendor-address-usages was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No vendor-address-usages was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgVendorAddressUsages domain entity to its corresponding VendorAddressUsages DTO
        /// </summary>
        /// <param name="source">IntgVendorAddressUsages domain entity</param>
        /// <returns>VendorAddressUsages DTO</returns>
        private Ellucian.Colleague.Dtos.VendorAddressUsages ConvertVendorAddressUsagesEntityToDto(IntgVendorAddressUsages source)
        {
            var vendorAddressUsages = new Ellucian.Colleague.Dtos.VendorAddressUsages();

            vendorAddressUsages.Id = source.Guid;
            //vendorAddressUsages.Code = source.Code;
            vendorAddressUsages.Title = source.Description;
            vendorAddressUsages.Description = null;

            return vendorAddressUsages;
        }


    }
}