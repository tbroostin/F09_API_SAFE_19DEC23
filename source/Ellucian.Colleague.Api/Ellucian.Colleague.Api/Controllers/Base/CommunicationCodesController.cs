/* Copyright 2012-2015 Ellucian Company L.P. and its affiliates.*/
using System;
using System.Linq;
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

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides access to CommunicationCode data.
    /// </summary>
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    [Authorize]
    public class CommunicationCodesController : BaseCompressedApiController
    {
        private readonly IReferenceDataRepository _referenceDataRepository;
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;

        /// <summary>
        /// CommunicationCodesController constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="referenceDataRepository">Repository of type <see cref="IReferenceDataRepository">IReferenceDataRepository</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public CommunicationCodesController(IAdapterRegistry adapterRegistry, IReferenceDataRepository referenceDataRepository, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _referenceDataRepository = referenceDataRepository;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all Communication Codes.
        /// </summary>
        /// <returns>All <see cref="CommunicationCode">Communication Code codes and descriptions.</see></returns>
        /// <exception cref="HttpResponseException">Thrown if there was an error retrieving CommunicationCodes. HTTP Status Code 400</exception>
        [Obsolete("Obsolete as of version 1.8. Use version 2 instead")]
        public IEnumerable<CommunicationCode> GetCommunicationCodes()
        {
            try
            {
                var communicationCodeEntityCollection = _referenceDataRepository.CommunicationCodes;

                // Get the right adapter for the type mapping
                var communicationCodeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CommunicationCode, CommunicationCode>();

                // Map the CommunicationCode entity to the program DTO
                var communicationCodeDtoCollection = new List<CommunicationCode>();
                foreach (var communicationCodeEntity in communicationCodeEntityCollection)
                {
                    var communicationCodeDto = communicationCodeDtoAdapter.MapToType(communicationCodeEntity);
                    communicationCodeDto.Url = (communicationCodeEntity.Hyperlinks == null || communicationCodeEntity.Hyperlinks.Count() == 0) ? string.Empty : communicationCodeEntity.Hyperlinks.First().Url;
                    communicationCodeDtoCollection.Add(communicationCodeDto);
                }

                return communicationCodeDtoCollection;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error retrieving communication codes resource", System.Net.HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Retrieves all Communication Codes. Version 2 as of API 1.8
        /// </summary>
        /// <returns>All <see cref="CommunicationCode">CommunicationCode2 objects.</see></returns>
        /// <exception cref="HttpResponseException">Thrown if there was an error retrieving CommunicationCodes. HTTP Status Code 400</exception>
        public IEnumerable<CommunicationCode2> GetCommunicationCodes2()
        {
            try
            {
                var communicationCodeEntityCollection = _referenceDataRepository.CommunicationCodes;

                // Get the right adapter for the type mapping
                var communicationCodeDtoAdapter = _adapterRegistry.GetAdapter<Ellucian.Colleague.Domain.Base.Entities.CommunicationCode, CommunicationCode2>();

                // Map the CommunicationCode entity to the program DTO
                var communicationCodeDtoCollection = new List<CommunicationCode2>();
                foreach (var communicationCodeEntity in communicationCodeEntityCollection)
                {
                    communicationCodeDtoCollection.Add(communicationCodeDtoAdapter.MapToType(communicationCodeEntity));
                }

                return communicationCodeDtoCollection;
            }
            catch (Exception e)
            {
                _logger.Error(e, e.Message);
                throw CreateHttpResponseException("Error retrieving communication codes resource", System.Net.HttpStatusCode.BadRequest);
            }
        }
    }
}