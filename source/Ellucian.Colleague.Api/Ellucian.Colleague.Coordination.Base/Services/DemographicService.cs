// Copyright 2015-2017 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Web.Adapters;
using Ellucian.Web.Dependency;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Dtos.EnumProperties;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Representation of an demographic service
    /// </summary>
    [RegisterType]
    public class DemographicService : BaseCoordinationService, IDemographicService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly ILogger repoLogger;
        private const string _dataOrigin = "Colleague";

        /// <summary>
        /// Initializes a new instance of the <see cref="DemographicService"/> class.
        /// </summary>
        /// <param name="referenceDataRepository">The reference data repository.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="configurationRepository">The configuration repository.</param>
        /// <param name="staffRepository">The staff repository</param>
        public DemographicService(IReferenceDataRepository referenceDataRepository, IAdapterRegistry adapterRegistry, ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository, ILogger logger, IConfigurationRepository configurationRepository, IStaffRepository staffRepository)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, staffRepository, configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _configurationRepository = configurationRepository;

            this.repoLogger = logger;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Gets all citizenship statuses
        /// </summary>
        /// <returns>Collection of CitizenshipStatus DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.CitizenshipStatus>> GetCitizenshipStatusesAsync(bool bypassCache = false)
        {
            var citizenshipStatusCollection = new List<Ellucian.Colleague.Dtos.CitizenshipStatus>();

            var citizenshipStatusEntities = await _referenceDataRepository.GetCitizenshipStatusesAsync(bypassCache);
            if (citizenshipStatusEntities != null && citizenshipStatusEntities.Count() > 0)
            {
                foreach (var citizenshipStatus in citizenshipStatusEntities)
                {
                    citizenshipStatusCollection.Add(ConvertCitizenshipStatusEntityToDto(citizenshipStatus));
                }
            }
            return citizenshipStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Get a citizenship status from its GUID
        /// </summary>
        /// <returns>CitizenshipStatus DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.CitizenshipStatus> GetCitizenshipStatusByGuidAsync(string guid)
        {
            try
            {
                return ConvertCitizenshipStatusEntityToDto((await _referenceDataRepository.GetCitizenshipStatusesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Citizenship status not found for GUID " + guid, ex);
            }
        }

        /// <remarks>For use with Ellucian HeDM Version 4</remarks>
        /// <summary>
        /// Gets all ethnicities
        /// </summary>
        /// <returns>Collection of <see cref="Ethnicity2"/> DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Ethnicity2>> GetEthnicities2Async(bool bypassCache = false)
        {
            var ethnicityCollection = new List<Ellucian.Colleague.Dtos.Ethnicity2>();

            var ethnicityEntities = await _referenceDataRepository.GetEthnicitiesAsync(bypassCache);
            if (ethnicityEntities != null && ethnicityEntities.Count() > 0)
            {
                foreach (var ethnicity in ethnicityEntities)
                {
                    ethnicityCollection.Add(ConvertEthnicityEntityToDto2(ethnicity));
                }
            }
            return ethnicityCollection;
        }

        /// <remarks>For use with Ellucian HeDM Version 4</remarks>
        /// <summary>
        /// Get an ethnicity from its GUID
        /// </summary>
        /// <returns>Ethnicity DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Ethnicity2> GetEthnicityById2Async(string guid)
        {
            try
            {
                return ConvertEthnicityEntityToDto2((await _referenceDataRepository.GetEthnicitiesAsync(true)).Where(e => e.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Ethnicity not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Gets all geographic area types
        /// </summary>
        /// <returns>Collection of GeographicAreaTypes DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.GeographicAreaType>> GetGeographicAreaTypesAsync(bool bypassCache = false)
        {
            var geographicAreaTypeCollection = new List<Ellucian.Colleague.Dtos.GeographicAreaType>();

            var geographicAreaTypeEntities = await _referenceDataRepository.GetGeographicAreaTypesAsync(bypassCache);
            if (geographicAreaTypeEntities != null && geographicAreaTypeEntities.Count() > 0)
            {
                foreach (var geographicAreaType in geographicAreaTypeEntities)
                {
                    geographicAreaTypeCollection.Add(ConvertGeographicAreaTypeEntityToDto(geographicAreaType));
                }
            }
            return geographicAreaTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Get a geographic area type from its GUID
        /// </summary>
        /// <returns>GeographicAreaType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.GeographicAreaType> GetGeographicAreaTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertGeographicAreaTypeEntityToDto((await _referenceDataRepository.GetGeographicAreaTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Geographic area type not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 5</remarks>
        /// <summary>
        /// Gets all identity document types
        /// </summary>
        /// <returns>Collection of IdentityDocumentType DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.IdentityDocumentType>> GetIdentityDocumentTypesAsync(bool bypassCache = false)
        {
            var identityDocumentTypeCollection = new List<Ellucian.Colleague.Dtos.IdentityDocumentType>();

            var identityDocumentTypeEntities = await _referenceDataRepository.GetIdentityDocumentTypesAsync(bypassCache);
            if (identityDocumentTypeEntities != null && identityDocumentTypeEntities.Count() > 0)
            {
                foreach (var identityDocumentType in identityDocumentTypeEntities)
                {
                    identityDocumentTypeCollection.Add(ConvertIdentityDocumentTypeEntityToDto(identityDocumentType));
                }
            }
            return identityDocumentTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 5</remarks>
        /// <summary>
        /// Get a identity document type from its GUID
        /// </summary>
        /// <returns>IdentityDocumentType DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.IdentityDocumentType> GetIdentityDocumentTypeByGuidAsync(string guid)
        {
            try
            {
                return ConvertIdentityDocumentTypeEntityToDto((await _referenceDataRepository.GetIdentityDocumentTypesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Identity Document Type not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Gets all person filters
        /// </summary>
        /// <returns>Collection of PersonFilter DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PersonFilter>> GetPersonFiltersAsync(bool bypassCache = false)
        {
            var personFilterCollection = new List<Ellucian.Colleague.Dtos.PersonFilter>();

            var personFilterEntities = await _referenceDataRepository.GetPersonFiltersAsync(bypassCache);
            if (personFilterEntities != null && personFilterEntities.Count() > 0)
            {
                foreach (var personFilter in personFilterEntities)
                {
                    personFilterCollection.Add(ConvertPersonFilterEntityToDto(personFilter));
                }
            }
            return personFilterCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM VERSION 6</remarks>
        /// <summary>
        /// Get a person filter from its GUID
        /// </summary>
        /// <returns>PersonFilter DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PersonFilter> GetPersonFilterByGuidAsync(string guid)
        {
            try
            {
                return ConvertPersonFilterEntityToDto((await _referenceDataRepository.GetPersonFiltersAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Person filter not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Gets all privacy statuses
        /// </summary>
        /// <returns>Collection of PrivacyStatuses DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.PrivacyStatus>> GetPrivacyStatusesAsync(bool bypassCache = false)
        {
            var privacyStatusCollection = new List<Ellucian.Colleague.Dtos.PrivacyStatus>();

            var privacyStatusEntities = await _referenceDataRepository.GetPrivacyStatusesAsync(bypassCache);
            if (privacyStatusEntities != null && privacyStatusEntities.Count() > 0)
            {
                var privacyMessages = await _referenceDataRepository.GetPrivacyMessagesAsync();

                foreach (var privacyStatus in privacyStatusEntities)
                {
                    string message = null;
                    if (privacyMessages != null)
                    {
                        privacyMessages.TryGetValue(privacyStatus.Code, out message);
                    }
                    privacyStatusCollection.Add(ConvertPrivacyStatusEntityToDto(privacyStatus, message));
                }
            }
            return privacyStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Get a privacy status from its GUID
        /// </summary>
        /// <returns>PrivacyStatus DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.PrivacyStatus> GetPrivacyStatusByGuidAsync(string guid)
        {
            try
            {
                var privacyStatus = (await _referenceDataRepository.GetPrivacyStatusesAsync(true)).Where(r => r.Guid == guid).First();
                var privacyMessages = await _referenceDataRepository.GetPrivacyMessagesAsync();
                string message = null;
                if (privacyMessages != null)
                {
                    privacyMessages.TryGetValue(privacyStatus.Code, out message);
                }
                return ConvertPrivacyStatusEntityToDto(privacyStatus, message);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Privacy status not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all races
        /// </summary>
        /// <returns>Collection of Race DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Race>> GetRacesAsync(bool bypassCache = false)
        {
            var raceCollection = new List<Ellucian.Colleague.Dtos.Race>();

            var raceEntities = await _referenceDataRepository.GetRacesAsync(bypassCache);
            if (raceEntities != null && raceEntities.Count() > 0)
            {
                foreach (var race in raceEntities)
                {
                    raceCollection.Add(ConvertRaceEntityToDto(race));
                }
            }
            return raceCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Gets all races
        /// </summary>
        /// <returns>Collection of Race DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Race2>> GetRaces2Async(bool bypassCache = false)
        {
            var raceCollection = new List<Ellucian.Colleague.Dtos.Race2>();

            var raceEntities = await _referenceDataRepository.GetRacesAsync(bypassCache);
            if (raceEntities != null && raceEntities.Count() > 0)
            {
                foreach (var race in raceEntities)
                {
                    raceCollection.Add(ConvertRaceEntityToDto2(race));
                }
            }
            return raceCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a race from its GUID
        /// </summary>
        /// <returns>Race DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Race> GetRaceByGuidAsync(string guid)
        {
            try
            {
                return ConvertRaceEntityToDto((await _referenceDataRepository.GetRacesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Race not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM VERSION 4</remarks>
        /// <summary>
        /// Get a race from its GUID
        /// </summary>
        /// <returns>Race DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Race2> GetRaceById2Async(string guid)
        {
            try
            {
                return ConvertRaceEntityToDto2((await _referenceDataRepository.GetRacesAsync(true)).Where(r => r.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Race not found for GUID " + guid, ex);
            }
        }

        /// <remarks>For use with Ellucian HeDM Version 4</remarks>
        /// <summary>
        /// Gets all religions
        /// </summary>
        /// <returns>Collection of <see cref="Religion"/> DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Religion>> GetReligionsAsync(bool bypassCache = false)
        {
            var religionCollection = new List<Ellucian.Colleague.Dtos.Religion>();

            var denominationEntities = await _referenceDataRepository.GetDenominationsAsync(bypassCache);
            if (denominationEntities != null && denominationEntities.Count() > 0)
            {
                foreach (var denomination in denominationEntities)
                {
                    religionCollection.Add(ConvertDenominationEntityToReligionDto(denomination));
                }
            }
            return religionCollection;
        }

        /// <remarks>For use with Ellucian HeDM Version 4</remarks>
        /// <summary>
        /// Get an religion from its GUID
        /// </summary>
        /// <returns>Religion DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.Religion> GetReligionByIdAsync(string guid)
        {
            try
            {
                return ConvertDenominationEntityToReligionDto((await _referenceDataRepository.GetDenominationsAsync(true)).Where(d => d.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Religion not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Gets all social media types
        /// </summary>
        /// <returns>Collection of social media type DTO objects</returns>
        public async Task<IEnumerable<Dtos.SocialMediaType>> GetSocialMediaTypesAsync(bool bypassCache)
        {
            var socialMediaTypeCollection = new List<Dtos.SocialMediaType>();

            var socialMediaTypeEntities = await _referenceDataRepository.GetSocialMediaTypesAsync(bypassCache);
            if (socialMediaTypeEntities != null && socialMediaTypeEntities.Count() > 0)
            {
                foreach (var socialMediaType in socialMediaTypeEntities)
                {
                    socialMediaTypeCollection.Add(ConvertSocialMediaTypeEntityToSocialMediaTypeDto(socialMediaType));
                }
            }
            return socialMediaTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HeDM</remarks>
        /// <summary>
        /// Get a social media type from its id
        /// </summary>
        /// <returns>SocialMediaType DTO object</returns>
        public async Task<Dtos.SocialMediaType> GetSocialMediaTypeByIdAsync(string id)
        {
            try
            {
                return ConvertSocialMediaTypeEntityToSocialMediaTypeDto((await _referenceDataRepository.GetSocialMediaTypesAsync(true)).Where(gcr => gcr.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Social Media Type not found for id " + id, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Social Media Type not found for id " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all source contexts
        /// </summary>
        /// <returns>Collection of <see cref="SourceContext">SourceContext</see> DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.SourceContext>> GetSourceContextsAsync(bool bypassCache = false)
        {
            var sourceContextsCollection = new List<Ellucian.Colleague.Dtos.SourceContext>();

            var sourceContextEntities = await _referenceDataRepository.GetSourceContextsAsync(bypassCache);
            if (sourceContextEntities != null && sourceContextEntities.Any())
            {
                foreach (var sourceContext in sourceContextEntities)
                {
                    sourceContextsCollection.Add(ConvertSourceContextEntityToSourceContextDto(sourceContext));
                }
            }
            return sourceContextsCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a source context from its ID
        /// </summary>
        /// <returns>SourceContext DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.SourceContext> GetSourceContextsByIdAsync(string id)
        {
            try
            {
                return ConvertSourceContextEntityToSourceContextDto((await _referenceDataRepository.GetSourceContextsAsync(true)).Where(cc => cc.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Source context not found for ID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all visa types
        /// </summary>
        /// <returns>Collection of VisaType DTO objects</returns>
        public async Task<IEnumerable<Dtos.VisaType>> GetVisaTypesAsync(bool bypassCache)
        {
            var visaTypeCollection = new List<Dtos.VisaType>();

            var visaTypeEntities = await _referenceDataRepository.GetVisaTypesAsync(bypassCache);
            if (visaTypeEntities != null && visaTypeEntities.Count() > 0)
            {
                foreach (var visaType in visaTypeEntities)
                {
                    visaTypeCollection.Add(ConvertVisaTypeEntityToVisaTypeDto(visaType));
                }
            }
            return visaTypeCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a visa type from its id
        /// </summary>
        /// <returns>VisaType DTO object</returns>
        public async Task<Dtos.VisaType> GetVisaTypeByIdAsync(string id)
        {
            try
            {
                return ConvertVisaTypeEntityToVisaTypeDto((await _referenceDataRepository.GetVisaTypesAsync(true)).Where(gcr => gcr.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Visa Type not found for id " + id, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Visa Type not found for id " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Get a marital status from its GUID
        /// </summary>
        /// <returns>MaritalStatus DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.MaritalStatus> GetMaritalStatusByGuidAsync(string guid)
        {
            try
            {
                return ConvertMaritalStatusEntityToDto((await _referenceDataRepository.GetMaritalStatusesAsync(true)).Where(ms => ms.Guid == guid).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("Marital Status not found for GUID " + guid, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Gets all races
        /// </summary>
        /// <returns>Collection of MaritalStatus DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus>> GetMaritalStatusesAsync(bool bypassCache = false)
        {
            var maritalStatusCollection = new List<Ellucian.Colleague.Dtos.MaritalStatus>();

            var maritalStatusEntities = await _referenceDataRepository.GetMaritalStatusesAsync(false);
            if (maritalStatusEntities != null && maritalStatusEntities.Count() > 0)
            {
                foreach (var maritalStatus in maritalStatusEntities)
                {
                    maritalStatusCollection.Add(ConvertMaritalStatusEntityToDto(maritalStatus));
                }
            }
            return maritalStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Get a marital status from its ID
        /// </summary>
        /// <returns>MaritalStatus2 DTO object</returns>
        public async Task<Ellucian.Colleague.Dtos.MaritalStatus2> GetMaritalStatusById2Async(string id)
        {
            try
            {
                return ConvertMaritalStatusEntityToDto2((await _referenceDataRepository.GetMaritalStatusesAsync(true)).Where(ms => ms.Guid == id).First());
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyNotFoundException("Marital Status not found for ID " + id, ex);
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Gets all Marital Statuses
        /// </summary>
        /// <returns>Collection of MaritalStatus2 DTO objects</returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.MaritalStatus2>> GetMaritalStatuses2Async(bool bypassCache = false)
        {
            var maritalStatusCollection = new List<Ellucian.Colleague.Dtos.MaritalStatus2>();

            var maritalStatusEntities = await _referenceDataRepository.GetMaritalStatusesAsync(bypassCache);
            if (maritalStatusEntities != null && maritalStatusEntities.Count() > 0)
            {
                foreach (var maritalStatus in maritalStatusEntities)
                {
                    maritalStatusCollection.Add(ConvertMaritalStatusEntityToDto2(maritalStatus));
                }
            }
            return maritalStatusCollection;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a CitizenshipStatus domain entity to its corresponding CitizenshipStatus DTO
        /// </summary>
        /// <param name="source">CitizenshipStatus domain entity</param>
        /// <returns>CitizenshipStatus DTO</returns>
        private Ellucian.Colleague.Dtos.CitizenshipStatus ConvertCitizenshipStatusEntityToDto(Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatus source)
        {
            var citizenshipStatus = new Ellucian.Colleague.Dtos.CitizenshipStatus();

            //race.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            citizenshipStatus.Id = source.Guid;
            citizenshipStatus.Code = source.Code;
            citizenshipStatus.Title = source.Description;
            citizenshipStatus.Description = null;
            citizenshipStatus.citizenshipStatusType = ConvertCitizenshipStatusTypeDomainEnumToCitizenshipStatusTypeDtoEnum(source.CitizenshipStatusType);

            return citizenshipStatus;
        }

        /// <remarks>For use with Ellucian HeDM Version 4</remarks>
        /// <summary>
        /// Converts an Denomination domain entity to its corresponding Religion DTO
        /// </summary>
        /// <param name="source">Denomination domain entity</param>
        /// <returns>Religion DTO</returns>
        private Ellucian.Colleague.Dtos.Religion ConvertDenominationEntityToReligionDto(Ellucian.Colleague.Domain.Base.Entities.Denomination source)
        {
            var religion = new Ellucian.Colleague.Dtos.Religion();

            religion.Id = source.Guid;
            religion.Code = source.Code;
            religion.Title = source.Description;
            religion.Description = null;

            return religion;
        }

        /// <remarks>For use with Ellucian HeDM Version 4</remarks>
        /// <summary>
        /// Converts an Ethnicity domain entity to its corresponding Ethnicity DTO
        /// </summary>
        /// <param name="source">Ethnicity domain entity</param>
        /// <returns>Ethnicity DTO</returns>
        private Ellucian.Colleague.Dtos.Ethnicity2 ConvertEthnicityEntityToDto2(Ellucian.Colleague.Domain.Base.Entities.Ethnicity source)
        {
            var ethnicity = new Ellucian.Colleague.Dtos.Ethnicity2();

            ethnicity.Id = source.Guid;
            ethnicity.Code = source.Code;
            ethnicity.Title = source.Description;
            ethnicity.Description = null;
            ethnicity.EthnicityReporting = ConvertEthnicityType2DomainToEthnicityReporting(source.Type);
            return ethnicity;
        }

        /// <summary>
        /// Converts domain ehtnicity type to EthnicityReporting
        /// </summary>
        /// <param name="ethnicityType"></param>
        /// <returns></returns>
        private List<EthnicityReporting> ConvertEthnicityType2DomainToEthnicityReporting(Domain.Base.Entities.EthnicityType ethnicityType)
        {
            List<EthnicityReporting> ethnicityReportingList = new List<EthnicityReporting>() 
            {
                new EthnicityReporting()
                {
                    EthnicityReportingCountry = new EthnicityReportingCountry()
                    {
                        CountryCode = CountryCodeType.USA,
                        EthnicCategory = ConvertEthnicityType2DomainEnumToEthnicityDtoEnum(ethnicityType)
                    }
                }
            };
            return ethnicityReportingList;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a GeographicAreaType domain entity to its corresponding GeographicAreaType DTO
        /// </summary>
        /// <param name="source">GeographicAreaType domain entity</param>
        /// <returns>GeographicAreaType DTO</returns>
        private Ellucian.Colleague.Dtos.GeographicAreaType ConvertGeographicAreaTypeEntityToDto(Ellucian.Colleague.Domain.Base.Entities.GeographicAreaType source)
        {
            var geographicAreaType = new Ellucian.Colleague.Dtos.GeographicAreaType();

            //race.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            geographicAreaType.Id = source.Guid;
            geographicAreaType.Code = source.Code;
            geographicAreaType.Title = source.Description;
            geographicAreaType.Description = null;
            geographicAreaType.geographicAreaTypeCategory = ConvertGeographicAreaTypeCategoryDomainEnumToGeographicAreaTypeCategoryDtoEnum(source.GeographicAreaTypeCategory);

            return geographicAreaType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a IdentityDocumentType domain entity to its corresponding IdentityDocumentType DTO
        /// </summary>
        /// <param name="source">IdentityDocumentType domain entity</param>
        /// <returns>IdentityDocumentType DTO</returns>
        private Ellucian.Colleague.Dtos.IdentityDocumentType ConvertIdentityDocumentTypeEntityToDto(Ellucian.Colleague.Domain.Base.Entities.IdentityDocumentType source)
        {
            var identityDocumentType = new Ellucian.Colleague.Dtos.IdentityDocumentType();

            identityDocumentType.Id = source.Guid;
            identityDocumentType.Code = source.Code;
            identityDocumentType.Title = source.Description;
            identityDocumentType.Description = null;
            identityDocumentType.identityDocumentTypeCategory = ConvertIdentityDocumentTypeCategoryDomainEnumToIdentityDocumentTypeCategoryDtoEnum(source.IdentityDocumentTypeCategory);

            return identityDocumentType;
        }

        /// <remarks>FOR USE WITH ELLUCIAN EEDM</remarks>
        /// <summary>
        /// Converts a PersonFilter domain entity to its corresponding PersonFilter DTO
        /// </summary>
        /// <param name="source">PersonFilter domain entity</param>
        /// <returns>PersonFilter DTO</returns>
        private Ellucian.Colleague.Dtos.PersonFilter ConvertPersonFilterEntityToDto(Ellucian.Colleague.Domain.Base.Entities.PersonFilter source)
        {
            var personFilter = new Ellucian.Colleague.Dtos.PersonFilter();

            personFilter.Id = source.Guid;
            personFilter.Code = source.Code;
            personFilter.Title = source.Description;
            personFilter.Description = null;

            return personFilter;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a PrivacyStatus domain entity to its corresponding PrivacyStatus DTO
        /// </summary>
        /// <param name="source">PrivacyStatus domain entity</param>
        /// <param name="message">The message/description associated to the PrivacyStatus domain entity</param>
        /// <returns>PrivacyStatus DTO</returns>
        private Ellucian.Colleague.Dtos.PrivacyStatus ConvertPrivacyStatusEntityToDto(Ellucian.Colleague.Domain.Base.Entities.PrivacyStatus source, string message = null)
        {
            var privacyStatus = new Ellucian.Colleague.Dtos.PrivacyStatus();

            privacyStatus.Id = source.Guid;
            privacyStatus.Code = source.Code;
            privacyStatus.Title = source.Description;
            privacyStatus.Description = message;
            privacyStatus.privacyStatusType = ConvertPrivacyStatusTypeDomainEnumToPrivacyStatusTypeDtoEnum(source.PrivacyStatusType);

            return privacyStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a Race domain entity to its corresponding Race DTO
        /// </summary>
        /// <param name="source">Race domain entity</param>
        /// <returns>Race DTO</returns>
        private Ellucian.Colleague.Dtos.Race ConvertRaceEntityToDto(Ellucian.Colleague.Domain.Base.Entities.Race source)
        {
            var race = new Ellucian.Colleague.Dtos.Race();

            //race.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            race.Guid = source.Guid;
            race.Abbreviation = source.Code;
            race.Title = source.Description;
            race.Description = null;
            race.RaceType = ConvertRaceTypeDomainEnumToRaceTypeDtoEnum(source.Type);

            return race;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a Race domain entity to its corresponding Race DTO
        /// </summary>
        /// <param name="source">Race domain entity</param>
        /// <returns>Race DTO</returns>
        private Ellucian.Colleague.Dtos.Race2 ConvertRaceEntityToDto2(Ellucian.Colleague.Domain.Base.Entities.Race source)
        {
            var race = new Ellucian.Colleague.Dtos.Race2();

            //race.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            race.Id = source.Guid;
            race.Code = source.Code;
            race.Title = source.Description;
            race.Description = null;
            race.RaceReporting = ConvertRaceTypeToRaceReporting(source.Type);
            return race;
        }

        /// <summary>
        /// Converts race type to race reporting
        /// </summary>
        /// <param name="raceType"></param>
        /// <returns></returns>
        private List<RaceReporting> ConvertRaceTypeToRaceReporting(Domain.Base.Entities.RaceType? raceType)
        {
            ReportingRacialCategory? reportingRacialCategory = ConvertRaceTypeDomainEnumToReportingRacialCategoryDtoEnum(raceType);
            List<RaceReporting> raceReportingList = null;
            if (reportingRacialCategory != null)
            {
                raceReportingList = new List<RaceReporting>();
                RaceReporting raceReporting = new RaceReporting();
                RaceReportingCountry raceReportingCountry = new RaceReportingCountry();
                raceReportingCountry.CountryCode = CountryCodeType.USA;
                raceReportingCountry.ReportingRacialCategory = reportingRacialCategory;
                raceReporting.RaceReportingCountry = raceReportingCountry;
                raceReportingList.Add(raceReporting);
            }
            return raceReportingList;
        }

        /// <summary>
        /// Converts a RaceType domain enumeration value to reporting racial category
        /// </summary>
        /// <param name="source">RaceType domain enumeration value</param>
        /// <returns>RaceType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EnumProperties.ReportingRacialCategory? ConvertRaceTypeDomainEnumToReportingRacialCategoryDtoEnum(Ellucian.Colleague.Domain.Base.Entities.RaceType? source)
        {
            switch (source)
            {
                case Domain.Base.Entities.RaceType.AmericanIndian:
                    return ReportingRacialCategory.AmericanIndianOrAlaskaNative;
                case Domain.Base.Entities.RaceType.Asian:
                    return ReportingRacialCategory.Asian;
                case Domain.Base.Entities.RaceType.Black:
                    return ReportingRacialCategory.BlackOrAfricanAmerican;
                case Domain.Base.Entities.RaceType.PacificIslander:
                    return ReportingRacialCategory.HawaiianOrPacificIslander;
                case Domain.Base.Entities.RaceType.White:
                    return ReportingRacialCategory.White;
                default:
                    return null;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a SourceContext domain entity to its corresponding SourceContext DTO
        /// </summary>
        /// <param name="source">SourceContext domain entity</param>
        /// <returns>SourceContext DTO</returns>
        private Ellucian.Colleague.Dtos.SourceContext ConvertSourceContextEntityToSourceContextDto(Ellucian.Colleague.Domain.Base.Entities.SourceContext source)
        {
            var sourceContext = new Ellucian.Colleague.Dtos.SourceContext();
            sourceContext.Id = source.Guid;
            sourceContext.Code = source.Code;
            sourceContext.Title = source.Description;
            sourceContext.Description = null;
            return sourceContext;
        }

        /// <summary>
        /// Converts social media type entity to dto
        /// </summary>
        /// <param name="socialMediaType">socialMediaType</param>
        /// <returns>socialMediaType</returns>
        private Dtos.SocialMediaType ConvertSocialMediaTypeEntityToSocialMediaTypeDto(Ellucian.Colleague.Domain.Base.Entities.SocialMediaType socialMediaType)
        {
            var gcr = new Dtos.SocialMediaType()
            {

                Id = socialMediaType.Guid,
                Code = socialMediaType.Code,
                Title = socialMediaType.Description,
                Description = null,
                SocialMediaTypeCategory = ConvertSocialMediaTypeCategoryDomainEnumToSocialMediaTypeCategoryDtoEnum(socialMediaType.Type)
            };
            return gcr;
        }

        /// <summary>
        /// Converts visa type entity to dto
        /// </summary>
        /// <param name="visaType">VisaTypeGuidItem</param>
        /// <returns>VisaType</returns>
        private Dtos.VisaType ConvertVisaTypeEntityToVisaTypeDto(Ellucian.Colleague.Domain.Base.Entities.VisaTypeGuidItem visaType)
        {
            var visa = new Dtos.VisaType()
            {

                Id = visaType.Guid,
                Code = visaType.Code,
                Title = visaType.Description,
                Description = null,
                VisaTypeCategory = ConvertVisaTypeDomainEnumToVisaTypeCategory(visaType.VisaTypeCategory)
            };
            return visa;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a MaritalStatus domain entity to its corresponding MaritalStatus DTO
        /// </summary>
        /// <param name="source">MaritalStatus domain entity</param>
        /// <returns>MaritalStatus DTO</returns>
        private Ellucian.Colleague.Dtos.MaritalStatus ConvertMaritalStatusEntityToDto(Ellucian.Colleague.Domain.Base.Entities.MaritalStatus source)
        {
            var maritalStatus = new Ellucian.Colleague.Dtos.MaritalStatus();

            //maritalStatus.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            maritalStatus.Guid = source.Guid;
            maritalStatus.Abbreviation = source.Code;
            maritalStatus.Title = source.Description;
            maritalStatus.Description = null;
            maritalStatus.StatusType = ConvertMaritalStatusTypeDomainEnumToMaritalStatusTypeDtoEnum(source.Type);

            return maritalStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM Version 4</remarks>
        /// <summary>
        /// Converts a MaritalStatus domain entity to its corresponding MaritalStatus DTO
        /// </summary>
        /// <param name="source">MaritalStatus domain entity</param>
        /// <returns>MaritalStatus2 DTO</returns>
        private Ellucian.Colleague.Dtos.MaritalStatus2 ConvertMaritalStatusEntityToDto2(Ellucian.Colleague.Domain.Base.Entities.MaritalStatus source)
        {
            var maritalStatus = new Ellucian.Colleague.Dtos.MaritalStatus2();

            //maritalStatus.Metadata = new Dtos.MetadataObject(_dataOrigin); // TODO: JPM2 - How do we set data origin from Colleague to LDM?
            maritalStatus.Id = source.Guid;
            maritalStatus.Code = source.Code;
            maritalStatus.Title = source.Description;
            maritalStatus.Description = null;
            maritalStatus.StatusType = ConvertMaritalStatusTypeDomainEnumToMaritalStatusType2DtoEnum(source.Type);

            return maritalStatus;
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a CitizenshipStatusType domain enumeration value to its corresponding CitizenshipStatusType DTO enumeration value
        /// </summary>
        /// <param name="source">CitizenshipStatusType domain enumeration value</param>
        /// <returns>CitizenshipStatusType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.CitizenshipStatusType ConvertCitizenshipStatusTypeDomainEnumToCitizenshipStatusTypeDtoEnum(Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatusType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.CitizenshipStatusType.Citizen:
                    return Dtos.CitizenshipStatusType.Citizen;
                default:
                    return Dtos.CitizenshipStatusType.NonCitizen;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts an EthnicityType domain enumeration value to its corresponding Ethnicity DTO enumeration value
        /// </summary>
        /// <param name="source">EthnicityType domain enumeration value</param>
        /// <returns>EthnicityType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EthnicityType ConvertEthnicityTypeDomainEnumToEthnicityDtoEnum(Ellucian.Colleague.Domain.Base.Entities.EthnicityType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.EthnicityType.Hispanic:
                    return Dtos.EthnicityType.Hispanic;
                default:
                    return Dtos.EthnicityType.Nonhispanic;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a GeographicAreaTypeCategory domain enumeration value to its corresponding GeographicAreaTypeCategory DTO enumeration value
        /// </summary>
        /// <param name="source">GeographicAreaTypeCategory domain enumeration value</param>
        /// <returns>GeographicAreaTypeCategory DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.GeographicAreaTypeCategory ConvertGeographicAreaTypeCategoryDomainEnumToGeographicAreaTypeCategoryDtoEnum(Ellucian.Colleague.Domain.Base.Entities.GeographicAreaTypeCategory source)
        {
            switch (source)
            {
                case Domain.Base.Entities.GeographicAreaTypeCategory.Fundraising:
                    return Dtos.GeographicAreaTypeCategory.Fundraising;
                case Domain.Base.Entities.GeographicAreaTypeCategory.Governmental:
                    return Dtos.GeographicAreaTypeCategory.Governmental;
                case Domain.Base.Entities.GeographicAreaTypeCategory.Postal:
                    return Dtos.GeographicAreaTypeCategory.Postal;
                default:
                    return Dtos.GeographicAreaTypeCategory.Recruitment;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a IdentityDocumentTypeCategory domain enumeration value to its corresponding IdentityDocumentTypeCategory DTO enumeration value
        /// </summary>
        /// <param name="source">IdentityDocumentTypeCategory domain enumeration value</param>
        /// <returns>IdentityDocumentTypeCategory DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.IdentityDocumentTypeCategory ConvertIdentityDocumentTypeCategoryDomainEnumToIdentityDocumentTypeCategoryDtoEnum(Ellucian.Colleague.Domain.Base.Entities.IdentityDocumentTypeCategory source)
        {
            switch (source)
            {
                case Domain.Base.Entities.IdentityDocumentTypeCategory.Passport:
                    return Dtos.IdentityDocumentTypeCategory.Passport;
                case Domain.Base.Entities.IdentityDocumentTypeCategory.Other:
                    return Dtos.IdentityDocumentTypeCategory.Other;
                case Domain.Base.Entities.IdentityDocumentTypeCategory.PhotoId:
                    return Dtos.IdentityDocumentTypeCategory.PhotoID;
                default:
                    return Dtos.IdentityDocumentTypeCategory.Other;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a PrivacyStatusType domain enumeration value to its corresponding PrivacyStatusType DTO enumeration value
        /// </summary>
        /// <param name="source">PrivacyStatusType domain enumeration value</param>
        /// <returns>PrivacyStatusType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.PrivacyStatusType ConvertPrivacyStatusTypeDomainEnumToPrivacyStatusTypeDtoEnum(Ellucian.Colleague.Domain.Base.Entities.PrivacyStatusType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.PrivacyStatusType.restricted:
                    return Dtos.PrivacyStatusType.Restricted;
                case Domain.Base.Entities.PrivacyStatusType.unrestricted:
                    return Dtos.PrivacyStatusType.Unrestricted;
                default:
                    return Dtos.PrivacyStatusType.Restricted;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts an EthnicityType domain enumeration value to its corresponding Ethnicity DTO enumeration value
        /// </summary>
        /// <param name="source">EthnicityType domain enumeration value</param>
        /// <returns>EthnicityType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.EthnicityType2 ConvertEthnicityType2DomainEnumToEthnicityDtoEnum(Ellucian.Colleague.Domain.Base.Entities.EthnicityType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.EthnicityType.Hispanic:
                    return Dtos.EthnicityType2.Hispanic;
                case Domain.Base.Entities.EthnicityType.NonHispanic:
                default:
                    return Dtos.EthnicityType2.Nonhispanic;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a RaceType domain enumeration value to its corresponding RaceType DTO enumeration value
        /// </summary>
        /// <param name="source">RaceType domain enumeration value</param>
        /// <returns>RaceType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.RaceType? ConvertRaceTypeDomainEnumToRaceTypeDtoEnum(Ellucian.Colleague.Domain.Base.Entities.RaceType? source)
        {
            switch (source)
            {
                case Domain.Base.Entities.RaceType.AmericanIndian:
                    return Dtos.RaceType.AmericanIndian;
                case Domain.Base.Entities.RaceType.Asian:
                    return Dtos.RaceType.Asian;
                case Domain.Base.Entities.RaceType.Black:
                    return Dtos.RaceType.Black;
                case Domain.Base.Entities.RaceType.PacificIslander:
                    return Dtos.RaceType.OtherPacificIslander;
                case Domain.Base.Entities.RaceType.White:
                    return Dtos.RaceType.White;
                default:
                    return null;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a RaceType domain enumeration value to its corresponding RaceType DTO enumeration value
        /// </summary>
        /// <param name="source">RaceType domain enumeration value</param>
        /// <returns>RaceType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.RaceType2 ConvertRaceTypeDomainEnumToRaceType2DtoEnum(Ellucian.Colleague.Domain.Base.Entities.RaceType source)
        {
            switch (source)
            {
                case Domain.Base.Entities.RaceType.AmericanIndian:
                    return Dtos.RaceType2.AmericanIndian;
                case Domain.Base.Entities.RaceType.Asian:
                    return Dtos.RaceType2.Asian;
                case Domain.Base.Entities.RaceType.Black:
                    return Dtos.RaceType2.Black;
                case Domain.Base.Entities.RaceType.PacificIslander:
                    return Dtos.RaceType2.OtherPacificIslander;
                default:
                    return Dtos.RaceType2.White;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a VisaType domain enumeration value to its corresponding VisaType DTO enumeration value
        /// </summary>
        /// <param name="source">VisaType domain enumeration value</param>
        /// <returns>VisaType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.VisaTypeCategory ConvertVisaTypeDomainEnumToVisaTypeCategory(Ellucian.Colleague.Domain.Base.Entities.VisaTypeCategory source)
        {
            switch (source)
            {
                case Domain.Base.Entities.VisaTypeCategory.Immigrant:
                    return VisaTypeCategory.Immigrant;
                case Domain.Base.Entities.VisaTypeCategory.NonImmigrant:
                    return VisaTypeCategory.NonImmigrant;
                default:
                    return VisaTypeCategory.NonImmigrant;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a SocialMediaTypeCategory domain enumeration value to its corresponding SocialMediaTypeCategory DTO enumeration value
        /// </summary>
        /// <param name="source">SocialMediaTypeCategory domain enumeration value</param>
        /// <returns>SocialMediaTypeCategory DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.SocialMediaTypeCategory ConvertSocialMediaTypeCategoryDomainEnumToSocialMediaTypeCategoryDtoEnum(Ellucian.Colleague.Domain.Base.Entities.SocialMediaTypeCategory source)
        {
            switch (source)
            {
                case Domain.Base.Entities.SocialMediaTypeCategory.blog:
                    return Dtos.SocialMediaTypeCategory.blog;
                case Domain.Base.Entities.SocialMediaTypeCategory.facebook:
                    return Dtos.SocialMediaTypeCategory.facebook;
                case Domain.Base.Entities.SocialMediaTypeCategory.foursquare:
                    return Dtos.SocialMediaTypeCategory.foursquare;
                case Domain.Base.Entities.SocialMediaTypeCategory.hangouts:
                    return Dtos.SocialMediaTypeCategory.hangouts;
                case Domain.Base.Entities.SocialMediaTypeCategory.icq:
                    return Dtos.SocialMediaTypeCategory.icq;
                case Domain.Base.Entities.SocialMediaTypeCategory.instagram:
                    return Dtos.SocialMediaTypeCategory.instagram;
                case Domain.Base.Entities.SocialMediaTypeCategory.jabber:
                    return Dtos.SocialMediaTypeCategory.jabber;
                case Domain.Base.Entities.SocialMediaTypeCategory.linkedin:
                    return Dtos.SocialMediaTypeCategory.linkedin;
                case Domain.Base.Entities.SocialMediaTypeCategory.other:
                    return Dtos.SocialMediaTypeCategory.other;
                case Domain.Base.Entities.SocialMediaTypeCategory.pinterest:
                    return Dtos.SocialMediaTypeCategory.pinterest;
                case Domain.Base.Entities.SocialMediaTypeCategory.qq:
                    return Dtos.SocialMediaTypeCategory.qq;
                case Domain.Base.Entities.SocialMediaTypeCategory.skype:
                    return Dtos.SocialMediaTypeCategory.skype;
                case Domain.Base.Entities.SocialMediaTypeCategory.tumblr:
                    return Dtos.SocialMediaTypeCategory.tumblr;
                case Domain.Base.Entities.SocialMediaTypeCategory.twitter:
                    return Dtos.SocialMediaTypeCategory.twitter;
                case Domain.Base.Entities.SocialMediaTypeCategory.website:
                    return Dtos.SocialMediaTypeCategory.website;
                case Domain.Base.Entities.SocialMediaTypeCategory.windowsLive:
                    return Dtos.SocialMediaTypeCategory.windowsLive;
                case Domain.Base.Entities.SocialMediaTypeCategory.yahoo:
                    return Dtos.SocialMediaTypeCategory.yahoo;
                case Domain.Base.Entities.SocialMediaTypeCategory.youtube:
                    return Dtos.SocialMediaTypeCategory.youtube;
                default:
                    return Dtos.SocialMediaTypeCategory.other;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a MaritalStatusType domain enumeration value to its corresponding MaritalStatusType DTO enumeration value
        /// </summary>
        /// <param name="source">MaritalStatusType domain enumeration value</param>
        /// <returns>MaritalStatusType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.MaritalStatusType? ConvertMaritalStatusTypeDomainEnumToMaritalStatusTypeDtoEnum(Ellucian.Colleague.Domain.Base.Entities.MaritalStatusType? source)
        {
            switch (source)
            {
                case Domain.Base.Entities.MaritalStatusType.Separated:
                    return Dtos.MaritalStatusType.Separated;
                case Domain.Base.Entities.MaritalStatusType.Married:
                    return Dtos.MaritalStatusType.Married;
                case Domain.Base.Entities.MaritalStatusType.Divorced:
                    return Dtos.MaritalStatusType.Divorced;
                case Domain.Base.Entities.MaritalStatusType.Widowed:
                    return Dtos.MaritalStatusType.Widowed;
                case Domain.Base.Entities.MaritalStatusType.Single:
                    return Dtos.MaritalStatusType.Single;
                default:
                    return null;
            }
        }

        /// <remarks>FOR USE WITH ELLUCIAN HEDM</remarks>
        /// <summary>
        /// Converts a MaritalStatusType domain enumeration value to its corresponding MaritalStatusType DTO enumeration value
        /// </summary>
        /// <param name="source">MaritalStatusType domain enumeration value</param>
        /// <returns>MaritalStatusType DTO enumeration value</returns>
        private Ellucian.Colleague.Dtos.MaritalStatusType2? ConvertMaritalStatusTypeDomainEnumToMaritalStatusType2DtoEnum(Ellucian.Colleague.Domain.Base.Entities.MaritalStatusType? source)
        {
            switch (source)
            {
                case Domain.Base.Entities.MaritalStatusType.Separated:
                    return Dtos.MaritalStatusType2.Separated;
                case Domain.Base.Entities.MaritalStatusType.Married:
                    return Dtos.MaritalStatusType2.Married;
                case Domain.Base.Entities.MaritalStatusType.Divorced:
                    return Dtos.MaritalStatusType2.Divorced;
                case Domain.Base.Entities.MaritalStatusType.Widowed:
                    return Dtos.MaritalStatusType2.Widowed;
                case Domain.Base.Entities.MaritalStatusType.Single:
                    return Dtos.MaritalStatusType2.Single;
                default:
                    return Dtos.MaritalStatusType2.Single;
            }
        }
    }
}
