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
    /// <summary>
    /// A web request creator for unit testing.
    /// Code swiped from 
    /// http://hamidshahid.blogspot.com/2013/01/mocking-httpwebrequest.html
    /// </summary>
    public class MockWebRequestCreator : IWebRequestCreate
    {

        private static object lockObject = new object();

        private static Mock<WebRequest> webRequestMock;
        private static WebRequest nextRequest;

        public static WebRequest NextRequest
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

        /// <summary>
        /// Utility method to set the next WebRequest 
        /// </summary>
        /// <param name="httpWebResponseMock">A Mock WebResponse object, setup for your needs. The underlying object
        /// will be returned as the response by the WebRequest return value of this method.</param>
        /// <returns></returns>
        public static WebRequest CreateWebRequest(Mock<WebResponse> webResponseMock)
        {
            webRequestMock = new Mock<WebRequest>();
            webRequestMock.Setup(r => r.GetResponse()).Returns(webResponseMock.Object);

            webRequestMock.SetupAllProperties();
            nextRequest = webRequestMock.Object;

            return webRequestMock.Object;
        }
    }
}
