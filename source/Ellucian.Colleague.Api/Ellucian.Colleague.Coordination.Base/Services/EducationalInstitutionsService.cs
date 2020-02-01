// Copyright 2016-2018 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Dependency;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Coordination.Base.Adapters;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Dmi.Runtime;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.EnumProperties;
using Ellucian.Colleague.Domain.Base;

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
        public static char _SM = Convert.ToChar(DynamicArray.SM);

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

        private IEnumerable<Domain.Base.Entities.SocialMediaType> _socialMediaTypes = null;
        private async Task<IEnumerable<Domain.Base.Entities.SocialMediaType>> GetSocialMediaTypesAsync(bool bypassCache)
        {
            if (_socialMediaTypes == null)
            {
                _socialMediaTypes = await _referenceDataRepository.GetSocialMediaTypesAsync(bypassCache);
            }
            return _socialMediaTypes;
        }

        private IEnumerable<Domain.Base.Entities.EmailType> _emailTypes = null;
        private async Task<IEnumerable<Domain.Base.Entities.EmailType>> GetEmailTypesAsync(bool bypassCache)
        {
            if (_emailTypes == null)
            {
                _emailTypes = await _referenceDataRepository.GetEmailTypesAsync(bypassCache);
            }
            return _emailTypes;
        }

        private IEnumerable<string> _homeInstitutionsList = null;
        private async Task<IEnumerable<string>> GetHomeInstitutionIdList()
        {
            if (_homeInstitutionsList == null)
            {
                _homeInstitutionsList = await _referenceDataRepository.GetHomeInstitutionIdList();
            }
            return _homeInstitutionsList;
        }


        private IEnumerable<Domain.Base.Entities.AddressType2> _addressType2 = null;
        private async Task<IEnumerable<Domain.Base.Entities.AddressType2>> GetAddressTypes2Async(bool bypassCache)
        {
            if (_addressType2 == null)
            {
                _addressType2 = await _referenceDataRepository.GetAddressTypes2Async(false);
            }
            return _addressType2;
        }

        private IEnumerable<Domain.Base.Entities.PhoneType> _phoneType = null;
        private async Task<IEnumerable<Domain.Base.Entities.PhoneType>> GetPhoneTypesAsync(bool bypassCache)
        {
            if (_phoneType == null)
            {
                _phoneType = await _referenceDataRepository.GetPhoneTypesAsync(false);
            }
            return _phoneType;
        }


        /// <summary>
        /// get page of educational institutions by type
        /// </summary>
        /// <param name="offset">item number to start at</param>
        /// <param name="limit">number of items to return on page</param>
        /// <param name="type"></param>
        /// <param name="ignoreCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<EducationalInstitution>, int>> GetEducationalInstitutionsByTypeAsync(int offset, int limit, Dtos.EnumProperties.EducationalInstitutionType? type, bool bypassCache = false)
        {
            CheckEducationalInstitutionsViewPermissions();

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

            var institutions = await _institutionRepository.GetInstitutionAsync(offset, limit, typeFilter);
           
            if (institutions != null && institutions.Item1.Any())
            {
                var ids = institutions.Item1
                   .Where(x => (!string.IsNullOrEmpty(x.Id)))
                   .Select(x => x.Id).Distinct().ToList();

                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync(ids);


                var convertedInstitutionList = await ConvertInstitutionEntityListToEducationInsitutionDtoList(institutions.Item1, personGuidCollection);

                return new Tuple<IEnumerable<EducationalInstitution>, int>(convertedInstitutionList, institutions.Item2);
            }
            else
            {
                return new Tuple<IEnumerable<EducationalInstitution>, int>(new List<EducationalInstitution>(), 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<EducationalInstitution> GetEducationalInstitutionByGuidAsync(string id, bool bypassCache = false)
        {
            CheckEducationalInstitutionsViewPermissions();

            try
            {
                var institution = await _institutionRepository.GetInstitutionByGuidAsync(id);

                if (institution == null)
                {
                    throw new Exception(string.Concat("Unable to locate educational-institutions with the ID '", id, "'"));
                }

                var personGuidCollection = await _personRepository.GetPersonGuidsCollectionAsync
                  (new List<string>() { institution.Id });

                var returnedList = await ConvertInstitutionEntityListToEducationInsitutionDtoList(new List<Institution>() { institution }, personGuidCollection, bypassCache);

                if (returnedList.Any())
                {
                    return returnedList.First();
                }
                else
                {
                    throw new Exception(string.Concat("Unable to locate educational-institutions with the ID '", id, "'"));
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Unable to locate educational-institutions with the ID '", id, "'"));
            }
        }

       
        /// <summary>
        /// Convert ienumerable of institution entities to ienumerable of educational-institutions dtos 
        /// </summary>
        /// <param name="institutions">ienumerable of institution entities</param>
        /// <returns>ienumerable of educational-institutions dtos</returns>
        private async Task<IEnumerable<EducationalInstitution>> ConvertInstitutionEntityListToEducationInsitutionDtoList(IEnumerable<Institution> institutions,
            Dictionary<string, string> personGuidCollection, bool bypassCache = false)
        {
            var returnEducationInsitutionList = new List<EducationalInstitution>();
            bool errorsThrown = false;
            var errorStringBuilder = new StringBuilder();
            //email types for use in conversion adapter
            var emailTypes = await GetEmailTypesAsync(bypassCache);
            var emailAdapter = new PersonIntgEmailAddressEntityToDtoAdapter(_iAdapterRegistry, _logger);

            var socialMediaTypesList = await GetSocialMediaTypesAsync(bypassCache);
            var socialMediaAdapter = _iAdapterRegistry.GetAdapter<Tuple<IEnumerable<Domain.Base.Entities.SocialMedia>, IEnumerable<Domain.Base.Entities.SocialMediaType>>,
                        IEnumerable<Dtos.DtoProperties.PersonSocialMediaDtoProperty>>();

            foreach (var institution in institutions)
            {
                var educationInsitution = new EducationalInstitution();

                try
                {
                    if (personGuidCollection == null)
                    {
                        throw new KeyNotFoundException(string.Concat("educationInsitution, ", "Unable to locate guid for : '", institution.Id, "'"));
                    }
                    // educationInsitution.Id = await _personRepository.GetPersonGuidFromIdAsync(institution.Id);
                    var educationInsitutionGuid = string.Empty;
                    personGuidCollection.TryGetValue(institution.Id, out educationInsitutionGuid);
                    if (string.IsNullOrEmpty(educationInsitutionGuid))
                    {
                        throw new KeyNotFoundException(string.Concat("educationInsitution, ", "Unable to locate guid for : '", institution.Id, "'"));
                    }
                    educationInsitution.Id = educationInsitutionGuid;

                    educationInsitution.Title = institution.Name;
                    switch (institution.InstitutionType)
                    {
                        case InstType.College:
                            educationInsitution.Type = EducationalInstitutionType.PostSecondarySchool;
                            break;
                        case InstType.HighSchool:
                            educationInsitution.Type = EducationalInstitutionType.SecondarySchool;
                            break;
                        default:
                            throw new Exception(string.Concat("The educational-institution with the ID ", educationInsitution.Id, " does not have a valid type."));
                    }

                    var homeInstitutionsList = await GetHomeInstitutionIdList();
                    if (homeInstitutionsList.Contains(institution.Id))
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

                    // var tuplePerson = await _personRepository.GetPersonIntegrationData2Async(institution.Id);
                    // var emailEntities = tuplePerson.Item1;
                    // var phoneEntities = tuplePerson.Item2;
                    // var addressEntities = tuplePerson.Item3;
                    // var socialMediaEntities = tuplePerson.Item4;
                    var emailEntities = institution.EmailAddresses;
                    var phoneEntities = institution.Phones;
                    var addressEntities = institution.Addresses;
                    var socialMediaEntities = institution.SocialMedia;

                    var emailDtoReturn = emailAdapter.MapToType(emailEntities, emailTypes);
                    if (emailDtoReturn != null && emailDtoReturn.Any())
                    {
                        educationInsitution.EmailAddresses = emailDtoReturn;
                    }

                    var addressDtoReturn = await GetAddressesAsync(addressEntities, bypassCache);
                    if (addressDtoReturn != null && addressDtoReturn.Any())
                    {
                        educationInsitution.Addresses = addressDtoReturn;
                    }

                    var phoneDtoReturn = await GetPhonesAsync(phoneEntities);
                    if (phoneDtoReturn != null && phoneDtoReturn.Any())
                    {
                        educationInsitution.Phones = phoneDtoReturn;
                    }

                    var socialTuple = new Tuple <IEnumerable<Domain.Base.Entities.SocialMedia>, IEnumerable<Domain.Base.Entities.SocialMediaType>>
                        (socialMediaEntities, socialMediaTypesList);

                    var socialMediaDtoReturn = socialMediaAdapter.MapToType(socialTuple);
                    if (socialMediaDtoReturn != null && socialMediaDtoReturn.Any())
                    {
                        educationInsitution.SocialMedia = socialMediaDtoReturn;
                    }

                    returnEducationInsitutionList.Add(educationInsitution);
                }
                catch (Exception exception)
                {
                    errorStringBuilder.Append(exception.Message);
                    errorStringBuilder.AppendLine();
                    errorsThrown = true;
                }
            }

            if (errorsThrown)
            {
                throw new Exception(errorStringBuilder.ToString());
            }

            return returnEducationInsitutionList;
        }

      
        private async Task<IEnumerable<Dtos.DtoProperties.PersonPhoneDtoProperty>> GetPhonesAsync(IEnumerable<Domain.Base.Entities.Phone> phoneEntities, bool bypassCache = false)
        {
            var phoneDtos = new List<Dtos.DtoProperties.PersonPhoneDtoProperty>();
            if (phoneEntities != null && phoneEntities.Count() > 0)
            {
                var phoneTypeEntities = await GetPhoneTypesAsync(bypassCache);

                foreach (var phoneEntity in phoneEntities)
                {
                    string guid = "";
                    string category = "";
                    Domain.Base.Entities.PhoneType phoneTypeEntity = null;
                    try
                    {
                        
                        if (phoneTypeEntities != null)
                            phoneTypeEntity = phoneTypeEntities.FirstOrDefault(pt => pt.Code == phoneEntity.TypeCode);

                        if (phoneTypeEntity == null)
                        {
                            continue;
                        }
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
                    catch
                    {
                        // do not fail if we can't find a guid from the code table or category
                        // Just exclude the phone number from the output.
                    }
                }
            }
            return phoneDtos;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonAddressDtoProperty>> GetAddressesAsync(IEnumerable<Domain.Base.Entities.Address> addressEntities, bool bypassCache = false)
        {
            var addressDtos = new List<Dtos.DtoProperties.PersonAddressDtoProperty>();
            if (addressEntities != null && addressEntities.Count() > 0)
            {
                foreach (var addressEntity in addressEntities)
                {
                    if (addressEntity != null && addressEntity.TypeCode != null && !string.IsNullOrEmpty(addressEntity.TypeCode))
                    {
                        // Repeate the address when we have multiple types.
                        // Multiple types are separated by sub-value marks.
                        var addressTypes = await GetAddressTypes2Async(bypassCache);
                        string[] addrTypes = addressEntity.TypeCode.Split(_SM);
                        for (int i = 0; i < addrTypes.Length; i++)
                        {
                            var addrType = addrTypes[i];
                            var addressDto = new Dtos.DtoProperties.PersonAddressDtoProperty();
                            addressDto.address = new Dtos.PersonAddress() { Id = addressEntity.Guid };
                            var type = addressTypes.FirstOrDefault(at => at.Code == addrType);
                            if (type != null)
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
    }
}