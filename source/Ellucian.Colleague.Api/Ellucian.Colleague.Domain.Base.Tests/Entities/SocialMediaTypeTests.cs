// Copyright 2015 Ellucian Company L.P. and its affiliates.
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Colleague.Domain.Base.Entities;

namespace Ellucian.Colleague.Domain.Base.Tests.Entities
{
    [TestClass]
    public class SocialMediaTypeTests
    {
        private string guid;
        private string code;
        private string description;
        private SocialMediaTypeCategory socialMediaTypeCategory;
        private SocialMediaType socialMediaType;
       

        [TestInitialize]
        public void Initialize()
        {
            guid = Guid.NewGuid().ToString();
            code = "FB";
            description = "Facebook";
            socialMediaTypeCategory = SocialMediaTypeCategory.facebook;
        }

        [TestClass]
        public class SocialMediaTypeConstructor : SocialMediaTypeTests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SocialMediaTypeConstructorNullGuid()
            {
                socialMediaType = new SocialMediaType(null, code, description, socialMediaTypeCategory);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SocialMediaTypeConstructorEmptyGuid()
            {
                socialMediaType = new SocialMediaType(string.Empty, code, description, socialMediaTypeCategory);
            }

            [TestMethod]
            public void SocialMediaTypeConstructorValidGuid()
            {
                socialMediaType = new SocialMediaType(guid, code, description, socialMediaTypeCategory);
                Assert.AreEqual(guid, socialMediaType.Guid);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SocialMediaTypeConstructorNullCode()
            {
                socialMediaType = new SocialMediaType(guid, null, description, socialMediaTypeCategory);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SocialMediaTypeConstructorEmptyCode()
            {
                socialMediaType = new SocialMediaType(guid, string.Empty, description, socialMediaTypeCategory);
            }

            [TestMethod]
            public void SocialMediaTypeConstructorValidCode()
            {
                socialMediaType = new SocialMediaType(guid, code, description, socialMediaTypeCategory);
                Assert.AreEqual(code, socialMediaType.Code);
            }

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SocialMediaTypeConstructorNullDescription()
            {
                socialMediaType = new SocialMediaType(guid, code, null, socialMediaTypeCategory);
            }       

            [TestMethod]
            [ExpectedException(typeof(ArgumentNullException))]
            public void SocialMediaTypeConstructorEmptyDescription()
            {
                socialMediaType = new SocialMediaType(guid, code, string.Empty, socialMediaTypeCategory);
            }

            [TestMethod]
            public void SocialMediaTypeConstructorValidDescription()
            {
                socialMediaType = new SocialMediaType(guid, code, description, socialMediaTypeCategory);
                Assert.AreEqual(description, socialMediaType.Description);
            }

            [TestMethod]
            public void SocialMediaTypeConstructorValidType()
            {
                socialMediaType = new SocialMediaType(guid, code, description, socialMediaTypeCategory);
                Assert.AreEqual(socialMediaTypeCategory, socialMediaType.Type);
            }
        }
    }
}