// Copyright 2016 Ellucian Company L.P. and its affiliates.
using Ellucian.Web.Http.Routes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Routing;

namespace Ellucian.Web.Http.Tests.Filters
{
    /// <summary>
    ///This is a test class for CustomMediaTypeAttributeFilter and is intended
    ///to contain all CustomMediaTypeAttributeFilter Unit Tests
    ///</summary>
    [TestClass]
    public class CustomMediaTypeAttributeFilterTests
    {
        private Mock<HttpContextBase> context;
        private Mock<HttpRequestBase> request;
        private Route fakeRoute;

        [TestInitialize]
        public void Initialize()
        {
            context  = new Mock<HttpContextBase>();
            request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            fakeRoute = new Route("fakeroute", null);
        }

        [TestMethod]
        public void MatchOnStandardMediaTypesTest()
        {
            //Arrange
            var valuesV1 = new RouteValueDictionary();
            var valuesV2 = new RouteValueDictionary();
            var valuesV3 = new RouteValueDictionary();
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint("customTypeV1");
            

            // Verify that requests with a standard media type specifying version #3 will match API with version #3 only.
            var acceptHeaders = new string[] { "application/vnd.ellucian.v3+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);

            //Act 
            customConstraint.Match(context.Object, fakeRoute, null, valuesV1, RouteDirection.IncomingRequest);
            defaultConstraintV2.Match(context.Object, fakeRoute, null, valuesV2, RouteDirection.IncomingRequest);
            nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, valuesV3, RouteDirection.IncomingRequest);

            //Assert
            Assert.IsFalse(valuesV1.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV1.ContainsValue("application/vnd.ellucian.v3+json"));
            Assert.IsFalse(valuesV2.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV2.ContainsValue("application/vnd.ellucian.v3+json"));
            Assert.IsTrue(valuesV3.ContainsKey("RequestedContentType"));
            Assert.IsTrue(valuesV3.ContainsValue("application/vnd.ellucian.v3+json"));
        }
        [TestMethod]
        public void MatchOnStandardMediaTypes_HedtechTest()
        {
            //Arrange
            var valuesV1 = new RouteValueDictionary();
            var valuesV2 = new RouteValueDictionary();
            var valuesV3 = new RouteValueDictionary();
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint("customTypeV1");
            
            //Hedtech
            var acceptHeaders = new string[] { "application/vnd.hedtech.v3+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);

            //Act 
            customConstraint.Match(context.Object, fakeRoute, null, valuesV1, RouteDirection.IncomingRequest);
            defaultConstraintV2.Match(context.Object, fakeRoute, null, valuesV2, RouteDirection.IncomingRequest);
            nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, valuesV3, RouteDirection.IncomingRequest);

            //Assert
            Assert.IsFalse(valuesV1.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV1.ContainsValue("application/vnd.hedtech.v3+json"));
            Assert.IsFalse(valuesV2.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV2.ContainsValue("application/vnd.hedtech.v3+json"));
            Assert.IsTrue(valuesV3.ContainsKey("RequestedContentType"));
            Assert.IsTrue(valuesV3.ContainsValue("application/vnd.hedtech.v3+json"));
        }
        [TestMethod]
        public void MatchOnStandardMediaTypes_CaseTest()
        {
            //Arrange
            var valuesV1 = new RouteValueDictionary();
            var valuesV2 = new RouteValueDictionary();
            var valuesV3 = new RouteValueDictionary();
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint("customTypeV1");

            // the case of the media type should not matter
            var acceptHeaders = new string[] { "application/vnd.HEDTECH.V3+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);

            //Act 
            customConstraint.Match(context.Object, fakeRoute, null, valuesV1, RouteDirection.IncomingRequest);
            defaultConstraintV2.Match(context.Object, fakeRoute, null, valuesV2, RouteDirection.IncomingRequest);
            nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, valuesV3, RouteDirection.IncomingRequest);

            //Assert
            Assert.IsFalse(valuesV1.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV1.ContainsValue("application/vnd.HEDTECH.V3+json"));
            Assert.IsFalse(valuesV2.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV2.ContainsValue("application/vnd.HEDTECH.V3+json"));
            Assert.IsTrue(valuesV3.ContainsKey("RequestedContentType"));
            Assert.IsTrue(valuesV3.ContainsValue("application/vnd.HEDTECH.V3+json"));
            
            
        }
        [TestMethod]
        public void MatchOnStandardMediaTypes_MultipleTest()
        {
            //Arrange
            var valuesV1 = new RouteValueDictionary();
            var valuesV2 = new RouteValueDictionary();
            var valuesV3 = new RouteValueDictionary();
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint("customTypeV1");

            // Verify that if a request specifies multiple media types, and if one of those media types is 
            // a standard media type whose version # matches the API's version number, then it's a match.
            var acceptHeaders = new string[]
            {
                "bogustype", 
                "application/vnd.ellucian.v1+json",
                "application/vnd.ellucian.v3+json",
                "application/vnd.ellucian.v4+json"
            };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);

            //Act 
            customConstraint.Match(context.Object, fakeRoute, null, valuesV1, RouteDirection.IncomingRequest);
            defaultConstraintV2.Match(context.Object, fakeRoute, null, valuesV2, RouteDirection.IncomingRequest);
            nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, valuesV3, RouteDirection.IncomingRequest);

            //Assert
            Assert.IsFalse(valuesV1.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV1.ContainsValue("application/vnd.ellucian.v3+json"));
            Assert.IsFalse(valuesV2.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV2.ContainsValue("application/vnd.ellucian.v3+json"));
            Assert.IsTrue(valuesV3.ContainsKey("RequestedContentType"));
            Assert.IsTrue(valuesV3.ContainsValue("application/vnd.ellucian.v3+json"));

        }

        [TestMethod]
        public void MatchOnStandardMediaTypes_MultipleCaseTest()
        {
            //Arrange
            var valuesV1 = new RouteValueDictionary();
            var valuesV2 = new RouteValueDictionary();
            var valuesV3 = new RouteValueDictionary();
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint("customTypeV1");

            // the case of the media type should not matter
            var acceptHeaders = new string[]
            {
                "boguStype", 
                "application/vnd.Hedtech.v1+json",
                "APPLICATION/VND.HEDTECH.V3+JSON",
                "application/Vnd.Hedtech.v4+json"
            };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);

            //Act 
            customConstraint.Match(context.Object, fakeRoute, null, valuesV1, RouteDirection.IncomingRequest);
            defaultConstraintV2.Match(context.Object, fakeRoute, null, valuesV2, RouteDirection.IncomingRequest);
            nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, valuesV3, RouteDirection.IncomingRequest);

            //Assert
            Assert.IsFalse(valuesV1.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV1.ContainsValue("APPLICATION/VND.HEDTECH.V3+JSON"));
            Assert.IsFalse(valuesV2.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV2.ContainsValue("APPLICATION/VND.HEDTECH.V3+JSON"));
            Assert.IsTrue(valuesV3.ContainsKey("RequestedContentType"));
            Assert.IsTrue(valuesV3.ContainsValue("APPLICATION/VND.HEDTECH.V3+JSON"));
        }
        [TestMethod]
        public void MatchOnCustomMediaTypesTest()
        {
            //Arrange
            var valuesV1 = new RouteValueDictionary();
            var valuesV2 = new RouteValueDictionary();
            var valuesV3 = new RouteValueDictionary();
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV1", "customTypeV2", "customTypeV3" });

            //no match media type
            var acceptHeaders = new string[] { "bogusmediatype" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);

            //Act 
            customConstraint.Match(context.Object, fakeRoute, null, valuesV1, RouteDirection.IncomingRequest);
            defaultConstraintV2.Match(context.Object, fakeRoute, null, valuesV2, RouteDirection.IncomingRequest);
            nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, valuesV3, RouteDirection.IncomingRequest);

            //Assert
            Assert.IsFalse(valuesV1.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV1.ContainsValue("bogusmediatype"));
            Assert.IsFalse(valuesV2.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV2.ContainsValue("bogusmediatype"));
            Assert.IsFalse(valuesV3.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV3.ContainsValue("bogusmediatype"));


        }
        [TestMethod]
        public void MatchOnCustomMediaTypes_CustomTypeV1Test()
        {
            //Arrange
            var valuesV1 = new RouteValueDictionary();
            var valuesV2 = new RouteValueDictionary();
            var valuesV3 = new RouteValueDictionary();
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV1", "customTypeV2", "customTypeV3" });

            // Verify that a request that has one or more of the custom media types will match
            var acceptHeaders = new string[] { "customTypeV1" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);

            //Act
            customConstraint.Match(context.Object, fakeRoute, null, valuesV1, RouteDirection.IncomingRequest);
            defaultConstraintV2.Match(context.Object, fakeRoute, null, valuesV2, RouteDirection.IncomingRequest);
            nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, valuesV3, RouteDirection.IncomingRequest);

            //Assert
            Assert.IsTrue(valuesV1.ContainsKey("RequestedContentType"));
            Assert.IsTrue(valuesV1.ContainsValue("customTypeV1"));
            Assert.IsFalse(valuesV2.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV2.ContainsValue("customTypeV1"));
            Assert.IsFalse(valuesV3.ContainsKey("RequestedContentType"));
            Assert.IsFalse(valuesV3.ContainsValue("customTypeV1"));
        }
    }
}
