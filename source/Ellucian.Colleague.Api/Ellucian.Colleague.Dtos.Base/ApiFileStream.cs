using System;
using System.IO;

namespace Ellucian.Colleague.Dtos.Base
{
    /// <summary>
    /// Provides a generic wrapper for binary files returned by the web api.
    /// </summary>
    /// <remarks>
    /// This object is not meant to be directly serialized without additional/custom serialization techniques.
    /// </remarks>
    public class ApiFileStream
    {
        private const string DefaultContentType = "text/plain";
        
        /// <summary>
        /// Gets a Stream representing the binary data.
        /// </summary>
        public Stream Data { get; private set; }
        /// <summary>
        /// Gets a string stating the content type of the binary data.
        /// </summary>
        public string ContentType { get; private set; }
        /// <summary>
        /// Gets or sets an optional filename for this binary data.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Creates a new ApiFileStream using the provided data stream and optional content type.
        /// </summary>
        /// <param name="data">Required Stream representing the binary data.</param>
        /// <param name="contentType">
        /// Optional string indicating the content type of the data stream.
        /// If not provided the default content type of 'text/plain' will be used.
        /// </param>
        public ApiFileStream(Stream data, string contentType = DefaultContentType)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "data stream cannot be null");
            }
            this.Data = data;
            ContentType = contentType;
        }
    }
}
