// Copyright 2016-2023 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Dmi.Runtime;
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
    public class EducationalInstitutionsService : BaseCoordinationService, IEducationalInstitutionsService
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IInstitutionRepository _institutionRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _iAdapterRegistry;
        private readonly ILogger _logger;
        private IEnumerable<Domain.Base.Entities.SocialMediaType> _socialMediaTypes = null;
        private IEnumerable<Domain.Base.Entities.EmailType> _emailTypes = null;
        private IEnumerable<string> _homeInstitutionsList = null;
        private IEnumerable<Domain.Base.Entities.AddressType2> _addressType2 = null;
        private IEnumerable<Domain.Base.Entities.PhoneType> _phoneType = null;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="referenceDataRepository"></param>
        /// <param name="personRepository"></param>
        /// <param name="institutionRepository"></param>
        /// <param name="configurationRepository"></param>
        /// <param name="iAdapterRegistry"></param>
        /// <param name="logger"></param>
        public EducationalInstitutionsService(IReferenceDataRepository referenceDataRepository, IPersonRepository personRepository,
            IInstitutionRepository institutionRepository, IConfigurationRepository configurationRepository, IAdapterRegistry iAdapterRegistry,
            ICurrentUserFactory currentUserFactory, IRoleRepository roleRepository, ILogger logger)
            : base(iAdapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _referenceDataRepository = referenceDataRepository;
            _institutionRepository = institutionRepository;
            _personRepository = personRepository;
            _configurationRepository = configurationRepository;
            _iAdapterRegistry = iAdapterRegistry;
            _logger = logger;
        }

        private async Task<IEnumerable<Domain.Base.Entities.SocialMediaType>> GetSocialMediaTypesAsync(bool bypassCache)
        {
            if (_socialMediaTypes == null)
            {
                _socialMediaTypes = await _referenceDataRepository.GetSocialMediaTypesAsync(bypassCache);
            }
            return _socialMediaTypes;
        }

        private async Task<IEnumerable<Domain.Base.Entities.EmailType>> GetEmailTypesAsync(bool bypassCache)
        {
            if (_emailTypes == null)
            {
                _emailTypes = await _referenceDataRepository.GetEmailTypesAsync(bypassCache);
            }
            return _emailTypes;
        }

        private async Task<IEnumerable<string>> GetHomeInstitutionIdList()
        {
            if (_homeInstitutionsList == null)
            {
                _homeInstitutionsList = await _referenceDataRepository.GetHomeInstitutionIdList();
            }
            return _homeInstitutionsList;
        }


        private async Task<IEnumerable<Domain.Base.Entities.AddressType2>> GetAddressTypes2Async(bool bypassCache)
        {
            if (_addressType2 == null)
            {
                _addressType2 = await _referenceDataRepository.GetAddressTypes2Async(bypassCache);
            }
            return _addressType2;
        }

        private async Task<IEnumerable<Domain.Base.Entities.PhoneType>> GetPhoneTypesAsync(bool bypassCache)
        {
            if (_phoneType == null)
            {
                _phoneType = await _referenceDataRepository.GetPhoneTypesAsync(bypassCache);
            }
            return _phoneType;
        }

        #region public methods

        /// <summary>
        /// get educational institutions by type
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <param name="type"></param>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<EducationalInstitution>, int>> GetEducationalInstitutionsByTypeAsync(int offset, int limit, Dtos.EducationalInstitution educationalInstitution,
                Dtos.EnumProperties.EducationalInstitutionType? type, bool bypassCache = false)
        {
            //CheckEducationalInstitutionsViewPermissions();

            #region apply filters

            InstType? typeFilter = null;

            switch (type)
            {
                case EducationalInstitutionType.PostSecondarySchool:
                    typeFilter = InstType.College;
                    break;
                case EducationalInstitutionType.SecondarySchool:
                    typeFilter = InstType.HighSchool;
                    break;
            }

            List<Tuple<string, string>> creds = new List<Tuple<string, string>>();
            if (educationalInstitution != null && educationalInstitution.Credentials != null && educationalInstitution.Credentials.Any())
            {
                educationalInstitution.Credentials.ToList().ForEach(i =>
               {
                   var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                   Tuple<string, string> tuple = new Tuple<string, string>(i.Type.ToString(), tempValue);
                   creds.Add(tuple);
               });
            }


            #endregion

            #region get domain entities from repository

            Tuple<IEnumerable<Institution>, int> institutions = null;

            try
            {
                institutions = await _institutionRepository.GetInstitutionAsync(offset, limit, typeFilter, creds);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
                throw IntegrationApiException;
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "Bad.Data");
                throw IntegrationApiException;
            }

            if (institutions == null || !institutions.Item1.Any())
            {
                return new Tuple<IEnumerable<EducationalInstitution>, int>(new List<EducationalInstitution>(), 0);
            }

            #endregion

            #region build response

            var educationalInsitutionsCollection = new List<EducationalInstitution>();

            try
            {
                var ids = institutions.Item1
                   .Where(x => (!string.IsNullOrEmpty(x.Id)))
                   .Select(x => x.Id).Distinct().ToList();

                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);

                foreach (var institution in institutions.Item1)
                {
                    try
                    {
                        educationalInsitutionsCollection.Add(await BuildEducationalInstitution(institution, personGuidCollection, bypassCache));
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "Global.Internal.Error", id: institution.Id);
                    }
                }
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An unexpected error occurred extracting request. " + ex.Message, "Global.Internal.Error");
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return new Tuple<IEnumerable<EducationalInstitution>, int>(educationalInsitutionsCollection, institutions.Item2);

            #endregion
        }

        /// <summary>
        /// GetEducationalInstitutionByGuidAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<EducationalInstitution> GetEducationalInstitutionByGuidAsync(string id, bool bypassCache = false)
        {
            if (string.IsNullOrEmpty(id))
            {
                //throw new ArgumentNullException("id", "The GUID must be specified in the request URL.");
                IntegrationApiExceptionAddError("Must provide a educational-institutions GUID for retrieval.", "Missing.GUID",
                  "", "", System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }

            //CheckEducationalInstitutionsViewPermissions();

            Institution institution;

            try
            {
                institution = await _institutionRepository.GetInstitutionByGuidAsync(id);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: id);
                throw IntegrationApiException;
            }
            catch (KeyNotFoundException)
            {
                //throw new KeyNotFoundException("No educational-institutions was found for GUID '" + id + "'.");
                IntegrationApiExceptionAddError("No educational-institutions was found for GUID '" + id + "'.", "GUID.Not.Found",
                     id, "", System.Net.HttpStatusCode.NotFound);
                throw IntegrationApiException;
            }

            if (institution == null)
            {
                //throw new KeyNotFoundException("No educational-institutions was found for GUID '" + id + "'.");
                IntegrationApiExceptionAddError("No educational-institutions was found for GUID '" + id + "'.", "GUID.Not.Found",
                     id, "", System.Net.HttpStatusCode.NotFound);
            }

            EducationalInstitution educationalInstitution = null;

            try
            {
                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(new List<string>() { institution.Id });

                educationalInstitution = await BuildEducationalInstitution(institution, personGuidCollection, bypassCache);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError("An unexpected error occurred extracting request. " + ex.Message, "Global.Internal.Error", id);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return educationalInstitution;
        }

        #endregion

        #region private methods

        private async Task<EducationalInstitution> BuildEducationalInstitution(Institution institution, Dictionary<string, string> personGuidCollection, bool bypassCache)
        {
            if (institution == null)
            {
                throw new ArgumentNullException("institution", "institution is a required property.");
            }

            var educationInsitution = new EducationalInstitution();

            var educationInsitutionGuid = string.Empty;

            if (personGuidCollection == null)
            {
                IntegrationApiExceptionAddError(string.Concat("Unable to locate guid for : '", institution.Id, "'"), "GUID.Not.Found", id: institution.Id);
            }
            else
            {
                personGuidCollection.TryGetValue(institution.Id, out educationInsitutionGuid);
                if (string.IsNullOrEmpty(educationInsitutionGuid))
                {
                    IntegrationApiExceptionAddError(string.Concat("Unable to locate guid for : '", institution.Id, "'"), "GUID.Not.Found", id: institution.Id);
                }
                educationInsitution.Id = educationInsitutionGuid;
            }

            if (string.IsNullOrWhiteSpace(institution.Name))
            {
                IntegrationApiExceptionAddError("Institution name is missing.", "Bad.Data", educationInsitutionGuid, institution.Id);
            }
            else
            {
                educationInsitution.Title = institution.Name;
            }
            switch (institution.InstitutionType)
            {
                case InstType.College:
                    educationInsitution.Type = EducationalInstitutionType.PostSecondarySchool;
                    break;
                case InstType.HighSchool:
                    educationInsitution.Type = EducationalInstitutionType.SecondarySchool;
                    break;
                default:
                    IntegrationApiExceptionAddError(string.Concat("The educational-institution with the ID '", institution.Id, "' does not have a valid type."), "Bad.Data", educationInsitutionGuid, institution.Id);
                    break;
            }

            var homeInstitutionsList = await GetHomeInstitutionIdList();
            if (homeInstitutionsList != null && homeInstitutionsList.Contains(institution.Id))
            {
                educationInsitution.HomeInstitution = HomeInstitutionType.Home;
            }
            else
            {
                educationInsitution.HomeInstitution = HomeInstitutionType.External;
            }

            if (!string.IsNullOrEmpty(institution.Ceeb))
            {
                educationInsitution.StandardizedCodes = new List<StandardizedCodesDtoProperty>()
                {
                    new StandardizedCodesDtoProperty()
                     {
                       Country = new StandardizedCeebCodesDtoProperty()
                        {
                           Ceeb = institution.Ceeb
                        }
                    }
                };
            }

            if (institution.EmailAddresses != null && institution.EmailAddresses.Any())
            {
                var personEmailAddressDtos = await GetPersonEmailTypeDtoCollection(institution.EmailAddresses, institution.Id, bypassCache);
                if (personEmailAddressDtos != null && personEmailAddressDtos.Any())
                {
                    educationInsitution.EmailAddresses = personEmailAddressDtos;
                }
            }

            if (institution.Addresses != null && institution.Addresses.Any())
            {
                var personAddressDtos = await GetPersonAddressDtoCollectionAsync(institution.Addresses, institution.Id, bypassCache);
                if (personAddressDtos != null && personAddressDtos.Any())
                {
                    educationInsitution.Addresses = personAddressDtos;
                }
            }

            if (institution.Phones != null && institution.Phones.Any())
            {
                var personPhoneDtos = await GetPersonPhoneDtoCollectionAsync(institution.Phones, institution.Id, bypassCache);
                if (personPhoneDtos != null && personPhoneDtos.Any())
                {
                    educationInsitution.Phones = personPhoneDtos;
                }
            }

            if (institution.SocialMedia != null && institution.SocialMedia.Any())
            {
                var personSocialMediaDtos = await GetPersonSocialMediaDtoCollection(institution.SocialMedia, institution.Id, bypassCache);
                if (personSocialMediaDtos != null && personSocialMediaDtos.Any())
                {
                    educationInsitution.SocialMedia = personSocialMediaDtos;
                }
            }
            if (!string.IsNullOrEmpty(institution.Id))
            {
                educationInsitution.Credentials = new List<Credential3DtoProperty>()
                {
                    new Credential3DtoProperty()
                    {
                        Type = Credential3Type.ColleaguePersonId,
                        Value = institution.Id
                    }
                };
            }

            return educationInsitution;
        }

        private async Task<List<PersonEmailDtoProperty>> GetPersonEmailTypeDtoCollection(IEnumerable<Domain.Base.Entities.EmailAddress> emailAddresses, string institutionId, bool bypassCache = false)
        {
            var emailAddressDtos = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();

            IEnumerable<Domain.Base.Entities.EmailType> emailTypes = null;

            try
            {
                emailTypes = await GetEmailTypesAsync(bypassCache);
            }
            catch (Exception)
            {
                //do not throw error
                return emailAddressDtos;
            }

            if (emailTypes == null || !emailTypes.Any())
            {
                return emailAddressDtos;
            }

            foreach (var emailAddressEntity in emailAddresses)
            {
                try
                {

                    var codeItem = emailTypes.FirstOrDefault(pt => pt.Code == emailAddressEntity.TypeCode);
                    if (codeItem != null && !string.IsNullOrEmpty(codeItem.Guid))
                    {
                        var addressDto = new Dtos.DtoProperties.PersonEmailDtoProperty()
                        {
                            Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty()
                            {
                                EmailType = (Dtos.EmailTypeList)Enum.Parse(typeof(Dtos.EmailTypeList), codeItem.EmailTypeCategory.ToString()),
                                Detail = new Dtos.GuidObject2(codeItem.Guid)
                            },
                            Address = emailAddressEntity.Value
                        };

                        if (emailAddressEntity.IsPreferred)
                        {
                            addressDto.Preference = Dtos.EnumProperties.PersonEmailPreference.Primary;
                        }

                        emailAddressDtos.Add(addressDto);
                    }
                    else
                    {
                        _logger.Error(string.Concat("Could not convert entity to dto for email address: '", emailAddressEntity.Value, "'.  Institution ID: '", institutionId, "'"));
                    }
                }
                catch (Exception ex)
                {
                    //Don't rethrow, log issue. If there isn't an email address, cant log anything.
                    if (!string.IsNullOrEmpty(emailAddressEntity.Value))
                    {
                        _logger.Error(ex, string.Concat("Could not convert entity to dto for email address: '", emailAddressEntity.Value, "'.  Institution ID: '", institutionId, "'"));
                    }
                }
            }

            return emailAddressDtos;
        }

        private async Task<List<PersonSocialMediaDtoProperty>> GetPersonSocialMediaDtoCollection(IEnumerable<SocialMedia> socialMediaCollection, string institutionId, bool bypassCache = false)
        {
            List<Dtos.DtoProperties.PersonSocialMediaDtoProperty> socialMediaEntries = new List<Dtos.DtoProperties.PersonSocialMediaDtoProperty>();

            if ((socialMediaCollection != null) && (socialMediaCollection.Any()))
            {
                IEnumerable<Domain.Base.Entities.SocialMediaType> socialMediaTypeList = null;
                try
                {
                    socialMediaTypeList = await this.GetSocialMediaTypesAsync(bypassCache);
                }
                catch (Exception)
                {
                    //do not throw error
                    return socialMediaEntries;
                }

                if (socialMediaTypeList == null || !socialMediaTypeList.Any())
                {
                    return socialMediaEntries;
                }

                foreach (var mediaType in socialMediaCollection)
                {
                    try
                    {
                        var socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty();
                        if (mediaType.TypeCode.ToLowerInvariant() == "website")
                        {
                            string guid = "";
                            var socialMediaEntity = socialMediaTypeList.FirstOrDefault(ic => ic.Type.ToString() == mediaType.TypeCode);
                            if ((socialMediaEntity != null) && (!string.IsNullOrEmpty(guid)))
                            {
                                guid = socialMediaEntity.Guid;

                                socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                                {
                                    Type = new Dtos.DtoProperties.PersonSocialMediaType()
                                    {
                                        Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), mediaType.TypeCode.ToString()),
                                        Detail = new Dtos.GuidObject2(guid)
                                    },
                                    Address = mediaType.Handle
                                };
                            }
                            else
                            {
                                socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                                {
                                    Type = new Dtos.DtoProperties.PersonSocialMediaType()
                                    {
                                        Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), mediaType.TypeCode.ToString())
                                    },
                                    Address = mediaType.Handle
                                };
                            }
                        }
                        else
                        {
                            var socialMediaEntity = socialMediaTypeList.FirstOrDefault(ic => ic.Code == mediaType.TypeCode);
                            if (socialMediaEntity != null && !string.IsNullOrEmpty(socialMediaEntity.Guid))
                            {
                                socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                                {
                                    Type = new Dtos.DtoProperties.PersonSocialMediaType()
                                    {
                                        Category = (Dtos.SocialMediaTypeCategory)Enum.Parse(typeof(Dtos.SocialMediaTypeCategory), socialMediaEntity.Type.ToString()),
                                        Detail = new Dtos.GuidObject2(socialMediaEntity.Guid)
                                    },
                                    Address = mediaType.Handle
                                };
                            }
                            if (mediaType.IsPreferred) socialMedia.Preference = Dtos.EnumProperties.PersonPreference.Primary;

                            socialMediaEntries.Add(socialMedia);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Do not include code since we couldn't find a category
                        logger.Error(ex.Message, "could not find SocialMediaTypeCategory");
                    }
                }
            }

            return socialMediaEntries;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonPhoneDtoProperty>> GetPersonPhoneDtoCollectionAsync(IEnumerable<Domain.Base.Entities.Phone> phoneEntities, string institutionId, bool bypassCache = false)
        {
            var phoneDtos = new List<Dtos.DtoProperties.PersonPhoneDtoProperty>();
            if (phoneEntities != null && phoneEntities.Count() > 0)
            {
                IEnumerable<Domain.Base.Entities.PhoneType> phoneTypeEntities = null;
                try
                {
                    phoneTypeEntities = await GetPhoneTypesAsync(bypassCache);
                }
                catch (Exception)
                {
                    //do not throw exception
                    return phoneDtos;
                }

                if (phoneTypeEntities == null || !phoneTypeEntities.Any())
                {
                    return phoneDtos;
                }


                foreach (var phoneEntity in phoneEntities)
                {
                    string guid = "";
                    string category = "";
                    Domain.Base.Entities.PhoneType phoneTypeEntity = null;
                    try
                    {
                        phoneTypeEntity = phoneTypeEntities.FirstOrDefault(pt => pt.Code == phoneEntity.TypeCode);

                        if ((phoneTypeEntity != null) && (!string.IsNullOrEmpty(phoneTypeEntity.Guid)))
                        {
                            guid = phoneTypeEntity.Guid;
                            category = phoneTypeEntity.PhoneTypeCategory.ToString();

                            var phoneDto = new Dtos.DtoProperties.PersonPhoneDtoProperty()
                            {
                                Number = phoneEntity.Number,
                                Extension = string.IsNullOrEmpty(phoneEntity.Extension) ? null : phoneEntity.Extension,
                                Type = new Dtos.DtoProperties.PersonPhoneTypeDtoProperty()
                                {
                                    PhoneType = (Dtos.EnumProperties.PersonPhoneTypeCategory)Enum.Parse(typeof(Dtos.EnumProperties.PersonPhoneTypeCategory), category),
                                    Detail = string.IsNullOrEmpty(guid) ? null : new Dtos.GuidObject2(guid)
                                },
                                CountryCallingCode = phoneEntity.CountryCallingCode
                            };
                            if (phoneEntity.IsPreferred) phoneDto.Preference = Dtos.EnumProperties.PersonPreference.Primary;

                            phoneDtos.Add(phoneDto);
                        }
                    }
                    catch (Exception ex)
                    {
                        // do not fail if we can't find a guid from the code table or category
                        // Just exclude the phone number from the output.
                        logger.Error(ex.Message, "Could not find a guid. Phone number excluded from output.");
                    }
                }

            }
            return phoneDtos;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonAddressDtoProperty>> GetPersonAddressDtoCollectionAsync(IEnumerable<Domain.Base.Entities.Address> addressEntities, string institutionId, bool bypassCache = false)
        {
            var addressDtos = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
            if (addressEntities != null && addressEntities.Count() > 0)
            {
                // Repeate the address when we have multiple types.
                // Multiple types are separated by sub-value marks.
                IEnumerable<Domain.Base.Entities.AddressType2> addressTypes = null;
                try
                {
                    addressTypes = await GetAddressTypes2Async(bypassCache);
                }
                catch (Exception)
                {
                    // do not throw exception
                    return addressDtos;
                }

                if ((addressTypes == null || !addressTypes.Any()))
                {
                    return addressDtos;
                }

                foreach (var addressEntity in addressEntities)
                {
                    if (addressEntity.TypeCode != null && !string.IsNullOrEmpty(addressEntity.TypeCode))
                    {
                        string[] addrTypes = addressEntity.TypeCode.Split(DmiString._SM);
                        for (int i = 0; i < addrTypes.Length; i++)
                        {
                            var addrType = addrTypes[i];
                            var addressDto = new Dtos.DtoProperties.PersonAddressDtoProperty();
                            addressDto.address = new Dtos.PersonAddress() { Id = addressEntity.Guid };
                            var type = addressTypes.FirstOrDefault(at => at.Code == addrType);
                            if ((type != null) && (!string.IsNullOrEmpty(type.Guid)))
                            {
                                addressDto.Type = new Dtos.DtoProperties.PersonAddressTypeDtoProperty();
                                addressDto.Type.AddressType =
                                    (Dtos.EnumProperties.AddressType)
                                        Enum.Parse(typeof(Dtos.EnumProperties.AddressType),
                                            type.AddressTypeCategory.ToString());
                                addressDto.Type.Detail = new Dtos.GuidObject2(type.Guid);
                            }
                            if (addressEntity.IsPreferredResidence && i == 0)
                                addressDto.Preference = Dtos.EnumProperties.PersonPreference.Primary;
                            addressDto.AddressEffectiveStart = addressEntity.EffectiveStartDate;
                            addressDto.AddressEffectiveEnd = addressEntity.EffectiveEndDate;

                            addressDtos.Add(addressDto);
                        }
                    }
                }
            }
            return addressDtos;
        }

        /// <summary>
        /// Provides an integration user permission to view/get educational institutions from Colleague.
        /// </summary>
        private void CheckEducationalInstitutionsViewPermissions()
        {
            // access is ok if the current user has the view educational institutions permission
            if (!HasPermission(BasePermissionCodes.ViewEducationalInstitution))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view educational institutions.");
                throw new PermissionsException("User is not authorized to view educational institutions.");
            }
        }

        #endregion
    }
}