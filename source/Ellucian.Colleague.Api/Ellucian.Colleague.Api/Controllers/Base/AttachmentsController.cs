// Copyright 2019-2020 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Colleague.Dtos.Base;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.Http.Extensions;
using Ellucian.Web.Http.Results;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using slf4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides access to Attachment data.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class AttachmentsController : BaseCompressedApiController
    {
        private const string encrContentKeyHeader = "X-Encr-Content-Key";
        private const string encrIVHeader = "X-Encr-IV";
        private const string encrTypeHeader = "X-Encr-Type";
        private const string encrKeyIdHeader = "X-Encr-Key-Id";

        private readonly IAttachmentService _attachmentService;
        private readonly ILogger _logger;
        private readonly ApiSettings _apiSettings;

        /// <summary>
        /// Initializes a new instance of the AttachmentsController class.
        /// </summary>
        /// <param name="attachmentService">Service of type <see cref="IAttachmentService">IAttachmentService</see></param>
        /// <param name="logger">Logger of type <see cref="ILogger">ILogger</see></param>
        /// <param name="apiSettings">API settings</param>
        public AttachmentsController(IAttachmentService attachmentService, ILogger logger, ApiSettings apiSettings)
        {
            _attachmentService = attachmentService;
            _logger = logger;
            _apiSettings = apiSettings;
        }

        /// <summary>
        /// Get attachments
        /// </summary>
        /// <param name="owner">Owner (optional) to get attachments for</param>
        /// <param name="collectionId">Collection Id (optional) to get attachments for</param>
        /// <param name="tagOne">TagOne value to get attachments for</param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        [HttpGet]
        public async Task<IEnumerable<Attachment>> GetAttachmentsAsync(
            [FromUri(Name = "owner")] string owner = null,
            [FromUri(Name = "collectionid")] string collectionId = null,
            [FromUri(Name = "tagone")] string tagOne = null)
        {
            try
            {
                // get the attachments using filters
                return await _attachmentService.GetAttachmentsAsync(owner, collectionId, tagOne);
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
        /// Get the attachment's content
        /// </summary>
        /// <param name="id">Id of the attachment whose content is requested</param>
        /// <returns>A HttpResponseMessage with the attachment contents</returns>
        [HttpGet]
        public async Task<IHttpActionResult> GetAttachmentContentAsync(string id)
        {
            try
            {
                // get the attachment and content
                var attachmentTuple = await _attachmentService.GetAttachmentContentAsync(id);

                // return the encryption metadata, if present, in response headers
                var responseHeaders = new Dictionary<string, string>();
                if (attachmentTuple.Item3 != null)
                {
                    responseHeaders.Add(encrContentKeyHeader, Convert.ToBase64String(attachmentTuple.Item3.EncrContentKey));
                    responseHeaders.Add(encrIVHeader, Convert.ToBase64String(attachmentTuple.Item3.EncrIV));
                    responseHeaders.Add(encrTypeHeader, attachmentTuple.Item3.EncrType);
                    responseHeaders.Add(encrKeyIdHeader, attachmentTuple.Item3.EncrKeyId);
                }
                
                // return the content
                return new FileContentActionResult(attachmentTuple.Item2, attachmentTuple.Item1.Name,
                    attachmentTuple.Item1.ContentType, responseHeaders);
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
        /// Query attachments
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>List of <see cref="Attachment">Attachments</see></returns>
        [HttpPost]
        public async Task<IEnumerable<Attachment>> QueryAttachmentsByPostAsync([FromBody]AttachmentSearchCriteria criteria)
        {
            try
            {
                // get attachments by query criteria
                return await _attachmentService.QueryAttachmentsAsync(criteria);
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
        /// Create the new attachment
        /// </summary>
        /// <returns>Newly created <see cref="Attachment">Attachment</see></returns>
        [HttpPost]
        public async Task<Attachment> PostAsync()
        {
            try
            {
                if (Request.Content.IsMimeMultipartContent())
                {
                    // attachment metadata and content
                    return await PostAttachmentAndStreamAsync();
                }
                else
                {
                    // attachment metadata only
                    var attachmentDto = JsonConvert.DeserializeObject<Attachment>(new StreamReader(
                        await Request.Content.ReadAsStreamAsync()).ReadToEnd());
                    return await _attachmentService.PostAttachmentAsync(attachmentDto, GetAttachmentEncryptionMetadata());
                }                
            }
            catch (IOException ioe)
            {
                _logger.Error(ioe, ioe.Message);
                throw CreateHttpResponseException(ioe.Message);
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
        /// Update the attachment
        /// </summary>
        /// <param name="id">ID of the attachment to update</param>
        /// <param name="attachment">The <see cref="Attachment">Attachment</see> to update</param>
        /// <returns>The updated <see cref="Attachment">Attachment</see></returns>
        [HttpPut]
        public async Task<Attachment> PutAsync([FromUri] string id, [FromBody] Attachment attachment)
        {
            try
            {
                return await _attachmentService.PutAttachmentAsync(id, attachment);
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
        /// Delete the attachment
        /// </summary>
        /// <param name="id">Id of the attachment to delete</param>
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAsync(string id)
        {
            try
            {
                // delete the attachment
                await _attachmentService.DeleteAttachmentAsync(id);
                return Request.CreateResponse(HttpStatusCode.OK);
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

        // Get the encryption metadata from the request, if present
        private AttachmentEncryption GetAttachmentEncryptionMetadata()
        {
            AttachmentEncryption attachmentEncryption = null;

            // get the key ID
            string encrKeyId;
            IEnumerable<string> headerValues;
            if (Request.Headers.TryGetValues(encrKeyIdHeader, out headerValues))
            {
                encrKeyId = headerValues.FirstOrDefault();
                    
                // get the rest of the encryption metadata
                string encrIV = null;
                string encrContentKey = null;
                string encrType = null;
                if (Request.Headers.TryGetValues(encrIVHeader, out headerValues))
                    encrIV = headerValues.FirstOrDefault();                    
                if (Request.Headers.TryGetValues(encrContentKeyHeader, out headerValues))
                    encrContentKey = headerValues.FirstOrDefault();
                if (Request.Headers.TryGetValues(encrTypeHeader, out headerValues))
                    encrType = headerValues.FirstOrDefault();

                attachmentEncryption = new AttachmentEncryption(encrKeyId, encrType, Convert.FromBase64String(encrContentKey),
                        Convert.FromBase64String(encrIV));
            }

            return attachmentEncryption;
        }

        // Get the attachment metadata, stream, and temp file locations from the request
        private async Task<Attachment> PostAttachmentAndStreamAsync()
        {
            Attachment attachment = null;
            Stream attachmentContentStream = null;
            var attachmentTempFilePaths = new List<string>();

            try
            {
                // verify the request length does not exceed the max size
                var requestSizeHeader = Request.Content.Headers.FirstOrDefault(h => h.Key == "Content-Length");
                if (requestSizeHeader.Value.Any())
                {
                    long requestSize;
                    if (long.TryParse(requestSizeHeader.Value.First(), out requestSize))
                    {
                        if (requestSize > _apiSettings.AttachRequestMaxSize)
                        {
                            _logger.Error(string.Format("Request size of {0} exceeded max attachment request size of {1}", requestSize,
                                _apiSettings.AttachRequestMaxSize));
                            throw new ArgumentException("Max attachment request size exceeded");
                        }
                    }
                }

                // setup the input stream to bypass the max request length for the application
                var content = new StreamContent(HttpContext.Current.Request.GetBufferlessInputStream(true));
                foreach (var header in Request.Content.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
                // split the request and write attachment content to a local temp file
                var provider = new CustomMultipartFormDataStreamProvider(_attachmentService.GetAttachTempFilePath(),
                    "temp_attachment_" + Guid.NewGuid());
                var requestData = await content.ReadAsMultipartAsync(provider);

                // get the temp file location                
                if (requestData.FileData != null && requestData.FileData.Any())
                requestData.FileData.ToList().ForEach(fd => attachmentTempFilePaths.Add(fd.LocalFileName));

                if (attachmentTempFilePaths.Count() == 0)
                    throw new ArgumentException("No attachment content found in the request");
                if (attachmentTempFilePaths.Count() > 1)
                    throw new ArgumentException("Multiple attachment content in a single request is not allowed");

                // get the attachment metadata
                var attachmentHttpContent = requestData.Contents.Where(c => string.IsNullOrEmpty(c.Headers.ContentDisposition.FileName)).First();
                if (attachmentHttpContent != null)
                    attachment = JsonConvert.DeserializeObject<Attachment>(new StreamReader(await attachmentHttpContent.ReadAsStreamAsync()).ReadToEnd());
                if (attachment == null)
                    throw new ArgumentException("Attachment metadata not found in request");

                // open the file to pass the stream
                var filePath = attachmentTempFilePaths.First();
                attachmentContentStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, useAsync: true);

                // set the attachment size
                attachment.Size = new FileInfo(filePath).Length;

                return await _attachmentService.PostAttachmentAndContentAsync(attachment, GetAttachmentEncryptionMetadata(), attachmentContentStream);
            }
            finally
            {
                if (attachmentContentStream != null)
                {
                    try
                    {
                        attachmentContentStream.Close();
                    }
                    catch (Exception e)
                    {
                        string info = "Could not close stream when creating attachment content";
                        if (attachment != null && !string.IsNullOrEmpty(attachment.Id) && !string.IsNullOrEmpty(attachment.Name))
                            info = string.Format("{0}, id = {1} ({2})", info, attachment.Id, attachment.Name);
                        _logger.Info(e, info);
                    }
                }

                if (attachmentTempFilePaths != null && attachmentTempFilePaths.Any())
                {
                    // delete temp files.  Multiple may have been created if a multi attachment upload was attempted
                    foreach (var tempFile in attachmentTempFilePaths)
                    {
                        if (File.Exists(tempFile))
                        {
                            try
                            {
                                File.Delete(tempFile);
                            }
                            catch (Exception e)
                            {
                                _logger.Info(e, string.Format("Could not delete temp file {0}", tempFile));
                            }
                        }
                    }
                }
            }
        }
    }
}