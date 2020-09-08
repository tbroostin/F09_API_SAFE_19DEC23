// Copyright 2014-2019 Ellucian Company L.P. and its affiliates.

using System;
using System.Collections.Generic;
using System.Linq;
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Web.Dependency;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;
using slf4net;
using System.Threading.Tasks;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Services;
using Ellucian.Colleague.Domain.Exceptions;
using Ellucian.Colleague.Dtos;
using Ellucian.Colleague.Dtos.DtoProperties;
using Ellucian.Colleague.Dtos.EnumProperties;
using CitizenshipStatusType = Ellucian.Colleague.Domain.Base.Entities.CitizenshipStatusType;
using CredentialType = Ellucian.Colleague.Dtos.EnumProperties.CredentialType;
using EthnicityType = Ellucian.Colleague.Domain.Base.Entities.EthnicityType;
using IdentityDocumentTypeCategory = Ellucian.Colleague.Domain.Base.Entities.IdentityDocumentTypeCategory;
using PersonName = Ellucian.Colleague.Domain.Base.Entities.PersonName;
using PersonRoleType = Ellucian.Colleague.Domain.Base.Entities.PersonRoleType;
using PersonVisa = Ellucian.Colleague.Domain.Base.Entities.PersonVisa;
using PrivacyStatusType = Ellucian.Colleague.Domain.Base.Entities.PrivacyStatusType;
using RaceType = Ellucian.Colleague.Domain.Base.Entities.RaceType;
using VisaTypeCategory = Ellucian.Colleague.Domain.Base.Entities.VisaTypeCategory;
using System.Text;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Dmi.Runtime;
using System.Diagnostics;
using Ellucian.Web.Http.Exceptions;

namespace Ellucian.Colleague.Coordination.Base.Services
{
    /// <summary>
    /// Services for person objects.
    /// </summary>
    [RegisterType]
    public class PersonService : BaseCoordinationService, IPersonService
    {
        private readonly IPersonRepository _personRepository;
        private readonly IPersonBaseRepository _personBaseRepository;
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly IProxyRepository _proxyRepository;
        public static char _SM = Convert.ToChar(DynamicArray.SM);

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="personRepository">The person repository.</param>
        /// <param name="referenceDataRepository">The reference data repository.</param>
        /// <param name="relationshipRepository">The relationship repository.</param>
        /// <param name="profileRepository">The profile repository.</param>
        /// <param name="configurationRepository">The configuration repository</param>
        /// <param name="proxyRepository">The proxy repository</param>
        /// <param name="currentUserFactory">The current user factory.</param>
        /// <param name="roleRepository">The role repository.</param>
        /// <param name="logger">The logger.</param>
        public PersonService(IAdapterRegistry adapterRegistry,
            IPersonRepository personRepository,
            IPersonBaseRepository personBaseRepository,
            IReferenceDataRepository referenceDataRepository,
            IProfileRepository profileRepository,
            IConfigurationRepository configurationRepository,
            IRelationshipRepository relationshipRepository,
            IProxyRepository proxyRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger, configurationRepository: configurationRepository)
        {
            _personRepository = personRepository;
            _personBaseRepository = personBaseRepository;
            _referenceDataRepository = referenceDataRepository;
            _profileRepository = profileRepository;
            _relationshipRepository = relationshipRepository;
            _configurationRepository = configurationRepository;
            _proxyRepository = proxyRepository;
        }



        #region Get all reference data

        public IEnumerable<Domain.Base.Entities.PrivacyStatus> _personPrivacyStatuses { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.PrivacyStatus>> GetPrivacyStatusesAsync(bool bypassCache)
        {
            if (_personPrivacyStatuses == null)
            {
                _personPrivacyStatuses = await _referenceDataRepository.GetPrivacyStatusesAsync(bypassCache);
            }
            return _personPrivacyStatuses;
        }


        public IEnumerable<Domain.Base.Entities.PersonNameTypeItem> _personNameTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.PersonNameTypeItem>> GetPersonNameTypesAsync(bool bypassCache)
        {
            if (_personNameTypes == null)
            {
                _personNameTypes = await _referenceDataRepository.GetPersonNameTypesAsync(bypassCache);
            }
            return _personNameTypes;
        }

        public IEnumerable<Domain.Base.Entities.MaritalStatus> _maritalStatuses { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.MaritalStatus>> GetMaritalStatusesAsync(bool bypassCache = false)
        {
            if (_maritalStatuses == null)
            {
                _maritalStatuses = await _referenceDataRepository.GetMaritalStatusesAsync(bypassCache);
            }
            return _maritalStatuses;
        }

        public IEnumerable<Domain.Base.Entities.GenderIdentityType> _genderIdentityTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.GenderIdentityType>> GetGenderIdentityTypesAsync(bool bypassCache = false)
        {
            if (_genderIdentityTypes == null)
            {
                _genderIdentityTypes = await _referenceDataRepository.GetGenderIdentityTypesAsync(bypassCache);
            }
            return _genderIdentityTypes;
        }

        public IEnumerable<Domain.Base.Entities.PersonalPronounType> _personalPronounTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.PersonalPronounType>> GetPersonalPronounTypesAsync(bool bypassCache = false)
        {
            if (_personalPronounTypes == null)
            {
                _personalPronounTypes = await _referenceDataRepository.GetPersonalPronounTypesAsync(bypassCache);
            }
            return _personalPronounTypes;
        }

        public IEnumerable<Domain.Base.Entities.AltIdTypes> _alternateIdTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.AltIdTypes>> GetAlternateIdTypesAsync(bool bypassCache = false)
        {
            if (_alternateIdTypes == null)
            {
                _alternateIdTypes = await _referenceDataRepository.GetAlternateIdTypesAsync(bypassCache);
            }
            return _alternateIdTypes;
        }
        public IEnumerable<Domain.Base.Entities.CitizenshipStatus> _citizenshipStatuses { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.CitizenshipStatus>> GetCitizenshipStatusesAsync(bool bypassCache)
        {
            if (_citizenshipStatuses == null)
            {
                _citizenshipStatuses = await _referenceDataRepository.GetCitizenshipStatusesAsync(bypassCache);
            }
            return _citizenshipStatuses;
        }

        public IEnumerable<Domain.Base.Entities.VisaTypeGuidItem> _visaTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.VisaTypeGuidItem>> GetVisaTypesAsync(bool bypassCache)
        {
            if (_visaTypes == null)
            {
                _visaTypes = await _referenceDataRepository.GetVisaTypesAsync(bypassCache);
            }
            return _visaTypes;
        }

        public IEnumerable<Domain.Base.Entities.Race> _races { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.Race>> GetRacesAsync(bool bypassCache)
        {
            if (_races == null)
            {
                _races = await _referenceDataRepository.GetRacesAsync(bypassCache);
            }
            return _races;
        }

        public IEnumerable<Domain.Base.Entities.EmailType> _emailTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.EmailType>> GetEmailTypesAsync(bool bypassCache)
        {
            if (_emailTypes == null)
            {
                _emailTypes = await _referenceDataRepository.GetEmailTypesAsync(bypassCache);
            }
            return _emailTypes;
        }

        public IEnumerable<Domain.Base.Entities.Chapter> _chapters { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.Chapter>> GetChaptersAsync(bool bypassCache)
        {
            if (_chapters == null)
            {
                _chapters = await _referenceDataRepository.GetChaptersAsync(bypassCache);
            }
            return _chapters;
        }

        public IEnumerable<Domain.Base.Entities.Country> _countries { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.Country>> GetCountryCodesAsync(bool bypassCache)
        {
            if (_countries == null)
            {
                _countries = await _referenceDataRepository.GetCountryCodesAsync(bypassCache);
            }
            return _countries;
        }

        public IEnumerable<Domain.Base.Entities.State> _states { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.State>> GetStateCodesAsync(bool bypassCache)
        {
            if (_states == null)
            {
                _states = await _referenceDataRepository.GetStateCodesAsync(bypassCache);
            }
            return _states;
        }

        public IEnumerable<Domain.Base.Entities.IdentityDocumentType> _identityDocumentType { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.IdentityDocumentType>> GetIdentityDocumentTypesAsync(bool bypassCache)
        {
            if (_identityDocumentType == null)
            {
                _identityDocumentType = await _referenceDataRepository.GetIdentityDocumentTypesAsync(bypassCache);
            }
            return _identityDocumentType;
        }

        public IEnumerable<Domain.Base.Entities.SocialMediaType> _socialMediaTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.SocialMediaType>> GetSocialMediaTypesAsync(bool bypassCache)
        {
            if (_socialMediaTypes == null)
            {
                _socialMediaTypes = await _referenceDataRepository.GetSocialMediaTypesAsync(bypassCache);
            }
            return _socialMediaTypes;
        }


        public IEnumerable<Domain.Base.Entities.Denomination> _denominations { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.Denomination>> GetDenominationsAsync(bool bypassCache)
        {
            if (_denominations == null)
            {
                _denominations = await _referenceDataRepository.GetDenominationsAsync(bypassCache);
            }
            return _denominations;
        }

        public IEnumerable<Domain.Base.Entities.Ethnicity> _ethnicities { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.Ethnicity>> GetEthnicitiesAsync(bool bypassCache)
        {
            if (_ethnicities == null)
            {
                _ethnicities = await _referenceDataRepository.GetEthnicitiesAsync(bypassCache);
            }
            return _ethnicities;
        }

        public IEnumerable<Domain.Base.Entities.AddressType2> _addressTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.AddressType2>> GetAddressTypes2Async(bool bypassCache)
        {
            if (_addressTypes == null)
            {
                _addressTypes = await _referenceDataRepository.GetAddressTypes2Async(bypassCache);
            }
            return _addressTypes;
        }

        public IEnumerable<Domain.Base.Entities.PhoneType> _phoneTypes { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.PhoneType>> GetPhoneTypesAsync(bool bypassCache)
        {
            if (_phoneTypes == null)
            {
                _phoneTypes = await _referenceDataRepository.GetPhoneTypesAsync(bypassCache);
            }
            return _phoneTypes;
        }

        public IEnumerable<Domain.Base.Entities.Interest> _interests { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.Interest>> GetInterestsAsync(bool bypassCache)
        {
            if (_interests == null)
            {
                _interests = await _referenceDataRepository.GetInterestsAsync(bypassCache);
            }
            return _interests;
        }

        public IEnumerable<Domain.Base.Entities.County> _counties { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.County>> GetCountiesAsync(bool bypassCache)
        {
            if (_counties == null)
            {
                _counties = await _referenceDataRepository.GetCountiesAsync(bypassCache);
            }
            return _counties;
        }

        public IEnumerable<Domain.Base.Entities.MilStatuses> _milStatuses { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.MilStatuses>> GetMilStatusesAsync(bool bypassCache)
        {
            if (_milStatuses == null)
            {
                _milStatuses = await _referenceDataRepository.GetMilStatusesAsync(bypassCache);
            }
            return _milStatuses;
        }

        public IEnumerable<Domain.Base.Entities.Place> _places { get; set; }
        private async Task<IEnumerable<Domain.Base.Entities.Place>> GetPlacesAsync(bool bypassCache)
        {
            if (_places == null)
            {
                _places = await _referenceDataRepository.GetPlacesAsync(bypassCache);
            }
            return _places;
        }




        #endregion

        #region Get Person Methods

        /// <summary>
        /// Gets paged persons credentials
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonCredential>, int>> GetAllPersonCredentialsAsync(int offset, int limit, bool bypassCache)
        {
            List<Dtos.PersonCredential> personCredentials = new List<PersonCredential>();

            //verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            Tuple<IEnumerable<string>, int> personIds = await _personRepository.GetPersonGuidsAsync(offset, limit, bypassCache);
            if (personIds == null || personIds.Item1 == null)
            {
                return new Tuple<IEnumerable<Dtos.PersonCredential>, int>(personCredentials, 0);
            }

            var personEntities = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(personIds.Item1);

            foreach (var personEntity in personEntities)
            {
                var personCredentialDtos = new Dtos.PersonCredential();
                personCredentialDtos.Id = personEntity.Guid;
                personCredentialDtos.Credentials = await GetPersonCredentials(personEntity);
                personCredentials.Add(personCredentialDtos);
            }
            return personCredentials.Any() ? new Tuple<IEnumerable<Dtos.PersonCredential>, int>(personCredentials, personIds.Item2) : null;
        }

        /// <summary>
        /// Get a person credentials by guid without using a cache.
        /// </summary>
        /// <param name="guids">A global identifier of a person.</param>
        /// <returns>The <see cref="Dtos.PersonCredential">person credentials</see></returns>
        public async Task<Dtos.PersonCredential> GetPersonCredentialByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id", "Must provide id to get person credential");

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            var personEntity = await _personRepository.GetPersonByGuidNonCachedAsync(id);

            if (personEntity == null)
                throw new KeyNotFoundException("Person id not found in repository");

            var personDtos = new Dtos.PersonCredential();
            personDtos.Id = id;
            personDtos.Credentials = await GetPersonCredentials(personEntity);

            return personDtos;
        }

        /// <summary>
        /// Gets paged persons credentials
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonCredential2>, int>> GetAllPersonCredentials2Async(int offset, int limit, bool bypassCache)
        {
            try
            {
                //verify the user has the permission to view any person
                CheckUserPersonViewPermissions();

                var personIds = await _personRepository.GetPersonGuidsAsync(offset, limit, bypassCache);
                if (personIds == null || personIds.Item1 == null)
                {
                    return new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(new List<PersonCredential2>(), 0);
                }

                await this.GetPersonPins(personIds.Item1.ToArray());

                var personEntities = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(personIds.Item1);

                var personCredentials = new List<PersonCredential2>();

                foreach (var personEntity in personEntities)
                {
                    var personCredentialDtos = new Dtos.PersonCredential2();
                    personCredentialDtos.Id = personEntity.Guid;
                    personCredentialDtos.Credentials = await GetPersonCredentials2(personEntity);
                    personCredentials.Add(personCredentialDtos);
                }
                return personCredentials.Any() ? new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(personCredentials, personIds.Item2) :
                                                 new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(new List<PersonCredential2>(), 0);
            }
            catch (RepositoryException rex)
            {
                logger.Error(rex.Message);
                throw rex;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw ex;
            }
        }

        /// <summary>
        /// Gets paged persons credentials
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonCredential2>, int>> GetAllPersonCredentials3Async(int offset, int limit,
            PersonCredential2 personFilterCredentials, bool bypassCache)
        {
            try
            {
                //verify the user has the permission to view any person
                CheckUserPersonViewPermissions();

                PersonFilterCriteria personFilterCriteria = new PersonFilterCriteria();

                if (personFilterCredentials.Credentials != null && personFilterCredentials.Credentials.Any())
                {
                    List<Tuple<string, string>> creds = new List<Tuple<string, string>>();
                    personFilterCredentials.Credentials.ToList().ForEach(i =>
                    {
                        var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                        Tuple<string, string> tuple = new Tuple<string, string>(i.Type.ToString(), tempValue);
                        creds.Add(tuple);
                    });
                    personFilterCriteria.Credentials = creds;
                }

                if (personFilterCriteria != null && personFilterCriteria.Credentials == null)
                {
                    personFilterCriteria = null;
                }

                var personIds = await _personRepository.GetFilteredPerson2GuidsAsync(offset, limit, bypassCache, personFilterCriteria, null);

                var personCredentials = new List<PersonCredential2>();
                if (personIds != null && personIds.Item1 != null && personIds.Item1.Any())
                {
                    await this.GetPersonPins(personIds.Item1.ToArray());

                    var personEntities = await _personRepository.GetPersonCredentialsIntegrationByGuidNonCachedAsync(personIds.Item1);

                    foreach (var personEntity in personEntities)
                    {
                        var personCredentialDtos = new Dtos.PersonCredential2();
                        personCredentialDtos.Id = personEntity.Guid;
                        personCredentialDtos.Credentials = await GetPersonCredentials2(personEntity);
                        personCredentials.Add(personCredentialDtos);
                    }
                }
                return personCredentials.Any() ? new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(personCredentials, personIds.Item2) :
                                                 new Tuple<IEnumerable<Dtos.PersonCredential2>, int>(new List<PersonCredential2>(), 0);
            }
            catch (RepositoryException rex)
            {
                logger.Error(rex.Message);
                throw rex;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// Gets paged persons credentials
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Tuple<IEnumerable<Dtos.PersonCredential3>, int>> GetAllPersonCredentials4Async(int offset, int limit,
            PersonCredential3 personFilterCredentials, bool bypassCache)
        {
            try
            {
                //verify the user has the permission to view any person
                CheckUserPersonViewPermissions();

                PersonFilterCriteria personFilterCriteria = new PersonFilterCriteria();

                if (personFilterCredentials.Credentials != null && personFilterCredentials.Credentials.Any())
                {
                    List<Tuple<string, string>> creds = new List<Tuple<string, string>>();
                    personFilterCredentials.Credentials.ToList().ForEach(i =>
                    {
                        var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                        Tuple<string, string> tuple = new Tuple<string, string>(i.Type.ToString(), tempValue);
                        creds.Add(tuple);
                    });
                    personFilterCriteria.Credentials = creds;
                }

                if (personFilterCredentials.AlternativeCredentials != null && personFilterCredentials.AlternativeCredentials.Any())
                {
                    var altIdTypesList = await GetAlternateIdTypesAsync(false);
                    List<Tuple<string, string>> altCreds = new List<Tuple<string, string>>();
                    personFilterCredentials.AlternativeCredentials.ToList().ForEach(i => 
                    {
                        if (i.Type != null && !string.IsNullOrEmpty(i.Type.Id))
                        {
                            var altCredTypeEntity = altIdTypesList.FirstOrDefault(e => e.Guid.Equals(i.Type.Id, StringComparison.OrdinalIgnoreCase));
                            if (altCredTypeEntity != null)
                            {
                                var altCredTypeCode = altCredTypeEntity.Code;
                                if (!string.IsNullOrEmpty(altCredTypeCode))
                                {
                                    var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                                    Tuple<string, string> tuple = new Tuple<string, string>(altCredTypeCode, tempValue);
                                    altCreds.Add(tuple);
                                }
                            }
                        }
                        else
                        {
                            var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                            Tuple<string, string> tuple = new Tuple<string, string>("", tempValue);
                            altCreds.Add(tuple);
                        }
                    });
                    if (altCreds == null || !altCreds.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.PersonCredential3>, int>(new List<PersonCredential3>(), 0);
                    }
                    personFilterCriteria.AlternativeCredentials = altCreds;
                }

                if (personFilterCriteria != null && personFilterCriteria.Credentials == null && personFilterCriteria.AlternativeCredentials == null)
                {
                    personFilterCriteria = null;
                }

                var personIds = await _personRepository.GetFilteredPerson2GuidsAsync(offset, limit, bypassCache, personFilterCriteria, null);

                var personCredentials = new List<PersonCredential3>();
                if (personIds != null && personIds.Item1 != null && personIds.Item1.Any())
                {
                    await this.GetPersonPins(personIds.Item1.ToArray());

                    var personEntities = await _personRepository.GetPersonCredentialsIntegrationByGuidNonCachedAsync(personIds.Item1);

                    foreach (var personEntity in personEntities)
                    {
                        var personCredentialDtos = new Dtos.PersonCredential3();
                        personCredentialDtos.Id = personEntity.Guid;
                        personCredentialDtos.Credentials = GetPersonCredentials5(personEntity);

                        //get alternative credentials
                        if (personEntity.PersonAltIds != null && personEntity.PersonAltIds.Any())
                        {
                            var altCred = await GetAlternativeCredentials(personEntity.Id, personEntity.PersonAltIds, bypassCache);
                            if (altCred != null && altCred.Any())
                                personCredentialDtos.AlternativeCredentials = altCred;
                        }
                        personCredentials.Add(personCredentialDtos);

                    }
                }
                return personCredentials.Any() ? new Tuple<IEnumerable<Dtos.PersonCredential3>, int>(personCredentials, personIds.Item2) :
                                                 new Tuple<IEnumerable<Dtos.PersonCredential3>, int>(new List<PersonCredential3>(), 0);
            }
            catch (RepositoryException rex)
            {
                logger.Error(rex.Message);
                throw rex;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                throw ex;
            }
        }


        /// <summary>
        /// Get a person credentials by guid without using a cache.
        /// </summary>
        /// <param name="guids">A global identifier of a person.</param>
        /// <returns>The <see cref="Dtos.PersonCredential">person credentials</see></returns>
        public async Task<Dtos.PersonCredential2> GetPersonCredential2ByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id", "Must provide id to get person credential");

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            var personEntity = await _personRepository.GetPersonByGuidNonCachedAsync(id);

            if (personEntity == null)
                throw new KeyNotFoundException("Person id not found in repository");

            await this.GetPersonPins(new string[] { personEntity.Guid });

            var personDtos = new Dtos.PersonCredential2();
            personDtos.Id = id;
            personDtos.Credentials = await GetPersonCredentials2(personEntity);

            return personDtos;
        }

        /// <summary>
        /// Get a person credentials by guid without using a cache.
        /// </summary>
        /// <param name="guids">A global identifier of a person.</param>
        /// <returns>The <see cref="Dtos.PersonCredential2">person credentials</see></returns>
        public async Task<Dtos.PersonCredential2> GetPersonCredential3ByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id", "Must provide id to get person credential");

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            var personEntity = await _personRepository.GetPersonCredentialByGuidNonCachedAsync(id);

            if (personEntity == null)
                throw new KeyNotFoundException("Person id not found in repository");

            await this.GetPersonPins(new string[] { personEntity.Guid });

            var personDtos = new Dtos.PersonCredential2();
            personDtos.Id = id;
            personDtos.Credentials = await GetPersonCredentials2(personEntity);

            return personDtos;
        }

        /// <summary>
        /// Get a person credentials by guid without using a cache.
        /// </summary>
        /// <param name="guids">A global identifier of a person.</param>
        /// <returns>The <see cref="Dtos.PersonCredential2">person credentials</see></returns>
        public async Task<Dtos.PersonCredential3> GetPersonCredential4ByGuidAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id", "Must provide id to get person credential");

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            var personEntity = await _personRepository.GetPersonCredentialByGuidNonCachedAsync(id);

            if (personEntity == null)
                throw new KeyNotFoundException("Person id not found in repository");

            await this.GetPersonPins(new string[] { personEntity.Guid });

            var personDtos = new Dtos.PersonCredential3();
            personDtos.Id = id;
            personDtos.Credentials = GetPersonCredentials5(personEntity);
            //get alternative credentials
            if (personEntity.PersonAltIds != null && personEntity.PersonAltIds.Any())
            {
                var altCred = await GetAlternativeCredentials(personEntity.Id, personEntity.PersonAltIds, false);
                if (altCred != null && altCred.Any())
                    personDtos.AlternativeCredentials = altCred;
            }


            return personDtos;
        }

        /// <summary>
        /// Gets a collection of CredentialDtoProperty2 from a Person domain entity
        /// </summary>
        /// <param name="person">A <see cref="Domain.Base.Entities.Person"> Person domain entity</param>
        /// <returns>A collection of <see cref="Dtos.DtoProperties.CredentialDtoProperty2"> CredentialDtoProperty2</returns>
        private async Task<IEnumerable<Dtos.DtoProperties.CredentialDtoProperty2>> GetPersonCredentials2(
            Domain.Base.Entities.Person person)
        {
            // Colleague Person ID
            var credentials = new List<Dtos.DtoProperties.CredentialDtoProperty2>()
            {
                new Dtos.DtoProperties.CredentialDtoProperty2()
                {
                    Type = Dtos.EnumProperties.CredentialType2.ColleaguePersonId,
                    Value = person.Id
                }
            };
            // Elevate ID
            if (person.PersonAltIds != null && person.PersonAltIds.Any())
            {
                var elevPersonAltIdList =
                    person.PersonAltIds.Where(
                        a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                if (elevPersonAltIdList != null && elevPersonAltIdList.Any())
                {
                    foreach (var elevPersonAltId in elevPersonAltIdList)
                    {
                        if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                        {
                            credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty2()
                            {
                                Type = Dtos.EnumProperties.CredentialType2.ElevateID,
                                Value = elevPersonAltId.Id
                            });
                        }
                    }
                }
            }
            // SSN
            if (!string.IsNullOrEmpty(person.GovernmentId))
            {
                var type = Dtos.EnumProperties.CredentialType2.Sin;
                var countryCode = await _personRepository.GetHostCountryAsync();
                if (countryCode.Equals("USA", StringComparison.OrdinalIgnoreCase))
                {
                    type = Dtos.EnumProperties.CredentialType2.Ssn;
                }
                credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty2()
                {
                    Type = type,
                    Value = person.GovernmentId
                });
            }
            //PERSON.PIN
            if (_personPins != null)
            {
                var personPinEntity = _personPins.FirstOrDefault(i => i.PersonId.Equals(person.Id, StringComparison.OrdinalIgnoreCase));
                if ((personPinEntity != null) && (!string.IsNullOrEmpty(personPinEntity.PersonPinUserId)))
                {
                    credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty2()
                    {
                        Type = Dtos.EnumProperties.CredentialType2.ColleagueUserName,
                        Value = personPinEntity.PersonPinUserId
                    });
                }
            }
            return credentials;
        }

        /// <summary>
        /// Get all person pins
        /// </summary>
        public IEnumerable<PersonPin> _personPins { get; set; }
        private async Task<IEnumerable<PersonPin>> GetPersonPins(string[] guids)
        {
            if ((_personPins == null) && (guids != null) && (guids.Any()))
            {
                _personPins = await _personRepository.GetPersonPinsAsync(guids);
            }
            return _personPins;
        }

        /// <summary>
        /// Get profile information for a person
        /// </summary>
        /// <param name="id">Person Id</param>
        /// <returns><see cref="Profile">Profile</see> DTO object</returns>
        public async Task<Dtos.Base.Profile> GetProfileAsync(string personId, bool useCache = true)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person's id is required to retrieve profile information");
            }

            // For now, profile information may only be viewed by that person.
            if (!(await UserCanViewProfileForPerson(personId)))
            {
                string message = CurrentUser + " cannot view profile for person " + personId;
                logger.Info(message);
                throw new PermissionsException();
            }

            Domain.Base.Entities.Profile profileEntity = await _profileRepository.GetProfileAsync(personId, useCache);
            if (profileEntity == null)
            {
                throw new Exception("Profile information could not be retrieved for person " + personId);
            }

            var profileDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.Profile, Dtos.Base.Profile>();
            Dtos.Base.Profile profileDto = profileDtoAdapter.MapToType(profileEntity);

            return profileDto;
        }

        public async Task<Dtos.Base.PersonProxyDetails> GetPersonProxyDetailsAsync(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "A person's id is required to retrieve profile information");
            }
            if (!(await UserCanViewProfileForPerson(personId)))
            {
                string message = CurrentUser + " cannot view profile for person " + personId;
                logger.Info(message);
                throw new PermissionsException();
            }
            var person = await GetProfileAsync(personId, false);
            if (person == null)
            {
                throw new Exception("Information could not be retrieved for person " + personId);
            }
            if (person.IsDeceased)
            {
                throw new Exception("Cannot retrieve proxy details for a deceased person");
            }
            var preferredName = person.PreferredName;

            var proxyConfig = await _proxyRepository.GetProxyConfigurationAsync();
            if (proxyConfig == null)
            {
                throw new Exception("Could not retrieve proxy configuration");
            }
            var emailFromHierarchy = await _personRepository.GetEmailAddressFromHierarchyAsync(personId, proxyConfig.ProxyEmailAddressHierarchy);
            var emailAddress = person.PreferredEmailAddress;
            if (emailFromHierarchy != null)
            {
                emailAddress = emailFromHierarchy.Value;
            }
            Domain.Base.Entities.PersonProxyDetails personProxyDetailsEntity = new Domain.Base.Entities.PersonProxyDetails(personId, preferredName, emailAddress);
            var personProxyDetailsDtoAdapter = _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonProxyDetails, Dtos.Base.PersonProxyDetails>();
            Dtos.Base.PersonProxyDetails personProxyDetailsDto = personProxyDetailsDtoAdapter.MapToType(personProxyDetailsEntity);

            return personProxyDetailsDto;
        }

        #endregion

        #region Get HEDM V6 Person Methods

        /// <summary>
        /// Get a person by guid without using a cache.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <returns>The <see cref="Dtos.Person2">person</see></returns>
        public async Task<Dtos.Person2> GetPerson2ByGuidNonCachedAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a person");

            // get the person ID associated with the incoming guid
            var personId = await _personRepository.GetPersonIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(personId))
                throw new KeyNotFoundException("Person ID associated to guid '" + guid + "' not found in repository");

            // verify the user has the permission to view themselves or any person
            CheckUserPersonViewPermissions(personId);

            var personEntity = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(guid);

            if (personEntity == null)
                throw new KeyNotFoundException("Person '" + guid + "' not found in repository");
            if ((!string.IsNullOrEmpty(personEntity.PersonCorpIndicator)) &&
                    (personEntity.PersonCorpIndicator.Equals("Y", StringComparison.InvariantCultureIgnoreCase)))
                throw new KeyNotFoundException(string.Format("'{0}' belongs to organization or educational institution. Can not be retrieved.", guid));

            return await ConvertPerson2EntityToDtoAsync(personEntity, false);
        }

        /// <summary>
        /// Get a list of persons by guid without using a cache.
        /// </summary>
        /// <param name="guids">List of person guids</param>
        /// <returns>List of <see cref="Dtos.Person2">persons</see></returns>
        public async Task<IEnumerable<Dtos.Person2>> GetPerson2ByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get persons");

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            var personEntities = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(guids);

            if (personEntities == null || personEntities.Count() == 0)
                throw new KeyNotFoundException("Person guids not found in repository");

            var personDtos = new List<Dtos.Person2>();
            foreach (var personEntity in personEntities)
            {
                personDtos.Add(await ConvertPerson2EntityToDtoAsync(personEntity, false));
            }

            return personDtos;
        }

        /// <summary>
        /// Get a list of persons by guid without using a cache.
        /// </summary>
        /// <param name="guids">List of person guids</param>
        /// <returns>List of <see cref="Dtos.Person3">persons</see></returns>
        public async Task<IEnumerable<Dtos.Person3>> GetPerson3ByGuidNonCachedAsync(IEnumerable<string> guids)
        {
            if (guids == null || guids.Count() == 0)
                throw new ArgumentNullException("guids", "Must provide guids to get persons");

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            var personEntities = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(guids);

            if (personEntities == null || personEntities.Count() == 0)
                throw new KeyNotFoundException("Person guids not found in repository");

            var personDtos = new List<Dtos.Person3>();
            foreach (var personEntity in personEntities)
            {
                personDtos.Add(await ConvertPerson3EntityToDtoAsync(personEntity, false));
            }

            return personDtos;
        }

        /// <summary>
        /// Get a person by guid using a cache.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="bypassCache"></param>
        /// <returns>The <see cref="Dtos.Person2">person</see></returns>
        public async Task<Dtos.Person2> GetPerson2ByGuidAsync(string guid, bool bypassCache)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a person");

            // get the person ID associated with the incoming guid
            var personId = await _personRepository.GetPersonIdFromGuidAsync(guid);
            if (string.IsNullOrEmpty(personId))
                throw new KeyNotFoundException("Person ID associated to guid '" + guid + "' not found in repository");

            // verify the user has the permission to view themselves or any person
            CheckUserPersonViewPermissions(personId);
            try
            {
                var personEntity = await _personRepository.GetPersonIntegrationByGuidAsync(guid, bypassCache);

                if (personEntity == null)
                    throw new KeyNotFoundException("Person '" + guid + "' not found in repository");

                if ((!string.IsNullOrEmpty(personEntity.PersonCorpIndicator)) &&
                        (personEntity.PersonCorpIndicator.Equals("Y", StringComparison.InvariantCultureIgnoreCase)))
                    throw new KeyNotFoundException(string.Format("'{0}' belongs to organization or educational institution. Can not be retrieved.", guid));

                return await ConvertPerson2EntityToDtoAsync(personEntity, false, bypassCache);
            }
            catch (RepositoryException e)
            {
                throw e;
            }

        }

        /// Get person data associated with a particular role.
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="title">Specific title</param>
        /// <param name="firstName">Specific first name</param>
        /// <param name="middleName">Specific middle name</param>
        /// <param name="lastNamePrefix">Last name beings with</param>
        /// <param name="lastName">Specific last name</param>
        /// <param name="pedigree">Specific suffix</param>
        /// <param name="preferredName">Specific preferred name</param>
        /// <param name="role">Specific role of a person</param>
        /// <param name="credentialType">Credential type of either colleagueId or ssn</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns>List of <see cref="Dtos.Person2">persons</see></returns>
        public async Task<Tuple<IEnumerable<Dtos.Person2>, int>> GetPerson2NonCachedAsync(int offset, int limit, bool bypassCache,
            string title, string firstName, string middleName, string lastNamePrefix, string lastName, string pedigree,
            string preferredName, string role, string credentialType, string credentialValue, string personFilter)
        {
            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();
            var personDtos = new List<Dtos.Person2>();
            try
            {
                //we are going to call the filter method done for V12
                PersonFilterCriteria personFilterCriteria = new PersonFilterCriteria();
                if (!string.IsNullOrEmpty(role))
                    personFilterCriteria.Roles = new List<string> { role };
                if (!string.IsNullOrEmpty(credentialType) && !string.IsNullOrEmpty(credentialValue))
                {
                    List<Tuple<string, string>> creds = new List<Tuple<string, string>> { new Tuple<string, string>(credentialType, credentialValue) };
                    personFilterCriteria.Credentials = creds;
                }
                if (!string.IsNullOrEmpty(title) || !string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(middleName) || !string.IsNullOrEmpty(lastNamePrefix) || !string.IsNullOrEmpty(lastName) || !string.IsNullOrEmpty(pedigree) || !string.IsNullOrEmpty(preferredName))
                {
                    PersonNamesCriteria personNamesCriteria = new PersonNamesCriteria();
                    if (!string.IsNullOrEmpty(title))
                    {
                        personNamesCriteria.Title = title;
                    }
                    if (!string.IsNullOrEmpty(firstName))
                    {
                        personNamesCriteria.FirstName = firstName;
                    }
                    if (!string.IsNullOrEmpty(middleName))
                    {
                        personNamesCriteria.MiddleName = middleName;
                    }
                    if (!string.IsNullOrEmpty(lastNamePrefix))
                    {
                        personNamesCriteria.LastNamePrefix = lastNamePrefix;
                    }
                    if (!string.IsNullOrEmpty(lastName))
                    {
                        personNamesCriteria.LastName = lastName;
                    }
                    if (!string.IsNullOrEmpty(pedigree))
                    {
                        personNamesCriteria.Pedigree = pedigree;
                    }
                    if (!string.IsNullOrEmpty(preferredName))
                    {
                        personNamesCriteria.PreferredName = preferredName;
                    }
                    personFilterCriteria.Names = new List<PersonNamesCriteria> { personNamesCriteria };
                }

                if (personFilterCriteria != null && personFilterCriteria.Credentials == null && personFilterCriteria.Emails == null && personFilterCriteria.Names == null && personFilterCriteria.Roles == null)
                {
                    personFilterCriteria = null;
                }

                var filteredTuple = await _personRepository.GetFilteredPerson2GuidsAsync(offset, limit, bypassCache, personFilterCriteria, personFilter);

                if (filteredTuple != null && filteredTuple.Item1 != null && filteredTuple.Item1.Any())
                {
                    var personEntities = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(filteredTuple.Item1);
                    foreach (var personEntity in personEntities)
                    {
                        // get all faculty person object with no addresses and phones
                        personDtos.Add(await ConvertPerson2EntityToDtoAsync(personEntity, false, bypassCache));
                    }
                }

                return personDtos != null && personDtos.Any() ? new Tuple<IEnumerable<Dtos.Person2>, int>(personDtos, filteredTuple.Item2) :
                    new Tuple<IEnumerable<Person2>, int>(new List<Dtos.Person2>(), 0);
            }
            catch (RepositoryException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        /// <summary>
        /// Get a person by guid using a cache.
        /// </summary>
        /// <param name="guid">Guid of the person in Colleague.</param>
        /// <param name="bypassCache"></param>
        /// <returns>The <see cref="Dtos.Person3">person</see></returns>
        public async Task<Dtos.Person3> GetPerson3ByGuidAsync(string guid, bool bypassCache)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a person");

            // get the person ID associated with the incoming guid
            var personId = await _personRepository.GetPersonIdFromGuidAsync(guid);

            if (string.IsNullOrEmpty(personId))
                throw new KeyNotFoundException("Person ID associated to guid '" + guid + "' not found in repository");

            // verify the user has the permission to view themselves or any person
            CheckUserPersonViewPermissions(personId);

            try
            {
                var personEntity = await _personRepository.GetPersonIntegrationByGuidAsync(guid, bypassCache);

                if (personEntity == null)
                    throw new KeyNotFoundException("Person '" + guid + "' not found in repository");

                if ((!string.IsNullOrEmpty(personEntity.PersonCorpIndicator)) &&
                    (personEntity.PersonCorpIndicator.Equals("Y", StringComparison.InvariantCultureIgnoreCase)))
                    throw new KeyNotFoundException(string.Format("'{0}' belongs to organization or educational institution. Can not be retrieved.", guid));


                return await ConvertPerson3EntityToDtoAsync(personEntity, false, bypassCache);
            }
            catch (RepositoryException e)
            {
                throw e;
            }
        }

        #region Persons V12 Chnages
        /// <summary>
        /// Get a person by guid using a cache.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Person4> GetPerson4ByGuidAsync(string guid, bool bypassCache)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a person");

            // get the person ID associated with the incoming guid
            var personId = await _personRepository.GetPersonIdFromGuidAsync(guid);

            if (string.IsNullOrEmpty(personId))
                throw new KeyNotFoundException("Person ID associated to guid '" + guid + "' not found in repository");

            // verify the user has the permission to view themselves or any person
            CheckUserPersonViewPermissions(personId);

            try
            {
                var personEntity = await _personRepository.GetPersonIntegration2ByGuidAsync(guid, bypassCache);

                if (personEntity == null)
                    throw new KeyNotFoundException("Person '" + guid + "' not found in repository");

                if ((!string.IsNullOrEmpty(personEntity.PersonCorpIndicator)) &&
                    (personEntity.PersonCorpIndicator.Equals("Y", StringComparison.InvariantCultureIgnoreCase)))
                    throw new KeyNotFoundException(string.Format("'{0}' belongs to organization or educational institution. Can not be retrieved.", guid));


                return await ConvertPerson4EntityToDtoAsync(personEntity, false, bypassCache);
            }
            catch (RepositoryException e)
            {
                throw e;
            }
            catch (ArgumentException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        private async Task<Dtos.Person4> ConvertPerson4EntityToDtoAsync(
            Domain.Base.Entities.PersonIntegration personEntity, bool getLimitedData, bool bypassCache = false)
        {
            // create the person DTO
            var personDto = new Dtos.Person4();

            personDto.Id = personEntity.Guid;
            personDto.PrivacyStatus = await GetPersonPrivacyAsync(personEntity.PrivacyStatus, personEntity.PrivacyStatusCode, bypassCache);
            personDto.PersonNames = await GetPersonNames2Async(personEntity, bypassCache);
            personDto.BirthDate = personEntity.BirthDate;
            personDto.DeceasedDate = personEntity.DeceasedDate;
            if (!string.IsNullOrEmpty(personEntity.Gender)) personDto.GenderType = GetGenderType2(personEntity.Gender);
            personDto.Religion = (!string.IsNullOrEmpty(personEntity.Religion)
                ? await GetPersonReligionGuidAsync(personEntity.Religion)
                : null);
            personDto.Ethnicity = (personEntity.EthnicCodes != null && personEntity.EthnicCodes.Any()
                ? await GetPersonEthnicityGuidAsync(personEntity.EthnicCodes)
                : null);
            personDto.Races = (personEntity.RaceCodes != null && personEntity.RaceCodes.Any()
                ? await GetPersonRaceGuidsAsync(personEntity.RaceCodes)
                : null);
            if ((personEntity.PrimaryLanguage != null) || (personEntity.SecondaryLanguages != null && personEntity.SecondaryLanguages.Any()))
            {
                var languages = await GetPersonLanguagesAsync(personEntity.PrimaryLanguage, personEntity.SecondaryLanguages, personDto.Id);
                if (languages != null && languages.Any())
                {
                    personDto.Languages = languages;
                }
            }
            if (!string.IsNullOrEmpty(personEntity.MaritalStatusCode))
                personDto.MaritalStatus = await GetPersonMaritalStatusGuidAsync(personEntity.MaritalStatusCode);
            if (personEntity.AlienStatus != null)
                personDto.CitizenshipStatus = await GetPersonCitizenshipAsync(personEntity.AlienStatus, bypassCache);
            if (!string.IsNullOrEmpty(personEntity.BirthCountry))
                personDto.CountryOfBirth = await GetPersonCountryAsync(personEntity.BirthCountry);
            if (!string.IsNullOrEmpty(personEntity.Citizenship))
                personDto.CitizenshipCountry = await GetPersonCountryAsync(personEntity.Citizenship);
            personDto.Roles = await GetPersonRolesAsync(personEntity.Id, personEntity.Roles);
            if (personEntity.Passport != null || personEntity.DriverLicense != null || (personEntity.IdentityDocuments != null && personEntity.IdentityDocuments.Any()))
            {
                personDto.IdentityDocuments = await GetPersonIdentityDocumentsAsync(personEntity, bypassCache);
            }
            string[] personGuids = { personEntity.Guid };
            await this.GetPersonPins(personGuids);

            var creds = GetPersonCredentials4(personEntity);
            var credDto = new List<Credential3DtoProperty>();
            foreach (var cred in creds)
            {
                credDto.Add(cred);
            }
            personDto.Credentials = credDto.AsEnumerable();
            if ((personEntity.Interests != null) && (personEntity.Interests.Any()))
                personDto.Interests = await GetPersonInterestsAsync(personEntity.Interests, bypassCache);

            // get addresses, email addresses and phones
            List<Domain.Base.Entities.EmailAddress> emailEntities = personEntity.EmailAddresses;
            List<Domain.Base.Entities.Phone> phoneEntities = personEntity.Phones;
            List<Domain.Base.Entities.Address> addressEntities = personEntity.Addresses;
            List<Domain.Base.Entities.SocialMedia> socialMediaEntities = personEntity.SocialMedia;

            var emailAddresses = await GetEmailAddresses2(emailEntities, bypassCache);
            if ((emailAddresses != null) && (emailAddresses.Any())) personDto.EmailAddresses = emailAddresses;
            var addresses = await GetAddresses2Async(addressEntities, bypassCache);
            if ((addresses != null) && (addresses.Any())) personDto.Addresses = addresses;


            var phoneNumbers = await GetPhones2Async(phoneEntities, bypassCache);
            if ((phoneNumbers != null) && (phoneNumbers.Any())) personDto.Phones = phoneNumbers;
            if ((socialMediaEntities != null) && (socialMediaEntities.Any()))
                personDto.SocialMedia = await GetPersonSocialMediaAsync(socialMediaEntities, bypassCache);

            //Veteran Statuses
            if (!string.IsNullOrEmpty(personEntity.MilitaryStatus))
            {
                var milStatus = (await MilitaryStatusesAsync(bypassCache)).FirstOrDefault(rec => rec.Code.Equals(personEntity.MilitaryStatus, StringComparison.OrdinalIgnoreCase));
                if (milStatus == null)
                {
                    throw new KeyNotFoundException(string.Format("Veteran status not found for code {0}.", personEntity.MilitaryStatus));
                }

                if (milStatus.Category == null)
                {
                    throw new InvalidOperationException(string.Format("An error has occurred.  Veteran Status categories must be mapped on CDHP. code: {0}", personEntity.MilitaryStatus));
                }

                Dtos.EnumProperties.VeteranStatusesCategory? category = null;
                switch (milStatus.Category)
                {
                    case VeteranStatusCategory.Activeduty:
                        category = Dtos.EnumProperties.VeteranStatusesCategory.Activeduty;
                        break;
                    case VeteranStatusCategory.Nonprotectedveteran:
                        category = Dtos.EnumProperties.VeteranStatusesCategory.Nonprotectedveteran;
                        break;
                    case VeteranStatusCategory.Nonveteran:
                        category = Dtos.EnumProperties.VeteranStatusesCategory.Nonveteran;
                        break;
                    case VeteranStatusCategory.Protectedveteran:
                        category = Dtos.EnumProperties.VeteranStatusesCategory.Protectedveteran;
                        break;
                }
                personDto.VeteranStatus = new PersonVeteranStatusDtoProperty()
                {
                    Detail = new GuidObject2(milStatus.Guid),
                    VeteranStatusCategory = category
                };
            }

            return personDto;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonName2DtoProperty>> GetPersonNames2Async(
            Domain.Base.Entities.PersonIntegration person, bool bypassCache = false)
        {
            List<Dtos.DtoProperties.PersonName2DtoProperty> personNames =
                new List<Dtos.DtoProperties.PersonName2DtoProperty>();

            var preferredNameType = person.PreferredNameType;

            // Legal Name for a person
            var personNameTypeItem = (await GetPersonNameTypesAsync(bypassCache)).FirstOrDefault(
                pn => pn.Code == "LEGAL");
            if (personNameTypeItem != null)
            {
                var personName = new Dtos.DtoProperties.PersonName2DtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                    {
                        Category = Dtos.EnumProperties.PersonNameType2.Legal,
                        Detail =
                            new Dtos.GuidObject2(
                                    personNameTypeItem.Guid)
                    },
                    FullName =
                        !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("LEGAL", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                        BuildFullName("FM", person.Prefix, person.FirstName, person.MiddleName,
                            person.LastName, person.Suffix),
                    Preference = (string.IsNullOrEmpty(preferredNameType) || preferredNameType.Equals("LEGAL", StringComparison.CurrentCultureIgnoreCase)) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    Title = string.IsNullOrEmpty(person.Prefix) ? null : person.Prefix,
                    FirstName = string.IsNullOrEmpty(person.FirstName) ? null : person.FirstName,
                    MiddleName = string.IsNullOrEmpty(person.MiddleName) ? null : person.MiddleName,
                    LastName = string.IsNullOrEmpty(person.LastName) ? null : person.LastName,
                    LastNamePrefix =
                        person.LastName.Contains(" ")
                            ? person.LastName.Split(" ".ToCharArray())[0].ToString()
                            : null,
                    Pedigree = string.IsNullOrEmpty(person.Suffix) ? null : person.Suffix,
                    ProfessionalAbbreviation =
                        person.ProfessionalAbbreviations.Any() ? person.ProfessionalAbbreviations : null
                };
                personNames.Add(personName);
            }

            // Birth Name
            if (!string.IsNullOrEmpty(person.BirthNameLast) || !string.IsNullOrEmpty(person.BirthNameFirst) ||
                !string.IsNullOrEmpty(person.BirthNameMiddle))
            {
                var birthNameTypeItem = (await GetPersonNameTypesAsync(bypassCache))
                    .FirstOrDefault(pn => pn.Code == "BIRTH");
                if (birthNameTypeItem != null)
                {
                    var birthName = new Dtos.DtoProperties.PersonName2DtoProperty()
                    {
                        NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.PersonNameType2.Birth,
                            Detail =
                                new Dtos.GuidObject2(
                                        birthNameTypeItem.Guid)
                        },
                        FullName =
                            !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("BIRTH", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                            BuildFullName("FM", "", person.BirthNameFirst, person.BirthNameMiddle, person.BirthNameLast, ""),
                        FirstName = string.IsNullOrEmpty(person.BirthNameFirst) ? null : person.BirthNameFirst,
                        MiddleName = string.IsNullOrEmpty(person.BirthNameMiddle) ? null : person.BirthNameMiddle,
                        LastName = string.IsNullOrEmpty(person.BirthNameLast) ? null : person.BirthNameLast,
                        LastNamePrefix =
                            person.BirthNameLast.Contains(" ")
                                ? person.BirthNameLast.Split(" ".ToCharArray())[0].ToString()
                                    : null,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("BIRTH", StringComparison.CurrentCultureIgnoreCase) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };
                    personNames.Add(birthName);
                }
            }

            // Chosen Name
            if (!string.IsNullOrEmpty(person.ChosenLastName) || !string.IsNullOrEmpty(person.ChosenFirstName) ||
                !string.IsNullOrEmpty(person.ChosenMiddleName))
            {
                var chosenNameTypeItem = (await GetPersonNameTypesAsync(bypassCache))
                    .FirstOrDefault(pn => pn.Code == "CHOSEN");
                if (chosenNameTypeItem != null)
                {
                    var chosenName = new Dtos.DtoProperties.PersonName2DtoProperty()
                    {
                        NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.PersonNameType2.Favored,
                            Detail =
                                new Dtos.GuidObject2(
                                        chosenNameTypeItem.Guid)
                        },
                        FullName =
                            !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("CHOSEN", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                            BuildFullName("FM", "", person.ChosenFirstName, person.ChosenMiddleName, person.ChosenLastName, ""),
                        FirstName = string.IsNullOrEmpty(person.ChosenFirstName) ? null : person.ChosenFirstName,
                        MiddleName = string.IsNullOrEmpty(person.ChosenMiddleName) ? null : person.ChosenMiddleName,
                        LastName = string.IsNullOrEmpty(person.ChosenLastName) ? null : person.ChosenLastName,
                        LastNamePrefix =
                            person.ChosenLastName.Contains(" ")
                                ? person.ChosenLastName.Split(" ".ToCharArray())[0].ToString()
                                    : null,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("CHOSEN", StringComparison.CurrentCultureIgnoreCase) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };
                    personNames.Add(chosenName);
                }
            }

            // Nickname
            if (!string.IsNullOrEmpty(person.Nickname))
            {
                var nickNameTypeItem = (await GetPersonNameTypesAsync(false))
                    .FirstOrDefault(pn => pn.Code == "NICKNAME");
                if (nickNameTypeItem != null)
                {
                    var nickName = new Dtos.DtoProperties.PersonName2DtoProperty()
                    {
                        NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.PersonNameType2.Personal,
                            Detail =
                                new Dtos.GuidObject2(
                                        nickNameTypeItem.Guid)
                        },
                        FullName = person.Nickname,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("NICKNAME", StringComparison.CurrentCultureIgnoreCase) ?
                           Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };

                    personNames.Add(nickName);
                }
            }

            // Name History
            if ((person.FormerNames != null) && (person.FormerNames.Any()))
            {
                var historyNameTypeItem = (await GetPersonNameTypesAsync(false))
                    .FirstOrDefault(pn => pn.Code == "HISTORY");
                if (historyNameTypeItem != null)
                {
                    foreach (var name in person.FormerNames)
                    {
                        var formerName = new Dtos.DtoProperties.PersonName2DtoProperty()
                        {
                            NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                            {
                                Category = Dtos.EnumProperties.PersonNameType2.Personal,
                                Detail =
                                    new Dtos.GuidObject2(
                                            historyNameTypeItem.Guid)
                            },
                            FullName = BuildFullName("FM", "", name.GivenName, name.MiddleName, name.FamilyName, ""),
                            FirstName = string.IsNullOrEmpty(name.GivenName) ? null : name.GivenName,
                            MiddleName = string.IsNullOrEmpty(name.MiddleName) ? null : name.MiddleName,
                            LastName = string.IsNullOrEmpty(name.FamilyName) ? null : name.FamilyName,
                            LastNamePrefix =
                                name.FamilyName.Contains(" ")
                                    ? name.FamilyName.Split(" ".ToCharArray())[0].ToString()
                                    : null
                        };
                        personNames.Add(formerName);
                    }
                }
            }
            return personNames;
        }


        /// <summary>
        /// Gets a collection of CredentialDtoProperty2 from a Person domain entity
        /// </summary>
        /// <param name="person">A <see cref="Domain.Base.Entities.Person"> Person domain entity</param>
        /// <returns>A collection of <see cref="Dtos.DtoProperties.CredentialDtoProperty2"> CredentialDtoProperty2</returns>
        private async Task<IEnumerable<Dtos.DtoProperties.Credential3DtoProperty>> GetPersonCredentials3(
            Domain.Base.Entities.Person person)
        {
            // Colleague Person ID
            var credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
            {
                new Dtos.DtoProperties.Credential3DtoProperty()
                {
                    Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId,
                    Value = person.Id
                }
            };
            // Elevate ID
            if (person.PersonAltIds != null && person.PersonAltIds.Any())
            {
                //Produce an error if there are more than one elevate id's, it means bad data
                if (person.PersonAltIds.Count(altId => altId.Type.Equals("ELEV", StringComparison.OrdinalIgnoreCase)) > 1)
                {
                    throw new InvalidOperationException(string.Format("Person ID '{0}': You cannot have more than one elevate id.", person.Id));
                }
                var elevPersonAltId =
                    person.PersonAltIds.FirstOrDefault(
                        a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                {
                    credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ElevateID,
                        Value = elevPersonAltId.Id
                    });
                }
            }
            // SSN
            if (!string.IsNullOrEmpty(person.GovernmentId))
            {
                var type = Dtos.EnumProperties.Credential3Type.Sin;
                var countryCode = await _personRepository.GetHostCountryAsync();
                if (countryCode.Equals("USA", StringComparison.OrdinalIgnoreCase))
                {
                    type = Dtos.EnumProperties.Credential3Type.Ssn;
                }
                credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                {
                    Type = type,
                    Value = person.GovernmentId
                });
            }
            //PERSON.PIN
            if (_personPins != null)
            {
                var personPinEntity = _personPins.FirstOrDefault(i => i.PersonId.Equals(person.Id, StringComparison.OrdinalIgnoreCase));
                if ((personPinEntity != null) && (!string.IsNullOrEmpty(personPinEntity.PersonPinUserId)))
                {
                    credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ColleagueUserName,
                        Value = personPinEntity.PersonPinUserId
                    });
                }
            }
            return credentials;
        }

        /// <summary>
        /// Gets a collection of CredentialDtoProperty2 from a Person domain entity
        /// </summary>
        /// <param name="person">A <see cref="Domain.Base.Entities.Person"> Person domain entity</param>
        /// <returns>A collection of <see cref="Dtos.DtoProperties.CredentialDtoProperty2"> CredentialDtoProperty2</returns>
        private IEnumerable<Dtos.DtoProperties.Credential3DtoProperty> GetPersonCredentials4(
            Domain.Base.Entities.Person person)
        {
            // Colleague Person ID
            var credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
            {
                new Dtos.DtoProperties.Credential3DtoProperty()
                {
                    Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId,
                    Value = person.Id
                }
            };
            // Elevate ID
            if (person.PersonAltIds != null && person.PersonAltIds.Any())
            {
                // Produce an error if there are more than one elevate id's, it means bad data
                if (person.PersonAltIds.Count(altId => altId.Type.Equals("ELEV", StringComparison.OrdinalIgnoreCase)) > 1)
                {
                    throw new InvalidOperationException(string.Format("Person ID '{0}': You cannot have more than one elevate id.", person.Id));
                }
                var elevPersonAltId =
                    person.PersonAltIds.FirstOrDefault(
                        a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                {
                    credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ElevateID,
                        Value = elevPersonAltId.Id
                    });
                }
            }
            // SSN
            if (!string.IsNullOrEmpty(person.GovernmentId))
            {
                var type = Dtos.EnumProperties.Credential3Type.TaxIdentificationNumber;
                credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                {
                    Type = type,
                    Value = person.GovernmentId
                });
            }
            //PERSON.PIN
            if (_personPins != null)
            {
                var personPinEntity = _personPins.FirstOrDefault(i => i.PersonId.Equals(person.Id, StringComparison.OrdinalIgnoreCase));
                if ((personPinEntity != null) && (!string.IsNullOrEmpty(personPinEntity.PersonPinUserId)))
                {
                    credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ColleagueUserName,
                        Value = personPinEntity.PersonPinUserId
                    });
                }
            }
            return credentials;
        }

        /// <summary>
        /// Gets a collection of CredentialDtoProperty2 from a Person domain entity
        /// </summary>
        /// <param name="person">A <see cref="Domain.Base.Entities.Person"> Person domain entity</param>
        /// <returns>A collection of <see cref="Dtos.DtoProperties.CredentialDtoProperty2"> CredentialDtoProperty2</returns>
        private IEnumerable<Dtos.DtoProperties.Credential3DtoProperty> GetPersonCredentials5(
            Domain.Base.Entities.Person person)
        {
            // Colleague Person ID
            var credentials = new List<Dtos.DtoProperties.Credential3DtoProperty>()
            {
                new Dtos.DtoProperties.Credential3DtoProperty()
                {
                    Type = Dtos.EnumProperties.Credential3Type.ColleaguePersonId,
                    Value = person.Id
                }
            };
            // Elevate ID
            if (person.PersonAltIds != null && person.PersonAltIds.Any())
            {
                // Allow more than one Elevate ID as of v12.1.0
                var elevPersonAltIdList =
                    person.PersonAltIds.Where(
                        a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                if (elevPersonAltIdList != null && elevPersonAltIdList.Any())
                {
                    foreach (var elevPersonAltId in elevPersonAltIdList)
                    {
                        if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                        {
                            credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                            {
                                Type = Dtos.EnumProperties.Credential3Type.ElevateID,
                                Value = elevPersonAltId.Id
                            });
                        }
                    }
                }
            }
            // SSN
            if (!string.IsNullOrEmpty(person.GovernmentId))
            {
                var type = Dtos.EnumProperties.Credential3Type.TaxIdentificationNumber;
                credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                {
                    Type = type,
                    Value = person.GovernmentId
                });
            }
            //PERSON.PIN
            if (_personPins != null)
            {
                var personPinEntity = _personPins.FirstOrDefault(i => i.PersonId.Equals(person.Id, StringComparison.OrdinalIgnoreCase));
                if ((personPinEntity != null) && (!string.IsNullOrEmpty(personPinEntity.PersonPinUserId)))
                {
                    credentials.Add(new Dtos.DtoProperties.Credential3DtoProperty()
                    {
                        Type = Dtos.EnumProperties.Credential3Type.ColleagueUserName,
                        Value = personPinEntity.PersonPinUserId
                    });
                }
            }
            return credentials;
        }

        /// <summary>
        /// Gets a collection of Alternative Credentials from a Person domain entity
        /// </summary>
        /// <param name="person">A <see cref="PersonAlt"> Person domain entity</param>
        /// <returns>A collection of <see cref="AlternativeCredentials"> CredentialDtoProperty2</returns>
        private async Task<IEnumerable<AlternativeCredentials>> GetAlternativeCredentials(string personId, List<PersonAlt> personAltIds, bool bypassCache)
        {
            // Alternative Credentials
            if (personAltIds != null && personAltIds.Any())
            {
                var credentials = new List<AlternativeCredentials>();
                foreach (var cred in personAltIds)
                {
                    //we do not want to display elevateId under alternative credentials 
                    // cred type can be null and it is supported.
                    if (cred != null && cred.Type != Domain.Base.Entities.PersonAlt.ElevatePersonAltType)
                    {
                        if (!string.IsNullOrEmpty(cred.Type))
                        {
                            try
                            {
                                var credType = ConvertCodeToGuid(await GetAlternateIdTypesAsync(bypassCache), cred.Type);
                                if (!string.IsNullOrEmpty(credType))
                                {
                                    credentials.Add(new AlternativeCredentials() { Type = new GuidObject2(credType), Value = cred.Id });
                                }
                            }
                            catch
                            {
                                // Even though the codes are missing for alternative ID types, update the credential ID only.
                                credentials.Add(new AlternativeCredentials() { Type = null, Value = cred.Id });
                            }
                        }
                        else
                        {
                            credentials.Add(new AlternativeCredentials() { Type = null, Value = cred.Id });
                        }
                    }
                }
                return credentials;
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Get HEDM V8 person data associated to the specified filters
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass Cache</param>
        /// <param name="personFilter">person filters</param>
        /// <param name="personFilterFilter">person Filter Filter</param>
        /// <param name="preferredNameFilter">preferred Name filter</param>
        /// 
        public async Task<Tuple<IEnumerable<Dtos.Person3>, int>> GetPerson3NonCachedAsync(int offset, int limit, bool bypassCache, Dtos.Filters.PersonFilter personFilter, string personFilterFilter, string preferredNameFilter)
        {
            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();
            var personDtos = new List<Dtos.Person3>();
            try
            {
                //we are going to call the filter method done for V12
                PersonFilterCriteria personFilterCriteria = new PersonFilterCriteria();
                //process person filter
                if (personFilter != null)
                {
                    if (personFilter.Credentials != null && personFilter.Credentials.Any())
                    {
                        List<Tuple<string, string>> creds = new List<Tuple<string, string>>();
                        personFilter.Credentials.ToList().ForEach(i =>
                        {
                            var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                            Tuple<string, string> tuple = new Tuple<string, string>(i.Type.ToString(), tempValue);
                            creds.Add(tuple);
                        });
                        personFilterCriteria.Credentials = creds;
                    }
                    if (personFilter.Roles != null && personFilter.Roles.Any())
                    {
                        personFilterCriteria.Roles = new List<string>();
                        personFilter.Roles.ToList().ForEach(i =>
                        {
                            personFilterCriteria.Roles.Add(i.RoleType.ToString());
                        });
                    }
                    if (personFilter.PersonNames != null)
                    {
                        personFilterCriteria.Names = new List<PersonNamesCriteria>();
                        personFilter.PersonNames.ToList().ForEach(i =>
                        {
                            if (i != null)
                            {
                                PersonNamesCriteria personNamesCriteria = new PersonNamesCriteria();
                                if (!string.IsNullOrEmpty(i.Title))
                                {
                                    personNamesCriteria.Title = i.Title;
                                }
                                if (!string.IsNullOrEmpty(i.FirstName))
                                {
                                    personNamesCriteria.FirstName = i.FirstName;
                                }
                                if (!string.IsNullOrEmpty(i.MiddleName))
                                {
                                    personNamesCriteria.MiddleName = i.MiddleName;
                                }
                                if (!string.IsNullOrEmpty(i.LastNamePrefix))
                                {
                                    personNamesCriteria.LastNamePrefix = i.LastNamePrefix;
                                }
                                if (!string.IsNullOrEmpty(i.LastName))
                                {
                                    personNamesCriteria.LastName = i.LastName;
                                }
                                if (!string.IsNullOrEmpty(i.Pedigree))
                                {
                                    personNamesCriteria.Pedigree = i.Pedigree;
                                }
                                personFilterCriteria.Names.Add(personNamesCriteria);
                            }
                        });
                    }
                }
                //process preferred name
                if (!string.IsNullOrEmpty(preferredNameFilter))
                {
                    personFilterCriteria.Names = new List<PersonNamesCriteria> { new PersonNamesCriteria { PreferredName = preferredNameFilter } };
                }



                if (personFilterCriteria != null && personFilterCriteria.Credentials == null && personFilterCriteria.Emails == null && personFilterCriteria.Names == null && personFilterCriteria.Roles == null)
                {
                    personFilterCriteria = null;
                }

                var filteredTuple = await _personRepository.GetFilteredPerson2GuidsAsync(offset, limit, bypassCache, personFilterCriteria, personFilterFilter);

                if (filteredTuple.Item1 != null && filteredTuple.Item1.Any())
                {
                    IEnumerable<PersonIntegration> personEntities = null;
                    try
                    {
                        personEntities = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(filteredTuple.Item1);
                        await this.GetPersonPins(filteredTuple.Item1.ToArray());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    foreach (var personEntity in personEntities)
                    {
                        // get all faculty person object with no addresses and phones
                        personDtos.Add(await ConvertPerson3EntityToDtoAsync(personEntity, false, bypassCache));
                    }
                }

                return personDtos != null && personDtos.Any() ? new Tuple<IEnumerable<Dtos.Person3>, int>(personDtos, filteredTuple.Item2) :
                    new Tuple<IEnumerable<Person3>, int>(new List<Dtos.Person3>(), 0);
            }
            catch (RepositoryException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// Get person data associated with a particular role.
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="bypassCache">Flag to bypass cache</param>
        /// <param name="title">Specific title</param>
        /// <param name="firstName">Specific first name</param>
        /// <param name="middleName">Specific middle name</param>
        /// <param name="lastNamePrefix">Last name beings with</param>
        /// <param name="lastName">Specific last name</param>
        /// <param name="pedigree">Specific suffix</param>
        /// <param name="preferredName">Specific preferred name</param>
        /// <param name="role">Specific role of a person</param>
        /// <param name="credentialType">Credential type of either colleagueId or ssn</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <param name="personFilter">Person Saved List selection or list name from person-filters</param>
        /// <returns>List of <see cref="Dtos.Person3">persons</see></returns>
        public async Task<Tuple<IEnumerable<Dtos.Person4>, int>> GetPerson4NonCachedAsync(int offset, int limit, bool bypassCache, Person4 person, string personFilter)
        {
            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            var personDtos = new List<Dtos.Person4>();
            //titles, firstnames, middlenames, lastNamePrefixes, lastNames
            PersonFilterCriteria personFilterCriteria = new PersonFilterCriteria();

            if (person != null)
            {
                if (person.Credentials != null && person.Credentials.Any())
                {
                    List<Tuple<string, string>> creds = new List<Tuple<string, string>>();
                    person.Credentials.ToList().ForEach(i =>
                    {
                        var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                        Tuple<string, string> tuple = new Tuple<string, string>(i.Type.ToString(), tempValue);
                        creds.Add(tuple);
                    });
                    personFilterCriteria.Credentials = creds;
                }
                if (person.EmailAddresses != null && person.EmailAddresses.Any())
                {
                    personFilterCriteria.Emails = new List<string>();
                    person.EmailAddresses.ToList().ForEach(i =>
                    {
                        if (!string.IsNullOrEmpty(i.Address))
                        {
                            personFilterCriteria.Emails.Add(i.Address.ToUpperInvariant());
                        }
                    });
                }
                if (person.Roles != null && person.Roles.Any())
                {
                    personFilterCriteria.Roles = new List<string>();
                    person.Roles.ToList().ForEach(i =>
                    {
                        personFilterCriteria.Roles.Add(i.RoleType.ToString());
                    });
                }
                if (person.PersonNames != null)
                {
                    personFilterCriteria.Names = new List<PersonNamesCriteria>();
                    person.PersonNames.ToList().ForEach(i =>
                    {
                        if (i != null)
                        {
                            PersonNamesCriteria personNamesCriteria = new PersonNamesCriteria();
                            if (!string.IsNullOrEmpty(i.Title))
                            {
                                personNamesCriteria.Title = i.Title;
                            }
                            if (!string.IsNullOrEmpty(i.FirstName))
                            {
                                personNamesCriteria.FirstName = i.FirstName;
                            }
                            if (!string.IsNullOrEmpty(i.MiddleName))
                            {
                                personNamesCriteria.MiddleName = i.MiddleName;
                            }
                            if (!string.IsNullOrEmpty(i.LastNamePrefix))
                            {
                                personNamesCriteria.LastNamePrefix = i.LastNamePrefix;
                            }
                            if (!string.IsNullOrEmpty(i.LastName))
                            {
                                personNamesCriteria.LastName = i.LastName;
                            }
                            personFilterCriteria.Names.Add(personNamesCriteria);
                        }
                    });
                }
            }
            // get all person guids
            //var filteredTuple = await _personRepository.GetFilteredPerson2GuidsAsync(offset, limit, bypassCache,
            //    titles, firstNames, middleNames, lastNamePrefixes, lastNames, roles, credentialTupleList, personFilter);
            if (personFilterCriteria != null && personFilterCriteria.Credentials == null && personFilterCriteria.Emails == null && personFilterCriteria.Names == null && personFilterCriteria.Roles == null)
            {
                personFilterCriteria = null;
            }

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            var filteredTuple = await _personRepository.GetFilteredPerson2GuidsAsync(offset, limit, bypassCache, personFilterCriteria, personFilter);
            //stopwatch.Stop();
            // logger.Info(string.Format("Time elapsed to retrieve person Ids from the filter : {0}", stopwatch.Elapsed));
            //stopwatch.Start();
            if (filteredTuple.Item1 != null && filteredTuple.Item1.Any())
            {
                IEnumerable<PersonIntegration> personEntities = null;
                //stopwatch.Start();
                try
                {
                    personEntities = await _personRepository.GetPersonIntegration2ByGuidNonCachedAsync(filteredTuple.Item1);
                    await this.GetPersonPins(filteredTuple.Item1.ToArray());
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                //stopwatch.Stop();
                //logger.Info(string.Format("Time elapsed to getting entities to DTO : {0}", stopwatch.Elapsed));
                //stopwatch.Start();
                foreach (var personEntity in personEntities)
                {
                    // get all person person object with no addresses and phones
                    personDtos.Add(await ConvertPerson4EntityToDtoAsync(personEntity, false, bypassCache));
                }
                // stopwatch.Stop();
                //logger.Info(string.Format("Time elapsed to converting entity to DTO : {0}", stopwatch.Elapsed));
            }

            return personDtos != null && personDtos.Any() ? new Tuple<IEnumerable<Dtos.Person4>, int>(personDtos, filteredTuple.Item2) :
                new Tuple<IEnumerable<Person4>, int>(new List<Dtos.Person4>(), 0);
        }

        #endregion

        #region Get Persons V12.1.0 
        /// <summary>
        /// Get a person by guid using a cache.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="bypassCache"></param>
        /// <returns></returns>
        public async Task<Person5> GetPerson5ByGuidAsync(string guid, bool bypassCache)
        {
            if (string.IsNullOrEmpty(guid))
                throw new ArgumentNullException("guid", "Must provide a guid to get a person");

            // get the person ID associated with the incoming guid
            string personId = string.Empty;
            try
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(guid);
            }
            catch (Exception)
            {
                throw new KeyNotFoundException(string.Format("No persons was found for guid '{0}'.", guid));
            }

            if (string.IsNullOrEmpty(personId))
                throw new KeyNotFoundException(string.Format("No persons was found for guid '{0}'.", guid));

            // verify the user has the permission to view themselves or any person
            CheckUserPersonViewPermissions2(personId);

            PersonIntegration personEntity = null;
            try
            {
                personEntity = await _personRepository.GetPersonIntegration3ByGuidAsync(guid, bypassCache);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }

            if (personEntity == null)
                throw new KeyNotFoundException(string.Format("No persons was found for guid '{0}'", guid));

            if ((!string.IsNullOrEmpty(personEntity.PersonCorpIndicator)) &&
                (personEntity.PersonCorpIndicator.Equals("Y", StringComparison.InvariantCultureIgnoreCase)))
                throw new KeyNotFoundException(string.Format("'{0}' belongs to organization or educational institution. Can not be retrieved.", guid));

            var person = await ConvertPerson5EntityToDtoAsync(personEntity, false, bypassCache);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return person;
        }

        private async Task<Dtos.Person5> ConvertPerson5EntityToDtoAsync(
            Domain.Base.Entities.PersonIntegration personEntity, bool getLimitedData, bool bypassCache = false)
        {
            if (personEntity.Guid == null)
            {
               IntegrationApiExceptionAddError("Person guid must be provided.", "persons.id", 
                   id: personEntity.Id );
            }

            // create the person DTO
            var personDto = new Dtos.Person5();

            personDto.Id = personEntity.Guid;
            personDto.PrivacyStatus = await GetPersonPrivacyAsync(personEntity.PrivacyStatus, personEntity.PrivacyStatusCode, bypassCache);
            personDto.PersonNames = await GetPersonNames2Async(personEntity, bypassCache);
            personDto.BirthDate = personEntity.BirthDate;
            personDto.DeceasedDate = personEntity.DeceasedDate;
            if (!string.IsNullOrEmpty(personEntity.Gender))
                personDto.GenderType = GetGenderType2(personEntity.Gender);
            if (!string.IsNullOrEmpty(personEntity.GenderIdentityCode))
            {
                string genderIdGuid = string.Empty;
                try
                {
                    genderIdGuid = ConvertCodeToGuid(await GetGenderIdentityTypesAsync(bypassCache), personEntity.GenderIdentityCode);
                }
                catch
                {
                    genderIdGuid = string.Empty;
                }
                if (!string.IsNullOrEmpty(genderIdGuid))
                    personDto.GenderIdentity = new Dtos.GuidObject2(genderIdGuid);
                else
                    IntegrationApiExceptionAddError(string.Format("Gender Identity code of '{0}' is not valid.", personEntity.GenderIdentityCode), "persons.genderIdentity", personEntity.Guid, personEntity.Id);
            }
            if (!string.IsNullOrEmpty(personEntity.PersonalPronounCode))
            {
                string pronounGuid = string.Empty;
                try
                {
                    pronounGuid = ConvertCodeToGuid(await GetPersonalPronounTypesAsync(bypassCache), personEntity.PersonalPronounCode);
                }
                catch
                {
                    pronounGuid = string.Empty;
                }
                if (!string.IsNullOrEmpty(pronounGuid))
                    personDto.PersonalPronoun = new Dtos.GuidObject2(pronounGuid);
                else
                    IntegrationApiExceptionAddError(string.Format("Personal Pronoun code of '{0}' is not valid.", personEntity.PersonalPronounCode), "persons.personalPronoun", personEntity.Guid, personEntity.Id);
            }
            personDto.Religion = (!string.IsNullOrEmpty(personEntity.Religion)
                ? await GetPersonReligionGuidAsync(personEntity.Religion)
                : null);
            personDto.Ethnicity = (personEntity.EthnicCodes != null && personEntity.EthnicCodes.Any()
                ? await GetPersonEthnicityGuidAsync(personEntity.EthnicCodes)
                : null);
            personDto.Races = (personEntity.RaceCodes != null && personEntity.RaceCodes.Any()
                ? await GetPersonRaceGuidsAsync(personEntity.RaceCodes)
                : null);
            if ((personEntity.PrimaryLanguage != null) || (personEntity.SecondaryLanguages != null && personEntity.SecondaryLanguages.Any()))
            {
                var languages = await GetPersonLanguages2Async(personEntity.PrimaryLanguage, personEntity.SecondaryLanguages, personDto.Id);
                if (languages != null && languages.Any())
                {
                    personDto.Languages = languages;
                }
            }

            if (!string.IsNullOrEmpty(personEntity.MaritalStatusCode))
                personDto.MaritalStatus = await GetPersonMaritalStatusGuidAsync(personEntity.MaritalStatusCode);
            if (personEntity.AlienStatus != null)
                personDto.CitizenshipStatus = await GetPersonCitizenshipAsync(personEntity.AlienStatus, bypassCache);
            if (!string.IsNullOrEmpty(personEntity.BirthCountry))
                personDto.CountryOfBirth = await GetPersonCountryAsync(personEntity.BirthCountry);
            if (!string.IsNullOrEmpty(personEntity.Citizenship))
                personDto.CitizenshipCountry = await GetPersonCountryAsync(personEntity.Citizenship);
            personDto.Roles = await GetPersonRolesAsync(personEntity.Id, personEntity.Roles);
            if (personEntity.Passport != null || personEntity.DriverLicense != null || (personEntity.IdentityDocuments != null && personEntity.IdentityDocuments.Any()))
            {
               personDto.IdentityDocuments = await GetPersonIdentityDocuments2Async(personEntity, bypassCache);                
            }
            string[] personGuids = { personEntity.Guid };
            await this.GetPersonPins(personGuids);

            var creds = GetPersonCredentials5(personEntity);
            var credDto = new List<Credential3DtoProperty>();
            foreach (var cred in creds)
            {
                credDto.Add(cred);
            }
            personDto.Credentials = credDto.AsEnumerable();
            //get alternative credentials
            if (personEntity.PersonAltIds != null && personEntity.PersonAltIds.Any())
            {
                var altCred = await GetAlternativeCredentials(personEntity.Id, personEntity.PersonAltIds, bypassCache);
                if (altCred != null && altCred.Any())
                    personDto.AlternativeCredentials = altCred;
            }
            if ((personEntity.Interests != null) && (personEntity.Interests.Any()))
                personDto.Interests = await GetPersonInterestsAsync(personEntity.Interests, bypassCache);

            // get addresses, email addresses and phones
            List<Domain.Base.Entities.EmailAddress> emailEntities = personEntity.EmailAddresses;
            List<Domain.Base.Entities.Phone> phoneEntities = personEntity.Phones;
            List<Domain.Base.Entities.Address> addressEntities = personEntity.Addresses;
            List<Domain.Base.Entities.SocialMedia> socialMediaEntities = personEntity.SocialMedia;

            var emailAddresses = await GetEmailAddresses2(emailEntities, bypassCache);
            if ((emailAddresses != null) && (emailAddresses.Any())) personDto.EmailAddresses = emailAddresses;
            
            var addresses = await GetAddresses3Async(addressEntities, personEntity.Guid, personEntity.Id, bypassCache);
            if ((addresses != null) && (addresses.Any())) personDto.Addresses = addresses;
           
            var phoneNumbers = await GetPhones2Async(phoneEntities, bypassCache);
            if ((phoneNumbers != null) && (phoneNumbers.Any())) personDto.Phones = phoneNumbers;
            if ((socialMediaEntities != null) && (socialMediaEntities.Any()))
                personDto.SocialMedia = await GetPersonSocialMediaAsync(socialMediaEntities, bypassCache);

            //Veteran Statuses
            if (!string.IsNullOrEmpty(personEntity.MilitaryStatus))
            {
                var milStatus = (await MilitaryStatusesAsync(bypassCache)).FirstOrDefault(rec => rec.Code.Equals(personEntity.MilitaryStatus, StringComparison.OrdinalIgnoreCase));
                if (milStatus == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Veteran status code '{0}' cannot be found.",  personEntity.MilitaryStatus),
                        "persons.veteranStatus", personEntity.Guid, personEntity.Id);
                }
                else
                {
                    if (milStatus.Category == null)
                    {
                        IntegrationApiExceptionAddError(string.Format("Veteran Status categories must be mapped on CDHP for code '{0}'.", personEntity.MilitaryStatus),
                            "persons.veteranStatus", personEntity.Guid, personEntity.Id);
                    }
                    else
                    {
                        Dtos.EnumProperties.VeteranStatusesCategory? category = null;
                        switch (milStatus.Category)
                        {
                            case VeteranStatusCategory.Activeduty:
                                category = Dtos.EnumProperties.VeteranStatusesCategory.Activeduty;
                                break;
                            case VeteranStatusCategory.Nonprotectedveteran:
                                category = Dtos.EnumProperties.VeteranStatusesCategory.Nonprotectedveteran;
                                break;
                            case VeteranStatusCategory.Nonveteran:
                                category = Dtos.EnumProperties.VeteranStatusesCategory.Nonveteran;
                                break;
                            case VeteranStatusCategory.Protectedveteran:
                                category = Dtos.EnumProperties.VeteranStatusesCategory.Protectedveteran;
                                break;
                        }
                        personDto.VeteranStatus = new PersonVeteranStatusDtoProperty()
                        {
                            Detail = new GuidObject2(milStatus.Guid),
                            VeteranStatusCategory = category
                        };
                    }
                }
            }

            return personDto;
        }

        /// <summary>
        /// Get person data associated with a particular role.
        /// </summary>
        /// <param name="offset">pagiong offset</param>
        /// <param name="limit">paging limit</param>
        /// <param name="bypassCache">bypassCache flag</param>
        /// <param name="person">person filter represented by DTO</param>
        /// <param name="personFilter">personFilterId filter</param>
        /// <returns>List of <see cref="Dtos.Person5">persons</see></returns>
        public async Task<Tuple<IEnumerable<Dtos.Person5>, int>> GetPerson5NonCachedAsync(int offset, int limit, bool bypassCache, Person5 person, string personFilter)
        {
            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions2();

            var personDtos = new List<Dtos.Person5>();
            //titles, firstnames, middlenames, lastNamePrefixes, lastNames
            var personFilterCriteria = new PersonFilterCriteria();

            if (person != null)
            {
                if (person.Credentials != null && person.Credentials.Any())
                {
                    var creds = new List<Tuple<string, string>>();
                    person.Credentials.ToList().ForEach(i =>
                    {
                        var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                        var tuple = new Tuple<string, string>(i.Type.ToString(), tempValue);
                        creds.Add(tuple);
                    });
                    personFilterCriteria.Credentials = creds;
                }
                if (person.AlternativeCredentials != null && person.AlternativeCredentials.Any())
                {
                    var altIdTypesList = await GetAlternateIdTypesAsync(false);
                    var altCreds = new List<Tuple<string, string>>();
                    person.AlternativeCredentials.ToList().ForEach(i =>
                    {
                        if (i.Type != null && !string.IsNullOrEmpty(i.Type.Id))
                        {
                            var altCredTypeEntity = altIdTypesList.FirstOrDefault(e => e.Guid.Equals(i.Type.Id, StringComparison.OrdinalIgnoreCase));
                            if (altCredTypeEntity != null)
                            {
                                var altCredTypeCode = altCredTypeEntity.Code;
                                if (!string.IsNullOrEmpty(altCredTypeCode))
                                {
                                    var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                                    var tuple = new Tuple<string, string>(altCredTypeCode, tempValue);
                                    altCreds.Add(tuple);
                                }
                            }
                        }
                        else
                        {
                            var tempValue = i.Value.Contains("'") ? i.Value.Replace("'", string.Empty) : i.Value;
                            var tuple = new Tuple<string, string>("", tempValue);
                            altCreds.Add(tuple);
                        }
                    });
                    if (altCreds == null || !altCreds.Any())
                    {
                        return new Tuple<IEnumerable<Dtos.Person5>, int>(new List<Person5>(), 0);
                    }
                    personFilterCriteria.AlternativeCredentials = altCreds;
                }
                if (person.EmailAddresses != null && person.EmailAddresses.Any())
                {
                    personFilterCriteria.Emails = new List<string>();
                    person.EmailAddresses.ToList().ForEach(i =>
                    {
                        if (!string.IsNullOrEmpty(i.Address))
                        {
                            personFilterCriteria.Emails.Add(i.Address.ToUpperInvariant());
                        }
                    });
                }
                if (person.Roles != null && person.Roles.Any())
                {
                    personFilterCriteria.Roles = new List<string>();
                    person.Roles.ToList().ForEach(i =>
                    {
                        personFilterCriteria.Roles.Add(i.RoleType.ToString());
                    });
                }
                if (person.PersonNames != null)
                {
                    personFilterCriteria.Names = new List<PersonNamesCriteria>();
                    person.PersonNames.ToList().ForEach(i =>
                    {
                        if (i != null)
                        {
                            var personNamesCriteria = new PersonNamesCriteria();
                            if (!string.IsNullOrEmpty(i.Title))
                            {
                                personNamesCriteria.Title = i.Title;
                            }
                            if (!string.IsNullOrEmpty(i.FirstName))
                            {
                                personNamesCriteria.FirstName = i.FirstName;
                            }
                            if (!string.IsNullOrEmpty(i.MiddleName))
                            {
                                personNamesCriteria.MiddleName = i.MiddleName;
                            }
                            if (!string.IsNullOrEmpty(i.LastNamePrefix))
                            {
                                personNamesCriteria.LastNamePrefix = i.LastNamePrefix;
                            }
                            if (!string.IsNullOrEmpty(i.LastName))
                            {
                                personNamesCriteria.LastName = i.LastName;
                            }
                            personFilterCriteria.Names.Add(personNamesCriteria);
                        }
                    });
                }
            }
            // get all person guids
            if (personFilterCriteria != null && personFilterCriteria.Credentials == null && personFilterCriteria.AlternativeCredentials == null && personFilterCriteria.Emails == null && personFilterCriteria.Names == null && personFilterCriteria.Roles == null)
            {
                personFilterCriteria = null;
            }

            //var stopwatch = new Stopwatch();
            //stopwatch.Start();
            Tuple<IEnumerable<string>, int> filteredTuple = null;
            try
            {
                filteredTuple = await _personRepository.GetFilteredPerson3GuidsAsync(offset, limit, bypassCache, personFilterCriteria, personFilter);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            //stopwatch.Stop();
            // logger.Info(string.Format("Time elapsed to retrieve person Ids from the filter : {0}", stopwatch.Elapsed));
            //stopwatch.Start();
            if (filteredTuple.Item1 != null && filteredTuple.Item1.Any())
            {
                //stopwatch.Start();
                IEnumerable<PersonIntegration> personEntities = null;

                try
                {
                    personEntities = await _personRepository.GetPersonIntegration3ByGuidNonCachedAsync(filteredTuple.Item1);
                    await this.GetPersonPins(filteredTuple.Item1.ToArray());
                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                
                //stopwatch.Stop();
                //logger.Info(string.Format("Time elapsed to getting entities to DTO : {0}", stopwatch.Elapsed));
                //stopwatch.Start();
                foreach (var personEntity in personEntities)
                {
                    // get all person person object with no addresses and phones
                    personDtos.Add(await ConvertPerson5EntityToDtoAsync(personEntity, false, bypassCache));
                }
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                // stopwatch.Stop();
                //logger.Info(string.Format("Time elapsed to converting entity to DTO : {0}", stopwatch.Elapsed));
            }

            return personDtos != null && personDtos.Any() ? new Tuple<IEnumerable<Dtos.Person5>, int>(personDtos, filteredTuple.Item2) :
                new Tuple<IEnumerable<Person5>, int>(new List<Dtos.Person5>(), 0);
        }
        #endregion

        #region Create Person Methods

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="personDto">The <see cref="Dtos.Person2">person</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Dtos.Person2">person</see></returns>
        public async Task<Dtos.Person2> CreatePerson2Async(Dtos.Person2 personDto)
        {
            if (personDto == null)
                throw new ArgumentNullException("personDto", "Must provide a person object for creation");
            if (string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide a guid for person creation");

            PersonIntegration createdPersonEntity = null;
            // verify the user has the permission to create a person
            CheckUserPersonCreatePermissions();

            _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                // map the person DTO to entities
                var personEntity = await ConvertPerson2DtoToEntityAsync(null, personDto);
                var addressEntities = await ConvertAddressDtoToAddressEntitiesAsync(personDto.Addresses);
                var phoneEntities = await ConvertPhoneDtoToPhoneEntities(personDto.Phones);

                // create the person entity in the database
                createdPersonEntity = await _personRepository.Create2Async(personEntity, addressEntities, phoneEntities);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            // return the newly created person
            return await ConvertPerson2EntityToDtoAsync(createdPersonEntity, false);
        }

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="personDto">The <see cref="Dtos.Person3">person</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Dtos.Person3">person</see></returns>
        public async Task<Dtos.Person3> CreatePerson3Async(Dtos.Person3 personDto)
        {
            if (personDto == null)
                throw new ArgumentNullException("personDto", "Must provide a person object for creation");
            if (string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide a guid for person creation");

            PersonIntegration createdPersonEntity = null;
            // verify the user has the permission to create a person
            CheckUserPersonCreatePermissions();

            _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                // map the person DTO to entities
                var personEntity = await ConvertPerson3DtoToEntityAsync(null, personDto);
                var addressEntities = await ConvertAddressDtoToAddressEntitiesAsync(personDto.Addresses);
                var phoneEntities = await ConvertPhoneDtoToPhoneEntities(personDto.Phones);

                // create the person entity in the database
                createdPersonEntity = await _personRepository.Create2Async(personEntity, addressEntities, phoneEntities);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            // return the newly created person
            return await ConvertPerson3EntityToDtoAsync(createdPersonEntity, false);
        }

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="personDto">The <see cref="Dtos.Person4">person</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Dtos.Person4">person</see></returns>
        public async Task<Dtos.Person4> CreatePerson4Async(Dtos.Person4 personDto)
        {
            if (personDto == null)
                throw new ArgumentNullException("personDto", "Must provide a person object for creation");
            if (string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide a guid for person creation");

            PersonIntegration createdPersonEntity = null;
            // verify the user has the permission to create a person
            CheckUserPersonCreatePermissions();

            _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                // map the person DTO to entities
                //The name type is no longer required for each name in the names array. 
                //However, the only use case where we can accept a name w/out a type.category or type.id is on a POST 
                //where the names array only contains a single name. In that case we can assume it is the legal name and respond w/ the name type indicating a legal name.
                if (personDto.PersonNames != null && personDto.PersonNames.Count() == 1 && personDto.PersonNames.FirstOrDefault().NameType == null)
                {
                    var nametype = new PersonNameTypeDtoProperty();
                    nametype.Category = PersonNameType2.Legal;
                    personDto.PersonNames.FirstOrDefault().NameType = nametype;
                }
                var personEntity = await ConvertPerson4DtoToEntityAsync(null, personDto);
                var addressEntities = await ConvertAddressDtoToAddressEntitiesAsync(personDto.Addresses);
                var phoneEntities = await ConvertPhoneDtoToPhoneEntities(personDto.Phones);

                // create the person entity in the database
                createdPersonEntity = await _personRepository.Create2Async(personEntity, addressEntities, phoneEntities);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            // return the newly created person
            return await ConvertPerson4EntityToDtoAsync(createdPersonEntity, false);
        }

        /// <summary>
        /// Create a person.
        /// </summary>
        /// <param name="personDto">The <see cref="Dtos.Person5">person</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Dtos.Person5">person</see></returns>
        public async Task<Dtos.Person5> CreatePerson5Async(Dtos.Person5 personDto)
        {
            if (personDto == null)
            {
                IntegrationApiExceptionAddError("Must provide a person object for creation", "persons");
                throw IntegrationApiException;
            }
            if (string.IsNullOrEmpty(personDto.Id))
            {
                IntegrationApiExceptionAddError("Must provide a guid for person creation", "persons");
                throw IntegrationApiException;
            }

            PersonIntegration createdPersonEntity = null;
            var personGuid = personDto.Id;
            var personId = string.Empty;
            // verify the user has the permission to create a person
            CheckUserPersonCreatePermissions2();

            _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            try
            {
                // map the person DTO to entities
                //The name type is no longer required for each name in the names array. 
                //However, the only use case where we can accept a name w/out a type.category or type.id is on a POST 
                //where the names array only contains a single name. In that case we can assume it is the legal name and respond w/ the name type indicating a legal name.
                if (personDto.PersonNames != null && personDto.PersonNames.Count() == 1 && personDto.PersonNames.FirstOrDefault().NameType == null)
                {
                    var nametype = new PersonNameTypeDtoProperty();
                    nametype.Category = PersonNameType2.Legal;
                    personDto.PersonNames.FirstOrDefault().NameType = nametype;
                }
                var personEntity = await ConvertPerson5DtoToEntityAsync(null, personDto);
                IEnumerable<Domain.Base.Entities.Address> addressEntities = null;
                IEnumerable<Domain.Base.Entities.Phone> phoneEntities = null;
                try
                {
                    addressEntities = await ConvertAddressDtoToAddressEntitiesAsync(personDto.Addresses);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.addresses", personEntity.Guid, personEntity.Id);
                }
                try
                {
                    phoneEntities = await ConvertPhoneDtoToPhoneEntities(personDto.Phones);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.phones", personEntity.Guid, personEntity.Id);
                }

                if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                {
                    throw IntegrationApiException;
                }

                // create the person entity in the database
                if (personEntity != null)
                {
                    createdPersonEntity = await _personRepository.Create2Async(personEntity, addressEntities, phoneEntities, 2);
                    if (createdPersonEntity != null)
                    {
                        personGuid = createdPersonEntity.Guid;
                        personId = createdPersonEntity.Id;
                    }
                }
            }
            catch (IntegrationApiException)
            {
                // Drop through on Integration API exception
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex, guid: personGuid, id: personId);
            }
            catch (Exception ex)
            {
                IntegrationApiExceptionAddError(ex.Message, "persons", personGuid, personId);
            }

            Person5 person = null;
            if (createdPersonEntity != null)
                person = await ConvertPerson5EntityToDtoAsync(createdPersonEntity, false);

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            // return the newly created person
            return person;
        }
        #endregion

        #region Update Person Methods

        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="personDto">The <see cref="Dtos.Person2">person</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Dtos.Person2">person</see></returns>
        public async Task<Dtos.Person2> UpdatePerson2Async(Dtos.Person2 personDto)
        {
            if (personDto == null)
                throw new ArgumentNullException("personDto", "Must provide a person for update");
            if (string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide a guid for person update");

            // get the person ID associated with the incoming guid
            string personId = null;
            try
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(personDto.Id);
            }
            catch (KeyNotFoundException)
            {
                // suppress KeyNotFound.  PUT can be called with an externally-supplied GUID to create a new person
            }
            _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // partial put is merging the data incorrectly for privacy status of unrestricted. Clear the detail id.
            if (personDto.PrivacyStatus != null && personDto.PrivacyStatus.PrivacyCategory == Dtos.PrivacyStatusType.Unrestricted && personDto.PrivacyStatus.Detail != null && !string.IsNullOrEmpty(personDto.PrivacyStatus.Detail.Id))
            {
                personDto.PrivacyStatus.Detail = null;
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(personId))
            {
                try
                {
                    // verify the user has the permission to update a person
                    CheckUserPersonUpdatePermissions(personId);

                    // map the person DTO to entities
                    var personEntity = await ConvertPerson2DtoToEntityAsync(personId, personDto);
                    var addressEntites = await ConvertAddressDtoToAddressEntitiesAsync(personDto.Addresses);
                    var phoneEntities = await ConvertPhoneDtoToPhoneEntities(personDto.Phones);

                    // update the person in the database
                    var updatedPersonEntity =
                        await _personRepository.Update2Async(personEntity, addressEntites, phoneEntities);

                    // return the newly updated person
                    return await ConvertPerson2EntityToDtoAsync(updatedPersonEntity, false);

                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            else
            {
                // perform a create instead
                return await CreatePerson2Async(personDto);
            }
        }


        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="personDto">The <see cref="Dtos.Person3">person</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Dtos.Person3">person</see></returns>
        public async Task<Dtos.Person3> UpdatePerson3Async(Dtos.Person3 personDto)
        {
            if (personDto == null)
                throw new ArgumentNullException("personDto", "Must provide a person for update");
            if (string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide a guid for person update");

            // get the person ID associated with the incoming guid
            string personId = null;
            try
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(personDto.Id);
            }
            catch (KeyNotFoundException)
            {
                // suppress KeyNotFound.  PUT can be called with an externally-supplied GUID to create a new person
            }

            _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

            // partial put is merging the data incorrectly for privacy status of unrestricted. Clear the detail id.
            if (personDto.PrivacyStatus != null && personDto.PrivacyStatus.PrivacyCategory == Dtos.PrivacyStatusType.Unrestricted && personDto.PrivacyStatus.Detail != null && !string.IsNullOrEmpty(personDto.PrivacyStatus.Detail.Id))
            {
                personDto.PrivacyStatus.Detail = null;
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(personId))
            {
                try
                {
                    // verify the user has the permission to update a person
                    CheckUserPersonUpdatePermissions(personId);

                    // map the person DTO to entities
                    var personEntity = await ConvertPerson3DtoToEntityAsync(personId, personDto);
                    var addressEntites = await ConvertAddressDtoToAddressEntitiesAsync(personDto.Addresses);
                    var phoneEntities = await ConvertPhoneDtoToPhoneEntities(personDto.Phones);

                    // update the person in the database
                    var updatedPersonEntity =
                        await _personRepository.Update2Async(personEntity, addressEntites, phoneEntities);

                    // return the newly updated person
                    return await ConvertPerson3EntityToDtoAsync(updatedPersonEntity, false);

                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            else
            {
                // perform a create instead
                return await CreatePerson3Async(personDto);
            }
        }

        /// <summary>
        /// Update a person.
        /// </summary>
        /// <param name="personDto">The <see cref="Dtos.Person4">person</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Dtos.Person4">person</see></returns>
        public async Task<Dtos.Person4> UpdatePerson4Async(Dtos.Person4 personDto)
        {
            if (personDto == null)
                throw new ArgumentNullException("personDto", "Must provide a person for update");
            if (string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide a guid for person update");

            // get the person ID associated with the incoming guid
            string personId = null;
            try
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(personDto.Id);
            }
            catch (KeyNotFoundException)
            {
                // suppress KeyNotFound.  PUT can be called with an externally-supplied GUID to create a new person
            }

            _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            // partial put is merging the data incorrectly for privacy status of unrestricted. Clear the detail id.
            if (personDto.PrivacyStatus != null && personDto.PrivacyStatus.PrivacyCategory == Dtos.PrivacyStatusType.Unrestricted && personDto.PrivacyStatus.Detail != null && !string.IsNullOrEmpty(personDto.PrivacyStatus.Detail.Id))
            {
                personDto.PrivacyStatus.Detail = null;
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(personId))
            {
                try
                {
                    // verify the user has the permission to update a person
                    CheckUserPersonUpdatePermissions(personId);

                    // map the person DTO to entities
                    var personEntity = await ConvertPerson4DtoToEntityAsync(personId, personDto);
                    var addressEntites = await ConvertAddressDtoToAddressEntitiesAsync(personDto.Addresses);
                    var phoneEntities = await ConvertPhoneDtoToPhoneEntities(personDto.Phones);

                    // update the person in the database
                    var updatedPersonEntity =
                        await _personRepository.Update2Async(personEntity, addressEntites, phoneEntities);

                    // return the newly updated person
                    return await ConvertPerson4EntityToDtoAsync(updatedPersonEntity, false);

                }
                catch (RepositoryException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }
            }
            else
            {
                // perform a create instead
                return await CreatePerson4Async(personDto);
            }
        }

        /// <summary>
        /// Update a person 
        /// </summary>
        /// <param name="personDto">The <see cref="Dtos.Person5">person</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Dtos.Person5">person</see></returns>
        public async Task<Dtos.Person5> UpdatePerson5Async(Dtos.Person5 personDto)
        {
            if (personDto == null)
            {
                IntegrationApiExceptionAddError("Must provide a person object for update", "persons");
                throw IntegrationApiException;
            }
            if (string.IsNullOrEmpty(personDto.Id))
            {
                IntegrationApiExceptionAddError("Must provide a guid for person update", "persons");
                throw IntegrationApiException;
            }

            // get the person ID associated with the incoming guid
            string personId = null;
            try
            {
                personId = await _personRepository.GetPersonIdFromGuidAsync(personDto.Id);
            }
            catch (KeyNotFoundException)
            {
                // suppress KeyNotFound.  PUT can be called with an externally-supplied GUID to create a new person
            }

            _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;
            // partial put is merging the data incorrectly for privacy status of unrestricted. Clear the detail id.
            if (personDto.PrivacyStatus != null && personDto.PrivacyStatus.PrivacyCategory == Dtos.PrivacyStatusType.Unrestricted && personDto.PrivacyStatus.Detail != null && !string.IsNullOrEmpty(personDto.PrivacyStatus.Detail.Id))
            {
                personDto.PrivacyStatus.Detail = null;
            }

            // verify the GUID exists to perform an update.  If not, perform a create instead
            if (!string.IsNullOrEmpty(personId))
            {
                PersonIntegration updatedPersonEntity = null;
                var personGuid = personDto.Id;
                try
                {
                    // verify the user has the permission to update a person
                    CheckUserPersonUpdatePermissions2(personId);

                    // map the person DTO to entities
                    var personEntity = await ConvertPerson5DtoToEntityAsync(personId, personDto);
                    IEnumerable<Domain.Base.Entities.Address> addressEntities = null;
                    IEnumerable<Domain.Base.Entities.Phone> phoneEntities = null;
                    try
                    {
                        addressEntities = await ConvertAddressDtoToAddressEntitiesAsync(personDto.Addresses);
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "persons.addresses", personEntity.Guid, personEntity.Id);
                    }
                    try
                    {
                        phoneEntities = await ConvertPhoneDtoToPhoneEntities(personDto.Phones);
                    }
                    catch (Exception ex)
                    {
                        IntegrationApiExceptionAddError(ex.Message, "persons.phones", personEntity.Guid, personEntity.Id);
                    }

                    if (IntegrationApiException != null && IntegrationApiException.Errors != null && IntegrationApiException.Errors.Any())
                    {
                        throw IntegrationApiException;
                    }

                    // update the person in the database
                    if (personEntity != null)
                    {
                        updatedPersonEntity = await _personRepository.Update2Async(personEntity, addressEntities, phoneEntities, 2);
                        if (updatedPersonEntity != null)
                        {
                            personId = updatedPersonEntity.Id;
                            personGuid = updatedPersonEntity.Guid;
                        }
                    }
                }
                catch (IntegrationApiException)
                {
                    // Drop through on Integration API exception
                }
                catch (RepositoryException ex)
                {
                    IntegrationApiExceptionAddError(ex, guid: personGuid, id: personId);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons", personGuid, personId);
                }

                // return the newly updated person
                Person5 person = null;
                if (updatedPersonEntity != null)
                    person = await ConvertPerson5EntityToDtoAsync(updatedPersonEntity, false);
                if (IntegrationApiException != null)
                {
                    throw IntegrationApiException;
                }
                return person;
            }
            else
            {
                // perform a create instead
                return await CreatePerson5Async(personDto);
            }
        }


        /// <summary>
        /// Updates a person's profile data.
        /// </summary>
        /// <param name="profileDto">Profile dto to update</param>
        /// <returns>The updated Profile dto</returns>
        [Obsolete("Obsolete as of API 1.16. Use version 2 of this method instead.")]
        public async Task<Dtos.Base.Profile> UpdateProfileAsync(Dtos.Base.Profile profileDto)
        {
            if (profileDto == null)
                throw new ArgumentNullException("profileDto", "Must provide a profile for update");

            if (string.IsNullOrEmpty(profileDto.Id))
                throw new ArgumentNullException("profileDto", "Must provide an ID for profile update");

            var profileDtoToEntityAdapter =
                _adapterRegistry.GetAdapter<Dtos.Base.Profile, Domain.Base.Entities.Profile>();
            Domain.Base.Entities.Profile profileEntity = null;
            try
            {
                profileEntity = profileDtoToEntityAdapter.MapToType(profileDto);
            }
            catch (Exception ex)
            {
                // The adapter will throw an exception if anything about the data is not acceptable to domain rules
                logger.Error("Error occurred converting profile dto to entity: " + ex.Message);
                throw;
            }

            var repoProfile = await _profileRepository.GetProfileAsync(profileEntity.Id, false);
            var configuration = await _configurationRepository.GetUserProfileConfigurationAsync();
            var userPermissions = GetUserPermissionCodes();
            ProfileProcessor.InitializeLogger(logger);
            bool isProfileChanged = false;
            Domain.Base.Entities.Profile verifiedProfile = ProfileProcessor.VerifyProfileUpdate(profileEntity,
                repoProfile, configuration, CurrentUser, userPermissions, out isProfileChanged);

            var profileEntityToDtoAdapter =
                _adapterRegistry.GetAdapter<Domain.Base.Entities.Profile, Dtos.Base.Profile>();
            Domain.Base.Entities.Profile returnedProfileEntity = null;

            // If changes detected, update the profile and return the updated item. If no changes detected, return the profile retrieved from the repository
            if (isProfileChanged)
            {
                try
                {
                    returnedProfileEntity = await _profileRepository.UpdateProfileAsync(verifiedProfile);
                }
                catch (Exception ex)
                {
                    logger.Error("Error occurred during profile update: " + ex.Message);
                    throw;
                }
            }
            else
            {
                returnedProfileEntity = repoProfile;
            }

            try
            {
                return profileEntityToDtoAdapter.MapToType(returnedProfileEntity);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred converting profile entity to dto: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Updates a person's profile data.
        /// </summary>
        /// <param name="profileDto">Profile dto to update</param>
        /// <returns>The updated Profile dto</returns>
        public async Task<Dtos.Base.Profile> UpdateProfile2Async(Dtos.Base.Profile profileDto)
        {
            if (profileDto == null)
                throw new ArgumentNullException("profileDto", "Must provide a profile for update");

            if (string.IsNullOrEmpty(profileDto.Id))
                throw new ArgumentNullException("profileDto", "Must provide an ID for profile update");

            var profileDtoToEntityAdapter =
                _adapterRegistry.GetAdapter<Dtos.Base.Profile, Domain.Base.Entities.Profile>();
            Domain.Base.Entities.Profile profileEntity = null;
            try
            {
                profileEntity = profileDtoToEntityAdapter.MapToType(profileDto);
            }
            catch (Exception ex)
            {
                // The adapter will throw an exception if anything about the data is not acceptable to domain rules
                logger.Error("Error occurred converting profile dto to entity: " + ex.Message);
                throw;
            }

            var repoProfile = await _profileRepository.GetProfileAsync(profileEntity.Id, false);
            // Update of User Profile has no need for address change request address types so using null parameter.
            var configuration = await _configurationRepository.GetUserProfileConfiguration2Async(null);
            var userPermissions = GetUserPermissionCodes();
            ProfileProcessor.InitializeLogger(logger);
            bool isProfileChanged = false;
            Domain.Base.Entities.Profile verifiedProfile = ProfileProcessor.VerifyProfileUpdate2(profileEntity,
                repoProfile, configuration, CurrentUser, userPermissions, out isProfileChanged);

            var profileEntityToDtoAdapter =
                _adapterRegistry.GetAdapter<Domain.Base.Entities.Profile, Dtos.Base.Profile>();
            Domain.Base.Entities.Profile returnedProfileEntity = null;

            // If changes detected, update the profile and return the updated item. If no changes detected, return the profile retrieved from the repository
            if (isProfileChanged)
            {
                try
                {
                    returnedProfileEntity = await _profileRepository.UpdateProfileAsync(verifiedProfile);
                }
                catch (Exception ex)
                {
                    logger.Error("Error occurred during profile update: " + ex.Message);
                    throw;
                }
            }
            else
            {
                returnedProfileEntity = repoProfile;
            }

            try
            {
                return profileEntityToDtoAdapter.MapToType(returnedProfileEntity);
            }
            catch (Exception ex)
            {
                logger.Error("Error occurred converting profile entity to dto: " + ex.Message);
                throw;
            }
        }

        #endregion

        #region Organization Methods
        //organizations are really persons so these methods are here to reuse code and service logic already in place

        /// <summary>
        /// Get person data associated with a particular role.
        /// </summary>
        /// <param name="Offset">Paging offset</param>
        /// <param name="Limit">Paging limit</param>
        /// <param name="role">Specific role of a person</param>
        /// <param name="credentialType">Credential type of either colleagueId or ssn</param>
        /// <param name="credentialValue">Specific value of the credential to be evaluated</param>
        /// <returns>List of <see cref="Dtos.Organization2">persons</see></returns>
        public async Task<Tuple<IEnumerable<Dtos.Organization2>, int>> GetOrganizations2Async(int offset, int limit,
            string role, string credentialType, string credentialValue)
        {
            // verify the user has the permission to view any person
            CheckUserOrganizationViewPermissions();

            var orgDtos = new List<Dtos.Organization2>();
            var totalcount = new int();

            // get all person guids
            var filteredTuple = await _personRepository.GetFilteredOrganizationGuidsAsync(offset, limit,
                role, credentialType, credentialValue);

            if (filteredTuple.Item1 != null && filteredTuple.Item1.Any())
            {
                totalcount = filteredTuple.Item2;
                var personEntities = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(filteredTuple.Item1);
                foreach (var personEntity in personEntities)
                {
                    orgDtos.Add(await ConvertPersonIntegrationEntityToOrganizationDtoAsync(personEntity));
                }
            }

            return new Tuple<IEnumerable<Dtos.Organization2>, int>(orgDtos, totalcount);
        }

        /// <remarks>FOR USE WITH ELLUCIAN Data Model</remarks>
        /// <summary>
        /// Get an organization from its GUID
        /// </summary>
        /// <returns>Organization2 DTO object</returns>
        public async Task<Organization2> GetOrganization2Async(string guid)
        {
            // verify the user has the permission to view any person
            CheckUserOrganizationViewPermissions();

            try
            {
                var personOrg = await _personRepository.GetPersonByGuidNonCachedAsync(guid);

                //make sure the person record retreived is a organization, not just a person
                if (!string.Equals(personOrg.PersonCorpIndicator, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    throw new KeyNotFoundException(string.Concat("Organization not found for id: ", guid));
                }

                var personIntgData = await _personRepository.GetPersonIntegrationByGuidNonCachedAsync(guid);

                return await ConvertPersonIntegrationEntityToOrganizationDtoAsync(personIntgData);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new KeyNotFoundException(string.Concat("Organization not found for id: ", guid));
            }
        }

        /// <summary>
        /// Create a Organization.
        /// </summary>
        /// <param name="organizationDto">The <see cref="Organization2">organization</see> entity to create in the database.</param>
        /// <returns>The newly created <see cref="Organization2">organization</see></returns>
        public async Task<Organization2> CreateOrganizationAsync(Organization2 organizationDto)
        {
            if (organizationDto == null)
                throw new ArgumentNullException("organizationDto", "Must provide a organization for create");
            if (string.IsNullOrEmpty(organizationDto.Id))
                throw new ArgumentNullException("organizationDto", "Must provide a guid for organization create, provide nil guid when id is not known");

            try
            {
                // verify the user has the permission to create a organization
                CheckUserOrganizationCreatePermissions();

                // Pass down the extended data dictionary
                _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                // map the organization DTO to entities
                var personEntity = await ConvertOrganization2DtoToPersonEntityAsync(null, organizationDto);
                var addressEntities = await ConvertAddressDtoToAddressEntitiesAsync(organizationDto.Addresses);
                var phoneEntities = await ConvertPhoneDtoToPhoneEntities(organizationDto.Phones);

                // update the organization in the database
                var createdOrganizationEntity = await _personRepository.CreateOrganizationAsync(personEntity, addressEntities, phoneEntities);

                // return the newly updated organization
                return await ConvertPersonIntegrationEntityToOrganizationDtoAsync(createdOrganizationEntity);
            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        /// <summary>
        /// Update a Organization.
        /// </summary>
        /// <param name="organizationDto">The <see cref="Organization2">organization</see> entity to update in the database.</param>
        /// <returns>The newly updated <see cref="Organization2">organization</see></returns>
        public async Task<Organization2> UpdateOrganizationAsync(Organization2 organizationDto)
        {
            if (organizationDto == null)
                throw new ArgumentNullException("organizationDto", "Must provide a organization for update");
            if (string.IsNullOrEmpty(organizationDto.Id))
                throw new ArgumentNullException("organizationDto", "Must provide a guid for organization update");
            try
            {

                var guidInfo = await _referenceDataRepository.GetGuidLookupResultFromGuidAsync(organizationDto.Id);

                if (guidInfo != null && !string.Equals(guidInfo.Entity, "PERSON", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException(string.Concat("The id ", organizationDto.Id.ToString(),
                        " already exists and it does not belong to an Organization. If you are trying to create with a specific id use a different id."));
                }

                // get the person ID associated with the incoming guid
                var personOrgId = await _personRepository.GetPersonIdFromGuidAsync(organizationDto.Id);

                // verify the GUID exists to perform an update.  If not, perform a create instead
                if (!string.IsNullOrEmpty(personOrgId))
                {

                    // verify the user has the permission to update a organization
                    CheckUserOrganizationUpdatePermissions();

                    // Pass down the extended data dictionary
                    _personRepository.EthosExtendedDataDictionary = EthosExtendedDataDictionary;

                    // map the organization DTO to entities
                    var personEntity = await ConvertOrganization2DtoToPersonEntityAsync(personOrgId, organizationDto);
                    var addressEntites = await ConvertAddressDtoToAddressEntitiesAsync(organizationDto.Addresses);
                    var phoneEntities = await ConvertPhoneDtoToPhoneEntities(organizationDto.Phones);

                    // update the organization in the database
                    var updatedOrganizationEntity =
                        await _personRepository.UpdateOrganizationAsync(personEntity, addressEntites, phoneEntities);

                    // return the newly updated organization
                    return await ConvertPersonIntegrationEntityToOrganizationDtoAsync(updatedOrganizationEntity);

                }
                else
                {
                    // perform a create instead
                    return await CreateOrganizationAsync(organizationDto);
                }

            }
            catch (RepositoryException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        #endregion

        #region Query Person

        /// <summary>
        /// Query person by person DTO.
        /// </summary>
        /// <param name="person">The <see cref="Person2">person</see> entity to query by.</param>
        /// <returns>List of matching <see cref="Person2">persons</see></returns>
        public async Task<IEnumerable<Dtos.Person2>> QueryPerson2ByPostAsync(Dtos.Person2 personDtoIn)
        {
            if (personDtoIn == null)
                throw new ArgumentNullException("personDtoIn", "Person required to query");
            if (personDtoIn.PersonNames == null)
                throw new ArgumentNullException("personDtoIn", "PersonNames must be defined in query");

            var personPrimaryName =
                personDtoIn.PersonNames.Where(p => p.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
            if (personPrimaryName == null || string.IsNullOrEmpty(personPrimaryName.FirstName) ||
                string.IsNullOrEmpty(personPrimaryName.LastName))
                throw new ArgumentNullException("personDtoIn", "Person first and last name required to query");

            var personBirthName =
                personDtoIn.PersonNames.Where(p => p.NameType.Category == Dtos.EnumProperties.PersonNameType2.Birth).FirstOrDefault();

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            IEnumerable<Dtos.Person2> personDtos = new List<Dtos.Person2>();

            // create a person entity of the allowable match criteria
            Domain.Base.Entities.Person personEntity = null;
            string personId = MapColleagueId2(personDtoIn.Credentials);
            personEntity = new Domain.Base.Entities.Person(personId, personPrimaryName.LastName);
            personEntity.FirstName = personPrimaryName.FirstName;
            personEntity.MiddleName = personPrimaryName.MiddleName;
            personEntity.BirthNameFirst = personBirthName == null ? string.Empty : personBirthName.FirstName;
            personEntity.BirthNameMiddle = personBirthName == null ? string.Empty : personBirthName.MiddleName;
            personEntity.BirthNameLast = personBirthName == null ? string.Empty : personBirthName.LastName;
            personEntity.BirthDate = personDtoIn.BirthDate;
            personEntity.Gender = MapGenderType2(personDtoIn.GenderType);
            personEntity.GovernmentId = MapSsn2(personDtoIn.Credentials);
            if (personDtoIn.EmailAddresses != null && personDtoIn.EmailAddresses.Count() > 0)
            {
                var emailAddressEntity = (await MapEmailAddresses2(personDtoIn.EmailAddresses)).FirstOrDefault();
                if (emailAddressEntity != null)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            // get the person matches
            var personGuids = await _personRepository.GetMatchingPersonsAsync(personEntity);

            // get the DTOs for the person matches 
            if (personGuids != null && personGuids.Count() > 0)
            {
                personDtos = await GetPerson2ByGuidNonCachedAsync(personGuids);
            }

            return personDtos;
        }

        /// <summary>
        /// Query person by person DTO.
        /// </summary>
        /// <param name="person">The <see cref="Person3">person</see> entity to query by.</param>
        /// <returns>List of matching <see cref="Person3">persons</see></returns>
        public async Task<IEnumerable<Dtos.Person3>> QueryPerson3ByPostAsync(Dtos.Person3 personDtoIn)
        {
            if (personDtoIn == null)
                throw new ArgumentNullException("personDtoIn", "Person required to query");
            if (personDtoIn.PersonNames == null)
                throw new ArgumentNullException("personDtoIn", "PersonNames must be defined in query");

            var personPrimaryName =
                personDtoIn.PersonNames.Where(p => p.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
            if (personPrimaryName == null || string.IsNullOrEmpty(personPrimaryName.FirstName) ||
                string.IsNullOrEmpty(personPrimaryName.LastName))
                throw new ArgumentNullException("personDtoIn", "Person first and last name required to query");

            var personBirthName =
                personDtoIn.PersonNames.Where(p => p.NameType.Category == Dtos.EnumProperties.PersonNameType2.Birth).FirstOrDefault();

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            IEnumerable<Dtos.Person3> personDtos = new List<Dtos.Person3>();

            // create a person entity of the allowable match criteria
            Domain.Base.Entities.Person personEntity = null;
            string personId = MapColleagueId3(personDtoIn.Credentials);
            personEntity = new Domain.Base.Entities.Person(personId, personPrimaryName.LastName);
            personEntity.FirstName = personPrimaryName.FirstName;
            personEntity.MiddleName = personPrimaryName.MiddleName;
            personEntity.BirthNameFirst = personBirthName == null ? string.Empty : personBirthName.FirstName;
            personEntity.BirthNameMiddle = personBirthName == null ? string.Empty : personBirthName.MiddleName;
            personEntity.BirthNameLast = personBirthName == null ? string.Empty : personBirthName.LastName;
            personEntity.BirthDate = personDtoIn.BirthDate;
            personEntity.Gender = MapGenderType2(personDtoIn.GenderType);
            personEntity.GovernmentId = MapSsn3(personDtoIn.Credentials);
            if (personDtoIn.EmailAddresses != null && personDtoIn.EmailAddresses.Count() > 0)
            {
                var emailAddressEntity = (await MapEmailAddresses2(personDtoIn.EmailAddresses)).FirstOrDefault();
                if (emailAddressEntity != null)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            // get the person matches
            var personGuids = await _personRepository.GetMatchingPersonsAsync(personEntity);

            // get the DTOs for the person matches 
            if (personGuids != null && personGuids.Count() > 0)
            {
                personDtos = await GetPerson3ByGuidNonCachedAsync(personGuids);
            }

            return personDtos;
        }

        /// <summary>
        /// Query person by person DTO.
        /// </summary>
        /// <param name="person">The <see cref="Person4">person</see> entity to query by.</param>
        /// <returns>List of matching <see cref="Person4">persons</see></returns>
        public async Task<IEnumerable<Dtos.Person4>> QueryPerson4ByPostAsync(Dtos.Person4 personDtoIn, bool bypassCache = false)
        {
            if (personDtoIn == null)
                throw new ArgumentNullException("personDtoIn", "Person required to query");
            if (personDtoIn.PersonNames == null)
                throw new ArgumentNullException("personDtoIn", "PersonNames must be defined in query");

            PersonName2DtoProperty personPrimaryName = null;
            try
            {
                personPrimaryName = personDtoIn.PersonNames.Where(p => p.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
            }
            catch
            {
                personPrimaryName = personDtoIn.PersonNames.FirstOrDefault();
            }
            if (personPrimaryName == null || string.IsNullOrEmpty(personPrimaryName.FirstName) ||
                string.IsNullOrEmpty(personPrimaryName.LastName))
                throw new ArgumentNullException("personDtoIn", "Person first and last name required to query");

            PersonName2DtoProperty personBirthName = null;
            try
            {
                personBirthName = personDtoIn.PersonNames.Where(p => p.NameType.Category == Dtos.EnumProperties.PersonNameType2.Birth).FirstOrDefault();
            }
            catch
            {
                // Leave personBirthName null if we can't find a match.
            }

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions();

            List<Dtos.Person4> personDtos = new List<Dtos.Person4>();

            // create a person entity of the allowable match criteria
            Domain.Base.Entities.Person personEntity = null;
            string personId = MapColleagueId4(personDtoIn.Credentials);
            personEntity = new Domain.Base.Entities.Person(personId, personPrimaryName.LastName);
            personEntity.FirstName = personPrimaryName.FirstName;
            personEntity.MiddleName = personPrimaryName.MiddleName;
            personEntity.BirthNameFirst = personBirthName == null ? string.Empty : personBirthName.FirstName;
            personEntity.BirthNameMiddle = personBirthName == null ? string.Empty : personBirthName.MiddleName;
            personEntity.BirthNameLast = personBirthName == null ? string.Empty : personBirthName.LastName;
            personEntity.BirthDate = personDtoIn.BirthDate;
            personEntity.Gender = MapGenderType2(personDtoIn.GenderType);
            personEntity.GovernmentId = MapSsn4(personDtoIn.Credentials);
            if (personDtoIn.EmailAddresses != null && personDtoIn.EmailAddresses.Count() > 0)
            {
                var emailAddressEntity = (await MapEmailAddresses2(personDtoIn.EmailAddresses)).FirstOrDefault();
                if (emailAddressEntity != null)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            // get the person matches
            var personGuids = await _personRepository.GetMatchingPersonsAsync(personEntity);

            // get the DTOs for the person matches 
            if (personGuids != null && personGuids.Any())
            {
                foreach (var guid in personGuids)
                {
                    personDtos.Add(await GetPerson4ByGuidAsync(guid, bypassCache));
                }
            }

            return personDtos;
        }

        /// <summary>
        /// Query person by person DTO.
        /// </summary>
        /// <param name="person">The <see cref="Person5">person</see> entity to query by.</param>
        /// <returns>List of matching <see cref="Person5">persons</see></returns>
        public async Task<IEnumerable<Dtos.Person5>> QueryPerson5ByPostAsync(Dtos.Person5 personDtoIn, bool bypassCache = false)
        {
            if (personDtoIn == null)
            { 
                // throw new ArgumentNullException("personDtoIn", "Person required to query");
                IntegrationApiExceptionAddError("Person required to query.", "qapi/persons");
                throw IntegrationApiException;
            }
            if (personDtoIn.PersonNames == null)
            {
                // throw new ArgumentNullException("personDtoIn", "PersonNames must be defined in query");
                IntegrationApiExceptionAddError("Names must be defined in query.", "qapi/persons.names");
                throw IntegrationApiException;
            }

            PersonName2DtoProperty personPrimaryName = null;
            try
            {
                personPrimaryName = personDtoIn.PersonNames.Where(p => p.NameType.Category == Dtos.EnumProperties.PersonNameType2.Legal).FirstOrDefault();
            }
            catch
            {
                personPrimaryName = personDtoIn.PersonNames.FirstOrDefault();
            }
            if (personPrimaryName == null || string.IsNullOrEmpty(personPrimaryName.FirstName) ||
                string.IsNullOrEmpty(personPrimaryName.LastName))
            {
                // throw new ArgumentNullException("personDtoIn", "Person first and last name required to query");
                var code = "qapi/persons.names.firstName";
                if (string.IsNullOrEmpty(personPrimaryName.LastName)) code = "qapi/person.names.lastName";
                IntegrationApiExceptionAddError("Person first and last name are required for matching.", code);
                throw IntegrationApiException;
            }

            PersonName2DtoProperty personBirthName = null;
            try
            {
                personBirthName = personDtoIn.PersonNames.Where(p => p.NameType.Category == Dtos.EnumProperties.PersonNameType2.Birth).FirstOrDefault();
            }
            catch
            {
                // Leave personBirthName null if we can't find a match.
            }

            // verify the user has the permission to view any person
            CheckUserPersonViewPermissions2();

            List<Dtos.Person5> personDtos = new List<Dtos.Person5>();

            // create a person entity of the allowable match criteria
            Domain.Base.Entities.Person personEntity = null;
            string personId = MapColleagueId4(personDtoIn.Credentials);
            personEntity = new Domain.Base.Entities.Person(personId, personPrimaryName.LastName);
            personEntity.FirstName = personPrimaryName.FirstName;
            personEntity.MiddleName = personPrimaryName.MiddleName;
            personEntity.BirthNameFirst = personBirthName == null ? string.Empty : personBirthName.FirstName;
            personEntity.BirthNameMiddle = personBirthName == null ? string.Empty : personBirthName.MiddleName;
            personEntity.BirthNameLast = personBirthName == null ? string.Empty : personBirthName.LastName;
            personEntity.BirthDate = personDtoIn.BirthDate;
            personEntity.Gender = MapGenderType2(personDtoIn.GenderType);
            personEntity.GovernmentId = MapSsn4(personDtoIn.Credentials);
            if (personDtoIn.EmailAddresses != null && personDtoIn.EmailAddresses.Count() > 0)
            {
                var emailAddressEntity = (await MapEmailAddresses3(personDtoIn.EmailAddresses)).FirstOrDefault();
                if (emailAddressEntity != null)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            // get the person matches
            IEnumerable<string> personGuids = null;
            try
            {
                personGuids = await _personRepository.GetMatchingPersons2Async(personEntity);
            }
            catch (RepositoryException ex)
            {
                IntegrationApiExceptionAddError(ex);
            }

            // get the DTOs for the person matches 
            if (personGuids != null && personGuids.Any())
            {
                foreach (var guid in personGuids)
                {
                    try
                    {
                        personDtos.Add(await GetPerson5ByGuidAsync(guid, bypassCache));
                    }
                    catch (RepositoryException ex)
                    {
                        IntegrationApiExceptionAddError(ex);
                    }
                }
            }
            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }

            return personDtos;
        }

        /// <summary>
        /// Query person by criteria and return the results of the matching algorithm
        /// </summary>
        /// <param name="person">The <see cref="Dtos.Base.PersonMatchCriteria">criteria</see> to query by.</param>
        /// <returns>List of matching <see cref="Dtos.Base.PersonMatchResult">results</see></returns>
        public async Task<IEnumerable<Dtos.Base.PersonMatchResult>> QueryPersonMatchResultsByPostAsync(
            Dtos.Base.PersonMatchCriteria criteria)
        {
            if (criteria == null)
                throw new ArgumentNullException("criteria", "Criteria required to query");

            // Remove any invalid names - for example, if the Former Name fields were not supplied/filled in, 
            // or if a first name was sent without a corresponding last name.
            if (criteria.MatchNames != null)
            {
                criteria.MatchNames =
                    criteria.MatchNames.Where(
                        n => !string.IsNullOrEmpty(n.FamilyName) && !string.IsNullOrEmpty(n.GivenName));
            }

            var adapter =
                _adapterRegistry.GetAdapter<Dtos.Base.PersonMatchCriteria, Domain.Base.Entities.PersonMatchCriteria>();
            var entCriteria = adapter.MapToType(criteria);
            var entResults = await _personRepository.GetMatchingPersonResultsAsync(entCriteria);

            var results = new List<Dtos.Base.PersonMatchResult>();
            if (entResults != null && entResults.Any())
            {
                var dtoAdapter =
                    _adapterRegistry.GetAdapter<Domain.Base.Entities.PersonMatchResult, Dtos.Base.PersonMatchResult>();
                results = entResults.Select(x => dtoAdapter.MapToType(x)).ToList();
            }

            return results;
        }

        /// <summary>
        /// Retrieves the matching Persons for the ids provided or searches keyword
        /// for the matching Persons if a first and last name are provided.  
        /// In the latter case, a middle name is optional.
        /// Matching is done by partial name; i.e., 'Bro' will match 'Brown' or 'Brodie'. 
        /// Capitalization is ignored.
        /// </summary>
        /// <remarks>the following keyword input is legal
        /// <list type="bullet">
        /// <item>a Colleague id.  Short ids will be zero-padded.</item>
        /// <item>First Last</item>
        /// <item>First Middle Last</item>
        /// <item>Last, First</item>
        /// <item>Last, First Middle</item>
        /// </list>
        /// </remarks>
        /// <param name="criteria">Keyword can either be a Person ID or a first and last name.  A middle name is optional.</param>
        /// <returns>An enumeration of <see cref="Dtos.Base.Person">Person</see> with populated ID and first, middle and last names</returns>
        /// <exception cref="ArgumentNullException">Criteria must be provided</exception>
        /// <exception cref="PermissionsException">Person must have permissions to search for persons</exception>
        public async Task<IEnumerable<Dtos.Base.Person>> QueryPersonNamesByPostAsync(PersonNameQueryCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException("criteria", "criteria must be provided to search for persons.");
            }

            CheckUserNameSearchPermissions();

            var personBases = await _personBaseRepository.SearchByIdsOrNamesAsync(criteria.Ids, criteria.QueryKeyword);
            var results = new List<Dtos.Base.Person>();

            foreach (var personBase in personBases)
            {
                var personDto = new Dtos.Base.Person()
                {
                    Id = personBase.Id,
                    FirstName = personBase.FirstName,
                    MiddleName = personBase.MiddleName,
                    LastName = personBase.LastName,
                    BirthNameFirst = personBase.BirthNameFirst,
                    BirthNameMiddle = personBase.BirthNameMiddle,
                    BirthNameLast = personBase.BirthNameLast,
                    PreferredName = personBase.PreferredName,
                    PrivacyStatusCode = personBase.PrivacyStatusCode
                };
                results.Add(personDto);
            }
            return results;
        }

        #endregion

        #region Permissions

        /// <summary>
        /// Verifies if the user has the correct permission to view the person.
        /// </summary>
        private void CheckUserPersonViewPermissions(string personId)
        {
            // access is ok if the current user is the person being viewed
            if (!CurrentUser.IsPerson(personId))
            {
                // not the current user, must have view any person permission
                CheckUserPersonViewPermissions();
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permission to view the person.
        /// </summary>
        private void CheckUserPersonViewPermissions2(string personId)
        {
            // access is ok if the current user is the person being viewed
            if (!CurrentUser.IsPerson(personId))
            {
                // not the current user, must have view any person permission
                CheckUserPersonViewPermissions2();
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permission to view any person.
        /// </summary>
        private void CheckUserPersonViewPermissions()
        {
            // access is ok if the current user has the view any person permission
            if (!HasPermission(BasePermissionCodes.ViewAnyPerson) && !HasPermission(BasePermissionCodes.CreatePerson) && !HasPermission(BasePermissionCodes.UpdatePerson))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view any person.");
                throw new PermissionsException("User is not authorized to view any person.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permission to view any person.
        /// </summary>
        private void CheckUserPersonViewPermissions2()
        {
            // access is ok if the current user has the view any person permission
            if (!HasPermission(BasePermissionCodes.ViewAnyPerson) && !HasPermission(BasePermissionCodes.CreatePerson) && !HasPermission(BasePermissionCodes.UpdatePerson))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view any person.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to view any person.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to create a organization.
        /// </summary>
        private void CheckUserOrganizationCreatePermissions()
        {
            // access is ok if the current user has the create organization permission
            if (!HasPermission(BasePermissionCodes.CreateOrganization))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create an organization.");
                throw new PermissionsException("User is not authorized to create an organization.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to update a organization.
        /// </summary>
        private void CheckUserOrganizationUpdatePermissions()
        {
            // access is ok if the current user has the update organization permission
            if (!HasPermission(BasePermissionCodes.UpdateOrganization))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update an organization.");
                throw new PermissionsException("User is not authorized to update an organization.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to view a organization.
        /// </summary>
        private void CheckUserOrganizationViewPermissions()
        {
            // access is ok if the current user has the view organization permission
            if (!HasPermission(BasePermissionCodes.ViewOrganization) && !HasPermission(BasePermissionCodes.CreateOrganization) && !HasPermission(BasePermissionCodes.UpdateOrganization))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view an organization.");
                throw new PermissionsException("User is not authorized to view an organization.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to create a person.
        /// </summary>
        private void CheckUserPersonCreatePermissions()
        {
            // access is ok if the current user has the create person permission
            if (!HasPermission(BasePermissionCodes.CreatePerson))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create a person.");
                throw new PermissionsException("User is not authorized to create a person.");
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to create a person.
        /// </summary>
        private void CheckUserPersonCreatePermissions2()
        {
            // access is ok if the current user has the create person permission
            if (!HasPermission(BasePermissionCodes.CreatePerson))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to create a person.");
                IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to create a person.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                throw IntegrationApiException;
            }
        }

        /// <summary>
        /// Verifies if the user has the correct permissions to update a person.
        /// </summary>
        private void CheckUserPersonUpdatePermissions(string personId)
        {
            // access is ok if the current user is the person being updated
            if (!CurrentUser.IsPerson(personId))
            {
                // access is ok if the current user has the update person permission
                if (!HasPermission(BasePermissionCodes.UpdatePerson))
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update a person.");
                    throw new PermissionsException("User is not authorized to update a person.");
                }
            }
        }
        /// <summary>
        /// Verifies if the user has the correct permissions to update a person.
        /// </summary>
        private void CheckUserPersonUpdatePermissions2(string personId)
        {
            // access is ok if the current user is the person being updated
            if (!CurrentUser.IsPerson(personId))
            {
                // access is ok if the current user has the update person permission
                if (!HasPermission(BasePermissionCodes.UpdatePerson))
                {
                    logger.Error("User '" + CurrentUser.UserId + "' is not authorized to update a person.");
                    IntegrationApiExceptionAddError("User '" + CurrentUser.UserId + "' is not authorized to update a person.", "Access.Denied", httpStatusCode: System.Net.HttpStatusCode.Forbidden);
                    throw IntegrationApiException;
                }
            }
        }

        /// <summary>
        /// Verifies if the user is permitted to view another person's profile
        /// A person can only view a profile for:
        /// 1. Himself/herself
        /// 2. Someone with whom they have an ERP-recognized relationship
        /// 3. Someone who currently has permission to proxy for the user
        /// </summary>
        private async Task<bool> UserCanViewProfileForPerson(string personId)
        {
            if (!CurrentUser.IsPerson(personId) && !HasProxyAccessForPerson(personId))
            {
                var relationshipIds = await _relationshipRepository.GetRelatedPersonIdsAsync(CurrentUser.PersonId);
                if (relationshipIds.Contains(personId))
                {
                    return true;
                }
                var proxyUsers = await _proxyRepository.GetUserProxyPermissionsAsync(CurrentUser.PersonId);
                if (proxyUsers.Select(pu => pu.Id).Contains(personId))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Verifies if a user has the correct permissions to search for person names.
        /// </summary>
        private void CheckUserNameSearchPermissions()
        {
            if (!HasPermission(BasePermissionCodes.ViewAnyPerson) &&
                !HasPermission(BasePermissionCodes.ViewPersonEmergencyContacts) &&
                !HasPermission(BasePermissionCodes.ViewPersonHealthConditions) &&
                !HasPermission(BasePermissionCodes.ViewPersonOtherEmergencyInformation))
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to search for a person name.");
                throw new PermissionsException("User is not authorized to search for a person name.");
            }
        }

        #endregion

        #region Convert Person Entity to Dto Methods

        private async Task<IEnumerable<Dtos.DtoProperties.CredentialDtoProperty>> GetPersonCredentials(
            Domain.Base.Entities.Person person)
        {
            // Colleague Person ID
            var credentials = new List<Dtos.DtoProperties.CredentialDtoProperty>()
            {
                new Dtos.DtoProperties.CredentialDtoProperty()
                {
                    Type = Dtos.EnumProperties.CredentialType.ColleaguePersonId,
                    Value = person.Id
                }
            };
            // Elevate ID
            if (person.PersonAltIds != null && person.PersonAltIds.Count() > 0)
            {
                var elevPersonAltIdList =
    person.PersonAltIds.Where(
        a => a.Type == Domain.Base.Entities.PersonAlt.ElevatePersonAltType);
                if (elevPersonAltIdList != null && elevPersonAltIdList.Any())
                {
                    foreach (var elevPersonAltId in elevPersonAltIdList)
                    {
                        if (elevPersonAltId != null && !string.IsNullOrEmpty(elevPersonAltId.Id))
                        {
                            credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty()
                            {
                                Type = Dtos.EnumProperties.CredentialType.ElevateID,
                                Value = elevPersonAltId.Id
                            });
                        }
                    }
                }
            }
            // SSN
            if (!string.IsNullOrEmpty(person.GovernmentId))
            {
                var type = Dtos.EnumProperties.CredentialType.Sin;
                var countryCode = await _personRepository.GetHostCountryAsync();
                if (countryCode.Equals("USA", StringComparison.OrdinalIgnoreCase))
                {
                    type = Dtos.EnumProperties.CredentialType.Ssn;
                }
                credentials.Add(new Dtos.DtoProperties.CredentialDtoProperty()
                {
                    Type = type,
                    Value = person.GovernmentId
                });
            }
            return credentials;
        }

        #endregion

        #region Convert Person HEDM V6 Entity to Dto Methods

        private async Task<Dtos.Person2> ConvertPerson2EntityToDtoAsync(
            Domain.Base.Entities.PersonIntegration personEntity, bool getLimitedData, bool bypassCache = false)
        {
            // create the person DTO
            var personDto = new Dtos.Person2();

            personDto.Id = personEntity.Guid;
            personDto.PrivacyStatus = await GetPersonPrivacyAsync(personEntity.PrivacyStatus, personEntity.PrivacyStatusCode, bypassCache);
            personDto.PersonNames = await GetPersonNamesAsync(personEntity, bypassCache);
            personDto.BirthDate = personEntity.BirthDate;
            personDto.DeceasedDate = personEntity.DeceasedDate;
            if (!string.IsNullOrEmpty(personEntity.Gender)) personDto.GenderType = GetGenderType2(personEntity.Gender);
            personDto.Religion = (!string.IsNullOrEmpty(personEntity.Religion)
                ? await GetPersonReligionGuidAsync(personEntity.Religion, bypassCache)
                : null);
            personDto.Ethnicity = (personEntity.EthnicCodes != null && personEntity.EthnicCodes.Any()
                ? await GetPersonEthnicityGuidAsync(personEntity.EthnicCodes, bypassCache)
                : null);
            personDto.Races = (personEntity.RaceCodes != null && personEntity.RaceCodes.Any()
                ? await GetPersonRaceGuidsAsync(personEntity.RaceCodes, bypassCache)
                : null);
            if ((personEntity.PrimaryLanguage != null) || (personEntity.SecondaryLanguages != null && personEntity.SecondaryLanguages.Any()))
            {
                var languages = await GetPersonLanguagesAsync(personEntity.PrimaryLanguage, personEntity.SecondaryLanguages, personDto.Id);
                if (languages != null && languages.Any())
                {
                    personDto.Languages = languages;
                }
            }
            if (!string.IsNullOrEmpty(personEntity.MaritalStatusCode))
                personDto.MaritalStatus = await GetPersonMaritalStatusGuidAsync(personEntity.MaritalStatusCode, bypassCache);
            if (personEntity.AlienStatus != null)
                personDto.CitizenshipStatus = await GetPersonCitizenshipAsync(personEntity.AlienStatus, bypassCache);
            if (!string.IsNullOrEmpty(personEntity.BirthCountry))
                personDto.CountryOfBirth = await GetPersonCountryAsync(personEntity.BirthCountry, bypassCache);
            if (!string.IsNullOrEmpty(personEntity.Citizenship))
                personDto.CitizenshipCountry = await GetPersonCountryAsync(personEntity.Citizenship, bypassCache);
            personDto.Roles = await GetPersonRolesAsync(personEntity.Id, personEntity.Roles);
            if (personEntity.Passport != null || personEntity.DriverLicense != null || (personEntity.IdentityDocuments != null && personEntity.IdentityDocuments.Any()))
            {
                personDto.IdentityDocuments = await GetPersonIdentityDocumentsAsync(personEntity, bypassCache);
            }

            var creds = await GetPersonCredentials(personEntity);
            var credDto = new List<CredentialDtoProperty>();
            foreach (var cred in creds)
            {
                credDto.Add(cred);
            }
            personDto.Credentials = credDto.AsEnumerable();

            if ((personEntity.Interests != null) && (personEntity.Interests.Any()))
                personDto.Interests = await GetPersonInterestsAsync(personEntity.Interests, bypassCache);

            // get addresses, email addresses and phones
            List<Domain.Base.Entities.EmailAddress> emailEntities = personEntity.EmailAddresses;
            List<Domain.Base.Entities.Phone> phoneEntities = personEntity.Phones;
            List<Domain.Base.Entities.Address> addressEntities = personEntity.Addresses;
            List<Domain.Base.Entities.SocialMedia> socialMediaEntities = personEntity.SocialMedia;

            var emailAddresses = await GetEmailAddresses2(emailEntities, bypassCache);
            if ((emailAddresses != null) && (emailAddresses.Any())) personDto.EmailAddresses = emailAddresses;
            var addresses = await GetAddresses2Async(addressEntities, bypassCache);
            if ((addresses != null) && (addresses.Any())) personDto.Addresses = addresses;
            var phoneNumbers = await GetPhones2Async(phoneEntities, bypassCache);
            if ((phoneNumbers != null) && (phoneNumbers.Any())) personDto.Phones = phoneNumbers;
            if ((socialMediaEntities != null) && (socialMediaEntities.Any()))
                personDto.SocialMedia = await GetPersonSocialMediaAsync(socialMediaEntities, bypassCache);

            return personDto;
        }

        private async Task<Dtos.Person3> ConvertPerson3EntityToDtoAsync(
            Domain.Base.Entities.PersonIntegration personEntity, bool getLimitedData, bool bypassCache = false)
        {
            // create the person DTO
            var personDto = new Dtos.Person3();

            personDto.Id = personEntity.Guid;
            personDto.PrivacyStatus = await GetPersonPrivacyAsync(personEntity.PrivacyStatus, personEntity.PrivacyStatusCode, bypassCache);
            personDto.PersonNames = await GetPersonNamesAsync(personEntity, bypassCache);
            personDto.BirthDate = personEntity.BirthDate;
            personDto.DeceasedDate = personEntity.DeceasedDate;
            if (!string.IsNullOrEmpty(personEntity.Gender)) personDto.GenderType = GetGenderType2(personEntity.Gender);
            personDto.Religion = (!string.IsNullOrEmpty(personEntity.Religion)
                ? await GetPersonReligionGuidAsync(personEntity.Religion)
                : null);
            personDto.Ethnicity = (personEntity.EthnicCodes != null && personEntity.EthnicCodes.Any()
                ? await GetPersonEthnicityGuidAsync(personEntity.EthnicCodes)
                : null);
            personDto.Races = (personEntity.RaceCodes != null && personEntity.RaceCodes.Any()
                ? await GetPersonRaceGuidsAsync(personEntity.RaceCodes)
                : null);
            if ((personEntity.PrimaryLanguage != null) || (personEntity.SecondaryLanguages != null && personEntity.SecondaryLanguages.Any()))
            {
                var languages = await GetPersonLanguagesAsync(personEntity.PrimaryLanguage, personEntity.SecondaryLanguages, personDto.Id);
                if (languages != null && languages.Any())
                {
                    personDto.Languages = languages;
                }
            }
            if (!string.IsNullOrEmpty(personEntity.MaritalStatusCode))
                personDto.MaritalStatus = await GetPersonMaritalStatusGuidAsync(personEntity.MaritalStatusCode);
            if (personEntity.AlienStatus != null)
                personDto.CitizenshipStatus = await GetPersonCitizenshipAsync(personEntity.AlienStatus, bypassCache);
            if (!string.IsNullOrEmpty(personEntity.BirthCountry))
                personDto.CountryOfBirth = await GetPersonCountryAsync(personEntity.BirthCountry);
            if (!string.IsNullOrEmpty(personEntity.Citizenship))
                personDto.CitizenshipCountry = await GetPersonCountryAsync(personEntity.Citizenship);
            personDto.Roles = await GetPersonRolesAsync(personEntity.Id, personEntity.Roles);
            if (personEntity.Passport != null || personEntity.DriverLicense != null || (personEntity.IdentityDocuments != null && personEntity.IdentityDocuments.Any()))
            {
                personDto.IdentityDocuments = await GetPersonIdentityDocumentsAsync(personEntity, bypassCache);
            }
            string[] personGuids = { personEntity.Guid };
            await this.GetPersonPins(personGuids);

            var creds = await GetPersonCredentials2(personEntity);
            var credDto = new List<CredentialDtoProperty2>();
            foreach (var cred in creds)
            {
                credDto.Add(cred);
            }
            personDto.Credentials = credDto.AsEnumerable();
            if ((personEntity.Interests != null) && (personEntity.Interests.Any()))
                personDto.Interests = await GetPersonInterestsAsync(personEntity.Interests, bypassCache);

            // get addresses, email addresses and phones
            List<Domain.Base.Entities.EmailAddress> emailEntities = personEntity.EmailAddresses;
            List<Domain.Base.Entities.Phone> phoneEntities = personEntity.Phones;
            List<Domain.Base.Entities.Address> addressEntities = personEntity.Addresses;
            List<Domain.Base.Entities.SocialMedia> socialMediaEntities = personEntity.SocialMedia;

            var emailAddresses = await GetEmailAddresses2(emailEntities, bypassCache);
            if ((emailAddresses != null) && (emailAddresses.Any())) personDto.EmailAddresses = emailAddresses;
            var addresses = await GetAddresses2Async(addressEntities, bypassCache);
            if ((addresses != null) && (addresses.Any())) personDto.Addresses = addresses;


            var phoneNumbers = await GetPhones2Async(phoneEntities, bypassCache);
            if ((phoneNumbers != null) && (phoneNumbers.Any())) personDto.Phones = phoneNumbers;
            if ((socialMediaEntities != null) && (socialMediaEntities.Any()))
                personDto.SocialMedia = await GetPersonSocialMediaAsync(socialMediaEntities, bypassCache);

            return personDto;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonNameDtoProperty>> GetPersonNamesAsync(
            Domain.Base.Entities.PersonIntegration person, bool bypassCache = false)
        {
            List<Dtos.DtoProperties.PersonNameDtoProperty> personNames =
                new List<Dtos.DtoProperties.PersonNameDtoProperty>();

            var preferredNameType = person.PreferredNameType;

            // Legal Name for a person
            var personNameTypeItem = (await GetPersonNameTypesAsync(bypassCache)).FirstOrDefault(
                pn => pn.Code == "LEGAL");
            if (personNameTypeItem != null)
            {
                var personName = new Dtos.DtoProperties.PersonNameDtoProperty()
                {
                    NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                    {
                        Category = Dtos.EnumProperties.PersonNameType2.Legal,
                        Detail =
                            new Dtos.GuidObject2(
                                    personNameTypeItem.Guid)
                    },
                    FullName =
                        !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("LEGAL", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                        BuildFullName("FM", person.Prefix, person.FirstName, person.MiddleName,
                            person.LastName, person.Suffix),
                    Preference = (string.IsNullOrEmpty(preferredNameType) || preferredNameType.Equals("LEGAL", StringComparison.CurrentCultureIgnoreCase)) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    Title = string.IsNullOrEmpty(person.Prefix) ? null : person.Prefix,
                    FirstName = string.IsNullOrEmpty(person.FirstName) ? null : person.FirstName,
                    MiddleName = string.IsNullOrEmpty(person.MiddleName) ? null : person.MiddleName,
                    LastName = string.IsNullOrEmpty(person.LastName) ? null : person.LastName,
                    LastNamePrefix =
                        person.LastName.Contains(" ")
                            ? person.LastName.Split(" ".ToCharArray())[0].ToString()
                            : null,
                    Pedigree = string.IsNullOrEmpty(person.Suffix) ? null : person.Suffix,
                    ProfessionalAbbreviation =
                        person.ProfessionalAbbreviations.Any() ? person.ProfessionalAbbreviations : null
                };
                personNames.Add(personName);
            }

            // Birth Name
            if (!string.IsNullOrEmpty(person.BirthNameLast) || !string.IsNullOrEmpty(person.BirthNameFirst) ||
                !string.IsNullOrEmpty(person.BirthNameMiddle))
            {
                var birthNameTypeItem = (await GetPersonNameTypesAsync(bypassCache))
                    .FirstOrDefault(pn => pn.Code == "BIRTH");
                if (birthNameTypeItem != null)
                {
                    var birthName = new Dtos.DtoProperties.PersonNameDtoProperty()
                    {
                        NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.PersonNameType2.Birth,
                            Detail =
                                new Dtos.GuidObject2(
                                        birthNameTypeItem.Guid)
                        },
                        FullName =
                            !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("BIRTH", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                            BuildFullName("FM", "", person.BirthNameFirst, person.BirthNameMiddle, person.BirthNameLast, ""),
                        FirstName = string.IsNullOrEmpty(person.BirthNameFirst) ? null : person.BirthNameFirst,
                        MiddleName = string.IsNullOrEmpty(person.BirthNameMiddle) ? null : person.BirthNameMiddle,
                        LastName = string.IsNullOrEmpty(person.BirthNameLast) ? null : person.BirthNameLast,
                        LastNamePrefix =
                            person.BirthNameLast.Contains(" ")
                                ? person.BirthNameLast.Split(" ".ToCharArray())[0].ToString()
                                    : null,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("BIRTH", StringComparison.CurrentCultureIgnoreCase) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };
                    personNames.Add(birthName);
                }
            }

            // Chosen Name
            if (!string.IsNullOrEmpty(person.ChosenLastName) || !string.IsNullOrEmpty(person.ChosenFirstName) ||
                !string.IsNullOrEmpty(person.ChosenMiddleName))
            {
                var chosenNameTypeItem = (await GetPersonNameTypesAsync(bypassCache))
                    .FirstOrDefault(pn => pn.Code == "CHOSEN");
                if (chosenNameTypeItem != null)
                {
                    var chosenName = new Dtos.DtoProperties.PersonNameDtoProperty()
                    {
                        NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.PersonNameType2.Personal,
                            Detail =
                                new Dtos.GuidObject2(
                                        chosenNameTypeItem.Guid)
                        },
                        FullName =
                            !string.IsNullOrEmpty(preferredNameType) &&
                            preferredNameType.Equals("CHOSEN", StringComparison.CurrentCultureIgnoreCase) &&
                            !string.IsNullOrEmpty(person.PreferredName) ?
                            person.PreferredName :
                            BuildFullName("FM", "", person.ChosenFirstName, person.ChosenMiddleName, person.ChosenLastName, ""),
                        FirstName = string.IsNullOrEmpty(person.ChosenFirstName) ? null : person.ChosenFirstName,
                        MiddleName = string.IsNullOrEmpty(person.ChosenMiddleName) ? null : person.ChosenMiddleName,
                        LastName = string.IsNullOrEmpty(person.ChosenLastName) ? null : person.ChosenLastName,
                        LastNamePrefix =
                            person.ChosenLastName.Contains(" ")
                                ? person.ChosenLastName.Split(" ".ToCharArray())[0].ToString()
                                    : null,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("CHOSEN", StringComparison.CurrentCultureIgnoreCase) ?
                            Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };
                    personNames.Add(chosenName);
                }
            }

            // Nickname
            if (!string.IsNullOrEmpty(person.Nickname))
            {
                var nickNameTypeItem = (await GetPersonNameTypesAsync(false))
                    .FirstOrDefault(pn => pn.Code == "NICKNAME");
                if (nickNameTypeItem != null)
                {
                    var nickName = new Dtos.DtoProperties.PersonNameDtoProperty()
                    {
                        NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                        {
                            Category = Dtos.EnumProperties.PersonNameType2.Personal,
                            Detail =
                                new Dtos.GuidObject2(
                                        nickNameTypeItem.Guid)
                        },
                        FullName = person.Nickname,
                        Preference = !string.IsNullOrEmpty(preferredNameType) && preferredNameType.Equals("NICKNAME", StringComparison.CurrentCultureIgnoreCase) ?
                           Dtos.EnumProperties.PersonNamePreference.Preferred : (PersonNamePreference?)null,
                    };

                    personNames.Add(nickName);
                }
            }

            // Name History
            if ((person.FormerNames != null) && (person.FormerNames.Any()))
            {
                var historyNameTypeItem = (await GetPersonNameTypesAsync(false))
                    .FirstOrDefault(pn => pn.Code == "HISTORY");
                if (historyNameTypeItem != null)
                {
                    foreach (var name in person.FormerNames)
                    {
                        var formerName = new Dtos.DtoProperties.PersonNameDtoProperty()
                        {
                            NameType = new Dtos.DtoProperties.PersonNameTypeDtoProperty()
                            {
                                Category = Dtos.EnumProperties.PersonNameType2.Personal,
                                Detail =
                                    new Dtos.GuidObject2(
                                            historyNameTypeItem.Guid)
                            },
                            FullName = BuildFullName("FM", "", name.GivenName, name.MiddleName, name.FamilyName, ""),
                            FirstName = string.IsNullOrEmpty(name.GivenName) ? null : name.GivenName,
                            MiddleName = string.IsNullOrEmpty(name.MiddleName) ? null : name.MiddleName,
                            LastName = string.IsNullOrEmpty(name.FamilyName) ? null : name.FamilyName,
                            LastNamePrefix =
                                name.FamilyName.Contains(" ")
                                    ? name.FamilyName.Split(" ".ToCharArray())[0].ToString()
                                    : null
                        };
                        personNames.Add(formerName);
                    }
                }
            }
            return personNames;
        }

        private string BuildFullName(string preferredName, string prefix, string first, string middle, string last,
            string suffix)
        {
            string fullName = "";
            if (string.IsNullOrEmpty(preferredName)) preferredName = "FM";
            if ((first != null) && (first.Length == 1)) first = string.Concat(first, ".");
            if ((middle != null) && (middle.Length == 1)) middle = string.Concat(middle, ".");
            var firstInitial = !string.IsNullOrEmpty(first) ? string.Concat(first.Remove(1), ". ") : string.Empty;
            var middleInitial = !string.IsNullOrEmpty(middle) ? string.Concat(middle.Remove(1), ". ") : string.Empty;
            if (!string.IsNullOrEmpty(suffix)) suffix = string.Concat(", ", suffix);
            first = !string.IsNullOrEmpty(first) ? string.Concat(first, " ") : string.Empty;
            middle = !string.IsNullOrEmpty(middle) ? string.Concat(middle, " ") : string.Empty;
            prefix = !string.IsNullOrEmpty(prefix) ? string.Concat(prefix, " ") : string.Empty;

            switch (preferredName.ToUpper())
            {
                case ("IM"):
                    fullName = string.Concat(prefix, firstInitial, middle, last, suffix);
                    break;
                case ("II"):
                    fullName = string.Concat(prefix, firstInitial, middleInitial, last, suffix);
                    break;
                case ("FM"):
                    fullName = string.Concat(prefix, first, middle, last, suffix);
                    break;
                case ("FI"):
                    fullName = string.Concat(prefix, first, middleInitial, last, suffix);
                    break;
                default:
                    fullName = preferredName;
                    break;
            }
            return fullName.Trim();
        }

        private Dtos.EnumProperties.GenderType2? GetGenderType2(string gender)
        {
            switch (gender)
            {
                case "M":
                    {
                        return Dtos.EnumProperties.GenderType2.Male;
                    }
                case "F":
                    {
                        return Dtos.EnumProperties.GenderType2.Female;
                    }
                default:
                    {
                        return Dtos.EnumProperties.GenderType2.Unknown;
                    }
            }
        }

        private async Task<Dtos.DtoProperties.PersonMaritalStatusDtoProperty> GetPersonMaritalStatusGuidAsync(
            string maritalStatusCode, bool bypassCache = false)
        {
            Dtos.DtoProperties.PersonMaritalStatusDtoProperty maritalStatusDto =
                new Dtos.DtoProperties.PersonMaritalStatusDtoProperty();
            // get the marital status guid and category
            if (!string.IsNullOrEmpty(maritalStatusCode))
            {
                var maritalStatusEntity =
                    (await GetMaritalStatusesAsync(bypassCache)).FirstOrDefault(
                        m => m.Code == maritalStatusCode);
                if (maritalStatusEntity != null && !string.IsNullOrEmpty(maritalStatusEntity.Guid))
                {
                    maritalStatusDto.Detail = new Dtos.GuidObject2(maritalStatusEntity.Guid);
                    switch (maritalStatusEntity.Type)
                    {
                        case (Ellucian.Colleague.Domain.Base.Entities.MaritalStatusType.Single):
                            maritalStatusDto.MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Single;
                            break;
                        case (Ellucian.Colleague.Domain.Base.Entities.MaritalStatusType.Married):
                            maritalStatusDto.MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Married;
                            break;
                        case (Ellucian.Colleague.Domain.Base.Entities.MaritalStatusType.Divorced):
                            maritalStatusDto.MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Divorced;
                            break;
                        case (Ellucian.Colleague.Domain.Base.Entities.MaritalStatusType.Separated):
                            maritalStatusDto.MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Separated;
                            break;
                        case (Ellucian.Colleague.Domain.Base.Entities.MaritalStatusType.Widowed):
                            maritalStatusDto.MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Widowed;
                            break;
                        default:
                            maritalStatusDto.MaritalCategory = Dtos.EnumProperties.PersonMaritalStatusCategory.Single;
                            break;
                    }
                }
            }
            return maritalStatusDto;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonRaceDtoProperty>> GetPersonRaceGuidsAsync(
            IEnumerable<string> raceCodes, bool bypassCache = false)
        {
            List<Dtos.DtoProperties.PersonRaceDtoProperty> raceDtos =
                new List<Dtos.DtoProperties.PersonRaceDtoProperty>();

            // get the race guids and category
            if (raceCodes != null && raceCodes.Count() > 0)
            {
                foreach (var raceCode in raceCodes)
                {
                    Dtos.DtoProperties.PersonRaceDtoProperty raceDto = new Dtos.DtoProperties.PersonRaceDtoProperty();

                    var raceEntity =
                        (await GetRacesAsync(bypassCache)).FirstOrDefault(r => r.Code == raceCode);
                    if (raceEntity != null && !string.IsNullOrEmpty(raceEntity.Guid))
                    {
                        raceDto.Race = new Dtos.GuidObject2(raceEntity.Guid);
                        var raceReportingItem = new Dtos.DtoProperties.PersonRaceReporting();
                        raceReportingItem.Country = new Dtos.DtoProperties.PersonRaceReportingCountry()
                        {
                            Code = Dtos.CountryCodeType.USA,
                            RacialCategory = null
                        };

                        switch (raceEntity.Type)
                        {
                            case (RaceType.AmericanIndian):
                                raceReportingItem.Country.RacialCategory =
                                    Dtos.EnumProperties.PersonRaceCategory.AmericanIndianOrAlaskaNative;
                                break;
                            case (RaceType.Asian):
                                raceReportingItem.Country.RacialCategory = Dtos.EnumProperties.PersonRaceCategory.Asian;
                                break;
                            case (RaceType.Black):
                                raceReportingItem.Country.RacialCategory =
                                    Dtos.EnumProperties.PersonRaceCategory.BlackOrAfricanAmerican;
                                break;
                            case (RaceType.PacificIslander):
                                raceReportingItem.Country.RacialCategory =
                                    Dtos.EnumProperties.PersonRaceCategory.HawaiianOrPacificIslander;
                                break;
                            case (RaceType.White):
                                raceReportingItem.Country.RacialCategory = Dtos.EnumProperties.PersonRaceCategory.White;
                                break;
                        }
                        if (raceEntity.Type != null)
                        {
                            var raceReporting = new List<Dtos.DtoProperties.PersonRaceReporting>();
                            raceReporting.Add(raceReportingItem);
                            raceDto.Reporting = raceReporting;
                        }

                        raceDtos.Add(raceDto);
                    }
                }
            }
            return raceDtos;
        }

        private async Task<Dtos.GuidObject2> GetPersonReligionGuidAsync(string religionCode, bool bypassCache = false)
        {
            Dtos.GuidObject2 religionDto = new Dtos.GuidObject2();
            // get the ethnicity guid and category
            if (!string.IsNullOrEmpty(religionCode))
            {
                try
                {
                    var religionEntity =
                        (await GetDenominationsAsync(bypassCache)).FirstOrDefault(r => r.Code == religionCode);
                    if (religionEntity != null && !string.IsNullOrEmpty(religionEntity.Guid))
                    {
                        religionDto = new Dtos.GuidObject2(religionEntity.Guid);
                    }
                }
                catch
                {
                    // Do nothing if not found in denominations table
                }
            }
            return religionDto;
        }

        private async Task<Dtos.DtoProperties.PersonEthnicityDtoProperty> GetPersonEthnicityGuidAsync(
            IEnumerable<string> ethnicityCodes, bool bypassCache = false)
        {
            Dtos.DtoProperties.PersonEthnicityDtoProperty ethnicityDto =
                new Dtos.DtoProperties.PersonEthnicityDtoProperty();
            // get the ethnicity guid and category
            if (ethnicityCodes != null && ethnicityCodes.Count() > 0)
            {
                var ethnicityEntity =
                    (await GetEthnicitiesAsync(bypassCache)).FirstOrDefault(
                        e => e.Code == ethnicityCodes.First());
                if (ethnicityEntity != null && !string.IsNullOrEmpty(ethnicityEntity.Guid))
                {
                    ethnicityDto.EthnicGroup = new Dtos.GuidObject2(ethnicityEntity.Guid);
                    var ethnicityReportingItem = new Dtos.DtoProperties.PersonEthnicityReporting();
                    ethnicityReportingItem.Country = new Dtos.DtoProperties.PersonEthnicityReportingCountry()
                    {
                        Code = Dtos.CountryCodeType.USA,
                        EthnicCategory = null
                    };

                    switch (ethnicityEntity.Type)
                    {
                        case EthnicityType.Hispanic:
                            ethnicityReportingItem.Country.EthnicCategory =
                                Dtos.EnumProperties.PersonEthnicityCategory.Hispanic;
                            break;
                        default:
                            ethnicityReportingItem.Country.EthnicCategory =
                                Dtos.EnumProperties.PersonEthnicityCategory.NonHispanic;
                            break;
                    }
                    var ethnicityReporting = new List<Dtos.DtoProperties.PersonEthnicityReporting>();
                    ethnicityReporting.Add(ethnicityReportingItem);
                    ethnicityDto.Reporting = ethnicityReporting;
                }
            }
            return ethnicityDto;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonRoleDtoProperty>> GetPersonRolesAsync(string personId,
            IEnumerable<Ellucian.Colleague.Domain.Base.Entities.PersonRole> personRoles)
        {
            var roles = new List<Dtos.DtoProperties.PersonRoleDtoProperty>();

            if (personRoles != null)
            {
                foreach (var personRole in personRoles)
                {
                    var role = new Dtos.DtoProperties.PersonRoleDtoProperty()
                    {
                        RoleType =
                            (Dtos.EnumProperties.PersonRoleType)Enum.Parse(typeof(Dtos.EnumProperties.PersonRoleType),
                                personRole.RoleType.ToString()),
                        StartOn = personRole.StartDate,
                        EndOn = personRole.EndDate
                    };
                    roles.Add(role);
                }
            }
            return roles.Any() ? roles : null;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonAddressDtoProperty>> GetAddresses2Async(
            IEnumerable<Domain.Base.Entities.Address> addressEntities, bool bypassCache = false)
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
                            else
                            {
                                var addressId = await _personRepository.GetAddressIdFromGuidAsync(addressEntity.Guid);
                                var errorMessage = string.Format("Address type '{0}' for address record ID '{1}' is not valid.", addrType, addressId);
                                logger.Error(errorMessage);
                                throw new Exception(errorMessage);

                            }
                            if (addressEntity.IsPreferredResidence && i == 0)
                                addressDto.Preference = Dtos.EnumProperties.PersonPreference.Primary;
                            addressDto.AddressEffectiveStart = addressEntity.EffectiveStartDate;
                            addressDto.AddressEffectiveEnd = addressEntity.EffectiveEndDate;

                            if (addressEntity.SeasonalDates != null)
                            {
                                var seasonalOccupancies = new List<Dtos.DtoProperties.PersonAddressRecurrenceDtoProperty>();
                                int year = DateTime.Today.Year;
                                foreach (var assocEntity in addressEntity.SeasonalDates)
                                {
                                    try
                                    {
                                        int endYear = year;
                                        int startMonth = int.Parse(assocEntity.StartOn.Split("/".ToCharArray())[0]);
                                        int startDay = int.Parse(assocEntity.StartOn.Split("/".ToCharArray())[1]);
                                        int endMonth = int.Parse(assocEntity.EndOn.Split("/".ToCharArray())[0]);
                                        int endDay = int.Parse(assocEntity.EndOn.Split("/".ToCharArray())[1]);
                                        if (endMonth < startMonth) endYear = endYear + 1;

                                        var recurrence = new Dtos.Recurrence3()
                                        {
                                            TimePeriod = new Dtos.RepeatTimePeriod2()
                                            {
                                                StartOn = new DateTime(year, startMonth, startDay),
                                                EndOn = new DateTime(endYear, endMonth, endDay)
                                            }
                                        };
                                        recurrence.RepeatRule = new Dtos.RepeatRuleDaily()
                                        {
                                            Type = Dtos.FrequencyType2.Daily,
                                            Interval = 1,
                                            Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(year, endMonth, endDay) }
                                        };
                                        seasonalOccupancies.Add(
                                            new Dtos.DtoProperties.PersonAddressRecurrenceDtoProperty()
                                            {
                                                Recurrence = recurrence
                                            });
                                    }
                                    catch
                                    {
                                        // Invalid seasonal start or end dates, just ignore and don't include
                                    }
                                }
                                if (seasonalOccupancies.Any())
                                {
                                    addressDto.SeasonalOccupancies = seasonalOccupancies;
                                }
                            }

                            addressDtos.Add(addressDto);
                        }
                    }
                }
            }
            return addressDtos;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonAddressDtoProperty>> GetAddresses3Async(
           IEnumerable<Domain.Base.Entities.Address> addressEntities, string guid ="", string id="", bool bypassCache = false)
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
                            else
                            {
                                var addressId = await _personRepository.GetAddressIdFromGuidAsync(addressEntity.Guid);
                                var errorMessage = string.Format("Address type '{0}' for address record ID '{1}' is not valid.", addrType, addressId);
                                logger.Error(errorMessage);
                                IntegrationApiExceptionAddError(errorMessage, "persons.addresses", guid, id);

                            }
                            if (addressEntity.IsPreferredResidence && i == 0)
                                addressDto.Preference = Dtos.EnumProperties.PersonPreference.Primary;
                            addressDto.AddressEffectiveStart = addressEntity.EffectiveStartDate;
                            addressDto.AddressEffectiveEnd = addressEntity.EffectiveEndDate;

                            if (addressEntity.SeasonalDates != null)
                            {
                                var seasonalOccupancies = new List<Dtos.DtoProperties.PersonAddressRecurrenceDtoProperty>();
                                int year = DateTime.Today.Year;
                                foreach (var assocEntity in addressEntity.SeasonalDates)
                                {
                                    try
                                    {
                                        int endYear = year;
                                        int startMonth = int.Parse(assocEntity.StartOn.Split("/".ToCharArray())[0]);
                                        int startDay = int.Parse(assocEntity.StartOn.Split("/".ToCharArray())[1]);
                                        int endMonth = int.Parse(assocEntity.EndOn.Split("/".ToCharArray())[0]);
                                        int endDay = int.Parse(assocEntity.EndOn.Split("/".ToCharArray())[1]);
                                        if (endMonth < startMonth) endYear = endYear + 1;

                                        var recurrence = new Dtos.Recurrence3()
                                        {
                                            TimePeriod = new Dtos.RepeatTimePeriod2()
                                            {
                                                StartOn = new DateTime(year, startMonth, startDay),
                                                EndOn = new DateTime(endYear, endMonth, endDay)
                                            }
                                        };
                                        recurrence.RepeatRule = new Dtos.RepeatRuleDaily()
                                        {
                                            Type = Dtos.FrequencyType2.Daily,
                                            Interval = 1,
                                            Ends = new Dtos.RepeatRuleEnds() { Date = new DateTime(year, endMonth, endDay) }
                                        };
                                        seasonalOccupancies.Add(
                                            new Dtos.DtoProperties.PersonAddressRecurrenceDtoProperty()
                                            {
                                                Recurrence = recurrence
                                            });
                                    }
                                    catch
                                    {
                                        // Invalid seasonal start or end dates, just ignore and don't include
                                    }
                                }
                                if (seasonalOccupancies.Any())
                                {
                                    addressDto.SeasonalOccupancies = seasonalOccupancies;
                                }
                            }

                            addressDtos.Add(addressDto);
                        }
                    }
                }
            }
            return addressDtos;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonPhoneDtoProperty>> GetPhones2Async(
            IEnumerable<Domain.Base.Entities.Phone> phoneEntities, bool bypassCache = false)
        {
            var phoneDtos = new List<Dtos.DtoProperties.PersonPhoneDtoProperty>();
            if (phoneEntities != null && phoneEntities.Count() > 0)
            {
                foreach (var phoneEntity in phoneEntities)
                {
                    string guid = "";
                    string category = "Other";
                    try
                    {
                        var phoneTypeEntity =
                            (await GetPhoneTypesAsync(bypassCache)).FirstOrDefault(
                                pt => pt.Code == phoneEntity.TypeCode);
                        if (phoneTypeEntity != null)
                        {
                            guid = phoneTypeEntity.Guid;
                            category = phoneTypeEntity.PhoneTypeCategory.ToString();
                        }

                        var phoneDto = new Dtos.DtoProperties.PersonPhoneDtoProperty()
                        {
                            Number = phoneEntity.Number,
                            Extension = string.IsNullOrEmpty(phoneEntity.Extension) ? null : phoneEntity.Extension,
                            Type = new Dtos.DtoProperties.PersonPhoneTypeDtoProperty()
                            {
                                PhoneType =
                                    (Dtos.EnumProperties.PersonPhoneTypeCategory)
                                        Enum.Parse(typeof(Dtos.EnumProperties.PersonPhoneTypeCategory), category),
                                Detail = string.IsNullOrEmpty(guid) ? null : new Dtos.GuidObject2(guid)
                            }
                        };
                        if (!string.IsNullOrEmpty(phoneEntity.CountryCallingCode)) phoneDto.CountryCallingCode = phoneEntity.CountryCallingCode;
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

        private async Task<IEnumerable<Dtos.DtoProperties.PersonEmailDtoProperty>> GetEmailAddresses2(
            IEnumerable<Domain.Base.Entities.EmailAddress> emailAddressEntities, bool bypassCache = false)
        {
            var emailAddressDtos = new List<Dtos.DtoProperties.PersonEmailDtoProperty>();
            if (emailAddressEntities != null && emailAddressEntities.Count() > 0)
            {
                foreach (var emailAddressEntity in emailAddressEntities)
                {
                    string guid = "";
                    string category = "Other";
                    try
                    {
                        var codeItem =
                            (await GetEmailTypesAsync(bypassCache)).FirstOrDefault(
                                pt => pt.Code == emailAddressEntity.TypeCode);
                        if (codeItem != null)
                        {
                            guid = codeItem.Guid;
                            category = codeItem.EmailTypeCategory.ToString();
                        }

                        var addressDto = new Dtos.DtoProperties.PersonEmailDtoProperty()
                        {
                            Type = new Dtos.DtoProperties.PersonEmailTypeDtoProperty()
                            {
                                EmailType = (Dtos.EmailTypeList)Enum.Parse(typeof(Dtos.EmailTypeList), category),
                                Detail = string.IsNullOrEmpty(guid) ? null : new Dtos.GuidObject2(guid)
                            },
                            Address = emailAddressEntity.Value
                        };
                        if (emailAddressEntity.IsPreferred)
                            addressDto.Preference = Dtos.EnumProperties.PersonEmailPreference.Primary;

                        emailAddressDtos.Add(addressDto);
                    }
                    catch
                    {
                        // do not fail if we can't find a guid from the code table or translate the cateory
                        // Just exclude this email address.
                    }
                }
            }
            return emailAddressDtos;
        }

        private async Task<string> GetPersonCountryAsync(string countryCode, bool bypassCache = false)
        {
            string iso3Code = "";
            if (!string.IsNullOrEmpty(countryCode))
            {
                try
                {
                    iso3Code =
                        (await GetCountryCodesAsync(bypassCache)).FirstOrDefault(
                            cc => cc.Code == countryCode).Iso3Code;
                }
                catch
                {
                    iso3Code = (countryCode.Length > 3) ? countryCode.Substring(0, 3) : countryCode;
                }
            }
            return iso3Code;
        }

        private async Task<Dtos.DtoProperties.PersonPrivacyDtoProperty> GetPersonPrivacyAsync(
            PrivacyStatusType privacyStatus, string privacyStatusCode, bool bypassCache)
        {
            Dtos.DtoProperties.PersonPrivacyDtoProperty personPrivacy =
                new Dtos.DtoProperties.PersonPrivacyDtoProperty();

            try
            {
                if (string.IsNullOrEmpty(privacyStatusCode))
                {
                    var statusEntity =
                        (await GetPrivacyStatusesAsync(bypassCache)).FirstOrDefault(
                            ps => ps.PrivacyStatusType == privacyStatus);
                    personPrivacy.PrivacyCategory = (privacyStatus == PrivacyStatusType.restricted)
                        ? Dtos.PrivacyStatusType.Restricted
                        : Dtos.PrivacyStatusType.Unrestricted;
                    if (statusEntity != null && !string.IsNullOrEmpty(statusEntity.Guid))
                        personPrivacy.Detail = new Dtos.GuidObject2(statusEntity.Guid);
                }
                else
                {
                    var statusEntity =
                        (await GetPrivacyStatusesAsync(bypassCache)).FirstOrDefault(
                            ps => ps.Code == privacyStatusCode);
                    if (statusEntity != null && !string.IsNullOrEmpty(statusEntity.Guid))
                    {
                        personPrivacy.PrivacyCategory = (statusEntity.PrivacyStatusType == PrivacyStatusType.restricted)
                            ? Dtos.PrivacyStatusType.Restricted
                            : Dtos.PrivacyStatusType.Unrestricted;
                        personPrivacy.Detail = new Dtos.GuidObject2(statusEntity.Guid);
                    }
                }
            }
            catch
            {
                // Do nothing if the code doesn't exist.
            }

            return personPrivacy;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonLanguageDtoProperty>> GetPersonLanguagesAsync(string primaryLanguage, List<string> secondaryLanguages,
            string personDtoId)
        {
            var personLanguages = new List<Dtos.DtoProperties.PersonLanguageDtoProperty>();
            if (!string.IsNullOrEmpty(primaryLanguage))
            {
                var codeAssoc = (await _personBaseRepository.GetLanguagesAsync()).ValsEntityAssociation.Where(v => v != null && v.ValInternalCodeAssocMember == primaryLanguage).FirstOrDefault();
                if (codeAssoc != null)
                {
                    var languageIsoCode = codeAssoc.ValActionCode3AssocMember;
                    if (!string.IsNullOrEmpty(languageIsoCode))
                    {
                        var personLanguage = new Dtos.DtoProperties.PersonLanguageDtoProperty();
                        try
                        {
                            personLanguage.Code = (Dtos.EnumProperties.PersonLanguageCode)System.Enum.Parse(typeof(Dtos.EnumProperties.PersonLanguageCode), languageIsoCode.ToLower().Trim());
                        }
                        catch
                        {
                            throw new ArgumentException(string.Format("The language ISO code '{0}' is an invalid enumeration value for languages.code.", languageIsoCode.ToLower()), "Invalid.LanguageIsoCode");
                        }
                        personLanguage.Preference = Dtos.EnumProperties.PersonLanguagePreference.Primary;
                        personLanguages.Add(personLanguage);
                    }
                    else
                    {
                        throw new ArgumentException(string.Format("The language '{0}' is not mapped to a language ISO code.", primaryLanguage), "Invalid.Language");
                    }
                }
            }
            if (secondaryLanguages != null && secondaryLanguages.Any())
            {
                foreach (var language in secondaryLanguages)
                {
                    var codeAssoc = (await _personBaseRepository.GetLanguagesAsync()).ValsEntityAssociation.Where(v => v != null && v.ValInternalCodeAssocMember == language).FirstOrDefault();
                    if (codeAssoc != null)
                    {
                        var languageIsoCode = codeAssoc.ValActionCode3AssocMember;
                        if (!string.IsNullOrEmpty(languageIsoCode))
                        {
                            var personLanguage = new Dtos.DtoProperties.PersonLanguageDtoProperty();
                            try
                            {
                                personLanguage.Code = (Dtos.EnumProperties.PersonLanguageCode)System.Enum.Parse(typeof(Dtos.EnumProperties.PersonLanguageCode), languageIsoCode.ToLower().Trim());
                            }
                            catch
                            {
                                throw new ArgumentException(string.Format("The language ISO code '{0}' is an invalid enumeration value for languages.code.", languageIsoCode.ToLower()), "Invalid.LanguageIsoCode");
                            }
                            personLanguage.Preference = Dtos.EnumProperties.PersonLanguagePreference.Secondary;
                            personLanguages.Add(personLanguage);
                        }
                        else
                        {
                            throw new ArgumentException(string.Format("The language '{0}' is not mapped to a language ISO code.", language), "Invalid.Language");
                        }
                    }
                }
            }

            return personLanguages;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonLanguageDtoProperty>> GetPersonLanguages2Async(string primaryLanguage, List<string> secondaryLanguages,
            string personDtoId)
        {
            var personLanguages = new List<Dtos.DtoProperties.PersonLanguageDtoProperty>();
            if (!string.IsNullOrEmpty(primaryLanguage))
            {
                var codeAssoc = (await _personBaseRepository.GetLanguagesAsync()).ValsEntityAssociation.Where(v => v != null && v.ValInternalCodeAssocMember == primaryLanguage).FirstOrDefault();
                if (codeAssoc != null)
                {
                    var languageIsoCode = codeAssoc.ValActionCode3AssocMember;
                    if (!string.IsNullOrEmpty(languageIsoCode))
                    {
                        var personLanguage = new Dtos.DtoProperties.PersonLanguageDtoProperty();
                        try
                        {
                            personLanguage.Code = (Dtos.EnumProperties.PersonLanguageCode)System.Enum.Parse(typeof(Dtos.EnumProperties.PersonLanguageCode), languageIsoCode.ToLower().Trim());
                        }
                        catch
                        {
                            IntegrationApiExceptionAddError(string.Format("The language ISO code '{0}' is an invalid enumeration value for languages.code.", languageIsoCode.ToLower()), "Invalid.LanguageIsoCode", personDtoId);
                        }
                        personLanguage.Preference = Dtos.EnumProperties.PersonLanguagePreference.Primary;
                        personLanguages.Add(personLanguage);
                    }
                    else
                    {
                        var ex = new RepositoryException();
                        IntegrationApiExceptionAddError(string.Format("The language '{0}' is not mapped to a language ISO code.", primaryLanguage), "Invalid.Language", personDtoId);
                    }
                }
            }
            if (secondaryLanguages != null && secondaryLanguages.Any())
            {
                foreach (var language in secondaryLanguages)
                {
                    var codeAssoc = (await _personBaseRepository.GetLanguagesAsync()).ValsEntityAssociation.Where(v => v != null && v.ValInternalCodeAssocMember == language).FirstOrDefault();
                    if (codeAssoc != null)
                    {
                        var languageIsoCode = codeAssoc.ValActionCode3AssocMember;
                        if (!string.IsNullOrEmpty(languageIsoCode))
                        {
                            var personLanguage = new Dtos.DtoProperties.PersonLanguageDtoProperty();
                            try
                            {
                                personLanguage.Code = (Dtos.EnumProperties.PersonLanguageCode)System.Enum.Parse(typeof(Dtos.EnumProperties.PersonLanguageCode), languageIsoCode.ToLower().Trim());
                            }
                            catch
                            {
                                IntegrationApiExceptionAddError(string.Format("The language ISO code '{0}' is an invalid enumeration value for languages.code.", languageIsoCode.ToLower()), "Invalid.LanguageIsoCode", personDtoId);
                            }
                            personLanguage.Preference = Dtos.EnumProperties.PersonLanguagePreference.Secondary;
                            personLanguages.Add(personLanguage);
                        }
                        else
                        {
                            var ex = new RepositoryException();
                            IntegrationApiExceptionAddError(string.Format("The language '{0}' is not mapped to a language ISO code.", language), "Invalid.Language", personDtoId);
                        }
                    }
                }
            }

            return personLanguages;
        }

        private async Task<Dtos.DtoProperties.PersonCitizenshipDtoProperty> GetPersonCitizenshipAsync(
            string alienStatusCode, bool bypassCache = false)
        {
            var citizenShipStatus = new Dtos.DtoProperties.PersonCitizenshipDtoProperty();

            if (!string.IsNullOrEmpty(alienStatusCode))
            {
                try
                {
                    var citizenshipStatusEntity =
                        (await GetCitizenshipStatusesAsync(bypassCache)).FirstOrDefault(
                            ct => ct.Code == alienStatusCode);
                    if (citizenshipStatusEntity != null)
                        citizenShipStatus = new Dtos.DtoProperties.PersonCitizenshipDtoProperty()
                        {
                            Category =
                                (Dtos.CitizenshipStatusType)
                                    Enum.Parse(typeof(Dtos.CitizenshipStatusType),
                                        citizenshipStatusEntity.CitizenshipStatusType.ToString()),
                            Detail = new Dtos.GuidObject2(citizenshipStatusEntity.Guid)
                        };
                }
                catch
                {
                    // do not fail if we can't find a guid and category from the code table
                    // just return an empty status object
                }
            }
            else
            {
                return null;
            }

            return citizenShipStatus;
        }

        private async Task<Dtos.DtoProperties.PersonVisaDtoProperty> GetPersonVisaStatusAsync(PersonVisa personVisaEntity, bool bypassCache = false)
        {
            Dtos.DtoProperties.PersonVisaDtoProperty personVisa = new Dtos.DtoProperties.PersonVisaDtoProperty();

            if (personVisaEntity != null)
            {
                try
                {
                    var visaEntity =
                        (await GetVisaTypesAsync(bypassCache)).FirstOrDefault(
                            vt => vt.Code == personVisaEntity.Type);
                    personVisa = new Dtos.DtoProperties.PersonVisaDtoProperty()
                    {
                        Category =
                            (Dtos.VisaTypeCategory)
                                Enum.Parse(typeof(Dtos.VisaTypeCategory), visaEntity.VisaTypeCategory.ToString()),
                        Detail = new Dtos.GuidObject2(visaEntity.Guid),
                        Status =
                            (personVisaEntity.ExpireDate != null &&
                             personVisaEntity.ExpireDate.GetValueOrDefault().Date <= DateTime.Today.Date)
                                ? Dtos.EnumProperties.VisaStatus.Expired
                                : Dtos.EnumProperties.VisaStatus.Current,
                        StartOn = personVisaEntity.IssueDate,
                        EndOn = personVisaEntity.ExpireDate
                    };
                }
                catch
                {
                    // do not fail if we can't find a guid and category from the code table
                    // just return an empty status object
                }
            }

            return personVisa;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonIdentityDocument>> GetPersonIdentityDocumentsAsync(PersonIntegration personEntity, bool bypassCache = false)
        {
            var personIdentityDocuments = new List<Dtos.DtoProperties.PersonIdentityDocument>();

            // Passport
            var passportEntity = personEntity.Passport;
            if (passportEntity != null)
            {
                if (!string.IsNullOrEmpty(passportEntity.PassportNumber))
                {
                    string guid = "";
                    try
                    {
                        var identityDocumentType = (await GetIdentityDocumentTypesAsync(bypassCache)).FirstOrDefault(
                                pt =>
                                    pt.IdentityDocumentTypeCategory ==
                                Domain.Base.Entities.IdentityDocumentTypeCategory.Passport);
                        if (identityDocumentType != null)
                            guid = identityDocumentType.Guid;
                    }
                    catch
                    {
                        // do not fail if we can't find a guid from the code table
                    }

                    if (string.IsNullOrEmpty(passportEntity.IssuingCountry.Trim()))
                    {
                        throw new ArgumentException(string.Concat("Passport number '", passportEntity.PassportNumber, "' does not have a valid issuing country set."));
                    }

                    var identityDocument = new Dtos.DtoProperties.PersonIdentityDocument()
                    {

                        DocumentId = passportEntity.PassportNumber,
                        ExpiresOn = passportEntity.ExpireDate,
                        Type = new Dtos.DtoProperties.PersonIdentityDocumentType()
                        {
                            Category = Dtos.EnumProperties.PersonIdentityDocumentCategory.Passport,
                            Detail = (string.IsNullOrEmpty(guid)) ? null : new Dtos.GuidObject2(guid)
                        }
                    };
                    try
                    {
                        if (!string.IsNullOrEmpty(passportEntity.IssuingCountry.Trim()))
                        {
                            identityDocument.Country = new PersonIdentityDocumentCountryDtoProperty()
                            {
                                Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), (await GetPersonCountryAsync(passportEntity.IssuingCountry)))
                            };
                        }
                        personIdentityDocuments.Add(identityDocument);
                    }
                    catch
                    {
                        // Do not include identity document
                    }
                }
            }

            // Drivers License
            var driverEntity = personEntity.DriverLicense;
            if (driverEntity != null)
            {
                if (!string.IsNullOrEmpty(driverEntity.LicenseNumber))
                {
                    string guid = string.Empty;
                    try
                    {
                        var identityDocumentType = (await GetIdentityDocumentTypesAsync(bypassCache)).FirstOrDefault(
                                pt =>
                                    pt.IdentityDocumentTypeCategory ==
                                Domain.Base.Entities.IdentityDocumentTypeCategory.PhotoId);
                        if (identityDocumentType != null)
                            guid = identityDocumentType.Guid;
                    }
                    catch
                    {
                        // do not fail if we can't find a guid from the code table
                    }

                    var stateInfo = (await GetStateCodesAsync(bypassCache)).FirstOrDefault(s => s.Code == driverEntity.IssuingState);

                    if (stateInfo != null)
                    {
                        //default country to USA if it is not set
                        var countryCode = await _personRepository.GetHostCountryAsync();
                        string countryCodeIso3 = string.Empty;
                        switch (countryCode)
                        {
                            case "USA":
                                countryCode = "US";
                                countryCodeIso3 = "USA";
                                break;
                            case "CANADA":
                                countryCode = "CA";
                                countryCodeIso3 = "CAN";
                                break;
                            default:
                                countryCode = "US";
                                countryCodeIso3 = "USA";
                                break;
                        }
                        if (!string.IsNullOrEmpty(stateInfo.CountryCode))
                        {
                            var country = stateInfo.CountryCode;
                            var countryEntity = (await GetCountryCodesAsync(bypassCache)).FirstOrDefault(x => x.Code == country);
                            if (countryEntity != null && !string.IsNullOrEmpty(countryEntity.IsoAlpha3Code))
                            {
                                countryCode = countryEntity.IsoCode;
                                countryCodeIso3 = countryEntity.IsoAlpha3Code;
                            }
                        }
                        string regionCode = string.Empty;
                        if (!string.IsNullOrEmpty(driverEntity.IssuingState))
                        {
                            // Validate the region code against the places table
                            regionCode = string.Concat(countryCode, "-", driverEntity.IssuingState);
                            var place = (await GetPlacesAsync(bypassCache)).FirstOrDefault(x => x.PlacesRegion == regionCode && x.PlacesCountry == countryCodeIso3);
                            if (place == null)
                            {
                                regionCode = string.Empty;
                            }
                        }

                        var identityDocument = new Dtos.DtoProperties.PersonIdentityDocument()
                        {
                            DocumentId = driverEntity.LicenseNumber,
                            ExpiresOn = driverEntity.ExpireDate,
                            Type = new Dtos.DtoProperties.PersonIdentityDocumentType()
                            {
                                Category = Dtos.EnumProperties.PersonIdentityDocumentCategory.PhotoId,
                                Detail = (string.IsNullOrEmpty(guid)) ? null : new Dtos.GuidObject2(guid)
                            }
                        };

                        //Colleague only supports drivers licenses for US and CA
                        if (!string.IsNullOrEmpty(regionCode))
                        {
                            identityDocument.Country = new PersonIdentityDocumentCountryDtoProperty()
                            {
                                Code = (IsoCode)Enum.Parse(typeof(IsoCode), countryCodeIso3),
                                Region = new AddressRegion()
                                {
                                    Code = regionCode
                                }
                            };
                        }

                        personIdentityDocuments.Add(identityDocument);
                    }
                    else
                    {
                        var identityDocument = new Dtos.DtoProperties.PersonIdentityDocument()
                        {
                            DocumentId = driverEntity.LicenseNumber,
                            ExpiresOn = driverEntity.ExpireDate,
                            Type = new Dtos.DtoProperties.PersonIdentityDocumentType()
                            {
                                Category = Dtos.EnumProperties.PersonIdentityDocumentCategory.PhotoId,
                                Detail = (string.IsNullOrEmpty(guid)) ? null : new Dtos.GuidObject2(guid)
                            }
                        };

                        personIdentityDocuments.Add(identityDocument);
                    }
                }
            }

            // Other Identity Documents
            if (personEntity.IdentityDocuments != null)
            {
                foreach (var document in personEntity.IdentityDocuments)
                {
                    if (!string.IsNullOrEmpty(document.Number))
                    {
                        string guid = "";
                        try
                        {
                            var identityDocumentType = (await GetIdentityDocumentTypesAsync(bypassCache)).FirstOrDefault(
                                    pt =>
                                        pt.IdentityDocumentTypeCategory ==
                                    Domain.Base.Entities.IdentityDocumentTypeCategory.Other);
                            if (identityDocumentType != null)
                                guid = identityDocumentType.Guid;
                        }
                        catch
                        {
                            // do not fail if we can't find a guid from the code table
                        }

                        var identityDocument = new Dtos.DtoProperties.PersonIdentityDocument()
                        {

                            DocumentId = document.Number,
                            ExpiresOn = document.ExpireDate,
                            Type = new Dtos.DtoProperties.PersonIdentityDocumentType()
                            {
                                Category = Dtos.EnumProperties.PersonIdentityDocumentCategory.Other,
                                Detail = (string.IsNullOrEmpty(guid)) ? null : new Dtos.GuidObject2(guid)
                            }
                        };
                        try
                        {
                            if (!string.IsNullOrEmpty(document.Country.Trim()))
                            {
                                identityDocument.Country = new PersonIdentityDocumentCountryDtoProperty()
                                {
                                    Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), document.Country.Trim())
                                };
                                if (!string.IsNullOrEmpty(document.Region))
                                {
                                    identityDocument.Country.Region = new AddressRegion() { Code = document.Region };
                                }
                            }
                            personIdentityDocuments.Add(identityDocument);
                        }
                        catch
                        {
                            // Do not include identity document
                        }
                    }
                }
            }

            return personIdentityDocuments.Any() ? personIdentityDocuments : null;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonIdentityDocument>> GetPersonIdentityDocuments2Async(PersonIntegration personEntity, bool bypassCache = false)
        {
            var personIdentityDocuments = new List<Dtos.DtoProperties.PersonIdentityDocument>();

            // Passport
            var passportEntity = personEntity.Passport;
            if (passportEntity != null)
            {
                if (!string.IsNullOrEmpty(passportEntity.PassportNumber))
                {
                    string guid = "";
                    try
                    {
                        var identityDocumentType = (await GetIdentityDocumentTypesAsync(bypassCache)).FirstOrDefault(
                                pt =>
                                    pt.IdentityDocumentTypeCategory ==
                                Domain.Base.Entities.IdentityDocumentTypeCategory.Passport);
                        if (identityDocumentType != null)
                            guid = identityDocumentType.Guid;
                    }
                    catch
                    {
                        // do not fail if we can't find a guid from the code table
                    }

                    if (string.IsNullOrEmpty(passportEntity.IssuingCountry.Trim()))
                    {
                        IntegrationApiExceptionAddError(string.Format("Passport number '{0}' does not have a valid issuing country set.", passportEntity.PassportNumber), "persons.identityDocuments", personEntity.Guid, personEntity.Id);
                    }

                    var identityDocument = new Dtos.DtoProperties.PersonIdentityDocument()
                    {

                        DocumentId = passportEntity.PassportNumber,
                        ExpiresOn = passportEntity.ExpireDate,
                        Type = new Dtos.DtoProperties.PersonIdentityDocumentType()
                        {
                            Category = Dtos.EnumProperties.PersonIdentityDocumentCategory.Passport,
                            Detail = (string.IsNullOrEmpty(guid)) ? null : new Dtos.GuidObject2(guid)
                        }
                    };
                    try
                    {
                        if (!string.IsNullOrEmpty(passportEntity.IssuingCountry.Trim()))
                        {
                            identityDocument.Country = new PersonIdentityDocumentCountryDtoProperty()
                            {
                                Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), (await GetPersonCountryAsync(passportEntity.IssuingCountry)))
                            };
                        }
                        personIdentityDocuments.Add(identityDocument);
                    }
                    catch
                    {
                        // Do not include identity document
                    }
                }
            }

            // Drivers License
            var driverEntity = personEntity.DriverLicense;
            if (driverEntity != null)
            {
                if (!string.IsNullOrEmpty(driverEntity.LicenseNumber))
                {
                    string guid = string.Empty;
                    try
                    {
                        var identityDocumentType = (await GetIdentityDocumentTypesAsync(bypassCache)).FirstOrDefault(
                                pt =>
                                    pt.IdentityDocumentTypeCategory ==
                                Domain.Base.Entities.IdentityDocumentTypeCategory.PhotoId);
                        if (identityDocumentType != null)
                            guid = identityDocumentType.Guid;
                    }
                    catch
                    {
                        // do not fail if we can't find a guid from the code table
                    }

                    var stateInfo = (await GetStateCodesAsync(bypassCache)).FirstOrDefault(s => s.Code == driverEntity.IssuingState);

                    if (stateInfo != null)
                    {
                        //default country to USA if it is not set
                        var countryCode = await _personRepository.GetHostCountryAsync();
                        string countryCodeIso3 = string.Empty;
                        switch (countryCode)
                        {
                            case "USA":
                                countryCode = "US";
                                countryCodeIso3 = "USA";
                                break;
                            case "CANADA":
                                countryCode = "CA";
                                countryCodeIso3 = "CAN";
                                break;
                            default:
                                countryCode = "US";
                                countryCodeIso3 = "USA";
                                break;
                        }
                        if (!string.IsNullOrEmpty(stateInfo.CountryCode))
                        {
                            var country = stateInfo.CountryCode;
                            var countryEntity = (await GetCountryCodesAsync(bypassCache)).FirstOrDefault(x => x.Code == country);
                            if (countryEntity != null && !string.IsNullOrEmpty(countryEntity.IsoAlpha3Code))
                            {
                                countryCode = countryEntity.IsoCode;
                                countryCodeIso3 = countryEntity.IsoAlpha3Code;
                            }
                        }
                        string regionCode = string.Empty;
                        if (!string.IsNullOrEmpty(driverEntity.IssuingState))
                        {
                            // Validate the region code against the places table
                            regionCode = string.Concat(countryCode, "-", driverEntity.IssuingState);
                            var place = (await GetPlacesAsync(bypassCache)).FirstOrDefault(x => x.PlacesRegion == regionCode && x.PlacesCountry == countryCodeIso3);
                            if (place == null)
                            {
                                regionCode = string.Empty;
                            }
                        }

                        var identityDocument = new Dtos.DtoProperties.PersonIdentityDocument()
                        {
                            DocumentId = driverEntity.LicenseNumber,
                            ExpiresOn = driverEntity.ExpireDate,
                            Type = new Dtos.DtoProperties.PersonIdentityDocumentType()
                            {
                                Category = Dtos.EnumProperties.PersonIdentityDocumentCategory.PhotoId,
                                Detail = (string.IsNullOrEmpty(guid)) ? null : new Dtos.GuidObject2(guid)
                            }
                        };

                        //Colleague only supports drivers licenses for US and CA
                        if (!string.IsNullOrEmpty(regionCode))
                        {
                            identityDocument.Country = new PersonIdentityDocumentCountryDtoProperty()
                            {
                                Code = (IsoCode)Enum.Parse(typeof(IsoCode), countryCodeIso3),
                                Region = new AddressRegion()
                                {
                                    Code = regionCode
                                }
                            };
                        }

                        personIdentityDocuments.Add(identityDocument);
                    }
                    else
                    {
                        var identityDocument = new Dtos.DtoProperties.PersonIdentityDocument()
                        {
                            DocumentId = driverEntity.LicenseNumber,
                            ExpiresOn = driverEntity.ExpireDate,
                            Type = new Dtos.DtoProperties.PersonIdentityDocumentType()
                            {
                                Category = Dtos.EnumProperties.PersonIdentityDocumentCategory.PhotoId,
                                Detail = (string.IsNullOrEmpty(guid)) ? null : new Dtos.GuidObject2(guid)
                            }
                        };

                        personIdentityDocuments.Add(identityDocument);
                    }
                }
            }

            // Other Identity Documents
            if (personEntity.IdentityDocuments != null)
            {
                foreach (var document in personEntity.IdentityDocuments)
                {
                    if (!string.IsNullOrEmpty(document.Number))
                    {
                        string guid = "";
                        try
                        {
                            var identityDocumentType = (await GetIdentityDocumentTypesAsync(bypassCache)).FirstOrDefault(
                                    pt =>
                                        pt.IdentityDocumentTypeCategory ==
                                    Domain.Base.Entities.IdentityDocumentTypeCategory.Other);
                            if (identityDocumentType != null)
                                guid = identityDocumentType.Guid;
                        }
                        catch
                        {
                            // do not fail if we can't find a guid from the code table
                        }

                        var identityDocument = new Dtos.DtoProperties.PersonIdentityDocument()
                        {

                            DocumentId = document.Number,
                            ExpiresOn = document.ExpireDate,
                            Type = new Dtos.DtoProperties.PersonIdentityDocumentType()
                            {
                                Category = Dtos.EnumProperties.PersonIdentityDocumentCategory.Other,
                                Detail = (string.IsNullOrEmpty(guid)) ? null : new Dtos.GuidObject2(guid)
                            }
                        };
                        try
                        {
                            if (!string.IsNullOrEmpty(document.Country.Trim()))
                            {
                                identityDocument.Country = new PersonIdentityDocumentCountryDtoProperty()
                                {
                                    Code = (Dtos.EnumProperties.IsoCode)System.Enum.Parse(typeof(Dtos.EnumProperties.IsoCode), document.Country.Trim())
                                };
                                if (!string.IsNullOrEmpty(document.Region))
                                {
                                    identityDocument.Country.Region = new AddressRegion() { Code = document.Region };
                                }
                            }
                            personIdentityDocuments.Add(identityDocument);
                        }
                        catch
                        {
                            // Do not include identity document
                        }
                    }
                }
            }

            return personIdentityDocuments.Any() ? personIdentityDocuments : null;
        }

        private async Task<IEnumerable<Dtos.GuidObject2>> GetPersonInterestsAsync(List<string> interestCodes, bool bypassCache = false)
        {
            List<Dtos.GuidObject2> interestGuids = new List<Dtos.GuidObject2>();

            foreach (var interest in interestCodes)
            {
                try
                {
                    var firstOrDefault = (await GetInterestsAsync(bypassCache)).FirstOrDefault(
                        ic => ic.Code == interest);
                    if (firstOrDefault != null)
                    {
                        var guid = firstOrDefault.Guid;
                        interestGuids.Add(new Dtos.GuidObject2(guid));
                    }
                }
                catch
                {
                    // do nothing if guid does not exist
                }
            }

            return interestGuids;
        }

        private async Task<IEnumerable<Dtos.DtoProperties.PersonSocialMediaDtoProperty>> GetPersonSocialMediaAsync(
            List<Ellucian.Colleague.Domain.Base.Entities.SocialMedia> mediaTypes, bool bypassCache = false)
        {
            List<Dtos.DtoProperties.PersonSocialMediaDtoProperty> socialMediaEntries =
                new List<Dtos.DtoProperties.PersonSocialMediaDtoProperty>();

            foreach (var mediaType in mediaTypes)
            {
                try
                {
                    var socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty();
                    if (mediaType.TypeCode.ToLowerInvariant() == "website")
                    {
                        string guid = "";
                        var socialMediaEntity =
                            (await GetSocialMediaTypesAsync(bypassCache)).FirstOrDefault(
                                ic => ic.Type.ToString() == mediaType.TypeCode);
                        if (socialMediaEntity != null)
                        {
                            guid = socialMediaEntity.Guid;
                            socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                            {
                                Type = new Dtos.DtoProperties.PersonSocialMediaType()
                                {
                                    Category =
                                        (Dtos.SocialMediaTypeCategory)
                                            Enum.Parse(typeof(Dtos.SocialMediaTypeCategory),
                                                mediaType.TypeCode.ToString()),
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
                                    Category =
                                        (Dtos.SocialMediaTypeCategory)
                                            Enum.Parse(typeof(Dtos.SocialMediaTypeCategory),
                                                mediaType.TypeCode.ToString())
                                },
                                Address = mediaType.Handle
                            };
                        }
                    }
                    else
                    {
                        var socialMediaEntity =
                            (await GetSocialMediaTypesAsync(bypassCache)).FirstOrDefault(
                                ic => ic.Code == mediaType.TypeCode);
                        socialMedia = new Dtos.DtoProperties.PersonSocialMediaDtoProperty()
                        {
                            Type = new Dtos.DtoProperties.PersonSocialMediaType()
                            {
                                Category =
                                    (Dtos.SocialMediaTypeCategory)
                                        Enum.Parse(typeof(Dtos.SocialMediaTypeCategory),
                                            socialMediaEntity.Type.ToString()),
                                Detail = new Dtos.GuidObject2(socialMediaEntity.Guid)
                            },
                            Address = mediaType.Handle
                        };
                    }
                    if (mediaType.IsPreferred) socialMedia.Preference = Dtos.EnumProperties.PersonPreference.Primary;

                    socialMediaEntries.Add(socialMedia);
                }
                catch
                {
                    // Do not include code since we couldn't find a category
                }
            }

            return socialMediaEntries;
        }

        /// <summary>
        /// Gets military statuses.
        /// </summary>
        IEnumerable<MilStatuses> _militaryStatuses = null;
        private async Task<IEnumerable<MilStatuses>> MilitaryStatusesAsync(bool bypassCache)
        {
            if (_militaryStatuses == null)
            {
                _militaryStatuses = await GetMilStatusesAsync(bypassCache);
            }
            return _militaryStatuses;
        }
        #endregion

        #region Convert Person Dto to Entity Methods

        private async Task<Domain.Base.Entities.PersonIntegration> ConvertPerson2DtoToEntityAsync(string personId,
            Dtos.Person2 personDto)
        {
            if (personDto == null || string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide guid for person");

            PersonIntegration personEntity = null;
            personEntity = await ConvertPersonNames(personId, personDto.Id, personDto.PersonNames);
            personEntity.BirthDate = personDto.BirthDate;
            personEntity.DeceasedDate = personDto.DeceasedDate;

            // email addresses
            var emailAddressEntities = await MapEmailAddresses2(personDto.EmailAddresses);
            if (emailAddressEntities != null && emailAddressEntities.Count() > 0)
            {
                foreach (var emailAddressEntity in emailAddressEntities)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            //Make sure birth date is before today's date
            if (personDto.BirthDate != null && personDto.BirthDate > DateTime.Today)
            {
                throw new InvalidOperationException("Date of birth cannot be after the current date.");
            }

            //Make sure birth date is before deceased date
            if (personDto.BirthDate != null && personDto.DeceasedDate != null && personDto.BirthDate > personDto.DeceasedDate)
            {
                throw new InvalidOperationException("Date of birth cannot be after deceased date.");
            }

            //privacy status
            if (personDto.PrivacyStatus != null)
            {
                var privacyStatusEntity = await ConvertPerson2PrivacyStatusAsync(personDto.PrivacyStatus);
                if (privacyStatusEntity == null)
                {
                    personEntity.PrivacyStatus = PrivacyStatusType.unrestricted;
                }
                else
                {
                    personEntity.PrivacyStatus = privacyStatusEntity.PrivacyStatusType;
                    personEntity.PrivacyStatusCode = privacyStatusEntity.Code;
                }
            }

            //Gender
            if (personDto.GenderType != null)
            {
                personEntity.Gender = ConvertGenderType2String(personDto.GenderType);
            }

            //religion
            if (personDto.Religion != null)
            {
                personEntity.Religion = await ConvertPerson2DtoReligionCodeToEntityAsync(personDto.Religion.Id);
            }

            //ethnicity code
            if (personDto.Ethnicity != null)
            {
                personEntity.EthnicCodes = await ConvertPerson2DtoEthnicityCodesEntityAsync(personDto.Ethnicity);
            }

            //races
            if (personDto.Races != null && personDto.Races.Any())
            {
                personEntity.RaceCodes = await ConvertPerson2DtoRaceCodesToEntityAsync(personDto.Races);
            }

            //language
            if (personDto.Languages != null && personDto.Languages.Any())
            {
                personEntity.Languages.AddRange(ConvertPerson2DtoLanguagesToEntity(personId, personDto.Id,
                    personDto.Languages));
            }

            //marital status
            if (personDto.MaritalStatus != null)
            {                
                personEntity = await ConvertPerson2MaritalStatusDtoToEntityAsync(personEntity, personDto.MaritalStatus);
            }

            //citizenshipStatus
            if (personDto.CitizenshipStatus != null)
            {
                personEntity.AlienStatus =
                    await ConvertPerson2CitizenshipStatusDtoToEntityAsync(personDto.CitizenshipStatus);
            }

            //countryOfBirth
            if (!string.IsNullOrEmpty(personDto.CountryOfBirth))
            {
                personEntity.BirthCountry = await ConvertPerson2CountryCodeDtoToEntityAsync(personDto.CountryOfBirth);
            }

            //countryOfCitizenship
            if (!string.IsNullOrEmpty(personDto.CitizenshipCountry))
            {
                personEntity.Citizenship = await ConvertPerson2CountryCodeDtoToEntityAsync(personDto.CitizenshipCountry);
            }

            //roles
            if (personDto.Roles != null && personDto.Roles.Any())
            {
                personEntity.Roles.AddRange(ConvertPerson2DtoRolesToEntity(personDto.Roles));
            }

            //identityDocuments
            if (personDto.IdentityDocuments != null && personDto.IdentityDocuments.Any())
            {
                //type is required for identity documents.
                if (personDto.IdentityDocuments.Where(doc => doc.Type == null).Count() > 0)
                {
                    throw new InvalidOperationException("Type is required for identityDocuments.");
                }
                //Typoe category is required.
                if (personDto.IdentityDocuments.Where(doc => doc.Type != null && string.IsNullOrEmpty(doc.Type.Category.ToString())).Count() > 0)
                {
                    throw new InvalidOperationException("Type category is required for identityDocuments.");
                }
                //type detail Id required for detail object.
                if (personDto.IdentityDocuments.Where(doc => doc.Type != null && doc.Type.Detail != null && string.IsNullOrEmpty(doc.Type.Detail.Id)).Count() > 0)
                {
                    throw new InvalidOperationException("Type detail Id is required for identityDocuments.");
                }
                personEntity.Passport =
                    await
                        ConvertPerson2DtoPassportDocumentToEntityAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
                personEntity.DriverLicense =
                    await
                        ConvertPerson2DtoDriversLicenseToEntityDocumentAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
                personEntity.IdentityDocuments =
                    await
                        ConvertPerson2DtoIdentityDocumentsToEntityAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
            }

            // Social Media
            if (personDto.SocialMedia != null && personDto.SocialMedia.Any())
            {
                personEntity.SocialMedia.AddRange(await ConvertPerson2DtoSocialMediaToEntity(personDto.SocialMedia));
            }

            // credentials
            if (personDto.Credentials != null && personDto.Credentials.Any())
            {
                ConvertPerson2DtoCredsToEntity(personId, personDto.Credentials, personEntity);
            }

            // interests
            if (personDto.Interests != null && personDto.Interests.Any())
            {
                personEntity.Interests = await ConvertPerson2DtoInterestsToEntityAsync(personDto.Interests);
            }

            return personEntity;
        }

        private async Task<Domain.Base.Entities.PersonIntegration> ConvertPerson3DtoToEntityAsync(string personId,
            Dtos.Person3 personDto)
        {
            if (personDto == null || string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide guid for person");

            PersonIntegration personEntity = null;
            personEntity = await ConvertPersonNames(personId, personDto.Id, personDto.PersonNames);
            personEntity.BirthDate = personDto.BirthDate;
            personEntity.DeceasedDate = personDto.DeceasedDate;

            // email addresses
            var emailAddressEntities = await MapEmailAddresses2(personDto.EmailAddresses);
            if (emailAddressEntities != null && emailAddressEntities.Count() > 0)
            {
                foreach (var emailAddressEntity in emailAddressEntities)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            //Make sure birth date is before today's date
            if (personDto.BirthDate != null && personDto.BirthDate > DateTime.Today)
            {
                throw new InvalidOperationException("Date of birth cannot be after the current date.");
            }

            //Make sure birth date is before deceased date
            if (personDto.BirthDate != null && personDto.DeceasedDate != null && personDto.BirthDate > personDto.DeceasedDate)
            {
                throw new InvalidOperationException("Date of birth cannot be after deceased date.");
            }

            //privacy status
            if (personDto.PrivacyStatus != null)
            {
                var privacyStatusEntity = await ConvertPerson2PrivacyStatusAsync(personDto.PrivacyStatus);
                if (privacyStatusEntity == null)
                {
                    personEntity.PrivacyStatus = PrivacyStatusType.unrestricted;
                }
                else
                {
                    personEntity.PrivacyStatus = privacyStatusEntity.PrivacyStatusType;
                    personEntity.PrivacyStatusCode = privacyStatusEntity.Code;
                }
            }

            //Gender
            if (personDto.GenderType != null)
            {
                personEntity.Gender = ConvertGenderType2String(personDto.GenderType);
            }

            //religion
            if (personDto.Religion != null)
            {
                personEntity.Religion = await ConvertPerson2DtoReligionCodeToEntityAsync(personDto.Religion.Id);
            }

            //ethnicity code
            if (personDto.Ethnicity != null)
            {
                personEntity.EthnicCodes = await ConvertPerson2DtoEthnicityCodesEntityAsync(personDto.Ethnicity);
            }

            //races
            if (personDto.Races != null && personDto.Races.Any())
            {
                personEntity.RaceCodes = await ConvertPerson2DtoRaceCodesToEntityAsync(personDto.Races);
            }

            //language
            if (personDto.Languages != null && personDto.Languages.Any())
            {
                personEntity.Languages.AddRange(ConvertPerson2DtoLanguagesToEntity(personId, personDto.Id,
                    personDto.Languages));
            }

            //marital status
            if (personDto.MaritalStatus != null)
            {
                personEntity = await ConvertPerson2MaritalStatusDtoToEntityAsync(personEntity, personDto.MaritalStatus);
            }

            //citizenshipStatus
            if (personDto.CitizenshipStatus != null)
            {
                personEntity.AlienStatus =
                    await ConvertPerson2CitizenshipStatusDtoToEntityAsync(personDto.CitizenshipStatus);
            }

            //countryOfBirth
            if (!string.IsNullOrEmpty(personDto.CountryOfBirth))
            {
                personEntity.BirthCountry = await ConvertPerson2CountryCodeDtoToEntityAsync(personDto.CountryOfBirth);
            }

            //countryOfCitizenship
            if (!string.IsNullOrEmpty(personDto.CitizenshipCountry))
            {
                personEntity.Citizenship = await ConvertPerson2CountryCodeDtoToEntityAsync(personDto.CitizenshipCountry);
            }

            //roles
            if (personDto.Roles != null && personDto.Roles.Any())
            {
                personEntity.Roles.AddRange(ConvertPerson2DtoRolesToEntity(personDto.Roles));
            }

            //identityDocuments
            if (personDto.IdentityDocuments != null && personDto.IdentityDocuments.Any())
            {
                //type is required for identity documents.
                if (personDto.IdentityDocuments.Where(doc => doc.Type == null).Count() > 0)
                {
                    throw new InvalidOperationException("Type is required for identityDocuments.");
                }
                //Typoe category is required.
                if (personDto.IdentityDocuments.Where(doc => doc.Type != null && string.IsNullOrEmpty(doc.Type.Category.ToString())).Count() > 0)
                {
                    throw new InvalidOperationException("Type category is required for identityDocuments.");
                }
                //type detail Id required for detail object.
                if (personDto.IdentityDocuments.Where(doc => doc.Type != null && doc.Type.Detail != null && string.IsNullOrEmpty(doc.Type.Detail.Id)).Count() > 0)
                {
                    throw new InvalidOperationException("Type detail Id is required for identityDocuments.");
                }
                personEntity.Passport =
                    await
                        ConvertPerson2DtoPassportDocumentToEntityAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
                personEntity.DriverLicense =
                    await
                        ConvertPerson2DtoDriversLicenseToEntityDocumentAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
                personEntity.IdentityDocuments =
                    await
                        ConvertPerson2DtoIdentityDocumentsToEntityAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
            }

            // Social Media
            if (personDto.SocialMedia != null && personDto.SocialMedia.Any())
            {
                personEntity.SocialMedia.AddRange(await ConvertPerson2DtoSocialMediaToEntity(personDto.SocialMedia));
            }

            // credentials
            if (personDto.Credentials != null && personDto.Credentials.Any())
            {
                if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType2.ColleagueUserName) > 0 && !string.IsNullOrEmpty(personId))
                {
                    // check to see if user added a Colleague Username; They cannot maintain a username through API's but 
                    // If they entered the same value that already stored in colleague we won't issue an error.
                    // Otherwise issue an error indicating that colleague usernames are to be maintained in colleague.
                    CredentialDtoProperty2 username = personDto.Credentials.FirstOrDefault(i => i.Type == Dtos.EnumProperties.CredentialType2.ColleagueUserName);

                    string[] personGuids = { personEntity.Guid };
                    await this.GetPersonPins(personGuids);
                    var personPin = _personPins.FirstOrDefault();

                    if (personPin == null || personPin.PersonPinUserId != username.Value)
                    {
                        throw new InvalidOperationException("You cannot add/edit Colleague usernames. You must maintain them in Colleague.");
                    }
                    if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType2.ColleagueUserName) > 1)
                    {
                        throw new InvalidOperationException("You cannot include more than one Colleague username.");
                    }
                }
                else if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType2.ColleagueUserName) > 0 && string.IsNullOrEmpty(personId))
                {
                    throw new InvalidOperationException("You cannot add/edit Colleague usernames. You must maintain them in Colleague.");
                }
                ConvertPerson3DtoCredsToEntity(personId, personDto.Credentials, personEntity);
            }

            // interests
            if (personDto.Interests != null && personDto.Interests.Any())
            {
                personEntity.Interests = await ConvertPerson2DtoInterestsToEntityAsync(personDto.Interests);
            }

            return personEntity;
        }

        private async Task<Domain.Base.Entities.PersonIntegration> ConvertPerson4DtoToEntityAsync(string personId,
            Dtos.Person4 personDto)
        {
            if (personDto == null || string.IsNullOrEmpty(personDto.Id))
                throw new ArgumentNullException("personDto", "Must provide guid for person");

            PersonIntegration personEntity = null;
            personEntity = await ConvertPersonNames2(personId, personDto.Id, personDto.PersonNames);
            personEntity.BirthDate = personDto.BirthDate;
            personEntity.DeceasedDate = personDto.DeceasedDate;

            // email addresses
            var emailAddressEntities = await MapEmailAddresses2(personDto.EmailAddresses);
            if (emailAddressEntities != null && emailAddressEntities.Count() > 0)
            {
                foreach (var emailAddressEntity in emailAddressEntities)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            //Make sure birth date is before today's date
            if (personDto.BirthDate != null && personDto.BirthDate > DateTime.Today)
            {
                throw new InvalidOperationException("Date of birth cannot be after the current date.");
            }

            //Make sure birth date is before deceased date
            if (personDto.BirthDate != null && personDto.DeceasedDate != null && personDto.BirthDate > personDto.DeceasedDate)
            {
                throw new InvalidOperationException("Date of birth cannot be after deceased date.");
            }

            //privacy status
            if (personDto.PrivacyStatus != null)
            {
                var privacyStatusEntity = await ConvertPerson2PrivacyStatusAsync(personDto.PrivacyStatus);
                if (privacyStatusEntity == null)
                {
                    personEntity.PrivacyStatus = PrivacyStatusType.unrestricted;
                }
                else
                {
                    personEntity.PrivacyStatus = privacyStatusEntity.PrivacyStatusType;
                    personEntity.PrivacyStatusCode = privacyStatusEntity.Code;
                }
            }

            //Gender
            if (personDto.GenderType != null)
            {
                personEntity.Gender = ConvertGenderType2String(personDto.GenderType);
            }

            //religion
            if (personDto.Religion != null)
            {
                personEntity.Religion = await ConvertPerson2DtoReligionCodeToEntityAsync(personDto.Religion.Id);
            }

            //ethnicity code
            if (personDto.Ethnicity != null)
            {
                personEntity.EthnicCodes = await ConvertPerson2DtoEthnicityCodesEntityAsync(personDto.Ethnicity);
            }

            //races
            if (personDto.Races != null && personDto.Races.Any())
            {
                personEntity.RaceCodes = await ConvertPerson2DtoRaceCodesToEntityAsync(personDto.Races);
            }

            //language
            if (personDto.Languages != null && personDto.Languages.Any())
            {
                personEntity.Languages.AddRange(ConvertPerson2DtoLanguagesToEntity(personId, personDto.Id,
                    personDto.Languages));
            }

            //marital status
            if (personDto.MaritalStatus != null)
            {
                personEntity = await ConvertPerson2MaritalStatusDtoToEntityAsync(personEntity, personDto.MaritalStatus);
            }

            //citizenshipStatus
            if (personDto.CitizenshipStatus != null)
            {
                personEntity.AlienStatus =
                    await ConvertPerson2CitizenshipStatusDtoToEntityAsync(personDto.CitizenshipStatus);
            }

            //countryOfBirth
            if (!string.IsNullOrEmpty(personDto.CountryOfBirth))
            {
                personEntity.BirthCountry = await ConvertPerson2CountryCodeDtoToEntityAsync(personDto.CountryOfBirth);
            }

            //countryOfCitizenship
            if (!string.IsNullOrEmpty(personDto.CitizenshipCountry))
            {
                personEntity.Citizenship = await ConvertPerson2CountryCodeDtoToEntityAsync(personDto.CitizenshipCountry);
            }

            //roles
            if (personDto.Roles != null && personDto.Roles.Any())
            {
                personEntity.Roles.AddRange(ConvertPerson2DtoRolesToEntity(personDto.Roles));
            }

            //identityDocuments
            if (personDto.IdentityDocuments != null && personDto.IdentityDocuments.Any())
            {
                //type is required for identity documents.
                if (personDto.IdentityDocuments.Where(doc => doc.Type == null).Count() > 0)
                {
                    throw new InvalidOperationException("Type is required for identityDocuments.");
                }
                //Typoe category is required.
                if (personDto.IdentityDocuments.Where(doc => doc.Type != null && string.IsNullOrEmpty(doc.Type.Category.ToString())).Count() > 0)
                {
                    throw new InvalidOperationException("Type category is required for identityDocuments.");
                }
                //type detail Id required for detail object.
                if (personDto.IdentityDocuments.Where(doc => doc.Type != null && doc.Type.Detail != null && string.IsNullOrEmpty(doc.Type.Detail.Id)).Count() > 0)
                {
                    throw new InvalidOperationException("Type detail Id is required for identityDocuments.");
                }
                personEntity.Passport =
                    await
                        ConvertPerson2DtoPassportDocumentToEntityAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
                personEntity.DriverLicense =
                    await
                        ConvertPerson2DtoDriversLicenseToEntityDocumentAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
                personEntity.IdentityDocuments =
                    await
                        ConvertPerson2DtoIdentityDocumentsToEntityAsync(personId, personDto.Id,
                            personDto.IdentityDocuments);
            }

            // Social Media
            if (personDto.SocialMedia != null && personDto.SocialMedia.Any())
            {
                personEntity.SocialMedia.AddRange(await ConvertPerson2DtoSocialMediaToEntity(personDto.SocialMedia));
            }

            // credentials
            if (personDto.Credentials != null && personDto.Credentials.Any())
            {
                if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleagueUserName) > 0 && !string.IsNullOrEmpty(personId))
                {
                    // check to see if user added a Colleague Username; They cannot maintain a username through API's but 
                    // If they entered the same value that already stored in colleague we won't issue an error.
                    // Otherwise issue an error indicating that colleague usernames are to be maintained in colleague.
                    Dtos.DtoProperties.Credential3DtoProperty username = personDto.Credentials.FirstOrDefault(i => i.Type == Dtos.EnumProperties.Credential3Type.ColleagueUserName);

                    string[] personGuids = { personEntity.Guid };
                    await this.GetPersonPins(personGuids);
                    if (_personPins == null)
                    {
                        throw new InvalidOperationException("You cannot add/edit Colleague usernames. You must maintain them in Colleague.");
                    }
                    var personPin = _personPins.FirstOrDefault();

                    if (personPin == null || personPin.PersonPinUserId != username.Value)
                    {
                        throw new InvalidOperationException("You cannot add/edit Colleague usernames. You must maintain them in Colleague.");
                    }
                    if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleagueUserName) > 1)
                    {
                        throw new InvalidOperationException("You cannot include more than one Colleague username.");
                    }
                }
                else if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleagueUserName) > 0 && string.IsNullOrEmpty(personId))
                {
                    throw new InvalidOperationException("You cannot add/edit Colleague usernames. You must maintain them in Colleague.");
                }
                ConvertPerson4DtoCredsToEntity(personId, personDto.Credentials, personEntity);
            }

            // interests
            if (personDto.Interests != null && personDto.Interests.Any())
            {
                personEntity.Interests = await ConvertPerson2DtoInterestsToEntityAsync(personDto.Interests);
            }

            return personEntity;
        }

        private async Task<Domain.Base.Entities.PersonIntegration> ConvertPerson5DtoToEntityAsync(string personId,
            Dtos.Person5 personDto)
        {
            if (personDto == null || string.IsNullOrEmpty(personDto.Id))
            {
                IntegrationApiExceptionAddError("Must provide guid for person", "persons.id", personDto.Id, personId);
            }

            PersonIntegration personEntity = null;
            personEntity = await ConvertPersonNames3(personId, personDto.Id, personDto.PersonNames);
            if (personEntity != null)
            {
                personEntity.BirthDate = personDto.BirthDate;
                personEntity.DeceasedDate = personDto.DeceasedDate;
            }

            // email addresses
            var emailAddressEntities = await MapEmailAddresses3(personDto.EmailAddresses);
            if (personEntity != null && emailAddressEntities != null && emailAddressEntities.Count() > 0)
            {
                foreach (var emailAddressEntity in emailAddressEntities)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            //Make sure birth date is before today's date
            if (personDto.BirthDate != null && personDto.BirthDate > DateTime.Today)
            {
                IntegrationApiExceptionAddError("Date of birth cannot be after the current date.", "persons.dateOfBirth", personDto.Id, personId);
            }

            //Make sure birth date is before deceased date
            if (personDto.BirthDate != null && personDto.DeceasedDate != null && personDto.BirthDate > personDto.DeceasedDate)
            {
                IntegrationApiExceptionAddError("Date of birth cannot be after deceased date.", "persons.dateDeceased", personDto.Id, personId);
            }

            //privacy status
            if (personDto.PrivacyStatus != null)
            {
                var privacyStatusEntity = await ConvertPerson3PrivacyStatusAsync(personDto.PrivacyStatus, personDto.Id, personId);
                if (personEntity != null)
                {
                    if (privacyStatusEntity == null)
                    {
                        personEntity.PrivacyStatus = PrivacyStatusType.unrestricted;
                    }
                    else
                    {
                        personEntity.PrivacyStatus = privacyStatusEntity.PrivacyStatusType;
                        personEntity.PrivacyStatusCode = privacyStatusEntity.Code;
                    }
                }
            }

            //Gender
            if (personDto.GenderType != null)
            {
                var gender = ConvertGenderType2String(personDto.GenderType);
                if (personEntity != null)
                    personEntity.Gender = gender;
            }

            //get Gender Indentity Code
            if (personDto.GenderIdentity != null)
            {
                var genderIdCode = string.Empty;
                if (string.IsNullOrEmpty(personDto.GenderIdentity.Id))
                {
                    IntegrationApiExceptionAddError("Gender Identity id is a required field when Gender Identity is in the message body.", "persons.genderIdentity.id", personDto.Id, personId);
                }
                else
                {
                    try
                    {
                        genderIdCode = ConvertGuidToCode(await GetGenderIdentityTypesAsync(false), personDto.GenderIdentity.Id);
                    }
                    catch
                    {
                        genderIdCode = string.Empty;
                    }
                    if (string.IsNullOrEmpty(genderIdCode) && personDto.GenderIdentity.Id != null)
                    {
                        IntegrationApiExceptionAddError(string.Concat("Gender Identity ID '", personDto.GenderIdentity.Id.ToString(), "' was not found."), "persons.genderIdentity.id", personDto.Id, personId);
                    }
                    else
                    {
                        if (personEntity != null)
                            personEntity.GenderIdentityCode = genderIdCode;
                    }
                }
            }

            //get Personal Pronoun code
            if (personDto.PersonalPronoun != null)
            {
                var personalPronounCode = string.Empty;
                if (string.IsNullOrEmpty(personDto.PersonalPronoun.Id))
                {
                    IntegrationApiExceptionAddError("Personal Pronoun id is a required field when Personal Pronoun is in the message body.", "persons.personalPronoun.id", personDto.Id, personId);
                }
                else
                {
                    try
                    {
                        personalPronounCode = ConvertGuidToCode(await GetPersonalPronounTypesAsync(false), personDto.PersonalPronoun.Id);
                    }
                    catch
                    {
                        personalPronounCode = string.Empty;
                    }
                    if (string.IsNullOrEmpty(personalPronounCode) && personDto.PersonalPronoun.Id != null)
                    {
                        IntegrationApiExceptionAddError(string.Concat("Personal Pronoun ID '", personDto.PersonalPronoun.Id.ToString(), "' was not found."), "persons.personalPronoun.id", personDto.Id, personId);
                    }
                    else
                    {
                        if (personEntity != null)
                            personEntity.PersonalPronounCode = personalPronounCode;
                    }
                }
            }

            //religion
            if (personDto.Religion != null)
            {
                try
                {
                    var religion = await ConvertPerson2DtoReligionCodeToEntityAsync(personDto.Religion.Id);
                    if (personEntity != null)
                        personEntity.Religion = religion;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.religion", personDto.Id, personId);
                }
            }

            //ethnicity code
            if (personDto.Ethnicity != null)
            {
                try
                {
                    var ethnicCodes = await ConvertPerson2DtoEthnicityCodesEntityAsync(personDto.Ethnicity);
                    if (personEntity != null)
                        personEntity.EthnicCodes = ethnicCodes;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.ethnicity", personDto.Id, personId);
                }
            }

            //races
            if (personDto.Races != null && personDto.Races.Any())
            {
                try
                {
                    var races = await ConvertPerson2DtoRaceCodesToEntityAsync(personDto.Races);
                    if (personEntity != null)
                        personEntity.RaceCodes = races;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.races", personDto.Id, personId);
                }
            }

            //language
            if (personDto.Languages != null && personDto.Languages.Any())
            {
                try
                {
                    var languages = ConvertPerson2DtoLanguagesToEntity(personId, personDto.Id, personDto.Languages);
                    if (personEntity != null)
                        personEntity.Languages.AddRange(languages);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.languages", personDto.Id, personId);
                }
            }

            //marital status
            if (personDto.MaritalStatus != null)
            {
                try
                {
                    personEntity = await ConvertPerson2MaritalStatusDtoToEntityAsync(personEntity, personDto.MaritalStatus);                
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.maritalStatus", personDto.Id, personId);
                }
            }

            //citizenshipStatus
            if (personDto.CitizenshipStatus != null)
            {
                try
                {
                    var alienStatus = await ConvertPerson2CitizenshipStatusDtoToEntityAsync(personDto.CitizenshipStatus);
                    if (personEntity != null)
                        personEntity.AlienStatus = alienStatus;
                        
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.citizenshipStatus", personDto.Id, personId);
                }
            }

            //countryOfBirth
            if (!string.IsNullOrEmpty(personDto.CountryOfBirth))
            {
                try
                {
                    var birthCountry = await ConvertPerson2CountryCodeDtoToEntityAsync(personDto.CountryOfBirth);
                    if (personEntity != null)
                        personEntity.BirthCountry = birthCountry;
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Country not found with Iso3 code: '{0}'", personDto.CountryOfBirth), "persons.countryOfBirth", personDto.Id, personId);
                }
            }

            //countryOfCitizenship
            if (!string.IsNullOrEmpty(personDto.CitizenshipCountry))
            {
                try
                {
                    var citizenship = await ConvertPerson2CountryCodeDtoToEntityAsync(personDto.CitizenshipCountry);
                    if (personEntity != null)
                        personEntity.Citizenship = citizenship;
                }
                catch (Exception)
                {
                    IntegrationApiExceptionAddError(string.Format("Country not found with Iso3 code: '{0}'", personDto.CitizenshipCountry), "persons.citizenshipCountry", personDto.Id, personId);
                }
            }

            //roles
            if (personDto.Roles != null && personDto.Roles.Any())
            {
                try
                {
                    var roles = ConvertPerson2DtoRolesToEntity(personDto.Roles);
                    if (personEntity != null)
                        personEntity.Roles.AddRange(roles);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.roles", personDto.Id, personId);
                }
            }

            //identityDocuments
            if (personDto.IdentityDocuments != null && personDto.IdentityDocuments.Any())
            {
                //type is required for identity documents.
                if (personDto.IdentityDocuments.Where(doc => doc.Type == null).Count() > 0)
                {
                    IntegrationApiExceptionAddError("Type is required for identityDocuments.", "persons.identityDocuments.type", personDto.Id, personId);
                }
                //Type category is required.
                if (personDto.IdentityDocuments.Where(doc => doc.Type != null && string.IsNullOrEmpty(doc.Type.Category.ToString())).Any())
                {
                    IntegrationApiExceptionAddError("Type category is required for identityDocuments.", "persons.identityDocuments.type.category", personDto.Id, personId);
                }
                //type detail Id required for detail object.
                if (personDto.IdentityDocuments.Where(doc => doc.Type != null && doc.Type.Detail != null && string.IsNullOrEmpty(doc.Type.Detail.Id)).Any())
                {
                    IntegrationApiExceptionAddError("Type detail Id is required for identityDocuments.", "persons.identityDocuments.type.detail.id", personDto.Id, personId);
                }
                // Passport
                try
                {
                    var passPort = await ConvertPerson2DtoPassportDocumentToEntityAsync(personId, personDto.Id, personDto.IdentityDocuments);
                    if (personEntity != null)
                        personEntity.Passport = passPort;
                        
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.identityDocuments", personDto.Id, personId);
                }
                // Drivers License
                try
                {
                    var driverLicense = await ConvertPerson2DtoDriversLicenseToEntityDocumentAsync(personId, personDto.Id, personDto.IdentityDocuments);
                    if (personEntity != null)
                        personEntity.DriverLicense = driverLicense;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.identityDocuments", personDto.Id, personId);
                }
                // Other documents
                try
                {
                    var identityDocuments = await ConvertPerson2DtoIdentityDocumentsToEntityAsync(personId, personDto.Id, personDto.IdentityDocuments);
                    if (personEntity != null)
                        personEntity.IdentityDocuments = identityDocuments;  
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.identityDocuments", personDto.Id, personId);
                }
            }

            // Social Media
            if (personDto.SocialMedia != null && personDto.SocialMedia.Any())
            {
                try
                {
                    var socialMedia = await ConvertPerson2DtoSocialMediaToEntity(personDto.SocialMedia);
                    if (personEntity != null)
                        personEntity.SocialMedia.AddRange(socialMedia);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.socialMedia", personDto.Id, personId);
                }
            }

            // credentials
            if (personDto.Credentials != null && personDto.Credentials.Any())
            {
                if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleagueUserName) > 0 && !string.IsNullOrEmpty(personId))
                {
                    // check to see if user added a Colleague Username; They cannot maintain a username through API's but 
                    // If they entered the same value that already stored in colleague we won't issue an error.
                    // Otherwise issue an error indicating that colleague usernames are to be maintained in colleague.
                    Dtos.DtoProperties.Credential3DtoProperty username = personDto.Credentials.FirstOrDefault(i => i.Type == Dtos.EnumProperties.Credential3Type.ColleagueUserName);

                    if (personEntity != null)
                    {
                        string[] personGuids = { personEntity.Guid };
                        await this.GetPersonPins(personGuids);
                        if (_personPins == null)
                        {
                            IntegrationApiExceptionAddError("You cannot add/edit Colleague usernames. You must maintain them in Colleague.", "persons.credentials", personDto.Id, personId);
                        }
                        else
                        {
                            var personPin = _personPins.FirstOrDefault();

                            if (personPin == null || personPin.PersonPinUserId != username.Value)
                            {
                                IntegrationApiExceptionAddError("You cannot add/edit Colleague usernames. You must maintain them in Colleague.", "persons.credentials", personDto.Id, personId);
                            }
                            else
                            {
                                if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleagueUserName) > 1)
                                {
                                    IntegrationApiExceptionAddError("You cannot include more than one Colleague username.", "persons.credentials", personDto.Id, personId);
                                }
                            }
                        }
                    }
                }
                else if (personDto.Credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleagueUserName) > 0 && string.IsNullOrEmpty(personId))
                {
                    IntegrationApiExceptionAddError("You cannot add/edit Colleague usernames. You must maintain them in Colleague.", "persons.credentials", personDto.Id, personId);
                }
                try
                {
                    if (personEntity != null)
                        ConvertPerson5DtoCredsToEntity(personId, personDto.Credentials, personEntity);
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.credentials", personDto.Id, personId);
                }
            }

            // interests
            if (personDto.Interests != null && personDto.Interests.Any())
            {
                try
                {
                    var interests = await ConvertPerson2DtoInterestsToEntityAsync(personDto.Interests);
                    if (personEntity != null)
                        personEntity.Interests = interests;
                }
                catch (Exception ex)
                {
                    IntegrationApiExceptionAddError(ex.Message, "persons.interests", personDto.Id, personId);
                }
            }

            //set alternative credentials
            if (personDto.AlternativeCredentials != null && personDto.AlternativeCredentials.Any())
            {
                foreach (var cred in personDto.AlternativeCredentials)
                {
                    if (string.IsNullOrEmpty(cred.Value))
                    {
                        IntegrationApiExceptionAddError("Alternative Credentials value is a required field when Alternative Credentials is in the message body.", "persons.alternativeCredentials", personDto.Id, personId);
                    }
                    else
                    {
                        //change the type to code
                        var altCredTypeCode = string.Empty;
                        if (cred.Type != null)
                        {
                            if (string.IsNullOrEmpty(cred.Type.Id))
                            {
                                IntegrationApiExceptionAddError("Alternative Credentials Type id is a required field when Alternative Credentials Type is in the message body.", "persons.alternativeCredentials", personDto.Id, personId);
                            }
                            else
                            {
                                try
                                {
                                    altCredTypeCode = ConvertGuidToCode(await GetAlternateIdTypesAsync(false), cred.Type.Id);
                                }
                                catch
                                {
                                    altCredTypeCode = string.Empty;
                                }
                                if (string.IsNullOrEmpty(altCredTypeCode) && cred.Type.Id != null)
                                {
                                    IntegrationApiExceptionAddError(string.Concat("Alternative Credentials Type Id '", cred.Type.Id, "' was not found."), "persons.alternativeCredentials", personDto.Id, personId);
                                }
                                //elevate Id is not supported here. It needs to be in credentials 
                                if (!string.IsNullOrEmpty(altCredTypeCode) && altCredTypeCode.Equals("ELEV"))
                                {
                                    IntegrationApiExceptionAddError(string.Concat("Alternative Credentials Type Id '", cred.Type.Id, "' is not valid. An Elevate ID must be requested using the credentials array"), "persons.alternativeCredentials", personDto.Id, personId);
                                }
                            }
                        }
                        else
                        {
                            IntegrationApiExceptionAddError("Alternative Credentials Type id is a required field when Alternative Credentials is in the message body.", "persons.alternativeCredentials", personDto.Id, personId);
                        }
                        if (!string.IsNullOrEmpty(altCredTypeCode))
                        {
                            var altId = new PersonAlt(cred.Value, altCredTypeCode);
                            if (personEntity != null && altId != null)
                                personEntity.PersonAltIds.Add(altId);
                        }
                    }
                }
            }

            return personEntity;
        }

        private async Task<PersonIntegration> ConvertPersonNames(string personId, string guid, IEnumerable<Dtos.DtoProperties.PersonNameDtoProperty> PersonNames)
        {
            PersonIntegration personEntity = null;

            if (PersonNames == null)
                throw new ArgumentNullException("personDto", "Must provide person name");

            var nameCategories =
              PersonNames.Where(pn => pn.NameType == null || pn.NameType.Category == null).ToList();

            if (nameCategories.Any())
            {
                throw new ArgumentNullException("personDto", "Name type category is required.");
            }

            var preferredNames =
                PersonNames.Where(pn => pn.Preference == Dtos.EnumProperties.PersonNamePreference.Preferred)
                    .ToList();

            if (preferredNames.Any() && preferredNames.Count > 1)
            {
                throw new ArgumentNullException("personDto", "Only one name type can be identified as preferred.");
            }
            var nameTypes = (await GetPersonNameTypesAsync(false)).ToList();
                        
            // person legal name
            var primaryNames =
                PersonNames.Where(pn => pn.NameType.Category == PersonNameType2.Legal).ToList();
            if (!primaryNames.Any())
            {
                throw new ArgumentNullException("personDto", "A legal name is required in the names array.");
            }
            if (primaryNames.Count() > 1)
            {
                throw new ArgumentException("personDto",
                    "Colleague does not support more than one legal name for a person.");
            }
            var primaryName = primaryNames.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(primaryName.LastName))
            {
                throw new ArgumentNullException("personDto", "Last name is required for a legal name.");
            }
            if (string.IsNullOrEmpty(primaryName.FullName))
            {
                throw new ArgumentNullException("personDto", "Full name is required for a legal name.");
            }
            if ((primaryName.NameType.Detail != null) && (primaryName.NameType.Detail.Id == null))
            {
                throw new ArgumentException("personDto", "If providing Legal NameType Detail, then Detail.id is required.");
            }
            if ((primaryName.NameType.Detail != null) && (primaryName.NameType.Detail.Id != null))
            {
                var legalNameType = nameTypes.FirstOrDefault(x => x.Code == "LEGAL");
                if (legalNameType == null)
                {
                    throw new RepositoryException("Unable to retrieve PersonNameType used to detemine legal name.");
                }
                if (!primaryName.NameType.Detail.Id.Equals(legalNameType.Guid, StringComparison.InvariantCulture))
                {
                    throw new ArgumentException("personDto", "Detail.id provided for category legal not valid.");
                }
            }

            //process last name prefix
            if (!string.IsNullOrEmpty(primaryName.LastNamePrefix) && !string.IsNullOrEmpty(primaryName.LastName))
            {
                string lprefix = primaryName.LastName.Substring(0, primaryName.LastNamePrefix.Length);
                if (!string.Equals(primaryName.LastNamePrefix, lprefix, StringComparison.OrdinalIgnoreCase))
                    primaryName.LastName = string.Concat(primaryName.LastNamePrefix, " ", primaryName.LastName);
            }

            // professional abbreviations
            var professionalAbbreviations = new List<string>();

            if (primaryName.ProfessionalAbbreviation != null)
            {
                professionalAbbreviations = primaryName.ProfessionalAbbreviation.ToList();
            }

            personEntity = new PersonIntegration(personId, primaryName.LastName)
            {
                Guid = guid,
                Prefix = primaryName.Title,
                FirstName = primaryName.FirstName,
                MiddleName = primaryName.MiddleName,
                Suffix = primaryName.Pedigree,
                ProfessionalAbbreviations = professionalAbbreviations
            };
            if (primaryName.Preference == PersonNamePreference.Preferred)
            {
                personEntity.PreferredName = primaryName.FullName;
                personEntity.PreferredNameType = "LEGAL";
            }


            // person birth name
            var birthNames =
                PersonNames.Where(pn => pn.NameType != null && pn.NameType.Category == PersonNameType2.Birth).ToList();

            if (birthNames.Any())
            {
                if (birthNames.Count() > 1)
                {
                    throw new ArgumentNullException("personDto",
                        "Colleague does not support more than one birth name for a person.");
                }
                var birthName = birthNames.FirstOrDefault();

                if (birthName != null)
                {
                    if ((string.IsNullOrEmpty(birthName.FirstName)) &&
                        (string.IsNullOrEmpty(birthName.LastName)) &&
                        (string.IsNullOrEmpty(birthName.MiddleName)))
                    {
                        throw new ArgumentNullException("personDto",
                            "Either the firstName, middleName, or lastName is needed for a birth name.");
                    }
                    if (string.IsNullOrEmpty(birthName.FullName))
                    {
                        throw new ArgumentNullException("personDto", "Full Name is needed for a birth name.");
                    }
                    if ((birthName.NameType.Detail != null) && (birthName.NameType.Detail.Id == null))
                    {
                        throw new ArgumentException("personDto", "If providing Birth NameType Detail, then Detail.id is required.");
                    }
                    if ((birthName.NameType.Detail != null) && (birthName.NameType.Detail.Id != null))
                    {
                        var birthNameType = nameTypes.FirstOrDefault(x => x.Code == "BIRTH");
                        if (birthNameType == null)
                        {
                            throw new RepositoryException(
                                "Unable to retrieve PersonNameType used to detemine birth name.");
                        }
                        if (!birthName.NameType.Detail.Id.Equals(birthNameType.Guid, StringComparison.InvariantCulture))
                        {
                            throw new ArgumentException("personDto", "Detail.id provided for category birth not valid.");
                        }
                    }

                    //process last name prefix
                    if (!string.IsNullOrEmpty(birthName.LastNamePrefix) && !string.IsNullOrEmpty(birthName.LastName))
                    {
                        string bprefix = birthName.LastName.Substring(0, birthName.LastNamePrefix.Length);
                        if (!string.Equals(birthName.LastNamePrefix, bprefix, StringComparison.OrdinalIgnoreCase))
                            birthName.LastName = string.Concat(birthName.LastNamePrefix, " ", birthName.LastName);
                    }

                    personEntity.BirthNameLast = birthName.LastName;
                    personEntity.BirthNameFirst = birthName.FirstName;
                    personEntity.BirthNameMiddle = birthName.MiddleName;

                    if (birthName.Preference == PersonNamePreference.Preferred)
                    {
                        personEntity.PreferredName = birthName.FullName;
                        personEntity.PreferredNameType = "BIRTH";
                    }
                }
            }

            var personalNames =
                PersonNames.Where(pn => pn.NameType != null && pn.NameType.Category == PersonNameType2.Personal).ToList();

            if (personalNames.Any())
            {
                // Person Nick Name
                // Nickname are a sub-group of the personal name type.  Determined by matching detail.id      
                var nickNameType = nameTypes.FirstOrDefault(x => x.Code == "NICKNAME");
                if (nickNameType == null)
                {
                    throw new RepositoryException("Unable to retrieve PersonNameType used to detemine nickname.");
                }

                var nickNames =
                        personalNames.Where(pn => pn.NameType != null && pn.NameType.Detail != null && !string.IsNullOrEmpty(pn.NameType.Detail.Id) && pn.NameType.Detail.Id.Equals(nickNameType.Guid, StringComparison.OrdinalIgnoreCase));
                if (nickNames.Any())
                {
                    if (nickNames.Count() > 1)
                    {
                        throw new ArgumentNullException("personDto",
                            "Colleague does not support more than one nickname for a person.");
                    }
                    var nickName = nickNames.FirstOrDefault();
                    if (nickName != null)
                    {
                        if (string.IsNullOrEmpty(nickName.FullName))
                        {
                            throw new ArgumentNullException("personDto", "Full Name is required for a nickname.");
                        }

                        // On PUT and POST, if NickName is identified as Preferred, than the First Name property of NickName will be stored into 
                        // Colleague’s Nickname field unless the First Name property of NickName is blank.  If that is blank, the fullName
                        // property of NickName will be stored in Colleague’s NickName 
                        if (nickName.Preference == PersonNamePreference.Preferred)
                        {
                            personEntity.PreferredName = nickName.FullName;
                            personEntity.Nickname = string.IsNullOrEmpty(nickName.FirstName)
                                ? nickName.FullName
                                : nickName.FirstName;
                            personEntity.PreferredNameType = "NICKNAME";
                        }
                        else
                        {
                            personEntity.Nickname = nickName.FullName;
                        }
                    }
                }

                // person chosen name
                // Chosen names are a sub-group of the personal name type.  Determined by matching detail.id      
                var chosenNameType = nameTypes.FirstOrDefault(x => x.Code == "CHOSEN");
                if (chosenNameType == null)
                {
                    throw new RepositoryException("Unable to retrieve PersonNameType used to detemine chosen name.");
                }

                var chosenNames =
                        personalNames.Where(pn => pn.NameType != null && pn.NameType.Detail != null && !string.IsNullOrEmpty(pn.NameType.Detail.Id) && pn.NameType.Detail.Id.Equals(chosenNameType.Guid, StringComparison.OrdinalIgnoreCase));
                if (chosenNames.Any())
                {
                    if (chosenNames.Count() > 1)
                    {
                        throw new ArgumentNullException("personDto",
                            "Colleague does not support more than one chosen name for a person.");
                    }
                    var chosenName = chosenNames.FirstOrDefault();

                    if (chosenName != null)
                    {
                        if ((string.IsNullOrEmpty(chosenName.FirstName)) &&
                            (string.IsNullOrEmpty(chosenName.LastName)) &&
                            (string.IsNullOrEmpty(chosenName.MiddleName)))
                        {
                            throw new ArgumentNullException("personDto",
                                "Either the firstName, middleName, or lastName is needed for a chosen name.");
                        }
                        if (string.IsNullOrEmpty(chosenName.FullName))
                        {
                            throw new ArgumentNullException("personDto", "Full Name is needed for a chosen name.");
                        }
                        if ((chosenName.NameType.Detail != null) && (chosenName.NameType.Detail.Id == null))
                        {
                            throw new ArgumentException("personDto", "If providing Chosen NameType Detail, then Detail.id is required.");
                        }
                        if ((chosenName.NameType.Detail != null) && (chosenName.NameType.Detail.Id != null))
                        {
                            if (chosenNameType == null)
                            {
                                throw new RepositoryException(
                                    "Unable to retrieve PersonNameType used to detemine chosen name.");
                            }
                            if (!chosenName.NameType.Detail.Id.Equals(chosenNameType.Guid, StringComparison.OrdinalIgnoreCase))
                            {
                                throw new ArgumentException("personDto", "Detail.id provided for category personal not valid.");
                            }
                        }

                        //process last name prefix
                        if (!string.IsNullOrEmpty(chosenName.LastNamePrefix) && !string.IsNullOrEmpty(chosenName.LastName))
                        {
                            string cPrefix = chosenName.LastName.Substring(0, chosenName.LastNamePrefix.Length);
                            if (!string.Equals(chosenName.LastNamePrefix, cPrefix, StringComparison.OrdinalIgnoreCase))
                                chosenName.LastName = string.Concat(chosenName.LastNamePrefix, " ", chosenName.LastName);
                        }
                        personEntity.ChosenLastName = chosenName.LastName;
                        personEntity.ChosenFirstName = chosenName.FirstName;
                        personEntity.ChosenMiddleName = chosenName.MiddleName;

                        if (chosenName.Preference == PersonNamePreference.Preferred)
                        {
                            personEntity.PreferredName = chosenName.FullName;
                            personEntity.PreferredNameType = "CHOSEN";
                        }
                    }
                }

                // all other remaining personal names are recorded as historical names.
                var historyNameType = nameTypes.FirstOrDefault(x => x.Code == "HISTORY");
                if (historyNameType == null)
                {
                    throw new RepositoryException("Unable to retrieve PersonNameType used to detemine historyname.");
                }

                // From the subset of personal names, retrieve history/former names
                // For a PUT or POST where the names.type.category is "personal" with no corresponding detail guid, 
                // we will save the incoming name to the NAME.HISTORY.FIRST.NAME, NAME.HISTORY.LAST.NAME, and 
                // NAME.HISTORY.MIDDLE.NAME. 

                var historyNames =
                        personalNames.Where(
                            pn =>
                                (pn.NameType.Detail == null || pn.NameType.Detail.Id == null) ||
                                pn.NameType.Detail.Id == historyNameType.Guid).ToList();

                var formerNames = new List<PersonName>();
                foreach (var historyName in historyNames)
                {
                    if ((historyName.NameType.Detail != null) && (historyName.NameType.Detail.Id == null))
                    {
                        throw new ArgumentException("personDto", "If providing History NameType Detail, then Detail.id is required.");
                    }
                    if (string.IsNullOrEmpty(historyName.LastName))
                    {
                        throw new ArgumentNullException("personDto",
                                                "Last Name is required for a former name.");
                    }
                    if (historyName.Preference == Dtos.EnumProperties.PersonNamePreference.Preferred)
                    {
                        throw new ArgumentException("personDto",
                            "Can not assign Preferred to a name type of Former Name.");
                    }
                    if (string.IsNullOrEmpty(historyName.FullName))
                    {
                        throw new ArgumentNullException("personDto", "Full Name is required for a former name.");
                    }

                    //process last name prefix
                    if (!string.IsNullOrEmpty(historyName.LastNamePrefix) && !string.IsNullOrEmpty(historyName.LastName))
                    {
                        string hprefix = historyName.LastName.Substring(0, historyName.LastNamePrefix.Length);
                        if (!string.Equals(historyName.LastNamePrefix, hprefix, StringComparison.OrdinalIgnoreCase))
                            historyName.LastName = string.Concat(historyName.LastNamePrefix, " ", historyName.LastName);
                    }

                    var formerName = new PersonName(historyName.FirstName, historyName.MiddleName,
                        historyName.LastName);
                    formerNames.Add(formerName);

                }
                personEntity.FormerNames = formerNames;
            }
            return personEntity;
        }

        private async Task<PersonIntegration> ConvertPersonNames2(string personId, string guid, IEnumerable<Dtos.DtoProperties.PersonName2DtoProperty> PersonNames)
        {
            PersonIntegration personEntity = null;

            if (PersonNames == null)
                throw new ArgumentNullException("personDto", "Must provide person name");

            var nameCategories =
              PersonNames.Where(pn => pn.NameType == null || pn.NameType.Category == null).ToList();

            if (nameCategories.Any())
            {
                throw new ArgumentNullException("personDto", "Name type category is required.");
            }

            var preferredNames =
                PersonNames.Where(pn => pn.Preference == Dtos.EnumProperties.PersonNamePreference.Preferred)
                    .ToList();

            if (preferredNames.Any() && preferredNames.Count > 1)
            {
                throw new ArgumentNullException("personDto", "Only one name type can be identified as preferred.");
            }
            var nameTypes = (await GetPersonNameTypesAsync(false)).ToList();
            //check to make sure the input name types are correct.
            foreach (var name in PersonNames)
            {
                if (name.NameType.Detail != null)
                {
                    var nameCategoryGuid = nameTypes.Where(nm => nm.Guid == name.NameType.Detail.Id);
                    if (nameCategoryGuid == null || !nameCategoryGuid.Any())
                    {
                        throw new ArgumentNullException("personDto", string.Concat("Name type category with detail Id of ", name.NameType.Detail.Id, " is not valid."));
                    }
                }
            }

            // person legal name
            var primaryNames =
                PersonNames.Where(pn => pn.NameType.Category == PersonNameType2.Legal).ToList();
            if (!primaryNames.Any())
            {
                throw new ArgumentNullException("personDto", "A legal name is required in the names array.");
            }
            if (primaryNames.Count() > 1)
            {
                throw new ArgumentException("personDto",
                    "Colleague does not support more than one legal name for a person.");
            }
            var primaryName = primaryNames.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(primaryName.LastName))
            {
                throw new ArgumentNullException("personDto", "Last name is required for a legal name.");
            }
            if (string.IsNullOrEmpty(primaryName.FullName))
            {
                throw new ArgumentNullException("personDto", "Full name is required for a legal name.");
            }
            if ((primaryName.NameType.Detail != null) && (primaryName.NameType.Detail.Id == null))
            {
                throw new ArgumentException("personDto", "If providing Legal NameType Detail, then Detail.id is required.");
            }
            if ((primaryName.NameType.Detail != null) && (primaryName.NameType.Detail.Id != null))
            {
                var legalNameType = nameTypes.FirstOrDefault(x => x.Code == "LEGAL");
                if (legalNameType == null)
                {
                    throw new RepositoryException("Unable to retrieve PersonNameType used to detemine legal name.");
                }
                if (!primaryName.NameType.Detail.Id.Equals(legalNameType.Guid, StringComparison.InvariantCulture))
                {
                    throw new ArgumentException("personDto", "Detail.id provided for category legal not valid.");
                }
            }
            //process last name prefix
            if (!string.IsNullOrEmpty(primaryName.LastNamePrefix) && !string.IsNullOrEmpty(primaryName.LastName))
            {
                string lprefix = primaryName.LastName.Substring(0, primaryName.LastNamePrefix.Length);
                if (!string.Equals(primaryName.LastNamePrefix, lprefix, StringComparison.OrdinalIgnoreCase))
                    primaryName.LastName = string.Concat(primaryName.LastNamePrefix, " ", primaryName.LastName);
            }
            personEntity = new PersonIntegration(personId, primaryName.LastName)
            {
                Guid = guid,
                Prefix = primaryName.Title,
                FirstName = primaryName.FirstName,
                MiddleName = primaryName.MiddleName,
                Suffix = primaryName.Pedigree,

            };
            if (primaryName.Preference == PersonNamePreference.Preferred)
            {
                personEntity.PreferredName = primaryName.FullName;
                personEntity.PreferredNameType = "LEGAL";
            }


            // person birth name
            var birthNames =
                PersonNames.Where(pn => pn.NameType.Category == PersonNameType2.Birth).ToList();

            if (birthNames.Any())
            {
                if (birthNames.Count() > 1)
                {
                    throw new ArgumentNullException("personDto",
                        "Colleague does not support more than one birth name for a person.");
                }
                var birthName = birthNames.FirstOrDefault();

                if (birthName != null)
                {
                    if ((string.IsNullOrEmpty(birthName.FirstName)) &&
                        (string.IsNullOrEmpty(birthName.LastName)) &&
                        (string.IsNullOrEmpty(birthName.MiddleName)))
                    {
                        throw new ArgumentNullException("personDto",
                            "Either the firstName, middleName, or lastName is needed for a birth name.");
                    }
                    if (string.IsNullOrEmpty(birthName.FullName))
                    {
                        throw new ArgumentNullException("personDto", "Full Name is needed for a birth name.");
                    }
                    if ((birthName.NameType.Detail != null) && (birthName.NameType.Detail.Id == null))
                    {
                        throw new ArgumentException("personDto", "If providing Birth NameType Detail, then Detail.id is required.");
                    }
                    if ((birthName.NameType.Detail != null) && (birthName.NameType.Detail.Id != null))
                    {
                        var birthNameType = nameTypes.FirstOrDefault(x => x.Code == "BIRTH");
                        if (birthNameType == null)
                        {
                            throw new RepositoryException(
                                "Unable to retrieve PersonNameType used to detemine birth name.");
                        }
                        if (!birthName.NameType.Detail.Id.Equals(birthNameType.Guid, StringComparison.InvariantCulture))
                        {
                            throw new ArgumentException("personDto", "Detail.id provided for category birth not valid.");
                        }
                    }

                    //process last name prefix
                    if (!string.IsNullOrEmpty(birthName.LastNamePrefix) && !string.IsNullOrEmpty(birthName.LastName))
                    {
                        string bprefix = birthName.LastName.Substring(0, birthName.LastNamePrefix.Length);
                        if (!string.Equals(birthName.LastNamePrefix, bprefix, StringComparison.OrdinalIgnoreCase))
                            birthName.LastName = string.Concat(birthName.LastNamePrefix, " ", birthName.LastName);
                    }
                    personEntity.BirthNameLast = birthName.LastName;
                    personEntity.BirthNameFirst = birthName.FirstName;
                    personEntity.BirthNameMiddle = birthName.MiddleName;

                    if (birthName.Preference == PersonNamePreference.Preferred)
                    {
                        personEntity.PreferredName = birthName.FullName;
                        personEntity.PreferredNameType = "BIRTH";
                    }
                }
            }

            var favoredNames = PersonNames.Where(pn => pn.NameType != null && pn.NameType.Category != null && pn.NameType.Category == PersonNameType2.Favored).ToList();
            if (favoredNames.Any())
            {

                var chosenNameType = nameTypes.FirstOrDefault(x => x.Code == "CHOSEN");
                if (chosenNameType == null)
                {
                    throw new RepositoryException("Unable to retrieve PersonNameType used to detemine chosen name.");
                }
                var chosenNames = favoredNames.Where(pn => pn.NameType != null && pn.NameType.Detail != null && !string.IsNullOrEmpty(pn.NameType.Detail.Id) && pn.NameType.Detail.Id.Equals(chosenNameType.Guid, StringComparison.OrdinalIgnoreCase));
                if (chosenNames == null || !chosenNames.Any())
                {
                    chosenNames = favoredNames;
                }
                if (chosenNames.Any())
                {
                    if (chosenNames.Count() > 1)
                    {
                        throw new ArgumentNullException("personDto",
                            "Colleague does not support more than one chosen name for a person.");
                    }
                    var chosenName = chosenNames.FirstOrDefault();

                    if (chosenName != null)
                    {
                        if ((string.IsNullOrEmpty(chosenName.FirstName)) &&
                            (string.IsNullOrEmpty(chosenName.LastName)) &&
                            (string.IsNullOrEmpty(chosenName.MiddleName)))
                        {
                            throw new ArgumentNullException("personDto",
                                "Either the firstName, middleName, or lastName is needed for a chosen name.");
                        }
                        if (string.IsNullOrEmpty(chosenName.FullName))
                        {
                            throw new ArgumentNullException("personDto", "Full Name is needed for a chosen name.");
                        }
                        if ((chosenName.NameType.Detail != null) && (chosenName.NameType.Detail.Id == null))
                        {
                            throw new ArgumentException("personDto", "If providing Chosen NameType Detail, then Detail.id is required.");
                        }
                        if ((chosenName.NameType.Detail != null) && (chosenName.NameType.Detail.Id != null))
                        {
                            if (chosenNameType == null)
                            {
                                throw new RepositoryException(
                                    "Unable to retrieve PersonNameType used to detemine chosen name.");
                            }
                            if (!chosenName.NameType.Detail.Id.Equals(chosenNameType.Guid, StringComparison.InvariantCulture))
                            {
                                throw new ArgumentException("personDto", "Detail.id provided for category personal not valid.");
                            }
                        }

                        //process last name prefix
                        if (!string.IsNullOrEmpty(chosenName.LastNamePrefix) && !string.IsNullOrEmpty(chosenName.LastName))
                        {
                            string cPrefix = chosenName.LastName.Substring(0, chosenName.LastNamePrefix.Length);
                            if (!string.Equals(chosenName.LastNamePrefix, cPrefix, StringComparison.OrdinalIgnoreCase))
                                chosenName.LastName = string.Concat(chosenName.LastNamePrefix, " ", chosenName.LastName);
                        }
                        personEntity.ChosenLastName = chosenName.LastName;
                        personEntity.ChosenFirstName = chosenName.FirstName;
                        personEntity.ChosenMiddleName = chosenName.MiddleName;

                        if (chosenName.Preference == PersonNamePreference.Preferred)
                        {
                            personEntity.PreferredName = chosenName.FullName;
                            personEntity.PreferredNameType = "CHOSEN";
                        }
                    }
                }
            }

            var personalNames =
            PersonNames.Where(pn => pn.NameType.Category != null && pn.NameType.Category != null && pn.NameType.Category == PersonNameType2.Personal).ToList();

            if (personalNames.Any())
            {
                // Person Nick Name
                // Nickname are a sub-group of the personal name type.  Determined by matching detail.id      
                var nickNameType = nameTypes.FirstOrDefault(x => x.Code == "NICKNAME");
                if (nickNameType == null)
                {
                    throw new RepositoryException("Unable to retrieve PersonNameType used to detemine nickname.");
                }

                var nickNames =
                        personalNames.Where(pn => pn.NameType != null && pn.NameType.Detail != null && !string.IsNullOrEmpty(pn.NameType.Detail.Id) && pn.NameType.Detail.Id.Equals(nickNameType.Guid, StringComparison.OrdinalIgnoreCase));

                if (nickNames.Any())
                {
                    if (nickNames.Count() > 1)
                    {
                        throw new ArgumentNullException("personDto",
                            "Colleague does not support more than one nickname for a person.");
                    }
                    var nickName = nickNames.FirstOrDefault();
                    if (nickName != null)
                    {
                        if (string.IsNullOrEmpty(nickName.FullName))
                        {
                            throw new ArgumentNullException("personDto", "Full Name is required for a nickname.");
                        }

                        // On PUT and POST, if NickName is identified as Preferred, than the First Name property of NickName will be stored into 
                        // Colleague’s Nickname field unless the First Name property of NickName is blank.  If that is blank, the fullName
                        // property of NickName will be stored in Colleague’s NickName 
                        if (nickName.Preference == PersonNamePreference.Preferred)
                        {
                            personEntity.PreferredName = nickName.FullName;
                            personEntity.Nickname = string.IsNullOrEmpty(nickName.FirstName)
                                ? nickName.FullName
                                : nickName.FirstName;
                            personEntity.PreferredNameType = "NICKNAME";
                        }
                        else
                        {
                            personEntity.Nickname = nickName.FullName;
                        }
                    }
                }

                // all other remaining personal names are recorded as historical names.
                var historyNameType = nameTypes.FirstOrDefault(x => x.Code == "HISTORY");
                if (historyNameType == null)
                {
                    throw new RepositoryException("Unable to retrieve PersonNameType used to detemine historyname.");
                }

                // From the subset of personal names, retrieve history/former names
                // For a PUT or POST where the names.type.category is "personal" with no corresponding detail guid, 
                // we will save the incoming name to the NAME.HISTORY.FIRST.NAME, NAME.HISTORY.LAST.NAME, and 
                // NAME.HISTORY.MIDDLE.NAME. 

                var historyNames =
                        personalNames.Where(
                            pn =>
                                (pn.NameType.Detail == null || pn.NameType.Detail.Id == null) ||
                                pn.NameType.Detail.Id == historyNameType.Guid).ToList();

                var formerNames = new List<PersonName>();
                foreach (var historyName in historyNames)
                {
                    if ((historyName.NameType.Detail != null) && (historyName.NameType.Detail.Id == null))
                    {
                        throw new ArgumentException("personDto", "If providing History NameType Detail, then Detail.id is required.");
                    }
                    if (string.IsNullOrEmpty(historyName.LastName))
                    {
                        throw new ArgumentNullException("personDto",
                                                "Last Name is required for a former name.");
                    }
                    if (historyName.Preference == Dtos.EnumProperties.PersonNamePreference.Preferred)
                    {
                        throw new ArgumentException("personDto",
                            "Can not assign Preferred to a name type of Former Name.");
                    }
                    if (string.IsNullOrEmpty(historyName.FullName))
                    {
                        throw new ArgumentNullException("personDto", "Full Name is required for a former name.");
                    }

                    //process last name prefix
                    if (!string.IsNullOrEmpty(historyName.LastNamePrefix) && !string.IsNullOrEmpty(historyName.LastName))
                    {
                        string hprefix = historyName.LastName.Substring(0, historyName.LastNamePrefix.Length);
                        if (!string.Equals(historyName.LastNamePrefix, hprefix, StringComparison.OrdinalIgnoreCase))
                            historyName.LastName = string.Concat(historyName.LastNamePrefix, " ", historyName.LastName);
                    }

                    var formerName = new PersonName(historyName.FirstName, historyName.MiddleName,
                        historyName.LastName);
                    formerNames.Add(formerName);

                }
                personEntity.FormerNames = formerNames;
            }
            return personEntity;
        }


        private async Task<PersonIntegration> ConvertPersonNames3(string personId, string guid, IEnumerable<Dtos.DtoProperties.PersonName2DtoProperty> PersonNames)
        {
            PersonIntegration personEntity = null;

            if (PersonNames == null)
            {
                IntegrationApiExceptionAddError("Must provide person name", "persons.names", guid, personId);
                return personEntity;
            }
            
            var nameCategories =
              PersonNames.Where(pn => pn.NameType == null || pn.NameType.Category == null).ToList();

            if (nameCategories.Any())
            {
                IntegrationApiExceptionAddError("Name type category is required.", "persons.names.category", guid, personId);
                return personEntity;
            }

            var preferredNames =
                PersonNames.Where(pn => pn.Preference == Dtos.EnumProperties.PersonNamePreference.Preferred)
                    .ToList();

            if (preferredNames.Any() && preferredNames.Count > 1)
            {
                IntegrationApiExceptionAddError("Only one name type can be identified as preferred.", "persons.names.preference", guid, personId);
            }
            var nameTypes = (await GetPersonNameTypesAsync(false)).ToList();
            //check to make sure the input name types are correct.            
            foreach (var name in PersonNames)
            {
                if (name.NameType.Detail != null)
                {
                    var nameCategoryGuid = nameTypes.Where(nm => nm.Guid == name.NameType.Detail.Id);
                    if (nameCategoryGuid == null || !nameCategoryGuid.Any())
                    {
                        IntegrationApiExceptionAddError(string.Concat("Name type category with detail Id of '", name.NameType.Detail.Id, "' is not valid."), "persons.names.type.detail.id", guid, personId);
                    }
                }                
            }

            // person legal name
            var primaryNames =
                PersonNames.Where(pn => pn.NameType.Category == PersonNameType2.Legal).ToList();
            if (primaryNames == null || !primaryNames.Any())
            {
                IntegrationApiExceptionAddError("A legal name is required in the names array.", "persons.names.type.category", guid, personId);
            }
            if (primaryNames != null && primaryNames.Any())
            {
                if (primaryNames.Count() > 1)
                {
                    IntegrationApiExceptionAddError("Colleague does not support more than one legal name for a person.", "persons.names.type.category", guid, personId);
                }
                var primaryName = primaryNames.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(primaryName.LastName))
                {
                    IntegrationApiExceptionAddError("Last name is required for a legal name.", "persons.names.lastName", guid, personId);
                }
                if (string.IsNullOrEmpty(primaryName.FullName))
                {
                    IntegrationApiExceptionAddError("Full name is required for a legal name.", "persons.names.fullName", guid, personId);
                }
                if ((primaryName.NameType.Detail != null) && (primaryName.NameType.Detail.Id == null))
                {
                    IntegrationApiExceptionAddError("If providing Legal NameType Detail, then Detail.id is required.", "persons.names.type.detail.id", guid, personId);
                }
                if ((primaryName.NameType != null) && (primaryName.NameType.Detail != null) && (primaryName.NameType.Detail.Id != null))
                {
                    var legalNameType = nameTypes.FirstOrDefault(x => x.Code == "LEGAL");
                    if (legalNameType == null)
                    {
                        IntegrationApiExceptionAddError("Unable to retrieve PersonNameType used to detemine legal name.", "persons.names.type", guid, personId);
                    }
                    else
                    {
                        if (!primaryName.NameType.Detail.Id.Equals(legalNameType.Guid, StringComparison.InvariantCulture))
                        {
                            IntegrationApiExceptionAddError("Detail.id provided for category legal not valid.", "persons.names.type.detail.id", guid, personId);
                        }
                    }
                }
                //process last name prefix
                if (!string.IsNullOrEmpty(primaryName.LastNamePrefix) && !string.IsNullOrEmpty(primaryName.LastName))
                {
                    try
                    {
                        string lprefix = primaryName.LastName.Substring(0, primaryName.LastNamePrefix.Length);
                        if (!string.Equals(primaryName.LastNamePrefix, lprefix, StringComparison.OrdinalIgnoreCase))
                            primaryName.LastName = string.Concat(primaryName.LastNamePrefix, " ", primaryName.LastName);
                    }
                    catch
                    {
                        // Ignore possible object reference errors
                    }
                }
                var professionalAbbreviations = new List<string>();
                if (primaryName.ProfessionalAbbreviation != null)
                {
                    professionalAbbreviations = primaryName.ProfessionalAbbreviation.ToList();
                }

                if (primaryName != null && !string.IsNullOrEmpty(primaryName.LastName))
                {
                    personEntity = new PersonIntegration(personId, primaryName.LastName)
                    {
                        Guid = guid,
                        Prefix = primaryName.Title,
                        FirstName = primaryName.FirstName,
                        MiddleName = primaryName.MiddleName,
                        Suffix = primaryName.Pedigree,
                        ProfessionalAbbreviations = professionalAbbreviations
                    };
                    if (primaryName.Preference == PersonNamePreference.Preferred)
                    {
                        personEntity.PreferredName = primaryName.FullName;
                        personEntity.PreferredNameType = "LEGAL";
                    }
                }
            }

            // person birth name
            var birthNames =
                PersonNames.Where(pn => pn.NameType.Category == PersonNameType2.Birth).ToList();

            if (birthNames.Any())
            {
                if (birthNames.Count() > 1)
                {
                    IntegrationApiExceptionAddError("Colleague does not support more than one birth name for a person.", "persons.names.type.category", guid, personId);
                }
                var birthName = birthNames.FirstOrDefault();

                if (birthName != null)
                {
                    if ((string.IsNullOrEmpty(birthName.FirstName)) &&
                        (string.IsNullOrEmpty(birthName.LastName)) &&
                        (string.IsNullOrEmpty(birthName.MiddleName)))
                    {
                        IntegrationApiExceptionAddError("Either the firstName, middleName, or lastName is needed for a birth name.", "persons.names.lastName", guid, personId);
                    }
                    if (string.IsNullOrEmpty(birthName.FullName))
                    {
                        IntegrationApiExceptionAddError("Full Name is needed for a birth name.", "persons.names.fullName", guid, personId);
                    }
                    if ((birthName.NameType.Detail != null) && (birthName.NameType.Detail.Id == null))
                    {
                        IntegrationApiExceptionAddError("If providing Birth NameType Detail, then Detail.id is required.", "persons.names.type.detail.id", guid, personId);
                    }
                    if ((birthName.NameType.Detail != null) && (birthName.NameType.Detail.Id != null))
                    {
                        var birthNameType = nameTypes.FirstOrDefault(x => x.Code == "BIRTH");
                        if (birthNameType == null)
                        {
                            IntegrationApiExceptionAddError("Unable to retrieve PersonNameType used to detemine birth name.", "persons.names.type.detail.id", guid, personId);
                        }
                        else
                        {
                            if (!birthName.NameType.Detail.Id.Equals(birthNameType.Guid, StringComparison.InvariantCulture))
                            {
                                IntegrationApiExceptionAddError("Detail.id provided for category birth not valid.", "persons.names.type.detail.id", guid, personId);
                            }
                        }
                    }

                    //process last name prefix
                    if (!string.IsNullOrEmpty(birthName.LastNamePrefix) && !string.IsNullOrEmpty(birthName.LastName))
                    {
                        try
                        {
                            string bprefix = birthName.LastName.Substring(0, birthName.LastNamePrefix.Length);
                            if (!string.Equals(birthName.LastNamePrefix, bprefix, StringComparison.OrdinalIgnoreCase))
                                birthName.LastName = string.Concat(birthName.LastNamePrefix, " ", birthName.LastName);
                        }
                        catch
                        {
                            // Ignore here.  Missing data was already reported but may cause object reference error.
                        }
                    }
                    if (personEntity != null)
                    {
                        personEntity.BirthNameLast = birthName.LastName;
                        personEntity.BirthNameFirst = birthName.FirstName;
                        personEntity.BirthNameMiddle = birthName.MiddleName;

                        if (birthName.Preference == PersonNamePreference.Preferred)
                        {
                            personEntity.PreferredName = birthName.FullName;
                            personEntity.PreferredNameType = "BIRTH";
                        }
                    }
                }
            }

            var favoredNames = PersonNames.Where(pn => pn.NameType != null && pn.NameType.Category != null && pn.NameType.Category == PersonNameType2.Favored).ToList();
            if (favoredNames.Any())
            {

                var chosenNameType = nameTypes.FirstOrDefault(x => x.Code == "CHOSEN");
                if (chosenNameType == null)
                {
                    IntegrationApiExceptionAddError("Unable to retrieve PersonNameType used to detemine chosen name.", "persons.names.type", guid, personId);
                }
                else
                {
                    var chosenNames = favoredNames.Where(pn => pn.NameType != null && pn.NameType.Detail != null && !string.IsNullOrEmpty(pn.NameType.Detail.Id) && pn.NameType.Detail.Id.Equals(chosenNameType.Guid, StringComparison.OrdinalIgnoreCase));
                    if (chosenNames == null || !chosenNames.Any())
                    {
                        chosenNames = favoredNames;
                    }
                    if (chosenNames.Any())
                    {
                        if (chosenNames.Count() > 1)
                        {
                            IntegrationApiExceptionAddError("Colleague does not support more than one chosen name for a person.", "persons.names.type", guid, personId);
                        }
                        var chosenName = chosenNames.FirstOrDefault();

                        if (chosenName != null)
                        {
                            if ((string.IsNullOrEmpty(chosenName.FirstName)) &&
                                (string.IsNullOrEmpty(chosenName.LastName)) &&
                                (string.IsNullOrEmpty(chosenName.MiddleName)))
                            {
                                IntegrationApiExceptionAddError("Either the firstName, middleName, or lastName is needed for a chosen name.", "persons.names.type", guid, personId);
                            }
                            if (string.IsNullOrEmpty(chosenName.FullName))
                            {
                                IntegrationApiExceptionAddError("Full Name is needed for a chosen name.", "persons.names.fullName", guid, personId);
                            }
                            if ((chosenName.NameType.Detail != null) && (chosenName.NameType.Detail.Id == null))
                            {
                                IntegrationApiExceptionAddError("If providing Chosen NameType Detail, then Detail.id is required.", "persons.names.type.detail.id", guid, personId);
                            }
                            if ((chosenName.NameType.Detail != null) && (chosenName.NameType.Detail.Id != null))
                            {
                                if (chosenNameType == null)
                                {
                                    IntegrationApiExceptionAddError("Unable to retrieve PersonNameType used to detemine chosen name.", "persons.names.type.detail.id", guid, personId);
                                }
                                if (!chosenName.NameType.Detail.Id.Equals(chosenNameType.Guid, StringComparison.InvariantCulture))
                                {
                                    IntegrationApiExceptionAddError("Detail.id provided for category personal not valid.", "persons.names.type.detail.id", guid, personId);
                                }
                            }

                            //process last name prefix
                            if (!string.IsNullOrEmpty(chosenName.LastNamePrefix) && !string.IsNullOrEmpty(chosenName.LastName))
                            {
                                try
                                {
                                    string cPrefix = chosenName.LastName.Substring(0, chosenName.LastNamePrefix.Length);
                                    if (!string.Equals(chosenName.LastNamePrefix, cPrefix, StringComparison.OrdinalIgnoreCase))
                                        chosenName.LastName = string.Concat(chosenName.LastNamePrefix, " ", chosenName.LastName);
                                }
                                catch
                                {
                                    // Ignore possible object reference errors
                                }
                            }
                            if (personEntity != null)
                            {
                                personEntity.ChosenLastName = chosenName.LastName;
                                personEntity.ChosenFirstName = chosenName.FirstName;
                                personEntity.ChosenMiddleName = chosenName.MiddleName;

                                if (chosenName.Preference == PersonNamePreference.Preferred)
                                {
                                    personEntity.PreferredName = chosenName.FullName;
                                    personEntity.PreferredNameType = "CHOSEN";
                                }
                            }
                        }
                    }
                }
            }

            var personalNames =
            PersonNames.Where(pn => pn.NameType.Category != null && pn.NameType.Category != null && pn.NameType.Category == PersonNameType2.Personal).ToList();

            if (personalNames.Any())
            {
                // Person Nick Name
                // Nickname are a sub-group of the personal name type.  Determined by matching detail.id      
                var nickNameType = nameTypes.FirstOrDefault(x => x.Code == "NICKNAME");
                if (nickNameType == null)
                {
                    IntegrationApiExceptionAddError("Unable to retrieve PersonNameType used to detemine nickname.", "persons.names.type.category", guid, personId);
                }
                else
                { 
                var nickNames =
                        personalNames.Where(pn => pn.NameType != null && pn.NameType.Detail != null && !string.IsNullOrEmpty(pn.NameType.Detail.Id) && pn.NameType.Detail.Id.Equals(nickNameType.Guid, StringComparison.OrdinalIgnoreCase));

                    if (nickNames.Any())
                    {
                        if (nickNames.Count() > 1)
                        {
                            IntegrationApiExceptionAddError("Colleague does not support more than one nickname for a person.", "persons.names.type.category", guid, personId);
                        }
                        var nickName = nickNames.FirstOrDefault();
                        if (nickName != null)
                        {
                            if (string.IsNullOrEmpty(nickName.FullName))
                            {
                                IntegrationApiExceptionAddError("Full Name is required for a nickname.", "persons.names.fullName", guid, personId);
                            }

                            // On PUT and POST, if NickName is identified as Preferred, than the First Name property of NickName will be stored into 
                            // Colleague’s Nickname field unless the First Name property of NickName is blank.  If that is blank, the fullName
                            // property of NickName will be stored in Colleague’s NickName 
                            if (personEntity != null)
                            {
                                if (nickName.Preference == PersonNamePreference.Preferred)
                                {
                                    personEntity.PreferredName = nickName.FullName;
                                    personEntity.Nickname = string.IsNullOrEmpty(nickName.FirstName)
                                        ? nickName.FullName
                                        : nickName.FirstName;
                                    personEntity.PreferredNameType = "NICKNAME";
                                }
                                else
                                {
                                    personEntity.Nickname = nickName.FullName;
                                }
                            }
                        }
                    }
                }

                // all other remaining personal names are recorded as historical names.
                var historyNameType = nameTypes.FirstOrDefault(x => x.Code == "HISTORY");
                if (historyNameType == null)
                {
                    IntegrationApiExceptionAddError("Unable to retrieve PersonNameType used to detemine historyname.", "persons.names.type.category", guid, personId);
                }
                else
                {

                    // From the subset of personal names, retrieve history/former names
                    // For a PUT or POST where the names.type.category is "personal" with no corresponding detail guid, 
                    // we will save the incoming name to the NAME.HISTORY.FIRST.NAME, NAME.HISTORY.LAST.NAME, and 
                    // NAME.HISTORY.MIDDLE.NAME. 

                    var historyNames =
                            personalNames.Where(
                                pn =>
                                    (pn.NameType.Detail == null || pn.NameType.Detail.Id == null) ||
                                    pn.NameType.Detail.Id == historyNameType.Guid).ToList();

                    var formerNames = new List<PersonName>();
                    foreach (var historyName in historyNames)
                    {
                        if ((historyName.NameType.Detail != null) && (historyName.NameType.Detail.Id == null))
                        {
                            IntegrationApiExceptionAddError("If providing History NameType Detail, then Detail.id is required.", "persons.names.type.detail.id", guid, personId);
                        }
                        if (string.IsNullOrEmpty(historyName.LastName))
                        {
                            IntegrationApiExceptionAddError("Last Name is required for a former name.", "persons.names.lastName", guid, personId);
                        }
                        if (historyName.Preference == Dtos.EnumProperties.PersonNamePreference.Preferred)
                        {
                            IntegrationApiExceptionAddError("Can not assign Preferred to a name type of Former Name.", "persons.names.preference", guid, personId);
                        }
                        if (string.IsNullOrEmpty(historyName.FullName))
                        {
                            IntegrationApiExceptionAddError("Full Name is required for a former name.", "persons.names.fullName", guid, personId);
                        }

                        //process last name prefix
                        if (!string.IsNullOrEmpty(historyName.LastNamePrefix) && !string.IsNullOrEmpty(historyName.LastName))
                        {
                            try
                            {
                                string hprefix = historyName.LastName.Substring(0, historyName.LastNamePrefix.Length);
                                if (!string.Equals(historyName.LastNamePrefix, hprefix, StringComparison.OrdinalIgnoreCase))
                                    historyName.LastName = string.Concat(historyName.LastNamePrefix, " ", historyName.LastName);
                            }
                            catch
                            {
                                // Ignore possible object reference errors
                            }
                        }

                        try
                        {
                            var formerName = new PersonName(historyName.FirstName, historyName.MiddleName,
                                historyName.LastName);
                            formerNames.Add(formerName);
                        }
                        catch
                        {
                            // Ignore errors from missing last name in constructor or object reference error.
                        }
                    }
                    if (personEntity != null)
                    {
                        personEntity.FormerNames = formerNames;
                    }
                }
            }
            return personEntity;
        }

        /// <summary>
        /// Converts privacy status
        /// </summary>
        /// <param name="personPrivacyDtoProperty"></param>
        /// <returns>PrivacyStatusType</returns>
        private async Task<Domain.Base.Entities.PrivacyStatus> ConvertPerson2PrivacyStatusAsync(Dtos.DtoProperties.PersonPrivacyDtoProperty personPrivacyDtoProperty)
        {
            if (personPrivacyDtoProperty.Detail != null && string.IsNullOrEmpty(personPrivacyDtoProperty.Detail.Id))
            {
                throw new ArgumentNullException("privacyStatus.detail", "Must provide an id for privacyStatus detail.");
            }

            try
            {
                var isDefined = Dtos.PrivacyStatusType.IsDefined(typeof(Dtos.PrivacyStatusType), personPrivacyDtoProperty.PrivacyCategory);
            }
            catch
            {
                throw new ArgumentNullException("privacyStatus.privacyCategory", "Must provide privacyCategory for privacyStatus.");
            }

            Domain.Base.Entities.PrivacyStatus privacyStatusEntity = null;
            if (personPrivacyDtoProperty.Detail != null && !string.IsNullOrEmpty(personPrivacyDtoProperty.Detail.Id))
            {
                var privacyStatusEntities = await GetPrivacyStatusesAsync(false);

                privacyStatusEntity = privacyStatusEntities.FirstOrDefault(st => st.Guid.Equals(personPrivacyDtoProperty.Detail.Id, StringComparison.OrdinalIgnoreCase));
                if (privacyStatusEntity == null)
                {
                    throw new KeyNotFoundException("Privacy status associated to guid '" + personPrivacyDtoProperty.Detail.Id + "' not found in repository.");
                }
                Domain.Base.Entities.PrivacyStatusType privacyStatus;
                try
                {
                    privacyStatus = (Domain.Base.Entities.PrivacyStatusType)Enum.Parse(
                        typeof(Domain.Base.Entities.PrivacyStatusType), personPrivacyDtoProperty.PrivacyCategory.ToString().ToLower());
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("Error occured parsing privacy status type: " + personPrivacyDtoProperty.PrivacyCategory.ToString());
                }
                if (!privacyStatusEntity.PrivacyStatusType.Equals(privacyStatus))
                {
                    throw new InvalidOperationException(string.Concat("Provided privacy status type ", personPrivacyDtoProperty.PrivacyCategory.ToString(),
                        " does not match the privacy status type by id ", personPrivacyDtoProperty.Detail.Id));
                }
            }
            else if (personPrivacyDtoProperty.Detail == null)
            {
                var privacyStatusEntities = await GetPrivacyStatusesAsync(false);

                try
                {
                    if (personPrivacyDtoProperty.PrivacyCategory != Dtos.PrivacyStatusType.Unrestricted)
                    {
                        Domain.Base.Entities.PrivacyStatusType privacyStatus = (Domain.Base.Entities.PrivacyStatusType)Enum.Parse(typeof(Domain.Base.Entities.PrivacyStatusType), personPrivacyDtoProperty.PrivacyCategory.ToString().ToLower());

                        privacyStatusEntity = privacyStatusEntities.FirstOrDefault(st => st.PrivacyStatusType.Equals(privacyStatus));
                        if (privacyStatusEntity == null)
                        {
                            throw new KeyNotFoundException("Privacy status associated with enum '" + privacyStatus.ToString() + "' not found in repository.");
                        }
                    }
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Privacy category not found with category type: " + personPrivacyDtoProperty.PrivacyCategory.ToString());
                }
            }
            return privacyStatusEntity;
        }

        /// <summary>
        /// Converts privacy status
        /// </summary>
        /// <param name="personPrivacyDtoProperty"></param>
        /// <returns>PrivacyStatusType</returns>
        private async Task<Domain.Base.Entities.PrivacyStatus> ConvertPerson3PrivacyStatusAsync(Dtos.DtoProperties.PersonPrivacyDtoProperty personPrivacyDtoProperty, string guid, string personId)
        {
            if (personPrivacyDtoProperty.Detail != null && string.IsNullOrEmpty(personPrivacyDtoProperty.Detail.Id))
            {
                IntegrationApiExceptionAddError("Must provide an id for privacyStatus detail.", "persons.privacyStatus.detail.id", guid, personId);
            }

            try
            {
                var isDefined = Dtos.PrivacyStatusType.IsDefined(typeof(Dtos.PrivacyStatusType), personPrivacyDtoProperty.PrivacyCategory);
            }
            catch
            {
                IntegrationApiExceptionAddError("Must provide privacyCategory for privacyStatus.", "persons.privacyStatus.privacyCategory", guid, personId);
            }

            Domain.Base.Entities.PrivacyStatus privacyStatusEntity = null;
            if (personPrivacyDtoProperty.Detail != null && !string.IsNullOrEmpty(personPrivacyDtoProperty.Detail.Id))
            {
                var privacyStatusEntities = await GetPrivacyStatusesAsync(false);

                privacyStatusEntity = privacyStatusEntities.FirstOrDefault(st => st.Guid.Equals(personPrivacyDtoProperty.Detail.Id, StringComparison.OrdinalIgnoreCase));
                if (privacyStatusEntity == null)
                {
                    IntegrationApiExceptionAddError("Privacy status associated to guid '" + personPrivacyDtoProperty.Detail.Id + "' not found in repository.", "persons.privacyStatus.detail.id", guid, personId);
                }
                else
                {
                    Domain.Base.Entities.PrivacyStatusType privacyStatus = PrivacyStatusType.unrestricted;
                    try
                    {
                        privacyStatus = (Domain.Base.Entities.PrivacyStatusType)Enum.Parse(
                            typeof(Domain.Base.Entities.PrivacyStatusType), personPrivacyDtoProperty.PrivacyCategory.ToString().ToLower());
                    }
                    catch (Exception)
                    {
                        IntegrationApiExceptionAddError("Error occured parsing privacy status type: " + personPrivacyDtoProperty.PrivacyCategory.ToString(), "persons.privacyStatus.privacyCategory", guid, personId);
                    }
                    if (!privacyStatusEntity.PrivacyStatusType.Equals(privacyStatus))
                    {
                        IntegrationApiExceptionAddError(string.Concat("Provided privacy status type ", personPrivacyDtoProperty.PrivacyCategory.ToString(),
                            " does not match the privacy status type by id ", personPrivacyDtoProperty.Detail.Id), "persons.privacyStatus.detail.id", guid, personId);
                    }
                }
            }
            else if (personPrivacyDtoProperty.Detail == null)
            {
                var privacyStatusEntities = await GetPrivacyStatusesAsync(false);

                try
                {
                    if (personPrivacyDtoProperty.PrivacyCategory != Dtos.PrivacyStatusType.Unrestricted)
                    {
                        Domain.Base.Entities.PrivacyStatusType privacyStatus = (Domain.Base.Entities.PrivacyStatusType)Enum.Parse(typeof(Domain.Base.Entities.PrivacyStatusType), personPrivacyDtoProperty.PrivacyCategory.ToString().ToLower());

                        privacyStatusEntity = privacyStatusEntities.FirstOrDefault(st => st.PrivacyStatusType.Equals(privacyStatus));
                        if (privacyStatusEntity == null)
                        {
                            IntegrationApiExceptionAddError("Privacy status associated with enum '" + privacyStatus.ToString() + "' not found in repository.", "persons.privacyStatus.privacyCategory", guid, personId);
                        }
                    }
                }
                catch (ArgumentException)
                {
                    IntegrationApiExceptionAddError("Privacy category not found with category type: " + personPrivacyDtoProperty.PrivacyCategory.ToString(), "persons.privacyStatus.privacyCategory", guid, personId);
                }
            }
            return privacyStatusEntity;
        }

        /// <summary>
        /// Convert Race code
        /// </summary>
        /// <param name="racesDtoProperties"></param>
        /// <returns>List<string></returns>
        private async Task<List<string>> ConvertPerson2DtoRaceCodesToEntityAsync(IEnumerable<Dtos.DtoProperties.PersonRaceDtoProperty> racesDtoProperties)
        {
            List<string> raceCodes = null;
            var raceEntities = await GetRacesAsync(false);

            foreach (var raceDtoProperty in racesDtoProperties)
            {
                Domain.Base.Entities.Race raceEntity = null;

                //Validate race id is provided
                if (raceDtoProperty.Race != null && string.IsNullOrEmpty(raceDtoProperty.Race.Id))
                {
                    throw new ArgumentNullException("races.race.id", "Must provide an id for race detail.");
                }

                //Validate that reporting race country racial category, per discussion with Kelly only one should be in payload
                if (raceDtoProperty.Reporting != null && raceDtoProperty.Reporting.Count() > 1)
                {
                    throw new InvalidOperationException("More than one entry in the reporting array for a single race is not supported.");
                }

                //Validate if reporting country is not null then racial category is required
                if (raceDtoProperty.Reporting != null && raceDtoProperty.Reporting.Any())
                {
                    var reporting = raceDtoProperty.Reporting.FirstOrDefault();
                    // Only support racial category reporting country of USA
                    if (reporting.Country != null && reporting.Country.Code != CountryCodeType.USA)
                    {
                        throw new ArgumentOutOfRangeException("Only USA is supported for race reporting in Colleague.");
                    }
                    if (reporting.Country != null && reporting.Country.Code == Dtos.CountryCodeType.USA && reporting.Country.RacialCategory == null)
                    {
                        throw new ArgumentNullException("If the country.code is 'USA' then the races.reporting.country.racialCategory property is required.");
                    }
                }

                //Validate that entity RacialCategoryType by id is same as RacialCategory
                if (raceDtoProperty.Race != null &&
                    !string.IsNullOrEmpty(raceDtoProperty.Race.Id) &&
                    raceDtoProperty.Reporting != null &&
                    raceDtoProperty.Reporting.Any())
                {
                    var reporting = raceDtoProperty.Reporting.FirstOrDefault();
                    if (reporting.Country != null && reporting.Country.RacialCategory != null)
                    {
                        raceEntity = raceEntities.FirstOrDefault(r => r.Guid.Equals(raceDtoProperty.Race.Id, StringComparison.OrdinalIgnoreCase));
                        if (raceEntity == null)
                        {
                            throw new KeyNotFoundException("Race ID associated with guid '" + raceDtoProperty.Race.Id + "' was not found.");
                        }

                        try
                        {
                            RaceType? entityRaceType = ConvertDtoRaceCategoryToEntityType(reporting.Country.RacialCategory);
                            if (!raceEntity.Type.Equals(entityRaceType))
                            {
                                throw new InvalidOperationException(string.Concat("Provided racial category of ", raceEntity.Type.ToString(),
                                    " does not match the race specified by id ", raceDtoProperty.Race.Id));
                            }
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException("Racial category of " + reporting.Country.RacialCategory.Value.ToString() + " is not valid.");
                        }
                    }
                }


                if (raceDtoProperty.Race != null && !string.IsNullOrEmpty(raceDtoProperty.Race.Id))
                {
                    raceEntity = raceEntities.FirstOrDefault(r => r.Guid.Equals(raceDtoProperty.Race.Id, StringComparison.OrdinalIgnoreCase));
                    if (raceEntity == null)
                    {
                        throw new KeyNotFoundException("Race ID associated with guid '" + raceDtoProperty.Race.Id + "' was not found.");
                    }

                    if (raceCodes == null) raceCodes = new List<string>();
                    raceCodes.Add(raceEntity.Code);
                }
                else if (raceDtoProperty.Race == null && raceDtoProperty.Reporting != null && raceDtoProperty.Reporting.Any())
                {
                    /*
                      For a POST or PUT, if no races.race.id is specified in the payload, then use the enumeration here to find the corresponding code from the PERSON.RACES valcode table 
                      (retrieve the first one w/ the 1st special processing code corresponding to the HEDM enumeration) and store the resulting code or codes to PER.RACES.  
                      Be careful to maintain the same order of races as what was submitted in the payload. 
                    */
                    var reporting = raceDtoProperty.Reporting.FirstOrDefault();
                    if (reporting != null && reporting.Country != null && reporting.Country.RacialCategory != null)
                    {
                        if (raceCodes == null) raceCodes = new List<string>();

                        if (reporting.Country.RacialCategory != null)
                        {
                            try
                            {
                                Domain.Base.Entities.RaceType? entityRaceType = ConvertDtoRaceCategoryToEntityType(reporting.Country.RacialCategory);
                                if (entityRaceType == null)
                                {
                                    throw new ArgumentException("Racial category of " + reporting.Country.RacialCategory.Value.ToString() + " is not valid.");
                                }
                                raceEntity = raceEntities.FirstOrDefault(r => r.Type.Equals(entityRaceType));
                                raceCodes.Add(raceEntity.Code);
                            }
                            catch (ArgumentException)
                            {
                                throw new ArgumentException("Could not find race type with reporting.country.racialCategory : " + reporting.Country.RacialCategory.Value.ToString());
                            }
                        }
                    }
                }
            }
            return raceCodes;
        }

        private RaceType? ConvertDtoRaceCategoryToEntityType(Dtos.EnumProperties.PersonRaceCategory? personRaceCategory)
        {
            switch (personRaceCategory)
            {
                case Dtos.EnumProperties.PersonRaceCategory.AmericanIndianOrAlaskaNative:
                    return RaceType.AmericanIndian;
                case Dtos.EnumProperties.PersonRaceCategory.Asian:
                    return RaceType.Asian;
                case Dtos.EnumProperties.PersonRaceCategory.BlackOrAfricanAmerican:
                    return RaceType.Black;
                case Dtos.EnumProperties.PersonRaceCategory.HawaiianOrPacificIslander:
                    return RaceType.PacificIslander;
                case Dtos.EnumProperties.PersonRaceCategory.White:
                    return RaceType.White;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts language
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="personGuid"></param>
        /// <param name="languagesDtoProperty"></param>
        /// <returns>List<PersonLanguage></returns>
        private List<PersonLanguage> ConvertPerson2DtoLanguagesToEntity(string personId, string personGuid, IEnumerable<Dtos.DtoProperties.PersonLanguageDtoProperty> languagesDtoProperty)
        {
            List<PersonLanguage> languagesEntities = new List<PersonLanguage>();

            if (languagesDtoProperty.Count(i => i.Preference == Dtos.EnumProperties.PersonLanguagePreference.Primary) > 1)
            {
                throw new InvalidOperationException("The person may not have more than one language with a preference of 'primary'.");
            }

            foreach (var languageDtoProperty in languagesDtoProperty)
            {
                if (languageDtoProperty.Code == PersonLanguageCode.NotSet)
                {
                    throw new ArgumentNullException("languageDtoProperty.code", "Must provide code a valid ISO 639-3 for language.");
                }

                PersonLanguage personLanguageEntity = new PersonLanguage(string.IsNullOrEmpty(personId) ? "$NEW.PERSON" : personId, languageDtoProperty.Code.ToString());
                personLanguageEntity.PersonGuid = personGuid;
                if (languageDtoProperty.Preference != null)
                {
                    personLanguageEntity.Preference = (LanguagePreference)Enum.Parse(typeof(LanguagePreference), languageDtoProperty.Preference.ToString());
                }
                languagesEntities.Add(personLanguageEntity);
            }
            return languagesEntities;
        }

        /// <summary>
        /// Converts Marital Status in personEntity
        /// </summary>        
        /// <param name="personEntity"><</param>
        /// <param name="personMaritalStatusDtoProperty"></param>
        /// <returns>Domain.Base.Entities.PersonIntegration</returns>
        private async Task<Domain.Base.Entities.PersonIntegration> ConvertPerson2MaritalStatusDtoToEntityAsync(PersonIntegration personEntity, Dtos.DtoProperties.PersonMaritalStatusDtoProperty personMaritalStatusDtoProperty)
        {
            try
            {
                var isDefined = Enum.IsDefined(typeof(Dtos.EnumProperties.PersonMaritalStatusCategory), personMaritalStatusDtoProperty.MaritalCategory);
            }
            catch
            {
                throw new ArgumentNullException("maritalStatus.maritalCategory", "Must provide a valid category for marital status.");
            }

            if (personMaritalStatusDtoProperty.Detail != null && string.IsNullOrEmpty(personMaritalStatusDtoProperty.Detail.Id))
            {
                throw new ArgumentNullException("maritalStatus.detail.id", "Must provide an id for marital status detail.");
            }

            Domain.Base.Entities.MaritalState? maritalState = null;
            var maritalStatusEntities = await GetMaritalStatusesAsync();

            if (personMaritalStatusDtoProperty.Detail != null && !string.IsNullOrEmpty(personMaritalStatusDtoProperty.Detail.Id))
            {
                var maritalStatusEntityById = maritalStatusEntities.FirstOrDefault(
                            m => m.Guid == personMaritalStatusDtoProperty.Detail.Id);
                if (maritalStatusEntityById == null)
                {
                    throw new KeyNotFoundException("Could not find marital status with id: " + personMaritalStatusDtoProperty.Detail.Id);
                }

                try
                {
                    Domain.Base.Entities.MaritalStatusType maritalStatusCategory = (Domain.Base.Entities.MaritalStatusType)Enum.Parse(typeof(Domain.Base.Entities.MaritalStatusType),
                                                                                personMaritalStatusDtoProperty.MaritalCategory.ToString());

                    if (!maritalStatusEntityById.Type.Equals(maritalStatusCategory))
                    {
                        //
                        // Allow for the possibility of a dto marital status GUID with a null type when dto marital category is "single"
                        //  because Ethos integration intentionally treats a marital statuses with no special processing as single.
                        //
                        if (!(maritalStatusEntityById.Type == null && maritalStatusCategory == Domain.Base.Entities.MaritalStatusType.Single))
                        {
                            throw new InvalidOperationException("maritalStatus.maritalCategory does not match the entity marital category by id: " + personMaritalStatusDtoProperty.Detail.Id);
                        }
                        else
                        {
                            // Save the marital status code of the GUID from the DTO that is unknown so that is written and/or preserved on the person
                            // despite the "single" category.  
                            personEntity.MaritalStatusCode = maritalStatusEntityById.Code;
                            return personEntity;
                        }
                    }

                    maritalState = (Domain.Base.Entities.MaritalState?)maritalStatusEntityById.Type;
                    personEntity.MaritalStatus = maritalState;
                    return personEntity;
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Could not find marital status type with marital category: " + personMaritalStatusDtoProperty.MaritalCategory.ToString());
                }
            }
            else if (personMaritalStatusDtoProperty.Detail == null)
            {
                try
                {
                    Domain.Base.Entities.MaritalStatusType maritalStatusCategory = (Domain.Base.Entities.MaritalStatusType)Enum.Parse(typeof(Domain.Base.Entities.MaritalStatusType),
                                                                personMaritalStatusDtoProperty.MaritalCategory.ToString());

                    var maritalStatusEntity = maritalStatusEntities.FirstOrDefault(m => m.Type.Equals(maritalStatusCategory));
                    if (maritalStatusEntity == null)
                    {
                        throw new KeyNotFoundException("Could not find marital status with marital category: " + personMaritalStatusDtoProperty.MaritalCategory.ToString());
                    }
                    maritalState = (Domain.Base.Entities.MaritalState)Enum.Parse(typeof(Domain.Base.Entities.MaritalState), maritalStatusEntity.Type.ToString());
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Could not find marital status type with marital category: " + personMaritalStatusDtoProperty.MaritalCategory.ToString());
                }
            }
            personEntity.MaritalStatus = maritalState;
            return personEntity;
        }

        /// <summary>
        /// Converts citizenship status
        /// </summary>
        /// <param name="personCitizenshipDtoProperty"></param>
        /// <returns>string</returns>
        private async Task<string> ConvertPerson2CitizenshipStatusDtoToEntityAsync(Dtos.DtoProperties.PersonCitizenshipDtoProperty personCitizenshipDtoProperty)
        {
            string citizenshipStatus = string.Empty;
            try
            {
                var isDefined = Enum.IsDefined(typeof(Dtos.CitizenshipStatusType), personCitizenshipDtoProperty.Category);
            }
            catch
            {
                throw new ArgumentNullException("citizenshipStatus.category", "Must provide a valid category for citizenshipStatus");
            }

            //check the required id field
            if (personCitizenshipDtoProperty != null && personCitizenshipDtoProperty.Detail != null && string.IsNullOrEmpty(personCitizenshipDtoProperty.Detail.Id))
            {
                throw new ArgumentNullException("citizenshipStatus.detail.id", "Citizenship detail id is required.");
            }

            var citizenshipStatusesEntities = await GetCitizenshipStatusesAsync(false);
            Domain.Base.Entities.CitizenshipStatus statusEntityByid = null;

            //validate that provided category matched the one in the entitybyid
            if (personCitizenshipDtoProperty != null &&
                personCitizenshipDtoProperty.Detail != null &&
                !string.IsNullOrEmpty(personCitizenshipDtoProperty.Detail.Id))
            {
                try
                {
                    var statusEntityByCategory = (CitizenshipStatusType)Enum.Parse(typeof(CitizenshipStatusType), personCitizenshipDtoProperty.Category.ToString());
                    statusEntityByid = citizenshipStatusesEntities.FirstOrDefault(i => i.Guid.Equals(personCitizenshipDtoProperty.Detail.Id, StringComparison.OrdinalIgnoreCase));

                    if (statusEntityByid == null)
                    {
                        throw new KeyNotFoundException("Citizenship status ID associated with guid '" + personCitizenshipDtoProperty.Detail.Id + "' not found in repository.");
                    }

                    if (statusEntityByid != null & !statusEntityByCategory.Equals(statusEntityByid.CitizenshipStatusType))
                    {
                        throw new InvalidOperationException("Citizenship category does not match with the category type by id.");
                    }
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Citizenship status type entity not found with category: " + personCitizenshipDtoProperty.Category.ToString());
                }
            }

            if (personCitizenshipDtoProperty != null && personCitizenshipDtoProperty.Detail != null && !string.IsNullOrEmpty(personCitizenshipDtoProperty.Detail.Id))
            {
                if (statusEntityByid == null)
                {
                    statusEntityByid = citizenshipStatusesEntities
                        .FirstOrDefault(i => i.Guid.Equals(personCitizenshipDtoProperty.Detail.Id, StringComparison.OrdinalIgnoreCase));
                }

                if (statusEntityByid == null)
                {
                    throw new KeyNotFoundException("Citizenship status ID associated with guid '" + personCitizenshipDtoProperty.Detail.Id + "' not found in repository.");
                }

                if (statusEntityByid != null)
                {
                    citizenshipStatus = statusEntityByid.Code;
                }
            }
            else if (personCitizenshipDtoProperty != null)
            {
                try
                {
                    var statusCategory = (CitizenshipStatusType)Enum.Parse(typeof(CitizenshipStatusType), personCitizenshipDtoProperty.Category.ToString());
                    var statusEntityByCategory = citizenshipStatusesEntities
                        .FirstOrDefault(i => i.CitizenshipStatusType.Equals(statusCategory));
                    if (statusEntityByCategory != null)
                    {
                        citizenshipStatus = statusEntityByCategory.Code;
                    }
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Citizenship status type entity not found with category: " + personCitizenshipDtoProperty.Category.ToString());
                }
            }

            return citizenshipStatus;
        }

        /// <summary>
        /// Converts visa
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="personVisaDtoProperty"></param>
        /// <returns>PersonVisa</returns>
        private async Task<PersonVisa> ConvertPerson2VisaToEntityAsync(string personId, Dtos.DtoProperties.PersonVisaDtoProperty personVisaDtoProperty)
        {
            PersonVisa personVisa = null;
            try
            {
                var isDefined = Enum.IsDefined(typeof(Dtos.VisaTypeCategory), personVisaDtoProperty.Category);
            }
            catch
            {
                throw new InvalidOperationException("Visa type category is required.");
            }

            if (personVisaDtoProperty.StartOn != null && personVisaDtoProperty.EndOn != null & personVisaDtoProperty.StartOn > personVisaDtoProperty.EndOn)
            {
                throw new InvalidOperationException("Start on date cannot be after the end on date.");
            }

            if (personVisaDtoProperty.Detail != null && string.IsNullOrEmpty(personVisaDtoProperty.Detail.Id))
            {
                throw new ArgumentNullException("visaStatus.detail.id", "Must provide id for visa status detail.");
            }
            if (personVisaDtoProperty.Detail != null && !string.IsNullOrEmpty(personVisaDtoProperty.Detail.Id))
            {
                var personVisaTypes = await GetVisaTypesAsync(false);
                var personVisaType = personVisaTypes.FirstOrDefault(vt => vt.Guid.Equals(personVisaDtoProperty.Detail.Id, StringComparison.OrdinalIgnoreCase));

                if (personVisaType == null)
                {
                    throw new KeyNotFoundException("Person visa type ID associated with guid '" + personVisaDtoProperty.Detail.Id + "' not found in repository.");
                }

                try
                {
                    VisaTypeCategory visaTypeCategory =
                        (VisaTypeCategory)
                            Enum.Parse(typeof(VisaTypeCategory), personVisaDtoProperty.Category.ToString());

                    if (!personVisaType.VisaTypeCategory.Equals(visaTypeCategory))
                    {
                        throw new InvalidOperationException(
                            "Visa type category does not match with the category type by id.");
                    }

                    personVisa = new PersonVisa(string.IsNullOrEmpty(personId) ? "$NEW.PERSON" : personId,
                        personVisaDtoProperty.Category.ToString());
                    personVisa.ExpireDate = (personVisaDtoProperty.EndOn == null)
                        ? default(DateTime?)
                        : personVisaDtoProperty.EndOn.Value.DateTime;
                    personVisa.Guid = personVisaDtoProperty.Detail.Id;
                    personVisa.IssueDate = (personVisaDtoProperty.StartOn == null)
                        ? default(DateTime?)
                        : personVisaDtoProperty.StartOn.Value.DateTime;
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Person visa type entity not found with category: " +
                                                personVisaDtoProperty.Category.ToString());
                }
            }
            else
            {
                try
                {
                    VisaTypeCategory visaTypeCategory = (VisaTypeCategory)Enum.Parse(typeof(VisaTypeCategory), personVisaDtoProperty.Category.ToString());

                    var personVisaTypes = await GetVisaTypesAsync(false);
                    var personVisaType = personVisaTypes.FirstOrDefault(vt => vt.VisaTypeCategory.Equals(visaTypeCategory));

                    if (personVisaType == null)
                    {
                        throw new KeyNotFoundException("Person visa type entity associated with category '" + personVisaDtoProperty.Category.ToString() + "' not found in repository.");
                    }

                    personVisa = new PersonVisa(personId, personVisaDtoProperty.Category.ToString());
                    personVisa.ExpireDate = (personVisaDtoProperty.EndOn == null) ? default(DateTime?) : personVisaDtoProperty.EndOn.Value.DateTime;
                    personVisa.Guid = personVisaType.Guid;
                    personVisa.IssueDate = (personVisaDtoProperty.StartOn == null) ? default(DateTime?) : personVisaDtoProperty.StartOn.Value.DateTime;
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Person visa type entity not found with category: " + personVisaDtoProperty.Category.ToString());
                }
            }
            return personVisa;
        }

        /// <summary>
        /// Converts roles
        /// </summary>
        /// <param name="personDtoRoles"></param>
        /// <returns>List<PersonRole></returns>
        private List<PersonRole> ConvertPerson2DtoRolesToEntity(IEnumerable<Dtos.DtoProperties.PersonRoleDtoProperty> personDtoRoles)
        {
            List<PersonRole> personRolesEntities = new List<PersonRole>();

            if (personDtoRoles != null && personDtoRoles.Any())
            {
                foreach (var roleDto in personDtoRoles)
                {
                    try
                    {
                        if (roleDto.StartOn != null && roleDto.EndOn != null && roleDto.StartOn > roleDto.EndOn)
                        {
                            throw new InvalidOperationException("Start on date cannot be after the end on date.");
                        }
                        if (!string.IsNullOrEmpty(roleDto.RoleType.ToString()))
                        {
                            PersonRoleType roleType = (PersonRoleType)Enum.Parse(typeof(PersonRoleType), roleDto.RoleType.ToString());
                            PersonRole role =
                                new PersonRole(
                                                roleType,
                                                (roleDto.StartOn == null) ? default(DateTime?) : roleDto.StartOn.Value.DateTime,
                                                (roleDto.EndOn == null) ? default(DateTime?) : roleDto.EndOn.Value.DateTime
                                              );
                            personRolesEntities.Add(role);
                        }
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException("Role type entity not found with role type: " + roleDto.RoleType.ToString());
                    }
                }
            }

            return personRolesEntities;
        }

        /// <summary>
        /// Converts roles
        /// </summary>
        /// <param name="organizationDtoRoles"></param>
        /// <returns>List<PersonRole></returns>
        private List<PersonRole> ConvertOrganization2DtoRolesToEntity(IEnumerable<Dtos.DtoProperties.OrganizationRoleDtoProperty> organizationDtoRoles)
        {
            List<PersonRole> personRolesEntities = new List<PersonRole>();

            if (organizationDtoRoles != null && organizationDtoRoles.Any())
            {
                foreach (var roleDto in organizationDtoRoles)
                {
                    try
                    {
                        if (roleDto.StartOn != null && roleDto.EndOn != null && roleDto.StartOn > roleDto.EndOn)
                        {
                            throw new InvalidOperationException("Start on date cannot be after the end on date.");
                        }

                        PersonRoleType roleType = (PersonRoleType)Enum.Parse(typeof(PersonRoleType), roleDto.Type.ToString());
                        PersonRole role =
                            new PersonRole(
                                            roleType,
                                            (roleDto.StartOn == null) ? default(DateTime?) : roleDto.StartOn.Value.DateTime,
                                            (roleDto.EndOn == null) ? default(DateTime?) : roleDto.EndOn.Value.DateTime
                                          );
                        personRolesEntities.Add(role);
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException("Role type entity not found with role type: " + roleDto.Type.ToString());
                    }
                }
            }

            return personRolesEntities;
        }

        /// <summary>
        /// Converts interests
        /// </summary>
        /// <param name="interests"></param>
        /// <returns>List<string></returns>
        private async Task<List<string>> ConvertPerson2DtoInterestsToEntityAsync(IEnumerable<Dtos.GuidObject2> interests)
        {
            List<string> interestList = null;
            if (interests.Any())
            {
                var interestEntities = await GetInterestsAsync(false);
                foreach (var interest in interests)
                {
                    if (string.IsNullOrEmpty(interest.Id))
                    {
                        throw new ArgumentNullException("interest.id", "Must provide an id for interest.");
                    }

                    var interestEntity = interestEntities.FirstOrDefault(i => i.Guid == interest.Id);
                    if (interestEntity == null)
                    {
                        throw new KeyNotFoundException("Interest entity not found with id: " + interest.Id);
                    }
                    if (interestList == null) interestList = new List<string>();

                    if (!interestList.Contains(interestEntity.Code))
                        interestList.Add(interestEntity.Code);
                }
            }
            return interestList;
        }

        /// <summary>
        /// Converts social media data
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="personDto"></param>
        /// <param name="personEntity"></param>
        private async Task<List<Domain.Base.Entities.SocialMedia>> ConvertPerson2DtoSocialMediaToEntity(IEnumerable<PersonSocialMediaDtoProperty> socialMediaDtoList)
        {
            var socialMediaEntities = new List<Domain.Base.Entities.SocialMedia>();
            if (socialMediaDtoList != null)
            {
                var socialMediaTypeEntities = await GetSocialMediaTypesAsync(false);
                foreach (var socialMediaDto in socialMediaDtoList)
                {
                    if (socialMediaDto.Type == null)
                        throw new ArgumentNullException("personDto", "Social media type is required to create a new social media");
                    if (string.IsNullOrEmpty(socialMediaDto.Address))
                        throw new ArgumentNullException("personDto", "Social media handle is required to create a new social media");

                    if (socialMediaDto.Type.Category != Dtos.SocialMediaTypeCategory.website)
                    {
                        string socialMediaType = "";
                        try
                        {
                            if (socialMediaDto.Type.Detail != null && !string.IsNullOrEmpty(socialMediaDto.Type.Detail.Id))
                            {
                                socialMediaType = socialMediaTypeEntities.FirstOrDefault(et => et.Guid == socialMediaDto.Type.Detail.Id).Code;
                            }
                            else
                            {
                                socialMediaType = socialMediaTypeEntities.FirstOrDefault(et => string.Equals(et.Type.ToString(), socialMediaDto.Type.Category.ToString(), StringComparison.OrdinalIgnoreCase)).Code;
                            }

                            bool socialMediaPreference = false;
                            if (socialMediaDto.Preference == Dtos.EnumProperties.PersonPreference.Primary) socialMediaPreference = true;
                            var socialMediaEntity = new Domain.Base.Entities.SocialMedia(socialMediaType, socialMediaDto.Address, socialMediaPreference);

                            socialMediaEntities.Add(socialMediaEntity);
                        }
                        catch
                        {
                            throw new ArgumentOutOfRangeException("socialMediaDto.Type", string.Format("Could not find the social media type for handle '{0}'. ", socialMediaDto.Address));
                        }
                    }
                    else
                    {
                        var socialMediaEntity = new Domain.Base.Entities.SocialMedia("website", socialMediaDto.Address, false);
                        socialMediaEntities.Add(socialMediaEntity);
                    }
                }
            }
            return socialMediaEntities;
        }

        /// <summary>
        /// Converts credentials
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="credentials"></param>
        /// <param name="personEntity"></param>
        private static void ConvertPerson2DtoCredsToEntity(string personId, IEnumerable<CredentialDtoProperty> credentials, PersonIntegration personEntity)
        {
            //credential type is required. 
            if (credentials != null && credentials.Count(c => string.IsNullOrEmpty(c.Type.ToString())) > 0)
            {
                throw new InvalidOperationException("Credentials Type is required. ");
            }
            //Validate to make sure not more than one of each type is passed in the payload
            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType.ColleaguePersonId) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one Colleague id.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType.Ssn) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one SSN.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType.Sin) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one SIN.");
            }
            
            //Validate that only Ssn or Sin is included and not both
            Dtos.EnumProperties.CredentialType?[] creds = new Dtos.EnumProperties.CredentialType?[] { Dtos.EnumProperties.CredentialType.Sin, Dtos.EnumProperties.CredentialType.Ssn };
            var ssnSin = credentials.Where(c => creds.Contains(c.Type));
            if (ssnSin != null && ssnSin.Count() > 1)
            {
                throw new InvalidOperationException("You cannot include SSN and SIN at the same time.");
            }

            //Validate the colleagueId type against personId
            var colleagueIdCred = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType.ColleaguePersonId);
            if (colleagueIdCred != null)
            {
                if (!string.IsNullOrEmpty(personId) && !personId.Equals(colleagueIdCred.Value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Provided Colleague credentials id does not match Colleague person id.");
                }
                else
                {
                    if (string.IsNullOrEmpty(personId) && !string.IsNullOrEmpty(colleagueIdCred.Value))
                    {
                        throw new InvalidOperationException("Assignment of Colleague credentials id is not allowed on a new person.");
                    }
                }
            }

            //Cannot include any other type of credentials such as bannerId, bannerSourceId, bannerUserName, bannerUdcId
            if (credentials != null && credentials.Any(crd => crd.Type.Equals(Dtos.EnumProperties.CredentialType.BannerId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType.BannerSourcedId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType.BannerUdcId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType.BannerUserName)))
            {
                throw new InvalidOperationException("Credential type bannerId, bannerSourceId, bannerUserName, bannerUdcId are not supported in colleague.");
            }

            if (credentials != null)
            {
                //SSN
                var ssn = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType.Ssn);
                if (ssn != null)
                {
                    if (string.IsNullOrEmpty(ssn.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for SSN if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(ssn.Value, "SSN");
                    personEntity.PersonAltIds.Add(personAlt);
                }

                //Sin
                var sin = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType.Sin);
                if (sin != null)
                {
                    if (string.IsNullOrEmpty(sin.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for SIN if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(sin.Value, "SIN");
                    personEntity.PersonAltIds.Add(personAlt);
                }
                
                //Elevate Id
                var elevateIdList = credentials.Where(c => c.Type == Dtos.EnumProperties.CredentialType.ElevateID);
                if (elevateIdList != null && elevateIdList.Any())
                {
                    foreach (var elevateId in elevateIdList)
                    {
                        if (elevateId != null)
                        {
                            if (string.IsNullOrEmpty(elevateId.Value))
                            {
                                throw new ArgumentNullException("credentials.value", "Must provide a value for elevate id if the credentials property is included.");
                            }
                            PersonAlt personAlt = new PersonAlt(elevateId.Value, "ELEV");
                            personEntity.PersonAltIds.Add(personAlt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts credentials
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="credentials"></param>
        /// <param name="personEntity"></param>
        private void ConvertPerson3DtoCredsToEntity(string personId, IEnumerable<CredentialDtoProperty2> credentials, PersonIntegration personEntity)
        {
            //credential type is required. 
            if (credentials != null && credentials.Count(c => string.IsNullOrEmpty(c.Type.ToString())) > 0)
            {
                throw new InvalidOperationException("Credentials Type is required. ");
            }
            //Validate to make sure not more than one of each type is passed in the payload
            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType2.ColleaguePersonId) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one Colleague id.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType2.Ssn) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one SSN.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.CredentialType2.Sin) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one SIN.");
            }

            //Validate that only Ssn or Sin is included and not both
            Dtos.EnumProperties.CredentialType2?[] creds = new Dtos.EnumProperties.CredentialType2?[] { Dtos.EnumProperties.CredentialType2.Sin, Dtos.EnumProperties.CredentialType2.Ssn };
            var ssnSin = credentials.Where(c => creds.Contains(c.Type));
            if (ssnSin != null && ssnSin.Count() > 1)
            {
                throw new InvalidOperationException("You cannot include SSN and SIN at the same time.");
            }

            //Validate the colleagueId type against personId
            var colleagueIdCred = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.ColleaguePersonId);
            if (colleagueIdCred != null)
            {
                if (!string.IsNullOrEmpty(personId) && !personId.Equals(colleagueIdCred.Value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Provided Colleague credentials id does not match Colleague person id.");
                }
                else
                {
                    if (string.IsNullOrEmpty(personId) && !string.IsNullOrEmpty(colleagueIdCred.Value))
                    {
                        throw new InvalidOperationException("Assignment of Colleague credentials id is not allowed on a new person.");
                    }
                }
            }

            //Cannot include any other type of credentials such as bannerId, bannerSourceId, bannerUserName, bannerUdcId
            if (credentials != null && credentials.Any(crd => crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerSourcedId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerUdcId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerUserName)))
            {
                throw new InvalidOperationException("Credential type bannerId, bannerSourceId, bannerUserName, bannerUdcId are not supported in colleague.");
            }

            if (credentials != null)
            {
                //SSN
                var ssn = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.Ssn);
                if (ssn != null)
                {
                    if (string.IsNullOrEmpty(ssn.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for SSN if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(ssn.Value, "SSN");
                    personEntity.PersonAltIds.Add(personAlt);
                }

                //Sin
                var sin = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.CredentialType2.Sin);
                if (sin != null)
                {
                    if (string.IsNullOrEmpty(sin.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for SIN if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(sin.Value, "SIN");
                    personEntity.PersonAltIds.Add(personAlt);
                }
                
                //Elevate Id
                var elevateIdList = credentials.Where(c => c.Type == Dtos.EnumProperties.CredentialType2.ElevateID);
                if (elevateIdList != null && elevateIdList.Any())
                {
                    foreach (var elevateId in elevateIdList)
                    {
                        if (elevateId != null)
                        {
                            if (string.IsNullOrEmpty(elevateId.Value))
                            {
                                throw new ArgumentNullException("credentials.value", "Must provide a value for elevate id if the credentials property is included.");
                            }
                            PersonAlt personAlt = new PersonAlt(elevateId.Value, "ELEV");
                            personEntity.PersonAltIds.Add(personAlt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts credentials
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="credentials"></param>
        /// <param name="personEntity"></param>
        private void ConvertPerson4DtoCredsToEntity(string personId, IEnumerable<Credential3DtoProperty> credentials, PersonIntegration personEntity)
        {
            //credential type is required. 
            if (credentials != null && credentials.Count(c => string.IsNullOrEmpty(c.Type.ToString())) > 0)
            {
                throw new InvalidOperationException("Credentials Type is required. ");
            }
            //Validate to make sure not more than one of each type is passed in the payload
            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleaguePersonId) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one Colleague id.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.Ssn) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one SSN.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.Sin) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one SIN.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.TaxIdentificationNumber) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one tax indentification number.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ElevateID) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one elevate id.");
            }

            //Validate that only Ssn or Sin or taxindentification number is included and not the combination
            Dtos.EnumProperties.Credential3Type?[] creds = new Dtos.EnumProperties.Credential3Type?[] { Dtos.EnumProperties.Credential3Type.Sin, Dtos.EnumProperties.Credential3Type.Ssn, Credential3Type.TaxIdentificationNumber };
            var ssnSin = credentials.Where(c => creds.Contains(c.Type));
            if (ssnSin != null && ssnSin.Count() > 1)
            {
                throw new InvalidOperationException("You cannot include SSN and SIN and Tax Identification Number at the same time.");
            }

            //Validate the colleagueId type against personId
            var colleagueIdCred = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleaguePersonId);
            if (colleagueIdCred != null)
            {
                if (!string.IsNullOrEmpty(personId) && !personId.Equals(colleagueIdCred.Value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Provided Colleague credentials id does not match Colleague person id.");
                }
                else
                {
                    if (string.IsNullOrEmpty(personId) && !string.IsNullOrEmpty(colleagueIdCred.Value))
                    {
                        throw new InvalidOperationException("Assignment of Colleague credentials id is not allowed on a new person.");
                    }
                }
            }

            //Cannot include any other type of credentials such as bannerId, bannerSourceId, bannerUserName, bannerUdcId
            if (credentials != null && credentials.Any(crd => crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerSourcedId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerUdcId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerUserName)))
            {
                throw new InvalidOperationException("Credential type bannerId, bannerSourceId, bannerUserName, bannerUdcId are not supported in colleague.");
            }

            if (credentials != null)
            {
                //SSN
                var ssn = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.Ssn);
                if (ssn != null)
                {
                    if (string.IsNullOrEmpty(ssn.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for SSN if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(ssn.Value, "SSN");
                    personEntity.PersonAltIds.Add(personAlt);
                }

                //Sin
                var sin = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.Sin);
                if (sin != null)
                {
                    if (string.IsNullOrEmpty(sin.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for SIN if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(sin.Value, "SIN");
                    personEntity.PersonAltIds.Add(personAlt);
                }

                //tax indentification number
                var taxId = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.TaxIdentificationNumber);
                if (taxId != null)
                {
                    if (string.IsNullOrEmpty(taxId.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for tax indentification number if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(taxId.Value, "TAXIDENTIFICATIONNUMBER");
                    personEntity.PersonAltIds.Add(personAlt);
                }

                //Elevate Id
                var elevateId = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.ElevateID);
                if (elevateId != null)
                {
                    if (string.IsNullOrEmpty(elevateId.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for elevate id if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(elevateId.Value, "ELEV");
                    personEntity.PersonAltIds.Add(personAlt);
                }
            }
        }

        /// <summary>
        /// Converts credentials
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="credentials"></param>
        /// <param name="personEntity"></param>
        private void ConvertPerson5DtoCredsToEntity(string personId, IEnumerable<Credential3DtoProperty> credentials, PersonIntegration personEntity)
        {
            //credential type is required. 
            if (credentials != null && credentials.Count(c => string.IsNullOrEmpty(c.Type.ToString())) > 0)
            {
                throw new InvalidOperationException("Credentials Type is required. ");
            }
            //Validate to make sure not more than one of each type is passed in the payload
            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleaguePersonId) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one Colleague id.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.Ssn) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one SSN.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.Sin) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one SIN.");
            }

            if (credentials != null && credentials.Count(c => c.Type == Dtos.EnumProperties.Credential3Type.TaxIdentificationNumber) > 1)
            {
                throw new InvalidOperationException("You cannot include more than one tax indentification number.");
            }

            //Validate that only Ssn or Sin or taxindentification number is included and not the combination
            Dtos.EnumProperties.Credential3Type?[] creds = new Dtos.EnumProperties.Credential3Type?[] { Dtos.EnumProperties.Credential3Type.Sin, Dtos.EnumProperties.Credential3Type.Ssn, Credential3Type.TaxIdentificationNumber };
            var ssnSin = credentials.Where(c => creds.Contains(c.Type));
            if (ssnSin != null && ssnSin.Count() > 1)
            {
                throw new InvalidOperationException("You cannot include SSN and SIN and Tax Identification Number at the same time.");
            }

            //Validate the colleagueId type against personId
            var colleagueIdCred = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.ColleaguePersonId);
            if (colleagueIdCred != null)
            {
                if (!string.IsNullOrEmpty(personId) && !personId.Equals(colleagueIdCred.Value, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Provided Colleague credentials id does not match Colleague person id.");
                }
                else
                {
                    if (string.IsNullOrEmpty(personId) && !string.IsNullOrEmpty(colleagueIdCred.Value))
                    {
                        throw new InvalidOperationException("Assignment of Colleague credentials id is not allowed on a new person.");
                    }
                }
            }

            //Cannot include any other type of credentials such as bannerId, bannerSourceId, bannerUserName, bannerUdcId
            if (credentials != null && credentials.Any(crd => crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerSourcedId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerUdcId) ||
                                                                                 crd.Type.Equals(Dtos.EnumProperties.CredentialType2.BannerUserName)))
            {
                throw new InvalidOperationException("Credential type bannerId, bannerSourceId, bannerUserName, bannerUdcId are not supported in colleague.");
            }

            if (credentials != null)
            {
                //SSN
                var ssn = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.Ssn);
                if (ssn != null)
                {
                    if (string.IsNullOrEmpty(ssn.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for SSN if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(ssn.Value, "TAXIDENTIFICATIONNUMBER");
                    personEntity.PersonAltIds.Add(personAlt);
                }

                //Sin
                var sin = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.Sin);
                if (sin != null)
                {
                    if (string.IsNullOrEmpty(sin.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for SIN if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(sin.Value, "TAXIDENTIFICATIONNUMBER");
                    personEntity.PersonAltIds.Add(personAlt);
                }

                //tax indentification number
                var taxId = credentials.FirstOrDefault(c => c.Type == Dtos.EnumProperties.Credential3Type.TaxIdentificationNumber);
                if (taxId != null)
                {
                    if (string.IsNullOrEmpty(taxId.Value))
                    {
                        throw new ArgumentNullException("credentials.value", "Must provide a value for tax indentification number if the credentials property is included.");
                    }
                    PersonAlt personAlt = new PersonAlt(taxId.Value, "TAXIDENTIFICATIONNUMBER");
                    personEntity.PersonAltIds.Add(personAlt);
                }

                //Elevate Id
                var elevateIdList = credentials.Where(c => c.Type == Dtos.EnumProperties.Credential3Type.ElevateID);
                if (elevateIdList != null && elevateIdList.Any())
                {
                    foreach (var elevateId in elevateIdList)
                    {
                        if (elevateId != null)
                        {
                            if (string.IsNullOrEmpty(elevateId.Value))
                            {
                                throw new ArgumentNullException("credentials.value", "Must provide a value for elevate id if the credentials property is included.");
                            }
                            PersonAlt personAlt = new PersonAlt(elevateId.Value, "ELEV");
                            personEntity.PersonAltIds.Add(personAlt);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts gender type
        /// </summary>
        /// <param name="genderType2"></param>
        /// <returns></returns>
        private string ConvertGenderType2String(Dtos.EnumProperties.GenderType2? genderType2)
        {
            switch (genderType2)
            {
                case Dtos.EnumProperties.GenderType2.Male:
                    {
                        return "M";
                    }
                case Dtos.EnumProperties.GenderType2.Female:
                    {
                        return "F";
                    }
                default:
                    {
                        return string.Empty;
                    }
            }
        }

        /// <summary>
        /// Converts religion code
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private async Task<string> ConvertPerson2DtoReligionCodeToEntityAsync(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                throw new ArgumentNullException("personDto.religion.id", "Must provide an id for religion.");
            }

            string religionCode = string.Empty;

            if (!string.IsNullOrEmpty(guid))
            {
                var religionEntity = (await GetDenominationsAsync(false)).FirstOrDefault(r => r.Guid == guid);
                if (religionEntity == null)
                {
                    throw new KeyNotFoundException("Religion ID associated to guid '" + guid + "' not found in repository");
                }

                if (religionEntity != null && !string.IsNullOrEmpty(religionEntity.Guid))
                {
                    religionCode = religionEntity.Code;
                }
            }
            return religionCode;
        }

        /// <summary>
        /// Converts ethnicity
        /// </summary>
        /// <param name="personEthnicityDtoProperty"></param>
        /// <returns></returns>
        private async Task<List<string>> ConvertPerson2DtoEthnicityCodesEntityAsync(Dtos.DtoProperties.PersonEthnicityDtoProperty personEthnicityDtoProperty)
        {
            List<string> ethnicityCodes = null;

            //Validate id is required
            if (personEthnicityDtoProperty.EthnicGroup != null && string.IsNullOrEmpty(personEthnicityDtoProperty.EthnicGroup.Id))
            {
                throw new ArgumentNullException("PersonEthnicityDtoProperty.ethnicGroup.id", "Must provide an id for ethnicity group");
            }

            //Validate reporting contains only one item - per discussions with Kelly
            if (personEthnicityDtoProperty.Reporting != null && personEthnicityDtoProperty.Reporting.Count() > 1)
            {
                throw new InvalidOperationException("More than one ethnicity reporting category is not supported.");
            }

            //Validate required items in reporting
            if (personEthnicityDtoProperty.Reporting != null)
            {
                var reportingItem = personEthnicityDtoProperty.Reporting.FirstOrDefault();
                // Only support racial category reporting country of USA
                if (reportingItem.Country != null && reportingItem.Country.Code != CountryCodeType.USA)
                {
                    throw new ArgumentOutOfRangeException("Only USA is supported for ethnicity reporting in Colleague.");
                }
                if (reportingItem != null && reportingItem.Country != null && reportingItem.Country.EthnicCategory == null)
                {
                    throw new ArgumentNullException("reporting.country.ethnicCategory", "Reporting country ethnic category is required.");
                }
            }

            var ethnicityEntities = await GetEthnicitiesAsync(false);

            if (personEthnicityDtoProperty.EthnicGroup != null &&
                !string.IsNullOrEmpty(personEthnicityDtoProperty.EthnicGroup.Id) &&
                personEthnicityDtoProperty.Reporting != null
                && personEthnicityDtoProperty.Reporting.Any())
            {
                var reporting = personEthnicityDtoProperty.Reporting.FirstOrDefault();

                if (reporting.Country != null && reporting.Country.EthnicCategory != null)
                {
                    try
                    {
                        var ethnicCategory = (EthnicityType)Enum.Parse(typeof(Ellucian.Colleague.Domain.Base.Entities.EthnicityType), reporting.Country.EthnicCategory.Value.ToString());
                        var ethnicityEntity = ethnicityEntities
                            .FirstOrDefault(e => e.Guid.Equals(personEthnicityDtoProperty.EthnicGroup.Id, StringComparison.OrdinalIgnoreCase));

                        if (ethnicityEntity == null)
                        {
                            throw new KeyNotFoundException("Ethnicity ID associated with guid '" + personEthnicityDtoProperty.EthnicGroup.Id + "' not found in repository.");
                        }

                        if (reporting.Country != null && reporting.Country.EthnicCategory != null && reporting.Country.EthnicCategory.HasValue)
                        {
                            if (ethnicityEntity != null && !ethnicityEntity.Type.Equals(ethnicCategory))
                            {
                                throw new InvalidOperationException(string.Concat("Provided ethnic category '", reporting.Country.EthnicCategory.Value.ToString(),
                                    "' does not match the ethnic category for id ", personEthnicityDtoProperty.EthnicGroup.Id));
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        throw new ArgumentException("Could not find enitity for ethnicity.reporting.country.ethnicCategory :" + reporting.Country.EthnicCategory.Value.ToString());
                    }
                }
            }

            //Conversion
            if (personEthnicityDtoProperty.EthnicGroup != null && !string.IsNullOrEmpty(personEthnicityDtoProperty.EthnicGroup.Id))
            {
                var ethnicityEntity = ethnicityEntities.FirstOrDefault(e => e.Guid.Equals(personEthnicityDtoProperty.EthnicGroup.Id, StringComparison.OrdinalIgnoreCase));
                if (ethnicityEntity == null)
                {
                    throw new KeyNotFoundException("Could not find enitity for ethnicity.ethnicGroup.id:" + personEthnicityDtoProperty.EthnicGroup.Id);
                }
                string ethnicityCode = ethnicityEntity.Code;

                if (ethnicityCodes == null) ethnicityCodes = new List<string>();

                ethnicityCodes.Add(ethnicityCode);
            }
            else if (personEthnicityDtoProperty.EthnicGroup == null && personEthnicityDtoProperty.Reporting != null && personEthnicityDtoProperty.Reporting.Any())
            {
                string ethnicCategory = string.Empty;
                try
                {
                    var reportingItem = personEthnicityDtoProperty.Reporting.FirstOrDefault();

                    if (reportingItem.Country != null && reportingItem.Country.EthnicCategory != null)
                    {
                        ethnicCategory = reportingItem.Country.EthnicCategory.Value.ToString();
                        var ethnicityEnityType = (EthnicityType)Enum.Parse(typeof(Ellucian.Colleague.Domain.Base.Entities.EthnicityType), ethnicCategory);
                        var ethnicityEntity = ethnicityEntities.FirstOrDefault(e => e.Type.Equals(ethnicityEnityType));
                        if (ethnicityCodes == null) ethnicityCodes = new List<string>();

                        if (ethnicityEntity != null) ethnicityCodes.Add(ethnicityEntity.Code);
                    }
                }
                catch (ArgumentException)
                {
                    throw new ArgumentException("Could not find enitity for ethnicity.reporting.country.ethnicCategory :" + ethnicCategory);
                }
            }
            return ethnicityCodes;
        }

        /// <summary>
        /// Converts drivers license
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="personGuid"></param>
        /// <param name="personIdentityDocuments"></param>
        /// <returns></returns>
        private async Task<PersonDriverLicense> ConvertPerson2DtoDriversLicenseToEntityDocumentAsync(string personId, string personGuid, IEnumerable<Dtos.DtoProperties.PersonIdentityDocument> personIdentityDocuments)
        {
            PersonDriverLicense personDriverLicense = null;

            if (personIdentityDocuments != null && personIdentityDocuments.Any())
            {
                if (personIdentityDocuments.Where(doc => doc.Type.Category == Dtos.EnumProperties.PersonIdentityDocumentCategory.PhotoId).Count() > 1)
                {
                    throw new InvalidOperationException("Providing more than one drivers license is not supported.");
                }

                var driversLic = personIdentityDocuments.FirstOrDefault(d => d.Type.Category == Dtos.EnumProperties.PersonIdentityDocumentCategory.PhotoId);

                if (driversLic != null)
                {
                    if (driversLic.Type.Detail != null && string.IsNullOrEmpty(driversLic.Type.Detail.Id))
                    {
                        throw new ArgumentNullException("identityDocuments.type.detail.id", "An identity document detail id is required.");
                    }
                    if (driversLic.Country != null && driversLic.Country.Code != IsoCode.USA && driversLic.Country.Code != IsoCode.CAN)
                    {
                        throw new ArgumentOutOfRangeException("identityDocuments.country.code", "Only 'USA' and 'CAN' country codes are supported for a drivers license");
                    }

                    if (driversLic.Type.Detail != null && !string.IsNullOrEmpty(driversLic.Type.Detail.Id))
                    {
                        var identityDocTypes = await GetIdentityDocumentTypesAsync(false);

                        var driverLicenseDocType = identityDocTypes.FirstOrDefault(doc => doc.Guid.Equals(driversLic.Type.Detail.Id, StringComparison.OrdinalIgnoreCase));
                        if (driverLicenseDocType == null)
                        {
                            throw new KeyNotFoundException("Could not find enitity for identity document type with detail id: " + driversLic.Type.Detail.Id);
                        }

                        if (driverLicenseDocType.IdentityDocumentTypeCategory != IdentityDocumentTypeCategory.PhotoId)
                        {
                            throw new InvalidOperationException("Document entity type by Id '" + driversLic.Type.Detail.Id + "' does not match the document category of photoId");
                        }

                        personDriverLicense = new PersonDriverLicense(string.IsNullOrEmpty(personId) ? "$NEW.PERSON" : personId, driversLic.DocumentId, driversLic.ExpiresOn)
                        {
                            PersonGuid = personGuid,
                            DocType = driverLicenseDocType.Code
                        };

                        if (driversLic.Country != null && driversLic.Country.Region != null && !string.IsNullOrEmpty(driversLic.Country.Region.Code))
                        {
                            var issuingState = driversLic.Country.Region.Code.Split("-".ToCharArray()).ElementAt(1);
                            personDriverLicense.IssuingState = issuingState;
                        }
                    }
                    else if (Enum.IsDefined(typeof(Dtos.EnumProperties.PersonIdentityDocumentCategory), driversLic.Type.Category))
                    {
                        try
                        {
                            var identityDocTypes = await GetIdentityDocumentTypesAsync(false);

                            IdentityDocumentTypeCategory docTypeCategory = (IdentityDocumentTypeCategory)Enum.Parse(typeof(IdentityDocumentTypeCategory), driversLic.Type.Category.ToString());
                            var driverLicenseDocType = identityDocTypes.FirstOrDefault(doc => doc.IdentityDocumentTypeCategory == docTypeCategory && doc.Code.Equals("LICENSE", StringComparison.OrdinalIgnoreCase));
                            if (driverLicenseDocType == null)
                            {
                                throw new KeyNotFoundException("Could not find enitity for identity document type with category: " + driversLic.Type.Category.ToString());
                            }

                            personDriverLicense = new PersonDriverLicense(string.IsNullOrEmpty(personId) ? "$NEW.PERSON" : personId, driversLic.DocumentId, driversLic.ExpiresOn)
                            {
                                PersonGuid = personGuid,
                                DocType = driverLicenseDocType.Code
                            };

                            if (driversLic.Country != null && driversLic.Country.Region != null && !string.IsNullOrEmpty(driversLic.Country.Region.Code))
                            {
                                var issuingState = driversLic.Country.Region.Code.Split("-".ToCharArray()).ElementAt(1);
                                personDriverLicense.IssuingState = issuingState;
                            }
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException("Drivers license type entity not found with type: " + driversLic.Type.Category.ToString());
                        }
                    }
                }
            }
            return personDriverLicense;
        }

        /// <summary>
        /// Converts other identity documents to person Entity
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="personGuid"></param>
        /// <param name="personIdentityDocuments"></param>
        /// <returns></returns>
        private async Task<List<Domain.Base.Entities.PersonIdentityDocuments>> ConvertPerson2DtoIdentityDocumentsToEntityAsync(string personId, string personGuid, IEnumerable<Dtos.DtoProperties.PersonIdentityDocument> personIdentityDocuments)
        {
            List<Domain.Base.Entities.PersonIdentityDocuments> personDocuments = new List<PersonIdentityDocuments>();

            if (personIdentityDocuments != null && personIdentityDocuments.Any())
            {
                foreach (var document in personIdentityDocuments)
                {
                    Domain.Base.Entities.PersonIdentityDocuments personDocument = null;
                    if (document != null && document.Type != null && document.Type.Category == PersonIdentityDocumentCategory.Other)
                    {
                        string countryCode = "";
                        if (document.Country != null && document.Country.Code != null)
                        {
                            countryCode = document.Country.Code.ToString();
                            var place = (await GetPlacesAsync(false)).FirstOrDefault(x => x.PlacesCountry == countryCode);
                            if (place == null)
                            {
                                throw new ArgumentException(string.Format("Country code '{0}' cannot be found on the places table. ", countryCode), "identityDocuments.country.code");
                            }
                        }
                        if (document.Country != null && document.Country.Region != null && !string.IsNullOrEmpty(document.Country.Region.Code))
                        {
                            if (document.Country.Region != null && !string.IsNullOrEmpty(document.Country.Region.Code))
                            {
                                var regionCode = document.Country.Region.Code;
                                var place = (await GetPlacesAsync(false)).FirstOrDefault(x => x.PlacesCountry == countryCode && x.PlacesRegion == regionCode);
                                if (place == null)
                                {
                                    throw new ArgumentException(string.Format("Region code '{0}' with country '{1}' cannot be found on the places table. ", regionCode, countryCode), "identityDocuments.country.region.code");
                                }
                            }
                        }
                        if (document.Type.Detail != null && !string.IsNullOrEmpty(document.Type.Detail.Id))
                        {
                            var identityDocTypes = await GetIdentityDocumentTypesAsync(false);

                            var documentDocType = identityDocTypes.FirstOrDefault(doc => doc.Guid.Equals(document.Type.Detail.Id, StringComparison.OrdinalIgnoreCase));
                            if (documentDocType == null)
                            {
                                throw new KeyNotFoundException("Could not find enitity for identity document type with detail id: " + document.Type.Detail.Id);
                            }

                            if (documentDocType.IdentityDocumentTypeCategory != IdentityDocumentTypeCategory.Other)
                            {
                                throw new InvalidOperationException("Document entity type by Id '" + document.Type.Detail.Id + "' does not match the document category of 'other'");
                            }

                            personDocument = new Domain.Base.Entities.PersonIdentityDocuments(string.IsNullOrEmpty(personId) ? "$NEW.PERSON" : personId, document.DocumentId, document.ExpiresOn)
                            {
                                PersonGuid = personGuid,
                                DocType = documentDocType.Code,
                                Country = (document.Country != null) ? document.Country.Code.ToString() : null,
                                Region = (document.Country != null && document.Country.Region != null) ? document.Country.Region.Code : null
                            };
                        }
                        else if (Enum.IsDefined(typeof(Dtos.EnumProperties.PersonIdentityDocumentCategory), document.Type.Category))
                        {
                            try
                            {
                                var identityDocTypes = await GetIdentityDocumentTypesAsync(false);

                                IdentityDocumentTypeCategory docTypeCategory = (IdentityDocumentTypeCategory)Enum.Parse(typeof(IdentityDocumentTypeCategory), document.Type.Category.ToString());
                                var identityDocType = identityDocTypes.FirstOrDefault(doc => doc.IdentityDocumentTypeCategory == docTypeCategory && doc.Code.Equals("OTHER", StringComparison.OrdinalIgnoreCase));
                                if (identityDocType == null)
                                {
                                    throw new KeyNotFoundException("Could not find enitity for identity document type with category: " + document.Type.Category.ToString());
                                }

                                personDocument = new Domain.Base.Entities.PersonIdentityDocuments(string.IsNullOrEmpty(personId) ? "$NEW.PERSON" : personId, document.DocumentId, document.ExpiresOn)
                                {
                                    PersonGuid = personGuid,
                                    DocType = identityDocType.Code,
                                    Country = (document.Country != null) ? document.Country.Code.ToString() : null,
                                    Region = (document.Country != null && document.Country.Region != null) ? document.Country.Region.Code : null
                                };
                            }
                            catch (ArgumentException)
                            {
                                throw new ArgumentException("Identity Document type entity not found with type: " + document.Type.Category.ToString());
                            }
                        }
                        if (personDocument != null)
                        {
                            personDocuments.Add(personDocument);
                        }
                    }
                }
            }
            return personDocuments.Any() ? personDocuments : null;
        }

        /// <summary>
        /// Converts passport
        /// </summary>
        /// <param name="personId"></param>
        /// <param name="personGuid"></param>
        /// <param name="personIdentityDocuments"></param>
        /// <returns>PersonPassport</returns>
        private async Task<PersonPassport> ConvertPerson2DtoPassportDocumentToEntityAsync(string personId, string personGuid, IEnumerable<Dtos.DtoProperties.PersonIdentityDocument> personIdentityDocuments)
        {
            PersonPassport personPassport = null;
            if (personIdentityDocuments != null && personIdentityDocuments.Any())
            {
                if (personIdentityDocuments.Where(doc => doc.Type != null && doc.Type.Category == Dtos.EnumProperties.PersonIdentityDocumentCategory.Passport).Count() > 1)
                {
                    throw new InvalidOperationException("Providing information on more than one passport is not supported.");
                }

                var passportDoc = personIdentityDocuments.FirstOrDefault(d => d.Type != null && d.Type.Category == Dtos.EnumProperties.PersonIdentityDocumentCategory.Passport);
                if (passportDoc != null)
                {
                    if (passportDoc.Type.Detail != null && string.IsNullOrEmpty(passportDoc.Type.Detail.Id))
                    {
                        throw new ArgumentNullException("identityDocuments.type.detail.id", "An identity document detail id is required.");
                    }
                    //in colleague country code is required for a passport
                    if (passportDoc.Country == null || (passportDoc.Country != null && string.IsNullOrEmpty(passportDoc.Country.Code.ToString())))
                    {
                        throw new ArgumentNullException("identityDocuments.country.code", "Contry code is required for an identity document type of passport.");
                    }
                    if (passportDoc.Country != null && passportDoc.Country.Region != null)
                    {
                        throw new ArgumentOutOfRangeException("identityDocuments.country.region", "Region cannot be specified for a passport");
                    }

                    if (passportDoc.Type.Detail != null && !string.IsNullOrEmpty(passportDoc.Type.Detail.Id))
                    {
                        var identityDocTypes = await GetIdentityDocumentTypesAsync(false);

                        var passportDocType = identityDocTypes.FirstOrDefault(doc => doc.Guid.Equals(passportDoc.Type.Detail.Id, StringComparison.OrdinalIgnoreCase));
                        if (passportDocType == null)
                        {
                            throw new KeyNotFoundException("Could not find enitity for identity document type with detail id: " + passportDoc.Type.Detail.Id);
                        }

                        personPassport = new PersonPassport(string.IsNullOrEmpty(personId) ? "$NEW.PERSON" : personId, passportDoc.DocumentId)
                        {
                            ExpireDate = passportDoc.ExpiresOn,
                            IssuingCountry = passportDoc.Country != null && passportDoc.Country.Code != null ?
                                                await ConvertPerson2CountryCodeDtoToEntityAsync(passportDoc.Country.Code.ToString()) : null,
                            PersonGuid = personGuid,
                            DocType = passportDocType.Code
                        };
                    }
                    else if (Enum.IsDefined(typeof(Dtos.EnumProperties.PersonIdentityDocumentCategory), passportDoc.Type.Category))
                    {
                        try
                        {
                            var identityDocTypes = await GetIdentityDocumentTypesAsync(false);

                            IdentityDocumentTypeCategory docTypeCategory = (IdentityDocumentTypeCategory)Enum.Parse(typeof(IdentityDocumentTypeCategory), passportDoc.Type.Category.ToString());
                            var passportDocType = identityDocTypes.FirstOrDefault(doc => doc.IdentityDocumentTypeCategory == docTypeCategory && doc.Code.Equals("PASSPORT", StringComparison.OrdinalIgnoreCase));
                            if (passportDocType == null)
                            {
                                throw new KeyNotFoundException("Could not find enitity for identity document type with category: " + passportDoc.Type.Category.ToString());
                            }

                            personPassport = new PersonPassport(string.IsNullOrEmpty(personId) ? "$NEW.PERSON" : personId, passportDoc.DocumentId)
                            {
                                ExpireDate = passportDoc.ExpiresOn,
                                IssuingCountry = passportDoc.Country != null && passportDoc.Country.Code != null ?
                                                await ConvertPerson2CountryCodeDtoToEntityAsync(passportDoc.Country.Code.ToString()) : null,
                                PersonGuid = personGuid,
                                DocType = passportDocType.Code
                            };
                        }
                        catch (ArgumentException)
                        {
                            throw new ArgumentException("Could not find enitity for identity document type with category: " + passportDoc.Type.Category.ToString());
                        }
                    }
                }
            }
            return personPassport;
        }

        /// <summary>
        /// Converts country code
        /// </summary>
        /// <param name="countryIso3Code"></param>
        /// <returns>string</returns>
        private async Task<string> ConvertPerson2CountryCodeDtoToEntityAsync(string countryIso3Code)
        {
            string code = "";
            if (!string.IsNullOrEmpty(countryIso3Code))
            {
                //if there is more than one country that matches the ISO code, we need to pick the country that does not have "Not in use" set to Yes
                Domain.Base.Entities.Country entity = null;
                var entities = (await GetCountryCodesAsync(false)).Where(c => c.Iso3Code.Equals(countryIso3Code, StringComparison.OrdinalIgnoreCase));
                if (entities != null && entities.Any())
                {
                    if (entities.Count() > 1)
                        entity = entities.FirstOrDefault(con => con.IsNotInUse == false);
                    if (entity == null)
                        entity = entities.FirstOrDefault();
                }
                if (entity == null)
                {
                    throw new KeyNotFoundException("Country not found with Iso3 code: " + countryIso3Code);
                }
                code = entity.Code;
            }
            return code;
        }

        private string MapPrefixCode(string prefixCode)
        {
            string prefix = null;
            if (!string.IsNullOrEmpty(prefixCode))
            {
                var prefixEntity = _referenceDataRepository.Prefixes.FirstOrDefault(p => p.Code == prefixCode);
                if (prefixEntity != null && !string.IsNullOrEmpty(prefixEntity.Abbreviation))
                {
                    prefix = prefixEntity.Abbreviation;
                }
                else
                {
                    logger.Info("The '" + prefixCode +
                                "' prefix code is not defined.  Ignoring during person create/update.");
                }
            }
            return prefix;
        }

        private string MapSuffixCode(string suffixCode)
        {
            string suffix = null;
            if (!string.IsNullOrEmpty(suffixCode))
            {
                var suffixEntity = _referenceDataRepository.Suffixes.FirstOrDefault(s => s.Code == suffixCode);
                if (suffixEntity != null && !string.IsNullOrEmpty(suffixEntity.Abbreviation))
                {
                    suffix = suffixEntity.Abbreviation;
                }
                else
                {
                    logger.Info("The '" + suffixCode +
                                "' suffix code is not defined.  Ignoring during person create/update.");
                }
            }
            return suffix;
        }

        private string MapSsn(IEnumerable<Dtos.Credential> credentials)
        {
            string ssn = null;
            if (credentials != null)
            {
                var ssnCredential =
                    credentials.Where(c => c.CredentialType == Dtos.CredentialType.SocialSecurityNumber)
                        .FirstOrDefault();
                if (ssnCredential != null && !string.IsNullOrEmpty(ssnCredential.Id))
                {
                    ssn = ssnCredential.Id;
                }
            }
            return ssn;
        }

        private string MapSsn2(IEnumerable<Dtos.DtoProperties.CredentialDtoProperty> credentials)
        {
            string ssn = null;
            if (credentials != null)
            {
                var ssnCredential =
                    credentials.Where(c => c.Type == Dtos.EnumProperties.CredentialType.Ssn || c.Type == Dtos.EnumProperties.CredentialType.Sin)
                        .FirstOrDefault();
                if (ssnCredential != null && !string.IsNullOrEmpty(ssnCredential.Value))
                {
                    ssn = ssnCredential.Value;
                }
            }
            return ssn;
        }

        private string MapSsn3(IEnumerable<CredentialDtoProperty2> credentials)
        {
            string ssn = null;
            if (credentials != null)
            {
                var ssnCredential =
                    credentials.Where(c => c.Type == CredentialType2.Ssn || c.Type == CredentialType2.Sin)
                        .FirstOrDefault();
                if (ssnCredential != null && !string.IsNullOrEmpty(ssnCredential.Value))
                {
                    ssn = ssnCredential.Value;
                }
            }
            return ssn;
        }

        private string MapSsn4(IEnumerable<Credential3DtoProperty> credentials)
        {
            string ssn = null;
            if (credentials != null)
            {
                var ssnCredential =
                    credentials.Where(c => c.Type == Credential3Type.Ssn || c.Type == Credential3Type.Sin || c.Type == Credential3Type.TaxIdentificationNumber)
                        .FirstOrDefault();
                if (ssnCredential != null && !string.IsNullOrEmpty(ssnCredential.Value))
                {
                    ssn = ssnCredential.Value;
                }
            }
            return ssn;
        }

        private string MapColleagueId(IEnumerable<Credential> credentials)
        {
            string personId = null;
            if (credentials != null)
            {
                var personIdCredential =
                    credentials.Where(c => c.CredentialType == Dtos.CredentialType.ColleaguePersonId).FirstOrDefault();
                if (personIdCredential != null && !string.IsNullOrEmpty(personIdCredential.Id))
                {
                    personId = personIdCredential.Id;
                }
            }
            return personId;
        }

        private string MapColleagueId2(IEnumerable<CredentialDtoProperty> credentials)
        {
            string personId = null;
            if (credentials != null)
            {
                var personIdCredential =
                    credentials.Where(c => c.Type == CredentialType.ColleaguePersonId).FirstOrDefault();
                if (personIdCredential != null && !string.IsNullOrEmpty(personIdCredential.Value))
                {
                    personId = personIdCredential.Value;
                }
            }
            return personId;
        }

        private string MapColleagueId3(IEnumerable<CredentialDtoProperty2> credentials)
        {
            string personId = null;
            if (credentials != null)
            {
                var personIdCredential =
                    credentials.Where(c => c.Type == CredentialType2.ColleaguePersonId).FirstOrDefault();
                if (personIdCredential != null && !string.IsNullOrEmpty(personIdCredential.Value))
                {
                    personId = personIdCredential.Value;
                }
            }
            return personId;
        }

        private string MapColleagueId4(IEnumerable<Credential3DtoProperty> credentials)
        {
            string personId = null;
            if (credentials != null)
            {
                var personIdCredential =
                    credentials.Where(c => c.Type == Credential3Type.ColleaguePersonId).FirstOrDefault();
                if (personIdCredential != null && !string.IsNullOrEmpty(personIdCredential.Value))
                {
                    personId = personIdCredential.Value;
                }
            }
            return personId;
        }

        private IEnumerable<Domain.Base.Entities.PersonAlt> MapPersonAltIds(IEnumerable<Dtos.Credential> credentials)
        {
            var personAltIds = new List<Domain.Base.Entities.PersonAlt>();
            if (credentials != null)
            {
                // map the person's Elevate ID
                var elevatePersonIdCredential =
                    credentials.Where(c => c.CredentialType == Dtos.CredentialType.ElevatePersonId).FirstOrDefault();
                if (elevatePersonIdCredential != null && !string.IsNullOrEmpty(elevatePersonIdCredential.Id))
                {
                    personAltIds.Add(new Domain.Base.Entities.PersonAlt(elevatePersonIdCredential.Id,
                        Domain.Base.Entities.PersonAlt.ElevatePersonAltType));
                }
            }
            return personAltIds;
        }

        private string MapGenderType(Dtos.GenderType? genderType)
        {
            switch (genderType)
            {
                case Dtos.GenderType.Male:
                    {
                        return "M";
                    }
                case Dtos.GenderType.Female:
                    {
                        return "F";
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private string MapGenderType2(Dtos.EnumProperties.GenderType2? genderType)
        {
            switch (genderType)
            {
                case Dtos.EnumProperties.GenderType2.Male:
                    {
                        return "M";
                    }
                case Dtos.EnumProperties.GenderType2.Female:
                    {
                        return "F";
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        private async Task<string> MapMaritalStatusAsync(Dtos.GuidObject maritalStatusGuid)
        {
            string maritalStatus = null;
            if (maritalStatusGuid != null && !string.IsNullOrEmpty(maritalStatusGuid.Guid))
            {
                var maritalStatusEntity
                    =
                    (await GetMaritalStatusesAsync()).FirstOrDefault(
                        m => m.Guid == maritalStatusGuid.Guid);
                if (maritalStatusEntity == null || string.IsNullOrEmpty(maritalStatusEntity.Code))
                {
                    string error = "The '" + maritalStatusGuid.Guid + "' marital status guid is not defined.";
                    logger.Error(error);
                    throw new ArgumentException(error);
                }
                maritalStatus = maritalStatusEntity.Code;
            }
            return maritalStatus;
        }

        private async Task<List<string>> MapEthnicityAsync(Dtos.GuidObject ethnicityGuid)
        {
            List<string> ethnicityCodes = null;
            if (ethnicityGuid != null && !string.IsNullOrEmpty(ethnicityGuid.Guid))
            {
                var ethnicityEntity
                    =
                    (await GetEthnicitiesAsync(false)).FirstOrDefault(e => e.Guid == ethnicityGuid.Guid);
                if (ethnicityEntity == null || string.IsNullOrEmpty(ethnicityEntity.Code))
                {
                    string error = "The '" + ethnicityGuid.Guid + "' ethnicity guid is not defined.";
                    logger.Error(error);
                    throw new ArgumentException(error);
                }
                ethnicityCodes = new List<string>() { ethnicityEntity.Code };
            }
            return ethnicityCodes;
        }

        private async Task<List<string>> MapRacesAsync(IEnumerable<Dtos.GuidObject> raceGuids)
        {
            List<string> raceCodes = null;
            if (raceGuids != null && raceGuids.Count() > 0)
            {
                raceCodes = new List<string>();
                foreach (var raceGuid in raceGuids)
                {
                    var raceEntity =
                        (await GetRacesAsync(false)).FirstOrDefault(r => r.Guid == raceGuid.Guid);
                    if (raceEntity == null || string.IsNullOrEmpty(raceEntity.Code))
                    {
                        string error = "The '" + raceGuid.Guid + "' race guid is not defined.";
                        logger.Error(error);
                        throw new ArgumentException(error);
                    }
                    raceCodes.Add(raceEntity.Code);
                }
            }
            return raceCodes;
        }

        private IEnumerable<Domain.Base.Entities.EmailAddress> MapEmailAddresses(
            IEnumerable<Dtos.EmailAddress> emailAddressDtos)
        {
            List<Domain.Base.Entities.EmailAddress> emailAddressEntities = null;
            if (emailAddressDtos != null && emailAddressDtos.Count() > 0)
            {
                emailAddressEntities = new List<Domain.Base.Entities.EmailAddress>();

                foreach (var emailAddressDto in emailAddressDtos)
                {
                    emailAddressEntities.Add(new Domain.Base.Entities.EmailAddress(emailAddressDto.Address,
                        emailAddressDto.EmailAddressType.ToString()));
                }
            }
            return emailAddressEntities;
        }

        private async Task<IEnumerable<Domain.Base.Entities.EmailAddress>> MapEmailAddresses2(
            IEnumerable<Dtos.DtoProperties.PersonEmailDtoProperty> emailAddressDtos)
        {
            List<Domain.Base.Entities.EmailAddress> emailAddressEntities = null;
            if (emailAddressDtos != null && emailAddressDtos.Any())
            {
                emailAddressEntities = new List<Domain.Base.Entities.EmailAddress>();
                var emailTypeEntities = await GetEmailTypesAsync(false);
                // Disallow duplicate email types in the DTO
                List<string> duplicateTypes = new List<string>();

                foreach (var emailAddressDto in emailAddressDtos)
                {
                    if (emailAddressDto.Type == null || emailAddressDto.Type.EmailType == null)
                        throw new ArgumentNullException("emailAddressDto.Type", "Email type is required to create a new email");
                    if (string.IsNullOrEmpty(emailAddressDto.Address))
                        throw new ArgumentNullException("emailAddressDto.Address", "Email address is required to create a new email");

                    string emailType = "";

                    if (emailAddressDto.Type.Detail != null && !string.IsNullOrEmpty(emailAddressDto.Type.Detail.Id))
                    {
                        try
                        {
                            emailType = emailTypeEntities.FirstOrDefault(et => et.Guid == emailAddressDto.Type.Detail.Id).Code;
                        }
                        catch
                        {
                            throw new ArgumentOutOfRangeException("emailAddressDto.Type", string.Format("Could not find the email type for email address '{0}' using guid '{1}. ", emailAddressDto.Address, emailAddressDto.Type.Detail.Id));
                        }
                    }
                    else
                    {
                        try
                        {
                            emailType = emailTypeEntities.FirstOrDefault(et => string.Equals(et.EmailTypeCategory.ToString(), emailAddressDto.Type.EmailType.ToString(), StringComparison.OrdinalIgnoreCase)).Code;
                        }
                        catch
                        {
                            throw new ArgumentOutOfRangeException("emailAddressDto.Type", string.Format("Could not find the email type for email address '{0}' using type '{1}'. ", emailAddressDto.Address, emailAddressDto.Type.EmailType.ToString()));
                        }
                    }
                    if (string.IsNullOrEmpty(emailType))
                    {
                        throw new ArgumentException(string.Format("Cannot find email type for '{0}'. ", emailAddressDto.Type.EmailType.ToString()), "emailAddressDto.Type.EmailType");
                    }
                    if (duplicateTypes.Contains(emailType))
                    {
                        throw new ArgumentException(string.Format("An email type cannot be duplicated for '{0}', type '{1}'", emailAddressDto.Address, emailAddressDto.Type.EmailType.ToString()));
                    }
                    duplicateTypes.Add(emailType);
                    var emailEntity = new Domain.Base.Entities.EmailAddress(emailAddressDto.Address, emailType);

                    if (emailAddressDto.Preference == Dtos.EnumProperties.PersonEmailPreference.Primary)
                    {
                        emailEntity.IsPreferred = true;
                    }
                    emailAddressEntities.Add(emailEntity);
                }
            }
            return emailAddressEntities;
        }

        private async Task<IEnumerable<Domain.Base.Entities.EmailAddress>> MapEmailAddresses3(
            IEnumerable<Dtos.DtoProperties.PersonEmailDtoProperty> emailAddressDtos)
        {
            List<Domain.Base.Entities.EmailAddress> emailAddressEntities = null;
            if (emailAddressDtos != null && emailAddressDtos.Any())
            {
                emailAddressEntities = new List<Domain.Base.Entities.EmailAddress>();
                var emailTypeEntities = await GetEmailTypesAsync(false);
                // Disallow duplicate email types in the DTO
                List<string> duplicateTypes = new List<string>();

                foreach (var emailAddressDto in emailAddressDtos)
                {
                    if (emailAddressDto.Type == null || emailAddressDto.Type.EmailType == null)
                    {
                        // throw new ArgumentNullException("emailAddressDto.Type", "Email type is required to create a new email");
                        IntegrationApiExceptionAddError("Email type is required when defining the emails object.", "persons.emails.type");
                        return emailAddressEntities;
                    }
                    if (string.IsNullOrEmpty(emailAddressDto.Address))
                    {
                        // throw new ArgumentNullException("emailAddressDto.Address", "Email address is required to create a new email");
                        IntegrationApiExceptionAddError("Email address is required when defining the emails object.", "persons.emails.address");
                        return emailAddressEntities;
                    }

                    string emailType = "";

                    if (emailAddressDto.Type.Detail != null && !string.IsNullOrEmpty(emailAddressDto.Type.Detail.Id))
                    {
                        try
                        {
                            emailType = emailTypeEntities.FirstOrDefault(et => et.Guid == emailAddressDto.Type.Detail.Id).Code;
                        }
                        catch
                        {
                            // throw new ArgumentOutOfRangeException("emailAddressDto.Type", string.Format("Could not find the email type for email address '{0}' using guid '{1}. ", emailAddressDto.Address, emailAddressDto.Type.Detail.Id));
                            IntegrationApiExceptionAddError(string.Format("Could not find the email type for email address '{0}' using guid '{1}. ", emailAddressDto.Address, emailAddressDto.Type.Detail.Id), "persons.emails.type.detail.id");
                        }
                    }
                    else
                    {
                        try
                        {
                            emailType = emailTypeEntities.FirstOrDefault(et => string.Equals(et.EmailTypeCategory.ToString(), emailAddressDto.Type.EmailType.ToString(), StringComparison.OrdinalIgnoreCase)).Code;
                        }
                        catch
                        {
                            // throw new ArgumentOutOfRangeException("emailAddressDto.Type", string.Format("Could not find the email type for email address '{0}' using type '{1}'. ", emailAddressDto.Address, emailAddressDto.Type.EmailType.ToString()));
                            IntegrationApiExceptionAddError(string.Format("Could not find the email type for email address '{0}' using type '{1}'. ", emailAddressDto.Address, emailAddressDto.Type.EmailType.ToString()), "persons.emails.type.emailType");
                        }
                    }
                    if (string.IsNullOrEmpty(emailType))
                    {
                        // throw new ArgumentException(string.Format("Cannot find email type for '{0}'. ", emailAddressDto.Type.EmailType.ToString()), "emailAddressDto.Type.EmailType");
                        IntegrationApiExceptionAddError(string.Format("Cannot find email type for '{0}'. ", emailAddressDto.Type.EmailType.ToString()), "persons.emails.type.emailType");
                    }
                    if (!string.IsNullOrEmpty(emailType))
                    {
                        if (duplicateTypes.Contains(emailType))
                        {
                            // throw new ArgumentException(string.Format("An email type cannot be duplicated for '{0}', type '{1}'", emailAddressDto.Address, emailAddressDto.Type.EmailType.ToString()));
                            IntegrationApiExceptionAddError(string.Format("An email type cannot be duplicated for '{0}', type '{1}'", emailAddressDto.Address, emailAddressDto.Type.EmailType.ToString()), "persons.emails");
                        }
                        duplicateTypes.Add(emailType);
                        var emailEntity = new Domain.Base.Entities.EmailAddress(emailAddressDto.Address, emailType);

                        if (emailAddressDto.Preference == Dtos.EnumProperties.PersonEmailPreference.Primary)
                        {
                            emailEntity.IsPreferred = true;
                        }
                        emailAddressEntities.Add(emailEntity);
                    }
                }
            }
            return emailAddressEntities;
        }

        #endregion

        #region Convert Person Dto to Address Entities Methods

        private async Task<IEnumerable<Domain.Base.Entities.Address>> ConvertAddressDtoToAddressEntitiesAsync(IEnumerable<PersonAddressDtoProperty> addressList)
        {
            List<Domain.Base.Entities.Address> addressEntities = null;
            if (addressList != null && addressList.Any())
            {
                addressEntities = new List<Domain.Base.Entities.Address>();
                foreach (var addressDto in addressList)
                {
                    if (addressDto.address == null)
                        throw new ArgumentNullException("personDto", "Address property is required");

                    if (addressDto.Type == null || addressDto.Type.AddressType == null)
                        throw new ArgumentNullException("personDto", "Address type is required");

                    // if given a nonnull GUID - check to make sure it is for a valid address
                    if (addressDto.address.Id != null && addressDto.address.Id != Guid.Empty.ToString())
                    {
                        var addressId = await _personRepository.GetAddressIdFromGuidAsync(addressDto.address.Id);
                        if (addressId == null)
                        {
                            throw new ArgumentException("Address GUID '" + addressDto.address.Id + "' not found.", "personDto");
                        }
                    }

                    var addressDtoAddress = addressDto.address;
                    var addressEntity = new Domain.Base.Entities.Address();
                    if (string.IsNullOrEmpty(addressDtoAddress.Id) || addressDtoAddress.Id == Guid.Empty.ToString())
                    {
                        if (addressDtoAddress.AddressLines == null || !addressDtoAddress.AddressLines.Any())
                            throw new ArgumentNullException("personDto", "Street address lines are required.");

                        var addressCountry = new Dtos.AddressCountry();


                        addressEntity.Guid = addressDtoAddress.Id;
                        addressEntity.AddressLines = addressDtoAddress.AddressLines;
                        addressEntity.Latitude = addressDtoAddress.Latitude;
                        addressEntity.Longitude = addressDtoAddress.Longitude;
                        addressEntity.AddressChapter = new List<string>();

                        if ((addressDtoAddress.GeographicAreas != null) && (addressDtoAddress.GeographicAreas.Any()))
                        {
                            var chapterEntities = await GetChaptersAsync(false);

                            foreach (var area in addressDtoAddress.GeographicAreas)
                            {
                                var geographicAreaEntity = await _referenceDataRepository.GetRecordInfoFromGuidGeographicAreaAsync(area.Id);
                                if (geographicAreaEntity == Domain.Base.Entities.GeographicAreaTypeCategory.Fundraising)
                                {
                                    var chapter = chapterEntities.FirstOrDefault(x => x.Guid == area.Id);
                                    if (chapter != null)
                                        addressEntity.AddressChapter.Add(chapter.Code);
                                }
                            }
                        }
                        if (addressDtoAddress.Place != null)
                        {
                            if (addressDtoAddress.Place != null && addressDtoAddress.Place.Country != null && !string.IsNullOrEmpty(addressDtoAddress.Place.Country.Code.ToString()))
                            {
                                addressCountry = addressDtoAddress.Place.Country;
                            }
                            else
                            {
                                throw new ArgumentNullException("addressDto.place.country.code", "A country code is required for an address with a place defined.");
                            }
                            //if there is more than one country that matches the ISO code, we need to pick the country that does not have "Not in use" set to Yes
                            Domain.Base.Entities.Country country = null;
                            var countryEntities = (await GetCountryCodesAsync(false)).Where(cc => cc.IsoAlpha3Code == addressCountry.Code.ToString());
                            if (countryEntities != null && countryEntities.Any())
                            {
                                if (countryEntities.Count() > 1)
                                    country = countryEntities.FirstOrDefault(con => con.IsNotInUse == false);
                                if (country == null)
                                    country = countryEntities.FirstOrDefault();
                            }
                            //var country = (await GetCountryCodesAsync(false)).FirstOrDefault(x => x.IsoAlpha3Code == addressCountry.Code.ToString());
                            if (country == null)
                            {
                                throw new KeyNotFoundException(string.Concat("Unable to locate country code '", addressCountry.Code, "'"));
                            }

                            switch (addressCountry.Code)
                            {
                                case Dtos.EnumProperties.IsoCode.USA:
                                    addressEntity.CountryCode = country.IsoAlpha3Code;
                                    addressEntity.CorrectionDigit = string.IsNullOrEmpty(addressCountry.CorrectionDigit) ? null : addressCountry.CorrectionDigit;
                                    addressEntity.CarrierRoute = string.IsNullOrEmpty(addressCountry.CarrierRoute) ? null : addressCountry.CarrierRoute;
                                    addressEntity.DeliveryPoint = string.IsNullOrEmpty(addressCountry.DeliveryPoint) ? null : addressCountry.DeliveryPoint;
                                    break;
                                default:
                                    addressEntity.CountryCode = country.IsoAlpha3Code;
                                    if (!string.IsNullOrEmpty(addressCountry.CorrectionDigit) || !string.IsNullOrEmpty(addressCountry.CarrierRoute) || !string.IsNullOrEmpty(addressCountry.DeliveryPoint))
                                    {
                                        throw new ArgumentOutOfRangeException("addressDto.place.country", "correctionDigit, carrierRoute and deliveryPoint can only be specified when code is 'USA'.");
                                    }
                                    break;
                            }

                            if (addressCountry.Region != null && !string.IsNullOrEmpty(addressCountry.Region.Code))
                            {
                                if (addressCountry.Code == IsoCode.USA || addressCountry.Code == IsoCode.CAN)
                                {
                                    string state = "";
                                    if (addressCountry.Region.Code.Contains("-"))
                                        state = addressCountry.Region.Code.Substring(3);
                                    var states =
                                        (await GetStateCodesAsync(false)).FirstOrDefault(
                                            x => x.Code == state);

                                    if (states != null)
                                    {
                                        addressEntity.State = states.Code;
                                    }
                                    else
                                    {
                                        //throw an execption if the state is not valid for US or Canada. 
                                        throw new ArgumentOutOfRangeException("addressDto.place.region", string.Format("'{0}' is not defined as a state or province. ", state));
                                    }
                                }
                                else
                                {
                                    addressEntity.IntlRegion = addressCountry.Region == null
                                        ? null
                                        : addressCountry.Region.Code;
                                }
                            }

                            if (addressCountry.SubRegion != null && !string.IsNullOrEmpty(addressCountry.SubRegion.Code))
                            {
                                var county = (await GetCountiesAsync(false)).FirstOrDefault(c => c.Code == addressCountry.SubRegion.Code);
                                if (county != null)
                                    addressEntity.County = county.Code;
                                else
                                    addressEntity.IntlSubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
                            }

                            addressEntity.City = string.IsNullOrEmpty(addressCountry.Locality) ? null : addressCountry.Locality;
                            addressEntity.PostalCode = string.IsNullOrEmpty(addressCountry.PostalCode) ? null : addressCountry.PostalCode;
                            addressEntity.IntlLocality = addressCountry.Locality;
                            addressEntity.IntlPostalCode = addressCountry.PostalCode;
                            addressEntity.IntlRegion = addressCountry.Region == null ? null : addressCountry.Region.Code;
                            addressEntity.IntlSubRegion = addressCountry.SubRegion == null ? null : addressCountry.SubRegion.Code;
                        }
                    }
                    else
                    {
                        addressEntity.Guid = !string.IsNullOrEmpty(addressDtoAddress.Id) ? addressDtoAddress.Id : null;
                    }
                    var addressTypes = await GetAddressTypes2Async(false);
                    if (addressDto.Type.Detail != null && !string.IsNullOrEmpty(addressDto.Type.Detail.Id))
                    {
                        try
                        {
                            addressEntity.TypeCode = addressTypes.FirstOrDefault(at => at.Guid == addressDto.Type.Detail.Id).Code;
                        }
                        catch
                        {
                            throw new ArgumentOutOfRangeException("address.Type.Detail.Id", string.Format("Invalid Address Type guid '{0}' ", addressDto.Type.Detail.Id));
                        }
                    }
                    else
                    {
                        try
                        {
                            addressEntity.TypeCode = addressTypes.FirstOrDefault(at => at.AddressTypeCategory.ToString().Equals(addressDto.Type.AddressType.ToString(), StringComparison.OrdinalIgnoreCase)).Code;
                        }
                        catch
                        {
                            throw new ArgumentOutOfRangeException("address.Type.Addresstype", string.Format("Invalid Address Type '{0}' ", addressDto.Type.AddressType.ToString()));
                        }
                    }
                    // Update Effective dates and seasonal dates
                    var effectiveStartDate = addressDto.AddressEffectiveStart;
                    var effectiveEndDate = addressDto.AddressEffectiveEnd;
                    if (effectiveStartDate > effectiveEndDate && effectiveEndDate.HasValue)
                    {
                        throw new ArgumentOutOfRangeException("address.startOn", "Address effective end date must be after effective start date.");
                    }
                    if (addressDto.AddressEffectiveStart.HasValue)
                    {
                        addressEntity.EffectiveStartDate = addressDto.AddressEffectiveStart;
                    }
                    if (addressDto.AddressEffectiveEnd.HasValue)
                    {
                        addressEntity.EffectiveEndDate = addressDto.AddressEffectiveEnd;
                    }
                    if (addressDto.SeasonalOccupancies != null)
                    {
                        addressEntity.SeasonalDates = new List<AddressSeasonalDates>();
                        foreach (var seasonal in addressDto.SeasonalOccupancies)
                        {
                            string startOn = "";
                            string endOn = "";
                            if (seasonal.Recurrence != null)
                            {
                                if (seasonal.Recurrence.TimePeriod != null)
                                {
                                    var startDate = seasonal.Recurrence.TimePeriod.StartOn;
                                    var endDate = seasonal.Recurrence.TimePeriod.EndOn;
                                    if (startDate > endDate)
                                    {
                                        throw new ArgumentOutOfRangeException(
                                            "address.seasonalOccupancies.recurrance.timePeriod.startOn",
                                            "Seasonal end date must be after seasonal start date.");
                                    }
                                    startOn = string.Concat(startDate.Value.Month, "/", startDate.Value.Day);
                                    endOn = string.Concat(endDate.Value.Month, "/", endDate.Value.Day);
                                }
                            }
                            if (!string.IsNullOrEmpty(startOn) && !string.IsNullOrEmpty(endOn))
                            {
                                var seasonalDates = new AddressSeasonalDates(startOn, endOn);
                                addressEntity.SeasonalDates.Add(seasonalDates);
                            }
                        }
                    }
                    addressEntity.IsPreferredResidence = addressDto.Preference == PersonPreference.Primary ? true : false;

                    // If we already have the address ID in the collection than consolidate
                    // the data for address type and make sure we don't have conflicting data
                    var existingAddress = addressEntities.FirstOrDefault(adr => adr.Guid == addressEntity.Guid &&
                        adr.AddressLines.SequenceEqual(addressEntity.AddressLines) &&
                        adr.IntlLocality == addressEntity.IntlLocality && adr.IntlPostalCode == adr.IntlPostalCode &&
                        adr.IntlRegion == addressEntity.IntlRegion && adr.IntlSubRegion == addressEntity.IntlSubRegion &&
                        adr.PostalCode == addressEntity.PostalCode && adr.State == addressEntity.State &&
                        adr.CountryCode == addressEntity.CountryCode);
                    if (existingAddress != null)
                    {
                        // Remove the original address entity
                        var index = addressEntities.FindIndex(adr => adr.Guid == addressEntity.Guid &&
                            adr.AddressLines.SequenceEqual(addressEntity.AddressLines) &&
                            adr.IntlLocality == addressEntity.IntlLocality && adr.IntlPostalCode == adr.IntlPostalCode &&
                            adr.IntlRegion == addressEntity.IntlRegion && adr.IntlSubRegion == addressEntity.IntlSubRegion &&
                            adr.PostalCode == addressEntity.PostalCode && adr.State == addressEntity.State &&
                            adr.CountryCode == addressEntity.CountryCode);
                        addressEntities.RemoveAt(index);
                        // Validate properties that must match
                        if (addressEntity.EffectiveStartDate.HasValue && existingAddress.EffectiveStartDate.HasValue)
                        {
                            if (addressEntity.EffectiveStartDate.Value != existingAddress.EffectiveStartDate.Value)
                            {
                                throw new ArgumentOutOfRangeException(
                                    "persons.addresses.address.startOn",
                                    "Existing Address of the same Id must have the same start date across all instances");
                            }
                        }
                        if (addressEntity.TypeCode == existingAddress.TypeCode)
                        {
                            throw new ArgumentOutOfRangeException(

                                "persons.addresses.address.type.addressType",
                                "An address can not be defined more than once with the same address type");
                        }
                        // Check effective end date.  If passed than do not add the address type to the new entity.
                        if (addressEntity.EffectiveEndDate <= DateTime.Today)
                        {
                            addressEntity.TypeCode = existingAddress.TypeCode;
                            addressEntity.EffectiveEndDate = existingAddress.EffectiveEndDate;
                        }
                        if (existingAddress.EffectiveEndDate <= DateTime.Today)
                        {
                            existingAddress.TypeCode = addressEntity.TypeCode;
                        }
                        // Update the address type to include types from both address entities
                        var addrTypes = existingAddress.TypeCode;
                        var addrType = addressEntity.TypeCode;
                        string[] newAddressTypes = addrTypes.Split(_SM);
                        if (!newAddressTypes.Contains(addrType))
                        {
                            if (addressEntity.IsPreferredResidence || addressEntity.IsPreferredAddress)
                            {
                                // Put preferred address type first
                                addrTypes = string.Concat(addrType, _SM, addrTypes);
                            }
                            else
                            {
                                addrTypes = string.Concat(addrTypes, _SM, addrType);
                            }
                            addressEntity.TypeCode = addrTypes;
                        }
                        // Update the Seasonal Dates
                        if (existingAddress.SeasonalDates != null)
                        {
                            if (existingAddress.SeasonalDates.Any())
                            {
                                foreach (var seasonalDates in existingAddress.SeasonalDates)
                                {
                                    // This could be subvalued so need to split on subvalue mark ASCII 252.
                                    var startDate = seasonalDates.StartOn;
                                    var endDate = seasonalDates.EndOn;
                                    try
                                    {
                                        AddressSeasonalDates newSeasonalDates = new AddressSeasonalDates(startDate, endDate);
                                        if (!addressEntity.SeasonalDates.Contains(newSeasonalDates))
                                        {
                                            addressEntity.SeasonalDates.Add(seasonalDates);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new ArgumentOutOfRangeException("persons.addresses.address.seasonalOccupancies", ex);
                                    }
                                }
                            }
                        }
                        // Address Preferences
                        if (existingAddress.IsPreferredAddress) addressEntity.IsPreferredAddress = true;
                        if (existingAddress.IsPreferredResidence) addressEntity.IsPreferredResidence = true;
                        // Phone numbers
                        foreach (var phone in existingAddress.PhoneNumbers)
                        {
                            if (!addressEntity.PhoneNumbers.Contains(phone))
                            {
                                addressEntity.AddPhone(phone);
                            }
                        }
                    }
                    addressEntities.Add(addressEntity);
                }
            }
            return addressEntities;
        }

        #endregion

        #region Convert Person Dto to Phone Entities Methods

        private async Task<IEnumerable<Domain.Base.Entities.Phone>> ConvertPhoneDtoToPhoneEntities(IEnumerable<PersonPhoneDtoProperty> phoneList)
        {
            var phoneEntities = new List<Domain.Base.Entities.Phone>();
            if (phoneList != null && phoneList.Any())
            {
                var phoneTypeEntities = await GetPhoneTypesAsync(false);
                foreach (var phoneDto in phoneList)
                {
                    if (string.IsNullOrEmpty(phoneDto.Number))
                        throw new ArgumentNullException("personDto.Phone.Number", "Phone number is required to create a new phone");
                    if (phoneDto.Type == null || phoneDto.Type.PhoneType == null)
                        throw new ArgumentNullException("personDto.Phone.Type", string.Format("A valid Phone type is required for phone number '{0}' ", phoneDto.Number));
                    if (phoneDto.Type.Detail != null && string.IsNullOrEmpty(phoneDto.Type.Detail.Id))
                        throw new ArgumentNullException("personDto.Phone.Type.Detail.Id", "The Detail Id is required when Detail has been defined.");

                    string phoneType = "";
                    if (phoneDto.Type.Detail != null && !string.IsNullOrEmpty(phoneDto.Type.Detail.Id))
                    {
                        try
                        {
                            phoneType = phoneTypeEntities.FirstOrDefault(et => et.Guid == phoneDto.Type.Detail.Id).Code;
                        }
                        catch
                        {
                            throw new ArgumentOutOfRangeException("phoneDto.Type.Detail.Id", string.Format("Could not find the phone type detail id '{0}' for phone number '{1}'. ", phoneDto.Type.Detail.Id, phoneDto.Number));
                        }
                    }
                    else
                    {
                        var phoneTypeEntity = phoneTypeEntities.FirstOrDefault(et => string.Equals(et.PhoneTypeCategory.ToString(), phoneDto.Type.PhoneType.ToString(), StringComparison.OrdinalIgnoreCase));
                        if (phoneTypeEntity != null)
                        {
                            phoneType = phoneTypeEntity.Code;
                        }
                    }

                    var phoneEntity = new Domain.Base.Entities.Phone(phoneDto.Number, phoneType, phoneDto.Extension);
                    if (phoneDto.Preference == Dtos.EnumProperties.PersonPreference.Primary) phoneEntity.IsPreferred = true;
                    phoneEntity.CountryCallingCode = phoneDto.CountryCallingCode;

                    phoneEntities.Add(phoneEntity);
                }
            }
            return phoneEntities;
        }

        #endregion

        #region Convert Organiztion Dto to Entity Methods

        private async Task<Domain.Base.Entities.PersonIntegration> ConvertOrganization2DtoToPersonEntityAsync(string personOrgId, Dtos.Organization2 organizationDto)
        {

            PersonIntegration personEntity = null;

            if (organizationDto == null || string.IsNullOrEmpty(organizationDto.Id))
                throw new ArgumentNullException("organizationDto", "Must provide guid for organization");

            if (organizationDto.Title == null)
                throw new ArgumentNullException("organizationDto", "Must provide organization title");

            personEntity = new PersonIntegration(personOrgId, organizationDto.Title)
            {
                Guid = organizationDto.Id.Equals(Guid.Empty.ToString(), StringComparison.OrdinalIgnoreCase) ? null : organizationDto.Id
            };

            personEntity.PreferredName = organizationDto.Title;

            // email addresses
            var emailAddressEntities = await MapEmailAddresses2(organizationDto.EmailAddresses);
            if (emailAddressEntities != null && emailAddressEntities.Count() > 0)
            {
                foreach (var emailAddressEntity in emailAddressEntities)
                {
                    personEntity.EmailAddresses.Add(emailAddressEntity);
                }
            }

            //roles
            if (organizationDto.Roles != null && organizationDto.Roles.Any())
            {
                personEntity.Roles.AddRange(ConvertOrganization2DtoRolesToEntity(organizationDto.Roles));
            }

            // Social Media
            if (organizationDto.SocialMedia != null && organizationDto.SocialMedia.Any())
            {
                personEntity.SocialMedia.AddRange(await ConvertPerson2DtoSocialMediaToEntity(organizationDto.SocialMedia));
            }

            // credentials
            if (organizationDto.Credentials != null && organizationDto.Credentials.Any())
            {
                //check to see if any unallowed credential types are listed
                if (organizationDto.Credentials.Any(
                        c => c.Type == CredentialType.Ssn || c.Type == CredentialType.Sin ||
                            c.Type == CredentialType.BannerId || c.Type == CredentialType.BannerSourcedId ||
                            c.Type == CredentialType.BannerUdcId || c.Type == CredentialType.BannerUserName))
                {
                    throw new ArgumentException("Unsupported credentials used. ssn, sin, bannerId, bannerSourceId, bannerUserName and bannerUdcId are not supported for use on Organizations");
                }

                //get the credentials that are of colleaguepersonid present, if any
                var colleaguePersonIdCreds = organizationDto.Credentials.Where(c => c.Type == CredentialType.ColleaguePersonId).ToList();

                if (colleaguePersonIdCreds.Any())
                {
                    //more than one colleaguepersonid is not allowed
                    if (colleaguePersonIdCreds.Count > 1)
                    {
                        throw new ArgumentException("Only one ColleaguePersonId credential is allowed.");
                    }
                    else if (colleaguePersonIdCreds.Count == 1 && !string.IsNullOrEmpty(personOrgId) &&
                        !string.Equals(personOrgId, colleaguePersonIdCreds[0].Value, StringComparison.OrdinalIgnoreCase))
                    {
                        //can only get here if there is a colleagueid present and the organization already exists so it has a colleague person id
                        //won't get here if the organization doesn't exists as the id will be passed in as null
                        throw new ArgumentException("ColleaguePersonId used does not match the existing id, this is not allowed to be changed.");
                    }
                }

                //get the credentials that are of colleaguepersonid present, if any
                var elevateIdCreds = organizationDto.Credentials.Where(c => c.Type == CredentialType.ElevateID).ToList();

                //more than one elevateid is not allowed
                if (elevateIdCreds.Count > 1)
                {
                    throw new ArgumentException("Only one ElevateId credential is allowed.");
                }

                ConvertPerson2DtoCredsToEntity(personOrgId, organizationDto.Credentials, personEntity);
            }

            return personEntity;
        }

        #endregion

        #region Convert Person Entity to Organization DTO

        /// <summary>
        /// Convert PersonIntegration return to Organization DTO
        /// </summary>
        /// <param name="personEntity"></param>
        /// <param name="bypassCache"></param>
        /// <returns>Organization2 DTO for Data Model</returns>
        private async Task<Dtos.Organization2> ConvertPersonIntegrationEntityToOrganizationDtoAsync(
            Domain.Base.Entities.PersonIntegration personEntity, bool bypassCache = false)
        {
            // create the person DTO
            var organizationDto = new Dtos.Organization2();

            organizationDto.Id = personEntity.Guid;
            organizationDto.Title = personEntity.PreferredName;

            var orgRoles = new List<Dtos.DtoProperties.OrganizationRoleDtoProperty>();

            if (personEntity.Roles != null && personEntity.Roles.Any())
            {
                foreach (var personRole in personEntity.Roles)
                {
                    //only add org roles that match to correct type
                    var orgRole = new Dtos.DtoProperties.OrganizationRoleDtoProperty();
                    switch (personRole.RoleType)
                    {
                        case PersonRoleType.Vendor:
                            orgRole.Type = OrganizationRoleType.Vendor;
                            orgRole.StartOn = personRole.StartDate;
                            orgRole.EndOn = personRole.EndDate;
                            orgRoles.Add(orgRole);
                            break;
                        case PersonRoleType.Partner:
                            orgRole.Type = OrganizationRoleType.Partner;
                            orgRole.StartOn = personRole.StartDate;
                            orgRole.EndOn = personRole.EndDate;
                            orgRoles.Add(orgRole);
                            break;
                        case PersonRoleType.Affiliate:
                            orgRole.Type = OrganizationRoleType.Affiliate;
                            orgRole.StartOn = personRole.StartDate;
                            orgRole.EndOn = personRole.EndDate;
                            orgRoles.Add(orgRole);
                            break;
                        case PersonRoleType.Constituent:
                            orgRole.Type = OrganizationRoleType.Constituent;
                            orgRole.StartOn = personRole.StartDate;
                            orgRole.EndOn = personRole.EndDate;
                            orgRoles.Add(orgRole);
                            break;
                    }
                }
            }

            if (orgRoles.Any())
            {
                organizationDto.Roles = orgRoles;
            }

            organizationDto.Credentials = await GetPersonCredentials(personEntity);

            List<Domain.Base.Entities.EmailAddress> emailEntities = personEntity.EmailAddresses;
            List<Domain.Base.Entities.Phone> phoneEntities = personEntity.Phones;
            List<Domain.Base.Entities.Address> addressEntities = personEntity.Addresses;
            List<Domain.Base.Entities.SocialMedia> socialMediaEntities = personEntity.SocialMedia;

            var emailAddresses = await GetEmailAddresses2(emailEntities, bypassCache);
            if ((emailAddresses != null) && (emailAddresses.Any())) organizationDto.EmailAddresses = emailAddresses;
            if (addressEntities != null)
            {
                var addresses = await GetAddresses2Async(addressEntities.Where(a => a.AddressLines != null && a.AddressLines.Any()).ToList(), bypassCache);
                if ((addresses != null) && (addresses.Any())) organizationDto.Addresses = addresses;
            }
            var phoneNumbers = await GetPhones2Async(phoneEntities, bypassCache);
            if ((phoneNumbers != null) && (phoneNumbers.Any())) organizationDto.Phones = phoneNumbers;
            if ((socialMediaEntities != null) && (socialMediaEntities.Any()))
                organizationDto.SocialMedia = await GetPersonSocialMediaAsync(socialMediaEntities, bypassCache);


            return organizationDto;
        }

        #endregion

        #region Other Methods

        public async Task<string> CheckCitizenshipfields(PersonCitizenshipDtoProperty status, string country, PersonCitizenshipDtoProperty oldStatus = null, string oldCountry = null)
        {
            bool nullStatus = status == null;
            bool nullCountry = string.IsNullOrEmpty(country);

            bool nullOldStatus = oldStatus == null;
            bool nullOldCountry = string.IsNullOrEmpty(oldCountry);


            var hostCountry = await _personRepository.GetHostCountryAsync(); //USA or CANADA
            bool isUSA = hostCountry == "USA";


            if (!nullStatus && (status.Category == null || status.Detail == null || string.IsNullOrEmpty(status.Detail.Id)))
            {
                throw new ApplicationException("Citizenship status requires Category and Detail.");
            }
            // Validate that the status category matches to the status detail id
            if (!nullStatus && status.Category != null && status.Detail != null && !string.IsNullOrEmpty(status.Detail.Id))
            {
                var citizenshipStatus = (await GetCitizenshipStatusesAsync(false)).FirstOrDefault(cs => cs.Guid == status.Detail.Id);
                if (citizenshipStatus == null)
                {
                    throw new ApplicationException(string.Format("Citizenship status detail id '{0}' is invalid.", status.Detail.Id));
                }
                else
                {
                    var category = citizenshipStatus.CitizenshipStatusType;
                    switch (category)
                    {
                        case CitizenshipStatusType.Citizen:
                            if (status.Category != Dtos.CitizenshipStatusType.Citizen)
                            {
                                throw new ApplicationException(string.Format("Citizenship status category '{0}' doesn't match the Detail Id category of '{1}'.", status.Category, citizenshipStatus.CitizenshipStatusType));
                            }
                            break;
                        case CitizenshipStatusType.NonCitizen:
                            if (status.Category != Dtos.CitizenshipStatusType.NonCitizen)
                            {
                                throw new ApplicationException(string.Format("Citizenship status category '{0}' doesn't match the Detail Id category of '{1}'.", status.Category, citizenshipStatus.CitizenshipStatusType));
                            }
                            break;

                    }
                }
            }

            if (nullOldStatus || oldStatus.Category == null || oldStatus.Detail == null || string.IsNullOrEmpty(oldStatus.Detail.Id))
            {
                nullOldStatus = true;
            }

            // This is a PUT
            if (nullStatus && !nullOldStatus && isUSA)
            {
                throw new ApplicationException("Citizenship status cannot be unset.");
            }

            return "";
        }

        public async Task CheckCitizenshipfields2(PersonCitizenshipDtoProperty status, string country, PersonCitizenshipDtoProperty oldStatus = null, string oldCountry = null, string personGuid = "")
        {
            bool nullStatus = status == null;
            bool nullCountry = string.IsNullOrEmpty(country);

            bool nullOldStatus = oldStatus == null;
            bool nullOldCountry = string.IsNullOrEmpty(oldCountry);


            var hostCountry = await _personRepository.GetHostCountryAsync(); //USA or CANADA
            bool isUSA = hostCountry == "USA";

            string personId = "";
            if (!string.IsNullOrEmpty(personGuid))
            {
                try
                {
                    personId = await _personRepository.GetPersonIdFromGuidAsync(personGuid);
                }
                catch
                {
                    personId = "";
                }
            }

            if (!nullStatus && (status.Category == null || status.Detail == null || string.IsNullOrEmpty(status.Detail.Id)))
            {
                // throw new ApplicationException("Citizenship status requires Category and Detail.");
                IntegrationApiExceptionAddError("Citizenship status requires Category and Detail.", "Validation.Exception", personGuid, personId);
            }
            // Validate that the status category matches to the status detail id
            if (!nullStatus && status.Category != null && status.Detail != null && !string.IsNullOrEmpty(status.Detail.Id))
            {
                var citizenshipStatus = (await GetCitizenshipStatusesAsync(false)).FirstOrDefault(cs => cs.Guid == status.Detail.Id);
                if (citizenshipStatus == null)
                {
                    IntegrationApiExceptionAddError(string.Format("Citizenship status detail id '{0}' is invalid.", status.Detail.Id), "Validation.Exception", personGuid, personId);
                }
                else
                {
                    var category = citizenshipStatus.CitizenshipStatusType;
                    switch (category)
                    {
                        case CitizenshipStatusType.Citizen:
                            if (status.Category != Dtos.CitizenshipStatusType.Citizen)
                            {
                                IntegrationApiExceptionAddError(string.Format("Citizenship status category '{0}' doesn't match the Detail Id category of '{1}'.", status.Category, citizenshipStatus.CitizenshipStatusType), "Validation.Exception", personGuid, personId);
                            }
                            break;
                        case CitizenshipStatusType.NonCitizen:
                            if (status.Category != Dtos.CitizenshipStatusType.NonCitizen)
                            {
                                IntegrationApiExceptionAddError(string.Format("Citizenship status category '{0}' doesn't match the Detail Id category of '{1}'.", status.Category, citizenshipStatus.CitizenshipStatusType), "Validation.Exception", personGuid, personId);
                            }
                            break;

                    }
                }
            }

            if (nullOldStatus || oldStatus.Category == null || oldStatus.Detail == null || string.IsNullOrEmpty(oldStatus.Detail.Id))
            {
                nullOldStatus = true;
            }

            // This is a PUT
            if (nullStatus && !nullOldStatus && isUSA)
            {
                // throw new ApplicationException("Citizenship status cannot be unset.");
                IntegrationApiExceptionAddError("Citizenship status cannot be unset.", "Validation.Exception", personGuid, personId);
            }

            if (IntegrationApiException != null)
            {
                throw IntegrationApiException;
            }
            return;
        }

        #endregion
    }
}