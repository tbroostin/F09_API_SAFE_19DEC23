﻿// Copyright 2012-2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

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
        private readonly IPhoneNumberService _phoneNumberService;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the PhoneNumbersController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="phoneNumberRepository">Repository of type <see cref="IPhoneNumberRepository">IPhoneNumberRepository</see></param>        
        /// <param name="configurationRepository">Repository of type<see cref="IConfigurationRepository">IConfigurationRepository</see></param>   
        /// <param name="phoneNumberService">Service of type <see cref="IPhoneNumberService">IPhoneNumberService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public PhoneNumbersController(IAdapterRegistry adapterRegistry, IPhoneNumberRepository phoneNumberRepository, IConfigurationRepository configurationRepository, IPhoneNumberService phoneNumberService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _phoneNumberRepository = phoneNumberRepository;
            _configurationRepository = configurationRepository;
            _phoneNumberService = phoneNumberService;
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
        /// <accessComments>
        /// Authenticated users can query their own phone numbers only. Users with the QUERY.PHONE.NUMBERS permission can query phone numbers for anyone.
        /// </accessComments>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PhoneNumber>> QueryPhoneNumbersAsync(PhoneNumberQueryCriteria criteria)
        {
            if (criteria == null || criteria.PersonIds == null || criteria.PersonIds.Count() == 0)
            {
                _logger.Error("Invalid personIds parameter: null or empty.");
                throw CreateHttpResponseException("No person IDs provided.", HttpStatusCode.BadRequest);
            }
            try
            {
                var phoneDtoCollection = await _phoneNumberService.QueryPhoneNumbersAsync(criteria);
                return phoneDtoCollection;
            }
            catch (PermissionsException pex)
            {
                var permissionsMessage = "You do not have permission to query phone numbers.";
                _logger.Error(pex, permissionsMessage);
                throw CreateHttpResponseException(permissionsMessage, HttpStatusCode.Forbidden);
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
        /// <accessComments>
        /// Authenticated users can query their own phone numbers only. Users with the QUERY.PHONE.NUMBERS permission can query phone numbers for anyone.
        /// </accessComments>
        public async Task<IEnumerable<Ellucian.Colleague.Dtos.Base.PilotPhoneNumber>> QueryPilotPhoneNumbersAsync(PhoneNumberQueryCriteria criteria)
        {
            if (criteria == null || criteria.PersonIds == null || criteria.PersonIds.Count() == 0)
            {
                _logger.Error("Invalid personIds parameter: null or empty.");
                throw CreateHttpResponseException("No person IDs provided.", HttpStatusCode.BadRequest);
            }
            try
            {
                var pilotPhoneDtoCollection = await _phoneNumberService.QueryPilotPhoneNumbersAsync(criteria); 
                return pilotPhoneDtoCollection;
            }
            catch (PermissionsException pex)
            {
                var permissionsMessage = "You do not have permission to query phone numbers.";
                _logger.Error(pex, permissionsMessage);
                throw CreateHttpResponseException(permissionsMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                _logger.Error(e, "QueryPhoneNumbers error");
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}
