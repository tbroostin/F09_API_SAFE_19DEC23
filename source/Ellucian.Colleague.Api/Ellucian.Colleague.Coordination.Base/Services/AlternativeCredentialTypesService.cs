//Copyright 2018 Ellucian Company L.P. and its affiliates.

using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class AlternativeCredentialTypesService : BaseCoordinationService, IAlternativeCredentialTypesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;

        public AlternativeCredentialTypesService(

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
        /// Gets all alternative-credential-types
        /// </summary>
        /// <returns>Collection of AlternativeCredentialTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.AlternativeCredentialTypes>> GetAlternativeCredentialTypesAsync(bool bypassCache = false)
        {
            var alternativeCredentialTypesCollection = new List<Ellucian.Colleague.Dtos.AlternativeCredentialTypes>();

            var alternativeCredentialTypesEntities = await _referenceDataRepository.GetAlternateIdTypesAsync(bypassCache);
            if (alternativeCredentialTypesEntities != null && alternativeCredentialTypesEntities.Any())
            {
                foreach (var alternativeCredentialTypes in alternativeCredentialTypesEntities)
                {
                    alternativeCredentialTypesCollection.Add(ConvertAlternativeCredentialTypesEntityToDto(alternativeCredentialTypes));
                }
            }
            return alternativeCredentialTypesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a AlternativeCredentialTypes from its GUID
        /// </summary>
        /// <returns>AlternativeCredentialTypes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.AlternativeCredentialTypes> GetAlternativeCredentialTypesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertAlternativeCredentialTypesEntityToDto((await _referenceDataRepository.GetAlternateIdTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("alternative-credential-types not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("alternative-credential-types not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a AltIdTypes domain entity to its corresponding AlternativeCredentialTypes DTO
        /// </summary>
        /// <param name="source">AltIdTypes domain entity</param>
        /// <returns>AlternativeCredentialTypes DTO</returns>
        private Ellucian.Colleague.Dtos.AlternativeCredentialTypes ConvertAlternativeCredentialTypesEntityToDto(AltIdTypes source)
        {
            var alternativeCredentialTypes = new Ellucian.Colleague.Dtos.AlternativeCredentialTypes();

            alternativeCredentialTypes.Id = source.Guid;
            alternativeCredentialTypes.Code = source.Code;
            alternativeCredentialTypes.Title = source.Description;
            alternativeCredentialTypes.Description = null;

            return alternativeCredentialTypes;
        }
    }
}