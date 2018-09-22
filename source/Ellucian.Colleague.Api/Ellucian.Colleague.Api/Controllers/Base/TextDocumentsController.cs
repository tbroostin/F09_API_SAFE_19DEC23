// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Filters;
using Ellucian.Web.License;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers
{
    /// <summary>
    /// Provides specific version information
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class TextDocumentsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly ILogger _logger;
        private readonly IDocumentService _documentService;

        /// <summary>
        /// Initializes a new instance of the AddressesController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="documentService">Service of type <see cref="IDocumentService">IDocumentService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public TextDocumentsController(IAdapterRegistry adapterRegistry, IDocumentService documentService, ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _documentService = documentService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves a text document
        /// </summary>
        /// <param name="documentId">ID of document to build</param>
        /// <param name="primaryEntity">Primary entity for document creation</param>
        /// <param name="primaryId">Primary record ID</param>
        /// <param name="personId">ID of person for whom document is being created</param>
        /// <returns>A text document</returns>
        /// <exception><see cref="HttpResponseException">HttpResponseException</see> with <see cref="HttpResponseMessage">HttpResponseMessage</see> containing <see cref="HttpStatusCode">HttpStatusCode</see>.BadRequest returned if the payment plan is not provided.</exception>
        [ParameterSubstitutionFilter(ParameterNames = new string[] { "documentId" })]
        public async Task<TextDocument> GetAsync([FromUri]string documentId, [FromUri]string primaryEntity, [FromUri]string primaryId, [FromUri]string personId)
        {
            if (string.IsNullOrEmpty(documentId))
            {
                string message = "Text document ID cannot be null.";
                _logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(primaryEntity))
            {
                string message = "Primary entity cannot be null.";
                _logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            if (string.IsNullOrEmpty(primaryId))
            {
                string message = "Primary record ID cannot be null.";
                _logger.Error(message);
                throw CreateHttpResponseException(message, HttpStatusCode.BadRequest);
            }
            try
            {
                return await _documentService.GetTextDocumentAsync(documentId, primaryEntity, primaryId, personId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                throw CreateHttpResponseException();
            }
        }

    }
}
