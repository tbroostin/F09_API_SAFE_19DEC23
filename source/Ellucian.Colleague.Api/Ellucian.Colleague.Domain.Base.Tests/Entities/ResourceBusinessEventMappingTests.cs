// Copyright 2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ellucian.Colleague.Domain.Base.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class ResourceBusinessEventMappingTests
    {
        private string resourceName;
        private string resourceVersion;
        private string pathSegment;
        private string businessEvent;

        private ResourceBusinessEventMapping mapping1;

        [TestInitialize]
        public void Initialize()
        {
            resourceName = "course";
            resourceVersion = "1";
            pathSegment = "/courses/";
            businessEvent = "event1";
            mapping1 = new ResourceBusinessEventMapping(resourceName, resourceVersion, pathSegment, businessEvent);
        }

        [TestClass]
        public class ResourceBusinessEventMapping_Constructor : ResourceBusinessEventMappingTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResourceBusinessEventMapping_Constructor_NullResourceName()
            {
                mapping1 = new ResourceBusinessEventMapping(null, resourceVersion, pathSegment, businessEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResourceBusinessEventMapping_Constructor_EmptyResourceName()
            {
                mapping1 = new ResourceBusinessEventMapping(string.Empty, resourceVersion, pathSegment, businessEvent);
            }

            [TestMethod]
            public void ResourceBusinessEventMapping_Constructor_ValidResourceName()
            {
                mapping1 = new ResourceBusinessEventMapping(resourceName, resourceVersion, pathSegment, businessEvent);
                Assert.AreEqual(resourceName, mapping1.ResourceName);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void ResourceBusinessEventMapping_Constructor_InvalidResourceVersion()
            {
                mapping1 = new ResourceBusinessEventMapping(resourceName, "-1", pathSegment, businessEvent);
            }

            [TestMethod]
            public void ResourceBusinessEventMapping_Constructor_ValidResourceVersion()
            {
                mapping1 = new ResourceBusinessEventMapping(resourceName, resourceVersion, pathSegment, businessEvent);
                Assert.AreEqual(resourceVersion, mapping1.ResourceVersion);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResourceBusinessEventMapping_Constructor_NullPathSegment()
            {
                mapping1 = new ResourceBusinessEventMapping(resourceName, resourceVersion, null, businessEvent);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void ResourceBusinessEventMapping_Constructor_EmptyPathSegment()
            {
                mapping1 = new ResourceBusinessEventMapping(resourceName, resourceVersion, string.Empty, businessEvent);
            }

            [TestMethod]
            public void ResourceBusinessEventMapping_Constructor_ValidPathSegment()
            {
                mapping1 = new ResourceBusinessEventMapping(resourceName, resourceVersion, pathSegment, businessEvent);
                Assert.AreEqual(pathSegment, mapping1.PathSegment);
            }

            [TestMethod]
            public void ResourceBusinessEventMapping_Constructor_ValidBusinessEvent()
            {
                mapping1 = new ResourceBusinessEventMapping(resourceName, resourceVersion, pathSegment, businessEvent);
                Assert.AreEqual(businessEvent, mapping1.BusinessEvent);
            }
        }

        [TestClass]
        public class ResourceBusinessEventMapping_Equals : ResourceBusinessEventMappingTests
        {
            [TestMethod]
            public void ResourceBusinessEventMappingEqualsNullItem()
            {
                Assert.IsFalse(mapping1.Equals(null));
            }

            [TestMethod]
            public void ResourceBusinessEventMappingEqualsNonResourceBusinessEventMappingObject()
            {
                Assert.IsFalse(mapping1.Equals("abc"));
            }

            [TestMethod]
            public void ResourceBusinessEventMappingEqualsDifferentResourceName()
            {
                var mapping2 = new ResourceBusinessEventMapping(resourceName + "A", resourceVersion, pathSegment, businessEvent);
                Assert.IsFalse(mapping1.Equals(mapping2));
            }

            [TestMethod]
            public void ResourceBusinessEventMappingEqualsDifferentResourceVersion()
            {
                var mapping2 = new ResourceBusinessEventMapping(resourceName, "2", pathSegment, businessEvent);
                Assert.IsFalse(mapping1.Equals(mapping2));
            }

            [TestMethod]
            public void ResourceBusinessEventMappingEqualsMatchingItem()
            {
                var mapping2 = new ResourceBusinessEventMapping(resourceName, resourceVersion, pathSegment, businessEvent);
                Assert.IsTrue(mapping1.Equals(mapping2));
            }
        }

        [TestClass]
        public class ResourceBusinessEventMapping_GetHashCode : ResourceBusinessEventMappingTests
        {
            [TestMethod]
            public void ResourceBusinessEventMappingSameCodeHashEqual()
            {
                var mapping2 = new ResourceBusinessEventMapping(resourceName, resourceVersion, pathSegment, businessEvent);
                Assert.AreEqual(mapping1.GetHashCode(), mapping2.GetHashCode());
            }

            [TestMethod]
            public void ResourceBusinessEventMappingDifferentCodeHashNotEqual()
            {
                var mapping2 = new ResourceBusinessEventMapping(resourceName +"A", resourceVersion, pathSegment, businessEvent);
                Assert.AreNotEqual(mapping1.GetHashCode(), mapping2.GetHashCode());
            }
        }
    }
}