// Copyright 2012-2021 Ellucian Company L.P. and its affiliates.
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Colleague.Coordination.Base.Services;
using Ellucian.Data.Colleague.Exceptions;
using Ellucian.Web.Http.Configuration;
using Ellucian.Web.Http.Controllers;
using Ellucian.Web.License;
using Ellucian.Web.Security;
using Newtonsoft.Json;
using slf4net;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Colleague.Api.Controllers.Base
{
    /// <summary>
    /// Provides a API controller for fetching photos.
    /// </summary>
    [Authorize]
    [LicenseProvider(typeof(EllucianLicenseProvider))]
    [EllucianLicenseModule(ModuleConstants.Base)]
    public class PhotosController : BaseCompressedApiController
    {
        private readonly IPhotoService photoService;
        private readonly ApiSettings apiSettings;
        private readonly ILogger logger;
        private const string invalidSessionErrorMessage = "Your previous session has expired and is no longer valid.";
        private const string invalidPermissionsErrorMessage = "The current user does not have the permissions to perform the requested operation.";
        /// <summary>
        /// injection constructor
        /// </summary>
        /// <param name="photoService">IPhotoService instance.</param>
        /// <param name="apiSettings">ISettingsRepository instance.</param>
        /// <param name="logger">ILogger instance.</param>
        public PhotosController(IPhotoService photoService, ApiSettings apiSettings, ILogger logger)
        {
            this.photoService = photoService;
            this.apiSettings = apiSettings;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves the photo configuration.
        /// </summary>
        /// <returns>Bool based on presence of PhotoURL and PhotoType</returns>
        public HttpResponseMessage GetUserPhotoConfiguration()
        {
            try
            {
                var settingsResult = apiSettings.PhotoConfiguration;
                HttpResponseMessage result = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(settingsResult))
                };
                return result;
            }
            catch (Exception e)
            {
                logger.Debug(e, e.Message);
                throw CreateHttpResponseException(e.Message);
            }
        }

        /// <summary>
        /// Retrieves a person's photo using the provided id.
        /// </summary>
        /// <param name="id">Person's ID</param>
        /// <returns>The photo as a stream. The content-type will be based on the type of image being returned.</returns>
        /// <exception cref="FileNotFoundException">Thrown when a person's photo was not found.</exception>
        /// <accessComments>
        /// A person may view their own photo. Authenticated users with the CAN.VIEW.PERSON.PHOTOS permission can see other people's photos.
        /// </accessComments>
        public async Task<HttpResponseMessage> GetPersonPhoto(string id)
        {
            try
            {
                var repoResult = await photoService.GetPersonPhotoAsync(id);
                HttpResponseMessage result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                result.Content = new StreamContent(repoResult.PhotoStream);
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(repoResult.ContentType);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = repoResult.Filename ?? id };
                return result;
            }
            catch (ColleagueSessionExpiredException csse)
            {
                logger.Error(csse, csse.Message);
                throw CreateHttpResponseException(invalidSessionErrorMessage, HttpStatusCode.Unauthorized);
            }
            catch (PermissionsException pex)
            {
                logger.Error(pex.ToString());
                throw CreateHttpResponseException(invalidPermissionsErrorMessage, HttpStatusCode.Forbidden);
            }
            catch (Exception e)
            {
                logger.Error(e, e.Message);
                throw CreateNotFoundException("Person photo", id);
            }
        }
    }
}
