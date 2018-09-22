using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Http.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Ellucian.Web.Http.Tests.Extensions
{
    [TestClass]
    public class HttpResponseMessageExtensionsTests
    {
        [TestMethod]
        public void AddPagingHeaders_ValidateHeaders_withTotalRecCount()
        {
            //Arrange
            int Offset = 5;
            int Limit = 10;
            int TotalRecCount = 50;
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Uri uri = new Uri("http://testhost/Test.Api/TestController?");
            request.RequestUri = uri;
            response.RequestMessage = request;
            string ExpectedFirst = "<http://testhost/Test.Api/TestController?offset=0&limit=10>; rel=\"first\",";
            string ExpectedPrev = "<http://testhost/Test.Api/TestController?offset=0&limit=10>; rel=\"prev\",";
            string ExpectedNext = "<http://testhost/Test.Api/TestController?offset=15&limit=10>; rel=\"next\",";
            string ExpectedLast = "<http://testhost/Test.Api/TestController?offset=40&limit=10>; rel=\"last\"";
            string ExpectedLink = string.Format("{0}{1}{2}{3}", ExpectedFirst, ExpectedPrev, ExpectedNext, ExpectedLast);

            //Act
            response.AddPagingHeaders(Offset, Limit, TotalRecCount);
            //To test without TotalRecCount, use below statement
            //response.AddPagingHeaders(Offset, Limit);

            bool LinkFlg = response.Headers.Contains("Link");
            bool TotalRecFlg = response.Headers.Contains("X-Total-Count");
            
            //Assert
            if (LinkFlg)
            { 
                string ActualLink = response.Headers.GetValues("Link").FirstOrDefault();
                Assert.AreEqual(ExpectedLink, ActualLink);
            }
           
            if (TotalRecFlg)
            {
                string ActualTotal = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                Assert.AreEqual(TotalRecCount.ToString(), ActualTotal);
            }
        }


        [TestMethod]
        public void AddPagingHeaders_ValidateHeaders_withInvalidOffset()
        {
            //Arrange
            int Offset = 75;
            int Limit = 10;
            int TotalRecCount = 50;
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Uri uri = new Uri("http://testhost/Test.Api/TestController?");
            request.RequestUri = uri;
            response.RequestMessage = request;
            string ExpectedFirst = "<http://testhost/Test.Api/TestController?offset=0&limit=10>; rel=\"first\",";
            string ExpectedLast = "<http://testhost/Test.Api/TestController?offset=40&limit=10>; rel=\"last\"";
            string ExpectedLink = string.Format("{0}{1}", ExpectedFirst, ExpectedLast);

            //Act
            response.AddPagingHeaders(Offset, Limit, TotalRecCount);
            //To test without TotalRecCount, use below statement
            //response.AddPagingHeaders(Offset, Limit);

            bool LinkFlg = response.Headers.Contains("Link");
            bool TotalRecFlg = response.Headers.Contains("X-Total-Count");

            //Assert
            if (LinkFlg)
            {
                string ActualLink = response.Headers.GetValues("Link").FirstOrDefault();
                Assert.AreEqual(ExpectedLink, ActualLink);
            }

            if (TotalRecFlg)
            {
                string ActualTotal = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                Assert.AreEqual(TotalRecCount.ToString(), ActualTotal);
            }
        }

        [TestMethod]
        public void AddXTotalCountHeader()
        {
            //Arrange
            int TotalRecCount = 50;
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage();
            Uri uri = new Uri("http://testhost/Test.Api/TestController?");
            request.RequestUri = uri;
            response.RequestMessage = request;

            //Act
            response.AddTotalRecordCountHeader(TotalRecCount);
            bool TotalRecFlg = response.Headers.Contains("X-Total-Count");

            //Assert
            if (TotalRecFlg)
            {
                string ActualTotal = response.Headers.GetValues("X-Total-Count").FirstOrDefault();
                Assert.AreEqual(TotalRecCount.ToString(), ActualTotal);
            }
        }
    }
}
