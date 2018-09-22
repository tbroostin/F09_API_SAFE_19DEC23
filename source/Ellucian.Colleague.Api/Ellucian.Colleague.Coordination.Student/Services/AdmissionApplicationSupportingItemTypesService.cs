//Copyright 2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Student.Entities;
using Ellucian.Colleague.Domain.Student.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Web.Adapters;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Security;


namespace Ellucian.Colleague.Coordination.Student.Services
{
    [RegisterType]
    public class AdmissionApplicationSupportingItemTypesService : BaseCoordinationService, IAdmissionApplicationSupportingItemTypesService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;        
        private readonly ILogger _logger;

        public AdmissionApplicationSupportingItemTypesService(
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            IReferenceDataRepository referenceDataRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _configurationRepository = configurationRepository;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Gets all admission-application-supporting-item-types
        /// </summary>
        /// <returns>Collection of AdmissionApplicationSupportingItemTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes>> GetAdmissionApplicationSupportingItemTypesAsync(bool bypassCache = false)
        {
            var admissionApplicationSupportingItemTypesCollection = new List<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes>();

            var admissionApplicationSupportingItemTypesEntities = await _referenceDataRepository.GetAdmissionApplicationSupportingItemTypesAsync(bypassCache);
            if (admissionApplicationSupportingItemTypesEntities != null && admissionApplicationSupportingItemTypesEntities.Any())
            {
                foreach (var admissionApplicationSupportingItemTypes in admissionApplicationSupportingItemTypesEntities)
                {
                    admissionApplicationSupportingItemTypesCollection.Add(ConvertCommunicationCodeEntityToDto(admissionApplicationSupportingItemTypes));
                }
            }
            return admissionApplicationSupportingItemTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN ELLUCIAN DATA MODEL</remarks>
        /// <summary>
        /// Get a AdmissionApplicationSupportingItemTypes from its GUID
        /// </summary>
        /// <returns>AdmissionApplicationSupportingItemTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes> GetAdmissionApplicationSupportingItemTypesByGuidAsync(string guid)
        {
            try
            {
                return ConvertCommunicationCodeEntityToDto((await _referenceDataRepository.GetAdmissionApplicationSupportingItemTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("admission-application-supporting-item-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("admission-application-supporting-item-types not found for GUID " + guid, ex);
            }
        }

        /// <summary>
        /// Converts a SupportingItemTypes domain entity to its corresponding AdmissionApplicationSupportingItemTypes DTO
        /// </summary>
        /// <param name="source">SupportingItemTypes domain entity</param>
        /// <returns>AdmissionApplicationSupportingItemTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes ConvertCommunicationCodeEntityToDto(CommunicationCode source)
        {
            var admissionApplicationSupportingItemTypes = new Ellucian.Colleague.Dtos.AdmissionApplicationSupportingItemTypes();

            admissionApplicationSupportingItemTypes.Id = source.Guid;
            admissionApplicationSupportingItemTypes.Code = source.Code;
            admissionApplicationSupportingItemTypes.Title = source.Description;
            admissionApplicationSupportingItemTypes.Description = null;
            return admissionApplicationSupportingItemTypes;
        }
    }
}