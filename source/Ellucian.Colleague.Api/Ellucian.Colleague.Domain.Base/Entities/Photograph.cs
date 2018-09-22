// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ellucian.Colleague.Domain.Base.Entities
{
    /// <summary>
    /// Provides an object for representing a photograph and its associated metadata.
    /// </summary>
    [Serializable]
    public class Photograph
    {
        private const string DefaultContentType = "application/octet-stream";

        /// <summary>
        /// Gets a Stream representing the photo as a binary object.
        /// </summary>
        public Stream PhotoStream { get; private set; }
        /// <summary>
        /// Gets a string indicating the content type of the PhotoStream.
        /// </summary>
        public string ContentType { get; private set; }
        /// <summary>
        /// Gets or sets an optional filename for this photograph.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="photoStream">Required Stream representing the photograph as binary data.</param>
        /// <param name="contentType">
        /// Optional string indicating the content type of the photo.
        /// If not provided the default content type of 'application/octet-stream' will be used.
        /// </param>
        public Photograph(Stream photoStream, string contentType = DefaultContentType)
        {
            if (photoStream == null)
            {
                throw new ArgumentNullException("photoStream", "photoStream argument cannot be null");
            }
            PhotoStream = photoStream;
            ContentType = contentType;
        }
    }
}
