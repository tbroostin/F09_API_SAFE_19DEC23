// Copyright 2017-2018 Ellucian Company L.P. and its affiliates.

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
    public class PersonalPronounTypeService : BaseCoordinationService, IPersonalPronounTypeService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;

        public PersonalPronounTypeService(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository,                                     
                                         ICurrentUserFactory currentUserFactory,
                                         IRoleRepository roleRepository,
                                         IConfigurationRepository configurationRepository,
                                         ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
        }

        public async Task<IEnumerable<Dtos.Base.PersonalPronounType>> GetBasePersonalPronounTypesAsync(bool ignoreCache=false)
        {
            var personalPronounTypesEntityCollection = await _referenceDataRepository.GetPersonalPronounTypesAsync(ignoreCache);
            var adapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonalPronounType, Dtos.Base.PersonalPronounType>();
            var personalPronounTypesDtoCollection = new List<Dtos.Base.PersonalPronounType>();
            foreach (var personalPronounTypesEntity in personalPronounTypesEntityCollection)
            {
                var personalPronounTypesDto = adapter.MapToType(personalPronounTypesEntity);
                personalPronounTypesDtoCollection.Add(personalPronounTypesDto);
            }
            return personalPronounTypesDtoCollection;
        }


        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all personal-pronouns
        /// </summary>
        /// <returns>Collection of PersonalPronouns DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonalPronouns>> GetPersonalPronounsAsync(bool bypassCache = false)
        {
            var personalPronounsCollection = new List<Ellucian.Colleague.Dtos.PersonalPronouns>();

            var personalPronounsEntities = await _referenceDataRepository.GetPersonalPronounTypesAsync(bypassCache);
            if (personalPronounsEntities != null && personalPronounsEntities.Any())
            {
                foreach (var personalPronouns in personalPronounsEntities)
                {
                    personalPronounsCollection.Add(ConvertPersonalPronounsEntityToDto(personalPronouns));
                }
            }
            return personalPronounsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a PersonalPronouns from its GUID
        /// </summary>
        /// <returns>PersonalPronouns DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonalPronouns> GetPersonalPronounsByGuidAsync(string guid, bool bypassCache = true)
        {
            try
            {
                return ConvertPersonalPronounsEntityToDto((await _referenceDataRepository.GetPersonalPronounTypesAsync(bypassCache)).Where(r => r.Guid == guid).First());
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("personal-pronouns not found for GUID " + guid, ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("personal-pronouns not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PersonalPronouns domain entity to its corresponding PersonalPronouns DTO
        /// </summary>
        /// <param name="source">PersonalPronouns domain entity</param>
        /// <returns>PersonalPronouns DTO</returns>
        private Ellucian.Colleague.Dtos.PersonalPronouns ConvertPersonalPronounsEntityToDto(PersonalPronounType source)
        {
            var personalPronouns = new Ellucian.Colleague.Dtos.PersonalPronouns();

            personalPronouns.Id = source.Guid;
            personalPronouns.Code = source.Code;
            personalPronouns.Title = source.Description;
            personalPronouns.Description = null;

            return personalPronouns;
        }
    }
}
