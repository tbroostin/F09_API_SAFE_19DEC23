//Copyright 2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;


namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class EmergencyContactTypesService : BaseCoordinationService, IEmergencyContactTypesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public EmergencyContactTypesService(

            IReferenceDataRepository referenceDataRepository,
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
        /// Gets all emergency-contact-types
        /// </summary>
        /// <returns>Collection of EmergencyContactTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmergencyContactTypes>> GetEmergencyContactTypesAsync(bool bypassCache = false)
        {
            var emergencyContactTypesCollection = new List<Ellucian.Colleague.Dtos.EmergencyContactTypes>();

            var emergencyContactTypesEntities = await _referenceDataRepository.GetIntgPersonEmerTypesAsync(bypassCache);
            if (emergencyContactTypesEntities != null && emergencyContactTypesEntities.Any())
            {
                foreach (var emergencyContactTypes in emergencyContactTypesEntities)
                {
                    emergencyContactTypesCollection.Add(ConvertEmergencyContactTypesEntityToDto(emergencyContactTypes));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return emergencyContactTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmergencyContactTypes from its GUID
        /// </summary>
        /// <returns>EmergencyContactTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmergencyContactTypes> GetEmergencyContactTypesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertEmergencyContactTypesEntityToDto((await _referenceDataRepository.GetIntgPersonEmerTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No emergency-contact-types was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No emergency-contact-types was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgPersonEmerTypes domain entity to its corresponding EmergencyContactTypes DTO
        /// </summary>
        /// <param name="source">IntgPersonEmerTypes domain entity</param>
        /// <returns>EmergencyContactTypes DTO</returns>
        private Ellucian.Colleague.Dtos.EmergencyContactTypes ConvertEmergencyContactTypesEntityToDto(IntgPersonEmerTypes source)
        {
            var emergencyContactTypes = new Ellucian.Colleague.Dtos.EmergencyContactTypes();

            emergencyContactTypes.Id = source.Guid;
            emergencyContactTypes.Code = source.Code;
            emergencyContactTypes.Title = source.Description;
            emergencyContactTypes.Description = null;

            return emergencyContactTypes;
        }
    }
}
