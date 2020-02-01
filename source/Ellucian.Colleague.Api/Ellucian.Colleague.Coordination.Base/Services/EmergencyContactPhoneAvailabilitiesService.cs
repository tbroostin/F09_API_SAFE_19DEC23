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
    public class EmergencyContactPhoneAvailabilitiesService : BaseCoordinationService, IEmergencyContactPhoneAvailabilitiesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public EmergencyContactPhoneAvailabilitiesService(

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
        /// Gets all emergency-contact-phone-availabilities
        /// </summary>
        /// <returns>Collection of EmergencyContactPhoneAvailabilities DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities>> GetEmergencyContactPhoneAvailabilitiesAsync(bool bypassCache = false)
        {
            var emergencyContactPhoneAvailabilitiesCollection = new List<Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities>();

            var emergencyContactPhoneAvailabilitiesEntities = await _referenceDataRepository.GetIntgPersonEmerPhoneTypesAsync(bypassCache);
            if (emergencyContactPhoneAvailabilitiesEntities != null && emergencyContactPhoneAvailabilitiesEntities.Any())
            {
                foreach (var emergencyContactPhoneAvailabilities in emergencyContactPhoneAvailabilitiesEntities)
                {
                    emergencyContactPhoneAvailabilitiesCollection.Add(ConvertEmergencyContactPhoneAvailabilitiesEntityToDto(emergencyContactPhoneAvailabilities));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return emergencyContactPhoneAvailabilitiesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a EmergencyContactPhoneAvailabilities from its GUID
        /// </summary>
        /// <returns>EmergencyContactPhoneAvailabilities DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities> GetEmergencyContactPhoneAvailabilitiesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertEmergencyContactPhoneAvailabilitiesEntityToDto((await _referenceDataRepository.GetIntgPersonEmerPhoneTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No emergency-contact-phone-availabilities was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No emergency-contact-phone-availabilities was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a IntgPersonEmerPhoneTypes domain entity to its corresponding EmergencyContactPhoneAvailabilities DTO
        /// </summary>
        /// <param name="source">IntgPersonEmerPhoneTypes domain entity</param>
        /// <returns>EmergencyContactPhoneAvailabilities DTO</returns>
        private Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities ConvertEmergencyContactPhoneAvailabilitiesEntityToDto(IntgPersonEmerPhoneTypes source)
        {
            var emergencyContactPhoneAvailabilities = new Ellucian.Colleague.Dtos.EmergencyContactPhoneAvailabilities();

            emergencyContactPhoneAvailabilities.Id = source.Guid;
            emergencyContactPhoneAvailabilities.Code = source.Code;
            emergencyContactPhoneAvailabilities.Title = source.Description;
            emergencyContactPhoneAvailabilities.Description = null;

            return emergencyContactPhoneAvailabilities;
        }
    }
}
