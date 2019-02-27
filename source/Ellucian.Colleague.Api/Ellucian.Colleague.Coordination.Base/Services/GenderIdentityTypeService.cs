// Copyright 2018 Ellucian Company L.P. and its affiliates.

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
    public class GenderIdentityTypeService : BaseCoordinationService, IGenderIdentityTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;

        public GenderIdentityTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         IConfigurationRepository configurationRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        public async Task<IEnumerable<Dtos.Base.GenderIdentityType>> GetBaseGenderIdentityTypesAsync(bool ignoreCache = false)
        {
            try
            {
                var genderIdentityTypesEntityCollection = await _referenceDataRepository.GetGenderIdentityTypesAsync(ignoreCache);
                var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.GenderIdentityType, Dtos.Base.GenderIdentityType>();
                var genderIdentityTypesDtoCollection = new List<Dtos.Base.GenderIdentityType>();
                foreach (var genderIdentityTypesEntity in genderIdentityTypesEntityCollection)
                {
                    var genderIdentityTypesDto = adapter.MapToType(genderIdentityTypesEntity);
                    genderIdentityTypesDtoCollection.Add(genderIdentityTypesDto);
                }
                return genderIdentityTypesDtoCollection;
            }
            catch (Exception ex)
            {
                var message = string.Format("Exception occurred while trying to retrieve gendery identities ", ex.Message);
                logger.Info(ex, message);
                throw;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all gender-identities
        /// </summary>
        /// <returns>Collection of GenderIdentities DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.GenderIdentities>> GetGenderIdentitiesAsync(bool bypassCache = false)
        {
            var genderIdentitiesCollection = new List<Ellucian.Colleague.Dtos.GenderIdentities>();

            var genderIdentitiesEntities = await _referenceDataRepository.GetGenderIdentityTypesAsync(bypassCache);
            if (genderIdentitiesEntities != null && genderIdentitiesEntities.Any())
            {
                foreach (var genderIdentities in genderIdentitiesEntities)
                {
                    genderIdentitiesCollection.Add(ConvertGenderIdentitiesEntityToDto(genderIdentities));
                }
            }
            return genderIdentitiesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a GenderIdentities from its GUID
        /// </summary>
        /// <returns>GenderIdentities DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.GenderIdentities> GetGenderIdentitiesByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertGenderIdentitiesEntityToDto((await _referenceDataRepository.GetGenderIdentityTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("gender-identities not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("gender-identities not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a GenderIdentities domain entity to its corresponding GenderIdentities DTO
        /// </summary>
        /// <param name="source">GenderIdentities domain entity</param>
        /// <returns>GenderIdentities DTO</returns>
        private Ellucian.Colleague.Dtos.GenderIdentities ConvertGenderIdentitiesEntityToDto(GenderIdentityType source)
        {
            var genderIdentities = new Ellucian.Colleague.Dtos.GenderIdentities();

            genderIdentities.Id = source.Guid;
            genderIdentities.Code = source.Code;
            genderIdentities.Title = source.Description;
            genderIdentities.Description = null;

            return genderIdentities;
        }
    }
}