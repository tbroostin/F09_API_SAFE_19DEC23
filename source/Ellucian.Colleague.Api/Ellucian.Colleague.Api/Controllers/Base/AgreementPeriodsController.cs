﻿// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to Citizen Type data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AgreementPeriodsController : BaseCompressedApiController
    {
        private readonly IAgreementsRepository _agreementsRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AgreementPeriodsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="agreementsRepository">Repository of type <see cref="IAgreementsRepository">IAgreementsRepository</see></param>
        /// <param name="logger">Interface to logger</param>
        public AgreementPeriodsController(IAdapterRegistry adapterRegistry, IAgreementsRepository agreementsRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _agreementsRepository = agreementsRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all <see cref="AgreementPeriod">agreement periods</see>
        /// </summary>
        /// <returns>All agreement periods</returns>
        /// <accessComments>Any authenticated user can get agreement period information.</accessComments>
        public async Task<IEnumerable<AgreementPeriod>> GetAgreementPeriodsAsync()
        {
            try
            {
                bool bypassCache = false;
                if (Request.Headers.CacheControl != null)
                {
                    if (Request.Headers.CacheControl.NoCache)
                    {
                        bypassCache = true;
                    }
                }

                var agreementPeriodCollection = await _agreementsRepository.GetAgreementPeriodsAsync(bypassCache);

                var agreementPeriodDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.AgreementPeriod, AgreementPeriod>();

                var agreementPeriodDtoCollection = new List<AgreementPeriod>();
                foreach (var agreementPeriod in agreementPeriodCollection)
                {
                    agreementPeriodDtoCollection.Add(agreementPeriodDtoAdapter.MapToType(agreementPeriod));
                }

                return agreementPeriodDtoCollection;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve agreement periods.");
            }

        }
    }
}
