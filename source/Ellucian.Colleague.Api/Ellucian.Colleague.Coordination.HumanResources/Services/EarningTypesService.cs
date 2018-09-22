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
    public class EarningTypesService : BaseCoordinationService, IEarningTypesService
    {

        private readonly IHumanResourcesReferenceDataRepository _referenceDataRepository;

        public EarningTypesService(

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
        /// Gets all earning-types
        /// </summary>
        /// <returns>Collection of EarningTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EarningTypes>> GetEarningTypesAsync(bool bypassCache = false)
        {
            var earningTypesCollection = new List<Ellucian.Colleague.Dtos.EarningTypes>();

            var earningTypesEntities = await _referenceDataRepository.GetEarningTypesAsync(bypassCache);
            if (earningTypesEntities != null && earningTypesEntities.Any())
            {
                foreach (var earningTypes in earningTypesEntities)
                {
                    earningTypesCollection.Add(ConvertEarningTypesEntityToDto(earningTypes));
                }
            }
            return earningTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EarningTypes from its GUID
        /// </summary>
        /// <returns>EarningTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EarningTypes> GetEarningTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertEarningTypesEntityToDto((await _referenceDataRepository.GetEarningTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("earning-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("earning-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Earntype domain entity to its corresponding EarningTypes DTO
        /// </summary>
        /// <param name="source">Earntype domain entity</param>
        /// <returns>EarningTypes DTO</returns>
        private Ellucian.Colleague.Dtos.EarningTypes ConvertEarningTypesEntityToDto(EarningType2 source)
        {
            var earningTypes = new Ellucian.Colleague.Dtos.EarningTypes();

            earningTypes.Id = source.Guid;
            earningTypes.Code = source.Code;
            earningTypes.Title = source.Description;
            earningTypes.Description = null;           
                                                                        
            return earningTypes;
        }

      
    }
   
}