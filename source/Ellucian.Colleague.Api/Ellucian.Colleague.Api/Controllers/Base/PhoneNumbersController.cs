// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Ellucian.Web.Http.Controllers;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Http;
using Ellucian.Web.Http.Filters;
using Ellucian.Colleague.Data.Base.DataContracts;
using Ellucian.Colleague.Data.Base.Repositories;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Colleague.Api.Licensing;
using slf4net;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;
using Ellucian.Web.Adapters;
using Ellucian.Web.Security;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Address data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PhoneNumbersController : BaseCompressedApiController
    {
        private readonly IPhoneNumberRepository _phoneNumberRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PhoneNumbersController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="phoneNumberRepository">Repository of type <see cref="IPhoneNumberRepository">IPhoneNumberRepository</see></param>        
        /// <param name="configurationRepository">Repository of type<see cref="IConfigurationRepository">IConfigurationRepository</see></param>        
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PhoneNumbersController(IAdapterRegistry adapterRegistry, IPhoneNumberRepository phoneNumberRepository, IConfigurationRepository configurationRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _phoneNumberRepository = phoneNumberRepository;
            _configurationRepository = configurationRepository;
            _logger = logger;
        }
        /// <summary>
        /// Get all current phone numbers for a person
        /// </summary>
        /// <param name="personId">Person to get phone numbers for</param>
        /// <returns>PhoneNumber Object <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        public Ellucian.Colleague.Dtos.Base.PhoneNumber GetPersonPhones(string personId)
        {
            if (string.IsNullOrEmpty(personId))
            {
                _logger.Error("Invalid personId parameter");
                throw CreateHttpResponseException("The personId is required.", HttpStatusCode.BadRequest);
            }
            try
            {
                var phoneCollection = _phoneNumberRepository.GetPersonPhones(personId);
                // Get the right adapter for the type mapping
                var phoneDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PhoneNumber, Ellucian.Colleague.Dtos.Base.PhoneNumber>();
                // Map the PhoneNumber entity to the Address DTO
                return phoneDtoAdapter.MapToType(phoneCollection);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }
        /// <summary>
        /// Get a list of phone numbers from a list of Person keys
        /// </summary>
        /// <param name="criteria">Selection Criteria including PersonIds list.</param>
        /// <returns>List of Phone Number Objects <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        public IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneNumber> QueryPhoneNumbers(PhoneNumberQueryCriteria criteria)
        {
            if (criteria.PersonIds == null || criteria.PersonIds.Count() == 0)
            {
                _logger.Error("Invalid personIds parameter: null or empty.");
                throw CreateHttpResponseException("No person IDs provided.", HttpStatusCode.BadRequest);
            }
            try
            {
                var phoneDtoCollection = new List<Ellucian.Colleague.Dtos.Base.PhoneNumber>();
                var phoneCollection = _phoneNumberRepository.GetPersonPhonesByIds(criteria.PersonIds.ToList());
                // Get the right adapter for the type mapping
                var phoneDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.PhoneNumber, Ellucian.Colleague.Dtos.Base.PhoneNumber>();
                // Map the Address entity to the Address DTO
                foreach (var address in phoneCollection)
                {
                    phoneDtoCollection.Add(phoneDtoAdapter.MapToType(address));
                }

                return phoneDtoCollection;
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "QueryPhoneNumbers error");
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Get a list of phone numbers from a list of Pilot Person keys
        /// </summary>
        /// <param name="criteria">Selection Criteria including PersonIds list.</param>
        /// <returns>List of Phone Number Objects <see cref="Ellucian.Colleague.Dtos.Base.PhoneNumber">PhoneNumber</see></returns>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PilotPhoneNumber>> QueryPilotPhoneNumbersAsync(PhoneNumberQueryCriteria criteria)
        {
            if (criteria.PersonIds == null || criteria.PersonIds.Count() == 0)
            {
                _logger.Error("Invalid personIds parameter: null or empty.");
                throw CreateHttpResponseException("No person IDs provided.", HttpStatusCode.BadRequest);
            }
            try
            {
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
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "QueryPhoneNumbers error");
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}
