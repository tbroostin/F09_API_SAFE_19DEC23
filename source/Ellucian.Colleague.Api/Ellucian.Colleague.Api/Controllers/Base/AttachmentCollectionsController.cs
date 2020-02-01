// Copyright 2019 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Adapters;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
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
    /// Provides access to Attachment Collection data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AttachmentCollectionsController : BaseCompressedApiController
    {
        private readonly IAdapterRegistry _adapterRegistry;
        private readonly IAttachmentCollectionService _attachmentCollectionService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the AttachmentsController class.
        /// </summary>
        /// <param name="adapterRegistry">Adapter registry of type <see cref="IAdapterRegistry">IAdapterRegistry</see></param>
        /// <param name="attachmentCollectionService">Service of type <see cref="IAttachmentCollectionService">IAttachmentCollectionService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        public AttachmentCollectionsController(IAdapterRegistry adapterRegistry, IAttachmentCollectionService attachmentCollectionService,
            ILogger logger)
        {
            _adapterRegistry = adapterRegistry;
            _attachmentCollectionService = attachmentCollectionService;
            _logger = logger;
        }

        /// <summary>
        /// Get the attachment collection by ID
        /// </summary>
        /// <param name="id">The attachment collection Id</param>
        /// <returns>The <see cref="AttachmentCollection">Attachment Collection</see></returns>
        [HttpGet]
        public async Task<AttachmentCollection> GetAttachmentCollectionByIdAsync(string id)
        {
            try
            {
                // get the attachment collection by ID
                return await _attachmentCollectionService.GetAttachmentCollectionByIdAsync(id);
            }
            catch (PermissionsException pex)
            {
                throw CreateHttpResponseException(pex.Message, HttpStatusCode.Forbidden);
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
        /// Get the attachment collections for current user
        /// </summary>
        /// <returns>List of <see cref="AttachmentCollection">Attachment Collections</see></returns>
        [HttpGet]
        public async Task<IEnumerable<AttachmentCollection>> GetAttachmentCollectionsByUserAsync()
        {
            try
            {
                // get the attachment collections for the current user
                return await _attachmentCollectionService.GetAttachmentCollectionsByUserAsync();
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
        /// Create the new attachment collection
        /// </summary>
        /// <param name="attachmentCollection">The updated <see cref="AttachmentCollection">Attachment Collection</see></param>
        /// <returns>Newly created <see cref="AttachmentCollection">Attachment Collection</see></returns>
        [HttpPost]
        public async Task<AttachmentCollection> PostAsync([FromBody] AttachmentCollection attachmentCollection)
        {
            try
            {
                // create the new attachment collection
                return await _attachmentCollectionService.PostAttachmentCollectionAsync(attachmentCollection);
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
        /// Update the attachment collection
        /// </summary>
        /// <param name="id">The ID of the attachment collection</param>
        /// <param name="attachmentCollection">The updated <see cref="AttachmentCollection">Attachment Collection</see></param>
        /// <returns>The updated <see cref="AttachmentCollection">Attachment Collection</see></returns>
        [HttpPut]
        public async Task<AttachmentCollection> PutAsync([FromUri] string id, [FromBody] AttachmentCollection attachmentCollection)
        {
            try
            {
                // update the attachment collection
                return await _attachmentCollectionService.PutAttachmentCollectionAsync(id, attachmentCollection);
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
        /// Get this user's effective permissions for the attachment collection
        /// </summary>
        /// <param name="id">The ID of the attachment collection</param>
        /// <returns>This user's <see cref="AttachmentCollectionEffectivePermissions">Attachment Collection Effective Permissions</see></returns>
        public async Task<AttachmentCollectionEffectivePermissions> GetEffectivePermissionsAsync([FromUri] string id)
        {
            try
            {                
                return await _attachmentCollectionService.GetEffectivePermissionsAsync(id);
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