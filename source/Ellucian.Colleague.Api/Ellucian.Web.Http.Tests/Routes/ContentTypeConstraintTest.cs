// Copyright 2020 Ellucian Company L.P. and its affiliates.

using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;
using Ellucian.Web.Http.Routes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ellucian.Web.Http.Tests.Routes
{
    [TestClass]
    public class ContentTypeConstraintTest
    {
        private const string CustomMediaTypeFormat = "application/vnd.custom.integration.v{0}+json";

        [TestMethod]
        public void MatchOnContentTypesTest_SemanticVersion()
        {
            var customConstraint = new ContentTypeConstraint(string.Format(CustomMediaTypeFormat, "12.1.0"));
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            request.Setup(x => x.ContentType).Returns(string.Format(CustomMediaTypeFormat, "12.1.0"));


            var routeValueDictionary = new RouteValueDictionary();

            object requestedContentType = string.Empty;
            routeValueDictionary.TryGetValue("RequestedContentType", out requestedContentType);

            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        [TestMethod]
        public void MatchOnContentTypesTest_NonSemanticVersion()
        {
            var customConstraint = new ContentTypeConstraint(string.Format(CustomMediaTypeFormat, "1"));
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            request.Setup(x => x.ContentType).Returns(string.Format(CustomMediaTypeFormat, "1"));


            var routeValueDictionary = new RouteValueDictionary();

            object requestedContentType = string.Empty;
            routeValueDictionary.TryGetValue("RequestedContentType", out requestedContentType);

            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        [TestMethod]
        public void MatchOnContentTypesTest_SemanticVersion_MajorOnly()
        {
            var customConstraint = new ContentTypeConstraint(string.Format(CustomMediaTypeFormat, "12.0.0"));
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);


            request.Setup(x => x.ContentType).Returns(string.Format(CustomMediaTypeFormat, "12"));

            var routeValueDictionary = new RouteValueDictionary();

            object requestedContentType = string.Empty;
            routeValueDictionary.TryGetValue("RequestedContentType", out requestedContentType);

            Assert.IsTrue(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        [TestMethod]
        public void MatchOnContentTypesTest_SemanticVersion_InvalidMajorOnly()
        {
            var customConstraint = new ContentTypeConstraint(string.Format(CustomMediaTypeFormat, "12.0.0"));
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);


            request.Setup(x => x.ContentType).Returns(string.Format(CustomMediaTypeFormat, "1"));

            var routeValueDictionary = new RouteValueDictionary();

            object requestedContentType = string.Empty;
            routeValueDictionary.TryGetValue("RequestedContentType", out requestedContentType);

            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }

        [TestMethod]
        public void MatchOnContentTypesTest_SemanticVersion_Invalid()
        {
            var customConstraint = new ContentTypeConstraint(string.Format(CustomMediaTypeFormat, "1.0.0"));
            var context = new Mock<HttpContextBase>();
            var request = new Mock<HttpRequestBase>();
            context.Setup(x => x.Request).Returns(request.Object);
            request.Setup(x => x.Headers).Returns(new NameValueCollection() { });
            Route fakeRoute = new Route("fakeroute", null);

            request.Setup(x => x.ContentType).Returns(string.Format(CustomMediaTypeFormat, "1.1.0"));

            var routeValueDictionary = new RouteValueDictionary();

            object requestedContentType = string.Empty;
            routeValueDictionary.TryGetValue("RequestedContentType", out requestedContentType);

            Assert.IsFalse(customConstraint.Match(context.Object, fakeRoute, null, new RouteValueDictionary(), RouteDirection.IncomingRequest));
        }
    }
}