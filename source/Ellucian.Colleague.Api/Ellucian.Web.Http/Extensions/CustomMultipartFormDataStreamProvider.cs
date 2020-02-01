// Copyright 2019 Ellucian Company L.P.and its affiliates.
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Ellucian.Web.Http.Extensions
{
    /// <summary>
    /// Custom MultipartFormDataStreamProvider to override the temp file names
    /// </summary>
    public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        private readonly string fileName;

        public CustomMultipartFormDataStreamProvider(string path, string fileName) 
            : base(path) 
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            this.fileName = fileName;
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            return fileName;
        }
    }
}