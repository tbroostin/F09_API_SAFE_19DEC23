// Copyright 2012-2014 Ellucian Company L.P. and its affiliates.
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ellucian.Web.Http.Configuration;

namespace Ellucian.Web.Http.Configuration.Tests
{
    [TestClass]
    public class ApiSettingsTests
    {
        public int id;
        public string name;
        public int version;
        public ApiSettings apiSettings;

        [TestInitialize]
        public void Initialize()
        {
            id = 5;
            name = "production config";
            version = 3;
            apiSettings = new ApiSettings(id, name, version);
        }

        [TestMethod]
        public void Id()
        {
            Assert.AreEqual(id, apiSettings.Id);
        }

        [TestMethod]
        public void Name()
        {
            Assert.AreEqual(name, apiSettings.Name);
        }

        [TestMethod]
        public void Version()
        {
            Assert.AreEqual(version, apiSettings.Version);
        }
        
        [TestMethod]
        public void NameOnlyConstructor()
        {
            var nameOnlySettings = new ApiSettings(name);
            Assert.AreEqual(name, nameOnlySettings.Name);
            Assert.AreEqual(0, nameOnlySettings.Id);
            Assert.AreEqual(0, nameOnlySettings.Version);
            Assert.IsNotNull(nameOnlySettings.PhotoHeaders);
            Assert.AreEqual(5000, nameOnlySettings.BulkReadSize);
            Assert.AreEqual(TimeZoneInfo.Local.Id, nameOnlySettings.ColleagueTimeZone);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsErrorIfNameNull()
        {
            new ApiSettings(id, null, version);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ThrowsErrorIfNameEmpty()
        {
            new ApiSettings(id, string.Empty, version);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NameOnlyConstructorThrowsErrorIfNameNull()
        {
            new ApiSettings(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NameOnlyConstructorThrowsErrorIfNameEmpty()
        {
            new ApiSettings(string.Empty);
        }

    }
}
