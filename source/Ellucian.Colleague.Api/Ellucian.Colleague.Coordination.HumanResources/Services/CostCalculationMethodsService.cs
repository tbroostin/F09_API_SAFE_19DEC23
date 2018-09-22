//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.HumanResources.Adapters;
using Ellucian.Colleague.Domain.HumanResources.Entities;
using Ellucian.Colleague.Domain.HumanResources.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.HumanResources.Services
{
    [RegisterType]
    public class CostCalculationMethodsService : BaseCoordinationService, ICostCalculationMethodsService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public CostCalculationMethodsService(

            IHumanResourcesReferenceDataRepository referenceDataRepository,
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
        /// Gets all cost-calculation-methods
        /// </summary>
        /// <returns>Collection of CostCalculationMethods DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CostCalculationMethods>> GetCostCalculationMethodsAsync(bool bypassCache = false)
        {
            var costCalculationMethodsCollection = new List<Ellucian.Colleague.Dtos.CostCalculationMethods>();

            var costCalculationMethodsEntities = await _referenceDataRepository.GetCostCalculationMethodsAsync(bypassCache);
            if (costCalculationMethodsEntities != null && costCalculationMethodsEntities.Any())
            {
                foreach (var costCalculationMethods in costCalculationMethodsEntities)
                {
                    costCalculationMethodsCollection.Add(ConvertCostCalculationMethodsEntityToDto(costCalculationMethods));
                }
            }
            return costCalculationMethodsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a CostCalculationMethods from its GUID
        /// </summary>
        /// <returns>CostCalculationMethods DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CostCalculationMethods> GetCostCalculationMethodsByGuidAsync(string guid)
        {
            try
            {
                return ConvertCostCalculationMethodsEntityToDto((await _referenceDataRepository.GetCostCalculationMethodsAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("cost-calculation-methods not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("cost-calculation-methods not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a BdCalcMethods domain entity to its corresponding CostCalculationMethods DTO
        /// </summary>
        /// <param name="source">BdCalcMethods domain entity</param>
        /// <returns>CostCalculationMethods DTO</returns>
        private Ellucian.Colleague.Dtos.CostCalculationMethods ConvertCostCalculationMethodsEntityToDto(CostCalculationMethod source)
        {
            var costCalculationMethods = new Ellucian.Colleague.Dtos.CostCalculationMethods();

            costCalculationMethods.Id = source.Guid;
            costCalculationMethods.Code = source.Code;
            costCalculationMethods.Title = source.Description;
            costCalculationMethods.Description = null;           
                                                                        
            return costCalculationMethods;
        }

      
    }
  
}