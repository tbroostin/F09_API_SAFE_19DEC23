// Copyright 2014-2018 Ellucian Company L.P. and its affiliates.

using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;
using Ellucian.Web.Http.Routes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Web.Http.Tests.Routes
{

    [TestClass]
    public class HeaderVersionConstraintTest
    {
        private const string CustomMediaTypeFormat = "application/vnd.custom.integration.v{0}+json";

        [TestMethod]
        public void MatchOnStandardMediaTypesTest()
        {
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV1" });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);
            
            // Verify that requests with a standard media type specifying version #3 will match API with version #3 only.
            var acceptHeaders = new string[] { "application/vnd.ellucian.v3+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "application/vnd.hedtech.v3+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            acceptHeaders = new string[] { "application/vnd.HEDTECH.V3+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // Verify that if a request specifies multiple media types, and if one of those media types is 
            // a standard media type whose version # matches the API's version number, then it's a match.
            acceptHeaders = new string[]
            {
                "bogustype", 
                "application/vnd.ellucian.v1+json",
                "application/vnd.ellucian.v3+json",
                "application/vnd.ellucian.v4+json"
            };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            
            acceptHeaders = new string[]
            {
                "bogustype", 
                "application/vnd.hedtech.v1+json",
                "application/vnd.hedtech.v3+json",
                "application/vnd.hedtech.v4+json"
            };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            acceptHeaders = new string[]
            {
                "boguStype", 
                "application/vnd.Hedtech.v1+json",
                "APPLICATION/VND.HEDTECH.V3+JSON",
                "application/Vnd.Hedtech.v4+json"
            };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        [TestMethod]
        public void MatchOnCustomMediaTypesTest()
        {
            // If the API uses custom media types, only requests containing one of those custom media type will be matched. 

            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV1", "customTypeV2", "customTypeV3" });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            // Verify that a request that doesn't have any of the custom media types will not match the custom API
            var acceptHeaders = new string[] { "application/vnd.ellucian.v3+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "bogusmediatype" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // Verify that a request that has one or more of the custom media types will match
            acceptHeaders = new string[] { "customTypeV1" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "customTypeV3" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "customTypeV1", "customTypeV2", "customTypeV3" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            acceptHeaders = new string[] { "customtypev1" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // Verify it doesn't matter if there are other requested media types, as long as one of the supported
            // custom media types is included.
            acceptHeaders = new string[] { "bogusmediatype", "customTypeV1" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            acceptHeaders = new string[] { "BOGUSMEDIATYPE", "CUSTOMTYPEV1" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        [TestMethod]
        public void MatchDefaultMediaTypeTest()
        {
            // If request specify no media type, or generic media types, the default API matches. 
            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV2" });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            Route fakeRoute = new Route("fakeroute", null);

            // no media type provided, only default API matches?
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(new string[] { });
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // generic media types provided, only default API matches?
            var acceptHeaders = new string[] { "application/json", "application/xml", "*/*" };
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            
            // also verify that generic media types can be case insensitive
            acceptHeaders = new string[] { "APPLICATION/JSON", "application/XML", "TEXT/plain" };
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // just for grin, if the generic types come in both the legacy and accept headers
            // they are still ignored.
            acceptHeaders = new string[] { "application/json", "application/xml" };
            var legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/json" },
                    { "X-Ellucian-Media-Type", "application/xml" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // if request contains other media types not supported, no match at all
            acceptHeaders = new string[] { "application/vnd.hedtech.v1" };
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "bogusmediatype" };
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

        }

        [TestMethod]
        public void MatchLegacyMediaTypeHeaderTest()
        {
            // various tests to confirm that media types stored in the legacy header
            // X-Ellucian-Media-Type still get processed.

            var defaultConstraintV2 = new HeaderVersionConstraint(2, true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(3, false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV2" });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            request.Setup(x => x.AcceptTypes).Returns(new string[] { });
            context.Setup(x => x.Request).Returns(request.Object);
            Route fakeRoute = new Route("fakeroute", null);

            // generic media types requested
            var legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/json" },
                    { "X-Ellucian-Media-Type", "application/xml" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // supported media types requested
            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/vnd.hedtech.v3" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/vnd.ellucian.v3" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "APPLICATION/VND.ELLUCIAN.V3" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // other media types requested that don't match
            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/vnd.hedtech.v1" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // custom media type supported
            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "customTypeV2" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

        }

        [TestMethod]
        public void SemanticMatchOnStandardMediaTypesTest()
        {
            var defaultConstraintV2 = new HeaderVersionConstraint("2.0.0", true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint("3.0.0", false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV1.0.0" });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            // Verify that requests with a standard media type specifying version #3 will match API with version #3 only.
            var acceptHeaders = new string[] { "application/vnd.ellucian.v3.0.0+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "application/vnd.hedtech.v3.0.0+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            acceptHeaders = new string[] { "application/vnd.HEDTECH.V3.0.0+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // Verify that if a request specifies multiple media types, and if one of those media types is 
            // a standard media type whose version # matches the API's version number, then it's a match.
            acceptHeaders = new string[]
            {
                "bogustype",
                "application/vnd.ellucian.v1.0.0+json",
                "application/vnd.ellucian.v3.0.0+json",
                "application/vnd.ellucian.v4.0.0+json"
            };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[]
            {
                "bogustype",
                "application/vnd.hedtech.v1.0.0+json",
                "application/vnd.hedtech.v3.0.0+json",
                "application/vnd.hedtech.v4.0.0+json"
            };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            acceptHeaders = new string[]
            {
                "boguStype",
                "application/vnd.Hedtech.v1.0.0+json",
                "APPLICATION/VND.HEDTECH.V3.0.0+JSON",
                "application/Vnd.Hedtech.v4.0.0+json"
            };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest()
        {
            // If the API uses custom media types, only requests containing one of those custom media type will be matched. 

            var defaultConstraintV2 = new HeaderVersionConstraint("2.0.0", true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint("3.0.0", false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV1.0.0", "customTypeV2.0.0", "customTypeV3.0.0" });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            // Verify that a request that doesn't have any of the custom media types will not match the custom API
            var acceptHeaders = new string[] { "application/vnd.ellucian.v3.0.0+json" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "bogusmediatype" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // Verify that a request that has one or more of the custom media types will match
            acceptHeaders = new string[] { "customTypeV1.0.0" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "customTypeV3.0.0" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "customTypeV1.0.0", "customTypeV2.0.0", "customTypeV3.0.0" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            acceptHeaders = new string[] { "customtypev1.0.0" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // Verify it doesn't matter if there are other requested media types, as long as one of the supported
            // custom media types is included.
            acceptHeaders = new string[] { "bogusmediatype", "customTypeV1.0.0" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            acceptHeaders = new string[] { "BOGUSMEDIATYPE", "CUSTOMTYPEV1.0.0" };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            context.Setup(x => x.Request).Returns(request.Object);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        [TestMethod]
        public void SemanticMatchDefaultMediaTypeTest()
        {
            // If request specify no media type, or generic media types, the default API matches. 
            var defaultConstraintV2 = new HeaderVersionConstraint("2.0.0", true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint("3.0.0", false);
            var customConstraint = new HeaderVersionConstraint(new string[] { "customTypeV2.0.0" });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            Route fakeRoute = new Route("fakeroute", null);

            // no media type provided, only default API matches?
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(new string[] { });
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // generic media types provided, only default API matches?
            var acceptHeaders = new string[] { "application/json", "application/xml", "*/*" };
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // also verify that generic media types can be case insensitive
            acceptHeaders = new string[] { "APPLICATION/JSON", "application/XML", "TEXT/plain" };
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // just for grin, if the generic types come in both the legacy and accept headers
            // they are still ignored.
            acceptHeaders = new string[] { "application/json", "application/xml" };
            var legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/json" },
                    { "X-Ellucian-Media-Type", "application/xml" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // if request contains other media types not supported, no match at all
            acceptHeaders = new string[] { "application/vnd.hedtech.v1.0.0" };
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            acceptHeaders = new string[] { "bogusmediatype" };
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

        }

        [TestMethod]
        public void SemanticMatchLegacyMediaTypeHeaderTest()
        {
            // various tests to confirm that media types stored in the legacy header
            // X-Ellucian-Media-Type still get processed.

            var defaultConstraintV2 = new HeaderVersionConstraint("2.0.0", true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint("3.0.0", false);
            var customConstraint = new HeaderVersionConstraint( new string[] { "customTypeV2.0.0" });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            request.Setup(x => x.AcceptTypes).Returns(new string[] { });
            context.Setup(x => x.Request).Returns(request.Object);
            Route fakeRoute = new Route("fakeroute", null);

            // generic media types requested
            var legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/json" },
                    { "X-Ellucian-Media-Type", "application/xml" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsTrue(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // supported media types requested
            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/vnd.hedtech.v3.0.0" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/vnd.ellucian.v3.0.0" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // the case of the media type should not matter
            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "APPLICATION/VND.ELLUCIAN.V3.0.0" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // other media types requested that don't match
            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "application/vnd.hedtech.v1.0.0" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

            // custom media type supported
            legacyHeaders = new NameValueCollection
            {
                    { "X-Ellucian-Media-Type", "customTypeV2.0.0" }
            };
            request.Setup(x => x.Headers).Returns(legacyHeaders);
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));

        }

        /// <summary>
        /// Supported Versions: 6
        /// Requested Version:  6
        /// Payload Version:    6
        /// Expected Response:  correct
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_1()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "6"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "5"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint( new string[] { string.Format(CustomMediaTypeFormat, "6") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "6") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 6
        /// Requested Version:  6.0.0
        /// Payload Version:    6
        /// Expected Response:  No Match
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_2()
        {
            var defaultConstraintV2 = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "6"), satisfyVersionlessRequest: true);
            var nonDefaultConstraintV3 = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "5"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint(new string[] { string.Format(CustomMediaTypeFormat, "6") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "6.0.0") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraintV2.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraintV3.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 16.0.0
        /// Requested Version:  16
        /// Payload Version:    16.0.0
        /// Expected Response:  No Match
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_3()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "16.0.0"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "15.0.0"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint(new string[] { string.Format(CustomMediaTypeFormat, "16.0.0") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "16") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 16.0.0
        /// Requested Version:  16.0.0
        /// Payload Version:    16.0.0
        /// Expected Response:  correct
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_4()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "16.0.0"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "15.0.0"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint( new string[] { string.Format(CustomMediaTypeFormat, "16.0.0") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "16.0.0") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 6, 6.1.0
        /// Requested Version:  6
        /// Payload Version:    6
        /// Expected Response:  correct
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_5()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "6.1.0"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "6"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint(new string[] { string.Format(CustomMediaTypeFormat, "6"), string.Format(CustomMediaTypeFormat, "6.1.0") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "6") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 6, 6.1.0
        /// Requested Version:  6.0.0
        /// Payload Version:    6
        /// Expected Response:  No Match
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_6()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "6.1.0"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "6"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint(new string[] { string.Format(CustomMediaTypeFormat, "6"), string.Format(CustomMediaTypeFormat, "6.1.0") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "6.0.0") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 6, 6.1.0
        /// Requested Version:  6.1.0
        /// Payload Version:    6.1.0
        /// Expected Response:  Match
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_7()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "6.1.0"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "6"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint(new string[] { string.Format(CustomMediaTypeFormat, "6"), string.Format(CustomMediaTypeFormat, "6.1.0") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "6.1.0") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 16.0.0, 16.1.0
        /// Requested Version:  16
        /// Payload Version:    16.0.0
        /// Expected Response:  No Match
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_8()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "16.1.0"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "16.0.0"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint(new string[] { string.Format(CustomMediaTypeFormat, "16.0.0"), string.Format(CustomMediaTypeFormat, "16.1.0") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "16") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 16.0.0, 16.1.0
        /// Requested Version:  16.0.0
        /// Payload Version:    16.0.0
        /// Expected Response:  Match
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_9()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "16.1.0"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "16.0.0"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint(new string[] { string.Format(CustomMediaTypeFormat, "16.0.0"), string.Format(CustomMediaTypeFormat, "16.1.0") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "16.0.0") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        /// <summary>
        /// Supported Versions: 16.0.0, 16.1.0
        /// Requested Version:  16.1.0
        /// Payload Version:    16.1.0
        /// Expected Response:  Match
        /// </summary>
        [TestMethod]
        public void SemanticMatchOnCustomMediaTypesTest_10()
        {
            var defaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "16.1.0"), satisfyVersionlessRequest: true);
            var nonDefaultConstraint = new HeaderVersionConstraint(customMediaTypes: string.Format(CustomMediaTypeFormat, "16.0.0"), satisfyVersionlessRequest: false);
            var customConstraint = new HeaderVersionConstraint(new string[] { string.Format(CustomMediaTypeFormat, "16.0.0"), string.Format(CustomMediaTypeFormat, "16.1.0") });
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            var acceptHeaders = new string[] { string.Format(CustomMediaTypeFormat, "16.1.0") };
            request.Setup(x => x.AcceptTypes).Returns(acceptHeaders);
            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsTrue(defaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
            Assert.IsFalse(nonDefaultConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }
    }
}
