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
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    [RegisterType]
    public class LanguagesService : BaseCoordinationService, ILanguagesService
    {

        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly ILanguageRepository _languageRepository;

        public LanguagesService(

            ILanguageRepository languageRepository,
            IReferenceDataRepository referenceDataRepository,
            IAdapterRegistry adapterRegistry,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IConfigurationRepository configurationRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _languageRepository = languageRepository;
            _referenceDataRepository = referenceDataRepository;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all languages
        /// </summary>
        /// <returns>Collection of Languages DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Languages>> GetLanguagesAsync(bool bypassCache = false)
        {
            var languagesCollection = new List<Ellucian.Colleague.Dtos.Languages>();

            var languagesEntities = await _languageRepository.GetLanguagesAsync(bypassCache);
            if (languagesEntities != null && languagesEntities.Any())
            {
                foreach (var languages in languagesEntities)
                {
                    languagesCollection.Add(ConvertLanguagesEntityToDto(languages));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return languagesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a Languages from its GUID
        /// </summary>
        /// <returns>Languages DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Languages> GetLanguagesByGuidAsync(string guid, bool bypassCache = true)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new KeyNotFoundException("GUID required to get a language");
            }

            Language2 languageEntity = null;
            try
            {
                languageEntity = await _languageRepository.GetLanguageByGuidAsync(guid, bypassCache);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            if (languageEntity == null)
            {
                throw new KeyNotFoundException("No language was found for guid " + guid);
            }

            return ConvertLanguagesEntityToDto(languageEntity);
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Languages domain entity to its corresponding Languages DTO
        /// </summary>
        /// <param name="source">Languages domain entity</param>
        /// <returns>Languages DTO</returns>
        private Ellucian.Colleague.Dtos.Languages ConvertLanguagesEntityToDto(Ellucian.Colleague.Domain.Base.Entities.Language2 source)
        {
            var languages = new Ellucian.Colleague.Dtos.Languages();

            languages.Id = source.Guid;
            languages.Code = source.Code;
            languages.Title = source.Description;
            if (!string.IsNullOrEmpty(source.IsoCode))
            {
                languages.ISOCode = source.IsoCode;
            }
            return languages;
        }

        /// <summary>
        /// Update a language.
        /// </summary>
        /// <param name="language">The <see cref="language">language</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="languages">languages</see></returns>
        public async Task<Dtos.Languages> UpdateLanguageAsync(Languages language, bool bypassCache = true)
        {
            await CheckUserUpdateLanguageAsync();

            if (language == null)
                throw new ArgumentNullException("language", "Must provide a language for update");

            if (string.IsNullOrEmpty(language.Id))
                throw new ArgumentNullException("language", "Must provide a guid for language update");

            // get the ID associated with the incoming guid
            var languageDto = await GetLanguagesByGuidAsync(language.Id);
            if (languageDto == null || string.IsNullOrEmpty(languageDto.Code))
            {
                throw new ArgumentException(string.Format("Invalid GUID '{0}' provided for update to Languages.", language.Id));
            }

            var languageEntity = new Language2(language.Id, language.Code, language.Title)
            {
                IsoCode = string.IsNullOrEmpty(language.ISOCode) ?
                "" : language.ISOCode.ToUpper()
            };
            try
            {

                var languageUpdatedEntity = await _languageRepository.UpdateLanguageAsync(languageEntity);

                if (languageUpdatedEntity != null)
                {
                    return ConvertLanguagesEntityToDto(languageUpdatedEntity);
                }
                else
                {
                    throw new ArgumentException(string.Format("Unable to build language object from '{0}'.", language.Id));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //return await GetLanguagesByGuidAsync(language.Id, true);
        }

        /// <summary>
        /// Provides an integration user permission to update ISO code on Languages
        /// </summary>
        private async Task CheckUserUpdateLanguageAsync()
        {
            var userPermissionList = (await GetUserPermissionCodesAsync()).ToList();

            // access is ok if the current user has the update any address
            if (!userPermissionList.Contains(BasePermissionCodes.UpdateLanguage))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update languages.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to update a language.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Gets all language-iso-codes
        /// </summary>
        /// <returns>Collection of LanguageIsoCodes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.LanguageIsoCodes>> GetLanguageIsoCodesAsync(bool bypassCache = false)
        {
            var languageIsoCodesCollection = new List<Ellucian.Colleague.Dtos.LanguageIsoCodes>();

            var languageIsoCodesEntities = await _referenceDataRepository.GetLanguageIsoCodesAsync(bypassCache);
            if (languageIsoCodesEntities != null && languageIsoCodesEntities.Any())
            {
                foreach (var languageIsoCodes in languageIsoCodesEntities)
                {
                    languageIsoCodesCollection.Add(ConvertLanguageIsoCodesEntityToDto(languageIsoCodes));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
            }
            return languageIsoCodesCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Get a LanguageIsoCodes from its GUID
        /// </summary>
        /// <returns>LanguageIsoCodes DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.LanguageIsoCodes> GetLanguageIsoCodeByGuidAsync(string guid)
        {

            try
            {
                return ConvertLanguageIsoCodesEntityToDto(await _referenceDataRepository.GetLanguageIsoCodeByGuidAsync(guid));
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException(string.Format("No language ISO code was found for guid '{0}'", guid), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException(string.Format("No language ISO code was found for guid '{0}'", guid), ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a Languages domain entity to its corresponding LanguageIsoCodes DTO
        /// </summary>
        /// <param name="source">Languages domain entity</param>
        /// <returns>LanguageIsoCodes DTO</returns>
        private Ellucian.Colleague.Dtos.LanguageIsoCodes ConvertLanguageIsoCodesEntityToDto(Domain.Base.Entities.LanguageIsoCodes source)
        {
            var languageIsoCodes = new Ellucian.Colleague.Dtos.LanguageIsoCodes();

            if (source == null)
            {
                throw new KeyNotFoundException("source is required to convert a languageIsoCode entity to DTO.");
            }
            languageIsoCodes.Id = source.Guid;
            languageIsoCodes.IsoCode = source.Code;
            languageIsoCodes.Title = source.Description;
            languageIsoCodes.Status = (!string.IsNullOrEmpty(source.InactiveFlag) && source.InactiveFlag.Equals("Y", StringComparison.OrdinalIgnoreCase))
                ? Status.Inactive : Status.Active;

            return languageIsoCodes;
        }


    }
}
