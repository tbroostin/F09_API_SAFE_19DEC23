// Copyright 2019 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Ellucian.Web.Http.Results
{
    /// <summary>
    /// Handles IHttpActionResult return type for files
    /// </summary>
    public class FileContentActionResult : IHttpActionResult
    {
        public string filePath;
        public string fileName;
        public string contentType;
        public Dictionary<string, string> customHeaders;
        
        public FileContentActionResult(string filePath, string fileName, string contentType, Dictionary<string, string> customHeaders)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");
            if (string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException("contentType");

            this.filePath = filePath;
            this.fileName = fileName;
            this.contentType = contentType;
            this.customHeaders = customHeaders;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            // create the response
            var response = new HttpResponseMessage();            
            response.Content = new StreamContent
                (new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096,
                    FileOptions.Asynchronous | FileOptions.DeleteOnClose));  // delete the temp file on close
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
            // add any custom headers
            if (customHeaders != null)
            {
                foreach (var customHeader in customHeaders)
                {
                    response.Headers.Add(customHeader.Key, customHeader.Value);
                }
            }

            return Task.FromResult(response);
        }
    }
}