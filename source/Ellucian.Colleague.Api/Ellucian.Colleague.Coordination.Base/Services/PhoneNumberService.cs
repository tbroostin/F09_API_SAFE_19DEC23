﻿// Copyright 2019-2022 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Domain.Base;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Domain.Repositories;
using Ellucian.Colleague.Dtos.Base;
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
    public class PhoneNumberService : BaseCoordinationService, IPhoneNumberService
    {
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IPhoneNumberRepository _phoneNumberRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberService"/> class.
        /// </summary>
        /// <param name="adapterRegistry">The adapter registry.</param>
        /// <param name="phoneNumberRepository">The phone number repository.</param>
        /// <param name="currentUserFactory">The current user factory</param>
        /// <param name="logger">The logger</param>
        /// <param name="roleRepository">Role repository</param>
        /// <param name="configurationRepository">The configuration repository</param>
        public PhoneNumberService(IPhoneNumberRepository phoneNumberRepository,
            IConfigurationRepository configurationRepository,
            ICurrentUserFactory currentUserFactory,
            IRoleRepository roleRepository,
            IAdapterRegistry adapterRegistry,
            ILogger logger)
            : base(adapterRegistry, currentUserFactory, roleRepository, logger)
        {
            this._configurationRepository = configurationRepository;
            this._phoneNumberRepository = phoneNumberRepository;
        }

        /// <summary>
        /// Get a list of phone numbers from a list of Person keys
        /// </summary>
        /// <param name="criteria">Selection Criteria including PersonIds list.</param>
        /// <returns>List of Phone Number Objects <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneNumber>> QueryPhoneNumbersAsync(PhoneNumberQueryCriteria criteria)
        {
            if (criteria == null || criteria.PersonIds == null || criteria.PersonIds.Count() == 0)
            {
                logger.Error("Invalid personIds parameter: null or empty.");
                throw new ArgumentNullException("criteria", "No person IDs provided.");
            }

            VerifyUserCanQueryPhoneNumbers(criteria);

            var phoneDtoCollection = new List<Ellucian.Colleague.Dtos.Base.PhoneNumber>();
            var phoneCollection = await _phoneNumberRepository.GetPersonPhonesByIdsAsync(criteria.PersonIds.ToList());
            // Get the right adapter for the type mapping
            var phoneDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PhoneNumber, Ellucian.Colleague.Dtos.Base.PhoneNumber>();
            // Map the Address entity to the Address DTO
            foreach (var address in phoneCollection)
            {
                phoneDtoCollection.Add(phoneDtoAdapter.MapToType(address));
            }

            return phoneDtoCollection;            
        }

        /// <summary>
        /// Get a list of phone numbers from a list of Pilot Person keys
        /// </summary>
        /// <param name="criteria">Selection Criteria including PersonIds list.</param>
        /// <returns>List of Phone Number Objects <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PilotPhoneNumber>> QueryPilotPhoneNumbersAsync(PhoneNumberQueryCriteria criteria)
        {
            if (criteria == null || criteria.PersonIds == null || criteria.PersonIds.Count() == 0)
            {
                logger.Error("Invalid personIds parameter: null or empty.");
                throw new ArgumentNullException("criteria", "No person IDs provided.");
            }

            VerifyUserCanQueryPhoneNumbers(criteria);

            var pilotConfiguration = await _configurationRepository.GetPilotConfigurationAsync();
            var pilotPhoneDtoCollection = new List<Ellucian.Colleague.Dtos.Base.PilotPhoneNumber>();
            var pilotPhoneCollection = await _phoneNumberRepository.GetPilotPersonPhonesByIdsAsync(criteria.PersonIds.ToList(), pilotConfiguration);
            //var pilotPhoneCollection = _phoneNumberRepository.GetPilotPersonPhonesByIds(criteria.PersonIds.ToList(), pilotConfiguration);                         
            // Get the right adapter for the type mapping
            var pilotPhoneDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PilotPhoneNumber, Ellucian.Colleague.Dtos.Base.PilotPhoneNumber>();
            // Map the Pilot phone number entity to the Pilot phone number DTO
            foreach (var person in pilotPhoneCollection)
            {
                pilotPhoneDtoCollection.Add(pilotPhoneDtoAdapter.MapToType(person));
            }

            return pilotPhoneDtoCollection;
        }

        private void VerifyUserCanQueryPhoneNumbers(PhoneNumberQueryCriteria criteria)
        {
            if ((criteria != null && criteria.PersonIds != null && criteria.PersonIds.Any(id => id != CurrentUser.PersonId)) && !HasPermission(BasePermissionCodes.QueryPhoneNumbers))
            {
                throw new PermissionsException("You do not have permission to query phone numbers for others.");
            }
            return;
        }
        /// <summary>
        /// Get all current phone numbers for a person
        /// </summary>
        /// <param name="personId">Person to get phone numbers for</param>
        /// <accessComments>
        /// Users can retrieve their own phone numbers or person's with permission VIEW.PERSON.INFORMATION can retrieve phone numbers for other users.
        /// </accessComments>
        /// <returns>PhoneNumber Object<see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        public async Task<Dtos.Base.PhoneNumber> GetPersonPhones2Async(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                throw new ArgumentNullException("personId", "Person Id is required to retrieve the person's phone numbers.");
            }
            await CheckGetPersonViewPermission(personId);

            var phoneCollection = await _phoneNumberRepository.GetPersonPhonesAsync(personId);
            // Get the right adapter for the type mapping
            var phoneDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PhoneNumber, Ellucian.Colleague.Dtos.Base.PhoneNumber>();
            // Map the PhoneNumber entity to the Address DTO
            return phoneDtoAdapter.MapToType(phoneCollection);
        }

        /// <summary>
        /// Helper method to determine if the user has permission to view person information.
        /// Only a user can retrieve its own phone number
        /// Or a user with VIEW.PERSON.INFORMATION can retrieve phone numbers
        /// </summary>
        /// <exception><see cref="PermissionsException">PermissionsException</see></exception>
        private async Task CheckGetPersonViewPermission(string personId)
        {
            IEnumerable<string> userPermissionList = await GetUserPermissionCodesAsync();
            if (!userPermissionList.Contains(BasePermissionCodes.ViewPersonInformation) && CurrentUser.PersonId != personId)
            {
                logger.Error("User '" + CurrentUser.UserId + "' is not authorized to view person information.");
                throw new PermissionsException("User is not authorized to view person information.");
            }
        }

    }
}
