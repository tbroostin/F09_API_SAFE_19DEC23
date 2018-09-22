// Copyright 2012-2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Linq;
using System.Net;
using Ellucian.Colleague.Domain.Base.Entities;
using Ellucian.Colleague.Domain.Base.Repositories;
using Ellucian.Data.Colleague;
using Ellucian.Data.Colleague.DataContracts;
using Ellucian.Data.Colleague.Repositories;
using Ellucian.Web.Cache;
using Ellucian.Web.Dependency;
using Ellucian.Web.Http.Configuration;
using slf4net;

namespace Ellucian.Colleague.Data.Base.Repositories
{
    /// <summary>
    /// Provides a repository for getting photographs.
    /// </summary>
    [RegisterType]
    public class PhotoRepository : BaseColleagueRepository, IPhotoRepository
    {
        private readonly ApiSettings apiSettings;

        public PhotoRepository(ICacheProvider cacheProvider, IColleagueTransactionFactory transactionFactory, ILogger logger, ApiSettings apiSettings)
            :base(cacheProvider, transactionFactory, logger)
        {
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Gets a person's photo using the person's id.
        /// </summary>
        /// <param name="id">The person's id</param>
        /// <returns>A Photograph of the person.</returns>
        public Photograph GetPersonPhoto(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException("id");
            }

            if (!IsPhotoConfigurationValid)
            {
                throw new Exception("Photos have not been configured");
            }

            var personPhotograph = RetrievePhotoFromRemoteServer(id);
            if (personPhotograph != null)
            {
                return personPhotograph;
            }
            else
            {
                throw new Exception(string.Format("Photo not found for person id '{0}'", id));
            }

        }

        /// <summary>
        /// Gets a photo from a remote HTTP server/service using the provided photo id and configuration.
        /// </summary>
        /// <param name="photoId">id/filename of the photo on the remove server (excluding a file extension).</param>
        /// <param name="photoServerConfiguration">configuration containing the parameters needed to form a valid HTTP request to the remote server.</param>
        /// <returns>A Photograph if the file exists on the remote server or null if the file does not exist</returns>
        private Photograph RetrievePhotoFromRemoteServer(string photoId)
        {
            Photograph photograph = null;
            try
            {
                string photoUri = string.Format(apiSettings.PhotoURL, photoId);
                ApplValcodes imageTypesValcode = GetImageMimeTypes();
                ApplValcodesVals entry = imageTypesValcode.ValsEntityAssociation.Where(a => a.ValInternalCodeAssocMember == apiSettings.PhotoType).FirstOrDefault();
                string imageContentType = entry.ValExternalRepresentationAssocMember;

                // http request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(photoUri);
                if (apiSettings.PhotoHeaders != null && apiSettings.PhotoHeaders.Count > 0)
                {
                    foreach (var customHeader in apiSettings.PhotoHeaders)
                    {
                        if (!string.IsNullOrEmpty(customHeader.Key))
                        {
                            request.Headers.Add(customHeader.Key, customHeader.Value ?? string.Empty);
                        }
                    }
                }

                logger.Debug("Retrieving image from remote server, URI='{0}', Headers='{1}'", request.Address, request.Headers.ToString());
                
                // execute
                WebResponse webResponse = request.GetResponse();
                photograph = new Photograph(webResponse.GetResponseStream(), imageContentType);
                //photograph.Filename = string.Format("{0}.{1}", photoId, photoServerConfiguration.ImageExtension.TrimStart('.'));
            }
            catch (Exception e)
            {
                logger.Error(e, "Error retrieving image from remote server");
            }

            return photograph;
        }

        /// <summary>
        ///  gets a value indicating if the photo configuration values are valid.
        /// </summary>
        private bool IsPhotoConfigurationValid
        {
            get
            {
                if (apiSettings == null)
                {
                    return false;
                }
                else if (string.IsNullOrEmpty(apiSettings.PhotoURL) || string.IsNullOrEmpty(apiSettings.PhotoType))
                {
                    return false;
                }
                var imageTypesValcode = GetImageMimeTypes();
                var b = imageTypesValcode.ValsEntityAssociation.Where(a => a.ValInternalCodeAssocMember == apiSettings.PhotoType).FirstOrDefault();
                if (b == null)
                {
                    return false;
                }
                return true;
            }
        }

        private ApplValcodes GetImageMimeTypes()
        {
            var imageMimeTypes = GetOrAddToCache<ApplValcodes>("ImageMimeTypes",
                () =>
                {
                    ApplValcodes imageMimeTypesValcode = DataReader.ReadRecord<ApplValcodes>("UT.VALCODES", "IMAGE.MIME.TYPE");

                    if (imageMimeTypesValcode == null)
                    {
                        var errorMessage = "Unable to access IMAGE.MIME.TYPE valcode table.";
                        logger.Info(errorMessage);
                        throw new Exception(errorMessage);
                    }
                    return imageMimeTypesValcode;
                }
                );
            return imageMimeTypes;
        }
    }
}
