//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Coordination.Student.Adapters;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base.Repositories;

namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class HousingResidentTypesService : StudentCoordinationService, IHousingResidentTypesService
    {

        private readonly IStudentReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;

        public HousingResidentTypesService(

            IStudentReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger,
            IConfigurationRepository configurationRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, null, configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all housing-resident-types
        /// </summary>
        /// <returns>Collection of HousingResidentTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.HousingResidentTypes>> GetHousingResidentTypesAsync(bool bypassCache = false)
        {
            var housingResidentTypesCollection = new List<Ellucian.Colleague.Dtos.HousingResidentTypes>();

            var housingResidentTypesEntities = await _referenceDataRepository.GetHousingResidentTypesAsync(bypassCache);
            if (housingResidentTypesEntities != null && housingResidentTypesEntities.Any())
            {
                foreach (var housingResidentTypes in housingResidentTypesEntities)
                {
                    housingResidentTypesCollection.Add(ConvertHousingResidentTypesEntityToDto(housingResidentTypes));
                }
            }
            return housingResidentTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a HousingResidentTypes from its GUID
        /// </summary>
        /// <returns>HousingResidentTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.HousingResidentTypes> GetHousingResidentTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertHousingResidentTypesEntityToDto((await _referenceDataRepository.GetHousingResidentTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("housing-resident-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("housing-resident-types not found for GUID " + guid, ex);
            }
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a HousingResidentTypes domain entity to its corresponding HousingResidentTypes DTO
        /// </summary>
        /// <param name="source">HousingResidentTypes domain entity</param>
        /// <returns>HousingResidentTypes DTO</returns>
        private Ellucian.Colleague.Dtos.HousingResidentTypes ConvertHousingResidentTypesEntityToDto(HousingResidentType source)
        {
            var housingResidentTypes = new Ellucian.Colleague.Dtos.HousingResidentTypes();

            housingResidentTypes.Id = source.Guid;
            housingResidentTypes.Code = source.Code;
            housingResidentTypes.Title = source.Description;
            housingResidentTypes.Description = null;           
                                                                        
            return housingResidentTypes;
        }

   }
}