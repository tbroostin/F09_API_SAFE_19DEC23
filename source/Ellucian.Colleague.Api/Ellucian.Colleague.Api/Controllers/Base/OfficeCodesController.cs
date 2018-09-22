// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Exposes OfficeCode data
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    [Authorize]
    public class OfficeCodesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository referenceDataRepository;
        private readonly IAdapterRegistry adapterRegistry;
        private readonly ILogger logger;

        /// <summary>
        /// OfficeCodesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public OfficeCodesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            this.adapterRegistry = adapterRegistry;
            this.referenceDataRepository = referenceDataRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Get Colleague OfficeCodes
        /// </summary>
        /// <accessComments>
        /// Any authenticated user can get these resources
        /// </accessComments>
        /// <returns>A List of OfficeCode DTOs</returns>
        /// <exception cref="HttpResponseException">Thrown if there was an error retrieving OfficeCodes. HTTP Status Code 400</exception>
        public IEnumerable<OfficeCode> GetOfficeCodes()
        {
            try
            {
                var officeCodeEntityList = referenceDataRepository.OfficeCodes;

                var officeCodeDtoAdapter = adapterRegistry.GetAdapter<Domain.Base.Entities.OfficeCode, OfficeCode>();

                var officeCodeDtoList = new List<OfficeCode>();
                foreach (var officeCodeEntity in officeCodeEntityList)
                {
                    officeCodeDtoList.Add(officeCodeDtoAdapter.MapToType(officeCodeEntity));
                }

                return officeCodeDtoList;
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateHttpResponseException("Unable to retrieve Office Codes", System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}