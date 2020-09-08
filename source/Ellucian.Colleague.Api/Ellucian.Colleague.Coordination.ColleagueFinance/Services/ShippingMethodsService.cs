//Copyright 2017 Ellucian Company L.P. and its affiliates.

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
    public class ShippingMethodsService : BaseCoordinationService, IShippingMethodsService
    {

        private readonly IColleagueFinanceReferenceDataRepository _referenceDataRepository;

        public ShippingMethodsService(

            IColleagueFinanceReferenceDataRepository referenceDataRepository,
            IConfigurationRepository configurationRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {

            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all free-on-board-types
        /// </summary>
        /// <returns>Collection of ShippingMethods DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.ShippingMethods>> GetShippingMethodsAsync(bool bypassCache = false)
        {
            var shippingMethodsCollection = new List<Ellucian.Colleague.Dtos.ShippingMethods>();

            var shippingMethodsEntities = await _referenceDataRepository.GetShippingMethodsAsync(bypassCache);
            if (shippingMethodsEntities != null && shippingMethodsEntities.Any())
            {
                foreach (var shippingMethods in shippingMethodsEntities)
                {
                    shippingMethodsCollection.Add(ConvertShippingMethodsEntityToDto(shippingMethods));
                }
            }
            return shippingMethodsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a ShippingMethods from its GUID
        /// </summary>
        /// <returns>ShippingMethods DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.ShippingMethods> GetShippingMethodsByGuidAsync(string guid)
        {
            try
            {
                return ConvertShippingMethodsEntityToDto((await _referenceDataRepository.GetShippingMethodsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("shipping-methods not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("shipping-methods not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Gets all Ship Via Codes with descriptions
        /// </summary>
        /// <returns>Collection of ShipViaCodes</returns>
        public async Task<IEnumerable<Dtos.ColleagueFinance.ShipViaCode>> GetShipViaCodesAsync()
        {
            var shipViaCodeCollection = new List<Dtos.ColleagueFinance.ShipViaCode>();

            var shipViaCodesEntities = await _referenceDataRepository.GetShipViaCodesAsync();
            if (shipViaCodesEntities != null && shipViaCodesEntities.Any())
            {
                //sort the entities on code, then by description
                shipViaCodesEntities = shipViaCodesEntities.OrderBy(x => x.Code);
                if (shipViaCodesEntities != null && shipViaCodesEntities.Any())
                {
                    foreach (var shipViaCodeEntity in shipViaCodesEntities)
                    {
                        //convert shipViaCode entity to dto
                        shipViaCodeCollection.Add(ConvertShipViaCodeEntityToDto(shipViaCodeEntity));
                    }
                }
            }
            return shipViaCodeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a ShippingMethods domain entity to its corresponding ShippingMethods DTO
        /// </summary>
        /// <param name="source">ShippingMethods domain entity</param>
        /// <returns>ShippingMethods DTO</returns>
        private Ellucian.Colleague.Dtos.ShippingMethods ConvertShippingMethodsEntityToDto(ShippingMethod source)
        {
            var shippingMethods = new Ellucian.Colleague.Dtos.ShippingMethods();

            shippingMethods.Id = source.Guid;
            shippingMethods.Code = source.Code;
            shippingMethods.Title = source.Description;
            shippingMethods.Description = null;

            return shippingMethods;
        }

        private Ellucian.Colleague.Dtos.ColleagueFinance.ShipViaCode ConvertShipViaCodeEntityToDto(Ellucian.Colleague.Domain.ColleagueFinance.Entities.ShipViaCode source)
        {
            var shipViaCode = new Ellucian.Colleague.Dtos.ColleagueFinance.ShipViaCode();
            shipViaCode.Code = source.Code;
            shipViaCode.Description = source.Description;
            return shipViaCode;
        }
        
    }

}