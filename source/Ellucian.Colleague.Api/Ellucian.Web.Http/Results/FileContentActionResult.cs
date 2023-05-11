// Copyright 2019-2021 Ellucian Company L.P. and its affiliates.
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
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }
        private string filePath;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        private string fileName;
        
        public string ContentType
        {
            get { return contentType; }
            set { contentType = value; }
        }
        private string contentType;

        public Dictionary<string, string> CustomHeaders
        {
            get { return customHeaders; }
            set { customHeaders = value; }
        }
        private Dictionary<string, string> customHeaders;

        public FileContentActionResult(string filePath, string fileName, string contentType, Dictionary<string, string> customHeaders)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");
            if (string.IsNullOrEmpty(contentType))
                throw new ArgumentNullException("contentType");

            this.FilePath = filePath;
            this.FileName = fileName;
            this.ContentType = contentType;
            this.CustomHeaders = customHeaders;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            // create the response
            var response = new HttpResponseMessage();            
            response.Content = new StreamContent
                (new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096,
                    FileOptions.Asynchronous | FileOptions.DeleteOnClose));  // delete the temp file on close
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(ContentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = FileName };
            // add any custom headers
            if (CustomHeaders != null)
            {
                foreach (var customHeader in CustomHeaders)
                {
                    response.Headers.Add(customHeader.Key, customHeader.Value);
                }
            }

            return Task.FromResult(response);
        }
    }
}