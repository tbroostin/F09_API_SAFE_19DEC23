// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Content Key data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class ContentKeysController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IContentKeyService _contentKeyService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="contentKeyService">Service of type <see cref="IContentKeyService">IContentKeyService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public ContentKeysController(IAdapterRegistry adapterRegistry, IContentKeyService contentKeyService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _contentKeyService = contentKeyService;
            _logger = logger;
        }

        /// <summary>
        /// Get a Content Key
        /// </summary>
        /// <param name="id">The encryption key ID to use to encrypt the content key</param>
        /// <returns>The <see cref="ContentKey">Content Key</see></returns>
        [HttpGet]
        public async Task<ContentKey> GetContentKeyAsync(string id)
        {
            try
            {
                return await _contentKeyService.GetContentKeyAsync(id);
            }
            catch (KeyNotFoundException knfe)
            {
                throw CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Post an encrypted content key to have it decrypted
        /// </summary>
        /// <param name="contentKeyRequest">The <see cref="ContentKeyRequest">Content Key Request</see></param>
        /// <returns>The <see cref="ContentKey">Content Key</see></returns>
        [HttpPost]
        public async Task<ContentKey> PostContentKeyAsync(ContentKeyRequest contentKeyRequest)
        {
            try
            {
                return await _contentKeyService.PostContentKeyAsync(contentKeyRequest);
            }
            catch (KeyNotFoundException knfe)
            {
                throw CreateHttpResponseException(knfe.Message, HttpStatusCode.NotFound);
            }
            catch (Exception e)
            {
                throw CreateHttpResponseException(e.Message);
            }
        }
    }
}