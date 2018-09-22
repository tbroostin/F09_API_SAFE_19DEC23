// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Moq;

namespace Ellucian.Web.Http.TestUtil
{

    public class MockHttpWebRequestCreator : IWebRequestCreate
    {
        private static object lockObject = new object();

        private static Mock<HttpWebRequest> webRequestMock;
        private static HttpWebRequest nextRequest;

        public static HttpWebRequest NextRequest
        {
            get { return nextRequest; }
            set
            {
                lock (lockObject)
                {
                    nextRequest = value;
                }
            }
        }

        /// <summary>See <see cref="IWebRequestCreate.Create"/>.</summary>
        public WebRequest Create(Uri uri)
        {
            webRequestMock.Setup(r => r.RequestUri).Returns(uri);         
            nextRequest.Headers = new WebHeaderCollection();
            return NextRequest;
        }

        public HttpWebRequest CreateHttp(Uri uri)
        {
            webRequestMock.Setup(r => r.RequestUri).Returns(uri);
            nextRequest.Headers = new WebHeaderCollection();
            return NextRequest;
        }

        /// <summary>
        /// Utility method to set the next WebRequest 
        /// </summary>
        /// <param name="httpWebResponseMock">A Mock WebResponse object, setup for your needs. The underlying object
        /// will be returned as the response by the WebRequest return value of this method.</param>
        /// <returns></returns>
        public static HttpWebRequest CreateWebRequest(Mock<HttpWebResponse> webResponseMock)
        {
            webRequestMock = new Mock<HttpWebRequest>();
            webRequestMock.Setup(r => r.GetResponse()).Returns(webResponseMock.Object);
            webRequestMock.Setup(r => r.GetResponseAsync()).ReturnsAsync(webResponseMock.Object);
            webRequestMock.Setup(r => r.GetRequestStreamAsync()).ReturnsAsync(new MemoryStream());
            webRequestMock.SetupAllProperties();
            ;
                        
            nextRequest = webRequestMock.Object;

            return webRequestMock.Object;
        }
    }
}