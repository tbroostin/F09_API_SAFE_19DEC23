// Copyright 2012-2018 Ellucian Company L.P. and its affiliates.
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Ellucian.Colleague.Api.Licensing;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Web.Http.Controllers;
using slf4net;
using Ellucian.Colleague.Configuration.Licensing;
using Ellucian.Web.License;

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
        private readonly IPhotoRepository photoRepository;
        private readonly ILogger logger;

        /// <summary>
        /// injection constructor
        /// </summary>
        /// <param name="photoRepository">IPhotoRepository instance.</param>
        /// <param name="logger">ILogger instance.</param>
        public PhotosController(IPhotoRepository photoRepository, ILogger logger)
        {
            this.photoRepository = photoRepository;
            this.logger = logger;
        }

        /// <summary>
        /// Retrieves a person's photo using the provided id.
        /// </summary>
        /// <param name="id">Person's ID</param>
        /// <returns>The photo as a stream. The content-type will be based on the type of image being returned.</returns>
        /// <exception cref="FileNotFoundException">Thrown when a person's photo was not found.</exception>
        public HttpResponseMessage GetPersonPhoto(string id)
        {
            try
            {
                var repoResult = photoRepository.GetPersonPhoto(id);
                HttpResponseMessage result = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                result.Content = new StreamContent(repoResult.PhotoStream);
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(repoResult.ContentType);
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = repoResult.Filename ?? id };
                return result;
            }
            catch (Exception e)
            {
                logger.Debug(e, e.Message);
                throw CreateNotFoundException("Person photo", id);
            }
        }
    }
}
